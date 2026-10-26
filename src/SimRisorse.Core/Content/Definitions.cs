using System.Collections.Immutable;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Content;

public sealed record ResourceDefinition(string Id, string Name);
public sealed record ProductionRecipe(ResourceBundle Input, ResourceBundle Output);
public sealed record BuildingDefinition(string Id, string Name, ResourceBundle Cost, int WorkplacesPerBuilding,
    ProductionRecipe? Recipe, int AllocationPriority, ImmutableArray<string> Prerequisites, int HousingPerBuilding = 0);
