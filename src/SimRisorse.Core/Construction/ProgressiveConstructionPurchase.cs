using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Construction;

/// <summary>Immutable quotation and payment outcome; no session mutation or persistence.</summary>
public sealed record ProgressiveConstructionPurchaseResult(bool Succeeded, ResourceBundle Cost,
    ResourceBundle RemainingResources, ConstructionAcquisitionHistory History);

public static class ProgressiveConstructionPurchase
{
    /// <summary>
    /// Checks funds and creates replacement resource/history values together. Preflight for
    /// prerequisites, housing, storage and tier research remains the session's responsibility.
    /// </summary>
    public static ProgressiveConstructionPurchaseResult TryPurchase(string familyId,
        ResourceBundle baseCost, decimal tierMultiplier, decimal growthFactor, int quantity,
        ResourceBundle available, ConstructionAcquisitionHistory history)
    {
        ArgumentNullException.ThrowIfNull(available);
        ArgumentNullException.ThrowIfNull(history);
        var acquired = history.Acquired(familyId);
        var cost = ProgressiveConstructionPricing.QuoteNewBuildings(
            baseCost, tierMultiplier, growthFactor, acquired, quantity);
        if (!available.Covers(cost))
            return new(false, cost, available, history);

        // Calculate everything before returning the replacement state. The caller can
        // commit both values atomically in its versioned GameSession.
        var nextHistory = history.RegisterPurchase(familyId, quantity);
        var remainder = available - cost;
        return new(true, cost, remainder, nextHistory);
    }
}
