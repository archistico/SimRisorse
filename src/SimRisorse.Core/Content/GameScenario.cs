using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;

namespace SimRisorse.Core.Content;

public sealed class GameScenario
{
    public GameScenario(string id, GameCatalog catalog, int population, ResourceBundle capacity,
        ResourceBundle initialResources, IEnumerable<BuildingState> initialBuildings, PopulationRules? populationRules = null)
    {
        ContentIdentifier.Validate(id, nameof(id));
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentOutOfRangeException.ThrowIfNegative(population);
        ArgumentNullException.ThrowIfNull(initialBuildings);
        catalog.ValidateBundle(capacity);
        catalog.ValidateBundle(initialResources);
        var initial = initialBuildings.ToImmutableArray();
        ValidateBuildings(catalog, initial, population);
        Id = id;
        Catalog = catalog;
        Population = population;
        PopulationRules = populationRules;
        if (populationRules is not null && !catalog.ResourcesById.ContainsKey(populationRules.FoodResourceId))
            throw new ArgumentException("Population food must be a catalog resource.", nameof(populationRules));
        if (populationRules?.AdvancedHousing is { } advanced &&
            (!catalog.ResourcesById.ContainsKey(advanced.BreadResourceId) ||
             advanced.BreadResourceId == populationRules?.FoodResourceId ||
             !catalog.BuildingsById.TryGetValue(advanced.BasicHousingBuildingId, out var basicHousing) ||
             basicHousing.HousingPerBuilding == 0))
            throw new ArgumentException("Advanced housing requires a different known food resource and a basic housing building.", nameof(populationRules));
        if (populationRules?.HousingGrowth is { } growth &&
            (!catalog.BuildingsById.TryGetValue(growth.SmallHouseId, out var small) || small.HousingPerBuilding == 0 ||
             !catalog.BuildingsById.TryGetValue(growth.LargeHouseId, out var large) || large.HousingPerBuilding == 0))
            throw new ArgumentException("Accelerated growth requires two known housing building types.", nameof(populationRules));
        Capacity = capacity;
        InitialResources = initialResources;
        var byId = initial.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        InitialBuildings = catalog.Buildings.Select(item => byId.GetValueOrDefault(item.Id) ?? new(item.Id, 0, 0)).ToImmutableArray();
        if (!StorageCapacity(InitialBuildings).Covers(initialResources)) throw new ArgumentException("Initial stocks exceed storage capacity.");
        if (populationRules is not null && HousingCapacity(InitialBuildings) < population)
            throw new ArgumentException("Initial population exceeds housing.", nameof(initialBuildings));
    }

    public string Id { get; }
    public GameCatalog Catalog { get; }
    public int Population { get; }
    public PopulationRules? PopulationRules { get; }
    /// <summary>Base storage before building effects; sessions use <see cref="StorageCapacity"/>.</summary>
    public ResourceBundle Capacity { get; }
    public ResourceBundle InitialResources { get; }
    public ImmutableArray<BuildingState> InitialBuildings { get; }

    internal int HousingCapacity(IEnumerable<BuildingState> buildings)
    {
        if (PopulationRules is null) return Population;
        long capacity = 0;
        foreach (var building in buildings)
        {
            capacity += (long)building.Count * Catalog.BuildingsById[building.BuildingId].HousingPerBuilding;
            if (capacity > int.MaxValue) throw new ArgumentException("Housing exceeds the population limit.");
        }
        return (int)capacity;
    }

    internal int BasicHousingCapacity(IEnumerable<BuildingState> buildings)
    {
        var basicId = PopulationRules?.AdvancedHousing?.BasicHousingBuildingId;
        if (basicId is null) return HousingCapacity(buildings);
        var count = buildings.First(item => item.BuildingId == basicId).Count;
        return checked(count * Catalog.BuildingsById[basicId].HousingPerBuilding);
    }

    /// <summary>Base capacity plus storage effects. Throws <see cref="OverflowException"/> beyond the numeric limit.</summary>
    internal ResourceBundle StorageCapacity(IEnumerable<BuildingState> buildings)
    {
        var capacity = Capacity;
        foreach (var building in buildings)
            if (building.Count > 0 && Catalog.BuildingsById[building.BuildingId].StoragePerBuilding is { } storage)
                capacity += storage.Scale(building.Count);
        return capacity;
    }

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
