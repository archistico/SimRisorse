using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>RC50: constructed food buildings, their staff and stock survive an SQLite 5 restart.</summary>
public sealed class F1D05d3FoodIndustrySessionPersistenceTests
{
    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);
    private static readonly SchoolResearchRules School = new("food", U(10m), 120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Depot = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("bread", U(1m), U(1m), 1),
        new("meat", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("vegetables", U(1m), U(1m), 4)
    ]);

    private static V15FoodIndustryCampaignSession Start()
    {
        var scenario = CivilGrowthScenario.Create();
        var v14 = V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), School, Technologies);
        v14 = v14 with
        {
            Resources = v14.Resources + ResourceBundle.Create(("planks", 600m), ("stone", 600m)),
            ProductiveFamilies = [.. v14.ProductiveFamilies.Select(item => item.FamilyId == "fisher"
                ? item with { Base = item.Base with { Buildings = 1 } } : item)],
            PurchaseCounters = [.. v14.PurchaseCounters.Select(item => item.FamilyId == "fisher"
                ? item with { Acquired = 1 } : item)]
        };
        var initial = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(v14, Depot), Depot);
        return new(initial, scenario, School, Technologies, Depot);
    }

    [Fact]
    public void ConstructedStaffedHunterAndDepotCanBeRestoredWithCostsAlreadyDebited()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-rc50-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var filename = Path.Combine(folder, "food.sqlite3");
            var session = Start();
            Assert.True(session.Build("hunter", 1).Succeeded);
            Assert.True(session.Build("food-depot", 1).Succeeded);
            Assert.Equal(FoodIndustryActionFailure.None, session.AssignWorkers("hunter", 1));
            Assert.Equal(FoodIndustryActionFailure.None, session.AssignWorkers("food-depot", 1));
            var before = session.Snapshot();
            var store = new V15SqliteFoodSaveStore(filename, CivilGrowthScenario.Create(),
                School, Technologies, Depot);
            store.Save(before);
            var restored = new V15FoodIndustryCampaignSession(store.Load(), CivilGrowthScenario.Create(),
                School, Technologies, Depot);
            Assert.Equal(before.Industry, restored.Snapshot().Industry);
            Assert.Equal(before.Depot, restored.Snapshot().Depot);
            Assert.Equal(before.CityCampaign.Resources, restored.Snapshot().CityCampaign.Resources);
            Assert.Equal(before.CityCampaign.Population, restored.Snapshot().CityCampaign.Population);
            Assert.Equal(before.CityCampaign.ElapsedTicks, restored.Snapshot().CityCampaign.ElapsedTicks);
            Assert.Equal(session.Capacities, restored.Capacities);
            Assert.Equal(session.AvailableWorkers, restored.AvailableWorkers);
            Assert.Equal(U(100m), restored.Capacities.DepotReady);
            Assert.Equal(U(20m), restored.Capacities.Animals["game"]);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void FailedPurchaseDoesNotChangeNextSavedCheckpoint()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-rc50-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var filename = Path.Combine(folder, "food.sqlite3");
            var session = Start();
            var before = session.Snapshot();
            Assert.Equal(FoodIndustryActionFailure.MissingPrerequisite, session.Build("stable", 1).Failure);
            var store = new V15SqliteFoodSaveStore(filename, CivilGrowthScenario.Create(),
                School, Technologies, Depot);
            store.Save(session.Snapshot());
            var loaded = store.Load();
            Assert.Equal(before.Industry, loaded.Industry);
            Assert.Equal(before.Depot, loaded.Depot);
            Assert.Equal(before.CityCampaign.Resources, loaded.CityCampaign.Resources);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
