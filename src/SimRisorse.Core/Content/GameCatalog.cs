using System.Collections.Frozen;
using System.Collections.Immutable;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Content;

public sealed class GameCatalog
{
    public GameCatalog(string rulesVersion, IEnumerable<ResourceDefinition> resources, IEnumerable<BuildingDefinition> buildings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rulesVersion);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(buildings);
        RulesVersion = rulesVersion;
        Resources = resources.ToImmutableArray();
        var definitions = buildings.ToImmutableArray();
        if (Resources.IsEmpty || definitions.IsEmpty) throw new ArgumentException("A catalog needs resources and buildings.");
        var resourceIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var resource in Resources)
        {
            ArgumentNullException.ThrowIfNull(resource);
            ContentIdentifier.Validate(resource.Id, nameof(resources));
            ArgumentException.ThrowIfNullOrWhiteSpace(resource.Name);
            if (!resourceIds.Add(resource.Id)) throw new ArgumentException($"Duplicate resource: {resource.Id}.", nameof(resources));
        }
        var buildingIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var building in definitions)
        {
            ArgumentNullException.ThrowIfNull(building);
            ContentIdentifier.Validate(building.Id, nameof(buildings));
            ArgumentException.ThrowIfNullOrWhiteSpace(building.Name);
            if (!buildingIds.Add(building.Id)) throw new ArgumentException($"Duplicate building: {building.Id}.", nameof(buildings));
            if (building.WorkplacesPerBuilding < 0 || building.HousingPerBuilding < 0 || building.Prerequisites.IsDefault)
                throw new ArgumentException($"Invalid workplace count or prerequisites: {building.Id}.", nameof(buildings));
            ValidateBundle(building.Cost, resourceIds);
            if (building.StoragePerBuilding is { } storage) ValidateBundle(storage, resourceIds);
            if (building.Recipe is { } recipe)
            {
                ValidateBundle(recipe.Input, resourceIds);
                ValidateBundle(recipe.Output, resourceIds);
                if (building.WorkplacesPerBuilding == 0 || recipe.Output.Amounts.IsEmpty)
                    throw new ArgumentException($"A productive building needs workplaces and output: {building.Id}.", nameof(buildings));
            }
        }
        Buildings = definitions.OrderBy(item => item.AllocationPriority).ThenBy(item => item.Id, StringComparer.Ordinal).ToImmutableArray();
        Resources = Resources.OrderBy(item => item.Id, StringComparer.Ordinal).ToImmutableArray();
        BuildingsById = Buildings.ToFrozenDictionary(item => item.Id, StringComparer.Ordinal);
        ResourcesById = Resources.ToFrozenDictionary(item => item.Id, StringComparer.Ordinal);
        ValidatePrerequisites();
    }

    public string RulesVersion { get; }
    public ImmutableArray<ResourceDefinition> Resources { get; }
    public ImmutableArray<BuildingDefinition> Buildings { get; }
    public FrozenDictionary<string, ResourceDefinition> ResourcesById { get; }
    public FrozenDictionary<string, BuildingDefinition> BuildingsById { get; }

    internal void ValidateBundle(ResourceBundle bundle) => ValidateBundle(bundle, ResourcesById.Keys);

    private static void ValidateBundle(ResourceBundle bundle, IEnumerable<string> known)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        var resources = known.ToHashSet(StringComparer.Ordinal);
        if (bundle.Amounts.Keys.Any(id => !resources.Contains(id))) throw new ArgumentException("A resource reference is missing from the catalog.");
    }

    private void ValidatePrerequisites()
    {
        var remaining = new Dictionary<string, int>(StringComparer.Ordinal);
        var dependents = Buildings.ToDictionary(item => item.Id, _ => new List<string>(), StringComparer.Ordinal);
        foreach (var building in Buildings)
        {
            var distinct = building.Prerequisites.Distinct(StringComparer.Ordinal).ToArray();
            if (distinct.Length != building.Prerequisites.Length || distinct.Any(id => id is null || !BuildingsById.ContainsKey(id)))
                throw new ArgumentException($"Invalid prerequisite reference: {building.Id}.");
            remaining.Add(building.Id, distinct.Length);
            foreach (var prerequisite in distinct) dependents[prerequisite].Add(building.Id);
        }
        var ready = new Queue<string>(remaining.Where(item => item.Value == 0).Select(item => item.Key));
        var visited = 0;
        while (ready.TryDequeue(out var id))
        {
            visited++;
            foreach (var next in dependents[id]) if (--remaining[next] == 0) ready.Enqueue(next);
        }
        if (visited != Buildings.Length) throw new ArgumentException("Building prerequisites contain a cycle.");
    }
}
