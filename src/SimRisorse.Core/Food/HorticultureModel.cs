using System.Collections.Immutable;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Food;

/// <summary>A single Orto/Frutteto recipe. Water is optional, so an accessible
/// short food chain can be tested without committing to a balancing cost.</summary>
public sealed class HorticultureCropRecipe
{
    public HorticultureCropRecipe(string cropResourceId, ResourceAmount cropPerBatch,
        ResourceAmount waterPerBatch, int priority)
    {
        ContentIdentifier.Validate(cropResourceId, nameof(cropResourceId));
        if (cropPerBatch == ResourceAmount.Zero)
            throw new ArgumentOutOfRangeException(nameof(cropPerBatch), "A crop batch must produce something.");
        CropResourceId = cropResourceId;
        CropPerBatch = cropPerBatch;
        WaterPerBatch = waterPerBatch;
        Priority = priority;
    }

    public string CropResourceId { get; }
    public ResourceAmount CropPerBatch { get; }
    public ResourceAmount WaterPerBatch { get; }
    public int Priority { get; }
}

/// <summary>F1-D03 isolated rules. Garden and orchard are distinct buildings
/// and their yields are deliberately configurable, not active v14 prices.</summary>
public sealed class HorticulturePolicy
{
    public HorticulturePolicy(string waterResourceId, string vegetablesResourceId, string fruitResourceId,
        HorticultureCropRecipe garden, HorticultureCropRecipe orchard)
    {
        ContentIdentifier.Validate(waterResourceId, nameof(waterResourceId));
        ContentIdentifier.Validate(vegetablesResourceId, nameof(vegetablesResourceId));
        ContentIdentifier.Validate(fruitResourceId, nameof(fruitResourceId));
        if (new[] { waterResourceId, vegetablesResourceId, fruitResourceId }
            .Distinct(StringComparer.Ordinal).Count() != 3)
            throw new ArgumentException("Water, vegetables and fruit must be distinct resources.");
        ArgumentNullException.ThrowIfNull(garden);
        ArgumentNullException.ThrowIfNull(orchard);
        if (garden.CropResourceId != vegetablesResourceId || orchard.CropResourceId != fruitResourceId)
            throw new ArgumentException("Garden must produce vegetables and orchard must produce fruit.");

        WaterResourceId = waterResourceId;
        VegetablesResourceId = vegetablesResourceId;
        FruitResourceId = fruitResourceId;
        Garden = garden;
        Orchard = orchard;
        PrioritizedRecipes = [.. new[] { garden, orchard }.OrderBy(recipe => recipe.Priority)
            .ThenBy(recipe => recipe.CropResourceId, StringComparer.Ordinal)];
    }

    public string WaterResourceId { get; }
    public string VegetablesResourceId { get; }
    public string FruitResourceId { get; }
    public HorticultureCropRecipe Garden { get; }
    public HorticultureCropRecipe Orchard { get; }
    public ImmutableArray<HorticultureCropRecipe> PrioritizedRecipes { get; }
}

public sealed record HorticultureCropRun(string CropResourceId, long Batches,
    ResourceBundle Inputs, ResourceBundle Outputs);
public sealed record HorticultureTickResult(ResourceBundle NextStock,
    ResourceBundle Consumed, ResourceBundle Produced, ImmutableArray<HorticultureCropRun> Runs);

/// <summary>Deterministic, side-effect-free production. Each Orto and Frutteto
/// uses its own workers and output capacity; they only compete for water when
/// configured to require it. Fruit/vegetables remain raw stock until received
/// and processed by the food depot (F1-D04), never direct ready-to-eat food.</summary>
public static class HorticulturePlanner
{
    public static HorticultureTickResult PlanTick(HorticulturePolicy policy,
        ResourceBundle stock, ResourceBundle capacity, long gardenWorkers, long orchardWorkers)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(stock);
        ArgumentNullException.ThrowIfNull(capacity);
        ArgumentOutOfRangeException.ThrowIfNegative(gardenWorkers);
        ArgumentOutOfRangeException.ThrowIfNegative(orchardWorkers);

        var allowed = new[] { policy.WaterResourceId, policy.VegetablesResourceId,
            policy.FruitResourceId }.ToHashSet(StringComparer.Ordinal);
        if (stock.Amounts.Keys.Any(id => !allowed.Contains(id)) ||
            capacity.Amounts.Keys.Any(id => !allowed.Contains(id)))
            throw new ArgumentException("Horticulture only accepts water, vegetables and fruit.");

        var waterAvailable = stock[policy.WaterResourceId].Milliunits;
        var consumed = ResourceBundle.Empty;
        var produced = ResourceBundle.Empty;
        var runs = ImmutableArray.CreateBuilder<HorticultureCropRun>(2);

        foreach (var recipe in policy.PrioritizedRecipes)
        {
            var workers = recipe.CropResourceId == policy.VegetablesResourceId
                ? gardenWorkers : orchardWorkers;
            var free = Math.Max(0L, capacity[recipe.CropResourceId].Milliunits -
                stock[recipe.CropResourceId].Milliunits);
            var byWater = recipe.WaterPerBatch == ResourceAmount.Zero
                ? long.MaxValue : waterAvailable / recipe.WaterPerBatch.Milliunits;
            var batches = Math.Min(workers,
                Math.Min(free / recipe.CropPerBatch.Milliunits, byWater));
            var usedWater = recipe.WaterPerBatch * batches;
            var grown = recipe.CropPerBatch * batches;
            var inputs = AsBundle(policy.WaterResourceId, usedWater);
            var outputs = AsBundle(recipe.CropResourceId, grown);
            consumed += inputs;
            produced += outputs;
            waterAvailable -= usedWater.Milliunits;
            runs.Add(new(recipe.CropResourceId, batches, inputs, outputs));
        }

        return new(stock - consumed + produced, consumed, produced, runs.MoveToImmutable());
    }

    private static ResourceBundle AsBundle(string id, ResourceAmount amount) => amount == ResourceAmount.Zero
        ? ResourceBundle.Empty
        : ResourceBundle.FromAmounts([KeyValuePair.Create(id, amount)]);
}
