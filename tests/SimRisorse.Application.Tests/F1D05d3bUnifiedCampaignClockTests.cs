using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Military;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC51 regression guards: one owner of ready food, one clock, atomic saves.</summary>
public sealed class F1D05d3bUnifiedCampaignClockTests
{
    private static ResourceAmount U(decimal n) => ResourceAmount.FromUnits(n);
    private static readonly SchoolResearchRules School = new("food", U(10m), 4, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
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
        int depots = 0, int depotWorkers = 0)
    {
        var baseline = CivilGrowthScenario.Create();
        var city = V14CampaignSnapshots.MigrateFromV13(baseline,
            GameSession.Create(baseline).Snapshot(), School, Technologies);
        if (fisher || oldFoodStore)
        {
            city = city with
            {
                ProductiveFamilies = [.. city.ProductiveFamilies.Select(f =>
                    (f.FamilyId == "fisher" && fisher ||
                     f.FamilyId == "food-store" && oldFoodStore)
                        ? f with { Base = new TierOccupancy(1, 1) } : f)],
                PurchaseCounters = [.. city.PurchaseCounters.Select(c =>
                    (c.FamilyId == "fisher" && fisher ||
                     c.FamilyId == "food-store" && oldFoodStore)
                        ? c with { Acquired = 1 } : c)]
            };
        }
        var migrated = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, Depot), Depot);
        return migrated with
        {
            Depot = new FoodDepotStock(ResourceBundle.Empty, ResourceAmount.Zero),
            Industry = migrated.Industry with
            {
                Buildings = new FoodIndustryBuildings(new FoodIndustryCrew(0, 0, 0, 0, 0, depots),
                    new FoodIndustryCrew(0, 0, 0, 0, 0, depotWorkers))
            }
        };
    }

    private static V15FoodIndustryCampaignSession Create(V15FoodCampaignCheckpoint state) =>
        new(state, CivilGrowthScenario.Create(), School, Technologies, Depot);

    private static V15FoodCampaignCheckpoint After(V15FoodCampaignCheckpoint current, TimeSpan elapsed) =>
        V15UnifiedCampaignClock.Plan(current, CivilGrowthScenario.Create(), School, Technologies,
            FoodIndustryConstructionCatalog.Candidate, Meat, Crops, Depot, elapsed).Next;

    [Fact]
    public void ReadyFoodIsNeverStoredInCityOrProducedByLegacyFoodStore()
    {
        var original = Seed(oldFoodStore: true);
        var initial = original with
        {
            CityCampaign = original.CityCampaign with
            {
                Resources = ResourceBundle.Create(("fish", 5m))
            }
        };
        var next = After(initial, TimeSpan.FromSeconds(1));
        Assert.Equal(U(5m), next.CityCampaign.Resources["fish"]);
        Assert.Equal(ResourceAmount.Zero, next.CityCampaign.Resources["food"]);
        Assert.Equal(ResourceAmount.Zero, next.Depot.ReadyFood);
        Assert.Equal(1, next.CityCampaign.ProductiveFamilies.Single(f => f.FamilyId == "food-store").Base.AssignedWorkers);
    }

    [Fact]
    public void FisherFeedsOnlyNewDepotAndRationsAreDeductedOnce()
    {
        var initial = Seed(fisher: true, oldFoodStore: true, depots: 1, depotWorkers: 1);
        var result = V15UnifiedCampaignClock.Plan(initial, CivilGrowthScenario.Create(),
            School, Technologies, FoodIndustryConstructionCatalog.Candidate,
            Meat, Crops, Depot, TimeSpan.FromSeconds(2));
        Assert.Equal(2, result.SecondsAdvanced);
        Assert.Equal(2, result.MealsServed);
        Assert.Equal(U(2m), result.ReadyFoodProduced);
        Assert.Equal(U(1.840m), result.Next.Depot.ReadyFood);
        Assert.Equal(ResourceAmount.Zero, result.Next.CityCampaign.Resources["food"]);
        Assert.Equal(ResourceAmount.Zero, result.Next.CityCampaign.Resources["fish"]);
        Assert.Equal(ResourceAmount.Zero, result.Next.Depot.RawFood["fish"]);
    }

    [Fact]
    public void WithoutNewDepotWorkerNoNewReadyFoodIsCreated()
    {
        var start = Seed(fisher: true, oldFoodStore: true, depots: 1);
        var end = After(start, TimeSpan.FromSeconds(2));
        Assert.Equal(ResourceAmount.Zero, end.Depot.ReadyFood);
        Assert.Equal(U(2m), end.Depot.RawFood["fish"]);
        Assert.Equal(ResourceAmount.Zero, end.CityCampaign.Resources["food"]);
    }

    [Fact]
    public void AdvancedResidentsConsumeBothRationsAtomically()
    {
        var state = Seed();
        var city = state.CityCampaign with
        {
            Population = 9,
            OtherBuildings = [.. state.CityCampaign.OtherBuildings.Select(b => b.BuildingId == "large-house"
                ? b with { Count = 1 } : b)],
            PurchaseCounters = [.. state.CityCampaign.PurchaseCounters.Select(c => c.FamilyId == "large-house"
                ? c with { Acquired = 1 } : c)]
        };
        var withBread = state with
        {
            CityCampaign = city,
            Depot = new FoodDepotStock(ResourceBundle.Create(("bread", 1m)), U(1m))
        };
        var fed = After(withBread, TimeSpan.FromSeconds(1));
        Assert.Equal(U(0.910m), fed.Depot.ReadyFood);
        Assert.Equal(U(0.985m), fed.Depot.RawFood["bread"]);
        Assert.Equal(ResourceAmount.Zero, fed.CityCampaign.Resources["food"]);
        Assert.Equal(ResourceAmount.Zero, fed.CityCampaign.Resources["bread"]);
    }

    [Fact]
    public void MissingBreadNeverConsumesOnlyReadyFood()
    {
        var state = Seed();
        var city = state.CityCampaign with
        {
            Population = 9,
            OtherBuildings = [.. state.CityCampaign.OtherBuildings.Select(b => b.BuildingId == "large-house"
                ? b with { Count = 1 } : b)],
            PurchaseCounters = [.. state.CityCampaign.PurchaseCounters.Select(c => c.FamilyId == "large-house"
                ? c with { Acquired = 1 } : c)]
        };
        state = state with { CityCampaign = city,
            Depot = new FoodDepotStock(ResourceBundle.Empty, U(1m)) };
        var result = V15UnifiedCampaignClock.Plan(state, CivilGrowthScenario.Create(),
            School, Technologies, FoodIndustryConstructionCatalog.Candidate,
            Meat, Crops, Depot, TimeSpan.FromSeconds(1));
        Assert.Equal(0, result.MealsServed);
        Assert.Equal(1, result.MealsMissed);
        Assert.Equal(U(1m), result.Next.Depot.ReadyFood);
        Assert.Equal(0, result.Next.CityCampaign.GrowthProgressSeconds);
    }

    [Fact]
    public void MissingReadyFoodNeverConsumesOnlyBread()
    {
        var start = Seed() with
        {
            Depot = new FoodDepotStock(ResourceBundle.Create(("bread", 2m)), ResourceAmount.Zero)
        };
        var end = After(start, TimeSpan.FromSeconds(1));
        Assert.Equal(U(2m), end.Depot.RawFood["bread"]);
        Assert.Equal(ResourceAmount.Zero, end.Depot.ReadyFood);
    }

    [Fact]
    public void ContinuousAndSplitAdvanceHaveEquivalentFullCheckpoint()
    {
        var initial = Seed(fisher: true, depots: 1, depotWorkers: 1);
        var continuous = After(initial, TimeSpan.FromSeconds(5.5));
        var split = After(After(After(initial, TimeSpan.FromMilliseconds(500)),
            TimeSpan.FromSeconds(2)), TimeSpan.FromSeconds(3));
        Assert.Equal(continuous.CityCampaign.Resources, split.CityCampaign.Resources);
        Assert.Equal(continuous.CityCampaign.ElapsedTicks, split.CityCampaign.ElapsedTicks);
        Assert.Equal(continuous.CityCampaign.Population, split.CityCampaign.Population);
        Assert.Equal(continuous.CityCampaign.GrowthProgressSeconds, split.CityCampaign.GrowthProgressSeconds);
        Assert.Equal(continuous.Depot, split.Depot);
        Assert.Equal(continuous.Industry, split.Industry);
        Assert.Equal(continuous.CityCampaign.Military, split.CityCampaign.Military);
        Assert.Equal(continuous.CityCampaign.SchoolResearch, split.CityCampaign.SchoolResearch);
        Assert.Equal(continuous.CityCampaign.ResearchBalanceMilliPoints, split.CityCampaign.ResearchBalanceMilliPoints);
    }

    [Fact]
    public void FractionalAdvanceDoesNotProduceBeforeOneFullSecond()
    {
        var start = Seed(fisher: true, depots: 1, depotWorkers: 1);
        var partial = After(start, TimeSpan.FromMilliseconds(900));
        Assert.Equal(start.Depot, partial.Depot);
        Assert.Equal(start.CityCampaign.Resources, partial.CityCampaign.Resources);
        Assert.Equal(TimeSpan.FromMilliseconds(900).Ticks, partial.CityCampaign.ElapsedTicks);
        var complete = After(partial, TimeSpan.FromMilliseconds(100));
        Assert.Equal(U(0.920m), complete.Depot.ReadyFood);
    }

    [Fact]
    public void ResearchAndSchoolTrainingAdvanceInSameClockAsFood()
    {
        var start = Seed(fisher: true, depots: 1, depotWorkers: 1);
        start = start with { CityCampaign = start.CityCampaign with
        {
            SchoolCount = 1,
            SchoolResearch = new SchoolResearchState(0, [0], 1, 1),
            PurchaseCounters = [.. start.CityCampaign.PurchaseCounters.Select(c => c.FamilyId == "school"
                ? c with { Acquired = 1 } : c)]
        } };
        var end = After(start, TimeSpan.FromSeconds(4));
        Assert.Equal(2, end.CityCampaign.SchoolResearch.QualifiedResearchers);
        Assert.Equal(1, end.CityCampaign.SchoolResearch.ActiveResearchers);
        Assert.Equal(400L, end.CityCampaign.ResearchBalanceMilliPoints);
        Assert.Equal(U(3.680m), end.Depot.ReadyFood);
    }

    [Fact]
    public void SessionIsPausedOnCreationAndRequiresResume()
    {
        var session = Create(Seed(fisher: true, depots: 1, depotWorkers: 1));
        var initial = session.Snapshot();
        Assert.True(session.IsPaused);
        Assert.True(session.Advance(TimeSpan.FromSeconds(2), Meat, Crops).Succeeded);
        Assert.Same(initial, session.Snapshot());
        session.Resume();
        Assert.True(session.Advance(TimeSpan.FromSeconds(2), Meat, Crops).Succeeded);
        Assert.Equal(TimeSpan.FromSeconds(2).Ticks, session.Snapshot().CityCampaign.ElapsedTicks);
        session.Pause();
        var stopped = session.Snapshot();
        Assert.True(session.Advance(TimeSpan.FromSeconds(1), Meat, Crops).Succeeded);
        Assert.Same(stopped, session.Snapshot());
    }

    [Fact]
    public void NegativeOrTooLongElapsedAreRejectedWithoutMutation()
    {
        var session = Create(Seed());
        session.Resume();
        var previous = session.Snapshot();
        Assert.Equal(GameError.InvalidElapsedTime,
            session.Advance(TimeSpan.FromSeconds(-1), Meat, Crops).Error);
        Assert.Equal(GameError.AdvanceLimitExceeded,
            session.Advance(GameSession.MaximumAdvance + TimeSpan.FromSeconds(1), Meat, Crops).Error);
        Assert.Same(previous, session.Snapshot());
    }

    [Fact]
    public void InvalidFoodPolicyCannotPartiallyCommitEvenAfterLegacyProduction()
    {
        var session = Create(Seed(fisher: true, depots: 1, depotWorkers: 1));
        session.Resume();
        var previous = session.Snapshot();
        var incorrect = new MeatChainPolicy("flour", "water", "cattle", "game", "meat",
            ResourceBundle.Create(("flour", 1m), ("water", 1m)), U(1m), U(1m),
            [new("cattle", U(1m), U(2m), 0), new("game", U(1m), U(1m), 1)]);
        Assert.Throws<ArgumentException>(() => session.Advance(TimeSpan.FromSeconds(1), incorrect, Crops));
        Assert.Same(previous, session.Snapshot());
    }

    [Fact]
    public void PopulationGrowthUsesDepotRationsAndHousingConstraint()
    {
        var state = Seed();
        state = state with
        {
            CityCampaign = state.CityCampaign with
            {
                OtherBuildings = [.. state.CityCampaign.OtherBuildings.Select(b => b.BuildingId == "small-house"
                    ? b with { Count = 2 } : b)],
                PurchaseCounters = [.. state.CityCampaign.PurchaseCounters.Select(c => c.FamilyId == "small-house"
                    ? c with { Acquired = 2 } : c)]
            },
            Depot = new FoodDepotStock(ResourceBundle.Empty, U(20m))
        };
        // A second small house increases the growth rate (23/20); enough food is available.
        var next = After(state, TimeSpan.FromSeconds(60));
        Assert.Equal(9, next.CityCampaign.Population);
        Assert.True(next.Depot.ReadyFood.Milliunits < U(20m).Milliunits);
        Assert.Equal(ResourceAmount.Zero, next.CityCampaign.Resources["food"]);
    }

    [Fact]
    public void IndividualMilitaryTrainingAdvancesEvenDuringFoodShortage()
    {
        var start = Seed();
        start = start with { CityCampaign = start.CityCampaign with
        {
            OtherBuildings = [.. start.CityCampaign.OtherBuildings.Select(b => b.BuildingId == "barracks"
                ? b with { Count = 1 } : b)],
            PurchaseCounters = [.. start.CityCampaign.PurchaseCounters.Select(c => c.FamilyId == "barracks"
                ? c with { Acquired = 1 } : c)],
            Military = new MilitaryTrainingState(0, 1, 0, 29, [29])
        } };
        var end = After(start, TimeSpan.FromSeconds(1));
        Assert.Equal(1, end.CityCampaign.Military.AvailableSoldiers);
        Assert.Equal(0, end.CityCampaign.Military.TrainingRecruits);
        Assert.Equal(ResourceAmount.Zero, end.Depot.ReadyFood);
        Assert.Equal(start.CityCampaign.Population, end.CityCampaign.Population);
    }

}
