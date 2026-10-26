using SimRisorse.Application.Features.Construction.ConstructBuildings;
using SimRisorse.Application.Features.Population.AssignWorkers;
using SimRisorse.Application.Features.Sessions.AdvanceTime;
using SimRisorse.Application.Features.Sessions.GetGameState;
using SimRisorse.Application.Features.Sessions.SetPause;
using SimRisorse.Application.ReadModels;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class BreadChainTests
{
    [Fact]
    public void MillAndBakeryRequireTheirSuppliers()
    {
        var game = GameSession.Create(CivilGrowthScenario.Create());
        Assert.Equal(GameError.MissingPrerequisite, game.Construct("mill", 1).Error);
        Assert.Equal(GameError.MissingPrerequisite, game.Construct("bakery", 1).Error);
        Assert.True(game.Construct("farm", 1).Succeeded);
        Assert.Equal(GameError.InsufficientResources, game.Construct("mill", 1).Error);
        var restored = GameSession.Restore(game.Scenario, game.Snapshot() with
        {
            Resources = game.Resources + ResourceBundle.Create(("planks", 500m), ("stone", 500m))
        });
        Assert.True(restored.Construct("mill", 1).Succeeded);
        Assert.Equal(GameError.MissingPrerequisite, restored.Construct("bakery", 1).Error);
        Assert.True(restored.Construct("well", 1).Succeeded);
        Assert.True(restored.Construct("bakery", 1).Succeeded);
    }

    [Fact]
    public void BreadChainStartsInOrderAndAddsThreeFoodPerSecond()
    {
        var runtime = new GameRuntime(CivilGrowthScenario.Create());
        Build(runtime, "fisher", 1);
        Build(runtime, "food-store", 1);
        new SetPauseHandler(runtime).Handle(new(false));
        var advance = new AdvanceTimeHandler(runtime);
        advance.Handle(new(TimeSpan.FromSeconds(61)));
        Assert.True(new ConstructBuildingsHandler(runtime).Handle(new("small-house", 1)).Succeeded);
        var ready = advance.Handle(new(TimeSpan.FromSeconds(539))).View;
        Assert.Equal(16, ready.Population);
        Assert.Equal(539m, Resource(ready, "planks").Quantity);
        Assert.Equal(590m, Resource(ready, "stone").Quantity);

        Build(runtime, "farm", 2);
        Build(runtime, "well", 1);
        Build(runtime, "mill", 1);
        Build(runtime, "bakery", 1);
        var read = new GetGameStateHandler(runtime);
        Assert.Equal(119m, Resource(read.Handle(), "planks").Quantity);
        Assert.Equal(310m, Resource(read.Handle(), "stone").Quantity);
        Assert.Equal(6, read.Handle().AvailableWorkers);

        var runnable = new List<(int Mill, int Bakery)>();
        for (var second = 0; second < 10; second++)
        {
            var before = read.Handle();
            runnable.Add((Building(before, "mill").RunnableWorkers, Building(before, "bakery").RunnableWorkers));
            var after = advance.Handle(new(TimeSpan.FromSeconds(1))).View;
            foreach (var resource in before.Resources)
                Assert.Equal(resource.Quantity + resource.NetPerSecond, Resource(after, resource.Id).Quantity);
        }
        Assert.Equal([(0, 0), (1, 0), (1, 1)], runnable.Take(3));
        var running = read.Handle();
        Assert.Equal(4m, Resource(running, "food").ProductionPerSecond);
        Assert.Equal(2m, Resource(running, "grain").Quantity);
        Assert.Equal(1m, Resource(running, "flour").Quantity);
        Assert.Equal(2m, Resource(running, "water").Quantity);
        foreach (var id in new[] { "grain", "flour", "water" })
            Assert.Equal(0m, Resource(running, id).NetPerSecond);
        Assert.Equal(ProductionBlockReason.None, Building(running, "bakery").BlockReason);
    }

    private static void Build(GameRuntime runtime, string id, int count)
    {
        Assert.True(new ConstructBuildingsHandler(runtime).Handle(new(id, count)).Succeeded);
        Assert.True(new AssignWorkersHandler(runtime).Handle(new(id, count)).Succeeded);
    }

    private static ResourceView Resource(GameView view, string id) => view.Resources.Single(item => item.Id == id);
    private static BuildingView Building(GameView view, string id) => view.Buildings.Single(item => item.Id == id);
}
