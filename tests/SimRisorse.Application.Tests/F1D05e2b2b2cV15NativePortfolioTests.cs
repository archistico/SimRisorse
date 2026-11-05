using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC68: the producer and purchase-ledger types belong to v15;
/// converting at the temporary SQLite/v14 boundary must not lose content.</summary>
public sealed class F1D05e2b2b2cV15NativePortfolioTests
{
    private static readonly SchoolResearchRules School = new(
        "food", ResourceAmount.FromUnits(2m), 3, 1, 2, 100);
    private static readonly TechnologyCatalog Tech = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Policy = UnifiedFoodInputs.CreatePolicy([
        new("fish", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 0),
        new("meat", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 1),
        new("vegetables", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 2),
        new("fruit", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 3),
        new("bread", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 4)
    ]);

    private static V14CampaignSnapshot StartingCity()
    {
        var baseline = CivilGrowthScenario.Create();
        var v14 = V14CampaignSnapshots.MigrateFromV13(baseline,
            GameSession.Create(baseline).Snapshot(), School, Tech);
        return V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(v14, Policy), Policy).CityCampaign.ToInherited();
    }

    private static void AssertFamiliesMatch(V14CampaignSnapshot expected, V14CampaignSnapshot actual)
    {
        Assert.Equal(expected.ProductiveFamilies.ToArray(), actual.ProductiveFamilies.ToArray());
        Assert.Equal(expected.PurchaseCounters.ToArray(), actual.PurchaseCounters.ToArray());
    }

    [Fact]
    public void NativeCityUsesV15OwnedTypesNotV14FamilyOrCounter()
    {
        var native = V15CityCampaignSnapshot.FromInherited(StartingCity());
        Assert.IsType<V15ProductiveFamilySnapshot>(native.ProductiveFamilies[0]);
        Assert.IsType<V15PurchaseCounter>(native.PurchaseCounters[0]);
        Assert.Equal(15, native.ProductiveFamilies.Length);
    }

    [Fact]
    public void ConversionPreservesEveryTierOccupancyIncludingWorkerAssignments()
    {
        var city = StartingCity();
        var current = city.ProductiveFamilies[0];
        var modified = city with { ProductiveFamilies = city.ProductiveFamilies.SetItem(0,
            current with { Base = new TierOccupancy(17, 11),
                Large = new TierOccupancy(5, 13), Advanced = new TierOccupancy(2, 16) }) };
        var native = V15CityCampaignSnapshot.FromInherited(modified);
        AssertFamiliesMatch(modified, native.ToInherited());
        Assert.Equal(16, native.ProductiveFamilies[0].Advanced.AssignedWorkers);
    }

    [Fact]
    public void PurchaseHistoryDoesNotResetWhenOwnedBuildingsDiffer()
    {
        var city = StartingCity();
        var counter = city.PurchaseCounters[0];
        var edited = city with { PurchaseCounters = city.PurchaseCounters.SetItem(0,
            counter with { Acquired = 12345 }) };
        var native = V15CityCampaignSnapshot.FromInherited(edited);
        Assert.Equal(12345, native.PurchaseCounters[0].Acquired);
        AssertFamiliesMatch(edited, native.ToInherited());
    }

    [Fact]
    public void ConvertedArraysRetainOriginalOrderAndIdentifiers()
    {
        var city = StartingCity();
        var altered = city with
        {
            ProductiveFamilies = city.ProductiveFamilies.Reverse().ToImmutableArray(),
            PurchaseCounters = city.PurchaseCounters.Reverse().ToImmutableArray()
        };
        var converted = V15CityCampaignSnapshot.FromInherited(altered);
        Assert.Equal(altered.ProductiveFamilies.Select(f => f.FamilyId).ToArray(),
            converted.ProductiveFamilies.Select(f => f.FamilyId).ToArray());
        Assert.Equal(altered.PurchaseCounters.Select(c => c.FamilyId).ToArray(),
            converted.PurchaseCounters.Select(c => c.FamilyId).ToArray());
        AssertFamiliesMatch(altered, converted.ToInherited());
    }

    [Fact]
    public void RetiredHistoricalCounterIsPreservedWithoutRecreatingRetiredProducer()
    {
        var city = StartingCity();
        var index = Array.FindIndex(city.PurchaseCounters.ToArray(), c => c.FamilyId == "food-store");
        // The original acquisition ledger may retain a retired purchase count.
        Assert.True(index >= 0);
        var edited = city with { PurchaseCounters = city.PurchaseCounters.SetItem(index,
            city.PurchaseCounters[index] with { Acquired = 19 }) };
        var native = V15CityCampaignSnapshot.FromInherited(edited);
        Assert.DoesNotContain(native.ProductiveFamilies, f => f.FamilyId == "food-store");
        Assert.Equal(19, native.PurchaseCounters[index].Acquired);
        AssertFamiliesMatch(edited, native.ToInherited());
    }

    [Fact]
    public void BoundaryRejectsUninitializedLegacyCollections()
    {
        var city = StartingCity();
        Assert.Throws<ArgumentException>(() => V15CityCampaignSnapshot.FromInherited(
            city with { ProductiveFamilies = default }));
        Assert.Throws<ArgumentException>(() => V15CityCampaignSnapshot.FromInherited(
            city with { PurchaseCounters = default }));
    }

    [Fact]
    public void BoundaryRejectsUninitializedNativeCollections()
    {
        var native = V15CityCampaignSnapshot.FromInherited(StartingCity());
        Assert.Throws<ArgumentException>(() => (native with { ProductiveFamilies = default }).ToInherited());
        Assert.Throws<ArgumentException>(() => (native with { PurchaseCounters = default }).ToInherited());
    }

    [Fact]
    public void NativeValidatorRejectsDuplicateProducerIdWithoutModifyingSource()
    {
        var baseline = CivilGrowthScenario.Create();
        var native = V15CityCampaignSnapshot.FromInherited(StartingCity());
        var duplicate = native with { ProductiveFamilies = native.ProductiveFamilies.Add(native.ProductiveFamilies[0]) };
        Assert.Throws<ArgumentException>(() => V15CityCampaignValidator.ValidateNative(
            baseline, duplicate, School, Tech));
        Assert.Equal(15, native.ProductiveFamilies.Length);
    }

    [Fact]
    public void NativeValidatorRejectsRolledBackAcquisitionHistory()
    {
        var baseline = CivilGrowthScenario.Create();
        var native = V15CityCampaignSnapshot.FromInherited(StartingCity());
        var altered = native with { PurchaseCounters = native.PurchaseCounters.SetItem(0,
            native.PurchaseCounters[0] with { Acquired = -1 }) };
        Assert.Throws<ArgumentException>(() => V15CityCampaignValidator.ValidateNative(
            baseline, altered, School, Tech));
        Assert.Equal(native.PurchaseCounters[0].Acquired,
            V15CityCampaignSnapshot.FromInherited(StartingCity()).PurchaseCounters[0].Acquired);
    }
}
