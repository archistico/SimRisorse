using SimRisorse.Application.ReadModels;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Persistence;

public enum GameSaveError { None, NotFound, UnsupportedFormat, IncompatibleRules, InvalidData, Unavailable, Superseded, Canceled }

public sealed class GameSaveException(GameSaveError error, string message, Exception? inner = null) : Exception(message, inner)
{
    public GameSaveError Error { get; } = error;
}

/// <summary>
/// Single checkpoint slot. Replacing a checkpoint written for another scenario or rules version must first preserve
/// the replaced checkpoint, so explicit conversions never lose the original file.
/// </summary>
public interface IGameSaveStore
{
    void Save(GameSnapshot snapshot);
    GameSnapshot Load();
}

/// <param name="UpgradedFromRules">Rules version of the original checkpoint when a load converted it.</param>
public sealed record PersistenceResult(GameSaveError Error, GameView View, string? UpgradedFromRules = null)
{
    public bool Succeeded => Error == GameSaveError.None;
}
