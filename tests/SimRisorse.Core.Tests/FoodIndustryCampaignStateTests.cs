using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class FoodIndustryCampaignStateTests
{
    private static FoodIndustryCrew Crew(long stable = 0, long hunter = 0, long butcher = 0,
        long garden = 0, long orchard = 0, long depot = 0) =>
        new(stable, hunter, butcher, garden, orchard, depot);

    [Fact]
    public void WorkplacesAreDerivedFromSixOwnedBaseBuildingCounts()
    {
        var sites = new FoodIndustryBuildings(Crew(1, 2, 3, 4, 5, 6), Crew(1, 2, 2, 2, 3, 3));
        Assert.Equal(21L, sites.Workplaces.Total);
        sites.Validate(population: 30, busyOutsideFood: 10);
        Assert.Equal(6, FoodIndustrySites.Ids.Length);
        Assert.Equal(5L, FoodIndustrySites.Get(sites.Counts, "orchard"));
    }

    [Fact]
    public void CannotAllocateWorkersToBuildingsThatDoNotExist()
    {
        Assert.Throws<ArgumentException>(() => new FoodIndustryBuildings(
            Crew(hunter: 0), Crew(hunter: 1)).Validate(10, 0));
        Assert.Throws<ArgumentException>(() => new FoodIndustryBuildings(
            Crew(depot: 1), Crew(depot: 2)).Validate(10, 0));
    }

    [Fact]
    public void FoodStaffCannotReusePeopleOccupiedByLegacyIndustries()
    {
        Assert.Throws<ArgumentException>(() => new FoodIndustryBuildings(
            Crew(stable: 3), Crew(stable: 3)).Validate(population: 8, busyOutsideFood: 6));
    }

    [Fact]
    public void OnlyCanonicalIntermediateResourcesAreAccepted()
    {
        new FoodIndustryCampaignState(ResourceBundle.Create(("cattle", 1m)),
            ResourceBundle.Create(("vegetables", 1m)), FoodIndustryBuildings.Empty).Validate(8, 0);
        Assert.Throws<ArgumentException>(() => new FoodIndustryCampaignState(
            ResourceBundle.Create(("fish", 1m)), ResourceBundle.Empty,
            FoodIndustryBuildings.Empty).Validate(8, 0));
    }

    [Fact]
    public void AllSixBuildingStatesMustBePresentWhenRebuildingFromSqlite()
    {
        Assert.Throws<ArgumentException>(() => FoodIndustrySites.FromDictionary(
            new Dictionary<string, long> { ["stable"] = 1 }));
        Assert.Throws<ArgumentException>(() => FoodIndustrySites.Get(Crew(), "other"));
    }
}
