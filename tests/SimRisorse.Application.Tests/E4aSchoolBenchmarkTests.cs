using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class E4aSchoolBenchmarkTests
{
    [Fact]
    public void OneVsTwoSchoolsUsesRealEnrollmentAndMeasuresQualification()
    {
        var runs = E4aSchoolBenchmark.Run();
        Assert.Equal(2, runs.Count);
        var one = Assert.Single(runs, item => item.ScenarioId == "E4A-06");
        var two = Assert.Single(runs, item => item.ScenarioId == "E4A-07");
        Assert.Equal("pass", one.Outcome);
        Assert.Equal("pass", two.Outcome);
        Assert.Equal(240, one.AllQualifiedSeconds);
        Assert.Equal(120, two.AllQualifiedSeconds);
        Assert.Equal(120, one.FirstQualifiedSeconds);
        Assert.Equal(120, two.FirstQualifiedSeconds);
        Assert.Equal(1, one.SchoolCount);
        Assert.Equal(2, two.SchoolCount);
        Assert.Equal(1, one.Samples[0].WaitingStudents);
        Assert.Equal(0, two.Samples[0].WaitingStudents);
        Assert.Equal(2, one.Samples[^1].QualifiedResearchers);
        Assert.Equal(2, two.Samples[^1].QualifiedResearchers);
        Assert.Equal(0, one.Samples[^1].ActiveResearchers);
        Assert.All(runs, run => Assert.Contains(run.SetupActions,
            action => action.StartsWith("craft pencils:", StringComparison.Ordinal)));
    }

    [Fact]
    public void SchoolBenchmarksAreDeterministicAcrossIndependentRuns()
    {
        var first = JsonSerializer.Serialize(E4aSchoolBenchmark.Run());
        var second = JsonSerializer.Serialize(E4aSchoolBenchmark.Run());
        Assert.Equal(first, second);
    }

    [Fact]
    public void CsvHasInvariantDecimalsCorrectSamplesAndNoInventedCheckpoints()
    {
        var runs = E4aSchoolBenchmark.Run();
        var csv = E4aSchoolBenchmark.ToCsv(runs);
        var lines = csv.TrimEnd('\r', '\n').Split('\n');
        Assert.Equal(1 + runs.Sum(run => run.Samples.Count), lines.Length);
        Assert.Contains("school_count,elapsed_seconds", lines[0], StringComparison.Ordinal);
        Assert.Contains("E4A-06", csv, StringComparison.Ordinal);
        Assert.Contains("E4A-07", csv, StringComparison.Ordinal);
        Assert.Contains("controlled: +600 planks", csv, StringComparison.Ordinal);
        Assert.DoesNotContain("build_sha256", lines[0], StringComparison.Ordinal);
        Assert.DoesNotContain("checkpoint_id", lines[0], StringComparison.Ordinal);
    }
}
