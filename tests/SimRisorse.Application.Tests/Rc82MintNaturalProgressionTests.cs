using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC82: natural MintTen uses the real v15 start, not RC81 funded fixtures.</summary>
public sealed class Rc82MintNaturalProgressionTests
{
    private static V15IndustrialProgressionRun Mint(string policy, int seconds = 0) =>
        V15IndustrialProgressionBenchmark.RunMintTen(policy, seconds);

    [Theory]
    [InlineData(V15IndustrialProgressionBenchmark.PolicyResearchFirst)]
    [InlineData(V15IndustrialProgressionBenchmark.PolicyBreadFirst)]
    public void FreshGameMintRunIsExplicitlyVersionedAndHasNoGrants(string policy)
    {
        var result = Mint(policy);
        var initial = V15BenchmarkFixture.Create().Start;
        Assert.Equal("E4B-M01", result.ScenarioId);
        Assert.Equal(V15CrewRules.MintTenRulesVersion, result.RulesVersion);
        Assert.Equal("fresh-game/no-grants", result.Fixture);
        Assert.Equal("timeout", result.Outcome);
        Assert.True(result.Diagnostic);
        Assert.Null(result.CompletionSeconds);
        Assert.Equal(0, Assert.Single(result.Samples).ElapsedSeconds);
        Assert.Equal("civil-growth-13", result.BaselineRulesVersion);
        Assert.Equal(initial.CityCampaign.ScenarioId, result.ScenarioBaselineId);
        Assert.DoesNotContain(result.Events, e => e.Result == "milestone");
    }

    [Theory]
    [InlineData(V15IndustrialProgressionBenchmark.PolicyResearchFirst)]
    [InlineData(V15IndustrialProgressionBenchmark.PolicyBreadFirst)]
    public void CrewStrategyRequiresOneAtomicAssignmentOfTenWorkers(string policy)
    {
        var steps = V15IndustrialProgressionBenchmark.Steps("V15-I03", policy, V15CrewMode.MintTen);
        Assert.Equal("staff-mint-team", steps[^1]);
        Assert.Equal(1, steps.Count(s => s == "staff-mint-team"));
        Assert.DoesNotContain("staff-mint", steps);
        Assert.Equal("build-mint", steps[^2]);
        var original = V15IndustrialProgressionBenchmark.Steps("V15-I03", policy);
        Assert.Equal(original.Count, steps.Count);
        Assert.Equal(original.SkipLast(1), steps.SkipLast(1));
    }

    [Theory]
    [InlineData(V15IndustrialProgressionBenchmark.PolicyResearchFirst)]
    [InlineData(V15IndustrialProgressionBenchmark.PolicyBreadFirst)]
    public void SameStartingInventoryForLegacyAndTenWorkerModes(string policy)
    {
        var mint = Mint(policy);
        var legacy = V15IndustrialProgressionBenchmark.Run("V15-I03", policy, 0);
        Assert.Equal(V15FoodCampaignCheckpoints.RulesVersion, legacy.RulesVersion);
        Assert.Equal(legacy.Fixture, mint.Fixture);
        Assert.Equal(legacy.InitialPhysicalStockMilliunits.OrderBy(p => p.Key),
            mint.InitialPhysicalStockMilliunits.OrderBy(p => p.Key));
        Assert.Equal(legacy.Samples[0].Population, mint.Samples[0].Population);
        Assert.Equal(legacy.Samples[0].FreeWorkers, mint.Samples[0].FreeWorkers);
    }

    [Theory]
    [InlineData("V15-I01")]
    [InlineData("V15-I02")]
    public void DifferentProgressionGoalCannotMasqueradeAsMintExperiment(string scenario)
    {
        Assert.Throws<ArgumentException>(() => V15IndustrialProgressionBenchmark.Run(
            scenario, "research-first", 0, V15CrewMode.MintTen));
        Assert.Throws<ArgumentException>(() => V15IndustrialProgressionBenchmark.Steps(
            scenario, "research-first", V15CrewMode.MintTen));
    }

    [Fact]
    public void UnknownCrewModeIsRejectedBeforeExecutingAnyAction()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => V15IndustrialProgressionBenchmark.Run(
            "V15-I03", "research-first", 0, (V15CrewMode)42));
    }

    [Fact]
    public void InvalidPolicyAndNegativeHorizonAreRejected()
    {
        Assert.Throws<ArgumentException>(() => Mint("unsupported"));
        Assert.Throws<ArgumentOutOfRangeException>(() => Mint("research-first", -1));
    }

    [Theory]
    [InlineData(V15IndustrialProgressionBenchmark.PolicyResearchFirst)]
    [InlineData(V15IndustrialProgressionBenchmark.PolicyBreadFirst)]
    public void OneSecondIsNotInventedProofOfTenWorkerMintProduction(string policy)
    {
        var run = Mint(policy, 1);
        Assert.True(run.Diagnostic);
        if (run.Outcome != "pass")
        {
            Assert.Null(run.CompletionSeconds);
            Assert.DoesNotContain(run.Events, e => e.Result == "milestone");
        }
        Assert.All(run.Events, e => Assert.True(e.ResourceCostsMilliunits.Values.All(n => n >= 0)));
        Assert.Equal(1, run.Samples[^1].ElapsedSeconds);
    }

    [Theory]
    [InlineData(V15IndustrialProgressionBenchmark.PolicyResearchFirst)]
    [InlineData(V15IndustrialProgressionBenchmark.PolicyBreadFirst)]
    public void ShortRunsAreDeterministicAndDoNotMutateTheBaseline(string policy)
    {
        var initial = JsonSerializer.Serialize(V15BenchmarkFixture.Create().Start);
        var left = Mint(policy, 61);
        var right = Mint(policy, 61);
        Assert.Equal(JsonSerializer.Serialize(left), JsonSerializer.Serialize(right));
        Assert.Equal(initial, JsonSerializer.Serialize(V15BenchmarkFixture.Create().Start));
        Assert.Equal("E4B-M01", left.ScenarioId);
    }

    [Fact]
    public void ExistingLegacyStudyAndOutputsRemainUnchangedByNewMode()
    {
        var original = V15IndustrialProgressionBenchmark.Run("V15-I03", "bread-first", 0);
        var explicitLegacy = V15IndustrialProgressionBenchmark.Run("V15-I03", "bread-first", 0,
            V15CrewMode.Legacy);
        Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(explicitLegacy));
        Assert.Equal("staff-mint", V15IndustrialProgressionBenchmark.Steps(
            "V15-I03", "bread-first")[^1]);
    }

    [Fact]
    public void NaturalAnalyzerDoesNotConflateDifferentStaffingContracts()
    {
        var mintResearch = Mint("research-first");
        var mintBread = Mint("bread-first");
        var legacy = V15IndustrialProgressionBenchmark.Run("V15-I03", "research-first", 0);
        var analysis = V15IndustrialProgressionAnalysis.Analyze([mintResearch, mintBread, legacy]);
        Assert.Equal(3, analysis.RunCount);
        Assert.Equal(3, analysis.Diagnostics);
        Assert.Equal(0, analysis.Passed);
        var comparison = Assert.Single(analysis.Comparisons);
        Assert.Equal("E4B-M01", comparison.ScenarioId);
        Assert.Null(comparison.BreadFirstMinusResearchFirstSeconds);
        Assert.Equal(3, V15IndustrialConstraintAnalysis.Analyze([mintResearch, mintBread, legacy]).Runs.Count);
    }

    [Fact]
    public void MintTenAndLegacyPoliciesKeepSameRequiredPrerequisites()
    {
        foreach (var policy in new[] { "research-first", "bread-first" })
        {
            var mint = V15IndustrialProgressionBenchmark.Steps("V15-I03", policy, V15CrewMode.MintTen);
            Assert.Contains("build-large-house", mint);
            Assert.Contains("staff-bakery", mint);
            Assert.Contains("staff-coal-mine", mint);
            Assert.Contains("buy-tech-industria-1", mint);
            Assert.Contains("build-gold-smelter", mint);
            Assert.Contains("build-mint", mint);
        }
    }
}
