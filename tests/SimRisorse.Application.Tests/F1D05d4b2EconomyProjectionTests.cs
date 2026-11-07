using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC56: measure actual gross flow at the v15 recipe/meal boundaries.</summary>
public sealed class F1D05d4b2EconomyProjectionTests
{
    private static ResourceAmount U(decimal value) => ResourceAmount.FromUnits(value);
    private static readonly SchoolResearchRules School = new("food", U(10m), 4, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodIndustryConstructionCatalog Sites = FoodIndustryConstructionCatalog.Candidate;
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

    private static V15FoodCampaignCheckpoint Seed(bool fisher = false, bool oldFoodStore = false,
        int depotCount = 0, int depotWorkers = 0)
    {
        var scenario = CivilGrowthScenario.Create();
        var city = V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), School, Technologies);
        city = city with
        {
            ProductiveFamilies = [.. city.ProductiveFamilies.Select(f =>
                (f.FamilyId == "fisher" && fisher || f.FamilyId == "food-store" && oldFoodStore)
                    ? f with { Base = new TierOccupancy(1, 1) } : f)],
            PurchaseCounters = [.. city.PurchaseCounters.Select(c =>
                (c.FamilyId == "fisher" && fisher || c.FamilyId == "food-store" && oldFoodStore)
                    ? c with { Acquired = 1 } : c)]
        };
        var state = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, Depot), Depot);
        return state with
        {
            Depot = new FoodDepotStock(ResourceBundle.Empty, ResourceAmount.Zero),
            Industry = state.Industry with
            {
                Buildings = new FoodIndustryBuildings(
                    new FoodIndustryCrew(0, 0, 0, 0, 0, depotCount),
                    new FoodIndustryCrew(0, 0, 0, 0, 0, depotWorkers))
            }
        };
    }

    private static V15EconomyPreview Preview(V15FoodCampaignCheckpoint checkpoint) =>
        V15EconomyProjector.Project(checkpoint, CivilGrowthScenario.Create(), School,
            Technologies, Sites, Meat, Crops, Depot);

    [Fact]
    public void FisherDepotAndMealContributorsExactlyReconcileGrossFlows()
    {
        var view = Preview(Seed(fisher: true, depotCount: 1, depotWorkers: 1));
        var sources = Assert.IsAssignableFrom<IReadOnlyList<ResourceFlowContributor>>(view.Contributors);
        foreach (var id in new[] { "fish", "food", "bread", "water", "grain", "meat", "fruit" })
        {
            Assert.Equal(view.Production[id].Units, sources.Sum(x => x.Production[id].Units));
            Assert.Equal(view.Consumption[id].Units, sources.Sum(x => x.Consumption[id].Units));
        }
        Assert.Contains(sources, x => x.Source.Contains("Pescatore", StringComparison.Ordinal));
        Assert.Contains(sources, x => x.Source.StartsWith("Abitanti (", StringComparison.Ordinal) &&
            x.Consumption["food"] == U(.080m));
        Assert.Contains(sources, x => x.Source.Contains("Deposito", StringComparison.OrdinalIgnoreCase) &&
            x.Production["food"] == U(1m));
    }

    [Fact]
    public void EveryNewFoodIndustrySourceUsesTheMeasuredStageRuns()
    {
        var start = Seed(depotCount: 1);
        start = start with
        {
            CityCampaign = start.CityCampaign with
            {
                Resources = ResourceBundle.Create(("grain", 3m), ("water", 3m))
            },
            Industry = start.Industry with
            {
                Buildings = new FoodIndustryBuildings(
                    new FoodIndustryCrew(1, 1, 0, 1, 0, 1),
                    new FoodIndustryCrew(1, 1, 0, 1, 0, 1))
            }
        };
        var view = Preview(start);
        var sources = Assert.IsAssignableFrom<IReadOnlyList<ResourceFlowContributor>>(view.Contributors);
        foreach (var id in new[] { "cattle", "game", "vegetables", "water", "grain", "food" })
        {
            Assert.Equal(view.Production[id].Units, sources.Sum(x => x.Production[id].Units));
            Assert.Equal(view.Consumption[id].Units, sources.Sum(x => x.Consumption[id].Units));
        }
        Assert.Contains(sources, x => x.Production["cattle"] == U(1m));
        Assert.Contains(sources, x => x.Production["vegetables"] == U(2m));
    }

    [Fact]
    public void UnstaffedFoodSitesDoNotAppearAsActiveFlowSources()
    {
        var view = Preview(Seed(fisher: true, depotCount: 1));
        var sources = Assert.IsAssignableFrom<IReadOnlyList<ResourceFlowContributor>>(view.Contributors);
        Assert.DoesNotContain(sources, x => x.Source.Contains("Deposito", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(ResourceAmount.Zero, view.Production["food"]);
    }

    [Fact]
    public void OneFisherOneDepotReportsBothActualGrossStagesAndOneMeal()
    {
        var start = Seed(fisher: true, oldFoodStore: true, depotCount: 1, depotWorkers: 1);
        var before = start;
        var view = Preview(start);
        Assert.Equal(U(1m), view.Production["fish"]);
        Assert.Equal(U(1m), view.Consumption["fish"]);
        Assert.Equal(U(1m), view.Production["food"]);
        Assert.Equal(U(.080m), view.Consumption["food"]);
        Assert.Equal(.920m, view.NetPerSecond("food"));
        Assert.Equal(ResourceAmount.Zero, view.NextSecond.CityCampaign.Resources["food"]);
        Assert.Equal(U(.920m), view.NextSecond.Depot.ReadyFood);
        Assert.Equal(before.Depot.ReadyFood, start.Depot.ReadyFood); // Pure preview, no commit.
    }

    [Fact]
    public void DepotReceiptIsNotASecondProductionOrConsumption()
    {
        var start = Seed(fisher: true, depotCount: 1);
        var view = Preview(start);
        Assert.Equal(U(1m), view.Production["fish"]);
        Assert.Equal(ResourceAmount.Zero, view.Consumption["fish"]);
        Assert.Equal(ResourceAmount.Zero, view.Production["food"]);
        Assert.Equal(U(1m), view.NextSecond.Depot.RawFood["fish"]);
        Assert.Equal(ResourceAmount.Zero, view.NextSecond.CityCampaign.Resources["fish"]);
    }

    [Fact]
    public void ObsoleteFoodStoreCannotGenerateOrConsumeAnyResource()
    {
        var start = Seed(oldFoodStore: true) with
        {
            CityCampaign = Seed(oldFoodStore: true).CityCampaign with
            {
                Resources = ResourceBundle.Create(("fish", 5m))
            }
        };
        var view = Preview(start);
        Assert.Equal(ResourceAmount.Zero, view.Production["food"]);
        Assert.Equal(ResourceAmount.Zero, view.Consumption["fish"]);
        Assert.Equal(U(5m), view.NextSecond.CityCampaign.Resources["fish"]);
    }

    [Fact]
    public void CapacityLimitsGrossProcessingBeforeAnAtomicMeal()
    {
        var start = Seed(fisher: true, depotCount: 1, depotWorkers: 1);
        start = start with { Depot = new FoodDepotStock(ResourceBundle.Create(("fish", 1m)), U(100m)) };
        var view = Preview(start);
        Assert.Equal(ResourceAmount.Zero, view.Production["food"]);
        Assert.Equal(U(.080m), view.Consumption["food"]);
        Assert.Equal(U(99.920m), view.NextSecond.Depot.ReadyFood);
        Assert.Equal(U(2m), view.NextSecond.Depot.RawFood["fish"]);
    }

    [Fact]
    public void AtomicAdvancedMealMissingBreadReportsNoReadyFoodConsumption()
    {
        var start = Seed();
        var city = start.CityCampaign with
        {
            Population = 9,
            OtherBuildings = [.. start.CityCampaign.OtherBuildings.Select(b =>
                b.BuildingId == "large-house" ? b with { Count = 1 } : b)],
            PurchaseCounters = [.. start.CityCampaign.PurchaseCounters.Select(c =>
                c.FamilyId == "large-house" ? c with { Acquired = 1 } : c)]
        };
        start = start with { CityCampaign = city,
            Depot = new FoodDepotStock(ResourceBundle.Empty, U(1m)) };
        var view = Preview(start);
        Assert.Equal(ResourceAmount.Zero, view.Consumption["food"]);
        Assert.Equal(ResourceAmount.Zero, view.Consumption["bread"]);
        Assert.Equal(U(1m), view.NextSecond.Depot.ReadyFood);
    }

    [Fact]
    public void MeatAndHorticultureUseRealInputsAndDoNotDoubleCountDeliveries()
    {
        var start = Seed(depotCount: 1);
        start = start with
        {
            CityCampaign = start.CityCampaign with
            {
                Resources = ResourceBundle.Create(("grain", 3m), ("water", 3m))
            },
            Industry = start.Industry with
            {
                Buildings = new FoodIndustryBuildings(
                    new FoodIndustryCrew(1, 1, 0, 1, 0, 1),
                    new FoodIndustryCrew(1, 1, 0, 1, 0, 1))
            }
        };
        var view = Preview(start);
        Assert.Equal(U(1m), view.Production["cattle"]);
        Assert.Equal(U(1m), view.Production["game"]);
        Assert.Equal(U(2m), view.Production["vegetables"]);
        Assert.Equal(U(1m), view.Consumption["grain"]);
        Assert.Equal(U(2m), view.Consumption["water"]);
        Assert.Equal(U(1m), view.Consumption["vegetables"]);
        Assert.Equal(U(1m), view.Production["food"]);
        Assert.Equal(U(1m), view.NextSecond.Depot.RawFood["vegetables"]);
    }

    [Fact]
    public void GrossLedgerReconcilesEveryDisplayedPhysicalStockToTheMilliunit()
    {
        var start = Seed(fisher: true, depotCount: 1, depotWorkers: 1);
        var view = Preview(start);
        var scenario = CivilGrowthScenario.Create();
        var before = V15CampaignCatalogProjection.Project(scenario, start, Sites);
        var after = V15CampaignCatalogProjection.Project(scenario, view.NextSecond, Sites);
        foreach (var item in before.Resources)
        {
            var current = after.Resources.Single(row => row.Id == item.Id);
            Assert.Equal(item.Quantity.Units + view.NetPerSecond(item.Id), current.Quantity.Units);
        }
    }

    [Fact]
    public void ProjectionWorksAtFractionalClockWithoutAdvancingLiveCheckpoint()
    {
        var start = Seed(fisher: true, depotCount: 1, depotWorkers: 1);
        var partial = V15UnifiedCampaignClock.Plan(start, CivilGrowthScenario.Create(),
            School, Technologies, Sites, Meat, Crops, Depot, TimeSpan.FromMilliseconds(650)).Next;
        var view = Preview(partial);
        Assert.Equal(TimeSpan.FromMilliseconds(650).Ticks, partial.CityCampaign.ElapsedTicks);
        Assert.Equal(partial.CityCampaign.ElapsedTicks + TimeSpan.TicksPerSecond,
            view.NextSecond.CityCampaign.ElapsedTicks);
        Assert.Equal(U(1m), view.Production["food"]);
        Assert.Equal(U(.080m), view.Consumption["food"]);
    }

    [Fact]
    public void CapacitySourcesSumExactlyAndReadyFoodHasOnlyOneOwner()
    {
        var start = Seed(depotCount: 2);
        var rows = V15CampaignCatalogProjection.Project(CivilGrowthScenario.Create(), start, Sites).Resources;
        foreach (var row in rows)
            Assert.Equal(row.Capacity.Milliunits, row.CapacitySources.Sum(source => source.Amount.Milliunits));
        var ready = rows.Single(row => row.Id == "food");
        Assert.Single(ready.CapacitySources);
        Assert.Equal("Deposito · Cibo pronto", ready.CapacitySources[0].Source);
        Assert.Equal(U(200m), ready.Capacity);
        Assert.Contains(rows.Single(row => row.Id == "fish").CapacitySources,
            source => source.Source == "Deposito · ingredienti" && source.Amount == U(100m));
    }
}
