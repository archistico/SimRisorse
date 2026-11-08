using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Infrastructure.Experimental;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class Rc102MandatoryCompositeFenceTests
{
    private sealed class Files : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(),
            "simrisorse-rc102-" + Guid.NewGuid().ToString("N"));
        public string Active => Path.Combine(Root, "active.sqlite3");
        public string Backup => Path.Combine(Root, "backup.sqlite3");
        public string Restored => Path.Combine(Root, "restored.sqlite3");
        public string Authority => Path.Combine(Root, "authority.sqlite3");
        public string ForeignAuthority => Path.Combine(Root, "other-authority.sqlite3");
        public Files() => Directory.CreateDirectory(Root);
        public void Dispose() => Directory.Delete(Root, recursive: true);
    }

    private static E4bV15FundingCompositeSqliteStore Store(string file, V15BenchmarkFixture fx) =>
        new(file, fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15FundingAuthorityRegistry Registry(string file, V15BenchmarkFixture fx) =>
        new(file, fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15FundingCompositeRecovery Recovery(V15BenchmarkFixture fx) =>
        new(fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bFundingInstruction Begin() => new(E4bFundingAction.BeginMint, ProductionTier.Base);
    private static E4bFundingInstruction Deposit() =>
        new(E4bFundingAction.Deposit, Materials: ResourceBundle.Create(("planks", 1m)));

    private static (V15BenchmarkFixture Fx, E4bV15FundingAuthorityRegistry Registry,
        E4bV15FundingAuthorityLease Lease) Setup(Files f)
    {
        var fx = V15BenchmarkFixture.Create();
        Store(f.Active, fx).Create(fx.Start);
        var registry = Registry(f.Authority, fx);
        var lease = registry.RegisterInitial(f.Active);
        return (fx, registry, lease);
    }

    private static void Sql(string file, string statement)
    {
        using var db = new SqliteConnection(new SqliteConnectionStringBuilder
            { DataSource = file, Pooling = false }.ToString());
        db.Open();
        using var command = db.CreateCommand();
        command.CommandText = statement;
        command.ExecuteNonQuery();
    }

    [Fact]
    public void RegisteredFileRejectsDirectLegacyApply()
    {
        using var f = new Files();
        var (fx, registry, lease) = Setup(f);
        Assert.Throws<InvalidOperationException>(() => Store(f.Active, fx).Apply("bypass", 0, Begin()));
        Assert.Equal(0L, Store(f.Active, fx).Load().Revision);
        Assert.Equal(E4bFundingApplyStatus.Applied, registry.Apply(lease, "begin", 0, Begin()).Status);
    }

    [Fact]
    public void RegisteredFileRejectsDirectLegacyClock()
    {
        using var f = new Files();
        var (fx, registry, lease) = Setup(f);
        Assert.Throws<InvalidOperationException>(() =>
            Store(f.Active, fx).AdvanceClock("bypass", 0, fx.Meat, fx.Crops));
        Assert.Equal(E4bFundingApplyStatus.Applied,
            registry.AdvanceClock(lease, "tick", 0, fx.Meat, fx.Crops).Status);
        Assert.Equal(1L, Store(f.Active, fx).Load().Revision);
    }

    [Fact]
    public void RawSqlCannotModifyRegisteredCityImage()
    {
        using var f = new Files();
        var (fx, _, _) = Setup(f);
        Assert.Throws<SqliteException>(() =>
            Sql(f.Active, "UPDATE composite_state SET revision=revision+1 WHERE slot=1;"));
        Assert.Equal(0L, Store(f.Active, fx).Load().Revision);
    }

    [Fact]
    public void RawSqlCannotInsertFakeReceipt()
    {
        using var f = new Files();
        var (fx, _, _) = Setup(f);
        Assert.Throws<SqliteException>(() => Sql(f.Active,
            "INSERT INTO composite_receipt(request_id,payload,committed_revision) VALUES ('forged','x',1);"));
        Assert.Equal(0L, Store(f.Active, fx).Load().Revision);
    }

    [Fact]
    public void RawSqlCannotChangeAuthorityBinding()
    {
        using var f = new Files();
        var (_, registry, lease) = Setup(f);
        Assert.Throws<SqliteException>(() => Sql(f.Active,
            "UPDATE composite_authority_guard SET registry_path='different' WHERE slot=1;"));
        Assert.Equal(lease, registry.Current());
    }

    [Fact]
    public void RawSqlCannotDeleteAuthorizationBinding()
    {
        using var f = new Files();
        var (_, registry, lease) = Setup(f);
        Assert.Throws<SqliteException>(() => Sql(f.Active,
            "DELETE FROM composite_authority_guard WHERE slot=1;"));
        Assert.Equal(lease, registry.Current());
    }

    [Fact]
    public void UnregisteredCompositeIsUsableThenLegacyRegistryReopenInstallsFence()
    {
        using var f = new Files();
        var fx = V15BenchmarkFixture.Create();
        Store(f.Active, fx).Create(fx.Start);
        Assert.Equal(E4bFundingApplyStatus.Applied, Store(f.Active, fx).Apply("begin", 0, Begin()).Status);
        Assert.Equal(1L, Store(f.Active, fx).Load().Revision);

        // Construct an RC101 registry with an already active lineage but no
        // RC102 protection. Its first RC102 reopen upgrades the linked file.
        using (var db = new SqliteConnection(new SqliteConnectionStringBuilder
            { DataSource = f.Authority, Pooling = false }.ToString()))
        {
            db.Open();
            foreach (var sql in new[]
            {
                $"PRAGMA application_id = {E4bV15FundingAuthorityRegistry.ApplicationId};",
                $"PRAGMA user_version = {E4bV15FundingAuthorityRegistry.FileVersion};",
                "CREATE TABLE authority_state(slot INTEGER PRIMARY KEY CHECK(slot=1), epoch INTEGER NOT NULL, active_path TEXT NOT NULL, token TEXT NOT NULL);"
            })
            {
                using var command = db.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
            using var insert = db.CreateCommand();
            insert.CommandText = "INSERT INTO authority_state(slot,epoch,active_path,token) VALUES (1,1,$path,'old-token');";
            insert.Parameters.AddWithValue("$path", Path.GetFullPath(f.Active));
            insert.ExecuteNonQuery();
        }
        var legacyUpgraded = Registry(f.Authority, fx);
        Assert.Throws<InvalidOperationException>(() => Store(f.Active, fx).Apply("bypass", 1, Deposit()));
        Assert.Equal(E4bFundingApplyStatus.Applied,
            legacyUpgraded.Apply(legacyUpgraded.Current(), "legitimate", 1, Deposit()).Status);
        Assert.Equal(2L, Store(f.Active, fx).Load().Revision);
    }

    [Fact]
    public void BackupInheritsFenceWithoutPromotingAuthority()
    {
        using var f = new Files();
        var (fx, _, _) = Setup(f);
        Recovery(fx).BackupToNewFile(f.Active, f.Backup);
        Assert.Throws<InvalidOperationException>(() => Store(f.Backup, fx).Apply("bypass", 0, Begin()));
        Assert.Equal(0L, Store(f.Backup, fx).Load().Revision);
    }

    [Fact]
    public void PromotedBranchRemainsFencedForDirectWriters()
    {
        using var f = new Files();
        var (fx, registry, oldLease) = Setup(f);
        Recovery(fx).BackupToNewFile(f.Active, f.Backup);
        var lease = registry.Promote(oldLease, f.Backup);
        Assert.Throws<InvalidOperationException>(() => Store(f.Backup, fx).Apply("bypass", 0, Begin()));
        Assert.Throws<InvalidOperationException>(() => registry.Apply(oldLease, "old", 0, Begin()));
        Assert.Equal(E4bFundingApplyStatus.Applied, registry.Apply(lease, "new", 0, Begin()).Status);
        Assert.Equal(0L, Store(f.Active, fx).Load().Revision);
    }

    [Fact]
    public void RestoredCopyKeepsFenceAndCanBePromotedWhenIdentical()
    {
        using var f = new Files();
        var (fx, registry, lease) = Setup(f);
        Recovery(fx).BackupToNewFile(f.Active, f.Backup);
        Recovery(fx).RestoreToNewFile(f.Backup, f.Restored);
        Assert.Throws<InvalidOperationException>(() => Store(f.Restored, fx).Apply("bypass", 0, Begin()));
        var promoted = registry.Promote(lease, f.Restored);
        Assert.Equal(E4bFundingApplyStatus.Applied, registry.Apply(promoted, "begin", 0, Begin()).Status);
    }

    [Fact]
    public void CopyingRegistryToAnotherPathCannotAuthorizeTheSameComposite()
    {
        using var f = new Files();
        var (fx, _, _) = Setup(f);
        File.Copy(f.Authority, f.ForeignAuthority);
        Assert.Throws<InvalidDataException>(() => Registry(f.ForeignAuthority, fx));
        Assert.Equal(0L, Store(f.Active, fx).Load().Revision);
    }

    [Fact]
    public void AnotherRegistryCannotRegisterAlreadyFencedComposite()
    {
        using var f = new Files();
        var (fx, registry, lease) = Setup(f);
        var other = Registry(f.ForeignAuthority, fx);
        Assert.Throws<InvalidDataException>(() => other.RegisterInitial(f.Active));
        Assert.Equal(lease, registry.Current());
        Assert.Equal(0L, Store(f.Active, fx).Load().Revision);
    }
}
