using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class E4aNaturalBenchmarkTests
{
    [Fact]
    public void FirstFoodProductionUsesOnlyRealInitialResourcesAndCommands()
    {
        var run = E4aNaturalBenchmark.Run("E4A-01", maxSeconds: 120);

        Assert.Equal("natural: no stock/population/research grants", run.Fixture);
        Assert.Equal("natural-reference-v1", run.PolicyId);
        Assert.Equal(8, run.InitialStock.Population);
        Assert.Equal(120m, run.InitialStock.PlanksUnits);
        Assert.Equal(80m, run.InitialStock.StoneUnits);
        Assert.Equal(30m, run.InitialStock.FoodUnits);
        Assert.Equal(0m, run.InitialStock.CoalUnits);
        Assert.Equal(0m, run.InitialStock.PencilsUnits);
        Assert.Equal("pass", run.Outcome);
        Assert.NotNull(run.CompletionSeconds);
        Assert.InRange(run.CompletionSeconds.Value, 1, 120);
        Assert.Contains(run.Events, item => item.Stage == "build fisher Base" && item.Outcome == "executed");
        Assert.Contains(run.Events, item => item.Stage == "build food-store Base" && item.Outcome == "executed");
        Assert.Contains(run.Samples, item => item.ElapsedSeconds > 0 && item.FoodUnits > 30m);
    }

    [Fact]
    public void UnreachedGoalsAreMarkedTimeoutRatherThanInventingCompletionTimes()
    {
        var run = E4aNaturalBenchmark.Run("E4A-05", maxSeconds: 0);

        Assert.Equal("timeout", run.Outcome);
        Assert.Null(run.CompletionSeconds);
        Assert.Equal(0, Assert.Single(run.Samples).ElapsedSeconds);
        Assert.DoesNotContain(run.Events, item => item.Outcome == "blocked");
    }

    [Fact]
    public void ShortNaturalRunIsDeterministicAndCsvHasOneRowPerRealSample()
    {
        var first = E4aNaturalBenchmark.Run("E4A-01", maxSeconds: 120);
        var second = E4aNaturalBenchmark.Run("E4A-01", maxSeconds: 120);
        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        var csv = E4aNaturalBenchmark.ToCsv([first]);
        Assert.Equal(first.Samples.Count + 1, csv.TrimEnd('\r', '\n').Split('\n').Length);
        Assert.Contains("elapsed_seconds,stage,population", csv, StringComparison.Ordinal);
        Assert.Contains("natural-reference-v1", csv, StringComparison.Ordinal);
        Assert.DoesNotContain("controlled: +", csv, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidScenarioAndNegativeHorizonAreRejected()
    {
        Assert.Throws<ArgumentException>(() => E4aNaturalBenchmark.Run("not-a-scenario"));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            E4aNaturalBenchmark.Run("E4A-01", maxSeconds: -1));
    }
}
