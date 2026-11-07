using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC88: distinguish real industrial receipts from merely scripted
/// actions and ensure zero-second probes never assert completion.</summary>
public sealed class Rc88IndustrialChainAuditTests
{
    private static V15IndustrialProgressionRun Diagnostic(string scenario = "V15-I03",
        string policy = "research-first", bool mintTen = false) => mintTen
        ? V15IndustrialProgressionBenchmark.RunMintTen(policy, 0)
        : V15IndustrialProgressionBenchmark.Run(scenario, policy, 0);

    [Theory]
    [InlineData("V15-I02", "research-first", false)]
    [InlineData("V15-I02", "bread-first", false)]
    [InlineData("V15-I03", "research-first", false)]
    [InlineData("V15-I03", "bread-first", false)]
    [InlineData("V15-I03", "research-first", true)]
    [InlineData("V15-I03", "bread-first", true)]
    [InlineData("V15-I03", "housing-buffer", true)]
    public void SevenRealZeroSecondProbesAreDiagnosticAndNeverComplete(
        string scenario, string policy, bool mintTen)
    {
        var run = Diagnostic(scenario, policy, mintTen);
        var row = Assert.Single(V15IndustrialChainAudit.Analyze([run]).Observations);
        Assert.Equal("timeout", row.Outcome);
        Assert.True(row.Diagnostic);
        Assert.False(row.Verified);
        Assert.Null(row.CompletionSeconds);
        Assert.Equal(0, row.ClockOutputMilliunits);
        Assert.Equal(0, run.EffectiveHorizonSeconds);
        Assert.True(row.ExecutedSteps < row.TotalSteps);
        Assert.DoesNotContain(run.Events, e => e.Result == "milestone");
    }

    [Theory]
    [InlineData("V15-I02", "swords")]
    [InlineData("V15-I03", "coins")]
    public void MissingOutputResourceInSparseFreshInventoryMeansZero(
        string scenario, string outputResource)
    {
        var physical = V15StockAuditor.PhysicalTotals(V15BenchmarkFixture.Create().Start);
        Assert.False(physical.Amounts.ContainsKey(outputResource));
        Assert.Equal(0L, physical[outputResource].Milliunits);

        // The real benchmark returns a sparse initial stock: the audit must
        // interpret the omitted zero, not throw KeyNotFoundException.
        var run = Diagnostic(scenario);
        Assert.False(run.InitialPhysicalStockMilliunits.ContainsKey(outputResource));
        var observation = Assert.Single(V15IndustrialChainAudit.Analyze([run]).Observations);
        Assert.False(observation.Verified);
        Assert.Equal(0L, observation.FinalStockMilliunits);
    }

    [Fact]
    public void CombinedAuditCountsSevenIndependentWorkforceAndPolicyPaths()
    {
        var report = V15IndustrialChainAudit.Analyze([
            Diagnostic("V15-I02", "research-first"), Diagnostic("V15-I02", "bread-first"),
            Diagnostic("V15-I03", "research-first"), Diagnostic("V15-I03", "bread-first"),
            Diagnostic(policy: "research-first", mintTen: true),
            Diagnostic(policy: "bread-first", mintTen: true),
            Diagnostic(policy: "housing-buffer", mintTen: true)
        ]);
        Assert.Equal(V15IndustrialChainAudit.Version, report.Version);
        Assert.Equal(7, report.RunCount);
        Assert.Equal(7, report.DiagnosticRuns);
        Assert.Equal(0, report.VerifiedRuns);
        Assert.Equal(3, report.Comparisons.Count);
        Assert.All(report.Comparisons, c => Assert.Null(c.BreadMinusResearchSeconds));
        Assert.Contains(report.Observations, r => r.ScenarioId == "V15-I03" &&
            r.PolicyId == "housing-buffer" && r.RulesVersion == "civil-growth-15-mint-crew10");
        // RC85 also permits paid housing-buffer in Legacy. The default RC88
        // seven-run selection is only a subset, not a new limitation on input.
        var legacyHousing = Assert.Single(V15IndustrialChainAudit.Analyze([
            Diagnostic(policy: "housing-buffer")]).Observations);
        Assert.Equal("civil-growth-15", legacyHousing.RulesVersion);
        Assert.False(legacyHousing.Verified);
    }

    [Fact]
    public void DuplicateCampaignRegimeAndPolicyAreRejected()
    {
        var run = Diagnostic();
        Assert.Throws<ArgumentException>(() => V15IndustrialChainAudit.Analyze([run, run]));
    }

    [Fact]
    public void FirstResearchOnlyAndNullInputsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => V15IndustrialChainAudit.Analyze([]));
        Assert.Throws<ArgumentNullException>(() => V15IndustrialChainAudit.Analyze(null!));
        Assert.Throws<ArgumentException>(() => V15IndustrialChainAudit.Analyze([null!]));
        Assert.Throws<ArgumentException>(() => V15IndustrialChainAudit.Analyze([
            V15IndustrialProgressionBenchmark.Run("V15-I01", "research-first", 0)]));
    }

    [Fact]
    public void ForgedOutputMilestoneWithoutFullChainIsRejected()
    {
        var original = Diagnostic();
        var milestone = new V15IndustrialProgressionEvent(0, original.Goal, "milestone", "fake",
            new Dictionary<string, long>(), 0, ProducedMilliunits: 3000);
        Assert.Throws<ArgumentException>(() => V15IndustrialChainAudit.Analyze([
            original with
            {
                Outcome = "pass", CompletionSeconds = 0,
                Events = [.. original.Events, milestone],
                Samples = [original.Samples[0] with
                { NextAction = "completed", CoinsMilliunits = 3000, ExtractionOneUnlocked = true }]
            }]));
    }

    [Fact]
    public void WrongInitialInventoryCannotAppearAsAnUnfundedCampaign()
    {
        var original = Diagnostic();
        var inventory = original.InitialPhysicalStockMilliunits.ToDictionary(p => p.Key, p => p.Value);
        inventory["pencils"] = 0;
        Assert.Throws<ArgumentException>(() => V15IndustrialChainAudit.Analyze([
            original with { InitialPhysicalStockMilliunits = inventory }]));
    }

    [Fact]
    public void SkippedOrReorderedConstructionCommandsAreRejected()
    {
        var original = Diagnostic();
        var altered = original.Events.ToArray();
        Assert.NotEmpty(altered);
        altered[0] = altered[0] with { Action = "build-mint" };
        Assert.Throws<ArgumentException>(() => V15IndustrialChainAudit.Analyze([
            original with { Events = altered }]));
    }

    [Fact]
    public void ZeroPriceOnPaidBuildingCannotCountAsReceipt()
    {
        var original = Diagnostic();
        var altered = original.Events.Select(e => e.Action == "build-fisher" && e.Result == "executed"
            ? e with { ResourceCostsMilliunits = new Dictionary<string, long>() } : e).ToArray();
        Assert.Contains(original.Events, e => e.Action == "build-fisher" && e.Result == "executed");
        Assert.Throws<ArgumentException>(() => V15IndustrialChainAudit.Analyze([
            original with { Events = altered }]));
    }

    [Fact]
    public void ResearchPointsCannotBeAwardedWhileBuilding()
    {
        var original = Diagnostic();
        var altered = original.Events.Select(e => e.Action == "build-fisher" && e.Result == "executed"
            ? e with { ResearchSpentMilliPoints = 200000 } : e).ToArray();
        Assert.Throws<ArgumentException>(() => V15IndustrialChainAudit.Analyze([
            original with { Events = altered }]));
    }

    [Fact]
    public void NegativeOrUnpaidClockProductionIsRejected()
    {
        var original = Diagnostic();
        var altered = original.Events.Select((e, i) => i == 0
            ? e with { ProducedMilliunits = -1 } : e).ToArray();
        Assert.NotEmpty(altered);
        Assert.Throws<ArgumentException>(() => V15IndustrialChainAudit.Analyze([
            original with { Events = altered }]));
    }

    [Fact]
    public void CsvAndMarkdownLabelUnknownCompletionTimes()
    {
        var report = V15IndustrialChainAudit.Analyze([Diagnostic()]);
        var csv = V15IndustrialChainAudit.ToCsv(report);
        var markdown = V15IndustrialChainAudit.ToMarkdown(report);
        Assert.Contains("clock_output_milli", csv);
        Assert.Contains("produced_milliunits", V15IndustrialProgressionBenchmark.EventsCsv([Diagnostic()]));
        Assert.Contains("research-first", csv);
        Assert.Contains("diagnostico", markdown);
        Assert.Contains("non prova un softlock", markdown);
        Assert.DoesNotContain("verificato |", markdown);
    }

    [Fact]
    public void PartialRealClockRunDoesNotImplyRealMintOutput()
    {
        var run = V15IndustrialProgressionBenchmark.RunMintTen("research-first", 61);
        var report = V15IndustrialChainAudit.Analyze([run]);
        var row = Assert.Single(report.Observations);
        Assert.True(row.Diagnostic);
        if (run.Outcome != "pass")
        {
            Assert.Null(row.CompletionSeconds);
            Assert.False(row.Verified);
        }
        Assert.All(run.Events, e => Assert.True(e.ResourceCostsMilliunits.Values.All(v => v >= 0)));
    }
}
