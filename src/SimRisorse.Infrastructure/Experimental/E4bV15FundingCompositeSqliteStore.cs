using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Infrastructure.Persistence;

namespace SimRisorse.Infrastructure.Experimental;

/// <summary>RC95 experimental container. A complete, independently validated v15
/// generation-6 SQLite image and the escrow ledger are committed as one SQLite row,
/// together with an idempotency receipt. This is NOT the normal player-save format.
/// No synthetic deliveries or direct UI calls are accepted. RC98 adds mint completion only inside this isolated composite database.</summary>
public sealed class E4bV15FundingCompositeSqliteStore
{
    public const int ApplicationId = 0x53465235;
    public const int FileVersion = 1;
    private const int MaxImageBytes = 32 * 1024 * 1024;
    private const string AuthorityFunction = "rc102_authorized_write";
    private const string AuthorityTable = "composite_authority_guard";
    private const string IdentityTable = "composite_campaign_identity";
    private const int RequiredIdentityTriggers = 3;
    private const int RequiredAuthorityTriggers = 8;
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private readonly GameScenario _baseline;
    private readonly SchoolResearchRules _school;
    private readonly TechnologyCatalog _technologies;
    private readonly FoodConversionPolicy _food;

    public E4bV15FundingCompositeSqliteStore(string path, GameScenario baseline,
        SchoolResearchRules school, TechnologyCatalog technologies, FoodConversionPolicy food)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        _school = school ?? throw new ArgumentNullException(nameof(school));
        _technologies = technologies ?? throw new ArgumentNullException(nameof(technologies));
        _food = food ?? throw new ArgumentNullException(nameof(food));
        DatabasePath = Path.GetFullPath(path);
        if (!Directory.Exists(Path.GetDirectoryName(DatabasePath)))
            throw new DirectoryNotFoundException("Create the RC95 experimental directory first.");
        // Existing files are inspected BEFORE opening any write-capable connection.
        // This prevents accidental changes to native v15 and RC93 save formats.
        if (File.Exists(DatabasePath))
        {
            using var probe = Open(SqliteOpenMode.ReadOnly);
            var id = Scalar(probe, null, "PRAGMA application_id;");
            var version = Scalar(probe, null, "PRAGMA user_version;");
            if (id != ApplicationId || version != FileVersion)
                throw new InvalidDataException("Not an RC95 composite funding database.");
        }
        using var db = Open(SqliteOpenMode.ReadWriteCreate);
        using var tx = db.BeginTransaction();
        var application = Scalar(db, tx, "PRAGMA application_id;");
        var fileVersion = Scalar(db, tx, "PRAGMA user_version;");
        if (application == 0 && fileVersion == 0)
        {
            if (Scalar(db, tx, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';") != 0)
                throw new InvalidDataException("Refusing to initialize an unrelated database.");
            Execute(db, tx, $"PRAGMA application_id = {ApplicationId};");
            Execute(db, tx, $"PRAGMA user_version = {FileVersion};");
            Execute(db, tx, """
                CREATE TABLE composite_state (
                    slot INTEGER PRIMARY KEY CHECK(slot=1),
                    revision INTEGER NOT NULL CHECK(revision >= 0),
                    native_v15_image BLOB NOT NULL,
                    funding_json TEXT NOT NULL
                );
                CREATE TABLE composite_receipt (
                    request_id TEXT PRIMARY KEY NOT NULL,
                    payload TEXT NOT NULL,
                    committed_revision INTEGER NOT NULL CHECK(committed_revision > 0)
                );
                CREATE TABLE composite_campaign_identity (
                    slot INTEGER PRIMARY KEY CHECK(slot=1),
                    campaign_id TEXT NOT NULL
                );
                """);
            using (var identity = Sql(db, tx,
                "INSERT INTO composite_campaign_identity(slot,campaign_id) VALUES (1,$id);"))
            {
                identity.Parameters.AddWithValue("$id", Guid.NewGuid().ToString("N"));
                identity.ExecuteNonQuery();
            }
        }
        else if (application != ApplicationId || fileVersion != FileVersion ||
            Scalar(db, tx, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('composite_state','composite_receipt');") != 2)
            throw new InvalidDataException("Invalid RC95 composite SQLite schema.");
        tx.Commit();
    }

    public string DatabasePath { get; }

    /// <summary>
    /// RC103: a persistent identity for this experimental campaign, independent
    /// of the filename and authority epoch. Older unregistered RC95–RC102 files
    /// have no ID until they are explicitly registered with an authority.
    /// This read never changes the database.
    /// </summary>
    public string? ReadCampaignId()
    {
        using var db = Open(SqliteOpenMode.ReadOnly);
        return ReadCampaignId(db, null);
    }

    private static string? ReadCampaignId(SqliteConnection db, SqliteTransaction? tx)
    {
        if (Scalar(db, tx, $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{IdentityTable}';") == 0)
            return null;
        string id;
        using (var command = Sql(db, tx,
            "SELECT campaign_id FROM composite_campaign_identity WHERE slot=1;"))
        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read()) throw new InvalidDataException("Missing composite campaign identity.");
            id = reader.GetString(0);
            if (reader.Read()) throw new InvalidDataException("Duplicated composite campaign identity.");
        }
        if (!Guid.TryParseExact(id, "N", out var parsed) ||
            !string.Equals(parsed.ToString("N"), id, StringComparison.Ordinal) ||
            Scalar(db, tx, "SELECT COUNT(*) FROM composite_campaign_identity;") != 1)
            throw new InvalidDataException("Invalid or duplicated composite campaign identity.");
        return id;
    }

    private static string EnsureCampaignId(SqliteConnection db, SqliteTransaction tx)
    {
        var existing = ReadCampaignId(db, tx);
        if (existing is not null) return existing;
        Execute(db, tx, "CREATE TABLE composite_campaign_identity (slot INTEGER PRIMARY KEY CHECK(slot=1), campaign_id TEXT NOT NULL);");
        var id = Guid.NewGuid().ToString("N");
        using var command = Sql(db, tx,
            "INSERT INTO composite_campaign_identity(slot,campaign_id) VALUES (1,$id);");
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
        return id;
    }

    public E4bV15FundingShadow Create(V15FoodCampaignCheckpoint initial)
    {
        var shadow = E4bV15FundingShadow.Open(_baseline, _school, _technologies, _food, initial);
        var image = WriteNativeImage(shadow.Campaign);
        var funding = E4bCurveAFundingSqliteStore.Encode(shadow.Funding.ExportCheckpoint());
        using var db = Open(SqliteOpenMode.ReadWrite);
        AuthorizeWrite(db, null);
        using var tx = db.BeginTransaction();
        using var command = Sql(db, tx, "INSERT INTO composite_state(slot,revision,native_v15_image,funding_json) VALUES (1,0,$image,$funding);");
        command.Parameters.AddWithValue("$image", image);
        command.Parameters.AddWithValue("$funding", funding);
        command.ExecuteNonQuery(); // Creating a second session is never an overwrite.
        tx.Commit();
        return shadow;
    }

    public E4bV15FundingShadow Load()
    {
        using var db = Open(SqliteOpenMode.ReadOnly);
        return Read(db, null);
    }

    /// <summary>One transaction for city image, escrow and receipt. Reusing the same
    /// request with the same payload is idempotent even after further operations;
    /// replay of a different payload is rejected. Optimistic revision is enforced.</summary>
    public E4bV15FundingCompositeResult Apply(string requestId, long expectedRevision,
        E4bFundingInstruction instruction) => ApplyCore(requestId, expectedRevision, instruction, null);

    // Only the authority registry can call this variant. The registry holds its
    // SQLite write transaction throughout the operation, including the store commit.
    internal E4bV15FundingCompositeResult ApplyAuthorized(string registryPath,
        string requestId, long expectedRevision, E4bFundingInstruction instruction) =>
        ApplyCore(requestId, expectedRevision, instruction, registryPath);

    private E4bV15FundingCompositeResult ApplyCore(string requestId, long expectedRevision,
        E4bFundingInstruction instruction, string? registryPath)
    {
        ArgumentNullException.ThrowIfNull(instruction);
        var payload = instruction.CanonicalPayload();
        if (instruction.Action is not (E4bFundingAction.BeginMint or E4bFundingAction.Deposit or
            E4bFundingAction.Cancel or E4bFundingAction.Complete))
            throw new ArgumentException("Only city-backed mint financing operations are supported.", nameof(instruction));
        return ApplyTransaction(requestId, expectedRevision, payload, current =>
        {
            E4bV15FundingShadow advanced = current;
            var accepted = instruction.Action switch
            {
                E4bFundingAction.BeginMint => current.TryBeginMint(instruction.Tier!.Value,
                    expectedRevision, out advanced),
                E4bFundingAction.Deposit => current.TryDeposit(instruction.Materials!,
                    expectedRevision, out advanced),
                E4bFundingAction.Cancel => current.TryCancel(expectedRevision, out advanced),
                E4bFundingAction.Complete => current.TryCompleteMint(expectedRevision, out advanced),
                _ => false
            };
            return accepted ? advanced : null;
        }, registryPath);
    }

    /// <summary>RC97: exactly one real v15 second, committed atomically together
    /// with the escrow and an idempotency receipt. The caller supplies the same
    /// canonical production policies used by the game; their serialized digest
    /// is bound to the request ID so a retry with different rules conflicts.</summary>
    public E4bV15FundingCompositeResult AdvanceClock(string requestId,
        long expectedRevision, MeatChainPolicy meat, HorticulturePolicy crops) =>
        AdvanceClockCore(requestId, expectedRevision, meat, crops, null);

    internal E4bV15FundingCompositeResult AdvanceClockAuthorized(string registryPath,
        string requestId, long expectedRevision, MeatChainPolicy meat, HorticulturePolicy crops) =>
        AdvanceClockCore(requestId, expectedRevision, meat, crops, registryPath);

    private E4bV15FundingCompositeResult AdvanceClockCore(string requestId,
        long expectedRevision, MeatChainPolicy meat, HorticulturePolicy crops, string? registryPath)
    {
        ArgumentNullException.ThrowIfNull(meat);
        ArgumentNullException.ThrowIfNull(crops);
        var policyJson = JsonSerializer.Serialize(new { Meat = meat, Crops = crops });
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(policyJson)));
        return ApplyTransaction(requestId, expectedRevision, "clock:v1:1s:" + fingerprint,
            current => current.TryAdvanceClock(meat, crops, expectedRevision,
                out var next) ? next : null, registryPath);
    }

    private E4bV15FundingCompositeResult ApplyTransaction(string requestId,
        long expectedRevision, string payload,
        Func<E4bV15FundingShadow, E4bV15FundingShadow?> transition, string? registryPath)
    {
        ValidateKey(requestId);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedRevision);
        using var db = Open(SqliteOpenMode.ReadWrite);
        AuthorizeWrite(db, registryPath);
        using var tx = db.BeginTransaction();
        var current = Read(db, tx);
        using (var lookup = Sql(db, tx, "SELECT payload FROM composite_receipt WHERE request_id=$id;"))
        {
            lookup.Parameters.AddWithValue("$id", requestId);
            var previous = lookup.ExecuteScalar() as string;
            if (previous is not null)
            {
                tx.Commit();
                return new(previous == payload ? E4bFundingApplyStatus.Replayed :
                    E4bFundingApplyStatus.KeyConflict, current);
            }
        }
        if (current.Revision != expectedRevision || current.Revision == long.MaxValue)
            return new(E4bFundingApplyStatus.StaleRevision, current);
        var advanced = transition(current);
        if (advanced is null) return new(E4bFundingApplyStatus.Rejected, current);

        // Both the entire native city image and the escrow snapshot are verified
        // BEFORE replacing the row, and are committed with the receipt in one tx.
        var image = WriteNativeImage(advanced.Campaign);
        var funding = E4bCurveAFundingSqliteStore.Encode(advanced.Funding.ExportCheckpoint());
        var native = ReadNativeImage(image);
        _ = E4bV15FundingShadow.Restore(new(E4bV15FundingShadow.Version,
            advanced.Revision, native, E4bCurveAFundingSqliteStore.Decode(funding)),
            _baseline, _school, _technologies, _food);
        using (var update = Sql(db, tx,
            "UPDATE composite_state SET revision=$new,native_v15_image=$image,funding_json=$funding WHERE slot=1 AND revision=$old;"))
        {
            update.Parameters.AddWithValue("$new", advanced.Revision);
            update.Parameters.AddWithValue("$image", image);
            update.Parameters.AddWithValue("$funding", funding);
            update.Parameters.AddWithValue("$old", expectedRevision);
            if (update.ExecuteNonQuery() != 1)
                throw new InvalidOperationException("Stale composite state. No materials were committed.");
        }
        using (var receipt = Sql(db, tx,
            "INSERT INTO composite_receipt(request_id,payload,committed_revision) VALUES ($id,$payload,$revision);"))
        {
            receipt.Parameters.AddWithValue("$id", requestId);
            receipt.Parameters.AddWithValue("$payload", payload);
            receipt.Parameters.AddWithValue("$revision", advanced.Revision);
            receipt.ExecuteNonQuery();
        }
        tx.Commit();
        return new(E4bFundingApplyStatus.Applied, advanced);
    }

    /// <summary>
    /// RC102: permanently bind an existing composite to its authority-registry
    /// path. SQLite triggers then reject ALL ordinary writes to city, receipts,
    /// or the binding, including writes from already-open legacy store objects.
    /// Backups inherit the fence. A copied registry at a different path does
    /// not authorize the same composite.
    /// </summary>
    internal void FenceForAuthority(string registryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registryPath);
        var canonical = Path.GetFullPath(registryPath);
        using var db = Open(SqliteOpenMode.ReadWrite);
        using var tx = db.BeginTransaction();
        // The migration is confined to laboratory files explicitly registered
        // under the authority lock; old unregistered files remain loadable.
        _ = EnsureCampaignId(db, tx);
        if (Scalar(db, tx, $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{AuthorityTable}';") == 0)
        {
            if (Scalar(db, tx, "SELECT COUNT(*) FROM composite_state WHERE slot=1;") != 1)
                throw new InvalidDataException("Cannot fence an uninitialized composite database.");
            Execute(db, tx, "CREATE TABLE composite_authority_guard (slot INTEGER PRIMARY KEY CHECK(slot=1), registry_path TEXT NOT NULL);");
            using (var binding = Sql(db, tx,
                "INSERT INTO composite_authority_guard(slot,registry_path) VALUES (1,$registry);"))
            {
                binding.Parameters.AddWithValue("$registry", canonical);
                binding.ExecuteNonQuery();
            }
            foreach (var (table, action, name) in new[]
            {
                ("composite_state", "INSERT", "state_insert"),
                ("composite_state", "UPDATE", "state_update"),
                ("composite_state", "DELETE", "state_delete"),
                ("composite_receipt", "INSERT", "receipt_insert"),
                ("composite_receipt", "UPDATE", "receipt_update"),
                ("composite_receipt", "DELETE", "receipt_delete"),
                (AuthorityTable, "UPDATE", "guard_update"),
                (AuthorityTable, "DELETE", "guard_delete")
            })
            {
                // SQLite must resolve the connection-local function when the
                // write occurs. No function on raw SQLite / old store connections:
                // fail closed, even if those writers ignore the registry API.
                Execute(db, tx, $"""
                    CREATE TRIGGER rc102_guard_{name} BEFORE {action} ON {table}
                    BEGIN
                        SELECT CASE WHEN {AuthorityFunction}() = 1 THEN 1
                            ELSE RAISE(ABORT, 'RC102 authority required') END;
                    END;
                    """);
            }
        }
        // The campaign identity must be immutable on registered files as well.
        // Installed separately so RC102's original eight triggers remain intact.
        foreach (var (operation, name) in new[]
        {
            ("INSERT", "identity_insert"),
            ("UPDATE", "identity_update"),
            ("DELETE", "identity_delete")
        })
        {
            if (Scalar(db, tx, $"SELECT COUNT(*) FROM sqlite_master WHERE type='trigger' AND name='rc103_guard_{name}';") == 0)
                Execute(db, tx, $"""
                    CREATE TRIGGER rc103_guard_{name} BEFORE {operation} ON {IdentityTable}
                    BEGIN
                        SELECT CASE WHEN {AuthorityFunction}() = 1 THEN 1
                            ELSE RAISE(ABORT, 'RC103 campaign authority required') END;
                    END;
                    """);
        }
        VerifyAuthorityFence(db, tx, canonical);
        tx.Commit();
    }

    private static bool HasAuthorityFence(SqliteConnection db) =>
        Scalar(db, null, $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{AuthorityTable}';") == 1;

    private static void VerifyAuthorityFence(SqliteConnection db, SqliteTransaction? tx, string registryPath)
    {
        if (Scalar(db, tx, $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{AuthorityTable}';") != 1)
            throw new InvalidDataException("The composite is not bound to an authority registry.");
        using (var query = Sql(db, tx, "SELECT registry_path FROM composite_authority_guard WHERE slot=1;"))
        {
            var actual = query.ExecuteScalar() as string;
            if (actual is null || !string.Equals(actual, registryPath, PathComparison) ||
                Scalar(db, tx, "SELECT COUNT(*) FROM composite_authority_guard;") != 1)
                throw new InvalidDataException("Composite authority binding belongs to another registry.");
        }
        if (Scalar(db, tx,
            "SELECT COUNT(*) FROM sqlite_master WHERE type='trigger' AND name GLOB 'rc102_guard_*';") != RequiredAuthorityTriggers ||
            Scalar(db, tx,
            "SELECT COUNT(*) FROM sqlite_master WHERE type='trigger' AND name GLOB 'rc103_guard_*';") != RequiredIdentityTriggers)
            throw new InvalidDataException("The composite authority write triggers are missing or modified.");
        if (ReadCampaignId(db, tx) is null)
            throw new InvalidDataException("Registered composites must carry a campaign identity.");
    }

    private static void AuthorizeWrite(SqliteConnection db, string? registryPath)
    {
        if (registryPath is null)
        {
            if (HasAuthorityFence(db))
                throw new InvalidOperationException(
                    "This composite is registered: use the authority registry for all writes.");
            return;
        }
        var canonical = Path.GetFullPath(registryPath);
        VerifyAuthorityFence(db, null, canonical);
        // Registered ONLY on the registry-controlled connection and ONLY after
        // verifying the persisted binding. Ordinary connections lack this function.
        db.CreateFunction<long>(AuthorityFunction, () => 1L);
    }

    private E4bV15FundingShadow Read(SqliteConnection db, SqliteTransaction? tx)
    {
        long revision;
        byte[] image;
        string fundingJson;
        using (var command = Sql(db, tx,
            "SELECT revision,native_v15_image,funding_json FROM composite_state WHERE slot=1;"))
        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read()) throw new KeyNotFoundException("No RC95 experimental composite state was created.");
            revision = reader.GetInt64(0);
            image = reader.GetFieldValue<byte[]>(1);
            fundingJson = reader.GetString(2);
            if (reader.Read()) throw new InvalidDataException("Unexpected duplicate composite state.");
        }
        if (revision < 0 || image.Length == 0 || image.Length > MaxImageBytes)
            throw new InvalidDataException("Invalid composite checkpoint header or image size.");
        _ = ReadCampaignId(db, tx); // Reject malformed RC103 identity without changing legacy files.
        ValidateReceiptLog(db, tx, revision);
        try
        {
            var native = ReadNativeImage(image);
            var checkpoint = E4bCurveAFundingSqliteStore.Decode(fundingJson);
            return E4bV15FundingShadow.Restore(new(E4bV15FundingShadow.Version,
                revision, native, checkpoint), _baseline, _school, _technologies, _food);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or OverflowException)
        {
            throw new InvalidDataException("Corrupt or divergent city/escrow checkpoint.", ex);
        }
    }

    /// <summary>RC100: each accepted revision has precisely one persisted,
    /// distinct operation receipt. A missing, duplicated or reordered receipt
    /// invalidates a backup and blocks all further operations. This checks
    /// structural integrity, not authenticity against a malicious database edit.</summary>
    private static void ValidateReceiptLog(SqliteConnection db, SqliteTransaction? tx, long revision)
    {
        using var command = Sql(db, tx, """
            SELECT COUNT(*), COUNT(DISTINCT committed_revision),
                   COALESCE(MIN(committed_revision),0), COALESCE(MAX(committed_revision),0),
                   COALESCE(SUM(CASE WHEN length(request_id) = 0 OR length(request_id) > 96 OR
                       request_id GLOB '*[^A-Za-z0-9_-]*' OR
                       length(trim(payload)) = 0 THEN 1 ELSE 0 END),0)
            FROM composite_receipt;
            """);
        using var reader = command.ExecuteReader();
        if (!reader.Read() || reader.IsDBNull(0))
            throw new InvalidDataException("Missing composite operation receipt audit.");
        var count = reader.GetInt64(0);
        var unique = reader.GetInt64(1);
        var first = reader.GetInt64(2);
        var last = reader.GetInt64(3);
        var invalid = reader.GetInt64(4);
        if (count != revision || unique != revision || invalid != 0 ||
            (revision == 0 ? first != 0 || last != 0 : first != 1 || last != revision))
            throw new InvalidDataException("Composite revision and operation receipts do not reconcile.");
    }

    private byte[] WriteNativeImage(V15FoodCampaignCheckpoint campaign)
    {
        var temporary = TemporaryNativePath();
        try
        {
            new V15SqliteFoodSaveStore(temporary, _baseline, _school, _technologies, _food).Save(campaign);
            var image = File.ReadAllBytes(temporary);
            if (image.Length == 0 || image.Length > MaxImageBytes)
                throw new InvalidDataException("The native campaign image exceeds the RC95 size limit.");
            return image;
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private V15FoodCampaignCheckpoint ReadNativeImage(byte[] image)
    {
        if (image.Length == 0 || image.Length > MaxImageBytes)
            throw new InvalidDataException("Invalid embedded v15 image length.");
        var temporary = TemporaryNativePath();
        try
        {
            File.WriteAllBytes(temporary, image);
            return new V15SqliteFoodSaveStore(temporary, _baseline, _school, _technologies, _food).Load();
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private string TemporaryNativePath() => Path.Combine(Path.GetDirectoryName(DatabasePath)!,
        ".rc95-native-" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture) + ".sqlite3");

    private SqliteConnection Open(SqliteOpenMode mode)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath, Mode = mode, Pooling = false,
            ForeignKeys = true, DefaultTimeout = 5
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

    private static void Execute(SqliteConnection db, SqliteTransaction tx, string query)
    {
        using var command = Sql(db, tx, query);
        command.ExecuteNonQuery();
    }

    private static void ValidateKey(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length > 96 || value.Any(c => !((c is >= 'a' and <= 'z') ||
            (c is >= 'A' and <= 'Z') || (c is >= '0' and <= '9') || c is '-' or '_')))
            throw new ArgumentException("Request IDs accept only 1-96 ASCII letters, numbers, - or _.", nameof(value));
    }
}

public sealed record E4bV15FundingCompositeResult(E4bFundingApplyStatus Status,
    E4bV15FundingShadow State);
