using Microsoft.Data.Sqlite;
using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Application.Persistence;
using SimRisorse.Core.Food;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class Rc81MintCrewSqliteTests
{
    [Theory]
    [InlineData(V15CrewMode.Legacy, V15FoodCampaignCheckpoints.RulesVersion)]
    [InlineData(V15CrewMode.MintTen, V15CrewRules.MintTenRulesVersion)]
    public void SqliteRoundTripPreservesExactRulesWithoutChangingGenerationSixSchema(
        V15CrewMode mode, string expectedHeader)
    {
        var fx = V15BenchmarkFixture.Create();
        var path = Path.Combine(Path.GetTempPath(), "sr-rc81-" + Guid.NewGuid().ToString("N") + ".sqlite3");
        try
        {
            var store = new V15SqliteFoodSaveStore(path, fx.Baseline,
                fx.School, fx.Technologies, fx.Depot);
            var initial = fx.Start with { CrewMode = mode };
            store.Save(initial);
            using (var db = new SqliteConnection(new SqliteConnectionStringBuilder
                { DataSource = path, Pooling = false }.ToString()))
            {
                db.Open();
                using var cmd = db.CreateCommand();
                cmd.CommandText = "SELECT rules_version FROM game_save WHERE slot=1";
                Assert.Equal(expectedHeader, cmd.ExecuteScalar());
                cmd.CommandText = "PRAGMA user_version";
                Assert.Equal(6L, Convert.ToInt64(cmd.ExecuteScalar()));
            }
            var loaded = store.Load();
            Assert.Equal(mode, loaded.CrewMode);
            Assert.Equal(initial.CityCampaign.Resources, loaded.CityCampaign.Resources);
            Assert.Equal(initial.Industry, loaded.Industry);
            Assert.Equal(initial.Depot, loaded.Depot);
            new V15FoodIndustryCampaignSession(loaded, fx.Baseline,
                fx.School, fx.Technologies, fx.Depot).EnableMintCrews();
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void ActiveTenPersonMintSurvivesSaveReloadAndOneRealCampaignTick()
    {
        var fx = V15BenchmarkFixture.Create();
        var city = fx.Start.CityCampaign;
        city = city with
        {
            Population = 30,
            Resources = city.Resources + ResourceBundle.Create(("gold", 6m), ("coal", 6m)),
            OtherBuildings = city.OtherBuildings.Select(b => b.BuildingId == "small-house"
                ? b with { Count = 4 } : b).ToImmutableArray(),
            ProductiveFamilies = city.ProductiveFamilies.Select(f => f.FamilyId == "mint"
                ? f with { Base = new TierOccupancy(1, 10) } : f).ToImmutableArray(),
            PurchaseCounters = city.PurchaseCounters.Select(c => c.FamilyId switch
            {
                "mint" => c with { Acquired = 1 },
                "small-house" => c with { Acquired = 4 },
                _ => c
            }).ToImmutableArray()
        };
        var path = Path.Combine(Path.GetTempPath(), "sr-rc81-" + Guid.NewGuid().ToString("N") + ".sqlite3");
        try
        {
            var store = new V15SqliteFoodSaveStore(path, fx.Baseline,
                fx.School, fx.Technologies, fx.Depot);
            store.Save(fx.Start with { CityCampaign = city, CrewMode = V15CrewMode.MintTen });
            var game = new V15FoodIndustryCampaignSession(store.Load(), fx.Baseline,
                fx.School, fx.Technologies, fx.Depot);
            Assert.Equal(10, game.CityCatalog.Buildings.Single(b => b.Id == "mint").AssignedWorkers);
            game.Resume();
            Assert.True(game.Advance(TimeSpan.FromSeconds(1), fx.Meat, fx.Crops).Succeeded);
            Assert.Equal(3m, game.Snapshot().CityCampaign.Resources["coins"].Units);
            store.Save(game.Snapshot());
            var restored = store.Load();
            Assert.Equal(V15CrewMode.MintTen, restored.CrewMode);
            Assert.Equal(10, restored.CityCampaign.ProductiveFamilies.Single(f =>
                f.FamilyId == "mint").Base.AssignedWorkers);
            Assert.Equal(3m, restored.CityCampaign.Resources["coins"].Units);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void UnexpectedCrewHeaderIsRejectedRatherThanGuessingItsMeaning()
    {
        var fx = V15BenchmarkFixture.Create();
        var path = Path.Combine(Path.GetTempPath(), "sr-rc81-" + Guid.NewGuid().ToString("N") + ".sqlite3");
        try
        {
            var store = new V15SqliteFoodSaveStore(path, fx.Baseline,
                fx.School, fx.Technologies, fx.Depot);
            store.Save(fx.Start with { CrewMode = V15CrewMode.MintTen });
            using (var db = new SqliteConnection(new SqliteConnectionStringBuilder
                { DataSource = path, Pooling = false }.ToString()))
            {
                db.Open();
                using var cmd = db.CreateCommand();
                cmd.CommandText = "UPDATE game_save SET rules_version='unknown-mint-rules' WHERE slot=1";
                cmd.ExecuteNonQuery();
            }
            Assert.Throws<GameSaveException>(() => store.Load());
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}
