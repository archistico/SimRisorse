using SimRisorse.Core.Content;
using SimRisorse.Core.Research;

namespace SimRisorse.Core.Food;

/// <summary>
/// A pure F1-D05d.2 integration seam: uses staffing from persisted building counts,
/// returns the next complete checkpoint without advancing the historical clock.
/// F1-D05d.3 will compose it with legacy production/population/research in one tick.
/// </summary>
public sealed record V15FoodCampaignTick(V15FoodCampaignCheckpoint Next, FoodIndustryTickResult Detail);

public static class V15FoodCampaignTicks
{
    public static V15FoodCampaignTick Plan(
        V15FoodCampaignCheckpoint current, GameScenario baseline,
        SchoolResearchRules school, TechnologyCatalog technologies,
        MeatChainPolicy meatPolicy, HorticulturePolicy horticulturePolicy,
        FoodConversionPolicy depotPolicy, FoodIndustryCapacities capacities,
        FoodIndustryMeals meals)
    {
        V15FoodCampaignCheckpoints.Validate(current, baseline, school, technologies, depotPolicy);
        var buildings = current.Industry.Buildings;
        var personnel = buildings.PlannerPersonnel(current.CityCampaign.Population,
            V15FoodCampaignCheckpoints.BusyOutsideFood(current.CityCampaign));
        var tick = FoodIndustryTickPlanner.PlanTick(meatPolicy, horticulturePolicy, depotPolicy,
            current.Industry.ToStocks(current.CityCampaign.Resources, current.Depot),
            capacities, personnel, meals);
        var next = current with
        {
            CityCampaign = current.CityCampaign with { Resources = tick.Next.City },
            Depot = tick.Next.Depot,
            Industry = current.Industry with { Animals = tick.Next.Animals, Transit = tick.Next.Transit }
        };
        V15FoodCampaignCheckpoints.Validate(next, baseline, school, technologies, depotPolicy);
        return new(next, tick);
    }
}
