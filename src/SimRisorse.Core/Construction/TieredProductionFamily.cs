using System.Collections.Immutable;
using SimRisorse.Core.Content;

namespace SimRisorse.Core.Construction;

/// <summary>Three proposed productive-building sizes; none is activated in civil-growth-13.</summary>
public enum ProductionTier { Base = 0, Large = 1, Advanced = 2 }

public sealed record ProductionTierSpecification(ProductionTier Tier, int Workplaces,
    decimal CostMultiplier, ImmutableArray<string> RequiredTechnologies);

/// <summary>
/// Adapts a single-worker productive catalog entry into three tier specifications.
/// Recipe, priority, inputs, prerequisites and output resources stay those of the source entry.
/// Cost multipliers are supplied by the candidate ruleset, not hard-coded into the live campaign.
/// </summary>
public sealed class TieredProductionFamily
{
    private readonly ImmutableArray<ProductionTierSpecification> _tiers;

    public TieredProductionFamily(BuildingDefinition source, string largeTechnologyId, string advancedTechnologyId,
        decimal largeCostMultiplier, decimal advancedCostMultiplier)
    {
        ArgumentNullException.ThrowIfNull(source);
        ContentIdentifier.Validate(largeTechnologyId, nameof(largeTechnologyId));
        ContentIdentifier.Validate(advancedTechnologyId, nameof(advancedTechnologyId));
        if (largeTechnologyId == advancedTechnologyId)
            throw new ArgumentException("The two tier technologies must be different.");
        if (source.Recipe is null || source.WorkplacesPerBuilding != 1 || source.HousingPerBuilding != 0 || source.IsScenarioFixed)
            throw new ArgumentException("Only single-worker, non-fixed productive buildings can be tiered.", nameof(source));
        if (largeCostMultiplier <= 1m || advancedCostMultiplier <= largeCostMultiplier)
            throw new ArgumentOutOfRangeException(nameof(advancedCostMultiplier), "The tier prices must rise strictly.");
        Source = source;
        _tiers =
        [
            new(ProductionTier.Base, 1, 1m, ImmutableArray<string>.Empty),
            new(ProductionTier.Large, 3, largeCostMultiplier, [largeTechnologyId]),
            new(ProductionTier.Advanced, 10, advancedCostMultiplier, [largeTechnologyId, advancedTechnologyId])
        ];
    }

    public BuildingDefinition Source { get; }
    public string FamilyId => Source.Id;
    public ImmutableArray<ProductionTierSpecification> Tiers => _tiers;

    public ProductionTierSpecification Get(ProductionTier tier) => tier switch
    {
        ProductionTier.Base => _tiers[0],
        ProductionTier.Large => _tiers[1],
        ProductionTier.Advanced => _tiers[2],
        _ => throw new ArgumentOutOfRangeException(nameof(tier))
    };

    public bool IsUnlocked(ProductionTier tier, IReadOnlySet<string> unlockedTechnologies)
    {
        ArgumentNullException.ThrowIfNull(unlockedTechnologies);
        return Get(tier).RequiredTechnologies.All(unlockedTechnologies.Contains);
    }
}

/// <summary>Research coverage of the 16 productive families in the modern civilian campaign.</summary>
public static class TieredProductionCatalog
{
    private static readonly ImmutableSortedDictionary<string, (string Large, string Advanced)> Paths =
        new Dictionary<string, (string Large, string Advanced)>(StringComparer.Ordinal)
        {
            ["tree-feller"] = ("lavorazioni-1", "lavorazioni-2"),
            ["sawmill"] = ("lavorazioni-1", "lavorazioni-2"),
            ["stonecutter"] = ("lavorazioni-1", "lavorazioni-2"),
            ["fisher"] = ("alimentazione-1", "alimentazione-2"),
            ["food-store"] = ("alimentazione-1", "alimentazione-2"),
            ["farm"] = ("alimentazione-1", "alimentazione-2"),
            ["well"] = ("alimentazione-1", "alimentazione-2"),
            ["mill"] = ("alimentazione-1", "alimentazione-2"),
            ["bakery"] = ("alimentazione-1", "alimentazione-2"),
            ["iron-mine"] = ("estrazione-1", "estrazione-2"),
            ["coal-mine"] = ("estrazione-1", "estrazione-2"),
            ["gold-mine"] = ("estrazione-1", "estrazione-2"),
            ["iron-smelter"] = ("industria-1", "industria-2"),
            ["gold-smelter"] = ("industria-1", "industria-2"),
            ["forge"] = ("industria-1", "industria-2"),
            ["mint"] = ("industria-1", "industria-2")
        }.ToImmutableSortedDictionary(StringComparer.Ordinal);

    public static ImmutableArray<string> KnownFamilyIds => [.. Paths.Keys];

    /// <summary>Adapts the productive entries that actually exist in a given catalog, excluding historical absences.</summary>
    public static ImmutableArray<TieredProductionFamily> FromCatalog(GameCatalog catalog,
        decimal largeCostMultiplier, decimal advancedCostMultiplier)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var result = ImmutableArray.CreateBuilder<TieredProductionFamily>();
        foreach (var (id, path) in Paths)
            if (catalog.BuildingsById.TryGetValue(id, out var definition))
                result.Add(new(definition, path.Large, path.Advanced, largeCostMultiplier, advancedCostMultiplier));
        return result.ToImmutable();
    }
}
