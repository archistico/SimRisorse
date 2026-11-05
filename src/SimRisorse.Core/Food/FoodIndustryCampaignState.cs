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

    public static long Get(FoodIndustryCrew crew, string siteId) => siteId switch
    {
        "stable" => crew.Stable,
        "hunter" => crew.Hunter,
        "butcher" => crew.Butcher,
        "garden" => crew.Garden,
        "orchard" => crew.Orchard,
        "food-depot" => crew.Depot,
        _ => throw new ArgumentException("Unknown food-industry building.", nameof(siteId))
    };

    public static FoodIndustryCrew FromDictionary(IReadOnlyDictionary<string, long> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count != Ids.Length || Ids.Any(id => !values.ContainsKey(id)))
            throw new ArgumentException("Food-industry building state is incomplete.", nameof(values));
        return new(values["stable"], values["hunter"], values["butcher"],
            values["garden"], values["orchard"], values["food-depot"]);
    }
}

public sealed record FoodIndustryBuildings(FoodIndustryCrew Counts, FoodIndustryCrew Assigned)
{
    private static readonly FoodIndustryCrew Zero = new(0, 0, 0, 0, 0, 0);
    public static FoodIndustryBuildings Empty { get; } = new(Zero, Zero);

    // F1-D05d.2: all six base buildings have exactly one workstation.
    // Do NOT persist calculated workplaces as an independent source of truth.
    public FoodIndustryCrew Workplaces => Counts;

    public void Validate(int population, long busyOutsideFood)
    {
        ArgumentNullException.ThrowIfNull(Counts);
        ArgumentNullException.ThrowIfNull(Assigned);
        Counts.Validate();
        new FoodIndustryPersonnel(Assigned, Workplaces, population, busyOutsideFood).Validate();
    }
}

/// <summary>
/// Persistent food-industry state not represented in the inherited city snapshot:
/// cattle/game, processed-but-not-delivered food and the six building crews.
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
