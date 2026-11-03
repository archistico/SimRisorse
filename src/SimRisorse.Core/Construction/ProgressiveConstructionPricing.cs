using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Construction;

/// <summary>
/// Pure, deterministic construction quotations. The current v13 campaign still uses
/// fixed costs: this engine is a foundation for a future versioned ruleset.
/// All prices are rounded up once per resource and per purchased building.
/// </summary>
public static class ProgressiveConstructionPricing
{
    public static ResourceBundle QuoteNewBuildings(ResourceBundle baseCost, decimal tierMultiplier,
        decimal growthFactor, int previouslyAcquired, int quantity)
    {
        Validate(baseCost, tierMultiplier, growthFactor, previouslyAcquired);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        if (baseCost.Amounts.IsEmpty) return ResourceBundle.Empty;
        var factor = Power(growthFactor, previouslyAcquired);
        if (growthFactor == 1m)
            return QuoteAtFactor(baseCost, tierMultiplier, factor).Scale(quantity);

        var total = ResourceBundle.Empty;
        for (var index = 0; index < quantity; index++)
        {
            total += QuoteAtFactor(baseCost, tierMultiplier, factor);
            if (index + 1 < quantity) factor = checked(factor * growthFactor);
        }
        return total;
    }

    /// <summary>
    /// Price difference at the SAME current family counter; does not increment it.
    /// Returns a cost only, never a refund of stored resources.
    /// </summary>
    public static ResourceBundle QuoteUpgrade(ResourceBundle baseCost, decimal fromTierMultiplier,
        decimal toTierMultiplier, decimal growthFactor, int previouslyAcquired)
    {
        Validate(baseCost, fromTierMultiplier, growthFactor, previouslyAcquired);
        if (toTierMultiplier <= fromTierMultiplier)
            throw new ArgumentOutOfRangeException(nameof(toTierMultiplier), "Only upgrades to a more expensive tier are supported.");
        var factor = Power(growthFactor, previouslyAcquired);
        return QuoteAtFactor(baseCost, toTierMultiplier, factor) -
            QuoteAtFactor(baseCost, fromTierMultiplier, factor);
    }

    private static void Validate(ResourceBundle baseCost, decimal tierMultiplier, decimal growthFactor, int previouslyAcquired)
    {
        ArgumentNullException.ThrowIfNull(baseCost);
        if (tierMultiplier <= 0m) throw new ArgumentOutOfRangeException(nameof(tierMultiplier));
        if (growthFactor < 1m) throw new ArgumentOutOfRangeException(nameof(growthFactor));
        ArgumentOutOfRangeException.ThrowIfNegative(previouslyAcquired);
    }

    private static ResourceBundle QuoteAtFactor(ResourceBundle baseCost, decimal tierMultiplier, decimal factor) =>
        ResourceBundle.FromAmounts(baseCost.Amounts.Select(item =>
            KeyValuePair.Create(item.Key, ResourceAmount.FromUnits(
                decimal.Ceiling(checked(item.Value.Units * tierMultiplier * factor))))));

    private static decimal Power(decimal basis, int exponent)
    {
        var result = 1m;
        while (exponent > 0)
        {
            if ((exponent & 1) != 0) result = checked(result * basis);
            exponent >>= 1;
            if (exponent != 0) basis = checked(basis * basis);
        }
        return result;
    }
}
