using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Population;
using SimRisorse.Core.Research;

namespace SimRisorse.Core.Sessions;

/// <summary>
/// Proposed self-contained civil-growth-14 checkpoint. It deliberately does not change
/// GameSnapshot v2, the active v13 GameSession, or any existing SQLite file.
/// School count is stored separately until the school is added to the next catalog.
/// </summary>
public sealed record V14FamilySnapshot(string FamilyId, TierOccupancy Base,
    TierOccupancy Large, TierOccupancy Advanced);

public sealed record V14PurchaseCounter(string FamilyId, int Acquired);

public sealed record V14CampaignSnapshot(
    int FormatVersion, string ScenarioId, string RulesVersion, long ElapsedTicks,
    int Population, ResourceBundle Resources,
    ImmutableArray<BuildingState> OtherBuildings,
    ImmutableArray<V14FamilySnapshot> ProductiveFamilies,
    ImmutableArray<V14PurchaseCounter> PurchaseCounters,
    int SchoolCount,
    int GrowthProgressSeconds, int GrowthRemainderTwentieths,
    MilitaryTrainingState Military,
    SchoolResearchState SchoolResearch,
    long ResearchBalanceMilliPoints,
    ImmutableArray<string> UnlockedTechnologyIds, int PencilPaidStudents = 0);

/// <summary>
/// Explicit and strict v13 -> proposed v14 migration contract. No database writes,
/// no silently added population, no automatically unlocked technologies.
/// The returned shape can also validate later v14 upgrades and workforce changes.
/// </summary>
public static class V14CampaignSnapshots
{
    public const string RulesVersion = "civil-growth-14";
    public const int FormatVersion = 3;
    public const string SchoolId = "school";

    public static V14CampaignSnapshot MigrateFromV13(GameScenario sourceScenario, GameSnapshot snapshot,
        SchoolResearchRules schoolRules, TechnologyCatalog technologies)
    {
        ArgumentNullException.ThrowIfNull(sourceScenario);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(schoolRules);
        ArgumentNullException.ThrowIfNull(technologies);
        if (sourceScenario.Id != "civil-growth" || sourceScenario.Catalog.RulesVersion != "civil-growth-13" ||
            snapshot.RulesVersion != sourceScenario.Catalog.RulesVersion || snapshot.FormatVersion != GameSession.SnapshotFormatVersion)
            throw new ArgumentException("Only an intact civil-growth-13 checkpoint can enter this migration.", nameof(snapshot));
        // Reuse all existing fixed-Municipio, military, stock, housing and population validations.
        _ = GameSession.Restore(sourceScenario, snapshot);
        var productive = TieredProductionCatalog.FromCatalog(sourceScenario.Catalog, 2.6m, 6.5m);
        if (productive.Length != TieredProductionCatalog.KnownFamilyIds.Length)
            throw new ArgumentException("The v13 catalog is missing a productive family.", nameof(sourceScenario));
        var producerIds = productive.Select(item => item.FamilyId).ToHashSet(StringComparer.Ordinal);
        var byId = snapshot.Buildings.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        var families = productive.Select(family =>
        {
            var legacy = byId[family.FamilyId];
            var converted = TieredBuildingPortfolio.FromLegacy(family.Source, legacy.Count, legacy.AssignedWorkers);
            return new V14FamilySnapshot(family.FamilyId, converted.Base, converted.Large, converted.Advanced);
        }).OrderBy(item => item.FamilyId, StringComparer.Ordinal).ToImmutableArray();
        var otherBuildings = snapshot.Buildings.Where(item => !producerIds.Contains(item.BuildingId)).ToImmutableArray();
        var counters = sourceScenario.Catalog.Buildings
            .Where(item => !item.IsScenarioFixed)
            .Select(item => new V14PurchaseCounter(item.Id, byId[item.Id].Count))
            .Append(new V14PurchaseCounter(SchoolId, 0))
            .OrderBy(item => item.FamilyId, StringComparer.Ordinal).ToImmutableArray();
        var migrated = new V14CampaignSnapshot(FormatVersion, snapshot.ScenarioId, RulesVersion,
            snapshot.ElapsedTicks, snapshot.Population, snapshot.Resources, otherBuildings, families,
            counters, 0, snapshot.GrowthProgressSeconds!.Value, snapshot.GrowthRemainderTwentieths,
            snapshot.Military!, SchoolResearchState.Empty, 0, ImmutableArray<string>.Empty);
        Validate(sourceScenario, migrated, schoolRules, technologies);
        return migrated;
    }

    public static void Validate(GameScenario baselineV13, V14CampaignSnapshot candidate,
        SchoolResearchRules schoolRules, TechnologyCatalog technologies)
    {
        ArgumentNullException.ThrowIfNull(baselineV13);
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(schoolRules);
        ArgumentNullException.ThrowIfNull(technologies);
        if (baselineV13.Catalog.RulesVersion != "civil-growth-13" || baselineV13.Id != "civil-growth" ||
            baselineV13.MilitaryRules is null || baselineV13.PopulationRules is null ||
            !baselineV13.Catalog.ResourcesById.ContainsKey(schoolRules.FoodResourceId) ||
            candidate.FormatVersion != FormatVersion || candidate.RulesVersion != RulesVersion ||
            candidate.ScenarioId != baselineV13.Id || candidate.ElapsedTicks < 0 ||
            candidate.Population < baselineV13.Population || candidate.Resources is null ||
            candidate.OtherBuildings.IsDefault || candidate.ProductiveFamilies.IsDefault ||
            candidate.PurchaseCounters.IsDefault || candidate.UnlockedTechnologyIds.IsDefault ||
            candidate.SchoolCount < 0 || candidate.SchoolResearch is null || candidate.Military is null ||
            candidate.PencilPaidStudents < 0 || candidate.PencilPaidStudents >
                (long)candidate.SchoolResearch.WaitingStudents + candidate.SchoolResearch.TrainingStudents)
            throw new ArgumentException("Invalid advanced campaign checkpoint header.", nameof(candidate));

        var catalog = baselineV13.Catalog;
        // Pencils are a new v14-only stock entry; every other resource must be in v13.
        catalog.ValidateBundle(ResourceBundle.FromAmounts(candidate.Resources.Amounts
            .Where(item => item.Key != V14SchoolSupplies.ResourceId)));
        var availableFamilies = TieredProductionCatalog.FromCatalog(catalog, 2.6m, 6.5m);
        if (availableFamilies.Length != TieredProductionCatalog.KnownFamilyIds.Length ||
            candidate.ProductiveFamilies.Length != availableFamilies.Length)
            throw new ArgumentException("A productive family is missing or unexpected.", nameof(candidate));
        var knownFamilies = availableFamilies.ToDictionary(item => item.FamilyId, StringComparer.Ordinal);
        var currentFamilies = new Dictionary<string, TieredBuildingPortfolio>(StringComparer.Ordinal);
        foreach (var family in candidate.ProductiveFamilies)
        {
            if (family is null || !knownFamilies.ContainsKey(family.FamilyId) ||
                !currentFamilies.TryAdd(family.FamilyId, TieredBuildingPortfolio.Create(family.FamilyId,
                    family.Base, family.Large, family.Advanced)))
                throw new ArgumentException("Duplicate or unknown productive family.", nameof(candidate));
        }

        var ledger = ResearchLedger.Restore(ResearchPoints.FromMilliPoints(candidate.ResearchBalanceMilliPoints),
            candidate.UnlockedTechnologyIds, technologies);
        foreach (var family in currentFamilies.Values)
        {
            var definition = knownFamilies[family.FamilyId];
            if ((family.Large.Buildings > 0 && !definition.IsUnlocked(ProductionTier.Large, ledger.UnlockedTechnologies)) ||
                (family.Advanced.Buildings > 0 && !definition.IsUnlocked(ProductionTier.Advanced, ledger.UnlockedTechnologies)))
                throw new ArgumentException("An advanced building lacks its required research.", nameof(candidate));
        }

        var others = new Dictionary<string, BuildingState>(StringComparer.Ordinal);
        foreach (var state in candidate.OtherBuildings)
        {
            if (state is null || !catalog.BuildingsById.TryGetValue(state.BuildingId, out var definition) ||
                currentFamilies.ContainsKey(state.BuildingId) || !others.TryAdd(state.BuildingId, state) ||
                state.Count < 0 || state.AssignedWorkers < 0 ||
                state.AssignedWorkers > (long)state.Count * definition.WorkplacesPerBuilding)
                throw new ArgumentException("Unknown or invalid un-tiered building.", nameof(candidate));
            if (definition.IsScenarioFixed)
            {
                var initial = baselineV13.InitialBuildings.Single(item => item.BuildingId == state.BuildingId);
                if (state != initial)
                    throw new ArgumentException("The starting Municipio cannot be modified.", nameof(candidate));
            }
        }
        if (others.Count + currentFamilies.Count != catalog.Buildings.Length)
            throw new ArgumentException("A building type is missing.", nameof(candidate));

        var ownedById = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var state in others.Values)
            if (!catalog.BuildingsById[state.BuildingId].IsScenarioFixed)
                ownedById.Add(state.BuildingId, state.Count);
        foreach (var family in currentFamilies.Values) ownedById.Add(family.FamilyId, family.TotalBuildings);
        ownedById.Add(SchoolId, candidate.SchoolCount);
        var seenCounters = new HashSet<string>(StringComparer.Ordinal);
        foreach (var counter in candidate.PurchaseCounters)
        {
            if (counter is null || !seenCounters.Add(counter.FamilyId) ||
                !ownedById.TryGetValue(counter.FamilyId, out var owned) || counter.Acquired < owned)
                throw new ArgumentException("Invalid, duplicated or rolled-back purchase history.", nameof(candidate));
        }
        if (seenCounters.Count != ownedById.Count)
            throw new ArgumentException("Acquisition counters are incomplete.", nameof(candidate));

        var barracks = others[baselineV13.MilitaryRules!.BarracksId];
        MilitaryTraining.Validate(candidate.Military, baselineV13.MilitaryRules, barracks.Count);
        SchoolResearch.Validate(schoolRules, candidate.SchoolResearch, candidate.SchoolCount);
        var assigned = others.Values.Sum(item => (long)item.AssignedWorkers) +
            currentFamilies.Values.Sum(item => (long)item.AssignedWorkers) +
            candidate.Military.EnlistedPopulation + candidate.SchoolResearch.ReservedPopulation;
        if (assigned > candidate.Population)
            throw new ArgumentException("Workers, researchers and soldiers exceed population.", nameof(candidate));

        var combined = others.Values.Concat(currentFamilies.Values.Select(item =>
            new BuildingState(item.FamilyId, item.TotalBuildings, item.AssignedWorkers))).ToImmutableArray();
        var growthRules = baselineV13.PopulationRules!;
        if (candidate.Population > baselineV13.HousingCapacity(combined) ||
            candidate.GrowthProgressSeconds < 0 || candidate.GrowthProgressSeconds >= growthRules.GrowthIntervalSeconds ||
            candidate.GrowthRemainderTwentieths is < 0 or >= HousingGrowthRules.UnitsPerSecond)
            throw new ArgumentException("Invalid housing or population growth.", nameof(candidate));

        var capacity = baselineV13.Capacity;
        foreach (var definition in catalog.Buildings)
        {
            if (definition.StoragePerBuilding is not { } perBuilding) continue;
            var equivalents = currentFamilies.TryGetValue(definition.Id, out var family)
                ? family.StorageEquivalents : others[definition.Id].Count;
            if (equivalents > 0) capacity += perBuilding.Scale(equivalents);
        }
        capacity += ResourceBundle.FromAmounts([
            KeyValuePair.Create(V14SchoolSupplies.ResourceId, V14SchoolSupplies.Capacity(baselineV13, candidate))]);
        if (!capacity.Covers(candidate.Resources))
            throw new ArgumentException("Resources exceed storage capacity after migration.", nameof(candidate));
    }
}
