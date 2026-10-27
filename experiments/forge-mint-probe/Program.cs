using System.Text.Json;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

if (args is ["--help"])
{
    Console.WriteLine("ForgeMintProbe: isolated M5.2 A/B comparison; no arguments runs six profiles and six boundary checks as JSON.");
    Console.WriteLine("Does not load or save games, or change active civil-growth rules.");
    return 0;
}
if (args.Length != 0)
{
    Console.Error.WriteLine("Use no arguments for the comparison, or --help.");
    return 2;
}
try
{
    Console.WriteLine(JsonSerializer.Serialize(ForgeMintProbe.Run(), new JsonSerializerOptions { WriteIndented = true }));
    return 0;
}
catch (InvalidOperationException error)
{
    Console.Error.WriteLine(error.Message);
    return 1;
}

/// <summary>
/// An isolated, unapproved M5.2 hypothesis. Does not change the active game catalog,
/// register new rules versions, migrate saves, or write to the user's game directory.
/// </summary>
internal static class ForgeMintProbe
{
    private const int StartingPopulation = 40;
    private const int TargetSwords = 240;
    private const int TargetCoins = 360;

    private sealed record Setup(string Mode, int CoalMines, int Seconds, int? ExpectedFirstOutput,
        int? ExpectedTarget, decimal Swords, decimal Coins, decimal Coal, decimal Iron, decimal Gold,
        decimal CoalProduced, decimal CoalConsumed);

    internal sealed record Profile(string Pricing, string Mode, int CoalMines, int AssignedWorkers,
        int SecondsSimulated, int? FirstSwordsSecond, int? FirstCoinsSecond, int? TargetSecond,
        decimal PlanksSpent, decimal StoneSpent, decimal Swords, decimal Coins, decimal Iron, decimal Gold,
        decimal CoalProduced, decimal CoalConsumed, decimal CoalRemaining, int ReconciledSeconds);

    internal sealed record GuardResult(string Pricing, string Case, int ForgeRunnable, int MintRunnable,
        string ForgeBlockReason, string MintBlockReason, decimal Swords, decimal Coins, decimal Coal);

    internal sealed record Report(string State, string Basis, string Scenario, string[] Restrictions,
        string[] ReplayPatterns, Profile[] Profiles, GuardResult[] Guards);

    internal static Report Run()
    {
        Setup[] setups =
        [
            new("staged", 1, 241, 122, 241, 240, 360, 2, 0, 0, 482, 480),
            new("parallel", 2, 122, 3, 122, 240, 360, 6, 1, 1, 488, 482),
            new("always-staffed", 1, 180, null, null, 0, 0, 2, 179, 179, 360, 358)
        ];
        var results = new List<Profile>();
        var guards = new List<GuardResult>();
        foreach (var pricing in new[] { "A", "B" })
        {
            foreach (var setup in setups)
            {
                var full = RunProfile(pricing, setup, fractional: false);
                var fractions = RunProfile(pricing, setup, fractional: true);
                Check(JsonSerializer.Serialize(full.Result) == JsonSerializer.Serialize(fractions.Result) &&
                    full.Snapshot == fractions.Snapshot,
                    $"{pricing}/{setup.Mode}: fractional replay differs from the whole-second replay.");

                var result = full.Result;
                var expectedPlanks = (pricing == "A" ? 1300m : 1525m) + (setup.CoalMines - 1) * 130m;
                var expectedStone = (pricing == "A" ? 950m : 1120m) + (setup.CoalMines - 1) * 90m;
                Check(result.PlanksSpent == expectedPlanks && result.StoneSpent == expectedStone &&
                    result.Swords == setup.Swords && result.Coins == setup.Coins &&
                    result.CoalRemaining == setup.Coal && result.Iron == setup.Iron && result.Gold == setup.Gold &&
                    result.CoalProduced == setup.CoalProduced && result.CoalConsumed == setup.CoalConsumed &&
                    result.FirstSwordsSecond == setup.ExpectedFirstOutput &&
                    result.FirstCoinsSecond == setup.ExpectedFirstOutput &&
                    result.TargetSecond == setup.ExpectedTarget && result.ReconciledSeconds == setup.Seconds &&
                    result.CoalProduced - result.CoalConsumed == result.CoalRemaining,
                    $"{pricing}/{setup.Mode}: the theoretical cost, output or coal accounting changed.");
                results.Add(result);
            }
            foreach (var guardCase in new[] { "storage-near-full", "storage-exact-fit", "coal-priority" })
            {
                var full = RunGuard(pricing, guardCase, fractional: false);
                var fractions = RunGuard(pricing, guardCase, fractional: true);
                Check(JsonSerializer.Serialize(full.Result) == JsonSerializer.Serialize(fractions.Result) &&
                    full.Snapshot == fractions.Snapshot,
                    $"{pricing}/{guardCase}: boundary-check replay differs between whole and fractional seconds.");
                guards.Add(full.Result);
            }
        }

        return new("UNAPPROVED_EXPERIMENT", "M5.2 hypothetical A/B prices and recipes on the current civil-growth-7 core",
            "forge-mint-probe",
            ["40 residents and 40 housing capacity; ore preloaded and ore mines unstaffed",
             "All required buildings prepaid, 1000 food/bread and two warehouses; not a new-game strategy",
             "Forge priority 100 and mint 101 are unapproved; real content and save formats unchanged"],
            ["whole 1s", "250ms + 750ms per second, identical result and final snapshot"],
            results.ToArray(), guards.ToArray());
    }

    private static (GuardResult Result, string Snapshot) RunGuard(string pricing, string guardCase, bool fractional)
    {
        ResourceBundle extraStocks = guardCase switch
        {
            "storage-near-full" => ResourceBundle.Create(("coal", 2m), ("iron", 1m), ("gold", 1m),
                ("swords", 1999m), ("coins", 1998m)),
            "storage-exact-fit" => ResourceBundle.Create(("coal", 2m), ("iron", 1m), ("gold", 1m),
                ("swords", 1998m), ("coins", 1997m)),
            "coal-priority" => ResourceBundle.Create(("coal", 2m), ("iron", 1m), ("gold", 1m)),
            _ => throw new InvalidOperationException($"Unknown boundary check: {guardCase}.")
        };
        var game = GameSession.Create(CreateHypotheticalScenario(pricing, extraStocks));
        foreach (var buildingId in new[] { "iron-mine", "gold-mine", "coal-mine", "iron-smelter", "gold-smelter", "forge", "mint" })
            Check(game.Construct(buildingId, 1).Succeeded,
                $"{pricing}/{guardCase}: cannot buy the hypothetical {buildingId}.");
        Check(game.AssignWorkers("forge", 1).Succeeded && game.AssignWorkers("mint", 1).Succeeded,
            $"{pricing}/{guardCase}: cannot staff the two transformers.");
        if (guardCase == "coal-priority")
            Check(game.AssignWorkers("coal-mine", 1).Succeeded &&
                game.AssignWorkers("iron-smelter", 1).Succeeded &&
                game.AssignWorkers("gold-smelter", 1).Succeeded,
                $"{pricing}/{guardCase}: cannot staff the coal mine and smelters.");
        Check(game.StorageCapacity["swords"].Units == 2000m && game.StorageCapacity["coins"].Units == 2000m,
            "The controlled capacity must be 1000 base + two warehouses of 500 for each new resource.");

        game.Resume();
        var forecast = game.ForecastEconomy();
        var forge = forecast.Buildings.Single(item => item.BuildingId == "forge");
        var mint = forecast.Buildings.Single(item => item.BuildingId == "mint");
        if (guardCase == "storage-near-full")
        {
            Check(forge.RunnableWorkers == 0 && mint.RunnableWorkers == 0 &&
                forge.BlockReason.HasFlag(ProductionBlockReason.StorageFull) &&
                mint.BlockReason.HasFlag(ProductionBlockReason.StorageFull) &&
                forge.FullOutputIds.Contains("swords") && mint.FullOutputIds.Contains("coins"),
                $"{pricing}: an incomplete output batch was permitted near a full warehouse.");
        }
        else if (guardCase == "storage-exact-fit")
        {
            Check(forge.RunnableWorkers == 1 && mint.RunnableWorkers == 1 &&
                forge.BlockReason == ProductionBlockReason.None && mint.BlockReason == ProductionBlockReason.None,
                $"{pricing}: exact-fitting complete output batches should be runnable.");
        }
        else
        {
            var ironSmelter = forecast.Buildings.Single(item => item.BuildingId == "iron-smelter");
            var goldSmelter = forecast.Buildings.Single(item => item.BuildingId == "gold-smelter");
            Check(ironSmelter.RunnableWorkers == 1 && goldSmelter.RunnableWorkers == 1 &&
                forge.RunnableWorkers == 0 && mint.RunnableWorkers == 0 &&
                forge.MissingInputIds.Contains("coal") && mint.MissingInputIds.Contains("coal") &&
                forge.BlockReason.HasFlag(ProductionBlockReason.MissingInputs) &&
                mint.BlockReason.HasFlag(ProductionBlockReason.MissingInputs),
                $"{pricing}: smelters did not take priority over forge and mint for scarce coal.");
        }

        var before = game.Resources;
        if (fractional)
        {
            Check(game.Advance(TimeSpan.FromMilliseconds(250)).Succeeded && game.Resources.Equals(before),
                $"{pricing}/{guardCase}: a fractional second changed the stocks.");
            Check(game.Advance(TimeSpan.FromMilliseconds(750)).Succeeded,
                $"{pricing}/{guardCase}: the fractional step did not finish.");
        }
        else Check(game.Advance(TimeSpan.FromSeconds(1)).Succeeded,
            $"{pricing}/{guardCase}: the one-second advance failed.");
        if (guardCase == "storage-near-full")
            Check(game.Resources["swords"].Units == 1999m && game.Resources["coins"].Units == 1998m &&
                game.Resources["coal"].Units == 2m && game.Resources["iron"].Units == 1m &&
                game.Resources["gold"].Units == 1m,
                $"{pricing}: a blocked producer consumed inputs or created a partial batch.");
        else if (guardCase == "storage-exact-fit")
            Check(game.Resources["swords"].Units == 2000m && game.Resources["coins"].Units == 2000m &&
                game.Resources["coal"].Units == 0m && game.Resources["iron"].Units == 0m &&
                game.Resources["gold"].Units == 0m,
                $"{pricing}: a complete batch did not fill the warehouse exactly.");
        else
            Check(game.Resources["swords"].Units == 0m && game.Resources["coins"].Units == 0m &&
                game.Resources["coal"].Units == 2m && game.Resources["iron"].Units == 2m &&
                game.Resources["gold"].Units == 2m,
                $"{pricing}: coal was allocated to the transformers instead of the smelters.");
        Check(game.StorageCapacity.Covers(game.Resources) && game.Population == StartingPopulation &&
            game.HousingCapacity == StartingPopulation && game.GrowthProgressSeconds == 0 &&
            game.GrowthRemainderTwentieths == 0,
            $"{pricing}/{guardCase}: housing, growth or storage diverged from the control scenario.");
        game.Pause();
        return (new GuardResult(pricing, guardCase, forge.RunnableWorkers, mint.RunnableWorkers,
            forge.BlockReason.ToString(), mint.BlockReason.ToString(),
            game.Resources["swords"].Units, game.Resources["coins"].Units, game.Resources["coal"].Units),
            JsonSerializer.Serialize(game.Snapshot()));
    }

    private static (Profile Result, string Snapshot) RunProfile(string pricing, Setup setup, bool fractional)
    {
        var scenario = CreateHypotheticalScenario(pricing);
        var game = GameSession.Create(scenario);
        decimal planksSpent = 0m, stoneSpent = 0m;

        void Buy(string id, int count, int workers)
        {
            Check(game.Construct(id, count).Succeeded, $"{pricing}/{setup.Mode}: cannot buy {count} {id}.");
            var cost = scenario.Catalog.BuildingsById[id].Cost.Scale(count);
            planksSpent += cost["planks"].Units;
            stoneSpent += cost["stone"].Units;
            if (workers > 0)
                Check(game.AssignWorkers(id, workers).Succeeded,
                    $"{pricing}/{setup.Mode}: cannot assign {workers} workers to {id}.");
        }

        // Ore deposits exist solely as prerequisites: seeded stocks isolate coal allocation.
        Buy("iron-mine", 1, 0);
        Buy("gold-mine", 1, 0);
        Buy("coal-mine", setup.CoalMines, setup.CoalMines);
        Buy("iron-smelter", 1, 1);
        Buy("gold-smelter", 1, 1);
        var immediatelyStaffed = setup.Mode != "staged";
        Buy("forge", 1, immediatelyStaffed ? 1 : 0);
        Buy("mint", 1, immediatelyStaffed ? 1 : 0);
        var purchased = game.Resources;
        Check(purchased["planks"].Units + planksSpent == 1700m &&
            purchased["stone"].Units + stoneSpent == 1700m,
            "The real purchase costs were not reconciled.");

        int? firstSwords = null, firstCoins = null, reached = null;
        decimal producedCoal = 0m, consumedCoal = 0m;
        var reconciled = 0;
        game.Resume();

        for (var second = 0; second < setup.Seconds; second++)
        {
            if (setup.Mode == "staged" && second == 121)
            {
                Check(game.ReleaseWorkers("iron-smelter", 1).Succeeded &&
                    game.ReleaseWorkers("gold-smelter", 1).Succeeded &&
                    game.AssignWorkers("forge", 1).Succeeded &&
                    game.AssignWorkers("mint", 1).Succeeded,
                    "Cannot transfer the two smelter workers to forge and mint.");
            }

            var forecast = game.ForecastEconomy();
            Check(forecast.Population.BlockReason == GrowthBlockReason.HousingFull,
                $"{pricing}/{setup.Mode}: housing or food constraints altered the controlled profile.");
            var consumption = ResourceBundle.Empty;
            if (forecast.Population.FoodResourceId is { } food)
                consumption += ResourceBundle.FromAmounts([KeyValuePair.Create(food, forecast.Population.ConsumedFood)]);
            if (forecast.Population.BreadResourceId is { } bread)
                consumption += ResourceBundle.FromAmounts([KeyValuePair.Create(bread, forecast.Population.ConsumedBread)]);

            var inputs = ResourceBundle.Empty;
            var outputs = ResourceBundle.Empty;
            foreach (var building in forecast.Buildings)
            {
                if (building.RunnableWorkers == 0) continue;
                var recipe = scenario.Catalog.BuildingsById[building.BuildingId].Recipe!;
                inputs += recipe.Input.Scale(building.RunnableWorkers);
                outputs += recipe.Output.Scale(building.RunnableWorkers);
            }
            var coalMine = forecast.Buildings.Single(item => item.BuildingId == "coal-mine");
            var ironSmelter = forecast.Buildings.Single(item => item.BuildingId == "iron-smelter");
            var goldSmelter = forecast.Buildings.Single(item => item.BuildingId == "gold-smelter");
            var forge = forecast.Buildings.Single(item => item.BuildingId == "forge");
            var mint = forecast.Buildings.Single(item => item.BuildingId == "mint");
            producedCoal += coalMine.RunnableWorkers * 2m;
            consumedCoal += ironSmelter.RunnableWorkers + goldSmelter.RunnableWorkers +
                forge.RunnableWorkers + mint.RunnableWorkers;

            var before = game.Resources;
            if (fractional)
            {
                Check(game.Advance(TimeSpan.FromMilliseconds(250)).Succeeded,
                    "First fractional advance failed.");
                Check(game.Resources.Equals(before), "A fractional second produced resources prematurely.");
                Check(game.Advance(TimeSpan.FromMilliseconds(750)).Succeeded,
                    "Second fractional advance failed.");
            }
            else Check(game.Advance(TimeSpan.FromSeconds(1)).Succeeded, "Whole-second advance failed.");

            Check(game.Resources.Equals(before - consumption - inputs + outputs),
                $"{pricing}/{setup.Mode}: production ledger diverged at second {second + 1}.");
            Check(game.StorageCapacity.Covers(game.Resources) && game.Population == StartingPopulation &&
                game.HousingCapacity == StartingPopulation && game.GrowthProgressSeconds == 0 &&
                game.GrowthRemainderTwentieths == 0, "Housing, growth or storage changed unexpectedly.");
            if (firstSwords is null && game.Resources["swords"].Units > 0) firstSwords = second + 1;
            if (firstCoins is null && game.Resources["coins"].Units > 0) firstCoins = second + 1;
            if (reached is null && game.Resources["swords"].Units >= TargetSwords &&
                game.Resources["coins"].Units >= TargetCoins) reached = second + 1;
            reconciled++;
        }

        game.Pause();
        var result = new Profile(pricing, setup.Mode, setup.CoalMines, setup.CoalMines + 2 +
            (setup.Mode == "staged" ? 0 : 2), setup.Seconds,
            firstSwords, firstCoins, reached, planksSpent, stoneSpent,
            game.Resources["swords"].Units, game.Resources["coins"].Units,
            game.Resources["iron"].Units, game.Resources["gold"].Units,
            producedCoal, consumedCoal, game.Resources["coal"].Units, reconciled);
        return (result, JsonSerializer.Serialize(game.Snapshot()));
    }

    private static GameScenario CreateHypotheticalScenario(string pricing, ResourceBundle? extraStocks = null)
    {
        var original = CivilGrowthScenario.Create();
        var warehouseExtras = ResourceBundle.Create(("swords", 500m), ("coins", 500m));
        ResourceDefinition[] hypotheticalResources = [new("swords", "Spade"), new("coins", "Monete")];
        var resources = original.Catalog.Resources.Concat(hypotheticalResources);
        var forgeCost = pricing == "A" ? (240m, 180m) : (360m, 270m);
        var mintCost = pricing == "A" ? (210m, 160m) : (315m, 240m);
        BuildingDefinition[] hypotheticalBuildings =
        [
            new("forge", "Forgia (ipotesi)",
                ResourceBundle.Create(("planks", forgeCost.Item1), ("stone", forgeCost.Item2)), 1,
                new(ResourceBundle.Create(("iron", 1m), ("coal", 1m)), ResourceBundle.Create(("swords", 2m))),
                100, ["iron-smelter"]),
            new("mint", "Zecca (ipotesi)",
                ResourceBundle.Create(("planks", mintCost.Item1), ("stone", mintCost.Item2)), 1,
                new(ResourceBundle.Create(("gold", 1m), ("coal", 1m)), ResourceBundle.Create(("coins", 3m))),
                101, ["gold-smelter"])
        ];
        var buildings = original.Catalog.Buildings.Select(item => item.Id == "warehouse"
                ? item with { StoragePerBuilding = item.StoragePerBuilding! + warehouseExtras }
                : item)
            .Concat(hypotheticalBuildings);
        var catalog = new GameCatalog("forge-mint-hypothesis-" + pricing.ToLowerInvariant(), resources, buildings);
        var initial = catalog.Buildings.Select(item => new BuildingState(item.Id, item.Id switch
        {
            "small-house" or "large-house" or "bakery" => 1,
            "warehouse" => 2,
            _ => 0
        }, 0));
        return new GameScenario("forge-mint-probe", catalog, StartingPopulation,
            original.Capacity + ResourceBundle.Create(("swords", 1000m), ("coins", 1000m)),
            ResourceBundle.Create(("planks", 1700m), ("stone", 1700m), ("food", 1000m),
                ("bread", 1000m), ("iron-ore", 1000m), ("gold-ore", 1000m)) +
                (extraStocks ?? ResourceBundle.Empty),
            initial, original.PopulationRules);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
