using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class TechnologyPurchasesTests
{
    private static readonly TechnologyCatalog Catalog = TechnologyCatalog.ProposedTierTree;
    private static readonly HashSet<string> NoBuildings = new(StringComparer.Ordinal);

    [Fact]
    public void UnspentResearchAccumulatesWithNoProjectAndNeverUnlocksAutomatically()
    {
        var grown = ResearchLedger.Empty.Accrue(2, 500, 100);
        Assert.Equal(100m, grown.Balance.Units);
        Assert.Empty(grown.UnlockedTechnologies);
        Assert.Equal(ResearchPoints.Zero, ResearchLedger.Empty.Balance);
        var purchase = TechnologyPurchases.TryUnlock(Catalog, grown, "lavorazioni-1", NoBuildings);
        Assert.True(purchase.Succeeded);
        Assert.Equal(0m, purchase.Ledger.Balance.Units);
        Assert.Contains("lavorazioni-1", purchase.Ledger.UnlockedTechnologies);
        Assert.Empty(grown.UnlockedTechnologies);
    }

    [Fact]
    public void PurchasedTechnologyLeavesRemainingPointsAndCanBeChosenInAnyOrder()
    {
        var owned = ResearchLedger.Empty.Accrue(3, 1000, 100); // 300 points
        var food = TechnologyPurchases.TryUnlock(Catalog, owned, "alimentazione-1", NoBuildings);
        Assert.True(food.Succeeded);
        var wood = TechnologyPurchases.TryUnlock(Catalog, food.Ledger, "lavorazioni-1", NoBuildings);
        Assert.True(wood.Succeeded);
        Assert.Equal(100m, wood.Ledger.Balance.Units);
        Assert.Equal(2, wood.Ledger.UnlockedTechnologies.Count);
        Assert.Equal(300m, owned.Balance.Units);
    }

    [Fact]
    public void MissingPrerequisiteOrBalanceNeverConsumesResearch()
    {
        var ledger = ResearchLedger.Empty.Accrue(1, 3000, 100);
        var blocked = TechnologyPurchases.TryUnlock(Catalog, ledger, "industria-1", NoBuildings);
        Assert.Equal(TechnologyPurchaseFailure.MissingPrerequisite, blocked.Failure);
        Assert.Same(ledger, blocked.Ledger);
        var spent = TechnologyPurchases.TryUnlock(Catalog, ledger, "estrazione-1", NoBuildings);
        Assert.True(spent.Succeeded);
        var stillInsufficient = TechnologyPurchases.TryUnlock(Catalog, spent.Ledger, "industria-1", NoBuildings);
        Assert.Equal(TechnologyPurchaseFailure.InsufficientResearchPoints, stillInsufficient.Failure);
        Assert.Same(spent.Ledger, stillInsufficient.Ledger);
    }

    [Fact]
    public void UnknownLockedAndAlreadyPurchasedTechnologiesAreNotCharged()
    {
        var ledger = ResearchLedger.Empty.Accrue(1, 3000, 100);
        Assert.Equal(TechnologyPurchaseFailure.UnknownTechnology,
            TechnologyPurchases.TryUnlock(Catalog, ledger, "unknown-1", NoBuildings).Failure);
        Assert.Equal(TechnologyPurchaseFailure.NotImplemented,
            TechnologyPurchases.TryUnlock(Catalog, ledger, "navigazione-1", NoBuildings).Failure);
        Assert.Equal(TechnologyPurchaseFailure.NotImplemented,
            TechnologyPurchases.TryUnlock(Catalog, ledger, "logistica-1", NoBuildings).Failure);
        var bought = TechnologyPurchases.TryUnlock(Catalog, ledger, "alimentazione-1", NoBuildings);
        var duplicate = TechnologyPurchases.TryUnlock(Catalog, bought.Ledger, "alimentazione-1", NoBuildings);
        Assert.Equal(TechnologyPurchaseFailure.AlreadyUnlocked, duplicate.Failure);
        Assert.Same(bought.Ledger, duplicate.Ledger);
    }

    [Fact]
    public void CompletedTechnologiesUnlockTiersInExistingE2Model()
    {
        var source = new BuildingDefinition("sawmill", "Segheria", ResourceBundle.Create(("planks", 40m)), 1,
            new(ResourceBundle.Create(("logs", 2m)), ResourceBundle.Create(("planks", 1m))), 10, []);
        var family = new TieredProductionFamily(source, "lavorazioni-1", "lavorazioni-2", 2.6m, 6.5m);
        var first = ResearchLedger.Empty.Accrue(1, 1000, 100);
        var unlocked = TechnologyPurchases.TryUnlock(Catalog, first, "lavorazioni-1", NoBuildings).Ledger;
        Assert.True(family.IsUnlocked(ProductionTier.Large, unlocked.UnlockedTechnologies));
        Assert.False(family.IsUnlocked(ProductionTier.Advanced, unlocked.UnlockedTechnologies));
        var full = TechnologyPurchases.TryUnlock(Catalog, unlocked.Accrue(1, 3500, 100), "lavorazioni-2", NoBuildings);
        Assert.True(full.Succeeded);
        Assert.True(family.IsUnlocked(ProductionTier.Advanced, full.Ledger.UnlockedTechnologies));
    }

    [Fact]
    public void RequiredBuildingIsCheckedForCandidateThatDefinesOne()
    {
        var tree = new TechnologyCatalog([new TechnologyDefinition("test-tech", "Test", ResearchPoints.FromUnits(10m),
            true, [], ["mint"], [])]);
        var ledger = ResearchLedger.Empty.Accrue(1, 100, 100);
        var missing = TechnologyPurchases.TryUnlock(tree, ledger, "test-tech", NoBuildings);
        Assert.Equal(TechnologyPurchaseFailure.MissingBuilding, missing.Failure);
        Assert.Same(ledger, missing.Ledger);
        Assert.True(TechnologyPurchases.TryUnlock(tree, ledger, "test-tech",
            new HashSet<string>(StringComparer.Ordinal) { "mint" }).Succeeded);
    }

    [Fact]
    public void FromSchoolToVoluntaryResearchPurchaseToLargeBuildingIsACompleteIsolatedPath()
    {
        var rules = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
        var enrollment = SchoolResearch.Enroll(rules, SchoolResearchState.Empty,
            ResourceBundle.Create(("food", 30m)), 8, 2, 1, 1, 2);
        Assert.True(enrollment.Succeeded);
        var graduates = SchoolResearch.Advance(rules, enrollment.State, ResearchLedger.Empty, 1, 240);
        var staffing = SchoolResearch.Assign(rules, graduates.State, 1, 2);
        Assert.True(staffing.Succeeded);
        var research = SchoolResearch.Advance(rules, staffing.State, graduates.Ledger, 1, 500).Ledger;
        Assert.Equal(100m, research.Balance.Units);
        Assert.Empty(research.UnlockedTechnologies); // no automatic spending

        var source = new BuildingDefinition("sawmill", "Segheria", ResourceBundle.Create(("planks", 40m), ("stone", 20m)), 1,
            new(ResourceBundle.Create(("logs", 2m)), ResourceBundle.Create(("planks", 1m))), 10, []);
        var family = new TieredProductionFamily(source, "lavorazioni-1", "lavorazioni-2", 2.6m, 6.5m);
        var before = TieredBuildingPortfolio.FromLegacy(source, 1, 1);
        var owned = ConstructionAcquisitionHistory.FromOwned([("sawmill", 1)]);
        var materials = ResourceBundle.Create(("planks", 400m), ("stone", 400m));
        Assert.Equal(TieredPurchaseFailure.MissingResearch, TieredConstruction.TryBuild(
            family, before, ProductionTier.Large, 1, 1.15m, research.UnlockedTechnologies, materials, owned).Failure);
        var unlocked = TechnologyPurchases.TryUnlock(Catalog, research, "lavorazioni-1", NoBuildings);
        Assert.True(unlocked.Succeeded);
        var built = TieredConstruction.TryBuild(family, before, ProductionTier.Large, 1,
            1.15m, unlocked.Ledger.UnlockedTechnologies, materials, owned);
        Assert.True(built.Succeeded);
        Assert.Equal(2, built.History.Acquired("sawmill"));
        Assert.Equal(1, built.Portfolio.Large.Buildings);
        Assert.Equal(1, built.Portfolio.AssignedWorkers);
    }

    [Fact]
    public void InvalidGraphAndRestoreWithUnmetPrerequisitesAreRejected()
    {
        static TechnologyDefinition Definition(string id, params string[] needs) =>
            new(id, id, ResearchPoints.FromUnits(1m), true, needs, [], []);
        Assert.Throws<ArgumentException>(() => new TechnologyCatalog([Definition("a", "b")]));
        Assert.Throws<ArgumentException>(() => new TechnologyCatalog([Definition("a", "b"), Definition("b", "a")]));
        Assert.Throws<ArgumentException>(() => new TechnologyCatalog([Definition("a"), Definition("a")]));
        Assert.Throws<ArgumentException>(() => new TechnologyCatalog([
            new TechnologyDefinition("blocked", "Blocked", ResearchPoints.FromUnits(1m), false, [], [], []),
            Definition("a", "blocked")
        ]));
        Assert.Throws<ArgumentException>(() => ResearchLedger.Restore(ResearchPoints.FromUnits(4m), ["industria-1"], Catalog));
        Assert.Throws<ArgumentException>(() => ResearchLedger.Restore(ResearchPoints.Zero, ["lavorazioni-1", "lavorazioni-1"], Catalog));
        Assert.Throws<ArgumentException>(() => ResearchLedger.Restore(ResearchPoints.Zero, ["navigazione-1"], Catalog));
    }

    [Fact]
    public void CandidateTechnologiesMatchTierPathsAndHaveNoDuplicateFamilyCoverage()
    {
        var all = Catalog.Technologies;
        Assert.Equal(11, all.Length);
        Assert.Equal(8, all.Count(t => t.Available));
        Assert.Equal(3, all.Count(t => !t.Available));
        var expected = TieredProductionCatalog.KnownFamilyIds.ToHashSet(StringComparer.Ordinal);
        foreach (var suffix in new[] { "-1", "-2" })
        {
            var actual = all.Where(t => t.Available && t.Id.EndsWith(suffix, StringComparison.Ordinal))
                .SelectMany(t => t.UnlockedFamilies).ToArray();
            Assert.Equal(16, actual.Length);
            Assert.True(expected.SetEquals(actual));
        }
    }
}
