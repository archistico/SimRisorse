using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Food;

namespace SimRisorse.Simulation;

/// <summary>Extra metrics for the fully staffed, mixed-chain v15 stress profile.
/// Counts gross output, not current inventory: intermediates may be processed
/// or reach their physical capacity during a long simulation.</summary>
public sealed record V15CombinedStressAuditReport(
    int RequestedSeconds, long SimulatedSeconds, long ServedMeals, long MissedMeals,
    int FinalPopulation, long LegacyProducedFoodMilliunits, int ImbalancedSeconds,
    long CattleProducedMilliunits, long GameProducedMilliunits, long MeatProducedMilliunits,
    long VegetablesProducedMilliunits, long FruitProducedMilliunits,
    long BreadProducedMilliunits, long PreparedFoodProducedMilliunits);

public static class V15CombinedStressAudit
{
    public static V15CombinedStressAuditReport Run(int seconds)
    {
        if (seconds < 1 || seconds > 48 * 3600)
            throw new ArgumentOutOfRangeException(nameof(seconds), "Use 1..172800 seconds (48 hours maximum).");
        var scenario = V15CombinedFoodStressScenario.Create();
        var current = scenario.Initial;
        long served = 0, missed = 0;
        long cattle = 0, game = 0, meat = 0, vegetables = 0, fruit = 0, bread = 0, food = 0;
        for (var i = 0; i < seconds; i++)
        {
            var advance = scenario.Plan(current, TimeSpan.FromSeconds(1));
            var balance = V15StockAuditor.ReconcileSecond(current, advance);
            if (!balance.Balanced)
            {
                var differences = string.Join("; ", balance.Differences.Select(d =>
                    $"{d.ResourceId}: expected {d.ExpectedMilliunits}, got {d.ActualMilliunits}"));
                throw new InvalidOperationException($"Mixed-chain physical ledger failed at second {i + 1}: {differences}");
            }
            if (advance.SecondsAdvanced != 1)
                throw new InvalidOperationException("Mixed-chain clock did not advance exactly one second.");
            var gross = advance.FirstSecondProduction;
            cattle = checked(cattle + gross["cattle"].Milliunits);
            game = checked(game + gross["game"].Milliunits);
            meat = checked(meat + gross["meat"].Milliunits);
            vegetables = checked(vegetables + gross["vegetables"].Milliunits);
            fruit = checked(fruit + gross["fruit"].Milliunits);
            bread = checked(bread + gross["bread"].Milliunits);
            food = checked(food + gross["food"].Milliunits);
            served = checked(served + advance.MealsServed);
            missed = checked(missed + advance.MealsMissed);
            current = advance.Next;
        }
        if (cattle == 0 || game == 0 || meat == 0 || vegetables == 0 || fruit == 0 ||
            bread == 0 || food == 0)
            throw new InvalidOperationException("One or more mandatory food-industry production stages never ran.");
        if (current.CityCampaign.Resources["food"].Milliunits != 0)
            throw new InvalidOperationException("Legacy city food was produced in a v15 mixed stress run.");
        return new(seconds, current.CityCampaign.ElapsedTicks / TimeSpan.TicksPerSecond,
            served, missed, current.CityCampaign.Population,
            current.CityCampaign.Resources["food"].Milliunits, 0,
            cattle, game, meat, vegetables, fruit, bread, food);
    }
}
