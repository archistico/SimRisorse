using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;

namespace SimRisorse.Core.Food;

/// <summary>
/// Actual gross production and consumption of the next complete simulated second.
/// Both are measured by the unified clock at the recipe/meal boundaries, not
/// inferred from stock changes: moving fish or bread to the depot is not a cost.
/// Rates describe the current snapshot, NOT guaranteed long-term throughput.
/// </summary>
public sealed record V15EconomyPreview(ResourceBundle Production, ResourceBundle Consumption,
    V15FoodCampaignCheckpoint NextSecond,
    IReadOnlyList<ResourceFlowContributor>? Contributors = null)
{
    public decimal NetPerSecond(string resourceId) =>
        Production[resourceId].Units - Consumption[resourceId].Units;
}

public static class V15EconomyProjector
{
    public static V15EconomyPreview Project(V15FoodCampaignCheckpoint checkpoint,
        GameScenario baseline, SchoolResearchRules school, TechnologyCatalog technologies,
        FoodIndustryConstructionCatalog foodCatalog, MeatChainPolicy meat,
        HorticulturePolicy crops, FoodConversionPolicy depot)
    {
        var plan = V15UnifiedCampaignClock.Plan(checkpoint, baseline, school,
            technologies, foodCatalog, meat, crops, depot, TimeSpan.FromSeconds(1));
        return new(plan.FirstSecondProduction, plan.FirstSecondConsumption, plan.Next,
            plan.FirstSecondContributors);
    }
}
