using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC69: a v15 checkpoint owns exactly one native civic root.
/// No command may replace it with a mutable or cached v14 snapshot.</summary>
public sealed class F1D05e2b2b2dNativeCheckpointRootTests
{
    private static readonly SchoolResearchRules School = new(
        "food", ResourceAmount.FromUnits(2m), 3, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Policy = UnifiedFoodInputs.CreatePolicy([
        new("fish", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 0),
        new("meat", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 1),
        new("vegetables", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 2),
        new("fruit", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 3),
        new("bread", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 4)
    ]);

    private static V15FoodCampaignCheckpoint Seed()
    {
        var baseline = CivilGrowthScenario.Create();
        var inherited = V14CampaignSnapshots.MigrateFromV13(baseline,
            GameSession.Create(baseline).Snapshot(), School, Technologies);
        return V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(inherited, Policy), Policy);
    }

    private static void Check(V15FoodCampaignCheckpoint value) =>
        V15FoodCampaignCheckpoints.Validate(value,
            CivilGrowthScenario.Create(), School, Technologies, Policy);

    [Fact]
    public void CityCampaignIsNativeAndAliasIsExactlyTheSameInstance()
    {
        var checkpoint = Seed();
        Assert.IsType<V15CityCampaignSnapshot>(checkpoint.CityCampaign);
        Assert.Same(checkpoint.CityCampaign, checkpoint.NativeCityCampaign);
        Assert.Equal(15, checkpoint.CityCampaign.ProductiveFamilies.Length);
        Assert.DoesNotContain(checkpoint.CityCampaign.ProductiveFamilies,
            f => f.FamilyId == "food-store");
        Check(checkpoint);
    }

    [Fact]
    public void NativeFamilyAndPurchaseTypesAreDirectlyOwnedByRoot()
    {
        var city = Seed().CityCampaign;
        Assert.IsType<V15ProductiveFamilySnapshot>(city.ProductiveFamilies[0]);
        Assert.IsType<V15PurchaseCounter>(city.PurchaseCounters[0]);
        Assert.True(V15LegacyProducerAdapter.CanOmit(city));
        Assert.Same(city, V15LegacyProducerAdapter.WithoutEmptySlot(city));
    }

    [Fact]
    public void InvalidPurchaseHistoryIsRejectedWithoutChangingOriginalCheckpoint()
    {
        var checkpoint = Seed();
        var city = checkpoint.CityCampaign;
        var invalid = checkpoint with { CityCampaign = city with
        {
            PurchaseCounters = city.PurchaseCounters.SetItem(0,
                city.PurchaseCounters[0] with { Acquired = -1 })
        }};
        Assert.Throws<ArgumentException>(() => Check(invalid));
        Assert.Same(city, checkpoint.CityCampaign);
        Check(checkpoint);
    }

    [Fact]
    public void FailedConstructionKeepsSameNativeCheckpointReference()
    {
        var checkpoint = Seed();
        var session = new V15FoodIndustryCampaignSession(checkpoint,
            CivilGrowthScenario.Create(), School, Technologies, Policy);
        var before = session.Snapshot();
        Assert.False(session.Build("no-such-site", 1).Succeeded);
        Assert.Same(before, session.Snapshot());
        Assert.Same(before.CityCampaign, session.Snapshot().CityCampaign);
    }

    [Fact]
    public void ExplicitHistoricalImportRoundTripRetainsFieldsAndPurchaseOrder()
    {
        var city = Seed().CityCampaign;
        var altered = city with
        {
            ElapsedTicks = 12345,
            ResearchBalanceMilliPoints = 756,
            ProductiveFamilies = city.ProductiveFamilies.Reverse().ToImmutableArray(),
            PurchaseCounters = city.PurchaseCounters.Reverse().ToImmutableArray()
        };
        var imported = V15CityCampaignSnapshot.FromInherited(altered.ToInherited());
        Assert.Equal(altered.ElapsedTicks, imported.ElapsedTicks);
        Assert.Equal(altered.ResearchBalanceMilliPoints, imported.ResearchBalanceMilliPoints);
        Assert.Equal(altered.ProductiveFamilies.ToArray(), imported.ProductiveFamilies.ToArray());
        Assert.Equal(altered.PurchaseCounters.ToArray(), imported.PurchaseCounters.ToArray());
        Assert.Equal(altered.Military, imported.Military);
        Assert.Equal(altered.SchoolResearch, imported.SchoolResearch);
    }

    [Fact]
    public void OccupiedRetiredProducerIsNeverSilentlyDroppedByNativeCleanup()
    {
        var city = V15LegacyProducerAdapter.ForV14Rules(Seed().CityCampaign);
        var occupied = city with { ProductiveFamilies = city.ProductiveFamilies.Select(f =>
            f.FamilyId == "food-store"
                ? f with { Base = f.Base with { Buildings = 1, AssignedWorkers = 1 } }
                : f).ToImmutableArray() };
        Assert.False(V15LegacyProducerAdapter.CanOmit(occupied));
        Assert.Same(occupied, V15LegacyProducerAdapter.WithoutEmptySlot(occupied));
    }
}
