using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Simulation;

/// <summary>
/// Controlled midgame and high-density stress comparisons using the real civil
/// catalog and complete grain/water/flour/bread recipes. Each profile starts from
/// an identical synthetic snapshot across strategies: throughput, not setup time.
/// </summary>
internal static class BreadScaleBenchmark
{
    private const int DurationSeconds = 30 * 60;
    private const int LateSecondBakeryAt = 15 * 60;
    private const int MidgamePopulation = 220;
    private const int HighDensityPopulation = 320;

    internal sealed record StrategyResult(string Id, int FinalPopulation, int FinalAdvancedResidents,
        int HousingCapacity, int? SecondsTo240, int? SecondsTo250, int MissingFoodSeconds,
        int MissingBreadSeconds, int HousingFullSeconds, int BakeryRunningWorkerSeconds,
        int BakeryStorageLimitedSeconds, int AssignedWorkers, decimal FinalBread,
        decimal FinalFood, decimal AdditionalPlanksSpent, decimal AdditionalStoneSpent,
        int ReconciledSeconds, int FarmRunningWorkerSeconds, int WellRunningWorkerSeconds,
        int MillRunningWorkerSeconds, decimal BreadProduced, decimal BreadConsumed,
        decimal FinalGrain, decimal FinalWater, decimal FinalFlour,
        int? SecondsTo340, int? SecondsTo350);

    internal sealed record Comparison(string ScenarioId, string RulesVersion, int StartPopulation,
        int DurationSeconds, int SingleBakeryContinuousAdvancedLimit, int SecondChainWorkplaces,
        int LeanChainWorkplaces, StrategyResult[] Strategies);

    public static Comparison Run() => RunProfile(MidgamePopulation, largeHouses: 8, foodChains: 3);

    public static Comparison RunHighDensity() => RunProfile(HighDensityPopulation, largeHouses: 11, foodChains: 4);

    private static Comparison RunProfile(int startPopulation, int largeHouses, int foodChains)
    {
        var scenario = CivilGrowthScenario.CreateLegacyForBenchmarks();
        var advanced = scenario.PopulationRules?.AdvancedHousing ??
            throw new InvalidOperationException("The civil scenario has no advanced ration.");
        var bakery = scenario.Catalog.BuildingsById["bakery"].Recipe ??
            throw new InvalidOperationException("The bakery recipe is missing.");
        var maxResidents = checked((int)decimal.Floor(
            bakery.Output[advanced.BreadResourceId].Units / advanced.BreadPerPersonPerSecond.Units));
        var workers = 2 * scenario.Catalog.BuildingsById["farm"].WorkplacesPerBuilding +
            scenario.Catalog.BuildingsById["well"].WorkplacesPerBuilding +
            scenario.Catalog.BuildingsById["mill"].WorkplacesPerBuilding +
            scenario.Catalog.BuildingsById["bakery"].WorkplacesPerBuilding;
        return new(scenario.Id, scenario.Catalog.RulesVersion, startPopulation,
            DurationSeconds, maxResidents, workers,
            workers - scenario.Catalog.BuildingsById["farm"].WorkplacesPerBuilding,
            [RunStrategy("one-chain", startPopulation, largeHouses, foodChains),
                RunStrategy("two-chains", startPopulation, largeHouses, foodChains),
                RunStrategy("second-chain-late", startPopulation, largeHouses, foodChains),
                RunStrategy("lean-chain", startPopulation, largeHouses, foodChains)]);
    }

    private static StrategyResult RunStrategy(string id, int startPopulation, int largeHouses, int foodChains)
    {
        var scenario = CivilGrowthScenario.CreateLegacyForBenchmarks();
        var initial = GameSession.Create(scenario).Snapshot();
        var game = GameSession.Restore(scenario, initial with
        {
            Population = startPopulation,
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m),
                ("food", 1000m), ("bread", 100m)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId switch
            {
                "large-house" => item with { Count = largeHouses },
                "fisher" or "food-store" => item with { Count = foodChains, AssignedWorkers = foodChains },
                "farm" => item with { Count = 2, AssignedWorkers = 2 },
                "well" or "mill" or "bakery" => item with { Count = 1, AssignedWorkers = 1 },
                _ => item
            })]
        });
        decimal spentPlanks = 0m, spentStone = 0m;
        void AddChain(int additionalFarms)
        {
            // Cost, workplaces and prerequisites are exercised through real game commands.
            foreach (var (buildingId, count) in new[]
            {
                ("farm", additionalFarms), ("well", 1), ("mill", 1), ("bakery", 1)
            })
            {
                var constructed = game.Construct(buildingId, count);
                if (!constructed.Succeeded)
                    throw new InvalidOperationException($"Bread scale {id}: construct {buildingId}: {constructed.Error}.");
                var assigned = game.AssignWorkers(buildingId, count);
                if (!assigned.Succeeded)
                    throw new InvalidOperationException($"Bread scale {id}: staff {buildingId}: {assigned.Error}.");
                var cost = scenario.Catalog.BuildingsById[buildingId].Cost.Scale(count);
                spentPlanks += cost["planks"].Units;
                spentStone += cost["stone"].Units;
            }
        }

        switch (id)
        {
            case "two-chains": AddChain(additionalFarms: 2); break;
            case "lean-chain": AddChain(additionalFarms: 1); break;
            case "one-chain" or "second-chain-late": break;
            default: throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown bread strategy.");
        }
        game.Resume();
        int? to240 = startPopulation >= 240 ? 0 : null;
        int? to250 = startPopulation >= 250 ? 0 : null;
        int? to340 = startPopulation >= 340 ? 0 : null;
        int? to350 = startPopulation >= 350 ? 0 : null;
        var missingFood = 0;
        var missingBread = 0;
        var housingFull = 0;
        var bakeryWorkerSeconds = 0;
        var bakeryStorageSeconds = 0;
        var farmWorkerSeconds = 0;
        var wellWorkerSeconds = 0;
        var millWorkerSeconds = 0;
        var breadProduced = 0m;
        var breadConsumed = 0m;
        var reconciledSeconds = 0;

        for (var second = 0; second < DurationSeconds; second++)
        {
            if (id == "second-chain-late" && second == LateSecondBakeryAt)
                AddChain(additionalFarms: 2);
            var forecast = game.ForecastEconomy();
            switch (forecast.Population.BlockReason)
            {
                case GrowthBlockReason.MissingFood: missingFood++; break;
                case GrowthBlockReason.MissingBread: missingBread++; break;
                case GrowthBlockReason.HousingFull: housingFull++; break;
            }
            var bakery = forecast.Buildings.Single(item => item.BuildingId == "bakery");
            bakeryWorkerSeconds += bakery.RunnableWorkers;
            if (bakery.AssignedWorkers > 0 && bakery.BlockReason.HasFlag(ProductionBlockReason.StorageFull))
                bakeryStorageSeconds++;

            var before = game.Resources;
            var beforePopulation = game.Population;
            var beforeProgress = game.GrowthProgressSeconds;
            var beforeRemainder = game.GrowthRemainderTwentieths;
            if (!game.Advance(TimeSpan.FromSeconds(1)).Succeeded)
                throw new InvalidOperationException($"Bread scale {id}: advancing second {second + 1} failed.");

            // Reconcile the forecast with every actual resource change and birth.
            var consumed = ResourceBundle.Empty;
            if (forecast.Population.FoodResourceId is { } foodId)
                consumed += ResourceBundle.FromAmounts([KeyValuePair.Create(foodId, forecast.Population.ConsumedFood)]);
            if (forecast.Population.BreadResourceId is { } breadId)
                consumed += ResourceBundle.FromAmounts([KeyValuePair.Create(breadId, forecast.Population.ConsumedBread)]);
            var inputs = ResourceBundle.Empty;
            var outputs = ResourceBundle.Empty;
            foreach (var production in forecast.Buildings)
            {
                if (production.RunnableWorkers == 0) continue;
                switch (production.BuildingId)
                {
                    case "farm": farmWorkerSeconds += production.RunnableWorkers; break;
                    case "well": wellWorkerSeconds += production.RunnableWorkers; break;
                    case "mill": millWorkerSeconds += production.RunnableWorkers; break;
                }
                var recipe = scenario.Catalog.BuildingsById[production.BuildingId].Recipe ??
                    throw new InvalidOperationException($"Bread scale {id}: missing recipe for {production.BuildingId}.");
                inputs += recipe.Input.Scale(production.RunnableWorkers);
                outputs += recipe.Output.Scale(production.RunnableWorkers);
            }
            breadProduced += outputs["bread"].Units;
            breadConsumed += forecast.Population.ConsumedBread.Units;
            var growing = forecast.Population.BlockReason == GrowthBlockReason.None;
            var accrued = (long)beforeProgress * 20 + beforeRemainder +
                (growing ? forecast.Population.GrowthRateTwentieths : 0);
            var arriving = growing && accrued >= forecast.Population.IntervalSeconds * 20L;
            if (arriving) accrued -= forecast.Population.IntervalSeconds * 20L;
            var expectedProgress = (int)(accrued / 20);
            var expectedRemainder = (int)(accrued % 20);
            if (!game.Resources.Equals(before - consumed - inputs + outputs) ||
                game.Population != beforePopulation + (arriving ? 1 : 0) ||
                game.GrowthProgressSeconds != expectedProgress || game.GrowthRemainderTwentieths != expectedRemainder)
                throw new InvalidOperationException($"Bread scale {id}: forecast diverged at second {second + 1}.");
            var capacity = game.StorageCapacity;
            if (game.Resources.Amounts.Any(item => item.Value.Milliunits > capacity[item.Key].Milliunits))
                throw new InvalidOperationException($"Bread scale {id}: storage overflow at second {second + 1}.");
            reconciledSeconds++;
            if (game.Population >= 240) to240 ??= second + 1;
            if (game.Population >= 250) to250 ??= second + 1;
            if (game.Population >= 340) to340 ??= second + 1;
            if (game.Population >= 350) to350 ??= second + 1;
        }

        if (game.Resources["bread"].Units != 100m + breadProduced - breadConsumed)
            throw new InvalidOperationException($"Bread scale {id}: cumulative bread accounting differs.");

        var population = game.ForecastEconomy().Population;
        return new(id, game.Population, population.AdvancedResidents, game.HousingCapacity,
            to240, to250, missingFood, missingBread, housingFull,
            bakeryWorkerSeconds, bakeryStorageSeconds, game.Population - game.AvailableWorkers,
            game.Resources["bread"].Units, game.Resources["food"].Units,
            spentPlanks, spentStone, reconciledSeconds,
            farmWorkerSeconds, wellWorkerSeconds, millWorkerSeconds, breadProduced, breadConsumed,
            game.Resources["grain"].Units, game.Resources["water"].Units, game.Resources["flour"].Units,
            to340, to350);
    }
}
