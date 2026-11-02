using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class ForgeIndustryTests
{
    [Fact]
    public void CivilianTimelinePaysForTheForgeAndProducesSwordsWithoutChangingOtherPrices()
    {
        var initial = GameSession.Create(CivilGrowthScenario.Create());
        Assert.Equal(GameError.MissingPrerequisite, initial.Construct("forge", 1).Error);
        var game = IronIndustryTests.StartIndustry();
        Assert.True(game.Advance(TimeSpan.FromSeconds(300)).Succeeded);
        var before = game.Resources;
        BuildAndStaff(game, "forge");
        Assert.Equal(before["planks"].Units - 240m, game.Resources["planks"].Units);
        Assert.Equal(before["stone"].Units - 180m, game.Resources["stone"].Units);
        Assert.Equal(0m, game.Resources["swords"].Units);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(2m, game.Resources["swords"].Units);
        Assert.Equal(1401 * TimeSpan.TicksPerSecond, game.ElapsedTicks);
        Assert.True(game.Advance(TimeSpan.FromHours(8)).Succeeded);
        Assert.Equal(1000m, game.Resources["swords"].Units);
    }

    [Fact]
    public void FreshOreAndIronCannotBeUsedInTheSameSecondTheyAreProduced()
    {
        var game = Prepared(ResourceBundle.Empty);
        foreach (var id in new[] { "iron-mine", "coal-mine", "iron-smelter", "forge" })
            Assert.True(game.AssignWorkers(id, 1).Succeeded);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(0m, game.Resources["iron"].Units);
        Assert.Equal(0m, game.Resources["swords"].Units);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(1m, game.Resources["iron"].Units);
        Assert.Equal(0m, game.Resources["swords"].Units);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(1m, game.Resources["iron"].Units);
        Assert.Equal(3m, game.Resources["coal"].Units);
        Assert.Equal(2m, game.Resources["swords"].Units);
    }

    [Theory]
    [InlineData(999, 999, 10)]
    [InlineData(998, 1000, 9)]
    public void SwordBatchesAreIndivisibleAndABlockedForgeDoesNotConsumeInputs(int start, int end, int inputLeft)
    {
        var game = Prepared(ResourceBundle.Create(("swords", start), ("iron", 10m), ("coal", 10m)));
        Assert.True(game.AssignWorkers("forge", 1).Succeeded);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(end, game.Resources["swords"].Units);
        Assert.Equal(inputLeft, game.Resources["iron"].Units);
        Assert.Equal(inputLeft, game.Resources["coal"].Units);
        Assert.True(game.ForecastProduction().Single(item => item.BuildingId == "forge")
            .BlockReason.HasFlag(ProductionBlockReason.StorageFull));
        Assert.True(game.Construct("warehouse", 1).Succeeded);
        Assert.Equal(1500m, game.StorageCapacity["swords"].Units);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(end + 2, game.Resources["swords"].Units);
    }

    [Fact]
    public void BothSmeltersReserveScarceCoalBeforeTheForgeAndMoreCoalRestartsIt()
    {
        var game = Prepared(ResourceBundle.Create(("iron-ore", 10m), ("gold-ore", 10m), ("iron", 5m), ("coal", 2m)));
        foreach (var id in new[] { "iron-smelter", "gold-smelter", "coal-mine", "forge" })
            Assert.True(game.AssignWorkers(id, 1).Succeeded);
        var forge = game.ForecastProduction().Single(item => item.BuildingId == "forge");
        Assert.Equal(0, forge.RunnableWorkers);
        Assert.Contains("coal", forge.MissingInputIds);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(6m, game.Resources["iron"].Units);
        Assert.Equal(1m, game.Resources["gold"].Units);
        Assert.Equal(0m, game.Resources["swords"].Units);
        BuildAndStaff(game, "coal-mine");
        Assert.True(game.Advance(TimeSpan.FromSeconds(2)).Succeeded);
        Assert.Equal(2m, game.Resources["swords"].Units);
    }

    [Theory]
    [InlineData("iron", "coal")]
    [InlineData("coal", "iron")]
    public void MissingInputsAreNamedAndDoNotSpendTheOtherIngredient(string available, string missing)
    {
        var game = Prepared(ResourceBundle.Create((available, 10m)));
        Assert.True(game.AssignWorkers("forge", 1).Succeeded);
        Assert.Contains(missing, game.ForecastProduction().Single(item => item.BuildingId == "forge").MissingInputIds);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(10m, game.Resources[available].Units);
        Assert.Equal(0m, game.Resources["swords"].Units);
    }

    [Fact]
    public void UnstaffedPausedAndFractionalReplaysPreserveTheSameSwordStocks()
    {
        var game = Prepared(ResourceBundle.Create(("iron", 10m), ("coal", 10m)));
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(0m, game.Resources["swords"].Units);
        Assert.True(game.AssignWorkers("forge", 1).Succeeded);
        var split = GameSession.Restore(game.Scenario, game.Snapshot());
        split.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(4)).Succeeded);
        for (var second = 0; second < 4; second++)
        {
            Assert.True(split.Advance(TimeSpan.FromMilliseconds(250)).Succeeded);
            Assert.True(split.Advance(TimeSpan.FromMilliseconds(750)).Succeeded);
        }
        Assert.Equal(8m, game.Resources["swords"].Units);
        Assert.Equal(game.Resources, split.Resources);
        Assert.Equal(game.ElapsedTicks, split.ElapsedTicks);
        Assert.Equal(game.Snapshot().GrowthProgressSeconds, split.Snapshot().GrowthProgressSeconds);
        Assert.Equal(game.Snapshot().GrowthRemainderTwentieths, split.Snapshot().GrowthRemainderTwentieths);
        game.Pause();
        var paused = game.Snapshot();
        Assert.True(game.Advance(TimeSpan.FromHours(1)).Succeeded);
        Assert.Equal(paused.ElapsedTicks, game.ElapsedTicks);
        Assert.Equal(paused.Resources, game.Resources);
        Assert.Equal(paused.Buildings.ToArray(), game.Snapshot().Buildings.ToArray());
        Assert.Equal(paused.GrowthProgressSeconds, game.Snapshot().GrowthProgressSeconds);
        Assert.Equal(paused.GrowthRemainderTwentieths, game.Snapshot().GrowthRemainderTwentieths);
    }

    [Fact]
    public void V7BenchmarkRemainsTheGoldCatalogWithoutForgeOrSwords()
    {
        var old = CivilGrowthScenario.CreateGoldForBenchmarks();
        Assert.Equal("civil-growth-7", old.Catalog.RulesVersion);
        Assert.Equal(17, old.Catalog.Buildings.Length);
        Assert.Equal(14, old.Catalog.Resources.Length);
        Assert.False(old.Catalog.BuildingsById.ContainsKey("forge"));
        Assert.False(old.Catalog.ResourcesById.ContainsKey("swords"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public async Task EveryEarlierCivilVersionUpgradesWithNewContentAtZero(int version)
    {
        var old = GameSession.Create(CivilGrowthScenario.CreateForgeForBenchmarks()).Snapshot();
        var later = CivilGrowthScenario.Changes.Skip(version - 1).SelectMany(item => item.AddedBuildings).ToHashSet();
        old = old with
        {
            RulesVersion = $"civil-growth-{version}",
            ElapsedTicks = TimeSpan.FromMilliseconds(17500).Ticks,
            GrowthProgressSeconds = 17,
            GrowthRemainderTwentieths = version >= 5 ? 7 : 0,
            Resources = old.Resources + ResourceBundle.Create(("logs", 1.234m)),
            Buildings = [.. old.Buildings.Where(item => !later.Contains(item.BuildingId))]
        };
        var runtime = new GameRuntime(CivilGrowthScenario.Create());
        var loaded = await new LoadGameHandler(runtime, new SnapshotStore(old)).HandleAsync();
        Assert.True(loaded.Succeeded);
        Assert.Equal(CivilGrowthScenario.RulesVersion, loaded.View.RulesVersion);
        Assert.Equal(old.ElapsedTicks, loaded.View.ElapsedTicks);
        Assert.Equal(old.GrowthProgressSeconds, loaded.View.PopulationState.ProgressSeconds);
        Assert.Equal(old.GrowthRemainderTwentieths, loaded.View.PopulationState.GrowthRemainderTwentieths);
        Assert.Equal(1.234m, loaded.View.Resources.Single(item => item.Id == "logs").Quantity);
        Assert.Equal(0m, loaded.View.Resources.Single(item => item.Id == "swords").Quantity);
        Assert.Equal(0, loaded.View.Buildings.Single(item => item.Id == "forge").Count);
        Assert.Equal(0m, loaded.View.Resources.Single(item => item.Id == "coins").Quantity);
        Assert.Equal(0, loaded.View.Buildings.Single(item => item.Id == "mint").Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task V7CannotClaimSwordsOrForgeAndFailedLoadKeepsTheCurrentGame(bool claimsBuilding)
    {
        var old = GameSession.Create(CivilGrowthScenario.CreateGoldForBenchmarks()).Snapshot();
        old = claimsBuilding ? old with { Buildings = [.. old.Buildings, new("forge", 1, 0)] }
            : old with { Resources = old.Resources + ResourceBundle.Create(("swords", 0.001m)) };
        var loaded = await new LoadGameHandler(new GameRuntime(CivilGrowthScenario.Create()), new SnapshotStore(old)).HandleAsync();
        Assert.False(loaded.Succeeded);
        Assert.Equal(120m, loaded.View.Resources.Single(item => item.Id == "planks").Quantity);
        Assert.Equal(0m, loaded.View.Resources.Single(item => item.Id == "swords").Quantity);
        Assert.Equal(0, loaded.View.Buildings.Single(item => item.Id == "forge").Count);
    }

    private static GameSession Prepared(ResourceBundle industrialStock)
    {
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot();
        return GameSession.Restore(scenario, initial with
        {
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m), ("food", 100m), ("bread", 100m)) + industrialStock,
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId is "large-house" or "bakery" or
                "iron-mine" or "coal-mine" or "iron-smelter" or "gold-mine" or "gold-smelter" or "forge"
                    ? item with { Count = 1 } : item)]
        });
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
