using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;

namespace SimRisorse.Infrastructure.Experimental;

/// <summary>
/// RC102 authority coordination for one experimental composite lineage.
/// Registered composite files carry SQLite write triggers that reject direct
/// mutations, even through legacy store objects. The shared registry keeps
/// its cross-process SQLite write lock for the entire authorized store commit.
/// Unregistered legacy laboratory files and normal player saves are unaffected.
/// This is a safety protocol, NOT a security boundary against SQL schema edits.
/// </summary>
public sealed class E4bV15FundingAuthorityRegistry
{
    public const int ApplicationId = 0x53465241; // SFRA (not an RC95 or native v15 file)
    public const int FileVersion = 1;

    private readonly string _path;
    private readonly GameScenario _baseline;
    private readonly SchoolResearchRules _school;
    private readonly TechnologyCatalog _technologies;
    private readonly FoodConversionPolicy _food;
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    public E4bV15FundingAuthorityRegistry(string registryPath, GameScenario baseline,
        SchoolResearchRules school, TechnologyCatalog technologies, FoodConversionPolicy food)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registryPath);
        _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        _school = school ?? throw new ArgumentNullException(nameof(school));
        _technologies = technologies ?? throw new ArgumentNullException(nameof(technologies));
        _food = food ?? throw new ArgumentNullException(nameof(food));
        _path = Path.GetFullPath(registryPath);
        if (!Directory.Exists(Path.GetDirectoryName(_path)))
            throw new DirectoryNotFoundException("Create the authority registry directory first.");

        // Inspect existing files read-only before any write-capable connection.
        if (File.Exists(_path))
        {
            using var probe = Open(SqliteOpenMode.ReadOnly);
            if (Scalar(probe, null, "PRAGMA application_id;") != ApplicationId ||
                Scalar(probe, null, "PRAGMA user_version;") != FileVersion ||
                Scalar(probe, null,
                    "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='authority_state';") != 1)
                throw new InvalidDataException("Not an RC101 funding authority registry.");
        }
        using var db = Open(SqliteOpenMode.ReadWriteCreate);
        using var tx = db.BeginTransaction();
        var id = Scalar(db, tx, "PRAGMA application_id;");
        var version = Scalar(db, tx, "PRAGMA user_version;");
        if (id == 0 && version == 0)
        {
            if (Scalar(db, tx,
                "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';") != 0)
                throw new InvalidDataException("Refusing to initialize an unrelated SQLite database.");
            Execute(db, tx, $"PRAGMA application_id = {ApplicationId};");
            Execute(db, tx, $"PRAGMA user_version = {FileVersion};");
            Execute(db, tx, """
                CREATE TABLE authority_state (
                    slot INTEGER PRIMARY KEY CHECK(slot = 1),
                    epoch INTEGER NOT NULL CHECK(epoch >= 1),
                    active_path TEXT NOT NULL,
                    token TEXT NOT NULL
                );
                CREATE TABLE authority_campaign_identity (
                    slot INTEGER PRIMARY KEY CHECK(slot=1),
                    campaign_id TEXT NOT NULL
                );
                """);
        }
        else if (id != ApplicationId || version != FileVersion ||
            Scalar(db, tx,
                "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='authority_state';") != 1)
            throw new InvalidDataException("Invalid RC101 authority registry schema.");
        // Add a campaign binding to RC101/RC102 registries without changing
        // the application ID or breaking existing cooperative leases.
        if (Scalar(db, tx, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='authority_campaign_identity';") == 0)
            Execute(db, tx, "CREATE TABLE authority_campaign_identity (slot INTEGER PRIMARY KEY CHECK(slot=1), campaign_id TEXT NOT NULL);");
        // Upgrade an existing RC101 registry in place: fence its active
        // composite before releasing the shared registry write lock. This also
        // rejects a copied registry whose new path disagrees with the binding.
        if (Scalar(db, tx, "SELECT COUNT(*) FROM authority_state WHERE slot=1;") == 1)
        {
            var active = Read(db, tx);
            var registeredId = ReadRegisteredCampaignId(db, tx);
            if (!File.Exists(active.ActivePath))
                throw new FileNotFoundException("The authoritative composite is missing.", active.ActivePath);
            var beforeFence = Store(active.ActivePath).ReadCampaignId();
            if (registeredId is not null &&
                !string.Equals(registeredId, beforeFence, StringComparison.Ordinal))
                throw new InvalidDataException("Active composite belongs to another campaign identity.");
            Store(active.ActivePath).FenceForAuthority(_path);
            _ = Store(active.ActivePath).Load();
            var campaignId = RequireCampaignId(active.ActivePath);
            if (registeredId is null)
                BindCampaignId(db, tx, campaignId);
            else if (!string.Equals(registeredId, campaignId, StringComparison.Ordinal))
                throw new InvalidDataException("Active composite belongs to another campaign identity.");
        }
        tx.Commit();
    }

    /// <summary>Register one already-created, valid experimental composite file.
    /// A second registration cannot replace the active branch.</summary>
    public E4bV15FundingAuthorityLease RegisterInitial(string compositePath)
    {
        var path = Canonical(compositePath);
        if (!File.Exists(path))
            throw new FileNotFoundException("The source composite does not exist.", path);
        _ = Store(path).Load();
        using var db = Open(SqliteOpenMode.ReadWrite);
        using var tx = db.BeginTransaction();
        // A registered composite becomes write-fenced before publication.
        // If registry insertion fails, a fenced but inactive file fails closed.
        Store(path).FenceForAuthority(_path);
        var campaignId = RequireCampaignId(path);
        var lease = new E4bV15FundingAuthorityLease(1, path, NewToken());
        using (var command = Sql(db, tx,
            "INSERT INTO authority_state(slot,epoch,active_path,token) VALUES (1,$epoch,$path,$token);"))
        {
            command.Parameters.AddWithValue("$epoch", lease.Epoch);
            command.Parameters.AddWithValue("$path", lease.ActivePath);
            command.Parameters.AddWithValue("$token", lease.Token);
            command.ExecuteNonQuery();
        }
        BindCampaignId(db, tx, campaignId);
        tx.Commit();
        return lease;
    }

    /// <summary>Return the active branch after a process restart. This is a
    /// cooperative fencing token, not a secret or a security credential.</summary>
    public E4bV15FundingAuthorityLease Current()
    {
        using var db = Open(SqliteOpenMode.ReadOnly);
        var lease = Read(db, null);
        VerifyActiveCampaignId(db, null, lease.ActivePath);
        return lease;
    }

    /// <summary>The stable experimental campaign identifier stays the same
    /// across backup, restore, authority promotion and process restart.</summary>
    public string CurrentCampaignId()
    {
        using var db = Open(SqliteOpenMode.ReadOnly);
        var lease = Read(db, null);
        return VerifyActiveCampaignId(db, null, lease.ActivePath);
    }

    /// <summary>Process a funding operation only on the current active branch,
    /// with the authority SQLite write lock held for the entire store commit.</summary>
    public E4bV15FundingCompositeResult Apply(E4bV15FundingAuthorityLease lease,
        string requestId, long expectedRevision, E4bFundingInstruction instruction) =>
        WithActive(lease, path => Store(path).ApplyAuthorized(_path,
            requestId, expectedRevision, instruction));

    /// <summary>Advance the real v15 clock on the active branch only.</summary>
    public E4bV15FundingCompositeResult AdvanceClock(E4bV15FundingAuthorityLease lease,
        string requestId, long expectedRevision, MeatChainPolicy meat, HorticulturePolicy crops) =>
        WithActive(lease, path => Store(path).AdvanceClockAuthorized(_path,
            requestId, expectedRevision, meat, crops));

    /// <summary>Transfer authority to a restored copy with exactly the same
    /// city image, funding ledger AND receipts as the current branch. Any old
    /// lease is permanently fenced after this commit. Backups are never
    /// automatically promoted; divergence cannot be silently discarded.</summary>
    public E4bV15FundingAuthorityLease Promote(E4bV15FundingAuthorityLease lease,
        string restoredCompositePath)
    {
        var candidate = Canonical(restoredCompositePath);
        if (!File.Exists(candidate))
            throw new FileNotFoundException("The candidate composite does not exist.", candidate);
        using var db = Open(SqliteOpenMode.ReadWrite);
        using var tx = db.BeginTransaction();
        var active = RequireCurrent(db, tx, lease);
        var campaignId = VerifyActiveCampaignId(db, tx, active.ActivePath);
        if (string.Equals(active.ActivePath, candidate, PathComparison))
            throw new ArgumentException("Promotion requires a different composite file.",
                nameof(restoredCompositePath));
        if (active.Epoch == long.MaxValue)
            throw new InvalidOperationException("Authority epoch overflow.");

        // Serialize with all cooperating writes while comparing the entire
        // durable content, not just a revision number or a file timestamp.
        _ = Store(active.ActivePath).Load();
        _ = Store(candidate).Load();
        if (!string.Equals(campaignId, Store(candidate).ReadCampaignId(), StringComparison.Ordinal))
            throw new InvalidOperationException(
                "Cannot promote a different campaign or a pre-identity backup.");
        var sourceDigest = SnapshotDigest(active.ActivePath);
        var candidateDigest = SnapshotDigest(candidate);
        if (!CryptographicOperations.FixedTimeEquals(sourceDigest, candidateDigest))
            throw new InvalidOperationException(
                "The restored branch has diverged from the active checkpoint. Do not discard committed operations.");
        // A copy restored from an unfenced legacy composite must be fenced
        // BEFORE it becomes active. Re-check the digest after fencing so a
        // concurrent unguarded writer cannot change the candidate in between.
        Store(candidate).FenceForAuthority(_path);
        if (!CryptographicOperations.FixedTimeEquals(sourceDigest, SnapshotDigest(candidate)) ||
            !string.Equals(campaignId, Store(candidate).ReadCampaignId(), StringComparison.Ordinal))
            throw new InvalidOperationException("Candidate changed during authority promotion.");

        var promoted = new E4bV15FundingAuthorityLease(active.Epoch + 1, candidate, NewToken());
        using (var command = Sql(db, tx, """
            UPDATE authority_state SET epoch=$epoch, active_path=$path, token=$token
            WHERE slot=1 AND epoch=$oldEpoch AND token=$oldToken;
            """))
        {
            command.Parameters.AddWithValue("$epoch", promoted.Epoch);
            command.Parameters.AddWithValue("$path", promoted.ActivePath);
            command.Parameters.AddWithValue("$token", promoted.Token);
            command.Parameters.AddWithValue("$oldEpoch", active.Epoch);
            command.Parameters.AddWithValue("$oldToken", active.Token);
            if (command.ExecuteNonQuery() != 1)
                throw new InvalidOperationException("Authority was concurrently changed.");
        }
        tx.Commit();
        return promoted;
    }

    private T WithActive<T>(E4bV15FundingAuthorityLease lease, Func<string, T> write)
    {
        ArgumentNullException.ThrowIfNull(write);
        using var db = Open(SqliteOpenMode.ReadWrite);
        using var tx = db.BeginTransaction();
        var active = RequireCurrent(db, tx, lease);
        VerifyActiveCampaignId(db, tx, active.ActivePath);
        var result = write(active.ActivePath);
        tx.Commit();
        return result;
    }

    private static E4bV15FundingAuthorityLease RequireCurrent(SqliteConnection db,
        SqliteTransaction tx, E4bV15FundingAuthorityLease lease)
    {
        ArgumentNullException.ThrowIfNull(lease);
        var active = Read(db, tx);
        if (lease.Epoch != active.Epoch || lease.Token != active.Token ||
            !string.Equals(lease.ActivePath, active.ActivePath, PathComparison))
            throw new InvalidOperationException("Stale or foreign authority lease. No write was attempted.");
        return active;
    }

    private static E4bV15FundingAuthorityLease Read(SqliteConnection db, SqliteTransaction? tx)
    {
        using var command = Sql(db, tx,
            "SELECT epoch,active_path,token FROM authority_state WHERE slot=1;");
        using var reader = command.ExecuteReader();
        if (!reader.Read()) throw new InvalidOperationException("No authoritative experimental branch registered.");
        var lease = new E4bV15FundingAuthorityLease(reader.GetInt64(0),
            reader.GetString(1), reader.GetString(2));
        if (reader.Read() || lease.Epoch < 1 || string.IsNullOrWhiteSpace(lease.Token) ||
            !Path.IsPathFullyQualified(lease.ActivePath))
            throw new InvalidDataException("Invalid experimental authority state.");
        return lease;
    }

    private static byte[] SnapshotDigest(string path)
    {
        using var db = OpenPath(path, SqliteOpenMode.ReadOnly);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        // Domain-separate branches: identical stock and receipts from two
        // independently created campaigns must not make them interchangeable.
        using (var command = db.CreateCommand())
        {
            command.CommandText = "SELECT campaign_id FROM composite_campaign_identity WHERE slot=1;";
            if (command.ExecuteScalar() is not string id ||
                !Guid.TryParseExact(id, "N", out var parsed) || parsed.ToString("N") != id)
                throw new InvalidDataException("Missing or invalid stable campaign identity.");
            AppendBytes(hash, Encoding.UTF8.GetBytes(id));
        }
        using (var command = db.CreateCommand())
        {
            command.CommandText = "SELECT revision,native_v15_image,funding_json FROM composite_state WHERE slot=1;";
            using var reader = command.ExecuteReader();
            if (!reader.Read()) throw new InvalidDataException("Missing active composite checkpoint.");
            AppendNumber(hash, reader.GetInt64(0));
            AppendBytes(hash, reader.GetFieldValue<byte[]>(1));
            AppendBytes(hash, Encoding.UTF8.GetBytes(reader.GetString(2)));
            if (reader.Read()) throw new InvalidDataException("Duplicate composite checkpoint.");
        }
        using (var command = db.CreateCommand())
        {
            command.CommandText = "SELECT request_id,payload,committed_revision FROM composite_receipt ORDER BY committed_revision;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                AppendBytes(hash, Encoding.UTF8.GetBytes(reader.GetString(0)));
                AppendBytes(hash, Encoding.UTF8.GetBytes(reader.GetString(1)));
                AppendNumber(hash, reader.GetInt64(2));
            }
        }
        return hash.GetHashAndReset();
    }

    private static void AppendNumber(IncrementalHash hash, long number)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, number);
        hash.AppendData(bytes);
    }

    private static void AppendBytes(IncrementalHash hash, byte[] bytes)
    {
        AppendNumber(hash, bytes.LongLength);
        hash.AppendData(bytes);
    }

    private E4bV15FundingCompositeSqliteStore Store(string path) =>
        new(path, _baseline, _school, _technologies, _food);

    private string RequireCampaignId(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("The authoritative composite is missing.", path);
        return Store(path).ReadCampaignId() ?? throw new InvalidDataException(
            "An authoritative composite must have a stable campaign identity.");
    }

    private string VerifyActiveCampaignId(SqliteConnection db, SqliteTransaction? tx, string activePath)
    {
        var expected = ReadRegisteredCampaignId(db, tx) ??
            throw new InvalidDataException("The authority registry has no campaign binding.");
        var actual = RequireCampaignId(activePath);
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
            throw new InvalidDataException("Active composite identity differs from its authority registry.");
        return expected;
    }

    private static string? ReadRegisteredCampaignId(SqliteConnection db, SqliteTransaction? tx)
    {
        string id;
        using (var command = Sql(db, tx,
            "SELECT campaign_id FROM authority_campaign_identity WHERE slot=1;"))
        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read()) return null;
            id = reader.GetString(0);
            if (reader.Read()) throw new InvalidDataException("Duplicated registry campaign binding.");
        }
        if (!Guid.TryParseExact(id, "N", out var parsed) ||
            !string.Equals(parsed.ToString("N"), id, StringComparison.Ordinal) ||
            Scalar(db, tx, "SELECT COUNT(*) FROM authority_campaign_identity;") != 1)
            throw new InvalidDataException("Invalid experimental registry campaign binding.");
        return id;
    }

    private static void BindCampaignId(SqliteConnection db, SqliteTransaction tx, string campaignId)
    {
        using var command = Sql(db, tx,
            "INSERT INTO authority_campaign_identity(slot,campaign_id) VALUES(1,$id);");
        command.Parameters.AddWithValue("$id", campaignId);
        command.ExecuteNonQuery();
    }

    private static string Canonical(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Path.GetFullPath(path);
    }

    private static string NewToken() => Guid.NewGuid().ToString("N");

    private SqliteConnection Open(SqliteOpenMode mode) => OpenPath(_path, mode);

    private static SqliteConnection OpenPath(string path, SqliteOpenMode mode)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path, Mode = mode, Pooling = false,
            ForeignKeys = true, DefaultTimeout = 5
        }.ToString());
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    private static SqliteCommand Sql(SqliteConnection db, SqliteTransaction? tx, string sql)
    {
        var cmd = db.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        return cmd;
    }

    private static long Scalar(SqliteConnection db, SqliteTransaction? tx, string sql)
    {
        using var command = Sql(db, tx, sql);
        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static void Execute(SqliteConnection db, SqliteTransaction tx, string sql)
    {
        using var command = Sql(db, tx, sql);
        command.ExecuteNonQuery();
    }
}

/// <summary>Cooperative generation fence. Token is deliberately NOT an access-control secret.</summary>
public sealed record E4bV15FundingAuthorityLease(long Epoch, string ActivePath, string Token);
