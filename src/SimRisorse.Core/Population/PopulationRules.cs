using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Population;

public sealed record PopulationRules
{
    public PopulationRules(string foodResourceId, ResourceAmount foodPerPersonPerSecond, int growthIntervalSeconds)
    {
        ContentIdentifier.Validate(foodResourceId, nameof(foodResourceId));
        if (foodPerPersonPerSecond == ResourceAmount.Zero || foodPerPersonPerSecond.Milliunits > long.MaxValue / int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(foodPerPersonPerSecond));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(growthIntervalSeconds);
        FoodResourceId = foodResourceId;
        FoodPerPersonPerSecond = foodPerPersonPerSecond;
        GrowthIntervalSeconds = growthIntervalSeconds;
    }

    public string FoodResourceId { get; }
    public ResourceAmount FoodPerPersonPerSecond { get; }
    public int GrowthIntervalSeconds { get; }
}
