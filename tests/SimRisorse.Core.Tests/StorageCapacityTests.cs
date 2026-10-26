using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class StorageCapacityTests
{
    [Fact]
    public void StorageBuildingRaisesCapacityAndFullProductionResumesOnTheNextStep()
    {
        var game = GameSession.Create(Scenario());
        Assert.Equal(10m, game.StorageCapacity["stone"].Units);
        Assert.Equal(ProductionBlockReason.StorageFull, Stonecutter(game).BlockReason);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(3));
        Assert.Equal(10m, game.Resources["stone"].Units);
        Assert.True(game.Construct("store", 1).Succeeded);
        Assert.Equal(15m, game.StorageCapacity["stone"].Units);
        Assert.Equal(1000m, game.StorageCapacity["logs"].Units);
        Assert.Equal(1, Stonecutter(game).RunnableWorkers);
        Assert.Equal(ProductionBlockReason.NotProducer | ProductionBlockReason.NoWorkers, game.ForecastProduction().Single(item => item.BuildingId == "store").BlockReason);
        game.Advance(TimeSpan.FromSeconds(5));
        Assert.Equal(15m, game.Resources["stone"].Units);
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(15m, game.Resources["stone"].Units);
        Assert.Equal(ProductionBlockReason.StorageFull, Stonecutter(game).BlockReason);
    }

    [Fact]
    public void RestoredStocksAreCheckedAgainstTheStorageOfTheSavedBuildings()
    {
        var scenario = Scenario();
        var game = GameSession.Create(scenario);
        game.Construct("store", 2);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(20));
        var saved = game.Snapshot();
        Assert.Equal(20m, saved.Resources["stone"].Units);
        TestScenarios.EqualState(saved, GameSession.Restore(scenario, saved).Snapshot());
        var fewerStores = saved with { Buildings = [.. saved.Buildings.Select(item => item.BuildingId == "store" ? item with { Count = 1 } : item)] };
        Assert.Throws<ArgumentException>(() => GameSession.Restore(scenario, fewerStores));
        var overflow = saved with { Buildings = [.. saved.Buildings.Select(item => item.BuildingId == "store" ? item with { Count = int.MaxValue } : item)] };
        Assert.Throws<ArgumentException>(() => GameSession.Restore(Scenario(storage: 1_000_000_000m), overflow));
    }

    [Fact]
    public void StorageBeyondTheNumericLimitFailsAtomically()
    {
        var game = GameSession.Create(Scenario(storage: 1_000_000_000_000_000m));
        Assert.True(game.Construct("store", 9).Succeeded);
        var before = game.Snapshot();
        Assert.Equal(GameError.NumericLimit, game.Construct("store", 1).Error);
        TestScenarios.EqualState(before, game.Snapshot());
    }

    [Fact]
    public void InitialStocksMayUseTheStorageOfInitialBuildingsButNotMore()
    {
        Assert.Equal(15m, Scenario(stone: 15m, initialStores: 1).InitialResources["stone"].Units);
        Assert.Throws<ArgumentException>(() => Scenario(stone: 16m, initialStores: 1));
    }

    [Fact]
    public void StorageEffectsMustReferenceCatalogResources() => Assert.Throws<ArgumentException>(() =>
        new GameCatalog("invalid-1", [new("stone", "Stone")],
            [new("store", "store", ResourceBundle.Empty, 0, null, 5, [], StoragePerBuilding: ResourceBundle.Create(("gold", 1m)))]));

    private static BuildingProduction Stonecutter(GameSession game) => game.ForecastProduction().Single(item => item.BuildingId == "stonecutter");

    private static GameScenario Scenario(decimal storage = 5m, decimal stone = 10m, int initialStores = 0) => TestScenarios.Create(
        [
            TestScenarios.Building("stonecutter", output: ResourceBundle.Create(("stone", 1m)), priority: 30),
            new("store", "store", ResourceBundle.Empty, 0, null, 5, [], StoragePerBuilding: ResourceBundle.Create(("stone", storage)))
        ],
        ResourceBundle.Create(("stone", stone)),
        ResourceBundle.Create(("logs", 1000m), ("planks", 1000m), ("stone", 10m)),
        initial: [new BuildingState("stonecutter", 1, 1), new BuildingState("store", initialStores, 0)]);
}
