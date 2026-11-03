using System.Text.Json;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Simulation;

/// <summary>
/// M5.2c preparatory benchmark on the actual v9 rules. All strategies buy the same
/// metal chains, forge and mint; only coal capacity and worker assignments differ.
/// Synthetic stocks isolate the industrial bottleneck from civilian bootstrapping.
/// No game rules, saves or personal player data are changed.
/// </summary>
internal static class ForgeMintCoalBenchmark
{
    private const int Population = 40;
    private const int OreStock = 1000;
    private const int SwordsGoal = 240;
    private const int CoinsGoal = 360;
    private static readonly int[] Horizons = [1800, 3600];
    private static readonly string[] Machines = ["coal-mine", "iron-smelter", "gold-smelter", "forge", "mint"];

    private enum Strategy { Phased, Parallel, AllStaffed }

    internal sealed record MachineResult(string BuildingId, int AssignedWorkerSeconds, int RunningWorkerSeconds,
        int MissingCoalWorkerSeconds, int MissingOtherInputWorkerSeconds, int StorageBlockedWorkerSeconds);

    internal sealed record ProfileResult(string Id, int HorizonSeconds, int CoalMines, int PeakAssignedWorkers,
        decimal PlanksSpent, decimal StoneSpent, int? FirstSwordsSecond, int? FirstCoinsSecond,
        int? GoalReachedSecond, decimal FinalSwords, decimal FinalCoins, decimal FinalIron,
        decimal FinalGold, decimal FinalCoal, decimal CoalProduced, decimal CoalConsumed,
        int ReconciledSeconds, MachineResult[] Machines);

    internal sealed record Comparison(string ScenarioId, string RulesVersion, int Population,
        int SwordsGoal, int CoinsGoal, int[] HorizonsSeconds, string InitialState,
        string Limitations, string ReplayCheck, ProfileResult[] Profiles);

    internal static Comparison Run()
    {
        var strategies = new[] { Strategy.Phased, Strategy.Parallel, Strategy.AllStaffed };
        var results = new List<ProfileResult>(strategies.Length * Horizons.Length);
        foreach (var strategy in strategies)
        {
            foreach (var horizon in Horizons)
            {
                var whole = RunProfile(strategy, horizon, fractional: false);
                var fractional = RunProfile(strategy, horizon, fractional: true);
                Require(JsonSerializer.Serialize(whole.Result) == JsonSerializer.Serialize(fractional.Result) &&
                    whole.Snapshot == fractional.Snapshot,
                    $"{strategy} {horizon}s: whole/fractional replay diverged.");
                results.Add(whole.Result);
            }
        }

        // These theoretical figures were calculated before v9 existed. They are
        // checked here using real GameSession transactions, never substituted as data.
        Require(results.Single(p => p.Id == "phased" && p.HorizonSeconds == 1800).GoalReachedSecond == 241,
            "The phased v9 goal no longer matches the independently calculated 241 seconds.");
        Require(results.Single(p => p.Id == "parallel" && p.HorizonSeconds == 1800).GoalReachedSecond == 122,
            "The parallel v9 goal no longer matches the independently calculated 122 seconds.");
        foreach (var horizon in Horizons)
        {
            var allStaffed = results.Single(p => p.Id == "all-staffed" && p.HorizonSeconds == horizon);
            Require(allStaffed.Machines.Single(m => m.BuildingId == "forge").MissingCoalWorkerSeconds > 0 &&
                allStaffed.Machines.Single(m => m.BuildingId == "mint").MissingCoalWorkerSeconds > 0 &&
                allStaffed.GoalReachedSecond > 241,
                $"The one-mine fully staffed profile no longer demonstrates actual coal contention ({horizon}s).");
        }

        var scenario = CivilGrowthScenario.CreateMintForBenchmarks();
        return new(scenario.Id, scenario.Catalog.RulesVersion, Population, SwordsGoal, CoinsGoal,
            Horizons,
            "Synthetic 40/40 housed residents, 8 pre-existing warehouses, a bakery with no workers; " +
            "2,600 planks, 2,400 stone, 1,000 iron ore, 1,000 gold ore, 2,000 food, 3,000 bread; " +
            "zero initial coal, iron, gold, swords and coins. Metal mines are purchased but left unstaffed.",
            "Industrial purchase costs only; does not include the existing civilian infrastructure, " +
            "building time, raw ore extraction, new-game reachability, staffing opportunity costs or campaign duration. " +
            "Blocking categories can overlap for the same assigned worker-second.",
            "All six profiles checked at one-second granularity, with identical results and final snapshots " +
            "for 1 s steps and 250 ms + 750 ms steps (including checkpoint restores at 100 and 1000 s).",
            results.ToArray());
    }

    private static (ProfileResult Result, string Snapshot) RunProfile(Strategy strategy, int horizon, bool fractional)
    {
        var scenario = CivilGrowthScenario.CreateMintForBenchmarks();
        Require(scenario.Catalog.RulesVersion == "civil-growth-9", "This benchmark requires the v9 catalog.");
        var initial = GameSession.Create(scenario).Snapshot();
        var game = GameSession.Restore(scenario, initial with
        {
            Population = Population,
            Resources = ResourceBundle.Create(("planks", 2600m), ("stone", 2400m),
                ("iron-ore", OreStock), ("gold-ore", OreStock), ("food", 2000m), ("bread", 3000m)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId switch
            {
                "large-house" or "bakery" => item with { Count = 1 },
                "warehouse" => item with { Count = 8 },
                _ => item
            })]
        });

        var coalMines = strategy == Strategy.Parallel ? 2 : 1;
        decimal planksSpent = 0m, stoneSpent = 0m;
        void Buy(string id, int count = 1)
        {
            Require(game.Construct(id, count).Succeeded, $"{strategy}: cannot buy {count} x {id}.");
            var cost = scenario.Catalog.BuildingsById[id].Cost.Scale(count);
            planksSpent += cost["planks"].Units;
            stoneSpent += cost["stone"].Units;
        }
        void Staff(string id) => Require(game.AssignWorkers(id, 1).Succeeded,
            $"{strategy}: cannot assign {id}.");

        Buy("iron-mine");
        Buy("gold-mine");
        Buy("coal-mine", coalMines);
        Buy("iron-smelter");
        Buy("gold-smelter");
        Buy("forge");
        Buy("mint");
        for (var i = 0; i < coalMines; i++) Staff("coal-mine");
        Staff("iron-smelter");
        Staff("gold-smelter");
        if (strategy != Strategy.Phased) { Staff("forge"); Staff("mint"); }

        Require(planksSpent == (coalMines == 1 ? 1300m : 1430m) &&
            stoneSpent == (coalMines == 1 ? 950m : 1040m), "The actual v9 construction costs changed.");
        Require(game.Population == Population && game.HousingCapacity == Population,
            "The synthetic population must stay at its housing limit.");

        var metrics = Machines.ToDictionary(id => id, id => new int[5], StringComparer.Ordinal);
        int? firstSwords = null, firstCoins = null, goalReached = null;
        var peakWorkers = Population - game.AvailableWorkers;
        decimal coalProduced = 0m, coalConsumed = 0m;
        game.Resume();

        for (var second = 1; second <= horizon; second++)
        {
            if (strategy == Strategy.Phased && second == 122)
            {
                Require(game.ReleaseWorkers("iron-smelter", 1).Succeeded &&
                    game.ReleaseWorkers("gold-smelter", 1).Succeeded,
                    "Phased strategy could not release the smelter workers.");
                Staff("forge");
                Staff("mint");
                peakWorkers = Math.Max(peakWorkers, Population - game.AvailableWorkers);
                Require(game.Resources["iron"].Units == 120m && game.Resources["gold"].Units == 120m,
                    "The phased strategy did not stockpile 120 of each metal at second 121.");
            }

            var forecast = game.ForecastEconomy();
            Require(forecast.Population.BlockReason == GrowthBlockReason.HousingFull &&
                forecast.Population.ConsumedFood.Units == 0.4m &&
                forecast.Population.ConsumedBread.Units == 0.48m,
                $"{strategy}: the civilian resources no longer sustain the controlled {horizon}s run.");
            var before = game.Resources;
            var expected = before - ResourceBundle.Create(("food", forecast.Population.ConsumedFood.Units),
                ("bread", forecast.Population.ConsumedBread.Units));
            foreach (var producer in forecast.Buildings)
            {
                if (producer.RunnableWorkers > 0)
                {
                    var recipe = scenario.Catalog.BuildingsById[producer.BuildingId].Recipe!;
                    expected = expected - recipe.Input.Scale(producer.RunnableWorkers) +
                        recipe.Output.Scale(producer.RunnableWorkers);
                }
                if (!metrics.TryGetValue(producer.BuildingId, out var counters)) continue;
                counters[0] += producer.AssignedWorkers;
                counters[1] += producer.RunnableWorkers;
                var stopped = producer.AssignedWorkers - producer.RunnableWorkers;
                if (producer.MissingInputIds.Contains("coal")) counters[2] += stopped;
                if (producer.MissingInputIds.Any(id => id != "coal")) counters[3] += stopped;
                if (producer.FullOutputIds.Length > 0) counters[4] += stopped;
                if (producer.BuildingId == "coal-mine") coalProduced += 2m * producer.RunnableWorkers;
                else if (producer.BuildingId is "iron-smelter" or "gold-smelter" or "forge" or "mint")
                    coalConsumed += producer.RunnableWorkers;
            }

            if (fractional)
            {
                Require(game.Advance(TimeSpan.FromMilliseconds(250)).Succeeded, "The first partial step failed.");
                Require(game.Resources.Equals(before), "A fractional step produced resources prematurely.");
                if (second is 100 or 1000)
                {
                    // Resume a partially elapsed second from an actual checkpoint.
                    game = GameSession.Restore(scenario, game.Snapshot());
                    game.Resume();
                }
                Require(game.Advance(TimeSpan.FromMilliseconds(750)).Succeeded, "The second partial step failed.");
            }
            else Require(game.Advance(TimeSpan.FromSeconds(1)).Succeeded, "A full step failed.");
            Require(game.Resources.Equals(expected), $"{strategy}: ledger diverged at second {second}.");
            Require(game.StorageCapacity.Covers(game.Resources) && game.Population == Population &&
                game.GrowthProgressSeconds == 0 && game.GrowthRemainderTwentieths == 0,
                $"{strategy}: storage or population invariant failed at second {second}.");

            if (firstSwords is null && game.Resources["swords"].Units > 0) firstSwords = second;
            if (firstCoins is null && game.Resources["coins"].Units > 0) firstCoins = second;
            if (goalReached is null && game.Resources["swords"].Units >= SwordsGoal &&
                game.Resources["coins"].Units >= CoinsGoal) goalReached = second;
        }

        var ironRun = metrics["iron-smelter"][1];
        var goldRun = metrics["gold-smelter"][1];
        var forgeRun = metrics["forge"][1];
        var mintRun = metrics["mint"][1];
        Require(game.Resources["iron"].Units == ironRun - forgeRun &&
            game.Resources["gold"].Units == goldRun - mintRun &&
            game.Resources["swords"].Units == forgeRun * 2m &&
            game.Resources["coins"].Units == mintRun * 3m &&
            game.Resources["iron-ore"].Units == OreStock - ironRun * 2m &&
            game.Resources["gold-ore"].Units == OreStock - goldRun &&
            game.Resources["coal"].Units == coalProduced - coalConsumed,
            $"{strategy}: cumulative industrial ledger failed at {horizon} s.");

        game.Pause();
        var result = new ProfileResult(strategy switch
            {
                Strategy.Phased => "phased", Strategy.Parallel => "parallel", _ => "all-staffed"
            }, horizon, coalMines, peakWorkers, planksSpent, stoneSpent,
            firstSwords, firstCoins, goalReached, game.Resources["swords"].Units,
            game.Resources["coins"].Units, game.Resources["iron"].Units,
            game.Resources["gold"].Units, game.Resources["coal"].Units,
            coalProduced, coalConsumed, horizon,
            [.. Machines.Select(id => new MachineResult(id, metrics[id][0], metrics[id][1],
                metrics[id][2], metrics[id][3], metrics[id][4]))]);
        return (result, JsonSerializer.Serialize(game.Snapshot()));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
