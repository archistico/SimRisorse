using System.Collections.Immutable;
using SimRisorse.Core.Economy;

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
    ImmutableArray<ResourceLine> OutputPerWorker, ImmutableArray<string> MissingPrerequisites);
public sealed record GameView(string ScenarioId, string RulesVersion, long ElapsedTicks, bool IsPaused,
    int Population, int AvailableWorkers, ImmutableArray<ResourceView> Resources, ImmutableArray<BuildingView> Buildings);
