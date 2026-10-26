using Microsoft.Data.Sqlite;
using SimRisorse.StackProbe.Persistence;

namespace SimRisorse.StackProbe.Tests;

public sealed class TemporaryDatabase : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "SimRisorse.ProbeTests", Guid.NewGuid().ToString("N"));
    public string PathName => Path.Combine(_directory, "probe.sqlite3");
    public SqliteProbeSnapshotStore Store => new(PathName);

    public void Execute(string sql)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = PathName, Pooling = false }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }
}
