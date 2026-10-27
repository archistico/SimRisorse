using System.Collections.Immutable;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Population;

public enum GrowthBlockReason { None, FixedPopulation, HousingFull, MissingFood, MissingBread }

public sealed record PopulationForecast(int HousingCapacity, int ProgressSeconds, int IntervalSeconds,
    string? FoodResourceId, ResourceAmount RequiredFood, ResourceAmount ConsumedFood, GrowthBlockReason BlockReason,
    int BasicHousingCapacity, int AdvancedResidents, string? BreadResourceId,
    ResourceAmount RequiredBread, ResourceAmount ConsumedBread,
    int GrowthRemainderTwentieths = 0, int GrowthRateTwentieths = HousingGrowthRules.UnitsPerSecond);

public sealed record EconomyForecast(ImmutableArray<BuildingProduction> Buildings, PopulationForecast Population);

internal static class PopulationPlanner
{
    public static PopulationForecast Plan(PopulationRules? rules, int population, int housing, int basicHousing,
        int progress, int remainderTwentieths, int growthRateTwentieths, ResourceBundle stocks)
    {
        if (rules is null) return new(housing, 0, 0, null, default, default, GrowthBlockReason.FixedPopulation,
            housing, 0, null, default, default);
        var required = rules.FoodPerPersonPerSecond * population;
        var fed = stocks[rules.FoodResourceId].Milliunits >= required.Milliunits;
        var advancedRules = rules.AdvancedHousing;
        var advancedResidents = Math.Max(0, population - basicHousing);
        var requiredBread = advancedRules is null ? default : advancedRules.BreadPerPersonPerSecond * advancedResidents;
        var enoughBread = advancedRules is null || stocks[advancedRules.BreadResourceId].Milliunits >= requiredBread.Milliunits;
        var nextAdvancedResidents = population < housing ? Math.Max(0, population + 1 - basicHousing) : advancedResidents;
        var enoughForArrival = advancedRules is null || stocks[advancedRules.BreadResourceId].Milliunits >=
            (advancedRules.BreadPerPersonPerSecond * nextAdvancedResidents).Milliunits;
        var reason = !fed ? GrowthBlockReason.MissingFood : !enoughBread ? GrowthBlockReason.MissingBread :
            population >= housing ? GrowthBlockReason.HousingFull : !enoughForArrival ? GrowthBlockReason.MissingBread : GrowthBlockReason.None;
        return new(housing, progress, rules.GrowthIntervalSeconds, rules.FoodResourceId, required, fed ? required : default,
            reason, basicHousing, advancedResidents, advancedRules?.BreadResourceId, requiredBread,
            fed && enoughBread ? requiredBread : default, remainderTwentieths, growthRateTwentieths);
    }

    public static ResourceBundle Consumption(PopulationForecast forecast)
    {
        if (forecast.FoodResourceId is not { } id) return ResourceBundle.Empty;
        return forecast.BreadResourceId is { } bread
            ? ResourceBundle.FromAmounts([KeyValuePair.Create(id, forecast.ConsumedFood),
                KeyValuePair.Create(bread, forecast.ConsumedBread)])
            : ResourceBundle.FromAmounts([KeyValuePair.Create(id, forecast.ConsumedFood)]);
    }
}
