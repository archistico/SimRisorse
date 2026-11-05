using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC76: distinguish observations from speculative economic deadlocks.</summary>
public sealed class E4aV15IndustrialConstraintTests
{
    private static V15IndustrialProgressionRun Empty() =>
        V15IndustrialProgressionBenchmark.Run("V15-I01", "research-first", 0);

    private static V15IndustrialProgressionRun WithWait(string failure,
        IReadOnlyDictionary<string, long>? shortage = null)
    {
        var run = Empty();
        return run with
        {
            Events = [new V15IndustrialProgressionEvent(0, "build-coal-mine", "waiting", failure,
                new Dictionary<string, long>(), 0, shortage)],
            Samples = [run.Samples[0] with { IndustrialStocks = null }]
        };
    }

    [Fact]
    public void ZeroDurationReportHasMeasuredCityStocksWithoutGrants()
    {
        var run = Empty();
        var sample = Assert.Single(run.Samples);
        var stock = Assert.IsType<V15IndustrialStockProbe>(sample.IndustrialStocks);
        var city = V15BenchmarkFixture.Create().Start.CityCampaign;
        Assert.Equal(city.Resources["coal"].Milliunits,
            run.InitialPhysicalStockMilliunits.TryGetValue("coal", out var initialCoal) ? initialCoal : 0L);
        Assert.Equal(city.Resources["planks"].Milliunits,
            run.InitialPhysicalStockMilliunits.TryGetValue("planks", out var initialPlanks) ? initialPlanks : 0L);
        Assert.True(stock.PlanksStoredMilliunits <= city.Resources["planks"].Milliunits);
        Assert.True(stock.CoalStoredMilliunits <= city.Resources["coal"].Milliunits);
        Assert.All(stock.Items(), item => Assert.True(item.Stock >= 0 && item.Capacity >= 0));
    }

    [Fact]
    public void MeasuredMaterialDeficitIsPreservedAtFirstObservation()
    {
        var input = WithWait("InsufficientResources", new Dictionary<string, long> { ["coal"] = 1250 });
        var report = V15IndustrialConstraintAnalysis.Analyze([input]);
        var item = Assert.Single(Assert.Single(report.Runs).Observations,
            o => o.Evidence == "measured-shortage");
        Assert.Equal("coal", item.ResourceId);
        Assert.Equal(1250, item.DeficitMilliunits);
        Assert.Equal(0, item.FirstObservedSecond);
        Assert.Equal("construction-materials", item.Constraint);
    }

    [Fact]
    public void OldRecordsWithoutTelemetryRemainUnmeasuredNotZeroDeficit()
    {
        var source = WithWait("InsufficientResources");
        var legacy = source with
        {
            Samples = [source.Samples[0] with { IndustrialStocks = null }]
        };
        var row = Assert.Single(V15IndustrialConstraintAnalysis.Analyze([legacy]).Runs);
        Assert.False(row.HasStockTelemetry);
        var item = Assert.Single(row.Observations);
        Assert.Equal("failure-code-only", item.Evidence);
        Assert.Null(item.DeficitMilliunits);
        Assert.Contains("Nessuna telemetria", V15IndustrialConstraintAnalysis.ToMarkdown(
            V15IndustrialConstraintAnalysis.Analyze([legacy])));
    }

    [Fact]
    public void SaturationIsObservationAndNotStructuralDeadlock()
    {
        var source = Empty();
        var full = new V15IndustrialStockProbe(0, 100, 0, 100, 4000, 4000,
            0, 100, 0, 100, 0, 100, 0, 100);
        var run = source with { Events = [], Samples = [source.Samples[0] with { IndustrialStocks = full }] };
        var row = Assert.Single(V15IndustrialConstraintAnalysis.Analyze([run]).Runs);
        Assert.Equal(1, row.SaturatedStockSamples);
        var cap = Assert.Single(row.Observations);
        Assert.Equal("possible-storage-pressure", cap.Constraint);
        Assert.Equal("capacity-sample", cap.Evidence);
        Assert.Equal("coal", cap.ResourceId);
        Assert.Equal("timeout", row.Outcome);
        Assert.Contains("non prova", row.Conclusion + " " +
            V15IndustrialConstraintAnalysis.ToMarkdown(V15IndustrialConstraintAnalysis.Analyze([run])));
    }

    [Fact]
    public void ZeroCapacityDoesNotClaimSaturation()
    {
        var source = Empty();
        var noStorage = new V15IndustrialStockProbe(0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0);
        var run = source with { Events = [], Samples = [source.Samples[0] with { IndustrialStocks = noStorage }] };
        Assert.Empty(Assert.Single(V15IndustrialConstraintAnalysis.Analyze([run]).Runs).Observations);
    }

    [Fact]
    public void WorkforceFailureIsNotMisclassifiedAsMissingCoal()
    {
        var row = Assert.Single(V15IndustrialConstraintAnalysis.Analyze([
            WithWait("InsufficientPopulation")]).Runs);
        var observation = Assert.Single(row.Observations);
        Assert.Equal("workforce-or-population", observation.Constraint);
        Assert.Null(observation.ResourceId);
    }

    [Fact]
    public void RepeatedDeficitsAreObservationsNotAdditiveCosts()
    {
        var source = WithWait("InsufficientMaterials", new Dictionary<string, long> { ["planks"] = 6000 });
        var wait = source.Events[0];
        var run = source with { Events = [wait, wait with
        {
            MissingResourceMilliunits = new Dictionary<string, long> { ["planks"] = 1500 }
        }] };
        var measured = Assert.Single(Assert.Single(
            V15IndustrialConstraintAnalysis.Analyze([run]).Runs).Observations,
            item => item.Evidence == "measured-shortage");
        Assert.Equal(6000, measured.DeficitMilliunits);
        Assert.Equal(2, measured.Occurrences);
    }

    [Fact]
    public void NegativeDeficitInInputIsRejectedInsteadOfUsedInReport()
    {
        var invalid = WithWait("InsufficientResources", new Dictionary<string, long> { ["stone"] = -1 });
        Assert.Throws<ArgumentException>(() => V15IndustrialConstraintAnalysis.Analyze([invalid]));
    }

    [Fact]
    public void DuplicateScenarioPolicyIsRejected()
    {
        var source = Empty();
        Assert.Throws<ArgumentException>(() => V15IndustrialConstraintAnalysis.Analyze([source, source]));
    }

    [Fact]
    public void CsvAndMarkdownMakeEvidenceExplicit()
    {
        var report = V15IndustrialConstraintAnalysis.Analyze([
            WithWait("InsufficientResources", new Dictionary<string, long> { ["coal"] = 2500 })]);
        Assert.Contains("evidence,constraint,action,resource_id", V15IndustrialConstraintAnalysis.ToCsv(report));
        Assert.Contains("coal", V15IndustrialConstraintAnalysis.ToCsv(report));
        Assert.Contains("mancano 2500 millesimi", V15IndustrialConstraintAnalysis.ToMarkdown(report));
        Assert.Contains("non prova uno stallo", V15IndustrialConstraintAnalysis.ToMarkdown(report));
    }
}
