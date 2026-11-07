using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC84: diagnostic fixtures are NEVER evidence of full-horizon
/// completion, and analytical pricing is NEVER attributed to gameplay.</summary>
public sealed class Rc84MintNaturalFeasibilityTests
{
    private static V15IndustrialProgressionRun Mint(string policy) =>
        V15IndustrialProgressionBenchmark.RunMintTen(policy, 0);
    private static V15IndustrialProgressionRun Legacy(string policy) =>
        V15IndustrialProgressionBenchmark.Run("V15-I03", policy, 0);
    private static E4bMintNaturalFeasibilityReport Report() =>
        E4bMintNaturalFeasibilityAudit.Analyze([
            Mint("research-first"), Legacy("research-first"),
            Mint("bread-first"), Legacy("bread-first")]);

    [Theory]
    [InlineData("research-first")]
    [InlineData("bread-first")]
    public void SingleDiagnosticMintTenIsNotDeclaredAFullRun(string policy)
    {
        var report = E4bMintNaturalFeasibilityAudit.Analyze([Mint(policy)]);
        var row = Assert.Single(report.Observations);
        Assert.Equal("MintTen", row.CrewContract);
        Assert.Equal("diagnostic-only", row.Classification);
        Assert.Equal("timeout", row.Outcome);
        Assert.True(row.Diagnostic);
        Assert.Equal(0, row.ObservedSeconds);
        Assert.Null(row.CoinsMilestoneSecond);
        Assert.Null(row.FullCrewAssignedSecond);
        Assert.Equal(0, report.FullHorizonRuns);
        Assert.Equal(0, report.VerifiedMintTenMilestones);
        Assert.False(report.CompleteNaturalPolicyPairObserved);
    }

    [Fact]
    public void FourObservationsHaveTwoSeparateSavedStaffingContracts()
    {
        var report = Report();
        Assert.Equal("e4b-mint-natural-feasibility-1", report.Version);
        Assert.Equal("actual-v15-natural-legacy-prices/no-grants", report.EvidenceScope);
        Assert.Contains("not-applied", report.ProposalScope);
        Assert.Equal(4, report.ObservedRuns);
        Assert.Equal(4, report.DiagnosticRuns);
        Assert.Equal(2, report.Observations.Count(row => row.CrewContract == "MintTen"));
        Assert.Equal(2, report.Observations.Count(row => row.CrewContract == "Legacy"));
        Assert.Equal("in-memory/not-file-provenance", report.InputSha256);
    }

    [Fact]
    public void NineEconomicHypothesesNeverBecomeObservedTimes()
    {
        var report = Report();
        Assert.Equal(9, report.EconomicHypotheses.Count);
        Assert.Equal(9, report.EconomicHypotheses.Select(h => (h.CurveId, h.Tier)).Distinct().Count());
        Assert.Equal(new[] { "A", "B", "C" }, report.EconomicHypotheses
            .Select(h => h.CurveId).Distinct().ToArray());
        Assert.All(report.EconomicHypotheses, hypothesis =>
        {
            Assert.Equal(hypothesis.WorkersForOneMint * 2, hypothesis.WorkersForTwoMints);
            Assert.True(hypothesis.ExtraRequiredResidents >= hypothesis.WorkersForOneMint);
        });
        Assert.All(report.Observations, row => Assert.Null(row.CoinsMilestoneSecond));
    }

    [Fact]
    public void CsvExportsOneRowPerObservedNaturalRun()
    {
        var csv = E4bMintNaturalFeasibilityAudit.ToCsv(Report());
        Assert.Equal(5, csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.StartsWith("scenario_id,policy_id,crew_contract,", csv);
        Assert.Contains("diagnostic-only", csv);
        Assert.DoesNotContain("within-100-to-1000", csv);
    }

    [Fact]
    public void MarkdownExplicitlySeparatesObservedRunFromUnappliedProposals()
    {
        var markdown = E4bMintNaturalFeasibilityAudit.ToMarkdown(Report());
        Assert.Contains("fatti osservati e ipotesi separate", markdown);
        Assert.Contains("not-applied", markdown);
        Assert.Contains("NON verificate nella partita", markdown);
        Assert.Contains("non è un softlock globale", markdown);
        Assert.Contains("telemetria", markdown);
    }

    [Fact]
    public void RepeatAuditIsDeterministicAndDoesNotMutateCampaignStart()
    {
        var original = JsonSerializer.Serialize(V15BenchmarkFixture.Create().Start);
        var first = JsonSerializer.Serialize(Report());
        var second = JsonSerializer.Serialize(Report());
        Assert.Equal(first, second);
        Assert.Equal(original, JsonSerializer.Serialize(V15BenchmarkFixture.Create().Start));
    }

    [Fact]
    public void UnknownSavedCrewVersionCannotBeAnalyzed()
    {
        var invalid = Mint("research-first") with { RulesVersion = "civil-growth-15-mint-unknown" };
        Assert.Throws<ArgumentException>(() => E4bMintNaturalFeasibilityAudit.Analyze([invalid]));
    }

    [Fact]
    public void MintTenCannotDisguiseItselfAsLegacyScenario()
    {
        var invalid = Mint("research-first") with { ScenarioId = "V15-I03" };
        Assert.Throws<ArgumentException>(() => E4bMintNaturalFeasibilityAudit.Analyze([invalid]));
    }

    [Fact]
    public void UnrelatedGoalIsRejectedBeforeComparingPrices()
    {
        var invalid = Mint("research-first") with { Goal = "first-swords-after-research" };
        Assert.Throws<ArgumentException>(() => E4bMintNaturalFeasibilityAudit.Analyze([invalid]));
    }

    [Fact]
    public void DifferentBaselineIsRejected()
    {
        var invalid = Mint("bread-first") with { ScenarioBaselineId = "different-game" };
        Assert.Throws<ArgumentException>(() => E4bMintNaturalFeasibilityAudit.Analyze([
            Mint("research-first"), invalid]));
    }

    [Fact]
    public void DifferentDeclaredHorizonInvalidatesLikeForLikeComparison()
    {
        var different = Mint("bread-first") with { DeclaredHorizonSeconds = 60000 };
        Assert.Throws<ArgumentException>(() => E4bMintNaturalFeasibilityAudit.Analyze([
            Mint("research-first"), different]));
    }

    [Fact]
    public void DifferentStartingStockIsRejectedEvenWithSamePolicyHorizon()
    {
        var changed = Mint("bread-first");
        var inventory = changed.InitialPhysicalStockMilliunits.ToDictionary(
            p => p.Key, p => p.Value, StringComparer.Ordinal);
        var key = inventory.Keys.First();
        inventory[key] += 1000;
        changed = changed with { InitialPhysicalStockMilliunits = inventory };
        Assert.Throws<ArgumentException>(() => E4bMintNaturalFeasibilityAudit.Analyze([
            Mint("research-first"), changed]));
    }

    [Fact]
    public void CannotCalculateMintTenAuditUsingOnlyLegacyControls()
    {
        Assert.Throws<ArgumentException>(() => E4bMintNaturalFeasibilityAudit.Analyze([
            Legacy("research-first"), Legacy("bread-first")]));
    }

    [Fact]
    public void DuplicateScenarioAndPolicyIsRejected()
    {
        var run = Mint("research-first");
        Assert.Throws<ArgumentException>(() => E4bMintNaturalFeasibilityAudit.Analyze([run, run]));
    }

    [Fact]
    public void FalseCompletionWithNoProductionMilestoneIsRejected()
    {
        var run = Mint("research-first") with { Outcome = "pass", CompletionSeconds = 0 };
        Assert.Throws<ArgumentException>(() => E4bMintNaturalFeasibilityAudit.Analyze([run]));
    }

    [Fact]
    public void NullInputAndNullExportsFailClearly()
    {
        Assert.Throws<ArgumentNullException>(() => E4bMintNaturalFeasibilityAudit.Analyze(null!));
        Assert.Throws<ArgumentNullException>(() => E4bMintNaturalFeasibilityAudit.ToCsv(null!));
        Assert.Throws<ArgumentNullException>(() => E4bMintNaturalFeasibilityAudit.ToMarkdown(null!));
    }

    [Fact]
    public void EmptyInputCannotCreateAReport()
    {
        Assert.Throws<ArgumentException>(() => E4bMintNaturalFeasibilityAudit.Analyze([]));
    }
}
