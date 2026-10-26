using System.Collections.Immutable;
using Microsoft.Data.Sqlite;
using SimRisorse.Application.Persistence;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Infrastructure.Persistence;

public sealed class SqliteGameSaveStore(string path) : IGameSaveStore
{
    public const int ApplicationId = 0x53494D52;
    public const int FileVersion = 1;
    private const int MaximumEntries = 10000;
    public string DatabasePath { get; } = Path.GetFullPath(path);

    public void Save(GameSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        try { SaveCore(snapshot); }
        catch (SqliteException exception) { throw Translate(exception); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        { throw new GameSaveException(GameSaveError.Unavailable, "Cannot write the save file.", exception); }
    }

    public GameSnapshot Load()
    {
        if (!File.Exists(DatabasePath)) throw new GameSaveException(GameSaveError.NotFound, "Save file not found.");
        try { return LoadCore(); }
        catch (SqliteException exception) { throw Translate(exception); }
        catch (Exception exception) when (exception is ArgumentException or OverflowException or InvalidCastException or FormatException)
        { throw new GameSaveException(GameSaveError.InvalidData, "Save contents are invalid.", exception); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        { throw new GameSaveException(GameSaveError.Unavailable, "Cannot read the save file.", exception); }
    }

    private SqliteConnection Open(SqliteOpenMode mode)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = DatabasePath, Mode = mode, Pooling = false, ForeignKeys = true, DefaultTimeout = 3 }.ToString());
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    private static GameSaveException Translate(SqliteException exception) => new(
        exception.SqliteErrorCode is 1 or 11 or 26 ? GameSaveError.InvalidData : GameSaveError.Unavailable,
        "The database operation could not be completed.", exception);

    private static SqliteCommand Command(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        return command;
    }

    private static void CheckFormat(SqliteConnection connection, SqliteTransaction transaction, bool allowEmpty)
    {
        using var id = Command(connection, transaction, "PRAGMA application_id;");
        using var version = Command(connection, transaction, "PRAGMA user_version;");
        var applicationId = Convert.ToInt64(id.ExecuteScalar());
        var fileVersion = Convert.ToInt64(version.ExecuteScalar());
        if (applicationId == ApplicationId && fileVersion == FileVersion) return;
        if (allowEmpty && applicationId == 0 && fileVersion == 0)
        {
            using var tables = Command(connection, transaction, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';");
            if (Convert.ToInt64(tables.ExecuteScalar()) == 0) return;
        }
        throw new GameSaveException(GameSaveError.UnsupportedFormat, "Unrecognized or unsupported save format.");
    }

    private void SaveCore(GameSnapshot snapshot)
    {
        if (snapshot.FormatVersion != GameSession.SnapshotFormatVersion)
            throw new GameSaveException(GameSaveError.UnsupportedFormat, "Snapshot version is not supported.");
        if (snapshot.Resources is null || snapshot.Buildings.IsDefault || snapshot.ElapsedTicks < 0 || snapshot.Population < 0 ||
            string.IsNullOrWhiteSpace(snapshot.ScenarioId) || string.IsNullOrWhiteSpace(snapshot.RulesVersion) ||
            snapshot.Resources.Amounts.Count > MaximumEntries || snapshot.Buildings.Length > MaximumEntries ||
            snapshot.Buildings.Any(item => item is null || string.IsNullOrWhiteSpace(item.BuildingId) || item.Count < 0 || item.AssignedWorkers < 0) ||
            snapshot.Buildings.Select(item => item.BuildingId).Distinct(StringComparer.Ordinal).Count() != snapshot.Buildings.Length)
            throw new GameSaveException(GameSaveError.InvalidData, "Snapshot cannot be written.");
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        using var connection = Open(SqliteOpenMode.ReadWriteCreate);
        using var transaction = connection.BeginTransaction();
        CheckFormat(connection, transaction, allowEmpty: true);
        using (var schema = Command(connection, transaction, $"""
            PRAGMA application_id={ApplicationId};
            PRAGMA user_version={FileVersion};
            CREATE TABLE IF NOT EXISTS game_save (
                slot INTEGER PRIMARY KEY CHECK(slot=1), snapshot_version INTEGER NOT NULL,
                scenario_id TEXT NOT NULL, rules_version TEXT NOT NULL,
                elapsed_ticks INTEGER NOT NULL CHECK(elapsed_ticks>=0), population INTEGER NOT NULL CHECK(population>=0)
            ) STRICT;
            CREATE TABLE IF NOT EXISTS resource_stock (
                slot INTEGER NOT NULL REFERENCES game_save(slot), resource_id TEXT NOT NULL,
                milliunits INTEGER NOT NULL CHECK(milliunits>=0), PRIMARY KEY(slot,resource_id)
            ) STRICT;
            CREATE TABLE IF NOT EXISTS building_stock (
                slot INTEGER NOT NULL REFERENCES game_save(slot), building_id TEXT NOT NULL,
                count INTEGER NOT NULL CHECK(count>=0), workers INTEGER NOT NULL CHECK(workers>=0), PRIMARY KEY(slot,building_id)
            ) STRICT;
            """)) schema.ExecuteNonQuery();
        using (var existing = Command(connection, transaction, "SELECT snapshot_version FROM game_save WHERE slot=1;"))
        {
            var savedVersion = existing.ExecuteScalar();
            if (savedVersion is not null && Convert.ToInt64(savedVersion) != GameSession.SnapshotFormatVersion)
                throw new GameSaveException(GameSaveError.UnsupportedFormat, "Existing snapshot version must not be overwritten.");
        }
        using (var metadata = Command(connection, transaction, """
            INSERT INTO game_save VALUES(1,$version,$scenario,$rules,$time,$population)
            ON CONFLICT(slot) DO UPDATE SET snapshot_version=excluded.snapshot_version,
                scenario_id=excluded.scenario_id, rules_version=excluded.rules_version,
                elapsed_ticks=excluded.elapsed_ticks, population=excluded.population;
            """))
        {
            metadata.Parameters.AddWithValue("$version", snapshot.FormatVersion);
            metadata.Parameters.AddWithValue("$scenario", snapshot.ScenarioId);
            metadata.Parameters.AddWithValue("$rules", snapshot.RulesVersion);
            metadata.Parameters.AddWithValue("$time", snapshot.ElapsedTicks);
            metadata.Parameters.AddWithValue("$population", snapshot.Population);
            metadata.ExecuteNonQuery();
        }
        using (var clear = Command(connection, transaction, "DELETE FROM resource_stock WHERE slot=1; DELETE FROM building_stock WHERE slot=1;")) clear.ExecuteNonQuery();
        foreach (var (id, amount) in snapshot.Resources.Amounts)
        {
            using var insert = Command(connection, transaction, "INSERT INTO resource_stock VALUES(1,$id,$amount);");
            insert.Parameters.AddWithValue("$id", id);
            insert.Parameters.AddWithValue("$amount", amount.Milliunits);
            insert.ExecuteNonQuery();
        }
        foreach (var building in snapshot.Buildings)
        {
            using var insert = Command(connection, transaction, "INSERT INTO building_stock VALUES(1,$id,$count,$workers);");
            insert.Parameters.AddWithValue("$id", building.BuildingId);
            insert.Parameters.AddWithValue("$count", building.Count);
            insert.Parameters.AddWithValue("$workers", building.AssignedWorkers);
            insert.ExecuteNonQuery();
        }
        transaction.Commit();
    }

    private GameSnapshot LoadCore()
    {
        using var connection = Open(SqliteOpenMode.ReadOnly);
        using var transaction = connection.BeginTransaction(deferred: true);
        CheckFormat(connection, transaction, allowEmpty: false);
        int version, population;
        long elapsed;
        string scenario, rules;
        using (var command = Command(connection, transaction, "SELECT snapshot_version,scenario_id,rules_version,elapsed_ticks,population FROM game_save WHERE slot=1;"))
        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read()) throw new GameSaveException(GameSaveError.InvalidData, "The save is incomplete.");
            version = checked((int)reader.GetInt64(0));
            scenario = reader.GetString(1);
            rules = reader.GetString(2);
            elapsed = reader.GetInt64(3);
            population = checked((int)reader.GetInt64(4));
        }
        if (version != GameSession.SnapshotFormatVersion) throw new GameSaveException(GameSaveError.UnsupportedFormat, "Unsupported snapshot version.");
        if (elapsed < 0 || population < 0 || string.IsNullOrWhiteSpace(scenario) || string.IsNullOrWhiteSpace(rules))
            throw new GameSaveException(GameSaveError.InvalidData, "Invalid save metadata.");
        var resources = new List<KeyValuePair<string, ResourceAmount>>();
        using (var command = Command(connection, transaction, $"SELECT resource_id,milliunits FROM resource_stock WHERE slot=1 ORDER BY resource_id LIMIT {MaximumEntries + 1};"))
        using (var reader = command.ExecuteReader())
            while (reader.Read()) resources.Add(KeyValuePair.Create(reader.GetString(0), ResourceAmount.FromMilliunits(reader.GetInt64(1))));
        var buildings = ImmutableArray.CreateBuilder<BuildingState>();
        using (var command = Command(connection, transaction, $"SELECT building_id,count,workers FROM building_stock WHERE slot=1 ORDER BY building_id LIMIT {MaximumEntries + 1};"))
        using (var reader = command.ExecuteReader())
            while (reader.Read()) buildings.Add(new(reader.GetString(0), checked((int)reader.GetInt64(1)), checked((int)reader.GetInt64(2))));
        if (resources.Count > MaximumEntries || buildings.Count == 0 || buildings.Count > MaximumEntries ||
            buildings.Any(item => string.IsNullOrWhiteSpace(item.BuildingId) || item.Count < 0 || item.AssignedWorkers < 0))
            throw new GameSaveException(GameSaveError.InvalidData, "The save exceeds format limits.");
        var snapshot = new GameSnapshot(version, scenario, rules, elapsed, population, ResourceBundle.FromAmounts(resources), buildings.ToImmutable());
        transaction.Commit();
        return snapshot;
    }
}
