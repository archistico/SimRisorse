using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class GoldIndustryTests
{
    [Fact]
    public void GoldStartsFromTheCivilianTimelineAndBothSmeltersShareOneCoalMine()
    {
        var initial = GameSession.Create(CivilGrowthScenario.Create());
        Assert.Equal(GameError.MissingPrerequisite, initial.Construct("gold-mine", 1).Error);
        Assert.Equal(GameError.MissingPrerequisite, initial.Construct("gold-smelter", 1).Error);
        var game = StartGold();
        Assert.Equal(0m, game.Resources["gold"].Units);
        Assert.Equal(0, game.ForecastProduction().Single(item => item.BuildingId == "gold-smelter").RunnableWorkers);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(1m, game.Resources["gold-ore"].Units);
        Assert.Equal(0m, game.Resources["gold"].Units);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(1m, game.Resources["gold"].Units);
        Assert.Equal(401m, game.Resources["iron"].Units);
        Assert.Equal(1502 * TimeSpan.TicksPerSecond, game.ElapsedTicks);
        var coal = game.Resources["coal"];
        Assert.True(game.Advance(TimeSpan.FromSeconds(10)).Succeeded);
        Assert.Equal(coal, game.Resources["coal"]);
        Assert.Equal(11m, game.Resources["gold"].Units);
        Assert.Equal(411m, game.Resources["iron"].Units);
    }

    [Fact]
    public void ScarceCoalGoesToIronAndASecondCoalMineRestoresGoldProduction()
    {
        var started = StartGold();
        var game = GameSession.Restore(started.Scenario, started.Snapshot() with
        {
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m), ("food", 100m), ("bread", 100m),
                ("iron-ore", 100m), ("gold-ore", 100m))
        });
        BuildAndStaff(game, "iron-mine");
        BuildAndStaff(game, "iron-smelter");
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(2)).Succeeded);
        Assert.Equal(2m, game.Resources["iron"].Units);
        Assert.Equal(0m, game.Resources["gold"].Units);
        var gold = game.ForecastProduction().Single(item => item.BuildingId == "gold-smelter");
        Assert.Equal(0, gold.RunnableWorkers);
        Assert.Contains("coal", gold.MissingInputIds);
        BuildAndStaff(game, "coal-mine");
        Assert.True(game.Advance(TimeSpan.FromSeconds(2)).Succeeded);
        Assert.Equal(6m, game.Resources["iron"].Units);
        Assert.Equal(1m, game.Resources["gold"].Units);
        Assert.Equal(1, game.ForecastProduction().Single(item => item.BuildingId == "gold-smelter").RunnableWorkers);
    }

    [Fact]
    public void FullGoldStorageStopsSmeltingAndPaidExpansionRestartsIt()
    {
        var game = StartGold();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1100)).Succeeded);
        Assert.Equal(1000m, game.Resources["gold"].Units);
        Assert.True(game.ForecastProduction().Single(item => item.BuildingId == "gold-smelter")
            .BlockReason.HasFlag(ProductionBlockReason.StorageFull));
        Assert.True(game.Construct("warehouse", 1).Succeeded);
        Assert.Equal(1500m, game.StorageCapacity["gold"].Units);
        Assert.Equal(1500m, game.StorageCapacity["gold-ore"].Units);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(1001m, game.Resources["gold"].Units);
    }

    [Fact]
    public void TheV6IronCatalogRemainsAvailableWithoutGold()
    {
        var old = CivilGrowthScenario.CreateIronForBenchmarks();
        Assert.Equal("civil-growth-6", old.Catalog.RulesVersion);
        Assert.Equal(15, old.Catalog.Buildings.Length);
        Assert.Equal(12, old.Catalog.Resources.Length);
        Assert.True(old.Catalog.BuildingsById.ContainsKey("iron-smelter"));
        Assert.False(old.Catalog.BuildingsById.ContainsKey("gold-smelter"));
        Assert.False(old.Catalog.ResourcesById.ContainsKey("gold"));
    }

    [Theory]
    [InlineData("gold-ore")]
    [InlineData("gold")]
    public async Task V6CannotClaimGoldAndFailedLoadingDoesNotReplaceTheCurrentGame(string id)
    {
        var runtime = new GameRuntime(CivilGrowthScenario.Create());
        var previous = GameSession.Create(CivilGrowthScenario.CreateIronForBenchmarks()).Snapshot() with
        { Resources = ResourceBundle.Create((id, 1m)) };
        var loaded = await new LoadGameHandler(runtime, new SnapshotStore(previous)).HandleAsync();
        Assert.Equal(GameSaveError.IncompatibleRules, loaded.Error);
        Assert.Equal(CivilGrowthScenario.RulesVersion, loaded.View.RulesVersion);
        Assert.Equal(0m, loaded.View.Resources.Single(item => item.Id == id).Quantity);
        Assert.Equal(120m, loaded.View.Resources.Single(item => item.Id == "planks").Quantity);
    }

    private static GameSession StartGold()
    {
        var game = IronIndustryTests.StartIndustry();
        Assert.True(game.Advance(TimeSpan.FromSeconds(400)).Succeeded);
        BuildAndStaff(game, "gold-mine");
        BuildAndStaff(game, "gold-smelter");
        return game;
    }

    private static void BuildAndStaff(GameSession game, string id)
    {
        Assert.True(game.Construct(id, 1).Succeeded);
        Assert.True(game.AssignWorkers(id, 1).Succeeded);
    }

    private sealed class SnapshotStore(GameSnapshot snapshot) : IGameSaveStore
    {
        public GameSnapshot Load() => snapshot;
        public void Save(GameSnapshot value) => throw new NotSupportedException();
    }
}
