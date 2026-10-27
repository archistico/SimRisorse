using SimRisorse.Application.Features.Construction.ConstructBuildings;
using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Features.Population.AssignWorkers;
using SimRisorse.Application.Features.Sessions.AdvanceTime;
using SimRisorse.Application.Features.Sessions.GetGameState;
using SimRisorse.Application.Features.Sessions.SetPause;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class WarehouseTests
{
    [Fact]
    public void WarehouseExpandsEveryCivilResourceAndRestartsFullProduction()
    {
        var runtime = new GameRuntime(CivilGrowthScenario.Create());
        foreach (var id in new[] { "fisher", "food-store" })
        {
            Assert.True(new ConstructBuildingsHandler(runtime).Handle(new(id, 1)).Succeeded);
            Assert.True(new AssignWorkersHandler(runtime).Handle(new(id, 1)).Succeeded);
        }
        new SetPauseHandler(runtime).Handle(new(false));
        var advance = new AdvanceTimeHandler(runtime);
        advance.Handle(new(TimeSpan.FromSeconds(61)));
        Assert.True(new ConstructBuildingsHandler(runtime).Handle(new("small-house", 1)).Succeeded);
        var full = advance.Handle(new(TimeSpan.FromSeconds(1139))).View;
        foreach (var id in new[] { "planks", "stone" })
            Assert.Equal(1000m, Resource(full, id).Quantity);
        Assert.Equal(ProductionBlockReason.StorageFull, full.Buildings.Single(item => item.Id == "stonecutter").BlockReason);
        Assert.Equal(0m, Resource(full, "stone").NetPerSecond);

        var built = new ConstructBuildingsHandler(runtime).Handle(new("warehouse", 1));
        Assert.True(built.Succeeded);
        Assert.All(built.View.Resources.Where(item => item.Id != "bread"), item => Assert.Equal(1500m, item.Capacity));
        Assert.Equal(600m, Resource(built.View, "bread").Capacity);
        Assert.Equal(900m, Resource(built.View, "planks").Quantity);
        Assert.Equal(940m, Resource(built.View, "stone").Quantity);
        Assert.Equal(1, built.View.Buildings.Single(item => item.Id == "stonecutter").RunnableWorkers);
        Assert.Equal(ProductionBlockReason.NotProducer | ProductionBlockReason.NoWorkers,
            built.View.Buildings.Single(item => item.Id == "warehouse").BlockReason);
        Assert.Equal(built.View.Resources.Length, built.View.Buildings.Single(item => item.Id == "warehouse").StoragePerBuilding.Length);

        var before = new GetGameStateHandler(runtime).Handle();
        var after = advance.Handle(new(TimeSpan.FromSeconds(1))).View;
        foreach (var resource in before.Resources)
            Assert.Equal(resource.Quantity + resource.NetPerSecond, Resource(after, resource.Id).Quantity);
        Assert.Equal(941m, Resource(after, "stone").Quantity);
    }

    [Theory]
    [InlineData("civil-growth-1")]
    [InlineData("civil-growth-2")]
    [InlineData("civil-growth-3")]
    public async Task PreviousCivilCheckpointIsConvertedOnLoadKeepingProgress(string rules)
    {
        var scenario = CivilGrowthScenario.Create();
        var game = GameSession.Create(scenario);
        game.Construct("small-house", 1);
        game.Resume();
        game.Advance(TimeSpan.FromMilliseconds(59500));
        // Exercise conversion of a whole-second legacy checkpoint.
        var current = game.Snapshot() with { GrowthProgressSeconds = 59, GrowthRemainderTwentieths = 0 };
        var previous = Older(current, rules);
        Assert.True(previous.Buildings.Length < current.Buildings.Length);
        var runtime = new GameRuntime(scenario);
        var result = await new LoadGameHandler(runtime, new SnapshotStore(previous)).HandleAsync();
        Assert.True(result.Succeeded);
        Assert.Equal(rules, result.UpgradedFromRules);
        Assert.Equal(CivilGrowthScenario.RulesVersion, result.View.RulesVersion);
        Assert.True(result.View.IsPaused);
        foreach (var id in CivilGrowthScenario.Changes.SkipWhile(item => item.From != rules).SelectMany(item => item.AddedBuildings))
            Assert.Equal(0, result.View.Buildings.Single(item => item.Id == id).Count);
        Assert.Equal(1000m, Resource(result.View, "flour").Capacity);
        Assert.Equal(100m, Resource(result.View, "bread").Capacity);
        Assert.Equal(0m, Resource(result.View, "bread").Quantity);
        Assert.Equal(59, result.View.PopulationState.ProgressSeconds);
        Assert.Equal(current.ElapsedTicks, result.View.ElapsedTicks);
        foreach (var resource in result.View.Resources)
            Assert.Equal(current.Resources[resource.Id].Units, resource.Quantity);

        var same = await new LoadGameHandler(runtime, new SnapshotStore(current)).HandleAsync();
        Assert.True(same.Succeeded);
        Assert.Null(same.UpgradedFromRules);
    }

    [Fact]
    public async Task UnknownOrInconsistentRulesAreNotConverted()
    {
        var scenario = CivilGrowthScenario.Create();
        var current = GameSession.Create(scenario).Snapshot();
        var runtime = new GameRuntime(scenario);
        Assert.True(new ConstructBuildingsHandler(runtime).Handle(new("small-house", 1)).Succeeded);

        var unknown = await new LoadGameHandler(runtime, new SnapshotStore(current with { RulesVersion = "civil-growth-9" })).HandleAsync();
        Assert.Equal(GameSaveError.IncompatibleRules, unknown.Error);
        var otherScenario = await new LoadGameHandler(runtime,
            new SnapshotStore(Older(current, "civil-growth-1") with { ScenarioId = "economy-foundation" })).HandleAsync();
        Assert.Equal(GameSaveError.IncompatibleRules, otherScenario.Error);
        foreach (var rules in new[] { "civil-growth-1", "civil-growth-2", "civil-growth-3" })
        {
            var alreadyHasNewBuildings = await new LoadGameHandler(runtime,
                new SnapshotStore(current with { RulesVersion = rules })).HandleAsync();
            Assert.Equal(GameSaveError.InvalidData, alreadyHasNewBuildings.Error);
            Assert.Null(alreadyHasNewBuildings.UpgradedFromRules);
            Assert.Equal(2, alreadyHasNewBuildings.View.Buildings.Single(item => item.Id == "small-house").Count);
        }
        var breadBeforeItsIntroduction = await new LoadGameHandler(runtime,
            new SnapshotStore(Older(current, "civil-growth-3") with
            { Resources = ResourceBundle.Create(("bread", 1m)) })).HandleAsync();
        Assert.Equal(GameSaveError.IncompatibleRules, breadBeforeItsIntroduction.Error);
    }

    /// <summary>Rebuilds the shape an older rules version would have saved, by removing later building types.</summary>
    internal static GameSnapshot Older(GameSnapshot current, string rules)
    {
        var later = CivilGrowthScenario.Changes.SkipWhile(item => item.From != rules).SelectMany(item => item.AddedBuildings).ToHashSet();
        return current with { RulesVersion = rules, Buildings = [.. current.Buildings.Where(item => !later.Contains(item.BuildingId))] };
    }

    private static ReadModels.ResourceView Resource(ReadModels.GameView view, string id) => view.Resources.Single(item => item.Id == id);

    private sealed class SnapshotStore(GameSnapshot snapshot) : IGameSaveStore
    {
        public GameSnapshot Load() => snapshot;
        public void Save(GameSnapshot value) => throw new NotSupportedException();
    }
}
