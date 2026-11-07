using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class Rc93FundingCheckpointTests
{
    private static readonly ResourceBundle Capacity = ResourceBundle.Create(("planks", 100m), ("stone", 90m));
    private static E4bCurveAFundingSandbox Empty() =>
        E4bCurveAFundingSandbox.Open(ResourceBundle.Empty, Capacity);

    [Fact]
    public void EmptyCheckpointRoundTripsWithoutInventingResources()
    {
        var start = Empty();
        var restored = E4bCurveAFundingSandbox.Restore(start.ExportCheckpoint());
        Assert.True(restored.Reconciles());
        Assert.Equal(start.ExportCheckpoint(), restored.ExportCheckpoint());
        Assert.False(restored.HasPendingPurchase);
    }

    [Fact]
    public void PartialCheckpointPreservesFrozenQuoteAndAllMaterialBalances()
    {
        var initial = Empty();
        Assert.True(initial.TryBeginMint(ProductionTier.Base, out var begun));
        var installment = ProgressiveConstructionEscrow.Installment(begun.Escrow!.Quote,
            begun.RequiredPayments(ProductionTier.Base), 1);
        Assert.True(begun.TryDeliverHypothetical(installment, out var stocked));
        Assert.True(stocked.TryDeposit(installment, out var paid));
        var restored = E4bCurveAFundingSandbox.Restore(paid.ExportCheckpoint());
        Assert.Equal(begun.Escrow.Quote, restored.Escrow!.Quote);
        Assert.Equal(installment, restored.Escrowed);
        Assert.Equal(paid.Outstanding, restored.Outstanding);
        Assert.Equal(0, restored.AcquiredMints);
        Assert.True(restored.Reconciles());
    }

    [Fact]
    public void CorruptLedgerIsNotResumed()
    {
        var snapshot = Empty().ExportCheckpoint() with
        { Stock = ResourceBundle.Create(("planks", 1m)) };
        Assert.Throws<ArgumentException>(() => E4bCurveAFundingSandbox.Restore(snapshot));
    }

    [Fact]
    public void AlteredOrMissingPendingQuotationIsNotResumed()
    {
        Assert.True(Empty().TryBeginMint(ProductionTier.Large, out var opened));
        var valid = opened.ExportCheckpoint();
        Assert.Throws<ArgumentException>(() => E4bCurveAFundingSandbox.Restore(valid with
        { PendingQuote = ResourceBundle.Create(("planks", 1m)) }));
        Assert.Throws<ArgumentException>(() => E4bCurveAFundingSandbox.Restore(valid with
        { PendingPaid = null }));
        Assert.Throws<ArgumentException>(() => E4bCurveAFundingSandbox.Restore(valid with
        { PendingPaid = valid.PendingQuote! + ResourceBundle.Create(("planks", 1m)) }));
    }

    [Fact]
    public void InstructionPayloadIsCanonicalAndArgumentsAreStrict()
    {
        var a = new E4bFundingInstruction(E4bFundingAction.Deposit,
            Materials: ResourceBundle.Create(("stone", 1m), ("planks", 2m)));
        var b = new E4bFundingInstruction(E4bFundingAction.Deposit,
            Materials: ResourceBundle.Create(("planks", 2m), ("stone", 1m)));
        Assert.Equal(a.CanonicalPayload(), b.CanonicalPayload());
        Assert.NotEqual(a.CanonicalPayload(), new E4bFundingInstruction(
            E4bFundingAction.Deposit, Materials: ResourceBundle.Create(("stone", 1m))).CanonicalPayload());
        Assert.Throws<ArgumentException>(() => new E4bFundingInstruction(
            E4bFundingAction.BeginMint, Tier: ProductionTier.Base,
            Materials: ResourceBundle.Empty).CanonicalPayload());
        Assert.Throws<ArgumentException>(() => new E4bFundingInstruction(
            E4bFundingAction.Deposit).CanonicalPayload());
    }
}
