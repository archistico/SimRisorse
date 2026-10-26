using System.Collections.Concurrent;
using SimRisorse.Application.Features.Construction.ConstructBuildings;
using SimRisorse.Application.Features.Population.AssignWorkers;
using SimRisorse.Application.Features.Population.ReleaseWorkers;
using SimRisorse.Application.Features.Sessions.AdvanceTime;
using SimRisorse.Application.Features.Sessions.GetGameState;
using SimRisorse.Application.Features.Sessions.SetPause;
using SimRisorse.Application.Features.Sessions.StartGame;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class FeatureTests
{
    [Fact]
    public void ACompleteEconomicFlowCanRunThroughTheApplicationWithoutAWindow()
    {
        var runtime = new GameRuntime(EconomyFoundationScenario.Create());
        var construct = new ConstructBuildingsHandler(runtime);
        var assign = new AssignWorkersHandler(runtime);
        Assert.True(construct.Handle(new("tree-feller", 1)).Succeeded);
        Assert.True(assign.Handle(new("tree-feller", 1)).Succeeded);
        var before = new GetGameStateHandler(runtime).Handle();
        Assert.Equal(4, before.AvailableWorkers);
        Assert.True(before.IsPaused);
        new SetPauseHandler(runtime).Handle(new(false));
        var after = new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(2))).View;
        Assert.Equal(6m, after.Resources.Single(item => item.Id == "logs").Quantity);
        Assert.Equal(101m, after.Resources.Single(item => item.Id == "planks").Quantity);
        Assert.Equal(72m, after.Resources.Single(item => item.Id == "stone").Quantity);
        Assert.Equal(2, after.Buildings.Single(item => item.Id == "tree-feller").Count);
        Assert.Equal(0, before.ElapsedTicks);
        Assert.Equal(0m, before.Resources.Single(item => item.Id == "logs").Quantity);
    }

    [Fact]
    public void FailedCommandsReturnAnUnchangedCoherentViewAndTypedReason()
    {
        var runtime = new GameRuntime(EconomyFoundationScenario.Create());
        var construct = new ConstructBuildingsHandler(runtime);
        var failed = construct.Handle(new("sawmill", 100));
        Assert.Equal(GameError.InsufficientResources, failed.Error);
        Assert.Equal(120m, failed.View.Resources.Single(item => item.Id == "planks").Quantity);
        Assert.Equal(1, failed.View.Buildings.Single(item => item.Id == "sawmill").Count);
        var unknown = new AssignWorkersHandler(runtime).Handle(new("missing", 1));
        Assert.Equal(GameError.UnknownBuilding, unknown.Error);
        Assert.Equal(5, unknown.View.AvailableWorkers);
    }

    [Fact]
    public void PausingAndReleasingWorkersDoNotAdvanceTheGame()
    {
        var runtime = new GameRuntime(EconomyFoundationScenario.Create());
        var pause = new SetPauseHandler(runtime);
        var advance = new AdvanceTimeHandler(runtime);
        pause.Handle(new(false));
        advance.Handle(new(TimeSpan.FromMilliseconds(500)));
        pause.Handle(new(true));
        var release = new ReleaseWorkersHandler(runtime).Handle(new("sawmill", 1));
        var view = advance.Handle(new(TimeSpan.FromHours(8))).View;
        Assert.True(release.Succeeded);
        Assert.Equal(TimeSpan.TicksPerSecond / 2, view.ElapsedTicks);
        Assert.Equal(6, view.AvailableWorkers);
        Assert.True(view.IsPaused);
        Assert.Equal(120m, view.Resources.Single(item => item.Id == "planks").Quantity);
    }

    [Fact]
    public void StartingAgainResetsResourcesAssignmentsTimeAndPause()
    {
        var scenario = EconomyFoundationScenario.Create();
        var runtime = new GameRuntime(scenario);
        new ConstructBuildingsHandler(runtime).Handle(new("tree-feller", 2));
        new SetPauseHandler(runtime).Handle(new(false));
        new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(60)));
        var reset = new StartGameHandler(runtime).Handle(new(scenario));
        Assert.True(reset.IsPaused);
        Assert.Equal(0, reset.ElapsedTicks);
        Assert.Equal(5, reset.AvailableWorkers);
        Assert.Equal(120m, reset.Resources.Single(item => item.Id == "planks").Quantity);
        Assert.All(reset.Buildings, item => Assert.Equal(1, item.Count));
    }

    [Fact]
    public void ConcurrentConstructionAndReadsCannotDoubleSpend()
    {
        var runtime = new GameRuntime(EconomyFoundationScenario.Create());
        var handler = new ConstructBuildingsHandler(runtime);
        var results = new ConcurrentBag<CommandResult>();
        Parallel.For(0, 100, _ => results.Add(handler.Handle(new("tree-feller", 1))));
        Assert.Equal(6, results.Count(item => item.Succeeded));
        foreach (var result in results)
        {
            var count = result.View.Buildings.Single(item => item.Id == "tree-feller").Count;
            Assert.Equal(120m - (count - 1) * 20m, result.View.Resources.Single(item => item.Id == "planks").Quantity);
            Assert.Equal(80m - (count - 1) * 10m, result.View.Resources.Single(item => item.Id == "stone").Quantity);
        }
        var final = new GetGameStateHandler(runtime).Handle();
        Assert.Equal(7, final.Buildings.Single(item => item.Id == "tree-feller").Count);
    }

    [Fact]
    public void ConcurrentAssignmentsCannotDuplicatePeople()
    {
        var runtime = new GameRuntime(EconomyFoundationScenario.Create());
        new ConstructBuildingsHandler(runtime).Handle(new("tree-feller", 6));
        var assign = new AssignWorkersHandler(runtime);
        var results = new ConcurrentBag<CommandResult>();
        Parallel.For(0, 100, _ => results.Add(assign.Handle(new("tree-feller", 1))));
        Assert.Equal(5, results.Count(item => item.Succeeded));
        Assert.All(results, result => Assert.Equal(8, result.View.AvailableWorkers + result.View.Buildings.Sum(item => item.AssignedWorkers)));
        Assert.Equal(0, new GetGameStateHandler(runtime).Handle().AvailableWorkers);
    }

    [Fact]
    public void FoundationScenarioRetainsTheVerifiedMinuteTimeline()
    {
        var runtime = new GameRuntime(EconomyFoundationScenario.Create());
        new SetPauseHandler(runtime).Handle(new(false));
        var view = new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(60))).View;
        Assert.Equal(2m, view.Resources.Single(item => item.Id == "logs").Quantity);
        Assert.Equal(179m, view.Resources.Single(item => item.Id == "planks").Quantity);
        Assert.Equal(140m, view.Resources.Single(item => item.Id == "stone").Quantity);
    }
}
