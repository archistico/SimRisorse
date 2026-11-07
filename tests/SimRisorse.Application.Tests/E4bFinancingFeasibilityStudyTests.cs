using System.Globalization;
using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class E4bFinancingFeasibilityStudyTests
{
    private static E4bFinancingComparisonReport Report() => new E4bFinancingFeasibilityStudy().Run();

    [Fact]
    public void AllCurvesTiersAndBatchesAreComparedWithoutChangingLiveCatalog()
    {
        var snapshot = JsonSerializer.Serialize(V15BenchmarkFixture.Create().Baseline);
        var report = Report();
        Assert.Equal("analytical-proposal/not-applied", report.Mode);
        Assert.Equal(27, report.Rows.Count);
        Assert.Equal(27, report.Rows.Select(r => (r.CurveId, r.Tier, r.Quantity)).Distinct().Count());
        Assert.Equal(snapshot, JsonSerializer.Serialize(V15BenchmarkFixture.Create().Baseline));
    }

    [Fact]
    public void AtomicAndSequentialUseSameTotalButDifferentSinglePaymentThresholds()
    {
        var study = new E4bEconomicCurveStudy();
        var report = Report();
        foreach (var row in report.Rows)
        {
            var curve = E4bEconomicCurveStudy.Curves.Single(c => c.Id == row.CurveId);
            var total = ResourceBundle.Empty;
            for (var i = 0; i < row.Quantity; i++)
                total += study.Quote(curve, "mint", row.Tier, i);
            Assert.Equal(total, row.AtomicQuote);
            Assert.Equal(study.Quote(curve, "mint", row.Tier, row.Quantity - 1), row.LargestSingleQuote);
            Assert.True(row.AtomicQuote.Covers(row.LargestSingleQuote));
            Assert.True(row.AtomicWarehouseRoute.Warehouses >= row.SequentialWarehouseRoute.Warehouses);
        }
    }

    [Fact]
    public void LargerAtomicBatchesCanFailWhileTheirConstituentPurchasesFit()
    {
        var report = Report();
        var a = report.Rows.Single(r => r.CurveId == "A" && r.Tier == ProductionTier.Base && r.Quantity == 10);
        Assert.Equal("warehouse-payment-exceeds-capacity", a.AtomicWarehouseRoute.Status);
        Assert.True(a.StartingCapacityEscrowInstallments > 1);
        Assert.True(a.SequentialCapacityEscrowInstallments > 1);
        Assert.True(a.SequentialWarehouseRoute.Warehouses <= a.AtomicWarehouseRoute.Warehouses);
    }

    [Fact]
    public void EscrowTranchesReconcileExactlyAndFitTheDeclaredStartingCapacity()
    {
        var report = Report();
        foreach (var row in report.Rows)
        {
            var count = row.StartingCapacityEscrowInstallments;
            Assert.True(count > 0);
            var sum = ResourceBundle.Empty;
            for (long i = 1; i <= count; i++)
            {
                var tranche = ProgressiveConstructionEscrow.Installment(row.AtomicQuote, count, i);
                Assert.True(report.InitialCapacity.Covers(tranche));
                sum += tranche;
            }
            Assert.Equal(row.AtomicQuote, sum);
        }
    }

    [Fact]
    public void ExportsAreDeterministicAndNotLocaleDependent()
    {
        var first = Report();
        var second = Report();
        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        var csv = E4bFinancingFeasibilityStudy.ToCsv(first);
        var markdown = E4bFinancingFeasibilityStudy.ToMarkdown(first);
        Assert.Equal(28, csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.Contains("analytical-proposal", JsonSerializer.Serialize(first));
        Assert.Contains("proposta non attivata", markdown);
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");
            Assert.Equal(csv, E4bFinancingFeasibilityStudy.ToCsv(second));
            Assert.Equal(markdown, E4bFinancingFeasibilityStudy.ToMarkdown(second));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }
}
