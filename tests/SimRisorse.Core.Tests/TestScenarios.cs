using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Core.Tests;

internal static class TestScenarios
{
    public static BuildingDefinition Building(string id, ResourceBundle? input = null, ResourceBundle? output = null,
        int priority = 10, ResourceBundle? cost = null, int workplaces = 1, params string[] prerequisites) =>
        new(id, id, cost ?? ResourceBundle.Empty, workplaces,
            output is null ? null : new(input ?? ResourceBundle.Empty, output), priority, prerequisites.ToImmutableArray());

    public static GameScenario Create(IEnumerable<BuildingDefinition> buildings, ResourceBundle stock,
        ResourceBundle? capacity = null, int population = 8, IEnumerable<BuildingState>? initial = null)
    {
        var definitions = buildings.ToArray();
        var catalog = new GameCatalog("test-1", [new("logs", "Logs"), new("planks", "Planks"), new("stone", "Stone")], definitions);
        return new("test", catalog, population, capacity ?? ResourceBundle.Create(("logs", 1000m), ("planks", 1000m), ("stone", 1000m)),
            stock, initial ?? definitions.Select(item => new BuildingState(item.Id, 1, item.Recipe is null ? 0 : 1)));
    }

    public static GameScenario Economy() => Create(
        [
            Building("tree-feller", output: ResourceBundle.Create(("logs", 2m)), cost: ResourceBundle.Create(("planks", 20m), ("stone", 10m)), priority: 10),
            Building("sawmill", ResourceBundle.Create(("logs", 2m)), ResourceBundle.Create(("planks", 1m)), cost: ResourceBundle.Create(("planks", 40m), ("stone", 20m)), priority: 20),
            Building("stonecutter", output: ResourceBundle.Create(("stone", 1m)), cost: ResourceBundle.Create(("planks", 30m), ("stone", 10m)), priority: 30)
        ], ResourceBundle.Create(("planks", 120m), ("stone", 80m)));

    public static void EqualState(GameSnapshot expected, GameSnapshot actual)
    {
        Assert.Equal(expected.FormatVersion, actual.FormatVersion);
        Assert.Equal(expected.ScenarioId, actual.ScenarioId);
        Assert.Equal(expected.RulesVersion, actual.RulesVersion);
        Assert.Equal(expected.ElapsedTicks, actual.ElapsedTicks);
        Assert.Equal(expected.Population, actual.Population);
        Assert.Equal(expected.GrowthProgressSeconds, actual.GrowthProgressSeconds);
        Assert.Equal(expected.Resources, actual.Resources);
        Assert.Equal(expected.Buildings.ToArray(), actual.Buildings.ToArray());
    }
}
