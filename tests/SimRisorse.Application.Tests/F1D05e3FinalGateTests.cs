using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>
/// RC72: regression gate spanning the real mixed-chain v15 clock. These tests
/// use no player saves and do not assume any future economic balance values.
/// </summary>
public sealed class F1D05e3FinalGateTests
{
    [Fact]
    public void MixedChainReconcilesEverySecondOfTwoMinutesWithoutRestoringTheRetiredProducer()
    {
        var scenario = V15CombinedFoodStressScenario.Create();
        var current = scenario.Initial;
        long served = 0;
        long missed = 0;
        for (var second = 0; second < 120; second++)
        {
            var step = scenario.Plan(current, TimeSpan.FromSeconds(1));
            var audit = V15StockAuditor.ReconcileSecond(current, step);
            Assert.True(audit.Balanced, $"Stock imbalance at second {second + 1}: " +
                string.Join(", ", audit.Differences.Select(d => d.ResourceId)));
            Assert.Equal(1L, step.SecondsAdvanced);
            served += step.MealsServed;
            missed += step.MealsMissed;
            current = step.Next;
            Assert.Equal(ResourceAmount.Zero, current.CityCampaign.Resources["food"]);
            Assert.DoesNotContain(current.CityCampaign.ProductiveFamilies,
                family => family.FamilyId == V15LegacyProducerAdapter.RetiredFamilyId);
        }
        Assert.Equal(120L, served + missed);
        Assert.Equal(scenario.Initial.CityCampaign.ElapsedTicks + TimeSpan.FromSeconds(120).Ticks,
            current.CityCampaign.ElapsedTicks);
    }

    [Fact]
    public void MixedChainContinuousAndFractionalSplitAdvancesPreserveTheSamePhysicalState()
    {
        var scenario = V15CombinedFoodStressScenario.Create();
        var start = scenario.Initial;
        var continuous = scenario.Plan(start, TimeSpan.FromMilliseconds(120125)).Next;
        var split = scenario.Plan(start, TimeSpan.FromMilliseconds(125)).Next;
        for (var second = 0; second < 120; second++)
        {
            var step = scenario.Plan(split, TimeSpan.FromSeconds(1));
            Assert.True(V15StockAuditor.ReconcileSecond(split, step).Balanced);
            split = step.Next;
        }
        Assert.Equal(continuous.CityCampaign.ElapsedTicks, split.CityCampaign.ElapsedTicks);
        Assert.Equal(continuous.CityCampaign.Resources, split.CityCampaign.Resources);
        Assert.Equal(continuous.CityCampaign.Population, split.CityCampaign.Population);
        Assert.Equal(continuous.CityCampaign.GrowthProgressSeconds,
            split.CityCampaign.GrowthProgressSeconds);
        Assert.Equal(continuous.CityCampaign.GrowthRemainderTwentieths,
            split.CityCampaign.GrowthRemainderTwentieths);
        Assert.Equal(continuous.CityCampaign.ResearchBalanceMilliPoints,
            split.CityCampaign.ResearchBalanceMilliPoints);
        Assert.Equal(continuous.CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray(),
            split.CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray());
        Assert.Equal(continuous.CityCampaign.Military, split.CityCampaign.Military);
        Assert.Equal(continuous.Industry, split.Industry);
        Assert.Equal(continuous.Depot, split.Depot);
        Assert.Equal(V15StockAuditor.PhysicalTotals(continuous), V15StockAuditor.PhysicalTotals(split));
    }

    [Fact]
    public void ZeroDurationTickLeavesEveryPhysicalOwnerAndResearchStateUntouched()
    {
        var scenario = V15CombinedFoodStressScenario.Create();
        var before = scenario.Initial;
        var step = scenario.Plan(before, TimeSpan.Zero);
        Assert.Equal(0L, step.SecondsAdvanced);
        Assert.Equal(0L, step.MealsServed);
        Assert.Equal(0L, step.MealsMissed);
        Assert.Equal(ResourceBundle.Empty, step.FirstSecondProduction);
        Assert.Equal(ResourceBundle.Empty, step.FirstSecondConsumption);
        Assert.Equal(before.CityCampaign, step.Next.CityCampaign);
        Assert.Equal(before.Industry, step.Next.Industry);
        Assert.Equal(before.Depot, step.Next.Depot);
    }
}
