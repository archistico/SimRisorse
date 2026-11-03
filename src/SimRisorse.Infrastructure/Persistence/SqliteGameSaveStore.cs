using System.Collections.Immutable;
using Microsoft.Data.Sqlite;
using SimRisorse.Application.Persistence;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Infrastructure.Persistence;

public sealed class SqliteGameSaveStore(string path) : IGameSaveStore
{
    public const int ApplicationId = 0x53494D52;
    public const int FileVersion = 2;
    private const int MaximumEntries = 10000;
    // Military state remains mandatory across all military-capable rulesets.
    // Unknown and pre-military versions must never acquire an invented military state.
    private static bool RequiresMilitaryState(string rules) => rules is "civil-growth-10" or "civil-growth-11" or "civil-growth-12" or "civil-growth-13";
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

    /// <summary>0 absent; 1 original four-column schema; 2 incompatible v41 two-column schema.</summary>
    private static int MilitarySchema(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var query = Command(connection, transaction, "PRAGMA table_info(military_state);");
        var names = new HashSet<string>(StringComparer.Ordinal);
        using var reader = query.ExecuteReader();
        while (reader.Read()) names.Add(reader.GetString(1));
        if (names.Count == 0) return 0;
        if (names.SetEquals(new[] { "slot", "waiting", "training", "soldiers", "progress_seconds" })) return 1;
        if (names.SetEquals(new[] { "slot", "queued_recruits", "soldiers" })) return 2;
        throw new GameSaveException(GameSaveError.InvalidData, "Unrecognized military state columns.");
    }

    private static ImmutableArray<int> ReadIndividualProgress(SqliteConnection connection,
        SqliteTransaction transaction, int? expectedCount)
    {
        using var check = Command(connection, transaction,
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='military_training';");
        if (Convert.ToInt64(check.ExecuteScalar()) != 1)
            throw new GameSaveException(GameSaveError.InvalidData, "Individual training table is missing.");
        if (expectedCount is < 0 or > MaximumEntries)
            throw new GameSaveException(GameSaveError.InvalidData, "Training count exceeds format limits.");
        var progress = ImmutableArray.CreateBuilder<int>();
        using var query = Command(connection, transaction,
            $"SELECT position,progress_seconds FROM military_training WHERE slot=1 ORDER BY position LIMIT {MaximumEntries + 1};");
        using var reader = query.ExecuteReader();
        while (reader.Read())
        {
            if (progress.Count >= MaximumEntries || (expectedCount.HasValue && progress.Count >= expectedCount.Value) ||
                reader.GetInt64(0) != progress.Count)
                throw new GameSaveException(GameSaveError.InvalidData, "Training positions are missing or duplicated.");
            var value = reader.GetInt64(1);
            if (value < 0 || value >= MilitaryTrainingRules.IndividualBarracks.TrainingSecondsPerGroup)
                throw new GameSaveException(GameSaveError.InvalidData, "Individual training progress is invalid.");
            progress.Add(checked((int)value));
        }
        if (expectedCount.HasValue && progress.Count != expectedCount.Value)
            throw new GameSaveException(GameSaveError.InvalidData, "Training positions count differs from military state.");
        return progress.ToImmutable();
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
            (snapshot.FormatVersion == 1 && snapshot.Military is not null) ||
            (RequiresMilitaryState(snapshot.RulesVersion) != (snapshot.Military is not null)) ||
            (snapshot.Military?.HasIndividualProgress == true && snapshot.RulesVersion != "civil-growth-13") ||
            (snapshot.RulesVersion == "civil-growth-13" && snapshot.Military?.HasIndividualProgress != true) ||
            (snapshot.Military is { } militaryCandidate && (militaryCandidate.WaitingRecruits < 0 ||
                militaryCandidate.TrainingRecruits < 0 || militaryCandidate.AvailableSoldiers < 0 ||
                militaryCandidate.TrainingProgressSeconds < 0 || militaryCandidate.EnlistedPopulation > int.MaxValue)) ||
            snapshot.Buildings.Any(item => item is null || string.IsNullOrWhiteSpace(item.BuildingId) || item.Count < 0 || item.AssignedWorkers < 0) ||
            snapshot.Buildings.Select(item => item.BuildingId).Distinct(StringComparer.Ordinal).Count() != snapshot.Buildings.Length)
            throw new GameSaveException(GameSaveError.InvalidData, "Snapshot cannot be written.");
        if (snapshot.Military is { } militaryState)
        {
            var barracks = snapshot.Buildings.SingleOrDefault(item => item.BuildingId == MilitaryTrainingRules.SimpleBarracks.BarracksId);
            if (barracks is null || snapshot.Buildings.Sum(item => (long)item.AssignedWorkers) +
                militaryState.EnlistedPopulation > snapshot.Population)
                throw new GameSaveException(GameSaveError.InvalidData, "Military population or barracks is invalid.");
            try { MilitaryTraining.Validate(militaryState,
                snapshot.RulesVersion == "civil-growth-13" ? MilitaryTrainingRules.IndividualBarracks : MilitaryTrainingRules.SimpleBarracks,
                barracks.Count); }
            catch (ArgumentException exception)
            { throw new GameSaveException(GameSaveError.InvalidData, "Military state cannot be written.", exception); }
        }
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
            if (snapshot.Military is { } military)
            {
                // The 03/11 branch used the same military_state name with a different column layout.
                // The backup of replaced rules was made before this transaction; change schema atomically.
                if (MilitarySchema(connection, transaction) == 2)
                {
                    using var drop = Command(connection, transaction,
                        "DROP TABLE IF EXISTS military_training; DROP TABLE military_state;");
                    drop.ExecuteNonQuery();
                }
                using var militaryCommand = Command(connection, transaction, """
                    CREATE TABLE IF NOT EXISTS military_state (
                        slot INTEGER PRIMARY KEY CHECK(slot=1) REFERENCES game_save(slot),
                        waiting INTEGER NOT NULL CHECK(waiting>=0),
                        training INTEGER NOT NULL CHECK(training>=0),
                        soldiers INTEGER NOT NULL CHECK(soldiers>=0),
                        progress_seconds INTEGER NOT NULL CHECK(progress_seconds>=0)
                    ) STRICT;
                    INSERT INTO military_state VALUES(1,$waiting,$training,$soldiers,$progress)
                    ON CONFLICT(slot) DO UPDATE SET waiting=excluded.waiting, training=excluded.training,
                        soldiers=excluded.soldiers, progress_seconds=excluded.progress_seconds;
                    """);
                militaryCommand.Parameters.AddWithValue("$waiting", military.WaitingRecruits);
                militaryCommand.Parameters.AddWithValue("$training", military.TrainingRecruits);
                militaryCommand.Parameters.AddWithValue("$soldiers", military.AvailableSoldiers);
                militaryCommand.Parameters.AddWithValue("$progress", military.TrainingProgressSeconds);
                militaryCommand.ExecuteNonQuery();
                // Per-person training is a companion to the original four-column schema, not a schema overwrite.
                if (snapshot.RulesVersion == "civil-growth-13")
                {
                    using (var schema = Command(connection, transaction, """
                        CREATE TABLE IF NOT EXISTS military_training (
                            slot INTEGER NOT NULL REFERENCES game_save(slot),
                            position INTEGER NOT NULL CHECK(position>=0),
                            progress_seconds INTEGER NOT NULL CHECK(progress_seconds>=0),
                            PRIMARY KEY(slot,position)
                        ) STRICT;
                        DELETE FROM military_training WHERE slot=1;
                        """)) schema.ExecuteNonQuery();
                    for (var i = 0; i < military.IndividualProgressSeconds.Length; i++)
                    {
                        using var insert = Command(connection, transaction,
                            "INSERT INTO military_training VALUES(1,$position,$progress);");
                        insert.Parameters.AddWithValue("$position", i);
                        insert.Parameters.AddWithValue("$progress", military.IndividualProgressSeconds[i]);
                        insert.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using var hasMilitary = Command(connection, transaction,
                    "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='military_state';");
                if (Convert.ToInt64(hasMilitary.ExecuteScalar()) > 0)
                {
                    using var clearMilitary = Command(connection, transaction, "DELETE FROM military_state WHERE slot=1;");
                    clearMilitary.ExecuteNonQuery();
                }
            }
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
        MilitaryTrainingState? militaryState = null;
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
            var militarySchema = MilitarySchema(connection, transaction);
            if (militarySchema == 1)
            {
                using var military = Command(connection, transaction,
                    "SELECT waiting,training,soldiers,progress_seconds FROM military_state WHERE slot=1;");
                using var reader = military.ExecuteReader();
                if (reader.Read())
                {
                    var waiting = checked((int)reader.GetInt64(0));
                    var training = checked((int)reader.GetInt64(1));
                    var soldiers = checked((int)reader.GetInt64(2));
                    var trainingProgressSeconds = checked((int)reader.GetInt64(3));
                    militaryState = new(waiting, training, soldiers, trainingProgressSeconds);
                }
                reader.Close();
                if (rules == "civil-growth-13" && militaryState is { } individual)
                {
                    var individualProgressSeconds = ReadIndividualProgress(connection, transaction, individual.TrainingRecruits);
                    militaryState = individual with { IndividualProgressSeconds = individualProgressSeconds };
                }
            }
            else if (militarySchema == 2)
            {
                // Only the known v41 fork may use this alternate schema under the ambiguous v10 identifier.
                if (rules != "civil-growth-10")
                    throw new GameSaveException(GameSaveError.InvalidData, "Alternate military schema belongs to the v41 fork only.");
                using var military = Command(connection, transaction,
                    "SELECT queued_recruits,soldiers FROM military_state WHERE slot=1;");
                using var reader = military.ExecuteReader();
                if (reader.Read())
                {
                    var queued = checked((int)reader.GetInt64(0));
                    var soldiers = checked((int)reader.GetInt64(1));
                    reader.Close();
                    var forkProgressSeconds = ReadIndividualProgress(connection, transaction, null);
                    // The alternate branch has no explicit training count; read positions until exhausted.
                    militaryState = new MilitaryTrainingState(queued, forkProgressSeconds.Length, soldiers,
                        forkProgressSeconds.Length == 0 ? 0 : forkProgressSeconds[0], forkProgressSeconds);
                }
            }
        }
        if (RequiresMilitaryState(rules) != (militaryState is not null) ||
            (rules == "civil-growth-13" && militaryState?.HasIndividualProgress != true) ||
            (rules is "civil-growth-11" or "civil-growth-12" && militaryState?.HasIndividualProgress == true))
            throw new GameSaveException(GameSaveError.InvalidData, "Military state is missing or incompatible with rules.");
        if (militaryState is not null)
        {
            var barracks = buildings.SingleOrDefault(item => item.BuildingId == MilitaryTrainingRules.SimpleBarracks.BarracksId);
            if (barracks is null || buildings.Sum(item => (long)item.AssignedWorkers) +
                militaryState.EnlistedPopulation > population)
                throw new GameSaveException(GameSaveError.InvalidData, "Military population or barracks is invalid.");
            try { MilitaryTraining.Validate(militaryState,
                militaryState.HasIndividualProgress ? MilitaryTrainingRules.IndividualBarracks : MilitaryTrainingRules.SimpleBarracks,
                barracks.Count); }
            catch (ArgumentException exception)
            { throw new GameSaveException(GameSaveError.InvalidData, "Military state is invalid.", exception); }
        }
        var snapshot = new GameSnapshot(version, scenario, rules, elapsed, population,
            ResourceBundle.FromAmounts(resources), buildings.ToImmutable(), progress, remainder, militaryState);
        transaction.Commit();
        return snapshot;
    }
}
