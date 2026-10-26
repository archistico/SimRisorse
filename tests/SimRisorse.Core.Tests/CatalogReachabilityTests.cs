using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class CatalogReachabilityTests
{
    [Fact]
    public void ReferenceEconomyIsCompleteAndRenewsAllItsResources()
    {
        var report = CatalogReachability.Analyze(TestScenarios.Economy());
        Assert.True(report.IsComplete);
        Assert.Equal(new[] { "logs", "planks", "stone" }, report.RenewableResources);
        Assert.Equal(3, report.ReachableBuildings.Length);
    }

    [Fact]
    public void AnUnobtainableCostBlocksTheBuildingAndEveryDependent()
    {
        var report = Analyze(
            [
                Producer("quarry", output: ("stone", 1m)),
                Building("vault", cost: ResourceBundle.Create(("logs", 5m))),
                Building("tower", prerequisites: "vault"),
                Building("hall", prerequisites: "tower")
            ], owned: ["quarry"]);
        Assert.False(report.IsComplete);
        Assert.Equal(new[] { "quarry" }, report.ReachableBuildings);
        Assert.Equal(new[]
        {
            new CatalogIssue("hall", CatalogIssueKind.PrerequisiteUnreachable, "tower"),
            new CatalogIssue("tower", CatalogIssueKind.PrerequisiteUnreachable, "vault"),
            new CatalogIssue("vault", CatalogIssueKind.CostNotObtainable, "logs")
        }, report.Issues);
    }

    [Fact]
    public void InitialStockMayPayACostButNeverMakesARecipeInputRenewable()
    {
        var report = Analyze(
            [
                Building("statue", cost: ResourceBundle.Create(("planks", 50m))),
                Building("monument", cost: ResourceBundle.Create(("planks", 51m))),
                Producer("sawmill", input: ("logs", 1m), output: ("planks", 1m))
            ], stock: ResourceBundle.Create(("planks", 50m), ("logs", 500m)));
        Assert.Contains("statue", report.ReachableBuildings);
        Assert.Contains(new CatalogIssue("monument", CatalogIssueKind.CostNotObtainable, "planks"), report.Issues);
        Assert.Contains(new CatalogIssue("sawmill", CatalogIssueKind.InputNotObtainable, "logs"), report.Issues);
        Assert.Empty(report.RenewableResources);
    }

    [Fact]
    public void LargeCostsAndRecipesNeedAReachableStorageExpansion()
    {
        BuildingDefinition[] buildings =
        [
            Producer("quarry", output: ("stone", 1m)),
            Producer("crusher", output: ("logs", 1200m)),
            Building("castle", cost: ResourceBundle.Create(("stone", 1500m)))
        ];
        var limited = Analyze(buildings, owned: ["quarry"]);
        Assert.Contains(new CatalogIssue("castle", CatalogIssueKind.CostExceedsStorage, "stone"), limited.Issues);
        Assert.Contains(new CatalogIssue("crusher", CatalogIssueKind.RecipeExceedsStorage, "logs"), limited.Issues);

        var storage = ResourceBundle.Create(("logs", 500m), ("stone", 500m));
        var expanded = Analyze([.. buildings, Building("warehouse", cost: ResourceBundle.Create(("stone", 100m))) with { StoragePerBuilding = storage }],
            owned: ["quarry"]);
        Assert.True(expanded.IsComplete);
        Assert.Contains("castle", expanded.ReachableBuildings);
        Assert.Contains("logs", expanded.RenewableResources);
    }

    [Fact]
    public void OwnedBuildingsAreReachableEvenWhenTheirCostIsNot() => Assert.True(Analyze(
        [Producer("quarry", output: ("stone", 1m)) with { Cost = ResourceBundle.Create(("logs", 10m)) }], owned: ["quarry"]).IsComplete);

    private static ReachabilityReport Analyze(IEnumerable<BuildingDefinition> buildings, ResourceBundle? stock = null, string[]? owned = null)
    {
        var definitions = buildings.ToArray();
        return CatalogReachability.Analyze(TestScenarios.Create(definitions, stock ?? ResourceBundle.Empty,
            initial: definitions.Select(item => new BuildingState(item.Id, owned?.Contains(item.Id) == true ? 1 : 0, 0))));
    }

    private static BuildingDefinition Building(string id, ResourceBundle? cost = null, params string[] prerequisites) =>
        TestScenarios.Building(id, cost: cost, workplaces: 0, prerequisites: prerequisites);

    private static BuildingDefinition Producer(string id, (string Id, decimal Units)? input = null, (string Id, decimal Units) output = default) =>
        TestScenarios.Building(id, input is { } value ? ResourceBundle.Create(value) : null, ResourceBundle.Create(output));
}
