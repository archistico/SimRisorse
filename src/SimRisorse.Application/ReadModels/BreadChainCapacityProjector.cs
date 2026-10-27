using System.Collections.Immutable;

namespace SimRisorse.Application.ReadModels;

/// <summary>
/// Estimates an upper bound on average bread throughput from assigned workers,
/// recursively following the unique single-output recipe for each ingredient.
/// Stocks, other consumers, storage limits and production timing are excluded.
/// Unsupported or ambiguous recipe networks produce no estimate.
/// </summary>
internal static class BreadChainCapacityProjector
{
    public static BreadChainCapacityView? Project(string breadResourceId, decimal requiredBreadPerSecond,
        IReadOnlyList<BuildingView> buildings)
    {
        if (requiredBreadPerSecond <= 0m) return null;

        var producers = new Dictionary<string, BuildingView>(StringComparer.Ordinal);
        var demands = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var recursionPath = new HashSet<string>(StringComparer.Ordinal);

        bool Accumulate(string resourceId, decimal needed)
        {
            if (!recursionPath.Add(resourceId)) return false;

            var matching = buildings.Where(item => item.OutputPerWorker.Any(output =>
                output.ResourceId == resourceId)).ToArray();
            // Alternative recipes and co-products need an allocation model; do
            // not present a misleading theoretical bound for those scenarios.
            if (matching.Length != 1 || matching[0].OutputPerWorker.Length != 1)
                return false;

            var producer = matching[0];
            var output = producer.OutputPerWorker[0];
            if (output.Quantity <= 0m) return false;

            producers[resourceId] = producer;
            demands[resourceId] = demands.GetValueOrDefault(resourceId) + needed;
            var assignedWorkerEquivalent = needed / output.Quantity;
            foreach (var input in producer.InputPerWorker)
            {
                if (!Accumulate(input.ResourceId, assignedWorkerEquivalent * input.Quantity))
                    return false;
            }

            recursionPath.Remove(resourceId);
            return true;
        }

        if (!Accumulate(breadResourceId, requiredBreadPerSecond)) return null;

        var stages = ImmutableArray.CreateBuilder<BreadChainStageView>(demands.Count);
        foreach (var (resourceId, required) in demands.OrderBy(item => producers[item.Key].AllocationPriority))
        {
            var producer = producers[resourceId];
            var output = producer.OutputPerWorker[0];
            var minimumWorkers = decimal.Ceiling(required / output.Quantity);
            if (minimumWorkers > int.MaxValue) return null;

            var nominal = producer.AssignedWorkers * output.Quantity;
            stages.Add(new(resourceId, output.Name, producer.Id, producer.Name,
                required, nominal, decimal.Max(0m, required - nominal),
                nominal * requiredBreadPerSecond / required,
                producer.AssignedWorkers, (int)minimumWorkers,
                Math.Max(0, (int)minimumWorkers - producer.AssignedWorkers)));
        }

        if (stages.Count == 0) return null;
        var capacity = stages.Min(item => item.BreadEquivalentCapacityPerSecond);
        var limiting = stages.Where(item => item.BreadEquivalentCapacityPerSecond == capacity)
            .Select(item => item.ResourceId).ToImmutableArray();
        return new(capacity, stages.MoveToImmutable(), limiting);
    }
}
