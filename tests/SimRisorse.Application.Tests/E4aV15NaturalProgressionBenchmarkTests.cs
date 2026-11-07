using SimRisorse.Application.Benchmarks;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC73: native v15 unfunded benchmark contract, not a target-time test.</summary>
public sealed class E4aV15NaturalProgressionBenchmarkTests
{
    [Theory]
    [InlineData("V15-N01")]
    [InlineData("V15-N02")]
    [InlineData("V15-N03")]
    public void ZeroSecondRunIsDiagnosticWithNoInventedCompletion(string id)
    {
        var run = V15NaturalProgressionBenchmark.Run(id, maxSeconds: 0);
        Assert.Equal("civil-growth-15", run.RulesVersion);
        Assert.Equal("fresh-game/no-grants", run.Fixture);
        Assert.Equal("timeout", run.Outcome);
        Assert.True(run.Diagnostic);
        Assert.Null(run.CompletionSeconds);
        Assert.Equal(0, run.Samples[^1].ElapsedSeconds);
        Assert.DoesNotContain(run.Events, e => e.Result == "milestone");
    }

    [Fact]
    public void InitialUnfundedPhysicalStockMatchesTheRealFreshV15Game()
    {
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
        var depot = UnifiedFoodInputs.CreatePolicy([
            new("fish", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 0),
            new("meat", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 1),
            new("vegetables", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 2),
            new("fruit", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 3),
            new("bread", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 4)
        ]);
        var fresh = V15FoodCampaignCheckpoints.NewCampaign(
            baseline, school, TechnologyCatalog.ProposedTierTree, depot);
        var original = V15StockAuditor.PhysicalTotals(fresh);
        var run = V15NaturalProgressionBenchmark.Run("V15-N01", maxSeconds: 0);
        Assert.Equal(fresh.CityCampaign.Population, run.InitialPopulation);
        Assert.Equal(original.Amounts.Count, run.InitialPhysicalStockMilliunits.Count);
        foreach (var stock in original.Amounts)
            Assert.Equal(stock.Value.Milliunits, run.InitialPhysicalStockMilliunits[stock.Key]);
    }

    [Fact]
    public void ShortRunUsesTheRealClockAndEmitsOnlyObservedOutcomes()
    {
        var run = V15NaturalProgressionBenchmark.Run("V15-N03", maxSeconds: 2);
        Assert.Equal(2, run.EffectiveHorizonSeconds);
        Assert.Contains(run.Outcome, new[] { "pass", "timeout", "blocked" });
        if (run.Outcome == "pass")
        {
            Assert.InRange(run.CompletionSeconds!.Value, 1, 2);
            Assert.Contains(run.Events, e => e.Result == "milestone");
        }
        else
            Assert.Null(run.CompletionSeconds);
        Assert.Equal(run.Samples[^1].ElapsedSeconds, run.CompletionSeconds ??
            (run.Outcome == "blocked" ? run.Samples[^1].ElapsedSeconds : 2));
    }

    [Fact]
    public void RepeatedDiagnosticRunIsDeterministicAndCsvContainsVersionAndTimes()
    {
        var left = V15NaturalProgressionBenchmark.Run("V15-N01", maxSeconds: 61);
        var right = V15NaturalProgressionBenchmark.Run("V15-N01", maxSeconds: 61);
        Assert.Equal(left.Outcome, right.Outcome);
        Assert.Equal(left.CompletionSeconds, right.CompletionSeconds);
        Assert.Equal(left.Events.ToArray(), right.Events.ToArray());
        Assert.Equal(left.Samples.ToArray(), right.Samples.ToArray());
        var samples = V15NaturalProgressionBenchmark.SamplesCsv([left]);
        var events = V15NaturalProgressionBenchmark.EventsCsv([left]);
        Assert.Contains("effective_horizon_seconds,diagnostic", samples);
        Assert.Contains("V15-N01", samples);
        Assert.Contains("scenario_id,elapsed_seconds,action,result,reason", events);
        Assert.DoesNotContain("civil-growth-14", samples);
    }

    [Fact]
    public void InvalidScenarioAndNegativeDiagnosticHorizonAreRejected()
    {
        Assert.Throws<ArgumentException>(() => V15NaturalProgressionBenchmark.Run("E4A-01"));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            V15NaturalProgressionBenchmark.Run("V15-N01", maxSeconds: -1));
    }

    [Fact]
    public void VeryLargeDiagnosticHorizonNeverOverridesTheScenarioContract()
    {
        // The run must not actually simulate hours: use the published horizon
        // only as a value in a short diagnostic result, not as an assertion of completion.
        var run = V15NaturalProgressionBenchmark.Run("V15-N02", maxSeconds: 1);
        Assert.Equal(28800, run.DeclaredHorizonSeconds);
        Assert.Equal(1, run.EffectiveHorizonSeconds);
        Assert.True(run.Diagnostic);
    }
}
