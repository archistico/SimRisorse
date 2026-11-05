using SimRisorse.Application.Benchmarks;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC75: industrial/technology progress without grants or fictional results.</summary>
public sealed class E4aV15IndustrialProgressionTests
{
    [Theory]
    [InlineData("V15-I01", V15IndustrialProgressionBenchmark.PolicyResearchFirst)]
    [InlineData("V15-I01", V15IndustrialProgressionBenchmark.PolicyBreadFirst)]
    [InlineData("V15-I02", V15IndustrialProgressionBenchmark.PolicyResearchFirst)]
    [InlineData("V15-I02", V15IndustrialProgressionBenchmark.PolicyBreadFirst)]
    [InlineData("V15-I03", V15IndustrialProgressionBenchmark.PolicyResearchFirst)]
    [InlineData("V15-I03", V15IndustrialProgressionBenchmark.PolicyBreadFirst)]
    public void EveryAdvancedStrategyHasHonestZeroSecondDiagnostic(string scenario, string policy)
    {
        var run = V15IndustrialProgressionBenchmark.Run(scenario, policy, maxSeconds: 0);
        Assert.Equal("civil-growth-15", run.RulesVersion);
        Assert.Equal("fresh-game/no-grants", run.Fixture);
        Assert.True(run.Diagnostic);
        Assert.Equal("timeout", run.Outcome);
        Assert.Null(run.CompletionSeconds);
        Assert.Equal(0, run.Samples[^1].ElapsedSeconds);
        Assert.DoesNotContain(run.Events, e => e.Result == "milestone");
        Assert.All(run.Events, e => Assert.True(e.ResourceCostsMilliunits.Values.All(v => v >= 0)));
    }

    [Fact]
    public void NewRunnerStartsWithExactlyTheSamePhysicalInventoryAsRc73Runner()
    {
        var old = V15NaturalProgressionBenchmark.Run("V15-N01", maxSeconds: 0);
        var advanced = V15IndustrialProgressionBenchmark.Run("V15-I01",
            V15IndustrialProgressionBenchmark.PolicyResearchFirst, maxSeconds: 0);
        Assert.Equal(old.BaselineRulesVersion, advanced.BaselineRulesVersion);
        Assert.Equal(2, V15BenchmarkFixture.Create().School.PencilsPerStudent);
        Assert.Equal(old.InitialPhysicalStockMilliunits.OrderBy(p => p.Key),
            advanced.InitialPhysicalStockMilliunits.OrderBy(p => p.Key));
    }

    [Fact]
    public void PoliciesRetainTheSameNecessaryActionsButChangeTheirOrder()
    {
        var research = V15IndustrialProgressionBenchmark.Steps("V15-I03", "research-first");
        var bread = V15IndustrialProgressionBenchmark.Steps("V15-I03", "bread-first");
        Assert.Equal(research.OrderBy(x => x), bread.OrderBy(x => x));
        Assert.True(research.ToList().IndexOf("build-school") < research.ToList().IndexOf("build-bakery"));
        Assert.True(bread.ToList().IndexOf("build-bakery") < bread.ToList().IndexOf("build-school"));
        Assert.Contains("build-mint", research);
        Assert.Contains("buy-tech-estrazione-1", research);
        Assert.Contains("buy-tech-industria-1", research);
        Assert.Contains("craft-pencils", research);
        Assert.True(research.ToList().IndexOf("staff-coal-mine") < research.ToList().IndexOf("craft-pencils"));
        Assert.Contains("build-gold-smelter", research);
        Assert.Contains("build-iron-smelter",
            V15IndustrialProgressionBenchmark.Steps("V15-I02", "bread-first"));
    }

    [Fact]
    public void RepeatedShortAdvancedRunIsDeterministicAndReportsRealActionsOnly()
    {
        var left = V15IndustrialProgressionBenchmark.Run("V15-I01", "research-first", 61);
        var right = V15IndustrialProgressionBenchmark.Run("V15-I01", "research-first", 61);
        Assert.Equal(left.Outcome, right.Outcome);
        Assert.Equal(left.Events.Count, right.Events.Count);
        for (var i = 0; i < left.Events.Count; i++)
        {
            Assert.Equal(left.Events[i].ElapsedSeconds, right.Events[i].ElapsedSeconds);
            Assert.Equal(left.Events[i].Action, right.Events[i].Action);
            Assert.Equal(left.Events[i].Result, right.Events[i].Result);
            Assert.Equal(left.Events[i].Reason, right.Events[i].Reason);
            Assert.Equal(left.Events[i].ResearchSpentMilliPoints, right.Events[i].ResearchSpentMilliPoints);
            Assert.Equal(left.Events[i].ResourceCostsMilliunits.OrderBy(p => p.Key),
                right.Events[i].ResourceCostsMilliunits.OrderBy(p => p.Key));
        }
        Assert.Equal(left.Samples.ToArray(), right.Samples.ToArray());
        Assert.True(left.Diagnostic);
        var eventsCsv = V15IndustrialProgressionBenchmark.EventsCsv([left]);
        Assert.Contains("resource_id,spent_milli,research_spent_millipoints", eventsCsv);
        Assert.Contains("scenario_id,policy_id,elapsed_seconds", eventsCsv);
        Assert.Contains("estrazione1_unlocked", V15IndustrialProgressionBenchmark.SamplesCsv([left]));
    }

    [Fact]
    public void MissingScenarioPolicyAndNegativeDurationAreRejected()
    {
        Assert.Throws<ArgumentException>(() => V15IndustrialProgressionBenchmark.Run("V15-I99", "research-first", 0));
        Assert.Throws<ArgumentException>(() => V15IndustrialProgressionBenchmark.Run("V15-I01", "unknown", 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => V15IndustrialProgressionBenchmark.Run("V15-I01", "bread-first", -1));
    }

    private static V15IndustrialProgressionRun Synthetic(string policy, int completion = 60)
    {
        var original = V15IndustrialProgressionBenchmark.Run("V15-I01", policy, 0);
        return original with
        {
            EffectiveHorizonSeconds = original.DeclaredHorizonSeconds,
            Diagnostic = false,
            Outcome = "pass",
            CompletionSeconds = completion,
            Events = [
                new(0, "build-fisher", "executed", "",
                    new Dictionary<string, long> { ["planks"] = 40000, ["stone"] = 20000 }, 0),
                new(0, "craft-pencils", "executed", "",
                    new Dictionary<string, long> { ["planks"] = 1000, ["coal"] = 1000 }, 0),
                new(0, "enroll-student", "executed", "",
                    new Dictionary<string, long> { ["food"] = 10000, ["pencils"] = 2000 }, 0),
                new(0, "buy-tech-estrazione-1", "executed", "",
                    new Dictionary<string, long>(), 150000),
                new(completion, "unlock-estrazione-1", "milestone", "Tech observed",
                    new Dictionary<string, long>(), 0)
            ],
            Samples = [original.Samples[0], original.Samples[0] with
            {
                ElapsedSeconds = completion, NextAction = "completed", ExtractionOneUnlocked = true
            }]
        };
    }

    [Fact]
    public void AnalyzerCountsEveryPaidResourceAndResearchPointsWithoutDuplication()
    {
        var report = V15IndustrialProgressionAnalysis.Analyze([
            Synthetic("research-first"), Synthetic("bread-first", 90)]);
        Assert.Equal(2, report.Passed);
        var row = report.Scenarios.Single(r => r.PolicyId == "research-first");
        Assert.Equal(41000L, row.ResourceCostsMilliunits["planks"]);
        Assert.Equal(20000L, row.ResourceCostsMilliunits["stone"]);
        Assert.Equal(10000L, row.ResourceCostsMilliunits["food"]);
        Assert.Equal(1000L, row.ResourceCostsMilliunits["coal"]);
        Assert.Equal(2000L, row.ResourceCostsMilliunits["pencils"]);
        Assert.Equal(150000L, row.ResearchSpentMilliPoints);
        Assert.Equal(4, row.ExecutedActions);
        Assert.Equal(30L, Assert.Single(report.Comparisons).BreadFirstMinusResearchFirstSeconds);
        Assert.Contains("food", V15IndustrialProgressionAnalysis.ToMarkdown(report));
        Assert.Contains("resource_id,spent_milli", V15IndustrialProgressionAnalysis.ToCsv(report));
    }

    [Fact]
    public void PartialAndDiagnosticRunsCannotProduceAnInventedPolicyRanking()
    {
        var diagnostic = V15IndustrialProgressionBenchmark.Run("V15-I01", "research-first", 0);
        var report = V15IndustrialProgressionAnalysis.Analyze([diagnostic, Synthetic("bread-first")]);
        Assert.Null(Assert.Single(report.Comparisons).BreadFirstMinusResearchFirstSeconds);
        Assert.Equal("diagnostic-only", report.Scenarios[0].Classification);
        Assert.Null(report.Scenarios[0].CompletionSeconds);
    }

    [Fact]
    public void DuplicateInputNegativeCostsAndFakeSuccessAreRejected()
    {
        var a = Synthetic("research-first");
        Assert.Throws<ArgumentException>(() => V15IndustrialProgressionAnalysis.Analyze([a, a]));
        Assert.Throws<ArgumentException>(() => V15IndustrialProgressionAnalysis.Analyze([]));
        var negative = a with { Events = [new(0, "build-x", "executed", "",
            new Dictionary<string, long> { ["planks"] = -1 }, 0)] };
        Assert.Throws<ArgumentException>(() => V15IndustrialProgressionAnalysis.Analyze([negative]));
        var falsePass = a with { Events = [new(0, "build-x", "executed", "",
            new Dictionary<string, long>(), 0)] };
        Assert.Throws<ArgumentException>(() => V15IndustrialProgressionAnalysis.Analyze([falsePass]));
    }

    [Fact]
    public void ShortRunNeverClaimsAnUnobservedForgeOrMintProduction()
    {
        foreach (var scenario in new[] { "V15-I02", "V15-I03" })
        {
            var run = V15IndustrialProgressionBenchmark.Run(scenario, "research-first", 1);
            if (run.Outcome != "pass")
            {
                Assert.Null(run.CompletionSeconds);
                Assert.DoesNotContain(run.Events, e => e.Result == "milestone");
            }
        }
    }
}
