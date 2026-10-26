using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Content;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class ScenarioCatalogTests
{
    public static TheoryData<string> Scenarios => ["economy-foundation", "civil-growth"];

    [Theory]
    [MemberData(nameof(Scenarios))]
    public void EveryShippedBuildingIsReachableAndEveryRecipeCanRun(string id)
    {
        var scenario = id == "civil-growth" ? CivilGrowthScenario.Create() : EconomyFoundationScenario.Create();
        var report = CatalogReachability.Analyze(scenario);
        Assert.Empty(report.Issues);
        Assert.Equal(scenario.Catalog.Buildings.Select(item => item.Id), report.ReachableBuildings);
        Assert.Equal(scenario.Catalog.Resources.Select(item => item.Id), report.RenewableResources);
    }

    [Fact]
    public void EveryRulesChangeOnlyAddsBuildingsOfTheCurrentCatalog()
    {
        var catalog = CivilGrowthScenario.Create().Catalog;
        var added = CivilGrowthScenario.Changes.SelectMany(item => item.AddedBuildings).ToArray();
        Assert.Equal(added.Length, added.Distinct().Count());
        Assert.All(added, id => Assert.True(catalog.BuildingsById.ContainsKey(id)));
        Assert.Equal(CivilGrowthScenario.RulesVersion, CivilGrowthScenario.Changes[^1].To);
        for (var index = 1; index < CivilGrowthScenario.Changes.Length; index++)
            Assert.Equal(CivilGrowthScenario.Changes[index - 1].To, CivilGrowthScenario.Changes[index].From);
    }
}
