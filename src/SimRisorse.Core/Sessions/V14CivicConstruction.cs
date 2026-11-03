using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Sessions;

/// <summary>
/// Separate v14 outcome for non-productive catalog structures. Fixed Municipio,
/// productive families and the future School have their own explicit commands.
/// No action mutates the state on failure.
/// </summary>
public enum V14CivicConstructionFailure
{
    None,
    InvalidQuantity,
    UnknownBuilding,
    FixedBuilding,
    TieredBuilding,
    MissingPrerequisite,
    InsufficientResources,
    NumericLimit
}

public sealed record V14CivicConstructionResult(V14CivicConstructionFailure Failure, ResourceBundle Cost)
{
    public bool Succeeded => Failure == V14CivicConstructionFailure.None;
}
