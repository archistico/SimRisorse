using SimRisorse.Application.Features.Construction.ConstructBuildings;
using SimRisorse.Application.Features.Population.AssignWorkers;
using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Features.Sessions.AdvanceTime;
using SimRisorse.Application.Features.Sessions.GetGameState;
using SimRisorse.Application.Features.Sessions.SetPause;
using SimRisorse.Application.Scenarios;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.ReadModels;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class AdvancedHousingIntegrationTests
{
    [Fact]
    public async Task FirstAdvancedBreadShortageIsReportedEvenWhenBasicFoodIsAlsoMissing()
    {
        var scenario = CivilGrowthScenario.CreateLegacyForBenchmarks();
        var initial = GameSession.Create(scenario).Snapshot();
        var pendingArrival = initial with
        {
            Population = 16,
            GrowthProgressSeconds = 59,
            Resources = ResourceBundle.Empty,
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId switch
            {
                "small-house" => item with { Count = 2 },
                "large-house" => item with { Count = 1 },
                _ => item
            })]
        };
        var runtime = new GameRuntime(scenario);
        var loaded = await new LoadGameHandler(runtime, new InMemoryStore(pendingArrival)).HandleAsync();
        Assert.True(loaded.Succeeded);
        Assert.Equal(GrowthBlockReason.MissingFood, loaded.View.PopulationState.BlockReason);
        Assert.Equal(BreadSupplyState.FirstAdvancedArrivalNeedsStock, loaded.View.BreadSupply.State);
        Assert.Null(loaded.View.BreadSupply.ReserveSecondsWithoutProduction);
        Assert.Equal(0m, loaded.View.BreadSupply.RequiredPerSecond);
        Assert.Null(loaded.View.BreadSupply.MinimumAssignedWorkers);
        Assert.Null(loaded.View.BreadSupply.ChainCapacity);

        var withBread = new GameRuntime(scenario);
        var recovered = await new LoadGameHandler(withBread, new InMemoryStore(pendingArrival with
        {
            Resources = ResourceBundle.Create(("bread", 1m))
        })).HandleAsync();
        Assert.True(recovered.Succeeded);
        Assert.Equal(GrowthBlockReason.MissingFood, recovered.View.PopulationState.BlockReason);
        Assert.Equal(BreadSupplyState.Hidden, recovered.View.BreadSupply.State);
    }

    [Fact]
    public async Task BreadSupplyProjectionDistinguishesInputsStorageAndRealThroughput()
    {
        var scenario = CivilGrowthScenario.CreateLegacyForBenchmarks();
        var initial = GameSession.Create(scenario).Snapshot();
        var inhabited = initial with
        {
            Population = 17,
            Resources = ResourceBundle.Create(("food", 100m), ("bread", 20m)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId switch
            {
                "large-house" => item with { Count = 1 },
                "bakery" => item with { Count = 1, AssignedWorkers = 1 },
                _ => item
            })]
        };

        async Task<GameView> Read(GameSnapshot snapshot)
        {
            var runtime = new GameRuntime(scenario);
            var result = await new LoadGameHandler(runtime, new InMemoryStore(snapshot)).HandleAsync();
            Assert.True(result.Succeeded);
            return result.View;
        }

        var withoutInputs = await Read(inhabited);
        Assert.Equal(BreadSupplyState.MissingInputs, withoutInputs.BreadSupply.State);
        Assert.Equal(148m, withoutInputs.BreadSupply.ReserveSecondsWithoutProduction);
        Assert.Equal(0.135m, withoutInputs.BreadSupply.RequiredPerSecond);
        Assert.Equal(3m, withoutInputs.BreadSupply.NominalProductionPerSecond);
        Assert.Equal(0m, withoutInputs.BreadSupply.NextSecondProductionPerSecond);
        Assert.Equal(0m, withoutInputs.BreadSupply.NominalShortfallPerSecond);
        Assert.Equal(1, withoutInputs.BreadSupply.MinimumAssignedWorkers);
        Assert.Equal(0, withoutInputs.BreadSupply.AdditionalAssignedWorkers);

        var withInputs = await Read(inhabited with
        {
            Resources = inhabited.Resources + ResourceBundle.Create(("flour", 1m), ("water", 1m))
        });
        Assert.Equal(BreadSupplyState.SufficientNextSecond, withInputs.BreadSupply.State);
        Assert.Equal(3m, withInputs.Resources.Single(item => item.Id == "bread").ProductionPerSecond);
        Assert.Equal(3m, withInputs.BreadSupply.NextSecondProductionPerSecond);

        var full = await Read(inhabited with
        {
            Resources = ResourceBundle.Create(("food", 100m), ("bread", 100m), ("flour", 1m), ("water", 1m))
        });
        Assert.Equal(BreadSupplyState.StorageFull, full.BreadSupply.State);
        Assert.Equal(0m, full.Resources.Single(item => item.Id == "bread").ProductionPerSecond);
        Assert.Equal(3m, full.BreadSupply.NominalProductionPerSecond);
        Assert.Equal(0m, full.BreadSupply.NominalShortfallPerSecond);
    }

    [Fact]
    public async Task BreadCapacityQuantifiesTheSecondBakeryNeededBeyondTwoHundredAdvancedResidents()
    {
        var scenario = CivilGrowthScenario.CreateLegacyForBenchmarks();
        var initial = GameSession.Create(scenario).Snapshot();
        var colony = initial with
        {
            Population = 220,
            Resources = ResourceBundle.Create(("food", 500m), ("bread", 50m)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId switch
            {
                "large-house" => item with { Count = 7 },
                "farm" => item with { Count = 2, AssignedWorkers = 2 },
                "well" or "mill" => item with { Count = 1, AssignedWorkers = 1 },
                "bakery" => item with { Count = 1, AssignedWorkers = 1 },
                _ => item
            })]
        };

        async Task<GameView> Read(GameSnapshot snapshot)
        {
            var runtime = new GameRuntime(scenario);
            var result = await new LoadGameHandler(runtime, new InMemoryStore(snapshot)).HandleAsync();
            Assert.True(result.Succeeded);
            return result.View;
        }

        // One fully staffed bakery can nominally supply only 200 advanced people.
        var oneBakery = await Read(colony);
        Assert.Equal(212, oneBakery.PopulationState.AdvancedResidents);
        Assert.Equal(BreadSupplyState.NominalCapacityInsufficient, oneBakery.BreadSupply.State);
        Assert.Equal(3.180m, oneBakery.BreadSupply.RequiredPerSecond);
        Assert.Equal(3m, oneBakery.BreadSupply.NominalProductionPerSecond);
        Assert.Equal(0.180m, oneBakery.BreadSupply.NominalShortfallPerSecond);
        Assert.Equal(2, oneBakery.BreadSupply.MinimumAssignedWorkers);
        Assert.Equal(1, oneBakery.BreadSupply.AdditionalAssignedWorkers);
        var oneChain = Assert.IsType<BreadChainCapacityView>(oneBakery.BreadSupply.ChainCapacity);
        Assert.Equal(3m, oneChain.BreadEquivalentCapacityPerSecond);
        Assert.Equal(new[] { "grain", "water", "flour", "bread" },
            oneChain.Stages.Select(item => item.ResourceId));
        Assert.Equal(new[] { "grain", "water", "flour", "bread" }, oneChain.LimitingResourceIds);
        Assert.Equal(2.120m, oneChain.Stages.Single(item => item.ResourceId == "grain").RequiredResourcePerSecond);
        Assert.Equal(3, oneChain.Stages.Single(item => item.ResourceId == "grain").MinimumWorkers);
        Assert.Equal(1.060m, oneChain.Stages.Single(item => item.ResourceId == "flour").RequiredResourcePerSecond);
        Assert.Equal(2, oneChain.Stages.Single(item => item.ResourceId == "flour").MinimumWorkers);

        var emptyBakery = await Read(colony with
        {
            Buildings = [.. colony.Buildings.Select(item => item.BuildingId == "bakery"
                ? item with { AssignedWorkers = 0 } : item)]
        });
        Assert.Equal(0m, emptyBakery.BreadSupply.NominalProductionPerSecond);
        Assert.Equal(3.180m, emptyBakery.BreadSupply.NominalShortfallPerSecond);
        Assert.Equal(2, emptyBakery.BreadSupply.MinimumAssignedWorkers);
        Assert.Equal(2, emptyBakery.BreadSupply.AdditionalAssignedWorkers);

        var twoBakeries = await Read(colony with
        {
            Buildings = [.. colony.Buildings.Select(item => item.BuildingId == "bakery"
                ? item with { Count = 2, AssignedWorkers = 2 } : item)]
        });
        Assert.Equal(6m, twoBakeries.BreadSupply.NominalProductionPerSecond);
        Assert.Equal(0m, twoBakeries.BreadSupply.NominalShortfallPerSecond);
        Assert.Equal(2, twoBakeries.BreadSupply.MinimumAssignedWorkers);
        Assert.Equal(0, twoBakeries.BreadSupply.AdditionalAssignedWorkers);
        var chainStillLimited = Assert.IsType<BreadChainCapacityView>(twoBakeries.BreadSupply.ChainCapacity);
        Assert.Equal(3m, chainStillLimited.BreadEquivalentCapacityPerSecond);
        Assert.Equal(new[] { "grain", "water", "flour" }, chainStillLimited.LimitingResourceIds);
        // Staffing alone cannot guarantee throughput: here flour and water are absent.
        Assert.Equal(0m, twoBakeries.BreadSupply.NextSecondProductionPerSecond);
        Assert.Equal(BreadSupplyState.MissingInputs, twoBakeries.BreadSupply.State);

        var oneFarm = await Read(colony with
        {
            Buildings = [.. colony.Buildings.Select(item => item.BuildingId switch
            {
                "farm" => item with { Count = 1, AssignedWorkers = 1 },
                "bakery" => item with { Count = 2, AssignedWorkers = 2 },
                _ => item
            })]
        });
        var grainLimited = Assert.IsType<BreadChainCapacityView>(oneFarm.BreadSupply.ChainCapacity);
        Assert.Equal(1.5m, grainLimited.BreadEquivalentCapacityPerSecond);
        Assert.Equal(new[] { "grain" }, grainLimited.LimitingResourceIds);

        var fullSecondChain = await Read(colony with
        {
            Buildings = [.. colony.Buildings.Select(item => item.BuildingId switch
            {
                "farm" => item with { Count = 4, AssignedWorkers = 4 },
                "well" or "mill" or "bakery" => item with { Count = 2, AssignedWorkers = 2 },
                _ => item
            })]
        });
        var completeChain = Assert.IsType<BreadChainCapacityView>(fullSecondChain.BreadSupply.ChainCapacity);
        Assert.Equal(6m, completeChain.BreadEquivalentCapacityPerSecond);
        Assert.All(completeChain.Stages, item => Assert.Equal(0, item.AdditionalWorkers));
        // The bound measures workers and recipes, not immediate stocks.
        Assert.Equal(0m, fullSecondChain.BreadSupply.NextSecondProductionPerSecond);
    }

    [Fact]
    public void TheBreadChainUnlocksLargeHousingAndBothRationsMatchTheEconomicForecast()
    {
        var runtime = new GameRuntime(CivilGrowthScenario.CreateLegacyForBenchmarks());
        BuildAndStaff(runtime, "fisher");
        BuildAndStaff(runtime, "food-store");
        new SetPauseHandler(runtime).Handle(new(false));
        var advance = new AdvanceTimeHandler(runtime);
        advance.Handle(new(TimeSpan.FromSeconds(61)));
        Assert.True(new ConstructBuildingsHandler(runtime).Handle(new("small-house", 1)).Succeeded);
        advance.Handle(new(TimeSpan.FromSeconds(539)));
        var ready = new GetGameStateHandler(runtime).Handle();
        Assert.Equal(16, ready.Population);
        Assert.Equal(539m, Resource(ready, "planks").Quantity);

        foreach (var building in new[] { "farm", "farm", "well", "mill", "bakery" })
            BuildAndStaff(runtime, building);
        advance.Handle(new(TimeSpan.FromSeconds(160)));
        var beforeConstruction = new GetGameStateHandler(runtime).Handle();
        Assert.Equal(99m, Resource(beforeConstruction, "bread").Quantity);
        Assert.Equal(GrowthBlockReason.HousingFull, beforeConstruction.PopulationState.BlockReason);
        var largeHouse = new ConstructBuildingsHandler(runtime).Handle(new("large-house", 1));
        Assert.True(largeHouse.Succeeded);
        Assert.Equal(48, largeHouse.View.PopulationState.HousingCapacity);
        Assert.Equal(GrowthBlockReason.None, largeHouse.View.PopulationState.BlockReason);

        advance.Handle(new(TimeSpan.FromSeconds(60)));
        var populated = new GetGameStateHandler(runtime).Handle();
        Assert.Equal(17, populated.Population);
        Assert.Equal(16, populated.PopulationState.BasicHousingCapacity);
        Assert.Equal(1, populated.PopulationState.AdvancedResidents);
        Assert.Equal(0.17m, populated.PopulationState.RequiredFoodPerSecond);
        Assert.Equal(0.015m, populated.PopulationState.RequiredBreadPerSecond);
        Assert.Equal(0.015m, Resource(populated, "bread").ConsumptionPerSecond);
        Assert.Equal(0.17m, Resource(populated, "food").ConsumptionPerSecond);

        var after = advance.Handle(new(TimeSpan.FromSeconds(1))).View;
        foreach (var resource in populated.Resources)
            Assert.Equal(resource.Quantity + resource.NetPerSecond, Resource(after, resource.Id).Quantity);
        Assert.Equal(98.985m, Resource(after, "bread").Quantity);
    }

    [Fact]
    public async Task NewlyProducedFoodRecoversGrowthOnlyOnTheFollowingStepWithoutTakingBreadEarly()
    {
        var scenario = CivilGrowthScenario.CreateLegacyForBenchmarks();
        var initial = GameSession.Create(scenario).Snapshot();
        var stored = initial with
        {
            Population = 17,
            GrowthProgressSeconds = 59,
            Resources = ResourceBundle.Create(("fish", 1m), ("bread", 1m)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId switch
            {
                "large-house" => item with { Count = 1 },
                "fisher" => item with { Count = 1 },
                "food-store" => item with { Count = 1, AssignedWorkers = 1 },
                _ => item
            })]
        };
        var runtime = new GameRuntime(scenario);
        var loaded = await new LoadGameHandler(runtime, new InMemoryStore(stored)).HandleAsync();
        Assert.True(loaded.Succeeded);
        Assert.Equal(GrowthBlockReason.MissingFood, loaded.View.PopulationState.BlockReason);
        Assert.Equal(0m, loaded.View.PopulationState.ConsumedFoodPerSecond);
        Assert.Equal(0m, loaded.View.PopulationState.ConsumedBreadPerSecond);
        Assert.Equal(1m, Resource(loaded.View, "food").ProductionPerSecond);

        new SetPauseHandler(runtime).Handle(new(false));
        var first = new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(1))).View;
        Assert.Equal(17, first.Population);
        Assert.Equal(59, first.PopulationState.ProgressSeconds);
        Assert.Equal(1m, Resource(first, "food").Quantity);
        Assert.Equal(1m, Resource(first, "bread").Quantity);
        Assert.Equal(GrowthBlockReason.None, first.PopulationState.BlockReason);
        Assert.Equal(0.17m, first.PopulationState.ConsumedFoodPerSecond);
        Assert.Equal(0.135m, first.PopulationState.ConsumedBreadPerSecond);

        var second = new AdvanceTimeHandler(runtime).Handle(new(TimeSpan.FromSeconds(1))).View;
        Assert.Equal(18, second.Population);
        Assert.Equal(0, second.PopulationState.ProgressSeconds);
        Assert.Equal(0.83m, Resource(second, "food").Quantity);
        Assert.Equal(0.865m, Resource(second, "bread").Quantity);
        Assert.Equal(0.18m, second.PopulationState.RequiredFoodPerSecond);
        Assert.Equal(0.150m, second.PopulationState.RequiredBreadPerSecond);
    }

    private static void BuildAndStaff(GameRuntime runtime, string id)
    {
        Assert.True(new ConstructBuildingsHandler(runtime).Handle(new(id, 1)).Succeeded);
        Assert.True(new AssignWorkersHandler(runtime).Handle(new(id, 1)).Succeeded);
    }

    private static ReadModels.ResourceView Resource(ReadModels.GameView view, string id) =>
        view.Resources.Single(item => item.Id == id);

    private sealed class InMemoryStore(GameSnapshot snapshot) : IGameSaveStore
    {
        public GameSnapshot Load() => snapshot;
        public void Save(GameSnapshot value) => throw new NotSupportedException();
    }
}
