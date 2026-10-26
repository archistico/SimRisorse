using SimRisorse.Application.ReadModels;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Persistence;

public enum GameSaveError { None, NotFound, UnsupportedFormat, IncompatibleRules, InvalidData, Unavailable, Superseded, Canceled }

public sealed class GameSaveException(GameSaveError error, string message, Exception? inner = null) : Exception(message, inner)
{
    public GameSaveError Error { get; } = error;
}

public interface IGameSaveStore
{
    void Save(GameSnapshot snapshot);
    GameSnapshot Load();
}

public sealed record PersistenceResult(GameSaveError Error, GameView View)
{
    public bool Succeeded => Error == GameSaveError.None;
}
