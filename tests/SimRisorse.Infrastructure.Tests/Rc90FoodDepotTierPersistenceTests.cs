using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Application.Persistence;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>Depot tiers extend the existing generation-6 site table; old saves still load.</summary>
public sealed class Rc90FoodDepotTierPersistenceTests
{
    private static V15FoodCampaignCheckpoint Start(V15BenchmarkFixture fixture, bool tiers)
    {
        var current = fixture.Start;
        return current with
        {
            CityCampaign = current.CityCampaign with
            {
                UnlockedTechnologyIds = tiers ? ["alimentazione-1", "alimentazione-2"] : []
            },
            Industry = current.Industry with
            {
                Buildings = new FoodIndustryBuildings(
                    new FoodIndustryCrew(0, 0, 0, 0, 0, 1,
                        tiers ? 1 : 0, tiers ? 1 : 0),
                    new FoodIndustryCrew(0, 0, 0, 0, 0, 0,
                        tiers ? 1 : 0, tiers ? 1 : 0))
            }
        };
    }

    [Fact]
    public void LargeAndAdvancedDepotCountsAndStaffAreSavedAndRestored()
    {
        var fixture = V15BenchmarkFixture.Create();
        var file = Path.Combine(Path.GetTempPath(), "rc90-" + Guid.NewGuid().ToString("N") + ".sqlite3");
        try
        {
            var store = new V15SqliteFoodSaveStore(file, fixture.Baseline, fixture.School,
                fixture.Technologies, fixture.Depot);
            var expected = Start(fixture, true);
            store.Save(expected);
            var actual = store.Load();
            Assert.Equal(expected.Industry.Buildings, actual.Industry.Buildings);
            Assert.Equal(6, V15SqliteFoodSaveStore.FileVersion);
            Assert.Equal(ResourceAmount.FromUnits(1400m),
                store.Load().Industry.Buildings.Counts.DepotLarge == 1
                    ? FoodIndustryConstructionCatalog.Candidate.CapacitiesFor(actual.Industry.Buildings).DepotReady
                    : ResourceAmount.Zero);
        }
        finally { if (File.Exists(file)) File.Delete(file); }
    }

    [Fact]
    public void OlderSixRowCheckpointLoadsWithZeroLargeAndAdvancedTiers()
    {
        var fixture = V15BenchmarkFixture.Create();
        var file = Path.Combine(Path.GetTempPath(), "rc90-" + Guid.NewGuid().ToString("N") + ".sqlite3");
        try
        {
            var store = new V15SqliteFoodSaveStore(file, fixture.Baseline, fixture.School,
                fixture.Technologies, fixture.Depot);
            store.Save(Start(fixture, false));
            using (var connection = new SqliteConnection($"Data Source={file};Pooling=False"))
            {
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM food_industry_site WHERE site_id IN ('food-depot-large','food-depot-advanced')";
                Assert.Equal(2, cmd.ExecuteNonQuery());
            }
            var restored = store.Load();
            Assert.Equal(0, restored.Industry.Buildings.Counts.DepotLarge);
            Assert.Equal(0, restored.Industry.Buildings.Counts.DepotAdvanced);
        }
        finally { if (File.Exists(file)) File.Delete(file); }
    }

    [Fact]
    public void PartialSevenRowDepotPortfolioIsNotAccepted()
    {
        var fixture = V15BenchmarkFixture.Create();
        var file = Path.Combine(Path.GetTempPath(), "rc90-" + Guid.NewGuid().ToString("N") + ".sqlite3");
        try
        {
            var store = new V15SqliteFoodSaveStore(file, fixture.Baseline, fixture.School,
                fixture.Technologies, fixture.Depot);
            store.Save(Start(fixture, false));
            using (var connection = new SqliteConnection($"Data Source={file};Pooling=False"))
            {
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM food_industry_site WHERE site_id = 'food-depot-large'";
                Assert.Equal(1, cmd.ExecuteNonQuery());
            }
            Assert.Throws<GameSaveException>(() => store.Load());
        }
        finally { if (File.Exists(file)) File.Delete(file); }
    }
}
