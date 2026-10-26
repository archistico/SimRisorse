using SimRisorse.Application.Features.Construction.ConstructBuildings;
using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Features.Persistence.SaveGame;
using SimRisorse.Application.Features.Sessions.AdvanceTime;
using SimRisorse.Application.Features.Sessions.GetGameState;
using SimRisorse.Application.Features.Sessions.SetPause;
using SimRisorse.Application.Features.Sessions.StartGame;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class PersistenceFeatureTests
{
    [Fact]
    public async Task SavedSessionRestoresInPauseWithItsFractionalTime()
    {
        var runtime = CreateRuntime();
        using var store = new ControlledStore();
        new SetPauseHandler(runtime).Handle(new(false));
        new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromMilliseconds(1750)));
        Assert.True((await new SaveGameHandler(runtime, store).HandleAsync()).Succeeded);
        new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(1)));
        var loaded = await new LoadGameHandler(runtime, store).HandleAsync();
        Assert.True(loaded.Succeeded);
        Assert.True(loaded.View.IsPaused);
        Assert.Equal(TimeSpan.FromMilliseconds(1750).Ticks, loaded.View.ElapsedTicks);
    }

    [Theory]
    [InlineData("version", GameSaveError.UnsupportedFormat)]
    [InlineData("rules", GameSaveError.IncompatibleRules)]
    [InlineData("workers", GameSaveError.InvalidData)]
    public async Task IncompatibleOrInvalidSnapshotsDoNotReplaceTheCurrentGame(string change, GameSaveError error)
    {
        var runtime = CreateRuntime();
        using var store = new ControlledStore();
        var initial = store.Snapshot;
        store.Snapshot = change switch
        {
            "version" => initial with { FormatVersion = GameSession.SnapshotFormatVersion + 1 },
            "rules" => initial with { RulesVersion = "unknown" },
            _ => initial with { Buildings = [new("tree-feller", 1, 99), new("sawmill", 1, 1), new("stonecutter", 1, 1)] }
        };
        new ConstructBuildingsHandler(runtime).Handle(new("tree-feller", 1));
        var result = await new LoadGameHandler(runtime, store).HandleAsync();
        Assert.Equal(error, result.Error);
        Assert.Equal(2, result.View.Buildings.Single(item => item.Id == "tree-feller").Count);
        Assert.True(result.View.IsPaused);
    }

    [Fact]
    public async Task ADelayedLoadCannotReplaceANewGame()
    {
        var runtime = CreateRuntime();
        using var store = new ControlledStore { BlockLoad = true };
        var pending = new LoadGameHandler(runtime, store).HandleAsync();
        try
        {
            await store.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            new StartGameHandler(runtime).Handle(new(EconomyFoundationScenario.Create()));
            new ConstructBuildingsHandler(runtime).Handle(new("tree-feller", 2));
        }
        finally { store.Release.Set(); }
        var result = await pending;
        Assert.Equal(GameSaveError.Superseded, result.Error);
        Assert.Equal(3, result.View.Buildings.Single(item => item.Id == "tree-feller").Count);
    }

    [Fact]
    public async Task SavingDoesNotHoldTheRuntimeLockAndCapturesACoherentCheckpoint()
    {
        var runtime = CreateRuntime();
        using var store = new ControlledStore { BlockSave = true };
        var pending = new SaveGameHandler(runtime, store).HandleAsync();
        try
        {
            await store.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            new SetPauseHandler(runtime).Handle(new(false));
            var advanced = new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(1)));
            Assert.True(advanced.Succeeded);
            Assert.Equal(TimeSpan.TicksPerSecond, advanced.View.ElapsedTicks);
        }
        finally { store.Release.Set(); }
        Assert.True((await pending).Succeeded);
        Assert.Equal(0, store.Snapshot.ElapsedTicks);
    }

    [Fact]
    public async Task SavesAreSerializedAndTheFollowingSaveCapturesTheLatestState()
    {
        var runtime = CreateRuntime();
        using var store = new ControlledStore { BlockSave = true };
        var handler = new SaveGameHandler(runtime, store);
        var first = handler.HandleAsync();
        await store.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        new SetPauseHandler(runtime).Handle(new(false));
        new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(1)));
        var second = handler.HandleAsync();
        store.Release.Set();
        Assert.True((await first).Succeeded);
        Assert.True((await second).Succeeded);
        Assert.Equal(TimeSpan.TicksPerSecond, store.Snapshot.ElapsedTicks);
    }

    [Fact]
    public async Task MissingSaveAndCancellationReturnTypedErrors()
    {
        var runtime = CreateRuntime();
        using var store = new ControlledStore { Missing = true };
        Assert.Equal(GameSaveError.NotFound, (await new LoadGameHandler(runtime, store).HandleAsync()).Error);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Equal(GameSaveError.Canceled, (await new SaveGameHandler(runtime, store).HandleAsync(cancellation.Token)).Error);
    }

    [Fact]
    public void ResourceFlowsRespectTheSameContendedProductionPlan()
    {
        var runtime = CreateRuntime();
        var initial = new GetGameStateHandler(runtime).Handle();
        Assert.Equal(2m, initial.Resources.Single(item => item.Id == "logs").NetPerSecond);
        Assert.Equal(0m, initial.Resources.Single(item => item.Id == "planks").ProductionPerSecond);
        new SetPauseHandler(runtime).Handle(new(false));
        var next = new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(1))).View;
        Assert.Equal(2m, next.Resources.Single(item => item.Id == "logs").ProductionPerSecond);
        Assert.Equal(2m, next.Resources.Single(item => item.Id == "logs").ConsumptionPerSecond);
        Assert.Equal(0m, next.Resources.Single(item => item.Id == "logs").NetPerSecond);
        Assert.Equal(1m, next.Resources.Single(item => item.Id == "planks").NetPerSecond);
    }

    private static GameRuntime CreateRuntime() => new(EconomyFoundationScenario.Create());

    private sealed class ControlledStore : IGameSaveStore, IDisposable
    {
        public GameSnapshot Snapshot { get; set; } = GameSession.Create(EconomyFoundationScenario.Create()).Snapshot();
        public bool BlockLoad { get; init; }
        public bool BlockSave { get; init; }
        public bool Missing { get; init; }
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public ManualResetEventSlim Release { get; } = new();
        public void Save(GameSnapshot snapshot) { if (BlockSave) Wait(); Snapshot = snapshot; }
        public GameSnapshot Load()
        {
            if (BlockLoad) Wait();
            if (Missing) throw new GameSaveException(GameSaveError.NotFound, "Missing");
            return Snapshot;
        }
        private void Wait() { Started.TrySetResult(); if (!Release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException(); }
        public void Dispose() => Release.Dispose();
    }
}
