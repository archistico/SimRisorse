using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;

namespace SimRisorse.Infrastructure.Experimental;

public enum E4bFundingApplyStatus { Applied, Replayed, StaleRevision, Rejected, KeyConflict }
public sealed record E4bFundingStoredSession(string SessionId, long Revision,
    E4bCurveAFundingSandbox State);
public sealed record E4bFundingApplyResult(E4bFundingApplyStatus Status,
    E4bFundingStoredSession Session);

/// <summary>
/// RC93 opt-in SQLite laboratory. It NEVER opens or updates the v15 save store:
/// its application_id and schema are distinct. Every accepted instruction writes
/// stock, escrow, counters and the idempotency receipt in ONE SQLite transaction.
/// A failed command writes nothing. Retrying a committed request id is a no-op.
/// </summary>
public sealed class E4bCurveAFundingSqliteStore
{
    public const int ApplicationId = 0x53465239;
    public const int FileVersion = 1;
    public string DatabasePath { get; }

    public E4bCurveAFundingSqliteStore(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        DatabasePath = Path.GetFullPath(databasePath);
        if (!Directory.Exists(Path.GetDirectoryName(DatabasePath)))
            throw new DirectoryNotFoundException("The isolated funding directory must already exist.");
        // Inspect an existing database in read-only mode BEFORE attempting any
        // write transaction, including a foreign or player-save SQLite file.
        if (File.Exists(DatabasePath))
        {
            using var probe = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = DatabasePath, Mode = SqliteOpenMode.ReadOnly,
                Pooling = false, DefaultTimeout = 5
            }.ToString());
            probe.Open();
            using var existingId = probe.CreateCommand();
            existingId.CommandText = "PRAGMA application_id;";
            using var existingVersion = probe.CreateCommand();
            existingVersion.CommandText = "PRAGMA user_version;";
            var idValue = Convert.ToInt64(existingId.ExecuteScalar(), CultureInfo.InvariantCulture);
            var versionValue = Convert.ToInt64(existingVersion.ExecuteScalar(), CultureInfo.InvariantCulture);
            if ((idValue != 0 || versionValue != 0) &&
                (idValue != ApplicationId || versionValue != FileVersion))
                throw new InvalidDataException("Refusing to open a non-financing SQLite database.");
        }
        using var db = OpenConnection();
        using var transaction = db.BeginTransaction();
        var id = Scalar(db, transaction, "PRAGMA application_id;");
        var version = Scalar(db, transaction, "PRAGMA user_version;");
        if (id == 0 && version == 0)
        {
            if (Scalar(db, transaction,
                "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';") != 0)
                throw new InvalidDataException("Cannot initialize financing inside an unrelated SQLite database.");
            ExecuteNonQuery(db, transaction, $"PRAGMA application_id = {ApplicationId};");
            ExecuteNonQuery(db, transaction, $"PRAGMA user_version = {FileVersion};");
            ExecuteNonQuery(db, transaction, """
                CREATE TABLE funding_session (
                    session_id TEXT PRIMARY KEY NOT NULL,
                    revision INTEGER NOT NULL CHECK (revision >= 0),
                    state_json TEXT NOT NULL
                );
                CREATE TABLE funding_request (
                    session_id TEXT NOT NULL,
                    request_id TEXT NOT NULL,
                    canonical_payload TEXT NOT NULL,
                    committed_revision INTEGER NOT NULL,
                    PRIMARY KEY (session_id, request_id),
                    FOREIGN KEY (session_id) REFERENCES funding_session(session_id)
                );
                """);
        }
        else if (id != ApplicationId || version != FileVersion)
            throw new InvalidDataException("This is not an RC93 isolated financing database.");
        else if (Scalar(db, transaction,
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('funding_session','funding_request');") != 2)
            throw new InvalidDataException("Incomplete isolated financing schema.");
        transaction.Commit();
    }

    public E4bFundingStoredSession CreateSession(string sessionId, ResourceBundle stock,
        ResourceBundle capacity, int alreadyAcquiredMints = 0)
    {
        ValidateKey(sessionId, nameof(sessionId));
        var state = E4bCurveAFundingSandbox.Open(stock, capacity, alreadyAcquiredMints);
        using var db = OpenConnection();
        using var tx = db.BeginTransaction();
        using (var command = Sql(db, tx,
            "INSERT INTO funding_session(session_id, revision, state_json) VALUES ($id, 0, $state);"))
        {
            command.Parameters.AddWithValue("$id", sessionId);
            command.Parameters.AddWithValue("$state", Encode(state.ExportCheckpoint()));
            command.ExecuteNonQuery(); // Duplicate session id is rejected, never overwritten.
        }
        tx.Commit();
        return new(sessionId, 0, state);
    }

    public E4bFundingStoredSession Load(string sessionId)
    {
        ValidateKey(sessionId, nameof(sessionId));
        using var db = OpenConnection();
        using var tx = db.BeginTransaction();
        var session = Read(db, tx, sessionId);
        tx.Commit();
        return session;
    }

    public E4bFundingApplyResult Apply(string sessionId, string requestId, long expectedRevision,
        E4bFundingInstruction instruction)
    {
        ValidateKey(sessionId, nameof(sessionId));
        ValidateKey(requestId, nameof(requestId));
        ArgumentOutOfRangeException.ThrowIfNegative(expectedRevision);
        ArgumentNullException.ThrowIfNull(instruction);
        var canonical = instruction.CanonicalPayload();
        using var db = OpenConnection();
        // BeginTransaction() acquires a write reservation (non-deferred by default),
        // serializing competing requests before either checks the expected revision.
        using var tx = db.BeginTransaction();
        var current = Read(db, tx, sessionId);
        using (var receipt = Sql(db, tx,
            "SELECT canonical_payload FROM funding_request WHERE session_id=$sid AND request_id=$rid;"))
        {
            receipt.Parameters.AddWithValue("$sid", sessionId);
            receipt.Parameters.AddWithValue("$rid", requestId);
            var stored = receipt.ExecuteScalar() as string;
            if (stored is not null)
            {
                tx.Commit();
                return new(stored == canonical ? E4bFundingApplyStatus.Replayed :
                    E4bFundingApplyStatus.KeyConflict, current);
            }
        }
        if (current.Revision != expectedRevision || current.Revision == long.MaxValue)
            return new(E4bFundingApplyStatus.StaleRevision, current);
        if (!instruction.TryApply(current.State, out var advanced) || !advanced.Reconciles())
            return new(E4bFundingApplyStatus.Rejected, current);
        // Fail closed: check serialization and restoration before updating the DB.
        var snapshot = Encode(advanced.ExportCheckpoint());
        _ = E4bCurveAFundingSandbox.Restore(Decode(snapshot));
        var nextRevision = checked(current.Revision + 1);
        using (var update = Sql(db, tx,
            "UPDATE funding_session SET revision=$new, state_json=$state WHERE session_id=$id AND revision=$old;"))
        {
            update.Parameters.AddWithValue("$new", nextRevision);
            update.Parameters.AddWithValue("$state", snapshot);
            update.Parameters.AddWithValue("$id", sessionId);
            update.Parameters.AddWithValue("$old", current.Revision);
            if (update.ExecuteNonQuery() != 1)
                throw new InvalidOperationException("Concurrent financing revision change; transaction was rolled back.");
        }
        using (var receipt = Sql(db, tx,
            "INSERT INTO funding_request(session_id,request_id,canonical_payload,committed_revision) VALUES ($sid,$rid,$payload,$rev);"))
        {
            receipt.Parameters.AddWithValue("$sid", sessionId);
            receipt.Parameters.AddWithValue("$rid", requestId);
            receipt.Parameters.AddWithValue("$payload", canonical);
            receipt.Parameters.AddWithValue("$rev", nextRevision);
            receipt.ExecuteNonQuery();
        }
        tx.Commit();
        return new(E4bFundingApplyStatus.Applied,
            new E4bFundingStoredSession(sessionId, nextRevision, advanced));
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath, Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false, ForeignKeys = true, DefaultTimeout = 5
        }.ToString());
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    private static SqliteCommand Sql(SqliteConnection db, SqliteTransaction tx, string text)
    {
        var command = db.CreateCommand();
        command.Transaction = tx;
        command.CommandText = text;
        return command;
    }

    private static long Scalar(SqliteConnection db, SqliteTransaction tx, string text)
    {
        using var command = Sql(db, tx, text);
        return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    private static void ExecuteNonQuery(SqliteConnection db, SqliteTransaction tx, string text)
    {
        using var command = Sql(db, tx, text);
        command.ExecuteNonQuery();
    }

    private static E4bFundingStoredSession Read(SqliteConnection db, SqliteTransaction tx, string id)
    {
        using var command = Sql(db, tx,
            "SELECT revision, state_json FROM funding_session WHERE session_id=$id;");
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) throw new KeyNotFoundException("Funding sandbox session does not exist.");
        var revision = reader.GetInt64(0);
        var json = reader.GetString(1);
        if (revision < 0) throw new InvalidDataException("Negative financing revision.");
        try
        {
            return new(id, revision, E4bCurveAFundingSandbox.Restore(Decode(json)));
        }
        catch (Exception exception) when (exception is ArgumentException or JsonException or OverflowException)
        {
            throw new InvalidDataException("Corrupt or inconsistent financing checkpoint.", exception);
        }
    }

    private static void ValidateKey(string value, string argument)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, argument);
        if (value.Length > 96 || value.Any(c => !((c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '_')))
            throw new ArgumentException("Identifiers must be 1–96 ASCII letters, digits, hyphens or underscores.", argument);
    }

    private sealed class CheckpointDto
    {
        public int Version { get; set; }
        public SortedDictionary<string, long>? InitialStock { get; set; }
        public SortedDictionary<string, long>? Stock { get; set; }
        public SortedDictionary<string, long>? Capacity { get; set; }
        public SortedDictionary<string, long>? HypotheticalDelivered { get; set; }
        public SortedDictionary<string, long>? Committed { get; set; }
        public int AcquiredMints { get; set; }
        public int? PendingTier { get; set; }
        public SortedDictionary<string, long>? PendingQuote { get; set; }
        public SortedDictionary<string, long>? PendingPaid { get; set; }
        public SortedDictionary<string, long>? RealClockCredits { get; set; }
        public SortedDictionary<string, long>? RealClockDebits { get; set; }
        public E4bMintOrigin? MintOrigin { get; set; }
        public List<CompletedMintDto>? CompletedMints { get; set; }
    }

    private sealed class CompletedMintDto
    {
        public int Tier { get; set; }
        public int PreviouslyAcquired { get; set; }
        public SortedDictionary<string, long>? Quote { get; set; }
    }

    private static SortedDictionary<string, long> Amounts(ResourceBundle bundle) => new(
        bundle.Amounts.ToDictionary(pair => pair.Key, pair => pair.Value.Milliunits, StringComparer.Ordinal),
        StringComparer.Ordinal);

    private static ResourceBundle FromAmounts(SortedDictionary<string, long>? amounts)
    {
        if (amounts is null) throw new InvalidDataException("Missing financing material vector.");
        return ResourceBundle.FromAmounts(amounts.Select(pair =>
            KeyValuePair.Create(pair.Key, ResourceAmount.FromMilliunits(pair.Value))));
    }

    internal static string Encode(E4bFundingCheckpoint checkpoint) => JsonSerializer.Serialize(new CheckpointDto
    {
        Version = checkpoint.Version,
        InitialStock = Amounts(checkpoint.InitialStock), Stock = Amounts(checkpoint.Stock),
        Capacity = Amounts(checkpoint.Capacity),
        HypotheticalDelivered = Amounts(checkpoint.HypotheticalDelivered),
        Committed = Amounts(checkpoint.Committed), AcquiredMints = checkpoint.AcquiredMints,
        PendingTier = checkpoint.PendingTier is null ? null : (int)checkpoint.PendingTier.Value,
        PendingQuote = checkpoint.PendingQuote is null ? null : Amounts(checkpoint.PendingQuote),
        PendingPaid = checkpoint.PendingPaid is null ? null : Amounts(checkpoint.PendingPaid),
        RealClockCredits = checkpoint.RealClockCredits is null ? null : Amounts(checkpoint.RealClockCredits),
        RealClockDebits = checkpoint.RealClockDebits is null ? null : Amounts(checkpoint.RealClockDebits),
        MintOrigin = checkpoint.MintOrigin,
        CompletedMints = checkpoint.CompletedMints.IsDefault
            ? null : checkpoint.CompletedMints.Select(receipt => new CompletedMintDto
            {
                Tier = (int)receipt.Tier, PreviouslyAcquired = receipt.PreviouslyAcquired,
                Quote = Amounts(receipt.Quote)
            }).ToList()
    });

    internal static E4bFundingCheckpoint Decode(string json)
    {
        CheckpointDto dto;
        try { dto = JsonSerializer.Deserialize<CheckpointDto>(json) ??
            throw new InvalidDataException("Empty financing checkpoint."); }
        catch (JsonException e) { throw new InvalidDataException("Invalid financing checkpoint JSON.", e); }
        return new(dto.Version, FromAmounts(dto.InitialStock), FromAmounts(dto.Stock),
            FromAmounts(dto.Capacity), FromAmounts(dto.HypotheticalDelivered),
            FromAmounts(dto.Committed), dto.AcquiredMints,
            dto.PendingTier is null ? null : (ProductionTier)dto.PendingTier.Value,
            dto.PendingQuote is null ? null : FromAmounts(dto.PendingQuote),
            dto.PendingPaid is null ? null : FromAmounts(dto.PendingPaid),
            dto.RealClockCredits is null ? null : FromAmounts(dto.RealClockCredits),
            dto.RealClockDebits is null ? null : FromAmounts(dto.RealClockDebits),
            dto.MintOrigin, dto.CompletedMints is null ? default :
                dto.CompletedMints.Select(item => new E4bCompletedMint((ProductionTier)item.Tier,
                    item.PreviouslyAcquired, FromAmounts(item.Quote))).ToImmutableArray());
    }
}
