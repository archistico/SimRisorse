using System.Text.Json;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Simulation;

/// <summary>
/// Scripted real-start M5.2 benchmark. Unlike ForgeMintCoalBenchmark, there are
/// no seeded ores, metals, coal, food, bread, housing or warehouses: every item
/// is collected or built through the normal v9 GameSession economy.
/// The schedule through gold follows the historical civil/gold reference route.
/// </summary>
internal static class ForgeMintCampaignBenchmark
{
    private const int HorizonSeconds = 2400;
    private const int TargetSwords = 240;
    private const int TargetCoins = 360;

    private enum Strategy { PhasedOneMine, ConcurrentOneMine, ConcurrentTwoMines }

    internal sealed record BuildEvent(int AtSecond, string BuildingId, int Count, int AddedWorkers);
    internal sealed record Profile(string Id, int HorizonSeconds, int FinalPopulation,
        int PeakAssignedWorkers, decimal PlanksSpent, decimal StoneSpent,
        int? TransformersStartedSecond, int? FirstSwordsSecond, int? FirstCoinsSecond,
        int? GoalReachedSecond, decimal CoalBeforeTransformers, decimal CoalAtGoal,
        decimal FinalSwords, decimal FinalCoins, decimal FinalCoal,
        int ForgeMissingCoalWorkerSeconds, int MintMissingCoalWorkerSeconds,
        int ReconciledSeconds, BuildEvent[] Purchases);

    internal sealed record Comparison(string ScenarioId, string RulesVersion, int StartPopulation,
        int HorizonSeconds, int TargetSwords, int TargetCoins, string InitialState,
        string Route, string Caveat, string ReplayCheck, Profile[] Profiles);

    internal static Comparison Run()
    {
        var results = new List<Profile>(3);
        foreach (var strategy in new[] { Strategy.PhasedOneMine, Strategy.ConcurrentOneMine, Strategy.ConcurrentTwoMines })
        {
            var whole = Execute(strategy, fractional: false);
            var split = Execute(strategy, fractional: true);
            Check(JsonSerializer.Serialize(whole.Profile) == JsonSerializer.Serialize(split.Profile) &&
                whole.Snapshot == split.Snapshot,
                $"{strategy}: whole/fractional replay changed the completed colony.");
            Check(whole.Profile.GoalReachedSecond is > 0 and <= HorizonSeconds,
                $"{strategy}: the civil-to-industrial route cannot reach {TargetSwords} swords and {TargetCoins} coins.");
            results.Add(whole.Profile);
        }

        var first = results.Single(item => item.Id == "phased-one-mine");
        var second = results.Single(item => item.Id == "concurrent-one-mine");
        var third = results.Single(item => item.Id == "concurrent-two-mines");
        Check(third.PlanksSpent - first.PlanksSpent == 130m &&
            third.StoneSpent - first.StoneSpent == 90m &&
            second.PlanksSpent == first.PlanksSpent && second.StoneSpent == first.StoneSpent,
            "The campaign purchase-cost comparison is not balanced.");
        var scenario = CivilGrowthScenario.CreateMintForBenchmarks();
        return new(scenario.Id, scenario.Catalog.RulesVersion, scenario.Population,
            HorizonSeconds, TargetSwords, TargetCoins,
            "An untouched GameSession.Create(civil-growth-9): 8 people, 120 planks, 80 stone, 30 food, " +
            "the three initial staffed logging/stone buildings and one small house. " +
            "All industrial resources initially zero, no pre-built warehouse or advanced infrastructure.",
            "At 0 s build fish/food; 61 s a small house; 600 s grain, water, flour and bread; " +
            "760 s a large house; 1100 s iron/coal; 1500 s gold. At or after 2000 s " +
            "buy forge/mint when affordable; the two-mine profile first buys an extra coal mine. " +
            "The phased profile reassigns both smelter workers at transformer start.",
            "This is one scripted reference opening, not an automated player or proof of the best " +
            "strategy. Purchase time and previously accumulated coal affect the comparison. " +
            "No soldier, ship, colony, offline production or save-file mutation is introduced.",
            "Each strategy has a second-by-second resource ledger and an identical final snapshot " +
            "under 1-second versus 250+750 ms advances, with a partial-second snapshot restore.",
            results.ToArray());
    }

    private static (Profile Profile, string Snapshot) Execute(Strategy strategy, bool fractional)
    {
        var scenario = CivilGrowthScenario.CreateMintForBenchmarks();
        Check(scenario.Catalog.RulesVersion == "civil-growth-9", "Expected the active v9 rules.");
        var game = GameSession.Create(scenario);
        var events = new List<BuildEvent>();
        decimal planksSpent = 0m, stoneSpent = 0m;
        int peakWorkers = scenario.Population - game.AvailableWorkers;

        void Buy(string id, int count = 1, int staff = 0)
        {
            Check(game.Construct(id, count).Succeeded, $"{strategy}: unable to buy {count} x {id} at " +
                $"{game.ElapsedTicks / TimeSpan.TicksPerSecond}s.");
            if (staff > 0)
                Check(game.AssignWorkers(id, staff).Succeeded, $"{strategy}: unable to staff {id}.");
            var cost = scenario.Catalog.BuildingsById[id].Cost.Scale(count);
            planksSpent += cost["planks"].Units;
            stoneSpent += cost["stone"].Units;
            events.Add(new((int)(game.ElapsedTicks / TimeSpan.TicksPerSecond), id, count, staff));
            peakWorkers = Math.Max(peakWorkers, game.Population - game.AvailableWorkers);
        }

        Buy("fisher", staff: 1);
        Buy("food-store", staff: 1);
        var transformerStarted = (int?)null;
        int? firstSwords = null, firstCoins = null, goalSecond = null;
        var coalBefore = 0m;
        decimal? coalAtGoal = null;
        int forgeCoalBlocked = 0, mintCoalBlocked = 0;
        var secondCoalPurchased = false;
        game.Resume();

        for (var second = 1; second <= HorizonSeconds; second++)
        {
            // Actions happen at the elapsed-second boundary, before the next tick.
            switch (second - 1)
            {
                case 61:
                    Buy("small-house");
                    break;
                case 600:
                    Buy("farm", 2, staff: 2);
                    Buy("well", staff: 1);
                    Buy("mill", staff: 1);
                    Buy("bakery", staff: 1);
                    break;
                case 760:
                    Buy("large-house");
                    break;
                case 1100:
                    Buy("iron-mine", staff: 1);
                    Buy("coal-mine", staff: 1);
                    Buy("iron-smelter", staff: 1);
                    break;
                case 1500:
                    Buy("gold-mine", staff: 1);
                    Buy("gold-smelter", staff: 1);
                    break;
            }

            if (second > 2000 && transformerStarted is null)
            {
                if (strategy == Strategy.ConcurrentTwoMines && !secondCoalPurchased &&
                    game.Resources.Covers(scenario.Catalog.BuildingsById["coal-mine"].Cost))
                {
                    Buy("coal-mine", staff: 1);
                    secondCoalPurchased = true;
                }

                if (strategy != Strategy.ConcurrentTwoMines || secondCoalPurchased)
                {
                    var transformersCost = scenario.Catalog.BuildingsById["forge"].Cost +
                        scenario.Catalog.BuildingsById["mint"].Cost;
                    if (game.Resources.Covers(transformersCost))
                    {
                        Buy("forge");
                        Buy("mint");
                        if (strategy == Strategy.PhasedOneMine)
                        {
                            Check(game.ReleaseWorkers("iron-smelter", 1).Succeeded &&
                                game.ReleaseWorkers("gold-smelter", 1).Succeeded,
                                "The phased route could not release the smelters.");
                        }
                        Check(game.AssignWorkers("forge", 1).Succeeded &&
                            game.AssignWorkers("mint", 1).Succeeded,
                            "The colony cannot staff both transformers.");
                        peakWorkers = Math.Max(peakWorkers, game.Population - game.AvailableWorkers);
                        transformerStarted = second - 1;
                        coalBefore = game.Resources["coal"].Units;
                    }
                }
            }

            var forecast = game.ForecastEconomy();
            Check(forecast.Population.BlockReason is not (GrowthBlockReason.MissingFood or GrowthBlockReason.MissingBread),
                $"{strategy}: the reference colony cannot feed its residents at second {second}.");
            var expected = game.Resources;
            if (forecast.Population.FoodResourceId is { } food)
                expected -= ResourceBundle.Create((food, forecast.Population.ConsumedFood.Units));
            if (forecast.Population.BreadResourceId is { } bread)
                expected -= ResourceBundle.Create((bread, forecast.Population.ConsumedBread.Units));
            foreach (var production in forecast.Buildings)
            {
                if (production.RunnableWorkers > 0)
                {
                    var recipe = scenario.Catalog.BuildingsById[production.BuildingId].Recipe!;
                    expected = expected - recipe.Input.Scale(production.RunnableWorkers) +
                        recipe.Output.Scale(production.RunnableWorkers);
                }
                if (production.MissingInputIds.Contains("coal"))
                {
                    if (production.BuildingId == "forge")
                        forgeCoalBlocked += production.AssignedWorkers - production.RunnableWorkers;
                    if (production.BuildingId == "mint")
                        mintCoalBlocked += production.AssignedWorkers - production.RunnableWorkers;
                }
            }

            if (fractional)
            {
                var before = game.Resources;
                Check(game.Advance(TimeSpan.FromMilliseconds(250)).Succeeded, "Fractional step failed.");
                Check(before.Equals(game.Resources), "Resources changed before a whole second elapsed.");
                if (second is 999 or 1999)
                {
                    game = GameSession.Restore(scenario, game.Snapshot());
                    game.Resume();
                }
                Check(game.Advance(TimeSpan.FromMilliseconds(750)).Succeeded, "Fractional completion failed.");
            }
            else Check(game.Advance(TimeSpan.FromSeconds(1)).Succeeded, "Whole-second step failed.");
            Check(game.Resources.Equals(expected) && game.StorageCapacity.Covers(game.Resources),
                $"{strategy}: economy/stock ledger differs at second {second}.");

            if (firstSwords is null && game.Resources["swords"].Units > 0) firstSwords = second;
            if (firstCoins is null && game.Resources["coins"].Units > 0) firstCoins = second;
            if (goalSecond is null && game.Resources["swords"].Units >= TargetSwords &&
                game.Resources["coins"].Units >= TargetCoins)
            {
                goalSecond = second;
                coalAtGoal = game.Resources["coal"].Units;
            }
        }

        Check(transformerStarted is not null && firstSwords is not null && firstCoins is not null &&
            coalAtGoal is not null && game.Population > scenario.Population,
            $"{strategy}: the reference opening did not reach the transformer milestone.");
        var measuredCoalAtGoal = coalAtGoal ?? throw new InvalidOperationException(
            $"{strategy}: the coal stock at the goal was not recorded.");
        game.Pause();
        var profile = new Profile(strategy switch
            {
                Strategy.PhasedOneMine => "phased-one-mine",
                Strategy.ConcurrentOneMine => "concurrent-one-mine",
                _ => "concurrent-two-mines"
            }, HorizonSeconds, game.Population, peakWorkers, planksSpent, stoneSpent,
            transformerStarted, firstSwords, firstCoins, goalSecond, coalBefore,
            measuredCoalAtGoal, game.Resources["swords"].Units, game.Resources["coins"].Units,
            game.Resources["coal"].Units, forgeCoalBlocked, mintCoalBlocked,
            HorizonSeconds, events.ToArray());
        return (profile, JsonSerializer.Serialize(game.Snapshot()));
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
