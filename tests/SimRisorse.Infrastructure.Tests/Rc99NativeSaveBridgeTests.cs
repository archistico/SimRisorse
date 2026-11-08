using SimRisorse.Application.Benchmarks;
using SimRisorse.Application.Persistence;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Infrastructure.Experimental;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class Rc99NativeSaveBridgeTests
{
    private sealed class Files : IDisposable
    {
        public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(),
            "simrisorse-rc99-" + Guid.NewGuid().ToString("N"));
        public string Native => Path.Combine(DirectoryPath, "native.sqlite3");
        public string Composite => Path.Combine(DirectoryPath, "composite.sqlite3");
        public string Exported => Path.Combine(DirectoryPath, "exported.sqlite3");
        public Files() => Directory.CreateDirectory(DirectoryPath);
        public void Dispose() => Directory.Delete(DirectoryPath, recursive: true);
        public void AssertNoStagingFiles() => Assert.Empty(System.IO.Directory.GetFiles(
            DirectoryPath, ".rc99-bridge-*"));
    }

    private static E4bV15FundingSaveBridge Bridge(V15BenchmarkFixture fx) =>
        new(fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static V15SqliteFoodSaveStore Native(string path, V15BenchmarkFixture fx) =>
        new(path, fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    private static E4bV15FundingCompositeSqliteStore Composite(string path, V15BenchmarkFixture fx) =>
        new(path, fx.Baseline, fx.School, fx.Technologies, fx.Depot);

    [Fact]
    public void ImportCopiesARealNativeSaveWithoutChangingItsBytes()
    {
        var fx = V15BenchmarkFixture.Create();
        using var paths = new Files();
        Native(paths.Native, fx).Save(fx.Start);
        var original = File.ReadAllBytes(paths.Native);
        var imported = Bridge(fx).ImportNativeToNewComposite(paths.Native, paths.Composite);
        Assert.Equal(0L, imported.Revision);
        Assert.False(imported.IsFunding);
        Assert.Equal(fx.Start.CityCampaign.Resources, imported.Funding.Stock);
        Assert.Equal(fx.Start.CityCampaign.Population, imported.Campaign.CityCampaign.Population);
        Assert.Equal(original, File.ReadAllBytes(paths.Native));
        Assert.Equal(imported.Funding.Stock, Composite(paths.Composite, fx).Load().Funding.Stock);
        paths.AssertNoStagingFiles();
    }

    [Fact]
    public void ImportNeverOverwritesAnExistingCompositeFile()
    {
        var fx = V15BenchmarkFixture.Create();
        using var paths = new Files();
        Native(paths.Native, fx).Save(fx.Start);
        Composite(paths.Composite, fx).Create(fx.Start);
        var before = File.ReadAllBytes(paths.Composite);
        Assert.Throws<IOException>(() => Bridge(fx).ImportNativeToNewComposite(paths.Native, paths.Composite));
        Assert.Equal(before, File.ReadAllBytes(paths.Composite));
        paths.AssertNoStagingFiles();
    }

    [Fact]
    public void ForeignImportSourceIsRejectedWithoutCreatingDestination()
    {
        var fx = V15BenchmarkFixture.Create();
        using var paths = new Files();
        Composite(paths.Composite, fx).Create(fx.Start);
        Assert.Throws<GameSaveException>(() => Bridge(fx).ImportNativeToNewComposite(paths.Composite, paths.Exported));
        Assert.False(File.Exists(paths.Exported));
        paths.AssertNoStagingFiles();
    }

    [Fact]
    public void ImportRefusesSameSourceAndDestination()
    {
        var fx = V15BenchmarkFixture.Create();
        using var paths = new Files();
        Native(paths.Native, fx).Save(fx.Start);
        var original = File.ReadAllBytes(paths.Native);
        Assert.Throws<ArgumentException>(() => Bridge(fx).ImportNativeToNewComposite(paths.Native, paths.Native));
        Assert.Equal(original, File.ReadAllBytes(paths.Native));
    }

    [Fact]
    public void EmptyEscrowCanExportAsASeparatePlayableNativeSave()
    {
        var fx = V15BenchmarkFixture.Create();
        using var paths = new Files();
        Composite(paths.Composite, fx).Create(fx.Start);
        var bytes = File.ReadAllBytes(paths.Composite);
        var exported = Bridge(fx).ExportToNewNativeSave(paths.Composite, paths.Exported);
        var loaded = Native(paths.Exported, fx).Load();
        Assert.Equal(fx.Start.CityCampaign.Resources, loaded.CityCampaign.Resources);
        Assert.Equal(fx.Start.CityCampaign.Population, exported.CityCampaign.Population);
        Assert.Equal(bytes, File.ReadAllBytes(paths.Composite));
        paths.AssertNoStagingFiles();
    }

    [Fact]
    public void PartialDepositBlocksExportAndPreservesEscrow()
    {
        var fx = V15BenchmarkFixture.Create();
        using var paths = new Files();
        var store = Composite(paths.Composite, fx);
        store.Create(fx.Start);
        Assert.Equal(E4bFundingApplyStatus.Applied, store.Apply("begin", 0,
            new(E4bFundingAction.BeginMint, ProductionTier.Base)).Status);
        Assert.Equal(E4bFundingApplyStatus.Applied, store.Apply("pay", 1,
            new(E4bFundingAction.Deposit,
                Materials: ResourceBundle.Create(("planks", 1m)))).Status);
        var before = File.ReadAllBytes(paths.Composite);
        Assert.Throws<InvalidOperationException>(() =>
            Bridge(fx).ExportToNewNativeSave(paths.Composite, paths.Exported));
        Assert.False(File.Exists(paths.Exported));
        Assert.Equal(before, File.ReadAllBytes(paths.Composite));
        Assert.Equal(2L, store.Load().Revision);
        paths.AssertNoStagingFiles();
    }

    [Fact]
    public void BeginningAQuoteWithoutPaymentAlsoBlocksNativeExport()
    {
        var fx = V15BenchmarkFixture.Create();
        using var paths = new Files();
        var store = Composite(paths.Composite, fx);
        store.Create(fx.Start);
        store.Apply("begin", 0, new(E4bFundingAction.BeginMint, ProductionTier.Base));
        Assert.Throws<InvalidOperationException>(() =>
            Bridge(fx).ExportToNewNativeSave(paths.Composite, paths.Exported));
        Assert.False(File.Exists(paths.Exported));
    }

    [Fact]
    public void CompletedMintExportsOnceWithUnchangedFinalResourceBalance()
    {
        var fx = V15BenchmarkFixture.Create();
        using var paths = new Files();
        var store = Composite(paths.Composite, fx);
        store.Create(E4bV15MintCompletionFixture.PreProvisioned(fx));
        var begun = store.Apply("begin", 0,
            new(E4bFundingAction.BeginMint, ProductionTier.Base));
        Assert.Equal(E4bFundingApplyStatus.Applied, begun.Status);
        var quote = begun.State.Funding.Escrow!.Quote;
        var paid = store.Apply("deposit", begun.State.Revision,
            new(E4bFundingAction.Deposit, Materials: quote));
        Assert.Equal(E4bFundingApplyStatus.Applied, paid.Status);
        var finished = store.Apply("complete", paid.State.Revision,
            new(E4bFundingAction.Complete));
        Assert.Equal(E4bFundingApplyStatus.Applied, finished.Status);
        var before = File.ReadAllBytes(paths.Composite);
        var exported = Bridge(fx).ExportToNewNativeSave(paths.Composite, paths.Exported);
        var city = Native(paths.Exported, fx).Load().CityCampaign;
        Assert.Equal(paid.State.Campaign.CityCampaign.Resources, city.Resources);
        Assert.Equal(city.Resources, exported.CityCampaign.Resources);
        Assert.Equal(1, city.ProductiveFamilies.Single(f => f.FamilyId == "mint").Base.Buildings);
        Assert.Equal(1, city.PurchaseCounters.Single(c => c.FamilyId == "mint").Acquired);
        Assert.Equal(before, File.ReadAllBytes(paths.Composite));
        Assert.Single(store.Load().Funding.CompletedMints);
        paths.AssertNoStagingFiles();
    }

    [Fact]
    public void ExportNeverOverwritesAnExistingPlayerSave()
    {
        var fx = V15BenchmarkFixture.Create();
        using var paths = new Files();
        Composite(paths.Composite, fx).Create(fx.Start);
        Native(paths.Exported, fx).Save(fx.Start);
        var original = File.ReadAllBytes(paths.Exported);
        Assert.Throws<IOException>(() =>
            Bridge(fx).ExportToNewNativeSave(paths.Composite, paths.Exported));
        Assert.Equal(original, File.ReadAllBytes(paths.Exported));
        paths.AssertNoStagingFiles();
    }

    [Fact]
    public void ExportNeverConvertsCompositeDatabaseIntoPlayerSaveInPlace()
    {
        var fx = V15BenchmarkFixture.Create();
        using var paths = new Files();
        Composite(paths.Composite, fx).Create(fx.Start);
        var original = File.ReadAllBytes(paths.Composite);
        Assert.Throws<ArgumentException>(() =>
            Bridge(fx).ExportToNewNativeSave(paths.Composite, paths.Composite));
        Assert.Equal(original, File.ReadAllBytes(paths.Composite));
    }

    [Fact]
    public void MissingCompositeDoesNotCreateAnEmptyDatabase()
    {
        var fx = V15BenchmarkFixture.Create();
        using var paths = new Files();
        Assert.Throws<FileNotFoundException>(() =>
            Bridge(fx).ExportToNewNativeSave(paths.Composite, paths.Exported));
        Assert.False(File.Exists(paths.Composite));
        Assert.False(File.Exists(paths.Exported));
    }

    [Fact]
    public void MissingDestinationDirectoryDoesNotModifySource()
    {
        var fx = V15BenchmarkFixture.Create();
        using var paths = new Files();
        Native(paths.Native, fx).Save(fx.Start);
        var original = File.ReadAllBytes(paths.Native);
        var destination = Path.Combine(paths.DirectoryPath, "missing", "composite.sqlite3");
        Assert.Throws<DirectoryNotFoundException>(() =>
            Bridge(fx).ImportNativeToNewComposite(paths.Native, destination));
        Assert.Equal(original, File.ReadAllBytes(paths.Native));
    }
}
