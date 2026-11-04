using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class MeatChainFoundationTests
{
    // Test-only illustrative yields: the playable economy has NOT adopted these values.
    private static MeatChainPolicy Policy(int cattlePriority = 0, int gamePriority = 1,
        decimal cattleMeat = 3m, decimal gameMeat = 1m) =>
        new("grain", "water", "cattle", "game", "meat",
            ResourceBundle.Create(("grain", 1m), ("water", 1m)),
            ResourceAmount.FromUnits(2m), ResourceAmount.FromUnits(1m),
            [new("game", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(gameMeat), gamePriority),
             new("cattle", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(cattleMeat), cattlePriority)]);

    private static ResourceBundle Capacity(decimal cattle = 20m, decimal game = 20m, decimal meat = 40m) =>
        ResourceBundle.Create(("cattle", cattle), ("game", game), ("meat", meat));

    [Fact]
    public void StableNeedsBothGrainAndWaterBeforeProducingCattle()
    {
        var original = ResourceBundle.Create(("grain", 3m));
        var blocked = MeatChainPlanner.PlanTick(Policy(), original, Capacity(), 2, 0, 0);
        Assert.Equal(original, blocked.NextStock);
        Assert.Equal(0L, blocked.Runs[0].Batches);

        var fed = ResourceBundle.Create(("grain", 3m), ("water", 2m));
        var produced = MeatChainPlanner.PlanTick(Policy(), fed, Capacity(), 3, 0, 0);
        Assert.Equal(2L, produced.Runs[0].Batches);
        Assert.Equal(1m, produced.NextStock["grain"].Units);
        Assert.Equal(0m, produced.NextStock["water"].Units);
        Assert.Equal(4m, produced.NextStock["cattle"].Units);
        Assert.Equal(0m, fed["cattle"].Units);
    }

    [Fact]
    public void HuntingNeedsNoGrainOrWaterButRespectsWorkforceAndStorage()
    {
        var result = MeatChainPlanner.PlanTick(Policy(), ResourceBundle.Empty,
            Capacity(game: 2m), 0, 5, 0);
        Assert.Equal(2L, result.Runs[1].Batches);
        Assert.Equal(2m, result.NextStock["game"].Units);
        Assert.Equal(ResourceBundle.Empty, result.Consumed);
        Assert.Equal(0m, result.NextStock["meat"].Units);
    }

    [Fact]
    public void ButcherAllocatesSharedWorkersWithoutDoubleCounting()
    {
        var initial = ResourceBundle.Create(("cattle", 2m), ("game", 4m));
        var result = MeatChainPlanner.PlanTick(Policy(), initial, Capacity(), 0, 0, 3);
        Assert.Equal(new[] { MeatChainStage.Stable, MeatChainStage.Hunting,
            MeatChainStage.ButcherCattle, MeatChainStage.ButcherGame },
            result.Runs.Select(run => run.Stage));
        Assert.Equal(2L, result.Runs[2].Batches);
        Assert.Equal(1L, result.Runs[3].Batches);
        Assert.Equal(7m, result.NextStock["meat"].Units);
        Assert.Equal(3m, result.NextStock["game"].Units);
        Assert.Equal(0m, result.NextStock["cattle"].Units);
    }

    [Fact]
    public void NewAnimalsCannotBeButcheredUntilFollowingTick()
    {
        var first = MeatChainPlanner.PlanTick(Policy(),
            ResourceBundle.Create(("grain", 1m), ("water", 1m)), Capacity(), 1, 1, 5);
        Assert.Equal(2m, first.NextStock["cattle"].Units);
        Assert.Equal(1m, first.NextStock["game"].Units);
        Assert.Equal(0m, first.NextStock["meat"].Units);
        Assert.Equal(0L, first.Runs[2].Batches);
        Assert.Equal(0L, first.Runs[3].Batches);

        var second = MeatChainPlanner.PlanTick(Policy(), first.NextStock, Capacity(), 0, 0, 3);
        Assert.Equal(7m, second.NextStock["meat"].Units);
        Assert.Equal(0m, second.NextStock["cattle"].Units);
        Assert.Equal(0m, second.NextStock["game"].Units);
    }

    [Fact]
    public void FullMeatStoragePreventsButcheringWithoutConsumingAnimals()
    {
        var source = ResourceBundle.Create(("cattle", 1m), ("game", 2m), ("meat", 10m));
        var blocked = MeatChainPlanner.PlanTick(Policy(), source,
            Capacity(meat: 10m), 0, 0, 10);
        Assert.Equal(source, blocked.NextStock);
        Assert.Equal(0L, blocked.Runs[2].Batches);
        Assert.Equal(0L, blocked.Runs[3].Batches);
        Assert.Equal(ResourceBundle.Empty, blocked.Consumed);
    }

    [Fact]
    public void InsufficientRoomForWholeBatchDoesNotConsumeAnimal()
    {
        var source = ResourceBundle.Create(("cattle", 1m), ("meat", 8m));
        var result = MeatChainPlanner.PlanTick(Policy(), source, Capacity(meat: 10m), 0, 0, 1);
        Assert.Equal(source, result.NextStock); // 2 free, but the cattle recipe yields 3.
        Assert.Equal(0L, result.Runs[2].Batches);
    }

    [Fact]
    public void ButcherPriorityChangesSelectionWithSameWorkforce()
    {
        var source = ResourceBundle.Create(("cattle", 2m), ("game", 2m));
        var cattleFirst = MeatChainPlanner.PlanTick(Policy(), source, Capacity(), 0, 0, 1);
        var gameFirst = MeatChainPlanner.PlanTick(Policy(cattlePriority: 2, gamePriority: 1),
            source, Capacity(), 0, 0, 1);
        Assert.Equal(3m, cattleFirst.NextStock["meat"].Units);
        Assert.Equal(1m, gameFirst.NextStock["meat"].Units);
        Assert.Equal(MeatChainStage.ButcherGame, gameFirst.Runs[2].Stage);
        Assert.Equal(1L, gameFirst.Runs[2].Batches);
    }

    [Fact]
    public void YieldsAreParametersAndNotActivatedGameBalance()
    {
        var source = ResourceBundle.Create(("cattle", 1m), ("game", 1m));
        var highCattle = MeatChainPlanner.PlanTick(Policy(), source, Capacity(), 0, 0, 2);
        var equalYield = MeatChainPlanner.PlanTick(Policy(cattleMeat: 1m, gameMeat: 1m),
            source, Capacity(), 0, 0, 2);
        Assert.Equal(4m, highCattle.NextStock["meat"].Units);
        Assert.Equal(2m, equalYield.NextStock["meat"].Units);
    }

    [Fact]
    public void PolicyRejectsDuplicateAnimalRecipesAndMissingFeed()
    {
        var cattle = new ButcherMeatRecipe("cattle", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(2m), 0);
        Assert.Throws<ArgumentException>(() => new MeatChainPolicy(
            "grain", "water", "cattle", "game", "meat",
            ResourceBundle.Create(("grain", 1m), ("water", 1m)), ResourceAmount.FromUnits(1m),
            ResourceAmount.FromUnits(1m), [cattle, cattle]));
        Assert.Throws<ArgumentException>(() => new MeatChainPolicy(
            "grain", "water", "cattle", "game", "meat", ResourceBundle.Create(("grain", 1m)),
            ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m),
            [cattle, new("game", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 1)]));
    }

    [Fact]
    public void NoWorkersAndInvalidForeignResourcesDoNotCreateStock()
    {
        var source = ResourceBundle.Create(("grain", 5m), ("water", 5m), ("game", 2m));
        var idle = MeatChainPlanner.PlanTick(Policy(), source, Capacity(), 0, 0, 0);
        Assert.Equal(source, idle.NextStock);
        Assert.Equal(ResourceBundle.Empty, idle.Consumed);
        Assert.Throws<ArgumentException>(() => MeatChainPlanner.PlanTick(Policy(),
            ResourceBundle.Create(("coins", 10m)), Capacity(), 1, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => MeatChainPlanner.PlanTick(Policy(),
            source, Capacity(), 1, -1, 1));
    }

    [Fact]
    public void ProducedMeatDoesNotBecomeReadyFoodWithoutDepotWorkers()
    {
        var chain = MeatChainPlanner.PlanTick(Policy(),
            ResourceBundle.Create(("cattle", 1m)), Capacity(), 0, 0, 1);
        Assert.Equal(3m, chain.NextStock["meat"].Units);
        var depotPolicy = new FoodConversionPolicy("food", "bread",
            [new FoodConversionRecipe("meat", ResourceAmount.FromUnits(1m),
                ResourceAmount.FromUnits(1m), 0)]);
        var emptyDepot = new FoodDepotStock(ResourceBundle.Empty, ResourceAmount.Zero);
        Assert.Equal(0m, FoodDepotPlanner.Plan(depotPolicy, emptyDepot,
            ResourceAmount.FromUnits(10m), 5, default).ProducedFood.Units);
        var receipt = FoodDepotPlanner.Receive(depotPolicy, emptyDepot,
            ResourceBundle.Create(("meat", 3m)), ResourceBundle.Create(("meat", 10m)));
        Assert.True(receipt.Accepted);
        Assert.Equal(0m, FoodDepotPlanner.Plan(depotPolicy, receipt.NextStock,
            ResourceAmount.FromUnits(10m), 0, default).ProducedFood.Units);
        var prepared = FoodDepotPlanner.Plan(depotPolicy, receipt.NextStock,
            ResourceAmount.FromUnits(10m), 5, default);
        Assert.Equal(3m, prepared.ProducedFood.Units);
        Assert.Equal(0m, prepared.NextStock.RawFood["meat"].Units);
    }
}
