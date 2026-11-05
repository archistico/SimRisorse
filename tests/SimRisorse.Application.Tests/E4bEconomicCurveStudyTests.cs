using System.Globalization;
using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class E4bEconomicCurveStudyTests
{
    private readonly E4bEconomicCurveStudy _study = new();
    private static E4bCurveDefinition Curve(string id) => E4bEconomicCurveStudy.Curves.Single(c => c.Id == id);

    [Theory]
    [InlineData("A", 3360, 2560, 0, 0)]
    [InlineData("B", 7143, 5442, 40, 20)]
    [InlineData("C", 11161, 8504, 120, 80)]
    public void FirstMintPricesMatchDeclaredProposals(string id, int planks, int stone, int iron, int gold)
    {
        var cost = _study.Quote(Curve(id), "mint", ProductionTier.Base);
        Assert.Equal(planks, cost["planks"].Units);
        Assert.Equal(stone, cost["stone"].Units);
        Assert.Equal(iron, cost["iron"].Units);
        Assert.Equal(gold, cost["gold"].Units);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    [InlineData("C")]
    public void AllTiersAndBatchesPreserveHistoryAndNeverDiscountCopies(string id)
    {
        var curve = Curve(id);
        foreach (var tier in Enum.GetValues<ProductionTier>())
        {
            var sum = ResourceBundle.Empty;
            for (int i = 0; i < 10; i++)
            {
                var price = _study.Quote(curve, "mint", tier, i);
                if (i > 0) Assert.True(price.Covers(_study.Quote(curve, "mint", tier, i - 1)));
                sum += price;
                if (i == 4) Assert.Equal(sum, _study.Quote(curve, "mint", tier, 0, 5));
            }
            Assert.Equal(sum, _study.Quote(curve, "mint", tier, 0, 10));
        }
    }

    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    [InlineData("C")]
    public void UpgradesTelescopeAtSameCounterAndActualPurchasePathHasNoDiscount(string id)
    {
        var row = _study.Run().Upgrades.Single(r => r.CurveId == id);
        Assert.True(row.SameCounterTelescopes);
        Assert.True(row.NextCounterPathNoCheaper);
        Assert.True(row.PaidViaBaseLargeAdvanced.Covers(row.DirectAdvanced));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(9, 0)]
    [InlineData(10, 1)]
    [InlineData(19, 1)]
    [InlineData(20, 2)]
    public void TwoMintsNeedWholeDistinctTeams(int assigned, int running)
    {
        var result = E4bEconomicCurveStudy.ProjectMintCrew(2, ProductionTier.Base, assigned, 10000, 10000, 30000);
        Assert.Equal(running, result.RunnableModules);
        Assert.Equal(assigned, result.ReservedInactiveWorkers + 10 * running);
        Assert.Equal(running * 1000L, result.GoldConsumedMilliunits);
        Assert.Equal(running * 1000L, result.CoalConsumedMilliunits);
        Assert.Equal(running * 3000L, result.CoinsProducedMilliunits);
    }

    [Theory]
    [InlineData(0, 1000, 3000)]
    [InlineData(1000, 0, 3000)]
    [InlineData(1000, 1000, 2999)]
    public void MissingInputsOrFullOutputReserveWorkersWithoutConsumption(long gold, long coal, long space)
    {
        var result = E4bEconomicCurveStudy.ProjectMintCrew(1, ProductionTier.Base, 10, gold, coal, space);
        Assert.Equal(0, result.RunnableModules);
        Assert.Equal(10, result.ReservedInactiveWorkers);
        Assert.Equal(0, result.GoldConsumedMilliunits + result.CoalConsumedMilliunits + result.CoinsProducedMilliunits);
    }

    [Theory]
    [InlineData(1, ProductionTier.Base, 23, 10)]
    [InlineData(2, ProductionTier.Base, 36, 20)]
    [InlineData(1, ProductionTier.Large, 49, 30)]
    [InlineData(1, ProductionTier.Advanced, 143, 100)]
    public void SupportingPopulationCanFeedAndHouseItself(int buildings, ProductionTier tier, int population, int workers)
    {
        var row = E4bEconomicCurveStudy.Workforce(buildings, tier);
        Assert.Equal(population, row.RequiredResidents);
        Assert.Equal(workers, row.MintWorkers);
        Assert.True(row.HousingCapacity >= population);
        Assert.True(row.FisherWorkers >= row.RequiredFoodPerSecond);
        Assert.True(row.DepotWorkers >= row.RequiredFoodPerSecond);
        Assert.True(row.BakeryWorkers * 3 >= row.RequiredBreadPerSecond);
        Assert.True(row.MillWorkers >= row.BakeryWorkers);
        Assert.True(row.FarmWorkers >= row.RequiredBreadPerSecond * 2 / 3);
        Assert.Equal(population, workers + row.GoldMineWorkers + row.GoldSmelterWorkers + row.CoalMineWorkers +
            row.FoundationAndResearchWorkers + row.FisherWorkers + row.DepotWorkers + row.FarmWorkers +
            row.WellWorkers + row.MillWorkers + row.BakeryWorkers);
    }

    [Fact]
    public void AdvancedMaterialsAddIronBranchOnceWithoutRebuyingSharedPrerequisites()
    {
        var a = _study.Investment(Curve("A"), 1, ProductionTier.Base);
        var b = _study.Investment(Curve("B"), 1, ProductionTier.Base);
        Assert.False(a.RequiredBuildings.ContainsKey("iron-mine"));
        Assert.Equal(1, b.RequiredBuildings["iron-mine"]);
        Assert.Equal(1, b.RequiredBuildings["iron-smelter"]);
        Assert.Equal(1, b.RequiredBuildings["coal-mine"]);
        Assert.Equal(1, b.RequiredBuildings["school"]);
        Assert.Equal(2, b.RequiredBuildings["small-house"]);
        Assert.True(b.ConstructionWithoutWarehouses.Covers(b.CurrentMintPayment));
    }

    [Fact]
    public void BatchLiquidityAndUpgradeRouteAreDifferentFromSinglePurchase()
    {
        var report = _study.Run();
        var a = report.Quotes.Single(r => r.CurveId == "A" && r.FamilyId == "mint" && r.Tier == ProductionTier.Base);
        Assert.Equal("fits-warehouse-route", a.BatchFiveLiquidity.Status);
        Assert.Equal("warehouse-payment-exceeds-capacity", a.BatchTenLiquidity.Status);
        var b = report.Upgrades.Single(r => r.CurveId == "B");
        Assert.Equal("warehouse-payment-exceeds-capacity", b.DirectLiquidity.Status);
        Assert.Equal("fits-warehouse-route", b.UpgradeLiquidity.Status);
        Assert.False(b.DirectLiquidity.Capacity.Covers(b.DirectLiquidity.BlockedWarehouseQuote!));
        var advanced = report.Investments.Single(r => r.CurveId == "C" && r.MintBuildings == 1 && r.Tier == ProductionTier.Advanced);
        Assert.Null(advanced.ConstructionWithWarehouses);
        Assert.NotEqual(ResourceBundle.Empty, advanced.ConstructionWithoutWarehouses);
    }

    [Fact]
    public void MaterialWorkIncludesEveryRecipeStageAndKeepsReferenceUninflated()
    {
        // 30 planks: 30 sawmill-seconds + 30 log-worker-seconds; 10 stone: 10 seconds.
        Assert.Equal(70m, _study.ReferenceMaterialWorkerSeconds(ResourceBundle.Create(("planks", 30m), ("stone", 10m))));
        // Each metal: one mining second (iron needs two ore), one smelting second, half a coal second.
        Assert.Equal(5m, _study.ReferenceMaterialWorkerSeconds(ResourceBundle.Create(("gold", 1m), ("iron", 1m))));
    }

    [Fact]
    public void InvalidAndUnrepresentableQuotationsFailInsteadOfWrapping()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _study.Quote(Curve("A"), "mint", ProductionTier.Base, -1));
        Assert.Throws<ArgumentException>(() => _study.EntryCost(Curve("A") with { CopyGrowth = .9m }, "mint"));
        Assert.Throws<OverflowException>(() => _study.Quote(Curve("C"), "mint", ProductionTier.Advanced, 1000));
        Assert.Throws<ArgumentOutOfRangeException>(() => E4bEconomicCurveStudy.ProjectMintCrew(1, ProductionTier.Base, 11, 0, 0, 0));
    }

    [Fact]
    public void StudyIsDeterministicAndDoesNotModifyCatalogOrInventMeasurements()
    {
        var before = JsonSerializer.Serialize(V15BenchmarkFixture.Create().Baseline);
        var first = _study.Run();
        var second = new E4bEconomicCurveStudy().Run();
        Assert.Equal("analytical-proposal/not-applied", first.Mode);
        Assert.Null(first.MeasuredCurrentBaseline);
        Assert.Equal(81, first.Quotes.Count);
        Assert.Equal(18, first.Investments.Count);
        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        Assert.Equal(before, JsonSerializer.Serialize(V15BenchmarkFixture.Create().Baseline));
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");
            Assert.Equal(E4bCurveStudyExport.QuotesCsv(first), E4bCurveStudyExport.QuotesCsv(second));
            Assert.Contains("non finanziabile", E4bCurveStudyExport.ToMarkdown(first));
            Assert.Contains("warehouse-payment-exceeds-capacity", E4bCurveStudyExport.InvestmentsCsv(first));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Fact]
    public void DiagnosticMeasurementsCannotBePresentedAsTheNaturalBaseline()
    {
        var diagnostic = V15IndustrialProgressionBenchmark.Run("V15-I01", "research-first", 0);
        Assert.Throws<ArgumentException>(() => _study.Run([diagnostic]));
    }
}
