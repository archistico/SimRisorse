using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class EconomyTests
{
    [Theory]
    [InlineData(1, 2, 120, 81)]
    [InlineData(2, 2, 121, 82)]
    [InlineData(60, 2, 179, 140)]
    public void ReferenceTimelineUsesOnlyInputsFromThePreviousStep(int seconds, decimal logs, decimal planks, decimal stone)
    {
        var game = GameSession.Create(TestScenarios.Economy());
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(seconds)).Succeeded);
        Assert.Equal(ResourceBundle.Create(("logs", logs), ("planks", planks), ("stone", stone)), game.Resources);
    }

    [Fact]
    public void FractionalAmountsRemainExactAndCannotOverflowStorage()
    {
        var scenario = TestScenarios.Create([TestScenarios.Building("producer", output: ResourceBundle.Create(("logs", 0.1m)))],
            ResourceBundle.Empty, ResourceBundle.Create(("logs", 0.3m)));
        var game = GameSession.Create(scenario);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(10));
        Assert.Equal(0.3m, game.Resources["logs"].Units);
        Assert.Equal(ProductionBlockReason.StorageFull, Assert.Single(game.ForecastProduction()).BlockReason);
    }

    [Fact]
    public void PartialCapacityDoesNotExecuteAPartialRecipeOrConsumeItsInputs()
    {
        var scenario = TestScenarios.Create([TestScenarios.Building("processor", ResourceBundle.Create(("logs", 1m)), ResourceBundle.Create(("planks", 0.125m)))],
            ResourceBundle.Create(("logs", 10m)), ResourceBundle.Create(("logs", 10m), ("planks", 0.3m)));
        var game = GameSession.Create(scenario);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(3));
        Assert.Equal(8m, game.Resources["logs"].Units);
        Assert.Equal(0.25m, game.Resources["planks"].Units);
    }

    [Fact]
    public void WorkersWithoutInputsDoNotProduce()
    {
        var game = GameSession.Create(TestScenarios.Economy());
        game.ReleaseWorkers("tree-feller", 1);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(10));
        Assert.Equal(ResourceBundle.Create(("planks", 120m), ("stone", 90m)), game.Resources);
    }

    [Fact]
    public void OneWorkerMayRunWhenTwoAreAssignedButOnlyOneRecipeIsAffordable()
    {
        var scenario = TestScenarios.Create([TestScenarios.Building("processor", ResourceBundle.Create(("logs", 2m)), ResourceBundle.Create(("planks", 1m)))],
            ResourceBundle.Create(("logs", 3m)), initial: [new("processor", 2, 2)]);
        var game = GameSession.Create(scenario);
        var forecast = Assert.Single(game.ForecastProduction());
        Assert.Equal(1, forecast.RunnableWorkers);
        Assert.True(forecast.BlockReason.HasFlag(ProductionBlockReason.MissingInputs));
        Assert.Equal(new[] { "logs" }, forecast.MissingInputIds);
        Assert.Empty(forecast.FullOutputIds);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(ResourceBundle.Create(("logs", 1m), ("planks", 1m)), game.Resources);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SharedInputsFollowExplicitPriorityIndependentOfDefinitionOrder(bool reverse)
    {
        var first = TestScenarios.Building("first", ResourceBundle.Create(("logs", 1m)), ResourceBundle.Create(("planks", 1m)), priority: 5);
        var second = TestScenarios.Building("second", ResourceBundle.Create(("logs", 1m)), ResourceBundle.Create(("stone", 1m)), priority: 10);
        var scenario = TestScenarios.Create(reverse ? [second, first] : [first, second], ResourceBundle.Create(("logs", 1m)));
        var game = GameSession.Create(scenario);
        var forecast = game.ForecastProduction();
        Assert.Equal(1, forecast.Single(item => item.BuildingId == "first").RunnableWorkers);
        var secondForecast = forecast.Single(item => item.BuildingId == "second");
        Assert.Equal(0, secondForecast.RunnableWorkers);
        Assert.Equal(new[] { "logs" }, secondForecast.MissingInputIds);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(ResourceBundle.Create(("planks", 1m)), game.Resources);
    }

    [Fact]
    public void EqualPrioritiesUseOrdinalIdentifierOrder()
    {
        var scenario = TestScenarios.Create(
            [TestScenarios.Building("zeta", ResourceBundle.Create(("logs", 1m)), ResourceBundle.Create(("stone", 1m))),
             TestScenarios.Building("alpha", ResourceBundle.Create(("logs", 1m)), ResourceBundle.Create(("planks", 1m)))],
            ResourceBundle.Create(("logs", 1m)));
        var game = GameSession.Create(scenario);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(ResourceBundle.Create(("planks", 1m)), game.Resources);
    }

    [Fact]
    public void ProducersAlsoCompeteForTheSameFreeStorage()
    {
        var scenario = TestScenarios.Create(
            [TestScenarios.Building("first", output: ResourceBundle.Create(("logs", 1m)), priority: 1),
             TestScenarios.Building("second", output: ResourceBundle.Create(("logs", 1m)), priority: 2)],
            ResourceBundle.Empty, ResourceBundle.Create(("logs", 1m)));
        var game = GameSession.Create(scenario);
        var secondForecast = game.ForecastProduction().Single(item => item.BuildingId == "second");
        Assert.Equal(0, secondForecast.RunnableWorkers);
        Assert.Equal(new[] { "logs" }, secondForecast.FullOutputIds);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(1m, game.Resources["logs"].Units);
    }

    [Fact]
    public void ForecastIdentifiesInputAndOutputBottlenecksWithoutChangingTheEconomicStep()
    {
        var scenario = TestScenarios.Create(
            [TestScenarios.Building("processor", ResourceBundle.Create(("logs", 1m)), ResourceBundle.Create(("planks", 1m)))],
            ResourceBundle.Empty,
            ResourceBundle.Create(("logs", 10m), ("planks", 1m)));
        var game = GameSession.Create(scenario);
        var before = game.Snapshot();
        var forecast = Assert.Single(game.ForecastProduction());
        Assert.Equal(ProductionBlockReason.MissingInputs, forecast.BlockReason);
        Assert.Equal(new[] { "logs" }, forecast.MissingInputIds);
        Assert.Empty(forecast.FullOutputIds);
        TestScenarios.EqualState(before, game.Snapshot());

        var full = GameSession.Restore(scenario, before with
        {
            Resources = ResourceBundle.Create(("planks", 1m))
        });
        var blocked = Assert.Single(full.ForecastProduction());
        Assert.Equal(ProductionBlockReason.MissingInputs | ProductionBlockReason.StorageFull, blocked.BlockReason);
        Assert.Equal(new[] { "logs" }, blocked.MissingInputIds);
        Assert.Equal(new[] { "planks" }, blocked.FullOutputIds);
        full.Resume();
        full.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(ResourceBundle.Create(("planks", 1m)), full.Resources);
    }

    [Fact]
    public void CapacityFreedByConsumptionIsAvailableOnlyOnTheNextStep()
    {
        var scenario = TestScenarios.Create(
            [TestScenarios.Building("producer", output: ResourceBundle.Create(("logs", 1m)), priority: 1),
             TestScenarios.Building("processor", ResourceBundle.Create(("logs", 1m)), ResourceBundle.Create(("planks", 1m)), priority: 2)],
            ResourceBundle.Create(("logs", 1m)), ResourceBundle.Create(("logs", 1m), ("planks", 10m)));
        var game = GameSession.Create(scenario);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(ResourceBundle.Create(("planks", 1m)), game.Resources);
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(ResourceBundle.Create(("logs", 1m), ("planks", 1m)), game.Resources);
    }
}
