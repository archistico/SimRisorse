using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class TieredBuildingsTests
{
    private static readonly BuildingDefinition Sawmill = new("sawmill", "Segheria",
        ResourceBundle.Create(("planks", 40m), ("stone", 20m)), 1,
        new(ResourceBundle.Create(("logs", 2m)), ResourceBundle.Create(("planks", 1m))), 20, [],
        StoragePerBuilding: ResourceBundle.Create(("planks", 50m)));

    private static TieredProductionFamily Family() => new(Sawmill, "lavorazioni-1", "lavorazioni-2", 2.6m, 6.5m);

    private static readonly IReadOnlySet<string> BaseOnly = new HashSet<string>(StringComparer.Ordinal);
    private static readonly IReadOnlySet<string> LargeUnlocked = new HashSet<string>(StringComparer.Ordinal) { "lavorazioni-1" };
    private static readonly IReadOnlySet<string> AllUnlocked = new HashSet<string>(StringComparer.Ordinal) { "lavorazioni-1", "lavorazioni-2" };

    [Fact]
    public void ThreeTiersKeepRecipeAndWorkplaceMultipliersWithoutHiddenWorkerBonuses()
    {
        var family = Family();
        Assert.Equal(1, family.Get(ProductionTier.Base).Workplaces);
        Assert.Equal(3, family.Get(ProductionTier.Large).Workplaces);
        Assert.Equal(10, family.Get(ProductionTier.Advanced).Workplaces);
        Assert.Same(Sawmill.Recipe, family.Source.Recipe);
        Assert.False(family.IsUnlocked(ProductionTier.Large, BaseOnly));
        Assert.True(family.IsUnlocked(ProductionTier.Large, LargeUnlocked));
        Assert.False(family.IsUnlocked(ProductionTier.Advanced, LargeUnlocked));
        Assert.True(family.IsUnlocked(ProductionTier.Advanced, AllUnlocked));
    }

    [Fact]
    public void LegacyProducerImportsAsBaseWithoutDuplicatingWorkers()
    {
        var imported = TieredBuildingPortfolio.FromLegacy(Sawmill, 2, 2);
        Assert.Equal(2, imported.Base.Buildings);
        Assert.Equal(0, imported.Large.Buildings);
        Assert.Equal(0, imported.Advanced.Buildings);
        Assert.Equal(2, imported.AssignedWorkers);
        Assert.Equal(2, imported.TotalWorkplaces);
        Assert.Equal(2, imported.StorageEquivalents);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TieredBuildingPortfolio.Create("sawmill", new TierOccupancy(1, 2), default, default));
    }

    [Fact]
    public void UpgradeKeepsWorkersAndCountWhileReleasingOnlyNewVacancies()
    {
        var start = TieredBuildingPortfolio.FromLegacy(Sawmill, 1, 1);
        var large = start.Upgrade(ProductionTier.Base, ProductionTier.Large);
        Assert.Equal(0, large.Base.Buildings);
        Assert.Equal(1, large.Large.Buildings);
        Assert.Equal(1, large.AssignedWorkers);
        Assert.Equal(3, large.TotalWorkplaces);
        Assert.Equal(2, large.TotalWorkplaces - large.AssignedWorkers);
        var advanced = large.Upgrade(ProductionTier.Large, ProductionTier.Advanced);
        Assert.Equal(1, advanced.Advanced.Buildings);
        Assert.Equal(10, advanced.TotalWorkplaces);
        Assert.Equal(1, advanced.AssignedWorkers);
        Assert.Equal(1, start.Base.Buildings); // source immutable
    }

    [Fact]
    public void UpgradeBetweenAggregatedTiersMovesAtMostTheWorkersOfOneBuilding()
    {
        var before = TieredBuildingPortfolio.Create("sawmill", default, new(2, 4), default);
        var after = before.Upgrade(ProductionTier.Large, ProductionTier.Advanced);
        Assert.Equal(new TierOccupancy(1, 1), after.Large);
        Assert.Equal(new TierOccupancy(1, 3), after.Advanced);
        Assert.Equal(4, after.AssignedWorkers);
        Assert.Equal(2, after.TotalBuildings);
    }

    [Fact]
    public void TieredAssignmentsRespectActualVacanciesAndAvailablePopulation()
    {
        var empty = TieredBuildingPortfolio.Empty("sawmill").Build(ProductionTier.Large, 1);
        var two = empty.Assign(ProductionTier.Large, 2, 2);
        Assert.Equal(2, two.AssignedWorkers);
        Assert.Equal(0, empty.AssignedWorkers);
        Assert.Throws<InvalidOperationException>(() => two.Assign(ProductionTier.Large, 2, 10));
        Assert.Throws<InvalidOperationException>(() => two.Assign(ProductionTier.Large, 1, 0));
        var one = two.Release(ProductionTier.Large, 1);
        Assert.Equal(1, one.AssignedWorkers);
        Assert.Throws<InvalidOperationException>(() => one.Release(ProductionTier.Large, 2));
    }

    [Fact]
    public void NewLargeConstructionRequiresResearchAndHasProgressiveFamilyPrice()
    {
        var family = Family();
        var portfolio = TieredBuildingPortfolio.FromLegacy(Sawmill, 1, 1);
        var history = ConstructionAcquisitionHistory.FromOwned([("sawmill", 1)]);
        var resources = ResourceBundle.Create(("planks", 500m), ("stone", 500m));
        var locked = TieredConstruction.TryBuild(family, portfolio, ProductionTier.Large, 1, 1.15m,
            BaseOnly, resources, history);
        Assert.Equal(TieredPurchaseFailure.MissingResearch, locked.Failure);
        Assert.Same(portfolio, locked.Portfolio);
        Assert.Same(resources, locked.Resources);
        var bought = TieredConstruction.TryBuild(family, portfolio, ProductionTier.Large, 1, 1.15m,
            LargeUnlocked, resources, history);
        Assert.True(bought.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 120m), ("stone", 60m)), bought.Cost);
        Assert.Equal(2, bought.History.Acquired("sawmill"));
        Assert.Equal(1, bought.Portfolio.Base.Buildings);
        Assert.Equal(1, bought.Portfolio.Large.Buildings);
        Assert.Equal(4, bought.Portfolio.TotalWorkplaces);
        Assert.Equal(1, bought.Portfolio.AssignedWorkers);
    }

    [Fact]
    public void DirectAdvancedAndBaseUpgradeChargeExpectedPricesWithoutDuplicatingConstruction()
    {
        var family = Family();
        var before = TieredBuildingPortfolio.FromLegacy(Sawmill, 1, 1);
        var history = ConstructionAcquisitionHistory.FromOwned([("sawmill", 1)]);
        var resources = ResourceBundle.Create(("planks", 500m), ("stone", 500m));
        var direct = TieredConstruction.TryBuild(family, before, ProductionTier.Advanced, 1,
            1.15m, AllUnlocked, resources, history);
        Assert.True(direct.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 299m), ("stone", 150m)), direct.Cost);
        Assert.Equal(2, direct.History.Acquired("sawmill"));
        Assert.Equal(11, direct.Portfolio.TotalWorkplaces);
        var upgraded = TieredConstruction.TryUpgrade(family, before, ProductionTier.Base, ProductionTier.Advanced,
            1.15m, AllUnlocked, resources, history);
        Assert.True(upgraded.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 253m), ("stone", 127m)), upgraded.Cost);
        Assert.Same(history, upgraded.History);
        Assert.Equal(1, upgraded.Portfolio.TotalBuildings);
        Assert.Equal(10, upgraded.Portfolio.TotalWorkplaces);
        Assert.Equal(1, upgraded.Portfolio.AssignedWorkers);
    }

    [Fact]
    public void FailedUpgradeNeverSpendsResourcesOrChangesWorkerState()
    {
        var family = Family();
        var portfolio = TieredBuildingPortfolio.FromLegacy(Sawmill, 1, 1);
        var history = ConstructionAcquisitionHistory.FromOwned([("sawmill", 1)]);
        var resources = ResourceBundle.Create(("planks", 1m));
        var noMoney = TieredConstruction.TryUpgrade(family, portfolio, ProductionTier.Base,
            ProductionTier.Large, 1.15m, LargeUnlocked, resources, history);
        Assert.Equal(TieredPurchaseFailure.InsufficientResources, noMoney.Failure);
        Assert.Same(portfolio, noMoney.Portfolio);
        Assert.Same(history, noMoney.History);
        Assert.Same(resources, noMoney.Resources);
        var locked = TieredConstruction.TryUpgrade(family, portfolio, ProductionTier.Base,
            ProductionTier.Advanced, 1.15m, LargeUnlocked, resources, history);
        Assert.Equal(TieredPurchaseFailure.MissingResearch, locked.Failure);
        var missing = TieredConstruction.TryUpgrade(family, portfolio, ProductionTier.Large,
            ProductionTier.Advanced, 1.15m, AllUnlocked, resources, history);
        Assert.Equal(TieredPurchaseFailure.MissingSourceBuilding, missing.Failure);
    }

    [Fact]
    public void LargePlusAdvancedStorageIsProportionalToSlotsEvenWhenUnstaffed()
    {
        var portfolio = TieredBuildingPortfolio.Create("sawmill", new(1, 1), new(1, 0), new(1, 0));
        Assert.Equal(14, portfolio.TotalWorkplaces);
        Assert.Equal(1, portfolio.AssignedWorkers);
        Assert.Equal(14, portfolio.StorageEquivalents);
        var outputCapacity = Sawmill.StoragePerBuilding!.Scale(portfolio.StorageEquivalents);
        Assert.Equal(700m, outputCapacity["planks"].Units);
    }

    [Fact]
    public void TieredProductionUsesExistingPlannerAndRealAssignedWorkers()
    {
        var catalog = new GameCatalog("trial", [new("logs", "Tronchi"), new("planks", "Legno lavorato"), new("stone", "Pietra")], [Sawmill]);
        var portfolio = TieredBuildingPortfolio.Create("sawmill", default, default, new(1, 10));
        var before = ResourceBundle.Create(("logs", 25m));
        var result = TieredEconomyProjection.SimulateSecond(catalog, before,
            ResourceBundle.Create(("logs", 100m)), [], [portfolio]);
        Assert.Equal(500m, result.StorageCapacity["planks"].Units);
        Assert.Equal(5m, result.NextResources["logs"].Units);
        Assert.Equal(10m, result.NextResources["planks"].Units);
        Assert.Equal(10, Assert.Single(result.Production).RunnableWorkers);
        // An under-staffed advanced mill has no automatic 10x output.
        var shortStaffed = TieredBuildingPortfolio.Create("sawmill", default, default, new(1, 4));
        var limited = TieredEconomyProjection.SimulateSecond(catalog, before,
            ResourceBundle.Create(("logs", 100m)), [], [shortStaffed]);
        Assert.Equal(4m, limited.NextResources["planks"].Units);
        Assert.Equal(17m, limited.NextResources["logs"].Units);
    }

    [Fact]
    public void InputsAreContendedUsingOneSharedInventoryAndCapacityCapsOutput()
    {
        var collector = new BuildingDefinition("collector", "Altro produttore", ResourceBundle.Empty, 1,
            new(ResourceBundle.Create(("logs", 1m)), ResourceBundle.Create(("stone", 1m))), 10, []);
        var catalog = new GameCatalog("trial", [new("logs", "Logs"), new("planks", "Planks"), new("stone", "Stone")], [collector, Sawmill]);
        var portfolio = TieredBuildingPortfolio.Create("sawmill", default, default, new(1, 10));
        var result = TieredEconomyProjection.SimulateSecond(catalog,
            ResourceBundle.Create(("logs", 21m)), ResourceBundle.Create(("logs", 50m), ("stone", 50m)),
            [new BuildingState("collector", 1, 1)], [portfolio]);
        Assert.Equal(0m, result.NextResources["logs"].Units);
        Assert.Equal(1m, result.NextResources["stone"].Units);
        Assert.Equal(10m, result.NextResources["planks"].Units);
        var full = TieredEconomyProjection.SimulateSecond(catalog,
            ResourceBundle.Create(("logs", 21m), ("planks", 499m)), ResourceBundle.Create(("logs", 50m), ("stone", 50m)),
            [new BuildingState("collector", 1, 1)], [portfolio]);
        Assert.Equal(500m, full.NextResources["planks"].Units);
        Assert.Equal(1, full.Production.Single(p => p.BuildingId == "sawmill").RunnableWorkers);
    }

    [Fact]
    public void InvalidOrOverlappingProjectionIsRejectedRatherThanDoubleCounting()
    {
        var catalog = new GameCatalog("trial", [new("logs", "Logs"), new("planks", "Planks"), new("stone", "Stone")], [Sawmill]);
        var tiered = TieredBuildingPortfolio.FromLegacy(Sawmill, 1, 1);
        Assert.Throws<ArgumentException>(() => TieredEconomyProjection.SimulateSecond(catalog,
            ResourceBundle.Empty, ResourceBundle.Empty, [new BuildingState("sawmill", 1, 1)], [tiered]));
        Assert.Throws<ArgumentException>(() => TieredEconomyProjection.SimulateSecond(catalog,
            ResourceBundle.Empty, ResourceBundle.Empty, [], []));
        Assert.Throws<ArgumentException>(() => TieredEconomyProjection.SimulateSecond(catalog,
            ResourceBundle.Create(("planks", 100m)), ResourceBundle.Empty, [], [tiered]));
    }

    [Fact]
    public void LegacyGameSessionIsUnaffectedByPureTieredProjections()
    {
        var scenario = TestScenarios.Economy();
        var game = GameSession.Create(scenario);
        var original = game.Snapshot();
        _ = TieredProductionCatalog.FromCatalog(scenario.Catalog, 2.6m, 6.5m);
        var tier = TieredBuildingPortfolio.FromLegacy(scenario.Catalog.BuildingsById["sawmill"], 1, 1);
        _ = tier.Upgrade(ProductionTier.Base, ProductionTier.Large);
        TestScenarios.EqualState(original, game.Snapshot());
        Assert.Equal("test-1", game.Scenario.Catalog.RulesVersion);
    }
}
