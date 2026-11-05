using Microsoft.Data.Sqlite;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>F1-D05b: the v15 file is independent and cannot mutate live v14 checkpoints.</summary>
public sealed class V15FoodSaveStoreTests
{
    private static readonly SchoolResearchRules School = new(
        "food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;

    private static FoodConversionPolicy Policy() => UnifiedFoodInputs.CreatePolicy([
        new("fish", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 0),
        new("bread", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 1),
        new("meat", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 2),
        new("fruit", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 3),
        new("vegetables", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 4)
    ]);

    private sealed class Sandbox : IDisposable
    {
        public string Folder { get; } = Path.Combine(Path.GetTempPath(),
            "simrisorse-v15-test-" + Guid.NewGuid().ToString("N"));
        public string V14 => Path.Combine(Folder, "original-v14.sqlite3");
        public string V15 => Path.Combine(Folder, "isolated-v15.sqlite3");
        public Sandbox() => Directory.CreateDirectory(Folder);
        public V14SqliteSaveStore Old() => new(V14, CivilGrowthScenario.Create(), School, Technologies);
        public V15SqliteFoodSaveStore New() => new(V15, CivilGrowthScenario.Create(),
            School, Technologies, Policy());
        public void Execute(string sql)
        {
            using var conn = new SqliteConnection(new SqliteConnectionStringBuilder
            { DataSource = V15, Pooling = false, ForeignKeys = true }.ToString());
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
        public void Dispose() => Directory.Delete(Folder, recursive: true);
    }

    private static V14CampaignSnapshot Legacy()
    {
        var scenario = CivilGrowthScenario.Create();
        var snapshot = V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), School, Technologies);
        // Fractions are significant: v15 cannot round or silently convert them.
        return snapshot with { Resources = snapshot.Resources + ResourceBundle.Create(
            ("fish", 4.125m), ("bread", 2.333m), ("food", 0.875m)) };
    }

    private static V15FoodCampaignCheckpoint Stage(V14CampaignSnapshot old)
    {
        var policy = Policy();
        var plan = V14FoodStockMigration.Prepare(old, policy);
        return V15FoodCampaignCheckpoints.FromMigrationPlan(plan, policy);
    }

    private static void Compare(V15FoodCampaignCheckpoint expected, V15FoodCampaignCheckpoint actual)
    {
        Assert.Equal(expected.CityCampaign.Resources, actual.CityCampaign.Resources);
        Assert.Equal(expected.Depot.RawFood, actual.Depot.RawFood);
        Assert.Equal(expected.Depot.ReadyFood, actual.Depot.ReadyFood);
        Assert.Equal(expected.CityCampaign.Population, actual.CityCampaign.Population);
        Assert.Equal(expected.CityCampaign.ElapsedTicks, actual.CityCampaign.ElapsedTicks);
        Assert.Equal(expected.CityCampaign.ResearchBalanceMilliPoints, actual.CityCampaign.ResearchBalanceMilliPoints);
        Assert.Equal(expected.CityCampaign.SchoolResearch.WaitingStudents,
            actual.CityCampaign.SchoolResearch.WaitingStudents);
        Assert.Equal(expected.CityCampaign.SchoolResearch.QualifiedResearchers,
            actual.CityCampaign.SchoolResearch.QualifiedResearchers);
        Assert.Equal(expected.CityCampaign.SchoolResearch.ActiveResearchers,
            actual.CityCampaign.SchoolResearch.ActiveResearchers);
        Assert.Equal(expected.CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray(),
            actual.CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray());
        Assert.Equal(expected.CityCampaign.Military.WaitingRecruits, actual.CityCampaign.Military.WaitingRecruits);
        Assert.Equal(expected.CityCampaign.Military.TrainingRecruits, actual.CityCampaign.Military.TrainingRecruits);
        Assert.Equal(expected.CityCampaign.Military.AvailableSoldiers, actual.CityCampaign.Military.AvailableSoldiers);
        Assert.Equal(expected.CityCampaign.Military.TrainingProgressSeconds, actual.CityCampaign.Military.TrainingProgressSeconds);
        Assert.Equal(expected.CityCampaign.Military.IndividualProgressSeconds.ToArray(),
            actual.CityCampaign.Military.IndividualProgressSeconds.ToArray());
        Assert.Equal(expected.CityCampaign.OtherBuildings.OrderBy(x => x.BuildingId),
            actual.CityCampaign.OtherBuildings.OrderBy(x => x.BuildingId));
        Assert.Equal(expected.CityCampaign.ProductiveFamilies.OrderBy(x => x.FamilyId),
            actual.CityCampaign.ProductiveFamilies.OrderBy(x => x.FamilyId));
        Assert.Equal(expected.CityCampaign.PurchaseCounters.OrderBy(x => x.FamilyId),
            actual.CityCampaign.PurchaseCounters.OrderBy(x => x.FamilyId));
        Assert.Equal(expected.CityCampaign.UnlockedTechnologyIds.OrderBy(x => x),
            actual.CityCampaign.UnlockedTechnologyIds.OrderBy(x => x));
    }

    [Fact]
    public void V15RoundTripPreservesCityDepotAndMilliunitsExactly()
    {
        using var env = new Sandbox();
        var original = Legacy();
        var expected = Stage(original);
        env.New().Save(expected);
        var actual = env.New().Load();
        Compare(expected, actual);
        Assert.Equal(original.Resources, V15FoodCampaignCheckpoints.ToLegacyPreview(actual,
            CivilGrowthScenario.Create(), School, Technologies, Policy()).Resources);
        Assert.Equal(ResourceAmount.Zero, actual.CityCampaign.Resources["fish"]);
        Assert.Equal(ResourceAmount.Zero, actual.CityCampaign.Resources["food"]);
        Assert.Equal(ResourceAmount.FromUnits(4.125m), actual.Depot.RawFood["fish"]);
        Assert.Equal(ResourceAmount.FromUnits(2.333m), actual.Depot.RawFood["bread"]);
    }

    [Fact]
    public void ANewV15FileDoesNotReadOrModifyTheV14Original()
    {
        using var env = new Sandbox();
        var original = Legacy();
        env.Old().Save(original);
        var before = File.ReadAllBytes(env.V14);
        var plan = env.Old().PreviewFoodMigration(Policy());
        env.New().Save(V15FoodCampaignCheckpoints.FromMigrationPlan(plan, Policy()));
        Assert.Equal(before, File.ReadAllBytes(env.V14));
        Assert.Equal(original.Resources, env.Old().Load().Resources);
        Assert.DoesNotContain(Directory.GetFiles(env.Folder),
            filename => Path.GetFileName(filename).Contains("v14.backup", StringComparison.Ordinal));
        Assert.NotEqual(env.V14, env.V15);
    }

    [Fact]
    public void V15CannotReplaceAnExistingV14FileEvenAtTheSameDestination()
    {
        using var env = new Sandbox();
        var original = Legacy();
        new V14SqliteSaveStore(env.V15, CivilGrowthScenario.Create(), School, Technologies).Save(original);
        var bytes = File.ReadAllBytes(env.V15);
        var ex = Assert.Throws<GameSaveException>(() => env.New().Save(Stage(original)));
        Assert.Equal(GameSaveError.UnsupportedFormat, ex.Error);
        Assert.Equal(bytes, File.ReadAllBytes(env.V15));
    }

    [Fact]
    public void OverwritingValidV15PreservesAPreviousVerifiedSqliteBackup()
    {
        using var env = new Sandbox();
        var first = Stage(Legacy());
        env.New().Save(first);
        var updated = first with
        {
            Depot = first.Depot with { ReadyFood = first.Depot.ReadyFood + ResourceAmount.FromUnits(1m) }
        };
        env.New().Save(updated);
        var backup = Path.Combine(env.Folder, "isolated-v15.v15.backup.sqlite3");
        Assert.True(File.Exists(backup));
        Compare(first, new V15SqliteFoodSaveStore(backup, CivilGrowthScenario.Create(),
            School, Technologies, Policy()).Load());
        Compare(updated, env.New().Load());
    }

    [Fact]
    public void MissingReadyFoodRowIsRejectedAndExistingFileCannotBeOverwritten()
    {
        using var env = new Sandbox();
        var checkpoint = Stage(Legacy());
        env.New().Save(checkpoint);
        env.Execute("DELETE FROM food_ready_stock WHERE slot=1");
        var bytes = File.ReadAllBytes(env.V15);
        Assert.Equal(GameSaveError.InvalidData, Assert.Throws<GameSaveException>(() => env.New().Load()).Error);
        Assert.Throws<GameSaveException>(() => env.New().Save(checkpoint));
        Assert.Equal(bytes, File.ReadAllBytes(env.V15));
    }

    [Fact]
    public void UndeliveredCityFishAndBreadSurviveV15SQLiteRoundTrip()
    {
        using var env = new Sandbox();
        var original = Stage(Legacy());
        var awaitingDelivery = original with
        {
            CityCampaign = original.CityCampaign with
            {
                Resources = original.CityCampaign.Resources +
                    ResourceBundle.Create(("fish", 2.111m), ("bread", 3.222m))
            },
            Depot = original.Depot with
            {
                RawFood = original.Depot.RawFood +
                    ResourceBundle.Create(("fish", 4.333m), ("bread", 5.444m))
            }
        };
        env.New().Save(awaitingDelivery);
        var reloaded = env.New().Load();
        Compare(awaitingDelivery, reloaded);
        Assert.Equal(ResourceAmount.FromUnits(2.111m), reloaded.CityCampaign.Resources["fish"]);
        Assert.Equal(ResourceAmount.FromUnits(3.222m), reloaded.CityCampaign.Resources["bread"]);
        Assert.Equal(awaitingDelivery.Depot.RawFood, reloaded.Depot.RawFood);
        Assert.Equal(ResourceAmount.Zero, reloaded.CityCampaign.Resources["food"]);
    }

    [Fact]
    public void PreparedFoodCannotAppearInCityAndDepot()
    {
        using var env = new Sandbox();
        var checkpoint = Stage(Legacy());
        Assert.Throws<ArgumentException>(() => V15FoodCampaignCheckpoints.Validate(
            checkpoint with { CityCampaign = checkpoint.CityCampaign with
            { Resources = checkpoint.CityCampaign.Resources + ResourceBundle.Create(("food", 1m)) } },
            CivilGrowthScenario.Create(), School, Technologies, Policy()));
        env.New().Save(checkpoint);
        env.Execute("INSERT INTO resource_stock VALUES(1,'food',1000)");
        Assert.Equal(GameSaveError.InvalidData, Assert.Throws<GameSaveException>(() => env.New().Load()).Error);
    }

    [Fact]
    public void FutureFileVersionsAreRejectedWithoutMutatingTheFile()
    {
        using var env = new Sandbox();
        env.New().Save(Stage(Legacy()));
        env.Execute("PRAGMA user_version=99");
        var bytes = File.ReadAllBytes(env.V15);
        Assert.Equal(GameSaveError.UnsupportedFormat, Assert.Throws<GameSaveException>(() => env.New().Load()).Error);
        Assert.Throws<GameSaveException>(() => env.New().Save(Stage(Legacy())));
        Assert.Equal(bytes, File.ReadAllBytes(env.V15));
    }

    [Fact]
    public void FutureFoodAndHistoricalOverCapacityAreStoredWithoutLoss()
    {
        using var env = new Sandbox();
        var checkpoint = Stage(Legacy());
        var newDepot = checkpoint.Depot with
        {
            RawFood = checkpoint.Depot.RawFood + ResourceBundle.Create(("meat", 3.777m),
                ("fruit", 1200m), ("vegetables", 600m))
        };
        // The future depot cap may be below these stocks: v15 must not clip them.
        var extended = checkpoint with { Depot = newDepot };
        env.New().Save(extended);
        Compare(extended, env.New().Load());
    }

    [Fact]
    public void MissingV15FileIsNotCreatedByLoad()
    {
        using var env = new Sandbox();
        var files = Directory.GetFiles(env.Folder);
        Assert.Equal(GameSaveError.NotFound, Assert.Throws<GameSaveException>(() => env.New().Load()).Error);
        Assert.Equal(files, Directory.GetFiles(env.Folder));
    }
}
