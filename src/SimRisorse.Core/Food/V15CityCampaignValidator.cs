using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Population;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Core.Food;

/// <summary>
/// RC66: validates the authoritative civil slice of a v15 campaign against the
/// 15 live productive families, without padding with the retired food-store.
/// The actual validation works on the dedicated V15CityCampaignSnapshot; a
/// strict conversion boundary remains for legacy-shaped persistence consumers.
/// A populated retired site is accepted only as an explicitly unretired import
/// awaiting the existing retirement command; SQLite generation 6 refuses it.
/// </summary>
public static class V15CityCampaignValidator
{
    public static void Validate(GameScenario baseline, V14CampaignSnapshot city,
        SchoolResearchRules school, TechnologyCatalog technologies) =>
        ValidateNative(baseline, V15CityCampaignSnapshot.FromInherited(city), school, technologies);

    /// <summary>Native v15 validation. No legacy producer projection or v14
    /// checkpoint validation is performed.</summary>
    public static void ValidateNative(GameScenario baseline, V15CityCampaignSnapshot city,
        SchoolResearchRules school, TechnologyCatalog technologies)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(school);
        ArgumentNullException.ThrowIfNull(technologies);
        if (baseline.Id != "civil-growth" || baseline.Catalog.RulesVersion != "civil-growth-13" ||
            baseline.MilitaryRules is null || baseline.PopulationRules is null ||
            !baseline.Catalog.ResourcesById.ContainsKey(school.FoodResourceId) ||
            city.ScenarioId != baseline.Id || city.ElapsedTicks < 0 ||
            city.Population < baseline.Population || city.Resources is null ||
            city.OtherBuildings.IsDefault || city.ProductiveFamilies.IsDefault ||
            city.PurchaseCounters.IsDefault || city.UnlockedTechnologyIds.IsDefault ||
            city.SchoolCount < 0 || city.SchoolResearch is null || city.Military is null ||
            city.PencilPaidStudents < 0 || city.PencilPaidStudents >
                (long)city.SchoolResearch.WaitingStudents + city.SchoolResearch.TrainingStudents)
            throw new ArgumentException("Invalid v15 city checkpoint header.", nameof(city));

        // The baseline does not know pencils, but every other city-owned
        // resource must be registered in its catalog.
        baseline.Catalog.ValidateBundle(ResourceBundle.FromAmounts(city.Resources.Amounts
            .Where(item => item.Key != V14SchoolSupplies.ResourceId)));

        var definitions = TieredProductionCatalog.FromCatalog(baseline.Catalog, 2.6m, 6.5m)
            .ToDictionary(f => f.FamilyId, StringComparer.Ordinal);
        var liveDefinitions = definitions
            .Where(item => item.Key != V15LegacyProducerAdapter.RetiredFamilyId)
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        if (liveDefinitions.Count != TieredProductionCatalog.KnownFamilyIds.Length - 1)
            throw new ArgumentException("The source catalog cannot define the 15 v15 producers.", nameof(baseline));

        var families = new Dictionary<string, TieredBuildingPortfolio>(StringComparer.Ordinal);
        TieredBuildingPortfolio? retired = null;
        foreach (var family in city.ProductiveFamilies)
        {
            if (family is null || !families.TryAdd(family.FamilyId,
                TieredBuildingPortfolio.Create(family.FamilyId, family.Base, family.Large, family.Advanced)))
                throw new ArgumentException("Duplicate or invalid v15 productive family.", nameof(city));
            if (family.FamilyId == V15LegacyProducerAdapter.RetiredFamilyId)
                retired = families[family.FamilyId];
            else if (!liveDefinitions.ContainsKey(family.FamilyId))
                throw new ArgumentException("Unknown v15 productive family.", nameof(city));
        }
        if (families.Count != liveDefinitions.Count + (retired is null ? 0 : 1) ||
            liveDefinitions.Keys.Any(id => !families.ContainsKey(id)))
            throw new ArgumentException("A v15 productive family is missing or unexpected.", nameof(city));

        var ledger = ResearchLedger.Restore(ResearchPoints.FromMilliPoints(city.ResearchBalanceMilliPoints),
            city.UnlockedTechnologyIds, technologies);
        foreach (var family in families.Values)
        {
            // Legacy occupied imports must also satisfy their original research
            // requirements until explicitly retired, but are never run by v15.
            var definition = definitions[family.FamilyId];
            if (family.Large.Buildings > 0 && !definition.IsUnlocked(ProductionTier.Large, ledger.UnlockedTechnologies) ||
                family.Advanced.Buildings > 0 && !definition.IsUnlocked(ProductionTier.Advanced, ledger.UnlockedTechnologies))
                throw new ArgumentException("An advanced v15 building lacks research.", nameof(city));
        }

        var others = new Dictionary<string, BuildingState>(StringComparer.Ordinal);
        foreach (var state in city.OtherBuildings)
        {
            if (state is null || !baseline.Catalog.BuildingsById.TryGetValue(state.BuildingId, out var definition) ||
                liveDefinitions.ContainsKey(state.BuildingId) ||
                state.BuildingId == V15LegacyProducerAdapter.RetiredFamilyId ||
                !others.TryAdd(state.BuildingId, state) ||
                state.Count < 0 || state.AssignedWorkers < 0 ||
                state.AssignedWorkers > (long)state.Count * definition.WorkplacesPerBuilding)
                throw new ArgumentException("Unknown, duplicated or invalid v15 civic building.", nameof(city));
            if (definition.IsScenarioFixed &&
                state != baseline.InitialBuildings.Single(b => b.BuildingId == state.BuildingId))
                throw new ArgumentException("The starting Municipio cannot be modified.", nameof(city));
        }
        if (others.Count + liveDefinitions.Count != baseline.Catalog.Buildings.Length - 1)
            throw new ArgumentException("An inherited civic building is missing.", nameof(city));

        var owned = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var state in others.Values)
            if (!baseline.Catalog.BuildingsById[state.BuildingId].IsScenarioFixed)
                owned.Add(state.BuildingId, state.Count);
        foreach (var family in families.Values)
            if (family.FamilyId != V15LegacyProducerAdapter.RetiredFamilyId)
                owned.Add(family.FamilyId, family.TotalBuildings);
        owned.Add(V14CampaignSnapshots.SchoolId, city.SchoolCount);
        // Old acquisition history may still contain food-store after retirement;
        // it is historical accounting, not a productive building or extra storage.
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var counter in city.PurchaseCounters)
        {
            if (counter is null || !seen.Add(counter.FamilyId))
                throw new ArgumentException("Duplicate or null v15 purchase counter.", nameof(city));
            var minimum = counter.FamilyId == V15LegacyProducerAdapter.RetiredFamilyId
                ? retired?.TotalBuildings ?? 0
                : owned.TryGetValue(counter.FamilyId, out var count) ? count : -1;
            if (minimum < 0 || counter.Acquired < minimum)
                throw new ArgumentException("Invalid or rolled-back v15 purchase history.", nameof(city));
        }
        if (owned.Keys.Any(id => !seen.Contains(id)))
            throw new ArgumentException("The v15 purchase history is incomplete.", nameof(city));

        var militaryRules = baseline.MilitaryRules!;
        var barracks = others[militaryRules.BarracksId];
        MilitaryTraining.Validate(city.Military, militaryRules, barracks.Count);
        SchoolResearch.Validate(school, city.SchoolResearch, city.SchoolCount);
        var assigned = others.Values.Sum(b => (long)b.AssignedWorkers) +
            families.Values.Sum(f => (long)f.AssignedWorkers) +
            city.Military.EnlistedPopulation + city.SchoolResearch.ReservedPopulation;
        if (assigned > city.Population)
            throw new ArgumentException("Workers, school and military exceed v15 population.", nameof(city));

        var buildings = others.Values.Concat(families.Values
            .Where(f => f.FamilyId != V15LegacyProducerAdapter.RetiredFamilyId)
            .Select(f => new BuildingState(f.FamilyId, f.TotalBuildings, f.AssignedWorkers)))
            .ToImmutableArray();
        var growth = baseline.PopulationRules!;
        if (city.Population > baseline.HousingCapacity(buildings) ||
            city.GrowthProgressSeconds < 0 || city.GrowthProgressSeconds >= growth.GrowthIntervalSeconds ||
            city.GrowthRemainderTwentieths is < 0 or >= HousingGrowthRules.UnitsPerSecond)
            throw new ArgumentException("Invalid v15 housing or population growth.", nameof(city));

        // This is the *physical* v15 city capacity. It intentionally excludes
        // food-store's storage and the prepared-food depot's own compartments.
        if (!V15CityStorage.Calculate(baseline, city).Covers(city.Resources))
            throw new ArgumentException("v15 city stock exceeds real storage capacity.", nameof(city));
    }
}
