using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class PopulationGrowthTests
{
    [Fact]
    public void PartialGrowthAndFractionalTimeResumeExactlyAndNewPeopleEatOnTheNextStep()
    {
        var scenario = Scenario();
        var game = GameSession.Create(scenario);
        game.Construct("house", 1);
        game.Resume();
        game.Advance(TimeSpan.FromMilliseconds(59500));
        Assert.Equal(8, game.Population);
        Assert.Equal(59, game.GrowthProgressSeconds);
        Assert.Equal(25.28m, game.Resources["food"].Units);
        var saved = game.Snapshot();
        var restored = GameSession.Restore(scenario, saved);
        restored.Advance(TimeSpan.FromHours(8));
        TestScenarios.EqualState(saved, restored.Snapshot());
        restored.Resume();
        restored.Advance(TimeSpan.FromMilliseconds(500));
        Assert.Equal(9, restored.Population);
        Assert.Equal(0, restored.GrowthProgressSeconds);
        Assert.Equal(25.2m, restored.Resources["food"].Units);
        Assert.Equal(0.09m, restored.ForecastEconomy().Population.RequiredFood.Units);
        restored.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(25.11m, restored.Resources["food"].Units);
        Assert.Equal(9, restored.AvailableWorkers);
    }

    [Fact]
    public void FullHousesDoNotBankArrivalsAndAdditionalHousesDoNotMultiplyTheRate()
    {
        var game = GameSession.Create(Scenario(food: 1000m));
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(120));
        Assert.Equal(8, game.Population);
        Assert.Equal(0, game.GrowthProgressSeconds);
        Assert.Equal(GrowthBlockReason.HousingFull, game.ForecastEconomy().Population.BlockReason);
        game.Construct("house", 2);
        game.Advance(TimeSpan.FromSeconds(59));
        Assert.Equal(8, game.Population);
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(9, game.Population);
        game.Advance(TimeSpan.FromSeconds(2000));
        Assert.Equal(24, game.Population);
        Assert.Equal(0, game.GrowthProgressSeconds);
        game.Construct("house", 1);
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(24, game.Population);
        Assert.Equal(1, game.GrowthProgressSeconds);
    }

    [Fact]
    public void ShortagePreservesPeopleAndPartialGrowthAndNewFoodIsAvailableOnTheNextStep()
    {
        var scenario = Scenario();
        var state = GameSession.Create(scenario).Snapshot() with
        {
            ElapsedTicks = 59 * TimeSpan.TicksPerSecond, GrowthProgressSeconds = 59,
            Resources = ResourceBundle.Create(("food", 0.079m), ("fish", 2m)),
            Buildings = [new("house", 2, 0), new("food-store", 1, 1)]
        };
        var game = GameSession.Restore(scenario, state);
        var forecast = game.ForecastEconomy();
        Assert.Equal(GrowthBlockReason.MissingFood, forecast.Population.BlockReason);
        Assert.Equal(0m, forecast.Population.ConsumedFood.Units);
        Assert.Equal(1, forecast.Buildings.Single(item => item.BuildingId == "food-store").RunnableWorkers);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(1.079m, game.Resources["food"].Units);
        Assert.Equal(8, game.Population);
        Assert.Equal(59, game.GrowthProgressSeconds);
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(9, game.Population);
        Assert.Equal(0, game.GrowthProgressSeconds);
        Assert.Equal(1.999m, game.Resources["food"].Units);
    }

    [Fact]
    public void PopulationReservesFoodBeforeRecipesAndFreedSpaceIsNotReusedInTheSameStep()
    {
        var catalog = new GameCatalog("priority-1", [new("food", "Food"), new("fish", "Fish")],
        [
            new("house", "House", ResourceBundle.Empty, 0, null, 0, [], 8),
            new("burner", "Burner", ResourceBundle.Empty, 1,
                new(ResourceBundle.Create(("food", 0.08m)), ResourceBundle.Create(("fish", 1m))), 1, []),
            new("food-store", "Food store", ResourceBundle.Empty, 1,
                new(ResourceBundle.Create(("fish", 1m)), ResourceBundle.Create(("food", 1m))), 2, [])
        ]);
        var scenario = new GameScenario("priority", catalog, 8, ResourceBundle.Create(("food", 1m), ("fish", 100m)),
            ResourceBundle.Create(("food", 0.08m)), [new("house", 2, 0), new("burner", 1, 1)],
            new("food", ResourceAmount.FromUnits(0.01m), 60));
        var game = GameSession.Create(scenario);
        Assert.Equal(0, game.ForecastProduction().Single(item => item.BuildingId == "burner").RunnableWorkers);
        game.Resume(); game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(0m, game.Resources["food"].Units);
        Assert.Equal(0m, game.Resources["fish"].Units);
        var full = GameSession.Create(scenario).Snapshot() with
        {
            Resources = ResourceBundle.Create(("food", 1m), ("fish", 1m)),
            Buildings = [new("house", 2, 0), new("burner", 0, 0), new("food-store", 1, 1)]
        };
        var restored = GameSession.Restore(scenario, full);
        Assert.True(restored.ForecastProduction().Single(item => item.BuildingId == "food-store").BlockReason.HasFlag(ProductionBlockReason.StorageFull));
        restored.Resume(); restored.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(0.92m, restored.Resources["food"].Units);
        Assert.Equal(1m, restored.Resources["fish"].Units);
    }

    [Theory]
    [InlineData("missing-progress")]
    [InlineData("negative-progress")]
    [InlineData("completed-progress")]
    [InlineData("overpopulation")]
    [InlineData("missing-housing")]
    [InlineData("housing-overflow")]
    [InlineData("old-format")]
    public void InvalidGrowthSnapshotsAreRejected(string change)
    {
        var scenario = Scenario();
        var valid = GameSession.Create(scenario).Snapshot();
        var invalid = change switch
        {
            "missing-progress" => valid with { GrowthProgressSeconds = null },
            "negative-progress" => valid with { GrowthProgressSeconds = -1 },
            "completed-progress" => valid with { GrowthProgressSeconds = 60 },
            "overpopulation" => valid with { Population = 9 },
            "missing-housing" => valid with { Buildings = [new("house", 0, 0), new("food-store", 0, 0)] },
            "housing-overflow" => valid with { Buildings = [new("house", int.MaxValue, 0), new("food-store", 0, 0)] },
            _ => valid with { FormatVersion = 1 }
        };
        Assert.Throws<ArgumentException>(() => GameSession.Restore(scenario, invalid));
    }

    [Fact]
    public void HousingOverflowIsAnAtomicFailedPurchase()
    {
        var game = GameSession.Create(Scenario());
        var before = game.Snapshot();
        Assert.Equal(GameError.NumericLimit, game.Construct("house", int.MaxValue).Error);
        TestScenarios.EqualState(before, game.Snapshot());
    }

    [Fact]
    public void GrowthDefinitionsRequireKnownFoodPositiveRatesAndEnoughInitialHousing()
    {
        var scenario = Scenario();
        Assert.Throws<ArgumentOutOfRangeException>(() => new PopulationRules("food", default, 60));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PopulationRules("food", ResourceAmount.FromUnits(0.01m), 0));
        Assert.Throws<ArgumentException>(() => new GameCatalog("invalid", scenario.Catalog.Resources,
            scenario.Catalog.Buildings.Select(item => item with { HousingPerBuilding = -1 })));
        Assert.Throws<ArgumentException>(() => new GameScenario("invalid", scenario.Catalog, 8, scenario.Capacity,
            scenario.InitialResources, [], scenario.PopulationRules));
        Assert.Throws<ArgumentException>(() => new GameScenario("invalid", scenario.Catalog, 8, scenario.Capacity,
            scenario.InitialResources, scenario.InitialBuildings, new("unknown", ResourceAmount.FromUnits(0.01m), 60)));
    }

    private static GameScenario Scenario(decimal food = 30m)
    {
        var catalog = new GameCatalog("growth-1", [new("food", "Food"), new("fish", "Fish")],
        [
            new("house", "House", ResourceBundle.Empty, 0, null, 10, [], 8),
            new("food-store", "Food store", ResourceBundle.Empty, 1,
                new(ResourceBundle.Create(("fish", 1m)), ResourceBundle.Create(("food", 1m))), 20, [])
        ]);
        return new("growth", catalog, 8, ResourceBundle.Create(("food", 1000m), ("fish", 1000m)),
            ResourceBundle.Create(("food", food)), [new BuildingState("house", 1, 0)],
            new("food", ResourceAmount.FromUnits(0.01m), 60));
    }
}
