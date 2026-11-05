using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class F1D05d3FoodIndustryConstructionTests
{
    private static readonly SchoolResearchRules School =
        new("food", U(10m), 120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Depot = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("bread", U(1m), U(1m), 1),
        new("meat", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("vegetables", U(1m), U(1m), 4)
    ]);
    private static ResourceAmount U(decimal value) => ResourceAmount.FromUnits(value);

    private static V15FoodCampaignCheckpoint Seed(bool fisher = true)
    {
        var scenario = CivilGrowthScenario.Create();
        var legacy = V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), School, Technologies);
        legacy = legacy with
        {
            Resources = legacy.Resources + ResourceBundle.Create(("planks", 600m), ("stone", 600m))
        };
        if (fisher) legacy = UnlockLegacy(legacy, "fisher");
        return V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(legacy, Depot), Depot);
    }

    private static V14CampaignSnapshot UnlockLegacy(V14CampaignSnapshot input, string id) =>
        input with
        {
            OtherBuildings = [.. input.OtherBuildings.Select(item => item.BuildingId == id
                ? item with { Count = item.Count + 1 } : item)],
            ProductiveFamilies = [.. input.ProductiveFamilies.Select(item => item.FamilyId == id
                ? item with { Base = item.Base with { Buildings = item.Base.Buildings + 1 } } : item)],
            PurchaseCounters = [.. input.PurchaseCounters.Select(item => item.FamilyId == id
                ? item with { Acquired = item.Acquired + 1 } : item)]
        };

    private static V15FoodIndustryCampaignSession Create(V15FoodCampaignCheckpoint? checkpoint = null) =>
        new(checkpoint ?? Seed(), CivilGrowthScenario.Create(), School, Technologies, Depot);

    [Fact]
    public void SixCanonicalBuildingTypesAreDirectBuildsWithOnlyOneDepot()
    {
        var catalog = FoodIndustryConstructionCatalog.Candidate;
        catalog.ValidateAgainst(CivilGrowthScenario.Create());
        Assert.Equal(6, catalog.Sites.Length);
        Assert.Equal(6, catalog.Sites.Select(x => x.Id).Distinct().Count());
        Assert.DoesNotContain(catalog.Sites, x => x.Id == "food-store");
        Assert.Contains(catalog.Sites, x => x.Id == "food-depot" && x.Name == "Deposito alimentare");
    }

    [Fact]
    public void CatalogueRejectsUnknownOrCircularPrerequisites()
    {
        var baseCatalog = FoodIndustryConstructionCatalog.Candidate;
        var unknown = new FoodIndustryConstructionCatalog(baseCatalog.Sites.Select(item =>
            item.Id == "stable" ? item with { Prerequisites = ["no-building"] } : item), 1.15m);
        Assert.Throws<ArgumentException>(() => unknown.ValidateAgainst(CivilGrowthScenario.Create()));
        var circular = new FoodIndustryConstructionCatalog(baseCatalog.Sites.Select(item =>
            item.Id == "food-depot" ? item with { Prerequisites = ["butcher"] } : item), 1.15m);
        Assert.Throws<ArgumentException>(() => circular.ValidateAgainst(CivilGrowthScenario.Create()));
    }

    [Fact]
    public void FoodDepotCannotBeBoughtBeforeFisher()
    {
        var session = Create(Seed(fisher: false));
        var original = session.Snapshot();
        Assert.Equal(FoodIndustryActionFailure.MissingPrerequisite,
            session.Build("food-depot", 1).Failure);
        Assert.Same(original, session.Snapshot());
    }

    [Fact]
    public void FoodDepotBuildDoesNotConstructOrAssignLegacyFoodStore()
    {
        var session = Create();
        var result = session.Build("food-depot", 1);
        Assert.True(result.Succeeded);
        Assert.Equal(1L, session.Snapshot().Industry.Buildings.Counts.Depot);
        Assert.Equal(0L, session.Snapshot().Industry.Buildings.Assigned.Depot);
        Assert.DoesNotContain(session.Snapshot().CityCampaign.ProductiveFamilies,
            x => x.FamilyId == "food-store");
        Assert.Equal(U(100m), session.Capacities.DepotReady);
    }

    [Fact]
    public void CheckBuildAndActualBuildUseExactlyTheSameProgressiveQuote()
    {
        var session = Create();
        var before = session.Snapshot();
        var quote = session.CheckBuild("hunter", 2);
        Assert.True(quote.Succeeded);
        Assert.Equal(U(151m), quote.Cost["planks"]);
        Assert.Equal(U(76m), quote.Cost["stone"]);
        Assert.Same(before, session.Snapshot());
        var purchase = session.Build("hunter", 2);
        Assert.Equal(quote, purchase);
        Assert.Equal(2L, session.Snapshot().Industry.Buildings.Counts.Hunter);
        Assert.Equal(before.CityCampaign.Resources - quote.Cost,
            session.Snapshot().CityCampaign.Resources);
    }

    [Fact]
    public void LaterBuildingsCostMoreWithoutIntroducingNewDatabaseCounters()
    {
        var session = Create();
        Assert.True(session.Build("hunter", 1).Succeeded);
        var second = session.CheckBuild("hunter", 1);
        Assert.Equal(U(81m), second.Cost["planks"]);
        Assert.Equal(U(41m), second.Cost["stone"]);
    }

    [Fact]
    public void MissingFundsPreventEntireMultiBuildingPurchase()
    {
        var low = Seed();
        low = low with { CityCampaign = low.CityCampaign with
            { Resources = ResourceBundle.Create(("planks", 80m), ("stone", 30m)) } };
        var session = Create(low);
        var before = session.Snapshot();
        var result = session.Build("hunter", 2);
        Assert.Equal(FoodIndustryActionFailure.InsufficientResources, result.Failure);
        Assert.NotEqual(ResourceBundle.Empty, result.Cost);
        Assert.Same(before, session.Snapshot());
    }

    [Fact]
    public void InvalidIdsAndQuantitiesNeverMutateTheCheckpoint()
    {
        var session = Create();
        var before = session.Snapshot();
        Assert.Equal(FoodIndustryActionFailure.UnknownBuilding, session.Build("food-store", 1).Failure);
        Assert.Equal(FoodIndustryActionFailure.UnknownBuilding, session.Build(null, 1).Failure);
        Assert.Equal(FoodIndustryActionFailure.InvalidQuantity, session.Build("hunter", 0).Failure);
        Assert.Equal(FoodIndustryActionFailure.InvalidQuantity, session.Build("hunter", -1).Failure);
        Assert.Same(before, session.Snapshot());
    }

    [Fact]
    public void StableRequiresBothFarmAndWell()
    {
        var session = Create();
        Assert.Equal(FoodIndustryActionFailure.MissingPrerequisite, session.Build("stable", 1).Failure);
        var city = UnlockLegacy(UnlockLegacy(session.Snapshot().CityCampaign.ToInherited(), "farm"), "well");
        session = Create(session.Snapshot() with { CityCampaign = city });
        Assert.True(session.Build("stable", 1).Succeeded);
        Assert.Equal(U(20m), session.Capacities.Animals["cattle"]);
    }

    [Fact]
    public void ButcherAndOrchardRequireTheirOwnIndustryPrerequisites()
    {
        var session = Create();
        Assert.Equal(FoodIndustryActionFailure.MissingPrerequisite, session.Build("butcher", 1).Failure);
        Assert.Equal(FoodIndustryActionFailure.MissingPrerequisite, session.Build("orchard", 1).Failure);
        Assert.True(session.Build("food-depot", 1).Succeeded);
        Assert.True(session.Build("butcher", 1).Succeeded);
        Assert.Equal(U(20m), session.Capacities.Transit["meat"]);
    }

    [Fact]
    public void OnlyOwnedWorkplacesCanBeAssigned()
    {
        var session = Create();
        Assert.True(session.Build("hunter", 2).Succeeded);
        Assert.Equal(FoodIndustryActionFailure.InsufficientWorkplaces,
            session.AssignWorkers("hunter", 3));
        Assert.Equal(FoodIndustryActionFailure.None, session.AssignWorkers("hunter", 2));
        Assert.Equal(2L, session.Snapshot().Industry.Buildings.Assigned.Hunter);
        Assert.Equal(FoodIndustryActionFailure.InsufficientWorkplaces,
            session.AssignWorkers("hunter", 1));
    }

    [Fact]
    public void ReleaseCannotExceedAssignedAndIsAtomic()
    {
        var session = Create();
        Assert.True(session.Build("hunter", 1).Succeeded);
        Assert.Equal(FoodIndustryActionFailure.None, session.AssignWorkers("hunter", 1));
        var before = session.Snapshot();
        Assert.Equal(FoodIndustryActionFailure.InsufficientAssignedWorkers,
            session.ReleaseWorkers("hunter", 2));
        Assert.Same(before, session.Snapshot());
        Assert.Equal(FoodIndustryActionFailure.None, session.ReleaseWorkers("hunter", 1));
        Assert.Equal(0L, session.Snapshot().Industry.Buildings.Assigned.Hunter);
    }

    [Fact]
    public void CannotAssignFoodWorkersWhoAreReservedElsewhere()
    {
        var start = Seed();
        // The starting city already employs legacy industry workers. Reserve
        // exactly the remainder in Hunter, not more than the population permits:
        // the checkpoint must be valid BEFORE the rejected assignment.
        var city = start.CityCampaign;
        var alreadyBusy = city.OtherBuildings.Sum(b => (long)b.AssignedWorkers) +
            city.ProductiveFamilies.Sum(f => (long)f.Base.AssignedWorkers +
                f.Large.AssignedWorkers + f.Advanced.AssignedWorkers) +
            city.Military.EnlistedPopulation + city.SchoolResearch.ReservedPopulation;
        var free = city.Population - alreadyBusy;
        Assert.True(free > 0);
        start = start with { Industry = start.Industry with
        {
            Buildings = new FoodIndustryBuildings(new FoodIndustryCrew(0, free + 1, 0, 0, 0, 0),
                new FoodIndustryCrew(0, free, 0, 0, 0, 0))
        } };
        var session = Create(start);
        var before = session.Snapshot();
        Assert.Equal(0L, session.AvailableWorkers);
        Assert.Equal(FoodIndustryActionFailure.InsufficientPopulation,
            session.AssignWorkers("hunter", 1));
        Assert.Same(before, session.Snapshot());
    }

    [Fact]
    public void StorageCapacityDerivesFromSiteCountsWithoutInventedBaselines()
    {
        var catalog = FoodIndustryConstructionCatalog.Candidate;
        var empty = catalog.CapacitiesFor(FoodIndustryBuildings.Empty);
        Assert.Equal(ResourceAmount.Zero, empty.DepotReady);
        Assert.Equal(ResourceAmount.Zero, empty.Animals["game"]);
        var buildings = new FoodIndustryBuildings(new FoodIndustryCrew(2, 3, 1, 4, 5, 2),
            new FoodIndustryCrew(0, 0, 0, 0, 0, 0));
        var capacities = catalog.CapacitiesFor(buildings);
        Assert.Equal(U(40m), capacities.Animals["cattle"]);
        Assert.Equal(U(60m), capacities.Animals["game"]);
        Assert.Equal(U(20m), capacities.Transit["meat"]);
        Assert.Equal(U(80m), capacities.Transit["vegetables"]);
        Assert.Equal(U(100m), capacities.Transit["fruit"]);
        Assert.Equal(U(100m), capacities.DepotRaw["bread"]);
        Assert.Equal(U(200m), capacities.DepotReady);
    }

    [Fact]
    public void NewFoodStockInOverCapacityBinIsNotSilentlyTruncated()
    {
        var start = Seed();
        start = start with { Industry = start.Industry with
            { Animals = ResourceBundle.Create(("game", 100m)) } };
        var session = Create(start);
        Assert.Equal(ResourceAmount.Zero, session.Capacities.Animals["game"]);
        Assert.Equal(U(100m), session.Snapshot().Industry.Animals["game"]);
        Assert.True(session.Build("hunter", 1).Succeeded);
        Assert.Equal(U(100m), session.Snapshot().Industry.Animals["game"]);
    }

    private static MeatChainPolicy MeatPolicy => new("grain", "water", "cattle", "game", "meat",
        ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(1m), U(1m),
        [new("cattle", U(1m), U(1m), 0), new("game", U(1m), U(1m), 1)]);
    private static HorticulturePolicy Crops => new("water", "vegetables", "fruit",
        new("vegetables", U(1m), U(0m), 0), new("fruit", U(1m), U(0m), 1));

    [Fact]
    public void FoodStepUsesActualConstructedWorkplacesAndNoArtificialCapacity()
    {
        var session = Create();
        Assert.True(session.Build("hunter", 1).Succeeded);
        Assert.Equal(FoodIndustryActionFailure.None, session.AssignWorkers("hunter", 1));
        var oldTicks = session.Snapshot().CityCampaign.ElapsedTicks;
        session.StepFood(MeatPolicy, Crops, new(U(0m), U(0m), U(0m)));
        Assert.Equal(U(1m), session.Snapshot().Industry.Animals["game"]);
        Assert.Equal(oldTicks, session.Snapshot().CityCampaign.ElapsedTicks);
        Assert.Equal(ResourceAmount.Zero, session.Snapshot().Industry.Transit["meat"]);
    }

    [Fact]
    public void HunterButcherDepotDeliverWithoutDoubleUsingAnimalsOrPeople()
    {
        var session = Create();
        Assert.True(session.Build("food-depot", 1).Succeeded);
        Assert.True(session.Build("butcher", 1).Succeeded);
        Assert.True(session.Build("hunter", 1).Succeeded);
        Assert.Equal(FoodIndustryActionFailure.None, session.AssignWorkers("food-depot", 1));
        Assert.Equal(FoodIndustryActionFailure.None, session.AssignWorkers("butcher", 1));
        Assert.Equal(FoodIndustryActionFailure.None, session.AssignWorkers("hunter", 1));
        var startingFood = session.Snapshot().Depot.ReadyFood;
        session.StepFood(MeatPolicy, Crops, new(U(0m), U(0m), U(0m)));
        Assert.Equal(U(1m), session.Snapshot().Industry.Animals["game"]);
        Assert.Equal(startingFood, session.Snapshot().Depot.ReadyFood);
        session.StepFood(MeatPolicy, Crops, new(U(0m), U(0m), U(0m)));
        Assert.Equal(U(1m), session.Snapshot().Industry.Animals["game"]);
        Assert.Equal(startingFood + U(1m), session.Snapshot().Depot.ReadyFood);
        Assert.Equal(ResourceAmount.Zero, session.Snapshot().Industry.Transit["meat"]);
        Assert.Equal(3L, session.Snapshot().Industry.Buildings.Assigned.Total);
    }
}
