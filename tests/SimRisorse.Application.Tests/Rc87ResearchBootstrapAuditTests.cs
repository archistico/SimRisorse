using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Research;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC87: a first research unlock is not the subsequent coal/gold chain.</summary>
public sealed class Rc87ResearchBootstrapAuditTests
{
    private static V15IndustrialProgressionRun Diagnostic(string policy, int seconds = 0) =>
        V15IndustrialProgressionBenchmark.Run("V15-I01", policy, seconds);

    private static V15IndustrialProgressionRun[] Pair() =>
    [
        Diagnostic(V15IndustrialProgressionBenchmark.PolicyResearchFirst),
        Diagnostic(V15IndustrialProgressionBenchmark.PolicyBreadFirst)
    ];

    private static V15IndustrialProgressionRun Completed(string policy, int seconds = 1800)
    {
        var starting = Diagnostic(policy);
        var spent = V15BenchmarkFixture.Create().Technologies.Technologies.Single(t =>
            t.Id == "estrazione-1").Cost.MilliPoints;
        return starting with
        {
            Outcome = "pass", Diagnostic = false, EffectiveHorizonSeconds = starting.DeclaredHorizonSeconds,
            CompletionSeconds = seconds, StopReason = "Observed via actual school research",
            Events =
            [
                new(0, "build-school", "executed", "",
                    new Dictionary<string, long> { ["planks"] = 120000L, ["stone"] = 80000L }, 0),
                new(60, "enroll-student", "executed", "",
                    new Dictionary<string, long> { ["pencils"] = 2000L, ["food"] = 10000L }, 0),
                new(180, "assign-researcher", "executed", "", new Dictionary<string, long>(), 0),
                new(seconds, "buy-tech-estrazione-1", "executed", "", new Dictionary<string, long>(), spent),
                new(seconds, "unlock-estrazione-1", "milestone", "Confirmed purchase",
                    new Dictionary<string, long>(), 0)
            ],
            Samples = [starting.Samples[0], starting.Samples[0] with
            {
                ElapsedSeconds = seconds, NextAction = "completed", ExtractionOneUnlocked = true,
                Schools = 1, QualifiedResearchers = 1, ActiveResearchers = 1
            }]
        };
    }

    [Theory]
    [InlineData("research-first")]
    [InlineData("bread-first")]
    public void FirstResearchStopsBeforeAnyCoalOrMintConstruction(string policy)
    {
        var steps = V15IndustrialProgressionBenchmark.Steps("V15-I01", policy);
        Assert.Equal("buy-tech-estrazione-1", steps[^1]);
        Assert.DoesNotContain("build-coal-mine", steps);
        Assert.DoesNotContain("staff-coal-mine", steps);
        Assert.DoesNotContain("craft-pencils", steps);
        Assert.True(steps.ToList().IndexOf("build-school") < steps.ToList().IndexOf("enroll-student"));
        Assert.True(steps.ToList().IndexOf("enroll-student") < steps.ToList().IndexOf("assign-researcher"));
    }

    [Fact]
    public void BreadFirstKeepsItsRealFoodChainInvestment()
    {
        var bread = V15IndustrialProgressionBenchmark.Steps("V15-I01", "bread-first");
        var research = V15IndustrialProgressionBenchmark.Steps("V15-I01", "research-first");
        Assert.Contains("build-bakery", bread);
        Assert.DoesNotContain("build-bakery", research);
        Assert.True(bread.ToList().IndexOf("build-bakery") < bread.ToList().IndexOf("build-school"));
    }

    [Fact]
    public void LongerMintPathStillConstructsCoalAfterResearch()
    {
        var mint = V15IndustrialProgressionBenchmark.Steps("V15-I03", "research-first");
        Assert.True(mint.ToList().IndexOf("buy-tech-estrazione-1") <
            mint.ToList().IndexOf("build-coal-mine"));
        Assert.True(mint.ToList().IndexOf("build-coal-mine") <
            mint.ToList().IndexOf("build-mint"));
    }

    [Fact]
    public void PreflightUsesRealCatalogueAndMunicipalStock()
    {
        var audit = V15ResearchBootstrapAudit.Analyze(Pair());
        Assert.Equal(2000L, audit.StartingPencilsMilliunits);
        Assert.Equal(2000L, audit.PencilsPerStudentMilliunits);
        Assert.Equal(150000L, audit.ExtractionCostMilliPoints);
        Assert.Equal(2, audit.RunCount);
        Assert.Equal(0, audit.VerifiedRuns);
        Assert.Null(audit.BreadMinusResearchSeconds);
    }

    [Theory]
    [InlineData("research-first")]
    [InlineData("bread-first")]
    public void ZeroSecondsCannotBeCountedAsAnUnlock(string policy)
    {
        var row = Assert.Single(V15ResearchBootstrapAudit.Analyze([Diagnostic(policy)]).Observations);
        Assert.True(row.Diagnostic);
        Assert.Equal("timeout", row.Outcome);
        Assert.False(row.Verified);
        Assert.False(row.TechnologyPurchased);
        Assert.Null(row.CompletionSeconds);
    }

    [Fact]
    public void ObservedResearchNeedsTheFullPurchaseChain()
    {
        var audit = V15ResearchBootstrapAudit.Analyze([
            Completed("research-first"), Completed("bread-first", 1860)]);
        Assert.Equal(2, audit.VerifiedRuns);
        Assert.Equal(60L, audit.BreadMinusResearchSeconds);
        Assert.All(audit.Observations, row =>
        {
            Assert.True(row.SchoolBuilt);
            Assert.True(row.StudentEnrolled);
            Assert.True(row.ResearcherAssigned);
            Assert.True(row.TechnologyPurchased);
            Assert.Equal(2000L, row.PencilsSpentMilliunits);
            Assert.Equal(150000L, row.ResearchSpentMilliPoints);
            Assert.True(row.ExtractionObserved);
            Assert.True(row.Verified);
        });
    }

    [Fact]
    public void SyntheticProofDoesNotAlterRealCampaignOrPretendToBeARealRun()
    {
        var first = JsonSerializer.Serialize(V15BenchmarkFixture.Create().Start);
        _ = V15ResearchBootstrapAudit.Analyze([Completed("research-first")]);
        Assert.Equal(first, JsonSerializer.Serialize(V15BenchmarkFixture.Create().Start));
    }

    [Fact]
    public void MissingReceiptCannotBeLaunderedIntoPass()
    {
        var run = Completed("research-first");
        var withoutEnrollment = run with { Events = run.Events.Where(e => e.Action != "enroll-student").ToArray() };
        Assert.Throws<ArgumentException>(() => V15ResearchBootstrapAudit.Analyze([withoutEnrollment]));
    }

    [Fact]
    public void FalsePencilReceiptIsRejected()
    {
        var run = Completed("research-first");
        var changed = run.Events.Select(e => e.Action == "enroll-student" ?
            e with { ResourceCostsMilliunits = new Dictionary<string, long> { ["pencils"] = 1000L } } : e).ToArray();
        Assert.Throws<ArgumentException>(() => V15ResearchBootstrapAudit.Analyze([run with { Events = changed }]));
        var wrongFood = run.Events.Select(e => e.Action == "enroll-student" ?
            e with { ResourceCostsMilliunits = new Dictionary<string, long>
                { ["pencils"] = 2000L, ["food"] = 1000L } } : e).ToArray();
        Assert.Throws<ArgumentException>(() => V15ResearchBootstrapAudit.Analyze([run with { Events = wrongFood }]));
    }

    [Fact]
    public void FalseResearchPriceIsRejected()
    {
        var run = Completed("research-first");
        var changed = run.Events.Select(e => e.Action == "buy-tech-estrazione-1" ?
            e with { ResearchSpentMilliPoints = 1 } : e).ToArray();
        Assert.Throws<ArgumentException>(() => V15ResearchBootstrapAudit.Analyze([run with { Events = changed }]));
    }

    [Fact]
    public void MissingClockEvidenceIsRejected()
    {
        var run = Completed("research-first");
        var samples = run.Samples.ToArray();
        samples[^1] = samples[^1] with { ExtractionOneUnlocked = false };
        Assert.Throws<ArgumentException>(() => V15ResearchBootstrapAudit.Analyze([run with { Samples = samples }]));
        Assert.Throws<ArgumentException>(() => V15ResearchBootstrapAudit.Analyze([
            Completed("research-first", seconds: 180)])); // enough for training, not research
    }

    [Fact]
    public void WrongInitialPencilQuantityIsRejected()
    {
        var run = Diagnostic("research-first");
        var stock = run.InitialPhysicalStockMilliunits.ToDictionary(p => p.Key, p => p.Value);
        stock["pencils"] = 0;
        Assert.Throws<ArgumentException>(() => V15ResearchBootstrapAudit.Analyze([
            run with { InitialPhysicalStockMilliunits = stock }]));
        stock["pencils"] = 2000L;
        var other = stock.Keys.First(id => id != "pencils");
        stock[other]++;
        Assert.Throws<ArgumentException>(() => V15ResearchBootstrapAudit.Analyze([
            run with { InitialPhysicalStockMilliunits = stock }]));
    }

    [Fact]
    public void CoalCraftingBeforeEnrolmentCannotMasqueradeAsMunicipalBootstrap()
    {
        var run = Completed("research-first");
        Assert.Throws<ArgumentException>(() => V15ResearchBootstrapAudit.Analyze([
            run with { Events = [new(0, "craft-pencils", "executed", "",
                new Dictionary<string, long>(), 0), .. run.Events] }]));
    }

    [Fact]
    public void DuplicatedPolicyAndWrongGoalAreRejected()
    {
        var run = Diagnostic("research-first");
        Assert.Throws<ArgumentException>(() => V15ResearchBootstrapAudit.Analyze([run, run]));
        Assert.Throws<ArgumentException>(() => V15ResearchBootstrapAudit.Analyze([
            run with { Goal = "first-coins-after-research" }]));
        Assert.Throws<ArgumentException>(() => V15ResearchBootstrapAudit.Analyze([]));
        Assert.Throws<ArgumentNullException>(() => V15ResearchBootstrapAudit.Analyze(null!));
        Assert.Throws<ArgumentException>(() => V15ResearchBootstrapAudit.Analyze([null!]));
    }

    [Fact]
    public void ExportsKeepDiagnosticTimingUnknown()
    {
        var report = V15ResearchBootstrapAudit.Analyze(Pair());
        var csv = V15ResearchBootstrapAudit.ToCsv(report);
        var md = V15ResearchBootstrapAudit.ToMarkdown(report);
        Assert.Equal(3, csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.Contains("research-first,timeout,true", csv);
        Assert.Contains("NON prova un softlock", md);
        Assert.Contains("non determinabile", md);
    }

    [Fact]
    public void RepeatableShortNaturalRunIsNotAnInventedSuccessfulResearch()
    {
        var run = Diagnostic("research-first", 61);
        var audit = V15ResearchBootstrapAudit.Analyze([run]);
        Assert.Equal("natural-v15/real-receipts/no-grants", audit.EvidenceScope);
        if (run.Outcome != "pass")
        {
            Assert.Null(run.CompletionSeconds);
            Assert.False(audit.Observations[0].Verified);
        }
        Assert.All(run.Events, item => Assert.True(item.ResourceCostsMilliunits.Values.All(n => n >= 0)));
    }
}
