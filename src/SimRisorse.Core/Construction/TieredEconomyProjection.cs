using System.Collections.Immutable;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Construction;

/// <summary>A single hypothetical second computed using the EXISTING per-worker production planner.</summary>
public sealed record TieredEconomyStep(ResourceBundle StorageCapacity, ResourceBundle NextResources,
    ImmutableArray<BuildingProduction> Production);

public static class TieredEconomyProjection
{
    /// <summary>
    /// Adapter for E2 isolated tests. The unchanged buildings plus tiered families must cover
    /// every catalog entry exactly once. Housing, food consumption and population remain the
    /// responsibility of the future versioned GameSession (not this projection).
    /// </summary>
    public static TieredEconomyStep SimulateSecond(GameCatalog catalog, ResourceBundle resources,
        ResourceBundle baseStorage, IEnumerable<BuildingState> unchanged,
        IEnumerable<TieredBuildingPortfolio> tiered)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(baseStorage);
        ArgumentNullException.ThrowIfNull(unchanged);
        ArgumentNullException.ThrowIfNull(tiered);
        catalog.ValidateBundle(resources);
        catalog.ValidateBundle(baseStorage);
        var states = new Dictionary<string, BuildingState>(StringComparer.Ordinal);
        foreach (var building in unchanged)
        {
            ArgumentNullException.ThrowIfNull(building);
            if (!states.TryAdd(building.BuildingId, building))
                throw new ArgumentException("Duplicate legacy building state.", nameof(unchanged));
        }
        var tieredMap = new Dictionary<string, TieredBuildingPortfolio>(StringComparer.Ordinal);
        foreach (var family in tiered)
        {
            ArgumentNullException.ThrowIfNull(family);
            if (!tieredMap.TryAdd(family.FamilyId, family) || states.ContainsKey(family.FamilyId))
                throw new ArgumentException("Duplicate family or overlapping state.", nameof(tiered));
        }
        if (states.Count + tieredMap.Count != catalog.Buildings.Length ||
            states.Keys.Concat(tieredMap.Keys).Any(id => !catalog.BuildingsById.ContainsKey(id)))
            throw new ArgumentException("State must cover every catalog building exactly once.");

        var capacity = baseStorage;
        foreach (var definition in catalog.Buildings)
        {
            if (tieredMap.TryGetValue(definition.Id, out var family))
            {
                if (definition.Recipe is null || definition.WorkplacesPerBuilding != 1 || definition.IsScenarioFixed)
                    throw new ArgumentException("A tiered family is not a supported producer.", nameof(tiered));
                states.Add(definition.Id, new BuildingState(definition.Id, family.TotalBuildings, family.AssignedWorkers));
                if (definition.StoragePerBuilding is { } familyStorage)
                    capacity += familyStorage.Scale(family.StorageEquivalents);
            }
            else
            {
                var current = states[definition.Id];
                if (current.Count < 0 || current.AssignedWorkers < 0 ||
                    (long)current.AssignedWorkers > (long)current.Count * definition.WorkplacesPerBuilding)
                    throw new ArgumentException("Invalid legacy building state.", nameof(unchanged));
                if (definition.StoragePerBuilding is { } existingStorage)
                    capacity += existingStorage.Scale(current.Count);
            }
        }
        if (!capacity.Covers(resources))
            throw new ArgumentException("Initial resources exceed effective tiered storage.", nameof(resources));
        var result = ProductionPlanner.Plan(catalog, resources, capacity, states, includeDiagnostics: true);
        return new(capacity, result.Resources, result.Buildings);
    }
}
