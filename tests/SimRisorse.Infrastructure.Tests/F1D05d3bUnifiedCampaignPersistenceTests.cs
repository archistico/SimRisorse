using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class F1D05d3bUnifiedCampaignPersistenceTests
{
    private static ResourceAmount U(decimal n) => ResourceAmount.FromUnits(n);
    private static readonly SchoolResearchRules School = new("food", U(10m), 120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly MeatChainPolicy Meat = new("grain", "water", "cattle", "game", "meat",
        ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(1m), U(1m),
        [new("cattle", U(1m), U(2m), 0), new("game", U(1m), U(1m), 1)]);
    private static readonly HorticulturePolicy Crops = new("water", "vegetables", "fruit",
        new("vegetables", U(2m), U(1m), 0), new("fruit", U(3m), U(1m), 1));
    private static readonly FoodConversionPolicy Depot = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
        new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("bread", U(1m), U(1m), 4)
    ]);

    private static V15FoodCampaignCheckpoint Start()
    {
        var baseline = CivilGrowthScenario.Create();
        var city = V14CampaignSnapshots.MigrateFromV13(baseline,
            GameSession.Create(baseline).Snapshot(), School, Technologies);
        city = city with
        {
            ProductiveFamilies = [.. city.ProductiveFamilies.Select(f => f.FamilyId == "fisher"
                ? f with { Base = new TierOccupancy(1, 1) } : f)],
            PurchaseCounters = [.. city.PurchaseCounters.Select(c => c.FamilyId == "fisher"
                ? c with { Acquired = 1 } : c)]
        };
        var migrated = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, Depot), Depot);
        return migrated with
        {
            Depot = new FoodDepotStock(ResourceBundle.Empty, ResourceAmount.Zero),
            Industry = migrated.Industry with
            {
                Buildings = new FoodIndustryBuildings(new FoodIndustryCrew(0, 1, 0, 0, 0, 1),
                    new FoodIndustryCrew(0, 1, 0, 0, 0, 1))
            }
        };
    }

    private static V15FoodIndustryCampaignSession Session(V15FoodCampaignCheckpoint state) =>
        new(state, CivilGrowthScenario.Create(), School, Technologies, Depot);

    [Fact]
    public void SaveReloadThenAdvanceMatchesContinuousClockWithFractionalTime()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-rc51-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var store = new V15SqliteFoodSaveStore(Path.Combine(folder, "game.sqlite3"),
                CivilGrowthScenario.Create(), School, Technologies, Depot);
            var start = Start();
            var continuous = Session(start);
            continuous.Resume();
            Assert.True(continuous.Advance(TimeSpan.FromSeconds(5.5), Meat, Crops).Succeeded);

            var interrupted = Session(start);
            interrupted.Resume();
            Assert.True(interrupted.Advance(TimeSpan.FromSeconds(2.5), Meat, Crops).Succeeded);
            store.Save(interrupted.Snapshot());
            var resumed = Session(store.Load());
            Assert.True(resumed.IsPaused);
            resumed.Resume();
            Assert.True(resumed.Advance(TimeSpan.FromSeconds(3), Meat, Crops).Succeeded);
            var expected = continuous.Snapshot();
            var actual = resumed.Snapshot();
            Assert.Equal(expected.CityCampaign.ElapsedTicks, actual.CityCampaign.ElapsedTicks);
            Assert.Equal(expected.CityCampaign.Resources, actual.CityCampaign.Resources);
            Assert.Equal(expected.CityCampaign.Population, actual.CityCampaign.Population);
            Assert.Equal(expected.Depot, actual.Depot);
            Assert.Equal(expected.Industry, actual.Industry);
            Assert.Equal(expected.CityCampaign.Military, actual.CityCampaign.Military);
            Assert.Equal(expected.CityCampaign.SchoolResearch, actual.CityCampaign.SchoolResearch);
            Assert.Equal(expected.CityCampaign.ResearchBalanceMilliPoints,
                actual.CityCampaign.ResearchBalanceMilliPoints);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public void SavedAnimalsAndTransitRemainExactAcrossNextClockStep()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-rc51-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var store = new V15SqliteFoodSaveStore(Path.Combine(folder, "game.sqlite3"),
                CivilGrowthScenario.Create(), School, Technologies, Depot);
            var original = Start();
            var start = original with
            {
                Industry = original.Industry with
                {
                    Animals = ResourceBundle.Create(("game", 2m)),
                    Transit = ResourceBundle.Create(("fruit", 3m)),
                    Buildings = new FoodIndustryBuildings(new FoodIndustryCrew(0, 1, 0, 0, 0, 1),
                        new FoodIndustryCrew(0, 1, 0, 0, 0, 1))
                }
            };
            var a = Session(start);
            a.Resume();
            Assert.True(a.Advance(TimeSpan.FromSeconds(1), Meat, Crops).Succeeded);
            store.Save(a.Snapshot());
            var b = Session(store.Load());
            b.Resume();
            Assert.True(b.Advance(TimeSpan.FromSeconds(2), Meat, Crops).Succeeded);
            var expected = Session(start);
            expected.Resume();
            Assert.True(expected.Advance(TimeSpan.FromSeconds(3), Meat, Crops).Succeeded);
            Assert.Equal(expected.Snapshot().Industry, b.Snapshot().Industry);
            Assert.Equal(expected.Snapshot().Depot, b.Snapshot().Depot);
            Assert.Equal(expected.Snapshot().CityCampaign.Resources, b.Snapshot().CityCampaign.Resources);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}
