using Microsoft.Data.Sqlite;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>RC69: SQLite generation 6 reads native city rows directly.</summary>
public sealed class F1D05e2b2b2dNativeCheckpointPersistenceTests
{
    private static readonly SchoolResearchRules School = new(
        "food", ResourceAmount.FromUnits(2m), 3, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Policy = UnifiedFoodInputs.CreatePolicy([
        new("fish", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 0),
        new("meat", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 1),
        new("vegetables", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 2),
        new("fruit", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 3),
        new("bread", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 4)
    ]);

    private static V15FoodCampaignCheckpoint Seed()
    {
        var baseline = CivilGrowthScenario.Create();
        var inherited = V14CampaignSnapshots.MigrateFromV13(baseline,
            GameSession.Create(baseline).Snapshot(), School, Technologies);
        return V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(inherited, Policy), Policy);
    }

    [Fact]
    public void RoundTripKeepsNativeCityTypesAndEveryProductiveTier()
    {
        var directory = Path.Combine(Path.GetTempPath(), "sr-rc69-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var file = Path.Combine(directory, "test.sqlite3");
            var store = new V15SqliteFoodSaveStore(file, CivilGrowthScenario.Create(),
                School, Technologies, Policy);
            var original = Seed();
            store.Save(original);
            var actual = store.Load();
            Assert.IsType<V15CityCampaignSnapshot>(actual.CityCampaign);
            Assert.Same(actual.CityCampaign, actual.NativeCityCampaign);
            Assert.Equal(original.CityCampaign.Resources, actual.CityCampaign.Resources);
            Assert.Equal(original.CityCampaign.ProductiveFamilies.OrderBy(f => f.FamilyId).ToArray(),
                actual.CityCampaign.ProductiveFamilies.OrderBy(f => f.FamilyId).ToArray());
            Assert.Equal(original.CityCampaign.PurchaseCounters.OrderBy(f => f.FamilyId).ToArray(),
                actual.CityCampaign.PurchaseCounters.OrderBy(f => f.FamilyId).ToArray());
            Assert.Equal(original.Depot, actual.Depot);
            Assert.Equal(original.Industry, actual.Industry);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public void NativeRootDoesNotAlterGeneration6SqliteContract()
    {
        var directory = Path.Combine(Path.GetTempPath(), "sr-rc69-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var file = Path.Combine(directory, "test.sqlite3");
            new V15SqliteFoodSaveStore(file, CivilGrowthScenario.Create(),
                School, Technologies, Policy).Save(Seed());
            using var db = new SqliteConnection(new SqliteConnectionStringBuilder
            { DataSource = file, Pooling = false }.ToString());
            db.Open();
            using var cmd = db.CreateCommand();
            cmd.CommandText = "PRAGMA user_version";
            Assert.Equal(6L, Convert.ToInt64(cmd.ExecuteScalar()));
            cmd.CommandText = "SELECT COUNT(*) FROM progression_family";
            Assert.Equal(45L, Convert.ToInt64(cmd.ExecuteScalar()));
            cmd.CommandText = "SELECT COUNT(*) FROM progression_family WHERE family_id='food-store'";
            Assert.Equal(0L, Convert.ToInt64(cmd.ExecuteScalar()));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
