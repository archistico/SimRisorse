using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class TownHallStorageTests
{
    [Fact]
    public void TownHallExplainsTheStarterStockAndIsTheOnlySourceOfSharedBaseStorage()
    {
        var scenario = CivilGrowthScenario.CreateTownHallForBenchmarks();
        var game = GameSession.Create(scenario);
        Assert.Equal("civil-growth-12", scenario.Catalog.RulesVersion);
        Assert.Equal(21, scenario.Catalog.Buildings.Length);
        Assert.Equal(16, scenario.Catalog.Resources.Length);
        Assert.Empty(scenario.Capacity.Amounts);
        Assert.Equal(1, game.Snapshot().Buildings.Single(item => item.BuildingId == "town-hall").Count);
        Assert.True(scenario.Catalog.BuildingsById["town-hall"].IsScenarioFixed);
        Assert.Equal(120m, game.Resources["planks"].Units);
        Assert.Equal(80m, game.Resources["stone"].Units);
        Assert.Equal(30m, game.Resources["food"].Units);
        Assert.Equal(1050m, game.StorageCapacity["logs"].Units);
        Assert.Equal(1050m, game.StorageCapacity["planks"].Units);
        Assert.Equal(1050m, game.StorageCapacity["stone"].Units);
        Assert.Equal(1010m, game.StorageCapacity["food"].Units);
        Assert.Equal(1010m, game.StorageCapacity["bread"].Units);
        Assert.Equal(1000m, game.StorageCapacity["swords"].Units);
    }

    [Fact]
    public void MunicipalityCannotBeBuiltAgainOrAlteredByAHandEditedCheckpoint()
    {
        var scenario = CivilGrowthScenario.CreateTownHallForBenchmarks();
        var game = GameSession.Create(scenario);
        var before = game.Snapshot();
        Assert.Equal(GameError.FixedBuilding, game.Construct("town-hall", 1).Error);
        Assert.Equal(before.Resources, game.Resources);
        Assert.Equal(before.Buildings.ToArray(), game.Snapshot().Buildings.ToArray());
        foreach (var count in new[] { 0, 2 })
        {
            var corrupted = before with
            {
                Buildings = [.. before.Buildings.Select(item => item.BuildingId == "town-hall"
                    ? item with { Count = count } : item)]
            };
            Assert.Throws<ArgumentException>(() => GameSession.Restore(scenario, corrupted));
            Assert.Throws<ArgumentException>(() => new GameScenario(scenario.Id, scenario.Catalog,
                scenario.Population, scenario.Capacity, scenario.InitialResources, corrupted.Buildings,
                scenario.PopulationRules, scenario.MilitaryRules));
        }
    }

    [Fact]
    public void EveryProducerStoresOnlyItsOutputsAndSupportBuildingsStoreRelevantGoods()
    {
        var scenario = CivilGrowthScenario.CreateTownHallForBenchmarks();
        foreach (var building in scenario.Catalog.Buildings.Where(item => item.Recipe is not null))
        {
            var storage = Assert.IsType<ResourceBundle>(building.StoragePerBuilding);
            Assert.Equal(building.Recipe!.Output.Amounts.Keys.ToArray(), storage.Amounts.Keys.ToArray());
            Assert.All(storage.Amounts.Values, amount => Assert.Equal(50m, amount.Units));
        }
        var buildings = scenario.Catalog.BuildingsById;
        Assert.Equal(10m, buildings["small-house"].StoragePerBuilding!["food"].Units);
        Assert.Equal(10m, buildings["small-house"].StoragePerBuilding!["bread"].Units);
        Assert.Equal(25m, buildings["large-house"].StoragePerBuilding!["food"].Units);
        Assert.Equal(25m, buildings["large-house"].StoragePerBuilding!["bread"].Units);
        Assert.Equal(20m, buildings["barracks"].StoragePerBuilding!["swords"].Units);
        Assert.Equal(0m, buildings["food-store"].StoragePerBuilding!["bread"].Units);
        Assert.Equal(500m, buildings["warehouse"].StoragePerBuilding!["bread"].Units);
    }

    [Fact]
    public void BakeryAddsFiftyBreadPlacesWithoutHidingProductionProblems()
    {
        var scenario = CivilGrowthScenario.CreateTownHallForBenchmarks();
        var original = GameSession.Create(scenario).Snapshot();
        var prepared = GameSession.Restore(scenario, original with
        {
            Resources = ResourceBundle.Create(("planks", 500m), ("stone", 500m), ("food", 30m)),
            Buildings = [.. original.Buildings.Select(item => item.BuildingId is "farm" or "well" or "mill"
                ? item with { Count = 1 } : item)]
        });
        Assert.Equal(1010m, prepared.StorageCapacity["bread"].Units);
        Assert.True(prepared.Construct("bakery", 1).Succeeded);
        Assert.Equal(1060m, prepared.StorageCapacity["bread"].Units);
        Assert.True(prepared.AssignWorkers("bakery", 1).Succeeded);
        var production = prepared.ForecastProduction().Single(item => item.BuildingId == "bakery");
        Assert.True(production.BlockReason.HasFlag(ProductionBlockReason.MissingInputs));
        Assert.Equal(0, production.RunnableWorkers);
    }
}
