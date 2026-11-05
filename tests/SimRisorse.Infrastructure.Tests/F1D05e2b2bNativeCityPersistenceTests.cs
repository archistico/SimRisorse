using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>RC65: the native commands and SQLite generation 6 share one
/// fifteen-family roster without invoking a V14CampaignSession.</summary>
public sealed class F1D05e2b2bNativeCityPersistenceTests
{
    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);
    private static readonly SchoolResearchRules School = new("food", U(2m), 3, 1, 2, 100);
    private static readonly TechnologyCatalog Tech = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Policy = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
        new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("bread", U(1m), U(1m), 4)
    ]);

    [Fact]
    public void NativePurchasesAndWorkerAssignmentsRoundTripWithFullHistory()
    {
        var scenario = CivilGrowthScenario.Create();
        var city = V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), School, Tech);
        city = city with { Resources = city.Resources + ResourceBundle.Create(
            ("planks", 500m), ("stone", 500m)) };
        var initial = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, Policy), Policy);
        var game = new V15FoodIndustryCampaignSession(initial, scenario, School, Tech, Policy);
        Assert.True(game.BuildCityProducer("fisher", ProductionTier.Base, 1).Succeeded);
        Assert.True(game.BuildCityBuilding("warehouse", 1).Succeeded);
        Assert.Equal(V14TierActionFailure.None,
            game.AssignCityProducerWorkers("fisher", ProductionTier.Base, 1));
        var before = game.Snapshot();
        var dir = Path.Combine(Path.GetTempPath(), "simrisorse-rc65-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var store = new V15SqliteFoodSaveStore(Path.Combine(dir, "native-v15.sqlite3"),
                scenario, School, Tech, Policy);
            store.Save(before);
            var loaded = new V15FoodIndustryCampaignSession(store.Load(), scenario, School, Tech, Policy)
                .Snapshot();
            Assert.Equal(before.CityCampaign.Resources, loaded.CityCampaign.Resources);
            Assert.Equal(before.CityCampaign.OtherBuildings.OrderBy(x => x.BuildingId).ToArray(),
                loaded.CityCampaign.OtherBuildings.OrderBy(x => x.BuildingId).ToArray());
            Assert.Equal(before.CityCampaign.ProductiveFamilies.OrderBy(x => x.FamilyId).ToArray(),
                loaded.CityCampaign.ProductiveFamilies.OrderBy(x => x.FamilyId).ToArray());
            Assert.Equal(before.CityCampaign.PurchaseCounters.OrderBy(x => x.FamilyId).ToArray(),
                loaded.CityCampaign.PurchaseCounters.OrderBy(x => x.FamilyId).ToArray());
            Assert.Equal(before.Depot, loaded.Depot);
            Assert.Equal(before.Industry, loaded.Industry);
            Assert.Equal(15, loaded.CityCampaign.ProductiveFamilies.Length);
            Assert.DoesNotContain(loaded.CityCampaign.ProductiveFamilies,
                f => f.FamilyId == "food-store");
        }
        finally { Directory.Delete(dir, recursive: true); }
    }
}
