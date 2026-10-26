using SimRisorse.Application.ReadModels;
using SimRisorse.Core.Content;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application;

public sealed record CommandResult(GameError Error, GameView View)
{
    public bool Succeeded => Error == GameError.None;
}

/// <summary>Owns one session; serializes all mutations and coherent read projections.</summary>
public sealed class GameRuntime(GameScenario scenario)
{
    private readonly object _gate = new();
    private GameSession _session = GameSession.Create(scenario);
    private long _revision;
    internal SemaphoreSlim PersistenceGate { get; } = new(1, 1);

    internal CommandResult Execute(Func<GameSession, GameActionResult> action)
    {
        lock (_gate)
        {
            var result = action(_session);
            if (result.Succeeded) _revision++;
            return new(result.Error, GameViewProjector.Project(_session));
        }
    }

    internal GameView Start(GameScenario? newScenario)
    {
        lock (_gate)
        {
            _session = GameSession.Create(newScenario ?? _session.Scenario);
            _revision++;
            return GameViewProjector.Project(_session);
        }
    }

    internal GameView Read()
    {
        lock (_gate) return GameViewProjector.Project(_session);
    }

    internal GameSnapshot CaptureSnapshot()
    {
        lock (_gate) return _session.Snapshot();
    }

    internal long BeginLoad()
    {
        lock (_gate)
        {
            _session.Pause();
            return ++_revision;
        }
    }

    internal Persistence.GameSaveError TryRestore(GameSnapshot snapshot, long revision)
    {
        lock (_gate)
        {
            if (_revision != revision) return Persistence.GameSaveError.Superseded;
            if (!GameSession.SupportsSnapshotFormat(snapshot.FormatVersion)) return Persistence.GameSaveError.UnsupportedFormat;
            if (snapshot.ScenarioId != _session.Scenario.Id || snapshot.RulesVersion != _session.Scenario.Catalog.RulesVersion)
                return Persistence.GameSaveError.IncompatibleRules;
            try
            {
                var replacement = GameSession.Restore(_session.Scenario, snapshot);
                _session = replacement;
                _revision++;
                return Persistence.GameSaveError.None;
            }
            catch (ArgumentException) { return Persistence.GameSaveError.InvalidData; }
        }
    }
}
