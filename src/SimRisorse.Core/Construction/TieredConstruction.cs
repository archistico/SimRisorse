using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Construction;

public enum TieredPurchaseFailure { None, MissingResearch, MissingSourceBuilding, InsufficientResources }

/// <summary>Atomic *proposed* replacements. No change to an active GameSession or SQLite save.</summary>
public sealed record TieredPurchaseResult(bool Succeeded, TieredPurchaseFailure Failure,
    ResourceBundle Cost, ResourceBundle Resources, ConstructionAcquisitionHistory History,
    TieredBuildingPortfolio Portfolio);

public static class TieredConstruction
{
    public static TieredPurchaseResult TryBuild(TieredProductionFamily family, TieredBuildingPortfolio portfolio,
        ProductionTier tier, int quantity, decimal growthFactor, IReadOnlySet<string> unlockedTechnologies,
        ResourceBundle resources, ConstructionAcquisitionHistory history)
    {
        Validate(family, portfolio, resources, history);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        // Even a locked tier must be a known tier: malformed requests are not normal gameplay failures.
        var multiplier = family.Get(tier).CostMultiplier;
        if (!family.IsUnlocked(tier, unlockedTechnologies))
            return Failure(TieredPurchaseFailure.MissingResearch, ResourceBundle.Empty, resources, history, portfolio);
        var proposed = portfolio.Build(tier, quantity); // all quantity/overflow checks before payment
        var payment = ProgressiveConstructionPurchase.TryPurchase(family.FamilyId, family.Source.Cost,
            multiplier, growthFactor, quantity, resources, history);
        if (!payment.Succeeded)
            return Failure(TieredPurchaseFailure.InsufficientResources, payment.Cost, resources, history, portfolio);
        return new(true, TieredPurchaseFailure.None, payment.Cost, payment.RemainingResources, payment.History, proposed);
    }

    public static TieredPurchaseResult TryUpgrade(TieredProductionFamily family, TieredBuildingPortfolio portfolio,
        ProductionTier from, ProductionTier to, decimal growthFactor, IReadOnlySet<string> unlockedTechnologies,
        ResourceBundle resources, ConstructionAcquisitionHistory history)
    {
        Validate(family, portfolio, resources, history);
        var fromDefinition = family.Get(from);
        var toDefinition = family.Get(to);
        if (from >= to) throw new ArgumentOutOfRangeException(nameof(to));
        if (!family.IsUnlocked(to, unlockedTechnologies))
            return Failure(TieredPurchaseFailure.MissingResearch, ResourceBundle.Empty, resources, history, portfolio);
        if (portfolio.At(from).Buildings == 0)
            return Failure(TieredPurchaseFailure.MissingSourceBuilding, ResourceBundle.Empty, resources, history, portfolio);
        var proposed = portfolio.Upgrade(from, to); // no mutation; validate before paying
        var price = ProgressiveConstructionPricing.QuoteUpgrade(family.Source.Cost,
            fromDefinition.CostMultiplier, toDefinition.CostMultiplier, growthFactor, history.Acquired(family.FamilyId));
        if (!resources.Covers(price))
            return Failure(TieredPurchaseFailure.InsufficientResources, price, resources, history, portfolio);
        return new(true, TieredPurchaseFailure.None, price, resources - price, history, proposed);
    }

    private static void Validate(TieredProductionFamily family, TieredBuildingPortfolio portfolio,
        ResourceBundle resources, ConstructionAcquisitionHistory history)
    {
        ArgumentNullException.ThrowIfNull(family);
        ArgumentNullException.ThrowIfNull(portfolio);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(history);
        if (portfolio.FamilyId != family.FamilyId || history.Acquired(family.FamilyId) < portfolio.TotalBuildings)
            throw new ArgumentException("Family ID or acquisition history is inconsistent with the portfolio.");
    }

    private static TieredPurchaseResult Failure(TieredPurchaseFailure reason, ResourceBundle cost,
        ResourceBundle resources, ConstructionAcquisitionHistory history, TieredBuildingPortfolio portfolio) =>
        new(false, reason, cost, resources, history, portfolio);
}
