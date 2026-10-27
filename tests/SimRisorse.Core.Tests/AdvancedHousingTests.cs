using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class AdvancedHousingTests
{
    [Fact]
    public void FirstAdvancedArrivalNeedsBreadAndNewPersonConsumesOnlyOnTheFollowingSecond()
    {
        var scenario = Scenario();
        var game = GameSession.Create(scenario);
        Assert.True(game.Construct("large-house", 1).Succeeded);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(60));
        Assert.Equal(8, game.Population);
        Assert.Equal(0, game.GrowthProgressSeconds);
        Assert.Equal(GrowthBlockReason.MissingBread, game.ForecastEconomy().Population.BlockReason);
        Assert.Equal(95.2m, game.Resources["food"].Units);

        var restored = GameSession.Restore(scenario, game.Snapshot() with
        {
            Resources = game.Resources + ResourceBundle.Create(("bread", 5m))
        });
        restored.Resume();
        restored.Advance(TimeSpan.FromSeconds(59));
        Assert.Equal(8, restored.Population);
        Assert.Equal(59, restored.GrowthProgressSeconds);
        restored.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(9, restored.Population);
        Assert.Equal(5m, restored.Resources["bread"].Units);
        Assert.Equal(1, restored.ForecastEconomy().Population.AdvancedResidents);
        Assert.Equal(0.015m, restored.ForecastEconomy().Population.RequiredBread.Units);
        restored.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(4.985m, restored.Resources["bread"].Units);
    }

    [Fact]
    public void MissingBreadSuspendsGrowthButNotBasicFoodConsumptionOrExistingPopulation()
    {
        var scenario = Scenario();
        var initial = GameSession.Create(scenario).Snapshot();
        var game = GameSession.Restore(scenario, initial with
        {
            Population = 10, GrowthProgressSeconds = 37, Resources = ResourceBundle.Create(("food", 10m)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId == "large-house" ? item with { Count = 1 } : item)]
        });
        var forecast = game.ForecastEconomy().Population;
        Assert.Equal(GrowthBlockReason.MissingBread, forecast.BlockReason);
        Assert.Equal(2, forecast.AdvancedResidents);
        Assert.Equal(0.03m, forecast.RequiredBread.Units);
        Assert.Equal(0m, forecast.ConsumedBread.Units);
        Assert.Equal(0.1m, forecast.ConsumedFood.Units);

        game.Resume();
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(10, game.Population);
        Assert.Equal(37, game.GrowthProgressSeconds);
        Assert.Equal(9.9m, game.Resources["food"].Units);

        var recovered = GameSession.Restore(scenario, game.Snapshot() with
        {
            Resources = game.Resources + ResourceBundle.Create(("bread", 10m))
        });
        recovered.Resume();
        recovered.Advance(TimeSpan.FromSeconds(23));
        Assert.Equal(11, recovered.Population);
        Assert.Equal(0, recovered.GrowthProgressSeconds);
        Assert.Equal(10m - 0.03m * 23, recovered.Resources["bread"].Units);
    }

    [Fact]
    public void MissingBasicFoodDoesNotConsumeAdvancedBreadOrErasePartialGrowth()
    {
        var scenario = Scenario();
        var initial = GameSession.Create(scenario).Snapshot();
        var game = GameSession.Restore(scenario, initial with
        {
            Population = 9, GrowthProgressSeconds = 42,
            Resources = ResourceBundle.Create(("bread", 10m)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId == "large-house" ? item with { Count = 1 } : item)]
        });
        var forecast = game.ForecastEconomy().Population;
        Assert.Equal(GrowthBlockReason.MissingFood, forecast.BlockReason);
        Assert.Equal(0.09m, forecast.RequiredFood.Units);
        Assert.Equal(0.015m, forecast.RequiredBread.Units);
        Assert.Equal(0m, forecast.ConsumedFood.Units);
        Assert.Equal(0m, forecast.ConsumedBread.Units);

        game.Resume();
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(9, game.Population);
        Assert.Equal(42, game.GrowthProgressSeconds);
        Assert.Equal(10m, game.Resources["bread"].Units);

        var recovered = GameSession.Restore(scenario, game.Snapshot() with
        {
            Resources = game.Resources + ResourceBundle.Create(("food", 2m))
        });
        recovered.Resume();
        recovered.Advance(TimeSpan.FromSeconds(18));
        Assert.Equal(10, recovered.Population);
        Assert.Equal(0, recovered.GrowthProgressSeconds);
        Assert.Equal(9.73m, recovered.Resources["bread"].Units);
    }

    [Fact]
    public void BuildingMoreSmallHousesReducesAdvancedResidentsWithoutLosingPeople()
    {
        var scenario = Scenario();
        var initial = GameSession.Create(scenario).Snapshot();
        var game = GameSession.Restore(scenario, initial with
        {
            Population = 9, Resources = ResourceBundle.Create(("food", 30m)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId == "large-house" ? item with { Count = 1 } : item)]
        });
        Assert.Equal(GrowthBlockReason.MissingBread, game.ForecastEconomy().Population.BlockReason);
        Assert.True(game.Construct("small-house", 1).Succeeded);
        var forecast = game.ForecastEconomy().Population;
        Assert.Equal(9, game.Population);
        Assert.Equal(16, forecast.BasicHousingCapacity);
        Assert.Equal(0, forecast.AdvancedResidents);
        Assert.Equal(0m, forecast.RequiredBread.Units);
        Assert.Equal(GrowthBlockReason.None, forecast.BlockReason);
    }

    [Fact]
    public void FullBreadStorageBlocksTheOvenWithoutBlockingPopulationRations()
    {
        var scenario = Scenario();
        var initial = GameSession.Create(scenario).Snapshot();
        var game = GameSession.Restore(scenario, initial with
        {
            Population = 9, Resources = ResourceBundle.Create(("food", 30m), ("bread", 100m)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId switch
            {
                "large-house" => item with { Count = 1 },
                "oven" => item with { Count = 1, AssignedWorkers = 1 },
                _ => item
            })]
        });
        var forecast = game.ForecastEconomy();
        Assert.Equal(GrowthBlockReason.None, forecast.Population.BlockReason);
        Assert.Equal(0, forecast.Buildings.Single(item => item.BuildingId == "oven").RunnableWorkers);
        Assert.Equal(ProductionBlockReason.StorageFull,
            forecast.Buildings.Single(item => item.BuildingId == "oven").BlockReason);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(99.985m, game.Resources["bread"].Units);
        Assert.Equal(9, game.Population);
        Assert.Equal(1, game.GrowthProgressSeconds);
    }

    [Fact]
    public void AdvancedRulesRejectUnknownResourcesBuildingsAndZeroRations()
    {
        var scenario = Scenario();
        Assert.Throws<ArgumentOutOfRangeException>(() => new AdvancedHousingRules("small-house", "bread", default));
        Assert.Throws<ArgumentException>(() => new GameScenario("invalid", scenario.Catalog, 8,
            scenario.Capacity, scenario.InitialResources, scenario.InitialBuildings,
            new PopulationRules("food", ResourceAmount.FromUnits(0.01m), 60,
                new AdvancedHousingRules("small-house", "unknown", ResourceAmount.FromUnits(0.015m)))));
        Assert.Throws<ArgumentException>(() => new GameScenario("invalid", scenario.Catalog, 8,
            scenario.Capacity, scenario.InitialResources, scenario.InitialBuildings,
            new PopulationRules("food", ResourceAmount.FromUnits(0.01m), 60,
                new AdvancedHousingRules("missing-house", "bread", ResourceAmount.FromUnits(0.015m)))));
        Assert.Throws<ArgumentException>(() => new GameScenario("invalid", scenario.Catalog, 8,
            scenario.Capacity, scenario.InitialResources, scenario.InitialBuildings,
            new PopulationRules("food", ResourceAmount.FromUnits(0.01m), 60,
                new AdvancedHousingRules("small-house", "food", ResourceAmount.FromUnits(0.015m)))));
    }

    [Fact]
    public void SecondOvenRestoresGrowthWhenAdvancedResidentsExceedOneOvensThroughput()
    {
        // A single oven yields 3 bread/s and can continuously feed at most 200
        // advanced residents (200 * 0.015). Above that threshold growth slows
        // once the stored buffer runs low; neither residents nor progress vanish.
        var reference = Scenario();
        var scenario = new GameScenario("advanced-throughput", reference.Catalog, 8,
            ResourceBundle.Create(("food", 5000m), ("bread", 100m)),
            ResourceBundle.Create(("food", 5000m), ("bread", 100m)),
            reference.InitialBuildings, reference.PopulationRules);
        var start = GameSession.Create(scenario).Snapshot();
        var populated = start with
        {
            Population = 220,
            Buildings = [.. start.Buildings.Select(item => item.BuildingId switch
            {
                "large-house" => item with { Count = 8 },
                "oven" => item with { Count = 1, AssignedWorkers = 1 },
                _ => item
            })]
        };
        var oneOven = GameSession.Restore(scenario, populated);
        var secondOvenLater = GameSession.Restore(scenario, populated);
        var twoOvens = GameSession.Restore(scenario, populated with
        {
            Buildings = [.. populated.Buildings.Select(item => item.BuildingId == "oven"
                ? item with { Count = 2, AssignedWorkers = 2 } : item)]
        });
        oneOven.Resume();
        secondOvenLater.Resume();
        twoOvens.Resume();

        Assert.True(oneOven.Advance(TimeSpan.FromMinutes(30)).Succeeded);
        Assert.True(secondOvenLater.Advance(TimeSpan.FromMinutes(15)).Succeeded);
        Assert.True(secondOvenLater.Construct("oven", 1).Succeeded);
        Assert.True(secondOvenLater.AssignWorkers("oven", 1).Succeeded);
        Assert.True(secondOvenLater.Advance(TimeSpan.FromMinutes(15)).Succeeded);
        Assert.True(twoOvens.Advance(TimeSpan.FromMinutes(30)).Succeeded);

        Assert.Equal(250, twoOvens.Population);
        Assert.True(oneOven.Population < secondOvenLater.Population);
        Assert.True(secondOvenLater.Population <= twoOvens.Population);
        Assert.True(oneOven.Resources["food"].Units > 0m);
        Assert.True(secondOvenLater.Resources["food"].Units > 0m);
        Assert.Equal(100m, scenario.Capacity["bread"].Units);
    }

    private static GameScenario Scenario()
    {
        var catalog = new GameCatalog("advanced-1", [new("food", "Cibo"), new("bread", "Pane")],
        [
            new("small-house", "Small house", ResourceBundle.Empty, 0, null, 10, [], HousingPerBuilding: 8),
            new("large-house", "Large house", ResourceBundle.Empty, 0, null, 20, [], HousingPerBuilding: 32),
            new("oven", "Oven", ResourceBundle.Empty, 1,
                new(ResourceBundle.Empty, ResourceBundle.Create(("bread", 3m))), 30, [])
        ]);
        return new("advanced", catalog, 8, ResourceBundle.Create(("food", 1000m), ("bread", 100m)),
            ResourceBundle.Create(("food", 100m)), [new BuildingState("small-house", 1, 0)],
            new PopulationRules("food", ResourceAmount.FromUnits(0.01m), 60,
                new AdvancedHousingRules("small-house", "bread", ResourceAmount.FromUnits(0.015m))));
    }
}
