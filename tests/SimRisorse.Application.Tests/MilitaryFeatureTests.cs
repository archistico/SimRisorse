using SimRisorse.Application.Features.Military.CancelQueuedRecruits;
using SimRisorse.Application.Features.Military.DischargeSoldiers;
using SimRisorse.Application.Features.Military.EnlistRecruits;
using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Features.Sessions.AdvanceTime;
using SimRisorse.Application.Features.Sessions.GetGameState;
using SimRisorse.Application.Features.Sessions.SetPause;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class MilitaryFeatureTests
{
    private static GameSnapshot Seed(decimal swords = 40m)
    {
        var initial = GameSession.Create(CivilGrowthScenario.CreateTownHallForBenchmarks()).Snapshot();
        return initial with
        {
            Population = 40,
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m),
                ("food", 100m), ("bread", 100m), ("swords", swords)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId is "large-house" or "forge" or "barracks"
                ? item with { Count = 1 } : item)]
        };
    }

    [Fact]
    public async Task FeatureHandlersEnlistWaitTrainCancelAndDischargeWithConsistentReadModels()
    {
        var runtime = new GameRuntime(CivilGrowthScenario.CreateTownHallForBenchmarks());
        Assert.True((await new LoadGameHandler(runtime, new SnapshotStore(Seed())).HandleAsync()).Succeeded);

        var enlist = new EnlistRecruitsHandler(runtime);
        Assert.True(enlist.Handle(new(5)).Succeeded);
        var after = new GetGameStateHandler(runtime).Handle();
        Assert.Equal(30m, after.Resources.Single(item => item.Id == "swords").Quantity);
        Assert.Equal(32, after.AvailableWorkers);
        Assert.Equal(40, after.Population);
        Assert.Equal((3, 2, 0, 2L), (after.Military!.WaitingRecruits,
            after.Military.TrainingRecruits, after.Military.AvailableSoldiers, after.Military.Seats));

        var cancel = new CancelQueuedRecruitsHandler(runtime);
        Assert.True(cancel.Handle(new(1)).Succeeded);
        Assert.Equal(32m, cancel.Handle(new(5)).View.Resources.Single(item => item.Id == "swords").Quantity);
        Assert.Equal(GameError.InsufficientQueuedRecruits, cancel.Handle(new(5)).Error);

        new SetPauseHandler(runtime).Handle(new(false));
        var midway = new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(30))).View;
        Assert.Equal(30, midway.Military!.TrainingProgressSeconds);
        var trained = new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(30))).View;
        Assert.Equal((0, 2, 2), (trained.Military!.WaitingRecruits,
            trained.Military.TrainingRecruits, trained.Military.AvailableSoldiers));

        var discharge = new DischargeSoldiersHandler(runtime);
        Assert.True(discharge.Handle(new(1)).Succeeded);
        var finished = new GetGameStateHandler(runtime).Handle();
        Assert.Equal(1, finished.Military!.AvailableSoldiers);
        Assert.Equal(34, finished.AvailableWorkers);
        Assert.Equal(32m, finished.Resources.Single(item => item.Id == "swords").Quantity);
        Assert.Equal(40, finished.Population);
    }

    [Fact]
    public async Task CancelWaitingDoesNotRefundPartiallyWhenSwordStorageIsFull()
    {
        var runtime = new GameRuntime(CivilGrowthScenario.CreateTownHallForBenchmarks());
        var snapshot = Seed(1000m);
        Assert.True((await new LoadGameHandler(runtime, new SnapshotStore(snapshot)).HandleAsync()).Succeeded);
        Assert.True(new EnlistRecruitsHandler(runtime).Handle(new(3)).Succeeded);
        var fresh = new GetGameStateHandler(runtime).Handle();
        var filled = snapshot with
        {
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m),
                ("food", 100m), ("bread", 100m), ("swords", 1070m)),
            Military = new SimRisorse.Core.Military.MilitaryTrainingState(1, 2, 0, 0)
        };
        Assert.Equal(994m, fresh.Resources.Single(item => item.Id == "swords").Quantity);
        Assert.True((await new LoadGameHandler(runtime, new SnapshotStore(filled)).HandleAsync()).Succeeded);
        var rejected = new CancelQueuedRecruitsHandler(runtime).Handle(new(1));
        Assert.Equal(GameError.InsufficientStorage, rejected.Error);
        Assert.Equal(1070m, rejected.View.Resources.Single(item => item.Id == "swords").Quantity);
        Assert.Equal(1, rejected.View.Military!.WaitingRecruits);
        Assert.Equal(2, rejected.View.Military.TrainingRecruits);
    }

    [Fact]
    public void HistoricalFoundationDoesNotAcceptMilitaryCommands()
    {
        var runtime = new GameRuntime(EconomyFoundationScenario.Create());
        Assert.Null(new GetGameStateHandler(runtime).Handle().Military);
        Assert.Equal(GameError.MissingBarracks, new EnlistRecruitsHandler(runtime).Handle(new(1)).Error);
        Assert.Equal(GameError.MissingBarracks, new DischargeSoldiersHandler(runtime).Handle(new(1)).Error);
        Assert.Equal(GameError.MissingBarracks, new CancelQueuedRecruitsHandler(runtime).Handle(new(1)).Error);
    }

    private sealed class SnapshotStore(GameSnapshot snapshot) : IGameSaveStore
    {
        public GameSnapshot Load() => snapshot;
        public void Save(GameSnapshot updated) => throw new InvalidOperationException("Read-only test fixture.");
    }
}
