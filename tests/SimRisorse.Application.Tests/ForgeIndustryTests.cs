using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class ForgeIndustryTests
{
    [Fact]
    public void ForgeHasApprovedCostRecipePriorityAndPrerequisiteWithoutChangingV7()
    {
        var current = CivilGrowthScenario.CreateForgeForBenchmarks();
        var forge = current.Catalog.BuildingsById["forge"];
        Assert.Equal("civil-growth-8", current.Catalog.RulesVersion);
        Assert.Equal(18, current.Catalog.Buildings.Length);
        Assert.Equal(15, current.Catalog.Resources.Length);
        Assert.Equal(240m, forge.Cost["planks"].Units);
        Assert.Equal(180m, forge.Cost["stone"].Units);
        Assert.Equal(1, forge.WorkplacesPerBuilding);
        Assert.Equal(100, forge.AllocationPriority);
        Assert.Equal(new[] { "iron-smelter" }, forge.Prerequisites);
        Assert.Equal(1m, forge.Recipe!.Input["iron"].Units);
        Assert.Equal(1m, forge.Recipe.Input["coal"].Units);
        Assert.Equal(2m, forge.Recipe.Output["swords"].Units);
        Assert.Equal(1000m, current.Capacity["swords"].Units);
        Assert.Equal(500m, current.Catalog.BuildingsById["warehouse"].StoragePerBuilding!["swords"].Units);
        Assert.Equal(GameError.MissingPrerequisite, GameSession.Create(current).Construct("forge", 1).Error);

        var old = CivilGrowthScenario.CreateGoldForBenchmarks();
        Assert.Equal("civil-growth-7", old.Catalog.RulesVersion);
        Assert.Equal(17, old.Catalog.Buildings.Length);
        Assert.Equal(14, old.Catalog.Resources.Length);
        Assert.False(old.Catalog.BuildingsById.ContainsKey("forge"));
        Assert.False(old.Catalog.ResourcesById.ContainsKey("swords"));
        var newScenario = CivilGrowthScenario.Create();
        Assert.Equal("civil-growth-13", newScenario.Catalog.RulesVersion);
        Assert.Equal(forge.AllocationPriority, newScenario.Catalog.BuildingsById["forge"].AllocationPriority);
        Assert.Equal(forge.Cost["planks"], newScenario.Catalog.BuildingsById["forge"].Cost["planks"]);
    }

    [Fact]
    public void ForgeIsPurchasedAndStaffedAndUsesMetalOnlyFromThePreviousSecond()
    {
        var scenario = CivilGrowthScenario.Create();
        var game = Seed(scenario, ResourceBundle.Create(("iron-ore", 10m), ("coal", 10m)),
            ironWorkers: 1, forgeWorkers: 0, forgeCount: 0);
        var planks = game.Resources["planks"].Units;
        var stone = game.Resources["stone"].Units;
        Assert.True(game.Construct("forge", 1).Succeeded);
        Assert.Equal(planks - 240m, game.Resources["planks"].Units);
        Assert.Equal(stone - 180m, game.Resources["stone"].Units);
        Assert.True(game.AssignWorkers("forge", 1).Succeeded);
        Assert.Equal(0, Forecast(game, "forge").RunnableWorkers);
        Assert.Contains("iron", Forecast(game, "forge").MissingInputIds);

        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(1m, game.Resources["iron"].Units);
        Assert.Equal(0m, game.Resources["swords"].Units);
        Assert.Equal(1, Forecast(game, "forge").RunnableWorkers);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(2m, game.Resources["swords"].Units);
        Assert.Equal(1m, game.Resources["iron"].Units);
        Assert.Equal(7m, game.Resources["coal"].Units);
    }

    [Theory]
    [InlineData(1048, true, 1050)]
    [InlineData(1049, false, 1049)]
    public void ForgeProductionHonorsIndivisibleTwoSwordLots(int startingSwords, bool canProduce, int expectedSwords)
    {
        var scenario = CivilGrowthScenario.Create();
        var game = Seed(scenario, ResourceBundle.Create(("iron", 10m), ("coal", 10m), ("swords", startingSwords)),
            ironWorkers: 0, forgeWorkers: 1);
        var forecast = Forecast(game, "forge");
        Assert.Equal(canProduce ? 1 : 0, forecast.RunnableWorkers);
        Assert.Equal(!canProduce, forecast.BlockReason.HasFlag(ProductionBlockReason.StorageFull));
        Assert.Equal(!canProduce, forecast.FullOutputIds.Contains("swords"));
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal((decimal)expectedSwords, game.Resources["swords"].Units);
        Assert.Equal(canProduce ? 9m : 10m, game.Resources["iron"].Units);
        if (!canProduce)
        {
            Assert.True(game.Construct("warehouse", 1).Succeeded);
            Assert.Equal(1550m, game.StorageCapacity["swords"].Units);
            Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
            Assert.Equal(1051m, game.Resources["swords"].Units);
        }
    }

    [Fact]
    public void ScarceCoalGoesToIronThenGoldSmeltersBeforeForge()
    {
        var scenario = CivilGrowthScenario.Create();
        var game = Seed(scenario, ResourceBundle.Create(("iron-ore", 10m), ("gold-ore", 10m),
            ("iron", 10m), ("coal", 2m)), ironWorkers: 1, goldWorkers: 1, forgeWorkers: 1);
        Assert.Equal(1, Forecast(game, "iron-smelter").RunnableWorkers);
        Assert.Equal(1, Forecast(game, "gold-smelter").RunnableWorkers);
        Assert.Equal(0, Forecast(game, "forge").RunnableWorkers);
        Assert.Contains("coal", Forecast(game, "forge").MissingInputIds);
        Assert.True(game.ReleaseWorkers("iron-smelter", 1).Succeeded);
        Assert.Equal(1, Forecast(game, "gold-smelter").RunnableWorkers);
        Assert.Equal(1, Forecast(game, "forge").RunnableWorkers);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(2m, game.Resources["swords"].Units);
        Assert.Equal(1m, game.Resources["gold"].Units);
        Assert.Equal(0m, game.Resources["coal"].Units);
    }

    [Fact]
    public async Task AClaimOfSwordsInAnOldV7CheckpointIsNotSilentlyUpgraded()
    {
        var old = GameSession.Create(CivilGrowthScenario.CreateGoldForBenchmarks()).Snapshot() with
        { Resources = ResourceBundle.Create(("swords", 1m)) };
        var runtime = new GameRuntime(CivilGrowthScenario.Create());
        var result = await new LoadGameHandler(runtime, new SnapshotStore(old)).HandleAsync();
        Assert.Equal(GameSaveError.IncompatibleRules, result.Error);
        Assert.Equal(CivilGrowthScenario.RulesVersion, result.View.RulesVersion);
        Assert.Equal(0m, result.View.Resources.Single(item => item.Id == "swords").Quantity);
    }

    private static GameSession Seed(SimRisorse.Core.Content.GameScenario scenario, ResourceBundle industrialStocks,
        int ironWorkers, int forgeWorkers, int goldWorkers = 0, int forgeCount = 1)
    {
        var original = GameSession.Create(scenario).Snapshot();
        return GameSession.Restore(scenario, original with
        {
            Population = 40,
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m),
                ("food", 100m), ("bread", 100m)) + industrialStocks,
            Buildings = [.. original.Buildings.Select(item => item.BuildingId switch
            {
                "large-house" => item with { Count = 1 },
                "iron-mine" or "coal-mine" or "gold-mine" => item with { Count = 1 },
                "iron-smelter" => item with { Count = 1, AssignedWorkers = ironWorkers },
                "gold-smelter" => item with { Count = 1, AssignedWorkers = goldWorkers },
                "forge" => item with { Count = forgeCount, AssignedWorkers = forgeWorkers },
                _ => item
            })]
        });
    }

    private static BuildingProduction Forecast(GameSession game, string id) =>
        game.ForecastProduction().Single(item => item.BuildingId == id);

    private sealed class SnapshotStore(GameSnapshot snapshot) : IGameSaveStore
    {
        public GameSnapshot Load() => snapshot;
        public void Save(GameSnapshot value) => throw new NotSupportedException();
    }
}
