using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC85: additional houses are real purchases, and a short
/// natural run cannot be misreported as proof of ten-worker feasibility.</summary>
public sealed class Rc85MintHousingBufferTests
{
    private static V15IndustrialProgressionRun Mint(string policy, int seconds = 0) =>
        V15IndustrialProgressionBenchmark.RunMintTen(policy, seconds);
    private static V15IndustrialProgressionRun Legacy(string policy) =>
        V15IndustrialProgressionBenchmark.Run("V15-I03", policy, 0);
    private static V15IndustrialProgressionRun[] MintPair() =>
        [Mint(V15IndustrialProgressionBenchmark.PolicyBreadFirst),
            Mint(V15IndustrialProgressionBenchmark.PolicyHousingBuffer)];
    private static E4bMintHousingBufferReport Report() =>
        E4bMintHousingBufferStudy.Analyze(MintPair());

    [Theory]
    [InlineData(V15CrewMode.MintTen)]
    [InlineData(V15CrewMode.Legacy)]
    public void HousingBufferIsTwoActualExtraConstructionActionsBeforeGold(V15CrewMode crew)
    {
        var baseline = V15IndustrialProgressionBenchmark.Steps("V15-I03", "bread-first", crew).ToArray();
        var buffered = V15IndustrialProgressionBenchmark.Steps("V15-I03", "housing-buffer", crew).ToArray();
        Assert.Equal(baseline.Length + 2, buffered.Length);
        var firstGold = Array.IndexOf(baseline, "build-gold-mine");
        var bufferedGold = Array.IndexOf(buffered, "build-gold-mine");
        Assert.True(firstGold > 0);
        Assert.Equal(firstGold + 2, bufferedGold);
        Assert.Equal("build-small-house", buffered[bufferedGold - 2]);
        Assert.Equal("build-small-house", buffered[bufferedGold - 1]);
        Assert.Equal(baseline, buffered.Where((_, i) => i != bufferedGold - 2 && i != bufferedGold - 1));
    }

    [Fact]
    public void MintCrewStillRequiresOneAtomicTenWorkerAssignment()
    {
        var steps = V15IndustrialProgressionBenchmark.Steps("V15-I03", "housing-buffer", V15CrewMode.MintTen);
        Assert.Equal("staff-mint-team", steps[^1]);
        Assert.Single(steps, action => action == "staff-mint-team");
        Assert.DoesNotContain("staff-mint", steps);
    }

    [Fact]
    public void HousingPolicyDoesNotApplyToResearchOrForgeBenchmarks()
    {
        Assert.Throws<ArgumentException>(() => V15IndustrialProgressionBenchmark.Steps("V15-I01", "housing-buffer"));
        Assert.Throws<ArgumentException>(() => V15IndustrialProgressionBenchmark.Run("V15-I02", "housing-buffer", 0));
    }

    [Fact]
    public void HousingPolicyNeverChangesOriginalBreadOrResearchSteps()
    {
        foreach (var policy in new[] { "research-first", "bread-first" })
        {
            var expected = V15IndustrialProgressionBenchmark.Steps("V15-I03", policy, V15CrewMode.Legacy);
            var actual = V15IndustrialProgressionBenchmark.Steps("V15-I03", policy, V15CrewMode.MintTen);
            Assert.Equal(expected.SkipLast(1), actual.SkipLast(1));
        }
    }

    [Fact]
    public void DiagnosticHasNoInventedHousingCompletion()
    {
        var buffered = Mint("housing-buffer");
        Assert.Equal("E4B-M01", buffered.ScenarioId);
        Assert.Equal(V15CrewRules.MintTenRulesVersion, buffered.RulesVersion);
        Assert.Equal("fresh-game/no-grants", buffered.Fixture);
        Assert.True(buffered.Diagnostic);
        Assert.Equal("timeout", buffered.Outcome);
        Assert.Null(buffered.CompletionSeconds);
        Assert.DoesNotContain(buffered.Events, e => e.Result == "milestone");
    }

    [Fact]
    public void ComparisonReportsOnlyObservedHousesAndNoTimesInDiagnostics()
    {
        var result = Report();
        var row = Assert.Single(result.Comparisons);
        Assert.Equal("MintTen", row.CrewContract);
        Assert.True(row.Diagnostic);
        Assert.Null(row.HousingMinusBaselineSeconds);
        Assert.Null(row.HousingCompletionSeconds);
        Assert.Equal("diagnostic-only/no-time-comparison", row.ComparisonStatus);
        Assert.Equal(row.HousingSmallHousesPurchased - row.BaselineSmallHousesPurchased,
            row.AdditionalSmallHousesPurchased);
        Assert.Equal(0, result.ComparableCompletedContracts);
    }

    [Fact]
    public void BothWorkforceContractsCanBeComparedSeparately()
    {
        var runs = new[] { Mint("bread-first"), Mint("housing-buffer"),
            Legacy("bread-first"), Legacy("housing-buffer") };
        var result = E4bMintHousingBufferStudy.Analyze(runs);
        Assert.Equal(4, result.NaturalRunCount);
        Assert.Equal(2, result.PairedContracts);
        Assert.Equal(new[] { "MintTen", "Legacy" }, result.Comparisons.Select(c => c.CrewContract));
        Assert.All(result.Comparisons, row => Assert.Null(row.HousingMinusBaselineSeconds));
    }

    [Fact]
    public void ExistingResearchStrategyMayBePresentWithoutBeingTheControl()
    {
        var runs = new[] { Mint("research-first"), Mint("bread-first"), Mint("housing-buffer") };
        var result = E4bMintHousingBufferStudy.Analyze(runs);
        Assert.Equal(3, result.NaturalRunCount);
        Assert.Single(result.Comparisons);
        Assert.Equal("bread-first", result.Comparisons[0].BaselinePolicyId);
    }

    [Fact]
    public void NewPolicyIsAcceptedByPriorIndustrialAndNaturalAuditors()
    {
        var runs = MintPair();
        Assert.Equal(2, V15IndustrialProgressionAnalysis.Analyze(runs).RunCount);
        Assert.Equal(2, V15IndustrialConstraintAnalysis.Analyze(runs).RunCount);
        Assert.Equal(2, E4bMintNaturalFeasibilityAudit.Analyze(runs).ObservedRuns);
    }

    [Fact]
    public void ComparisonRejectsMissingBreadFirstControl()
    {
        Assert.Throws<ArgumentException>(() => E4bMintHousingBufferStudy.Analyze([Mint("housing-buffer")]));
    }

    [Fact]
    public void ComparisonRequiresMintTenEvenIfLegacyHasBothPolicies()
    {
        Assert.Throws<ArgumentException>(() => E4bMintHousingBufferStudy.Analyze([
            Legacy("bread-first"), Legacy("housing-buffer")]));
    }

    [Fact]
    public void ComparisonRejectsUnknownSavedStaffingContract()
    {
        var runs = MintPair();
        runs[1] = runs[1] with { RulesVersion = "unknown-crew-rule" };
        Assert.Throws<ArgumentException>(() => E4bMintHousingBufferStudy.Analyze(runs));
    }

    [Fact]
    public void ComparisonRejectsInconsistentStartingInventory()
    {
        var runs = MintPair();
        var stock = runs[1].InitialPhysicalStockMilliunits.ToDictionary(k => k.Key, v => v.Value);
        var resource = stock.Keys.First();
        stock[resource]++;
        runs[1] = runs[1] with { InitialPhysicalStockMilliunits = stock };
        Assert.Throws<ArgumentException>(() => E4bMintHousingBufferStudy.Analyze(runs));
    }

    [Fact]
    public void ComparisonRejectsFalseMintMilestone()
    {
        var runs = MintPair();
        runs[1] = runs[1] with { Outcome = "pass", CompletionSeconds = 0 };
        Assert.Throws<ArgumentException>(() => E4bMintHousingBufferStudy.Analyze(runs));
    }

    [Fact]
    public void ExportsNeverInventANumericImprovement()
    {
        var report = Report();
        var csv = E4bMintHousingBufferStudy.ToCsv(report);
        var md = E4bMintHousingBufferStudy.ToMarkdown(report);
        Assert.StartsWith("crew_contract,baseline_policy,housing_policy,", csv);
        Assert.Equal(2, csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.Contains("diagnostic-only/no-time-comparison", csv);
        Assert.Contains("NON dimostra un softlock globale", md);
        Assert.Contains("prezzi legacy", md);
    }

    [Fact]
    public void ReportIsRepeatableWithoutMutatingTheFreshStart()
    {
        var before = JsonSerializer.Serialize(V15BenchmarkFixture.Create().Start);
        Assert.Equal(JsonSerializer.Serialize(Report()), JsonSerializer.Serialize(Report()));
        Assert.Equal(before, JsonSerializer.Serialize(V15BenchmarkFixture.Create().Start));
    }

    [Fact]
    public void JsonRoundtripPreservesOriginalRunLedger()
    {
        var runs = MintPair();
        var recovered = JsonSerializer.Deserialize<V15IndustrialProgressionRun[]>(JsonSerializer.Serialize(runs));
        Assert.NotNull(recovered);
        Assert.Equal(JsonSerializer.Serialize(E4bMintHousingBufferStudy.Analyze(runs)),
            JsonSerializer.Serialize(E4bMintHousingBufferStudy.Analyze(recovered!)));
    }

    [Fact]
    public void NullOrEmptyEvidenceIsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => E4bMintHousingBufferStudy.Analyze(null!));
        Assert.Throws<ArgumentException>(() => E4bMintHousingBufferStudy.Analyze([]));
        Assert.Throws<ArgumentNullException>(() => E4bMintHousingBufferStudy.ToCsv(null!));
        Assert.Throws<ArgumentNullException>(() => E4bMintHousingBufferStudy.ToMarkdown(null!));
    }

    [Fact]
    public void BudgetReceiptsAreNeverNegativeAndDoNotGrantResources()
    {
        var buffered = Mint("housing-buffer", 61);
        Assert.Equal("fresh-game/no-grants", buffered.Fixture);
        Assert.All(buffered.Events, e => Assert.All(e.ResourceCostsMilliunits, entry =>
            Assert.True(entry.Value >= 0)));
        Assert.All(buffered.Events, e => Assert.True(e.ResearchSpentMilliPoints >= 0));
        Assert.True(buffered.Samples[^1].ElapsedSeconds <= 61);
    }
}
