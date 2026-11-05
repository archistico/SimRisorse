using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Benchmarks;

/// <summary>
/// F1-D05e.2a: deterministic, explicitly funded TEST scenario for exercising
/// all six v15 food sites and the existing fish/grain/water/flour/bread chains.
/// It is never offered as a normal player start and never reads a personal save.
/// It is NOT a natural progression or an E4a balancing benchmark.
/// </summary>
public sealed record V15CombinedFoodStressScenario(
    GameScenario Baseline, SchoolResearchRules School, TechnologyCatalog Technologies,
    FoodIndustryConstructionCatalog FoodCatalog, MeatChainPolicy Meat,
    HorticulturePolicy Crops, FoodConversionPolicy Depot,
    V15FoodCampaignCheckpoint Initial)
{
    public V15CampaignAdvancePlan Plan(V15FoodCampaignCheckpoint current, TimeSpan elapsed) =>
        V15UnifiedCampaignClock.Plan(current, Baseline, School, Technologies,
            FoodCatalog, Meat, Crops, Depot, elapsed);

    private static ResourceAmount U(decimal value) => ResourceAmount.FromUnits(value);

    public static V15CombinedFoodStressScenario Create()
    {
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", U(10m), 120, 1, 2, 100);
        var technologies = TechnologyCatalog.ProposedTierTree;
        var meat = new MeatChainPolicy("grain", "water", "cattle", "game", "meat",
            ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(1m), U(1m),
            [new("cattle", U(1m), U(2m), 0), new("game", U(1m), U(1m), 1)]);
        var crops = new HorticulturePolicy("water", "vegetables", "fruit",
            new("vegetables", U(2m), U(1m), 0),
            new("fruit", U(3m), U(1m), 1));
        var depot = UnifiedFoodInputs.CreatePolicy([
            new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
            new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
            new("bread", U(1m), U(1m), 4)
        ]);
        var city = V14CampaignSnapshots.MigrateFromV13(baseline,
            GameSession.Create(baseline).Snapshot(), school, technologies);
        // 3 houses x 8 places = 24 residents. The scenario starts with
        // 3 foundation workers (tree-feller, sawmill, stonecutter) and adds
        // 13 food-chain workers (fish 1, grain 3, water 7, flour 1, bread 1).
        // Six v15 food-industry workers bring the employed total to 22, leaving 2.
        var historicalStaff = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["fisher"] = 1, ["farm"] = 3, ["well"] = 7,
            ["mill"] = 1, ["bakery"] = 1
        };
        city = city with
        {
            Population = 24,
            OtherBuildings = [.. city.OtherBuildings.Select(b => b.BuildingId == "small-house"
                ? b with { Count = 3 } : b)],
            ProductiveFamilies = [.. city.ProductiveFamilies.Select(f =>
                historicalStaff.TryGetValue(f.FamilyId, out var workers)
                    ? f with { Base = new TierOccupancy(workers, workers) } : f)],
            PurchaseCounters = [.. city.PurchaseCounters.Select(c =>
                c.FamilyId == "small-house" ? c with { Acquired = 3 } :
                historicalStaff.TryGetValue(c.FamilyId, out var workers)
                    ? c with { Acquired = workers } : c)],
            // Buffers permit the test to exercise several stages while water
            // and grain production are being scheduled by the inherited clock.
            Resources = ResourceBundle.Create(("grain", 50m), ("water", 50m), ("flour", 10m))
        };
        var checkpoint = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, depot), depot);
        checkpoint = checkpoint with
        {
            Depot = new FoodDepotStock(ResourceBundle.Empty, ResourceAmount.Zero),
            Industry = checkpoint.Industry with
            {
                Buildings = new FoodIndustryBuildings(
                    new FoodIndustryCrew(1, 1, 1, 1, 1, 1),
                    new FoodIndustryCrew(1, 1, 1, 1, 1, 1))
            }
        };
        V15FoodCampaignCheckpoints.Validate(checkpoint, baseline, school, technologies, depot);
        return new(baseline, school, technologies, FoodIndustryConstructionCatalog.Candidate,
            meat, crops, depot, checkpoint);
    }
}
