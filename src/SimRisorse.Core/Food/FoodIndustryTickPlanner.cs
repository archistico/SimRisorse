using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Food;

/// <summary>
/// F1-D05d/1: one explicit, non-persistent boundary for the five new food industries.
/// Animals and unprocessed meat/fruit/vegetables are NOT part of the inherited
/// v14 city inventory: they require their own storage in the future live save.
/// </summary>
public sealed record FoodIndustryStocks(
    ResourceBundle City, ResourceBundle Animals, ResourceBundle Transit,
    FoodDepotStock Depot);

/// <summary>Slots are supplied from actual owned building counts/tiers by the caller;
/// no workstation is implicitly created by allocating a worker.</summary>
public sealed record FoodIndustryCrew(long Stable, long Hunter, long Butcher,
    long Garden, long Orchard, long Depot)
{
    public long Total => checked(checked(checked(checked(checked(Stable + Hunter) + Butcher) + Garden) + Orchard) + Depot);

    public void Validate()
    {
        if (Stable < 0 || Hunter < 0 || Butcher < 0 || Garden < 0 || Orchard < 0 || Depot < 0)
            throw new ArgumentOutOfRangeException(nameof(FoodIndustryCrew), "Workforce cannot be negative.");
        _ = Total;
    }
}

/// <summary>Whole-population guard. BusyOutsideFood must include school trainees,
/// other production, military and every other reserved person exactly once.</summary>
public sealed record FoodIndustryPersonnel(FoodIndustryCrew Assigned, FoodIndustryCrew Workplaces,
    long Population, long BusyOutsideFood)
{
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(Assigned);
        ArgumentNullException.ThrowIfNull(Workplaces);
        Assigned.Validate();
        Workplaces.Validate();
        if (Population < 0 || BusyOutsideFood < 0 || BusyOutsideFood > Population ||
            Assigned.Stable > Workplaces.Stable || Assigned.Hunter > Workplaces.Hunter ||
            Assigned.Butcher > Workplaces.Butcher || Assigned.Garden > Workplaces.Garden ||
            Assigned.Orchard > Workplaces.Orchard || Assigned.Depot > Workplaces.Depot ||
            Assigned.Total > Population - BusyOutsideFood)
            throw new ArgumentException("Food staffing exceeds occupied sites or free population.", nameof(Assigned));
    }
}

/// <summary>Capacities are separated by owner; a historical over-capacity stock
/// is retained, never discarded. The city still owns legacy fish and bread.</summary>
public sealed record FoodIndustryCapacities(ResourceBundle Animals, ResourceBundle Transit,
    ResourceBundle DepotRaw, ResourceAmount DepotReady);

public sealed record FoodIndustryMeals(ResourceAmount MinimumBreadReserve,
    ResourceAmount FoodRation, ResourceAmount AdvancedBreadRation);

public sealed record FoodIndustryTickResult(FoodIndustryStocks Next,
    MeatChainTickResult Meat, HorticultureTickResult Horticulture,
    UnifiedFoodTickResult Depot);

/// <summary>
/// Deterministic ordering: (1) stable/hunt/butcher (butcher uses only animals
/// already present at tick start); (2) garden/orchard, using water left after
/// stables; (3) partial depot receipt; (4) staffed conversion; (5) atomic
/// ordinary+advanced meal. No save, game clock or UI is modified.
/// </summary>
public static class FoodIndustryTickPlanner
{
    /// <summary>Validate all rule identities even for a clock advance shorter than
    /// one second, before any elapsed-tick state is staged.</summary>
    public static void ValidatePolicies(MeatChainPolicy meatPolicy,
        HorticulturePolicy horticulturePolicy, FoodConversionPolicy depotPolicy)
    {
        ArgumentNullException.ThrowIfNull(meatPolicy);
        ArgumentNullException.ThrowIfNull(horticulturePolicy);
        ArgumentNullException.ThrowIfNull(depotPolicy);
        if (meatPolicy.GrainResourceId != "grain" || meatPolicy.WaterResourceId != "water" ||
            meatPolicy.CattleResourceId != "cattle" || meatPolicy.GameResourceId != "game" ||
            meatPolicy.MeatResourceId != UnifiedFoodInputs.Meat ||
            horticulturePolicy.WaterResourceId != "water" ||
            horticulturePolicy.FruitResourceId != UnifiedFoodInputs.Fruit ||
            horticulturePolicy.VegetablesResourceId != UnifiedFoodInputs.Vegetables ||
            depotPolicy.ReadyFoodResourceId != UnifiedFoodInputs.ReadyFood ||
            depotPolicy.BreadResourceId != UnifiedFoodInputs.Bread)
            throw new ArgumentException("The food industries require canonical resource identifiers.");
        _ = UnifiedFoodInputs.CreatePolicy(depotPolicy.Recipes);
    }

    public static FoodIndustryTickResult PlanTick(
        MeatChainPolicy meatPolicy, HorticulturePolicy horticulturePolicy,
        FoodConversionPolicy depotPolicy, FoodIndustryStocks stock,
        FoodIndustryCapacities capacity, FoodIndustryPersonnel personnel,
        FoodIndustryMeals meals)
    {
        ArgumentNullException.ThrowIfNull(meatPolicy);
        ArgumentNullException.ThrowIfNull(horticulturePolicy);
        ArgumentNullException.ThrowIfNull(depotPolicy);
        ArgumentNullException.ThrowIfNull(stock);
        ArgumentNullException.ThrowIfNull(capacity);
        ArgumentNullException.ThrowIfNull(personnel);
        ArgumentNullException.ThrowIfNull(meals);
        ArgumentNullException.ThrowIfNull(stock.City);
        ArgumentNullException.ThrowIfNull(stock.Animals);
        ArgumentNullException.ThrowIfNull(stock.Transit);
        ArgumentNullException.ThrowIfNull(stock.Depot);
        ArgumentNullException.ThrowIfNull(stock.Depot.RawFood);
        ArgumentNullException.ThrowIfNull(capacity.Animals);
        ArgumentNullException.ThrowIfNull(capacity.Transit);
        ArgumentNullException.ThrowIfNull(capacity.DepotRaw);
        personnel.Validate();

        // Only a single owner for each resource, and canonical stage boundaries.
        ValidatePolicies(meatPolicy, horticulturePolicy, depotPolicy);
        RequireOnly(stock.Animals, "cattle", "game");
        RequireOnly(stock.Transit, "meat", "fruit", "vegetables");
        RequireOnly(capacity.Animals, "cattle", "game");
        RequireOnly(capacity.Transit, "meat", "fruit", "vegetables");
        if (new[] { "food", "cattle", "game", "meat", "fruit", "vegetables" }
            .Any(id => stock.City[id] != ResourceAmount.Zero))
            throw new ArgumentException("Food or intermediate inventory has two owners.", nameof(stock));

        var staff = personnel.Assigned;
        var meatInput = ResourceBundle.FromAmounts(new[]
        {
            Item("grain", stock.City), Item("water", stock.City),
            Item("cattle", stock.Animals), Item("game", stock.Animals),
            Item("meat", stock.Transit)
        });
        var meat = MeatChainPlanner.PlanTick(meatPolicy, meatInput,
            capacity.Animals + Take(capacity.Transit, "meat"),
            staff.Stable, staff.Hunter, staff.Butcher);
        var cityAfterMeat = stock.City - Take(meat.Consumed, "grain", "water");

        var cropInput = ResourceBundle.FromAmounts(new[]
        {
            Item("water", cityAfterMeat), Item("fruit", stock.Transit),
            Item("vegetables", stock.Transit)
        });
        var crops = HorticulturePlanner.PlanTick(horticulturePolicy, cropInput,
            Take(capacity.Transit, "fruit", "vegetables"),
            staff.Garden, staff.Orchard);
        var cityAfterCrops = cityAfterMeat - Take(crops.Consumed, "water");
        var transit = Take(meat.NextStock, "meat") + Take(crops.NextStock, "fruit", "vegetables");

        var depot = UnifiedFoodSupply.PlanTick(depotPolicy, cityAfterCrops + transit,
            stock.Depot, capacity.DepotRaw, capacity.DepotReady, staff.Depot,
            meals.MinimumBreadReserve, meals.FoodRation, meals.AdvancedBreadRation);
        var remainingTransit = Take(depot.NextCityResources, "meat", "fruit", "vegetables");
        var remainingCity = depot.NextCityResources - remainingTransit;
        return new(new FoodIndustryStocks(remainingCity,
            Take(meat.NextStock, "cattle", "game"), remainingTransit, depot.NextDepot),
            meat, crops, depot);
    }

    private static KeyValuePair<string, ResourceAmount> Item(string id, ResourceBundle bundle) =>
        KeyValuePair.Create(id, bundle[id]);

    private static ResourceBundle Take(ResourceBundle bundle, params string[] ids) =>
        ResourceBundle.FromAmounts(ids.Select(id => Item(id, bundle)));

    private static void RequireOnly(ResourceBundle bundle, params string[] ids)
    {
        var allowed = ids.ToHashSet(StringComparer.Ordinal);
        if (bundle.Amounts.Keys.Any(id => !allowed.Contains(id)))
            throw new ArgumentException("Unexpected resource owner or capacity.", nameof(bundle));
    }
}
