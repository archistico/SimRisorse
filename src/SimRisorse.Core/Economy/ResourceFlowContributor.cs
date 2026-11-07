namespace SimRisorse.Core.Economy;

/// <summary>
/// Read-only, gross contribution to the next simulated second. A source can be a
/// productive building, the food depot or the inhabitants' atomic meal. Transfers
/// between storage owners must not be represented as production or consumption.
/// </summary>
public sealed record ResourceFlowContributor(string Source, ResourceBundle Production,
    ResourceBundle Consumption);
