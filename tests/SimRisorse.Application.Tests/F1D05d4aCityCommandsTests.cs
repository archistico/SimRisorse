using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Military;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC52: v15 command routing and catalog projection, not a GUI smoke test.</summary>
public sealed class F1D05d4aCityCommandsTests
{
    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);
    private static readonly SchoolResearchRules School = new("food", U(2m), 3, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Depot = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
        new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("bread", U(1m), U(1m), 4)
    ]);
    private static readonly MeatChainPolicy Meat = new("grain", "water", "cattle", "game", "meat",
        ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(1m), U(1m),
        [new("cattle", U(1m), U(2m), 0), new("game", U(1m), U(1m), 1)]);
    private static readonly HorticulturePolicy Crops = new("water", "vegetables", "fruit",
        new("vegetables", U(2m), U(1m), 0), new("fruit", U(3m), U(1m), 1));

    private static V15FoodCampaignCheckpoint Seed()
    {
        var scenario = CivilGrowthScenario.Create();
        var source = V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), School, Technologies);
        source = source with { Resources = source.Resources + ResourceBundle.Create(
            ("planks", 500m), ("stone", 500m), ("swords", 25m)) };
        return V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(source, Depot), Depot);
    }

    private static V15FoodIndustryCampaignSession Start(V15FoodCampaignCheckpoint? source = null) =>
        new(source ?? Seed(), CivilGrowthScenario.Create(), School, Technologies, Depot);

    private static void SetUpSchool(V15FoodIndustryCampaignSession game)
    {
        Assert.True(game.BuildCityProducer("fisher", ProductionTier.Base, 1).Succeeded);
        Assert.True(game.Build("food-depot", 1).Succeeded);
        Assert.True(game.BuildSchool(1).Succeeded);
    }

    private static V15FoodCampaignCheckpoint WithBarracks(V15FoodCampaignCheckpoint initial)
    {
        var city = initial.CityCampaign;
        return initial with { CityCampaign = city with
        {
            OtherBuildings = [.. city.OtherBuildings.Select(b => b.BuildingId == "barracks"
                ? b with { Count = 1 } : b)],
            PurchaseCounters = [.. city.PurchaseCounters.Select(c => c.FamilyId == "barracks"
                ? c with { Acquired = 1 } : c)]
        } };
    }

    [Fact]
    public void CatalogContainsSixFoodSitesAndDirectProducerTiersButNoRetiredDepot()
    {
        var view = Start().CityCatalog;
        Assert.Equal(6, view.Buildings.Count(b => b.Kind == V15BuildingKind.FoodIndustry));
        Assert.DoesNotContain(view.Buildings, b => b.Id.Contains("food-store", StringComparison.Ordinal));
        Assert.Contains(view.Buildings, b => b.Id == "food-depot" && b.Kind == V15BuildingKind.FoodIndustry);
        Assert.Contains(view.Buildings, b => b.Id == "fisher-large" && b.Tier == ProductionTier.Large);
        Assert.Contains(view.Buildings, b => b.Id == "mint-advanced" && b.Tier == ProductionTier.Advanced);
        Assert.Contains(view.Buildings, b => b.Id == "school" && b.Kind == V15BuildingKind.School);
        Assert.Equal(view.Buildings.Length, view.Buildings.Select(b => b.Id).Distinct().Count());
    }

    [Fact]
    public void ResourceSummaryAggregatesPhysicalInventoriesWithoutDoubleCountingFood()
    {
        var initial = Seed();
        var city = initial.CityCampaign;
        initial = initial with
        {
            CityCampaign = city with { Resources = city.Resources +
                ResourceBundle.Create(("fish", 2m), ("bread", 3m)) },
            Depot = new(ResourceBundle.Create(("fish", 4m), ("meat", 2m)), U(17m)),
            Industry = initial.Industry with
            {
                Transit = ResourceBundle.Create(("meat", 2m)),
                Animals = ResourceBundle.Create(("cattle", 3m)),
                Buildings = new(new(1, 0, 1, 0, 0, 1), new(0, 0, 0, 0, 0, 0))
            }
        };
        var resources = Start(initial).CityCatalog.Resources.ToDictionary(r => r.Id);
        Assert.Equal(U(17m), resources["food"].Quantity);
        Assert.Equal(U(100m), resources["food"].Capacity);
        Assert.Equal(U(6m), resources["fish"].Quantity);
        Assert.Equal(U(4m), resources["meat"].Quantity);
        Assert.Equal(U(3m), resources["cattle"].Quantity);
        Assert.Equal(U(20m), resources["cattle"].Capacity);
        Assert.Contains("pencils", resources.Keys);
        Assert.Equal(resources.Count, resources.Values.Select(r => r.Name).Distinct().Count());
    }

    [Fact]
    public void FishAndBreadCanWaitInTheCityWhileTheDepotHoldsOtherBatches()
    {
        var initial = Seed();
        var staged = initial with
        {
            CityCampaign = initial.CityCampaign with
            {
                Resources = initial.CityCampaign.Resources +
                    ResourceBundle.Create(("fish", 2m), ("bread", 3m))
            },
            Depot = initial.Depot with
            {
                RawFood = initial.Depot.RawFood +
                    ResourceBundle.Create(("fish", 4m), ("bread", 5m))
            }
        };
        // Two physical bins for each unprocessed ingredient are legitimate.
        // Prepared food must still have exactly one owner (the depot).
        var game = Start(staged);
        var resources = game.CityCatalog.Resources.ToDictionary(r => r.Id);
        Assert.Equal(U(6m), resources["fish"].Quantity);
        Assert.Equal(U(8m), resources["bread"].Quantity);
        Assert.Equal(staged.Depot.ReadyFood, resources["food"].Quantity);
        Assert.Equal(U(2m), game.Snapshot().CityCampaign.Resources["fish"]);
        Assert.Equal(U(4m), game.Snapshot().Depot.RawFood["fish"]);
        Assert.Equal(ResourceAmount.Zero, game.Snapshot().CityCampaign.Resources["food"]);
    }

    [Fact]
    public void RetiredFoodStoreCannotBePurchasedOrStaffedInNewRuleset()
    {
        var game = Start();
        var before = game.Snapshot();
        Assert.Equal(V14TierActionFailure.UnknownFamily,
            game.BuildCityProducer("food-store", ProductionTier.Base, 1).Failure);
        Assert.Equal(V14TierActionFailure.UnknownFamily,
            game.AssignCityProducerWorkers("food-store", ProductionTier.Base, 1));
        Assert.Same(before, game.Snapshot());
    }

    [Fact]
    public void ExplicitRetirementFreesLegacyStaffWithoutRefundingInventories()
    {
        var source = Seed();
        var city = V15LegacyProducerAdapter.ForV14Rules(source.CityCampaign);
        source = source with { CityCampaign = city with
        {
            ProductiveFamilies = [.. city.ProductiveFamilies.Select(f => f.FamilyId == "food-store"
                ? f with { Base = new TierOccupancy(1, 1) } : f)],
            PurchaseCounters = [.. city.PurchaseCounters.Select(c => c.FamilyId == "food-store"
                ? c with { Acquired = 1 } : c)]
        } };
        var game = Start(source);
        var before = game.AvailableWorkers;
        Assert.True(game.RetireLegacyFoodStore().Succeeded);
        Assert.Equal(before + 1, game.AvailableWorkers);
        Assert.Equal(source.CityCampaign.Resources, game.Snapshot().CityCampaign.Resources);
        Assert.Equal(source.Depot, game.Snapshot().Depot);
        Assert.True(game.RetireLegacyFoodStore().Succeeded);
        Assert.Equal(before + 1, game.AvailableWorkers);
        Assert.Equal(1, game.Snapshot().CityCampaign.PurchaseCounters.Single(c => c.FamilyId == "food-store").Acquired);
    }

    [Fact]
    public void LegacyCivicAndProducerConstructionReuseHistoryButKeepV15Checkpoint()
    {
        var game = Start();
        Assert.True(game.BuildCityProducer("fisher", ProductionTier.Base, 1).Succeeded);
        Assert.True(game.BuildCityBuilding("warehouse", 1).Succeeded);
        Assert.True(game.Build("food-depot", 1).Succeeded);
        Assert.Equal(1, game.Snapshot().CityCampaign.ProductiveFamilies.Single(f => f.FamilyId == "fisher").Base.Buildings);
        Assert.Equal(1, game.Snapshot().CityCampaign.OtherBuildings.Single(b => b.BuildingId == "warehouse").Count);
        Assert.Equal(1L, game.Snapshot().Industry.Buildings.Counts.Depot);
        Assert.Equal(1, game.CityCatalog.Buildings.Single(b => b.Id == "warehouse").Owned);
    }

    [Fact]
    public void SchoolConstructionRequiresTheNewDepotNotTheObsoleteFoodStore()
    {
        var game = Start();
        var before = game.Snapshot();
        Assert.Equal(V14SchoolConstructionFailure.MissingPrerequisite, game.BuildSchool(1).Failure);
        Assert.Same(before, game.Snapshot());
        Assert.True(game.BuildCityProducer("fisher", ProductionTier.Base, 1).Succeeded);
        Assert.True(game.Build("food-depot", 1).Succeeded);
        Assert.True(game.BuildSchool(1).Succeeded);
        Assert.Equal(1, game.Snapshot().CityCampaign.SchoolCount);
    }

    [Fact]
    public void StudentsPayReadyFoodToDepotAndNeverCreateCityFood()
    {
        var game = Start();
        SetUpSchool(game);
        var before = game.Snapshot();
        Assert.Equal(SchoolActionFailure.None, game.EnrollStudents(1));
        Assert.Equal(before.Depot.ReadyFood - U(2m), game.Snapshot().Depot.ReadyFood);
        Assert.Equal(ResourceAmount.Zero, game.Snapshot().CityCampaign.Resources["food"]);
        Assert.Equal(before.CityCampaign.Resources, game.Snapshot().CityCampaign.Resources);
        Assert.Equal(1, game.Snapshot().CityCampaign.SchoolResearch.ReservedPopulation);
    }

    [Fact]
    public void MissingDepotFoodRejectsEnrollmentAtomically()
    {
        var game = Start();
        SetUpSchool(game);
        var lowFood = game.Snapshot() with { Depot = new(ResourceBundle.Empty, U(1m)) };
        game = Start(lowFood);
        var before = game.Snapshot();
        Assert.Equal(SchoolActionFailure.InsufficientFood, game.EnrollStudents(1));
        Assert.Same(before, game.Snapshot());
    }

    [Fact]
    public void CancellingStudentsRefundsDepotAndKeepsCityUnchanged()
    {
        var game = Start();
        SetUpSchool(game);
        Assert.Equal(SchoolActionFailure.None, game.EnrollStudents(1));
        var before = game.Snapshot();
        Assert.Equal(SchoolActionFailure.None, game.CancelStudents(1));
        Assert.Equal(before.Depot.ReadyFood + U(2m), game.Snapshot().Depot.ReadyFood);
        Assert.Equal(before.CityCampaign.Resources, game.Snapshot().CityCampaign.Resources);
        Assert.Equal(0, game.Snapshot().CityCampaign.SchoolResearch.ReservedPopulation);
    }

    [Fact]
    public void StudentRefundDoesNotOverfillReadyFoodStorage()
    {
        var game = Start();
        SetUpSchool(game);
        Assert.Equal(SchoolActionFailure.None, game.EnrollStudents(1));
        game = Start(game.Snapshot() with { Depot = game.Snapshot().Depot with { ReadyFood = U(100m) } });
        var before = game.Snapshot();
        Assert.Equal(SchoolActionFailure.InsufficientStorage, game.CancelStudents(1));
        Assert.Same(before, game.Snapshot());
    }

    [Fact]
    public void FoodWorkersReduceThePoolAvailableForLegacyTierStaffing()
    {
        var game = Start();
        Assert.True(game.Build("hunter", 5).Succeeded);
        Assert.Equal(FoodIndustryActionFailure.None, game.AssignWorkers("hunter", 5));
        Assert.True(game.BuildCityProducer("fisher", ProductionTier.Base, 2).Succeeded);
        Assert.Equal(V14TierActionFailure.InsufficientPopulation,
            game.AssignCityProducerWorkers("fisher", ProductionTier.Base, 1));
        Assert.Equal(0, game.Snapshot().CityCampaign.ProductiveFamilies.Single(f => f.FamilyId == "fisher").Base.AssignedWorkers);
    }

    [Fact]
    public void EnrolledStudentsAndFoodWorkersCannotReuseTheSamePerson()
    {
        var game = Start();
        SetUpSchool(game);
        Assert.True(game.Build("hunter", 4).Succeeded);
        Assert.Equal(FoodIndustryActionFailure.None, game.AssignWorkers("hunter", 4));
        var before = game.Snapshot();
        Assert.Equal(SchoolActionFailure.InsufficientPopulation, game.EnrollStudents(2));
        Assert.Same(before, game.Snapshot());
    }

    [Fact]
    public void EnlistmentUsesCombinedWorkforceAndDoesNotSpendSwordsWhenFull()
    {
        var game = Start(WithBarracks(Seed()));
        Assert.True(game.Build("hunter", 5).Succeeded);
        Assert.Equal(FoodIndustryActionFailure.None, game.AssignWorkers("hunter", 5));
        var before = game.Snapshot();
        Assert.Equal(GameError.InsufficientPopulation, game.Enlist(1).Error);
        Assert.Same(before, game.Snapshot());
        Assert.Equal(FoodIndustryActionFailure.None, game.ReleaseWorkers("hunter", 1));
        Assert.True(game.Enlist(1).Succeeded);
        Assert.Equal(before.CityCampaign.Resources["swords"] - U(5m),
            game.Snapshot().CityCampaign.Resources["swords"]);
    }

    [Fact]
    public void ResearchPurchasesSpendPointsAndDoNotTouchFoodInventories()
    {
        var source = Seed();
        source = source with { CityCampaign = source.CityCampaign with
            { ResearchBalanceMilliPoints = ResearchPoints.FromUnits(120m).MilliPoints } };
        var game = Start(source);
        Assert.Equal(TechnologyPurchaseFailure.None, game.BuyTechnology("lavorazioni-1"));
        Assert.Equal(ResearchPoints.FromUnits(20m).MilliPoints,
            game.Snapshot().CityCampaign.ResearchBalanceMilliPoints);
        Assert.Equal(source.Depot, game.Snapshot().Depot);
        var after = game.Snapshot();
        Assert.Equal(TechnologyPurchaseFailure.AlreadyUnlocked, game.BuyTechnology("lavorazioni-1"));
        Assert.Same(after, game.Snapshot());
    }

    [Fact]
    public void SchoolGraduationAndResearchContinueInTheSameV15ClockAfterEnrollment()
    {
        var game = Start();
        SetUpSchool(game);
        Assert.Equal(SchoolActionFailure.None, game.EnrollStudents(1));
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(3), Meat, Crops).Succeeded);
        Assert.Equal(1, game.Snapshot().CityCampaign.SchoolResearch.QualifiedResearchers);
        Assert.Equal(SchoolActionFailure.None, game.AssignResearchers(1));
        Assert.True(game.Advance(TimeSpan.FromSeconds(1), Meat, Crops).Succeeded);
        Assert.Equal(100, game.Snapshot().CityCampaign.ResearchBalanceMilliPoints);
        Assert.Equal(ResourceAmount.Zero, game.Snapshot().CityCampaign.Resources["food"]);
    }
    [Fact]
    public void PencilPaidEnrollmentAndCancellationPreserveBothTypesOfInventory()
    {
        var source = Seed();
        source = source with { CityCampaign = source.CityCampaign with
            { Resources = source.CityCampaign.Resources + ResourceBundle.Create(("coal", 1m)) } };
        var withPencils = new SchoolResearchRules("food", U(2m), 3, 1, 2, 100, pencilsPerStudent: 2);
        var game = new V15FoodIndustryCampaignSession(source,
            CivilGrowthScenario.Create(), withPencils, Technologies, Depot);
        SetUpSchool(game);
        Assert.Equal(SchoolActionFailure.InsufficientPencils, game.EnrollStudents(1));
        Assert.Equal(PencilCraftFailure.None, game.CraftPencils(1));
        Assert.Equal(SchoolActionFailure.None, game.EnrollStudents(1));
        Assert.Equal(U(2m), game.Snapshot().CityCampaign.Resources["pencils"]);
        Assert.Equal(1, game.Snapshot().CityCampaign.PencilPaidStudents);
        Assert.Equal(SchoolActionFailure.None, game.CancelStudents(1));
        Assert.Equal(U(4m), game.Snapshot().CityCampaign.Resources["pencils"]);
        Assert.Equal(source.Depot.ReadyFood, game.Snapshot().Depot.ReadyFood);
        Assert.Equal(0, game.Snapshot().CityCampaign.PencilPaidStudents);
    }

    [Fact]
    public void RecruitmentReportsMissingBarracksBeforePopulationAndCanRefundEquipment()
    {
        var game = Start();
        Assert.Equal(GameError.MissingBarracks, game.Enlist(1).Error);
        game = Start(WithBarracks(Seed()));
        var before = game.Snapshot().CityCampaign.Resources["swords"];
        Assert.True(game.Enlist(1).Succeeded);
        Assert.Equal(before - U(5m), game.Snapshot().CityCampaign.Resources["swords"]);
        Assert.True(game.CancelRecruits(1).Succeeded);
        Assert.Equal(before, game.Snapshot().CityCampaign.Resources["swords"]);
        Assert.Equal(0, game.Snapshot().CityCampaign.Military.EnlistedPopulation);
    }

    [Fact]
    public void InvalidCommandsNeverChangeFoodStocksOrCityCheckpoint()
    {
        var game = Start();
        var before = game.Snapshot();
        Assert.Equal(V14CivicConstructionFailure.InvalidQuantity, game.BuildCityBuilding("warehouse", 0).Failure);
        Assert.Equal(V14TierActionFailure.UnknownFamily, game.BuildCityProducer("food-store", ProductionTier.Base, 1).Failure);
        Assert.Equal(V14SchoolConstructionFailure.InvalidQuantity, game.BuildSchool(0).Failure);
        Assert.Equal(SchoolActionFailure.InvalidQuantity, game.EnrollStudents(0));
        Assert.Equal(SchoolActionFailure.InvalidQuantity, game.CancelStudents(-1));
        Assert.Equal(GameError.InvalidQuantity, game.Enlist(-1).Error);
        Assert.Same(before, game.Snapshot());
    }
}
