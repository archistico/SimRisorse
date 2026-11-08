using System.Collections.Immutable;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class Rc98AtomicMintCompletionTests
{
    private static (V15BenchmarkFixture Fixture, E4bV15FundingShadow State) Open()
    {
        var fixture = V15BenchmarkFixture.Create();
        var state = E4bV15FundingShadow.Open(fixture.Baseline, fixture.School,
            fixture.Technologies, fixture.Depot,
            E4bV15MintCompletionFixture.PreProvisioned(fixture));
        return (fixture, state);
    }

    private static E4bV15FundingShadow Paid(E4bV15FundingShadow state,
        ProductionTier tier = ProductionTier.Base)
    {
        Assert.True(state.TryBeginMint(tier, state.Revision, out var begun));
        Assert.True(begun.TryDeposit(begun.Funding.Escrow!.Quote, begun.Revision, out var paid));
        return paid;
    }

    private static E4bV15FundingShadow Restore(V15BenchmarkFixture fixture,
        E4bV15FundingShadow state) => E4bV15FundingShadow.Restore(state.ExportCheckpoint(),
            fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot);

    [Fact]
    public void UnpaidOrRepeatedCompletionCannotCreateAnyMint()
    {
        var (_, initial) = Open();
        Assert.False(initial.TryCompleteMint(0, out var rejected));
        Assert.Same(initial, rejected);
        Assert.True(initial.TryBeginMint(ProductionTier.Base, 0, out var begun));
        Assert.False(begun.TryCompleteMint(1, out rejected));
        Assert.Same(begun, rejected);
        Assert.Equal(0, begun.Campaign.CityCampaign.ProductiveFamilies.Single(f => f.FamilyId == "mint").Base.Buildings);
    }

    [Fact]
    public void PaidMintChangesCityAndHistoryOnceWithoutSecondMaterialDebit()
    {
        var (_, initial) = Open();
        var paid = Paid(initial);
        var remaining = paid.Campaign.CityCampaign.Resources;
        var spent = paid.Funding.Escrowed;
        Assert.Equal(E4bV15FundingReadiness.ReadyForFutureAtomicCommit, paid.Readiness());
        Assert.True(paid.TryCompleteMint(2, out var built));
        Assert.Equal(remaining, built.Campaign.CityCampaign.Resources);
        Assert.Equal(remaining, built.Funding.Stock);
        Assert.Equal(spent, built.Funding.Committed);
        Assert.Equal(ResourceBundle.Empty, built.Funding.Escrowed);
        Assert.Equal(1, built.Funding.AcquiredMints);
        Assert.Equal(1, built.Campaign.CityCampaign.PurchaseCounters.Single(c => c.FamilyId == "mint").Acquired);
        Assert.Equal(1, built.Campaign.CityCampaign.ProductiveFamilies.Single(f => f.FamilyId == "mint").Base.Buildings);
        Assert.Equal(V15CityStorage.Calculate(V15BenchmarkFixture.Create().Baseline, built.Campaign.CityCampaign),
            built.Funding.Capacity);
        Assert.False(built.TryCompleteMint(3, out var again));
        Assert.Same(built, again);
        Assert.True(built.Funding.Reconciles());
    }

    [Fact]
    public void SecondMintUsesNextPriceAndPreservesFirstReceipt()
    {
        var (fixture, initial) = Open();
        var first = Paid(initial);
        Assert.True(first.TryCompleteMint(2, out var built));
        var secondQuote = built.Funding.QuoteNextMint(ProductionTier.Base);
        Assert.NotEqual(first.Funding.Escrow!.Quote, secondQuote);
        var second = Paid(built);
        Assert.True(second.TryCompleteMint(5, out var twice));
        Assert.Equal(2, twice.Campaign.CityCampaign.ProductiveFamilies.Single(f => f.FamilyId == "mint").Base.Buildings);
        Assert.Equal(2, twice.Funding.CompletedMints.Length);
        Assert.Equal(first.Funding.Escrow!.Quote + secondQuote, twice.Funding.Committed);
        Assert.Equal(twice.Campaign, Restore(fixture, twice).Campaign);
    }

    [Fact]
    public void ResearchIsRecheckedAtCompletionForHigherTier()
    {
        var (fixture, initial) = Open();
        var paid = Paid(initial, ProductionTier.Large);
        var state = paid.ExportCheckpoint();
        var withoutResearch = state with
        {
            Campaign = state.Campaign with
            {
                CityCampaign = state.Campaign.CityCampaign with
                { UnlockedTechnologyIds = ImmutableArray<string>.Empty }
            }
        };
        var restored = E4bV15FundingShadow.Restore(withoutResearch,
            fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot);
        Assert.Equal(E4bV15FundingReadiness.MissingResearch, restored.Readiness());
        Assert.False(restored.TryCompleteMint(restored.Revision, out var rejected));
        Assert.Same(restored, rejected);
    }

    [Fact]
    public void RemovedPrerequisitePreventsCompletionWithoutRefundingOrCrediting()
    {
        var (fixture, initial) = Open();
        var paid = Paid(initial);
        var checkpoint = paid.ExportCheckpoint();
        // Removing a producer also removes its local storage. Both halves of
        // the composite checkpoint must agree on the recalculated capacity;
        // otherwise Restore correctly rejects the fixture before Readiness runs.
        var cityWithoutSmelter = checkpoint.Campaign.CityCampaign with
        {
            ProductiveFamilies = checkpoint.Campaign.CityCampaign.ProductiveFamilies
                .Select(f => f.FamilyId == "gold-smelter" ? f with
                { Base = f.Base with { Buildings = 0 } } : f).ToImmutableArray()
        };
        var campaignWithoutSmelter = checkpoint.Campaign with
        {
            CityCampaign = cityWithoutSmelter
        };
        // A forged city-only change must still be rejected by the atomic
        // city/escrow consistency check, not silently normalized on restore.
        Assert.Throws<ArgumentException>(() => E4bV15FundingShadow.Restore(checkpoint with
        {
            Campaign = campaignWithoutSmelter
        }, fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot));
        var withoutSmelter = checkpoint with
        {
            Campaign = campaignWithoutSmelter,
            Funding = checkpoint.Funding with
            {
                Capacity = V15CityStorage.Calculate(fixture.Baseline, cityWithoutSmelter)
            }
        };
        Assert.NotEqual(checkpoint.Funding.Capacity, withoutSmelter.Funding.Capacity);
        var restored = E4bV15FundingShadow.Restore(withoutSmelter,
            fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot);
        Assert.Equal(E4bV15FundingReadiness.MissingPrerequisite, restored.Readiness());
        Assert.Equal(paid.Funding.Stock, restored.Funding.Stock);
        Assert.Equal(paid.Funding.Escrowed, restored.Funding.Escrowed);
        Assert.Equal(paid.Funding.Committed, restored.Funding.Committed);
        Assert.False(restored.TryCompleteMint(restored.Revision, out var rejected));
        Assert.Same(restored, rejected);
    }

    [Fact]
    public void ForgedCommittedQuoteAndCityCounterAreBothRejectedOnRestore()
    {
        var (fixture, initial) = Open();
        var paid = Paid(initial);
        Assert.True(paid.TryCompleteMint(2, out var built));
        var checkpoint = built.ExportCheckpoint();
        Assert.Throws<ArgumentException>(() => E4bV15FundingShadow.Restore(checkpoint with
        {
            Funding = checkpoint.Funding with
            { Committed = checkpoint.Funding.Committed + ResourceBundle.Create(("planks", 1m)) }
        }, fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot));
        Assert.Throws<ArgumentException>(() => E4bV15FundingShadow.Restore(checkpoint with
        {
            Campaign = checkpoint.Campaign with
            {
                CityCampaign = checkpoint.Campaign.CityCampaign with
                {
                    PurchaseCounters = checkpoint.Campaign.CityCampaign.PurchaseCounters
                        .Select(c => c.FamilyId == "mint" ? c with { Acquired = 2 } : c).ToImmutableArray()
                }
            }
        }, fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot));
    }

    [Fact]
    public void ClockAfterCompletionKeepsCommittedMaterialAndRecordedMint()
    {
        var (fixture, initial) = Open();
        var paid = Paid(initial);
        Assert.True(paid.TryCompleteMint(2, out var built));
        Assert.True(built.TryAdvanceClock(fixture.Meat, fixture.Crops, 3, out var ticked));
        Assert.Equal(built.Funding.Committed, ticked.Funding.Committed);
        Assert.Equal(built.Funding.CompletedMints[0], ticked.Funding.CompletedMints[0]);
        Assert.Equal(ticked.Campaign, Restore(fixture, ticked).Campaign);
    }
}
