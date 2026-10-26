using System.Text.Json;
using Microsoft.Data.Sqlite;
using SimRisorse.StackProbe.Model;

namespace SimRisorse.StackProbe.Persistence;

public sealed class SqliteProbeSnapshotStore(string databasePath) : IProbeSnapshotStore
{
    public string DatabasePath { get; } = Path.GetFullPath(databasePath);

    private SqliteConnection Open(SqliteOpenMode mode)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath, Mode = mode, Pooling = false, ForeignKeys = true, DefaultTimeout = 2
        }.ToString());
        try
        {
            connection.Open();
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    public void Save(ProbeSnapshot snapshot)
    {
        try { SaveCore(snapshot); }
        catch (SqliteException exception) { throw new IOException("Impossibile scrivere il salvataggio.", exception); }
    }

    private void SaveCore(ProbeSnapshot snapshot)
    {
        ProbeSession.Validate(snapshot);
        var payload = JsonSerializer.Serialize(snapshot);
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        using var connection = Open(SqliteOpenMode.ReadWriteCreate);
        using var transaction = connection.BeginTransaction();
        using var schema = connection.CreateCommand();
        schema.Transaction = transaction;
        schema.CommandText = """
            CREATE TABLE IF NOT EXISTS save_metadata (
                slot INTEGER PRIMARY KEY CHECK (slot = 1),
                format_version INTEGER NOT NULL,
                scenario_id TEXT NOT NULL,
                rules_version TEXT NOT NULL,
                elapsed_ticks INTEGER NOT NULL CHECK (elapsed_ticks >= 0)
            );
            CREATE TABLE IF NOT EXISTS save_payload (
                slot INTEGER PRIMARY KEY REFERENCES save_metadata(slot),
                payload TEXT NOT NULL
            );
            """;
        schema.ExecuteNonQuery();
        using var metadata = connection.CreateCommand();
        metadata.Transaction = transaction;
        metadata.CommandText = """
            INSERT INTO save_metadata VALUES (1, $format, $scenario, $rules, $time)
            ON CONFLICT(slot) DO UPDATE SET format_version=excluded.format_version,
                scenario_id=excluded.scenario_id, rules_version=excluded.rules_version, elapsed_ticks=excluded.elapsed_ticks;
            """;
        metadata.Parameters.AddWithValue("$format", snapshot.FormatVersion);
        metadata.Parameters.AddWithValue("$scenario", snapshot.ScenarioId);
        metadata.Parameters.AddWithValue("$rules", snapshot.RulesVersion);
        metadata.Parameters.AddWithValue("$time", snapshot.ElapsedTicks);
        metadata.ExecuteNonQuery();
        using var state = connection.CreateCommand();
        state.Transaction = transaction;
        state.CommandText = """
            INSERT INTO save_payload VALUES (1, $payload)
            ON CONFLICT(slot) DO UPDATE SET payload=excluded.payload;
            """;
        state.Parameters.AddWithValue("$payload", payload);
        state.ExecuteNonQuery();
        transaction.Commit();
    }

    public ProbeSnapshot Load()
    {
        try { return LoadCore(); }
        catch (SqliteException exception) { throw new IOException("Impossibile leggere il salvataggio.", exception); }
    }

    private ProbeSnapshot LoadCore()
    {
        if (!File.Exists(DatabasePath)) throw new FileNotFoundException("Nessun salvataggio disponibile.", DatabasePath);
        using var connection = Open(SqliteOpenMode.ReadOnly);
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT m.format_version, m.scenario_id, m.rules_version, m.elapsed_ticks, p.payload
            FROM save_metadata m JOIN save_payload p ON p.slot=m.slot WHERE m.slot=1;
            """;
        using var reader = command.ExecuteReader();
        if (!reader.Read()) throw new InvalidDataException("Il salvataggio è incompleto.");
        ProbeSnapshot snapshot;
        try
        {
            snapshot = JsonSerializer.Deserialize<ProbeSnapshot>(reader.GetString(4))
                ?? throw new InvalidDataException("Stato della partita assente.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Stato della partita non leggibile.", exception);
        }
        ProbeSession.Validate(snapshot);
        if (reader.GetInt32(0) != snapshot.FormatVersion || reader.GetString(1) != snapshot.ScenarioId ||
            reader.GetString(2) != snapshot.RulesVersion || reader.GetInt64(3) != snapshot.ElapsedTicks)
            throw new InvalidDataException("Metadati e stato della partita non corrispondono.");
        return snapshot;
    }
}
