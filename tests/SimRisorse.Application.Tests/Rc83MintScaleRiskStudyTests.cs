using System.Globalization;
using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC83 comparison is an analytical proposal, not measured gameplay.</summary>
public sealed class Rc83MintScaleRiskStudyTests
{
    private static readonly Lazy<E4bMintScaleRiskReport> Cached = new(
        () => new E4bMintScaleRiskStudy().Run());

    private static E4bMintScaleRiskReport Report() => Cached.Value;

    [Theory]
    [InlineData("A", ProductionTier.Base, 1)]
    [InlineData("A", ProductionTier.Large, 3)]
    [InlineData("A", ProductionTier.Advanced, 10)]
    [InlineData("B", ProductionTier.Base, 1)]
    [InlineData("B", ProductionTier.Large, 3)]
    [InlineData("B", ProductionTier.Advanced, 10)]
    [InlineData("C", ProductionTier.Base, 1)]
    [InlineData("C", ProductionTier.Large, 3)]
    [InlineData("C", ProductionTier.Advanced, 10)]
    public void EachCurveAndTierAccountsForSecondMintAndUpstreamCapacity(
        string curveId, ProductionTier tier, int modules)
    {
        var study = new E4bEconomicCurveStudy();
        var curve = E4bEconomicCurveStudy.Curves.Single(c => c.Id == curveId);
        var row = Assert.Single(Report().Rows, r => r.CurveId == curveId && r.Tier == tier);
        Assert.Equal(10, row.CrewSize);
        Assert.Equal(modules, row.ModulesPerMint);
        Assert.Equal(10 * modules, row.OneMint.Workforce.MintWorkers);
        Assert.Equal(20 * modules, row.TwoMints.Workforce.MintWorkers);
        Assert.Equal(10 * modules, row.AdditionalMintWorkers);
        Assert.Equal(1, row.AdditionalBuildings["mint"]);
        Assert.True(row.AdditionalResidents >= row.AdditionalMintWorkers);
        Assert.Equal(row.AdditionalResidents - row.AdditionalMintWorkers, row.AdditionalSupportWorkers);
        Assert.Equal(row.TwoMints.Workforce.AdditionalLargeHouses - row.OneMint.Workforce.AdditionalLargeHouses,
            row.AdditionalLargeHouses);
        Assert.Equal(row.TwoMints.ConstructionWithoutWarehouses,
            row.OneMint.ConstructionWithoutWarehouses + row.AdditionalSupplyChainCost);
        foreach (var (id, count) in row.TwoMints.RequiredBuildings)
            Assert.Equal(count, row.OneMint.RequiredBuildings.GetValueOrDefault(id) +
                row.AdditionalBuildings.GetValueOrDefault(id));

        Assert.Equal(study.Quote(curve, "mint", tier, 1), row.SecondMintQuote);
        Assert.Equal(study.Quote(curve, "mint", tier, 0, 2), row.TwoMintsAtomicQuote);
        Assert.Equal(row.TwoMintsAtomicQuote,
            study.Quote(curve, "mint", tier) + row.SecondMintQuote);
        Assert.Equal(study.AuditWarehouseRoute(curve, row.SecondMintQuote).Status,
            row.SecondMintWarehouseRoute.Status);
        Assert.Equal(study.AuditWarehouseRoute(curve, row.TwoMintsAtomicQuote).Status,
            row.AtomicTwoMintsWarehouseRoute.Status);
        Assert.Equal(ProgressiveConstructionEscrow.MinimumInstallments(
            row.SecondMintQuote, Report().StartingCapacity),
            row.SecondMintEscrowInstallmentsAtStartingCapacity);
        Assert.Equal(ProgressiveConstructionEscrow.MinimumInstallments(
            row.TwoMintsAtomicQuote, Report().StartingCapacity),
            row.AtomicEscrowInstallmentsAtStartingCapacity);
        Assert.True(row.AtomicEscrowInstallmentsAtStartingCapacity >=
            row.SecondMintEscrowInstallmentsAtStartingCapacity);
        Assert.True(row.SecondMintStonecutterEquivalents >= row.FirstMintStonecutterEquivalents);
        Assert.Equal(E4bMintScaleRiskStudy.TargetBand(row.FirstMintStonecutterEquivalents),
            row.FirstMintTargetBand);
        Assert.Equal(E4bMintScaleRiskStudy.TargetBand(row.SecondMintStonecutterEquivalents),
            row.SecondMintTargetBand);
    }

    [Theory]
    [InlineData("0", "below-100")]
    [InlineData("99.999", "below-100")]
    [InlineData("100", "within-100-to-1000")]
    [InlineData("1000", "within-100-to-1000")]
    [InlineData("1000.001", "above-1000")]
    public void TargetBandIsInclusiveAt100And1000(string value, string expected)
    {
        Assert.Equal(expected, E4bMintScaleRiskStudy.TargetBand(
            decimal.Parse(value, CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void NineComparisonsAreDistinctAndClearlyMarkedAsNotApplied()
    {
        var report = Report();
        Assert.Equal(9, report.Rows.Count);
        Assert.Equal(9, report.Rows.Select(r => (r.CurveId, r.Tier)).Distinct().Count());
        Assert.Equal("e4b-mint-scale-risk-1", report.Version);
        Assert.Equal("analytical-proposal/not-applied", report.Mode);
        Assert.Contains("warehouse-only", report.StorageScope);
        Assert.Contains("analytical", report.WorkforceScope);
        Assert.NotEqual(ResourceBundle.Empty, report.StartingCapacity);
    }

    [Fact]
    public void StudyDoesNotMutateTheExistingCampaignOrPricingProposals()
    {
        var before = JsonSerializer.Serialize(V15BenchmarkFixture.Create().Baseline);
        var first = JsonSerializer.Serialize(Report());
        var second = JsonSerializer.Serialize(new E4bMintScaleRiskStudy().Run());
        Assert.Equal(first, second);
        Assert.Equal(before, JsonSerializer.Serialize(V15BenchmarkFixture.Create().Baseline));
        Assert.NotEmpty(V15BenchmarkFixture.Create().Baseline.Catalog.RulesVersion);
    }

    [Fact]
    public void ExportsAreDeterministicAndKeepRiskScopeExplicit()
    {
        var report = Report();
        var csv = E4bMintScaleRiskStudy.ToCsv(report);
        Assert.Equal(10, csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.StartsWith("curve,tier,modules_per_mint,", csv);
        Assert.Contains("second_mint_warehouse_route", csv);
        var markdown = E4bMintScaleRiskStudy.ToMarkdown(report);
        Assert.Contains("solo studio", markdown);
        Assert.Contains("Nessuna modifica", markdown);
        Assert.Contains("non dimostra un softlock universale", markdown);
        Assert.Equal(9, markdown.Split('\n').Count(line => line.StartsWith("| A |") ||
            line.StartsWith("| B |") || line.StartsWith("| C |")));
        Assert.Equal(csv, E4bMintScaleRiskStudy.ToCsv(report));
    }

    [Fact]
    public void InvalidNegativeTargetAndNullExportsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => E4bMintScaleRiskStudy.TargetBand(-0.001m));
        Assert.Throws<ArgumentNullException>(() => E4bMintScaleRiskStudy.ToCsv(null!));
        Assert.Throws<ArgumentNullException>(() => E4bMintScaleRiskStudy.ToMarkdown(null!));
    }
}
