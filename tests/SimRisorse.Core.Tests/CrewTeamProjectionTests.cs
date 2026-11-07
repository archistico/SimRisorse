using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class CrewTeamProjectionTests
{
    private static readonly ResourceBundle Capacity = ResourceBundle.Create(
        ("gold", 100m), ("coal", 100m), ("coins", 100m), ("swords", 100m));
    private static readonly ResourceBundle Initial = ResourceBundle.Create(("gold", 20m), ("coal", 20m));

    private static GameCatalog Catalog() => new("e4b-rc80-test-1",
        [new ResourceDefinition("gold", "Gold"), new("coal", "Coal"),
            new("coins", "Coins"), new("swords", "Swords")],
        [new BuildingDefinition("workshop", "Workshop", ResourceBundle.Empty, 1,
            new ProductionRecipe(ResourceBundle.Create(("coal", 1m)),
                ResourceBundle.Create(("swords", 1m))), 5, ImmutableArray<string>.Empty),
         new BuildingDefinition("mint", "Mint", ResourceBundle.Empty, 1,
            new ProductionRecipe(ResourceBundle.Create(("gold", 1m), ("coal", 1m)),
                ResourceBundle.Create(("coins", 3m))), 10, ImmutableArray<string>.Empty)]);

    private static CrewProductionTick Step(CrewTeamPortfolio mint, ResourceBundle? resources = null,
        ResourceBundle? capacity = null, int workshopWorkers = 0, int residents = 200,
        int reservedOutside = 0) => CrewProductionProjection.SimulateSecond(
            Catalog(), resources ?? Initial, capacity ?? Capacity,
            [new BuildingState("workshop", 1, workshopWorkers)], [mint], residents, reservedOutside);

    private static CrewTeamPortfolio Base(int buildings, int assigned) =>
        CrewTeamPortfolio.Create("mint", 10, new(buildings, assigned));

    [Theory]
    [InlineData(0, 0, 0, 10)]
    [InlineData(9, 0, 9, 1)]
    [InlineData(10, 1, 0, 10)]
    [InlineData(19, 1, 9, 1)]
    [InlineData(20, 2, 0, 0)]
    public void TwoBasicMintsProduceOnlyWithFullIndependentTeams(
        int assigned, int runnable, int inactive, int missing)
    {
        var result = Step(Base(2, assigned));
        var status = Assert.Single(result.CrewStatus);
        Assert.Equal(2, status.OwnedModules);
        Assert.Equal(assigned, status.AssignedWorkers);
        Assert.Equal(runnable, status.StaffedModules);
        Assert.Equal(runnable, status.RunnableModules);
        Assert.Equal(inactive, status.IncompleteCrewWorkers);
        Assert.Equal(missing, status.MissingForNextModule);
        Assert.Equal(20m - runnable, result.NextResources["gold"].Units);
        Assert.Equal(20m - runnable, result.NextResources["coal"].Units);
        Assert.Equal(3m * runnable, result.NextResources["coins"].Units);
        Assert.Equal(ResourceBundle.Create(("coins", 3m * runnable)), status.Produced);
        Assert.Equal(ResourceBundle.Create(("gold", (decimal)runnable), ("coal", (decimal)runnable)), status.Consumed);
        var report = result.Production.Single(p => p.BuildingId == "mint");
        Assert.Equal(assigned, report.AssignedWorkers);
        Assert.Equal(runnable, report.RunnableWorkers);
        Assert.Equal(runnable < 2, report.BlockReason.HasFlag(ProductionBlockReason.InsufficientCrew));
    }

    [Theory]
    [InlineData(ProductionTier.Base, 1, 10)]
    [InlineData(ProductionTier.Large, 3, 30)]
    [InlineData(ProductionTier.Advanced, 10, 100)]
    public void AFullTierActivatesItsOneThreeOrTenModules(ProductionTier tier, int modules, int people)
    {
        var single = tier switch
        {
            ProductionTier.Base => CrewTeamPortfolio.Create("mint", 10, new(1, people)),
            ProductionTier.Large => CrewTeamPortfolio.Create("mint", 10, default, new(1, people)),
            _ => CrewTeamPortfolio.Create("mint", 10, default, default, new(1, people))
        };
        var result = Step(single);
        var status = Assert.Single(result.CrewStatus);
        Assert.Equal(modules, single.TotalModules);
        Assert.Equal(modules, status.RunnableModules);
        Assert.Equal(3m * modules, result.NextResources["coins"].Units);
        Assert.Equal(20m - modules, result.NextResources["gold"].Units);
    }

    [Fact]
    public void PersonnelInDifferentTiersCannotBePooledIntoOneCrew()
    {
        var split = CrewTeamPortfolio.Create("mint", 10, new(1, 9), new(1, 9));
        Assert.Equal(0, split.StaffedModules);
        Assert.Equal(18, split.IncompleteCrewWorkers);
        Assert.Equal(1, split.MissingForNextModule);
        Assert.Equal(0, Assert.Single(Step(split).CrewStatus).RunnableModules);
        var next = split.Assign(ProductionTier.Large, 1, freePopulation: 1);
        Assert.Equal(1, next.StaffedModules);
        Assert.Equal(0, split.StaffedModules);
        Assert.Equal(9, next.IncompleteCrewWorkers);
        Assert.Equal(3m, Step(next).NextResources["coins"].Units);
    }

    [Fact]
    public void InputShortageBlocksOnlyUnaffordableModulesAndNeverSpendsPartialCrewInputs()
    {
        var result = Step(Base(2, 20), ResourceBundle.Create(("gold", 1m), ("coal", 20m)));
        var mint = result.Production.Single(p => p.BuildingId == "mint");
        Assert.True(mint.BlockReason.HasFlag(ProductionBlockReason.MissingInputs));
        Assert.False(mint.BlockReason.HasFlag(ProductionBlockReason.InsufficientCrew));
        Assert.Equal(new[] { "gold" }, mint.MissingInputIds);
        Assert.Equal(1, Assert.Single(result.CrewStatus).RunnableModules);
        Assert.Equal(0m, result.NextResources["gold"].Units);
        Assert.Equal(19m, result.NextResources["coal"].Units);
        Assert.Equal(3m, result.NextResources["coins"].Units);
    }

    [Fact]
    public void FullCoinStorageBlocksMintWithoutConsumingGoldOrCoal()
    {
        var initial = ResourceBundle.Create(("gold", 20m), ("coal", 20m), ("coins", 99m));
        var result = Step(Base(2, 20), initial);
        var report = result.Production.Single(p => p.BuildingId == "mint");
        Assert.Equal(0, report.RunnableWorkers);
        Assert.True(report.BlockReason.HasFlag(ProductionBlockReason.StorageFull));
        Assert.Equal(new[] { "coins" }, report.FullOutputIds);
        Assert.Equal(initial, result.NextResources);
        Assert.Equal(ResourceBundle.Empty, Assert.Single(result.CrewStatus).Consumed);
    }

    [Fact]
    public void ExistingProducerGetsInputPriorityAndConsumesItsOwnWorkforceOnlyOnce()
    {
        var result = Step(Base(1, 10), ResourceBundle.Create(("gold", 1m), ("coal", 1m)),
            workshopWorkers: 1, residents: 11);
        Assert.Equal(1, result.Production.Single(p => p.BuildingId == "workshop").RunnableWorkers);
        var mint = result.Production.Single(p => p.BuildingId == "mint");
        Assert.Equal(0, mint.RunnableWorkers);
        Assert.True(mint.BlockReason.HasFlag(ProductionBlockReason.MissingInputs));
        Assert.Equal(ResourceBundle.Create(("gold", 1m), ("swords", 1m)), result.NextResources);
    }

    [Fact]
    public void OptedInCrewSizeOneKeepsLegacySingleWorkerSemantics()
    {
        var crew = CrewTeamPortfolio.Create("mint", 1, new(2, 2));
        var result = Step(crew);
        Assert.Equal(2, Assert.Single(result.CrewStatus).RunnableModules);
        Assert.Equal(6m, result.NextResources["coins"].Units);
        Assert.Equal(0, (int)(result.Production.Single(p => p.BuildingId == "mint").BlockReason &
            ProductionBlockReason.InsufficientCrew));
    }

    [Fact]
    public void NoWorkersAndMissingCrewAreDifferentDiagnostics()
    {
        var empty = Step(Base(1, 0)).Production.Single(p => p.BuildingId == "mint");
        Assert.True(empty.BlockReason.HasFlag(ProductionBlockReason.NoWorkers));
        Assert.True(empty.BlockReason.HasFlag(ProductionBlockReason.InsufficientCrew));
        var partial = Step(Base(1, 9)).Production.Single(p => p.BuildingId == "mint");
        Assert.False(partial.BlockReason.HasFlag(ProductionBlockReason.NoWorkers));
        Assert.True(partial.BlockReason.HasFlag(ProductionBlockReason.InsufficientCrew));
        Assert.False(partial.BlockReason.HasFlag(ProductionBlockReason.MissingInputs));
    }

    [Fact]
    public void BuildAssignAndReleaseAreImmutableAndValidateCapacity()
    {
        var before = Base(1, 9);
        var afterBuild = before.Build(ProductionTier.Base, 1);
        var staffed = afterBuild.Assign(ProductionTier.Base, 11, freePopulation: 11);
        Assert.Equal(1, before.TotalModules);
        Assert.Equal(2, staffed.TotalModules);
        Assert.Equal(2, staffed.StaffedModules);
        Assert.Equal(9, before.AssignedWorkers);
        Assert.Equal(19, staffed.Release(ProductionTier.Base, 1).AssignedWorkers);
        Assert.Throws<InvalidOperationException>(() => before.Assign(ProductionTier.Base, 2, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => before.Assign(ProductionTier.Base, 2, 2));
        Assert.Throws<InvalidOperationException>(() => before.Release(ProductionTier.Base, 10));
    }

    [Fact]
    public void PopulationBudgetIncludesWorkersReservedForFoodResearchAndMilitary()
    {
        Assert.Throws<InvalidOperationException>(() => Step(Base(1, 10), residents: 10, reservedOutside: 1));
        Assert.Equal(1, Assert.Single(Step(Base(1, 10), residents: 11, reservedOutside: 1).CrewStatus).RunnableModules);
    }

    [Fact]
    public void InvalidRostersAndPortfolioOverflowsAreRejectedBeforeProduction()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Base(1, 11));
        Assert.Throws<ArgumentOutOfRangeException>(() => CrewTeamPortfolio.Create("mint", 0, new(1, 0)));
        Assert.Throws<OverflowException>(() => CrewTeamPortfolio.Create("mint", 10,
            default, default, new(int.MaxValue, 0)));
        Assert.Throws<ArgumentException>(() => CrewProductionProjection.SimulateSecond(
            Catalog(), Initial, Capacity, [new BuildingState("mint", 1, 0)], [Base(1, 10)], 20));
        Assert.Throws<ArgumentException>(() => CrewProductionProjection.SimulateSecond(
            Catalog(), Initial, Capacity, [], [Base(1, 10)], 20));
        Assert.Throws<ArgumentException>(() => CrewProductionProjection.SimulateSecond(
            Catalog(), Initial, Capacity, [new BuildingState("workshop", 1, 0)],
            [CrewTeamPortfolio.Create("nonexistent", 10, new(1, 10))], 20));
    }

    [Fact]
    public void TwoIndependentStepsSpendExactlyTwoCompleteRecipesWithoutMutation()
    {
        var crew = Base(2, 19);
        var first = Step(crew);
        var second = Step(crew, first.NextResources);
        Assert.Equal(18m, second.NextResources["gold"].Units);
        Assert.Equal(18m, second.NextResources["coal"].Units);
        Assert.Equal(6m, second.NextResources["coins"].Units);
        Assert.Equal(20m, Initial["gold"].Units); // immutable input remains available
        Assert.Equal(19, crew.AssignedWorkers);
    }
}
