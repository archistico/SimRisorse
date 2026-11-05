using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class E4aV15IndustrialRecoveryTests
{
    private static V15CampaignAdvancePlan Tick(V15BenchmarkFixture setup, V15FoodCampaignCheckpoint state,
        bool diagnostics = true, int seconds = 1) => V15UnifiedCampaignClock.Plan(state, setup.Baseline,
            setup.School, setup.Technologies, FoodIndustryConstructionCatalog.Candidate, setup.Meat,
            setup.Crops, setup.Depot, TimeSpan.FromSeconds(seconds), diagnostics);

    private static V15FoodCampaignCheckpoint Stock(V15FoodCampaignCheckpoint state, string id, decimal units) =>
        state with { CityCampaign = state.CityCampaign with { Resources = ResourceBundle.FromAmounts(
            state.CityCampaign.Resources.Amounts.Where(kv => kv.Key != id)) + ResourceBundle.Create((id, units)) } };

    [Theory]
    [InlineData("V15-R01", "forge", 240000)]
    [InlineData("V15-R02", "mint", 360000)]
    public void SaturatedOutputHasMeasuredStorageOnlyLossAndNoInventedProduction(string id, string family, long loss)
    {
        var run = V15IndustrialRecoveryBenchmark.RunScenario(id)[0];
        Assert.Equal("controlled/declared-grants", run.Fixture);
        Assert.False(run.Diagnostic);
        Assert.Null(run.FirstProductionSecond);
        Assert.Equal(0, run.ProducedTargetMilliunits);
        Assert.Equal(120, run.MealsServed);
        Assert.Equal(0, run.MealsMissed);
        var total = Assert.Single(run.Totals, row => row.FamilyId == family);
        Assert.Equal(loss, total.NominalMilliunits);
        Assert.Equal(loss, total.StorageOnlyUnproducedMilliunits);
        Assert.Equal(120, total.StorageOnlySeconds);
        Assert.Equal(0, total.InputsOnlyUnproducedMilliunits);
        Assert.Equal(0, total.MixedUnproducedMilliunits);
        Assert.Equal(run.InitialCheckpoint.CityCampaign.Resources[run.TargetResourceId],
            run.FinalCheckpoint.CityCampaign.Resources[run.TargetResourceId]);
        Assert.All(run.Totals.Where(row => row.FamilyId != family), row => Assert.Equal(0, row.NominalMilliunits));
    }

    [Theory]
    [InlineData("V15-R01", 240000)]
    [InlineData("V15-R02", 360000)]
    public void PaidWarehouseRestoresProductionFromTheSameInitialState(string id, long produced)
    {
        var runs = V15IndustrialRecoveryBenchmark.RunScenario(id);
        var baseline = runs[0];
        var recovery = runs[1];
        Assert.Same(baseline.InitialCheckpoint, recovery.InitialCheckpoint);
        Assert.Equal(0, baseline.ProducedTargetMilliunits);
        Assert.Equal(produced, recovery.ProducedTargetMilliunits);
        Assert.Equal(1, recovery.FirstProductionSecond);
        var action = Assert.Single(recovery.Actions);
        Assert.True(action.Succeeded);
        Assert.Equal(100000, action.PaidMilliunits["planks"]);
        Assert.Equal(60000, action.PaidMilliunits["stone"]);
        Assert.Equal(100m, recovery.InitialCheckpoint.CityCampaign.Resources["planks"].Units -
            recovery.FinalCheckpoint.CityCampaign.Resources["planks"].Units);
        Assert.Equal(60m, recovery.InitialCheckpoint.CityCampaign.Resources["stone"].Units -
            recovery.FinalCheckpoint.CityCampaign.Resources["stone"].Units);
        Assert.Equal(produced, recovery.FinalCheckpoint.CityCampaign.Resources[recovery.TargetResourceId].Milliunits -
            recovery.InitialCheckpoint.CityCampaign.Resources[recovery.TargetResourceId].Milliunits);
    }

    [Fact]
    public void RealCoalStaffingRecoversForgeAndMintAfterTheColdStartSecond()
    {
        var runs = V15IndustrialRecoveryBenchmark.RunScenario("V15-R03");
        Assert.Same(runs[0].InitialCheckpoint, runs[1].InitialCheckpoint);
        Assert.Equal(0, runs[0].ProducedTargetMilliunits);
        Assert.Equal(360000, runs[0].Totals.Single(t => t.FamilyId == "mint").InputsOnlyUnproducedMilliunits);
        var recovered = runs[1];
        Assert.Equal(2, recovered.FirstProductionSecond);
        Assert.Equal(357000, recovered.ProducedTargetMilliunits);
        Assert.Equal(238000, recovered.Totals.Single(t => t.FamilyId == "forge").ProducedMilliunits);
        Assert.True(Assert.Single(recovered.Actions).Succeeded);
        Assert.Empty(recovered.Actions[0].PaidMilliunits);
        Assert.Equal(2, V15FoodCampaignCheckpoints.AvailableWorkers(recovered.InitialCheckpoint) -
            V15FoodCampaignCheckpoints.AvailableWorkers(recovered.FinalCheckpoint));
    }

    [Fact]
    public void SimultaneousCoalShortageAndFullOutputAreKeptMixed()
    {
        var setup = V15IndustrialRecoveryBenchmark.CreateFixture("V15-R01");
        var state = Stock(setup.Start, "coal", 0m);
        var plan = Tick(setup, state);
        var sample = Assert.Single(V15IndustrialRecoveryBenchmark.MeasureSecond(1, state, plan, setup.Baseline),
            s => s.FamilyId == "forge");
        Assert.Equal(2000, sample.MixedUnproducedMilliunits);
        Assert.Equal(0, sample.StorageOnlyUnproducedMilliunits);
        Assert.Equal(0, sample.InputsOnlyUnproducedMilliunits);
        Assert.Contains("coal", sample.MissingInputIds);
        Assert.Contains("swords", sample.FullOutputIds);
    }

    [Fact]
    public void PartialWorkerCapacityIsMeasuredWithoutCountingItAsAWholeStoppedSecond()
    {
        var setup = V15IndustrialRecoveryBenchmark.CreateFixture("V15-R01");
        var state = setup.Start with { CityCampaign = setup.Start.CityCampaign with
        {
            ProductiveFamilies = [.. setup.Start.CityCampaign.ProductiveFamilies.Select(f => f.FamilyId == "forge"
                ? f with { Base = new TierOccupancy(2, 2) } : f)],
            PurchaseCounters = [.. setup.Start.CityCampaign.PurchaseCounters.Select(c => c.FamilyId == "forge"
                ? c with { Acquired = 2 } : c)]
        } };
        state = Stock(state, "swords", V15CityStorage.Calculate(setup.Baseline, state.CityCampaign)["swords"].Units - 2m);
        var sample = V15IndustrialRecoveryBenchmark.MeasureSecond(1, state, Tick(setup, state), setup.Baseline)
            .Single(s => s.FamilyId == "forge");
        Assert.Equal(2, sample.AssignedWorkers);
        Assert.Equal(1, sample.RunnableWorkers);
        Assert.Equal(4000, sample.NominalMilliunits);
        Assert.Equal(2000, sample.ProducedMilliunits);
        Assert.Equal(2000, sample.StorageOnlyUnproducedMilliunits);
    }

    [Fact]
    public void EnablingClockDiagnosticsPreservesTheEntireTickAndGrossFlows()
    {
        var setup = V15IndustrialRecoveryBenchmark.CreateFixture("V15-R03");
        var plain = Tick(setup, setup.Start, diagnostics: false);
        var measured = Tick(setup, setup.Start);
        Assert.Null(plain.FirstSecondBuildings);
        Assert.NotNull(measured.FirstSecondBuildings);
        Assert.Equal(plain.Next, measured.Next);
        Assert.Equal(plain.FirstSecondProduction, measured.FirstSecondProduction);
        Assert.Equal(plain.FirstSecondConsumption, measured.FirstSecondConsumption);
        Assert.Equal(plain.MealsServed, measured.MealsServed);
        Assert.True(V15StockAuditor.ReconcileSecond(setup.Start, measured).Balanced);
    }

    [Fact]
    public void MissingDiagnosticsOrMultiSecondPlansCannotBeReportedAsMeasuredZeroLoss()
    {
        var setup = V15IndustrialRecoveryBenchmark.CreateFixture("V15-R01");
        Assert.Throws<ArgumentException>(() => V15IndustrialRecoveryBenchmark.MeasureSecond(1,
            setup.Start, Tick(setup, setup.Start, diagnostics: false), setup.Baseline));
        Assert.Throws<ArgumentException>(() => V15IndustrialRecoveryBenchmark.MeasureSecond(2,
            setup.Start, Tick(setup, setup.Start, seconds: 2), setup.Baseline));
    }

    [Theory]
    [InlineData("V15-R01")]
    [InlineData("V15-R02")]
    [InlineData("V15-R03")]
    public void ZeroSecondDiagnosticsDoNotExecuteRecoveryOrInventLoss(string id)
    {
        foreach (var run in V15IndustrialRecoveryBenchmark.RunScenario(id, 0))
        {
            Assert.True(run.Diagnostic);
            Assert.Equal(120, run.DeclaredHorizonSeconds);
            Assert.Equal(0, run.EffectiveHorizonSeconds);
            Assert.Empty(run.Samples);
            Assert.Empty(run.Actions);
            Assert.Empty(run.Totals);
            Assert.Equal(run.InitialCheckpoint, run.FinalCheckpoint);
            Assert.Null(run.FirstProductionSecond);
        }
    }

    [Fact]
    public void RepeatedExperimentsAreDeterministicAndExportsDeclareProvenanceAndUnits()
    {
        var first = V15IndustrialRecoveryBenchmark.RunAll(4);
        var second = V15IndustrialRecoveryBenchmark.RunAll(4);
        Assert.Equal(6, first.Count);
        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        foreach (var sample in first.SelectMany(r => r.Samples))
            Assert.Equal(sample.NominalMilliunits, sample.ProducedMilliunits +
                sample.StorageOnlyUnproducedMilliunits + sample.InputsOnlyUnproducedMilliunits +
                sample.MixedUnproducedMilliunits + sample.OtherUnproducedMilliunits);
        Assert.Contains("controlled/declared-grants", V15IndustrialRecoveryBenchmark.SamplesCsv(first));
        Assert.Contains("mixed_unproduced_milli", V15IndustrialRecoveryBenchmark.SamplesCsv(first));
        Assert.Contains("paid_milli", V15IndustrialRecoveryBenchmark.ActionsCsv(first));
        Assert.Contains("non partite naturali", V15IndustrialRecoveryBenchmark.ToMarkdown(first));
        Assert.Contains("non scorta distrutta", V15IndustrialRecoveryBenchmark.ToMarkdown(first));
    }

    [Fact]
    public void UnknownScenariosAndHorizonsOutsideTheDeclaredContractAreRejected()
    {
        Assert.Throws<ArgumentException>(() => V15IndustrialRecoveryBenchmark.RunScenario("V15-I01"));
        Assert.Throws<ArgumentOutOfRangeException>(() => V15IndustrialRecoveryBenchmark.RunAll(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => V15IndustrialRecoveryBenchmark.RunAll(121));
    }
}
