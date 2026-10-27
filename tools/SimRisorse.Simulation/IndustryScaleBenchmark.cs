using System.Text.Json;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Simulation;

/// <summary>
/// M5.1c: five fixed, synthetic coal-allocation experiments against the current catalog.
/// Ore is seeded to isolate coal; purchases, staffing and per-second production use GameSession.
/// This is not a new campaign strategy and does not modify any gameplay parameters.
/// </summary>
internal static class IndustryScaleBenchmark
{
    private const int HorizonSeconds = 180;
    private const int StartingPopulation = 40;
    private const int StartingOre = 1000;

    private sealed record Setup(string Id, int IronSmelters, int GoldSmelters, int CoalMines);

    internal sealed record ProfileResult(string Id, int IronSmelters, int GoldSmelters, int CoalMines,
        int AssignedWorkers, decimal PlanksSpent, decimal StoneSpent,
        decimal IronProduced, decimal GoldProduced, decimal CoalProduced, decimal CoalConsumed, decimal CoalRemaining,
        int IronRunningWorkerSeconds, int GoldRunningWorkerSeconds,
        int IronMissingCoalWorkerSeconds, int GoldMissingCoalWorkerSeconds,
        int ReconciledSeconds);

    internal sealed record Comparison(string ScenarioId, string RulesVersion, int StartPopulation,
        int HorizonSeconds, string InitialState, string Limitation, string[] ReplayPatterns,
        ProfileResult[] Profiles);

    internal static Comparison Run()
    {
        Setup[] setups =
        [
            new("A-balanced", 1, 1, 1),
            new("B-iron-priority", 2, 1, 1),
            new("C-coal-recovery", 2, 1, 2),
            new("D-gold-contention", 1, 2, 1),
            new("E-full-supply", 2, 2, 2)
        ];
        var results = new List<ProfileResult>(setups.Length);
        foreach (var setup in setups)
        {
            var whole = RunProfile(setup, split: false);
            var split = RunProfile(setup, split: true);
            Require(JsonSerializer.Serialize(whole.Result) == JsonSerializer.Serialize(split.Result) &&
                whole.Snapshot == split.Snapshot,
                $"The {setup.Id} replay differs between whole and fractional seconds.");
            results.Add(whole.Result);
        }
        var scenario = CivilGrowthScenario.Create();
        return new(scenario.Id, scenario.Catalog.RulesVersion, StartingPopulation, HorizonSeconds,
            "Synthetic colony: 40/40 housing, 2 warehouses, 1700 planks and stone, 1000 of each raw ore, " +
            "100 food and bread, zero coal/metals; purchased smelters and coal mines, ore mines present but unstaffed",
            "Isolates coal priority with prepaid raw ore; not a new-game timeline, price optimization, " +
            "long-term food sustainability or end-game balance measure",
            ["180 whole seconds", "180 x (250 ms + 750 ms); results and final snapshots identical"], results.ToArray());
    }

    private static (ProfileResult Result, string Snapshot) RunProfile(Setup setup, bool split)
    {
        var scenario = CivilGrowthScenario.Create();
        var original = GameSession.Create(scenario).Snapshot();
        var game = GameSession.Restore(scenario, original with
        {
            Population = StartingPopulation,
            Resources = ResourceBundle.Create(("planks", 1700m), ("stone", 1700m),
                ("food", 100m), ("bread", 100m), ("iron-ore", StartingOre), ("gold-ore", StartingOre)),
            Buildings = [.. original.Buildings.Select(item => item.BuildingId switch
            {
                "large-house" or "bakery" => item with { Count = 1 },
                "warehouse" => item with { Count = 2 },
                _ => item
            })]
        });

        var planksSpent = 0m;
        var stoneSpent = 0m;
        void Buy(string id, int count, bool staff)
        {
            Require(game.Construct(id, count).Succeeded, $"{setup.Id}: cannot construct {count} {id}.");
            var cost = scenario.Catalog.BuildingsById[id].Cost.Scale(count);
            planksSpent += cost["planks"].Units;
            stoneSpent += cost["stone"].Units;
            if (staff) Require(game.AssignWorkers(id, count).Succeeded, $"{setup.Id}: cannot staff {id}.");
        }

        Buy("iron-mine", 1, staff: false);
        Buy("gold-mine", 1, staff: false);
        Buy("coal-mine", setup.CoalMines, staff: true);
        Buy("iron-smelter", setup.IronSmelters, staff: true);
        Buy("gold-smelter", setup.GoldSmelters, staff: true);

        Require(game.Population == StartingPopulation && game.HousingCapacity == StartingPopulation,
            "The synthetic colony must be at its housing limit.");
        var purchased = game.Resources;
        var ironRunning = 0;
        var goldRunning = 0;
        var ironCoalMissing = 0;
        var goldCoalMissing = 0;
        var reconciled = 0;
        decimal coalProduced = 0m, coalConsumed = 0m;
        game.Resume();
        for (var second = 0; second < HorizonSeconds; second++)
        {
            var forecast = game.ForecastEconomy();
            Require(forecast.Population.BlockReason == GrowthBlockReason.HousingFull,
                $"{setup.Id}: population conditions changed.");
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
                var recipe = scenario.Catalog.BuildingsById[production.BuildingId].Recipe!;
                inputs += recipe.Input.Scale(production.RunnableWorkers);
                outputs += recipe.Output.Scale(production.RunnableWorkers);
            }
            var coal = forecast.Buildings.Single(item => item.BuildingId == "coal-mine");
            var iron = forecast.Buildings.Single(item => item.BuildingId == "iron-smelter");
            var gold = forecast.Buildings.Single(item => item.BuildingId == "gold-smelter");
            coalProduced += 2m * coal.RunnableWorkers;
            coalConsumed += iron.RunnableWorkers + gold.RunnableWorkers;
            ironRunning += iron.RunnableWorkers;
            goldRunning += gold.RunnableWorkers;
            if (iron.MissingInputIds.Contains("coal"))
                ironCoalMissing += iron.AssignedWorkers - iron.RunnableWorkers;
            if (gold.MissingInputIds.Contains("coal"))
                goldCoalMissing += gold.AssignedWorkers - gold.RunnableWorkers;

            var before = game.Resources;
            if (split)
            {
                Require(game.Advance(TimeSpan.FromMilliseconds(250)).Succeeded, "First fractional advance failed.");
                Require(game.Resources.Equals(before), "A partial second produced resources prematurely.");
                Require(game.Advance(TimeSpan.FromMilliseconds(750)).Succeeded, "Second fractional advance failed.");
            }
            else Require(game.Advance(TimeSpan.FromSeconds(1)).Succeeded, "Full second advance failed.");
            Require(game.Resources.Equals(before - consumed - inputs + outputs),
                $"{setup.Id}: production ledger diverged at second {second + 1}.");
            Require(game.StorageCapacity.Covers(game.Resources), $"{setup.Id}: storage capacity exceeded.");
            Require(game.Population == StartingPopulation && game.GrowthProgressSeconds == 0 &&
                game.GrowthRemainderTwentieths == 0, $"{setup.Id}: population progressed unexpectedly.");
            reconciled++;
        }

        var expectedIron = (HorizonSeconds - 1) * Math.Min(setup.IronSmelters, setup.CoalMines * 2);
        var expectedGold = (HorizonSeconds - 1) * Math.Min(setup.GoldSmelters,
            Math.Max(0, setup.CoalMines * 2 - setup.IronSmelters));
        Require(game.Resources["iron"].Units == expectedIron && game.Resources["gold"].Units == expectedGold,
            $"{setup.Id}: coal allocation changed from the declared priority and capacity.");
        Require(coalProduced - coalConsumed == game.Resources["coal"].Units &&
            ironRunning == expectedIron && goldRunning == expectedGold &&
            ironCoalMissing == setup.IronSmelters * HorizonSeconds - expectedIron &&
            goldCoalMissing == setup.GoldSmelters * HorizonSeconds - expectedGold,
            $"{setup.Id}: coal/worker ledger does not balance.");
        Require(purchased["planks"].Units + planksSpent == 1700m &&
            purchased["stone"].Units + stoneSpent == 1700m,
            $"{setup.Id}: the real purchases do not balance.");
        game.Pause();
        var result = new ProfileResult(setup.Id, setup.IronSmelters, setup.GoldSmelters, setup.CoalMines,
            setup.IronSmelters + setup.GoldSmelters + setup.CoalMines,
            planksSpent, stoneSpent, game.Resources["iron"].Units, game.Resources["gold"].Units,
            coalProduced, coalConsumed, game.Resources["coal"].Units,
            ironRunning, goldRunning, ironCoalMissing, goldCoalMissing, reconciled);
        return (result, JsonSerializer.Serialize(game.Snapshot()));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
