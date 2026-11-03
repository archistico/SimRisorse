using System.Collections.Immutable;
using Microsoft.Data.Sqlite;
using SimRisorse.Application.Persistence;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Infrastructure.Persistence;

/// <summary>
/// E3b/2 only: a separate, copy-on-write SQLite v3 checkpoint store. It is deliberately
/// NOT an IGameSaveStore and cannot be used by the live v13 GameSession.
/// A v13 file must never be the destination of a v14 Save or migration.
/// </summary>
public sealed class V14SqliteSaveStore
{
    public const int FileVersion = V14CampaignSnapshots.FormatVersion;
    private const int ApplicationId = SqliteGameSaveStore.ApplicationId;
    private const int Limit = 10000;
    private readonly GameScenario _baseline;
    private readonly SchoolResearchRules _school;
    private readonly TechnologyCatalog _technologies;

    public V14SqliteSaveStore(string path, GameScenario baselineV13,
        SchoolResearchRules schoolRules, TechnologyCatalog technologies)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _baseline = baselineV13 ?? throw new ArgumentNullException(nameof(baselineV13));
        _school = schoolRules ?? throw new ArgumentNullException(nameof(schoolRules));
        _technologies = technologies ?? throw new ArgumentNullException(nameof(technologies));
        if (_baseline.Id != "civil-growth" || _baseline.Catalog.RulesVersion != "civil-growth-13")
            throw new ArgumentException("A known v13 scenario is required.", nameof(baselineV13));
        DatabasePath = Path.GetFullPath(path);
    }

    public string DatabasePath { get; }

    /// <summary>Pure v13 read and conversion; the source is never modified.</summary>
    public V14CampaignSnapshot PreviewMigration(string legacyPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(legacyPath);
        if (SamePath(legacyPath, DatabasePath))
            throw new GameSaveException(GameSaveError.UnsupportedFormat, "Use a different file for the v14 checkpoint.");
        var original = new SqliteGameSaveStore(legacyPath).Load();
        try { return V14CampaignSnapshots.MigrateFromV13(_baseline, original, _school, _technologies); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or OverflowException)
        { throw Invalid("The legacy checkpoint cannot be migrated.", ex); }
    }

    /// <summary>Converts and saves to a DISTINCT v3 file, never overwriting the v13 source.</summary>
    public V14CampaignSnapshot SaveMigratedCopy(string legacyPath)
    {
        var converted = PreviewMigration(legacyPath);
        Save(converted);
        return converted;
    }

    public void Save(V14CampaignSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        try { V14CampaignSnapshots.Validate(_baseline, snapshot, _school, _technologies); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or OverflowException)
        { throw Invalid("The v14 checkpoint is not valid.", ex); }
        if (snapshot.Resources.Amounts.Count > Limit || snapshot.OtherBuildings.Length > Limit ||
            snapshot.ProductiveFamilies.Length > Limit || snapshot.PurchaseCounters.Length > Limit ||
            snapshot.SchoolResearch.TrainingStudents > Limit ||
            snapshot.Military.IndividualProgressSeconds.Length > Limit || snapshot.UnlockedTechnologyIds.Length > Limit)
            throw Invalid("The checkpoint exceeds SQLite format limits.");

        string? temporary = null;
        try
        {
            var directory = Path.GetDirectoryName(DatabasePath)!;
            Directory.CreateDirectory(directory);
            var existed = File.Exists(DatabasePath);
            // Refuse unknown/legacy/corrupt files BEFORE creating backups or changing any bytes.
            if (existed) _ = Load();
            temporary = Path.Combine(directory, $".{Path.GetFileName(DatabasePath)}.{Guid.NewGuid():N}.tmp");
            WriteNewFile(temporary, snapshot);
            _ = LoadFrom(temporary); // Validate the on-disk representation before promotion.
            if (existed)
            {
                BackupExisting(); // S1: consistent SQLite copy, never overwritten.
                File.Move(temporary, DatabasePath, overwrite: true);
            }
            else File.Move(temporary, DatabasePath);
            temporary = null;
        }
        catch (SqliteException ex) { throw Invalid("SQLite could not write the v14 checkpoint.", ex); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { throw new GameSaveException(GameSaveError.Unavailable, "The checkpoint could not be written.", ex); }
        finally
        {
            if (temporary is not null)
            {
                try { File.Delete(temporary); }
                catch (IOException) { /* Do not replace the primary failure. */ }
                catch (UnauthorizedAccessException) { /* Do not replace the primary failure. */ }
            }
        }
    }

    public V14CampaignSnapshot Load()
    {
        if (!File.Exists(DatabasePath))
            throw new GameSaveException(GameSaveError.NotFound, "The v14 checkpoint does not exist.");
        return LoadFrom(DatabasePath);
    }

    private V14CampaignSnapshot LoadFrom(string file)
    {
        try { return ReadFile(file); }
        catch (SqliteException ex) { throw Invalid("SQLite v3 checkpoint data are corrupt.", ex); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or InvalidCastException or FormatException or OverflowException or IndexOutOfRangeException)
        { throw Invalid("The v14 checkpoint is incomplete or inconsistent.", ex); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { throw new GameSaveException(GameSaveError.Unavailable, "Cannot read the checkpoint.", ex); }
    }

    private static GameSaveException Invalid(string message, Exception? inner = null) =>
        new(GameSaveError.InvalidData, message, inner);

    private static bool SamePath(string a, string b) => string.Equals(Path.GetFullPath(a), Path.GetFullPath(b),
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    private static SqliteConnection Open(string path, SqliteOpenMode mode)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = path, Mode = mode, Pooling = false, ForeignKeys = true, DefaultTimeout = 3 }.ToString());
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    private static SqliteCommand Sql(SqliteConnection connection, SqliteTransaction? transaction, string sql)
    {
        var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = sql;
        return cmd;
    }

    private static void CheckFile(SqliteConnection connection)
    {
        using var command = Sql(connection, null, "PRAGMA application_id;");
        using var version = Sql(connection, null, "PRAGMA user_version;");
        if (Convert.ToInt64(command.ExecuteScalar()) != ApplicationId ||
            Convert.ToInt64(version.ExecuteScalar()) != FileVersion)
            throw new GameSaveException(GameSaveError.UnsupportedFormat, "This is not a SimRisorse SQLite v3 save.");
        // E3b2/RC3 hardens the unpublished v3 draft. Without the explicit training
        // count, deleting the last progress row would silently remove a student.
        // Do not guess missing progress or reinterpret older experimental files.
        using var columns = Sql(connection, null, "PRAGMA table_info(progression_school_state);");
        using var reader = columns.ExecuteReader();
        var hasTrainingStudents = false;
        while (reader.Read())
            if (reader.GetString(1) == "training_students") hasTrainingStudents = true;
        if (!hasTrainingStudents)
            throw new GameSaveException(GameSaveError.UnsupportedFormat,
                "This experimental SQLite v3 checkpoint predates the training-count integrity field. " +
                "Create a new v3 copy from the unchanged v13 save.");
    }

    private static void Insert(SqliteConnection connection, SqliteTransaction tx, string sql, params object[] values)
    {
        using var command = Sql(connection, tx, sql);
        for (var i = 0; i < values.Length; i++) command.Parameters.AddWithValue($"$p{i}", values[i]);
        command.ExecuteNonQuery();
    }

    private static void WriteNewFile(string file, V14CampaignSnapshot s)
    {
        using var connection = Open(file, SqliteOpenMode.ReadWriteCreate);
        using var tx = connection.BeginTransaction();
        using (var schema = Sql(connection, tx, $"""
            PRAGMA application_id={ApplicationId};
            PRAGMA user_version={FileVersion};
            CREATE TABLE game_save (
                slot INTEGER PRIMARY KEY CHECK(slot=1), snapshot_version INTEGER NOT NULL CHECK(snapshot_version=3),
                scenario_id TEXT NOT NULL, rules_version TEXT NOT NULL,
                elapsed_ticks INTEGER NOT NULL CHECK(elapsed_ticks>=0), population INTEGER NOT NULL CHECK(population>=0)
            ) STRICT;
            CREATE TABLE resource_stock (
                slot INTEGER NOT NULL REFERENCES game_save(slot) CHECK(slot=1), resource_id TEXT NOT NULL,
                milliunits INTEGER NOT NULL CHECK(milliunits>=0), PRIMARY KEY(slot,resource_id)
            ) STRICT;
            CREATE TABLE building_stock (
                slot INTEGER NOT NULL REFERENCES game_save(slot) CHECK(slot=1), building_id TEXT NOT NULL,
                count INTEGER NOT NULL CHECK(count>=0), workers INTEGER NOT NULL CHECK(workers>=0), PRIMARY KEY(slot,building_id)
            ) STRICT;
            CREATE TABLE population_state (
                slot INTEGER PRIMARY KEY REFERENCES game_save(slot) CHECK(slot=1),
                growth_seconds INTEGER NOT NULL CHECK(growth_seconds>=0),
                twentieths INTEGER NOT NULL CHECK(twentieths BETWEEN 0 AND 19)
            ) STRICT;
            CREATE TABLE military_state (
                slot INTEGER PRIMARY KEY REFERENCES game_save(slot) CHECK(slot=1),
                waiting INTEGER NOT NULL CHECK(waiting>=0), training INTEGER NOT NULL CHECK(training>=0),
                soldiers INTEGER NOT NULL CHECK(soldiers>=0), progress_seconds INTEGER NOT NULL CHECK(progress_seconds>=0)
            ) STRICT;
            CREATE TABLE military_training (
                slot INTEGER NOT NULL REFERENCES game_save(slot) CHECK(slot=1), position INTEGER NOT NULL CHECK(position>=0),
                progress_seconds INTEGER NOT NULL CHECK(progress_seconds>=0), PRIMARY KEY(slot,position)
            ) STRICT;
            CREATE TABLE progression_school_state (
                slot INTEGER PRIMARY KEY REFERENCES game_save(slot) CHECK(slot=1),
                school_count INTEGER NOT NULL CHECK(school_count>=0),
                waiting_students INTEGER NOT NULL CHECK(waiting_students>=0),
                training_students INTEGER NOT NULL CHECK(training_students>=0),
                qualified_researchers INTEGER NOT NULL CHECK(qualified_researchers>=0),
                active_researchers INTEGER NOT NULL CHECK(active_researchers>=0),
                research_millipoints INTEGER NOT NULL CHECK(research_millipoints>=0),
                pencil_paid_students INTEGER NOT NULL DEFAULT 0 CHECK(pencil_paid_students>=0)
            ) STRICT;
            CREATE TABLE progression_training (
                slot INTEGER NOT NULL REFERENCES game_save(slot) CHECK(slot=1), position INTEGER NOT NULL CHECK(position>=0),
                progress_seconds INTEGER NOT NULL CHECK(progress_seconds>=0), PRIMARY KEY(slot,position)
            ) STRICT;
            CREATE TABLE progression_family (
                slot INTEGER NOT NULL REFERENCES game_save(slot) CHECK(slot=1), family_id TEXT NOT NULL,
                tier INTEGER NOT NULL CHECK(tier IN(0,1,2)), count INTEGER NOT NULL CHECK(count>=0),
                workers INTEGER NOT NULL CHECK(workers>=0), PRIMARY KEY(slot,family_id,tier)
            ) STRICT;
            CREATE TABLE progression_acquisitions (
                slot INTEGER NOT NULL REFERENCES game_save(slot) CHECK(slot=1), family_id TEXT NOT NULL,
                acquired INTEGER NOT NULL CHECK(acquired>=0), PRIMARY KEY(slot,family_id)
            ) STRICT;
            CREATE TABLE progression_technologies (
                slot INTEGER NOT NULL REFERENCES game_save(slot) CHECK(slot=1), technology_id TEXT NOT NULL,
                PRIMARY KEY(slot,technology_id)
            ) STRICT;
            """)) schema.ExecuteNonQuery();
        Insert(connection, tx, "INSERT INTO game_save VALUES(1,$p0,$p1,$p2,$p3,$p4)",
            s.FormatVersion, s.ScenarioId, s.RulesVersion, s.ElapsedTicks, s.Population);
        Insert(connection, tx, "INSERT INTO population_state VALUES(1,$p0,$p1)",
            s.GrowthProgressSeconds, s.GrowthRemainderTwentieths);
        Insert(connection, tx, "INSERT INTO military_state VALUES(1,$p0,$p1,$p2,$p3)",
            s.Military.WaitingRecruits, s.Military.TrainingRecruits,
            s.Military.AvailableSoldiers, s.Military.TrainingProgressSeconds);
        Insert(connection, tx, "INSERT INTO progression_school_state VALUES(1,$p0,$p1,$p2,$p3,$p4,$p5,$p6)",
            s.SchoolCount, s.SchoolResearch.WaitingStudents, s.SchoolResearch.TrainingStudents,
            s.SchoolResearch.QualifiedResearchers, s.SchoolResearch.ActiveResearchers,
            s.ResearchBalanceMilliPoints, s.PencilPaidStudents);
        foreach (var (id, amount) in s.Resources.Amounts)
            Insert(connection, tx, "INSERT INTO resource_stock VALUES(1,$p0,$p1)", id, amount.Milliunits);
        foreach (var b in s.OtherBuildings)
            Insert(connection, tx, "INSERT INTO building_stock VALUES(1,$p0,$p1,$p2)", b.BuildingId, b.Count, b.AssignedWorkers);
        for (var i = 0; i < s.Military.IndividualProgressSeconds.Length; i++)
            Insert(connection, tx, "INSERT INTO military_training VALUES(1,$p0,$p1)", i, s.Military.IndividualProgressSeconds[i]);
        for (var i = 0; i < s.SchoolResearch.TrainingProgressSeconds.Length; i++)
            Insert(connection, tx, "INSERT INTO progression_training VALUES(1,$p0,$p1)", i, s.SchoolResearch.TrainingProgressSeconds[i]);
        foreach (var family in s.ProductiveFamilies)
        {
            var occupancies = new[] { family.Base, family.Large, family.Advanced };
            for (var tier = 0; tier < occupancies.Length; tier++)
                Insert(connection, tx, "INSERT INTO progression_family VALUES(1,$p0,$p1,$p2,$p3)",
                    family.FamilyId, tier, occupancies[tier].Buildings, occupancies[tier].AssignedWorkers);
        }
        foreach (var history in s.PurchaseCounters)
            Insert(connection, tx, "INSERT INTO progression_acquisitions VALUES(1,$p0,$p1)", history.FamilyId, history.Acquired);
        foreach (var id in s.UnlockedTechnologyIds)
            Insert(connection, tx, "INSERT INTO progression_technologies VALUES(1,$p0)", id);
        tx.Commit();
    }

    private V14CampaignSnapshot ReadFile(string file)
    {
        using var connection = Open(file, SqliteOpenMode.ReadOnly);
        CheckFile(connection);
        using var tx = connection.BeginTransaction(deferred: true);
        // Reject extra slots, malformed generations, incomplete state and unexpected named tables.
        using (var tables = Sql(connection, tx, """
            SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;
            """))
        using (var reader = tables.ExecuteReader())
        {
            var expected = new HashSet<string>(StringComparer.Ordinal)
            { "game_save", "resource_stock", "building_stock", "population_state", "military_state",
              "military_training", "progression_school_state", "progression_training", "progression_family",
              "progression_acquisitions", "progression_technologies" };
            while (reader.Read())
                if (!expected.Remove(reader.GetString(0))) throw Invalid("Unexpected table in the v3 checkpoint.");
            if (expected.Count > 0) throw Invalid("A required v3 table is missing.");
        }
        using var header = Sql(connection, tx,
            "SELECT snapshot_version,scenario_id,rules_version,elapsed_ticks,population FROM game_save WHERE slot=1");
        using var h = header.ExecuteReader();
        if (!h.Read()) throw Invalid("Missing v3 header.");
        var version = checked((int)h.GetInt64(0));
        var scenario = h.GetString(1);
        var rules = h.GetString(2);
        var time = h.GetInt64(3);
        var population = checked((int)h.GetInt64(4));
        if (h.Read()) throw Invalid("Duplicated v3 header.");
        h.Close();
        if (version != FileVersion || scenario != _baseline.Id || rules != V14CampaignSnapshots.RulesVersion)
            throw new GameSaveException(GameSaveError.UnsupportedFormat, "The v3 snapshot has unknown rules.");
        var growth = ReadOne(connection, tx, "SELECT growth_seconds,twentieths FROM population_state WHERE slot=1", 2);
        var military = ReadOne(connection, tx, "SELECT waiting,training,soldiers,progress_seconds FROM military_state WHERE slot=1", 4);
        // Existing RC8 files have no pencil-paid column: their enrolled students
        // are grandfathered, and cancelling them must never mint free pencils.
        var paidColumn = HasColumn(connection, tx, "progression_school_state", "pencil_paid_students");
        var school = ReadOne(connection, tx,
            "SELECT school_count,waiting_students,training_students,qualified_researchers,active_researchers,research_millipoints" +
            (paidColumn ? ",pencil_paid_students" : "") + " FROM progression_school_state WHERE slot=1",
            paidColumn ? 7 : 6);
        var militaryProgress = ReadOrderedProgress(connection, tx, "military_training", checked((int)military[1]));
        // Compare with an independently persisted count: even a missing final
        // position must invalidate the checkpoint rather than lose a student.
        var studentProgress = ReadOrderedProgress(connection, tx, "progression_training", checked((int)school[2]));
        var resourceRows = ReadRows(connection, tx, "SELECT resource_id,milliunits FROM resource_stock WHERE slot=1 ORDER BY resource_id", 2);
        var resources = ResourceBundle.FromAmounts(resourceRows.Select(row =>
            KeyValuePair.Create((string)row[0], ResourceAmount.FromMilliunits((long)row[1]))));
        var otherRows = ReadRows(connection, tx, "SELECT building_id,count,workers FROM building_stock WHERE slot=1 ORDER BY building_id", 3);
        var other = otherRows.Select(row => new BuildingState((string)row[0], checked((int)(long)row[1]),
            checked((int)(long)row[2]))).ToImmutableArray();
        var familyRows = ReadRows(connection, tx,
            "SELECT family_id,tier,count,workers FROM progression_family WHERE slot=1 ORDER BY family_id,tier", 4);
        if (familyRows.Count != 3 * 16) throw Invalid("The v3 tier table must contain all 16 families and three tiers.");
        var familyBuilder = ImmutableArray.CreateBuilder<V14FamilySnapshot>();
        for (var i = 0; i < familyRows.Count; i += 3)
        {
            var id = (string)familyRows[i][0];
            var tiers = new TierOccupancy[3];
            for (var tier = 0; tier < 3; tier++)
            {
                var row = familyRows[i + tier];
                if ((string)row[0] != id || (long)row[1] != tier)
                    throw Invalid("Missing or duplicated tier position.");
                tiers[tier] = new(checked((int)(long)row[2]), checked((int)(long)row[3]));
            }
            familyBuilder.Add(new(id, tiers[0], tiers[1], tiers[2]));
        }
        var counters = ReadRows(connection, tx,
            "SELECT family_id,acquired FROM progression_acquisitions WHERE slot=1 ORDER BY family_id", 2)
            .Select(row => new V14PurchaseCounter((string)row[0], checked((int)(long)row[1]))).ToImmutableArray();
        var techIds = ReadRows(connection, tx,
            "SELECT technology_id FROM progression_technologies WHERE slot=1 ORDER BY technology_id", 1)
            .Select(row => (string)row[0]).ToImmutableArray();
        var result = new V14CampaignSnapshot(version, scenario, rules, time, population, resources,
            other, familyBuilder.ToImmutable(), counters, checked((int)school[0]),
            checked((int)growth[0]), checked((int)growth[1]),
            new MilitaryTrainingState(checked((int)military[0]), checked((int)military[1]),
                checked((int)military[2]), checked((int)military[3]), militaryProgress),
            new SchoolResearchState(checked((int)school[1]), studentProgress,
                checked((int)school[3]), checked((int)school[4])), school[5], techIds,
            paidColumn ? checked((int)school[6]) : 0);
        V14CampaignSnapshots.Validate(_baseline, result, _school, _technologies);
        tx.Commit();
        return result;
    }

    private static bool HasColumn(SqliteConnection connection, SqliteTransaction tx, string table, string column)
    {
        using var command = Sql(connection, tx, $"PRAGMA table_info({table});");
        using var reader = command.ExecuteReader();
        while (reader.Read()) if (reader.GetString(1) == column) return true;
        return false;
    }

    private static long[] ReadOne(SqliteConnection connection, SqliteTransaction tx, string sql, int count)
    {
        using var command = Sql(connection, tx, sql);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) throw Invalid("Required v3 state row is missing.");
        var values = new long[count];
        for (var i = 0; i < count; i++) values[i] = reader.GetInt64(i);
        if (reader.Read()) throw Invalid("More than one state row.");
        return values;
    }

    private static ImmutableArray<int> ReadOrderedProgress(SqliteConnection connection, SqliteTransaction tx,
        string table, int? count)
    {
        var rows = ReadRows(connection, tx,
            $"SELECT position,progress_seconds FROM {table} WHERE slot=1 ORDER BY position", 2);
        if (count is not null && rows.Count != count) throw Invalid("Training progress row count differs from the state.");
        var builder = ImmutableArray.CreateBuilder<int>();
        foreach (var row in rows)
        {
            if ((long)row[0] != builder.Count) throw Invalid("Training positions are not contiguous.");
            builder.Add(checked((int)(long)row[1]));
        }
        return builder.ToImmutable();
    }

    private static List<object[]> ReadRows(SqliteConnection connection, SqliteTransaction tx, string sql, int count)
    {
        using var command = Sql(connection, tx, sql + $" LIMIT {Limit + 1}");
        using var reader = command.ExecuteReader();
        var result = new List<object[]>();
        while (reader.Read())
        {
            if (result.Count >= Limit) throw Invalid("Too many rows in the v3 checkpoint.");
            var values = new object[count];
            for (var i = 0; i < count; i++) values[i] = reader.GetValue(i);
            result.Add(values);
        }
        return result;
    }

    private void BackupExisting()
    {
        using var source = Open(DatabasePath, SqliteOpenMode.ReadWrite);
        CheckFile(source);
        var directory = Path.GetDirectoryName(DatabasePath)!;
        var stem = Path.GetFileNameWithoutExtension(DatabasePath);
        var extension = Path.GetExtension(DatabasePath);
        for (var i = 1; i <= 100; i++)
        {
            var backup = Path.Combine(directory,
                $"{stem}.v14.backup{(i == 1 ? "" : "-" + i)}{extension}");
            if (File.Exists(backup)) continue;
            using var command = source.CreateCommand();
            command.CommandText = "VACUUM INTO $path;";
            command.Parameters.AddWithValue("$path", backup);
            command.ExecuteNonQuery();
            return;
        }
        throw new GameSaveException(GameSaveError.Unavailable, "All v14 backup slots are occupied.");
    }
}
