using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>RC70: audit the complete generation-6 checkpoint at the save boundary.
/// Validate content, not collection ordering: SQLite sorts keyed data by ID.</summary>
public sealed class F1D05e2b2eV15SqliteAuditTests
{
    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);
    private static readonly SchoolResearchRules School = new("food", U(10m), 120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Policy = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("bread", U(1m), U(1m), 1),
        new("meat", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("vegetables", U(1m), U(1m), 4)
    ]);

    private static V15FoodCampaignCheckpoint Seed()
    {
        var baseline = CivilGrowthScenario.Create();
        var v14 = V14CampaignSnapshots.MigrateFromV13(baseline,
            GameSession.Create(baseline).Snapshot(), School, Technologies);
        return V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(v14, Policy), Policy);
    }

    private static V15FoodCampaignCheckpoint RichCheckpoint(V15FoodCampaignCheckpoint source)
    {
        var city = source.CityCampaign;
        return source with
        {
            CityCampaign = city with
            {
                ElapsedTicks = TimeSpan.FromSeconds(15.25).Ticks,
                GrowthRemainderTwentieths = 7,
                ResearchBalanceMilliPoints = 12345,
                Resources = city.Resources + ResourceBundle.Create(("fish", 0.125m), ("bread", 0.333m)),
                OtherBuildings = [.. city.OtherBuildings.Reverse()],
                ProductiveFamilies = [.. city.ProductiveFamilies.Reverse()],
                PurchaseCounters = [.. city.PurchaseCounters.Reverse()],
                UnlockedTechnologyIds = [.. city.UnlockedTechnologyIds.Reverse()]
            },
            Depot = source.Depot with
            {
                RawFood = ResourceBundle.Create(("fish", 0.678m), ("bread", 0.789m),
                    ("meat", 0.234m), ("fruit", 0.456m), ("vegetables", 0.567m)),
                ReadyFood = U(0.987m)
            },
            Industry = source.Industry with
            {
                Animals = ResourceBundle.Create(("cattle", 0.654m), ("game", 0.321m)),
                Transit = ResourceBundle.Create(("meat", 0.222m),
                    ("fruit", 0.111m), ("vegetables", 0.444m))
            }
        };
    }

    [Fact]
    public void RichNativeStateSurvivesSaveAndReloadWithAllPhysicalOwnersAndFractions()
    {
        var directory = Path.Combine(Path.GetTempPath(), "simrisorse-rc70-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var store = new V15SqliteFoodSaveStore(Path.Combine(directory, "v15.sqlite3"),
                CivilGrowthScenario.Create(), School, Technologies, Policy);
            var expected = RichCheckpoint(Seed());
            store.Save(expected); // invokes the RC70 staged full-equivalence guard
            var actual = store.Load();
            Assert.Equal(expected.CityCampaign.ElapsedTicks, actual.CityCampaign.ElapsedTicks);
            Assert.Equal(expected.CityCampaign.GrowthRemainderTwentieths,
                actual.CityCampaign.GrowthRemainderTwentieths);
            Assert.Equal(expected.CityCampaign.ResearchBalanceMilliPoints,
                actual.CityCampaign.ResearchBalanceMilliPoints);
            Assert.Equal(expected.CityCampaign.Resources, actual.CityCampaign.Resources);
            Assert.Equal(expected.Depot, actual.Depot);
            Assert.Equal(expected.Industry, actual.Industry);
            Assert.Equal(expected.CityCampaign.OtherBuildings.OrderBy(b => b.BuildingId),
                actual.CityCampaign.OtherBuildings.OrderBy(b => b.BuildingId));
            Assert.Equal(expected.CityCampaign.ProductiveFamilies.OrderBy(f => f.FamilyId),
                actual.CityCampaign.ProductiveFamilies.OrderBy(f => f.FamilyId));
            Assert.Equal(expected.CityCampaign.PurchaseCounters.OrderBy(c => c.FamilyId),
                actual.CityCampaign.PurchaseCounters.OrderBy(c => c.FamilyId));
            Assert.Equal(expected.CityCampaign.Military, actual.CityCampaign.Military);
            Assert.Equal(expected.CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray(),
                actual.CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray());
            Assert.DoesNotContain(actual.CityCampaign.ProductiveFamilies,
                f => f.FamilyId == V15LegacyProducerAdapter.RetiredFamilyId);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public void RepeatedSaveRetainsEveryPriorCheckpointBackupAndCurrentState()
    {
        var directory = Path.Combine(Path.GetTempPath(), "simrisorse-rc70-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "v15.sqlite3");
            var store = new V15SqliteFoodSaveStore(path,
                CivilGrowthScenario.Create(), School, Technologies, Policy);
            var first = Seed();
            var second = RichCheckpoint(first);
            var third = second with { Depot = second.Depot with { ReadyFood = U(1.234m) } };
            store.Save(first);
            store.Save(second);
            store.Save(third);
            var backup1 = new V15SqliteFoodSaveStore(Path.Combine(directory, "v15.v15.backup.sqlite3"),
                CivilGrowthScenario.Create(), School, Technologies, Policy).Load();
            var backup2 = new V15SqliteFoodSaveStore(Path.Combine(directory, "v15.v15.backup-2.sqlite3"),
                CivilGrowthScenario.Create(), School, Technologies, Policy).Load();
            Assert.Equal(first.Depot, backup1.Depot);
            Assert.Equal(first.CityCampaign.ElapsedTicks, backup1.CityCampaign.ElapsedTicks);
            Assert.Equal(second.Depot, backup2.Depot);
            Assert.Equal(second.CityCampaign.Resources, backup2.CityCampaign.Resources);
            Assert.Equal(third.Depot, store.Load().Depot);
            Assert.Equal(third.CityCampaign.ElapsedTicks, store.Load().CityCampaign.ElapsedTicks);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
