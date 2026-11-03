using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class MilitarySessionTests
{
    private static readonly MilitaryTrainingRules Rules = MilitaryTrainingRules.SimpleBarracks;

    private static GameSession Seed(decimal swords = 40m)
    {
        var scenario = CivilGrowthScenario.CreateTownHallForBenchmarks();
        var initial = GameSession.Create(scenario).Snapshot();
        return GameSession.Restore(scenario, initial with
        {
            Population = 40,
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m),
                ("food", 100m), ("bread", 100m), ("swords", swords)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId switch
            {
                "large-house" or "forge" or "barracks" => item with { Count = 1 },
                _ => item
            })]
        });
    }

    [Fact]
    public void V12AddsTownHallWithoutChangingFrozenV10MilitaryRulesAndBreadCapacity()
    {
        var current = CivilGrowthScenario.CreateTownHallForBenchmarks();
        var v10 = CivilGrowthScenario.CreateBarracksForBenchmarks();
        var historical = CivilGrowthScenario.CreateMintForBenchmarks();
        Assert.Equal("civil-growth-12", current.Catalog.RulesVersion);
        Assert.Equal(21, current.Catalog.Buildings.Length);
        Assert.Equal(16, current.Catalog.Resources.Length);
        Assert.Equal("civil-growth-10", v10.Catalog.RulesVersion);
        Assert.Equal(100m, v10.Capacity["bread"].Units);
        Assert.Equal(0m, current.Capacity["bread"].Units);
        Assert.Equal(0m, current.Capacity["swords"].Units);
        Assert.Equal(1010m, GameSession.Create(current).StorageCapacity["bread"].Units);
        Assert.Equal(1000m, GameSession.Create(current).StorageCapacity["swords"].Units);
        Assert.Equal(Rules, v10.MilitaryRules);
        Assert.Equal(current.Catalog.Buildings.Length - 1, v10.Catalog.Buildings.Length);
        Assert.Equal("civil-growth-9", historical.Catalog.RulesVersion);
        Assert.Equal(19, historical.Catalog.Buildings.Length);
        Assert.Null(historical.MilitaryRules);
        Assert.False(historical.Catalog.BuildingsById.ContainsKey("barracks"));
        var barracks = current.Catalog.BuildingsById["barracks"];
        Assert.Equal(Rules.BarracksCost, barracks.Cost);
        Assert.Equal(new[] { "forge" }, barracks.Prerequisites);
        Assert.Equal(0, barracks.WorkplacesPerBuilding);
        Assert.Null(barracks.Recipe);
        Assert.Equal(GameError.MissingPrerequisite, GameSession.Create(current).Construct("barracks", 1).Error);
    }

    [Fact]
    public void RecruitmentReservesPopulationAndConsumesSwordsOnce()
    {
        var game = Seed();
        Assert.Equal(37, game.AvailableWorkers);
        Assert.True(game.Enlist(3).Succeeded);
        Assert.Equal(new MilitaryTrainingState(1, 2, 0, 0), game.MilitaryState);
        Assert.Equal(34m, game.Resources["swords"].Units);
        Assert.Equal(34, game.AvailableWorkers);
        Assert.Equal(GameError.InsufficientPopulation, game.Enlist(38).Error);
        Assert.Equal(GameError.InsufficientSoldiers, game.DischargeSoldiers(1).Error);
        Assert.Equal(GameError.InsufficientQueuedRecruits, game.CancelQueuedRecruits(2).Error);
        Assert.Equal(34m, game.Resources["swords"].Units);
        Assert.True(game.CancelQueuedRecruits(1).Succeeded);
        Assert.Equal(36m, game.Resources["swords"].Units);
        Assert.Equal(35, game.AvailableWorkers);
        Assert.Equal(GameError.InvalidQuantity, game.DischargeSoldiers(0).Error);
    }

    [Fact]
    public void GroupProgressStopsWhenPausedAndReplaysAcrossFractionalSnapshot()
    {
        var whole = Seed();
        var split = Seed();
        Assert.True(whole.Enlist(3).Succeeded);
        Assert.True(split.Enlist(3).Succeeded);
        whole.Resume();
        split.Resume();
        Assert.True(whole.Advance(TimeSpan.FromSeconds(121)).Succeeded);
        Assert.True(split.Advance(TimeSpan.FromMilliseconds(30500)).Succeeded);
        var checkpoint = split.Snapshot();
        Assert.Equal(new MilitaryTrainingState(1, 2, 0, 30), checkpoint.Military);
        var resumed = GameSession.Restore(CivilGrowthScenario.CreateTownHallForBenchmarks(), checkpoint);
        Assert.True(resumed.IsPaused);
        Assert.True(resumed.Advance(TimeSpan.FromHours(1)).Succeeded);
        Assert.Equal(checkpoint.Military, resumed.MilitaryState);
        resumed.Resume();
        Assert.True(resumed.Advance(TimeSpan.FromMilliseconds(90500)).Succeeded);
        Assert.Equal(whole.MilitaryState, resumed.MilitaryState);
        Assert.Equal(new MilitaryTrainingState(0, 0, 3, 0), resumed.MilitaryState);
        Assert.Equal(whole.Resources, resumed.Resources);
        Assert.Equal(whole.ElapsedTicks, resumed.ElapsedTicks);
        Assert.True(resumed.DischargeSoldiers(2).Succeeded);
        Assert.Equal(36, resumed.AvailableWorkers);
        Assert.Equal(34m, resumed.Resources["swords"].Units); // No refund for discharge.
    }

    [Fact]
    public void FullSwordWarehousePreventsQueueCancellationWithoutPartialRefund()
    {
        var game = Seed(1000m);
        Assert.True(game.Enlist(3).Succeeded);
        var before = game.Snapshot();
        var full = GameSession.Restore(CivilGrowthScenario.CreateTownHallForBenchmarks(), before with
        { Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m),
            ("food", 100m), ("bread", 100m), ("swords", 1070m)) });
        Assert.Equal(GameError.InsufficientStorage, full.CancelQueuedRecruits(1).Error);
        Assert.Equal(1070m, full.Resources["swords"].Units);
        Assert.Equal(before.Military, full.MilitaryState);
        Assert.Equal(34, full.AvailableWorkers);
    }

    [Fact]
    public void CorruptMilitaryCheckpointsAreRejectedRatherThanInventingSoldiers()
    {
        var scenario = CivilGrowthScenario.CreateTownHallForBenchmarks();
        var state = Seed().Snapshot();
        Assert.Throws<ArgumentException>(() => GameSession.Restore(scenario, state with { Military = null }));
        Assert.Throws<ArgumentException>(() => GameSession.Restore(scenario, state with
        { Military = new MilitaryTrainingState(0, 3, 0, 0) }));
        Assert.Throws<ArgumentException>(() => GameSession.Restore(scenario, state with
        { Military = new MilitaryTrainingState(0, 2, 39, 0) }));
        var v9 = CivilGrowthScenario.CreateMintForBenchmarks();
        Assert.Throws<ArgumentException>(() => GameSession.Restore(v9,
            GameSession.Create(v9).Snapshot() with { Military = MilitaryTrainingState.Empty }));
    }

    [Fact]
    public void RecruitsAndSoldiersKeepTheSameHousingAndRationsAsCivilians()
    {
        var civilian = Seed();
        var army = Seed();
        Assert.True(army.Enlist(10).Succeeded);
        civilian.Resume();
        army.Resume();
        Assert.True(civilian.Advance(TimeSpan.FromSeconds(120)).Succeeded);
        Assert.True(army.Advance(TimeSpan.FromSeconds(120)).Succeeded);
        Assert.Equal(40, civilian.Population);
        Assert.Equal(40, army.Population);
        Assert.Equal(civilian.HousingCapacity, army.HousingCapacity);
        Assert.Equal(civilian.Resources["food"], army.Resources["food"]);
        Assert.Equal(civilian.Resources["bread"], army.Resources["bread"]);
        Assert.Equal(10L, army.MilitaryState.EnlistedPopulation);
        Assert.Equal(27, army.AvailableWorkers);
    }
}
