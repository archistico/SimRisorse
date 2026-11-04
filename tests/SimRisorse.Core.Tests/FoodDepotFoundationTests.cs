using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class FoodDepotFoundationTests
{
    private static FoodConversionPolicy Policy(params FoodConversionRecipe[] recipes) =>
        new("food", "bread", recipes);

    private static FoodConversionRecipe Recipe(string source, decimal input = 1m,
        decimal output = 1m, int priority = 0) =>
        new(source, ResourceAmount.FromUnits(input), ResourceAmount.FromUnits(output), priority);

    private static FoodDepotStock Stock(ResourceBundle raw, decimal readyFood = 0m) =>
        new(raw, ResourceAmount.FromUnits(readyFood));

    [Fact]
    public void DepotProducesOnlyFromIngredientsReceivedInsideItsOwnInventory()
    {
        var policy = Policy(Recipe("fish"));
        var empty = Stock(ResourceBundle.Empty);
        var idle = FoodDepotPlanner.Plan(policy, empty, ResourceAmount.FromUnits(10m), 5, default);
        Assert.Equal(0m, idle.ProducedFood.Units);
        Assert.Equal(0m, idle.NextStock.ReadyFood.Units);
        var receipt = FoodDepotPlanner.Receive(policy, empty,
            ResourceBundle.Create(("fish", 3m)), ResourceBundle.Create(("fish", 10m)));
        Assert.True(receipt.Accepted);
        var result = FoodDepotPlanner.Plan(policy, receipt.NextStock, ResourceAmount.FromUnits(10m), 5, default);
        Assert.Equal(3m, result.ProducedFood.Units);
        Assert.Equal(3m, result.ConsumedIngredients["fish"].Units);
        Assert.Equal(0m, result.NextStock.RawFood["fish"].Units);
        Assert.Equal(3m, result.NextStock.ReadyFood.Units);
        Assert.Equal(0m, empty.ReadyFood.Units); // Planning cannot mutate the previous checkpoint.
    }

    [Fact]
    public void ReceiptsAreAtomicAndRespectRawStorageCapacity()
    {
        var policy = Policy(Recipe("fish"), Recipe("meat"));
        var original = Stock(ResourceBundle.Create(("fish", 4m)));
        var rejected = FoodDepotPlanner.Receive(policy, original,
            ResourceBundle.Create(("fish", 3m), ("meat", 1m)),
            ResourceBundle.Create(("fish", 6m), ("meat", 5m)));
        Assert.False(rejected.Accepted);
        Assert.Equal(original, rejected.NextStock);
        var accepted = FoodDepotPlanner.Receive(policy, original,
            ResourceBundle.Create(("fish", 2m)), ResourceBundle.Create(("fish", 6m), ("meat", 5m)));
        Assert.True(accepted.Accepted);
        Assert.Equal(6m, accepted.NextStock.RawFood["fish"].Units);
    }

    [Fact]
    public void ProcessingDoesNotOverrunJobsOrReadyFoodCapacityOrWasteInput()
    {
        var policy = Policy(Recipe("meat", 1m, 3m));
        var state = Stock(ResourceBundle.Create(("meat", 10m)), 2m);
        var limited = FoodDepotPlanner.Plan(policy, state, ResourceAmount.FromUnits(8m), 10, default);
        Assert.Equal(2L, limited.BatchesProcessed);
        Assert.Equal(6m, limited.ProducedFood.Units);
        Assert.Equal(8m, limited.NextStock.ReadyFood.Units);
        Assert.Equal(8m, limited.NextStock.RawFood["meat"].Units);
        var noStaff = FoodDepotPlanner.Plan(policy, state, ResourceAmount.FromUnits(30m), 0, default);
        Assert.Equal(state, noStaff.NextStock);
    }

    [Fact]
    public void BreadRequiredByAdvancedHousingCannotAlsoBecomeGenericFood()
    {
        var policy = Policy(Recipe("bread", 1m, 2m));
        var state = Stock(ResourceBundle.Create(("bread", 5m)));
        var result = FoodDepotPlanner.Plan(policy, state, ResourceAmount.FromUnits(100m), 10,
            ResourceAmount.FromUnits(4m));
        Assert.Equal(4m, result.ReservedBread.Units);
        Assert.Equal(1m, result.ConsumedIngredients["bread"].Units);
        Assert.Equal(2m, result.ProducedFood.Units);
        Assert.Equal(4m, result.NextStock.RawFood["bread"].Units);
        var fullReserve = FoodDepotPlanner.Plan(policy, state, ResourceAmount.FromUnits(100m), 10,
            ResourceAmount.FromUnits(10m));
        Assert.Equal(0m, fullReserve.ProducedFood.Units);
        Assert.Equal(5m, fullReserve.ReservedBread.Units);
    }

    [Fact]
    public void RecipePriorityAndOrdinalTieBreakAreIndependentOfDeclarationOrder()
    {
        var first = Policy(Recipe("fish", priority: 10), Recipe("vegetables", priority: 5),
            Recipe("fruit", priority: 5));
        var second = Policy(Recipe("fruit", priority: 5), Recipe("fish", priority: 10),
            Recipe("vegetables", priority: 5));
        var state = Stock(ResourceBundle.Create(("fish", 1m), ("fruit", 1m), ("vegetables", 1m)));
        var a = FoodDepotPlanner.Plan(first, state, ResourceAmount.FromUnits(10m), 2, default);
        var b = FoodDepotPlanner.Plan(second, state, ResourceAmount.FromUnits(10m), 2, default);
        Assert.Equal(new[] { "fruit", "vegetables" }, a.Runs.Select(item => item.SourceResourceId));
        Assert.Equal(a.NextStock, b.NextStock);
        Assert.Equal(a.ConsumedIngredients, b.ConsumedIngredients);
        Assert.Equal(a.Runs.ToArray(), b.Runs.ToArray());
        Assert.Equal(1m, a.NextStock.RawFood["fish"].Units);
    }

    [Fact]
    public void DuplicateRecipesAndPreparedFoodAsInputAreRejected()
    {
        Assert.Throws<ArgumentException>(() => Policy(Recipe("fish"), Recipe("fish", priority: 10)));
        Assert.Throws<ArgumentException>(() => Policy(Recipe("food")));
        Assert.Throws<ArgumentOutOfRangeException>(() => Recipe("meat", 0m));
    }

    [Fact]
    public void UnknownRawResourceAndDeliveryAreRejectedRatherThanSilentlyConverted()
    {
        var policy = Policy(Recipe("fish"));
        Assert.Throws<ArgumentException>(() => FoodDepotPlanner.Plan(policy,
            Stock(ResourceBundle.Create(("coins", 10m))), ResourceAmount.FromUnits(10m), 5, default));
        Assert.Throws<ArgumentException>(() => FoodDepotPlanner.Receive(policy,
            Stock(ResourceBundle.Empty), ResourceBundle.Create(("food", 2m)), ResourceBundle.Empty));
    }
}
