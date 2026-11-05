using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class F1D05SqliteFoodPreviewTests
{
    private static readonly SchoolResearchRules School = new(
        "food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);

    private static FoodConversionPolicy Policy() => UnifiedFoodInputs.CreatePolicy([
        new("fish", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 0),
        new("bread", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 1),
        new("meat", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 2),
        new("fruit", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 3),
        new("vegetables", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 4)
    ]);

    [Fact]
    public void PreviewOfRealV14SqliteCheckpointLeavesItsBytesAndBackupDirectoryUntouched()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-food-preflight-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var path = Path.Combine(folder, "v14.sqlite3");
            var scenario = CivilGrowthScenario.Create();
            var store = new V14SqliteSaveStore(path, scenario, School, TechnologyCatalog.ProposedTierTree);
            var original = V14CampaignSnapshots.MigrateFromV13(scenario,
                GameSession.Create(scenario).Snapshot(), School, TechnologyCatalog.ProposedTierTree);
            store.Save(original);
            var before = File.ReadAllBytes(path);
            var beforeFiles = Directory.GetFiles(folder).OrderBy(x => x, StringComparer.Ordinal).ToArray();

            var plan = store.PreviewFoodMigration(Policy());
            Assert.Equal(original.Resources, V14FoodStockMigration.RestoreLegacyResources(plan, Policy()));
            Assert.Equal(before, File.ReadAllBytes(path));
            Assert.Equal(beforeFiles, Directory.GetFiles(folder).OrderBy(x => x, StringComparer.Ordinal).ToArray());
            Assert.Equal(original.Resources, store.Load().Resources);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public void PreviewDoesNotCreateAFileWhenV14CheckpointIsMissing()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-food-missing-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var path = Path.Combine(folder, "missing.sqlite3");
            var store = new V14SqliteSaveStore(path, CivilGrowthScenario.Create(),
                School, TechnologyCatalog.ProposedTierTree);
            Assert.Throws<SimRisorse.Application.Persistence.GameSaveException>(() =>
                store.PreviewFoodMigration(Policy()));
            Assert.Empty(Directory.GetFiles(folder));
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}
