using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class HorticultureFoundationTests
{
    // These yields and water costs are illustrative test fixtures, not game balance.
    private static HorticulturePolicy Policy(decimal vegetableYield = 2m, decimal fruitYield = 3m,
        decimal gardenWater = 0m, decimal orchardWater = 0m,
        int gardenPriority = 0, int orchardPriority = 1) =>
        new("water", "vegetables", "fruit",
            new("vegetables", ResourceAmount.FromUnits(vegetableYield),
                ResourceAmount.FromUnits(gardenWater), gardenPriority),
            new("fruit", ResourceAmount.FromUnits(fruitYield),
                ResourceAmount.FromUnits(orchardWater), orchardPriority));

    private static ResourceBundle Capacity(decimal vegetables = 20m, decimal fruit = 20m) =>
        ResourceBundle.Create(("vegetables", vegetables), ("fruit", fruit));

    [Fact]
    public void GardenProducesVegetablesIndependentlyOfOrchard()
    {
        var result = HorticulturePlanner.PlanTick(Policy(), ResourceBundle.Empty, Capacity(), 2, 0);
        Assert.Equal(4m, result.NextStock["vegetables"].Units);
        Assert.Equal(0m, result.NextStock["fruit"].Units);
        Assert.Equal(2, result.Runs.Length);
        Assert.Equal(2L, result.Runs[0].Batches);
    }

    [Fact]
    public void OrchardProducesFruitIndependentlyOfGarden()
    {
        var result = HorticulturePlanner.PlanTick(Policy(), ResourceBundle.Empty, Capacity(), 0, 2);
        Assert.Equal(6m, result.NextStock["fruit"].Units);
        Assert.Equal(0m, result.NextStock["vegetables"].Units);
        Assert.Equal(ResourceBundle.Empty, result.Consumed);
    }

    [Fact]
    public void ParallelCropsUseSeparateWorkersAndCapacity()
    {
        var result = HorticulturePlanner.PlanTick(Policy(), ResourceBundle.Empty, Capacity(), 2, 3);
        Assert.Equal(4m, result.NextStock["vegetables"].Units);
        Assert.Equal(9m, result.NextStock["fruit"].Units);
    }

    [Fact]
    public void FullVegetableStorageOnlyStopsGarden()
    {
        var original = ResourceBundle.Create(("vegetables", 20m));
        var result = HorticulturePlanner.PlanTick(Policy(), original, Capacity(), 2, 2);
        Assert.Equal(20m, result.NextStock["vegetables"].Units);
        Assert.Equal(6m, result.NextStock["fruit"].Units);
        Assert.Equal(0L, result.Runs[0].Batches);
    }

    [Fact]
    public void PartialSpaceIsNotFilledWithAnInvalidFractionalBatch()
    {
        var original = ResourceBundle.Create(("vegetables", 19m), ("fruit", 18m));
        var result = HorticulturePlanner.PlanTick(Policy(), original, Capacity(), 1, 1);
        Assert.Equal(original, result.NextStock);
        Assert.Equal(ResourceBundle.Empty, result.Produced);
    }

    [Fact]
    public void OptionalWaterCannotBeSpentTwiceAcrossCrops()
    {
        var original = ResourceBundle.Create(("water", 3m));
        var result = HorticulturePlanner.PlanTick(Policy(gardenWater: 1m, orchardWater: 2m),
            original, Capacity(), 2, 2);
        Assert.Equal(4m, result.NextStock["vegetables"].Units);
        Assert.Equal(0m, result.NextStock["fruit"].Units);
        Assert.Equal(1m, result.NextStock["water"].Units);
    }

    [Fact]
    public void PriorityCanFavorOrchardWhenWaterIsScarce()
    {
        var original = ResourceBundle.Create(("water", 2m));
        var result = HorticulturePlanner.PlanTick(Policy(gardenWater: 1m, orchardWater: 2m,
            gardenPriority: 2, orchardPriority: 1), original, Capacity(), 2, 2);
        Assert.Equal("fruit", result.Runs[0].CropResourceId);
        Assert.Equal(3m, result.NextStock["fruit"].Units);
        Assert.Equal(0m, result.NextStock["vegetables"].Units);
        Assert.Equal(0m, result.NextStock["water"].Units);
    }

    [Fact]
    public void WaterWithoutEnoughForWholeBatchIsNotConsumed()
    {
        var source = ResourceBundle.Create(("water", 1m));
        var result = HorticulturePlanner.PlanTick(Policy(gardenWater: 2m, orchardWater: 2m),
            source, Capacity(), 3, 3);
        Assert.Equal(source, result.NextStock);
        Assert.Equal(ResourceBundle.Empty, result.Consumed);
    }

    [Fact]
    public void MissingWorkersProduceNothing()
    {
        var source = ResourceBundle.Create(("water", 9m), ("fruit", 2m));
        var result = HorticulturePlanner.PlanTick(Policy(), source, Capacity(), 0, 0);
        Assert.Equal(source, result.NextStock);
        Assert.Equal(ResourceBundle.Empty, result.Produced);
    }

    [Fact]
    public void InvalidConfigurationOrUnexpectedResourcesAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new HorticulturePolicy("fruit", "vegetables", "fruit",
            new("vegetables", ResourceAmount.FromUnits(1m), default, 0),
            new("fruit", ResourceAmount.FromUnits(1m), default, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HorticultureCropRecipe("fruit", default, default, 0));
        Assert.Throws<ArgumentException>(() => HorticulturePlanner.PlanTick(Policy(),
            ResourceBundle.Create(("meat", 1m)), Capacity(), 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => HorticulturePlanner.PlanTick(Policy(),
            ResourceBundle.Empty, Capacity(), -1, 0));
    }

    [Fact]
    public void RawProduceNeedsFoodDepotWorkBeforeBecomingReadyFood()
    {
        var crops = HorticulturePlanner.PlanTick(Policy(), ResourceBundle.Empty, Capacity(), 1, 1);
        Assert.Equal(5m, crops.NextStock["vegetables"].Units + crops.NextStock["fruit"].Units);
        var policy = new FoodConversionPolicy("food", "bread",
            [new("vegetables", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 0),
             new("fruit", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 1)]);
        var depot = new FoodDepotStock(ResourceBundle.Empty, ResourceAmount.Zero);
        Assert.Equal(0m, FoodDepotPlanner.Plan(policy, depot,
            ResourceAmount.FromUnits(20m), 5, default).ProducedFood.Units);
        var delivered = FoodDepotPlanner.Receive(policy, depot,
            crops.NextStock,
            ResourceBundle.Create(("vegetables", 10m), ("fruit", 10m)));
        Assert.True(delivered.Accepted);
        Assert.Equal(0m, FoodDepotPlanner.Plan(policy, delivered.NextStock,
            ResourceAmount.FromUnits(20m), 0, default).ProducedFood.Units);
        var prepared = FoodDepotPlanner.Plan(policy, delivered.NextStock,
            ResourceAmount.FromUnits(20m), 5, default);
        Assert.Equal(5m, prepared.ProducedFood.Units);
        Assert.Equal(ResourceBundle.Empty, prepared.NextStock.RawFood);
    }

    [Fact]
    public void EqualInputsGiveDeterministicResultsAndDoNotMutateStock()
    {
        var source = ResourceBundle.Create(("water", 6m));
        var rules = Policy(gardenWater: 1m, orchardWater: 1m);
        var first = HorticulturePlanner.PlanTick(rules, source, Capacity(), 2, 2);
        var second = HorticulturePlanner.PlanTick(rules, source, Capacity(), 2, 2);
        Assert.Equal(first.NextStock, second.NextStock);
        Assert.Equal(first.Consumed, second.Consumed);
        Assert.Equal(first.Produced, second.Produced);
        Assert.Equal(first.Runs.Select(run => (run.CropResourceId, run.Batches, run.Inputs, run.Outputs)).ToArray(),
            second.Runs.Select(run => (run.CropResourceId, run.Batches, run.Inputs, run.Outputs)).ToArray());
        Assert.Equal(6m, source["water"].Units);
    }
}
