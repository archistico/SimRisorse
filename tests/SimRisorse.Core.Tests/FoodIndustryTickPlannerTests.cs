using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Core.Tests;

/// <summary>RC48 F1-D05d/1: end-to-end food stages, tested in isolation.
/// The numbers are test fixtures, not approved game balancing values.</summary>
public sealed class FoodIndustryTickPlannerTests
{
    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);
    private static readonly MeatChainPolicy Meat = new("grain", "water", "cattle", "game", "meat",
        ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(1m), U(1m),
        [new("cattle", U(1m), U(2m), 0), new("game", U(1m), U(1m), 1)]);
    private static readonly HorticulturePolicy Crops = new("water", "vegetables", "fruit",
        new("vegetables", U(2m), U(1m), 0), new("fruit", U(3m), U(1m), 1));
    private static readonly FoodConversionPolicy DepotPolicy = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
        new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("bread", U(1m), U(1m), 4)
    ]);
    private static readonly FoodIndustryCapacities Capacities = new(
        ResourceBundle.Create(("cattle", 50m), ("game", 50m)),
        ResourceBundle.Create(("meat", 50m), ("fruit", 50m), ("vegetables", 50m)),
        ResourceBundle.Create(("fish", 50m), ("bread", 50m), ("meat", 50m),
            ("fruit", 50m), ("vegetables", 50m)), U(50m));
    private static readonly FoodIndustryMeals NoMeals = new(U(0m), U(0m), U(0m));
    private static FoodIndustryStocks Stock(ResourceBundle? city = null,
        ResourceBundle? animals = null, ResourceBundle? transit = null,
        FoodDepotStock? depot = null) => new(city ?? ResourceBundle.Empty,
        animals ?? ResourceBundle.Empty, transit ?? ResourceBundle.Empty,
        depot ?? new(ResourceBundle.Empty, U(0m)));
    private static FoodIndustryCrew Crew(long stable = 0, long hunter = 0, long butcher = 0,
        long garden = 0, long orchard = 0, long depot = 0) =>
        new(stable, hunter, butcher, garden, orchard, depot);
    private static FoodIndustryPersonnel Personnel(FoodIndustryCrew workers, long population = 20,
        long outside = 0, FoodIndustryCrew? workplaces = null) =>
        new(workers, workplaces ?? Crew(20, 20, 20, 20, 20, 20), population, outside);
    private static FoodIndustryTickResult Tick(FoodIndustryStocks stocks, FoodIndustryCrew workers,
        FoodIndustryCapacities? capacities = null, FoodIndustryMeals? meals = null,
        long population = 20, long outside = 0, FoodIndustryCrew? workplaces = null) =>
        FoodIndustryTickPlanner.PlanTick(Meat, Crops, DepotPolicy, stocks,
            capacities ?? Capacities, Personnel(workers, population, outside, workplaces),
            meals ?? NoMeals);

    [Fact]
    public void AllFiveNewIndustriesReachTheDepotWithCorrectIntermediateDelay()
    {
        var initial = Stock(ResourceBundle.Create(("grain", 3m), ("water", 4m), ("fish", 1m)));
        var workers = Crew(1, 1, 2, 1, 1, 10);
        var first = Tick(initial, workers);
        Assert.Equal(1m, first.Next.Animals["cattle"].Units);
        Assert.Equal(1m, first.Next.Animals["game"].Units);
        Assert.Equal(0m, first.Meat.Produced["meat"].Units); // no freshly bred animal is butchered
        Assert.Equal(2m, first.Next.City["grain"].Units); // one stable batch
        Assert.Equal(6m, first.Next.Depot.ReadyFood.Units); // fish (1) + vegetables (2) + fruit (3)
        Assert.Equal(3m, first.Horticulture.Produced["fruit"].Units); // orchard has its own stage
        Assert.Equal(ResourceBundle.Empty, initial.Animals); // no in-place mutation

        var second = Tick(first.Next, workers);
        Assert.Equal(3m, second.Meat.Produced["meat"].Units);
        Assert.True(second.Depot.Processing.ConsumedIngredients["meat"].Units > 0m);
        Assert.True(second.Next.Depot.ReadyFood.Units > first.Next.Depot.ReadyFood.Units);
    }

    [Fact]
    public void AnimalsWithoutButcherDoNotBecomeMeatOrReadyFood()
    {
        var state = Stock(ResourceBundle.Create(("grain", 1m), ("water", 1m)));
        var first = Tick(state, Crew(stable: 1, hunter: 1, depot: 3));
        Assert.Equal(0m, first.Next.Depot.ReadyFood.Units);
        Assert.Equal(1m, first.Next.Animals["cattle"].Units);
        Assert.Equal(1m, first.Next.Animals["game"].Units);
        var second = Tick(first.Next, Crew(depot: 3));
        Assert.Equal(0m, second.Next.Depot.ReadyFood.Units);
        Assert.Equal(first.Next.Animals, second.Next.Animals);
    }

    [Fact]
    public void MeatAndCropsCannotBypassAnUnstaffedDepot()
    {
        var state = Stock(ResourceBundle.Create(("water", 2m)),
            ResourceBundle.Create(("cattle", 1m)));
        var result = Tick(state, Crew(butcher: 1, garden: 1, orchard: 1));
        Assert.Equal(0m, result.Next.Depot.ReadyFood.Units);
        Assert.Equal(2m, result.Next.Depot.RawFood["meat"].Units);
        Assert.Equal(2m, result.Next.Depot.RawFood["vegetables"].Units);
        Assert.Equal(3m, result.Next.Depot.RawFood["fruit"].Units);
    }

    [Fact]
    public void WaterIsConsumedOnceAndStableHasPriorityOverCrops()
    {
        var state = Stock(ResourceBundle.Create(("grain", 3m), ("water", 1m)));
        var result = Tick(state, Crew(stable: 2, garden: 2, orchard: 2));
        Assert.Equal(0m, result.Next.City["water"].Units);
        Assert.Equal(2m, result.Next.City["grain"].Units);
        Assert.Equal(1m, result.Next.Animals["cattle"].Units);
        Assert.Equal(0m, result.Next.Depot.RawFood["vegetables"].Units);
        Assert.Equal(0m, result.Next.Depot.RawFood["fruit"].Units);
    }

    [Fact]
    public void RawDepotIsBoundedAndUnsoldTransitIsNotDiscarded()
    {
        var cap = Capacities with { DepotRaw = ResourceBundle.Create(("meat", 1m)) };
        var state = Stock(transit: ResourceBundle.Create(("meat", 3m), ("fruit", 2m)));
        var result = Tick(state, Crew(), cap);
        Assert.Equal(1m, result.Next.Depot.RawFood["meat"].Units);
        Assert.Equal(2m, result.Next.Transit["meat"].Units);
        Assert.Equal(2m, result.Next.Transit["fruit"].Units);
    }

    [Fact]
    public void HistoricalOvercapacitySurvivesAndBlocksFurtherDeliveries()
    {
        var cap = Capacities with { DepotRaw = ResourceBundle.Create(("fish", 1m)) };
        var state = Stock(ResourceBundle.Create(("fish", 2m)),
            depot: new(ResourceBundle.Create(("fish", 3m)), U(0m)));
        var result = Tick(state, Crew(), cap);
        Assert.Equal(3m, result.Next.Depot.RawFood["fish"].Units);
        Assert.Equal(2m, result.Next.City["fish"].Units);
    }

    [Fact]
    public void AdvancedBreadAndReadyFoodAreDeductedAtomically()
    {
        var state = Stock(depot: new(ResourceBundle.Create(("bread", 2m)), U(4m)));
        var result = Tick(state, Crew(depot: 1), meals: new(U(2m), U(2m), U(1m)));
        Assert.True(result.Depot.Meal.Served);
        Assert.Equal(2m, result.Next.Depot.ReadyFood.Units);
        Assert.Equal(1m, result.Next.Depot.RawFood["bread"].Units);
        Assert.Equal(0m, result.Depot.Processing.ConsumedIngredients["bread"].Units);
        var denied = Tick(result.Next, Crew(), meals: new(U(0m), U(3m), U(2m)));
        Assert.False(denied.Depot.Meal.Served);
        Assert.Equal(result.Next.Depot, denied.Next.Depot);
    }

    [Fact]
    public void WorkersCannotExceedFreePopulationOrBuildingWorkplaces()
    {
        var input = Stock();
        Assert.Throws<ArgumentException>(() => Tick(input, Crew(depot: 2),
            population: 10, outside: 9));
        Assert.Throws<ArgumentException>(() => Tick(input, Crew(stable: 2),
            workplaces: Crew(stable: 1)));
        Assert.Throws<ArgumentException>(() => Tick(input, Crew(hunter: 1),
            population: 5, outside: 6));
        Assert.Throws<ArgumentOutOfRangeException>(() => Tick(input, Crew(depot: -1)));
    }

    [Fact]
    public void DuplicateOrMisownedFoodIsRejectedBeforeTick()
    {
        Assert.Throws<ArgumentException>(() => Tick(Stock(ResourceBundle.Create(("food", 1m))), Crew()));
        Assert.Throws<ArgumentException>(() => Tick(Stock(ResourceBundle.Create(("meat", 1m))), Crew()));
        Assert.Throws<ArgumentException>(() => Tick(Stock(animals: ResourceBundle.Create(("fruit", 1m))), Crew()));
        Assert.Throws<ArgumentException>(() => Tick(Stock(transit: ResourceBundle.Create(("game", 1m))), Crew()));
    }

    [Fact]
    public void StorageOfNewIntermediateResourcesNeverEntersLegacyCity()
    {
        var state = Stock(ResourceBundle.Create(("water", 2m), ("stone", 99m)),
            animals: ResourceBundle.Create(("game", 1m)));
        var result = Tick(state, Crew(butcher: 1, garden: 1, orchard: 1, depot: 0));
        Assert.Equal(99m, result.Next.City["stone"].Units);
        Assert.Equal(0m, result.Next.City["meat"].Units);
        Assert.Equal(0m, result.Next.City["fruit"].Units);
        Assert.Equal(0m, result.Next.City["vegetables"].Units);
        Assert.Equal(1m, result.Next.Depot.RawFood["meat"].Units);
    }

    [Fact]
    public void PriorityAndResultsDoNotDependOnResourceBundleInsertionOrder()
    {
        var a = Stock(ResourceBundle.Create(("grain", 1m), ("water", 3m), ("fish", 2m)));
        var b = Stock(ResourceBundle.Create(("fish", 2m), ("water", 3m), ("grain", 1m)));
        var crew = Crew(stable: 1, garden: 1, orchard: 1, depot: 3);
        Assert.Equal(Tick(a, crew).Next, Tick(b, crew).Next);
    }
}
