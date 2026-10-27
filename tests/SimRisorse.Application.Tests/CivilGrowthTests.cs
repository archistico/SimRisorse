using SimRisorse.Application.Features.Construction.ConstructBuildings;
using SimRisorse.Application.Features.Population.AssignWorkers;
using SimRisorse.Application.Features.Sessions.AdvanceTime;
using SimRisorse.Application.Features.Sessions.GetGameState;
using SimRisorse.Application.Features.Sessions.SetPause;
using SimRisorse.Application.Features.Sessions.StartGame;
using SimRisorse.Application.Scenarios;
using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Persistence;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class CivilGrowthTests
{
    [Fact]
    public void ProjectedResourceBalancesMatchEverySecondIncludingPopulationArrivals()
    {
        var runtime = new GameRuntime(CivilGrowthScenario.CreateLegacyForBenchmarks());
        BuildAndStaffFood(runtime);
        new SetPauseHandler(runtime).Handle(new(false));
        var read = new GetGameStateHandler(runtime);
        for (var second = 1; second <= 181; second++)
        {
            if (second == 62) Assert.True(new ConstructBuildingsHandler(runtime).Handle(new("small-house", 1)).Succeeded);
            var before = read.Handle();
            var after = new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(1))).View;
            foreach (var resource in before.Resources)
                Assert.Equal(resource.Quantity + resource.NetPerSecond, after.Resources.Single(item => item.Id == resource.Id).Quantity);
        }
        Assert.Equal(10, read.Handle().Population);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    [InlineData(60)]
    public async Task InvalidGrowthFromStorageKeepsTheCurrentCivilSession(int? progress)
    {
        var scenario = CivilGrowthScenario.Create();
        var runtime = new GameRuntime(scenario);
        new ConstructBuildingsHandler(runtime).Handle(new("small-house", 1));
        var store = new SnapshotStore(GameSession.Create(scenario).Snapshot() with { GrowthProgressSeconds = progress });
        var result = await new LoadGameHandler(runtime, store).HandleAsync();
        Assert.Equal(GameSaveError.InvalidData, result.Error);
        Assert.Equal(16, result.View.PopulationState.HousingCapacity);
        Assert.True(result.View.IsPaused);
    }

    [Fact]
    public void InitialMaterialsCanStartFoodAndHousingWithoutStoppingBaseProduction()
    {
        var runtime = new GameRuntime(CivilGrowthScenario.CreateLegacyForBenchmarks());
        BuildAndStaffFood(runtime);
        var read = new GetGameStateHandler(runtime);
        Assert.Equal(3, read.Handle().AvailableWorkers);
        Assert.Equal(0.08m, read.Handle().Resources.Single(item => item.Id == "food").ConsumptionPerSecond);
        new SetPauseHandler(runtime).Handle(new(false));
        new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(61)));
        Assert.True(new ConstructBuildingsHandler(runtime).Handle(new("small-house", 1)).Succeeded);
        var before = read.Handle();
        Assert.Equal(8, before.Population);
        Assert.Equal(0, before.PopulationState.ProgressSeconds);
        Assert.Equal(16, before.PopulationState.HousingCapacity);
        Assert.Equal(1m, before.Resources.Single(item => item.Id == "food").ProductionPerSecond);
        new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(60)));
        var result = read.Handle();
        Assert.Equal(9, result.Population);
        Assert.Equal(4, result.AvailableWorkers);
        Assert.Equal(140.32m, result.Resources.Single(item => item.Id == "food").Quantity);
        Assert.Equal(0.09m, result.PopulationState.RequiredFoodPerSecond);
        foreach (var resource in result.Resources)
            Assert.InRange(resource.Quantity, 0m, resource.Capacity);
        new StartGameHandler(runtime).Handle(new());
        Assert.Equal("civil-growth", read.Handle().ScenarioId);
        Assert.Equal(8, read.Handle().Population);
        Assert.Equal(1, read.Handle().Buildings.Single(item => item.Id == "small-house").Count);
    }

    [Fact]
    public void InvestingInHousingFirstAndRunningOutOfFoodIsRecoverable()
    {
        var runtime = new GameRuntime(CivilGrowthScenario.CreateLegacyForBenchmarks());
        Assert.True(new ConstructBuildingsHandler(runtime).Handle(new("small-house", 1)).Succeeded);
        new SetPauseHandler(runtime).Handle(new(false));
        var advance = new AdvanceTimeHandler(runtime);
        advance.Handle(new(TimeSpan.FromSeconds(600)));
        var before = new GetGameStateHandler(runtime).Handle();
        Assert.Equal(13, before.Population);
        Assert.Equal(GrowthBlockReason.MissingFood, before.PopulationState.BlockReason);
        Assert.Equal(0m, before.Resources.Single(item => item.Id == "food").Quantity);
        Assert.Equal(10, before.AvailableWorkers);
        BuildAndStaffFood(runtime);
        var result = advance.Handle(new(TimeSpan.FromSeconds(62))).View;
        Assert.Equal(14, result.Population);
        Assert.Equal(0, result.PopulationState.ProgressSeconds);
        Assert.Equal(GrowthBlockReason.None, result.PopulationState.BlockReason);
        Assert.True(result.Resources.Single(item => item.Id == "food").Quantity > 0m);
    }

    private static void BuildAndStaffFood(GameRuntime runtime)
    {
        foreach (var id in new[] { "fisher", "food-store" })
        {
            Assert.True(new ConstructBuildingsHandler(runtime).Handle(new(id, 1)).Succeeded);
            Assert.True(new AssignWorkersHandler(runtime).Handle(new(id, 1)).Succeeded);
        }
    }

    private sealed class SnapshotStore(GameSnapshot snapshot) : IGameSaveStore
    {
        public GameSnapshot Load() => snapshot;
        public void Save(GameSnapshot value) => throw new NotSupportedException();
    }
}
