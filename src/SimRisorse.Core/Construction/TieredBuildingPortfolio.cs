using SimRisorse.Core.Content;

namespace SimRisorse.Core.Construction;

/// <summary>Counts and assigned workers for ONE size. Validated when placed into a portfolio.</summary>
public readonly record struct TierOccupancy(int Buildings, int AssignedWorkers);

/// <summary>
/// Immutable, aggregated workforce state for a single productive family.
/// This is an isolated E2 model; v13 BuildingState remains unchanged.
/// </summary>
public sealed class TieredBuildingPortfolio
{
    private TieredBuildingPortfolio(string familyId, TierOccupancy basic,
        TierOccupancy large, TierOccupancy advanced)
    {
        ContentIdentifier.Validate(familyId, nameof(familyId));
        Validate(basic, 1);
        Validate(large, 3);
        Validate(advanced, 10);
        FamilyId = familyId;
        Base = basic;
        Large = large;
        Advanced = advanced;
        _ = TotalBuildings;
        _ = TotalWorkplaces;
        _ = AssignedWorkers;
    }

    public string FamilyId { get; }
    public TierOccupancy Base { get; }
    public TierOccupancy Large { get; }
    public TierOccupancy Advanced { get; }
    public int TotalBuildings => checked(checked(Base.Buildings + Large.Buildings) + Advanced.Buildings);
    public int TotalWorkplaces => checked(checked(Base.Buildings + checked(3 * Large.Buildings)) + checked(10 * Advanced.Buildings));
    public int AssignedWorkers => checked(checked(Base.AssignedWorkers + Large.AssignedWorkers) + Advanced.AssignedWorkers);
    /// <summary>Local storage units relative to one base producer (+50 per output in v13).</summary>
    public int StorageEquivalents => TotalWorkplaces;

    public static TieredBuildingPortfolio Empty(string familyId) => Create(familyId, default, default, default);

    public static TieredBuildingPortfolio Create(string familyId, TierOccupancy basic,
        TierOccupancy large, TierOccupancy advanced) => new(familyId, basic, large, advanced);

    public static TieredBuildingPortfolio FromLegacy(BuildingDefinition definition, int buildingCount, int assignedWorkers)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (definition.Recipe is null || definition.WorkplacesPerBuilding != 1 || definition.IsScenarioFixed)
            throw new ArgumentException("Only single-worker productive buildings can be imported.", nameof(definition));
        return Create(definition.Id, new(buildingCount, assignedWorkers), default, default);
    }

    public TierOccupancy At(ProductionTier tier) => tier switch
    {
        ProductionTier.Base => Base,
        ProductionTier.Large => Large,
        ProductionTier.Advanced => Advanced,
        _ => throw new ArgumentOutOfRangeException(nameof(tier))
    };

    public TieredBuildingPortfolio Build(ProductionTier tier, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        var current = At(tier);
        return Replace(tier, current with { Buildings = checked(current.Buildings + quantity) });
    }

    /// <summary>
    /// Upgrade one building. In aggregated data, workers are packed into the first building
    /// of the source tier; only existing workers transfer and no extra worker is created.
    /// </summary>
    public TieredBuildingPortfolio Upgrade(ProductionTier from, ProductionTier to)
    {
        if (from >= to || to > ProductionTier.Advanced || from < ProductionTier.Base)
            throw new ArgumentOutOfRangeException(nameof(to), "Upgrades must move to a higher supported tier.");
        var source = At(from);
        var target = At(to);
        if (source.Buildings == 0)
            throw new InvalidOperationException("No source building is available for this upgrade.");
        var sourceSlots = Workplaces(from);
        var transferred = Math.Min(sourceSlots, source.AssignedWorkers);
        var reduced = source with
        {
            Buildings = source.Buildings - 1,
            AssignedWorkers = source.AssignedWorkers - transferred
        };
        var increased = target with
        {
            Buildings = checked(target.Buildings + 1),
            AssignedWorkers = checked(target.AssignedWorkers + transferred)
        };
        return Replace(from, reduced).Replace(to, increased);
    }

    public TieredBuildingPortfolio Assign(ProductionTier tier, int workers, int availablePopulation)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(workers);
        ArgumentOutOfRangeException.ThrowIfNegative(availablePopulation);
        var current = At(tier);
        if (workers > availablePopulation)
            throw new InvalidOperationException("Not enough available inhabitants.");
        if ((long)current.AssignedWorkers + workers > (long)current.Buildings * Workplaces(tier))
            throw new InvalidOperationException("Not enough workplaces in the selected tier.");
        return Replace(tier, current with { AssignedWorkers = checked(current.AssignedWorkers + workers) });
    }

    public TieredBuildingPortfolio Release(ProductionTier tier, int workers)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(workers);
        var current = At(tier);
        if (workers > current.AssignedWorkers)
            throw new InvalidOperationException("Cannot release more workers than assigned.");
        return Replace(tier, current with { AssignedWorkers = current.AssignedWorkers - workers });
    }

    private TieredBuildingPortfolio Replace(ProductionTier tier, TierOccupancy next) => tier switch
    {
        ProductionTier.Base => Create(FamilyId, next, Large, Advanced),
        ProductionTier.Large => Create(FamilyId, Base, next, Advanced),
        ProductionTier.Advanced => Create(FamilyId, Base, Large, next),
        _ => throw new ArgumentOutOfRangeException(nameof(tier))
    };

    private static int Workplaces(ProductionTier tier) => tier switch
    {
        ProductionTier.Base => 1,
        ProductionTier.Large => 3,
        ProductionTier.Advanced => 10,
        _ => throw new ArgumentOutOfRangeException(nameof(tier))
    };

    private static void Validate(TierOccupancy occupancy, int workplaces)
    {
        if (occupancy.Buildings < 0 || occupancy.AssignedWorkers < 0 ||
            (long)occupancy.AssignedWorkers > (long)occupancy.Buildings * workplaces)
            throw new ArgumentOutOfRangeException(nameof(occupancy), "Worker assignments exceed the buildings' capacity.");
    }
}
