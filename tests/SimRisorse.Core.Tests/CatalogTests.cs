using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class CatalogTests
{
    [Fact]
    public void FixedPointAmountsRejectSilentRoundingNegativeValuesAndOverflow()
    {
        Assert.Equal(1234, ResourceAmount.FromUnits(1.234m).Milliunits);
        Assert.Throws<ArgumentOutOfRangeException>(() => ResourceAmount.FromUnits(1.2345m));
        Assert.Throws<ArgumentOutOfRangeException>(() => ResourceAmount.FromUnits(-1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => ResourceAmount.FromUnits(decimal.MaxValue));
        Assert.Throws<OverflowException>(() => ResourceAmount.FromMilliunits(long.MaxValue) * 2);
        Assert.Throws<ArgumentOutOfRangeException>(() => ResourceBundle.Empty.Scale(-1));
    }

    [Fact]
    public void DuplicateAndUnknownResourceReferencesAreRejected()
    {
        var building = TestScenarios.Building("producer", output: ResourceBundle.Create(("logs", 1m)));
        Assert.Throws<ArgumentException>(() => new GameCatalog("v1", [new("logs", "Logs"), new("logs", "Duplicate")], [building]));
        Assert.Throws<ArgumentException>(() => new GameCatalog("v1", [new("stone", "Stone")], [building]));
        Assert.Throws<ArgumentException>(() => new GameCatalog("v1", [new("logs", "Logs")], [building, building]));
        Assert.Throws<ArgumentException>(() => ResourceBundle.Create(("logs", 1m), ("logs", 2m)));
    }

    [Theory]
    [InlineData("Logs")]
    [InlineData("1logs")]
    [InlineData("logs stone")]
    public void ResourceIdentifiersAreStableAndCaseSensitive(string id) =>
        Assert.Throws<ArgumentException>(() => ResourceBundle.Create((id, 1m)));

    [Fact]
    public void MissingOrCyclicPrerequisitesAreRejected()
    {
        var resources = new ResourceDefinition[] { new("logs", "Logs") };
        Assert.Throws<ArgumentException>(() => new GameCatalog("v1", resources, [TestScenarios.Building("alpha", prerequisites: ["missing"])]));
        Assert.Throws<ArgumentException>(() => new GameCatalog("v1", resources,
            [TestScenarios.Building("alpha", prerequisites: ["beta"]), TestScenarios.Building("beta", prerequisites: ["alpha"])]));
    }

    [Fact]
    public void InvalidRecipeAndPopulationDefinitionsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => TestScenarios.Create([TestScenarios.Building("producer", output: ResourceBundle.Empty)], ResourceBundle.Empty));
        Assert.Throws<ArgumentException>(() => TestScenarios.Create([TestScenarios.Building("producer", output: ResourceBundle.Create(("logs", 1m)), workplaces: 0)], ResourceBundle.Empty));
        Assert.Throws<ArgumentException>(() => TestScenarios.Create([TestScenarios.Building("producer")], ResourceBundle.Create(("logs", 2m)), ResourceBundle.Create(("logs", 1m))));
        Assert.Throws<ArgumentException>(() => TestScenarios.Create([TestScenarios.Building("producer", output: ResourceBundle.Create(("logs", 1m)))],
            ResourceBundle.Empty, population: 0));
    }

    [Fact]
    public void OmittedBuildingsStartAtZeroAndDoNotNeedMutablePlaceholders()
    {
        var scenario = TestScenarios.Create([TestScenarios.Building("empty")], ResourceBundle.Empty, initial: Array.Empty<BuildingState>());
        Assert.Equal(new BuildingState("empty", 0, 0), Assert.Single(scenario.InitialBuildings));
    }
}
