using System.Collections.Immutable;
using Microsoft.Data.Sqlite;
using SimRisorse.Application.Persistence;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Infrastructure.Persistence;

public sealed class SqliteGameSaveStore(string path) : IGameSaveStore
{
    public const int ApplicationId = 0x53494D52;
    public const int FileVersion = 2;
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

    private static int CheckFormat(SqliteConnection connection, SqliteTransaction transaction, bool allowEmpty)
    {
        using var id = Command(connection, transaction, "PRAGMA application_id;");
        using var version = Command(connection, transaction, "PRAGMA user_version;");
        var applicationId = Convert.ToInt64(id.ExecuteScalar());
        var fileVersion = Convert.ToInt64(version.ExecuteScalar());
        if (applicationId == ApplicationId && fileVersion is 1 or FileVersion) return (int)fileVersion;
        if (allowEmpty && applicationId == 0 && fileVersion == 0)
        {
            using var tables = Command(connection, transaction, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';");
            if (Convert.ToInt64(tables.ExecuteScalar()) == 0) return 0;
        }
        throw new GameSaveException(GameSaveError.UnsupportedFormat, "Unrecognized or unsupported save format.");
    }

    private void SaveCore(GameSnapshot snapshot)
    {
        if (!GameSession.SupportsSnapshotFormat(snapshot.FormatVersion))
            throw new GameSaveException(GameSaveError.UnsupportedFormat, "Snapshot version is not supported.");
        if (snapshot.Resources is null || snapshot.Buildings.IsDefault || snapshot.ElapsedTicks < 0 || snapshot.Population < 0 ||
            string.IsNullOrWhiteSpace(snapshot.ScenarioId) || string.IsNullOrWhiteSpace(snapshot.RulesVersion) ||
            snapshot.Resources.Amounts.Count > MaximumEntries || snapshot.Buildings.Length > MaximumEntries ||
            (snapshot.FormatVersion == 1 ? snapshot.GrowthProgressSeconds is not null : snapshot.GrowthProgressSeconds is not >= 0) ||
            snapshot.GrowthRemainderTwentieths is < 0 or >= HousingGrowthRules.UnitsPerSecond ||
            (snapshot.FormatVersion == 1 && snapshot.GrowthRemainderTwentieths != 0) ||
            snapshot.Buildings.Any(item => item is null || string.IsNullOrWhiteSpace(item.BuildingId) || item.Count < 0 || item.AssignedWorkers < 0) ||
            snapshot.Buildings.Select(item => item.BuildingId).Distinct(StringComparer.Ordinal).Count() != snapshot.Buildings.Length)
            throw new GameSaveException(GameSaveError.InvalidData, "Snapshot cannot be written.");
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        using var connection = Open(SqliteOpenMode.ReadWriteCreate);
        PreserveReplacedRules(connection, snapshot);
        using var transaction = connection.BeginTransaction();
        var fileVersion = CheckFormat(connection, transaction, allowEmpty: true);
        if (fileVersion != 0 && fileVersion != snapshot.FormatVersion)
            throw new GameSaveException(GameSaveError.UnsupportedFormat, "Different save generations use separate slots.");
        using (var schema = Command(connection, transaction, $"""
            PRAGMA application_id={ApplicationId};
            PRAGMA user_version={snapshot.FormatVersion};
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
            if (savedVersion is not null && Convert.ToInt64(savedVersion) != snapshot.FormatVersion)
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
        if (snapshot.FormatVersion == 2)
        {
            using var growth = Command(connection, transaction, """
                CREATE TABLE IF NOT EXISTS population_state (
                    slot INTEGER PRIMARY KEY CHECK(slot=1) REFERENCES game_save(slot),
                    growth_seconds INTEGER NOT NULL CHECK(growth_seconds>=0)
                ) STRICT;
                INSERT INTO population_state VALUES(1,$progress)
                ON CONFLICT(slot) DO UPDATE SET growth_seconds=excluded.growth_seconds;
                """);
            growth.Parameters.AddWithValue("$progress", snapshot.GrowthProgressSeconds!.Value);
            growth.ExecuteNonQuery();
            // Optional companion table keeps the original growth_seconds column and v2 file format compatible.
            // Older v2 checkpoints have no companion table and therefore implicitly have a zero remainder.
            using var fraction = Command(connection, transaction, """
                CREATE TABLE IF NOT EXISTS population_growth_fraction (
                    slot INTEGER PRIMARY KEY CHECK(slot=1) REFERENCES game_save(slot),
                    twentieths INTEGER NOT NULL CHECK(twentieths BETWEEN 0 AND 19)
                ) STRICT;
                INSERT INTO population_growth_fraction VALUES(1,$fraction)
                ON CONFLICT(slot) DO UPDATE SET twentieths=excluded.twentieths;
                """);
            fraction.Parameters.AddWithValue("$fraction", snapshot.GrowthRemainderTwentieths);
            fraction.ExecuteNonQuery();
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

    /// <summary>
    /// Copies the current checkpoint next to the save before it is replaced by a different scenario or rules version.
    /// Existing copies are never overwritten; the write is refused when no free name is available.
    /// </summary>
    private void PreserveReplacedRules(SqliteConnection connection, GameSnapshot snapshot)
    {
        string? rules = null;
        using (var read = connection.BeginTransaction(deferred: true))
        {
            // Other file generations are refused by the write below, so they need no copy.
            if (CheckFormat(connection, read, allowEmpty: true) != snapshot.FormatVersion) return;
            using var table = Command(connection, read, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='game_save';");
            if (Convert.ToInt64(table.ExecuteScalar()) == 0) return;
            using var command = Command(connection, read, "SELECT scenario_id,rules_version FROM game_save WHERE slot=1;");
            using var reader = command.ExecuteReader();
            if (reader.Read() && (reader.GetString(0) != snapshot.ScenarioId || reader.GetString(1) != snapshot.RulesVersion))
                rules = reader.GetString(1);
        }
        if (rules is null) return;
        var directory = Path.GetDirectoryName(DatabasePath)!;
        var name = $"{Path.GetFileNameWithoutExtension(DatabasePath)}.{SafeFileSegment(rules)}.backup";
        var extension = Path.GetExtension(DatabasePath);
        for (var attempt = 1; attempt <= 100; attempt++)
        {
            var target = Path.Combine(directory, attempt == 1 ? name + extension : $"{name}-{attempt}{extension}");
            if (File.Exists(target)) continue;
            using var vacuum = connection.CreateCommand();
            vacuum.CommandText = "VACUUM INTO $target;";
            vacuum.Parameters.AddWithValue("$target", target);
            vacuum.ExecuteNonQuery();
            return;
        }
        throw new GameSaveException(GameSaveError.Unavailable, "No free name for the backup of the replaced checkpoint.");
    }

    private static string SafeFileSegment(string value)
    {
        var safe = new string(value.Select(item => char.IsAsciiLetterOrDigit(item) || item is '-' or '_' ? item : '_').Take(64).ToArray());
        return safe.Length == 0 ? "unknown" : safe;
    }

    private GameSnapshot LoadCore()
    {
        using var connection = Open(SqliteOpenMode.ReadOnly);
        using var transaction = connection.BeginTransaction(deferred: true);
        var fileVersion = CheckFormat(connection, transaction, allowEmpty: false);
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
        if (!GameSession.SupportsSnapshotFormat(version)) throw new GameSaveException(GameSaveError.UnsupportedFormat, "Unsupported snapshot version.");
        if (version != fileVersion) throw new GameSaveException(GameSaveError.InvalidData, "Snapshot and file versions differ.");
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
        int? progress = null;
        var remainder = 0;
        if (version == 2)
        {
            using var command = Command(connection, transaction, "SELECT growth_seconds FROM population_state WHERE slot=1;");
            var value = command.ExecuteScalar();
            if (value is not long number || number < 0 || number > int.MaxValue)
                throw new GameSaveException(GameSaveError.InvalidData, "Growth state is missing or invalid.");
            progress = (int)number;
            using var hasFraction = Command(connection, transaction,
                "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='population_growth_fraction';");
            if (Convert.ToInt64(hasFraction.ExecuteScalar()) != 0)
            {
                using var fraction = Command(connection, transaction,
                    "SELECT twentieths FROM population_growth_fraction WHERE slot=1;");
                var fractionValue = fraction.ExecuteScalar();
                if (fractionValue is not long units || units is < 0 or >= HousingGrowthRules.UnitsPerSecond)
                    throw new GameSaveException(GameSaveError.InvalidData, "Growth fraction is missing or invalid.");
                remainder = (int)units;
            }
        }
        var snapshot = new GameSnapshot(version, scenario, rules, elapsed, population,
            ResourceBundle.FromAmounts(resources), buildings.ToImmutable(), progress, remainder);
        transaction.Commit();
        return snapshot;
    }
}
