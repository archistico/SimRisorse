using SimRisorse.Core.Content;

namespace SimRisorse.Core.Construction;

/// <summary>
/// E4b / RC80: a proposed, isolated staffing contract. A building owns 1/3/10
/// productive modules; each full module requires CrewSize distinct inhabitants.
/// RC81 reuses this tier-aware complete-team calculation in the live mint
/// clock; RC80 hypothetical studies remain independent of campaign state.
/// </summary>
public sealed class CrewTeamPortfolio
{
    private CrewTeamPortfolio(string familyId, int crewSize, TierOccupancy basic,
        TierOccupancy large, TierOccupancy advanced)
    {
        ContentIdentifier.Validate(familyId, nameof(familyId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(crewSize);
        Validate(basic, ProductionTier.Base, crewSize);
        Validate(large, ProductionTier.Large, crewSize);
        Validate(advanced, ProductionTier.Advanced, crewSize);
        FamilyId = familyId;
        CrewSize = crewSize;
        Base = basic;
        Large = large;
        Advanced = advanced;
        _ = TotalModules;
        _ = AssignedWorkers;
    }

    public string FamilyId { get; }
    public int CrewSize { get; }
    public TierOccupancy Base { get; }
    public TierOccupancy Large { get; }
    public TierOccupancy Advanced { get; }

    public int TotalBuildings => checked(checked(Base.Buildings + Large.Buildings) + Advanced.Buildings);
    public int TotalModules => checked(checked(Base.Buildings + checked(3 * Large.Buildings)) + checked(10 * Advanced.Buildings));
    public int AssignedWorkers => checked(checked(Base.AssignedWorkers + Large.AssignedWorkers) + Advanced.AssignedWorkers);
    public int StaffedModules => checked(checked(Base.AssignedWorkers / CrewSize + Large.AssignedWorkers / CrewSize)
        + Advanced.AssignedWorkers / CrewSize);
    public int UnstaffedModules => TotalModules - StaffedModules;
    public int IncompleteCrewWorkers => AssignedWorkers - checked(StaffedModules * CrewSize);

    /// <summary>Smallest further assignment to activate a module in any tier; zero if all are staffed.</summary>
    public int MissingForNextModule
    {
        get
        {
            var needed = int.MaxValue;
            foreach (var tier in Enum.GetValues<ProductionTier>())
            {
                var occupancy = At(tier);
                if (occupancy.AssignedWorkers / CrewSize < checked(occupancy.Buildings * ModulesPerBuilding(tier)))
                    needed = Math.Min(needed, CrewSize - occupancy.AssignedWorkers % CrewSize);
            }
            return needed == int.MaxValue ? 0 : needed;
        }
    }

    public static CrewTeamPortfolio Create(string familyId, int crewSize, TierOccupancy basic,
        TierOccupancy large = default, TierOccupancy advanced = default) =>
        new(familyId, crewSize, basic, large, advanced);

    public TierOccupancy At(ProductionTier tier) => tier switch
    {
        ProductionTier.Base => Base,
        ProductionTier.Large => Large,
        ProductionTier.Advanced => Advanced,
        _ => throw new ArgumentOutOfRangeException(nameof(tier))
    };

    public static int ModulesPerBuilding(ProductionTier tier) => tier switch
    {
        ProductionTier.Base => 1,
        ProductionTier.Large => 3,
        ProductionTier.Advanced => 10,
        _ => throw new ArgumentOutOfRangeException(nameof(tier))
    };

    public CrewTeamPortfolio Build(ProductionTier tier, int buildings)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(buildings);
        var current = At(tier);
        return Replace(tier, current with { Buildings = checked(current.Buildings + buildings) });
    }

    public CrewTeamPortfolio Assign(ProductionTier tier, int people, int freePopulation)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(people);
        ArgumentOutOfRangeException.ThrowIfNegative(freePopulation);
        if (people > freePopulation) throw new InvalidOperationException("Not enough available inhabitants.");
        var current = At(tier);
        return Replace(tier, current with { AssignedWorkers = checked(current.AssignedWorkers + people) });
    }

    public CrewTeamPortfolio Release(ProductionTier tier, int people)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(people);
        var current = At(tier);
        if (people > current.AssignedWorkers)
            throw new InvalidOperationException("Cannot release more workers than assigned.");
        return Replace(tier, current with { AssignedWorkers = current.AssignedWorkers - people });
    }

    private CrewTeamPortfolio Replace(ProductionTier tier, TierOccupancy next) => tier switch
    {
        ProductionTier.Base => Create(FamilyId, CrewSize, next, Large, Advanced),
        ProductionTier.Large => Create(FamilyId, CrewSize, Base, next, Advanced),
        ProductionTier.Advanced => Create(FamilyId, CrewSize, Base, Large, next),
        _ => throw new ArgumentOutOfRangeException(nameof(tier))
    };

    private static void Validate(TierOccupancy occupancy, ProductionTier tier, int crewSize)
    {
        if (occupancy.Buildings < 0 || occupancy.AssignedWorkers < 0)
            throw new ArgumentOutOfRangeException(nameof(occupancy), "Negative buildings or workers.");
        var places = checked((long)occupancy.Buildings * ModulesPerBuilding(tier) * crewSize);
        if (occupancy.AssignedWorkers > places)
            throw new ArgumentOutOfRangeException(nameof(occupancy), "Assigned workers exceed crew capacity.");
    }
}
