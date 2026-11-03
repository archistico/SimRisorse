using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class MilitaryV13IntegrationTests
{
    private static GameSession Seed()
    {
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot();
        return GameSession.Restore(scenario, initial with
        {
            Population = 40,
            Resources = ResourceBundle.Create(("planks", 900m), ("stone", 900m),
                ("food", 100m), ("bread", 100m), ("swords", 100m)),
            Buildings = [.. initial.Buildings.Select(building => building.BuildingId is "large-house" or "forge" or "barracks"
                ? building with { Count = 1 } : building)]
        });
    }

    [Fact]
    public void CurrentCatalogKeepsTownHallAndDistributedStorageButUsesV13Military()
    {
        var current = CivilGrowthScenario.Create();
        Assert.Equal("civil-growth-13", current.Catalog.RulesVersion);
        Assert.Equal(21, current.Catalog.Buildings.Length);
        Assert.Equal(16, current.Catalog.Resources.Length);
        Assert.Equal(260m, current.Catalog.BuildingsById["barracks"].Cost["planks"].Units);
        Assert.Equal(200m, current.Catalog.BuildingsById["barracks"].Cost["stone"].Units);
        Assert.Equal(1, GameSession.Create(current).Snapshot().Buildings.Single(x => x.BuildingId == "town-hall").Count);
        Assert.Equal(1000m, GameSession.Create(current).StorageCapacity["swords"].Units);
        Assert.Equal(1010m, GameSession.Create(current).StorageCapacity["bread"].Units);
        Assert.Equal(GameError.FixedBuilding, GameSession.Create(current).Construct("town-hall", 1).Error);
        var frozen = CivilGrowthScenario.CreateTownHallForBenchmarks();
        Assert.Equal("civil-growth-12", frozen.Catalog.RulesVersion);
        Assert.Equal(2, frozen.MilitaryRules!.SwordsPerRecruit);
        Assert.Equal(280m, frozen.Catalog.BuildingsById["barracks"].Cost["planks"].Units);
    }

    [Fact]
    public void NewTrainingDoesNotDestroyPopulationFoodOrCheckpointAndDischargeIsDeferred()
    {
        var game = Seed();
        var comparison = Seed();
        Assert.True(game.Enlist(3).Succeeded);
        Assert.Equal((2, 1, 0), (game.MilitaryState.WaitingRecruits,
            game.MilitaryState.TrainingRecruits, game.MilitaryState.AvailableSoldiers));
        Assert.Equal(85m, game.Resources["swords"].Units);
        Assert.Equal(34, game.AvailableWorkers);
        Assert.Equal(GameError.DischargeUnavailable, game.DischargeSoldiers(1).Error);
        game.Resume(); comparison.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(15)).Succeeded);
        Assert.True(comparison.Advance(TimeSpan.FromSeconds(15)).Succeeded);
        var pausedCheckpoint = game.Snapshot();
        var resumed = GameSession.Restore(CivilGrowthScenario.Create(), pausedCheckpoint);
        Assert.True(resumed.IsPaused);
        Assert.True(resumed.Advance(TimeSpan.FromHours(1)).Succeeded);
        Assert.Equal(pausedCheckpoint.Military, resumed.MilitaryState);
        Assert.Equal(pausedCheckpoint.ElapsedTicks, resumed.ElapsedTicks);
        Assert.Equal(pausedCheckpoint.Resources, resumed.Resources);
        resumed.Resume();
        Assert.True(resumed.Advance(TimeSpan.FromSeconds(75)).Succeeded);
        Assert.Equal((0, 0, 3), (resumed.MilitaryState.WaitingRecruits,
            resumed.MilitaryState.TrainingRecruits, resumed.MilitaryState.AvailableSoldiers));
        Assert.Equal(comparison.Population, game.Population);
        Assert.Equal(comparison.Resources["food"], game.Resources["food"]);
        Assert.Equal(comparison.Resources["bread"], game.Resources["bread"]);
        Assert.Equal(40, resumed.Population);
    }

    [Fact]
    public void CancelInTrainingReturnsSwordsAndPeopleAndIsStableAcrossCheckpoint()
    {
        var game = Seed();
        Assert.True(game.Enlist(2).Succeeded);
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(15)).Succeeded);
        var saved = game.Snapshot();
        var loaded = GameSession.Restore(CivilGrowthScenario.Create(), saved);
        Assert.Equal(saved.Military, loaded.MilitaryState);
        Assert.True(loaded.CancelQueuedRecruits(2).Succeeded);
        Assert.Equal(MilitaryTrainingState.IndividualEmpty, loaded.MilitaryState);
        Assert.Equal(100m, loaded.Resources["swords"].Units);
        Assert.Equal(37, loaded.AvailableWorkers);
    }
}
