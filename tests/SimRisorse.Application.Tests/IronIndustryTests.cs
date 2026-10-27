using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class IronIndustryTests
{
    [Fact]
    public void CivilianBootstrapUnlocksIndustryAndNewlyMinedInputsWaitUntilTheNextSecond()
    {
        var initial = GameSession.Create(CivilGrowthScenario.Create());
        Assert.Equal(GameError.MissingPrerequisite, initial.Construct("iron-mine", 1).Error);
        Assert.Equal(GameError.MissingPrerequisite, initial.Construct("coal-mine", 1).Error);
        Assert.Equal(GameError.MissingPrerequisite, initial.Construct("iron-smelter", 1).Error);

        var game = StartIndustry();
        Assert.Equal(0m, game.Resources["iron"].Units);
        Assert.Equal(0, game.ForecastProduction().Single(item => item.BuildingId == "iron-smelter").RunnableWorkers);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(2m, game.Resources["iron-ore"].Units);
        Assert.Equal(2m, game.Resources["coal"].Units);
        Assert.Equal(0m, game.Resources["iron"].Units);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(2m, game.Resources["iron-ore"].Units);
        Assert.Equal(3m, game.Resources["coal"].Units);
        Assert.Equal(1m, game.Resources["iron"].Units);
        Assert.Equal(1102 * TimeSpan.TicksPerSecond, game.ElapsedTicks);
    }

    [Fact]
    public void FullIronStorageStopsSmeltingAndAPaidWarehouseRestartsIt()
    {
        var game = StartIndustry();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1100)).Succeeded);
        Assert.Equal(1000m, game.Resources["iron"].Units);
        var blocked = game.ForecastProduction().Single(item => item.BuildingId == "iron-smelter");
        Assert.True(blocked.BlockReason.HasFlag(ProductionBlockReason.StorageFull));
        Assert.Equal(0, blocked.RunnableWorkers);
        Assert.True(game.Construct("warehouse", 1).Succeeded);
        foreach (var id in new[] { "iron-ore", "coal", "iron" })
            Assert.Equal(1500m, game.StorageCapacity[id].Units);
        Assert.Equal(1, game.ForecastProduction().Single(item => item.BuildingId == "iron-smelter").RunnableWorkers);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(1001m, game.Resources["iron"].Units);
    }

    [Fact]
    public void HistoricalCivilianCatalogsRemainSeparateFromTheIndustrialRules()
    {
        foreach (var historical in new[] { CivilGrowthScenario.CreateLegacyForBenchmarks(), CivilGrowthScenario.CreateAcceleratedForBenchmarks() })
        {
            Assert.Equal(12, historical.Catalog.Buildings.Length);
            Assert.Equal(9, historical.Catalog.Resources.Length);
            Assert.False(historical.Catalog.BuildingsById.ContainsKey("iron-smelter"));
            Assert.False(historical.Catalog.ResourcesById.ContainsKey("iron"));
        }
        Assert.Equal("civil-growth-4", CivilGrowthScenario.CreateLegacyForBenchmarks().Catalog.RulesVersion);
        Assert.Equal("civil-growth-5", CivilGrowthScenario.CreateAcceleratedForBenchmarks().Catalog.RulesVersion);
    }

    [Theory]
    [InlineData("iron-ore")]
    [InlineData("coal")]
    [InlineData("iron")]
    public async Task OlderRulesCannotClaimIndustrialResourcesAndFailedLoadKeepsTheCurrentGame(string id)
    {
        var scenario = CivilGrowthScenario.Create();
        var runtime = new GameRuntime(scenario);
        var saved = GameSession.Create(CivilGrowthScenario.CreateAcceleratedForBenchmarks()).Snapshot() with
        { Resources = ResourceBundle.Create((id, 1m)) };
        var result = await new LoadGameHandler(runtime, new SnapshotStore(saved)).HandleAsync();
        Assert.Equal(GameSaveError.IncompatibleRules, result.Error);
        Assert.Equal(CivilGrowthScenario.RulesVersion, result.View.RulesVersion);
        Assert.Equal(120m, result.View.Resources.Single(item => item.Id == "planks").Quantity);
        Assert.Equal(0m, result.View.Resources.Single(item => item.Id == id).Quantity);
    }

    internal static GameSession StartIndustry()
    {
        var game = GameSession.Create(CivilGrowthScenario.Create());
        void Build(string id, bool staff = true)
        {
            Assert.True(game.Construct(id, 1).Succeeded);
            if (staff) Assert.True(game.AssignWorkers(id, 1).Succeeded);
        }
        Build("fisher"); Build("food-store");
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(61)).Succeeded);
        Build("small-house", staff: false);
        Assert.True(game.Advance(TimeSpan.FromSeconds(539)).Succeeded);
        foreach (var id in new[] { "farm", "farm", "well", "mill", "bakery" }) Build(id);
        Assert.True(game.Advance(TimeSpan.FromSeconds(160)).Succeeded);
        Build("large-house", staff: false);
        Assert.True(game.Advance(TimeSpan.FromSeconds(340)).Succeeded);
        foreach (var id in new[] { "iron-mine", "coal-mine", "iron-smelter" }) Build(id);
        return game;
    }

    private sealed class SnapshotStore(GameSnapshot snapshot) : IGameSaveStore
    {
        public GameSnapshot Load() => snapshot;
        public void Save(GameSnapshot value) => throw new NotSupportedException();
    }
}
