using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC90: Alimentazione research unlocks real v15 food-depot tiers.</summary>
public sealed class Rc90FoodDepotTiersTests
{
    private static ResourceAmount U(decimal value) => ResourceAmount.FromUnits(value);

    private static (V15BenchmarkFixture Fixture, V15FoodIndustryCampaignSession Session) Setup()
    {
        var fixture = V15BenchmarkFixture.Create();
        var city = fixture.Start.CityCampaign;
        var enriched = fixture.Start with
        {
            CityCampaign = city with
            {
                // The fixed Municipio stores 1,000 units per city resource.
                // Supply enough for one Grande plus one Avanzato depot without
                // inventing an over-capacity starting inventory.
                Resources = city.Resources + ResourceBundle.Create(("planks", 800m), ("stone", 450m)),
                ResearchBalanceMilliPoints = ResearchPoints.FromUnits(1000m).MilliPoints,
                ProductiveFamilies = [.. city.ProductiveFamilies.Select(f => f.FamilyId == "fisher"
                    ? f with { Base = f.Base with { Buildings = 1 } } : f)],
                PurchaseCounters = [.. city.PurchaseCounters.Select(c => c.FamilyId == "fisher"
                    ? c with { Acquired = 1 } : c)]
            }
        };
        Assert.True(V15CityStorage.Calculate(fixture.Baseline, enriched.CityCampaign)
            .Covers(enriched.CityCampaign.Resources));
        return (fixture, new(enriched, fixture.Baseline, fixture.School, fixture.Technologies,
            fixture.Depot));
    }

    [Fact]
    public void TwoDepotTiersAreVisibleButLockedBeforeResearch()
    {
        var (fixture, session) = Setup();
        var view = session.CityCatalog;
        Assert.Contains(view.Buildings, b => b.Id == "food-depot-large" &&
            b.Tier == ProductionTier.Large && b.Name == "Deposito alimentare grande");
        Assert.Contains(view.Buildings, b => b.Id == "food-depot-advanced" &&
            b.Tier == ProductionTier.Advanced && b.Name == "Deposito alimentare avanzato");
        Assert.DoesNotContain(view.Buildings, b => b.Id == "food-store");
        Assert.Equal(FoodIndustryActionFailure.MissingResearch,
            session.CheckBuild("food-depot-large", 1).Failure);
        Assert.Equal(FoodIndustryActionFailure.MissingResearch,
            session.CheckBuild("food-depot-advanced", 1).Failure);
        Assert.Equal(6, session.Catalog.Sites.Length); // no second legacy depot or duplicate recipe
    }

    [Fact]
    public void AlimentazioneIUnlocksGrandeButNotAvanzato()
    {
        var (_, session) = Setup();
        Assert.Equal(TechnologyPurchaseFailure.None, session.BuyTechnology("alimentazione-1"));
        Assert.True(session.CheckBuild("food-depot-large", 1).Succeeded);
        Assert.Equal(FoodIndustryActionFailure.MissingResearch,
            session.CheckBuild("food-depot-advanced", 1).Failure);
    }

    [Fact]
    public void AlimentazioneIIUnlocksAdvancedAndRequiresPreviousResearch()
    {
        var (_, session) = Setup();
        Assert.Equal(TechnologyPurchaseFailure.MissingPrerequisite,
            session.BuyTechnology("alimentazione-2"));
        Assert.Equal(TechnologyPurchaseFailure.None, session.BuyTechnology("alimentazione-1"));
        Assert.Equal(TechnologyPurchaseFailure.None, session.BuyTechnology("alimentazione-2"));
        Assert.True(session.CheckBuild("food-depot-advanced", 1).Succeeded);
    }

    [Fact]
    public void DifferentTierPurchasesChargeMaterialsAndIncreaseActualStorage()
    {
        var (_, session) = Setup();
        Assert.Equal(TechnologyPurchaseFailure.None, session.BuyTechnology("alimentazione-1"));
        Assert.Equal(TechnologyPurchaseFailure.None, session.BuyTechnology("alimentazione-2"));
        var initial = session.Snapshot().CityCampaign.Resources;
        var largeQuote = session.CheckBuild("food-depot-large", 1);
        Assert.True(largeQuote.Succeeded);
        Assert.True(session.Build("food-depot-large", 1).Succeeded);
        Assert.Equal(initial - largeQuote.Cost, session.Snapshot().CityCampaign.Resources);
        Assert.Equal(U(300m), session.Capacities.DepotReady);
        Assert.Equal(U(150m), session.Capacities.DepotRaw["fish"]);
        var advancedQuote = session.CheckBuild("food-depot-advanced", 1);
        Assert.True(advancedQuote.Succeeded);
        Assert.True(session.Build("food-depot-advanced", 1).Succeeded);
        Assert.Equal(initial - largeQuote.Cost - advancedQuote.Cost, session.Snapshot().CityCampaign.Resources);
        Assert.Equal(U(1300m), session.Capacities.DepotReady);
        Assert.Equal(U(650m), session.Capacities.DepotRaw["bread"]);
        Assert.Equal(1, session.Snapshot().Industry.Buildings.Counts.DepotLarge);
        Assert.Equal(1, session.Snapshot().Industry.Buildings.Counts.DepotAdvanced);
    }

    [Fact]
    public void EachTierOwnsItsWorkersAndNeverBorrowsUnusedVacanciesFromAnotherTier()
    {
        var (_, session) = Setup();
        Assert.Equal(TechnologyPurchaseFailure.None, session.BuyTechnology("alimentazione-1"));
        Assert.True(session.Build("food-depot-large", 1).Succeeded);
        Assert.Equal(FoodIndustryActionFailure.InsufficientWorkplaces,
            session.AssignWorkers("food-depot", 1));
        Assert.Equal(FoodIndustryActionFailure.InsufficientWorkplaces,
            session.AssignWorkers("food-depot-large", 4));
        Assert.Equal(FoodIndustryActionFailure.None,
            session.AssignWorkers("food-depot-large", 3));
        Assert.Equal(3L, session.Snapshot().Industry.Buildings.Assigned.DepotLarge);
        Assert.Equal(0L, session.Snapshot().Industry.Buildings.Assigned.Depot);
        Assert.Equal(FoodIndustryActionFailure.None,
            session.ReleaseWorkers("food-depot-large", 2));
        Assert.Equal(1L, session.Snapshot().Industry.Buildings.Assigned.DepotLarge);
        Assert.Equal(FoodIndustryActionFailure.InsufficientAssignedWorkers,
            session.ReleaseWorkers("food-depot-large", 2));
    }

    [Fact]
    public void AStaffedGrandeDepotParticipatesInTheRealFoodClock()
    {
        var (fixture, session) = Setup();
        Assert.Equal(TechnologyPurchaseFailure.None, session.BuyTechnology("alimentazione-1"));
        Assert.True(session.Build("food-depot-large", 1).Succeeded);
        Assert.Equal(FoodIndustryActionFailure.None, session.AssignWorkers("food-depot-large", 3));
        var current = session.Snapshot();
        var stocked = current with { Depot = current.Depot with
            { RawFood = ResourceBundle.Create(("fish", 5m)) } };
        var live = new V15FoodIndustryCampaignSession(stocked, fixture.Baseline,
            fixture.School, fixture.Technologies, fixture.Depot);
        var step = live.StepFood(fixture.Meat, fixture.Crops,
            new FoodIndustryMeals(U(0m), U(0m), U(0m)));
        Assert.True(step.Next.Depot.ReadyFood.Milliunits > 0);
        Assert.Equal(3L, live.Snapshot().Industry.Buildings.Assigned.DepotLarge);
    }

    [Fact]
    public void InvalidTierStaffingIsRejectedAtCheckpointValidation()
    {
        var (fixture, session) = Setup();
        var current = session.Snapshot();
        var invalid = current with { Industry = current.Industry with
        {
            Buildings = current.Industry.Buildings with
            { Assigned = current.Industry.Buildings.Assigned with { DepotLarge = 1 } }
        } };
        Assert.Throws<ArgumentException>(() => V15FoodCampaignCheckpoints.Validate(
            invalid, fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot));
    }

    [Fact]
    public void FoodDepotBaseKeepsItsOriginalPricingAndCapacity()
    {
        var (_, session) = Setup();
        Assert.Equal(U(60m), session.CheckBuild("food-depot", 1).Cost["planks"]);
        Assert.True(session.Build("food-depot", 1).Succeeded);
        Assert.Equal(U(100m), session.Capacities.DepotReady);
        Assert.Equal(0L, session.Snapshot().Industry.Buildings.Counts.DepotLarge);
        Assert.Equal(0L, session.Snapshot().Industry.Buildings.Counts.DepotAdvanced);
    }
}
