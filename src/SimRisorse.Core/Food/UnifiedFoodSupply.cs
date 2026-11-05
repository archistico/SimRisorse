using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Food;

/// <summary>
/// F1-D04 resource contract. All five final edible products have exactly one route
/// to ready food. Cattle and game are NOT edible inputs here: the butcher must
/// first transform them into meat. Outputs and priorities remain configurable.
/// </summary>
public static class UnifiedFoodInputs
{
    public const string ReadyFood = "food";
    public const string Bread = "bread";
    public const string Fish = "fish";
    public const string Meat = "meat";
    public const string Fruit = "fruit";
    public const string Vegetables = "vegetables";

    public static FoodConversionPolicy CreatePolicy(IEnumerable<FoodConversionRecipe> recipes)
    {
        ArgumentNullException.ThrowIfNull(recipes);
        var result = new FoodConversionPolicy(ReadyFood, Bread, recipes);
        var expected = new[] { Fish, Bread, Meat, Fruit, Vegetables }
            .ToHashSet(StringComparer.Ordinal);
        if (result.Recipes.Length != expected.Count ||
            result.Recipes.Any(recipe => !expected.Contains(recipe.SourceResourceId)))
            throw new ArgumentException("Exactly fish, bread, meat, fruit and vegetables must be covered.", nameof(recipes));
        return result;
    }
}

/// <summary>
/// Prepared food is an ordinary ration; reserved bread is an additional ration
/// for advanced housing. Either the entire requested meal is served or nothing
/// is deducted. This keeps the two requirements independent and prevents
/// partial consumption when only one of them is available.
/// </summary>
public sealed record FoodDepotMealResult(bool Served, FoodDepotStock NextStock,
    ResourceAmount ConsumedReadyFood, ResourceAmount ConsumedBread);

public static class FoodDepotMeals
{
    public static FoodDepotMealResult Serve(FoodConversionPolicy policy, FoodDepotStock stock,
        ResourceAmount requiredReadyFood, ResourceAmount requiredBread)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(stock);
        ArgumentNullException.ThrowIfNull(stock.RawFood);
        if (stock.RawFood.Amounts.Keys.Except(policy.Recipes.Select(r => r.SourceResourceId),
                StringComparer.Ordinal).Any())
            throw new ArgumentException("Depot contains an ingredient not covered by its policy.", nameof(stock));
        if (stock.ReadyFood.Milliunits < requiredReadyFood.Milliunits ||
            stock.RawFood[policy.BreadResourceId].Milliunits < requiredBread.Milliunits)
            return new(false, stock, default, default);

        var bread = requiredBread == ResourceAmount.Zero ? ResourceBundle.Empty :
            ResourceBundle.FromAmounts([KeyValuePair.Create(policy.BreadResourceId, requiredBread)]);
        return new(true, new(stock.RawFood - bread, stock.ReadyFood - requiredReadyFood),
            requiredReadyFood, requiredBread);
    }
}

public sealed record UnifiedFoodTickResult(
    ResourceBundle NextCityResources,
    FoodDepotStock NextDepot,
    ResourceBundle DeliveredToDepot,
    FoodDepotProcessingResult Processing,
    FoodDepotMealResult Meal);

/// <summary>
/// The pure one-tick boundary between final food products in the city and the
/// food depot. Order: transfer available final products into depot raw bins,
/// process complete batches with depot workers, then serve ordinary food and
/// protected bread rations. Unrelated resources remain in the city bundle.
///
/// This coordinator is NOT yet connected to the playable v14 clock or SQLite:
/// F1-D05 activates it only after the one-time save migration is implemented.
/// </summary>
public static class UnifiedFoodSupply
{
    public static UnifiedFoodTickResult PlanTick(FoodConversionPolicy policy,
        ResourceBundle cityResources, FoodDepotStock depot,
        ResourceBundle rawCapacities, ResourceAmount readyFoodCapacity,
        long processingBatchesPerTick, ResourceAmount breadReserve,
        ResourceAmount readyFoodRation, ResourceAmount advancedBreadRation)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(cityResources);
        ArgumentNullException.ThrowIfNull(depot);
        ArgumentNullException.ThrowIfNull(depot.RawFood);
        ArgumentNullException.ThrowIfNull(rawCapacities);
        ArgumentOutOfRangeException.ThrowIfNegative(processingBatchesPerTick);

        // A migration must move legacy 'food' into the depot once, not let the
        // city and depot both own an independently consumable ready-food stock.
        if (cityResources[policy.ReadyFoodResourceId] != ResourceAmount.Zero)
            throw new ArgumentException("Ready food must have a single owner: the depot.", nameof(cityResources));

        // Keep any pre-migration over-capacity depot stock intact, but never
        // accept additional material until that bin has free capacity.
        var delivery = ResourceBundle.FromAmounts(policy.Recipes.Select(recipe =>
        {
            var id = recipe.SourceResourceId;
            var free = Math.Max(0L, rawCapacities[id].Milliunits - depot.RawFood[id].Milliunits);
            return KeyValuePair.Create(id, ResourceAmount.FromMilliunits(
                Math.Min(cityResources[id].Milliunits, free)));
        }));
        var receipt = FoodDepotPlanner.Receive(policy, depot, delivery, rawCapacities);
        if (!receipt.Accepted)
            throw new InvalidOperationException("Planned depot delivery unexpectedly exceeded capacity.");

        var requiredReserve = ResourceAmount.FromMilliunits(
            Math.Max(breadReserve.Milliunits, advancedBreadRation.Milliunits));
        var processed = FoodDepotPlanner.Plan(policy, receipt.NextStock, readyFoodCapacity,
            processingBatchesPerTick, requiredReserve);
        var meal = FoodDepotMeals.Serve(policy, processed.NextStock,
            readyFoodRation, advancedBreadRation);
        return new(cityResources - delivery, meal.NextStock, delivery, processed, meal);
    }
}
