using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Military;

namespace SimRisorse.Core.Sessions;

/// <summary>
/// Candidate, separate v14 school construction policy. The prices and 15% growth are
/// experimental parameters, not an amendment of the playable civil-growth-13 rules.
/// </summary>
public sealed class V14SchoolConstructionRules
{
    public V14SchoolConstructionRules(ResourceBundle cost, decimal growthFactor,
        IEnumerable<string> prerequisites)
    {
        ArgumentNullException.ThrowIfNull(cost);
        ArgumentNullException.ThrowIfNull(prerequisites);
        if (cost.Amounts.IsEmpty || growthFactor < 1m)
            throw new ArgumentOutOfRangeException(nameof(cost));
        var required = prerequisites.ToImmutableArray();
        if (required.IsDefaultOrEmpty || required.Any(string.IsNullOrWhiteSpace) ||
            required.Distinct(StringComparer.Ordinal).Count() != required.Length)
            throw new ArgumentException("School prerequisites must be distinct, known building IDs.", nameof(prerequisites));
        Cost = cost;
        GrowthFactor = growthFactor;
        Prerequisites = required;
    }

    public ResourceBundle Cost { get; }
    public decimal GrowthFactor { get; }
    public ImmutableArray<string> Prerequisites { get; }

    public static V14SchoolConstructionRules Candidate { get; } =
        new(ResourceBundle.Create(("planks", 120m), ("stone", 80m)), 1.15m,
            ["small-house", "food-store"]);
}

public enum V14SchoolConstructionFailure
{
    None, InvalidQuantity, MissingPrerequisite, InsufficientResources, NumericLimit
}

public sealed record V14SchoolConstructionResult(V14SchoolConstructionFailure Failure, ResourceBundle Cost)
{
    public bool Succeeded => Failure == V14SchoolConstructionFailure.None;
}

/// <summary>
/// E3b/3a-d: isolated owner of a validated v14 checkpoint. School construction, enrollment,
/// productive tier construction/upgrade, staffing and optional research purchases are atomic snapshot transitions. No v13 session,
/// UI or SQLite store is invoked or mutated. Tier construction, staffing and per-second time are supported in isolation.
/// Callers must serialize access to the session, just as for the existing GameSession.
/// </summary>
public sealed class V14CampaignSession
{
    private readonly GameScenario _baseline;
    private readonly SchoolResearchRules _school;
    private readonly TechnologyCatalog _technologies;
    private readonly V14SchoolConstructionRules _schoolBuilding;
    private readonly V14TierConstructionRules _tierConstruction;
    private readonly IReadOnlyDictionary<string, TieredProductionFamily> _productiveCatalog;
    private V14CampaignSnapshot _snapshot;

    private V14CampaignSession(GameScenario baselineV13, V14CampaignSnapshot snapshot,
        SchoolResearchRules schoolRules, TechnologyCatalog technologies,
        V14SchoolConstructionRules schoolBuilding, V14TierConstructionRules? tierConstruction = null)
    {
        _baseline = baselineV13 ?? throw new ArgumentNullException(nameof(baselineV13));
        _school = schoolRules ?? throw new ArgumentNullException(nameof(schoolRules));
        _technologies = technologies ?? throw new ArgumentNullException(nameof(technologies));
        _schoolBuilding = schoolBuilding ?? throw new ArgumentNullException(nameof(schoolBuilding));
        _tierConstruction = tierConstruction ?? V14TierConstructionRules.Candidate;
        _productiveCatalog = TieredProductionCatalog.FromCatalog(_baseline.Catalog,
            _tierConstruction.LargeMultiplier, _tierConstruction.AdvancedMultiplier)
            .ToDictionary(item => item.FamilyId, StringComparer.Ordinal);
        _baseline.Catalog.ValidateBundle(_schoolBuilding.Cost);
        foreach (var prerequisite in _schoolBuilding.Prerequisites)
            if (!_baseline.Catalog.BuildingsById.ContainsKey(prerequisite))
                throw new ArgumentException($"Unknown school prerequisite: {prerequisite}.", nameof(schoolBuilding));
        V14CampaignSnapshots.Validate(_baseline, snapshot, _school, _technologies);
        _snapshot = snapshot;
    }

    public static V14CampaignSession FromV13(GameScenario baselineV13, GameSnapshot source,
        SchoolResearchRules schoolRules, TechnologyCatalog technologies,
        V14SchoolConstructionRules schoolBuilding, V14TierConstructionRules? tierConstruction = null) => Restore(baselineV13,
            V14CampaignSnapshots.MigrateFromV13(baselineV13, source, schoolRules, technologies),
            schoolRules, technologies, schoolBuilding, tierConstruction);

    public static V14CampaignSession Restore(GameScenario baselineV13, V14CampaignSnapshot snapshot,
        SchoolResearchRules schoolRules, TechnologyCatalog technologies,
        V14SchoolConstructionRules schoolBuilding, V14TierConstructionRules? tierConstruction = null) =>
        new(baselineV13, snapshot, schoolRules, technologies, schoolBuilding, tierConstruction);

    /// <summary>Like GameSession, a restored or newly created session starts paused.</summary>
    public bool IsPaused { get; private set; } = true;

    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;

    /// <summary>
    /// Advance the single v14 simulation clock. All subsystem changes are committed
    /// together or not at all. Partial seconds carry over in ElapsedTicks.
    /// </summary>
    public GameActionResult Advance(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero) return new(GameError.InvalidElapsedTime);
        if (elapsed > GameSession.MaximumAdvance) return new(GameError.AdvanceLimitExceeded);
        if (IsPaused || elapsed == TimeSpan.Zero) return GameActionResult.Success;
        try
        {
            var next = V14CampaignClock.Advance(_baseline, _snapshot, _school, _technologies,
                elapsed, StorageCapacity());
            Commit(next);
            return GameActionResult.Success;
        }
        catch (OverflowException)
        {
            return new(GameError.NumericLimit);
        }
    }

    /// <summary>Read-only prediction shared by the classic seven-column resource table.</summary>
    public V14EconomyPreview ForecastEconomy() =>
        V14EconomyProjector.Project(_baseline, _snapshot, StorageCapacity());

    private static GameActionResult MilitaryResult(RecruitmentFailure failure) => new(failure switch
    {
        RecruitmentFailure.None => GameError.None,
        RecruitmentFailure.InvalidQuantity => GameError.InvalidQuantity,
        RecruitmentFailure.MissingBarracks => GameError.MissingBarracks,
        RecruitmentFailure.InsufficientPopulation => GameError.InsufficientPopulation,
        RecruitmentFailure.InsufficientSwords => GameError.InsufficientSwords,
        RecruitmentFailure.InsufficientSoldiers => GameError.InsufficientSoldiers,
        RecruitmentFailure.InsufficientQueuedRecruits => GameError.InsufficientQueuedRecruits,
        RecruitmentFailure.InsufficientStorage => GameError.InsufficientStorage,
        RecruitmentFailure.DischargeUnavailable => GameError.DischargeUnavailable,
        _ => GameError.NumericLimit
    });

    private GameActionResult ApplyMilitary(RecruitmentAttempt result)
    {
        if (!result.Succeeded) return MilitaryResult(result.Failure);
        Commit(_snapshot with { Resources = result.Resources, Military = result.State });
        return GameActionResult.Success;
    }

    /// <summary>Keep v13 enlistment rules, including research trainees in reserved population.</summary>
    public GameActionResult Enlist(int quantity)
    {
        if (_baseline.MilitaryRules is not { } rules) return new(GameError.MissingBarracks);
        var barracks = _snapshot.OtherBuildings.Single(item => item.BuildingId == rules.BarracksId).Count;
        var busy = checked(CivilianWorkers + _snapshot.SchoolResearch.ReservedPopulation);
        return ApplyMilitary(rules.IndividualTraining
            ? IndividualRecruitment.Enlist(rules, _snapshot.Military, _snapshot.Resources,
                _snapshot.Population, busy, barracks, quantity)
            : MilitaryTraining.Enlist(rules, _snapshot.Military, _snapshot.Resources,
                _snapshot.Population, busy, barracks, quantity));
    }

    /// <summary>Cancel pending recruits and refund equipment only if the shared store has room.</summary>
    public GameActionResult CancelRecruits(int quantity)
    {
        if (_baseline.MilitaryRules is not { } rules) return new(GameError.MissingBarracks);
        var barracks = _snapshot.OtherBuildings.Single(item => item.BuildingId == rules.BarracksId).Count;
        return ApplyMilitary(rules.IndividualTraining
            ? IndividualRecruitment.Cancel(rules, _snapshot.Military, _snapshot.Resources,
                StorageCapacity(), barracks, quantity)
            : MilitaryTraining.CancelWaiting(rules, _snapshot.Military, _snapshot.Resources,
                StorageCapacity(), barracks, quantity));
    }

    /// <summary>Individual soldiers cannot be discharged until missions are implemented.</summary>
    public GameActionResult DischargeSoldiers(int quantity)
    {
        if (_baseline.MilitaryRules is not { } rules) return new(GameError.MissingBarracks);
        var barracks = _snapshot.OtherBuildings.Single(item => item.BuildingId == rules.BarracksId).Count;
        return ApplyMilitary(MilitaryTraining.Discharge(rules, _snapshot.Military,
            _snapshot.Resources, barracks, quantity));
    }

    public V14CampaignSnapshot Snapshot() => _snapshot;
    public ResearchPoints ResearchBalance => ResearchPoints.FromMilliPoints(_snapshot.ResearchBalanceMilliPoints);
    public int AvailablePopulation => checked(_snapshot.Population - ReservedPopulation);

    private int ReservedPopulation => checked((int)(
        _snapshot.OtherBuildings.Sum(item => (long)item.AssignedWorkers) +
        _snapshot.ProductiveFamilies.Sum(item => (long)item.Base.AssignedWorkers +
            item.Large.AssignedWorkers + item.Advanced.AssignedWorkers) +
        _snapshot.Military.EnlistedPopulation + _snapshot.SchoolResearch.ReservedPopulation));

    private void Commit(V14CampaignSnapshot next)
    {
        V14CampaignSnapshots.Validate(_baseline, next, _school, _technologies);
        _snapshot = next;
    }

    public V14SchoolConstructionResult BuildSchool(int quantity)
    {
        if (quantity <= 0)
            return new(V14SchoolConstructionFailure.InvalidQuantity, ResourceBundle.Empty);
        if (_schoolBuilding.Prerequisites.Any(id => !_snapshot.OtherBuildings.Any(
                item => item.BuildingId == id && item.Count > 0) &&
                !_snapshot.ProductiveFamilies.Any(item => item.FamilyId == id &&
                    (long)item.Base.Buildings + item.Large.Buildings + item.Advanced.Buildings > 0)))
            return new(V14SchoolConstructionFailure.MissingPrerequisite, ResourceBundle.Empty);
        try
        {
            var history = _snapshot.PurchaseCounters.Single(item => item.FamilyId == V14CampaignSnapshots.SchoolId);
            var cost = ProgressiveConstructionPricing.QuoteNewBuildings(_schoolBuilding.Cost,
                1m, _schoolBuilding.GrowthFactor, history.Acquired, quantity);
            if (!_snapshot.Resources.Covers(cost))
                return new(V14SchoolConstructionFailure.InsufficientResources, cost);
            var next = _snapshot with
            {
                SchoolCount = checked(_snapshot.SchoolCount + quantity),
                Resources = _snapshot.Resources - cost,
                PurchaseCounters = _snapshot.PurchaseCounters.Select(item =>
                    item.FamilyId == V14CampaignSnapshots.SchoolId
                        ? item with { Acquired = checked(item.Acquired + quantity) }
                        : item).ToImmutableArray()
            };
            Commit(next);
            return new(V14SchoolConstructionFailure.None, cost);
        }
        catch (OverflowException)
        {
            return new(V14SchoolConstructionFailure.NumericLimit, ResourceBundle.Empty);
        }
    }

    /// <summary>
    /// E3b/3e: construct existing non-productive v13 catalog buildings (housing,
    /// warehouse, barracks) under v14 candidate family growth. Never constructs the
    /// fixed Municipio, a tiered productive family or the separately modeled School.
    /// Bulk purchases price every unit sequentially; commit is all-or-nothing.
    /// </summary>
    public V14CivicConstructionResult BuildCivicBuilding(string buildingId, int quantity)
    {
        if (quantity <= 0)
            return new(V14CivicConstructionFailure.InvalidQuantity, ResourceBundle.Empty);
        if (buildingId is null || !_baseline.Catalog.BuildingsById.TryGetValue(buildingId, out var definition))
            return new(V14CivicConstructionFailure.UnknownBuilding, ResourceBundle.Empty);
        if (definition.IsScenarioFixed)
            return new(V14CivicConstructionFailure.FixedBuilding, ResourceBundle.Empty);
        if (_productiveCatalog.ContainsKey(buildingId) || definition.Recipe is not null)
            return new(V14CivicConstructionFailure.TieredBuilding, ResourceBundle.Empty);
        // School is not in the v13 catalog; it remains a dedicated v14 command.
        if (definition.Prerequisites.Any(id => !OwnsBuilding(id)))
            return new(V14CivicConstructionFailure.MissingPrerequisite, ResourceBundle.Empty);
        try
        {
            var old = _snapshot.OtherBuildings.Single(item => item.BuildingId == buildingId);
            var nextCount = checked(old.Count + quantity);
            var history = _snapshot.PurchaseCounters.Single(item => item.FamilyId == buildingId);
            var nextHistory = checked(history.Acquired + quantity);
            if (definition.HousingPerBuilding > 0)
            {
                long currentHousing = _snapshot.OtherBuildings.Sum(item =>
                    (long)item.Count * _baseline.Catalog.BuildingsById[item.BuildingId].HousingPerBuilding);
                if (currentHousing + (long)quantity * definition.HousingPerBuilding > int.MaxValue)
                    return new(V14CivicConstructionFailure.NumericLimit, ResourceBundle.Empty);
            }
            if (definition.StoragePerBuilding is { } storage)
                _ = StorageCapacity() + storage.Scale(quantity);
            var cost = ProgressiveConstructionPricing.QuoteNewBuildings(definition.Cost, 1m,
                _tierConstruction.GrowthFactor, history.Acquired, quantity);
            if (!_snapshot.Resources.Covers(cost))
                return new(V14CivicConstructionFailure.InsufficientResources, cost);
            Commit(_snapshot with
            {
                Resources = _snapshot.Resources - cost,
                OtherBuildings = _snapshot.OtherBuildings.Select(item => item.BuildingId == buildingId
                    ? item with { Count = nextCount } : item).ToImmutableArray(),
                PurchaseCounters = ReplaceCounter(buildingId, nextHistory)
            });
            return new(V14CivicConstructionFailure.None, cost);
        }
        catch (OverflowException)
        {
            return new(V14CivicConstructionFailure.NumericLimit, ResourceBundle.Empty);
        }
    }

    private bool OwnsBuilding(string buildingId) =>
        _snapshot.OtherBuildings.Any(item => item.BuildingId == buildingId && item.Count > 0) ||
        _snapshot.ProductiveFamilies.Any(item => item.FamilyId == buildingId &&
            (long)item.Base.Buildings + item.Large.Buildings + item.Advanced.Buildings > 0) ||
        (buildingId == V14CampaignSnapshots.SchoolId && _snapshot.SchoolCount > 0);

    /// <summary>Craft pencils in the school workshop: 1 Legno lavorato + 1 Carbone -> 4 Matite.</summary>
    public PencilCraftFailure CraftPencils(int batches)
    {
        if (batches <= 0) return PencilCraftFailure.InvalidQuantity;
        if (_snapshot.SchoolCount == 0) return PencilCraftFailure.MissingSchool;
        try
        {
            var cost = V14SchoolSupplies.Inputs(batches);
            if (!_snapshot.Resources.Covers(cost)) return PencilCraftFailure.InsufficientMaterials;
            var output = V14SchoolSupplies.Output(batches);
            var result = _snapshot.Resources - cost + output;
            if (!StorageCapacity().Covers(result)) return PencilCraftFailure.InsufficientStorage;
            Commit(_snapshot with { Resources = result });
            return PencilCraftFailure.None;
        }
        catch (Exception e) when (e is OverflowException or ArgumentOutOfRangeException)
        {
            return PencilCraftFailure.NumericLimit;
        }
    }

    public SchoolActionFailure EnrollStudents(int count)
    {
        var attempt = SchoolResearch.Enroll(_school, _snapshot.SchoolResearch, _snapshot.Resources,
            _snapshot.Population, CivilianWorkers, checked((int)_snapshot.Military.EnlistedPopulation),
            _snapshot.SchoolCount, count);
        if (attempt.Succeeded)
            Commit(_snapshot with
            {
                Resources = attempt.Resources,
                SchoolResearch = attempt.State,
                PencilPaidStudents = checked(_snapshot.PencilPaidStudents +
                    (_school.PencilsPerStudent > 0 ? count : 0))
            });
        return attempt.Failure;
    }

    public SchoolActionFailure CancelStudents(int count)
    {
        var prepaid = Math.Min(count, _snapshot.PencilPaidStudents);
        var attempt = SchoolResearch.CancelTraining(_school, _snapshot.SchoolResearch,
            _snapshot.Resources, StorageCapacity(), _snapshot.SchoolCount, count, prepaid);
        if (attempt.Succeeded)
            Commit(_snapshot with
            {
                Resources = attempt.Resources,
                SchoolResearch = attempt.State,
                PencilPaidStudents = _snapshot.PencilPaidStudents - prepaid
            });
        return attempt.Failure;
    }

    public SchoolActionFailure AssignResearchers(int count) => ApplyStaffing(
        SchoolResearch.Assign(_school, _snapshot.SchoolResearch, _snapshot.SchoolCount, count));

    public SchoolActionFailure UnassignResearchers(int count) => ApplyStaffing(
        SchoolResearch.Unassign(_school, _snapshot.SchoolResearch, _snapshot.SchoolCount, count));

    public SchoolActionFailure ReturnResearchersToCivilian(int count) => ApplyStaffing(
        SchoolResearch.ReturnToCivilian(_school, _snapshot.SchoolResearch, _snapshot.SchoolCount, count));

    private SchoolActionFailure ApplyStaffing(SchoolStaffingResult result)
    {
        if (result.Succeeded) Commit(_snapshot with { SchoolResearch = result.State });
        return result.Failure;
    }

    public TechnologyPurchaseFailure BuyTechnology(string technologyId)
    {
        if (string.IsNullOrWhiteSpace(technologyId)) return TechnologyPurchaseFailure.UnknownTechnology;
        var ledger = ResearchLedger.Restore(ResearchBalance, _snapshot.UnlockedTechnologyIds, _technologies);
        var owned = new HashSet<string>(_snapshot.OtherBuildings.Where(item => item.Count > 0)
            .Select(item => item.BuildingId), StringComparer.Ordinal);
        foreach (var item in _snapshot.ProductiveFamilies)
            if ((long)item.Base.Buildings + item.Large.Buildings + item.Advanced.Buildings > 0)
                owned.Add(item.FamilyId);
        if (_snapshot.SchoolCount > 0) owned.Add(V14CampaignSnapshots.SchoolId);
        var attempt = TechnologyPurchases.TryUnlock(_technologies, ledger, technologyId, owned);
        if (attempt.Succeeded)
            Commit(_snapshot with
            {
                ResearchBalanceMilliPoints = attempt.Ledger.Balance.MilliPoints,
                UnlockedTechnologyIds = attempt.Ledger.UnlockedTechnologies
                    .OrderBy(id => id, StringComparer.Ordinal).ToImmutableArray()
            });
        return attempt.Failure;
    }

    /// <summary>
    /// Construct a new producer of a chosen size. A multi-purchase uses successive family
    /// prices; only a successful, validated transition replaces the snapshot.
    /// </summary>
    public V14TierActionResult BuildProducer(string familyId, ProductionTier tier, int quantity)
    {
        if (quantity <= 0) return TierFailure(V14TierActionFailure.InvalidQuantity);
        if (!ValidTier(tier)) return TierFailure(V14TierActionFailure.InvalidTier);
        if (familyId is null || !_productiveCatalog.TryGetValue(familyId, out var definition))
            return TierFailure(V14TierActionFailure.UnknownFamily);
        if (!HasBuildingPrerequisites(definition))
            return TierFailure(V14TierActionFailure.MissingPrerequisite);
        try
        {
            var current = GetPortfolio(familyId);
            var purchase = TieredConstruction.TryBuild(definition, current, tier, quantity,
                _tierConstruction.GrowthFactor, _snapshot.UnlockedTechnologyIds.ToHashSet(StringComparer.Ordinal),
                _snapshot.Resources, PurchaseHistory());
            if (!purchase.Succeeded)
                return TierFailure(MapPurchaseFailure(purchase.Failure), purchase.Cost);
            Commit(_snapshot with
            {
                Resources = purchase.Resources,
                ProductiveFamilies = ReplacePortfolio(purchase.Portfolio),
                PurchaseCounters = ReplaceCounter(familyId, purchase.History.Acquired(familyId))
            });
            return TierFailure(V14TierActionFailure.None, purchase.Cost);
        }
        catch (OverflowException)
        {
            return TierFailure(V14TierActionFailure.NumericLimit);
        }
    }

    /// <summary>Replace one producer by a larger one; do not increment purchase history.</summary>
    public V14TierActionResult UpgradeProducer(string familyId, ProductionTier from, ProductionTier to)
    {
        if (!ValidTier(from) || !ValidTier(to) || from >= to)
            return TierFailure(V14TierActionFailure.InvalidTier);
        if (familyId is null || !_productiveCatalog.TryGetValue(familyId, out var definition))
            return TierFailure(V14TierActionFailure.UnknownFamily);
        if (!HasBuildingPrerequisites(definition))
            return TierFailure(V14TierActionFailure.MissingPrerequisite);
        try
        {
            var purchase = TieredConstruction.TryUpgrade(definition, GetPortfolio(familyId), from, to,
                _tierConstruction.GrowthFactor, _snapshot.UnlockedTechnologyIds.ToHashSet(StringComparer.Ordinal),
                _snapshot.Resources, PurchaseHistory());
            if (!purchase.Succeeded)
                return TierFailure(MapPurchaseFailure(purchase.Failure), purchase.Cost);
            Commit(_snapshot with
            {
                Resources = purchase.Resources,
                ProductiveFamilies = ReplacePortfolio(purchase.Portfolio)
                // Upgrades deliberately leave PurchaseCounters unchanged.
            });
            return TierFailure(V14TierActionFailure.None, purchase.Cost);
        }
        catch (OverflowException)
        {
            return TierFailure(V14TierActionFailure.NumericLimit);
        }
    }

    /// <summary>Staff only existing vacancies using unassigned inhabitants.</summary>
    public V14TierActionFailure AssignProducerWorkers(string familyId, ProductionTier tier, int workers)
    {
        if (workers <= 0) return V14TierActionFailure.InvalidQuantity;
        if (!ValidTier(tier)) return V14TierActionFailure.InvalidTier;
        if (familyId is null || !_productiveCatalog.ContainsKey(familyId))
            return V14TierActionFailure.UnknownFamily;
        try
        {
            var portfolio = GetPortfolio(familyId);
            var slot = portfolio.At(tier);
            if ((long)slot.AssignedWorkers + workers > (long)slot.Buildings * Workplaces(tier))
                return V14TierActionFailure.InsufficientWorkplaces;
            if (workers > AvailablePopulation) return V14TierActionFailure.InsufficientPopulation;
            Commit(_snapshot with
            {
                ProductiveFamilies = ReplacePortfolio(portfolio.Assign(tier, workers, AvailablePopulation))
            });
            return V14TierActionFailure.None;
        }
        catch (OverflowException)
        {
            return V14TierActionFailure.NumericLimit;
        }
    }

    /// <summary>Return assigned producers to the free population; retain building counts.</summary>
    public V14TierActionFailure ReleaseProducerWorkers(string familyId, ProductionTier tier, int workers)
    {
        if (workers <= 0) return V14TierActionFailure.InvalidQuantity;
        if (!ValidTier(tier)) return V14TierActionFailure.InvalidTier;
        if (familyId is null || !_productiveCatalog.ContainsKey(familyId))
            return V14TierActionFailure.UnknownFamily;
        try
        {
            var portfolio = GetPortfolio(familyId);
            if (workers > portfolio.At(tier).AssignedWorkers)
                return V14TierActionFailure.InsufficientAssignedWorkers;
            Commit(_snapshot with { ProductiveFamilies = ReplacePortfolio(portfolio.Release(tier, workers)) });
            return V14TierActionFailure.None;
        }
        catch (OverflowException)
        {
            return V14TierActionFailure.NumericLimit;
        }
    }

    private TieredBuildingPortfolio GetPortfolio(string familyId)
    {
        var state = _snapshot.ProductiveFamilies.Single(item => item.FamilyId == familyId);
        return TieredBuildingPortfolio.Create(familyId, state.Base, state.Large, state.Advanced);
    }

    private ConstructionAcquisitionHistory PurchaseHistory() => ConstructionAcquisitionHistory.FromOwned(
        _snapshot.PurchaseCounters.Select(item => (item.FamilyId, item.Acquired)));

    private ImmutableArray<V14FamilySnapshot> ReplacePortfolio(TieredBuildingPortfolio portfolio) =>
        _snapshot.ProductiveFamilies.Select(item => item.FamilyId == portfolio.FamilyId
            ? new V14FamilySnapshot(portfolio.FamilyId, portfolio.Base, portfolio.Large, portfolio.Advanced)
            : item).ToImmutableArray();

    private ImmutableArray<V14PurchaseCounter> ReplaceCounter(string familyId, int acquired) =>
        _snapshot.PurchaseCounters.Select(item => item.FamilyId == familyId
            ? item with { Acquired = acquired } : item).ToImmutableArray();

    private bool HasBuildingPrerequisites(TieredProductionFamily definition) =>
        definition.Source.Prerequisites.All(OwnsBuilding);

    private static bool ValidTier(ProductionTier tier) => tier is
        ProductionTier.Base or ProductionTier.Large or ProductionTier.Advanced;

    private static int Workplaces(ProductionTier tier) => tier switch
    {
        ProductionTier.Base => 1,
        ProductionTier.Large => 3,
        ProductionTier.Advanced => 10,
        _ => throw new ArgumentOutOfRangeException(nameof(tier))
    };

    private static V14TierActionFailure MapPurchaseFailure(TieredPurchaseFailure failure) => failure switch
    {
        TieredPurchaseFailure.MissingResearch => V14TierActionFailure.MissingResearch,
        TieredPurchaseFailure.MissingSourceBuilding => V14TierActionFailure.MissingSourceBuilding,
        TieredPurchaseFailure.InsufficientResources => V14TierActionFailure.InsufficientResources,
        _ => throw new ArgumentOutOfRangeException(nameof(failure))
    };

    private static V14TierActionResult TierFailure(V14TierActionFailure failure, ResourceBundle? cost = null) =>
        new(failure, cost ?? ResourceBundle.Empty);

    private int CivilianWorkers => checked((int)(
        _snapshot.OtherBuildings.Sum(item => (long)item.AssignedWorkers) +
        _snapshot.ProductiveFamilies.Sum(item => (long)item.Base.AssignedWorkers +
            item.Large.AssignedWorkers + item.Advanced.AssignedWorkers)));

    private ResourceBundle StorageCapacity()
    {
        var capacity = _baseline.Capacity;
        foreach (var definition in _baseline.Catalog.Buildings)
        {
            if (definition.StoragePerBuilding is not { } perBuilding) continue;
            var productive = _snapshot.ProductiveFamilies.FirstOrDefault(item => item.FamilyId == definition.Id);
            var equivalents = productive is not null
                ? checked(productive.Base.Buildings + checked(3 * productive.Large.Buildings) +
                    checked(10 * productive.Advanced.Buildings))
                : _snapshot.OtherBuildings.Single(item => item.BuildingId == definition.Id).Count;
            if (equivalents > 0) capacity += perBuilding.Scale(equivalents);
        }
        return capacity + ResourceBundle.FromAmounts([
            KeyValuePair.Create(V14SchoolSupplies.ResourceId, V14SchoolSupplies.Capacity(_baseline, _snapshot))]);
    }
}
