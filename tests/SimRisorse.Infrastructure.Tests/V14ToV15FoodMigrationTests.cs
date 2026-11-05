using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>F1-D05c: a copy-on-write migration must never replace the live v14 campaign.</summary>
public sealed class V14ToV15FoodMigrationTests
{
    private static readonly SchoolResearchRules School = new(
        "food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;

    private static FoodConversionPolicy Policy() => UnifiedFoodInputs.CreatePolicy([
        new("fish", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 0),
        new("bread", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 1),
        new("meat", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 2),
        new("fruit", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 3),
        new("vegetables", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 4)
    ]);

    private static ResourceBundle Capacities() => ResourceBundle.Create(
        ("fish", 1m), ("bread", 1m), ("meat", 0m), ("fruit", 0m), ("vegetables", 0m));
    private static ResourceAmount ReadyCapacity => ResourceAmount.FromUnits(1m);

    private sealed class Environment : IDisposable
    {
        public string Folder { get; } = Path.Combine(Path.GetTempPath(),
            "simrisorse-food-copy-" + Guid.NewGuid().ToString("N"));
        public string V14 => Path.Combine(Folder, "original-v14.sqlite3");
        public string V15 => Path.Combine(Folder, "converted-v15.sqlite3");
        public Environment() => Directory.CreateDirectory(Folder);
        public V14SqliteSaveStore Source() => new(V14, CivilGrowthScenario.Create(), School, Technologies);
        public V15SqliteFoodSaveStore Destination() => new(V15, CivilGrowthScenario.Create(),
            School, Technologies, Policy());
        public V14ToV15FoodMigrator Migrator() => new(V14, V15, CivilGrowthScenario.Create(),
            School, Technologies, Policy());
        public V14CampaignSnapshot Seed()
        {
            var scenario = CivilGrowthScenario.Create();
            var initial = V14CampaignSnapshots.MigrateFromV13(scenario,
                GameSession.Create(scenario).Snapshot(), School, Technologies);
            var source = initial with { Resources = initial.Resources + ResourceBundle.Create(
                ("fish", 4.125m), ("bread", 2.333m), ("food", 0.875m)) };
            Source().Save(source);
            return source;
        }
        public void Dispose() => Directory.Delete(Folder, recursive: true);
    }

    [Fact]
    public void PreviewDoesNotCreateV15OrChangeSourceBytesOrBackups()
    {
        using var env = new Environment();
        var original = env.Seed();
        var before = File.ReadAllBytes(env.V14);
        var files = Directory.GetFiles(env.Folder);
        var preview = env.Migrator().Preview(Capacities(), ReadyCapacity);
        Assert.Equal(64, preview.SourceFileSha256.Length);
        Assert.False(preview.Capacity.Fits);
        Assert.Equal(original.Resources, V14FoodStockMigration.RestoreLegacyResources(preview.Plan, Policy()));
        Assert.Equal(before, File.ReadAllBytes(env.V14));
        Assert.Equal(files, Directory.GetFiles(env.Folder));
        Assert.False(File.Exists(env.V15));
    }

    [Fact]
    public void CopyPreservesOriginalBytesAndEntireHistoricFoodInventory()
    {
        using var env = new Environment();
        var original = env.Seed();
        var before = File.ReadAllBytes(env.V14);
        var receipt = env.Migrator().MigrateCopy(Capacities(), ReadyCapacity);
        Assert.False(receipt.AlreadyPresent);
        Assert.Equal(env.V14, receipt.SourcePath);
        Assert.Equal(env.V15, receipt.DestinationPath);
        Assert.False(receipt.Capacity.Fits); // Over-capacity stock is not deleted.
        Assert.Equal(before, File.ReadAllBytes(env.V14));
        var checkpoint = env.Destination().Load();
        Assert.Equal(original.Resources, V15FoodCampaignCheckpoints.ToLegacyPreview(checkpoint,
            CivilGrowthScenario.Create(), School, Technologies, Policy()).Resources);
        Assert.Equal(ResourceAmount.Zero, checkpoint.CityCampaign.Resources["fish"]);
        Assert.Equal(ResourceAmount.Zero, checkpoint.CityCampaign.Resources["food"]);
        Assert.Equal(ResourceAmount.FromUnits(4.125m), checkpoint.Depot.RawFood["fish"]);
        Assert.Equal(ResourceAmount.FromUnits(2.333m), checkpoint.Depot.RawFood["bread"]);
        Assert.Equal(original.Population, checkpoint.CityCampaign.Population);
        Assert.Equal(original.ElapsedTicks, checkpoint.CityCampaign.ElapsedTicks);
        Assert.Equal(original.PencilPaidStudents, checkpoint.CityCampaign.PencilPaidStudents);
        Assert.Equal(original.SchoolCount, checkpoint.CityCampaign.SchoolCount);
        Assert.Equal(original.OtherBuildings.OrderBy(x => x.BuildingId),
            checkpoint.CityCampaign.OtherBuildings.OrderBy(x => x.BuildingId));
        Assert.DoesNotContain(Directory.GetFiles(env.Folder),
            path => Path.GetFileName(path).Contains("backup", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RepeatedIdenticalMigrationIsIdempotentAndDoesNotCreateAnyBackup()
    {
        using var env = new Environment();
        env.Seed();
        var first = env.Migrator().MigrateCopy(Capacities(), ReadyCapacity);
        var bytes = File.ReadAllBytes(env.V15);
        var files = Directory.GetFiles(env.Folder).OrderBy(x => x).ToArray();
        var second = env.Migrator().MigrateCopy(Capacities(), ReadyCapacity);
        Assert.False(first.AlreadyPresent);
        Assert.True(second.AlreadyPresent);
        Assert.Equal(first.SourceFileSha256, second.SourceFileSha256);
        Assert.Equal(bytes, File.ReadAllBytes(env.V15));
        Assert.Equal(files, Directory.GetFiles(env.Folder).OrderBy(x => x).ToArray());
    }

    [Fact]
    public void DifferentValidV15CampaignIsNeverOverwritten()
    {
        using var env = new Environment();
        env.Seed();
        env.Migrator().MigrateCopy(Capacities(), ReadyCapacity);
        var first = env.Destination().Load();
        env.Destination().Save(first with { Depot = first.Depot with
        { ReadyFood = first.Depot.ReadyFood + ResourceAmount.FromUnits(2m) } });
        var before = File.ReadAllBytes(env.V15);
        var ex = Assert.Throws<GameSaveException>(() => env.Migrator().MigrateCopy(Capacities(), ReadyCapacity));
        Assert.Equal(GameSaveError.Superseded, ex.Error);
        Assert.Equal(before, File.ReadAllBytes(env.V15));
    }

    [Fact]
    public void ExistingNonV15DestinationRemainsByteIdentical()
    {
        using var env = new Environment();
        var original = env.Seed();
        new V14SqliteSaveStore(env.V15, CivilGrowthScenario.Create(), School, Technologies).Save(original);
        var before = File.ReadAllBytes(env.V15);
        Assert.Equal(GameSaveError.UnsupportedFormat,
            Assert.Throws<GameSaveException>(() => env.Migrator().MigrateCopy(Capacities(), ReadyCapacity)).Error);
        Assert.Equal(before, File.ReadAllBytes(env.V15));
    }

    [Fact]
    public void CorruptV14SourceFailsClosedWithoutDestination()
    {
        using var env = new Environment();
        File.WriteAllText(env.V14, "not sqlite");
        Assert.Throws<GameSaveException>(() => env.Migrator().MigrateCopy(Capacities(), ReadyCapacity));
        Assert.Equal("not sqlite", File.ReadAllText(env.V14));
        Assert.False(File.Exists(env.V15));
    }

    [Fact]
    public void MissingV14SourceDoesNotProduceV15File()
    {
        using var env = new Environment();
        Assert.Equal(GameSaveError.NotFound,
            Assert.Throws<GameSaveException>(() => env.Migrator().MigrateCopy(Capacities(), ReadyCapacity)).Error);
        Assert.Empty(Directory.GetFiles(env.Folder));
    }

    [Fact]
    public void SameInputAndOutputPathIsRejectedBeforeAnyWrite()
    {
        using var env = new Environment();
        env.Seed();
        var before = File.ReadAllBytes(env.V14);
        Assert.Equal(GameSaveError.UnsupportedFormat, Assert.Throws<GameSaveException>(() =>
            new V14ToV15FoodMigrator(env.V14, env.V14, CivilGrowthScenario.Create(),
                School, Technologies, Policy())).Error);
        Assert.Equal(before, File.ReadAllBytes(env.V14));
    }

    [Fact]
    public void CopyToNestedDirectoryPromotesOnlyVerifiedV15AndLeavesNoStagingFiles()
    {
        using var env = new Environment();
        var original = env.Seed();
        var nested = Path.Combine(env.Folder, "export", "v15.sqlite3");
        var migrator = new V14ToV15FoodMigrator(env.V14, nested, CivilGrowthScenario.Create(),
            School, Technologies, Policy());
        var receipt = migrator.MigrateCopy(Capacities(), ReadyCapacity);
        Assert.False(receipt.AlreadyPresent);
        Assert.Equal(nested, receipt.DestinationPath);
        Assert.Equal(new[] { nested }, Directory.GetFiles(Path.GetDirectoryName(nested)!));
        var reopened = new V15SqliteFoodSaveStore(nested, CivilGrowthScenario.Create(),
            School, Technologies, Policy()).Load();
        Assert.Equal(original.Resources, V15FoodCampaignCheckpoints.ToLegacyPreview(reopened,
            CivilGrowthScenario.Create(), School, Technologies, Policy()).Resources);
    }

    [Fact]
    public void AnOriginalChangedAfterMigrationIsNotSilentlyUsedToReplaceV15()
    {
        using var env = new Environment();
        var original = env.Seed();
        env.Migrator().MigrateCopy(Capacities(), ReadyCapacity);
        var before = File.ReadAllBytes(env.V15);
        env.Source().Save(original with { Resources = original.Resources +
            ResourceBundle.Create(("planks", 1m)) });
        var ex = Assert.Throws<GameSaveException>(() => env.Migrator().MigrateCopy(Capacities(), ReadyCapacity));
        Assert.Equal(GameSaveError.Superseded, ex.Error);
        Assert.Equal(before, File.ReadAllBytes(env.V15));
    }

    [Fact]
    public void RollbackIsPossibleByKeepingTheOriginalAndDiscardingOnlyTheNewCopy()
    {
        using var env = new Environment();
        var original = env.Seed();
        var originalBytes = File.ReadAllBytes(env.V14);
        env.Migrator().MigrateCopy(Capacities(), ReadyCapacity);
        File.Delete(env.V15); // Explicit operator/test rollback, never automatic.
        Assert.Equal(originalBytes, File.ReadAllBytes(env.V14));
        Assert.Equal(original.Resources, env.Source().Load().Resources);
        Assert.False(File.Exists(env.V15));
    }

}
