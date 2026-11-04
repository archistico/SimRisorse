using System.Collections.Immutable;
using Microsoft.Data.Sqlite;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class V14SchoolPencilPersistenceTests
{
    private static readonly SchoolResearchRules Rules = new("food", ResourceAmount.FromUnits(10m),
        120, 1, 2, 100, pencilsPerStudent: 2);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;

    [Fact]
    public void CraftEnrollSaveAndRestorePreservePencilInventoryAndRefundRights()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-pencils-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var scenario = CivilGrowthScenario.Create();
            var first = V14CampaignSession.FromV13(scenario, GameSession.Create(scenario).Snapshot(),
                Rules, Technologies, V14SchoolConstructionRules.Candidate).Snapshot();
            var seeded = first with
            {
                SchoolCount = 1,
                PurchaseCounters = first.PurchaseCounters.Select(item => item.FamilyId == "school"
                    ? item with { Acquired = 1 } : item).ToImmutableArray(),
                Resources = first.Resources + ResourceBundle.Create(("coal", 10m))
            };
            var session = V14CampaignSession.Restore(scenario, seeded,
                Rules, Technologies, V14SchoolConstructionRules.Candidate);
            Assert.Equal(PencilCraftFailure.None, session.CraftPencils(1));
            Assert.Equal(SchoolActionFailure.None, session.EnrollStudents(1));
            var store = new V14SqliteSaveStore(Path.Combine(folder, "v14.sqlite3"), scenario, Rules, Technologies);
            store.Save(session.Snapshot());
            var restored = store.Load();
            Assert.Equal(session.Snapshot().Resources, restored.Resources);
            Assert.Equal(1, restored.PencilPaidStudents);
            var recovered = V14CampaignSession.Restore(scenario, restored,
                Rules, Technologies, V14SchoolConstructionRules.Candidate);
            Assert.Equal(SchoolActionFailure.None, recovered.CancelStudents(1));
            Assert.Equal(4m, recovered.Snapshot().Resources["pencils"].Units);
            Assert.Equal(0, recovered.Snapshot().PencilPaidStudents);
            store.Save(recovered.Snapshot());
            Assert.Equal(recovered.Snapshot().Resources, store.Load().Resources);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public void EarlierV14SqliteWithoutPencilColumnLoadsAndNeverInventsRefundablePencils()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-legacy-pencil-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var scenario = CivilGrowthScenario.Create();
            var store = new V14SqliteSaveStore(Path.Combine(folder, "v14.sqlite3"), scenario, Rules, Technologies);
            var first = V14CampaignSession.FromV13(scenario, GameSession.Create(scenario).Snapshot(),
                Rules, Technologies, V14SchoolConstructionRules.Candidate).Snapshot();
            store.Save(first);
            using (var database = new SqliteConnection(new SqliteConnectionStringBuilder
                   { DataSource = store.DatabasePath, Pooling = false }.ToString()))
            {
                database.Open();
                using var command = database.CreateCommand();
                command.CommandText = "ALTER TABLE progression_school_state DROP COLUMN pencil_paid_students;";
                command.ExecuteNonQuery();
            }
            var loaded = store.Load();
            Assert.Equal(0, loaded.PencilPaidStudents);
            Assert.Equal(first.Resources, loaded.Resources);
            store.Save(loaded); // Safe copy-on-write replacement upgrades the experimental layout.
            Assert.Equal(0, store.Load().PencilPaidStudents);
            Assert.True(File.Exists(Path.Combine(folder, "v14.v14.backup.sqlite3")));
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
    [Fact]
    public void PrePencilStudentInOldSqliteCanBeCancelledWithoutInventingPencils()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-p1-d50-legacy-student-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var scenario = CivilGrowthScenario.Create();
            var store = new V14SqliteSaveStore(Path.Combine(folder, "v14.sqlite3"), scenario, Rules, Technologies);
            var baseline = V14CampaignSession.FromV13(scenario, GameSession.Create(scenario).Snapshot(),
                Rules, Technologies, V14SchoolConstructionRules.Candidate).Snapshot();
            var oldRules = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m),
                120, 1, 2, 100); // Before pencils were part of enrollment.
            var enrollment = SchoolResearch.Enroll(oldRules, baseline.SchoolResearch,
                baseline.Resources, baseline.Population, 0, 0, 1, 1);
            Assert.True(enrollment.Succeeded);
            var legacy = baseline with
            {
                SchoolCount = 1,
                PurchaseCounters = baseline.PurchaseCounters.Select(item => item.FamilyId == "school"
                    ? item with { Acquired = 1 } : item).ToImmutableArray(),
                Resources = enrollment.Resources,
                SchoolResearch = enrollment.State,
                PencilPaidStudents = 0
            };
            store.Save(legacy);
            // Simulate the actual old SQLite layout, not just a deserialized default value.
            using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder
                   { DataSource = store.DatabasePath, Pooling = false }.ToString()))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "ALTER TABLE progression_school_state DROP COLUMN pencil_paid_students;";
                command.ExecuteNonQuery();
            }
            var loaded = store.Load();
            Assert.Equal(0, loaded.PencilPaidStudents);
            Assert.Equal(1, loaded.SchoolResearch.TrainingStudents);
            var session = V14CampaignSession.Restore(scenario, loaded,
                Rules, Technologies, V14SchoolConstructionRules.Candidate);
            Assert.Equal(SchoolActionFailure.None, session.CancelStudents(1));
            Assert.Equal(0, session.Snapshot().SchoolResearch.TrainingStudents);
            Assert.Equal(0m, session.Snapshot().Resources["pencils"].Units);
            Assert.Equal(baseline.Resources["food"], session.Snapshot().Resources["food"]);
            store.Save(session.Snapshot());
            Assert.Equal(0, store.Load().PencilPaidStudents);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public void InvalidPencilRefundLedgerCannotOverwriteExistingCheckpoint()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-p1-d50-invalid-ledger-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var scenario = CivilGrowthScenario.Create();
            var store = new V14SqliteSaveStore(Path.Combine(folder, "v14.sqlite3"), scenario, Rules, Technologies);
            var valid = V14CampaignSession.FromV13(scenario, GameSession.Create(scenario).Snapshot(),
                Rules, Technologies, V14SchoolConstructionRules.Candidate).Snapshot();
            store.Save(valid);
            var before = File.ReadAllBytes(store.DatabasePath);
            // A paid student with no waiting/training students is impossible.
            var invalid = valid with { PencilPaidStudents = 1 };
            Assert.Equal(GameSaveError.InvalidData,
                Assert.Throws<GameSaveException>(() => store.Save(invalid)).Error);
            Assert.Equal(before, File.ReadAllBytes(store.DatabasePath));
            var restored = store.Load();
            Assert.Equal(valid.Resources, restored.Resources);
            Assert.Equal(0, restored.PencilPaidStudents);
            Assert.Equal(valid.SchoolResearch.WaitingStudents, restored.SchoolResearch.WaitingStudents);
            Assert.Equal(valid.SchoolResearch.TrainingStudents, restored.SchoolResearch.TrainingStudents);
            Assert.Equal(valid.SchoolResearch.QualifiedResearchers, restored.SchoolResearch.QualifiedResearchers);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

}
