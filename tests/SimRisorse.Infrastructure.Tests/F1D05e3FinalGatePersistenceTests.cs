using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Food;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>RC72: final combined-chain, fractional-time SQLite restart gate.</summary>
public sealed class F1D05e3FinalGatePersistenceTests
{
    [Fact]
    public void CombinedCheckpointAcrossThreeSqliteRestartsMatchesUninterruptedSimulation()
    {
        var scenario = V15CombinedFoodStressScenario.Create();
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-rc72-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var store = new V15SqliteFoodSaveStore(Path.Combine(folder, "checkpoint.sqlite3"),
                scenario.Baseline, scenario.School, scenario.Technologies, scenario.Depot);
            var reference = scenario.Plan(scenario.Initial, TimeSpan.FromMilliseconds(120125)).Next;
            var current = scenario.Initial;
            foreach (var interval in new[] { TimeSpan.FromMilliseconds(125),
                TimeSpan.FromSeconds(37), TimeSpan.FromSeconds(83) })
            {
                current = scenario.Plan(current, interval).Next;
                store.Save(current);
                current = store.Load();
                Assert.DoesNotContain(current.CityCampaign.ProductiveFamilies,
                    family => family.FamilyId == V15LegacyProducerAdapter.RetiredFamilyId);
            }
            Assert.Equal(reference.CityCampaign.ElapsedTicks, current.CityCampaign.ElapsedTicks);
            Assert.Equal(reference.CityCampaign.Resources, current.CityCampaign.Resources);
            Assert.Equal(reference.CityCampaign.Population, current.CityCampaign.Population);
            Assert.Equal(reference.CityCampaign.GrowthProgressSeconds,
                current.CityCampaign.GrowthProgressSeconds);
            Assert.Equal(reference.CityCampaign.GrowthRemainderTwentieths,
                current.CityCampaign.GrowthRemainderTwentieths);
            Assert.Equal(reference.CityCampaign.ResearchBalanceMilliPoints,
                current.CityCampaign.ResearchBalanceMilliPoints);
            Assert.Equal(reference.CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray(),
                current.CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray());
            Assert.Equal(reference.CityCampaign.Military, current.CityCampaign.Military);
            Assert.Equal(reference.Industry, current.Industry);
            Assert.Equal(reference.Depot, current.Depot);
            Assert.Equal(V15StockAuditor.PhysicalTotals(reference),
                V15StockAuditor.PhysicalTotals(current));
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}
