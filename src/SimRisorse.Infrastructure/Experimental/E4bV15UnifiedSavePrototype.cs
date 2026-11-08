using System.Globalization;
using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Infrastructure.Persistence;

namespace SimRisorse.Infrastructure.Experimental;

/// <summary>
/// RC105: non-destructive, READ-ONLY generation-7 format prototype. Unlike the
/// RC95 composite, the city uses the normal generation-6 physical SQLite tables
/// alongside the funding and receipt tables, in ONE file. The new format is not
/// used by the game or the production save loader, and cannot become authoritative.
/// There is deliberately NO public write/clock/construction API for this format.
/// </summary>
public sealed class E4bV15UnifiedSavePrototype
{
    public const int ApplicationId = 0x53465237; // SFR7; distinct from playable generation 6.
    public const int FileVersion = 7;
    private const string ReadOnlyMode = "rc105-experimental-not-authoritative";
    private const string StagePrefix = ".rc105-unified-";
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private readonly GameScenario _baseline;
    private readonly SchoolResearchRules _school;
    private readonly TechnologyCatalog _technologies;
    private readonly FoodConversionPolicy _food;

    public E4bV15UnifiedSavePrototype(GameScenario baseline, SchoolResearchRules school,
        TechnologyCatalog technologies, FoodConversionPolicy food)
    {
        _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        _school = school ?? throw new ArgumentNullException(nameof(school));
        _technologies = technologies ?? throw new ArgumentNullException(nameof(technologies));
        _food = food ?? throw new ArgumentNullException(nameof(food));
    }

    /// <summary>
    /// Snapshot ONLY the currently authoritative RC103 composite into a NEW
    /// SQLite file. An active cantiere, including its deposited materials,
    /// is preserved. Neither the source composite nor the authority registry is
    /// written or promoted. A stale or unregistered branch cannot be imported.
    /// </summary>
    public E4bV15UnifiedSaveInspection CreateFromAuthoritativeComposite(
        string compositePath, string registryPath, string destinationPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(compositePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(registryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        var source = Path.GetFullPath(compositePath);
        var registry = Path.GetFullPath(registryPath);
        var target = Path.GetFullPath(destinationPath);
        if (string.Equals(source, target, PathComparison) ||
            string.Equals(registry, target, PathComparison))
            throw new ArgumentException("Source, registry and destination must be distinct files.", nameof(destinationPath));
        if (!Directory.Exists(Path.GetDirectoryName(target)))
            throw new DirectoryNotFoundException("Create the prototype destination directory first.");
        RequireFreshTarget(target);
        var preflight = new E4bV15UnifiedSavePreflight(_baseline, _school, _technologies, _food);
        var before = preflight.Analyze(source, registry);
        if (!before.EligibleForFutureUnifiedMigration || before.CampaignId is null)
            throw new InvalidOperationException("RC105 requires an intact, registered authoritative composite branch.");

        var snapshot = StagePath(target);
        var stage = StagePath(target);
        try
        {
            // sqlite3_backup produces a consistent point-in-time image even if
            // the source is in WAL mode. Work on the private copy, never on a
            // live registered/guarded database.
            using (var original = Open(source, SqliteOpenMode.ReadOnly))
            using (var copy = Open(snapshot, SqliteOpenMode.ReadWriteCreate))
                original.BackupDatabase(copy);
            var composite = new E4bV15FundingCompositeSqliteStore(snapshot,
                _baseline, _school, _technologies, _food);
            var shadow = composite.Load();
            var identity = composite.ReadCampaignId();
            if (identity != before.CampaignId || shadow.Revision != before.Revision)
                throw new InvalidOperationException("The authoritative composite changed before snapshotting.");

            // Generate a real generation-6 city in SQLite tables, NOT a BLOB
            // containing another database (the old composite representation).
            new V15SqliteFoodSaveStore(stage, _baseline, _school,
                _technologies, _food).Save(shadow.Campaign);
            using (var sourceDb = Open(snapshot, SqliteOpenMode.ReadOnly))
            using (var destination = Open(stage, SqliteOpenMode.ReadWrite))
            using (var tx = destination.BeginTransaction())
            {
                Execute(destination, tx, $"PRAGMA application_id={ApplicationId};");
                Execute(destination, tx, $"PRAGMA user_version={FileVersion};");
                Execute(destination, tx, """
                    CREATE TABLE unified_campaign (
                        slot INTEGER PRIMARY KEY CHECK(slot=1),
                        campaign_id TEXT NOT NULL,
                        revision INTEGER NOT NULL CHECK(revision>=0),
                        funding_json TEXT NOT NULL,
                        authority_mode TEXT NOT NULL CHECK(authority_mode='rc105-experimental-not-authoritative')
                    ) STRICT;
                    CREATE TABLE unified_receipt (
                        request_id TEXT PRIMARY KEY NOT NULL,
                        payload TEXT NOT NULL,
                        committed_revision INTEGER NOT NULL CHECK(committed_revision>0)
                    ) STRICT;
                    """);
                using (var insert = Sql(destination, tx, """
                    INSERT INTO unified_campaign(slot,campaign_id,revision,funding_json,authority_mode)
                    VALUES (1,$campaign,$revision,$funding,$mode);
                    """))
                {
                    insert.Parameters.AddWithValue("$campaign", identity!);
                    insert.Parameters.AddWithValue("$revision", shadow.Revision);
                    insert.Parameters.AddWithValue("$funding", E4bCurveAFundingSqliteStore.Encode(
                        shadow.Funding.ExportCheckpoint()));
                    insert.Parameters.AddWithValue("$mode", ReadOnlyMode);
                    insert.ExecuteNonQuery();
                }
                using (var select = Sql(sourceDb, null, """
                    SELECT request_id,payload,committed_revision
                    FROM composite_receipt ORDER BY committed_revision;
                    """))
                using (var rows = select.ExecuteReader())
                {
                    long expected = 0;
                    while (rows.Read())
                    {
                        expected = checked(expected + 1);
                        if (rows.GetInt64(2) != expected)
                            throw new InvalidDataException("Composite receipt sequence is not contiguous.");
                        using var insert = Sql(destination, tx, """
                            INSERT INTO unified_receipt(request_id,payload,committed_revision)
                            VALUES ($id,$payload,$revision);
                            """);
                        insert.Parameters.AddWithValue("$id", rows.GetString(0));
                        insert.Parameters.AddWithValue("$payload", rows.GetString(1));
                        insert.Parameters.AddWithValue("$revision", expected);
                        insert.ExecuteNonQuery();
                    }
                    if (expected != shadow.Revision)
                        throw new InvalidDataException("The receipt count differs from the composite revision.");
                }
                tx.Commit();
            }
            var verified = Inspect(stage);
            if (verified.CampaignId != before.CampaignId ||
                verified.Revision != before.Revision ||
                !verified.Escrowed.Equals(before.Escrowed) ||
                verified.HasPendingFinancing != before.HasPendingFinancing)
                throw new InvalidDataException("The staged unified image diverges from the source snapshot.");
            // Preflight AGAIN: an intervening write or authority transfer must
            // not cause a misleading 'authoritative export' claim. This output
            // remains a non-authoritative snapshot even if the check passes.
            var after = preflight.Analyze(source, registry);
            if (!after.EligibleForFutureUnifiedMigration || after.CampaignId != before.CampaignId ||
                after.Revision != before.Revision)
                throw new InvalidOperationException("The authoritative branch changed during RC105 export.");
            RequireFreshTarget(target);
            File.Move(stage, target); // No overwrite, even if destination appeared meanwhile.
            return verified;
        }
        finally
        {
            DeleteStage(snapshot);
            DeleteStage(stage);
        }
    }

    /// <summary>
    /// Inspect a generation-7 experimental snapshot. Native city rows are
    /// verified through the strict generation-6 loader on a PRIVATE temporary
    /// copy with the RC105-only tables removed. The original is never modified.
    /// </summary>
    public E4bV15UnifiedSaveInspection Inspect(string prototypePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prototypePath);
        var path = Path.GetFullPath(prototypePath);
        if (!File.Exists(path)) throw new FileNotFoundException("RC105 prototype not found.", path);
        string campaignId;
        string fundingJson;
        long revision;
        long receiptCount;
        using (var db = Open(path, SqliteOpenMode.ReadOnly))
        {
            if (Scalar(db, null, "PRAGMA application_id;") != ApplicationId ||
                Scalar(db, null, "PRAGMA user_version;") != FileVersion)
                throw new InvalidDataException("Not an RC105 unified format prototype.");
            using (var integrity = Sql(db, null, "PRAGMA integrity_check;"))
                if (integrity.ExecuteScalar() is not string result || result != "ok")
                    throw new InvalidDataException("RC105 SQLite integrity_check failed.");
            if (Scalar(db, null, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('unified_campaign','unified_receipt');") != 2 ||
                Scalar(db, null, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';") != 18)
                throw new InvalidDataException("The RC105 unified schema is missing or contains unexpected tables.");
            using (var header = Sql(db, null, "SELECT campaign_id,revision,funding_json,authority_mode FROM unified_campaign WHERE slot=1;"))
            using (var reader = header.ExecuteReader())
            {
                if (!reader.Read()) throw new InvalidDataException("Missing RC105 campaign header.");
                campaignId = reader.GetString(0);
                revision = reader.GetInt64(1);
                fundingJson = reader.GetString(2);
                if (reader.GetString(3) != ReadOnlyMode || reader.Read())
                    throw new InvalidDataException("RC105 is not an inactive, single-slot prototype.");
            }
            if (!Guid.TryParseExact(campaignId, "N", out var guid) ||
                guid.ToString("N") != campaignId || revision < 0 ||
                Scalar(db, null, "SELECT COUNT(*) FROM unified_campaign;") != 1)
                throw new InvalidDataException("RC105 campaign identity or revision is corrupt.");
            using (var receipts = Sql(db, null, """
                SELECT COUNT(*),COUNT(DISTINCT committed_revision),
                       COALESCE(MIN(committed_revision),0),COALESCE(MAX(committed_revision),0),
                       COALESCE(SUM(CASE WHEN length(request_id)=0 OR length(request_id)>96 OR
                          request_id GLOB '*[^A-Za-z0-9_-]*' OR length(trim(payload))=0
                          THEN 1 ELSE 0 END),0)
                FROM unified_receipt;
                """))
            using (var reader = receipts.ExecuteReader())
            {
                if (!reader.Read()) throw new InvalidDataException("Missing RC105 receipt log.");
                receiptCount = reader.GetInt64(0);
                var distinct = reader.GetInt64(1);
                var first = reader.GetInt64(2);
                var last = reader.GetInt64(3);
                var invalid = reader.GetInt64(4);
                if (receiptCount != revision || distinct != revision || invalid != 0 ||
                    (revision == 0 ? first != 0 || last != 0 : first != 1 || last != revision))
                    throw new InvalidDataException("RC105 revision/receipt reconciliation failed.");
            }
        }

        var clone = StagePath(path);
        try
        {
            using (var original = Open(path, SqliteOpenMode.ReadOnly))
            using (var copy = Open(clone, SqliteOpenMode.ReadWriteCreate))
                original.BackupDatabase(copy);
            using (var writable = Open(clone, SqliteOpenMode.ReadWrite))
            using (var tx = writable.BeginTransaction())
            {
                Execute(writable, tx, "DROP TABLE unified_receipt; DROP TABLE unified_campaign;");
                Execute(writable, tx, $"PRAGMA application_id={SqliteGameSaveStore.ApplicationId};");
                Execute(writable, tx, $"PRAGMA user_version={V15SqliteFoodSaveStore.FileVersion};");
                tx.Commit();
            }
            var campaign = new V15SqliteFoodSaveStore(clone, _baseline, _school,
                _technologies, _food).Load();
            E4bV15FundingShadow shadow;
            try
            {
                var funding = E4bCurveAFundingSqliteStore.Decode(fundingJson);
                shadow = E4bV15FundingShadow.Restore(new(E4bV15FundingShadow.Version,
                    revision, campaign, funding), _baseline, _school, _technologies, _food);
            }
            catch (Exception ex) when (ex is ArgumentException or OverflowException or InvalidOperationException)
            {
                throw new InvalidDataException("RC105 city and funding have diverged.", ex);
            }
            return new E4bV15UnifiedSaveInspection(campaignId, revision, receiptCount,
                shadow.IsFunding, shadow.Funding.Escrowed, campaign,
                IsAuthoritative: false);
        }
        finally { DeleteStage(clone); }
    }

    private static void RequireFreshTarget(string target)
    {
        if (File.Exists(target) || Directory.Exists(target))
            throw new IOException("RC105 refuses to overwrite an existing file.");
    }

    private static SqliteConnection Open(string file, SqliteOpenMode mode)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = file, Mode = mode, Pooling = false, ForeignKeys = true, DefaultTimeout = 5
        }.ToString());
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    private static SqliteCommand Sql(SqliteConnection db, SqliteTransaction? transaction, string text)
    {
        var command = db.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = text;
        return command;
    }

    private static long Scalar(SqliteConnection db, SqliteTransaction? tx, string query)
    {
        using var command = Sql(db, tx, query);
        return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    private static void Execute(SqliteConnection db, SqliteTransaction tx, string commandText)
    {
        using var command = Sql(db, tx, commandText);
        command.ExecuteNonQuery();
    }

    private static string StagePath(string sibling) => Path.Combine(
        Path.GetDirectoryName(sibling)!, StagePrefix + Guid.NewGuid().ToString("N") + ".sqlite3");

    private static void DeleteStage(string file)
    {
        foreach (var path in new[] { file, file + "-wal", file + "-shm" })
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}

/// <summary>A verified experimental snapshot, NOT an active playable save.</summary>
public sealed record E4bV15UnifiedSaveInspection(
    string CampaignId, long Revision, long ReceiptCount,
    bool HasPendingFinancing, ResourceBundle Escrowed,
    V15FoodCampaignCheckpoint Campaign, bool IsAuthoritative);
