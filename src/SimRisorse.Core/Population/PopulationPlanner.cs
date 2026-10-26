using System.Collections.Immutable;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Population;

public enum GrowthBlockReason { None, FixedPopulation, HousingFull, MissingFood }

public sealed record PopulationForecast(int HousingCapacity, int ProgressSeconds, int IntervalSeconds,
    string? FoodResourceId, ResourceAmount RequiredFood, ResourceAmount ConsumedFood, GrowthBlockReason BlockReason);

public sealed record EconomyForecast(ImmutableArray<BuildingProduction> Buildings, PopulationForecast Population);

internal static class PopulationPlanner
{
    public static PopulationForecast Plan(PopulationRules? rules, int population, int housing, int progress, ResourceBundle stocks)
    {
        if (rules is null) return new(housing, 0, 0, null, default, default, GrowthBlockReason.FixedPopulation);
        var required = rules.FoodPerPersonPerSecond * population;
        var fed = stocks[rules.FoodResourceId].Milliunits >= required.Milliunits;
        return new(housing, progress, rules.GrowthIntervalSeconds, rules.FoodResourceId, required, fed ? required : default,
            !fed ? GrowthBlockReason.MissingFood : population >= housing ? GrowthBlockReason.HousingFull : GrowthBlockReason.None);
    }

    public static ResourceBundle Consumption(PopulationForecast forecast) => forecast.FoodResourceId is { } id
        ? ResourceBundle.FromAmounts([KeyValuePair.Create(id, forecast.ConsumedFood)]) : ResourceBundle.Empty;
}
