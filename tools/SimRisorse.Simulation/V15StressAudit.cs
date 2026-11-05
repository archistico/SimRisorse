using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Simulation;

/// <summary>
/// RC57 opt-in soak harness. It executes the REAL v15 clock, checks the
/// independent gross-flow/physical-stock ledger on every simulated second and
/// requires no personal save. Use the infrastructure tests for SQLite restarts.
/// Not part of validate.cmd: a 24h/48h soak can be expensive on older systems.
/// </summary>
public sealed record V15StressAuditReport(int RequestedSeconds, long SimulatedSeconds,
    long ServedMeals, long MissedMeals, int FinalPopulation,
    long PreparedFoodMilliunits, long FishMilliunits, long LegacyProducedFoodMilliunits,
    int ImbalancedSeconds);

public static class V15StressAudit
{
    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);

    public static V15StressAuditReport Run(int seconds)
    {
        if (seconds < 1 || seconds > 48 * 3600)
            throw new ArgumentOutOfRangeException(nameof(seconds), "Use 1..172800 seconds (48 hours maximum).");
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", U(10m), 120, 1, 2, 100);
        var technologies = TechnologyCatalog.ProposedTierTree;
        var depot = UnifiedFoodInputs.CreatePolicy([
            new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
            new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
            new("bread", U(1m), U(1m), 4)
        ]);
        var meat = new MeatChainPolicy("grain", "water", "cattle", "game", "meat",
            ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(1m), U(1m),
            [new("cattle", U(1m), U(2m), 0), new("game", U(1m), U(1m), 1)]);
        var crops = new HorticulturePolicy("water", "vegetables", "fruit",
            new("vegetables", U(2m), U(1m), 0), new("fruit", U(3m), U(1m), 1));
        var city = V14CampaignSnapshots.MigrateFromV13(baseline,
            GameSession.Create(baseline).Snapshot(), school, technologies);
        city = city with
        {
            ProductiveFamilies = [.. city.ProductiveFamilies.Select(f => f.FamilyId == "fisher"
                ? f with { Base = new TierOccupancy(1, 1) } : f)],
            PurchaseCounters = [.. city.PurchaseCounters.Select(c => c.FamilyId == "fisher"
                ? c with { Acquired = 1 } : c)]
        };
        var migrated = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, depot), depot);
        var current = migrated with
        {
            Depot = new FoodDepotStock(ResourceBundle.Empty, ResourceAmount.Zero),
            Industry = migrated.Industry with
            {
                Buildings = new FoodIndustryBuildings(
                    new FoodIndustryCrew(0, 0, 0, 0, 0, 1),
                    new FoodIndustryCrew(0, 0, 0, 0, 0, 1))
            }
        };
        long served = 0;
        long missed = 0;
        for (var i = 0; i < seconds; i++)
        {
            var plan = V15UnifiedCampaignClock.Plan(current, baseline, school,
                technologies, FoodIndustryConstructionCatalog.Candidate,
                meat, crops, depot, TimeSpan.FromSeconds(1));
            var audit = V15StockAuditor.ReconcileSecond(current, plan);
            if (!audit.Balanced)
            {
                var description = string.Join("; ", audit.Differences.Select(d =>
                    $"{d.ResourceId}: expected {d.ExpectedMilliunits}, got {d.ActualMilliunits}"));
                throw new InvalidOperationException($"Physical ledger failed at second {i + 1}: {description}");
            }
            current = plan.Next;
            served += plan.MealsServed;
            missed += plan.MealsMissed;
        }
        var totals = V15StockAuditor.PhysicalTotals(current);
        return new(seconds, current.CityCampaign.ElapsedTicks / TimeSpan.TicksPerSecond,
            served, missed, current.CityCampaign.Population, totals["food"].Milliunits,
            totals["fish"].Milliunits,
            current.CityCampaign.Resources[UnifiedFoodInputs.ReadyFood].Milliunits, 0);
    }
}
