using System.Collections.Immutable;
using Microsoft.Data.Sqlite;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>Isolated SQLite format-3 checks. No user files and no change to the v13 application store.</summary>
public sealed class V14SqliteSaveStoreTests
{
    private static readonly SchoolResearchRules School = new("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;

    private sealed class Sandbox : IDisposable
    {
        public string DirectoryName { get; } = Path.Combine(Path.GetTempPath(), "simrisorse-e3b2-" + Guid.NewGuid().ToString("N"));
        public string V13 => Path.Combine(DirectoryName, "original.sqlite3");
        public string V14 => Path.Combine(DirectoryName, "advanced.sqlite3");
        public Sandbox() => Directory.CreateDirectory(DirectoryName);
        public V14SqliteSaveStore Store() => new(V14, CivilGrowthScenario.Create(), School, Technologies);
        public int Execute(string sql)
        {
            using var db = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = V14, Pooling = false }.ToString());
            db.Open();
            using var command = db.CreateCommand();
            command.CommandText = sql;
            return command.ExecuteNonQuery();
        }
        public void Dispose() => Directory.Delete(DirectoryName, recursive: true);
    }

    private static V14CampaignSnapshot Migrated() => V14CampaignSnapshots.MigrateFromV13(
        CivilGrowthScenario.Create(), GameSession.Create(CivilGrowthScenario.Create()).Snapshot(), School, Technologies);

    private static void Equivalent(V14CampaignSnapshot expected, V14CampaignSnapshot actual)
    {
        Assert.Equal(expected.FormatVersion, actual.FormatVersion);
        Assert.Equal(expected.ScenarioId, actual.ScenarioId);
        Assert.Equal(expected.RulesVersion, actual.RulesVersion);
        Assert.Equal(expected.ElapsedTicks, actual.ElapsedTicks);
        Assert.Equal(expected.Population, actual.Population);
        Assert.Equal(expected.Resources, actual.Resources);
        // These are keyed sets in SQLite, not positional sequences. Compare complete records
        // by stable identity, so SQL row ordering cannot produce false round-trip failures.
        Assert.Equal(expected.OtherBuildings.OrderBy(b => b.BuildingId, StringComparer.Ordinal).ToArray(),
            actual.OtherBuildings.OrderBy(b => b.BuildingId, StringComparer.Ordinal).ToArray());
        Assert.Equal(expected.ProductiveFamilies.OrderBy(f => f.FamilyId, StringComparer.Ordinal).ToArray(),
            actual.ProductiveFamilies.OrderBy(f => f.FamilyId, StringComparer.Ordinal).ToArray());
        Assert.Equal(expected.PurchaseCounters.OrderBy(c => c.FamilyId, StringComparer.Ordinal).ToArray(),
            actual.PurchaseCounters.OrderBy(c => c.FamilyId, StringComparer.Ordinal).ToArray());
        Assert.Equal(expected.SchoolCount, actual.SchoolCount);
        Assert.Equal(expected.GrowthProgressSeconds, actual.GrowthProgressSeconds);
        Assert.Equal(expected.GrowthRemainderTwentieths, actual.GrowthRemainderTwentieths);
        Assert.Equal(expected.Military, actual.Military);
        Assert.Equal(expected.SchoolResearch.WaitingStudents, actual.SchoolResearch.WaitingStudents);
        Assert.Equal(expected.SchoolResearch.TrainingProgressSeconds.ToArray(), actual.SchoolResearch.TrainingProgressSeconds.ToArray());
        Assert.Equal(expected.SchoolResearch.QualifiedResearchers, actual.SchoolResearch.QualifiedResearchers);
        Assert.Equal(expected.SchoolResearch.ActiveResearchers, actual.SchoolResearch.ActiveResearchers);
        Assert.Equal(expected.ResearchBalanceMilliPoints, actual.ResearchBalanceMilliPoints);
        Assert.Equal(expected.UnlockedTechnologyIds.OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            actual.UnlockedTechnologyIds.OrderBy(id => id, StringComparer.Ordinal).ToArray());
    }

    private static void Invalid(Action action) =>
        Assert.Equal(GameSaveError.InvalidData, Assert.Throws<GameSaveException>(action).Error);

    [Fact]
    public void V14RoundTripUsesIndependentGenerationAndOldStoreRefusesIt()
    {
        using var sandbox = new Sandbox();
        var snapshot = Migrated() with { ElapsedTicks = TimeSpan.FromMinutes(13).Ticks,
            ResearchBalanceMilliPoints = 915_500 };
        var store = sandbox.Store();
        store.Save(snapshot);
        Equivalent(snapshot, store.Load());
        using var db = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = sandbox.V14, Pooling = false }.ToString());
        db.Open();
        using var version = db.CreateCommand();
        version.CommandText = "PRAGMA user_version;";
        Assert.Equal(3L, Convert.ToInt64(version.ExecuteScalar()));
        Assert.Equal(GameSaveError.UnsupportedFormat,
            Assert.Throws<GameSaveException>(() => new SqliteGameSaveStore(sandbox.V14).Load()).Error);
    }

    [Fact]
    public void MigrationCreatesSeparateV3FileAndKeepsLegacyBytesUntouched()
    {
        using var sandbox = new Sandbox();
        var v13 = GameSession.Create(CivilGrowthScenario.Create()).Snapshot();
        new SqliteGameSaveStore(sandbox.V13).Save(v13);
        var bytes = File.ReadAllBytes(sandbox.V13);
        var store = sandbox.Store();
        var migrated = store.SaveMigratedCopy(sandbox.V13);
        Equivalent(migrated, store.Load());
        Assert.Equal(bytes, File.ReadAllBytes(sandbox.V13));
        Assert.Equal("civil-growth-13", new SqliteGameSaveStore(sandbox.V13).Load().RulesVersion);
    }

    [Fact]
    public void SameSourceAndDestinationMigrationIsRefused()
    {
        using var sandbox = new Sandbox();
        var store = new V14SqliteSaveStore(sandbox.V13, CivilGrowthScenario.Create(), School, Technologies);
        Assert.Equal(GameSaveError.UnsupportedFormat,
            Assert.Throws<GameSaveException>(() => store.PreviewMigration(sandbox.V13)).Error);
    }

    [Fact]
    public void V14SaveCannotOverwriteLegacyV2File()
    {
        using var sandbox = new Sandbox();
        new SqliteGameSaveStore(sandbox.V14).Save(GameSession.Create(CivilGrowthScenario.Create()).Snapshot());
        var bytes = File.ReadAllBytes(sandbox.V14);
        Assert.Equal(GameSaveError.UnsupportedFormat,
            Assert.Throws<GameSaveException>(() => sandbox.Store().Save(Migrated())).Error);
        Assert.Equal(bytes, File.ReadAllBytes(sandbox.V14));
    }

    [Fact]
    public void NonemptySchoolProgressAndResearchUnlocksRoundTrip()
    {
        using var sandbox = new Sandbox();
        var snapshot = Migrated();
        snapshot = snapshot with
        {
            SchoolCount = 1,
            SchoolResearch = new SchoolResearchState(0, ImmutableArray.Create(37), 0, 0),
            ResearchBalanceMilliPoints = 250_000,
            UnlockedTechnologyIds = ["lavorazioni-1"],
            PurchaseCounters = snapshot.PurchaseCounters.Select(c => c.FamilyId == "school"
                ? c with { Acquired = 1 } : c).ToImmutableArray()
        };
        sandbox.Store().Save(snapshot);
        Equivalent(snapshot, sandbox.Store().Load());
    }

    [Fact]
    public void MilitaryIndividualTrainingProgressRoundTrips()
    {
        using var sandbox = new Sandbox();
        var snapshot = Migrated();
        snapshot = snapshot with
        {
            OtherBuildings = snapshot.OtherBuildings.Select(b => b.BuildingId == "barracks"
                ? b with { Count = 1 } : b).ToImmutableArray(),
            PurchaseCounters = snapshot.PurchaseCounters.Select(c => c.FamilyId == "barracks"
                ? c with { Acquired = 1 } : c).ToImmutableArray(),
            Military = new MilitaryTrainingState(0, 1, 0, 17, ImmutableArray.Create(17))
        };
        sandbox.Store().Save(snapshot);
        Equivalent(snapshot, sandbox.Store().Load());
    }

    [Fact]
    public void InvalidNewSnapshotDoesNotReplaceExistingFileOrMakeBackup()
    {
        using var sandbox = new Sandbox();
        var store = sandbox.Store();
        var snapshot = Migrated();
        store.Save(snapshot);
        var original = File.ReadAllBytes(sandbox.V14);
        Invalid(() => store.Save(snapshot with { ResearchBalanceMilliPoints = -1 }));
        Assert.Equal(original, File.ReadAllBytes(sandbox.V14));
        Assert.Empty(Directory.GetFiles(sandbox.DirectoryName, "*.backup*.sqlite3"));
    }

    [Fact]
    public void OverwriteKeepsConsistentS1BackupOfPreviousCheckpoint()
    {
        using var sandbox = new Sandbox();
        var store = sandbox.Store();
        var original = Migrated();
        store.Save(original);
        var changed = original with { ResearchBalanceMilliPoints = 100_000 };
        store.Save(changed);
        Equivalent(changed, store.Load());
        var backup = Path.Combine(sandbox.DirectoryName, "advanced.v14.backup.sqlite3");
        Assert.True(File.Exists(backup));
        Equivalent(original, new V14SqliteSaveStore(backup, CivilGrowthScenario.Create(), School, Technologies).Load());
        store.Save(changed with { ResearchBalanceMilliPoints = 200_000 });
        Assert.True(File.Exists(Path.Combine(sandbox.DirectoryName, "advanced.v14.backup-2.sqlite3")));
    }

    [Fact]
    public void MissingTierAndMissingTrainingPositionAreRejected()
    {
        using var sandbox = new Sandbox();
        var snapshot = Migrated() with
        {
            SchoolCount = 2,
            SchoolResearch = new SchoolResearchState(0, ImmutableArray.Create(25, 12), 0, 0)
        };
        snapshot = snapshot with { PurchaseCounters = snapshot.PurchaseCounters.Select(c => c.FamilyId == "school"
            ? c with { Acquired = 2 } : c).ToImmutableArray() };
        sandbox.Store().Save(snapshot);
        Assert.Equal(1, sandbox.Execute("DELETE FROM progression_family WHERE family_id='sawmill' AND tier=2;"));
        Invalid(() => sandbox.Store().Load());
        // Restore the row before testing a second, independent corruption.
        Assert.Equal(1, sandbox.Execute("INSERT INTO progression_family VALUES(1,'sawmill',2,0,0);"));
        // Two actual positions (0,1): deleting 0 leaves a gap, which must be rejected.
        Assert.Equal(1, sandbox.Execute("DELETE FROM progression_training WHERE slot=1 AND position=0;"));
        Invalid(() => sandbox.Store().Load());
    }

    [Fact]
    public void MissingFinalSchoolTrainingPositionIsRejectedAndCannotOverwriteCheckpoint()
    {
        using var sandbox = new Sandbox();
        var snapshot = Migrated() with
        {
            SchoolCount = 2,
            SchoolResearch = new SchoolResearchState(0, ImmutableArray.Create(25, 12), 0, 0)
        };
        snapshot = snapshot with
        {
            PurchaseCounters = snapshot.PurchaseCounters.Select(c => c.FamilyId == "school"
                ? c with { Acquired = 2 } : c).ToImmutableArray()
        };
        var store = sandbox.Store();
        store.Save(snapshot);
        // Position 0 is still present: contiguous-position checking alone misses this corruption.
        Assert.Equal(1, sandbox.Execute("DELETE FROM progression_training WHERE slot=1 AND position=1;"));
        var damaged = File.ReadAllBytes(sandbox.V14);
        Invalid(() => store.Load());
        Invalid(() => store.Save(Migrated()));
        Assert.Equal(damaged, File.ReadAllBytes(sandbox.V14));
        Assert.Empty(Directory.GetFiles(sandbox.DirectoryName, "*.backup*.sqlite3"));
    }

    [Fact]
    public void InconsistentDeclaredStudentCountIsRejected()
    {
        using var sandbox = new Sandbox();
        var snapshot = Migrated() with
        {
            SchoolCount = 1,
            SchoolResearch = new SchoolResearchState(0, ImmutableArray.Create(25), 0, 0)
        };
        snapshot = snapshot with
        {
            PurchaseCounters = snapshot.PurchaseCounters.Select(c => c.FamilyId == "school"
                ? c with { Acquired = 1 } : c).ToImmutableArray()
        };
        sandbox.Store().Save(snapshot);
        Assert.Equal(1, sandbox.Execute("UPDATE progression_school_state SET training_students=0 WHERE slot=1;"));
        Invalid(() => sandbox.Store().Load());
    }

    [Fact]
    public void PreIntegrityExperimentalV3IsNotSilentlyInterpretedAsCurrentSchema()
    {
        using var sandbox = new Sandbox();
        var store = sandbox.Store();
        store.Save(Migrated());
        // Mimics the earlier RC1/RC2 draft: valid file-version header but missing
        // the column required to detect trailing progress-row truncation.
        sandbox.Execute("ALTER TABLE progression_school_state DROP COLUMN training_students;");
        var oldBytes = File.ReadAllBytes(sandbox.V14);
        Assert.Equal(GameSaveError.UnsupportedFormat,
            Assert.Throws<GameSaveException>(() => store.Load()).Error);
        Assert.Equal(GameSaveError.UnsupportedFormat,
            Assert.Throws<GameSaveException>(() => store.Save(Migrated())).Error);
        Assert.Equal(oldBytes, File.ReadAllBytes(sandbox.V14));
        Assert.Empty(Directory.GetFiles(sandbox.DirectoryName, "*.backup*.sqlite3"));
    }

    [Fact]
    public void InvalidTechnologyAndMismatchedFileVersionAreRejected()
    {
        using var sandbox = new Sandbox();
        sandbox.Store().Save(Migrated());
        sandbox.Execute("INSERT INTO progression_technologies VALUES(1,'unknown-tech');");
        Invalid(() => sandbox.Store().Load());
        sandbox.Execute("DELETE FROM progression_technologies;");
        sandbox.Execute("PRAGMA user_version=2;");
        Assert.Equal(GameSaveError.UnsupportedFormat,
            Assert.Throws<GameSaveException>(() => sandbox.Store().Load()).Error);
    }

    [Fact]
    public void CorruptExistingFileIsNeverSilentlyOverwritten()
    {
        using var sandbox = new Sandbox();
        sandbox.Store().Save(Migrated());
        sandbox.Execute("DELETE FROM progression_school_state;");
        var bytes = File.ReadAllBytes(sandbox.V14);
        Invalid(() => sandbox.Store().Save(Migrated()));
        Assert.Equal(bytes, File.ReadAllBytes(sandbox.V14));
        Assert.Empty(Directory.GetFiles(sandbox.DirectoryName, "*.backup*.sqlite3"));
    }
}
