using System.Collections.Immutable;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Food;

/// <summary>One indivisible transformation of an edible input into ready-to-eat food.
/// A depot may own several recipes; they are processed by priority and ordinal resource ID.</summary>
public sealed class FoodConversionRecipe
{
    public FoodConversionRecipe(string sourceResourceId, ResourceAmount inputPerBatch,
        ResourceAmount foodPerBatch, int priority)
    {
        ContentIdentifier.Validate(sourceResourceId, nameof(sourceResourceId));
        if (inputPerBatch == ResourceAmount.Zero || foodPerBatch == ResourceAmount.Zero)
            throw new ArgumentOutOfRangeException(nameof(inputPerBatch), "Food conversions need positive input and output.");
        SourceResourceId = sourceResourceId;
        InputPerBatch = inputPerBatch;
        FoodPerBatch = foodPerBatch;
        Priority = priority;
    }

    public string SourceResourceId { get; }
    public ResourceAmount InputPerBatch { get; }
    public ResourceAmount FoodPerBatch { get; }
    public int Priority { get; }
}

/// <summary>F1 draft contract; no candidate economic yields are activated by defining this type.</summary>
public sealed class FoodConversionPolicy
{
    public FoodConversionPolicy(string readyFoodResourceId, string breadResourceId,
        IEnumerable<FoodConversionRecipe> recipes)
    {
        ContentIdentifier.Validate(readyFoodResourceId, nameof(readyFoodResourceId));
        ContentIdentifier.Validate(breadResourceId, nameof(breadResourceId));
        ArgumentNullException.ThrowIfNull(recipes);
        if (readyFoodResourceId == breadResourceId)
            throw new ArgumentException("Prepared food and bread must remain distinct.", nameof(breadResourceId));

        var entries = recipes.ToImmutableArray();
        if (entries.IsEmpty || entries.Any(item => item is null) ||
            entries.Any(item => item.SourceResourceId == readyFoodResourceId) ||
            entries.Select(item => item.SourceResourceId).Distinct(StringComparer.Ordinal).Count() != entries.Length)
            throw new ArgumentException("Each edible input needs one unique conversion; prepared food cannot be an input.", nameof(recipes));

        ReadyFoodResourceId = readyFoodResourceId;
        BreadResourceId = breadResourceId;
        Recipes = [.. entries.OrderBy(item => item.Priority)
            .ThenBy(item => item.SourceResourceId, StringComparer.Ordinal)];
    }

    public string ReadyFoodResourceId { get; }
    public string BreadResourceId { get; }
    public ImmutableArray<FoodConversionRecipe> Recipes { get; }
}

/// <summary>Physical depot inventory, separate from the general city inventory.
/// No ingredient counts as consumable food until a depot conversion produces ReadyFood.</summary>
public sealed record FoodDepotStock(ResourceBundle RawFood, ResourceAmount ReadyFood);

public sealed record FoodDepotReceipt(bool Accepted, FoodDepotStock NextStock);
public sealed record FoodConversionRun(string SourceResourceId, long Batches,
    ResourceAmount ConsumedInput, ResourceAmount ProducedFood);
public sealed record FoodDepotProcessingResult(FoodDepotStock NextStock, ResourceBundle ConsumedIngredients,
    ResourceAmount ProducedFood, ResourceAmount ReservedBread, long BatchesProcessed,
    ImmutableArray<FoodConversionRun> Runs);

/// <summary>Pure, deterministic planning foundation. The currently playable v14 session does NOT use this yet.
/// F1-D04 must integrate receipt/processing with the actual depot staffing and atomic world tick.</summary>
public static class FoodDepotPlanner
{
    /// <summary>Receive a full delivery or none of it; a depot cannot silently overflow its raw-food bins.</summary>
    public static FoodDepotReceipt Receive(FoodConversionPolicy policy, FoodDepotStock stock,
        ResourceBundle delivery, ResourceBundle rawCapacities)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ValidateStock(policy, stock);
        ArgumentNullException.ThrowIfNull(delivery);
        ArgumentNullException.ThrowIfNull(rawCapacities);
        var inputs = policy.Recipes.Select(item => item.SourceResourceId).ToHashSet(StringComparer.Ordinal);
        if (delivery.Amounts.Keys.Any(id => !inputs.Contains(id)))
            throw new ArgumentException("Only food inputs declared by the depot may be delivered.", nameof(delivery));
        if (delivery.Amounts.Any(item =>
                stock.RawFood[item.Key].Milliunits > rawCapacities[item.Key].Milliunits - item.Value.Milliunits))
            return new(false, stock);
        return new(true, stock with { RawFood = stock.RawFood + delivery });
    }

    /// <summary>Process whole recipe batches without exceeding worker throughput or prepared-food capacity.
    /// Reserved bread remains available to advanced residents and is never double-spent as generic food.</summary>
    public static FoodDepotProcessingResult Plan(FoodConversionPolicy policy, FoodDepotStock stock,
        ResourceAmount readyFoodCapacity, long maximumBatchesPerTick, ResourceAmount minimumBreadReserve)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ValidateStock(policy, stock);
        ArgumentOutOfRangeException.ThrowIfNegative(maximumBatchesPerTick);

        var breadStock = stock.RawFood[policy.BreadResourceId];
        var reservedBread = ResourceAmount.FromMilliunits(Math.Min(breadStock.Milliunits, minimumBreadReserve.Milliunits));
        var freeFood = Math.Max(0L, readyFoodCapacity.Milliunits - stock.ReadyFood.Milliunits);
        var remainingBatches = maximumBatchesPerTick;
        long totalFood = 0;
        var consumed = new List<KeyValuePair<string, ResourceAmount>>();
        var runs = ImmutableArray.CreateBuilder<FoodConversionRun>();

        foreach (var recipe in policy.Recipes)
        {
            if (remainingBatches == 0 || freeFood == 0) break;
            var inputMilliunits = stock.RawFood[recipe.SourceResourceId].Milliunits;
            if (recipe.SourceResourceId == policy.BreadResourceId)
                inputMilliunits -= reservedBread.Milliunits;
            var batches = Math.Min(remainingBatches,
                Math.Min(inputMilliunits / recipe.InputPerBatch.Milliunits,
                    freeFood / recipe.FoodPerBatch.Milliunits));
            if (batches == 0) continue;

            var input = recipe.InputPerBatch * batches;
            var output = recipe.FoodPerBatch * batches;
            consumed.Add(KeyValuePair.Create(recipe.SourceResourceId, input));
            runs.Add(new(recipe.SourceResourceId, batches, input, output));
            totalFood = checked(totalFood + output.Milliunits);
            freeFood -= output.Milliunits;
            remainingBatches -= batches;
        }

        var spent = ResourceBundle.FromAmounts(consumed);
        var produced = ResourceAmount.FromMilliunits(totalFood);
        return new(stock with
        {
            RawFood = stock.RawFood - spent,
            ReadyFood = stock.ReadyFood + produced
        }, spent, produced, reservedBread, maximumBatchesPerTick - remainingBatches, runs.ToImmutable());
    }

    private static void ValidateStock(FoodConversionPolicy policy, FoodDepotStock stock)
    {
        ArgumentNullException.ThrowIfNull(stock);
        ArgumentNullException.ThrowIfNull(stock.RawFood);
        var allowed = policy.Recipes.Select(item => item.SourceResourceId).ToHashSet(StringComparer.Ordinal);
        if (stock.RawFood.Amounts.Keys.Any(id => !allowed.Contains(id)))
            throw new ArgumentException("A food depot holds only ingredients declared by its conversion policy.", nameof(stock));
    }
}
