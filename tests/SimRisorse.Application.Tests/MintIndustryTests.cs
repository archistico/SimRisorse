using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class MintIndustryTests
{
    [Fact]
    public void CivilianGoldTimelinePaysForMintAndReachesTheLastWholeBatch()
    {
        Assert.Equal(GameError.MissingPrerequisite, GameSession.Create(CivilGrowthScenario.Create()).Construct("mint", 1).Error);
        var game = IronIndustryTests.StartIndustry();
        Assert.True(game.Advance(TimeSpan.FromSeconds(400)).Succeeded);
        BuildAndStaff(game, "gold-mine");
        BuildAndStaff(game, "gold-smelter");
        Assert.True(game.Advance(TimeSpan.FromSeconds(300)).Succeeded);
        var before = game.Resources;
        BuildAndStaff(game, "mint");
        Assert.Equal(before["planks"].Units - 210m, game.Resources["planks"].Units);
        Assert.Equal(before["stone"].Units - 160m, game.Resources["stone"].Units);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(1801 * TimeSpan.TicksPerSecond, game.ElapsedTicks);
        Assert.Equal(3m, game.Resources["coins"].Units);
        Assert.True(game.Advance(TimeSpan.FromHours(8)).Succeeded);
        Assert.Equal(999m, game.Resources["coins"].Units);
        Assert.Equal(1000m, game.StorageCapacity["coins"].Units);
        Assert.Contains("coins", game.ForecastProduction().Single(item => item.BuildingId == "mint").FullOutputIds);
    }

    [Fact]
    public void NewlyMinedAndSmeltedGoldWaitsUntilTheNextSecondBeforeMinting()
    {
        var game = Prepared(ResourceBundle.Empty);
        foreach (var id in new[] { "gold-mine", "coal-mine", "gold-smelter", "mint" })
            Assert.True(game.AssignWorkers(id, 1).Succeeded);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(0m, game.Resources["gold"].Units);
        Assert.Equal(0m, game.Resources["coins"].Units);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(1m, game.Resources["gold"].Units);
        Assert.Equal(0m, game.Resources["coins"].Units);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(1m, game.Resources["gold"].Units);
        Assert.Equal(3m, game.Resources["coal"].Units);
        Assert.Equal(3m, game.Resources["coins"].Units);
    }

    [Theory]
    [InlineData(997, 1000, 9)]
    [InlineData(998, 998, 10)]
    [InlineData(999, 999, 10)]
    [InlineData(1000, 1000, 10)]
    public void MintNeverSpendsInputsUnlessAWholeThreeCoinBatchFits(int start, int end, int inputLeft)
    {
        var game = Prepared(ResourceBundle.Create(("coins", start), ("gold", 10m), ("coal", 10m)));
        Assert.True(game.AssignWorkers("mint", 1).Succeeded);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(end, game.Resources["coins"].Units);
        Assert.Equal(inputLeft, game.Resources["gold"].Units);
        Assert.Equal(inputLeft, game.Resources["coal"].Units);
        Assert.True(game.ForecastProduction().Single(item => item.BuildingId == "mint").BlockReason.HasFlag(ProductionBlockReason.StorageFull));
        Assert.True(game.Construct("warehouse", 1).Succeeded);
        Assert.Equal(1500m, game.StorageCapacity["coins"].Units);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(end + 3, game.Resources["coins"].Units);
    }

    [Theory]
    [InlineData(2, 0, 0)]
    [InlineData(3, 2, 0)]
    [InlineData(4, 2, 3)]
    public void CoalIsReservedForIronGoldAndForgeBeforeMint(int coal, int swords, int coins)
    {
        var game = Prepared(ResourceBundle.Create(("coal", coal), ("iron-ore", 10m), ("gold-ore", 10m), ("iron", 10m), ("gold", 10m)));
        foreach (var id in new[] { "iron-smelter", "gold-smelter", "forge", "mint" })
            Assert.True(game.AssignWorkers(id, 1).Succeeded);
        var mint = game.ForecastProduction().Single(item => item.BuildingId == "mint");
        Assert.Equal(coins > 0 ? 1 : 0, mint.RunnableWorkers);
        if (coins == 0) Assert.Contains("coal", mint.MissingInputIds);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(swords, game.Resources["swords"].Units);
        Assert.Equal(coins, game.Resources["coins"].Units);
        Assert.Equal(0m, game.Resources["coal"].Units);
        Assert.Equal(swords > 0 ? 10m : 11m, game.Resources["iron"].Units);
        Assert.Equal(coins > 0 ? 10m : 11m, game.Resources["gold"].Units);
    }

    [Theory]
    [InlineData("gold", "coal")]
    [InlineData("coal", "gold")]
    public void MissingIngredientIsDiagnosedWithoutSpendingTheAvailableOne(string available, string missing)
    {
        var game = Prepared(ResourceBundle.Create((available, 10m)));
        Assert.True(game.AssignWorkers("mint", 1).Succeeded);
        Assert.Contains(missing, game.ForecastProduction().Single(item => item.BuildingId == "mint").MissingInputIds);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(0m, game.Resources["coins"].Units);
        Assert.Equal(10m, game.Resources[available].Units);
    }

    [Fact]
    public void ReleasingTheForgeWorkerLetsMintUseTheScarceCoal()
    {
        var game = Prepared(ResourceBundle.Create(("coal", 1m), ("iron", 10m), ("gold", 10m)));
        Assert.True(game.AssignWorkers("forge", 1).Succeeded);
        Assert.True(game.AssignWorkers("mint", 1).Succeeded);
        Assert.Equal(0, game.ForecastProduction().Single(item => item.BuildingId == "mint").RunnableWorkers);
        Assert.True(game.ReleaseWorkers("forge", 1).Succeeded);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(0m, game.Resources["swords"].Units);
        Assert.Equal(3m, game.Resources["coins"].Units);
    }

    [Fact]
    public void PausesStaffingAndFractionalReplaysDoNotCreateOrLoseCoins()
    {
        var game = Prepared(ResourceBundle.Create(("coal", 10m), ("gold", 10m)));
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(0m, game.Resources["coins"].Units);
        Assert.True(game.AssignWorkers("mint", 1).Succeeded);
        var split = GameSession.Restore(game.Scenario, game.Snapshot());
        split.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(4)).Succeeded);
        for (var second = 0; second < 4; second++)
        {
            Assert.True(split.Advance(TimeSpan.FromMilliseconds(250)).Succeeded);
            Assert.True(split.Advance(TimeSpan.FromMilliseconds(750)).Succeeded);
        }
        Assert.Equal(12m, game.Resources["coins"].Units);
        Assert.Equal(game.Resources, split.Resources);
        Assert.Equal(game.ElapsedTicks, split.ElapsedTicks);
        Assert.Equal(game.GrowthProgressSeconds, split.GrowthProgressSeconds);
        Assert.Equal(game.GrowthRemainderTwentieths, split.GrowthRemainderTwentieths);
        game.Pause();
        Assert.True(game.Advance(TimeSpan.FromHours(1)).Succeeded);
        Assert.Equal(game.ElapsedTicks, split.ElapsedTicks);
        Assert.Equal(game.Resources, split.Resources);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task V8CannotClaimCoinsOrMintAndFailedLoadKeepsTheCurrentGame(bool claimsBuilding)
    {
        var old = GameSession.Create(CivilGrowthScenario.CreateForgeForBenchmarks()).Snapshot();
        old = claimsBuilding ? old with { Buildings = [.. old.Buildings, new("mint", 1, 0)] }
            : old with { Resources = old.Resources + ResourceBundle.Create(("coins", 0.001m)) };
        var loaded = await new LoadGameHandler(new GameRuntime(CivilGrowthScenario.Create()), new SnapshotStore(old)).HandleAsync();
        Assert.Equal(claimsBuilding ? GameSaveError.InvalidData : GameSaveError.IncompatibleRules, loaded.Error);
        Assert.Equal(120m, loaded.View.Resources.Single(item => item.Id == "planks").Quantity);
        Assert.Equal(0m, loaded.View.Resources.Single(item => item.Id == "coins").Quantity);
        Assert.Equal(0, loaded.View.Buildings.Single(item => item.Id == "mint").Count);
    }

    [Fact]
    public void V8RemainsTheForgeCatalogWithoutMintOrCoins()
    {
        var old = CivilGrowthScenario.CreateForgeForBenchmarks();
        Assert.Equal("civil-growth-8", old.Catalog.RulesVersion);
        Assert.Equal(18, old.Catalog.Buildings.Length);
        Assert.Equal(15, old.Catalog.Resources.Length);
        Assert.True(old.Catalog.BuildingsById.ContainsKey("forge"));
        Assert.False(old.Catalog.BuildingsById.ContainsKey("mint"));
        Assert.False(old.Catalog.ResourcesById.ContainsKey("coins"));
    }

    private static GameSession Prepared(ResourceBundle industrialStock)
    {
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot();
        return GameSession.Restore(scenario, initial with
        {
            Population = 20,
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m), ("food", 100m), ("bread", 100m)) + industrialStock,
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId is "large-house" or "bakery" or
                "iron-mine" or "coal-mine" or "iron-smelter" or "gold-mine" or "gold-smelter" or "forge" or "mint"
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
