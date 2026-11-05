using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Core.Food;

/// <summary>
/// RC69: the v15 city is the authoritative civic root. The depot and
/// intermediate stocks are committed with it as one immutable checkpoint.
/// Legacy DTOs are accepted only by the explicit import/migration boundary.
/// </summary>
public sealed record V15FoodCampaignCheckpoint(V15CityCampaignSnapshot CityCampaign,
    FoodDepotStock Depot, FoodIndustryCampaignState Industry)
{
    // Read-only compatibility alias for existing v15 read models. It is the
    // SAME object, never another mutable/serialized civic state.
    public V15CityCampaignSnapshot NativeCityCampaign => CityCampaign;

    // Existing two-argument callers create an empty food industry.
    public V15FoodCampaignCheckpoint(V15CityCampaignSnapshot cityCampaign, FoodDepotStock depot)
        : this(cityCampaign, depot, FoodIndustryCampaignState.Empty) { }
}

public static class V15FoodCampaignCheckpoints
{
    public const int FileVersion = 6;
    public const string RulesVersion = "civil-growth-15";

    /// <summary>Pure in-memory preparation; does not open or write SQLite.</summary>
    public static V15FoodCampaignCheckpoint FromMigrationPlan(V14FoodStockMigrationPlan plan,
        FoodConversionPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(policy);
        _ = V14FoodStockMigration.RestoreLegacyResources(plan, policy);
        return new(V15CityCampaignSnapshot.FromInherited(V15LegacyProducerAdapter.WithoutEmptySlot(
                plan.SourceCheckpoint with { Resources = plan.CityAfterTransfer })),
            plan.DepotAfterTransfer);
    }

    /// <summary>Validate the native v15 city and the complete food checkpoint.
    /// Over-capacity depot stock is explicitly allowed to protect old inventory.
    /// Fish and bread can be held in city storage while awaiting depot delivery,
    /// and in depot raw storage after delivery; neither stock is double-counted.
    /// Prepared food is owned only by the depot, and intermediate meat/fruit/
    /// vegetables and animals are owned by their dedicated industry inventories.
    /// </summary>
    public static void Validate(V15FoodCampaignCheckpoint checkpoint, GameScenario baseline,
        SchoolResearchRules school, TechnologyCatalog technologies, FoodConversionPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(school);
        ArgumentNullException.ThrowIfNull(technologies);
        ArgumentNullException.ThrowIfNull(policy);
        if (checkpoint.CityCampaign is not { Resources: not null } ||
            checkpoint.Depot is not { RawFood: not null } || checkpoint.Industry is null)
            throw new ArgumentException("The food checkpoint is incomplete.", nameof(checkpoint));
        if (school.FoodResourceId != policy.ReadyFoodResourceId)
            throw new ArgumentException("The v15 school must use prepared food owned by the depot.", nameof(school));
        if (policy.ReadyFoodResourceId != UnifiedFoodInputs.ReadyFood ||
            policy.BreadResourceId != UnifiedFoodInputs.Bread)
            throw new ArgumentException("Unknown food policy identifiers.", nameof(policy));
        // Refuse missing/extra recipes as well as invalid foreign raw-food IDs.
        _ = UnifiedFoodInputs.CreatePolicy(policy.Recipes);
        var edibleIds = policy.Recipes.Select(r => r.SourceResourceId)
            .Append(policy.ReadyFoodResourceId).ToHashSet(StringComparer.Ordinal);
        // Fish and bread are produced in the v15 city and are transferred
        // to depot bins only when there is capacity. Both bins may legitimately
        // contain those resources at once, representing different physical stock.
        // Never permit prepared food in city storage: its sole owner is the depot.
        if (checkpoint.CityCampaign.Resources[policy.ReadyFoodResourceId] != ResourceAmount.Zero ||
            checkpoint.Depot.RawFood.Amounts.Keys.Any(id => id == policy.ReadyFoodResourceId || !edibleIds.Contains(id)))
            throw new ArgumentException("Ready food must be owned by the depot and its raw stock must use valid ingredient IDs.", nameof(checkpoint));
        // Validate the live 15-family city against v15 capacity/workforce rules.
        // Never manufacture a v14 food-store slot for ordinary validation.
        V15CityCampaignValidator.ValidateNative(baseline, checkpoint.CityCampaign, school, technologies);
        // Food-industry staff share the same population budget as every v15
        // productive family, civilian building, school and enlisted military person.
        var city = checkpoint.CityCampaign;
        checkpoint.Industry.Validate(city.Population, BusyOutsideFood(city));
        if (city.Resources.Amounts.Keys.Any(id => id is "cattle" or "game" or "meat" or "fruit" or "vegetables"))
            throw new ArgumentException("Food intermediates cannot be stored in the v15 city.", nameof(checkpoint));
    }

    /// <summary>Free residents after every v15 commitment: civic and productive
    /// workers, school reservations, enlisted military and the six food-industry
    /// crews. Read-only projection shared by gameplay and progression benchmarks.
    /// </summary>
    public static long AvailableWorkers(V15FoodCampaignCheckpoint checkpoint)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        return checked((long)checkpoint.CityCampaign.Population -
            BusyOutsideFood(checkpoint.CityCampaign) -
            checkpoint.Industry.Buildings.Assigned.Total);
    }

    internal static long BusyOutsideFood(V15CityCampaignSnapshot city) => checked(
        city.OtherBuildings.Sum(b => (long)b.AssignedWorkers) +
        city.ProductiveFamilies.Sum(f => (long)f.Base.AssignedWorkers +
            f.Large.AssignedWorkers + f.Advanced.AssignedWorkers) +
        city.Military.EnlistedPopulation + city.SchoolResearch.ReservedPopulation);

    /// <summary>Reverse projection for compatibility checks (never writes to v14).
    /// Only the historic food types can be represented by a v14 snapshot.
    /// This projection is diagnostic, not a v14 writer; the restored stock
    /// may no longer fit v14 capacities after new v15 production begins.
    /// </summary>
    public static V14CampaignSnapshot ToLegacyPreview(V15FoodCampaignCheckpoint checkpoint,
        GameScenario baseline, SchoolResearchRules school,
        TechnologyCatalog technologies, FoodConversionPolicy policy)
    {
        Validate(checkpoint, baseline, school, technologies, policy);
        if (!checkpoint.Industry.IsEmpty)
            throw new InvalidOperationException("A campaign with active food industries cannot be projected to v14.");
        var historicFood = ResourceBundle.FromAmounts(checkpoint.Depot.RawFood.Amounts
            .Where(kv => baseline.Catalog.ResourcesById.ContainsKey(kv.Key)));
        return V15LegacyProducerAdapter.ForV14Rules(checkpoint.CityCampaign.ToInherited()) with
        {
            Resources = checkpoint.CityCampaign.Resources + historicFood + ResourceBundle.FromAmounts(
                [KeyValuePair.Create(policy.ReadyFoodResourceId, checkpoint.Depot.ReadyFood)])
        };
    }
}
