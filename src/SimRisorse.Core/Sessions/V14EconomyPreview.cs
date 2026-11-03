using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Research;

namespace SimRisorse.Core.Sessions;

/// <summary>Read-only next-second projection: same planners and priority as the actual v14 clock.</summary>
public sealed record V14ResourceRate(string Id, decimal ProductionPerSecond, decimal ConsumptionPerSecond);

public sealed record V14EconomyPreview(ImmutableArray<V14ResourceRate> Resources,
    PopulationForecast Population, ImmutableArray<BuildingProduction> Buildings);

internal static class V14EconomyProjector
{
    public static V14EconomyPreview Project(GameScenario scenario, V14CampaignSnapshot state, ResourceBundle capacity)
    {
        var buildings = state.OtherBuildings.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        foreach (var family in state.ProductiveFamilies)
            buildings.Add(family.FamilyId, new BuildingState(family.FamilyId,
                checked(family.Base.Buildings + family.Large.Buildings + family.Advanced.Buildings),
                checked(family.Base.AssignedWorkers + family.Large.AssignedWorkers + family.Advanced.AssignedWorkers)));
        var housing = scenario.HousingCapacity(buildings.Values);
        var basic = scenario.BasicHousingCapacity(buildings.Values);
        var growth = scenario.PopulationRules?.HousingGrowth;
        var rate = growth is not null
            ? growth.Rate(buildings[growth.SmallHouseId].Count, buildings[growth.LargeHouseId].Count)
            : HousingGrowthRules.UnitsPerSecond;
        var population = PopulationPlanner.Plan(scenario.PopulationRules, state.Population, housing, basic,
            state.GrowthProgressSeconds, state.GrowthRemainderTwentieths, rate, state.Resources);
        var plan = ProductionPlanner.Plan(scenario.Catalog, state.Resources, capacity, buildings,
            PopulationPlanner.Consumption(population), includeDiagnostics: true);
        var reports = plan.Buildings.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        var lines = scenario.Catalog.Resources.Select(resource =>
        {
            decimal flow(bool output) => scenario.Catalog.Buildings.Sum(def =>
                (def.Recipe is null ? 0m : (output ? def.Recipe.Output : def.Recipe.Input)[resource.Id].Units)
                * reports[def.Id].RunnableWorkers);
            var consumed = flow(false) +
                (resource.Id == population.FoodResourceId ? population.ConsumedFood.Units : 0m) +
                (resource.Id == population.BreadResourceId ? population.ConsumedBread.Units : 0m);
            return new V14ResourceRate(resource.Id, flow(true), consumed);
        }).Append(new V14ResourceRate(V14SchoolSupplies.ResourceId, 0m, 0m)).ToImmutableArray();
        return new(lines, population, plan.Buildings);
    }
}
