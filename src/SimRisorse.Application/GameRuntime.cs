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

    /// <summary>Restores a checkpoint, converting it first when an explicit upgrade to the active rules is known.</summary>
    internal (Persistence.GameSaveError Error, string? UpgradedFromRules) TryRestore(GameSnapshot snapshot, long revision)
    {
        lock (_gate)
        {
            if (_revision != revision) return (Persistence.GameSaveError.Superseded, null);
            if (!GameSession.SupportsSnapshotFormat(snapshot.FormatVersion)) return (Persistence.GameSaveError.UnsupportedFormat, null);
            var rules = _session.Scenario.Catalog.RulesVersion;
            // Reject unknown or future rule identifiers before any migration attempt.
            // Only explicitly declared historical versions may be upgraded. The 03/11
            // individual-training fork retains the known v10 identifier and is still
            // distinguished by its stored military checkpoint in UpgradeOneStep.
            if (snapshot.ScenarioId != _session.Scenario.Id ||
                (snapshot.RulesVersion != rules &&
                 _session.Scenario.Id == Scenarios.CivilGrowthScenario.Id &&
                 !Scenarios.CivilGrowthScenario.Changes.Any(change => change.From == snapshot.RulesVersion)))
                return (Persistence.GameSaveError.IncompatibleRules, null);
            GameSnapshot upgraded;
            try { upgraded = snapshot.ScenarioId == _session.Scenario.Id ? Scenarios.ScenarioUpgrades.Apply(snapshot, rules) : snapshot; }
            catch (Exception exception) when (exception is ArgumentException or OverflowException or InvalidOperationException)
            { return (Persistence.GameSaveError.InvalidData, null); }
            if (upgraded.ScenarioId != _session.Scenario.Id || upgraded.RulesVersion != rules)
                return (Persistence.GameSaveError.IncompatibleRules, null);
            try
            {
                var replacement = GameSession.Restore(_session.Scenario, upgraded);
                _session = replacement;
                _revision++;
                return (Persistence.GameSaveError.None, ReferenceEquals(upgraded, snapshot) ? null : snapshot.RulesVersion);
            }
            catch (ArgumentException) { return (Persistence.GameSaveError.InvalidData, null); }
        }
    }
}
