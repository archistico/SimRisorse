using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC65: civic transactions execute on v15 state without a historical session.
/// These are contract tests for already accepted prices, workforce and atomicity.</summary>
public sealed class F1D05e2b2bNativeCityCommandTests
{
    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);
    private static readonly SchoolResearchRules School = new("food", U(2m), 3, 1, 2, 100);
    private static readonly TechnologyCatalog Tech = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Policy = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
        new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("bread", U(1m), U(1m), 4)
    ]);

    private static V15FoodIndustryCampaignSession Create(bool pencils = false)
    {
        var scenario = CivilGrowthScenario.Create();
        var rules = pencils ? new SchoolResearchRules("food", U(2m), 3, 1, 2, 100,
            pencilsPerStudent: 2) : School;
        var city = V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), rules, Tech);
        city = city with
        {
            Resources = city.Resources + ResourceBundle.Create(
                ("planks", 500m), ("stone", 500m), ("coal", 5m)),
            ResearchBalanceMilliPoints = ResearchPoints.FromUnits(500m).MilliPoints
        };
        var checkpoint = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, Policy), Policy);
        return new(checkpoint, scenario, rules, Tech, Policy);
    }

    [Fact]
    public void NativeCivicPurchasesDebitOnceAndUseAcquisitionHistory()
    {
        var game = Create();
        var start = game.Snapshot();
        var first = game.BuildCityBuilding("warehouse", 1);
        Assert.True(first.Succeeded);
        var second = game.BuildCityBuilding("warehouse", 1);
        Assert.True(second.Succeeded);
        Assert.True(second.Cost["planks"].Milliunits > first.Cost["planks"].Milliunits);
        Assert.Equal(2, game.Snapshot().CityCampaign.OtherBuildings
            .Single(b => b.BuildingId == "warehouse").Count);
        Assert.Equal(2, game.Snapshot().CityCampaign.PurchaseCounters
            .Single(c => c.FamilyId == "warehouse").Acquired);
        Assert.Equal(start.CityCampaign.Resources - first.Cost - second.Cost,
            game.Snapshot().CityCampaign.Resources);
        Assert.Equal(start.Depot, game.Snapshot().Depot);
        Assert.DoesNotContain(game.Snapshot().CityCampaign.ProductiveFamilies,
            f => f.FamilyId == "food-store");
    }

    [Fact]
    public void InvalidAndRetiredPurchasesNeverChangeCheckpoint()
    {
        var game = Create();
        var start = game.Snapshot();
        Assert.Equal(V14CivicConstructionFailure.UnknownBuilding,
            game.BuildCityBuilding("food-store", 1).Failure);
        Assert.Equal(V14TierActionFailure.UnknownFamily,
            game.BuildCityProducer("food-store", ProductionTier.Base, 1).Failure);
        Assert.Equal(V14TierActionFailure.UnknownFamily,
            game.UpgradeCityProducer("food-store", ProductionTier.Base, ProductionTier.Large).Failure);
        Assert.Equal(V14TierActionFailure.UnknownFamily,
            game.AssignCityProducerWorkers("food-store", ProductionTier.Base, 1));
        Assert.Equal(V14CivicConstructionFailure.FixedBuilding,
            game.BuildCityBuilding("town-hall", 1).Failure);
        Assert.Equal(V14TierActionFailure.InvalidQuantity,
            game.BuildCityProducer("fisher", ProductionTier.Base, 0).Failure);
        Assert.Same(start, game.Snapshot());
    }

    [Fact]
    public void NativeTierBuildUsesSharedWorkforceAndReleaseIsAtomic()
    {
        var game = Create();
        Assert.True(game.BuildCityProducer("fisher", ProductionTier.Base, 2).Succeeded);
        Assert.Equal(V14TierActionFailure.None,
            game.AssignCityProducerWorkers("fisher", ProductionTier.Base, 1));
        var after = game.Snapshot();
        Assert.Equal(V14TierActionFailure.InsufficientWorkplaces,
            game.AssignCityProducerWorkers("fisher", ProductionTier.Base, 2));
        Assert.Same(after, game.Snapshot());
        Assert.Equal(V14TierActionFailure.InsufficientAssignedWorkers,
            game.ReleaseCityProducerWorkers("fisher", ProductionTier.Base, 2));
        Assert.Same(after, game.Snapshot());
        Assert.Equal(V14TierActionFailure.None,
            game.ReleaseCityProducerWorkers("fisher", ProductionTier.Base, 1));
        Assert.Equal(0, game.Snapshot().CityCampaign.ProductiveFamilies
            .Single(f => f.FamilyId == "fisher").Base.AssignedWorkers);
    }

    [Fact]
    public void NativeUpgradeUsesResearchButDoesNotResetPurchaseHistory()
    {
        var game = Create();
        Assert.True(game.BuildCityProducer("fisher", ProductionTier.Base, 1).Succeeded);
        var denied = game.Snapshot();
        Assert.Equal(V14TierActionFailure.MissingResearch,
            game.UpgradeCityProducer("fisher", ProductionTier.Base, ProductionTier.Large).Failure);
        Assert.Same(denied, game.Snapshot());
        Assert.Equal(TechnologyPurchaseFailure.None, game.BuyTechnology("alimentazione-1"));
        var history = game.Snapshot().CityCampaign.PurchaseCounters
            .Single(c => c.FamilyId == "fisher").Acquired;
        Assert.True(game.UpgradeCityProducer("fisher", ProductionTier.Base,
            ProductionTier.Large).Succeeded);
        var family = game.Snapshot().CityCampaign.ProductiveFamilies
            .Single(f => f.FamilyId == "fisher");
        Assert.Equal(0, family.Base.Buildings);
        Assert.Equal(1, family.Large.Buildings);
        Assert.Equal(history, game.Snapshot().CityCampaign.PurchaseCounters
            .Single(c => c.FamilyId == "fisher").Acquired);
    }

    [Fact]
    public void NativeSchoolAndPencilWorkshopPreserveDepotOwnership()
    {
        var game = Create(pencils: true);
        Assert.Equal(V14SchoolConstructionFailure.MissingPrerequisite,
            game.BuildSchool(1).Failure);
        Assert.True(game.BuildCityProducer("fisher", ProductionTier.Base, 1).Succeeded);
        Assert.True(game.Build("food-depot", 1).Succeeded);
        Assert.True(game.BuildSchool(1).Succeeded);
        var before = game.Snapshot();
        Assert.Equal(PencilCraftFailure.None, game.CheckPencilCraft(1));
        Assert.Equal(PencilCraftFailure.None, game.CraftPencils(1));
        Assert.Equal(before.CityCampaign.Resources - V14SchoolSupplies.Inputs(1) +
            V14SchoolSupplies.Output(1), game.Snapshot().CityCampaign.Resources);
        Assert.Equal(before.Depot, game.Snapshot().Depot);
        Assert.Equal(ResourceAmount.Zero, game.Snapshot().CityCampaign.Resources["food"]);
    }

    [Fact]
    public void NativeTechnologyPurchaseDoesNotSpendPointsOnSecondAttempt()
    {
        var game = Create();
        Assert.Equal(TechnologyPurchaseFailure.None, game.BuyTechnology("lavorazioni-1"));
        var after = game.Snapshot();
        Assert.Equal(TechnologyPurchaseFailure.AlreadyUnlocked, game.BuyTechnology("lavorazioni-1"));
        Assert.Same(after, game.Snapshot());
        Assert.Contains("lavorazioni-1", after.CityCampaign.UnlockedTechnologyIds);
        Assert.Equal(ResourceAmount.Zero, after.CityCampaign.Resources["food"]);
    }
}
