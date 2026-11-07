using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class Rc92CurveAFundingSandboxTests
{
    private static readonly ResourceBundle Capacity = ResourceBundle.Create(("planks", 100m), ("stone", 90m));

    private static E4bCurveAFundingSandbox Empty() =>
        E4bCurveAFundingSandbox.Open(ResourceBundle.Empty, Capacity);

    [Theory]
    [InlineData(ProductionTier.Base)]
    [InlineData(ProductionTier.Large)]
    [InlineData(ProductionTier.Advanced)]
    public void FirstAndSecondMintFundExactlyWithRealStorageConstraints(ProductionTier tier)
    {
        var state = Empty();
        var study = new E4bEconomicCurveStudy();
        var curve = Assert.Single(E4bEconomicCurveStudy.Curves, c => c.Id == "A");
        for (var copy = 0; copy < 2; copy++)
        {
            var quote = study.Quote(curve, "mint", tier, copy);
            Assert.Equal(quote, state.QuoteNextMint(tier));
            var count = state.RequiredPayments(tier);
            Assert.True(count > 1);
            Assert.True(state.TryBeginMint(tier, out var started));
            state = started;
            Assert.Equal(quote, state.Escrow!.Quote);
            for (long i = 1; i <= count; i++)
            {
                var installment = ProgressiveConstructionEscrow.Installment(quote, count, i);
                Assert.True(Capacity.Covers(installment));
                Assert.True(state.TryDeliverHypothetical(installment, out var supplied));
                Assert.True(supplied.TryDeposit(installment, out var paid));
                state = paid;
                Assert.True(state.Reconciles());
                Assert.True(Capacity.Covers(state.Stock));
                Assert.Equal(copy, state.AcquiredMints);
            }
            Assert.True(state.TryComplete(out var completed));
            state = completed;
            Assert.Equal(copy + 1, state.AcquiredMints);
            Assert.False(state.HasPendingPurchase);
        }
        var expected = study.Quote(curve, "mint", tier, 0, 2);
        Assert.Equal(expected, state.Committed);
        Assert.Equal(expected, state.HypotheticalDelivered);
        Assert.Equal(ResourceBundle.Empty, state.Stock);
    }

    [Fact]
    public void ModeIsExplicitlyNotPlayableAndCannotModifyTheCampaign()
    {
        Assert.Contains("not-playable", E4bCurveAFundingSandbox.Mode);
        var fixture = V15BenchmarkFixture.Create();
        var before = System.Text.Json.JsonSerializer.Serialize(fixture.Start);
        var state = Empty();
        Assert.True(state.TryBeginMint(ProductionTier.Base, out _));
        Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(fixture.Start));
    }

    [Fact]
    public void CannotBeginTwiceOrDepositBeforeAnOpenPurchase()
    {
        var initial = Empty();
        Assert.False(initial.TryDeposit(ResourceBundle.Create(("planks", 1m)), out var rejected));
        Assert.Same(initial, rejected);
        Assert.True(initial.TryBeginMint(ProductionTier.Base, out var opened));
        Assert.False(opened.TryBeginMint(ProductionTier.Large, out rejected));
        Assert.Same(opened, rejected);
        Assert.Equal(initial.AcquiredMints, opened.AcquiredMints);
        Assert.True(opened.Reconciles());
    }

    [Fact]
    public void QuoteIsFrozenWhilePayingAndProgressionIsNotIncrementedEarly()
    {
        var initial = Empty();
        Assert.True(initial.TryBeginMint(ProductionTier.Base, out var opened));
        var quote = opened.Escrow!.Quote;
        var payment = ProgressiveConstructionEscrow.Installment(quote, opened.RequiredPayments(ProductionTier.Base), 1);
        Assert.True(opened.TryDeliverHypothetical(payment, out var supplied));
        Assert.True(supplied.TryDeposit(payment, out var partial));
        Assert.Equal(quote, partial.Escrow!.Quote);
        Assert.Equal(quote - payment, partial.Outstanding);
        Assert.Equal(0, partial.AcquiredMints);
        Assert.False(partial.TryComplete(out var rejected));
        Assert.Same(partial, rejected);
    }

    [Fact]
    public void ZeroAndOversizedAndWrongResourceDepositsAreRejectedWithoutSideEffects()
    {
        var state = E4bCurveAFundingSandbox.Open(ResourceBundle.Create(("planks", 100m)), Capacity);
        Assert.True(state.TryBeginMint(ProductionTier.Base, out var started));
        foreach (var invalid in new[]
        {
            ResourceBundle.Empty, ResourceBundle.Create(("planks", 101m)),
            ResourceBundle.Create(("stone", 91m)), ResourceBundle.Create(("gold", 1m))
        })
        {
            Assert.False(started.TryDeposit(invalid, out var rejected));
            Assert.Same(started, rejected);
        }
    }

    [Fact]
    public void PhysicalCapacityRejectsHypotheticalOverdeliveryWithoutMutation()
    {
        var state = E4bCurveAFundingSandbox.Open(ResourceBundle.Create(("planks", 99m)), Capacity);
        Assert.False(state.TryDeliverHypothetical(ResourceBundle.Create(("planks", 2m)), out var rejected));
        Assert.Same(state, rejected);
        Assert.False(state.TryDeliverHypothetical(ResourceBundle.Empty, out rejected));
        Assert.Same(state, rejected);
        Assert.True(state.TryDeliverHypothetical(ResourceBundle.Create(("planks", 1m)), out var accepted));
        Assert.Equal(ResourceBundle.Create(("planks", 100m)), accepted.Stock);
        Assert.True(accepted.Reconciles());
    }

    [Fact]
    public void CancellationRefundsOnlyIfStorageAllowsAndNeverIncrementsCounter()
    {
        var state = E4bCurveAFundingSandbox.Open(ResourceBundle.Create(("planks", 80m)), Capacity);
        Assert.True(state.TryBeginMint(ProductionTier.Base, out var opened));
        Assert.True(opened.TryDeposit(ResourceBundle.Create(("planks", 20m)), out var partial));
        Assert.True(partial.TryCancel(out var cancelled));
        Assert.Equal(ResourceBundle.Create(("planks", 80m)), cancelled.Stock);
        Assert.False(cancelled.HasPendingPurchase);
        Assert.Equal(0, cancelled.AcquiredMints);
        Assert.True(cancelled.Reconciles());
        Assert.False(cancelled.TryCancel(out var rejected));
        Assert.Same(cancelled, rejected);
    }

    [Fact]
    public void CancellationCannotLoseEscrowWhenRefundWouldExceedCapacity()
    {
        var state = E4bCurveAFundingSandbox.Open(ResourceBundle.Create(("planks", 60m)), Capacity);
        Assert.True(state.TryBeginMint(ProductionTier.Base, out var opened));
        Assert.True(opened.TryDeposit(ResourceBundle.Create(("planks", 20m)), out var partial));
        Assert.True(partial.TryDeliverHypothetical(ResourceBundle.Create(("planks", 55m)), out var filled));
        Assert.False(filled.TryCancel(out var rejected));
        Assert.Same(filled, rejected);
        Assert.Equal(ResourceBundle.Create(("planks", 20m)), rejected.Escrowed);
        Assert.Equal(ResourceBundle.Create(("planks", 95m)), rejected.Stock);
        Assert.True(rejected.Reconciles());
    }

    [Fact]
    public void FamilyCounterIsSharedAcrossTiersAndCannotBeAdvancedTwice()
    {
        var initial = E4bCurveAFundingSandbox.Open(ResourceBundle.Empty, Capacity, 1);
        var study = new E4bEconomicCurveStudy();
        var curve = Assert.Single(E4bEconomicCurveStudy.Curves, c => c.Id == "A");
        Assert.Equal(study.Quote(curve, "mint", ProductionTier.Base, 1),
            initial.QuoteNextMint(ProductionTier.Base));
        Assert.Equal(study.Quote(curve, "mint", ProductionTier.Advanced, 1),
            initial.QuoteNextMint(ProductionTier.Advanced));
        Assert.True(initial.TryBeginMint(ProductionTier.Large, out var pending));
        Assert.False(pending.TryComplete(out var rejected));
        Assert.Same(pending, rejected);
        Assert.Equal(1, rejected.AcquiredMints);
    }

    [Fact]
    public void InvalidInputsAndUnknownTierAreRejected()
    {
        Assert.Throws<ArgumentException>(() => E4bCurveAFundingSandbox.Open(
            ResourceBundle.Create(("planks", 101m)), Capacity));
        Assert.Throws<ArgumentOutOfRangeException>(() => E4bCurveAFundingSandbox.Open(
            ResourceBundle.Empty, Capacity, -1));
        Assert.Throws<ArgumentNullException>(() => E4bCurveAFundingSandbox.Open(null!, Capacity));
        Assert.Throws<ArgumentNullException>(() => E4bCurveAFundingSandbox.Open(ResourceBundle.Empty, null!));
        var state = Empty();
        Assert.Throws<ArgumentOutOfRangeException>(() => state.QuoteNextMint((ProductionTier)99));
        Assert.False(state.TryBeginMint((ProductionTier)99, out var rejected));
        Assert.Same(state, rejected);
        Assert.Throws<ArgumentNullException>(() => state.TryDeposit(null!, out _));
        Assert.Throws<ArgumentNullException>(() => state.TryDeliverHypothetical(null!, out _));
        var noCapacity = E4bCurveAFundingSandbox.Open(ResourceBundle.Empty, ResourceBundle.Empty);
        Assert.Throws<InvalidOperationException>(() => noCapacity.RequiredPayments(ProductionTier.Base));
        var extremelyHighCounter = E4bCurveAFundingSandbox.Open(ResourceBundle.Empty, Capacity, int.MaxValue);
        Assert.False(extremelyHighCounter.TryBeginMint(ProductionTier.Base, out var unmodified));
        Assert.Same(extremelyHighCounter, unmodified);
    }

    [Fact]
    public void StartingWithExistingAcquisitionsNeverGivesFreeExtraMint()
    {
        var state = E4bCurveAFundingSandbox.Open(ResourceBundle.Empty, Capacity, 4);
        Assert.Equal(4, state.AcquiredMints);
        Assert.True(state.TryBeginMint(ProductionTier.Base, out var opened));
        Assert.Equal(4, opened.AcquiredMints);
        Assert.Equal(state.QuoteNextMint(ProductionTier.Base), opened.Escrow!.Quote);
        Assert.True(opened.TryCancel(out var cancelled));
        Assert.Equal(4, cancelled.AcquiredMints);
    }

    [Fact]
    public void DemonstrationHasSixReconciledPurchasesButNoNaturalPlaytest()
    {
        var report = E4bCurveAFundingDemonstration.Run();
        Assert.Equal(6, report.Rows.Count);
        Assert.Equal(3, report.Rows.Select(row => row.Tier).Distinct().Count());
        Assert.All(report.Rows, row =>
        {
            Assert.True(row.Reconciles);
            Assert.True(row.RequiredPayments > 0);
            Assert.Equal(row.FrozenQuote, row.Paid);
            Assert.Equal(row.PurchaseIndex, row.AcquiredMintsAfter);
        });
        Assert.Contains("hypothetical", report.InputProvenance);
        Assert.Contains("not-approved", report.ApprovalStatus);
        Assert.Equal(E4bCurveAFundingSandbox.Mode, report.Mode);
    }

    [Fact]
    public void DemonstrationExportsAreDeterministicAndLabelTheLimitations()
    {
        var report = E4bCurveAFundingDemonstration.Run();
        var csv = E4bCurveAFundingDemonstration.ToCsv(report);
        var md = E4bCurveAFundingDemonstration.ToMarkdown(report);
        Assert.Equal(7, csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.Contains("required_payments", csv);
        Assert.Contains("Non attivo in partita", md);
        Assert.Contains("ipotetici", md);
        Assert.Equal(csv, E4bCurveAFundingDemonstration.ToCsv(E4bCurveAFundingDemonstration.Run()));
        Assert.Throws<ArgumentNullException>(() => E4bCurveAFundingDemonstration.ToCsv(null!));
        Assert.Throws<ArgumentNullException>(() => E4bCurveAFundingDemonstration.ToMarkdown(null!));
    }
}
