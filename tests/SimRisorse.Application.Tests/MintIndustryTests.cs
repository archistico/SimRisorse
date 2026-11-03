using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class MintIndustryTests
{
    [Fact]
    public void MintHasApprovedContractAndFrozenV8KeepsOnlyForgeAndSwords()
    {
        var current = CivilGrowthScenario.Create();
        var mint = current.Catalog.BuildingsById["mint"];
        Assert.Equal("civil-growth-13", current.Catalog.RulesVersion);
        Assert.Equal(21, current.Catalog.Buildings.Length);
        Assert.Equal(16, current.Catalog.Resources.Length);
        Assert.Equal(210m, mint.Cost["planks"].Units);
        Assert.Equal(160m, mint.Cost["stone"].Units);
        Assert.Equal(1, mint.WorkplacesPerBuilding);
        Assert.Equal(101, mint.AllocationPriority);
        Assert.Equal(new[] { "gold-smelter" }, mint.Prerequisites);
        Assert.Equal(1m, mint.Recipe!.Input["gold"].Units);
        Assert.Equal(1m, mint.Recipe.Input["coal"].Units);
        Assert.Equal(3m, mint.Recipe.Output["coins"].Units);
        Assert.Equal(0m, current.Capacity["coins"].Units);
        Assert.Equal(1000m, GameSession.Create(current).StorageCapacity["coins"].Units);
        Assert.Equal(500m, current.Catalog.BuildingsById["warehouse"].StoragePerBuilding!["coins"].Units);
        Assert.Equal(GameError.MissingPrerequisite, GameSession.Create(current).Construct("mint", 1).Error);

        var v8 = CivilGrowthScenario.CreateForgeForBenchmarks();
        Assert.Equal("civil-growth-8", v8.Catalog.RulesVersion);
        Assert.Equal(18, v8.Catalog.Buildings.Length);
        Assert.Equal(15, v8.Catalog.Resources.Length);
        Assert.True(v8.Catalog.BuildingsById.ContainsKey("forge"));
        Assert.True(v8.Catalog.ResourcesById.ContainsKey("swords"));
        Assert.False(v8.Catalog.BuildingsById.ContainsKey("mint"));
        Assert.False(v8.Catalog.ResourcesById.ContainsKey("coins"));
    }

    [Fact]
    public void MintCanBeBuiltAndStaffedButUsesGoldOnlyFromACompletedSecond()
    {
        var game = Seed(ResourceBundle.Create(("gold-ore", 10m), ("coal", 10m)),
            goldWorkers: 1, mintWorkers: 0, mintCount: 0);
        var planks = game.Resources["planks"].Units;
        var stone = game.Resources["stone"].Units;
        Assert.True(game.Construct("mint", 1).Succeeded);
        Assert.Equal(planks - 210m, game.Resources["planks"].Units);
        Assert.Equal(stone - 160m, game.Resources["stone"].Units);
        Assert.True(game.AssignWorkers("mint", 1).Succeeded);
        Assert.Equal(0, Forecast(game, "mint").RunnableWorkers);
        Assert.Contains("gold", Forecast(game, "mint").MissingInputIds);

        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(1m, game.Resources["gold"].Units);
        Assert.Equal(0m, game.Resources["coins"].Units);
        Assert.Equal(1, Forecast(game, "mint").RunnableWorkers);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(3m, game.Resources["coins"].Units);
        Assert.Equal(1m, game.Resources["gold"].Units);
        Assert.Equal(7m, game.Resources["coal"].Units);
    }

    [Theory]
    [InlineData(1047, true, 1050)]
    [InlineData(1048, false, 1048)]
    [InlineData(1049, false, 1049)]
    public void MintRequiresThreeFreeSlotsAndNeverConsumesIngredientsForBlockedLots(
        int startingCoins, bool canProduce, int expectedCoins)
    {
        var game = Seed(ResourceBundle.Create(("gold", 10m), ("coal", 10m), ("coins", startingCoins)),
            goldWorkers: 0, mintWorkers: 1);
        var forecast = Forecast(game, "mint");
        Assert.Equal(canProduce ? 1 : 0, forecast.RunnableWorkers);
        Assert.Equal(!canProduce, forecast.BlockReason.HasFlag(ProductionBlockReason.StorageFull));
        Assert.Equal(!canProduce, forecast.FullOutputIds.Contains("coins"));

        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal((decimal)expectedCoins, game.Resources["coins"].Units);
        Assert.Equal(canProduce ? 9m : 10m, game.Resources["gold"].Units);
        Assert.Equal(canProduce ? 9m : 10m, game.Resources["coal"].Units);

        if (!canProduce)
        {
            Assert.True(game.Construct("warehouse", 1).Succeeded);
            Assert.Equal(1550m, game.StorageCapacity["coins"].Units);
            Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
            Assert.Equal(startingCoins + 3m, game.Resources["coins"].Units);
        }
    }

    [Fact]
    public void ScarceCoalGoesToBothSmeltersAndForgeBeforeMint()
    {
        var game = Seed(ResourceBundle.Create(("iron-ore", 10m), ("gold-ore", 10m),
            ("iron", 10m), ("gold", 10m), ("coal", 3m)),
            goldWorkers: 1, mintWorkers: 1, ironWorkers: 1, forgeWorkers: 1);
        Assert.Equal(1, Forecast(game, "iron-smelter").RunnableWorkers);
        Assert.Equal(1, Forecast(game, "gold-smelter").RunnableWorkers);
        Assert.Equal(1, Forecast(game, "forge").RunnableWorkers);
        Assert.Equal(0, Forecast(game, "mint").RunnableWorkers);
        Assert.Contains("coal", Forecast(game, "mint").MissingInputIds);

        Assert.True(game.ReleaseWorkers("forge", 1).Succeeded);
        Assert.Equal(1, Forecast(game, "mint").RunnableWorkers);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(3m, game.Resources["coins"].Units);
        Assert.Equal(0m, game.Resources["swords"].Units);
        Assert.Equal(0m, game.Resources["coal"].Units);
    }

    [Fact]
    public void MintedCoinsAreNotAHiddenCostOfNormalConstruction()
    {
        var game = Seed(ResourceBundle.Create(("gold", 10m), ("coal", 10m), ("coins", 12m)),
            goldWorkers: 0, mintWorkers: 0);
        var before = game.Resources["coins"].Units;
        Assert.True(game.Construct("warehouse", 1).Succeeded);
        Assert.Equal(before, game.Resources["coins"].Units);
        Assert.Equal(1550m, game.StorageCapacity["coins"].Units);
    }

    [Fact]
    public void FractionalReplayWithSnapshotNeverDuplicatesMintedCoins()
    {
        var oneSecond = Seed(ResourceBundle.Create(("gold", 10m), ("coal", 10m)),
            goldWorkers: 0, mintWorkers: 1);
        var split = Seed(ResourceBundle.Create(("gold", 10m), ("coal", 10m)),
            goldWorkers: 0, mintWorkers: 1);
        oneSecond.Resume();
        split.Resume();
        Assert.True(oneSecond.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.True(split.Advance(TimeSpan.FromMilliseconds(250)).Succeeded);
        Assert.Equal(0m, split.Resources["coins"].Units);

        var restored = GameSession.Restore(CivilGrowthScenario.Create(), split.Snapshot());
        restored.Resume();
        Assert.True(restored.Advance(TimeSpan.FromMilliseconds(750)).Succeeded);
        foreach (var id in new[] { "coins", "gold", "coal" })
            Assert.Equal(oneSecond.Resources[id].Units, restored.Resources[id].Units);
        Assert.Equal(3m, restored.Resources["coins"].Units);
        Assert.Equal(oneSecond.ElapsedTicks, restored.ElapsedTicks);
    }

    [Fact]
    public async Task ACheckpointFromV8CannotClaimCoinsThatDidNotExistUnderItsRules()
    {
        var original = GameSession.Create(CivilGrowthScenario.CreateForgeForBenchmarks()).Snapshot();
        var invalid = original with { Resources = ResourceBundle.Create(("coins", 3m)) };
        var runtime = new GameRuntime(CivilGrowthScenario.Create());
        var result = await new LoadGameHandler(runtime, new SnapshotStore(invalid)).HandleAsync();
        Assert.Equal(GameSaveError.IncompatibleRules, result.Error);
        Assert.Equal(CivilGrowthScenario.RulesVersion, result.View.RulesVersion);
        Assert.Equal(0m, result.View.Resources.Single(item => item.Id == "coins").Quantity);
    }

    private static GameSession Seed(ResourceBundle stocks, int goldWorkers, int mintWorkers,
        int ironWorkers = 0, int forgeWorkers = 0, int mintCount = 1)
    {
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot();
        return GameSession.Restore(scenario, initial with
        {
            Population = 40,
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m),
                ("food", 100m), ("bread", 100m)) + stocks,
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId switch
            {
                "large-house" => item with { Count = 1 },
                "iron-mine" or "coal-mine" or "gold-mine" => item with { Count = 1 },
                "iron-smelter" => item with { Count = 1, AssignedWorkers = ironWorkers },
                "gold-smelter" => item with { Count = 1, AssignedWorkers = goldWorkers },
                "forge" => item with { Count = 1, AssignedWorkers = forgeWorkers },
                "mint" => item with { Count = mintCount, AssignedWorkers = mintWorkers },
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
