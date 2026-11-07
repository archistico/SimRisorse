using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class Rc94V15FundingShadowTests
{
    private static readonly ResourceBundle OnePlank = ResourceBundle.Create(("planks", 1m));

    private static (V15BenchmarkFixture Fixture, E4bV15FundingShadow Shadow) Setup()
    {
        var fixture = V15BenchmarkFixture.Create();
        var shadow = E4bV15FundingShadow.Open(fixture.Baseline, fixture.School,
            fixture.Technologies, fixture.Depot, fixture.Start);
        return (fixture, shadow);
    }

    private static E4bV15FundingShadow Restore(V15BenchmarkFixture fixture,
        E4bV15FundingShadowCheckpoint checkpoint) =>
        E4bV15FundingShadow.Restore(checkpoint, fixture.Baseline,
            fixture.School, fixture.Technologies, fixture.Depot);

    private static E4bV15FundingShadow Started() =>
        Begin(Setup().Shadow);

    private static E4bV15FundingShadow Begin(E4bV15FundingShadow shadow)
    {
        Assert.True(shadow.TryBeginMint(ProductionTier.Base, shadow.Revision, out var begun));
        return begun;
    }

    [Fact]
    public void OpenUsesActualCityStockStorageAndHistoricalMintCounter()
    {
        var (fixture, state) = Setup();
        Assert.Equal(fixture.Start.CityCampaign.Resources, state.Funding.Stock);
        Assert.Equal(0, state.Revision);
        Assert.Equal(fixture.Start.CityCampaign.PurchaseCounters.Single(c => c.FamilyId == "mint").Acquired,
            state.Funding.AcquiredMints);
        Assert.True(state.Funding.Reconciles());
        Assert.Contains("not-playable", E4bV15FundingShadow.Mode);
    }

    [Fact]
    public void BeginDoesNotDeductCityMaterialsOrCreateARealMint()
    {
        var before = Setup().Shadow;
        var next = Begin(before);
        Assert.Equal(before.Campaign, next.Campaign);
        Assert.Equal(before.Funding.Stock, next.Funding.Stock);
        Assert.Equal(before.Funding.AcquiredMints, next.Funding.AcquiredMints);
        Assert.Equal(1, next.Revision);
        Assert.True(next.Funding.HasPendingPurchase);
    }

    [Fact]
    public void DepositsDebitExactlyTheSameStockInCityAndEscrow()
    {
        var state = Started();
        var originalStock = state.Campaign.CityCampaign.Resources;
        Assert.True(state.TryDeposit(OnePlank, 1, out var paid));
        Assert.Equal(originalStock - OnePlank, paid.Campaign.CityCampaign.Resources);
        Assert.Equal(paid.Campaign.CityCampaign.Resources, paid.Funding.Stock);
        Assert.Equal(OnePlank, paid.Funding.Escrowed);
        Assert.Equal(2, paid.Revision);
        Assert.True(paid.Funding.Reconciles());
    }

    [Fact]
    public void PartialDepositSurvivesCombinedCheckpointRoundTrip()
    {
        var (fixture, initial) = Setup();
        Assert.True(Begin(initial).TryDeposit(OnePlank, 1, out var partial));
        var restored = Restore(fixture, partial.ExportCheckpoint());
        Assert.Equal(partial.Funding.Escrowed, restored.Funding.Escrowed);
        Assert.Equal(partial.Campaign, restored.Campaign);
        Assert.Equal(partial.Revision, restored.Revision);
        Assert.True(restored.Funding.Reconciles());
    }

    [Fact]
    public void CancelRestoresExactlyTheDebitedMaterials()
    {
        var (fixture, initial) = Setup();
        Assert.True(Begin(initial).TryDeposit(OnePlank, 1, out var paid));
        Assert.True(paid.TryCancel(2, out var canceled));
        Assert.Equal(fixture.Start.CityCampaign.Resources, canceled.Campaign.CityCampaign.Resources);
        Assert.False(canceled.IsFunding);
        Assert.Equal(3, canceled.Revision);
        Assert.True(canceled.Funding.Reconciles());
    }

    [Fact]
    public void WrongRevisionDoesNotStartDepositOrCancel()
    {
        var state = Started();
        Assert.False(state.TryBeginMint(ProductionTier.Large, 0, out var begun));
        Assert.Same(state, begun);
        Assert.False(state.TryDeposit(OnePlank, 0, out var paid));
        Assert.Same(state, paid);
        Assert.False(state.TryCancel(0, out var canceled));
        Assert.Same(state, canceled);
    }

    [Fact]
    public void OverpaymentAndAbsentStockCannotBeDeducted()
    {
        var state = Started();
        Assert.False(state.TryDeposit(ResourceBundle.Empty, 1, out var empty));
        Assert.Same(state, empty);
        Assert.False(state.TryDeposit(ResourceBundle.Create(("gold", 999999m)), 1, out var excess));
        Assert.Same(state, excess);
        Assert.Equal(state.Campaign.CityCampaign.Resources, state.Funding.Stock);
    }

    [Fact]
    public void CorruptOrForeignCityInventoryIsRejectedOnRestore()
    {
        var (fixture, state) = Setup();
        var snapshot = Begin(state).ExportCheckpoint();
        var tampered = snapshot with
        {
            Campaign = snapshot.Campaign with
            {
                CityCampaign = snapshot.Campaign.CityCampaign with
                { Resources = snapshot.Campaign.CityCampaign.Resources - OnePlank }
            }
        };
        Assert.Throws<ArgumentException>(() => Restore(fixture, tampered));
    }

    [Fact]
    public void SandboxOnlyHypotheticalDeliveriesAreNotAcceptedAsRealV15Resources()
    {
        var (fixture, shadow) = Setup();
        var snapshot = shadow.ExportCheckpoint();
        var tampered = snapshot with
        {
            Funding = snapshot.Funding with
            { HypotheticalDelivered = OnePlank, InitialStock = snapshot.Funding.InitialStock - OnePlank }
        };
        Assert.Throws<ArgumentException>(() => Restore(fixture, tampered));
    }

    [Fact]
    public void CounterOrPhysicalCapacityMismatchIsRejected()
    {
        var (fixture, shadow) = Setup();
        var snapshot = shadow.ExportCheckpoint();
        Assert.Throws<ArgumentException>(() => Restore(fixture, snapshot with
            { Funding = snapshot.Funding with { AcquiredMints = 1 } }));
        Assert.Throws<ArgumentException>(() => Restore(fixture, snapshot with
            { Funding = snapshot.Funding with { Capacity = ResourceBundle.Empty } }));
    }

    [Fact]
    public void ReadinessNeverClaimsThatAnUnpaidOrAbsentMintIsBuildable()
    {
        var state = Setup().Shadow;
        Assert.Equal(E4bV15FundingReadiness.NoPendingMint, state.Readiness());
        Assert.Equal(E4bV15FundingReadiness.MissingPayment, Begin(state).Readiness());
    }

    [Fact]
    public void InvalidHeadersAndACompletedSandboxReceiptAreRejected()
    {
        var (fixture, state) = Setup();
        var checkpoint = state.ExportCheckpoint();
        Assert.Throws<ArgumentException>(() => Restore(fixture, checkpoint with { Version = 99 }));
        Assert.Throws<ArgumentException>(() => Restore(fixture, checkpoint with { Revision = -1 }));
        Assert.Throws<ArgumentException>(() => Restore(fixture, checkpoint with
            { Funding = checkpoint.Funding with { Committed = OnePlank } }));
        Assert.Equal(fixture.Start, state.Campaign);
    }
}
