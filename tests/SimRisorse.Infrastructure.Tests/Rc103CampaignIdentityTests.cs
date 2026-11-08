using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Infrastructure.Experimental;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class Rc103CampaignIdentityTests
{
    private sealed class Files : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(),
            "simrisorse-rc103-" + Guid.NewGuid().ToString("N"));
        public string Active => Path.Combine(Root, "active.sqlite3");
        public string Second => Path.Combine(Root, "second.sqlite3");
        public string Backup => Path.Combine(Root, "backup.sqlite3");
        public string Restored => Path.Combine(Root, "restored.sqlite3");
        public string Registry => Path.Combine(Root, "authority.sqlite3");
        public Files() => Directory.CreateDirectory(Root);
        public void Dispose() => Directory.Delete(Root, recursive: true);
    }

    private static E4bV15FundingCompositeSqliteStore Store(string path, V15BenchmarkFixture fx) =>
        new(path, fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15FundingAuthorityRegistry Registry(Files files, V15BenchmarkFixture fx) =>
        new(files.Registry, fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15FundingCompositeRecovery Recovery(V15BenchmarkFixture fx) =>
        new(fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bFundingInstruction Begin() =>
        new(E4bFundingAction.BeginMint, ProductionTier.Base);

    private static void Sql(string path, string sql)
    {
        using var db = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path, Pooling = false
        }.ToString());
        db.Open();
        using var command = db.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    [Fact]
    public void NewlyCreatedExperimentalCampaignHasStableCanonicalIdentity()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        var first = Store(f.Active, fx);
        first.Create(fx.Start);
        var id = first.ReadCampaignId();
        Assert.NotNull(id);
        Assert.True(Guid.TryParseExact(id, "N", out _));
        Assert.Equal(id, Store(f.Active, fx).ReadCampaignId());
        Assert.Equal(0L, Store(f.Active, fx).Load().Revision);
    }

    [Fact]
    public void IndependentCampaignsDoNotShareIdentity()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        Store(f.Second, fx).Create(fx.Start);
        Assert.NotEqual(Store(f.Active, fx).ReadCampaignId(),
            Store(f.Second, fx).ReadCampaignId());
    }

    [Fact]
    public void BackupAndRestoreKeepTheSameCampaignIdentity()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var id = Store(f.Active, fx).ReadCampaignId();
        Recovery(fx).BackupToNewFile(f.Active, f.Backup);
        Recovery(fx).RestoreToNewFile(f.Backup, f.Restored);
        Assert.Equal(id, Store(f.Backup, fx).ReadCampaignId());
        Assert.Equal(id, Store(f.Restored, fx).ReadCampaignId());
    }

    [Fact]
    public void RegistryBindsCampaignIdentityAcrossRestartAndWrites()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var lease = registry.RegisterInitial(f.Active);
        var id = Store(f.Active, fx).ReadCampaignId();
        Assert.Equal(id, registry.CurrentCampaignId());
        Assert.Equal(E4bFundingApplyStatus.Applied,
            registry.Apply(lease, "begin", 0, Begin()).Status);
        Assert.Equal(id, Registry(f, fx).CurrentCampaignId());
        Assert.Equal(lease, Registry(f, fx).Current());
    }

    [Fact]
    public void PromotionRetainsIdentityAndFencesOldLease()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var old = registry.RegisterInitial(f.Active);
        var id = registry.CurrentCampaignId();
        Recovery(fx).BackupToNewFile(f.Active, f.Backup);
        var promoted = registry.Promote(old, f.Backup);
        Assert.Equal(id, registry.CurrentCampaignId());
        Assert.Equal(id, Store(f.Backup, fx).ReadCampaignId());
        Assert.Throws<InvalidOperationException>(() =>
            registry.Apply(old, "old", 0, Begin()));
        Assert.Equal(E4bFundingApplyStatus.Applied,
            registry.Apply(promoted, "new", 0, Begin()).Status);
    }

    [Fact]
    public void PromotionRefusesIndependentCampaignEvenAtSameRevision()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        Store(f.Second, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var lease = registry.RegisterInitial(f.Active);
        Assert.Throws<InvalidOperationException>(() => registry.Promote(lease, f.Second));
        Assert.Equal(lease, registry.Current());
        Assert.Equal(0L, Store(f.Active, fx).Load().Revision);
    }

    [Fact]
    public void ReplacingActiveFileWithDifferentCampaignIsDetectedBeforeWrite()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        Store(f.Second, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var lease = registry.RegisterInitial(f.Active);
        // Deliberate out-of-band file replacement: never part of normal gameplay.
        File.Move(f.Active, f.Backup);
        File.Copy(f.Second, f.Active);
        Assert.Throws<InvalidDataException>(() => registry.Current());
        Assert.Throws<InvalidDataException>(() =>
            registry.Apply(lease, "must-not-apply", 0, Begin()));
        Assert.Equal(0L, Store(f.Active, fx).Load().Revision);
        File.Delete(f.Active);
        Assert.Throws<FileNotFoundException>(() => registry.Current());
        Assert.False(File.Exists(f.Active)); // No placeholder created on missing active path.
    }

    [Fact]
    public void RawSqlCannotReplaceIdentityOfRegisteredComposite()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        registry.RegisterInitial(f.Active);
        var original = registry.CurrentCampaignId();
        Assert.Throws<SqliteException>(() => Sql(f.Active,
            $"UPDATE composite_campaign_identity SET campaign_id='{Guid.NewGuid():N}' WHERE slot=1;"));
        Assert.Equal(original, Store(f.Active, fx).ReadCampaignId());
    }

    [Fact]
    public void RawSqlCannotDeleteIdentityOfRegisteredComposite()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        Registry(f, fx).RegisterInitial(f.Active);
        Assert.Throws<SqliteException>(() => Sql(f.Active,
            "DELETE FROM composite_campaign_identity WHERE slot=1;"));
        Assert.NotNull(Store(f.Active, fx).ReadCampaignId());
    }

    [Fact]
    public void RegistryDetectsTamperedCampaignBindingWithoutChangingStock()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        var lease = registry.RegisterInitial(f.Active);
        Sql(f.Registry, $"UPDATE authority_campaign_identity SET campaign_id='{Guid.NewGuid():N}' WHERE slot=1;");
        Assert.Throws<InvalidDataException>(() => registry.Current());
        Assert.Throws<InvalidDataException>(() =>
            registry.Apply(lease, "blocked", 0, Begin()));
        Assert.Equal(0L, Store(f.Active, fx).Load().Revision);
    }

    [Fact]
    public void LegacyUnregisteredCompositeGetsIdentityOnlyWhenRegistered()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        Sql(f.Active, "DROP TABLE composite_campaign_identity;");
        Assert.Null(Store(f.Active, fx).ReadCampaignId());
        _ = Store(f.Active, fx).Load();
        var registry = Registry(f, fx);
        registry.RegisterInitial(f.Active);
        Assert.NotNull(Store(f.Active, fx).ReadCampaignId());
        Assert.Equal(Store(f.Active, fx).ReadCampaignId(), registry.CurrentCampaignId());
    }

    [Fact]
    public void LegacyRegistryUpgradeBindsAlreadyRegisteredComposite()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Store(f.Active, fx).Create(fx.Start);
        var original = Registry(f, fx);
        var lease = original.RegisterInitial(f.Active);
        var id = original.CurrentCampaignId();
        // Emulate an RC102 registry that predates the identity-binding table.
        Sql(f.Registry, "DROP TABLE authority_campaign_identity;");
        var upgraded = Registry(f, fx);
        Assert.Equal(id, upgraded.CurrentCampaignId());
        Assert.Equal(lease, upgraded.Current());
        Assert.Equal(E4bFundingApplyStatus.Applied,
            upgraded.Apply(lease, "begin", 0, Begin()).Status);
    }
}
