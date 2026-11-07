using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;

namespace SimRisorse.Application.Benchmarks;

/// <summary>RC93: a complete, versioned snapshot of the *non-playable* financing sandbox.
/// Stock and escrow are stored together; neither can be committed independently.</summary>
public sealed record E4bFundingCheckpoint(
    int Version, ResourceBundle InitialStock, ResourceBundle Stock, ResourceBundle Capacity,
    ResourceBundle HypotheticalDelivered, ResourceBundle Committed, int AcquiredMints,
    ProductionTier? PendingTier, ResourceBundle? PendingQuote, ResourceBundle? PendingPaid);

public enum E4bFundingAction { BeginMint, DeliverHypothetical, Deposit, Complete, Cancel }

/// <summary>Strict input contract for a durable funding operation. Material delivery
/// remains explicitly hypothetical: this is not an in-game resource earning command.</summary>
public sealed record E4bFundingInstruction(E4bFundingAction Action, ProductionTier? Tier = null,
    ResourceBundle? Materials = null)
{
    public void Validate()
    {
        if (!Enum.IsDefined(Action))
            throw new ArgumentException("Unknown funding action.");
        var invalid = Action switch
        {
            E4bFundingAction.BeginMint => Tier is null || !Enum.IsDefined(Tier.Value) || Materials is not null,
            E4bFundingAction.DeliverHypothetical or E4bFundingAction.Deposit =>
                Tier is not null || Materials is null || Materials.Equals(ResourceBundle.Empty),
            E4bFundingAction.Complete or E4bFundingAction.Cancel => Tier is not null || Materials is not null,
            _ => true
        };
        if (invalid)
            throw new ArgumentException("Invalid funding command arguments.");
    }

    public bool TryApply(E4bCurveAFundingSandbox state, out E4bCurveAFundingSandbox next)
    {
        ArgumentNullException.ThrowIfNull(state);
        Validate();
        return Action switch
        {
            E4bFundingAction.BeginMint => state.TryBeginMint(Tier!.Value, out next),
            E4bFundingAction.DeliverHypothetical => state.TryDeliverHypothetical(Materials!, out next),
            E4bFundingAction.Deposit => state.TryDeposit(Materials!, out next),
            E4bFundingAction.Complete => state.TryComplete(out next),
            E4bFundingAction.Cancel => state.TryCancel(out next),
            _ => throw new ArgumentOutOfRangeException(nameof(Action))
        };
    }

    /// <summary>Canonical, culture-independent payload for exact request-id reuse checks.</summary>
    public string CanonicalPayload()
    {
        Validate();
        var materials = Materials is null ? "-" : string.Join(";", Materials.Amounts.Select(
            entry => entry.Key + ":" + entry.Value.Milliunits.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        return ((int)Action).ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" +
            (Tier is null ? "-" : ((int)Tier.Value).ToString(System.Globalization.CultureInfo.InvariantCulture)) + "|" + materials;
    }
}
