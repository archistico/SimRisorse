using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC89: guardrails are analytical; never claim that an unplayed
/// proposal is achievable on the actual campaign clock.</summary>
public sealed class Rc89PriceReadinessTests
{
    private static readonly Lazy<E4bPriceReadinessReport> Cached = new(
        () => new E4bPriceReadinessStudy().Run());
    private static E4bPriceReadinessReport Report() => Cached.Value;

    [Theory]
    [InlineData("A", ProductionTier.Base, 10)]
    [InlineData("A", ProductionTier.Large, 30)]
    [InlineData("A", ProductionTier.Advanced, 100)]
    [InlineData("B", ProductionTier.Base, 10)]
    [InlineData("B", ProductionTier.Large, 30)]
    [InlineData("B", ProductionTier.Advanced, 100)]
    [InlineData("C", ProductionTier.Base, 10)]
    [InlineData("C", ProductionTier.Large, 30)]
    [InlineData("C", ProductionTier.Advanced, 100)]
    public void AllNineCandidatesReconcilePriceCapacityAndWorkers(
        string id, ProductionTier tier, int workers)
    {
        var report = Report();
        var row = Assert.Single(report.Rows, r => r.CurveId == id && r.Tier == tier);
        var study = new E4bEconomicCurveStudy();
        var curve = E4bEconomicCurveStudy.Curves.Single(c => c.Id == id);
        Assert.Equal(workers, row.WorkersForOneMint);
        Assert.Equal(workers * 2, row.WorkersForTwoMints);
        Assert.Equal(study.Quote(curve, "mint", tier), row.FirstMint);
        Assert.Equal(study.Quote(curve, "mint", tier, 1), row.SecondMint);
        Assert.Equal(row.FirstMint + row.SecondMint, row.AtomicTwoMints);
        Assert.Equal(study.Quote(curve, "mint", tier, 0, 2), row.AtomicTwoMints);
        Assert.Equal(row.FirstCapacityGaps.Count == 0, row.FirstFitsInitialCapacity);
        Assert.Equal(row.SecondCapacityGaps.Count == 0, row.SecondFitsInitialCapacity);
        Assert.Equal(row.AtomicCapacityGaps.Count == 0, row.AtomicTwoFitsInitialCapacity);
        Assert.Equal(ProgressiveConstructionEscrow.MinimumInstallments(
            row.FirstMint, report.InitialCapacity), row.FirstEscrowInstallments);
        Assert.Equal(ProgressiveConstructionEscrow.MinimumInstallments(
            row.SecondMint, report.InitialCapacity), row.SecondEscrowInstallments);
        Assert.Equal(ProgressiveConstructionEscrow.MinimumInstallments(
            row.AtomicTwoMints, report.InitialCapacity), row.AtomicEscrowInstallments);
        Assert.True(row.AtomicEscrowInstallments >= row.FirstEscrowInstallments);
        Assert.True(row.AtomicEscrowInstallments >= row.SecondEscrowInstallments);
    }

    [Theory]
    [InlineData(ProductionTier.Base, "below-100", "fits-warehouse-route", "fits-warehouse-route", "recalibrate-base-price")]
    [InlineData(ProductionTier.Base, "within-100-to-1000", "warehouse-payment-exceeds-capacity", "fits-warehouse-route", "review-progressive-funding")]
    [InlineData(ProductionTier.Advanced, "above-1000", "fits-warehouse-route", "fits-warehouse-route", "needs-natural-playtest")]
    public void DecisionPrioritiesNeverPretendProposalsAreApproved(
        ProductionTier tier, string band, string firstStatus, string secondStatus, string expected)
    {
        E4bStorageAudit Audit(string status) => new(status, 0, ResourceBundle.Empty,
            ResourceBundle.Empty, ResourceBundle.Empty, null);
        Assert.Equal(expected, E4bPriceReadinessStudy.Priority(tier, band,
            Audit(firstStatus), Audit(secondStatus)));
    }

    [Fact]
    public void CapacityGapsTreatMissingSparseKeysAsZero()
    {
        var quote = ResourceBundle.Create(("stone", 10m), ("planks", 4m));
        var capacity = ResourceBundle.Create(("stone", 11m));
        Assert.Equal(new[] { "planks" }, E4bPriceReadinessStudy.CapacityGaps(quote, capacity));
        Assert.Empty(E4bPriceReadinessStudy.CapacityGaps(ResourceBundle.Empty, capacity));
        Assert.Throws<ArgumentNullException>(() => E4bPriceReadinessStudy.CapacityGaps(null!, capacity));
        Assert.Throws<ArgumentNullException>(() => E4bPriceReadinessStudy.CapacityGaps(quote, null!));
    }

    [Fact]
    public void NoImportedRunsAreNotConfusedWithZeroSecondMeasurements()
    {
        var report = Report();
        Assert.Equal(E4bPriceReadinessStudy.Version, report.Version);
        Assert.Equal("analytical-proposal/not-applied", report.Mode);
        Assert.Equal(9, report.Rows.Count);
        Assert.Equal(0, report.ImportedRuns);
        Assert.Equal(0, report.VerifiedNaturalRuns);
        Assert.Equal(0, report.VerifiedMintTenRuns);
        Assert.Equal(0, report.DiagnosticRuns);
        Assert.Contains("not-approved", report.ApprovalStatus);
        Assert.All(report.Rows, row => Assert.NotEmpty(row.ReviewPriority));
    }

    [Fact]
    public void ImportedDiagnosticRemainsUnverifiedAndUnplayedAtCandidatePrices()
    {
        var run = V15IndustrialProgressionBenchmark.RunMintTen("research-first", 0);
        var report = new E4bPriceReadinessStudy().Run([run]);
        Assert.Equal(1, report.ImportedRuns);
        Assert.Equal(1, report.DiagnosticRuns);
        Assert.Equal(0, report.VerifiedNaturalRuns);
        Assert.Equal(0, report.VerifiedMintTenRuns);
        Assert.Contains("counterfactual", report.EvidenceScope);
        Assert.All(report.Rows, row => Assert.NotEqual(ResourceBundle.Empty, row.FirstMint));
    }

    [Fact]
    public void ForgedOrUnsupportedIndustrialEvidenceIsRejected()
    {
        var diagnostic = V15IndustrialProgressionBenchmark.RunMintTen("bread-first", 0);
        Assert.Throws<ArgumentException>(() => new E4bPriceReadinessStudy().Run([
            diagnostic with { Outcome = "pass" }]));
        Assert.Throws<ArgumentException>(() => new E4bPriceReadinessStudy().Run([]));
        Assert.Throws<ArgumentException>(() => new E4bPriceReadinessStudy().Run([
            V15IndustrialProgressionBenchmark.Run("V15-I01", "research-first", 0)]));
    }

    [Fact]
    public void CandidateReportIsDeterministicAndDoesNotChangeBaseline()
    {
        var before = JsonSerializer.Serialize(V15BenchmarkFixture.Create().Baseline);
        Assert.Equal(JsonSerializer.Serialize(Report()),
            JsonSerializer.Serialize(new E4bPriceReadinessStudy().Run()));
        Assert.Equal(before, JsonSerializer.Serialize(V15BenchmarkFixture.Create().Baseline));
    }

    [Fact]
    public void ExportsDistinguishRatesRoutesAndMissingEvidence()
    {
        var report = Report();
        var csv = E4bPriceReadinessStudy.ToCsv(report);
        var md = E4bPriceReadinessStudy.ToMarkdown(report);
        Assert.Equal(10, csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.Contains("atomic_escrow_installments", csv);
        Assert.Contains("first_quote,second_quote,atomic_two_quote", csv);
        Assert.Contains("first_capacity_gaps", csv);
        Assert.Contains("Nessun prezzo", md);
        Assert.Contains("non simula", md, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("non dimostra", md);
        Assert.Equal(csv, E4bPriceReadinessStudy.ToCsv(report));
        Assert.Throws<ArgumentNullException>(() => E4bPriceReadinessStudy.ToCsv(null!));
        Assert.Throws<ArgumentNullException>(() => E4bPriceReadinessStudy.ToMarkdown(null!));
    }
}
