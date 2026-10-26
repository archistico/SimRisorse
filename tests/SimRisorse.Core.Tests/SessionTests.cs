using System.Collections.Immutable;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class SessionTests
{
    [Fact]
    public void FailedBulkPurchaseIsAtomicAndNewBuildingsDoNotGainWorkers()
    {
        var game = GameSession.Create(TestScenarios.Economy());
        var before = game.Snapshot();
        Assert.Equal(GameError.InsufficientResources, game.Construct("sawmill", 4).Error);
        TestScenarios.EqualState(before, game.Snapshot());
        Assert.True(game.Construct("sawmill", 3).Succeeded);
        var sawmill = game.Snapshot().Buildings.Single(item => item.BuildingId == "sawmill");
        Assert.Equal(4, sawmill.Count);
        Assert.Equal(1, sawmill.AssignedWorkers);
        Assert.Equal(20m, game.Resources["stone"].Units);
        Assert.Equal(0m, game.Resources["planks"].Units);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void InvalidQuantitiesNeverChangeState(int count)
    {
        var game = GameSession.Create(TestScenarios.Economy());
        var before = game.Snapshot();
        Assert.Equal(GameError.InvalidQuantity, game.Construct("tree-feller", count).Error);
        Assert.Equal(GameError.InvalidQuantity, game.AssignWorkers("tree-feller", count).Error);
        Assert.Equal(GameError.InvalidQuantity, game.ReleaseWorkers("tree-feller", count).Error);
        TestScenarios.EqualState(before, game.Snapshot());
    }

    [Fact]
    public void UnknownBuildingsAreRejectedWithoutMutation()
    {
        var game = GameSession.Create(TestScenarios.Economy());
        var before = game.Snapshot();
        Assert.Equal(GameError.UnknownBuilding, game.Construct("unknown", 1).Error);
        Assert.Equal(GameError.UnknownBuilding, game.AssignWorkers("unknown", 1).Error);
        Assert.Equal(GameError.UnknownBuilding, game.ReleaseWorkers("unknown", 1).Error);
        TestScenarios.EqualState(before, game.Snapshot());
    }

    [Fact]
    public void PrerequisitesPreventConstructionUntilTheRequiredBuildingExists()
    {
        var scenario = TestScenarios.Create(
            [TestScenarios.Building("base"), TestScenarios.Building("advanced", prerequisites: ["base"])],
            ResourceBundle.Empty, initial: []);
        var game = GameSession.Create(scenario);
        Assert.Equal(GameError.MissingPrerequisite, game.Construct("advanced", 1).Error);
        Assert.True(game.Construct("base", 1).Succeeded);
        Assert.True(game.Construct("advanced", 1).Succeeded);
    }

    [Fact]
    public void WorkplacesAndPopulationAreDistinctConstraints()
    {
        var game = GameSession.Create(TestScenarios.Economy());
        Assert.Equal(GameError.InsufficientWorkplaces, game.AssignWorkers("tree-feller", 1).Error);
        Assert.True(game.Construct("tree-feller", 6).Succeeded);
        Assert.True(game.AssignWorkers("tree-feller", 5).Succeeded);
        Assert.Equal(0, game.AvailableWorkers);
        Assert.Equal(GameError.InsufficientPopulation, game.AssignWorkers("tree-feller", 1).Error);
        Assert.True(game.ReleaseWorkers("sawmill", 1).Succeeded);
        Assert.True(game.AssignWorkers("tree-feller", 1).Succeeded);
        Assert.Equal(GameError.InsufficientAssignedWorkers, game.ReleaseWorkers("sawmill", 1).Error);
        Assert.Equal(8, game.Snapshot().Buildings.Sum(item => item.AssignedWorkers));
    }

    [Fact]
    public void MultipleWorkplacesPerBuildingAreSupported()
    {
        var scenario = TestScenarios.Create([TestScenarios.Building("workshop", output: ResourceBundle.Create(("logs", 1m)), workplaces: 3)], ResourceBundle.Empty);
        var game = GameSession.Create(scenario);
        Assert.True(game.AssignWorkers("workshop", 2).Succeeded);
        Assert.Equal(GameError.InsufficientWorkplaces, game.AssignWorkers("workshop", 1).Error);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(3m, game.Resources["logs"].Units);
    }

    [Fact]
    public void BuildingCountOverflowDoesNotSpendResources()
    {
        var scenario = TestScenarios.Economy();
        var initial = GameSession.Create(scenario).Snapshot();
        var modified = initial with { Buildings = initial.Buildings.Select(item => item.BuildingId == "tree-feller" ? item with { Count = int.MaxValue } : item).ToImmutableArray() };
        var game = GameSession.Restore(scenario, modified);
        Assert.Equal(GameError.NumericLimit, game.Construct("tree-feller", 1).Error);
        TestScenarios.EqualState(modified, game.Snapshot());
    }

    [Fact]
    public void CostOverflowIsRejectedBeforeMutation()
    {
        var scenario = TestScenarios.Create([TestScenarios.Building("expensive", cost: ResourceBundle.FromAmounts(
            [KeyValuePair.Create("logs", ResourceAmount.FromMilliunits(long.MaxValue))]))], ResourceBundle.Empty);
        var game = GameSession.Create(scenario);
        var before = game.Snapshot();
        Assert.Equal(GameError.NumericLimit, game.Construct("expensive", 2).Error);
        TestScenarios.EqualState(before, game.Snapshot());
    }

    [Fact]
    public void OrdersInPauseAreAppliedWithoutAdvancingTime()
    {
        var game = GameSession.Create(TestScenarios.Economy());
        Assert.True(game.Construct("tree-feller", 1).Succeeded);
        Assert.True(game.AssignWorkers("tree-feller", 1).Succeeded);
        game.Advance(TimeSpan.FromHours(8));
        Assert.Equal(0, game.ElapsedTicks);
        Assert.True(game.IsPaused);
        Assert.Equal(100m, game.Resources["planks"].Units);
    }

    [Fact]
    public void InvalidOrExcessiveTimeAndTimeOverflowAreAtomic()
    {
        var scenario = TestScenarios.Economy();
        var game = GameSession.Create(scenario);
        game.Resume();
        var before = game.Snapshot();
        Assert.Equal(GameError.InvalidElapsedTime, game.Advance(TimeSpan.FromTicks(-1)).Error);
        Assert.Equal(GameError.AdvanceLimitExceeded, game.Advance(TimeSpan.FromDays(2)).Error);
        TestScenarios.EqualState(before, game.Snapshot());
        game = GameSession.Restore(scenario, before with { ElapsedTicks = long.MaxValue });
        game.Resume();
        Assert.Equal(GameError.NumericLimit, game.Advance(TimeSpan.FromTicks(1)).Error);
        Assert.Equal(long.MaxValue, game.ElapsedTicks);
    }
}
