using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Simulation;

/// <summary>
/// Fixed, disclosed comparison policies. This is a repeatable experiment, not an optimizer
/// or a claim about which decisions a human player would make.
/// </summary>
internal static class HousingBenchmark
{
    private const int HorizonSeconds = 8 * 3600;
    private const int StandardBreadSecond = 600;
    private const int StandardLargeHouseSecond = 760;
    private const int EarlyBreadSecond = 481;
    private const int EarlyLargeHouseSecond = 641;
    private const int EarlyInfrastructureSecond = 900;

    private enum Strategy { SmallOnly, LargeStandard, LargeEarly }

    internal sealed record StrategyResult(string Id, int? SecondsTo48, int? SecondsTo100, int? SecondsTo136,
        int FinalPopulation, int HousingCapacity, int SmallHouses, int LargeHouses, int Fishermen,
        int Warehouses, decimal PlanksSpent, decimal StoneSpent, int HousingFullSeconds,
        int MissingFoodSeconds, int MissingBreadSeconds, int BakeryActiveSeconds,
        int BakeryStorageBlockedSeconds, decimal FinalFood, decimal FinalBread, int MaxAssignedWorkers,
        int ReconciledSeconds);

    internal sealed record Comparison(string ScenarioId, string RulesVersion, int HorizonSeconds,
        int[] Targets, StrategyResult[] Strategies);

    public static Comparison Run(bool accelerated = false)
    {
        var scenario = accelerated ? CivilGrowthScenario.Create() : CivilGrowthScenario.CreateLegacyForBenchmarks();
        return new(scenario.Id, scenario.Catalog.RulesVersion, HorizonSeconds, [48, 100, 136],
            [RunStrategy(Strategy.SmallOnly, accelerated), RunStrategy(Strategy.LargeStandard, accelerated), RunStrategy(Strategy.LargeEarly, accelerated)]);
    }

    private static StrategyResult RunStrategy(Strategy strategy, bool accelerated)
    {
        var scenario = accelerated ? CivilGrowthScenario.Create() : CivilGrowthScenario.CreateLegacyForBenchmarks();
        var game = GameSession.Create(scenario);
        var planksSpent = 0m;
        var stoneSpent = 0m;
        var housingFullSeconds = 0;
        var missingFoodSeconds = 0;
        var missingBreadSeconds = 0;
        var bakeryActiveSeconds = 0;
        var bakeryStorageBlockedSeconds = 0;
        var maxAssignedWorkers = 0;
        var reconciledSeconds = 0;
        int? to48 = null, to100 = null, to136 = null;
        var breadChainBuilt = false;
        var secondFoodChainBuilt = false;
        var earlyWarehouseBuilt = false;
        var smallHouses = 1;
        var largeHouses = 0;

        void Buy(string id, bool staff = false)
        {
            var result = game.Construct(id, 1);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Benchmark {strategy}: failed to build {id}: {result.Error}.");
            var cost = scenario.Catalog.BuildingsById[id].Cost;
            planksSpent += cost["planks"].Units;
            stoneSpent += cost["stone"].Units;
            if (id == "small-house") smallHouses++;
            if (id == "large-house") largeHouses++;
            if (staff)
            {
                result = game.AssignWorkers(id, 1);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Benchmark {strategy}: failed to staff {id}: {result.Error}.");
            }
        }

        bool BuyIfAffordable(string id)
        {
            if (!game.Resources.Covers(scenario.Catalog.BuildingsById[id].Cost)) return false;
            Buy(id);
            return true;
        }

        void BuyFoodChain()
        {
            Buy("fisher", staff: true);
            Buy("food-store", staff: true);
        }

        void BuyBreadChain()
        {
            var costs = ResourceBundle.Create(("planks", 420m), ("stone", 280m));
            if (!game.Resources.Covers(costs) || game.AvailableWorkers < 5)
                throw new InvalidOperationException($"Benchmark {strategy}: bread chain cannot be started on schedule.");
            foreach (var id in new[] { "farm", "farm", "well", "mill", "bakery" }) Buy(id, staff: true);
            breadChainBuilt = true;
        }

        BuyFoodChain();
        game.Resume();

        for (var second = 0; second < HorizonSeconds; second++)
        {
            // The first extra small house is needed for growth and for the five bread-chain workers.
            // A small-only colony continues buying a house when the existing ones fill up.
            if (game.Population == game.HousingCapacity &&
                (strategy == Strategy.SmallOnly ? smallHouses < 17 : smallHouses < 2))
                BuyIfAffordable("small-house");

            if (strategy is not Strategy.SmallOnly)
            {
                var breadSecond = strategy == Strategy.LargeEarly ? EarlyBreadSecond : StandardBreadSecond;
                var firstLargeSecond = strategy == Strategy.LargeEarly ? EarlyLargeHouseSecond : StandardLargeHouseSecond;
                if (!breadChainBuilt && second >= breadSecond) BuyBreadChain();
                if (breadChainBuilt && second >= firstLargeSecond && largeHouses < 4 &&
                    (largeHouses == 0 || game.Population == game.HousingCapacity))
                    BuyIfAffordable("large-house");
            }

            if (strategy == Strategy.LargeEarly && second >= EarlyInfrastructureSecond && !earlyWarehouseBuilt)
            {
                if (!BuyIfAffordable("warehouse"))
                    throw new InvalidOperationException("Early benchmark warehouse is not affordable on schedule.");
                earlyWarehouseBuilt = true;
            }

            // A second fishing chain is needed before the 100-person threshold.
            var secondFishDue = strategy == Strategy.LargeEarly ? second >= EarlyInfrastructureSecond : game.Population >= 80;
            if (!secondFoodChainBuilt && secondFishDue)
            {
                var cost = ResourceBundle.Create(("planks", 100m), ("stone", 50m));
                if (!game.Resources.Covers(cost) || game.AvailableWorkers < 2)
                    throw new InvalidOperationException($"Benchmark {strategy}: second food chain is not affordable.");
                BuyFoodChain();
                secondFoodChainBuilt = true;
            }

            var forecast = game.ForecastEconomy();
            switch (forecast.Population.BlockReason)
            {
                case GrowthBlockReason.HousingFull: housingFullSeconds++; break;
                case GrowthBlockReason.MissingFood: missingFoodSeconds++; break;
                case GrowthBlockReason.MissingBread: missingBreadSeconds++; break;
            }
            var bakery = forecast.Buildings.Single(item => item.BuildingId == "bakery");
            if (bakery.RunnableWorkers > 0) bakeryActiveSeconds++;
            if (bakery.AssignedWorkers > 0 && bakery.BlockReason.HasFlag(ProductionBlockReason.StorageFull))
                bakeryStorageBlockedSeconds++;
            maxAssignedWorkers = Math.Max(maxAssignedWorkers, game.Population - game.AvailableWorkers);

            var resourcesBefore = game.Resources;
            var populationBefore = game.Population;
            var advanced = game.Advance(TimeSpan.FromSeconds(1));
            if (!advanced.Succeeded)
                throw new InvalidOperationException($"Benchmark {strategy}: failed to advance: {advanced.Error}.");
            ReconcileSecond(scenario, strategy, second, forecast, resourcesBefore, populationBefore, game);
            reconciledSeconds++;
            if (game.Population >= 48) to48 ??= second + 1;
            if (game.Population >= 100) to100 ??= second + 1;
            if (game.Population >= 136) to136 ??= second + 1;

            var capacity = game.StorageCapacity;
            if (game.Resources.Amounts.Any(item => item.Value.Milliunits > capacity[item.Key].Milliunits))
                throw new InvalidOperationException($"Benchmark {strategy}: resource exceeded storage capacity.");
        }

        var buildings = game.Snapshot().Buildings.ToDictionary(item => item.BuildingId, item => item.Count);
        return new(strategy switch
        {
            Strategy.SmallOnly => "small-only",
            Strategy.LargeStandard => "large-standard",
            _ => "large-early"
        }, to48, to100, to136, game.Population, game.HousingCapacity,
            buildings["small-house"], buildings["large-house"], buildings["fisher"], buildings["warehouse"],
            planksSpent, stoneSpent, housingFullSeconds, missingFoodSeconds, missingBreadSeconds,
            bakeryActiveSeconds, bakeryStorageBlockedSeconds, game.Resources["food"].Units,
            game.Resources["bread"].Units, maxAssignedWorkers, reconciledSeconds);
    }

    /// <summary>
    /// Independent per-second bookkeeping from the published production forecast:
    /// sum the actual recipe inputs/outputs and reserved rations, then compare the
    /// resulting stocks and population with the Core's completed economic step.
    /// This is intentionally independent of the planner's internal allocations.
    /// </summary>
    private static void ReconcileSecond(SimRisorse.Core.Content.GameScenario scenario, Strategy strategy, int second,
        EconomyForecast forecast, ResourceBundle resourcesBefore, int populationBefore, GameSession game)
    {
        var consumed = ResourceBundle.Empty;
        if (forecast.Population.FoodResourceId is { } food)
            consumed += ResourceBundle.FromAmounts([KeyValuePair.Create(food, forecast.Population.ConsumedFood)]);
        if (forecast.Population.BreadResourceId is { } bread)
            consumed += ResourceBundle.FromAmounts([KeyValuePair.Create(bread, forecast.Population.ConsumedBread)]);

        var inputs = ResourceBundle.Empty;
        var outputs = ResourceBundle.Empty;
        foreach (var production in forecast.Buildings)
        {
            if (production.RunnableWorkers == 0) continue;
            var recipe = scenario.Catalog.BuildingsById[production.BuildingId].Recipe ??
                throw new InvalidOperationException($"Benchmark {strategy}: {production.BuildingId} has workers but no recipe.");
            inputs += recipe.Input.Scale(production.RunnableWorkers);
            outputs += recipe.Output.Scale(production.RunnableWorkers);
        }

        var expectedResources = resourcesBefore - (consumed + inputs) + outputs;
        var populationPlan = forecast.Population;
        var growing = populationPlan.BlockReason == GrowthBlockReason.None;
        var accrued = (long)populationPlan.ProgressSeconds * 20 + populationPlan.GrowthRemainderTwentieths +
            (growing ? populationPlan.GrowthRateTwentieths : 0);
        var arriving = growing && accrued >= populationPlan.IntervalSeconds * 20L;
        var expectedPopulation = populationBefore + (arriving ? 1 : 0);
        if (arriving) accrued -= populationPlan.IntervalSeconds * 20L;
        var expectedProgress = (int)(accrued / 20);
        var expectedRemainder = (int)(accrued % 20);
        if (!expectedResources.Equals(game.Resources) || game.Population != expectedPopulation ||
            game.GrowthProgressSeconds != expectedProgress || game.GrowthRemainderTwentieths != expectedRemainder)
            throw new InvalidOperationException($"Benchmark {strategy}: economy/forecast divergence at second {second + 1}.");
    }
}
