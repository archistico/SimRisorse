using System.Collections.Immutable;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.ReadModels;

/// <summary>
/// Adapts the v14 tiered building ledger to the established v13 bread diagnostic.
/// Uses the actual v14 next-second forecast rather than inferring supply from stock.
/// Read-only; does not migrate or change either campaign.
/// </summary>
public static class V14BreadSupplyProjector
{
    public static BreadSupplyView Project(GameScenario baseline, V14CampaignSnapshot snapshot,
        V14EconomyPreview forecast, IReadOnlyList<ResourceView> resources)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(forecast);
        ArgumentNullException.ThrowIfNull(resources);

        var other = snapshot.OtherBuildings.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        var families = snapshot.ProductiveFamilies.ToDictionary(item => item.FamilyId, StringComparer.Ordinal);
        var production = forecast.Buildings.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        ImmutableArray<ResourceLine> Lines(ResourceBundle bundle) => bundle.Amounts.Select(item =>
            new ResourceLine(item.Key, baseline.Catalog.ResourcesById[item.Key].Name, item.Value.Units)).ToImmutableArray();
        ImmutableArray<string> Names(ImmutableArray<string> ids) => ids.Select(id =>
            baseline.Catalog.ResourcesById[id].Name).ToImmutableArray();

        var buildings = baseline.Catalog.Buildings.Select(definition =>
        {
            int count, assigned;
            if (families.TryGetValue(definition.Id, out var family))
            {
                count = checked(family.Base.Buildings + family.Large.Buildings + family.Advanced.Buildings);
                assigned = checked(family.Base.AssignedWorkers + family.Large.AssignedWorkers + family.Advanced.AssignedWorkers);
            }
            else
            {
                var building = other[definition.Id];
                count = building.Count;
                assigned = building.AssignedWorkers;
            }
            var actual = production[definition.Id];
            return new BuildingView(definition.Id, definition.Name, count, assigned,
                // For the bread chain we use assigned workers; tier capacity is
                // reflected by the count/assignments and forecast, not by this label.
                (long)count * definition.WorkplacesPerBuilding,
                definition.AllocationPriority, actual.RunnableWorkers, actual.BlockReason,
                Lines(definition.Cost), Lines(definition.Recipe?.Input ?? ResourceBundle.Empty),
                Lines(definition.Recipe?.Output ?? ResourceBundle.Empty),
                definition.Prerequisites.Where(id =>
                    (other.TryGetValue(id, out var prior) ? prior.Count :
                     families.TryGetValue(id, out var priorFamily) ?
                         priorFamily.Base.Buildings + priorFamily.Large.Buildings + priorFamily.Advanced.Buildings : 0) == 0)
                    .Select(id => baseline.Catalog.BuildingsById[id].Name).ToImmutableArray(),
                definition.HousingPerBuilding, Lines(definition.StoragePerBuilding ?? ResourceBundle.Empty),
                Names(actual.MissingInputIds), Names(actual.FullOutputIds), definition.IsScenarioFixed);
        }).ToImmutableArray();

        return BreadSupplyProjector.Project(baseline, snapshot.Population, forecast.Population, resources, buildings);
    }
}
