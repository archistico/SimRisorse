using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class TierCoverageTests
{
    [Fact]
    public void ActiveCatalogHasExactlySixteenProductiveTierFamiliesAndNoCivilOrMilitaryStructures()
    {
        var catalog = CivilGrowthScenario.Create().Catalog;
        var families = TieredProductionCatalog.FromCatalog(catalog, 2.6m, 6.5m);
        Assert.Equal(16, families.Length);
        Assert.Equal(16, TieredProductionCatalog.KnownFamilyIds.Length);
        Assert.Equal(16, families.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count());
        Assert.DoesNotContain(families, item => item.FamilyId is "warehouse" or "small-house" or "large-house" or "barracks" or "town-hall");
        foreach (var family in families)
        {
            Assert.NotNull(family.Source.Recipe);
            Assert.Equal(1, family.Source.WorkplacesPerBuilding);
            Assert.Equal(1, family.Get(ProductionTier.Base).Workplaces);
            Assert.Equal(3, family.Get(ProductionTier.Large).Workplaces);
            Assert.Equal(10, family.Get(ProductionTier.Advanced).Workplaces);
        }
    }

    [Fact]
    public void HistoricalCatalogOnlyProvidesExistingProductiveFamilies()
    {
        var catalog = EconomyFoundationScenario.Create().Catalog;
        var families = TieredProductionCatalog.FromCatalog(catalog, 2.6m, 6.5m);
        Assert.Equal(3, families.Length);
        Assert.Contains(families, item => item.FamilyId == "sawmill");
    }
}
