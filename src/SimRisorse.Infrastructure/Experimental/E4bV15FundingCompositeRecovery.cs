using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Content;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;

namespace SimRisorse.Infrastructure.Experimental;

/// <summary>
/// RC100 opt-in recovery for the isolated composite city-and-escrow file.
/// SQLite's online backup API copies one consistent database snapshot even
/// while another connection is working. The staged copy is validated before
/// publishing a NEW file. Existing saves/backups are never replaced.
/// A restored file is an independent branch; it is not automatically promoted
/// to the playable v15 save, and two branches must not be merged.
/// </summary>
public sealed class E4bV15FundingCompositeRecovery
{
    private readonly GameScenario _baseline;
    private readonly SchoolResearchRules _school;
    private readonly TechnologyCatalog _technologies;
    private readonly FoodConversionPolicy _food;

    public E4bV15FundingCompositeRecovery(GameScenario baseline,
        SchoolResearchRules school, TechnologyCatalog technologies, FoodConversionPolicy food)
    {
        _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        _school = school ?? throw new ArgumentNullException(nameof(school));
        _technologies = technologies ?? throw new ArgumentNullException(nameof(technologies));
        _food = food ?? throw new ArgumentNullException(nameof(food));
    }

    /// <summary>Create a consistent backup, including pending escrow and all
    /// idempotency receipts. Source is opened read-only; target must not exist.</summary>
    public E4bV15FundingShadow BackupToNewFile(string compositePath, string backupPath) =>
        CopyVerified(compositePath, backupPath);

    /// <summary>Recover to a NEW experimental file. The backup stays intact;
    /// neither a native v15 player save nor an existing composite is overwritten.</summary>
    public E4bV15FundingShadow RestoreToNewFile(string backupPath, string newCompositePath) =>
        CopyVerified(backupPath, newCompositePath);

    private E4bV15FundingShadow CopyVerified(string from, string to)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(from);
        ArgumentException.ThrowIfNullOrWhiteSpace(to);
        var source = Path.GetFullPath(from);
        var target = Path.GetFullPath(to);
        if (string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Source and destination must be different files.", nameof(to));
        if (!File.Exists(source)) throw new FileNotFoundException("The source composite does not exist.", source);
        if (!Directory.Exists(Path.GetDirectoryName(target)))
            throw new DirectoryNotFoundException("Create the target directory before recovery.");
        if (File.Exists(target) || Directory.Exists(target))
            throw new IOException("Recovery never replaces an existing file.");

        var staging = Path.Combine(Path.GetDirectoryName(target)!, 
            ".rc100-recovery-" + Guid.NewGuid().ToString("N") + ".sqlite3");
        try
        {
            using (var reader = Open(source, SqliteOpenMode.ReadOnly))
            {
                CheckHeader(reader); // A normal player save is never a funding source.
                using var writer = Open(staging, SqliteOpenMode.ReadWriteCreate);
                reader.BackupDatabase(writer);
            }
            // Validate the actual backed-up snapshot (not a separate, potentially
            // newer read of the live source): city, escrow, revisions and receipts.
            var restored = Store(staging).Load();
            using (var staged = Open(staging, SqliteOpenMode.ReadOnly))
            using (var integrity = staged.CreateCommand())
            {
                integrity.CommandText = "PRAGMA integrity_check;";
                if (integrity.ExecuteScalar() is not string result || result != "ok")
                    throw new InvalidDataException("The composite SQLite backup failed integrity_check.");
            }
            File.Move(staging, target); // Never overwrite an existing player file.
            return restored;
        }
        finally
        {
            // Only our random staging path and its SQLite sidecars are removed.
            // Never touch the original composite, backup or existing target.
            foreach (var temporary in new[] { staging, staging + "-wal",
                staging + "-shm", staging + "-journal" })
            {
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (IOException) { /* Do not hide the original exception. */ }
                catch (UnauthorizedAccessException) { /* Do not hide the original exception. */ }
            }
        }
    }

    private E4bV15FundingCompositeSqliteStore Store(string path) =>
        new(path, _baseline, _school, _technologies, _food);

    private static SqliteConnection Open(string path, SqliteOpenMode mode)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path, Mode = mode, Pooling = false,
            ForeignKeys = true, DefaultTimeout = 5
        }.ToString());
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    private static void CheckHeader(SqliteConnection db)
    {
        using var app = db.CreateCommand();
        app.CommandText = "PRAGMA application_id;";
        using var version = db.CreateCommand();
        version.CommandText = "PRAGMA user_version;";
        if (Convert.ToInt64(app.ExecuteScalar()) != E4bV15FundingCompositeSqliteStore.ApplicationId ||
            Convert.ToInt64(version.ExecuteScalar()) != E4bV15FundingCompositeSqliteStore.FileVersion)
            throw new InvalidDataException("The source is not an RC95+ experimental composite save.");
    }
}
