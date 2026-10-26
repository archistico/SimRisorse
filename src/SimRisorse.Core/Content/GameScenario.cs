using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Content;

public sealed class GameScenario
{
    public GameScenario(string id, GameCatalog catalog, int population, ResourceBundle capacity,
        ResourceBundle initialResources, IEnumerable<BuildingState> initialBuildings)
    {
        ContentIdentifier.Validate(id, nameof(id));
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentOutOfRangeException.ThrowIfNegative(population);
        ArgumentNullException.ThrowIfNull(initialBuildings);
        catalog.ValidateBundle(capacity);
        catalog.ValidateBundle(initialResources);
        if (!capacity.Covers(initialResources)) throw new ArgumentException("Initial stocks exceed storage capacity.");
        var initial = initialBuildings.ToImmutableArray();
        ValidateBuildings(catalog, initial, population);
        Id = id;
        Catalog = catalog;
        Population = population;
        Capacity = capacity;
        InitialResources = initialResources;
        var byId = initial.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        InitialBuildings = catalog.Buildings.Select(item => byId.GetValueOrDefault(item.Id) ?? new(item.Id, 0, 0)).ToImmutableArray();
    }

    public string Id { get; }
    public GameCatalog Catalog { get; }
    public int Population { get; }
    public ResourceBundle Capacity { get; }
    public ResourceBundle InitialResources { get; }
    public ImmutableArray<BuildingState> InitialBuildings { get; }

    internal static void ValidateBuildings(GameCatalog catalog, ImmutableArray<BuildingState> buildings, int population)
    {
        if (buildings.IsDefault) throw new ArgumentException("Building state is missing.");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        long assigned = 0;
        foreach (var building in buildings)
        {
            if (building is null || building.BuildingId is null || !catalog.BuildingsById.TryGetValue(building.BuildingId, out var definition) ||
                !seen.Add(building.BuildingId) || building.Count < 0 || building.AssignedWorkers < 0 ||
                building.AssignedWorkers > (long)building.Count * definition.WorkplacesPerBuilding)
                throw new ArgumentException("Unknown, duplicate or invalid building state.");
            assigned += building.AssignedWorkers;
        }
        if (assigned > population) throw new ArgumentException("Assignments exceed population.");
    }
}
