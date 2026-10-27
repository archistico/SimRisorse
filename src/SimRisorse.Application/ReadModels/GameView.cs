using System.Collections.Immutable;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;

namespace SimRisorse.Application.ReadModels;

public sealed record ResourceView(string Id, string Name, decimal Quantity, decimal Capacity,
    decimal ProductionPerSecond, decimal ConsumptionPerSecond)
{
    public decimal NetPerSecond => ProductionPerSecond - ConsumptionPerSecond;
}
public sealed record ResourceLine(string ResourceId, string Name, decimal Quantity);
public sealed record BuildingView(string Id, string Name, int Count, int AssignedWorkers, long Workplaces,
    int AllocationPriority, int RunnableWorkers, ProductionBlockReason BlockReason,
    ImmutableArray<ResourceLine> Cost, ImmutableArray<ResourceLine> InputPerWorker,
    ImmutableArray<ResourceLine> OutputPerWorker, ImmutableArray<string> MissingPrerequisites, int HousingPerBuilding,
    ImmutableArray<ResourceLine> StoragePerBuilding, ImmutableArray<string> MissingInputNames,
    ImmutableArray<string> FullOutputNames);
public sealed record PopulationView(int HousingCapacity, int ProgressSeconds, int IntervalSeconds,
    string? FoodResourceId, decimal RequiredFoodPerSecond, decimal ConsumedFoodPerSecond, GrowthBlockReason BlockReason,
    int BasicHousingCapacity, int AdvancedResidents, string? BreadResourceId,
    decimal RequiredBreadPerSecond, decimal ConsumedBreadPerSecond,
    int GrowthRemainderTwentieths = 0, int GrowthRateTwentieths = HousingGrowthRules.UnitsPerSecond);

/// <summary>UI-independent explanation of the next-second supply for the additional bread ration.</summary>
public enum BreadSupplyState
{
    Hidden, FirstAdvancedArrivalNeedsStock, NominalCapacityInsufficient, SufficientNextSecond,
    MissingInputs, StorageFull, LimitedNextSecond
}

/// <summary>
/// Demand and nominal supply for one link of a bread recipe chain.
/// Worker counts are theoretical average-capacity estimates, not a build order.
/// </summary>
public sealed record BreadChainStageView(string ResourceId, string ResourceName,
    string ProducerBuildingId, string ProducerName, decimal RequiredResourcePerSecond,
    decimal NominalResourcePerSecond, decimal MissingResourcePerSecond,
    decimal BreadEquivalentCapacityPerSecond, int AssignedWorkers, int MinimumWorkers,
    int AdditionalWorkers);

/// <summary>
/// Theoretical average upper bound of a uniquely determined bread recipe chain.
/// It is not the next-second forecast or a guarantee of uninterrupted output.
/// </summary>
public sealed record BreadChainCapacityView(decimal BreadEquivalentCapacityPerSecond,
    ImmutableArray<BreadChainStageView> Stages, ImmutableArray<string> LimitingResourceIds);

/// <summary>
/// Structured bread diagnosis, independent of Desktop presentation.
/// Nominal capacity counts assigned workers even when inputs or storage block them.
/// NextSecondProductionPerSecond includes those actual constraints.
/// Single-recipe worker estimates and the chain's average bound are informational.
/// </summary>
public sealed record BreadSupplyView(BreadSupplyState State, decimal? ReserveSecondsWithoutProduction,
    decimal RequiredPerSecond = 0m, decimal NominalProductionPerSecond = 0m,
    decimal NextSecondProductionPerSecond = 0m, decimal NominalShortfallPerSecond = 0m,
    int? MinimumAssignedWorkers = null, int? AdditionalAssignedWorkers = null,
    BreadChainCapacityView? ChainCapacity = null);

public sealed record GameView(string ScenarioId, string RulesVersion, long ElapsedTicks, bool IsPaused,
    int Population, int AvailableWorkers, ImmutableArray<ResourceView> Resources, ImmutableArray<BuildingView> Buildings,
    PopulationView PopulationState, BreadSupplyView BreadSupply);
