using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class UnifiedFoodSupplyTests
{
    private static FoodConversionRecipe Recipe(string id, int priority = 0, decimal output = 1m) =>
        new(id, ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(output), priority);

    private static FoodConversionPolicy Policy(params FoodConversionRecipe[] recipes) =>
        UnifiedFoodInputs.CreatePolicy(recipes.Length > 0 ? recipes :
        [Recipe("fish", 0), Recipe("bread", 4), Recipe("meat", 1),
         Recipe("fruit", 2), Recipe("vegetables", 3)]);

    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);

    private static ResourceBundle Capacity(decimal each = 100m) => ResourceBundle.Create(
        ("fish", each), ("bread", each), ("meat", each),
        ("fruit", each), ("vegetables", each));

    private static UnifiedFoodTickResult Tick(FoodConversionPolicy policy,
        ResourceBundle city, FoodDepotStock depot, ResourceBundle? capacity = null,
        long workers = 5, decimal readyCapacity = 100m, decimal breadReserve = 0m,
        decimal foodRation = 0m, decimal breadRation = 0m) =>
        UnifiedFoodSupply.PlanTick(policy, city, depot, capacity ?? Capacity(), U(readyCapacity),
            workers, U(breadReserve), U(foodRation), U(breadRation));

    [Fact]
    public void FiveFoodPolicyRequiresEachExactInputAndRejectsIntermediates()
    {
        Assert.Equal(new[] { "fish", "meat", "fruit", "vegetables", "bread" },
            Policy().Recipes.Select(recipe => recipe.SourceResourceId));
        Assert.Throws<ArgumentException>(() => Policy(Recipe("fish"), Recipe("bread"),
            Recipe("meat"), Recipe("fruit"), Recipe("cattle")));
        Assert.Throws<ArgumentException>(() => Policy(Recipe("fish"), Recipe("bread"),
            Recipe("meat"), Recipe("fruit")));
    }

    [Fact]
    public void AllFiveProductsMustBeTransferredAndProcessedBeforeBecomingFood()
    {
        var city = ResourceBundle.Create(("fish", 1m), ("bread", 1m), ("meat", 1m),
            ("fruit", 1m), ("vegetables", 1m), ("stone", 7m));
        var original = new FoodDepotStock(ResourceBundle.Empty, default);
        var result = Tick(Policy(), city, original);
        Assert.Equal(5L, result.Processing.BatchesProcessed);
        Assert.Equal(5m, result.NextDepot.ReadyFood.Units);
        Assert.Equal(5m, result.DeliveredToDepot.Amounts.Sum(item => item.Value.Units));
        Assert.Equal(0m, result.NextCityResources["fish"].Units);
        Assert.Equal(0m, result.NextCityResources["meat"].Units);
        Assert.Equal(0m, result.NextCityResources["fruit"].Units);
        Assert.Equal(0m, result.NextCityResources["vegetables"].Units);
        Assert.Equal(0m, result.NextCityResources["bread"].Units);
        Assert.Equal(7m, result.NextCityResources["stone"].Units);
        Assert.Equal(ResourceBundle.Empty, result.NextDepot.RawFood);
        Assert.Equal(1m, city["fish"].Units); // The original state remains immutable.
    }

    [Fact]
    public void BreadReservedForAdvancedHousingIsNotConvertedAndBothMealsArePaid()
    {
        var policy = Policy(Recipe("bread", priority: 0, output: 2m), Recipe("fish", priority: 1),
            Recipe("meat", priority: 2), Recipe("fruit", priority: 3), Recipe("vegetables", priority: 4));
        var result = Tick(policy, ResourceBundle.Create(("bread", 3m), ("fish", 2m)),
            new(ResourceBundle.Empty, default), workers: 4, breadReserve: 2m,
            foodRation: 3m, breadRation: 1m);
        Assert.True(result.Meal.Served);
        Assert.Equal(1m, result.Processing.ConsumedIngredients["bread"].Units);
        Assert.Equal(2m, result.Processing.ConsumedIngredients["fish"].Units);
        Assert.Equal(4m, result.Processing.ProducedFood.Units);
        Assert.Equal(1m, result.NextDepot.RawFood["bread"].Units);
        Assert.Equal(1m, result.NextDepot.ReadyFood.Units);
        Assert.Equal(1m, result.Meal.ConsumedBread.Units);
        Assert.Equal(3m, result.Meal.ConsumedReadyFood.Units);
    }

    [Fact]
    public void NoWorkersMeansNoFoodCreatedOrConsumedWithoutExistingPreparedStock()
    {
        var city = ResourceBundle.Create(("fish", 3m));
        var result = Tick(Policy(), city, new(ResourceBundle.Empty, default), workers: 0,
            foodRation: 1m);
        Assert.Equal(3m, result.NextDepot.RawFood["fish"].Units);
        Assert.Equal(0m, result.NextDepot.ReadyFood.Units);
        Assert.False(result.Meal.Served);
        Assert.Equal(0m, result.Processing.ProducedFood.Units);
    }

    [Fact]
    public void AnUnfillableMealDoesNotConsumeEitherTheBreadOrThePreparedFood()
    {
        var original = new FoodDepotStock(ResourceBundle.Create(("bread", 1m)), U(8m));
        var result = Tick(Policy(), ResourceBundle.Empty, original,
            workers: 0, foodRation: 2m, breadRation: 2m);
        Assert.False(result.Meal.Served);
        Assert.Equal(8m, result.NextDepot.ReadyFood.Units);
        Assert.Equal(1m, result.NextDepot.RawFood["bread"].Units);
    }

    [Fact]
    public void ReadyFoodCapacityStopsProductionButExistingMealsCanBeServed()
    {
        var state = new FoodDepotStock(ResourceBundle.Empty, U(4m));
        var result = Tick(Policy(), ResourceBundle.Create(("fish", 3m)), state,
            workers: 5, readyCapacity: 4m, foodRation: 2m);
        Assert.Equal(0L, result.Processing.BatchesProcessed);
        Assert.Equal(3m, result.NextDepot.RawFood["fish"].Units);
        Assert.Equal(2m, result.NextDepot.ReadyFood.Units);
        Assert.True(result.Meal.Served);
    }

    [Fact]
    public void RawCapacityLimitsReceiptWithoutLosingUnacceptedFood()
    {
        var state = new FoodDepotStock(ResourceBundle.Create(("fish", 4m)), default);
        var capacity = ResourceBundle.Create(("fish", 5m), ("bread", 5m),
            ("meat", 5m), ("fruit", 5m), ("vegetables", 5m));
        var result = Tick(Policy(), ResourceBundle.Create(("fish", 3m)), state,
            capacity: capacity, workers: 0);
        Assert.Equal(1m, result.DeliveredToDepot["fish"].Units);
        Assert.Equal(2m, result.NextCityResources["fish"].Units);
        Assert.Equal(5m, result.NextDepot.RawFood["fish"].Units);
    }

    [Fact]
    public void HistoricalOverCapacityRawStockIsRetainedAndNewDeliveryIsBlocked()
    {
        var state = new FoodDepotStock(ResourceBundle.Create(("fish", 8m)), default);
        var result = Tick(Policy(), ResourceBundle.Create(("fish", 3m)), state,
            capacity: Capacity(5m), workers: 0);
        Assert.Equal(8m, result.NextDepot.RawFood["fish"].Units);
        Assert.Equal(3m, result.NextCityResources["fish"].Units);
        Assert.Equal(ResourceBundle.Empty, result.DeliveredToDepot);
    }

    [Fact]
    public void OldCityPreparedFoodCannotSilentlyBeOwnedByTwoInventories()
    {
        var city = ResourceBundle.Create(("food", 30m), ("fish", 1m));
        var stock = new FoodDepotStock(ResourceBundle.Empty, U(30m));
        Assert.Throws<ArgumentException>(() => Tick(Policy(), city, stock));
    }

    [Fact]
    public void PriorityIsDeterministicAndIndependentOfDeclarationOrder()
    {
        var first = Policy(Recipe("bread", priority: 5), Recipe("fish", priority: 2),
            Recipe("meat", priority: 1), Recipe("fruit", priority: 3), Recipe("vegetables", priority: 4));
        var reversed = Policy(Recipe("vegetables", priority: 4), Recipe("fruit", priority: 3),
            Recipe("meat", priority: 1), Recipe("fish", priority: 2), Recipe("bread", priority: 5));
        var city = ResourceBundle.Create(("meat", 2m), ("fish", 2m), ("bread", 2m));
        var a = Tick(first, city, new(ResourceBundle.Empty, default), workers: 2);
        var b = Tick(reversed, city, new(ResourceBundle.Empty, default), workers: 2);
        Assert.Equal(a.NextCityResources, b.NextCityResources);
        Assert.Equal(a.NextDepot, b.NextDepot);
        Assert.Equal(new[] { "meat" }, a.Processing.Runs.Select(run => run.SourceResourceId));
    }

    [Fact]
    public void HorticultureProducesIngredientsButOnlyDepotWorkersCreateReadyFood()
    {
        var horticulture = new HorticulturePolicy("water", "vegetables", "fruit",
            new("vegetables", U(2m), default, 0),
            new("fruit", U(3m), default, 1));
        var crops = HorticulturePlanner.PlanTick(horticulture, ResourceBundle.Empty,
            ResourceBundle.Create(("vegetables", 10m), ("fruit", 10m)), 1, 1);
        var empty = new FoodDepotStock(ResourceBundle.Empty, default);
        var idle = Tick(Policy(), crops.NextStock, empty, workers: 0);
        Assert.Equal(0m, idle.NextDepot.ReadyFood.Units);
        Assert.Equal(2m, idle.NextDepot.RawFood["vegetables"].Units);
        Assert.Equal(3m, idle.NextDepot.RawFood["fruit"].Units);

        var worked = Tick(Policy(), idle.NextCityResources, idle.NextDepot,
            workers: 5, foodRation: 2m);
        Assert.True(worked.Meal.Served);
        Assert.Equal(3m, worked.NextDepot.ReadyFood.Units);
        Assert.Equal(ResourceBundle.Empty, worked.NextDepot.RawFood);
    }

    [Fact]
    public void CattleAndGameAreNotFoodUntilButcheredAndDepotProcessed()
    {
        var meatPolicy = new MeatChainPolicy("grain", "water", "cattle", "game", "meat",
            ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(2m), U(1m),
            // Two cattle yield three meat; one hunted animal yields one meat.
            // The two butcher workers therefore exercise BOTH alternative recipes.
            [new("cattle", U(2m), U(3m), 0), new("game", U(1m), U(1m), 1)]);
        var animalCapacity = ResourceBundle.Create(("cattle", 10m), ("game", 10m), ("meat", 30m));
        var animals = MeatChainPlanner.PlanTick(meatPolicy,
            ResourceBundle.Create(("grain", 1m), ("water", 1m)), animalCapacity, 1, 1, 2);
        Assert.Equal(0m, animals.NextStock["meat"].Units);
        var noMeat = Tick(Policy(), animals.NextStock,
            new(ResourceBundle.Empty, default), workers: 10);
        Assert.Equal(0m, noMeat.NextDepot.ReadyFood.Units);
        Assert.Equal(2m, noMeat.NextCityResources["cattle"].Units);
        Assert.Equal(1m, noMeat.NextCityResources["game"].Units);

        // Process the actual animal stock from the preceding step, not a fabricated replacement.
        var butchered = MeatChainPlanner.PlanTick(meatPolicy,
            noMeat.NextCityResources, animalCapacity, 0, 0, 2);
        Assert.Equal(1L, butchered.Runs.Single(run => run.Stage == MeatChainStage.ButcherCattle).Batches);
        Assert.Equal(1L, butchered.Runs.Single(run => run.Stage == MeatChainStage.ButcherGame).Batches);
        Assert.Equal(0m, butchered.NextStock["cattle"].Units);
        Assert.Equal(0m, butchered.NextStock["game"].Units);
        Assert.Equal(4m, butchered.NextStock["meat"].Units);
        var ready = Tick(Policy(), butchered.NextStock, noMeat.NextDepot,
            workers: 4, foodRation: 3m);
        Assert.True(ready.Meal.Served);
        Assert.Equal(1m, ready.NextDepot.ReadyFood.Units);
        Assert.Equal(0m, ready.NextCityResources["meat"].Units);
    }

    [Fact]
    public void AnExtraBreadReserveProtectsFutureRationsEvenWhenTheDepotCanProduceMore()
    {
        var policy = Policy(Recipe("bread", priority: 0), Recipe("fish", priority: 1),
            Recipe("meat", priority: 2), Recipe("fruit", priority: 3), Recipe("vegetables", priority: 4));
        var result = Tick(policy, ResourceBundle.Create(("bread", 4m)),
            new(ResourceBundle.Empty, U(2m)), workers: 100,
            breadReserve: 3m, foodRation: 1m, breadRation: 1m);
        Assert.True(result.Meal.Served);
        Assert.Equal(1m, result.Processing.ConsumedIngredients["bread"].Units);
        Assert.Equal(2m, result.NextDepot.RawFood["bread"].Units);
        Assert.Equal(2m, result.NextDepot.ReadyFood.Units);
    }
}
