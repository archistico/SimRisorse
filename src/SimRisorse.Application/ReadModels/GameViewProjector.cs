using System.Collections.Immutable;
using SimRisorse.Core.Sessions;
using SimRisorse.Core.Economy;

namespace SimRisorse.Application.ReadModels;

internal static class GameViewProjector
{
    public static GameView Project(GameSession session)
    {
        var snapshot = session.Snapshot();
        var scenario = session.Scenario;
        var buildings = snapshot.Buildings.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        var forecast = session.ForecastProduction().ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        ImmutableArray<ResourceLine> Lines(ResourceBundle bundle) => bundle.Amounts.Select(item =>
            new ResourceLine(item.Key, scenario.Catalog.ResourcesById[item.Key].Name, item.Value.Units)).ToImmutableArray();
        decimal Flow(string resourceId, bool output) => scenario.Catalog.Buildings.Sum(item =>
            (item.Recipe is null ? 0m : (output ? item.Recipe.Output : item.Recipe.Input)[resourceId].Units) * forecast[item.Id].RunnableWorkers);
        return new(scenario.Id, scenario.Catalog.RulesVersion, session.ElapsedTicks, session.IsPaused,
            scenario.Population, session.AvailableWorkers,
            scenario.Catalog.Resources.Select(item => new ResourceView(item.Id, item.Name,
                snapshot.Resources[item.Id].Units, scenario.Capacity[item.Id].Units, Flow(item.Id, true), Flow(item.Id, false))).ToImmutableArray(),
            scenario.Catalog.Buildings.Select(item => new BuildingView(item.Id, item.Name, buildings[item.Id].Count,
                buildings[item.Id].AssignedWorkers, (long)buildings[item.Id].Count * item.WorkplacesPerBuilding,
                item.AllocationPriority, forecast[item.Id].RunnableWorkers, forecast[item.Id].BlockReason,
                Lines(item.Cost), Lines(item.Recipe?.Input ?? ResourceBundle.Empty), Lines(item.Recipe?.Output ?? ResourceBundle.Empty),
                item.Prerequisites.Where(id => buildings[id].Count == 0).Select(id => scenario.Catalog.BuildingsById[id].Name).ToImmutableArray())).ToImmutableArray());
    }
}
