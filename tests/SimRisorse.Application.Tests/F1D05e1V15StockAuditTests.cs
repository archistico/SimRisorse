using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC57: independent stock-ledger and deterministic clock stress.</summary>
public sealed class F1D05e1V15StockAuditTests
{
    private static ResourceAmount U(decimal value) => ResourceAmount.FromUnits(value);
    private static readonly SchoolResearchRules School = new("food", U(10m), 4, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodIndustryConstructionCatalog Catalog = FoodIndustryConstructionCatalog.Candidate;
    private static readonly MeatChainPolicy Meat = new("grain", "water", "cattle", "game", "meat",
        ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(1m), U(1m),
        [new("cattle", U(1m), U(2m), 0), new("game", U(1m), U(1m), 1)]);
    private static readonly HorticulturePolicy Crops = new("water", "vegetables", "fruit",
        new("vegetables", U(2m), U(1m), 0), new("fruit", U(3m), U(1m), 1));
    private static readonly FoodConversionPolicy Depot = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
        new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("bread", U(1m), U(1m), 4)
    ]);

    private static V15FoodCampaignCheckpoint Seed(bool fisher = true, bool legacy = false,
        bool depot = true)
    {
        var scenario = CivilGrowthScenario.Create();
        var city = V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), School, Technologies);
        city = city with
        {
            ProductiveFamilies = [.. city.ProductiveFamilies.Select(f =>
                (f.FamilyId == "fisher" && fisher || f.FamilyId == "food-store" && legacy)
                    ? f with { Base = new TierOccupancy(1, 1) } : f)],
            PurchaseCounters = [.. city.PurchaseCounters.Select(c =>
                (c.FamilyId == "fisher" && fisher || c.FamilyId == "food-store" && legacy)
                    ? c with { Acquired = 1 } : c)]
        };
        var start = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, Depot), Depot);
        return start with
        {
            Depot = new FoodDepotStock(ResourceBundle.Empty, ResourceAmount.Zero),
            Industry = start.Industry with
            {
                Buildings = new FoodIndustryBuildings(
                    new FoodIndustryCrew(0, 0, 0, 0, 0, depot ? 1 : 0),
                    new FoodIndustryCrew(0, 0, 0, 0, 0, depot ? 1 : 0))
            }
        };
    }

    private static V15CampaignAdvancePlan Advance(V15FoodCampaignCheckpoint start, TimeSpan elapsed) =>
        V15UnifiedCampaignClock.Plan(start, CivilGrowthScenario.Create(), School,
            Technologies, Catalog, Meat, Crops, Depot, elapsed);

    private static void AssertReconciled(V15FoodCampaignCheckpoint start, V15CampaignAdvancePlan advance)
    {
        var report = V15StockAuditor.ReconcileSecond(start, advance);
        Assert.True(report.Balanced, string.Join("; ", report.Differences.Select(row =>
            $"{row.ResourceId}: expected {row.ExpectedMilliunits}, actual {row.ActualMilliunits}")));
        Assert.Empty(report.Differences);
    }

    [Fact]
    public void FisherToDepotAndMealBalanceWithoutDoubleCountingTransfers()
    {
        var start = Seed();
        var plan = Advance(start, TimeSpan.FromSeconds(1));
        AssertReconciled(start, plan);
        Assert.Equal(U(1m), plan.FirstSecondProduction["fish"]);
        Assert.Equal(U(1m), plan.FirstSecondConsumption["fish"]);
        Assert.Equal(U(1m), plan.FirstSecondProduction["food"]);
        Assert.Equal(U(.080m), plan.FirstSecondConsumption["food"]);
        Assert.Equal(U(.920m), V15StockAuditor.PhysicalTotals(plan.Next)["food"]);
    }

    [Fact]
    public void DeliveryWithoutStaffIsOnlyATransfer()
    {
        var start = Seed(depot: true);
        start = start with { Industry = start.Industry with
        {
            Buildings = new FoodIndustryBuildings(new FoodIndustryCrew(0, 0, 0, 0, 0, 1),
                new FoodIndustryCrew(0, 0, 0, 0, 0, 0))
        }};
        var plan = Advance(start, TimeSpan.FromSeconds(1));
        AssertReconciled(start, plan);
        Assert.Equal(U(1m), V15StockAuditor.PhysicalTotals(plan.Next)["fish"]);
        Assert.Equal(ResourceAmount.Zero, plan.FirstSecondConsumption["fish"]);
        Assert.Equal(ResourceAmount.Zero, plan.FirstSecondProduction["food"]);
    }

    [Fact]
    public void IntermediateAnimalAndCropTransfersBalanceAcrossAllInventories()
    {
        var start = Seed(fisher: false);
        start = start with
        {
            CityCampaign = start.CityCampaign with { Resources = ResourceBundle.Create(("grain", 3m), ("water", 3m)) },
            Industry = start.Industry with
            {
                Buildings = new FoodIndustryBuildings(new FoodIndustryCrew(1, 1, 0, 1, 0, 1),
                    new FoodIndustryCrew(1, 1, 0, 1, 0, 1))
            }
        };
        var plan = Advance(start, TimeSpan.FromSeconds(1));
        AssertReconciled(start, plan);
        Assert.Equal(U(1m), V15StockAuditor.PhysicalTotals(plan.Next)["cattle"]);
        Assert.Equal(U(1m), V15StockAuditor.PhysicalTotals(plan.Next)["game"]);
        Assert.Equal(U(1m), plan.FirstSecondProduction["food"]);
    }

    [Fact]
    public void FullReadyFoodBinAndOverCapacityIngredientsStayConserved()
    {
        var start = Seed();
        start = start with { Depot = new FoodDepotStock(ResourceBundle.Create(("fish", 51m)), U(120m)) };
        var plan = Advance(start, TimeSpan.FromSeconds(1));
        AssertReconciled(start, plan);
        Assert.Equal(ResourceAmount.Zero, plan.FirstSecondProduction["food"]);
        Assert.Equal(U(120m) - U(.080m), plan.Next.Depot.ReadyFood);
        Assert.Equal(U(51m), plan.Next.Depot.RawFood["fish"]);
    }

    [Fact]
    public void HistoricalFoodStoreCannotProduceAndItsStockIsNotLost()
    {
        var start = Seed(fisher: false, legacy: true, depot: false);
        start = start with { CityCampaign = start.CityCampaign with
        {
            Resources = ResourceBundle.Create(("fish", 5m))
        }};
        var plan = Advance(start, TimeSpan.FromSeconds(1));
        AssertReconciled(start, plan);
        Assert.Equal(ResourceAmount.Zero, plan.FirstSecondProduction["food"]);
        Assert.Equal(U(5m), V15StockAuditor.PhysicalTotals(plan.Next)["fish"]);
    }

    [Fact]
    public void AuditReportsAChangedGrossLedgerRatherThanMaskingIt()
    {
        var start = Seed();
        var actual = Advance(start, TimeSpan.FromSeconds(1));
        var tampered = actual with
        {
            FirstSecondProduction = actual.FirstSecondProduction + ResourceBundle.Create(("fish", .001m))
        };
        var report = V15StockAuditor.ReconcileSecond(start, tampered);
        Assert.False(report.Balanced);
        Assert.Single(report.Differences);
        Assert.Equal("fish", report.Differences[0].ResourceId);
        Assert.Equal(-1L, report.Differences[0].DifferenceMilliunits);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void FirstSecondLedgerCannotBeMisusedForAnotherDuration(int seconds)
    {
        var start = Seed();
        var plan = Advance(start, TimeSpan.FromSeconds(seconds));
        Assert.Throws<ArgumentException>(() => V15StockAuditor.ReconcileSecond(start, plan));
    }

    [Fact]
    public void LongInterruptedRunMatchesContinuousRunIncludingFractionalTicks()
    {
        var start = Seed(legacy: true);
        const int total = 120;
        var oneShot = Advance(start, TimeSpan.FromSeconds(total)).Next;
        var current = Advance(start, TimeSpan.FromMilliseconds(375)).Next;
        for (var i = 0; i < total; i++)
        {
            var plan = Advance(current, TimeSpan.FromSeconds(1));
            AssertReconciled(current, plan);
            current = plan.Next;
        }
        var matchingClock = Advance(oneShot, TimeSpan.FromMilliseconds(375)).Next;
        Assert.Equal(matchingClock.CityCampaign.ElapsedTicks, current.CityCampaign.ElapsedTicks);
        Assert.Equal(V15StockAuditor.PhysicalTotals(matchingClock),
            V15StockAuditor.PhysicalTotals(current));
        Assert.Equal(matchingClock.CityCampaign.Population, current.CityCampaign.Population);
        Assert.Equal(matchingClock.CityCampaign.GrowthProgressSeconds,
            current.CityCampaign.GrowthProgressSeconds);
        Assert.Equal(matchingClock.CityCampaign.GrowthRemainderTwentieths,
            current.CityCampaign.GrowthRemainderTwentieths);
        Assert.Equal(matchingClock.Industry, current.Industry);
        Assert.Equal(matchingClock.Depot, current.Depot);
        Assert.Equal(matchingClock.CityCampaign.ResearchBalanceMilliPoints,
            current.CityCampaign.ResearchBalanceMilliPoints);
    }

    [Fact]
    public void PhysicalTotalsIncludeEveryCompartmentAndReadyFoodExactlyOnce()
    {
        var start = Seed(fisher: false, depot: false);
        start = start with
        {
            CityCampaign = start.CityCampaign with { Resources = ResourceBundle.Create(("fish", 1m), ("bread", 2m)) },
            Depot = new FoodDepotStock(ResourceBundle.Create(("fish", 3m), ("bread", 4m)), U(5m)),
            Industry = start.Industry with
            {
                Animals = ResourceBundle.Create(("cattle", 6m), ("game", 7m)),
                Transit = ResourceBundle.Create(("meat", 8m), ("fruit", 9m), ("vegetables", 10m))
            }
        };
        var total = V15StockAuditor.PhysicalTotals(start);
        Assert.Equal(U(4m), total["fish"]);
        Assert.Equal(U(6m), total["bread"]);
        Assert.Equal(U(5m), total["food"]);
        Assert.Equal(U(6m), total["cattle"]);
        Assert.Equal(U(7m), total["game"]);
        Assert.Equal(U(8m), total["meat"]);
        Assert.Equal(U(9m), total["fruit"]);
        Assert.Equal(U(10m), total["vegetables"]);
    }
}
