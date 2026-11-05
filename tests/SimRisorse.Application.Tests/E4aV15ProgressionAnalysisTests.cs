using SimRisorse.Application.Benchmarks;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC74: analysis of evidence, NOT an assertion of economic reachability.</summary>
public sealed class E4aV15ProgressionAnalysisTests
{
    private static V15NaturalProgressionSample Sample(int second, string next = "build-depot",
        long workers = 2) => new(second, next, 8, workers,
        30000, 9000, 0, 0, 0, 0, 0, 0, 0);

    private static V15NaturalProgressionRun Run(string id = "V15-N01", string outcome = "timeout",
        bool diagnostic = false, IReadOnlyList<V15NaturalProgressionEvent>? events = null,
        IReadOnlyList<V15NaturalProgressionSample>? samples = null)
    {
        var full = 120;
        var horizon = diagnostic ? 60 : full;
        var finish = outcome == "blocked" ? 0 : horizon;
        return new(id, "civil-growth-15", "baseline", "scripted-1", "fresh-game/no-grants",
            "first-food", full, horizon, diagnostic, outcome,
            outcome == "pass" ? horizon : null,
            outcome == "pass" ? "food observed" : "No goal", 8,
            new Dictionary<string, long> { ["planks"] = 30000 },
            events ?? (outcome switch
            {
                "pass" => new V15NaturalProgressionEvent[] { new(0, "build-depot", "waiting", "InsufficientResources", 0, 0),
                           new(60, "build-depot", "executed", "", 2500, 1000),
                           new(horizon, "first-food", "milestone", "Food produced", 0, 0) },
                "blocked" => new V15NaturalProgressionEvent[] { new(0, "build-depot", "blocked", "PrerequisiteMissing", 0, 0) },
                _ => new V15NaturalProgressionEvent[] { new(0, "build-depot", "waiting", "InsufficientResources", 0, 0) }
            }),
            samples ?? new V15NaturalProgressionSample[] { Sample(0), Sample(finish, outcome == "pass" ? "completed" : "build-depot") });
    }

    [Fact]
    public void CompletedMilestoneHasObservedDurationAndOnlyRecordedMaterialCosts()
    {
        var analyzed = V15NaturalProgressionAnalysis.Analyze([Run(outcome: "pass")]);
        var row = Assert.Single(analyzed.Scenarios);
        Assert.Equal(1, analyzed.Completed);
        Assert.Equal("milestone-observed", row.Classification);
        Assert.Equal(120, row.CompletionSeconds);
        Assert.Equal(60, row.WaitingSeconds);
        Assert.Equal(1, row.ExecutedActions);
        Assert.Equal(2500m, row.PlanksSpentMilliunits);
        Assert.Equal(1000m, row.StoneSpentMilliunits);
        Assert.Equal(60L, row.WaitingSecondsByConstraint["resources"]);
    }

    [Fact]
    public void TimeoutIsNotAssertedToBeAStructuralEconomicDeadlock()
    {
        var row = Assert.Single(V15NaturalProgressionAnalysis.Analyze([Run()]).Scenarios);
        Assert.Equal("script-policy-timeout", row.Classification);
        Assert.Equal("resources", row.PrimaryConstraint);
        Assert.Equal(120, row.WaitingSeconds);
        Assert.Null(row.CompletionSeconds);
        Assert.Contains("NOT demonstrated", row.EvidenceNote);
    }

    [Fact]
    public void ShortDiagnosticIsNeverLabeledAsFullHorizonResult()
    {
        var result = V15NaturalProgressionAnalysis.Analyze([Run(diagnostic: true)]);
        Assert.Equal(1, result.DiagnosticCount);
        Assert.Equal("diagnostic-only", result.Scenarios[0].Classification);
        Assert.Equal(60, result.Scenarios[0].ObservedSeconds);
    }

    [Fact]
    public void BlockedActionIsAttributedToScriptWithoutFictitiousTime()
    {
        var result = V15NaturalProgressionAnalysis.Analyze([Run(outcome: "blocked")]);
        var row = Assert.Single(result.Scenarios);
        Assert.Equal("script-policy-blocked", row.Classification);
        Assert.Equal("prerequisite-or-technology", row.PrimaryConstraint);
        Assert.Equal(0, row.ObservedSeconds);
        Assert.Equal(0, row.WaitingSeconds);
    }

    [Fact]
    public void ChangedWaitingReasonsAreMeasuredInSeparateNonOverlappingIntervals()
    {
        V15NaturalProgressionEvent[] events =
        [
            new(0, "build-depot", "waiting", "InsufficientResources", 0, 0),
            new(60, "build-depot", "waiting", "InsufficientPopulation", 0, 0),
            new(90, "build-depot", "executed", "", 1200, 200)
        ];
        var row = Assert.Single(V15NaturalProgressionAnalysis.Analyze([
            Run(events: events, samples: [Sample(0), Sample(60), Sample(120, "await-production")])]).Scenarios);
        Assert.Equal(90, row.WaitingSeconds);
        Assert.Equal(60L, row.WaitingSecondsByConstraint["resources"]);
        Assert.Equal(30L, row.WaitingSecondsByConstraint["population"]);
        Assert.Equal("production-not-observed-within-horizon", row.Classification);
    }

    [Fact]
    public void DuplicateScenarioIdsAreNotSilentlyAggregated()
    {
        Assert.Throws<ArgumentException>(() => V15NaturalProgressionAnalysis.Analyze([Run(), Run()]));
        Assert.Throws<ArgumentException>(() => V15NaturalProgressionAnalysis.Analyze([]));
        Assert.Throws<ArgumentException>(() => V15NaturalProgressionAnalysis.Analyze([
            Run() with { Fixture = "controlled/granted-materials" }]));
    }

    [Fact]
    public void InconsistentCompletionAndUnsortedEventsAreRejected()
    {
        var falsePass = Run(outcome: "pass", events: [new(120, "x", "executed", "", 0, 0)]);
        Assert.Throws<ArgumentException>(() => V15NaturalProgressionAnalysis.Analyze([falsePass]));
        var unordered = Run(events: [new(60, "x", "waiting", "x", 0, 0),
            new(0, "x", "waiting", "x", 0, 0)]);
        Assert.Throws<ArgumentException>(() => V15NaturalProgressionAnalysis.Analyze([unordered]));
    }

    [Fact]
    public void CsvAndMarkdownDistinguishIncompleteMeasurementsAndTrackedCosts()
    {
        var report = V15NaturalProgressionAnalysis.Analyze([Run()]);
        var csv = V15NaturalProgressionAnalysis.ToCsv(report);
        var markdown = V15NaturalProgressionAnalysis.ToMarkdown(report);
        Assert.Contains("planks_spent_milli,stone_spent_milli", csv);
        Assert.Contains("script-policy-timeout", csv);
        Assert.Contains("soltanto Legname lavorato", markdown);
        Assert.Contains("non prova", markdown);
    }

    [Fact]
    public void RealZeroSecondBenchmarkCanBeAnalyzedWithoutSimulatingHours()
    {
        var actual = V15NaturalProgressionBenchmark.Run("V15-N01", maxSeconds: 0);
        var row = Assert.Single(V15NaturalProgressionAnalysis.Analyze([actual]).Scenarios);
        Assert.Equal("diagnostic-only", row.Classification);
        Assert.Equal(0, row.ObservedSeconds);
        Assert.Null(row.CompletionSeconds);
    }
}
