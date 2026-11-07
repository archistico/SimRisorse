using System.Collections.Immutable;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Food;

/// <summary>
/// F1-D05d.2: the six planned directly constructible food workplaces, including the depot.
/// Each base building has one workstation in this development checkpoint;
/// future tiers will change the catalog-derived multiplier, not the saved worker count.
/// Building counts and assigned workers are the only two persisted quantities.
/// </summary>
public static class FoodIndustrySites
{
    public static ImmutableArray<string> Ids { get; } =
        ["stable", "hunter", "butcher", "garden", "orchard", "food-depot"];

    // Tier rows are persistable but not part of the six independently modeled food recipes.
    public static ImmutableArray<string> DepotTierIds { get; } = ["food-depot-large", "food-depot-advanced"];
    public static ImmutableArray<string> PersistedIds { get; } = [.. Ids, .. DepotTierIds];

    public static long Get(FoodIndustryCrew crew, string siteId) => siteId switch
    {
        "stable" => crew.Stable,
        "hunter" => crew.Hunter,
        "butcher" => crew.Butcher,
        "garden" => crew.Garden,
        "orchard" => crew.Orchard,
        "food-depot" => crew.Depot,
        "food-depot-large" => crew.DepotLarge,
        "food-depot-advanced" => crew.DepotAdvanced,
        _ => throw new ArgumentException("Unknown food-industry building.", nameof(siteId))
    };

    public static FoodIndustryCrew FromDictionary(IReadOnlyDictionary<string, long> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if ((values.Count != Ids.Length && values.Count != PersistedIds.Length) ||
            Ids.Any(id => !values.ContainsKey(id)) ||
            values.Keys.Any(id => !PersistedIds.Contains(id)))
            throw new ArgumentException("Food-industry building state is incomplete.", nameof(values));
        // Six-key legacy in-memory fixtures still create the original base-only portfolio.
        return new(values["stable"], values["hunter"], values["butcher"],
            values["garden"], values["orchard"], values["food-depot"],
            values.TryGetValue("food-depot-large", out var large) ? large : 0,
            values.TryGetValue("food-depot-advanced", out var advanced) ? advanced : 0);
    }
}

public sealed record FoodIndustryBuildings(FoodIndustryCrew Counts, FoodIndustryCrew Assigned)
{
    private static readonly FoodIndustryCrew Zero = new(0, 0, 0, 0, 0, 0);
    public static FoodIndustryBuildings Empty { get; } = new(Zero, Zero);

    // Each Grande depot holds three staffed conversion modules; Avanzato ten.
    // The tiers have separate staffing limits. Derived workplaces are never saved.
    public FoodIndustryCrew Workplaces => Counts with
    {
        DepotLarge = checked(Counts.DepotLarge * 3),
        DepotAdvanced = checked(Counts.DepotAdvanced * 10)
    };

    // The food planner processes one shared physical depot. Aggregate staffed
    // modules only AFTER independently verifying each tier's staffing limits.
    public FoodIndustryPersonnel PlannerPersonnel(long population, long busyOutsideFood)
    {
        Validate(population, busyOutsideFood);
        var assigned = Assigned with
        {
            Depot = checked(Assigned.Depot + Assigned.DepotLarge + Assigned.DepotAdvanced),
            DepotLarge = 0, DepotAdvanced = 0
        };
        var workplaces = Workplaces with
        {
            Depot = checked(Workplaces.Depot + Workplaces.DepotLarge + Workplaces.DepotAdvanced),
            DepotLarge = 0, DepotAdvanced = 0
        };
        return new(assigned, workplaces, population, busyOutsideFood);
    }

    // Use the same population type as PlannerPersonnel/FoodIndustryPersonnel:
    // narrowing to int would reject valid long population values at compile time.
    public void Validate(long population, long busyOutsideFood)
    {
        ArgumentNullException.ThrowIfNull(Counts);
        ArgumentNullException.ThrowIfNull(Assigned);
        Counts.Validate();
        new FoodIndustryPersonnel(Assigned, Workplaces, population, busyOutsideFood).Validate();
    }
}

/// <summary>
/// Persistent food-industry state not represented in the inherited city snapshot:
/// cattle/game, processed-but-not-delivered food and the six base plus two tier crews.
/// Raw ingredients and prepared food remain exclusively in FoodDepotStock.
/// </summary>
public sealed record FoodIndustryCampaignState(
    ResourceBundle Animals, ResourceBundle Transit, FoodIndustryBuildings Buildings)
{
    public static FoodIndustryCampaignState Empty { get; } =
        new(ResourceBundle.Empty, ResourceBundle.Empty, FoodIndustryBuildings.Empty);

    public bool IsEmpty => Animals.Equals(ResourceBundle.Empty) &&
        Transit.Equals(ResourceBundle.Empty) && Buildings == FoodIndustryBuildings.Empty;

    public void Validate(int population, long busyOutsideFood)
    {
        ArgumentNullException.ThrowIfNull(Animals);
        ArgumentNullException.ThrowIfNull(Transit);
        ArgumentNullException.ThrowIfNull(Buildings);
        if (Animals.Amounts.Keys.Any(id => id is not ("cattle" or "game")) ||
            Transit.Amounts.Keys.Any(id => id is not ("meat" or "fruit" or "vegetables")))
            throw new ArgumentException("An intermediate resource belongs to the wrong inventory.", nameof(Animals));
        Buildings.Validate(population, busyOutsideFood);
    }

    public FoodIndustryStocks ToStocks(ResourceBundle city, FoodDepotStock depot) =>
        new(city, Animals, Transit, depot);
}
