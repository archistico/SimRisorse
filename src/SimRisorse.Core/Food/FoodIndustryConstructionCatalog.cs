using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Food;

/// <summary>
/// RC50 / F1-D05d.3a: direct-build catalogue for the six food-industry sites.
/// Prices, storage and prerequisites are provisional balancing inputs, not E4b parameters.
/// The historically named food-store belongs to the v14 rules and is NOT a second
/// v15 depot: only food-depot appears here.
/// </summary>
public sealed record FoodIndustrySiteDefinition(string Id, string Name,
    ResourceBundle BaseCost, ImmutableArray<string> Prerequisites,
    FoodIndustryCapacities StoragePerBuilding);

public sealed class FoodIndustryConstructionCatalog
{
    private readonly ImmutableDictionary<string, FoodIndustrySiteDefinition> _byId;

    public FoodIndustryConstructionCatalog(IEnumerable<FoodIndustrySiteDefinition> sites,
        decimal costGrowthFactor)
    {
        ArgumentNullException.ThrowIfNull(sites);
        if (costGrowthFactor < 1m)
            throw new ArgumentOutOfRangeException(nameof(costGrowthFactor));
        var items = sites.ToImmutableArray();
        if (items.Length != FoodIndustrySites.Ids.Length || items.Any(site => site is null) ||
            !items.Select(site => site.Id).ToHashSet(StringComparer.Ordinal)
                .SetEquals(FoodIndustrySites.Ids))
            throw new ArgumentException("Exactly six canonical food sites are required.", nameof(sites));
        if (items.Any(site => string.IsNullOrWhiteSpace(site.Name) ||
            site.BaseCost is null || site.BaseCost.Amounts.IsEmpty ||
            site.StoragePerBuilding is null || site.StoragePerBuilding.Animals is null ||
            site.StoragePerBuilding.Transit is null || site.StoragePerBuilding.DepotRaw is null ||
            site.Prerequisites.IsDefault || site.Prerequisites.Any(string.IsNullOrWhiteSpace) ||
            site.Prerequisites.Distinct(StringComparer.Ordinal).Count() != site.Prerequisites.Length ||
            site.Prerequisites.Contains(site.Id)))
            throw new ArgumentException("Invalid food-site cost, storage or prerequisites.", nameof(sites));
        _byId = items.ToImmutableDictionary(site => site.Id, StringComparer.Ordinal);
        CostGrowthFactor = costGrowthFactor;
        Sites = [.. FoodIndustrySites.Ids.Select(id => _byId[id])];
    }

    public decimal CostGrowthFactor { get; }
    public ImmutableArray<FoodIndustrySiteDefinition> Sites { get; }

    public bool TryGet(string? id, out FoodIndustrySiteDefinition? site) =>
        _byId.TryGetValue(id ?? string.Empty, out site);

    public void ValidateAgainst(GameScenario baseline)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        foreach (var site in Sites)
        {
            baseline.Catalog.ValidateBundle(site.BaseCost);
            foreach (var required in site.Prerequisites)
                if (!_byId.ContainsKey(required) && required != "school" &&
                    !baseline.Catalog.BuildingsById.ContainsKey(required))
                    throw new ArgumentException($"Unknown food prerequisite: {required}.", nameof(baseline));
        }
        // Detect cycles even when a dependency is a valid canonical food site.
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var active = new HashSet<string>(StringComparer.Ordinal);
        void Visit(string id)
        {
            if (visited.Contains(id)) return;
            if (!active.Add(id))
                throw new ArgumentException("Circular food construction prerequisites.", nameof(baseline));
            foreach (var dependency in _byId[id].Prerequisites)
                if (_byId.ContainsKey(dependency)) Visit(dependency);
            active.Remove(id);
            visited.Add(id);
        }
        foreach (var site in Sites) Visit(site.Id);
    }

    /// <summary>All four food-storage compartments derive solely from constructed sites.
    /// Historical over-capacity stocks remain intact: this is a production limit only.</summary>
    public FoodIndustryCapacities CapacitiesFor(FoodIndustryBuildings buildings)
    {
        ArgumentNullException.ThrowIfNull(buildings);
        var animals = ResourceBundle.Empty;
        var transit = ResourceBundle.Empty;
        var raw = ResourceBundle.Empty;
        var ready = ResourceAmount.Zero;
        foreach (var site in Sites)
        {
            var count = FoodIndustrySites.Get(buildings.Counts, site.Id);
            var storage = site.StoragePerBuilding;
            animals += storage.Animals.Scale(count);
            transit += storage.Transit.Scale(count);
            raw += storage.DepotRaw.Scale(count);
            ready += storage.DepotReady * count;
        }
        return new(animals, transit, raw, ready);
    }

    public ResourceBundle Quote(FoodIndustryBuildings buildings, string siteId, int quantity)
    {
        ArgumentNullException.ThrowIfNull(buildings);
        if (!_byId.TryGetValue(siteId, out var definition))
            throw new ArgumentException("Unknown food building.", nameof(siteId));
        var acquired = checked((int)FoodIndustrySites.Get(buildings.Counts, siteId));
        return ProgressiveConstructionPricing.QuoteNewBuildings(definition.BaseCost,
            1m, CostGrowthFactor, acquired, quantity);
    }

    private static FoodIndustryCapacities Storage(ResourceBundle? animals = null,
        ResourceBundle? transit = null, ResourceBundle? raw = null, decimal ready = 0m) =>
        new(animals ?? ResourceBundle.Empty, transit ?? ResourceBundle.Empty,
            raw ?? ResourceBundle.Empty, ResourceAmount.FromUnits(ready));

    public static FoodIndustryConstructionCatalog Candidate { get; } = new(
    [
        new("stable", "Stalla", ResourceBundle.Create(("planks", 110m), ("stone", 65m)),
            ["farm", "well"], Storage(animals: ResourceBundle.Create(("cattle", 20m)))),
        new("hunter", "Cacciatore", ResourceBundle.Create(("planks", 70m), ("stone", 35m)),
            ["small-house"], Storage(animals: ResourceBundle.Create(("game", 20m)))),
        new("butcher", "Macellaio", ResourceBundle.Create(("planks", 120m), ("stone", 80m)),
            ["food-depot"], Storage(transit: ResourceBundle.Create(("meat", 20m)))),
        new("garden", "Orto", ResourceBundle.Create(("planks", 65m), ("stone", 40m)),
            ["well"], Storage(transit: ResourceBundle.Create(("vegetables", 20m)))),
        new("orchard", "Frutteto", ResourceBundle.Create(("planks", 85m), ("stone", 50m)),
            ["garden"], Storage(transit: ResourceBundle.Create(("fruit", 20m)))),
        new("food-depot", "Deposito alimentare", ResourceBundle.Create(("planks", 60m), ("stone", 30m)),
            ["fisher"], Storage(raw: ResourceBundle.Create(("fish", 50m), ("bread", 50m),
                ("meat", 50m), ("fruit", 50m), ("vegetables", 50m)), ready: 100m))
    ], costGrowthFactor: 1.15m);
}
