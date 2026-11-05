using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC70: capacity for pencils is calculated from the native v15 city,
/// not by building a disposable V14CampaignSnapshot.</summary>
public sealed class F1D05e2b2eNativePencilCapacityTests
{
    private static V15CityCampaignSnapshot Seed()
    {
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
        var v14 = V14CampaignSnapshots.MigrateFromV13(baseline,
            GameSession.Create(baseline).Snapshot(), school, TechnologyCatalog.ProposedTierTree);
        return V15CityCampaignSnapshot.FromInherited(v14);
    }

    [Fact]
    public void NativePencilCapacityMatchesEstablishedFormulaAcrossWarehousesAndSchools()
    {
        var baseline = CivilGrowthScenario.Create();
        var seed = Seed();
        foreach (var warehouses in new[] { 0, 1, 7 })
        foreach (var schools in new[] { 0, 2, 8 })
        {
            var city = seed with
            {
                OtherBuildings = [.. seed.OtherBuildings.Select(b => b.BuildingId == "warehouse"
                    ? b with { Count = warehouses } : b)],
                SchoolCount = schools
            };
            Assert.Equal(V14SchoolSupplies.Capacity(baseline, city.ToInherited()),
                V15CityStorage.Calculate(baseline, city)[V14SchoolSupplies.ResourceId]);
            Assert.Equal(V15CityStorage.Calculate(baseline, city.ToInherited()),
                V15CityStorage.Calculate(baseline, city));
        }
    }

    [Fact]
    public void NativeCapacityExcludesRetiredFoodStoreDespiteLegacyProjection()
    {
        var baseline = CivilGrowthScenario.Create();
        var native = V15LegacyProducerAdapter.WithoutEmptySlot(Seed());
        Assert.DoesNotContain(native.ProductiveFamilies, f => f.FamilyId == "food-store");
        var legacyProjection = V15LegacyProducerAdapter.ForV14Rules(native);
        Assert.Equal(V15CityStorage.Calculate(baseline, native),
            V15CityStorage.Calculate(baseline, legacyProjection));
    }
}
