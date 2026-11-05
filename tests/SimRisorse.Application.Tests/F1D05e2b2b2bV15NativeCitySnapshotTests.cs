using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC67: native civil DTO is a lossless strict boundary, while the
/// actual v15 validator never recreates or validates a v14 food-store slot.</summary>
public sealed class F1D05e2b2b2bV15NativeCitySnapshotTests
{
    private static ResourceAmount U(decimal amount) => ResourceAmount.FromUnits(amount);
    private static readonly SchoolResearchRules School = new("food", U(2m), 3, 1, 2, 100);
    private static readonly TechnologyCatalog Tech = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Policy = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
        new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("bread", U(1m), U(1m), 4)
    ]);

    private static V15FoodCampaignCheckpoint Start()
    {
        var scenario = CivilGrowthScenario.Create();
        var legacy = V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), School, Tech);
        return V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(legacy, Policy), Policy);
    }

    private static void AssertCityPayloadEqual(V14CampaignSnapshot expected, V14CampaignSnapshot actual)
    {
        // ImmutableArray equality on a record is backing-array identity, not
        // sequence equivalence. Native v15 owns new DTO collections: compare
        // every immutable item, then every other checkpoint field via records.
        Assert.Equal(expected.ProductiveFamilies.ToArray(), actual.ProductiveFamilies.ToArray());
        Assert.Equal(expected.PurchaseCounters.ToArray(), actual.PurchaseCounters.ToArray());
        Assert.Equal(expected with
        {
            ProductiveFamilies = actual.ProductiveFamilies,
            PurchaseCounters = actual.PurchaseCounters
        }, actual);
    }

    [Fact]
    public void NativeViewDoesNotAddRetiredProducerOrChangeCity()
    {
        var checkpoint = Start();
        var native = checkpoint.NativeCityCampaign;
        Assert.Equal(15, native.ProductiveFamilies.Length);
        Assert.DoesNotContain(native.ProductiveFamilies, f => f.FamilyId == "food-store");
        AssertCityPayloadEqual(checkpoint.CityCampaign.ToInherited(), native.ToInherited());
        AssertCityPayloadEqual(checkpoint.CityCampaign.ToInherited(), checkpoint.NativeCityCampaign.ToInherited());
    }

    [Fact]
    public void NativeConversionRetainsEntireCityPayloadIncludingGrowthAndResearch()
    {
        var city = Start().CityCampaign;
        var edited = city with
        {
            ElapsedTicks = 1234567,
            GrowthProgressSeconds = 3,
            GrowthRemainderTwentieths = 7,
            ResearchBalanceMilliPoints = 1234,
            Resources = city.Resources + ResourceBundle.Create(("wood", 1m))
        };
        var native = V15CityCampaignSnapshot.FromInherited(edited.ToInherited());
        AssertCityPayloadEqual(edited.ToInherited(), native.ToInherited());
        Assert.Equal(1234567, native.ElapsedTicks);
        Assert.Equal(1234, native.ResearchBalanceMilliPoints);
        Assert.Equal(7, native.GrowthRemainderTwentieths);
    }

    [Fact]
    public void NativeBoundaryRejectsForeignInheritedFormatAndRules()
    {
        var inherited = Start().CityCampaign.ToInherited();
        Assert.Throws<ArgumentException>(() => V15CityCampaignSnapshot.FromInherited(
            inherited with { FormatVersion = inherited.FormatVersion + 1 }));
        Assert.Throws<ArgumentException>(() => V15CityCampaignSnapshot.FromInherited(
            inherited with { RulesVersion = "unknown-rules" }));
        Assert.Throws<ArgumentNullException>(() => V15CityCampaignSnapshot.FromInherited(null!));
    }

    [Fact]
    public void NativeValidationAndOldBoundaryAgreeForValidFifteenFamilyCity()
    {
        var city = Start().CityCampaign;
        var scenario = CivilGrowthScenario.Create();
        V15CityCampaignValidator.Validate(scenario, city.ToInherited(), School, Tech);
        V15CityCampaignValidator.ValidateNative(scenario,
            V15CityCampaignSnapshot.FromInherited(city.ToInherited()), School, Tech);
        Assert.Equal(15, city.ProductiveFamilies.Length);
    }

    [Fact]
    public void NativeValidationRejectsMissingFamilyWithoutMutatingInput()
    {
        var city = Start().NativeCityCampaign;
        var malformed = city with { ProductiveFamilies = city.ProductiveFamilies.Skip(1).ToImmutableArray() };
        Assert.Throws<ArgumentException>(() => V15CityCampaignValidator.ValidateNative(
            CivilGrowthScenario.Create(), malformed, School, Tech));
        Assert.Equal(15, city.ProductiveFamilies.Length);
    }

    [Fact]
    public void NativeStorageProjectionEqualsInheritedProjection()
    {
        var city = Start().CityCampaign;
        var scenario = CivilGrowthScenario.Create();
        var native = V15CityCampaignSnapshot.FromInherited(city.ToInherited());
        Assert.Equal(V15CityStorage.Calculate(scenario, city.ToInherited()),
            V15CityStorage.Calculate(scenario, native));
    }
}
