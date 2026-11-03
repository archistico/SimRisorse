using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using Xunit;

namespace SimRisorse.Core.Tests;

/// <summary>M5.3c: parametric training capacity and population/weapon conservation.</summary>
public sealed class MilitaryCapacityThroughputTests
{
    [Theory]
    [InlineData(1, 5, 150)]
    [InlineData(1, 10, 300)]
    [InlineData(1, 20, 600)]
    [InlineData(2, 5, 90)]
    [InlineData(2, 10, 150)]
    [InlineData(2, 20, 300)]
    [InlineData(4, 5, 60)]
    [InlineData(4, 10, 90)]
    [InlineData(4, 20, 150)]
    public void TrainingThroughputIsDeterministicWithoutConsumingExtraResidents(
        int barracks, int target, int expectedSeconds)
    {
        var rules = MilitaryTrainingRules.IndividualBarracks;
        var swords = ResourceBundle.Create((rules.SwordResourceId, 200m));
        var order = MilitaryTraining.Enlist(rules, MilitaryTrainingState.IndividualEmpty,
            swords, population: 60, assignedWorkers: 15, barracks, count: target);
        Assert.True(order.Succeeded);
        Assert.Equal(200m - target * 5m, order.Resources[rules.SwordResourceId].Units);
        Assert.Equal((long)target, order.State.EnlistedPopulation);
        var justBefore = MilitaryTraining.Advance(rules, order.State, barracks, expectedSeconds - 1);
        Assert.True(justBefore.AvailableSoldiers < target);
        var bulk = MilitaryTraining.Advance(rules, order.State, barracks, expectedSeconds);
        var smallSteps = order.State;
        for (var second = 0; second < expectedSeconds; second++)
        {
            smallSteps = MilitaryTraining.Advance(rules, smallSteps, barracks, 1);
            Assert.Equal((long)target, smallSteps.EnlistedPopulation);
        }
        Assert.Equal(bulk, smallSteps);
        Assert.Equal(target, bulk.AvailableSoldiers);
        Assert.Equal(0, bulk.TrainingRecruits);
        Assert.Equal(0, bulk.WaitingRecruits);
    }
}
