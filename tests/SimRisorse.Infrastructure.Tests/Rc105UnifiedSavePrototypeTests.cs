using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Application.Persistence;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Infrastructure.Experimental;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class Rc105UnifiedSavePrototypeTests
{
    private sealed class Files : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(),
            "simrisorse-rc105-" + Guid.NewGuid().ToString("N"));
        public string Composite => Path.Combine(Root, "source.sqlite3");
        public string Registry => Path.Combine(Root, "registry.sqlite3");
        public string Unified => Path.Combine(Root, "prototype.sqlite3");
        public string Backup => Path.Combine(Root, "old-backup.sqlite3");
        public Files() => Directory.CreateDirectory(Root);
        public void Dispose() => Directory.Delete(Root, recursive: true);
        public void AssertClean() => Assert.Empty(Directory.GetFiles(Root, ".rc105-unified-*"));
    }

    private static E4bV15UnifiedSavePrototype Prototype(V15BenchmarkFixture fx) =>
        new(fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15FundingCompositeSqliteStore Composite(Files f, V15BenchmarkFixture fx) =>
        new(f.Composite, fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15FundingAuthorityRegistry Registry(Files f, V15BenchmarkFixture fx) =>
        new(f.Registry, fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15UnifiedSaveInspection Convert(Files f, V15BenchmarkFixture fx) =>
        Prototype(fx).CreateFromAuthoritativeComposite(f.Composite, f.Registry, f.Unified);

    private static E4bV15FundingAuthorityRegistry CreateRegistered(Files f, V15BenchmarkFixture fx)
    {
        Composite(f, fx).Create(fx.Start);
        var registry = Registry(f, fx);
        registry.RegisterInitial(f.Composite);
        return registry;
    }

    private static void Change(string file, string query)
    {
        using var db = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = file, Mode = SqliteOpenMode.ReadWrite, Pooling = false }.ToString());
        db.Open();
        using var command = db.CreateCommand();
        command.CommandText = query;
        command.ExecuteNonQuery();
    }

    [Fact]
    public void RegisteredBranchProducesOneGeneration7FileWithCityTables()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        var registry = CreateRegistered(f, fx);
        var saved = Convert(f, fx);
        Assert.Equal(registry.CurrentCampaignId(), saved.CampaignId);
        Assert.Equal(0L, saved.Revision);
        Assert.Equal(0L, saved.ReceiptCount);
        Assert.False(saved.IsAuthoritative);
        Assert.Equal(fx.Start.CityCampaign.Resources, saved.Campaign.CityCampaign.Resources);
        using var db = new SqliteConnection($"Data Source={f.Unified};Mode=ReadOnly;Pooling=False");
        db.Open();
        using var command = db.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('game_save','resource_stock','unified_campaign','unified_receipt');";
        Assert.Equal(4L, System.Convert.ToInt64(command.ExecuteScalar()));
        f.AssertClean();
    }

    [Fact]
    public void OpenQuotationRemainsPresentWithoutFictitiousPayment()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        var registry = CreateRegistered(f, fx);
        var lease = registry.Current();
        Assert.Equal(E4bFundingApplyStatus.Applied, registry.Apply(lease, "begin", 0,
            new(E4bFundingAction.BeginMint, ProductionTier.Base)).Status);
        var saved = Convert(f, fx);
        Assert.True(saved.HasPendingFinancing);
        Assert.Equal(ResourceBundle.Empty, saved.Escrowed);
        Assert.Equal(1L, saved.ReceiptCount);
        Assert.Equal(1L, saved.Revision);
    }

    [Fact]
    public void PendingDepositsAndEveryReceiptSurviveRoundTrip()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        var registry = CreateRegistered(f, fx);
        var lease = registry.Current();
        registry.Apply(lease, "begin", 0, new(E4bFundingAction.BeginMint, ProductionTier.Base));
        var deposit = ResourceBundle.Create(("planks", 1m));
        Assert.Equal(E4bFundingApplyStatus.Applied, registry.Apply(lease, "pay", 1,
            new(E4bFundingAction.Deposit, Materials: deposit)).Status);
        var saved = Convert(f, fx);
        var reloaded = Prototype(fx).Inspect(f.Unified);
        Assert.True(saved.HasPendingFinancing);
        Assert.Equal(deposit, reloaded.Escrowed);
        Assert.Equal(2L, reloaded.Revision);
        Assert.Equal(2L, reloaded.ReceiptCount);
        Assert.Equal(Composite(f, fx).Load().Campaign.CityCampaign.Resources,
            reloaded.Campaign.CityCampaign.Resources);
    }

    [Fact]
    public void CompletedMintAndHistoricalCounterAreNotChargedAgain()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        var store = Composite(f, fx);
        store.Create(E4bV15MintCompletionFixture.PreProvisioned(fx));
        var registry = Registry(f, fx);
        var lease = registry.RegisterInitial(f.Composite);
        var opened = registry.Apply(lease, "begin", 0,
            new(E4bFundingAction.BeginMint, ProductionTier.Base));
        Assert.Equal(E4bFundingApplyStatus.Applied, opened.Status);
        var paid = registry.Apply(lease, "pay", 1,
            new(E4bFundingAction.Deposit, Materials: opened.State.Funding.Escrow!.Quote));
        Assert.Equal(E4bFundingApplyStatus.Applied, paid.Status);
        var finished = registry.Apply(lease, "complete", 2,
            new(E4bFundingAction.Complete));
        Assert.Equal(E4bFundingApplyStatus.Applied, finished.Status);
        var saved = Convert(f, fx);
        Assert.Equal(3L, saved.Revision);
        Assert.Equal(3L, saved.ReceiptCount);
        Assert.False(saved.HasPendingFinancing);
        Assert.Equal(finished.State.Campaign.CityCampaign.Resources, saved.Campaign.CityCampaign.Resources);
        Assert.Equal(1, saved.Campaign.CityCampaign.ProductiveFamilies
            .Single(row => row.FamilyId == "mint").Base.Buildings);
        Assert.Equal(1, saved.Campaign.CityCampaign.PurchaseCounters
            .Single(row => row.FamilyId == "mint").Acquired);
    }

    [Fact]
    public void UnregisteredCompositeCannotBeExportedAsUnifiedPrototype()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        Composite(f, fx).Create(fx.Start);
        Assert.Throws<InvalidOperationException>(() => Convert(f, fx));
        Assert.False(File.Exists(f.Unified));
        f.AssertClean();
    }

    [Fact]
    public void InactiveBackupIsNotPromotedToAuthoritativeUnifiedFormat()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        CreateRegistered(f, fx);
        new E4bV15FundingCompositeRecovery(fx.Baseline, fx.School, fx.Technologies,
            fx.Depot).BackupToNewFile(f.Composite, f.Backup);
        Assert.Throws<InvalidOperationException>(() => Prototype(fx)
            .CreateFromAuthoritativeComposite(f.Backup, f.Registry, f.Unified));
        Assert.False(File.Exists(f.Unified));
    }

    [Fact]
    public void MissingAuthorityRegistryCannotBeSilentlyRecreated()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        CreateRegistered(f, fx);
        File.Delete(f.Registry);
        Assert.Throws<InvalidOperationException>(() => Convert(f, fx));
        Assert.False(File.Exists(f.Registry));
        Assert.False(File.Exists(f.Unified));
    }

    [Fact]
    public void ExistingDestinationIsNeverOverwritten()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        CreateRegistered(f, fx);
        File.WriteAllText(f.Unified, "KEEP-ME");
        Assert.Throws<IOException>(() => Convert(f, fx));
        Assert.Equal("KEEP-ME", File.ReadAllText(f.Unified));
        f.AssertClean();
    }

    [Fact]
    public void CannotExportOverSourceOrRegistry()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        CreateRegistered(f, fx);
        Assert.Throws<ArgumentException>(() => Prototype(fx)
            .CreateFromAuthoritativeComposite(f.Composite, f.Registry, f.Composite));
        Assert.Throws<ArgumentException>(() => Prototype(fx)
            .CreateFromAuthoritativeComposite(f.Composite, f.Registry, f.Registry));
    }

    [Fact]
    public void MissingDestinationDirectoryDoesNotCreateSideEffects()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        CreateRegistered(f, fx);
        var destination = Path.Combine(f.Root, "missing", "future.sqlite3");
        Assert.Throws<DirectoryNotFoundException>(() => Prototype(fx)
            .CreateFromAuthoritativeComposite(f.Composite, f.Registry, destination));
        Assert.False(File.Exists(destination));
        f.AssertClean();
    }

    [Fact]
    public void DeletedReceiptInvalidatesPrototypeBeforeAnyRestore()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        var registry = CreateRegistered(f, fx);
        var lease = registry.Current();
        registry.Apply(lease, "begin", 0, new(E4bFundingAction.BeginMint, ProductionTier.Base));
        Convert(f, fx);
        Change(f.Unified, "DELETE FROM unified_receipt;");
        Assert.Throws<InvalidDataException>(() => Prototype(fx).Inspect(f.Unified));
    }

    [Fact]
    public void ChangedFinancingCannotBeUsedToInventMaterials()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        CreateRegistered(f, fx);
        Convert(f, fx);
        Change(f.Unified, "UPDATE unified_campaign SET funding_json='{}' WHERE slot=1;");
        Assert.Throws<InvalidDataException>(() => Prototype(fx).Inspect(f.Unified));
    }

    [Fact]
    public void IncorrectAuthorityFlagIsRejected()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        CreateRegistered(f, fx);
        Convert(f, fx);
        // The CHECK constraint must reject any attempt to promote this prototype.
        Assert.Throws<SqliteException>(() => Change(f.Unified,
            "UPDATE unified_campaign SET authority_mode='active' WHERE slot=1;"));
        Assert.False(Prototype(fx).Inspect(f.Unified).IsAuthoritative);
    }

    [Fact]
    public void OrdinaryGeneration6LoaderRejectsExperimentalGeneration7()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        CreateRegistered(f, fx);
        Convert(f, fx);
        Assert.Throws<GameSaveException>(() => new V15SqliteFoodSaveStore(f.Unified,
            fx.Baseline, fx.School, fx.Technologies, fx.Depot).Load());
    }

    [Fact]
    public void SourceAndAuthorityAreNotModifiedByPrototypeCreation()
    {
        var fx = V15BenchmarkFixture.Create();
        using var f = new Files();
        CreateRegistered(f, fx);
        var original = File.ReadAllBytes(f.Composite);
        var authority = File.ReadAllBytes(f.Registry);
        Convert(f, fx);
        Assert.Equal(original, File.ReadAllBytes(f.Composite));
        Assert.Equal(authority, File.ReadAllBytes(f.Registry));
        f.AssertClean();
    }
}
