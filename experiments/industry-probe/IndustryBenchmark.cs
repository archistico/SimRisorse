using System.Text.Json;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.IndustryProbe;

internal static class IndustryBenchmark
{
    private const int HorizonSeconds = 3600;

    internal sealed record FactoryResult(string Id, int RunningWorkerSeconds, int MissingInputSeconds, int StorageBlockedSeconds);
    internal sealed record ConstructionEvent(string BuildingId, int Second);
    internal sealed record Result(string Hypothesis, string Profile, string FinancingPolicy, string RulesVersion,
        decimal IndustryPlanksCost, decimal IndustryStoneCost, decimal FinancingPlanksSpent, decimal FinancingStoneSpent,
        int? FinancingWarehouseSecond, int ConstructionSecond, int FirstIronSecond, int? FirstGoldSecond,
        int SecondsTo100Iron, int? SecondsTo100Gold, decimal IronBeforeExpansion, decimal GoldBeforeExpansion,
        decimal MetalCapacityBeforeExpansion, int AssignedWorkers, ConstructionEvent[] ConstructionEvents, FactoryResult[] Factories,
        decimal IronAfterExpansion, decimal GoldAfterExpansion, int ReconciledSeconds);
    internal sealed record Comparison(string Status, string ScenarioId, string CivilianBaseline,
        int StartPopulation, string InitialState, int HorizonSeconds, int RecoverySeconds,
        string PurchasePolicy, string[] ReplayPatterns, Result[] Results);

    private sealed class Counters
    {
        public int Running;
        public int MissingInputs;
        public int StorageFull;
    }

    public static Comparison Run()
    {
        var results = new List<Result>();
        foreach (var expensive in new[] { false, true })
        foreach (var includeGold in new[] { false, true })
        foreach (var sequential in new[] { false, true })
        {
            var whole = RunProfile(expensive, includeGold, sequential, split: false);
            var split = RunProfile(expensive, includeGold, sequential, split: true);
            Require(JsonSerializer.Serialize(whole.Result) == JsonSerializer.Serialize(split.Result) &&
                whole.Checkpoint == split.Checkpoint, "Replay diverged between whole and split seconds.");
            results.Add(whole.Result);
        }
        return new("Experimental hypotheses; prices and recipes are not adopted gameplay rules", "industry-probe",
            CivilGrowthScenario.RulesVersion, 40,
            "Seeded civilian colony: 40 housing places, 10 assigned workers, food=100, bread=100; all other stocks zero",
            HorizonSeconds, 1,
            "Compare saving the full industrial budget (buying a warehouse if needed) with buying and staffing each supplier as soon as affordable, in the disclosed construction order",
            ["1 second", "250 ms + 750 ms; complete report and final snapshot identical"], results.ToArray());
    }

    private static (Result Result, string Checkpoint) RunProfile(bool expensive, bool includeGold, bool sequential, bool split)
    {
        var scenario = IndustryScenario.Create(expensive);
        Require(CatalogReachability.Analyze(scenario).IsComplete, "Experimental catalog has unreachable buildings.");
        var game = GameSession.Create(scenario);
        string[] ids = includeGold
            ? ["iron-mine", "coal-mine", "iron-smelter", "gold-mine", "gold-smelter"]
            : ["iron-mine", "coal-mine", "iron-smelter"];
        var budget = ids.Aggregate(ResourceBundle.Empty, (sum, id) => sum + scenario.Catalog.BuildingsById[id].Cost);
        var counters = ids.ToDictionary(id => id, _ => new Counters(), StringComparer.Ordinal);
        var constructionEvents = new List<ConstructionEvent>();
        var nextBuilding = 0;
        var spent = ResourceBundle.Empty;
        var produced = ResourceBundle.Empty;
        var consumed = ResourceBundle.Empty;
        var reconciled = 0;
        int? warehouseSecond = null, constructionSecond = null, firstIron = null, firstGold = null, to100Iron = null, to100Gold = null;

        void Buy(string id, bool staff = false)
        {
            Require(game.Construct(id, 1).Succeeded, $"Cannot construct {id}.");
            spent += scenario.Catalog.BuildingsById[id].Cost;
            if (staff) Require(game.AssignWorkers(id, 1).Succeeded, $"Cannot staff {id}.");
            constructionEvents.Add(new(id, checked((int)(game.ElapsedTicks / TimeSpan.TicksPerSecond))));
        }

        void Step()
        {
            var forecast = game.ForecastEconomy();
            Require(forecast.Population.BlockReason == GrowthBlockReason.HousingFull,
                "Synthetic colony must remain fed and at its disclosed housing limit.");
            var inputs = ResourceBundle.Create(("food", forecast.Population.ConsumedFood.Units),
                ("bread", forecast.Population.ConsumedBread.Units));
            var outputs = ResourceBundle.Empty;
            foreach (var production in forecast.Buildings)
            {
                if (counters.TryGetValue(production.BuildingId, out var count) && production.AssignedWorkers > 0)
                {
                    count.Running += production.RunnableWorkers;
                    if (production.BlockReason.HasFlag(ProductionBlockReason.MissingInputs)) count.MissingInputs++;
                    if (production.BlockReason.HasFlag(ProductionBlockReason.StorageFull)) count.StorageFull++;
                }
                if (production.RunnableWorkers == 0) continue;
                var recipe = scenario.Catalog.BuildingsById[production.BuildingId].Recipe!;
                inputs += recipe.Input.Scale(production.RunnableWorkers);
                outputs += recipe.Output.Scale(production.RunnableWorkers);
            }
            var before = game.Resources;
            if (split)
            {
                Require(game.Advance(TimeSpan.FromMilliseconds(250)).Succeeded, "First fractional advance failed.");
                Require(game.Resources.Equals(before), "A fractional step produced resources before the next whole second.");
                Require(game.Advance(TimeSpan.FromMilliseconds(750)).Succeeded, "Second fractional advance failed.");
            }
            else Require(game.Advance(TimeSpan.FromSeconds(1)).Succeeded, "Whole-second advance failed.");
            Require(game.Resources.Equals(before - inputs + outputs), "Forecast and actual stocks diverged.");
            produced += outputs;
            consumed += inputs;
            Require(game.Resources.Equals(scenario.InitialResources + produced - consumed - spent), "Cumulative resource ledger diverged.");
            Require(game.StorageCapacity.Covers(game.Resources), "Stocks exceeded capacity.");
            Require(game.Population == 40 && game.GrowthProgressSeconds == 0 && game.GrowthRemainderTwentieths == 0,
                "Synthetic population or growth progress changed.");
            reconciled++;
        }

        game.Resume();
        for (var second = 0; second < HorizonSeconds; second++)
        {
            if (constructionSecond is null)
            {
                if (sequential)
                {
                    while (nextBuilding < ids.Length && game.Resources.Covers(scenario.Catalog.BuildingsById[ids[nextBuilding]].Cost))
                        Buy(ids[nextBuilding++], staff: true);
                    if (nextBuilding == ids.Length) constructionSecond = second;
                }
                else
                {
                    if (!game.StorageCapacity.Covers(budget) &&
                        game.Resources.Covers(scenario.Catalog.BuildingsById["warehouse"].Cost))
                    {
                        Require(warehouseSecond is null, "One financing warehouse should cover the experimental budget.");
                        Buy("warehouse");
                        warehouseSecond = second;
                    }
                    if (game.Resources.Covers(budget))
                    {
                        foreach (var id in ids) Buy(id, staff: true);
                        constructionSecond = second;
                    }
                }
            }
            Step();
            if (game.Resources["iron"].Units > 0) firstIron ??= second + 1;
            if (game.Resources["gold"].Units > 0) firstGold ??= second + 1;
            if (game.Resources["iron"].Units >= 100m) to100Iron ??= second + 1;
            if (game.Resources["gold"].Units >= 100m) to100Gold ??= second + 1;
        }
        Require(constructionSecond.HasValue && firstIron.HasValue && to100Iron.HasValue, "Iron targets not reached.");
        Require(!includeGold || (firstGold.HasValue && to100Gold.HasValue), "Gold targets not reached.");
        Require(sequential || (firstIron == constructionSecond + 2 && (!includeGold || firstGold == constructionSecond + 2)),
            "A smelter used newly mined input too early or failed its initial production.");
        var ironBefore = game.Resources["iron"].Units;
        var goldBefore = game.Resources["gold"].Units;
        var capacityBefore = game.StorageCapacity["iron"].Units;
        Require(ironBefore == capacityBefore && (!includeGold || goldBefore == capacityBefore), "Metal storage did not fill.");
        Require(counters["iron-smelter"].StorageFull > 0 &&
            (!includeGold || counters["gold-smelter"].StorageFull > 0), "Storage blocking was not observed.");
        var financingSpent = spent;
        var factoryResults = ids.Select(id => new FactoryResult(id, counters[id].Running,
            counters[id].MissingInputs, counters[id].StorageFull)).ToArray();

        // Exercise recovery with a paid, real construction command, rather than editing stocks.
        Buy("warehouse");
        Step();
        Require(game.Resources["iron"].Units == ironBefore + 1m &&
            (!includeGold || game.Resources["gold"].Units == goldBefore + 1m), "Smelters did not resume after capacity expansion.");
        Require(game.ElapsedTicks == (HorizonSeconds + 1L) * TimeSpan.TicksPerSecond, "Incorrect elapsed time.");
        game.Pause();
        var result = new Result(expensive ? "B" : "A", includeGold ? "iron-and-gold" : "iron-only",
            sequential ? "as-affordable" : "whole-budget", scenario.Catalog.RulesVersion,
            budget["planks"].Units, budget["stone"].Units, financingSpent["planks"].Units, financingSpent["stone"].Units,
            warehouseSecond, constructionSecond!.Value, firstIron!.Value, firstGold, to100Iron!.Value, to100Gold,
            ironBefore, goldBefore, capacityBefore, game.Population - game.AvailableWorkers, constructionEvents.ToArray(), factoryResults,
            game.Resources["iron"].Units, game.Resources["gold"].Units, reconciled);
        return (result, JsonSerializer.Serialize(game.Snapshot()));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
