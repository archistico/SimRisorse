using System.Collections.Immutable;
using SimRisorse.Core.Content;

namespace SimRisorse.Core.Research;

/// <summary>A research purchase specification, separate from live v13 content.</summary>
public sealed class TechnologyDefinition
{
    public TechnologyDefinition(string id, string name, ResearchPoints cost, bool available,
        IEnumerable<string> prerequisites, IEnumerable<string> requiredBuildings,
        IEnumerable<string> unlockedFamilies)
    {
        ContentIdentifier.Validate(id, nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A technology needs a name.", nameof(name));
        if (cost.MilliPoints == 0) throw new ArgumentOutOfRangeException(nameof(cost));
        Id = id;
        Name = name;
        Cost = cost;
        Available = available;
        Prerequisites = Materialize(prerequisites, nameof(prerequisites));
        RequiredBuildings = Materialize(requiredBuildings, nameof(requiredBuildings));
        UnlockedFamilies = Materialize(unlockedFamilies, nameof(unlockedFamilies));
    }

    public string Id { get; }
    public string Name { get; }
    public ResearchPoints Cost { get; }
    public bool Available { get; }
    public ImmutableArray<string> Prerequisites { get; }
    public ImmutableArray<string> RequiredBuildings { get; }
    public ImmutableArray<string> UnlockedFamilies { get; }

    private static ImmutableArray<string> Materialize(IEnumerable<string> ids, string argument)
    {
        ArgumentNullException.ThrowIfNull(ids);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = ImmutableArray.CreateBuilder<string>();
        foreach (var id in ids)
        {
            ContentIdentifier.Validate(id, argument);
            if (!seen.Add(id)) throw new ArgumentException("Duplicate identifier.", argument);
            result.Add(id);
        }
        return result.ToImmutable();
    }
}

/// <summary>
/// Validated DAG; unavailable technologies are visible as proposals but NEVER purchasable.
/// The 8 productive technologies and 3 deferred entries are numerical CANDIDATES.
/// </summary>
public sealed class TechnologyCatalog
{
    private readonly ImmutableSortedDictionary<string, TechnologyDefinition> _byId;

    public TechnologyCatalog(IEnumerable<TechnologyDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        var builder = ImmutableSortedDictionary.CreateBuilder<string, TechnologyDefinition>(StringComparer.Ordinal);
        foreach (var tech in definitions)
        {
            ArgumentNullException.ThrowIfNull(tech);
            if (builder.ContainsKey(tech.Id))
                throw new ArgumentException($"Duplicate technology: {tech.Id}.", nameof(definitions));
            builder.Add(tech.Id, tech);
        }
        _byId = builder.ToImmutable();
        var marks = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var id in _byId.Keys) Visit(id);

        void Visit(string id)
        {
            if (marks.TryGetValue(id, out var mark))
            {
                if (mark == 1) throw new ArgumentException("Technology prerequisite cycle.", nameof(definitions));
                if (mark == 2) return;
            }
            marks[id] = 1;
            foreach (var required in _byId[id].Prerequisites)
            {
                if (!_byId.TryGetValue(required, out var prerequisite))
                    throw new ArgumentException($"Unknown prerequisite {required} for {id}.", nameof(definitions));
                if (_byId[id].Available && !prerequisite.Available)
                    throw new ArgumentException($"Purchasable technology {id} depends on unavailable {required}.", nameof(definitions));
                Visit(required);
            }
            marks[id] = 2;
        }
    }

    public ImmutableArray<TechnologyDefinition> Technologies => [.. _byId.Values];
    public bool TryGet(string id, out TechnologyDefinition? technology)
    {
        ContentIdentifier.Validate(id, nameof(id));
        return _byId.TryGetValue(id, out technology);
    }

    private static TechnologyDefinition Tech(string id, string name, decimal price, bool available,
        string[] prerequisites, string[] families, string[]? buildings = null) =>
        new(id, name, ResearchPoints.FromUnits(price), available, prerequisites, buildings ?? [], families);

    public static TechnologyCatalog ProposedTierTree { get; } = new(
    [
        Tech("lavorazioni-1", "Lavorazioni I", 100m, true, [], ["tree-feller", "sawmill", "stonecutter"]),
        Tech("lavorazioni-2", "Lavorazioni II", 350m, true, ["lavorazioni-1"], ["tree-feller", "sawmill", "stonecutter"]),
        Tech("alimentazione-1", "Alimentazione I", 100m, true, [], ["fisher", "food-store", "farm", "well", "mill", "bakery"]),
        Tech("alimentazione-2", "Alimentazione II", 350m, true, ["alimentazione-1"], ["fisher", "food-store", "farm", "well", "mill", "bakery"]),
        Tech("estrazione-1", "Estrazione I", 150m, true, [], ["iron-mine", "coal-mine", "gold-mine"]),
        Tech("estrazione-2", "Estrazione II", 400m, true, ["estrazione-1"], ["iron-mine", "coal-mine", "gold-mine"]),
        Tech("industria-1", "Industria I", 200m, true, ["estrazione-1"], ["iron-smelter", "gold-smelter", "forge", "mint"]),
        Tech("industria-2", "Industria II", 450m, true, ["industria-1"], ["iron-smelter", "gold-smelter", "forge", "mint"]),
        Tech("logistica-1", "Logistica I", 120m, false, [], []),
        Tech("logistica-2", "Logistica II", 360m, false, ["logistica-1"], []),
        Tech("navigazione-1", "Navigazione", 250m, false, ["lavorazioni-1"], [], ["mint"])
    ]);
}
