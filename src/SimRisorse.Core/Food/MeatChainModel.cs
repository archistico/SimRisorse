using System.Collections.Immutable;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Food;

/// <summary>Alternative butcher recipe: cattle and hunted game retain distinct yields.</summary>
public sealed class ButcherMeatRecipe
{
    public ButcherMeatRecipe(string animalResourceId, ResourceAmount animalsPerBatch,
        ResourceAmount meatPerBatch, int priority)
    {
        ContentIdentifier.Validate(animalResourceId, nameof(animalResourceId));
        if (animalsPerBatch == ResourceAmount.Zero || meatPerBatch == ResourceAmount.Zero)
            throw new ArgumentOutOfRangeException(nameof(animalsPerBatch), "Butchering needs positive input and output.");
        AnimalResourceId = animalResourceId;
        AnimalsPerBatch = animalsPerBatch;
        MeatPerBatch = meatPerBatch;
        Priority = priority;
    }

    public string AnimalResourceId { get; }
    public ResourceAmount AnimalsPerBatch { get; }
    public ResourceAmount MeatPerBatch { get; }
    public int Priority { get; }
}

/// <summary>F1-D02 draft policy: no new economic yields have been activated in the v14 catalog.</summary>
public sealed class MeatChainPolicy
{
    public MeatChainPolicy(string grainResourceId, string waterResourceId, string cattleResourceId,
        string gameResourceId, string meatResourceId, ResourceBundle feedPerCattleBatch,
        ResourceAmount cattlePerBatch, ResourceAmount gamePerHuntBatch,
        IEnumerable<ButcherMeatRecipe> butcherRecipes)
    {
        var ids = new[] { grainResourceId, waterResourceId, cattleResourceId, gameResourceId, meatResourceId };
        foreach (var id in ids) ContentIdentifier.Validate(id, nameof(grainResourceId));
        if (ids.Distinct(StringComparer.Ordinal).Count() != ids.Length)
            throw new ArgumentException("Feed, animals and meat need distinct resource IDs.", nameof(cattleResourceId));
        ArgumentNullException.ThrowIfNull(feedPerCattleBatch);
        ArgumentNullException.ThrowIfNull(butcherRecipes);
        if (feedPerCattleBatch.Amounts.Count != 2 ||
            feedPerCattleBatch[grainResourceId] == ResourceAmount.Zero ||
            feedPerCattleBatch[waterResourceId] == ResourceAmount.Zero)
            throw new ArgumentException("Each stable batch must consume grain and water, and nothing else.", nameof(feedPerCattleBatch));
        if (cattlePerBatch == ResourceAmount.Zero || gamePerHuntBatch == ResourceAmount.Zero)
            throw new ArgumentOutOfRangeException(nameof(cattlePerBatch), "Animal output per batch must be positive.");

        var recipes = butcherRecipes.ToImmutableArray();
        if (recipes.Length != 2 || recipes.Any(recipe => recipe is null) ||
            !recipes.Select(recipe => recipe.AnimalResourceId).ToHashSet(StringComparer.Ordinal)
                .SetEquals(new[] { cattleResourceId, gameResourceId }))
            throw new ArgumentException("The butcher needs exactly one cattle and one game recipe.", nameof(butcherRecipes));

        GrainResourceId = grainResourceId;
        WaterResourceId = waterResourceId;
        CattleResourceId = cattleResourceId;
        GameResourceId = gameResourceId;
        MeatResourceId = meatResourceId;
        FeedPerCattleBatch = feedPerCattleBatch;
        CattlePerBatch = cattlePerBatch;
        GamePerHuntBatch = gamePerHuntBatch;
        ButcherRecipes = [.. recipes.OrderBy(recipe => recipe.Priority)
            .ThenBy(recipe => recipe.AnimalResourceId, StringComparer.Ordinal)];
    }

    public string GrainResourceId { get; }
    public string WaterResourceId { get; }
    public string CattleResourceId { get; }
    public string GameResourceId { get; }
    public string MeatResourceId { get; }
    public ResourceBundle FeedPerCattleBatch { get; }
    public ResourceAmount CattlePerBatch { get; }
    public ResourceAmount GamePerHuntBatch { get; }
    public ImmutableArray<ButcherMeatRecipe> ButcherRecipes { get; }
}

public enum MeatChainStage { Stable, Hunting, ButcherCattle, ButcherGame }
public sealed record MeatChainStageRun(MeatChainStage Stage, long Batches,
    ResourceBundle Inputs, ResourceBundle Outputs);
public sealed record MeatChainTickResult(ResourceBundle NextStock,
    ResourceBundle Consumed, ResourceBundle Produced, ImmutableArray<MeatChainStageRun> Runs);

/// <summary>Pure deterministic planning. Only animals present at the START of a tick may
/// be butchered. Fresh animals become available next tick, and meat remains a raw input
/// until F1-D04's depot converts it. This is NOT yet wired into the playable v14 session.</summary>
public static class MeatChainPlanner
{
    public static MeatChainTickResult PlanTick(MeatChainPolicy policy, ResourceBundle stock,
        ResourceBundle capacity, long stableWorkers, long hunterWorkers, long butcherWorkers)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(stock);
        ArgumentNullException.ThrowIfNull(capacity);
        ArgumentOutOfRangeException.ThrowIfNegative(stableWorkers);
        ArgumentOutOfRangeException.ThrowIfNegative(hunterWorkers);
        ArgumentOutOfRangeException.ThrowIfNegative(butcherWorkers);
        var ids = new[] { policy.GrainResourceId, policy.WaterResourceId, policy.CattleResourceId,
            policy.GameResourceId, policy.MeatResourceId }.ToHashSet(StringComparer.Ordinal);
        if (stock.Amounts.Keys.Any(id => !ids.Contains(id)) ||
            capacity.Amounts.Keys.Any(id => !ids.Contains(id)))
            throw new ArgumentException("The chain accepts only its five declared resources.");

        var consumed = ResourceBundle.Empty;
        var produced = ResourceBundle.Empty;
        var runs = ImmutableArray.CreateBuilder<MeatChainStageRun>(4);

        var stableBatches = stableWorkers;
        foreach (var (id, amount) in policy.FeedPerCattleBatch.Amounts)
            stableBatches = Math.Min(stableBatches, stock[id].Milliunits / amount.Milliunits);
        stableBatches = Math.Min(stableBatches,
            FreeSpace(capacity[policy.CattleResourceId], stock[policy.CattleResourceId]) /
            policy.CattlePerBatch.Milliunits);
        var feed = policy.FeedPerCattleBatch.Scale(stableBatches);
        var newCattle = Single(policy.CattleResourceId, policy.CattlePerBatch * stableBatches);
        consumed += feed;
        produced += newCattle;
        runs.Add(new(MeatChainStage.Stable, stableBatches, feed, newCattle));

        var huntBatches = Math.Min(hunterWorkers,
            FreeSpace(capacity[policy.GameResourceId], stock[policy.GameResourceId]) /
            policy.GamePerHuntBatch.Milliunits);
        var newGame = Single(policy.GameResourceId, policy.GamePerHuntBatch * huntBatches);
        produced += newGame;
        runs.Add(new(MeatChainStage.Hunting, huntBatches, ResourceBundle.Empty, newGame));

        var freeMeat = FreeSpace(capacity[policy.MeatResourceId], stock[policy.MeatResourceId]);
        var remainingWorkers = butcherWorkers;
        foreach (var recipe in policy.ButcherRecipes)
        {
            var batches = Math.Min(remainingWorkers,
                Math.Min(stock[recipe.AnimalResourceId].Milliunits / recipe.AnimalsPerBatch.Milliunits,
                    freeMeat / recipe.MeatPerBatch.Milliunits));
            var input = Single(recipe.AnimalResourceId, recipe.AnimalsPerBatch * batches);
            var output = Single(policy.MeatResourceId, recipe.MeatPerBatch * batches);
            consumed += input;
            produced += output;
            remainingWorkers -= batches;
            freeMeat -= output[policy.MeatResourceId].Milliunits;
            runs.Add(new(recipe.AnimalResourceId == policy.CattleResourceId
                ? MeatChainStage.ButcherCattle : MeatChainStage.ButcherGame, batches, input, output));
        }
        return new(stock - consumed + produced, consumed, produced, runs.MoveToImmutable());
    }

    private static long FreeSpace(ResourceAmount capacity, ResourceAmount current) =>
        Math.Max(0L, capacity.Milliunits - current.Milliunits);

    private static ResourceBundle Single(string id, ResourceAmount amount) => amount == ResourceAmount.Zero
        ? ResourceBundle.Empty
        : ResourceBundle.FromAmounts([KeyValuePair.Create(id, amount)]);
}
