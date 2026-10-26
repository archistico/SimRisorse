using System.Text.Json;
using Microsoft.Data.Sqlite;
using SimRisorse.StackProbe.Model;
using Xunit;

namespace SimRisorse.StackProbe.Tests;

public sealed class PersistenceTests
{
    [Fact]
    public void SQLiteRoundTripPreservesTimeResourcesAndAssignments()
    {
        using var database = new TemporaryDatabase();
        var session = ProbeSession.Create();
        session.Construct(BuildingKind.TreeFeller);
        session.AssignWorker(BuildingKind.TreeFeller);
        session.Resume();
        session.Advance(TimeSpan.FromMilliseconds(1750));
        var expected = session.Snapshot();
        database.Store.Save(expected);
        var loaded = database.Store.Load();
        Assert.Equal(JsonSerializer.Serialize(expected), JsonSerializer.Serialize(loaded));
        var restored = ProbeSession.Restore(loaded);
        restored.Advance(TimeSpan.FromDays(30));
        Assert.Equal(expected.ElapsedTicks, restored.ElapsedTicks);
        Assert.True(restored.IsPaused);
    }

    [Fact]
    public void FailedSecondWriteRollsBackBothPayloadAndMetadata()
    {
        using var database = new TemporaryDatabase();
        var session = ProbeSession.Create();
        var before = session.Snapshot();
        database.Store.Save(before);
        database.Execute("""
            CREATE TRIGGER reject_payload BEFORE UPDATE ON save_payload
            BEGIN SELECT RAISE(ABORT, 'injected write failure'); END;
            """);
        session.Resume();
        session.Advance(TimeSpan.FromSeconds(10));
        session.Construct(BuildingKind.Sawmill);
        var error = Assert.Throws<IOException>(() => database.Store.Save(session.Snapshot()));
        Assert.IsType<SqliteException>(error.InnerException);
        Assert.Equal(JsonSerializer.Serialize(before), JsonSerializer.Serialize(database.Store.Load()));
    }

    [Fact]
    public void InconsistentMetadataIsRejected()
    {
        using var database = new TemporaryDatabase();
        database.Store.Save(ProbeSession.Create().Snapshot());
        database.Execute("UPDATE save_metadata SET elapsed_ticks=1 WHERE slot=1;");
        Assert.Throws<InvalidDataException>(() => database.Store.Load());
    }

    [Fact]
    public void InvalidSnapshotCannotOverwriteThePreviousSave()
    {
        using var database = new TemporaryDatabase();
        var before = ProbeSession.Create().Snapshot();
        database.Store.Save(before);
        Assert.Throws<InvalidDataException>(() => database.Store.Save(before with { RulesVersion = "unknown" }));
        Assert.Equal(JsonSerializer.Serialize(before), JsonSerializer.Serialize(database.Store.Load()));
    }

    [Fact]
    public void LoadingAMissingSaveDoesNotCreateAnEmptyDatabase()
    {
        using var database = new TemporaryDatabase();
        Assert.Throws<FileNotFoundException>(() => database.Store.Load());
        Assert.False(File.Exists(database.PathName));
    }
}
