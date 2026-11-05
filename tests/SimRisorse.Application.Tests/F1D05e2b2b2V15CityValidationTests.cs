using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC66: a normal v15 roster is validated without constructing the
/// missing v14 food-store; all civic, technology, capacity and staff invariants
/// are still enforced by the native validator.</summary>
public sealed class F1D05e2b2b2V15CityValidationTests
{
    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);
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
        var old = V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), School, Tech);
        return V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(old, Policy), Policy);
    }

    private static void Check(V15FoodCampaignCheckpoint value) =>
        V15FoodCampaignCheckpoints.Validate(value, CivilGrowthScenario.Create(), School, Tech, Policy);

    [Fact]
    public void FifteenFamilyCityPassesNativeValidationButNotUnmodifiedV14Validator()
    {
        var candidate = Start();
        Assert.Equal(15, candidate.CityCampaign.ProductiveFamilies.Length);
        Assert.Throws<ArgumentException>(() => V14CampaignSnapshots.Validate(
            CivilGrowthScenario.Create(), candidate.CityCampaign.ToInherited(), School, Tech));
        Check(candidate);
        Assert.Equal(15, candidate.CityCampaign.ProductiveFamilies.Length);
    }

    [Fact]
    public void NativeValidatorRejectsMissingLiveProducerEvenWithValidOtherFields()
    {
        var original = Start();
        var city = original.CityCampaign;
        var malformed = original with { CityCampaign = city with
        {
            ProductiveFamilies = city.ProductiveFamilies.Skip(1).ToImmutableArray()
        }};
        Assert.Throws<ArgumentException>(() => Check(malformed));
        Check(original);
    }

    [Fact]
    public void NativeValidatorRejectsDuplicatedProducerInsteadOfPaddingOldSlot()
    {
        var original = Start();
        var city = original.CityCampaign;
        var malformed = original with { CityCampaign = city with
        {
            ProductiveFamilies = city.ProductiveFamilies.Add(city.ProductiveFamilies[0])
        }};
        Assert.Throws<ArgumentException>(() => Check(malformed));
    }

    [Fact]
    public void NativeValidatorRejectsRolledBackAcquisitionHistory()
    {
        var original = Start();
        var city = original.CityCampaign;
        var purchased = city.ProductiveFamilies.First(f => f.Base.Buildings > 0);
        var malformed = original with { CityCampaign = city with
        {
            PurchaseCounters = city.PurchaseCounters.Select(c => c.FamilyId == purchased.FamilyId
                ? c with { Acquired = 0 } : c).ToImmutableArray()
        }};
        Assert.Throws<ArgumentException>(() => Check(malformed));
    }

    [Fact]
    public void NativeValidatorRejectsPhysicalStockBeyondRealCityCapacity()
    {
        var original = Start();
        var city = original.CityCampaign;
        var malformed = original with { CityCampaign = city with
        {
            Resources = city.Resources + ResourceBundle.Create(("planks", 100000m))
        }};
        Assert.Throws<ArgumentException>(() => Check(malformed));
    }

    [Fact]
    public void LegacyImportMayBeValidatedBeforeExplicitRetirementButIsNeverSynthesized()
    {
        var original = Start();
        var old = V15LegacyProducerAdapter.ForV14Rules(original.CityCampaign);
        var imported = original with { CityCampaign = old with
        {
            ProductiveFamilies = old.ProductiveFamilies.Select(f => f.FamilyId == "food-store"
                ? f with { Base = new TierOccupancy(1, 1) } : f).ToImmutableArray(),
            PurchaseCounters = old.PurchaseCounters.Select(c => c.FamilyId == "food-store"
                ? c with { Acquired = 1 } : c).ToImmutableArray()
        }};
        Check(imported);
        var session = new V15FoodIndustryCampaignSession(imported,
            CivilGrowthScenario.Create(), School, Tech, Policy);
        Assert.True(session.RetireLegacyFoodStore().Succeeded);
        Check(session.Snapshot());
        Assert.DoesNotContain(session.Snapshot().CityCampaign.ProductiveFamilies,
            f => f.FamilyId == "food-store");
    }

    [Fact]
    public void NativeValidatorRejectsWorkforceBeyondPopulation()
    {
        var original = Start();
        var city = original.CityCampaign;
        var working = city.ProductiveFamilies.First(f => f.Base.Buildings > 0);
        var malformed = original with { CityCampaign = city with
        {
            ProductiveFamilies = city.ProductiveFamilies.Select(f => f.FamilyId == working.FamilyId
                ? f with { Base = new TierOccupancy(10, 10) } : f).ToImmutableArray(),
            PurchaseCounters = city.PurchaseCounters.Select(c => c.FamilyId == working.FamilyId
                ? c with { Acquired = 10 } : c).ToImmutableArray()
        }};
        Assert.Throws<ArgumentException>(() => Check(malformed));
    }
}
