using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>RC59: exercise real SQLite restarts with every v15 food industry staffed.</summary>
public sealed class F1D05e2CombinedFoodPersistenceTests
{
    [Fact]
    public void MixedProductionSurvivesRestartWithExactlyTheSamePhysicalStocks()
    {
        var fixture = V15CombinedFoodStressScenario.Create();
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-rc59-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var store = new V15SqliteFoodSaveStore(Path.Combine(folder, "combined.sqlite3"),
                fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot);
            var expected = fixture.Plan(fixture.Initial, TimeSpan.FromSeconds(40)).Next;
            store.Save(expected);
            var restored = store.Load();
            Assert.Equal(V15StockAuditor.PhysicalTotals(expected),
                V15StockAuditor.PhysicalTotals(restored));
            Assert.Equal(expected.Industry, restored.Industry);
            Assert.Equal(expected.Depot, restored.Depot);
            Assert.Equal(expected.CityCampaign.Military, restored.CityCampaign.Military);
            Assert.Equal(expected.CityCampaign.ElapsedTicks, restored.CityCampaign.ElapsedTicks);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public void MixedProductionAcrossMultipleSQLiteReopeningsMatchesContinuousTime()
    {
        var fixture = V15CombinedFoodStressScenario.Create();
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-rc59-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var store = new V15SqliteFoodSaveStore(Path.Combine(folder, "combined.sqlite3"),
                fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot);
            var expected = fixture.Plan(fixture.Initial, TimeSpan.FromSeconds(60.75)).Next;
            var current = fixture.Initial;
            foreach (var interval in new[] { TimeSpan.FromSeconds(20.25),
                TimeSpan.FromSeconds(10.5), TimeSpan.FromSeconds(30) })
            {
                current = fixture.Plan(current, interval).Next;
                store.Save(current);
                current = store.Load();
            }
            Assert.Equal(V15StockAuditor.PhysicalTotals(expected),
                V15StockAuditor.PhysicalTotals(current));
            Assert.Equal(expected.CityCampaign.ElapsedTicks, current.CityCampaign.ElapsedTicks);
            Assert.Equal(expected.CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray(),
                current.CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray());
            Assert.Equal(expected.Depot, current.Depot);
            Assert.Equal(expected.Industry, current.Industry);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}
