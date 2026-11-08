using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Infrastructure.Experimental;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class Rc104UnifiedSavePreflightTests
{
    private sealed class Files : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(),
            "simrisorse-rc104-" + Guid.NewGuid().ToString("N"));
        public string Active => Path.Combine(Root, "active.sqlite3");
        public string Backup => Path.Combine(Root, "backup.sqlite3");
        public string Registry => Path.Combine(Root, "authority.sqlite3");
        public string OtherRegistry => Path.Combine(Root, "other-registry.sqlite3");
        public Files() => Directory.CreateDirectory(Root);
        public void Dispose() => Directory.Delete(Root, recursive: true);
    }

    private static E4bV15UnifiedSavePreflight Preflight(V15BenchmarkFixture fx) =>
        new(fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15FundingCompositeSqliteStore Store(Files f, V15BenchmarkFixture fx) =>
        new(f.Active, fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15FundingAuthorityRegistry Registry(Files f, V15BenchmarkFixture fx) =>
        new(f.Registry, fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15FundingCompositeRecovery Recovery(V15BenchmarkFixture fx) =>
        new(fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static void Sql(string path, string statement)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = path, Pooling = false }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = statement;
        command.ExecuteNonQuery();
    }

    [Fact]
    public void MissingSourceIsNeverCreatedByPreflight()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Assert.Throws<FileNotFoundException>(() => Preflight(fx).Analyze(f.Active));
        Assert.False(File.Exists(f.Active));
    }

    [Fact]
    public void NativeGeneration6IsRejectedRatherThanInterpretedAsEscrow()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        new V15SqliteFoodSaveStore(f.Active, fx.Baseline, fx.School,
            fx.Technologies, fx.Depot).Save(fx.Start);
        Assert.Throws<InvalidDataException>(() => Preflight(fx).Analyze(f.Active));
        Assert.NotNull(new V15SqliteFoodSaveStore(f.Active, fx.Baseline, fx.School,
            fx.Technologies, fx.Depot).Load());
    }

    [Fact]
    public void UnrelatedSQLiteDatabaseIsNotChangedOrAccepted()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Sql(f.Active, "CREATE TABLE unrelated (id INTEGER PRIMARY KEY);");
        Assert.Throws<InvalidDataException>(() => Preflight(fx).Analyze(f.Active));
        using var db = new SqliteConnection($"Data Source={f.Active};Mode=ReadOnly;Pooling=False");
        db.Open();
        using var query = db.CreateCommand();
        query.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE name='unrelated';";
        Assert.Equal(1L, Convert.ToInt64(query.ExecuteScalar()));
    }

    [Fact]
    public void UnregisteredValidCompositeIsOnlyEligibleForLegacyExport()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f, fx).Create(fx.Start);
        var report = Preflight(fx).Analyze(f.Active);
        Assert.False(report.EligibleForFutureUnifiedMigration);
        Assert.True(report.EligibleForGeneration6Export);
        Assert.Contains(E4bV15UnifiedSaveBlocker.NotRegistered, report.Blockers);
        Assert.NotNull(report.CampaignId);
        Assert.False(report.HasPendingFinancing);
        Assert.Equal(0L, report.Revision);
    }

    [Fact]
    public void LegacyUnregisteredFileWithoutIdentityIsNotSilentlyMigrated()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f, fx).Create(fx.Start);
        Sql(f.Active, "DROP TABLE composite_campaign_identity;");
        var report = Preflight(fx).Analyze(f.Active);
        Assert.Null(report.CampaignId);
        Assert.Contains(E4bV15UnifiedSaveBlocker.MissingCampaignIdentity, report.Blockers);
        Assert.Contains(E4bV15UnifiedSaveBlocker.NotRegistered, report.Blockers);
        Assert.False(report.EligibleForFutureUnifiedMigration);
        Assert.True(report.EligibleForGeneration6Export);
        Assert.Null(Store(f, fx).ReadCampaignId()); // No hidden ID migration during inspection.
    }

    [Fact]
    public void OpenQuotationWithoutDepositBlocksLegacyExport()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        var store = Store(f, fx);
        store.Create(fx.Start);
        Assert.Equal(E4bFundingApplyStatus.Applied, store.Apply("begin", 0,
            new(E4bFundingAction.BeginMint, ProductionTier.Base)).Status);
        var report = Preflight(fx).Analyze(f.Active);
        Assert.True(report.HasPendingFinancing);
        Assert.Equal(1L, report.Revision);
        Assert.False(report.EligibleForGeneration6Export);
        Assert.Contains(E4bV15UnifiedSaveBlocker.PendingFinancingBlocksLegacyExport, report.Blockers);
    }

    [Fact]
    public void MaterialsDepositedInEscrowBlockLegacyExportAndArePreserved()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        var store = Store(f, fx);
        store.Create(fx.Start);
        store.Apply("begin", 0, new(E4bFundingAction.BeginMint, ProductionTier.Base));
        var paid = ResourceBundle.Create(("planks", 1m));
        Assert.Equal(E4bFundingApplyStatus.Applied, store.Apply("deposit", 1,
            new(E4bFundingAction.Deposit, Materials: paid)).Status);
        var report = Preflight(fx).Analyze(f.Active);
        Assert.Equal(paid, report.Escrowed);
        Assert.False(report.EligibleForGeneration6Export);
        Assert.Equal(2L, Store(f, fx).Load().Revision);
    }

    [Fact]
    public void RegisteredCompositeRequiresRegistryForMigrationAssessment()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f, fx).Create(fx.Start);
        Registry(f, fx).RegisterInitial(f.Active);
        var report = Preflight(fx).Analyze(f.Active);
        Assert.False(report.EligibleForFutureUnifiedMigration);
        Assert.False(report.EligibleForGeneration6Export);
        Assert.Contains(E4bV15UnifiedSaveBlocker.RegistryVerificationRequired, report.Blockers);
    }

    [Fact]
    public void RegisteredActiveBranchPassesFutureMigrationPreconditions()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        registry.RegisterInitial(f.Active);
        var report = Preflight(fx).Analyze(f.Active, f.Registry);
        Assert.True(report.EligibleForFutureUnifiedMigration);
        Assert.True(report.EligibleForGeneration6Export);
        Assert.Empty(report.Blockers);
        Assert.Equal(registry.CurrentCampaignId(), report.CampaignId);
    }

    [Fact]
    public void PendingFundingOnAuthoritativeBranchMayBeMigratedButNotExportedToLegacy()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var lease = registry.RegisterInitial(f.Active);
        Assert.Equal(E4bFundingApplyStatus.Applied, registry.Apply(lease, "begin", 0,
            new(E4bFundingAction.BeginMint, ProductionTier.Base)).Status);
        var report = Preflight(fx).Analyze(f.Active, f.Registry);
        Assert.True(report.EligibleForFutureUnifiedMigration);
        Assert.False(report.EligibleForGeneration6Export);
        Assert.Contains(E4bV15UnifiedSaveBlocker.PendingFinancingBlocksLegacyExport, report.Blockers);
    }

    [Fact]
    public void RestoredBackupIsNotMistakenForActiveBranch()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f, fx).Create(fx.Start);
        Registry(f, fx).RegisterInitial(f.Active);
        Recovery(fx).BackupToNewFile(f.Active, f.Backup);
        var report = Preflight(fx).Analyze(f.Backup, f.Registry);
        Assert.False(report.EligibleForFutureUnifiedMigration);
        Assert.False(report.EligibleForGeneration6Export);
        Assert.Contains(E4bV15UnifiedSaveBlocker.InactiveBranch, report.Blockers);
        Assert.Equal(Store(f, fx).ReadCampaignId(), report.CampaignId);
    }

    [Fact]
    public void RegistryPathMismatchIsReportedWithoutCreatingFiles()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f, fx).Create(fx.Start);
        Registry(f, fx).RegisterInitial(f.Active);
        var report = Preflight(fx).Analyze(f.Active, f.OtherRegistry);
        Assert.Contains(E4bV15UnifiedSaveBlocker.ForeignAuthorityRegistry, report.Blockers);
        Assert.False(File.Exists(f.OtherRegistry));
    }

    [Fact]
    public void MissingRegistryFileIsReportedWithoutRecreatingAuthority()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f, fx).Create(fx.Start);
        Registry(f, fx).RegisterInitial(f.Active);
        File.Delete(f.Registry);
        var report = Preflight(fx).Analyze(f.Active, f.Registry);
        Assert.Contains(E4bV15UnifiedSaveBlocker.AuthorityRegistryMissing, report.Blockers);
        Assert.False(File.Exists(f.Registry));
    }

    [Fact]
    public void AuthorityCampaignMismatchBlocksMigrationWithoutWriting()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f, fx).Create(fx.Start);
        Registry(f, fx).RegisterInitial(f.Active);
        Sql(f.Registry, $"UPDATE authority_campaign_identity SET campaign_id='{Guid.NewGuid():N}' WHERE slot=1;");
        var report = Preflight(fx).Analyze(f.Active, f.Registry);
        Assert.False(report.EligibleForFutureUnifiedMigration);
        Assert.Contains(E4bV15UnifiedSaveBlocker.ForeignCampaignIdentity, report.Blockers);
    }

    [Fact]
    public void MissingMandatoryTriggerBlocksMigration()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f, fx).Create(fx.Start);
        Registry(f, fx).RegisterInitial(f.Active);
        Sql(f.Active, "DROP TRIGGER rc103_guard_identity_delete;");
        var report = Preflight(fx).Analyze(f.Active, f.Registry);
        Assert.False(report.EligibleForFutureUnifiedMigration);
        Assert.Contains(E4bV15UnifiedSaveBlocker.IncompleteAuthorityFence, report.Blockers);
    }
}
