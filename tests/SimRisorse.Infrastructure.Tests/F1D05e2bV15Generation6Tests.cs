using Microsoft.Data.Sqlite;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>RC61: generation 6 must have no persisted historical food-store
/// tier; restoring the empty compatibility slot must never lose real data.</summary>
public sealed class F1D05e2bV15Generation6Tests
{
    private static readonly SchoolResearchRules School = new("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Policy = UnifiedFoodInputs.CreatePolicy([
        new("fish", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 0),
        new("bread", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 1),
        new("meat", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 2),
        new("fruit", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 3),
        new("vegetables", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 4)
    ]);

    private sealed class Sandbox : IDisposable
    {
        public string Folder { get; } = Path.Combine(Path.GetTempPath(), "simrisorse-rc61-" + Guid.NewGuid().ToString("N"));
        public string File => Path.Combine(Folder, "v15-development.sqlite3");
        public Sandbox() => Directory.CreateDirectory(Folder);
        public V15SqliteFoodSaveStore Store() => new(File, CivilGrowthScenario.Create(), School, Technologies, Policy);
        public SqliteConnection Connect()
        {
            var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            { DataSource = File, Pooling = false, ForeignKeys = true }.ToString());
            connection.Open();
            return connection;
        }
        public long Scalar(string sql)
        {
            using var connection = Connect();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            return Convert.ToInt64(cmd.ExecuteScalar());
        }
        public void Execute(string sql)
        {
            using var connection = Connect();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
        public void Dispose() => Directory.Delete(Folder, recursive: true);
    }

    private static V15FoodCampaignCheckpoint Seed()
    {
        var scenario = CivilGrowthScenario.Create();
        var city = V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), School, Technologies);
        return V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, Policy), Policy);
    }

    [Fact]
    public void SqliteGeneration6StoresAndRestoresFifteenActiveFamiliesWithoutLegacySlot()
    {
        using var env = new Sandbox();
        var original = Seed();
        env.Store().Save(original);
        Assert.Equal(6L, env.Scalar("PRAGMA user_version"));
        Assert.Equal(45L, env.Scalar("SELECT COUNT(*) FROM progression_family"));
        Assert.Equal(0L, env.Scalar("SELECT COUNT(*) FROM progression_family WHERE family_id='food-store'"));
        var loaded = env.Store().Load();
        Assert.Equal(15, loaded.CityCampaign.ProductiveFamilies.Length);
        Assert.DoesNotContain(loaded.CityCampaign.ProductiveFamilies,
            f => f.FamilyId == "food-store");
        Assert.Equal(original.CityCampaign.ProductiveFamilies.Where(f => f.FamilyId != "food-store").OrderBy(f => f.FamilyId),
            loaded.CityCampaign.ProductiveFamilies.Where(f => f.FamilyId != "food-store").OrderBy(f => f.FamilyId));
        Assert.Equal(original.CityCampaign.Resources, loaded.CityCampaign.Resources);
        Assert.Equal(original.Depot, loaded.Depot);
        Assert.Equal(original.Industry, loaded.Industry);
    }

    [Fact]
    public void SavingOccupiedHistoricalProducerIsRejectedWithoutCreatingDatabase()
    {
        using var env = new Sandbox();
        var starting = Seed();
        var city = V15LegacyProducerAdapter.ForV14Rules(starting.CityCampaign);
        var occupied = starting with { CityCampaign = city with
        {
            ProductiveFamilies = [.. city.ProductiveFamilies.Select(f => f.FamilyId == "food-store"
                ? f with { Base = new TierOccupancy(1, 1) } : f)],
            PurchaseCounters = [.. city.PurchaseCounters.Select(c => c.FamilyId == "food-store"
                ? c with { Acquired = 1 } : c)]
        }};
        var problem = Assert.Throws<GameSaveException>(() => env.Store().Save(occupied));
        Assert.Equal(GameSaveError.InvalidData, problem.Error);
        Assert.False(File.Exists(env.File));
    }

    [Fact]
    public void ExplicitRetirementPreservesHistoricalCounterAndOtherStocks()
    {
        using var env = new Sandbox();
        var starting = Seed();
        var city = V15LegacyProducerAdapter.ForV14Rules(starting.CityCampaign);
        var occupied = starting with { CityCampaign = city with
        {
            ProductiveFamilies = [.. city.ProductiveFamilies.Select(f => f.FamilyId == "food-store"
                ? f with { Base = new TierOccupancy(1, 1) } : f)],
            PurchaseCounters = [.. city.PurchaseCounters.Select(c => c.FamilyId == "food-store"
                ? c with { Acquired = 1 } : c)]
        }};
        var session = new V15FoodIndustryCampaignSession(occupied,
            CivilGrowthScenario.Create(), School, Technologies, Policy);
        Assert.True(session.RetireLegacyFoodStore().Succeeded);
        env.Store().Save(session.Snapshot());
        var restored = env.Store().Load();
        Assert.Equal(1, restored.CityCampaign.PurchaseCounters.Single(c => c.FamilyId == "food-store").Acquired);
        Assert.Equal(0L, env.Scalar("SELECT COUNT(*) FROM progression_family WHERE family_id='food-store'"));
        Assert.Equal(starting.CityCampaign.Resources, restored.CityCampaign.Resources);
        Assert.Equal(starting.Depot, restored.Depot);
    }

    [Fact]
    public void DatabaseConstraintRejectsReintroducingRetiredProducer()
    {
        using var env = new Sandbox();
        var seed = Seed();
        env.Store().Save(seed);
        Assert.Throws<SqliteException>(() => env.Execute(
            "INSERT INTO progression_family VALUES(1,'food-store',0,0,0)"));
        Assert.Equal(seed.CityCampaign.Resources, env.Store().Load().CityCampaign.Resources);
    }

    [Fact]
    public void PreviousDevelopmentGenerationIsRejectedWithoutReplacement()
    {
        using var env = new Sandbox();
        var seed = Seed();
        env.Store().Save(seed);
        env.Execute("PRAGMA user_version=5");
        var oldBytes = File.ReadAllBytes(env.File);
        Assert.Equal(GameSaveError.UnsupportedFormat,
            Assert.Throws<GameSaveException>(() => env.Store().Load()).Error);
        Assert.Throws<GameSaveException>(() => env.Store().Save(seed));
        Assert.Equal(oldBytes, File.ReadAllBytes(env.File));
    }
}
