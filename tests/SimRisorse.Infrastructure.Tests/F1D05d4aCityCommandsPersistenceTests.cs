using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>RC52: cross-system commands use the same SQLite generation-5 checkpoint.</summary>
public sealed class F1D05d4aCityCommandsPersistenceTests
{
    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);
    private static readonly SchoolResearchRules School = new("food", U(2m), 3, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly FoodConversionPolicy Depot = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
        new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("bread", U(1m), U(1m), 4)
    ]);
    private static readonly MeatChainPolicy Meat = new("grain", "water", "cattle", "game", "meat",
        ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(1m), U(1m),
        [new("cattle", U(1m), U(2m), 0), new("game", U(1m), U(1m), 1)]);
    private static readonly HorticulturePolicy Crops = new("water", "vegetables", "fruit",
        new("vegetables", U(2m), U(1m), 0), new("fruit", U(3m), U(1m), 1));

    private static V15FoodCampaignCheckpoint Seed()
    {
        var baseline = CivilGrowthScenario.Create();
        var city = V14CampaignSnapshots.MigrateFromV13(baseline,
            GameSession.Create(baseline).Snapshot(), School, Technologies);
        city = city with { Resources = city.Resources + ResourceBundle.Create(
            ("planks", 500m), ("stone", 500m), ("swords", 25m)),
            ResearchBalanceMilliPoints = ResearchPoints.FromUnits(100m).MilliPoints };
        return V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, Depot), Depot);
    }

    private static V15FoodIndustryCampaignSession Session(V15FoodCampaignCheckpoint checkpoint) =>
        new(checkpoint, CivilGrowthScenario.Create(), School, Technologies, Depot);

    [Fact]
    public void SchoolResearchFoodStaffAndCityConstructionSurviveSaveAndContinue()
    {
        var dir = Path.Combine(Path.GetTempPath(), "simrisorse-rc52-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var game = Session(Seed());
            Assert.True(game.BuildCityProducer("fisher", ProductionTier.Base, 1).Succeeded);
            Assert.True(game.Build("food-depot", 1).Succeeded);
            Assert.True(game.BuildSchool(1).Succeeded);
            Assert.True(game.Build("hunter", 1).Succeeded);
            Assert.Equal(FoodIndustryActionFailure.None, game.AssignWorkers("hunter", 1));
            Assert.Equal(SchoolActionFailure.None, game.EnrollStudents(1));
            Assert.Equal(TechnologyPurchaseFailure.None, game.BuyTechnology("lavorazioni-1"));
            var before = game.Snapshot();
            var store = new V15SqliteFoodSaveStore(Path.Combine(dir, "save.sqlite3"),
                CivilGrowthScenario.Create(), School, Technologies, Depot);
            store.Save(before);
            var restored = Session(store.Load());
            Assert.True(restored.IsPaused);
            Assert.Equal(before.Industry, restored.Snapshot().Industry);
            Assert.Equal(before.Depot, restored.Snapshot().Depot);
            Assert.Equal(before.CityCampaign.Resources, restored.Snapshot().CityCampaign.Resources);
            Assert.Equal(before.CityCampaign.SchoolResearch.ReservedPopulation,
                restored.Snapshot().CityCampaign.SchoolResearch.ReservedPopulation);
            Assert.Equal(before.CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray(),
                restored.Snapshot().CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray());
            Assert.Equal(before.CityCampaign.UnlockedTechnologyIds.ToArray(),
                restored.Snapshot().CityCampaign.UnlockedTechnologyIds.ToArray());
            Assert.Equal(game.AvailableWorkers, restored.AvailableWorkers);
            game.Resume();
            restored.Resume();
            Assert.True(game.Advance(TimeSpan.FromSeconds(4), Meat, Crops).Succeeded);
            Assert.True(restored.Advance(TimeSpan.FromSeconds(4), Meat, Crops).Succeeded);
            Assert.Equal(game.Snapshot().Depot, restored.Snapshot().Depot);
            Assert.Equal(game.Snapshot().CityCampaign.Resources, restored.Snapshot().CityCampaign.Resources);
            Assert.Equal(game.Snapshot().CityCampaign.Military, restored.Snapshot().CityCampaign.Military);
            Assert.Equal(game.Snapshot().CityCampaign.SchoolResearch.QualifiedResearchers,
                restored.Snapshot().CityCampaign.SchoolResearch.QualifiedResearchers);
            Assert.Equal(game.Snapshot().CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray(),
                restored.Snapshot().CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray());
            Assert.Equal(game.Snapshot().CityCampaign.ResearchBalanceMilliPoints,
                restored.Snapshot().CityCampaign.ResearchBalanceMilliPoints);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    [Fact]
    public void RetiredProducerDoesNotReappearAfterSaveLoad()
    {
        var dir = Path.Combine(Path.GetTempPath(), "simrisorse-rc52-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var original = Seed();
            var city = V15LegacyProducerAdapter.ForV14Rules(original.CityCampaign);
            original = original with { CityCampaign = city with
            {
                ProductiveFamilies = [.. city.ProductiveFamilies.Select(f => f.FamilyId == "food-store"
                    ? f with { Base = new TierOccupancy(1, 1) } : f)],
                PurchaseCounters = [.. city.PurchaseCounters.Select(c => c.FamilyId == "food-store"
                    ? c with { Acquired = 1 } : c)]
            } };
            var game = Session(original);
            Assert.True(game.RetireLegacyFoodStore().Succeeded);
            var store = new V15SqliteFoodSaveStore(Path.Combine(dir, "save.sqlite3"),
                CivilGrowthScenario.Create(), School, Technologies, Depot);
            store.Save(game.Snapshot());
            var loaded = Session(store.Load());
            Assert.DoesNotContain(loaded.Snapshot().CityCampaign.ProductiveFamilies,
                f => f.FamilyId == "food-store");
            Assert.Equal(1, loaded.Snapshot().CityCampaign.PurchaseCounters.Single(c => c.FamilyId == "food-store").Acquired);
            Assert.Equal(original.CityCampaign.Resources, loaded.Snapshot().CityCampaign.Resources);
            Assert.Equal(original.Depot, loaded.Snapshot().Depot);
            Assert.DoesNotContain(loaded.CityCatalog.Buildings, b => b.Id == "food-store");
        }
        finally { Directory.Delete(dir, recursive: true); }
    }
}
