using Microsoft.Data.Sqlite;
using SimRisorse.Application;
using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Features.Persistence.SaveGame;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class SaveStoreTests
{
    [Fact]
    public async Task GoldUpgradePreservesIronStocksWorkersAndExistingWarehouseAndBacksUpV6()
    {
        using var database = new TestDatabase();
        var oldScenario = CivilGrowthScenario.CreateIronForBenchmarks();
        var initial = GameSession.Create(oldScenario).Snapshot();
        var old = GameSession.Restore(oldScenario, initial with
        {
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m), ("food", 100m), ("bread", 100m)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId is "large-house" or "bakery" or "warehouse"
                ? item with { Count = 1 } : item)]
        });
        foreach (var id in new[] { "iron-mine", "coal-mine", "iron-smelter" }) BuildAndStaff(old, id);
        old.Resume();
        Assert.True(old.Advance(TimeSpan.FromMilliseconds(5500)).Succeeded);
        var original = old.Snapshot();
        Assert.Equal(4m, original.Resources["iron"].Units);
        Assert.NotEqual(0, original.GrowthRemainderTwentieths);
        database.Store.Save(original);
        var bytes = File.ReadAllBytes(database.PathName);
        var runtime = new GameRuntime(CivilGrowthScenario.Create());
        var loaded = await new LoadGameHandler(runtime, database.Store).HandleAsync();
        Assert.True(loaded.Succeeded);
        Assert.True(loaded.View.IsPaused);
        Assert.Equal("civil-growth-6", loaded.UpgradedFromRules);
        Assert.Equal("civil-growth-7", loaded.View.RulesVersion);
        Assert.Equal(original.ElapsedTicks, loaded.View.ElapsedTicks);
        Assert.Equal(original.GrowthProgressSeconds, loaded.View.PopulationState.ProgressSeconds);
        Assert.Equal(original.GrowthRemainderTwentieths, loaded.View.PopulationState.GrowthRemainderTwentieths);
        foreach (var resource in loaded.View.Resources)
            Assert.Equal(original.Resources[resource.Id].Units, resource.Quantity);
        foreach (var building in original.Buildings)
        {
            var actual = loaded.View.Buildings.Single(item => item.Id == building.BuildingId);
            Assert.Equal(building.Count, actual.Count);
            Assert.Equal(building.AssignedWorkers, actual.AssignedWorkers);
        }
        foreach (var id in new[] { "gold", "gold-ore" })
        {
            Assert.Equal(0m, loaded.View.Resources.Single(item => item.Id == id).Quantity);
            Assert.Equal(1500m, loaded.View.Resources.Single(item => item.Id == id).Capacity);
        }
        Assert.Equal(bytes, File.ReadAllBytes(database.PathName));
        Assert.Single(Directory.GetFiles(database.DirectoryName));
        Assert.True((await new SaveGameHandler(runtime, database.Store).HandleAsync()).Succeeded);
        Equal(original, new SqliteGameSaveStore(Path.Combine(database.DirectoryName, "game.civil-growth-6.backup.sqlite3")).Load());
        Assert.Equal(2, database.Store.Load().FormatVersion);
        Assert.Equal("civil-growth-7", database.Store.Load().RulesVersion);
    }

    [Fact]
    public async Task IndustrialUpgradePreservesTheV5CheckpointAndBacksItUpBeforeTheFirstWrite()
    {
        using var database = new TestDatabase();
        var old = GameSession.Create(CivilGrowthScenario.CreateAcceleratedForBenchmarks());
        Assert.True(old.Construct("small-house", 1).Succeeded);
        old.Resume();
        Assert.True(old.Advance(TimeSpan.FromMilliseconds(52500)).Succeeded);
        var original = old.Snapshot();
        Assert.NotEqual(0, original.GrowthRemainderTwentieths);
        database.Store.Save(original);
        var bytes = File.ReadAllBytes(database.PathName);
        var runtime = new GameRuntime(CivilGrowthScenario.Create());
        var result = await new LoadGameHandler(runtime, database.Store).HandleAsync();
        Assert.True(result.Succeeded);
        Assert.True(result.View.IsPaused);
        Assert.Equal("civil-growth-5", result.UpgradedFromRules);
        Assert.Equal(CivilGrowthScenario.RulesVersion, result.View.RulesVersion);
        Assert.Equal(original.ElapsedTicks, result.View.ElapsedTicks);
        Assert.Equal(original.GrowthProgressSeconds, result.View.PopulationState.ProgressSeconds);
        Assert.Equal(original.GrowthRemainderTwentieths, result.View.PopulationState.GrowthRemainderTwentieths);
        foreach (var resource in result.View.Resources)
            Assert.Equal(original.Resources[resource.Id].Units, resource.Quantity);
        foreach (var id in new[] { "iron-mine", "coal-mine", "iron-smelter" })
        {
            Assert.Equal(0, result.View.Buildings.Single(item => item.Id == id).Count);
            Assert.Equal(0, result.View.Buildings.Single(item => item.Id == id).AssignedWorkers);
        }
        Assert.Equal(bytes, File.ReadAllBytes(database.PathName));
        Assert.Single(Directory.GetFiles(database.DirectoryName));
        Assert.True((await new SaveGameHandler(runtime, database.Store).HandleAsync()).Succeeded);
        var backup = Path.Combine(database.DirectoryName, "game.civil-growth-5.backup.sqlite3");
        Equal(original, new SqliteGameSaveStore(backup).Load());
        var upgraded = database.Store.Load();
        Assert.Equal(2, upgraded.FormatVersion);
        Assert.Equal(CivilGrowthScenario.RulesVersion, upgraded.RulesVersion);
        Assert.True((await new SaveGameHandler(runtime, database.Store).HandleAsync()).Succeeded);
        Assert.Equal(2, Directory.GetFiles(database.DirectoryName).Length);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IndustrialStocksWorkersAndFractionalTimeResumeExactlyFromSQLite(bool includeGold)
    {
        using var database = new TestDatabase();
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot();
        var game = GameSession.Restore(scenario, initial with
        {
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m), ("food", 100m), ("bread", 100m)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId is "large-house" or "bakery" ? item with { Count = 1 } : item)]
        });
        foreach (var id in new[] { "iron-mine", "coal-mine", "iron-smelter" }) BuildAndStaff(game, id);
        if (includeGold)
            foreach (var id in new[] { "gold-mine", "gold-smelter" }) BuildAndStaff(game, id);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromMilliseconds(5500)).Succeeded);
        Assert.Equal(4m, game.Resources["iron"].Units);
        if (includeGold) Assert.Equal(4m, game.Resources["gold"].Units);
        database.Store.Save(game.Snapshot());
        var resumed = GameSession.Restore(scenario, database.Store.Load());
        Assert.True(resumed.IsPaused);
        Assert.True(resumed.Advance(TimeSpan.FromHours(1)).Succeeded);
        Equal(game.Snapshot(), resumed.Snapshot());
        resumed.Resume();
        Assert.True(game.Advance(TimeSpan.FromMilliseconds(500)).Succeeded);
        Assert.True(resumed.Advance(TimeSpan.FromMilliseconds(500)).Succeeded);
        Assert.Equal(5m, resumed.Resources["iron"].Units);
        if (includeGold) Assert.Equal(5m, resumed.Resources["gold"].Units);
        Equal(game.Snapshot(), resumed.Snapshot());
        Assert.True(game.Advance(TimeSpan.FromSeconds(60)).Succeeded);
        Assert.True(resumed.Advance(TimeSpan.FromSeconds(60)).Succeeded);
        Equal(game.Snapshot(), resumed.Snapshot());
    }

    [Fact]
    public async Task LegacyGrowthSecondsUpgradeAndFractionalBonusSurviveSQLiteWithBackup()
    {
        using var database = new TestDatabase();
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(CivilGrowthScenario.CreateAcceleratedForBenchmarks()).Snapshot();
        var previous = initial with
        {
            RulesVersion = "civil-growth-4",
            Population = 8,
            ElapsedTicks = 59 * TimeSpan.TicksPerSecond,
            GrowthProgressSeconds = 59,
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId == "small-house" ? item with { Count = 2 } : item)]
        };
        database.Store.Save(previous);
        var runtime = new GameRuntime(scenario);
        var loaded = await new LoadGameHandler(runtime, database.Store).HandleAsync();
        Assert.True(loaded.Succeeded);
        Assert.Equal("civil-growth-4", loaded.UpgradedFromRules);
        Assert.Equal(CivilGrowthScenario.RulesVersion, loaded.View.RulesVersion);
        Assert.Equal(59, loaded.View.PopulationState.ProgressSeconds);
        Assert.Equal(0, loaded.View.PopulationState.GrowthRemainderTwentieths);

        Assert.True((await new SaveGameHandler(runtime, database.Store).HandleAsync()).Succeeded);
        var backup = Path.Combine(database.DirectoryName, "game.civil-growth-4.backup.sqlite3");
        Assert.True(File.Exists(backup));
        Equal(previous, new SqliteGameSaveStore(backup).Load());
        var converted = database.Store.Load();
        Assert.Equal(CivilGrowthScenario.RulesVersion, converted.RulesVersion);
        var game = GameSession.Restore(scenario, converted);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(9, game.Population);
        Assert.Equal(0, game.GrowthProgressSeconds);
        Assert.Equal(3, game.GrowthRemainderTwentieths);
        database.Store.Save(game.Snapshot());
        Equal(game.Snapshot(), database.Store.Load());
    }

    [Fact]
    public void OldV2SqliteWithoutFractionTableStillLoadsAsZeroRemainder()
    {
        using var database = new TestDatabase();
        var scenario = CivilGrowthScenario.CreateLegacyForBenchmarks();
        var original = GameSession.Create(scenario).Snapshot() with { GrowthProgressSeconds = 37 };
        database.Store.Save(original);
        database.Execute("DROP TABLE population_growth_fraction;");
        var read = database.Store.Load();
        Equal(original, read);
        Assert.Equal(0, read.GrowthRemainderTwentieths);
        database.Store.Save(read);
        Equal(original, database.Store.Load());
    }

    [Fact]
    public async Task OldBreadRulesUpgradeOnRealSQLiteWithoutInventingBreadAndKeepTheOriginalFile()
    {
        using var database = new TestDatabase();
        var scenario = CivilGrowthScenario.Create();
        var current = GameSession.Create(CivilGrowthScenario.CreateAcceleratedForBenchmarks()).Snapshot();
        var previous = current with
        {
            RulesVersion = "civil-growth-3",
            Resources = ResourceBundle.Create(("food", 123.456m)),
            Buildings = [.. current.Buildings.Where(item => item.BuildingId != "large-house")
                .Select(item => item.BuildingId is "farm" or "well" or "mill" or "bakery"
                    ? item with { Count = 1, AssignedWorkers = 1 } : item)]
        };
        database.Store.Save(previous);
        var runtime = new GameRuntime(scenario);
        var loaded = await new LoadGameHandler(runtime, database.Store).HandleAsync();
        Assert.True(loaded.Succeeded);
        Assert.Equal("civil-growth-3", loaded.UpgradedFromRules);
        Assert.True(loaded.View.IsPaused);
        Assert.Equal(CivilGrowthScenario.RulesVersion, loaded.View.RulesVersion);
        Assert.Equal(123.456m, loaded.View.Resources.Single(item => item.Id == "food").Quantity);
        Assert.Equal(0m, loaded.View.Resources.Single(item => item.Id == "bread").Quantity);
        Assert.Equal(100m, loaded.View.Resources.Single(item => item.Id == "bread").Capacity);
        Assert.Equal(0, loaded.View.Buildings.Single(item => item.Id == "large-house").Count);
        Assert.Equal(1, loaded.View.Buildings.Single(item => item.Id == "bakery").Count);
        Assert.Equal(1, loaded.View.Buildings.Single(item => item.Id == "bakery").AssignedWorkers);

        Assert.True((await new SaveGameHandler(runtime, database.Store).HandleAsync()).Succeeded);
        var backup = Path.Combine(database.DirectoryName, "game.civil-growth-3.backup.sqlite3");
        Assert.True(File.Exists(backup));
        Equal(previous, new SqliteGameSaveStore(backup).Load());
        var updated = database.Store.Load();
        Assert.Equal(CivilGrowthScenario.RulesVersion, updated.RulesVersion);
        Assert.Equal(2, updated.FormatVersion);
        Assert.Equal(123.456m, updated.Resources["food"].Units);
        Assert.Equal(0m, updated.Resources["bread"].Units);
    }

    [Fact]
    public void GrowthProgressAndFractionalTimeSurviveARealCheckpointAndOnlyOneArrivalFollows()
    {
        using var database = new TestDatabase();
        var scenario = CivilGrowthScenario.CreateLegacyForBenchmarks();
        var game = GameSession.Create(scenario);
        game.Construct("small-house", 1);
        game.Resume(); game.Advance(TimeSpan.FromMilliseconds(59500));
        database.Store.Save(game.Snapshot());
        var saved = database.Store.Load();
        Equal(game.Snapshot(), saved);
        Assert.Equal(2, saved.FormatVersion);
        Assert.Equal(59, saved.GrowthProgressSeconds);
        var restored = GameSession.Restore(scenario, saved);
        restored.Advance(TimeSpan.FromDays(1));
        Equal(saved, restored.Snapshot());
        restored.Resume(); restored.Advance(TimeSpan.FromMilliseconds(500));
        Assert.Equal(9, restored.Population);
        Assert.Equal(0, restored.GrowthProgressSeconds);
        Assert.Equal(25.2m, restored.Resources["food"].Units);
    }

    [Fact]
    public void AdvancedBreadColonyResumesFromSqliteAtTheSameFractionalSecond()
    {
        using var database = new TestDatabase();
        var scenario = CivilGrowthScenario.CreateLegacyForBenchmarks();
        var original = GameSession.Create(scenario);
        foreach (var id in new[] { "fisher", "food-store" }) BuildAndStaff(original, id);
        original.Resume();
        Assert.True(original.Advance(TimeSpan.FromSeconds(61)).Succeeded);
        Assert.True(original.Construct("small-house", 1).Succeeded);
        Assert.True(original.Advance(TimeSpan.FromSeconds(539)).Succeeded);
        foreach (var id in new[] { "farm", "farm", "well", "mill", "bakery" }) BuildAndStaff(original, id);
        Assert.True(original.Advance(TimeSpan.FromSeconds(160)).Succeeded);
        Assert.True(original.Construct("large-house", 1).Succeeded);
        Assert.True(original.Advance(TimeSpan.FromMilliseconds(119500)).Succeeded);
        Assert.Equal(17, original.Population);
        Assert.Equal(59, original.GrowthProgressSeconds);
        Assert.True(original.Resources["bread"].Units > 0m);

        database.Store.Save(original.Snapshot());
        var saved = database.Store.Load();
        Equal(original.Snapshot(), saved);
        Assert.Equal(2, saved.FormatVersion);
        Assert.Equal("civil-growth-4", saved.RulesVersion);
        Assert.Equal(1, saved.Buildings.Single(item => item.BuildingId == "large-house").Count);
        Assert.Equal(1, saved.Buildings.Single(item => item.BuildingId == "bakery").AssignedWorkers);
        var resumed = GameSession.Restore(scenario, saved);
        Assert.True(resumed.IsPaused);
        Assert.True(resumed.Advance(TimeSpan.FromHours(2)).Succeeded);
        Equal(saved, resumed.Snapshot());

        resumed.Resume();
        Assert.True(original.Advance(TimeSpan.FromMilliseconds(500)).Succeeded);
        Assert.True(resumed.Advance(TimeSpan.FromMilliseconds(500)).Succeeded);
        Equal(original.Snapshot(), resumed.Snapshot());
        Assert.Equal(18, resumed.Population);

        Assert.True(original.Advance(TimeSpan.FromMinutes(60)).Succeeded);
        Assert.True(resumed.Advance(TimeSpan.FromSeconds(3600)).Succeeded);
        Equal(original.Snapshot(), resumed.Snapshot());
    }

    [Fact]
    public void FailedV2SaveRollsBackGrowthPopulationAndStocksTogether()
    {
        using var database = new TestDatabase();
        var game = GameSession.Create(CivilGrowthScenario.Create());
        game.Construct("small-house", 1);
        game.Resume(); game.Advance(TimeSpan.FromSeconds(59));
        var before = game.Snapshot();
        database.Store.Save(before);
        database.Execute("CREATE TRIGGER fail_building BEFORE INSERT ON building_stock BEGIN SELECT RAISE(ABORT,'injected failure'); END;");
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Throws<GameSaveException>(() => database.Store.Save(game.Snapshot()));
        Equal(before, database.Store.Load());
    }

    [Theory]
    [InlineData("DELETE FROM population_state;")]
    [InlineData("PRAGMA ignore_check_constraints=ON; UPDATE population_state SET growth_seconds=-1;")]
    [InlineData("UPDATE population_state SET growth_seconds=2147483648;")]
    [InlineData("DROP TABLE population_state;")]
    public void IncompleteOrInvalidGrowthDataDoesNotBecomeZeroProgress(string change)
    {
        using var database = new TestDatabase();
        database.Store.Save(GameSession.Create(CivilGrowthScenario.Create()).Snapshot());
        database.Execute(change);
        Assert.Equal(GameSaveError.InvalidData, Assert.Throws<GameSaveException>(() => database.Store.Load()).Error);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DifferentSaveGenerationsCannotOverwriteEachOther(bool growthFirst)
    {
        using var database = new TestDatabase();
        var oldState = GameSession.Create(EconomyFoundationScenario.Create()).Snapshot();
        var newState = GameSession.Create(CivilGrowthScenario.Create()).Snapshot();
        database.Store.Save(growthFirst ? newState : oldState);
        var bytes = File.ReadAllBytes(database.PathName);
        Assert.Equal(GameSaveError.UnsupportedFormat,
            Assert.Throws<GameSaveException>(() => database.Store.Save(growthFirst ? oldState : newState)).Error);
        Assert.Equal(bytes, File.ReadAllBytes(database.PathName));
        Assert.Single(Directory.GetFiles(database.DirectoryName));
    }

    [Fact]
    public void ReplacingACheckpointWrittenWithOtherRulesKeepsACopyOfTheOriginal()
    {
        using var database = new TestDatabase();
        var scenario = CivilGrowthScenario.Create();
        var game = GameSession.Create(scenario);
        game.Construct("small-house", 1);
        game.Resume(); game.Advance(TimeSpan.FromSeconds(59));
        var current = game.Snapshot();
        var previous = current with
        {
            RulesVersion = "civil-growth-1",
            GrowthRemainderTwentieths = 0,
            Buildings = [.. current.Buildings.Where(item => !CivilGrowthScenario.Changes.Any(change => change.AddedBuildings.Contains(item.BuildingId)))]
        };
        database.Store.Save(previous);
        database.Store.Save(previous);
        Assert.Single(Directory.GetFiles(database.DirectoryName));

        database.Store.Save(current);
        var backup = Path.Combine(database.DirectoryName, "game.civil-growth-1.backup.sqlite3");
        Assert.Equal(2, Directory.GetFiles(database.DirectoryName).Length);
        Equal(previous, new SqliteGameSaveStore(backup).Load());
        Equal(current, database.Store.Load());
        database.Store.Save(current);
        Assert.Equal(2, Directory.GetFiles(database.DirectoryName).Length);

        database.Store.Save(previous with { RulesVersion = "../other rules" });
        Assert.True(File.Exists(Path.Combine(database.DirectoryName, $"game.{CivilGrowthScenario.RulesVersion}.backup.sqlite3")));
        database.Store.Save(current);
        Assert.True(File.Exists(Path.Combine(database.DirectoryName, "game.___other_rules.backup.sqlite3")));
        database.Store.Save(previous);
        Assert.True(File.Exists(Path.Combine(database.DirectoryName, $"game.{CivilGrowthScenario.RulesVersion}.backup-2.sqlite3")));
        Equal(previous, new SqliteGameSaveStore(backup).Load());
    }

    [Fact]
    public void M3CheckpointRemainsCompatibleAfterUpdatingTheDesktopAndSQLite()
    {
        var scenario = EconomyFoundationScenario.Create();
        var expected = GameSession.Create(scenario);
        expected.Construct("tree-feller", 1);
        expected.AssignWorkers("tree-feller", 1);
        expected.Resume();
        expected.Advance(TimeSpan.FromSeconds(1));
        expected.Pause();
        var fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "economy-foundation-v1.sqlite3");
        var original = File.ReadAllBytes(fixture);
        var snapshot = new SqliteGameSaveStore(fixture).Load();
        Equal(expected.Snapshot(), snapshot);
        var restored = GameSession.Restore(scenario, snapshot);
        Assert.True(restored.IsPaused);
        Equal(expected.Snapshot(), restored.Snapshot());
        Assert.Equal(original, File.ReadAllBytes(fixture));
        using var database = new TestDatabase();
        database.Store.Save(restored.Snapshot());
        Equal(snapshot, database.Store.Load());
    }

    [Fact]
    public void PersistenceHasNoDesktopOrAvaloniaDependency() => Assert.DoesNotContain(
        typeof(SqliteGameSaveStore).Assembly.GetReferencedAssemblies(),
        reference => reference.Name!.Contains("Avalonia", StringComparison.Ordinal) || reference.Name.Contains("Desktop", StringComparison.Ordinal));

    [Fact]
    public void ExactQuantitiesTimeAndAssignmentsSurviveARealDatabaseRoundTrip()
    {
        using var database = new TestDatabase();
        var scenario = EconomyFoundationScenario.Create();
        var game = GameSession.Create(scenario);
        game.Construct("tree-feller", 1);
        game.AssignWorkers("tree-feller", 1);
        game.Resume();
        game.Advance(TimeSpan.FromMilliseconds(1750));
        var expected = game.Snapshot();
        database.Store.Save(expected);
        var actual = database.Store.Load();
        Equal(expected, actual);
        var restored = GameSession.Restore(scenario, actual);
        restored.Advance(TimeSpan.FromHours(8));
        Assert.True(restored.IsPaused);
        Assert.Equal(expected.ElapsedTicks, restored.ElapsedTicks);
    }

    [Fact]
    public void FailureAfterMetadataAndResourcesRollsBackTheEntireSave()
    {
        using var database = new TestDatabase();
        var game = GameSession.Create(EconomyFoundationScenario.Create());
        var before = game.Snapshot();
        database.Store.Save(before);
        database.Execute("CREATE TRIGGER fail_building BEFORE INSERT ON building_stock BEGIN SELECT RAISE(ABORT,'injected failure'); END;");
        game.Construct("tree-feller", 1);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(10));
        var error = Assert.Throws<GameSaveException>(() => database.Store.Save(game.Snapshot()));
        Assert.IsType<SqliteException>(error.InnerException);
        Equal(before, database.Store.Load());
    }

    [Theory]
    [InlineData("PRAGMA user_version=3;")]
    [InlineData("UPDATE game_save SET snapshot_version=3;")]
    public void FutureVersionsAreNotLoadedOrOverwritten(string change)
    {
        using var database = new TestDatabase();
        var state = GameSession.Create(EconomyFoundationScenario.Create()).Snapshot();
        database.Store.Save(state);
        database.Execute(change);
        var bytes = File.ReadAllBytes(database.PathName);
        Assert.Equal(GameSaveError.UnsupportedFormat, Assert.Throws<GameSaveException>(() => database.Store.Load()).Error);
        Assert.Equal(GameSaveError.UnsupportedFormat, Assert.Throws<GameSaveException>(() => database.Store.Save(state)).Error);
        Assert.Equal(bytes, File.ReadAllBytes(database.PathName));
    }

    [Fact]
    public void UnrelatedDatabasesAreNeverOverwritten()
    {
        using var database = new TestDatabase();
        database.Execute("CREATE TABLE unrelated(value TEXT); INSERT INTO unrelated VALUES('keep me');");
        var bytes = File.ReadAllBytes(database.PathName);
        Assert.Equal(GameSaveError.UnsupportedFormat,
            Assert.Throws<GameSaveException>(() => database.Store.Save(GameSession.Create(EconomyFoundationScenario.Create()).Snapshot())).Error);
        Assert.Equal(bytes, File.ReadAllBytes(database.PathName));
    }

    [Fact]
    public void MissingFilesAreNotCreatedByLoading()
    {
        using var database = new TestDatabase();
        Assert.Equal(GameSaveError.NotFound, Assert.Throws<GameSaveException>(() => database.Store.Load()).Error);
        Assert.False(File.Exists(database.PathName));
    }

    [Theory]
    [InlineData("PRAGMA ignore_check_constraints=ON; UPDATE resource_stock SET milliunits=-1;")]
    [InlineData("PRAGMA ignore_check_constraints=ON; UPDATE building_stock SET workers=-1;")]
    [InlineData("PRAGMA ignore_check_constraints=ON; UPDATE game_save SET elapsed_ticks=-1;")]
    [InlineData("DELETE FROM building_stock;")]
    public void InvalidDatabaseContentsAreReportedWithoutDriverExceptions(string change)
    {
        using var database = new TestDatabase();
        database.Store.Save(GameSession.Create(EconomyFoundationScenario.Create()).Snapshot());
        database.Execute(change);
        Assert.Equal(GameSaveError.InvalidData, Assert.Throws<GameSaveException>(() => database.Store.Load()).Error);
    }

    [Fact]
    public void FractionalResourcesAreStoredAsIntegerMilliunits()
    {
        using var database = new TestDatabase();
        var before = GameSession.Create(EconomyFoundationScenario.Create()).Snapshot() with
        { Resources = SimRisorse.Core.Economy.ResourceBundle.Create(("logs", 1.234m)) };
        database.Store.Save(before);
        Assert.Equal(1234, database.Store.Load().Resources["logs"].Milliunits);
    }

    private static void Equal(GameSnapshot expected, GameSnapshot actual)
    {
        Assert.Equal(expected.ScenarioId, actual.ScenarioId);
        Assert.Equal(expected.RulesVersion, actual.RulesVersion);
        Assert.Equal(expected.FormatVersion, actual.FormatVersion);
        Assert.Equal(expected.ElapsedTicks, actual.ElapsedTicks);
        Assert.Equal(expected.Population, actual.Population);
        Assert.Equal(expected.GrowthProgressSeconds, actual.GrowthProgressSeconds);
        Assert.Equal(expected.GrowthRemainderTwentieths, actual.GrowthRemainderTwentieths);
        Assert.Equal(expected.Resources, actual.Resources);
        Assert.Equal(expected.Buildings.OrderBy(item => item.BuildingId).ToArray(), actual.Buildings.OrderBy(item => item.BuildingId).ToArray());
    }

    private static void BuildAndStaff(GameSession game, string id)
    {
        Assert.True(game.Construct(id, 1).Succeeded);
        Assert.True(game.AssignWorkers(id, 1).Succeeded);
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "SimRisorse.M3.Tests", Guid.NewGuid().ToString("N"));
        public string DirectoryName => _directory;
        public string PathName => Path.Combine(_directory, "game.sqlite3");
        public SqliteGameSaveStore Store => new(PathName);
        public void Execute(string sql)
        {
            Directory.CreateDirectory(_directory);
            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = PathName, Pooling = false }.ToString());
            connection.Open();
            using var command = connection.CreateCommand(); command.CommandText = sql; command.ExecuteNonQuery();
        }
        public void Dispose() { if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true); }
    }
}
