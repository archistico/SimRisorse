using System.Collections.Immutable;
using Microsoft.Data.Sqlite;
using SimRisorse.Application.Persistence;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Military;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Infrastructure.Persistence;

/// <summary>
/// F1-D05e.2b.1: development-only SQLite schema generation 6, civil-growth-15.
/// Stores the city campaign, depot and intermediate stocks/building crews in
/// one atomic checkpoint. Previous development generations are intentionally
/// incompatible; backwards compatibility is not required during development.
/// </summary>
public sealed class V15SqliteFoodSaveStore
{
    public const int FileVersion = V15FoodCampaignCheckpoints.FileVersion;
    private const int ApplicationId = SqliteGameSaveStore.ApplicationId;
    private const int Limit = 10000;
    private readonly GameScenario _baseline;
    private readonly SchoolResearchRules _school;
    private readonly TechnologyCatalog _technologies;
    private readonly FoodConversionPolicy _policy;

    public V15SqliteFoodSaveStore(string path, GameScenario baselineV13,
        SchoolResearchRules schoolRules, TechnologyCatalog technologies, FoodConversionPolicy policy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _baseline = baselineV13 ?? throw new ArgumentNullException(nameof(baselineV13));
        _school = schoolRules ?? throw new ArgumentNullException(nameof(schoolRules));
        _technologies = technologies ?? throw new ArgumentNullException(nameof(technologies));
        if (_baseline.Id != "civil-growth" || _baseline.Catalog.RulesVersion != "civil-growth-13")
            throw new ArgumentException("A known v13 scenario is required.", nameof(baselineV13));
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        DatabasePath = Path.GetFullPath(path);
    }

    public string DatabasePath { get; }

    public void Save(V15FoodCampaignCheckpoint snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        try { V15FoodCampaignCheckpoints.Validate(snapshot, _baseline, _school, _technologies, _policy); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or OverflowException)
        { throw Invalid("The v15 food checkpoint is not valid.", ex); }
        // Generation 6 never persists the retired producer. Reject occupied
        // historical slots; ordinary v15 checkpoints contain 15 families already.
        if (!V15LegacyProducerAdapter.CanOmit(snapshot.CityCampaign))
            throw Invalid("Retire the historical food-store before saving a generation-6 checkpoint.");
        // An imported, empty historical placeholder may be omitted, but never
        // persisted. Work on a separate immutable copy; do not mutate caller state.
        var canonical = snapshot with
        {
            CityCampaign = V15LegacyProducerAdapter.WithoutEmptySlot(snapshot.CityCampaign)
        };
        if (canonical.CityCampaign.Resources.Amounts.Count > Limit || canonical.CityCampaign.OtherBuildings.Length > Limit ||
            canonical.CityCampaign.ProductiveFamilies.Length > Limit || canonical.CityCampaign.PurchaseCounters.Length > Limit ||
            canonical.CityCampaign.SchoolResearch.TrainingStudents > Limit ||
            canonical.CityCampaign.Military.IndividualProgressSeconds.Length > Limit || canonical.CityCampaign.UnlockedTechnologyIds.Length > Limit ||
            snapshot.Depot.RawFood.Amounts.Count > Limit ||
            snapshot.Industry.Animals.Amounts.Count > Limit ||
            snapshot.Industry.Transit.Amounts.Count > Limit)
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
            WriteNewFile(temporary, canonical);
            // Loading proves validity; equivalence also proves that no valid but
            // different checkpoint can silently replace the caller's data.
            var restored = LoadFrom(temporary);
            if (!SameCheckpoint(canonical, restored))
                throw Invalid("The staged SQLite checkpoint differs from the requested state.");
            if (existed)
            {
                BackupExisting(); // S1: consistent SQLite copy, never overwritten.
                File.Move(temporary, DatabasePath, overwrite: true);
            }
            else File.Move(temporary, DatabasePath);
            temporary = null;
        }
        catch (SqliteException ex) { throw Invalid("SQLite could not write the v15 checkpoint.", ex); }
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

    public V15FoodCampaignCheckpoint Load()
    {
        if (!File.Exists(DatabasePath))
            throw new GameSaveException(GameSaveError.NotFound, "The v15 checkpoint does not exist.");
        return LoadFrom(DatabasePath);
    }

    private V15FoodCampaignCheckpoint LoadFrom(string file)
    {
        try { return ReadFile(file); }
        catch (SqliteException ex) { throw Invalid("SQLite generation 6 checkpoint data are corrupt.", ex); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or InvalidCastException or FormatException or OverflowException or IndexOutOfRangeException)
        { throw Invalid("The v15 checkpoint is incomplete or inconsistent.", ex); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { throw new GameSaveException(GameSaveError.Unavailable, "Cannot read the checkpoint.", ex); }
    }

    /// <summary>
    /// Compare every persisted v15 field. SQLite orders keyed rows by ID, while
    /// in-memory construction preserves catalog order; those collections are
    /// semantic maps. Individual training progress retains positional meaning.
    /// Run before promotion, not after replacing the live file.
    /// </summary>
    private static bool SameCheckpoint(V15FoodCampaignCheckpoint expected,
        V15FoodCampaignCheckpoint actual)
    {
        var a = expected.CityCampaign;
        var b = actual.CityCampaign;
        return expected.CrewMode == actual.CrewMode &&
            a.ScenarioId == b.ScenarioId && a.ElapsedTicks == b.ElapsedTicks &&
            a.Population == b.Population && a.Resources.Equals(b.Resources) &&
            a.SchoolCount == b.SchoolCount &&
            a.GrowthProgressSeconds == b.GrowthProgressSeconds &&
            a.GrowthRemainderTwentieths == b.GrowthRemainderTwentieths &&
            a.ResearchBalanceMilliPoints == b.ResearchBalanceMilliPoints &&
            a.PencilPaidStudents == b.PencilPaidStudents &&
            a.Military.Equals(b.Military) &&
            a.SchoolResearch.WaitingStudents == b.SchoolResearch.WaitingStudents &&
            a.SchoolResearch.QualifiedResearchers == b.SchoolResearch.QualifiedResearchers &&
            a.SchoolResearch.ActiveResearchers == b.SchoolResearch.ActiveResearchers &&
            a.SchoolResearch.TrainingProgressSeconds.SequenceEqual(
                b.SchoolResearch.TrainingProgressSeconds) &&
            SameKeyed(a.OtherBuildings, b.OtherBuildings, x => x.BuildingId) &&
            SameKeyed(a.ProductiveFamilies, b.ProductiveFamilies, x => x.FamilyId) &&
            SameKeyed(a.PurchaseCounters, b.PurchaseCounters, x => x.FamilyId) &&
            a.UnlockedTechnologyIds.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(
                b.UnlockedTechnologyIds.OrderBy(x => x, StringComparer.Ordinal)) &&
            expected.Depot.Equals(actual.Depot) &&
            expected.Industry.Equals(actual.Industry);
    }

    private static bool SameKeyed<T>(IEnumerable<T> expected, IEnumerable<T> actual,
        Func<T, string> key) where T : notnull =>
        expected.OrderBy(key, StringComparer.Ordinal).SequenceEqual(
            actual.OrderBy(key, StringComparer.Ordinal));

    private static GameSaveException Invalid(string message, Exception? inner = null) =>
        new(GameSaveError.InvalidData, message, inner);

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
            throw new GameSaveException(GameSaveError.UnsupportedFormat, "Unsupported SimRisorse food-save schema (generation 6 required).");
        // Never guess missing fields from an older development schema.
        using var columns = Sql(connection, null, "PRAGMA table_info(progression_school_state);");
        using var reader = columns.ExecuteReader();
        var required = new HashSet<string>(StringComparer.Ordinal)
        { "training_students", "pencil_paid_students" };
        while (reader.Read()) required.Remove(reader.GetString(1));
        if (required.Count != 0)
            throw new GameSaveException(GameSaveError.UnsupportedFormat,
                "This v15 checkpoint is missing mandatory school-state fields.");
    }

    private static void Insert(SqliteConnection connection, SqliteTransaction tx, string sql, params object[] values)
    {
        using var command = Sql(connection, tx, sql);
        for (var i = 0; i < values.Length; i++) command.Parameters.AddWithValue($"$p{i}", values[i]);
        command.ExecuteNonQuery();
    }

    private static void WriteNewFile(string file, V15FoodCampaignCheckpoint checkpoint)
    {
        var s = checkpoint.CityCampaign;
        using var connection = Open(file, SqliteOpenMode.ReadWriteCreate);
        using var tx = connection.BeginTransaction();
        using (var schema = Sql(connection, tx, $"""
            PRAGMA application_id={ApplicationId};
            PRAGMA user_version={FileVersion};
            CREATE TABLE game_save (
                slot INTEGER PRIMARY KEY CHECK(slot=1), snapshot_version INTEGER NOT NULL CHECK(snapshot_version={FileVersion}),
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
                slot INTEGER NOT NULL REFERENCES game_save(slot) CHECK(slot=1),
                family_id TEXT NOT NULL CHECK(family_id <> 'food-store'),
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
            CREATE TABLE food_raw_stock (
                slot INTEGER NOT NULL REFERENCES game_save(slot) CHECK(slot=1), resource_id TEXT NOT NULL,
                milliunits INTEGER NOT NULL CHECK(milliunits>=0), PRIMARY KEY(slot,resource_id)
            ) STRICT;
            CREATE TABLE food_ready_stock (
                slot INTEGER PRIMARY KEY REFERENCES game_save(slot) CHECK(slot=1),
                milliunits INTEGER NOT NULL CHECK(milliunits>=0)
            ) STRICT;
            CREATE TABLE food_industry_site (
                slot INTEGER NOT NULL REFERENCES game_save(slot) CHECK(slot=1),
                site_id TEXT NOT NULL,
                count INTEGER NOT NULL CHECK(count>=0),
                workers INTEGER NOT NULL CHECK(workers>=0),
                PRIMARY KEY(slot,site_id)
            ) STRICT;
            CREATE TABLE food_animals_stock (
                slot INTEGER NOT NULL REFERENCES game_save(slot) CHECK(slot=1),
                resource_id TEXT NOT NULL,
                milliunits INTEGER NOT NULL CHECK(milliunits>=0),
                PRIMARY KEY(slot,resource_id)
            ) STRICT;
            CREATE TABLE food_transit_stock (
                slot INTEGER NOT NULL REFERENCES game_save(slot) CHECK(slot=1),
                resource_id TEXT NOT NULL,
                milliunits INTEGER NOT NULL CHECK(milliunits>=0),
                PRIMARY KEY(slot,resource_id)
            ) STRICT;
            """)) schema.ExecuteNonQuery();
        Insert(connection, tx, "INSERT INTO game_save VALUES(1,$p0,$p1,$p2,$p3,$p4)",
            FileVersion, s.ScenarioId, V15CrewRules.SavedRulesVersion(checkpoint.CrewMode),
            s.ElapsedTicks, s.Population);
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
        foreach (var (id, amount) in checkpoint.Depot.RawFood.Amounts)
            Insert(connection, tx, "INSERT INTO food_raw_stock VALUES(1,$p0,$p1)", id, amount.Milliunits);
        Insert(connection, tx, "INSERT INTO food_ready_stock VALUES(1,$p0)", checkpoint.Depot.ReadyFood.Milliunits);
        foreach (var site in FoodIndustrySites.PersistedIds)
            Insert(connection, tx, "INSERT INTO food_industry_site VALUES(1,$p0,$p1,$p2)", site,
                FoodIndustrySites.Get(checkpoint.Industry.Buildings.Counts, site),
                FoodIndustrySites.Get(checkpoint.Industry.Buildings.Assigned, site));
        foreach (var (id, amount) in checkpoint.Industry.Animals.Amounts)
            Insert(connection, tx, "INSERT INTO food_animals_stock VALUES(1,$p0,$p1)", id, amount.Milliunits);
        foreach (var (id, amount) in checkpoint.Industry.Transit.Amounts)
            Insert(connection, tx, "INSERT INTO food_transit_stock VALUES(1,$p0,$p1)", id, amount.Milliunits);
        tx.Commit();
    }

    private V15FoodCampaignCheckpoint ReadFile(string file)
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
              "progression_acquisitions", "progression_technologies", "food_raw_stock", "food_ready_stock",
              "food_industry_site", "food_animals_stock", "food_transit_stock" };
            while (reader.Read())
                if (!expected.Remove(reader.GetString(0))) throw Invalid("Unexpected table in the v15 checkpoint.");
            if (expected.Count > 0) throw Invalid("A required v15 table is missing.");
        }
        using var header = Sql(connection, tx,
            "SELECT snapshot_version,scenario_id,rules_version,elapsed_ticks,population FROM game_save WHERE slot=1");
        using var h = header.ExecuteReader();
        if (!h.Read()) throw Invalid("Missing v15 header.");
        var version = checked((int)h.GetInt64(0));
        var scenario = h.GetString(1);
        var rules = h.GetString(2);
        var time = h.GetInt64(3);
        var population = checked((int)h.GetInt64(4));
        if (h.Read()) throw Invalid("Duplicated v15 header.");
        h.Close();
        if (version != FileVersion || scenario != _baseline.Id ||
            rules is not (V15FoodCampaignCheckpoints.RulesVersion or V15CrewRules.MintTenRulesVersion))
            throw new GameSaveException(GameSaveError.UnsupportedFormat, "The v15 food snapshot has unknown rules.");
        var crewMode = V15CrewRules.FromSavedRulesVersion(rules);
        var growth = ReadOne(connection, tx, "SELECT growth_seconds,twentieths FROM population_state WHERE slot=1", 2);
        var military = ReadOne(connection, tx, "SELECT waiting,training,soldiers,progress_seconds FROM military_state WHERE slot=1", 4);
        // Unlike the backward-compatible v14 loader, v15 mandates this field.
        var school = ReadOne(connection, tx,
            "SELECT school_count,waiting_students,training_students,qualified_researchers,active_researchers,research_millipoints," +
            "pencil_paid_students FROM progression_school_state WHERE slot=1", 7);
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
        // Persist only the fifteen real productive families; the v15 checkpoint
        // has no empty historical slot, even after reading from SQLite.
        if (familyRows.Count != 3 * (TieredProductionCatalog.KnownFamilyIds.Length - 1))
            throw Invalid("The generation-6 tier table requires exactly 15 active families and three tiers.");
        if (familyRows.Any(row => (string)row[0] == V15LegacyProducerAdapter.RetiredFamilyId))
            throw Invalid("The retired food-store must not be present in the generation-6 tier table.");
        // Decode directly into v15-owned production DTOs: no v14 checkpoint on load.
        var familyBuilder = ImmutableArray.CreateBuilder<V15ProductiveFamilySnapshot>();
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
            .Select(row => new V15PurchaseCounter((string)row[0], checked((int)(long)row[1]))).ToImmutableArray();
        var techIds = ReadRows(connection, tx,
            "SELECT technology_id FROM progression_technologies WHERE slot=1 ORDER BY technology_id", 1)
            .Select(row => (string)row[0]).ToImmutableArray();
        var campaign = new V15CityCampaignSnapshot(scenario, time, population, resources,
            other, familyBuilder.ToImmutable(),
            counters, checked((int)school[0]),
            checked((int)growth[0]), checked((int)growth[1]),
            new MilitaryTrainingState(checked((int)military[0]), checked((int)military[1]),
                checked((int)military[2]), checked((int)military[3]), militaryProgress),
            new SchoolResearchState(checked((int)school[1]), studentProgress,
                checked((int)school[3]), checked((int)school[4])), school[5], techIds,
            checked((int)school[6]));
        var rawRows = ReadRows(connection, tx,
            "SELECT resource_id,milliunits FROM food_raw_stock WHERE slot=1 ORDER BY resource_id", 2);
        var raw = ResourceBundle.FromAmounts(rawRows.Select(row =>
            KeyValuePair.Create((string)row[0], ResourceAmount.FromMilliunits((long)row[1]))));
        var ready = ReadOne(connection, tx,
            "SELECT milliunits FROM food_ready_stock WHERE slot=1", 1);
        var animalRows = ReadRows(connection, tx,
            "SELECT resource_id,milliunits FROM food_animals_stock WHERE slot=1 ORDER BY resource_id", 2);
        var animals = ResourceBundle.FromAmounts(animalRows.Select(row =>
            KeyValuePair.Create((string)row[0], ResourceAmount.FromMilliunits((long)row[1]))));
        var transitRows = ReadRows(connection, tx,
            "SELECT resource_id,milliunits FROM food_transit_stock WHERE slot=1 ORDER BY resource_id", 2);
        var transit = ResourceBundle.FromAmounts(transitRows.Select(row =>
            KeyValuePair.Create((string)row[0], ResourceAmount.FromMilliunits((long)row[1]))));
        var sites = ReadRows(connection, tx,
            "SELECT site_id,count,workers FROM food_industry_site WHERE slot=1 ORDER BY site_id", 3);
        var counts = new Dictionary<string, long>(StringComparer.Ordinal);
        var workers = new Dictionary<string, long>(StringComparer.Ordinal);
        // Generation 6 originally persisted six base food sites. Newer saves may
        // contain two additional depot-tier rows; missing tiers mean zero owned.
        if ((sites.Count != FoodIndustrySites.Ids.Length &&
                sites.Count != FoodIndustrySites.PersistedIds.Length) ||
            sites.Any(row => !FoodIndustrySites.PersistedIds.Contains((string)row[0])))
            throw Invalid("The v15 food-site table has incomplete or unexpected identities.");
        foreach (var row in sites)
        {
            if (!counts.TryAdd((string)row[0], (long)row[1]) ||
                !workers.TryAdd((string)row[0], (long)row[2]))
                throw Invalid("Duplicated food-industry building.");
        }
        var buildings = new FoodIndustryBuildings(
            FoodIndustrySites.FromDictionary(counts), FoodIndustrySites.FromDictionary(workers));
        var result = new V15FoodCampaignCheckpoint(campaign,
            new FoodDepotStock(raw, ResourceAmount.FromMilliunits(ready[0])),
            new FoodIndustryCampaignState(animals, transit, buildings)) { CrewMode = crewMode };
        V15FoodCampaignCheckpoints.Validate(result, _baseline, _school, _technologies, _policy);
        tx.Commit();
        return result;
    }

    private static long[] ReadOne(SqliteConnection connection, SqliteTransaction tx, string sql, int count)
    {
        using var command = Sql(connection, tx, sql);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) throw Invalid("Required v15 state row is missing.");
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
            if (result.Count >= Limit) throw Invalid("Too many rows in the v15 checkpoint.");
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
                $"{stem}.v15.backup{(i == 1 ? "" : "-" + i)}{extension}");
            if (File.Exists(backup)) continue;
            using var command = source.CreateCommand();
            command.CommandText = "VACUUM INTO $path;";
            command.Parameters.AddWithValue("$path", backup);
            command.ExecuteNonQuery();
            return;
        }
        // Historical numbered slots must never be deleted or recycled automatically.
        // Once all 100 legacy slots are taken, use a collision-resistant name for
        // each additional SQLite-consistent backup. No existing backup is touched.
        // Users may archive old backups themselves; there is intentionally no
        // silent retention/deletion policy.
        var overflow = Path.Combine(directory,
            $"{stem}.v15.backup-extra-{DateTime.UtcNow:yyyyMMddTHHmmssfffZ}-{Guid.NewGuid():N}{extension}");
        using var overflowCommand = source.CreateCommand();
        overflowCommand.CommandText = "VACUUM INTO $path;";
        overflowCommand.Parameters.AddWithValue("$path", overflow);
        overflowCommand.ExecuteNonQuery();
    }
}
