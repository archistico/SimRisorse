using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>RC64: generation-6 SQLite and the v15 in-memory state must agree
/// that the retired producer has no productive row.</summary>
public sealed class F1D05e2b2V15NativeRosterPersistenceTests
{
    private static ResourceAmount U(decimal value) => ResourceAmount.FromUnits(value);
    private static readonly SchoolResearchRules School = new("food", U(2m), 3, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Policy = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
        new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("bread", U(1m), U(1m), 4)
    ]);

    [Fact]
    public void ReloadDoesNotRegenerateRetiredFamilyEvenAfterCityCommand()
    {
        var baseline = CivilGrowthScenario.Create();
        var city = V14CampaignSnapshots.MigrateFromV13(baseline,
            GameSession.Create(baseline).Snapshot(), School, Technologies);
        var seed = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, Policy), Policy);
        var session = new V15FoodIndustryCampaignSession(seed, baseline, School, Technologies, Policy);
        var directory = Path.Combine(Path.GetTempPath(), "simrisorse-rc64-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var store = new V15SqliteFoodSaveStore(Path.Combine(directory, "new-v15.sqlite3"),
                baseline, School, Technologies, Policy);
            store.Save(session.Snapshot());
            var loaded = new V15FoodIndustryCampaignSession(store.Load(), baseline, School, Technologies, Policy);
            Assert.Equal(15, loaded.Snapshot().CityCampaign.ProductiveFamilies.Length);
            Assert.DoesNotContain(loaded.Snapshot().CityCampaign.ProductiveFamilies,
                f => f.FamilyId == "food-store");
            Assert.Equal(seed.CityCampaign.Resources, loaded.Snapshot().CityCampaign.Resources);
            Assert.Equal(seed.Depot, loaded.Snapshot().Depot);
            Assert.Equal(seed.Industry, loaded.Snapshot().Industry);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
