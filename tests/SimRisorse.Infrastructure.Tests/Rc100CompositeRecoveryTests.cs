using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Infrastructure.Experimental;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class Rc100CompositeRecoveryTests
{
    private sealed class Files : IDisposable
    {
        public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(),
            "simrisorse-rc100-" + Guid.NewGuid().ToString("N"));
        public string Live => Path.Combine(DirectoryPath, "live.sqlite3");
        public string Backup => Path.Combine(DirectoryPath, "backup.sqlite3");
        public string Recovered => Path.Combine(DirectoryPath, "recovered.sqlite3");
        public Files() => Directory.CreateDirectory(DirectoryPath);
        public void Dispose() => Directory.Delete(DirectoryPath, recursive: true);
        public void AssertNoStagingFiles() => Assert.Empty(
            System.IO.Directory.GetFiles(DirectoryPath, ".rc100-recovery-*.sqlite3"));
    }

    private static E4bV15FundingCompositeSqliteStore Store(string path, V15BenchmarkFixture fx) =>
        new(path, fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15FundingCompositeRecovery Recovery(V15BenchmarkFixture fx) =>
        new(fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bFundingInstruction Begin() =>
        new(E4bFundingAction.BeginMint, ProductionTier.Base);

    private static E4bFundingInstruction Pay() =>
        new(E4bFundingAction.Deposit, Materials: ResourceBundle.Create(("planks", 1m)));

    [Fact]
    public void FreshCompositeBackupAndRestoreKeepBothSourcesUntouched()
    {
        var fx = V15BenchmarkFixture.Create();
        using var files = new Files();
        Store(files.Live, fx).Create(fx.Start);
        var sourceBefore = File.ReadAllBytes(files.Live);
        var backedUp = Recovery(fx).BackupToNewFile(files.Live, files.Backup);
        var backupBefore = File.ReadAllBytes(files.Backup);
        var restored = Recovery(fx).RestoreToNewFile(files.Backup, files.Recovered);
        Assert.Equal(0L, backedUp.Revision);
        Assert.Equal(0L, restored.Revision);
        Assert.Equal(fx.Start.CityCampaign.Resources, restored.Campaign.CityCampaign.Resources);
        Assert.Equal(sourceBefore, File.ReadAllBytes(files.Live));
        Assert.Equal(backupBefore, File.ReadAllBytes(files.Backup));
        Assert.Equal(fx.Start.CityCampaign.Resources, Store(files.Recovered, fx).Load().Funding.Stock);
        files.AssertNoStagingFiles();
    }

    [Fact]
    public void PendingDepositsSurviveBackupRestoreAndOriginalRemainsUnchanged()
    {
        var fx = V15BenchmarkFixture.Create();
        using var files = new Files();
        var store = Store(files.Live, fx);
        store.Create(fx.Start);
        Assert.Equal(E4bFundingApplyStatus.Applied, store.Apply("begin", 0, Begin()).Status);
        Assert.Equal(E4bFundingApplyStatus.Applied, store.Apply("payment", 1, Pay()).Status);
        var initialBytes = File.ReadAllBytes(files.Live);
        var backup = Recovery(fx).BackupToNewFile(files.Live, files.Backup);
        var recovered = Recovery(fx).RestoreToNewFile(files.Backup, files.Recovered);
        Assert.True(backup.IsFunding);
        Assert.True(recovered.IsFunding);
        Assert.Equal(2L, recovered.Revision);
        Assert.Equal(ResourceBundle.Create(("planks", 1m)), recovered.Funding.Escrowed);
        Assert.Equal(fx.Start.CityCampaign.Resources - recovered.Funding.Escrowed,
            recovered.Campaign.CityCampaign.Resources);
        Assert.Equal(initialBytes, File.ReadAllBytes(files.Live));
        files.AssertNoStagingFiles();
    }

    [Fact]
    public void RecoveredReceiptReplaysWithoutSecondPayment()
    {
        var fx = V15BenchmarkFixture.Create();
        using var files = new Files();
        var store = Store(files.Live, fx);
        store.Create(fx.Start);
        store.Apply("begin", 0, Begin());
        store.Apply("payment", 1, Pay());
        Recovery(fx).BackupToNewFile(files.Live, files.Backup);
        Recovery(fx).RestoreToNewFile(files.Backup, files.Recovered);
        var copy = Store(files.Recovered, fx);
        var replay = copy.Apply("payment", 1, Pay());
        Assert.Equal(E4bFundingApplyStatus.Replayed, replay.Status);
        Assert.Equal(2L, replay.State.Revision);
        Assert.Equal(E4bFundingApplyStatus.KeyConflict, copy.Apply("payment", 2, Begin()).Status);
        Assert.Equal(2L, copy.Load().Revision);
    }

    [Fact]
    public void BackupIncludesCompletedMintAndCanContinueAfterRecovery()
    {
        var fx = V15BenchmarkFixture.Create();
        using var files = new Files();
        var store = Store(files.Live, fx);
        store.Create(E4bV15MintCompletionFixture.PreProvisioned(fx));
        var begun = store.Apply("begin", 0, Begin());
        Assert.Equal(E4bFundingApplyStatus.Applied, begun.Status);
        var paid = store.Apply("pay", 1, new(E4bFundingAction.Deposit,
            Materials: begun.State.Funding.Escrow!.Quote));
        Assert.Equal(E4bFundingApplyStatus.Applied, paid.Status);
        var completed = store.Apply("finish", 2, new(E4bFundingAction.Complete));
        Assert.Equal(E4bFundingApplyStatus.Applied, completed.Status);
        Recovery(fx).BackupToNewFile(files.Live, files.Backup);
        Recovery(fx).RestoreToNewFile(files.Backup, files.Recovered);
        var recovered = Store(files.Recovered, fx);
        var checkpoint = recovered.Load();
        Assert.False(checkpoint.IsFunding);
        Assert.Single(checkpoint.Funding.CompletedMints);
        Assert.Equal(1, checkpoint.Campaign.CityCampaign.PurchaseCounters.Single(
            c => c.FamilyId == "mint").Acquired);
        Assert.Equal(E4bFundingApplyStatus.Replayed, recovered.Apply("finish", 2,
            new(E4bFundingAction.Complete)).Status);
        Assert.Equal(E4bFundingApplyStatus.Applied, recovered.Apply("next", 3, Begin()).Status);
    }

    [Fact]
    public void ExistingDestinationIsNotOverwrittenByBackup()
    {
        var fx = V15BenchmarkFixture.Create();
        using var files = new Files();
        Store(files.Live, fx).Create(fx.Start);
        File.WriteAllBytes(files.Backup, [1, 2, 3, 4]);
        Assert.Throws<IOException>(() => Recovery(fx).BackupToNewFile(files.Live, files.Backup));
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, File.ReadAllBytes(files.Backup));
        files.AssertNoStagingFiles();
    }

    [Fact]
    public void ExistingDestinationIsNotOverwrittenByRestore()
    {
        var fx = V15BenchmarkFixture.Create();
        using var files = new Files();
        Store(files.Live, fx).Create(fx.Start);
        Recovery(fx).BackupToNewFile(files.Live, files.Backup);
        File.WriteAllBytes(files.Recovered, [5, 6]);
        Assert.Throws<IOException>(() => Recovery(fx).RestoreToNewFile(files.Backup, files.Recovered));
        Assert.Equal(new byte[] { 5, 6 }, File.ReadAllBytes(files.Recovered));
        files.AssertNoStagingFiles();
    }

    [Fact]
    public void SourceCannotBeDestination()
    {
        var fx = V15BenchmarkFixture.Create();
        using var files = new Files();
        Store(files.Live, fx).Create(fx.Start);
        Assert.Throws<ArgumentException>(() => Recovery(fx).BackupToNewFile(files.Live, files.Live));
        Assert.Throws<ArgumentException>(() => Recovery(fx).RestoreToNewFile(files.Live, files.Live));
    }

    [Fact]
    public void NativePlayerSaveIsRejectedWithoutCreatingBackup()
    {
        var fx = V15BenchmarkFixture.Create();
        using var files = new Files();
        new V15SqliteFoodSaveStore(files.Live, fx.Baseline, fx.School,
            fx.Technologies, fx.Depot).Save(fx.Start);
        var original = File.ReadAllBytes(files.Live);
        Assert.Throws<InvalidDataException>(() => Recovery(fx).BackupToNewFile(files.Live, files.Backup));
        Assert.False(File.Exists(files.Backup));
        Assert.Equal(original, File.ReadAllBytes(files.Live));
        files.AssertNoStagingFiles();
    }

    [Fact]
    public void MissingSourceDoesNotCreateDestination()
    {
        var fx = V15BenchmarkFixture.Create();
        using var files = new Files();
        Assert.Throws<FileNotFoundException>(() => Recovery(fx).BackupToNewFile(files.Live, files.Backup));
        Assert.False(File.Exists(files.Backup));
    }

    [Fact]
    public void MissingDirectoryDoesNotModifySource()
    {
        var fx = V15BenchmarkFixture.Create();
        using var files = new Files();
        Store(files.Live, fx).Create(fx.Start);
        var before = File.ReadAllBytes(files.Live);
        Assert.Throws<DirectoryNotFoundException>(() => Recovery(fx).BackupToNewFile(
            files.Live, Path.Combine(files.DirectoryPath, "missing", "backup.sqlite3")));
        Assert.Equal(before, File.ReadAllBytes(files.Live));
        files.AssertNoStagingFiles();
    }

    [Fact]
    public void MissingOperationReceiptInvalidatesRestoreAndFurtherPayments()
    {
        var fx = V15BenchmarkFixture.Create();
        using var files = new Files();
        var store = Store(files.Live, fx);
        store.Create(fx.Start);
        store.Apply("begin", 0, Begin());
        store.Apply("payment", 1, Pay());
        using (var db = Sqlite(files.Live))
        using (var command = db.CreateCommand())
        {
            command.CommandText = "DELETE FROM composite_receipt WHERE request_id='begin';";
            Assert.Equal(1, command.ExecuteNonQuery());
        }
        Assert.Throws<InvalidDataException>(() => store.Load());
        Assert.Throws<InvalidDataException>(() => store.Apply("another", 2, Pay()));
        Assert.Throws<InvalidDataException>(() => Recovery(fx).BackupToNewFile(files.Live, files.Backup));
        Assert.False(File.Exists(files.Backup));
        files.AssertNoStagingFiles();
    }

    [Fact]
    public void ReorderedReceiptRevisionIsRejectedBeforeRecovery()
    {
        var fx = V15BenchmarkFixture.Create();
        using var files = new Files();
        var store = Store(files.Live, fx);
        store.Create(fx.Start);
        store.Apply("begin", 0, Begin());
        store.Apply("payment", 1, Pay());
        using (var db = Sqlite(files.Live))
        using (var command = db.CreateCommand())
        {
            command.CommandText = "UPDATE composite_receipt SET committed_revision=5 WHERE request_id='payment';";
            Assert.Equal(1, command.ExecuteNonQuery());
        }
        Assert.Throws<InvalidDataException>(() => store.Load());
        Assert.Throws<InvalidDataException>(() => Recovery(fx).BackupToNewFile(files.Live, files.Backup));
        Assert.False(File.Exists(files.Backup));
    }

    private static SqliteConnection Sqlite(string path)
    {
        var db = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = path, Pooling = false }.ToString());
        db.Open();
        return db;
    }
}
