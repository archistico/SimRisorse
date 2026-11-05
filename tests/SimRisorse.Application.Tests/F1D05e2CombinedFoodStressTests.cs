using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC59: the existing soak used only fish + depot; exercise all six
/// new sites with farm, well, mill, bakery and fisher on the same clock.</summary>
public sealed class F1D05e2CombinedFoodStressTests
{
    private static ResourceAmount U(decimal value) => ResourceAmount.FromUnits(value);

    [Fact]
    public void FixtureHasEveryNewSiteStaffedWithoutExceedingTheSharedPopulationBudget()
    {
        var fixture = V15CombinedFoodStressScenario.Create();
        var session = new V15FoodIndustryCampaignSession(fixture.Initial,
            fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot);
        Assert.Equal(24, session.Snapshot().CityCampaign.Population);
        Assert.Equal(6L, session.Snapshot().Industry.Buildings.Assigned.Total);
        Assert.Equal(2L, session.AvailableWorkers);
        Assert.Equal(1, session.Snapshot().CityCampaign.ProductiveFamilies.Single(f =>
            f.FamilyId == "bakery").Base.AssignedWorkers);
    }

    [Fact]
    public void FixtureRetainsThreeFoundationWorkersWhenComputingCombinedFreePopulation()
    {
        var scenario = V15CombinedFoodStressScenario.Create();
        var session = new V15FoodIndustryCampaignSession(scenario.Initial,
            scenario.Baseline, scenario.School, scenario.Technologies, scenario.Depot);
        var city = session.Snapshot().CityCampaign;
        var families = city.ProductiveFamilies;

        long AssignedIn(params string[] ids) => families
            .Where(f => ids.Contains(f.FamilyId, StringComparer.Ordinal))
            .Sum(f => (long)f.Base.AssignedWorkers + f.Large.AssignedWorkers + f.Advanced.AssignedWorkers);

        // The benchmark adds 13 food-chain employees to the 3 already employed
        // in the scenario's initial tree-feller, sawmill and stonecutter.
        Assert.Equal(3L, AssignedIn("tree-feller", "sawmill", "stonecutter"));
        Assert.Equal(13L, AssignedIn("fisher", "farm", "well", "mill", "bakery"));
        var cityAssigned = families.Sum(f =>
            (long)f.Base.AssignedWorkers + f.Large.AssignedWorkers + f.Advanced.AssignedWorkers);
        Assert.Equal(16L, cityAssigned);
        var newIndustryAssigned = session.Snapshot().Industry.Buildings.Assigned.Total;
        Assert.Equal(6L, newIndustryAssigned);
        var otherReserved = city.OtherBuildings.Sum(b => (long)b.AssignedWorkers) +
            city.Military.EnlistedPopulation + city.SchoolResearch.ReservedPopulation;
        Assert.Equal(0L, otherReserved);
        Assert.Equal((long)city.Population, cityAssigned + newIndustryAssigned +
            otherReserved + session.AvailableWorkers);
        Assert.Equal(2L, session.AvailableWorkers);
    }

    [Fact]
    public void AllSevenPhysicalProductionStagesStartAndReconcileAtTheMilliunit()
    {
        var fixture = V15CombinedFoodStressScenario.Create();
        var current = fixture.Initial;
        var produced = ResourceBundle.Empty;
        for (var i = 0; i < 4; i++)
        {
            var plan = fixture.Plan(current, TimeSpan.FromSeconds(1));
            var balance = V15StockAuditor.ReconcileSecond(current, plan);
            Assert.True(balance.Balanced);
            produced += plan.FirstSecondProduction;
            current = plan.Next;
        }
        foreach (var id in new[] { "cattle", "game", "meat", "vegetables", "fruit", "bread", "food" })
            Assert.True(produced[id].Milliunits > 0, $"No production from {id}");
        Assert.Equal(ResourceAmount.Zero, current.CityCampaign.Resources["food"]);
    }

    [Fact]
    public void CombinedRunAtOneSecondPerTickMatchesOneContinuousAdvance()
    {
        var fixture = V15CombinedFoodStressScenario.Create();
        var oneShot = fixture.Plan(fixture.Initial, TimeSpan.FromSeconds(90)).Next;
        var current = fixture.Initial;
        for (var i = 0; i < 90; i++)
        {
            var plan = fixture.Plan(current, TimeSpan.FromSeconds(1));
            Assert.True(V15StockAuditor.ReconcileSecond(current, plan).Balanced);
            current = plan.Next;
        }
        Assert.Equal(V15StockAuditor.PhysicalTotals(oneShot), V15StockAuditor.PhysicalTotals(current));
        Assert.Equal(oneShot.CityCampaign.Population, current.CityCampaign.Population);
        Assert.Equal(oneShot.CityCampaign.ElapsedTicks, current.CityCampaign.ElapsedTicks);
        Assert.Equal(oneShot.Depot, current.Depot);
        Assert.Equal(oneShot.Industry, current.Industry);
        Assert.Equal(oneShot.CityCampaign.ResearchBalanceMilliPoints,
            current.CityCampaign.ResearchBalanceMilliPoints);
    }

    [Fact]
    public void LegacyFoodStoreRemainsSuppressedEvenWhenItHasOneAssignedWorker()
    {
        var fixture = V15CombinedFoodStressScenario.Create();
        var city = V15LegacyProducerAdapter.ForV14Rules(fixture.Initial.CityCampaign);
        var historical = fixture.Initial with { CityCampaign = city with
        {
            ProductiveFamilies = [.. city.ProductiveFamilies.Select(f => f.FamilyId == "food-store"
                ? f with { Base = new TierOccupancy(1, 1) } : f)],
            PurchaseCounters = [.. city.PurchaseCounters.Select(c => c.FamilyId == "food-store"
                ? c with { Acquired = 1 } : c)]
        }};
        var advance = fixture.Plan(historical, TimeSpan.FromSeconds(1));
        Assert.True(V15StockAuditor.ReconcileSecond(historical, advance).Balanced);
        Assert.Equal(ResourceAmount.Zero, advance.Next.CityCampaign.Resources["food"]);
        Assert.Equal(U(1m), advance.ReadyFoodProduced);
        Assert.Equal(U(1m), advance.FirstSecondProduction["food"]);
    }

    [Theory]
    [InlineData("fish")]
    [InlineData("meat")]
    [InlineData("vegetables")]
    [InlineData("fruit")]
    [InlineData("bread")]
    public void EachOfTheFiveDepotRecipesConvertsItsOwnIngredient(string ingredient)
    {
        var fixture = V15CombinedFoodStressScenario.Create();
        var start = fixture.Initial;
        var city = start.CityCampaign;
        var onlyDepot = start with
        {
            CityCampaign = city with
            {
                ProductiveFamilies = [.. city.ProductiveFamilies.Select(f =>
                    f with { Base = f.Base with { AssignedWorkers = 0 } })]
            },
            Industry = start.Industry with
            {
                Buildings = new FoodIndustryBuildings(start.Industry.Buildings.Counts,
                    new FoodIndustryCrew(0, 0, 0, 0, 0, 1))
            },
            Depot = new FoodDepotStock(ResourceBundle.Create((ingredient, 1m)), ResourceAmount.Zero)
        };
        var result = fixture.Plan(onlyDepot, TimeSpan.FromSeconds(1));
        Assert.True(V15StockAuditor.ReconcileSecond(onlyDepot, result).Balanced);
        Assert.Equal(U(1m), result.FirstSecondConsumption[ingredient]);
        Assert.Equal(U(1m), result.ReadyFoodProduced);
        Assert.Equal(ResourceAmount.Zero, result.Next.Depot.RawFood[ingredient]);
        Assert.Equal(ResourceAmount.Zero, result.Next.CityCampaign.Resources["food"]);
    }
}
