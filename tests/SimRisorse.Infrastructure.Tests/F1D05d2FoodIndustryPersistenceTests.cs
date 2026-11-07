using Microsoft.Data.Sqlite;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>RC49 / F1-D05d.2: one complete checkpoint for all food-industry state.</summary>
public sealed class F1D05d2FoodIndustryPersistenceTests
{
    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);
    private static readonly SchoolResearchRules School = new("food", U(10m), 120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy DepotPolicy = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("bread", U(1m), U(1m), 1),
        new("meat", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("vegetables", U(1m), U(1m), 4)
    ]);

    private sealed class TempStore : IDisposable
    {
        public string Folder { get; } = Path.Combine(Path.GetTempPath(),
            "simrisorse-food-rc62-" + Guid.NewGuid().ToString("N"));
        public string FilePath => Path.Combine(Folder, "generation6.sqlite3");
        public TempStore() => Directory.CreateDirectory(Folder);
        public V15SqliteFoodSaveStore Store() => new(FilePath, CivilGrowthScenario.Create(),
            School, Technologies, DepotPolicy);
        public void Mutate(string sql)
        {
            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            { DataSource = FilePath, Pooling = false, ForeignKeys = true }.ToString());
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        public long Scalar(string sql)
        {
            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            { DataSource = FilePath, Pooling = false, ForeignKeys = true }.ToString());
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            return Convert.ToInt64(command.ExecuteScalar());
        }
        public void Dispose() => Directory.Delete(Folder, recursive: true);
    }

    private static FoodIndustryCrew Crew(long stable = 0, long hunter = 0, long butcher = 0,
        long garden = 0, long orchard = 0, long depot = 0) =>
        new(stable, hunter, butcher, garden, orchard, depot);

    private static V15FoodCampaignCheckpoint Seed()
    {
        var scenario = CivilGrowthScenario.Create();
        var legacy = V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), School, Technologies);
        var staged = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(legacy, DepotPolicy), DepotPolicy);
        return staged with
        {
            Industry = new FoodIndustryCampaignState(
                ResourceBundle.Create(("cattle", 1.125m), ("game", 2.555m)),
                ResourceBundle.Create(("meat", 3.333m), ("fruit", 4.444m), ("vegetables", 5.555m)),
                new FoodIndustryBuildings(Crew(2, 1, 1, 3, 2, 1), Crew()))
        };
    }

    [Fact]
    public void SchemaSixRoundTripPreservesEveryIntermediateMilliunitAndSixBuildingCounts()
    {
        using var env = new TempStore();
        var checkpoint = Seed();
        env.Store().Save(checkpoint);
        var restored = env.Store().Load();
        Assert.Equal(6L, env.Scalar("PRAGMA user_version"));
        Assert.Equal(6L, env.Scalar("SELECT snapshot_version FROM game_save"));
        Assert.Equal(8L, env.Scalar("SELECT COUNT(*) FROM food_industry_site"));
        Assert.Equal(checkpoint.Industry, restored.Industry);
        Assert.Equal(checkpoint.CityCampaign.Resources, restored.CityCampaign.Resources);
        Assert.Equal(checkpoint.Depot, restored.Depot);
        Assert.Equal(checkpoint.CityCampaign.ElapsedTicks, restored.CityCampaign.ElapsedTicks);
        Assert.Equal(checkpoint.CityCampaign.Population, restored.CityCampaign.Population);
    }

    [Fact]
    public void TwoConsecutiveSavesKeepIndustryAndVerifiedPriorBackup()
    {
        using var env = new TempStore();
        var first = Seed();
        env.Store().Save(first);
        var second = first with { Industry = first.Industry with
        {
            Transit = first.Industry.Transit + ResourceBundle.Create(("meat", 0.001m))
        } };
        env.Store().Save(second);
        Assert.Equal(second.Industry, env.Store().Load().Industry);
        var backup = Path.Combine(env.Folder, "generation6.v15.backup.sqlite3");
        Assert.True(File.Exists(backup));
        var backupStore = new V15SqliteFoodSaveStore(backup, CivilGrowthScenario.Create(),
            School, Technologies, DepotPolicy);
        Assert.Equal(first.Industry, backupStore.Load().Industry);
    }

    [Fact]
    public void MissingBuildingRowRejectsWholeCheckpoint()
    {
        using var env = new TempStore();
        env.Store().Save(Seed());
        env.Mutate("DELETE FROM food_industry_site WHERE site_id='orchard'");
        Assert.Equal(GameSaveError.InvalidData,
            Assert.Throws<GameSaveException>(() => env.Store().Load()).Error);
    }

    [Fact]
    public void ExtraUnknownBuildingRowRejectsWholeCheckpoint()
    {
        using var env = new TempStore();
        env.Store().Save(Seed());
        env.Mutate("INSERT INTO food_industry_site VALUES(1,'unknown',1,0)");
        Assert.Equal(GameSaveError.InvalidData,
            Assert.Throws<GameSaveException>(() => env.Store().Load()).Error);
    }

    [Fact]
    public void PersistedWorkersCannotExceedOwnedBuildings()
    {
        using var env = new TempStore();
        env.Store().Save(Seed());
        env.Mutate("UPDATE food_industry_site SET workers=3 WHERE site_id='hunter'");
        Assert.Equal(GameSaveError.InvalidData,
            Assert.Throws<GameSaveException>(() => env.Store().Load()).Error);
    }

    [Fact]
    public void CombinedFoodAndLegacyWorkersCannotExceedWholePopulation()
    {
        var seed = Seed();
        var invalid = seed with { Industry = seed.Industry with
        {
            Buildings = new FoodIndustryBuildings(Crew(100, 100, 100, 100, 100, 100),
                Crew(100, 100, 100, 100, 100, 100))
        } };
        Assert.Throws<ArgumentException>(() => V15FoodCampaignCheckpoints.Validate(invalid,
            CivilGrowthScenario.Create(), School, Technologies, DepotPolicy));
    }

    [Fact]
    public void AnimalsCannotBeSavedInTransitOrWithUndeclaredIdentifiers()
    {
        var seed = Seed();
        var invalid = seed with { Industry = seed.Industry with
        {
            Animals = ResourceBundle.Create(("cattle", 1m), ("fruit", 1m))
        } };
        Assert.Throws<ArgumentException>(() => V15FoodCampaignCheckpoints.Validate(invalid,
            CivilGrowthScenario.Create(), School, Technologies, DepotPolicy));
        using var env = new TempStore();
        env.Store().Save(seed);
        env.Mutate("INSERT INTO food_animals_stock VALUES(1,'fruit',1000)");
        Assert.Equal(GameSaveError.InvalidData,
            Assert.Throws<GameSaveException>(() => env.Store().Load()).Error);
    }

    [Fact]
    public void OldGenerationFourIsIntentionallyRejectedAndNeverOverwritten()
    {
        using var env = new TempStore();
        var seed = Seed();
        env.Store().Save(seed);
        env.Mutate("PRAGMA user_version=4");
        var before = File.ReadAllBytes(env.FilePath);
        Assert.Equal(GameSaveError.UnsupportedFormat,
            Assert.Throws<GameSaveException>(() => env.Store().Load()).Error);
        Assert.Throws<GameSaveException>(() => env.Store().Save(seed));
        Assert.Equal(before, File.ReadAllBytes(env.FilePath));
    }

    [Fact]
    public void MissingIntermediateTableFailsClosed()
    {
        using var env = new TempStore();
        env.Store().Save(Seed());
        env.Mutate("DROP TABLE food_transit_stock");
        Assert.Equal(GameSaveError.InvalidData,
            Assert.Throws<GameSaveException>(() => env.Store().Load()).Error);
    }

    [Fact]
    public void UnknownTransitIdentifiersAreRejectedOnLoad()
    {
        using var env = new TempStore();
        env.Store().Save(Seed());
        env.Mutate("INSERT INTO food_transit_stock VALUES(1,'coins',1000)");
        Assert.Equal(GameSaveError.InvalidData,
            Assert.Throws<GameSaveException>(() => env.Store().Load()).Error);
    }

    [Fact]
    public void LegacyReverseProjectionRejectsActiveFoodIndustries()
    {
        Assert.Throws<InvalidOperationException>(() => V15FoodCampaignCheckpoints.ToLegacyPreview(
            Seed(), CivilGrowthScenario.Create(), School, Technologies, DepotPolicy));
    }

    [Fact]
    public void SavedAnimalsCanBeButcheredAfterResumeWithoutLosingTheUnprocessedSurplus()
    {
        using var env = new TempStore();
        var seed = Seed() with
        {
            Industry = new FoodIndustryCampaignState(ResourceBundle.Create(("game", 1m)),
                ResourceBundle.Create(("meat", 2.125m)),
                new FoodIndustryBuildings(Crew(hunter: 1, butcher: 1, depot: 1),
                    Crew(hunter: 1, butcher: 1, depot: 1)))
        };
        var meat = new MeatChainPolicy("grain", "water", "cattle", "game", "meat",
            ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(1m), U(1m),
            [new("cattle", U(1m), U(1m), 0), new("game", U(1m), U(1m), 1)]);
        var crops = new HorticulturePolicy("water", "vegetables", "fruit",
            new("vegetables", U(1m), U(0m), 0), new("fruit", U(1m), U(0m), 1));
        var cap = new FoodIndustryCapacities(
            ResourceBundle.Create(("cattle", 10m), ("game", 10m)),
            ResourceBundle.Create(("meat", 10m), ("fruit", 10m), ("vegetables", 10m)),
            ResourceBundle.Create(("fish", 10m), ("bread", 10m), ("meat", 1m),
                ("fruit", 10m), ("vegetables", 10m)), U(100m));
        var meals = new FoodIndustryMeals(U(0m), U(0m), U(0m));
        V15FoodCampaignTick Step(V15FoodCampaignCheckpoint checkpoint) =>
            V15FoodCampaignTicks.Plan(checkpoint, CivilGrowthScenario.Create(),
                School, Technologies, meat, crops, DepotPolicy, cap, meals);
        var first = Step(seed);
        // Depot can receive only one unit of meat and must leave the surplus in transit.
        Assert.True(first.Next.Industry.Transit["meat"].Milliunits > 0);
        env.Store().Save(first.Next);
        var loaded = env.Store().Load();
        Assert.Equal(first.Next.Industry, loaded.Industry);
        var expected = Step(first.Next).Next;
        var resumed = Step(loaded).Next;
        Assert.Equal(expected.Industry, resumed.Industry);
        Assert.Equal(expected.Depot, resumed.Depot);
        Assert.Equal(expected.CityCampaign.Resources, resumed.CityCampaign.Resources);
    }
}
