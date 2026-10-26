namespace SimRisorse.Core.Sessions;

public enum GameError
{
    None, InvalidQuantity, UnknownBuilding, MissingPrerequisite, InsufficientResources,
    InsufficientWorkplaces, InsufficientPopulation, InsufficientAssignedWorkers,
    NumericLimit, InvalidElapsedTime, AdvanceLimitExceeded
}

public readonly record struct GameActionResult(GameError Error)
{
    public bool Succeeded => Error == GameError.None;
    public static GameActionResult Success => new(GameError.None);
}
