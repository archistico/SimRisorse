using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Research;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>P1-D48: a purchased technology must unlock an actual implemented tier.</summary>
public sealed class TechnologyTierEffectTests
{
    private static readonly TechnologyCatalog Catalog = TechnologyCatalog.ProposedTierTree;

    private static TieredProductionFamily[] Families() =>
        TieredProductionCatalog.FromCatalog(CivilGrowthScenario.Create().Catalog, 2.6m, 6.5m).ToArray();

    private static TechnologyCatalog Change(string id, Func<TechnologyDefinition, TechnologyDefinition> change) =>
        new(Catalog.Technologies.Select(item => item.Id == id ? change(item) : item));

    private static TechnologyDefinition Copy(TechnologyDefinition source, bool? available = null,
        IEnumerable<string>? prerequisites = null, IEnumerable<string>? families = null) =>
        new(source.Id, source.Name, source.Cost, available ?? source.Available,
            prerequisites ?? source.Prerequisites, source.RequiredBuildings,
            families ?? source.UnlockedFamilies);

    [Fact]
    public void AllPurchasableTechnologiesHaveExactlyThePromisedImplementedBenefits()
    {
        var families = Families();
        Assert.Equal(16, families.Length);
        TechnologyTierEffects.ValidateCatalog(Catalog, families);
        foreach (var technology in Catalog.Technologies.Where(item => item.Available))
        {
            var effect = TechnologyTierEffects.ForTechnology(technology.Id, families);
            Assert.True(effect.HasEffect, technology.Id);
            Assert.True(effect.FamilyIds.SetEquals(technology.UnlockedFamilies), technology.Id);
            if (technology.Id.EndsWith("-1", StringComparison.Ordinal))
                Assert.Empty(effect.AdvancedFamilies);
            else
                Assert.Empty(effect.LargeFamilies);
        }
        foreach (var technology in Catalog.Technologies.Where(item => !item.Available))
            Assert.False(TechnologyTierEffects.ForTechnology(technology.Id, families).HasEffect);
    }

    [Fact]
    public void PurchasingAResearchWithoutActualCoverageIsRejectedAtCatalogGate()
    {
        var mismatched = Change("lavorazioni-1", item => Copy(item, families: ["sawmill"]));
        Assert.Throws<ArgumentException>(() => TechnologyTierEffects.ValidateCatalog(mismatched, Families()));
    }

    [Fact]
    public void UnimplementedTechnologyCannotBeMadePurchasableWithNoBenefit()
    {
        var mismatched = Change("logistica-1", item => Copy(item, available: true));
        Assert.Throws<ArgumentException>(() => TechnologyTierEffects.ValidateCatalog(mismatched, Families()));
    }

    [Fact]
    public void AdvancedResearchMustDependOnTheCorrespondingLargeTierResearch()
    {
        var mismatched = Change("lavorazioni-2", item => Copy(item, prerequisites: []));
        Assert.Throws<ArgumentException>(() => TechnologyTierEffects.ValidateCatalog(mismatched, Families()));
    }
}
