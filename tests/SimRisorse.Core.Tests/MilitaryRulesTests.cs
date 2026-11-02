using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class MilitaryRulesTests
{
    [Theory]
    [InlineData(0, 30, 1)]
    [InlineData(5, 0, 1)]
    [InlineData(5, 30, 0)]
    [InlineData(5, -1, 1)]
    public void RulesNeedPositiveEquipmentTimeAndPlaces(int swords, int seconds, int places) =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MilitaryRules("barracks", "planks", ResourceAmount.FromUnits(swords), seconds, places));

    [Theory]
    [InlineData("unknown-resource")]
    [InlineData("unknown-building")]
    [InlineData("with-workplaces")]
    [InlineData("without-growth")]
    public void ScenarioRejectsInconsistentRecruitment(string problem) =>
        Assert.Throws<ArgumentException>(() => Scenario(problem));

    [Fact]
    public void ConsistentRecruitmentStartsWithAnEmptyArmy()
    {
        var game = GameSession.Create(Scenario("none"));
        Assert.Equal(MilitaryState.Empty, game.Military);
        Assert.Equal(1, game.TrainingPlaces);
        Assert.Equal(8, game.AvailableWorkers);
        Assert.True(game.Enlist(2).Succeeded);
        Assert.Equal(6, game.AvailableWorkers);
        Assert.Equal(90m, game.Resources["planks"].Units);
        Assert.Equal(new MilitaryState(1, [0], 0), game.Military);
    }

    private static GameScenario Scenario(string problem)
    {
        var house = new BuildingDefinition("house", "House", ResourceBundle.Empty, 0, null, 5, [], HousingPerBuilding: 8);
        var barracks = new BuildingDefinition("barracks", "Barracks", ResourceBundle.Empty,
            problem == "with-workplaces" ? 1 : 0, null, 10, []);
        var catalog = new GameCatalog("test-military", [new("planks", "Planks"), new("food", "Food")], [house, barracks]);
        var rules = new MilitaryRules(problem == "unknown-building" ? "missing" : "barracks",
            problem == "unknown-resource" ? "iron" : "planks", ResourceAmount.FromUnits(5m), 30);
        var population = problem == "without-growth" ? null : new PopulationRules("food", ResourceAmount.FromUnits(0.01m), 60);
        return new GameScenario("test", catalog, 8, ResourceBundle.Create(("planks", 100m), ("food", 100m)),
            ResourceBundle.Create(("planks", 100m), ("food", 10m)),
            [new BuildingState("house", 1, 0), new BuildingState("barracks", 1, 0)], population, rules);
    }

    [Fact]
    public void ScenarioWithoutRecruitmentHasNoArmyAndRejectsOrders()
    {
        var game = GameSession.Create(TestScenarios.Economy());
        Assert.Null(game.Military);
        Assert.Equal(0, game.TrainingPlaces);
        Assert.Equal(GameError.MilitaryUnavailable, game.Enlist(1).Error);
        Assert.Equal(GameError.MilitaryUnavailable, game.CancelRecruits(1).Error);
    }
}
