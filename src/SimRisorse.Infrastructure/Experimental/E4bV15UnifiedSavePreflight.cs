using Microsoft.Data.Sqlite;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;

namespace SimRisorse.Infrastructure.Experimental;

/// <summary>
/// RC104: evidence required BEFORE attempting a future unified playable save.
/// This inspector never promotes a branch, converts a save or charges materials.
/// Passing the preflight does NOT mean that a generation-7 writer exists yet.
/// </summary>
public sealed class E4bV15UnifiedSavePreflight
{
    private const int AuthorityTriggers = 8;
    private const int IdentityTriggers = 3;
    private readonly GameScenario _baseline;
    private readonly SchoolResearchRules _school;
    private readonly TechnologyCatalog _technologies;
    private readonly FoodConversionPolicy _food;
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    public E4bV15UnifiedSavePreflight(GameScenario baseline, SchoolResearchRules school,
        TechnologyCatalog technologies, FoodConversionPolicy food)
    {
        _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        _school = school ?? throw new ArgumentNullException(nameof(school));
        _technologies = technologies ?? throw new ArgumentNullException(nameof(technologies));
        _food = food ?? throw new ArgumentNullException(nameof(food));
    }

    /// <summary>
    /// Inspect the complete composite, its receipts, optional authority registry
    /// and pending escrow. Paths are inspected, never created or replaced.
    /// A registered composite is eligible for a future native migration ONLY if
    /// it is the presently authoritative branch and its fence is intact.
    /// </summary>
    public E4bV15UnifiedSavePreflightReport Analyze(string compositePath,
        string? authorityRegistryPath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(compositePath);
        var path = Path.GetFullPath(compositePath);
        if (!File.Exists(path))
            throw new FileNotFoundException("The composite to inspect does not exist.", path);

        // First inspect as a read-only SQLite file. A native player save or an
        // unrelated SQLite database must never be interpreted as escrow data.
        using var db = OpenReadOnly(path);
        CheckHeader(db, E4bV15FundingCompositeSqliteStore.ApplicationId,
            E4bV15FundingCompositeSqliteStore.FileVersion);
        CheckIntegrity(db);

        // Existing RC95 validation checks every native v15 field, escrow balance
        // and the complete revision/receipt sequence. It does not create a state.
        var store = new E4bV15FundingCompositeSqliteStore(path,
            _baseline, _school, _technologies, _food);
        var state = store.Load();
        var identity = store.ReadCampaignId();
        var blockers = new List<E4bV15UnifiedSaveBlocker>();
        if (identity is null) blockers.Add(E4bV15UnifiedSaveBlocker.MissingCampaignIdentity);

        string? registeredPath = null;
        using (var guard = Sql(db,
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='composite_authority_guard';"))
        {
            if (Convert.ToInt64(guard.ExecuteScalar()) == 1)
            {
                using var binding = Sql(db,
                    "SELECT registry_path FROM composite_authority_guard WHERE slot=1;");
                registeredPath = binding.ExecuteScalar() as string;
                if (string.IsNullOrWhiteSpace(registeredPath))
                    throw new InvalidDataException("The composite has no valid authority binding.");
                if (Count(db, "SELECT COUNT(*) FROM composite_authority_guard;") != 1 ||
                    Count(db, "SELECT COUNT(*) FROM sqlite_master WHERE type='trigger' AND name GLOB 'rc102_guard_*';") != AuthorityTriggers ||
                    Count(db, "SELECT COUNT(*) FROM sqlite_master WHERE type='trigger' AND name GLOB 'rc103_guard_*';") != IdentityTriggers)
                    blockers.Add(E4bV15UnifiedSaveBlocker.IncompleteAuthorityFence);
            }
        }

        if (registeredPath is null)
        {
            blockers.Add(E4bV15UnifiedSaveBlocker.NotRegistered);
        }
        else if (string.IsNullOrWhiteSpace(authorityRegistryPath))
        {
            blockers.Add(E4bV15UnifiedSaveBlocker.RegistryVerificationRequired);
        }
        else
        {
            var registry = Path.GetFullPath(authorityRegistryPath);
            if (!string.Equals(registeredPath, registry, PathComparison))
                blockers.Add(E4bV15UnifiedSaveBlocker.ForeignAuthorityRegistry);
            else if (!File.Exists(registry))
                blockers.Add(E4bV15UnifiedSaveBlocker.AuthorityRegistryMissing);
            else
            {
                using var authority = OpenReadOnly(registry);
                CheckHeader(authority, E4bV15FundingAuthorityRegistry.ApplicationId,
                    E4bV15FundingAuthorityRegistry.FileVersion);
                CheckIntegrity(authority);
                if (Count(authority, "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='authority_campaign_identity';") != 1 ||
                    Count(authority, "SELECT COUNT(*) FROM authority_state;") != 1 ||
                    Count(authority, "SELECT COUNT(*) FROM authority_campaign_identity;") != 1)
                    throw new InvalidDataException("The authority registry is missing required state.");
                string activePath;
                string registryIdentity;
                using (var active = Sql(authority, "SELECT active_path FROM authority_state WHERE slot=1;"))
                    activePath = active.ExecuteScalar() as string ?? throw new InvalidDataException("The registry has no active path.");
                using (var campaign = Sql(authority,
                    "SELECT campaign_id FROM authority_campaign_identity WHERE slot=1;"))
                    registryIdentity = campaign.ExecuteScalar() as string ?? throw new InvalidDataException("The registry has no campaign identity.");
                if (!string.Equals(identity, registryIdentity, StringComparison.Ordinal))
                    blockers.Add(E4bV15UnifiedSaveBlocker.ForeignCampaignIdentity);
                if (!string.Equals(activePath, path, PathComparison))
                    blockers.Add(E4bV15UnifiedSaveBlocker.InactiveBranch);
            }
        }

        var pending = state.IsFunding || !state.Funding.Escrowed.Equals(ResourceBundle.Empty);
        var canMigrateInFuture = blockers.Count == 0;
        // Generation 6 cannot represent an open quotation or already-paid
        // materials. Unlike the future unified format, exporting it would lose
        // escrow and the receipt history. RC99 owns that separate export path.
        var canExportLegacy = !pending && blockers.All(x =>
            x is E4bV15UnifiedSaveBlocker.NotRegistered or
                 E4bV15UnifiedSaveBlocker.MissingCampaignIdentity);
        if (pending) blockers.Add(E4bV15UnifiedSaveBlocker.PendingFinancingBlocksLegacyExport);

        return new E4bV15UnifiedSavePreflightReport(path, identity, state.Revision,
            state.IsFunding, state.Funding.Escrowed, canMigrateInFuture,
            canExportLegacy, blockers.AsReadOnly());
    }

    private static SqliteConnection OpenReadOnly(string path)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path, Mode = SqliteOpenMode.ReadOnly, Pooling = false,
            ForeignKeys = true, DefaultTimeout = 5
        }.ToString());
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    private static SqliteCommand Sql(SqliteConnection db, string sql)
    {
        var command = db.CreateCommand();
        command.CommandText = sql;
        return command;
    }

    private static long Count(SqliteConnection db, string sql)
    {
        using var command = Sql(db, sql);
        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static void CheckHeader(SqliteConnection db, int id, int version)
    {
        if (Count(db, "PRAGMA application_id;") != id ||
            Count(db, "PRAGMA user_version;") != version)
            throw new InvalidDataException("Not a supported experimental funding database.");
    }

    private static void CheckIntegrity(SqliteConnection db)
    {
        using var command = Sql(db, "PRAGMA integrity_check;");
        if (command.ExecuteScalar() is not string result || result != "ok")
            throw new InvalidDataException("SQLite integrity_check failed.");
    }
}

public enum E4bV15UnifiedSaveBlocker
{
    MissingCampaignIdentity,
    NotRegistered,
    RegistryVerificationRequired,
    ForeignAuthorityRegistry,
    AuthorityRegistryMissing,
    ForeignCampaignIdentity,
    InactiveBranch,
    IncompleteAuthorityFence,
    PendingFinancingBlocksLegacyExport
}

/// <summary>A future-migration preflight, never a completed migration receipt.</summary>
public sealed record E4bV15UnifiedSavePreflightReport(
    string CompositePath,
    string? CampaignId,
    long Revision,
    bool HasPendingFinancing,
    ResourceBundle Escrowed,
    bool EligibleForFutureUnifiedMigration,
    bool EligibleForGeneration6Export,
    IReadOnlyList<E4bV15UnifiedSaveBlocker> Blockers);
