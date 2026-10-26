using Microsoft.Data.Sqlite;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class SaveStoreTests
{
    [Fact]
    public void GrowthProgressAndFractionalTimeSurviveARealCheckpointAndOnlyOneArrivalFollows()
    {
        using var database = new TestDatabase();
        var scenario = CivilGrowthScenario.Create();
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
        Assert.Equal(expected.Resources, actual.Resources);
        Assert.Equal(expected.Buildings.OrderBy(item => item.BuildingId).ToArray(), actual.Buildings.OrderBy(item => item.BuildingId).ToArray());
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "SimRisorse.M3.Tests", Guid.NewGuid().ToString("N"));
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
