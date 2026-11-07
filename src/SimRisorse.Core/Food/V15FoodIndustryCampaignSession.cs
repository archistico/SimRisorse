using SimRisorse.Core.Content;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Core.Food;

public enum FoodIndustryActionFailure
{
    None, UnknownBuilding, InvalidQuantity, MissingPrerequisite, MissingResearch,
    InsufficientResources, InsufficientWorkplaces, InsufficientPopulation,
    InsufficientAssignedWorkers, NumericLimit
}

public sealed record FoodIndustryBuildResult(FoodIndustryActionFailure Failure, ResourceBundle Cost)
{
    public bool Succeeded => Failure == FoodIndustryActionFailure.None;
}

/// <summary>
/// RC51 / F1-D05d.3b: persistent v15 commands and one unified headless clock.
/// The playable v14 GUI is intentionally unchanged. StepFood remains available
/// solely for isolated planner tests and does NOT advance time.
/// </summary>
public sealed partial class V15FoodIndustryCampaignSession
{
    private readonly GameScenario _baseline;
    private readonly SchoolResearchRules _school;
    private readonly TechnologyCatalog _technologies;
    private readonly FoodConversionPolicy _depotPolicy;
    private readonly FoodIndustryConstructionCatalog _catalog;
    private readonly IReadOnlyDictionary<string, TieredProductionFamily> _nativeFamilies;
    private V15FoodCampaignCheckpoint _current;

    public V15FoodIndustryCampaignSession(V15FoodCampaignCheckpoint starting,
        GameScenario baseline, SchoolResearchRules school,
        TechnologyCatalog technologies, FoodConversionPolicy depotPolicy,
        FoodIndustryConstructionCatalog? catalog = null)
    {
        _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        _school = school ?? throw new ArgumentNullException(nameof(school));
        _technologies = technologies ?? throw new ArgumentNullException(nameof(technologies));
        _depotPolicy = depotPolicy ?? throw new ArgumentNullException(nameof(depotPolicy));
        _catalog = catalog ?? FoodIndustryConstructionCatalog.Candidate;
        _catalog.ValidateAgainst(_baseline);
        // Preserve the v14 command boundary's technology/cost validation once,
        // but never construct a historical session to perform a v15 action.
        var allFamilies = TieredProductionCatalog.FromCatalog(_baseline.Catalog,
            V15TierRules.LargeMultiplier, V15TierRules.AdvancedMultiplier);
        TechnologyTierEffects.ValidateCatalog(_technologies, allFamilies);
        _baseline.Catalog.ValidateBundle(V15SchoolBuilding.Cost);
        foreach (var prerequisite in V15SchoolBuilding.Prerequisites)
            if (!_baseline.Catalog.BuildingsById.ContainsKey(prerequisite))
                throw new ArgumentException($"Unknown school prerequisite: {prerequisite}.", nameof(baseline));
        // Cache the 15 live productive families once, never on each GUI trial.
        _nativeFamilies = allFamilies
            .Where(f => f.FamilyId != V15LegacyProducerAdapter.RetiredFamilyId)
            .ToDictionary(f => f.FamilyId, StringComparer.Ordinal);
        _current = starting ?? throw new ArgumentNullException(nameof(starting));
        Commit(starting);
    }

    public bool IsPaused { get; private set; } = true;
    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;

    /// <summary>Advances all v15 systems atomically; no historical food-store recipe
    /// and no v14 food consumption. The clock is paused on new/restored sessions.</summary>
    public GameActionResult Advance(TimeSpan elapsed, MeatChainPolicy meatPolicy,
        HorticulturePolicy cropsPolicy)
    {
        if (elapsed < TimeSpan.Zero) return new(GameError.InvalidElapsedTime);
        if (elapsed > GameSession.MaximumAdvance) return new(GameError.AdvanceLimitExceeded);
        if (IsPaused || elapsed == TimeSpan.Zero) return GameActionResult.Success;
        try
        {
            var result = V15UnifiedCampaignClock.Plan(_current, _baseline, _school,
                _technologies, _catalog, meatPolicy, cropsPolicy, _depotPolicy, elapsed);
            Commit(result.Next);
            return GameActionResult.Success;
        }
        catch (OverflowException)
        {
            return new(GameError.NumericLimit);
        }
    }

    public V15FoodCampaignCheckpoint Snapshot() => _current;
    public V15CrewMode CrewMode => _current.CrewMode;

    /// <summary>Explicit, irreversible-in-session opt-in to the live ten-person
    /// mint rule. It is saved in the checkpoint header, never guessed on load.
    /// Existing campaigns and the approved desktop keep legacy rules by default.</summary>
    public void EnableMintCrews()
    {
        if (_current.CrewMode == V15CrewMode.MintTen) return;
        Commit(_current with { CrewMode = V15CrewMode.MintTen });
    }
    public FoodIndustryConstructionCatalog Catalog => _catalog;
    /// <summary>Current v15 catalog and resource projection for the approved Desktop UI.
    /// Computed from the committed checkpoint so constructions, staffing and stocks
    /// are reflected immediately, without caching or changing the session state.</summary>
    public V15CampaignCatalogView CityCatalog =>
        V15CampaignCatalogProjection.Project(_baseline, _current, _catalog);
    public FoodIndustryCapacities Capacities => _catalog.CapacitiesFor(_current.Industry.Buildings);
    public long AvailableWorkers => V15FoodCampaignCheckpoints.AvailableWorkers(_current);

    private void Commit(V15FoodCampaignCheckpoint next)
    {
        var canonical = next with
        {
            CityCampaign = V15LegacyProducerAdapter.WithoutEmptySlot(next.CityCampaign)
        };
        V15FoodCampaignCheckpoints.Validate(canonical, _baseline, _school, _technologies, _depotPolicy);
        _current = canonical;
    }

    private bool HasPrerequisite(string id) =>
        (FoodIndustrySites.Ids.Contains(id) &&
            FoodIndustrySites.Get(_current.Industry.Buildings.Counts, id) > 0) ||
        _current.CityCampaign.OtherBuildings.Any(item => item.BuildingId == id && item.Count > 0) ||
        _current.CityCampaign.ProductiveFamilies.Any(item => item.FamilyId == id &&
            (long)item.Base.Buildings + item.Large.Buildings + item.Advanced.Buildings > 0) ||
        (id == V14CampaignSnapshots.SchoolId && _current.CityCampaign.SchoolCount > 0);

    /// <summary>Uses the same checks as Build, suitable for a disabled/enabled GUI action.</summary>
    public FoodIndustryBuildResult CheckBuild(string? siteId, int quantity) =>
        EvaluateBuild(siteId, quantity).Result;

    public FoodIndustryBuildResult Build(string? siteId, int quantity)
    {
        var (result, next) = EvaluateBuild(siteId, quantity);
        if (result.Succeeded) Commit(next!);
        return result;
    }

    private (FoodIndustryBuildResult Result, V15FoodCampaignCheckpoint? Next) EvaluateBuild(
        string? siteId, int quantity)
    {
        static FoodIndustryBuildResult Fail(FoodIndustryActionFailure reason, ResourceBundle? cost = null) =>
            new(reason, cost ?? ResourceBundle.Empty);
        if (quantity <= 0) return (Fail(FoodIndustryActionFailure.InvalidQuantity), null);
        var isDepotTier = siteId is "food-depot-large" or "food-depot-advanced";
        if (!_catalog.TryGet(isDepotTier ? "food-depot" : siteId, out var site) || site is null)
            return (Fail(FoodIndustryActionFailure.UnknownBuilding), null);
        if (isDepotTier && !_current.CityCampaign.UnlockedTechnologyIds.Contains(
                siteId == "food-depot-large" ? "alimentazione-1" : "alimentazione-2"))
            return (Fail(FoodIndustryActionFailure.MissingResearch), null);
        if (site.Prerequisites.Any(required => !HasPrerequisite(required)))
            return (Fail(FoodIndustryActionFailure.MissingPrerequisite), null);
        try
        {
            var counts = _current.Industry.Buildings.Counts;
            var nextCount = checked(FoodIndustrySites.Get(counts, siteId!) + quantity);
            // The single counter is also the acquired-history counter, since demolition
            // is not available in this version. Keep it in the range of pricing APIs.
            if (nextCount > int.MaxValue)
                return (Fail(FoodIndustryActionFailure.NumericLimit), null);
            var price = _catalog.Quote(_current.Industry.Buildings, siteId!, quantity);
            if (!_current.CityCampaign.Resources.Covers(price))
                return (Fail(FoodIndustryActionFailure.InsufficientResources, price), null);
            var byId = FoodIndustrySites.PersistedIds.ToDictionary(id => id,
                id => id == siteId ? nextCount : FoodIndustrySites.Get(counts, id),
                StringComparer.Ordinal);
            var buildings = _current.Industry.Buildings with
            {
                Counts = FoodIndustrySites.FromDictionary(byId)
            };
            // Force capacity-overflow checks BEFORE committing anything.
            _ = _catalog.CapacitiesFor(buildings);
            var next = _current with
            {
                CityCampaign = _current.CityCampaign with
                    { Resources = _current.CityCampaign.Resources - price },
                Industry = _current.Industry with { Buildings = buildings }
            };
            V15FoodCampaignCheckpoints.Validate(next, _baseline, _school,
                _technologies, _depotPolicy);
            return (new FoodIndustryBuildResult(FoodIndustryActionFailure.None, price), next);
        }
        catch (OverflowException)
        {
            return (Fail(FoodIndustryActionFailure.NumericLimit), null);
        }
        catch (ArgumentOutOfRangeException)
        {
            // Extremely high progressive prices can exceed ResourceAmount's range.
            return (Fail(FoodIndustryActionFailure.NumericLimit), null);
        }
    }

    public FoodIndustryActionFailure AssignWorkers(string? siteId, int quantity) =>
        ChangeWorkers(siteId, quantity, release: false);

    public FoodIndustryActionFailure ReleaseWorkers(string? siteId, int quantity) =>
        ChangeWorkers(siteId, quantity, release: true);

    private FoodIndustryActionFailure ChangeWorkers(string? siteId, int quantity, bool release)
    {
        if (quantity <= 0) return FoodIndustryActionFailure.InvalidQuantity;
        if (!_catalog.TryGet(FoodIndustrySites.DepotTierIds.Contains(siteId ?? "")
                ? "food-depot" : siteId, out var site) || site is null)
            return FoodIndustryActionFailure.UnknownBuilding;
        try
        {
            var buildings = _current.Industry.Buildings;
            var previous = FoodIndustrySites.Get(buildings.Assigned, siteId!);
            if (release && quantity > previous)
                return FoodIndustryActionFailure.InsufficientAssignedWorkers;
            if (!release && (long)quantity > FoodIndustrySites.Get(buildings.Workplaces, siteId!) - previous)
                return FoodIndustryActionFailure.InsufficientWorkplaces;
            if (!release && quantity > AvailableWorkers)
                return FoodIndustryActionFailure.InsufficientPopulation;
            var newWorkers = release ? previous - quantity : checked(previous + quantity);
            var byId = FoodIndustrySites.PersistedIds.ToDictionary(id => id,
                id => id == siteId ? newWorkers : FoodIndustrySites.Get(buildings.Assigned, id),
                StringComparer.Ordinal);
            Commit(_current with { Industry = _current.Industry with
                { Buildings = buildings with { Assigned = FoodIndustrySites.FromDictionary(byId) } } });
            return FoodIndustryActionFailure.None;
        }
        catch (OverflowException)
        {
            return FoodIndustryActionFailure.NumericLimit;
        }
    }

    /// <summary>Explicit deterministic food-only step; historic clock is NOT advanced.
    /// Every stock, including animals and undelivered intermediates, is committed atomically.</summary>
    public FoodIndustryTickResult StepFood(MeatChainPolicy meatPolicy,
        HorticulturePolicy horticulturePolicy, FoodIndustryMeals meals)
    {
        var step = V15FoodCampaignTicks.Plan(_current, _baseline, _school,
            _technologies, meatPolicy, horticulturePolicy, _depotPolicy,
            Capacities, meals);
        Commit(step.Next);
        return step.Detail;
    }
}
