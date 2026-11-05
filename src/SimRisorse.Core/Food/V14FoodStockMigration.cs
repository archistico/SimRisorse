using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Core.Food;

/// <summary>
/// Read-only preparation of a future v14 -> food-depot checkpoint migration.
/// This is NOT a v15 persistence format and does not write to the game database.
/// Keep the original v14 snapshot intact until a separately versioned checkpoint
/// has been written, verified and made active by an explicit migration flow.
/// </summary>
public sealed record V14FoodStockMigrationPlan(
    V14CampaignSnapshot SourceCheckpoint,
    ResourceBundle CityAfterTransfer,
    FoodDepotStock DepotAfterTransfer);

/// <summary>Capacity requirements of the migrated stock, prior to any new production.
/// A positive shortfall is diagnostic only: migration MUST preserve historical
/// over-capacity stock instead of discarding or converting it automatically.</summary>
public sealed record FoodMigrationCapacityCheck(
    ResourceBundle RawShortfall,
    ResourceAmount ReadyFoodShortfall)
{
    public bool Fits => RawShortfall.Equals(ResourceBundle.Empty) &&
        ReadyFoodShortfall == ResourceAmount.Zero;
}

public static class V14FoodStockMigration
{
    /// <summary>
    /// Separate the existing prepared Cibo and the five edible ingredients from
    /// the legacy city inventory, without converting, consuming, rounding or
    /// clipping any stock. All other resources stay in the city.
    /// Only a v14-format checkpoint is accepted, never the new migrated format.
    /// The caller must validate the source snapshot with V14CampaignSnapshots.Validate
    /// (or load it through the validated v14 store) before using this plan.
    /// </summary>
    public static V14FoodStockMigrationPlan Prepare(
        V14CampaignSnapshot source, FoodConversionPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(policy);
        EnsurePolicy(policy);
        if (source.FormatVersion != V14CampaignSnapshots.FormatVersion ||
            !string.Equals(source.RulesVersion, V14CampaignSnapshots.RulesVersion, StringComparison.Ordinal) ||
            source.Resources is null)
            throw new ArgumentException("Only a validated v14 checkpoint can be prepared for food migration.", nameof(source));

        var raw = ResourceBundle.FromAmounts(policy.Recipes.Select(recipe =>
            KeyValuePair.Create(recipe.SourceResourceId, source.Resources[recipe.SourceResourceId])));
        var ready = source.Resources[policy.ReadyFoodResourceId];
        var transferred = raw + ToReadyBundle(policy, ready);
        var city = source.Resources - transferred;
        var plan = new V14FoodStockMigrationPlan(source, city, new FoodDepotStock(raw, ready));
        // Fail closed if the inventory would change even by a milliunit.
        _ = RestoreLegacyResources(plan, policy);
        return plan;
    }

    /// <summary>
    /// Inverse mapping used by the migration gate and rollback tests. Refuses
    /// duplicated edible resources in the city or an unrecognized depot input.
    /// It must reproduce the complete original v14 city inventory exactly.
    /// </summary>
    public static ResourceBundle RestoreLegacyResources(
        V14FoodStockMigrationPlan plan, FoodConversionPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(policy);
        EnsurePolicy(policy);
        ArgumentNullException.ThrowIfNull(plan.SourceCheckpoint);
        ArgumentNullException.ThrowIfNull(plan.SourceCheckpoint.Resources);
        ArgumentNullException.ThrowIfNull(plan.CityAfterTransfer);
        ArgumentNullException.ThrowIfNull(plan.DepotAfterTransfer);
        ArgumentNullException.ThrowIfNull(plan.DepotAfterTransfer.RawFood);

        var edibleIds = policy.Recipes.Select(recipe => recipe.SourceResourceId)
            .Append(policy.ReadyFoodResourceId).ToHashSet(StringComparer.Ordinal);
        if (plan.CityAfterTransfer.Amounts.Keys.Any(edibleIds.Contains) ||
            plan.DepotAfterTransfer.RawFood.Amounts.Keys.Any(id => !edibleIds.Contains(id) ||
                id == policy.ReadyFoodResourceId))
            throw new ArgumentException("Food has multiple owners or an unknown raw depot ingredient.", nameof(plan));

        var reconstructed = plan.CityAfterTransfer + plan.DepotAfterTransfer.RawFood +
            ToReadyBundle(policy, plan.DepotAfterTransfer.ReadyFood);
        if (!reconstructed.Equals(plan.SourceCheckpoint.Resources))
            throw new ArgumentException("Food migration is not lossless relative to the v14 checkpoint.", nameof(plan));
        return reconstructed;
    }

    /// <summary>
    /// Evaluate prospective capacities without truncating the historic stocks.
    /// A future v15 loader must temporarily accommodate any shortfall, or defer
    /// activation; it may never silently destroy the excess.
    /// </summary>
    public static FoodMigrationCapacityCheck AssessCapacity(
        V14FoodStockMigrationPlan plan, FoodConversionPolicy policy,
        ResourceBundle rawCapacities, ResourceAmount readyFoodCapacity)
    {
        _ = RestoreLegacyResources(plan, policy);
        ArgumentNullException.ThrowIfNull(rawCapacities);
        var deficits = ResourceBundle.FromAmounts(policy.Recipes.Select(recipe =>
        {
            var id = recipe.SourceResourceId;
            var stock = plan.DepotAfterTransfer.RawFood[id].Milliunits;
            var capacity = rawCapacities[id].Milliunits;
            return KeyValuePair.Create(id, ResourceAmount.FromMilliunits(Math.Max(0L, stock - capacity)));
        }));
        var readyDeficit = ResourceAmount.FromMilliunits(Math.Max(0L,
            plan.DepotAfterTransfer.ReadyFood.Milliunits - readyFoodCapacity.Milliunits));
        return new FoodMigrationCapacityCheck(deficits, readyDeficit);
    }

    private static ResourceBundle ToReadyBundle(FoodConversionPolicy policy, ResourceAmount ready) =>
        ResourceBundle.FromAmounts([KeyValuePair.Create(policy.ReadyFoodResourceId, ready)]);

    private static void EnsurePolicy(FoodConversionPolicy policy)
    {
        if (policy.ReadyFoodResourceId != UnifiedFoodInputs.ReadyFood ||
            policy.BreadResourceId != UnifiedFoodInputs.Bread)
            throw new ArgumentException("A v14 food migration requires the canonical Cibo and Pane identifiers.", nameof(policy));
        _ = UnifiedFoodInputs.CreatePolicy(policy.Recipes);
    }
}
