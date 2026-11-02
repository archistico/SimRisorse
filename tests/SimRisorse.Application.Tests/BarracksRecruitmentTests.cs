using System.Collections.Immutable;
using SimRisorse.Application.Features.Military.CancelRecruits;
using SimRisorse.Application.Features.Military.EnlistRecruits;
using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Features.Sessions.AdvanceTime;
using SimRisorse.Application.Features.Sessions.GetGameState;
using SimRisorse.Application.Features.Sessions.SetPause;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>M5.3 contract: Caserma 260/200 after the Forge, 5 swords per recruit, one recruit per barracks every 30 s.</summary>
public sealed class BarracksRecruitmentTests
{
    [Fact]
    public void BarracksNeedsTheForgeCostsItsPriceAndOffersNoWorkplaces()
    {
        var scenario = CivilGrowthScenario.Create();
        var barracks = scenario.Catalog.BuildingsById["barracks"];
        Assert.Equal("Caserma", barracks.Name);
        Assert.Equal(new[] { "forge" }, barracks.Prerequisites);
        Assert.Null(barracks.Recipe);
        Assert.Equal(0, barracks.WorkplacesPerBuilding);
        Assert.Equal(20, scenario.Catalog.Buildings.Length);
        Assert.Equal(16, scenario.Catalog.Resources.Length);
        Assert.Equal(5m, scenario.MilitaryRules!.SwordsPerRecruit.Units);
        Assert.Equal(30, scenario.MilitaryRules.TrainingSeconds);
        Assert.Equal(1, scenario.MilitaryRules.RecruitsPerBarracks);

        var noForge = Prepared(swords: 100m, forge: false, barracks: 0);
        Assert.Equal(GameError.MissingPrerequisite, noForge.Construct("barracks", 1).Error);
        var game = Prepared(swords: 100m, barracks: 0);
        Assert.Equal(GameError.MissingPrerequisite, game.Enlist(1).Error);
        var before = game.Resources;
        Assert.True(game.Construct("barracks", 1).Succeeded);
        Assert.Equal(before["planks"].Units - 260m, game.Resources["planks"].Units);
        Assert.Equal(before["stone"].Units - 200m, game.Resources["stone"].Units);
        Assert.Equal(1, game.TrainingPlaces);
        Assert.Equal(GameError.InsufficientWorkplaces, game.AssignWorkers("barracks", 1).Error);
    }

    [Fact]
    public void EnlistingSpendsFiveSwordsEachAndKeepsEveryPersonResident()
    {
        var game = Prepared(swords: 100m);
        var food = game.ForecastEconomy().Population.RequiredFood;
        Assert.True(game.Enlist(3).Succeeded);
        Assert.Equal(85m, game.Resources["swords"].Units);
        Assert.Equal(40, game.Population);
        Assert.Equal(37, game.AvailableWorkers);
        Assert.Equal(new MilitaryState(2, [0], 0), game.Military);
        Assert.Equal(food, game.ForecastEconomy().Population.RequiredFood);
        Assert.Equal(40, Balance(game));
    }

    [Fact]
    public void RefusedOrdersChangeNothing()
    {
        var game = Prepared(swords: 14m);
        Assert.True(game.AssignWorkers("forge", 1).Succeeded);
        var before = game.Snapshot();
        Assert.Equal(GameError.InvalidQuantity, game.Enlist(0).Error);
        Assert.Equal(GameError.InvalidQuantity, game.Enlist(-1).Error);
        Assert.Equal(GameError.InsufficientResources, game.Enlist(3).Error);
        Assert.Equal(GameError.InsufficientPopulation, game.Enlist(40).Error);
        Assert.Equal(GameError.InsufficientPopulation, game.Enlist(int.MaxValue).Error);
        Assert.Equal(GameError.InsufficientRecruits, game.CancelRecruits(1).Error);
        Assert.Equal(GameError.InvalidQuantity, game.CancelRecruits(0).Error);
        TestEqual(before, game.Snapshot());

        var civil = GameSession.Create(CivilGrowthScenario.CreateMintForBenchmarks());
        Assert.Equal(GameError.MilitaryUnavailable, civil.Enlist(1).Error);
        Assert.Equal(GameError.MilitaryUnavailable, civil.CancelRecruits(1).Error);
        Assert.Null(civil.Military);
    }

    [Fact]
    public void EmployedWorkersAreNeverRecruitedImplicitly()
    {
        var game = Prepared(swords: 200m);
        foreach (var id in new[] { "iron-mine", "coal-mine", "iron-smelter", "forge" })
            Assert.True(game.AssignWorkers(id, 1).Succeeded);
        Assert.Equal(36, game.AvailableWorkers);
        Assert.Equal(GameError.InsufficientPopulation, game.Enlist(37).Error);
        Assert.True(game.Enlist(36).Succeeded);
        Assert.Equal(0, game.AvailableWorkers);
        Assert.Equal(4, game.Snapshot().Buildings.Sum(item => item.AssignedWorkers));
        Assert.Equal(GameError.InsufficientPopulation, game.AssignWorkers("gold-mine", 1).Error);
        Assert.True(game.ReleaseWorkers("forge", 1).Succeeded);
        Assert.Equal(1, game.AvailableWorkers);
        Assert.Equal(40, Balance(game));
    }

    [Fact]
    public void OneBarracksTrainsOneRecruitEveryThirtyValidSeconds()
    {
        var game = Prepared(swords: 100m);
        Assert.True(game.Enlist(3).Succeeded);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(29)).Succeeded);
        Assert.Equal(new MilitaryState(2, [29], 0), game.Military);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(new MilitaryState(1, [0], 1), game.Military);
        game.Pause();
        Assert.True(game.Advance(TimeSpan.FromHours(1)).Succeeded);
        Assert.Equal(new MilitaryState(1, [0], 1), game.Military);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(60)).Succeeded);
        Assert.Equal(new MilitaryState(0, [], 3), game.Military);
        Assert.Equal(37, game.AvailableWorkers);
        Assert.Equal(40, game.Population);
        Assert.Equal(GameError.InsufficientRecruits, game.CancelRecruits(1).Error);
        Assert.Equal(40, Balance(game));
    }

    [Fact]
    public void EveryBarracksTrainsInParallelAndANewOneTakesAWaitingRecruitAtOnce()
    {
        var game = Prepared(swords: 100m, barracks: 2);
        Assert.True(game.Enlist(5).Succeeded);
        Assert.Equal(new MilitaryState(3, [0, 0], 0), game.Military);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(10)).Succeeded);
        Assert.True(game.Construct("barracks", 1).Succeeded);
        Assert.Equal(new MilitaryState(2, [10, 10, 0], 0), game.Military);
        Assert.True(game.Advance(TimeSpan.FromSeconds(20)).Succeeded);
        Assert.Equal(new MilitaryState(0, [20, 0, 0], 2), game.Military);
        Assert.True(game.Advance(TimeSpan.FromSeconds(30)).Succeeded);
        Assert.Equal(new MilitaryState(0, [], 5), game.Military);
    }

    [Fact]
    public void FractionalBatchedAndRestoredAdvancesEndInTheSameState()
    {
        var reference = Prepared(swords: 100m, barracks: 2);
        Assert.True(reference.Enlist(7).Succeeded);
        var split = GameSession.Restore(reference.Scenario, reference.Snapshot());
        var batch = GameSession.Restore(reference.Scenario, reference.Snapshot());
        foreach (var game in new[] { reference, split, batch }) game.Resume();
        for (var second = 0; second < 95; second++)
        {
            Assert.True(reference.Advance(TimeSpan.FromSeconds(1)).Succeeded);
            Assert.True(split.Advance(TimeSpan.FromMilliseconds(250)).Succeeded);
            Assert.True(split.Advance(TimeSpan.FromMilliseconds(750)).Succeeded);
        }
        Assert.True(batch.Advance(TimeSpan.FromSeconds(95)).Succeeded);
        TestEqual(reference.Snapshot(), split.Snapshot());
        TestEqual(reference.Snapshot(), batch.Snapshot());
        Assert.Equal(new MilitaryState(0, [5], 6), reference.Military);

        // A checkpoint half a second before a completion produces exactly one soldier after restoring it.
        var near = Prepared(swords: 10m);
        Assert.True(near.Enlist(1).Succeeded);
        near.Resume();
        Assert.True(near.Advance(TimeSpan.FromMilliseconds(29500)).Succeeded);
        var restored = GameSession.Restore(near.Scenario, near.Snapshot());
        Assert.True(restored.IsPaused);
        restored.Resume();
        Assert.True(near.Advance(TimeSpan.FromMilliseconds(500)).Succeeded);
        Assert.True(restored.Advance(TimeSpan.FromMilliseconds(500)).Succeeded);
        Assert.Equal(new MilitaryState(0, [], 1), restored.Military);
        TestEqual(near.Snapshot(), restored.Snapshot());
        Assert.True(restored.Advance(TimeSpan.FromSeconds(60)).Succeeded);
        Assert.Equal(1, restored.Military!.Soldiers);
    }

    [Fact]
    public void CancellingReturnsWaitingRecruitsFirstThenTheLeastAdvancedWithTheirSwords()
    {
        var game = Prepared(swords: 100m, barracks: 2);
        Assert.True(game.Enlist(4).Succeeded);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(10)).Succeeded);
        Assert.Equal(new MilitaryState(2, [10, 10], 0), game.Military);
        Assert.True(game.CancelRecruits(1).Succeeded);
        Assert.Equal(new MilitaryState(1, [10, 10], 0), game.Military);
        Assert.Equal(85m, game.Resources["swords"].Units);
        Assert.True(game.Advance(TimeSpan.FromSeconds(20)).Succeeded);
        Assert.Equal(new MilitaryState(0, [0], 2), game.Military);
        Assert.True(game.Advance(TimeSpan.FromSeconds(12)).Succeeded);
        Assert.Equal(GameError.InsufficientRecruits, game.CancelRecruits(2).Error);
        Assert.True(game.CancelRecruits(1).Succeeded);
        Assert.Equal(new MilitaryState(0, [], 2), game.Military);
        Assert.Equal(90m, game.Resources["swords"].Units);
        Assert.Equal(38, game.AvailableWorkers);
        Assert.True(game.Enlist(1).Succeeded);
        Assert.Equal(new MilitaryState(0, [0], 2), game.Military);
        Assert.Equal(40, Balance(game));
    }

    [Fact]
    public void CancellingIsRefusedWhenTheReturnedSwordsDoNotFit()
    {
        var game = Prepared(swords: 1000m);
        Assert.True(game.Enlist(2).Succeeded);
        Assert.True(game.AssignWorkers("forge", 1).Succeeded);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(2)).Succeeded);
        Assert.Equal(994m, game.Resources["swords"].Units);
        game.Pause();
        var before = game.Snapshot();
        Assert.Equal(GameError.InsufficientStorage, game.CancelRecruits(2).Error);
        TestEqual(before, game.Snapshot());
        Assert.True(game.CancelRecruits(1).Succeeded);
        Assert.Equal(999m, game.Resources["swords"].Units);
        Assert.Equal(new MilitaryState(0, [2], 0), game.Military);
    }

    [Fact]
    public async Task ReadModelExplainsLimitsCountsAndRemainingTime()
    {
        var runtime = new GameRuntime(CivilGrowthScenario.Create());
        var empty = new GetGameStateHandler(runtime).Handle().Military!;
        Assert.Equal(0, empty.TrainingPlaces);
        Assert.Equal(0, empty.MaximumEnlistable);
        Assert.Equal(GameError.MissingPrerequisite, new EnlistRecruitsHandler(runtime).Handle(new(1)).Error);

        var game = Prepared(swords: 37m, barracks: 2);
        Assert.True(game.AssignWorkers("forge", 1).Succeeded);
        Assert.True((await new LoadGameHandler(runtime, new SnapshotStore(game.Snapshot())).HandleAsync()).Succeeded);
        var view = new GetGameStateHandler(runtime).Handle().Military!;
        Assert.Equal(7, view.MaximumEnlistable);
        Assert.Equal(1, view.EmployedWorkers);
        Assert.Equal(2, view.TrainingPlaces);
        Assert.Equal("Spade", view.SwordsName);
        var enlisted = new EnlistRecruitsHandler(runtime).Handle(new(5));
        Assert.True(enlisted.Succeeded);
        var military = enlisted.View.Military!;
        Assert.Equal(34, enlisted.View.AvailableWorkers);
        Assert.Equal(3, military.QueuedRecruits);
        Assert.Equal(new[] { 30, 30 }, military.TrainingRemainingSeconds);
        Assert.Equal(90, military.SecondsUntilAllTrained);
        Assert.Equal(2, military.MaximumEnlistable);
        Assert.Equal(5, military.MaximumCancellable);
        Assert.Equal(40, enlisted.View.Population);
        Assert.Equal(enlisted.View.Population,
            military.EmployedWorkers + enlisted.View.AvailableWorkers + military.Recruits + military.Soldiers);

        new SetPauseHandler(runtime).Handle(new(false));
        var advance = new AdvanceTimeHandler(runtime);
        Assert.True(advance.Handle(new(TimeSpan.FromSeconds(89))).Succeeded);
        var almost = new GetGameStateHandler(runtime).Handle().Military!;
        Assert.Equal(1, almost.Recruits);
        Assert.Equal(4, almost.Soldiers);
        Assert.Equal(1, almost.SecondsUntilAllTrained);
        Assert.True(advance.Handle(new(TimeSpan.FromSeconds(1))).Succeeded);
        var done = new GetGameStateHandler(runtime).Handle().Military!;
        Assert.Equal(0, done.Recruits);
        Assert.Equal(5, done.Soldiers);
        Assert.Equal(0, done.SecondsUntilAllTrained);
        Assert.Equal(GameError.InsufficientRecruits, new CancelRecruitsHandler(runtime).Handle(new(1)).Error);
    }

    [Theory]
    [InlineData(3, 1, 1, 1, 89)]
    [InlineData(4, 1, 1, 1, 119)]
    [InlineData(5, 2, 7, 4, 53)]
    [InlineData(6, 2, 7, 4, 60)]
    public async Task PredictedCompletionMatchesTheSimulation(int recruits, int barracks, int firstSeconds, int totalBarracks, int expected)
    {
        var game = Prepared(swords: 100m, barracks: barracks);
        Assert.True(game.Enlist(1).Succeeded);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(firstSeconds)).Succeeded);
        if (totalBarracks > barracks) Assert.True(game.Construct("barracks", totalBarracks - barracks).Succeeded);
        Assert.True(game.Enlist(recruits - 1).Succeeded);
        var runtime = new GameRuntime(game.Scenario);
        Assert.True((await new LoadGameHandler(runtime, new SnapshotStore(game.Snapshot())).HandleAsync()).Succeeded);
        var predicted = new GetGameStateHandler(runtime).Handle().Military!.SecondsUntilAllTrained;
        Assert.Equal(expected, predicted);
        Assert.True(game.Advance(TimeSpan.FromSeconds(predicted - 1)).Succeeded);
        Assert.NotEqual(0, game.Military!.Recruits);
        Assert.True(game.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(0, game.Military!.Recruits);
        Assert.Equal(recruits, game.Military.Soldiers);
    }

    [Fact]
    public async Task V9CheckpointIsConvertedWithAnEmptyArmyAndItsSwordsPreserved()
    {
        var old = GameSession.Create(CivilGrowthScenario.CreateMintForBenchmarks()).Snapshot();
        Assert.Null(old.Military);
        old = old with
        {
            Population = 20,
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m), ("food", 100m), ("bread", 100m), ("swords", 12.345m)),
            Buildings = [.. old.Buildings.Select(item => item.BuildingId switch
            {
                "large-house" or "bakery" or "iron-mine" or "coal-mine" or "iron-smelter" => item with { Count = 1 },
                "forge" => item with { Count = 1, AssignedWorkers = 1 },
                _ => item
            })],
            ElapsedTicks = 12_345 * TimeSpan.TicksPerMillisecond
        };
        var runtime = new GameRuntime(CivilGrowthScenario.Create());
        var loaded = await new LoadGameHandler(runtime, new SnapshotStore(old)).HandleAsync();
        Assert.True(loaded.Succeeded);
        Assert.Equal("civil-growth-9", loaded.UpgradedFromRules);
        Assert.Equal(CivilGrowthScenario.RulesVersion, loaded.View.RulesVersion);
        Assert.Equal(0, loaded.View.Buildings.Single(item => item.Id == "barracks").Count);
        Assert.Equal(1, loaded.View.Buildings.Single(item => item.Id == "forge").AssignedWorkers);
        Assert.Equal(12.345m, loaded.View.Resources.Single(item => item.Id == "swords").Quantity);
        Assert.Equal(19, loaded.View.AvailableWorkers);
        Assert.Equal(old.ElapsedTicks, loaded.View.ElapsedTicks);
        var military = loaded.View.Military!;
        Assert.Equal(0, military.Recruits);
        Assert.Equal(0, military.Soldiers);
        Assert.Equal(0, military.TrainingPlaces);
    }

    [Theory]
    [InlineData("barracks")]
    [InlineData("recruits")]
    [InlineData("soldiers")]
    public async Task V9CannotClaimABarracksOrAnArmyAndTheCurrentGameIsKept(string claim)
    {
        var old = GameSession.Create(CivilGrowthScenario.CreateMintForBenchmarks()).Snapshot();
        old = claim switch
        {
            "barracks" => old with { Buildings = [.. old.Buildings, new("barracks", 1, 0)] },
            "recruits" => old with { Military = new MilitaryState(1, [], 0) },
            _ => old with { Military = new MilitaryState(0, [], 1) }
        };
        var runtime = new GameRuntime(CivilGrowthScenario.Create());
        var loaded = await new LoadGameHandler(runtime, new SnapshotStore(old)).HandleAsync();
        Assert.Equal(claim == "barracks" ? GameSaveError.InvalidData : GameSaveError.IncompatibleRules, loaded.Error);
        Assert.Equal(0, loaded.View.Military!.Soldiers);
        Assert.Equal(0, loaded.View.Buildings.Single(item => item.Id == "barracks").Count);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("free-place")]
    [InlineData("complete")]
    [InlineData("negative")]
    [InlineData("order")]
    [InlineData("too-many-places")]
    [InlineData("population")]
    [InlineData("default")]
    public void InconsistentMilitaryCheckpointsAreRejected(string change)
    {
        var game = Prepared(swords: 100m, barracks: 2);
        var valid = game.Snapshot();
        var invalid = change switch
        {
            "missing" => valid with { Military = null },
            "free-place" => valid with { Military = new MilitaryState(1, [3], 0) },
            "complete" => valid with { Military = new MilitaryState(0, [30], 0) },
            "negative" => valid with { Military = new MilitaryState(0, [], -1) },
            "order" => valid with { Military = new MilitaryState(0, [1, 2], 0) },
            "too-many-places" => valid with { Military = new MilitaryState(0, [1, 1, 1], 0) },
            "population" => valid with { Military = new MilitaryState(0, [], 41) },
            _ => valid with { Military = new MilitaryState(0, default, 0) }
        };
        Assert.Throws<ArgumentException>(() => GameSession.Restore(game.Scenario, invalid));
        Assert.NotNull(GameSession.Restore(game.Scenario, valid with { Military = new MilitaryState(2, [12, 0], 16) }));
        Assert.Throws<ArgumentException>(() =>
            GameSession.Restore(CivilGrowthScenario.CreateMintForBenchmarks(),
                GameSession.Create(CivilGrowthScenario.CreateMintForBenchmarks()).Snapshot() with { Military = MilitaryState.Empty }));
    }

    [Fact]
    public void V9RemainsTheMintCatalogWithoutBarracks()
    {
        var old = CivilGrowthScenario.CreateMintForBenchmarks();
        Assert.Equal("civil-growth-9", old.Catalog.RulesVersion);
        Assert.Equal(19, old.Catalog.Buildings.Length);
        Assert.Equal(16, old.Catalog.Resources.Length);
        Assert.False(old.Catalog.BuildingsById.ContainsKey("barracks"));
        Assert.Null(old.MilitaryRules);
    }

    [Fact]
    public void MilitaryStateComparesTrainingByValue()
    {
        Assert.Equal(new MilitaryState(1, ImmutableArray.Create(3, 2), 4), new MilitaryState(1, [3, 2], 4));
        Assert.NotEqual(new MilitaryState(1, [3, 2], 4), new MilitaryState(1, [2, 3], 4));
        Assert.Equal(MilitaryState.Empty, new MilitaryState(0, default, 0));
        Assert.Equal(7, new MilitaryState(2, [1, 1], 3).People);
    }

    /// <summary>Employed + free + recruits + soldiers, which must always equal the population.</summary>
    private static long Balance(GameSession game) =>
        game.Snapshot().Buildings.Sum(item => (long)item.AssignedWorkers) + game.AvailableWorkers + game.Military!.People;

    internal static GameSession Prepared(decimal swords, bool forge = true, int barracks = 1)
    {
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot();
        return GameSession.Restore(scenario, initial with
        {
            // Eight small-house and 32 large-house places are full: no arrival changes the counts under test.
            Population = 40,
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m), ("food", 100m), ("bread", 100m),
                ("iron", 10m), ("coal", 10m), ("swords", swords)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId switch
            {
                "large-house" or "bakery" or "iron-mine" or "coal-mine" or "iron-smelter" or "gold-mine" => item with { Count = 1 },
                "forge" => item with { Count = forge ? 1 : 0 },
                "barracks" => item with { Count = barracks },
                _ => item
            })]
        });
    }

    private static void TestEqual(GameSnapshot expected, GameSnapshot actual)
    {
        Assert.Equal(expected.ElapsedTicks, actual.ElapsedTicks);
        Assert.Equal(expected.Population, actual.Population);
        Assert.Equal(expected.GrowthProgressSeconds, actual.GrowthProgressSeconds);
        Assert.Equal(expected.GrowthRemainderTwentieths, actual.GrowthRemainderTwentieths);
        Assert.Equal(expected.Resources, actual.Resources);
        Assert.Equal(expected.Buildings.ToArray(), actual.Buildings.ToArray());
        Assert.Equal(expected.Military, actual.Military);
    }

    private sealed class SnapshotStore(GameSnapshot snapshot) : IGameSaveStore
    {
        public GameSnapshot Load() => snapshot;
        public void Save(GameSnapshot value) => throw new NotSupportedException();
    }
}
