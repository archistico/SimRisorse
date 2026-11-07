using SimRisorse.Core.Construction;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class Rc81MintCrewPortfolioTests
{
    [Theory]
    [InlineData(ProductionTier.Base, 1, 10)]
    [InlineData(ProductionTier.Large, 3, 30)]
    [InlineData(ProductionTier.Advanced, 10, 100)]
    public void MintCrewPortfolioReservesTenDistinctPeoplePerProductiveModule(
        ProductionTier tier, int modules, int people)
    {
        var portfolio = TieredBuildingPortfolio.Empty("mint");
        var crew = TieredBuildingPortfolio.Create("mint", default, default, default, 10)
            .Build(tier, 1);
        Assert.Equal(0, portfolio.TotalWorkplaces);
        Assert.Equal(10, crew.CrewSize);
        Assert.Equal(people, crew.TotalWorkplaces);
        Assert.Equal(modules, crew.StorageEquivalents);
        var assigned = crew.Assign(tier, people, people);
        Assert.Equal(people, assigned.AssignedWorkers);
        Assert.Throws<InvalidOperationException>(() => assigned.Assign(tier, 1, 1));
        Assert.Equal(people - 1, assigned.Release(tier, 1).AssignedWorkers);
    }

    [Fact]
    public void UpgradingMintTransfersAtMostOneBuildingCrewWithoutCreatingWorkers()
    {
        var initial = TieredBuildingPortfolio.Create("mint", new(2, 19), default, default, 10);
        var next = initial.Upgrade(ProductionTier.Base, ProductionTier.Large);
        Assert.Equal(new TierOccupancy(1, 9), next.Base);
        Assert.Equal(new TierOccupancy(1, 10), next.Large);
        Assert.Equal(19, next.AssignedWorkers);
        Assert.Equal(40, next.TotalWorkplaces);
        Assert.Equal(4, next.StorageEquivalents);
        Assert.Equal(10, initial.CrewSize);
        Assert.Equal(20, initial.TotalWorkplaces);
    }

    [Fact]
    public void OptedInRuleIsMintOnlyAndInvalidRulesAreRejected()
    {
        Assert.Equal(1, V15CrewRules.CrewSize(V15CrewMode.Legacy, "mint"));
        Assert.Equal(10, V15CrewRules.CrewSize(V15CrewMode.MintTen, "mint"));
        Assert.Equal(1, V15CrewRules.CrewSize(V15CrewMode.MintTen, "fisher"));
        Assert.Equal(V15CrewMode.MintTen,
            V15CrewRules.FromSavedRulesVersion(V15CrewRules.MintTenRulesVersion));
        Assert.Equal(V15CrewMode.Legacy,
            V15CrewRules.FromSavedRulesVersion(V15FoodCampaignCheckpoints.RulesVersion));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            V15CrewRules.SavedRulesVersion((V15CrewMode)42));
        Assert.Throws<ArgumentException>(() =>
            V15CrewRules.FromSavedRulesVersion("civil-growth-15-future"));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TieredBuildingPortfolio.Create("mint", default, default, default, 0));
    }

    [Fact]
    public void DefaultSinglePersonPortfolioIsUnchangedForOlderGames()
    {
        var legacy = TieredBuildingPortfolio.Create("mint", new(1, 1), new(1, 2), default);
        Assert.Equal(1, legacy.CrewSize);
        Assert.Equal(4, legacy.TotalWorkplaces);
        Assert.Equal(4, legacy.StorageEquivalents);
        Assert.Equal(3, legacy.AssignedWorkers);
        Assert.Equal(4, legacy.Assign(ProductionTier.Large, 1, 1).AssignedWorkers);
    }
}
