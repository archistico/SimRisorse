using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC64: the v14-shaped projection is an ephemeral boundary object,
/// never a productive family in a new v15 campaign.</summary>
public sealed class F1D05e2b2V15NativeRosterTests
{
    private static ResourceAmount U(decimal value) => ResourceAmount.FromUnits(value);
    private static readonly SchoolResearchRules School = new("food", U(2m), 3, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Policy = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
        new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("bread", U(1m), U(1m), 4)
    ]);

    private static V15FoodCampaignCheckpoint NewGame()
    {
        var baseline = CivilGrowthScenario.Create();
        var city = V14CampaignSnapshots.MigrateFromV13(baseline,
            GameSession.Create(baseline).Snapshot(), School, Technologies);
        return V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, Policy), Policy);
    }

    [Fact]
    public void NewCampaignHasFifteenFamiliesAndNeverMutatesTheV14Source()
    {
        var baseline = CivilGrowthScenario.Create();
        var source = V14CampaignSnapshots.MigrateFromV13(baseline,
            GameSession.Create(baseline).Snapshot(), School, Technologies);
        var checkpoint = NewGame();
        Assert.Equal(16, source.ProductiveFamilies.Length);
        Assert.Equal(15, checkpoint.CityCampaign.ProductiveFamilies.Length);
        Assert.DoesNotContain(checkpoint.CityCampaign.ProductiveFamilies, f => f.FamilyId == "food-store");
        V15FoodCampaignCheckpoints.Validate(checkpoint, baseline, School, Technologies, Policy);
        Assert.Contains(source.ProductiveFamilies, f => f.FamilyId == "food-store");
    }

    [Fact]
    public void TemporaryLegacyProjectionIsNotStoredInTheCampaign()
    {
        var checkpoint = NewGame();
        var bridge = V15LegacyProducerAdapter.ForV14Rules(checkpoint.CityCampaign);
        Assert.Equal(16, bridge.ProductiveFamilies.Length);
        var retired = Assert.Single(bridge.ProductiveFamilies, f => f.FamilyId == "food-store");
        Assert.Equal(default, retired.Base);
        Assert.Equal(default, retired.Large);
        Assert.Equal(default, retired.Advanced);
        Assert.Equal(15, checkpoint.CityCampaign.ProductiveFamilies.Length);
        Assert.Same(checkpoint.CityCampaign, V15LegacyProducerAdapter.WithoutEmptySlot(checkpoint.CityCampaign));
    }

    [Fact]
    public void CityCommandsReturnToTheFifteenFamilyRosterAfterEachCommit()
    {
        var baseline = CivilGrowthScenario.Create();
        var starting = NewGame();
        var city = starting.CityCampaign;
        starting = starting with { CityCampaign = city with
        {
            Resources = city.Resources + ResourceBundle.Create(("planks", 500m), ("stone", 500m))
        }};
        var campaign = new V15FoodIndustryCampaignSession(starting, baseline, School, Technologies, Policy);
        Assert.True(campaign.BuildCityProducer("fisher", ProductionTier.Base, 1).Succeeded);
        Assert.True(campaign.BuildCityBuilding("warehouse", 1).Succeeded);
        Assert.Equal(15, campaign.Snapshot().CityCampaign.ProductiveFamilies.Length);
        Assert.DoesNotContain(campaign.Snapshot().CityCampaign.ProductiveFamilies,
            f => f.FamilyId == "food-store");
        Assert.Equal(1, campaign.Snapshot().CityCampaign.ProductiveFamilies
            .Single(f => f.FamilyId == "fisher").Base.Buildings);
        // The v15 capacity projector must not expect the retired family to
        // exist as a city building after the v14 command adapter is discarded.
        Assert.Contains(campaign.CityCatalog.Buildings, b => b.SourceId == "fisher");
        Assert.DoesNotContain(campaign.CityCatalog.Buildings, b => b.Id == "food-store");
        V15FoodCampaignCheckpoints.Validate(campaign.Snapshot(), baseline, School, Technologies, Policy);
    }

    [Fact]
    public void UnifiedTickDoesNotPutRetiredProducerBackIntoTheCheckpoint()
    {
        var baseline = CivilGrowthScenario.Create();
        var meat = new MeatChainPolicy("grain", "water", "cattle", "game", "meat",
            ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(1m), U(1m),
            [new("cattle", U(1m), U(2m), 0), new("game", U(1m), U(1m), 1)]);
        var crops = new HorticulturePolicy("water", "vegetables", "fruit",
            new("vegetables", U(2m), U(1m), 0), new("fruit", U(3m), U(1m), 1));
        var first = NewGame();
        var step = V15UnifiedCampaignClock.Plan(first, baseline, School, Technologies,
            FoodIndustryConstructionCatalog.Candidate, meat, crops, Policy,
            TimeSpan.FromSeconds(1));
        Assert.Equal(1, step.SecondsAdvanced);
        Assert.Equal(15, step.Next.CityCampaign.ProductiveFamilies.Length);
        Assert.DoesNotContain(step.Next.CityCampaign.ProductiveFamilies,
            f => f.FamilyId == "food-store");
        Assert.Equal(ResourceAmount.Zero, step.Next.CityCampaign.Resources["food"]);
    }

    [Fact]
    public void RetirementOnNewCampaignIsIdempotentAndDoesNotCreateLegacySlot()
    {
        var campaign = new V15FoodIndustryCampaignSession(NewGame(), CivilGrowthScenario.Create(),
            School, Technologies, Policy);
        var before = campaign.Snapshot();
        Assert.True(campaign.RetireLegacyFoodStore().Succeeded);
        Assert.Same(before, campaign.Snapshot());
        Assert.True(campaign.RetireLegacyFoodStore().Succeeded);
        Assert.Equal(15, campaign.Snapshot().CityCampaign.ProductiveFamilies.Length);
    }
}
