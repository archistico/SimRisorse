using System.Text.Json;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Simulation;

/// <summary>Current approved recipes: controlled oracles and sustained, food-supported comparisons.</summary>
internal static class ForgeMintBenchmark
{
    private static readonly string[] IndustryIds = ["iron-mine", "gold-mine", "coal-mine", "iron-smelter", "gold-smelter", "forge", "mint"];
    private sealed record Setup(string Id, string Mode, int CoalMines, int Seconds, bool LiveMining);
    internal sealed record ProductionMetrics(string BuildingId, int AssignedWorkerSeconds, int RunningWorkerSeconds,
        int MissingCoalWorkerSeconds, int MissingInputWorkerSeconds, int StorageBlockedWorkerSeconds);
    internal sealed record Profile(string Id, string Mode, bool LiveMining, int CoalMines, int HorizonSeconds,
        int PeakIndustrialWorkers, decimal PlanksSpent, decimal StoneSpent,
        int? FirstSwordsSecond, int? FirstCoinsSecond, int? TargetSecond,
        decimal Swords, decimal Coins, decimal Iron, decimal Gold, decimal CoalProduced, decimal CoalConsumed,
        decimal CoalRemaining, decimal FoodRemaining, decimal BreadRemaining, int ReconciledSeconds,
        ProductionMetrics[] Production);
    internal sealed record Guard(string Id, decimal Swords, decimal Coins, decimal Coal, decimal Iron, decimal Gold);
    internal sealed record Report(string ScenarioId, string RulesVersion, int TargetSwords, int TargetCoins,
        string[] InitialConditions, string[] Limitations, string[] ReplayPatterns, Profile[] Profiles, Guard[] Guards);

    internal static Report Run()
    {
        var setups = new List<Setup>
        {
            new("control-staged", "staged", 1, 241, false),
            new("control-parallel", "parallel", 2, 122, false),
            new("control-contended", "contended", 1, 180, false)
        };
        foreach (var seconds in new[] { 1800, 28800 })
            foreach (var mode in new[] { "staged", "parallel", "contended" })
                setups.Add(new($"live-{mode}-{seconds}", mode, mode == "parallel" ? 2 : 1, seconds, true));

        var profiles = new List<Profile>();
        foreach (var setup in setups)
        {
            var whole = RunProfile(setup, split: false);
            var split = RunProfile(setup, split: true);
            Require(JsonSerializer.Serialize(whole.Result) == JsonSerializer.Serialize(split.Result) && whole.Snapshot == split.Snapshot,
                $"{setup.Id}: whole/fractional checkpoint replay differs.");
            var result = whole.Result;
            Require(result.PlanksSpent == (setup.CoalMines == 1 ? 1300m : 1430m) &&
                result.StoneSpent == (setup.CoalMines == 1 ? 950m : 1040m), $"{setup.Id}: approved purchase costs changed.");
            if (!setup.LiveMining)
            {
                var staged = setup.Mode == "staged";
                var parallel = setup.Mode == "parallel";
                Require(result.Swords == (staged || parallel ? 240m : 0m) &&
                    result.Coins == (staged || parallel ? 360m : 0m) &&
                    result.TargetSecond == (staged ? 241 : parallel ? 122 : null) &&
                    result.FirstSwordsSecond == (staged ? 122 : parallel ? 3 : null) &&
                    result.FirstCoinsSecond == result.FirstSwordsSecond &&
                    result.CoalProduced == (staged ? 482m : parallel ? 488m : 360m) &&
                    result.CoalConsumed == (staged ? 480m : parallel ? 482m : 358m),
                    $"{setup.Id}: controlled 241/122 s oracle or coal balance changed.");
            }
            else if (setup.Seconds == 28800)
                Require(result.Swords == 2000m && result.Coins == 1998m && result.TargetSecond is not null,
                    $"{setup.Id}: sustained profile did not reach the final whole batches.");
            else
                Require(result.Swords == (setup.Mode == "staged" ? 1680m : setup.Mode == "parallel" ? 2000m : 0m) &&
                    result.Coins == (setup.Mode == "contended" ? 0m : 1998m), $"{setup.Id}: 30-minute production changed.");
            profiles.Add(result);
        }

        var guards = new List<Guard>();
        foreach (var id in new[] { "one-slot", "two-coin-slots", "exact-fit", "coal-priority" })
        {
            var whole = RunGuard(id, split: false);
            var split = RunGuard(id, split: true);
            Require(whole.Result == split.Result && whole.Snapshot == split.Snapshot, $"{id}: guard replay differs.");
            guards.Add(whole.Result);
        }
        return new(CivilGrowthScenario.Id, CivilGrowthScenario.RulesVersion, 240, 360,
            ["Synthetic colony: 40/40 residents, two existing warehouses, 1700 planks/stone; zero coal, metals and crafted products",
             "Existing staffed food and bread chains, 100 food/bread; foundation material workers released; infrastructure excluded from purchase costs",
             "Controls: 1000 of each ore, ore mines purchased but unstaffed; live profiles: zero ores and two staffed ore mines",
             "Staged policy repeats 121 s smelting then 120 s crafting; parallel runs all transformers with two coal mines; contended uses one",
             "Storage guards: separate fixed checkpoints at ordinary 1000 capacity, existing test buildings and inputs"],
            ["Fixed approved A prices only; historical B remains in the separate experiment",
             "Synthetic profile timings exclude financing and civilian prerequisites; use the crafting campaign timeline for a real start",
             "Production diagnostics overlap: missing-input and storage worker-seconds are not disjoint",
             "No recruitment, ship payment, optimality proof or full-campaign duration measure"],
            ["whole seconds", "250 ms + 750 ms, with a paused in-memory checkpoint restored halfway; all metrics and snapshots equal"],
            profiles.ToArray(), guards.ToArray());
    }

    private static (Profile Result, string Snapshot) RunProfile(Setup setup, bool split)
    {
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot();
        var support = new Dictionary<string, int>(StringComparer.Ordinal)
        { ["fisher"] = 1, ["food-store"] = 1, ["farm"] = 2, ["well"] = 1, ["mill"] = 1, ["bakery"] = 1 };
        var game = GameSession.Restore(scenario, initial with
        {
            Population = 40,
            Resources = ResourceBundle.Create(("planks", 1700m), ("stone", 1700m), ("food", 100m), ("bread", 100m),
                ("iron-ore", setup.LiveMining ? 0m : 1000m), ("gold-ore", setup.LiveMining ? 0m : 1000m)),
            Buildings = [.. initial.Buildings.Select(item => support.TryGetValue(item.BuildingId, out var count)
                ? item with { Count = count, AssignedWorkers = count }
                : item.BuildingId == "large-house" ? item with { Count = 1 }
                : item.BuildingId == "warehouse" ? item with { Count = 2 }
                : item with { AssignedWorkers = 0 })]
        });
        decimal planksSpent = 0, stoneSpent = 0;
        void Buy(string id, int count, int workers)
        {
            Require(game.Construct(id, count).Succeeded, $"{setup.Id}: cannot purchase {id}.");
            var cost = scenario.Catalog.BuildingsById[id].Cost.Scale(count);
            planksSpent += cost["planks"].Units;
            stoneSpent += cost["stone"].Units;
            if (workers > 0) Require(game.AssignWorkers(id, workers).Succeeded, $"{setup.Id}: cannot staff {id}.");
        }
        Buy("iron-mine", 1, setup.LiveMining ? 1 : 0);
        Buy("gold-mine", 1, setup.LiveMining ? 1 : 0);
        Buy("coal-mine", setup.CoalMines, setup.CoalMines);
        Buy("iron-smelter", 1, 1);
        Buy("gold-smelter", 1, 1);
        Buy("forge", 1, setup.Mode == "staged" ? 0 : 1);
        Buy("mint", 1, setup.Mode == "staged" ? 0 : 1);
        Require(game.Resources["planks"].Units + planksSpent == 1700m && game.Resources["stone"].Units + stoneSpent == 1700m,
            $"{setup.Id}: purchases do not reconcile.");

        // assigned, running, missing coal, missing any input, storage blocked; all in worker-seconds.
        var metrics = IndustryIds.ToDictionary(id => id, _ => new int[5], StringComparer.Ordinal);
        decimal coalProduced = 0, coalConsumed = 0;
        int? firstSwords = null, firstCoins = null, target = null;
        var peakWorkers = 0;
        game.Resume();
        for (var second = 0; second < setup.Seconds; second++)
        {
            if (setup.Mode == "staged" && (second % 241 == 121 || second > 0 && second % 241 == 0))
            {
                var crafting = second % 241 == 121;
                foreach (var id in crafting ? new[] { "iron-smelter", "gold-smelter" } : new[] { "forge", "mint" })
                    Require(game.ReleaseWorkers(id, 1).Succeeded, $"{setup.Id}: cannot release {id}.");
                foreach (var id in crafting ? new[] { "forge", "mint" } : new[] { "iron-smelter", "gold-smelter" })
                    Require(game.AssignWorkers(id, 1).Succeeded, $"{setup.Id}: cannot assign {id}.");
            }
            var forecast = game.ForecastEconomy();
            Require(forecast.Population.BlockReason == GrowthBlockReason.HousingFull,
                $"{setup.Id}: food, bread or housing diverged at second {second + 1}.");
            var consumption = ResourceBundle.FromAmounts(
            [
                KeyValuePair.Create("food", forecast.Population.ConsumedFood),
                KeyValuePair.Create("bread", forecast.Population.ConsumedBread)
            ]);
            var inputs = ResourceBundle.Empty;
            var outputs = ResourceBundle.Empty;
            var assigned = 0;
            foreach (var production in forecast.Buildings)
            {
                if (metrics.TryGetValue(production.BuildingId, out var counter))
                {
                    var blocked = production.AssignedWorkers - production.RunnableWorkers;
                    assigned += production.AssignedWorkers;
                    counter[0] += production.AssignedWorkers;
                    counter[1] += production.RunnableWorkers;
                    if (production.MissingInputIds.Contains("coal")) counter[2] += blocked;
                    if (production.BlockReason.HasFlag(ProductionBlockReason.MissingInputs)) counter[3] += blocked;
                    if (production.BlockReason.HasFlag(ProductionBlockReason.StorageFull)) counter[4] += blocked;
                }
                if (production.RunnableWorkers == 0) continue;
                var recipe = scenario.Catalog.BuildingsById[production.BuildingId].Recipe!;
                inputs += recipe.Input.Scale(production.RunnableWorkers);
                outputs += recipe.Output.Scale(production.RunnableWorkers);
            }
            peakWorkers = Math.Max(peakWorkers, assigned);
            coalProduced += outputs["coal"].Units;
            coalConsumed += inputs["coal"].Units;
            var before = game.Resources;
            if (split)
            {
                Require(game.Advance(TimeSpan.FromMilliseconds(250)).Succeeded && game.Resources.Equals(before),
                    $"{setup.Id}: partial second changed resources.");
                if (second == setup.Seconds / 2)
                {
                    game.Pause();
                    game = GameSession.Restore(scenario, game.Snapshot());
                    Require(game.IsPaused, $"{setup.Id}: restore must pause.");
                    game.Resume();
                }
                Require(game.Advance(TimeSpan.FromMilliseconds(750)).Succeeded, $"{setup.Id}: fractional advance failed.");
            }
            else Require(game.Advance(TimeSpan.FromSeconds(1)).Succeeded, $"{setup.Id}: advance failed.");
            Require(game.Resources.Equals(before - consumption - inputs + outputs) && game.StorageCapacity.Covers(game.Resources),
                $"{setup.Id}: resource ledger or capacity diverged at second {second + 1}.");
            Require(game.Population == 40 && game.GrowthProgressSeconds == 0 && game.GrowthRemainderTwentieths == 0,
                $"{setup.Id}: population changed.");
            if (firstSwords is null && game.Resources["swords"].Units > 0) firstSwords = second + 1;
            if (firstCoins is null && game.Resources["coins"].Units > 0) firstCoins = second + 1;
            if (target is null && game.Resources["swords"].Units >= 240m && game.Resources["coins"].Units >= 360m) target = second + 1;
        }
        Require(coalProduced - coalConsumed == game.Resources["coal"].Units &&
            metrics["forge"][1] * 2m == game.Resources["swords"].Units && metrics["mint"][1] * 3m == game.Resources["coins"].Units,
            $"{setup.Id}: accumulated production ledger differs.");
        game.Pause();
        return (new(setup.Id, setup.Mode, setup.LiveMining, setup.CoalMines, setup.Seconds, peakWorkers, planksSpent, stoneSpent,
            firstSwords, firstCoins, target, game.Resources["swords"].Units, game.Resources["coins"].Units,
            game.Resources["iron"].Units, game.Resources["gold"].Units, coalProduced, coalConsumed, game.Resources["coal"].Units,
            game.Resources["food"].Units, game.Resources["bread"].Units, setup.Seconds,
            IndustryIds.Select(id => new ProductionMetrics(id, metrics[id][0], metrics[id][1], metrics[id][2], metrics[id][3], metrics[id][4])).ToArray()),
            JsonSerializer.Serialize(game.Snapshot()));
    }

    private static (Guard Result, string Snapshot) RunGuard(string id, bool split)
    {
        var priority = id == "coal-priority";
        var exact = id == "exact-fit";
        var swords = priority ? 0m : exact ? 998m : 999m;
        var coins = priority ? 0m : exact ? 997m : id == "one-slot" ? 999m : 998m;
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot();
        var game = GameSession.Restore(scenario, initial with
        {
            Resources = ResourceBundle.Create(("food", 30m), ("coal", 2m), ("iron", 1m), ("gold", 1m),
                ("iron-ore", 2m), ("gold-ore", 1m), ("swords", swords), ("coins", coins)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId is "forge" or "mint" ||
                priority && item.BuildingId is "iron-smelter" or "gold-smelter"
                    ? item with { Count = 1, AssignedWorkers = 1 } : item)]
        });
        var forecast = game.ForecastProduction();
        foreach (var buildingId in new[] { "forge", "mint" })
        {
            var production = forecast.Single(item => item.BuildingId == buildingId);
            Require(production.RunnableWorkers == (exact ? 1 : 0), $"{id}: invalid runnable batch.");
            if (!exact) Require(production.BlockReason.HasFlag(priority ? ProductionBlockReason.MissingInputs : ProductionBlockReason.StorageFull),
                $"{id}: missing block diagnosis.");
        }
        game.Resume();
        if (split)
        {
            Require(game.Advance(TimeSpan.FromMilliseconds(250)).Succeeded, $"{id}: partial advance failed.");
            game = GameSession.Restore(scenario, game.Snapshot());
            game.Resume();
            Require(game.Advance(TimeSpan.FromMilliseconds(750)).Succeeded, $"{id}: fractional advance failed.");
        }
        else Require(game.Advance(TimeSpan.FromSeconds(1)).Succeeded, $"{id}: advance failed.");
        Require(game.Resources["swords"].Units == swords + (exact ? 2m : 0m) &&
            game.Resources["coins"].Units == coins + (exact ? 3m : 0m) &&
            game.Resources["coal"].Units == (exact || priority ? 0m : 2m) &&
            game.Resources["iron"].Units == (exact ? 0m : priority ? 2m : 1m) &&
            game.Resources["gold"].Units == (exact ? 0m : priority ? 2m : 1m), $"{id}: inputs or whole batches diverged.");
        game.Pause();
        return (new(id, game.Resources["swords"].Units, game.Resources["coins"].Units,
            game.Resources["coal"].Units, game.Resources["iron"].Units, game.Resources["gold"].Units), JsonSerializer.Serialize(game.Snapshot()));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
