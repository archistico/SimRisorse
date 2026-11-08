using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Infrastructure.Experimental;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class Rc101FundingAuthorityTests
{
    private sealed class Files : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(),
            "simrisorse-rc101-" + Guid.NewGuid().ToString("N"));
        public string Active => Path.Combine(Root, "active.sqlite3");
        public string Backup => Path.Combine(Root, "copy.sqlite3");
        public string Registry => Path.Combine(Root, "authority.sqlite3");
        public Files() => Directory.CreateDirectory(Root);
        public void Dispose() => Directory.Delete(Root, recursive: true);
    }

    private static E4bV15FundingCompositeSqliteStore Store(string file, V15BenchmarkFixture fx) =>
        new(file, fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15FundingAuthorityRegistry Registry(Files f, V15BenchmarkFixture fx) =>
        new(f.Registry, fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15FundingCompositeRecovery Recovery(V15BenchmarkFixture fx) =>
        new(fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bFundingInstruction Begin() =>
        new(E4bFundingAction.BeginMint, ProductionTier.Base);

    private static E4bFundingInstruction Pay() =>
        new(E4bFundingAction.Deposit, Materials: ResourceBundle.Create(("planks", 1m)));

    [Fact]
    public void RegisteringOneValidBranchPersistsAcrossRegistryRestart()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var lease = Registry(f, fx).RegisterInitial(f.Active);
        var again = Registry(f, fx).Current();
        Assert.Equal(1L, again.Epoch);
        Assert.Equal(lease, again);
        Assert.Equal(Path.GetFullPath(f.Active), again.ActivePath);
    }

    [Fact]
    public void SecondRegistrationCannotReplaceActiveBranch()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var original = registry.RegisterInitial(f.Active);
        Assert.Throws<SqliteException>(() => registry.RegisterInitial(f.Active));
        Assert.Equal(original, registry.Current());
    }

    [Fact]
    public void MissingInitialCompositeDoesNotCreatePlaceholder()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        var registry = Registry(f, fx);
        Assert.Throws<FileNotFoundException>(() => registry.RegisterInitial(f.Active));
        Assert.False(File.Exists(f.Active));
        Assert.Throws<InvalidOperationException>(() => registry.Current());
    }

    [Fact]
    public void NativePlayerSaveCannotBeRegisteredAsComposite()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        new V15SqliteFoodSaveStore(f.Active, fx.Baseline, fx.School,
            fx.Technologies, fx.Depot).Save(fx.Start);
        var before = File.ReadAllBytes(f.Active);
        var registry = Registry(f, fx);
        Assert.Throws<InvalidDataException>(() => registry.RegisterInitial(f.Active));
        Assert.Equal(before, File.ReadAllBytes(f.Active));
    }

    [Fact]
    public void RegistryRefusesUnrelatedSQLiteFileWithoutReplacingIt()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        new V15SqliteFoodSaveStore(f.Registry, fx.Baseline, fx.School,
            fx.Technologies, fx.Depot).Save(fx.Start);
        var before = File.ReadAllBytes(f.Registry);
        Assert.Throws<InvalidDataException>(() => Registry(f, fx));
        Assert.Equal(before, File.ReadAllBytes(f.Registry));
    }

    [Fact]
    public void GuardedPaymentsPreserveIdempotencyAfterRegistryRestart()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var lease = registry.RegisterInitial(f.Active);
        Assert.Equal(E4bFundingApplyStatus.Applied,
            registry.Apply(lease, "begin", 0, Begin()).Status);
        Assert.Equal(E4bFundingApplyStatus.Applied,
            Registry(f, fx).Apply(lease, "pay", 1, Pay()).Status);
        var restarted = Registry(f, fx);
        Assert.Equal(E4bFundingApplyStatus.Replayed,
            restarted.Apply(restarted.Current(), "pay", 1, Pay()).Status);
        Assert.Equal(2L, Store(f.Active, fx).Load().Revision);
        Assert.Equal(ResourceBundle.Create(("planks", 1m)),
            Store(f.Active, fx).Load().Funding.Escrowed);
    }

    [Fact]
    public void GuardedClockReplaysOnlyOnceAcrossReopen()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var lease = registry.RegisterInitial(f.Active);
        var tick = registry.AdvanceClock(lease, "tick", 0, fx.Meat, fx.Crops);
        Assert.Equal(E4bFundingApplyStatus.Applied, tick.Status);
        Assert.Equal(E4bFundingApplyStatus.Replayed,
            Registry(f, fx).AdvanceClock(lease, "tick", 0, fx.Meat, fx.Crops).Status);
        Assert.Equal(tick.State.Campaign.CityCampaign.ElapsedTicks,
            Store(f.Active, fx).Load().Campaign.CityCampaign.ElapsedTicks);
        Assert.Equal(1L, Store(f.Active, fx).Load().Revision);
    }

    [Fact]
    public void PromotingIdenticalBackupFencesOldLeaseAndKeepsOriginalUntouched()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var oldLease = registry.RegisterInitial(f.Active);
        Assert.Equal(E4bFundingApplyStatus.Applied,
            registry.Apply(oldLease, "begin", 0, Begin()).Status);
        Recovery(fx).BackupToNewFile(f.Active, f.Backup);
        var before = Store(f.Active, fx).Load().Revision;
        var promoted = registry.Promote(oldLease, f.Backup);
        Assert.Equal(2L, promoted.Epoch);
        Assert.Equal(Path.GetFullPath(f.Backup), registry.Current().ActivePath);
        Assert.NotEqual(oldLease.Token, promoted.Token);
        Assert.Throws<InvalidOperationException>(() => registry.Apply(oldLease, "old", 1, Pay()));
        Assert.Equal(E4bFundingApplyStatus.Applied,
            registry.Apply(promoted, "new", 1, Pay()).Status);
        Assert.Equal(before, Store(f.Active, fx).Load().Revision);
        Assert.Equal(2L, Store(f.Backup, fx).Load().Revision);
    }

    [Fact]
    public void StaleBackupCannotEraseACommittedPayment()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var lease = registry.RegisterInitial(f.Active);
        Recovery(fx).BackupToNewFile(f.Active, f.Backup);
        Assert.Equal(E4bFundingApplyStatus.Applied,
            registry.Apply(lease, "begin", 0, Begin()).Status);
        Assert.Throws<InvalidOperationException>(() => registry.Promote(lease, f.Backup));
        Assert.Equal(lease, registry.Current());
        Assert.Equal(1L, Store(f.Active, fx).Load().Revision);
    }

    [Fact]
    public void TamperedReceiptCannotBePromotedEvenAtEqualRevision()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var lease = registry.RegisterInitial(f.Active);
        registry.Apply(lease, "begin", 0, Begin());
        Recovery(fx).BackupToNewFile(f.Active, f.Backup);
        using (var db = new SqliteConnection(new SqliteConnectionStringBuilder
            { DataSource = f.Backup, Pooling = false }.ToString()))
        {
            db.Open();
            // Simulate deliberate tampering by a privileged external editor.
            // Ordinary raw SQLite updates now fail closed on RC102 guarded copies.
            db.CreateFunction<long>("rc102_authorized_write", () => 1L);
            using var command = db.CreateCommand();
            command.CommandText = "UPDATE composite_receipt SET payload='forged' WHERE request_id='begin';";
            Assert.Equal(1, command.ExecuteNonQuery());
        }
        Assert.Equal(1L, Store(f.Backup, fx).Load().Revision);
        Assert.Throws<InvalidOperationException>(() => registry.Promote(lease, f.Backup));
        Assert.Equal(lease, registry.Current());
    }

    [Fact]
    public void SameFileCannotBePromotedAsASeparateBranch()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var lease = registry.RegisterInitial(f.Active);
        Assert.Throws<ArgumentException>(() => registry.Promote(lease, f.Active));
        Assert.Equal(lease, registry.Current());
    }

    [Fact]
    public void ForgedOrStaleLeaseIsFencedWithoutAWrite()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var lease = registry.RegisterInitial(f.Active);
        Assert.Throws<InvalidOperationException>(() =>
            registry.Apply(lease with { Token = "invalid" }, "bad", 0, Begin()));
        Assert.Throws<InvalidOperationException>(() =>
            registry.Apply(lease with { Epoch = 2 }, "old", 0, Begin()));
        Assert.Equal(0L, Store(f.Active, fx).Load().Revision);
        Assert.Equal(lease, registry.Current());
    }
}
