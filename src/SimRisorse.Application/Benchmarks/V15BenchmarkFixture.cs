using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Benchmarks;

/// <summary>Same v15 fresh-game rules as the existing E4a natural benchmark.</summary>
public sealed record V15BenchmarkFixture(GameScenario Baseline, SchoolResearchRules School,
    TechnologyCatalog Technologies, FoodConversionPolicy Depot, MeatChainPolicy Meat,
    HorticulturePolicy Crops, V15FoodCampaignCheckpoint Start)
{
    public static V15BenchmarkFixture Create()
    {
        static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", U(10m), 120, 1, 2, 100, pencilsPerStudent: 2);
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
        var start = V15FoodCampaignCheckpoints.NewCampaign(
            baseline, school, technologies, depot);
        return new(baseline, school, technologies, depot, meat, crops, start);
    }
}
