using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;

namespace SimRisorse.Core.Food;

/// <summary>
/// Read-only growth forecast for the v15 sidebar: same PopulationPlanner and
/// housing coefficients used by the world clock. Never advances the campaign.
/// </summary>
public static class V15CampaignGrowthProjection
{
    public const int MaximumGrowthMultiplier = 20;

    // One growth policy for both the v15 clock and its read-only forecast.
    internal static int GrowthRate(HousingGrowthRules? rules,
        IReadOnlyDictionary<string, BuildingState> buildings) => rules is null
        ? HousingGrowthRules.UnitsPerSecond
        : rules.Rate(buildings[rules.SmallHouseId].Count, buildings[rules.LargeHouseId].Count,
            MaximumGrowthMultiplier * HousingGrowthRules.UnitsPerSecond);

    public static PopulationForecast Project(GameScenario baseline, V15FoodCampaignCheckpoint checkpoint)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(checkpoint);
        var city = checkpoint.CityCampaign;
        var buildings = city.OtherBuildings.ToDictionary(b => b.BuildingId, StringComparer.Ordinal);
        foreach (var family in city.ProductiveFamilies)
            buildings.Add(family.FamilyId, new BuildingState(family.FamilyId,
                checked(family.Base.Buildings + family.Large.Buildings + family.Advanced.Buildings),
                checked(family.Base.AssignedWorkers + family.Large.AssignedWorkers + family.Advanced.AssignedWorkers)));
        var housing = baseline.HousingCapacity(buildings.Values);
        var basicHousing = baseline.BasicHousingCapacity(buildings.Values);
        var rules = baseline.PopulationRules;
        var rate = GrowthRate(rules?.HousingGrowth, buildings);
        var food = rules is null ? ResourceBundle.Empty : ResourceBundle.FromAmounts([
            KeyValuePair.Create(rules.FoodResourceId, checkpoint.Depot.ReadyFood),
            KeyValuePair.Create(rules.AdvancedHousing?.BreadResourceId ?? "bread",
                checkpoint.Depot.RawFood[rules.AdvancedHousing?.BreadResourceId ?? "bread"])
        ]);
        return PopulationPlanner.Plan(rules, city.Population, housing, basicHousing,
            city.GrowthProgressSeconds, city.GrowthRemainderTwentieths, rate, food);
    }
}
