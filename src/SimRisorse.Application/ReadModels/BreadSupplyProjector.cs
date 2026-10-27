using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.ReadModels;

/// <summary>
/// The UI should display a stable supply state and not reproduce the production
/// and population calculations. A shortage before the first advanced arrival
/// must remain visible even when missing basic food takes precedence in growth.
/// </summary>
internal static class BreadSupplyProjector
{
    public static BreadSupplyView Project(GameSession session, PopulationForecast population,
        IReadOnlyList<ResourceView> resources, IReadOnlyList<BuildingView> buildings)
    {
        if (session.Scenario.PopulationRules?.AdvancedHousing is not { } rules ||
            population.BreadResourceId is not { } breadId)
            return new(BreadSupplyState.Hidden, null);

        var bread = resources.Single(item => item.Id == breadId);
        var firstAdvancedArrival = population.AdvancedResidents == 0 &&
            session.Population < population.HousingCapacity &&
            (long)session.Population + 1 > population.BasicHousingCapacity;
        var firstArrivalDemand = firstAdvancedArrival
            ? rules.BreadPerPersonPerSecond.Units * ((long)session.Population + 1 - population.BasicHousingCapacity)
            : 0m;
        var stockRequiredForFirstArrival = firstAdvancedArrival && bread.Quantity < firstArrivalDemand;

        if (population.AdvancedResidents == 0)
            return new(stockRequiredForFirstArrival ? BreadSupplyState.FirstAdvancedArrivalNeedsStock : BreadSupplyState.Hidden, null);

        var required = population.RequiredBread.Units;
        // Include unstaffed definitions: the first bakery still has a known output
        // rate, so a shortage can be expressed in workers even with zero bakeries.
        var producerTypes = buildings.Where(item =>
            item.OutputPerWorker.Any(output => output.ResourceId == breadId)).ToArray();
        var nominal = producerTypes.Sum(item => item.AssignedWorkers *
            item.OutputPerWorker.Single(output => output.ResourceId == breadId).Quantity);
        var shortfall = decimal.Max(0m, required - nominal);

        int? minimumWorkers = null;
        int? additionalWorkers = null;
        if (producerTypes.Length == 1)
        {
            var breadPerWorker = producerTypes[0].OutputPerWorker.Single(output => output.ResourceId == breadId).Quantity;
            if (breadPerWorker > 0m)
            {
                var estimate = decimal.Ceiling(required / breadPerWorker);
                if (estimate <= int.MaxValue)
                {
                    minimumWorkers = (int)estimate;
                    additionalWorkers = Math.Max(0, minimumWorkers.Value - producerTypes[0].AssignedWorkers);
                }
            }
        }

        var staffedProducers = producerTypes.Where(item => item.AssignedWorkers > 0).ToArray();
        var state = nominal < required ? BreadSupplyState.NominalCapacityInsufficient :
            bread.ProductionPerSecond >= required ? BreadSupplyState.SufficientNextSecond :
            staffedProducers.Any(item => item.BlockReason.HasFlag(ProductionBlockReason.MissingInputs))
                ? BreadSupplyState.MissingInputs :
            staffedProducers.Any(item => item.BlockReason.HasFlag(ProductionBlockReason.StorageFull))
                ? BreadSupplyState.StorageFull : BreadSupplyState.LimitedNextSecond;
        return new(state, decimal.Floor(bread.Quantity / required), required, nominal,
            bread.ProductionPerSecond, shortfall, minimumWorkers, additionalWorkers,
            BreadChainCapacityProjector.Project(breadId, required, buildings));
    }
}
