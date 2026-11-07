using System.Collections.Immutable;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Construction;

/// <summary>
/// One isolated hypothetical second using the REAL ProductionPlanner, with
/// explicitly modeled crew families. It does not mutate campaign state or saves.
/// RunnableWorkers in production diagnostics means runnable MODULES for opted-in
/// families; AssignedWorkers continues to mean individual inhabitants.
/// </summary>
public sealed record CrewProductionTick(ResourceBundle NextResources,
    ImmutableArray<BuildingProduction> Production,
    ImmutableArray<CrewProductionStatus> CrewStatus);

public sealed record CrewProductionStatus(string FamilyId, int OwnedModules, int AssignedWorkers,
    int StaffedModules, int RunnableModules, int IncompleteCrewWorkers,
    int MissingForNextModule, ResourceBundle Produced, ResourceBundle Consumed);

public static class CrewProductionProjection
{
    public static CrewProductionTick SimulateSecond(GameCatalog catalog, ResourceBundle resources,
        ResourceBundle capacity, IEnumerable<BuildingState> unchanged,
        IEnumerable<CrewTeamPortfolio> crewFamilies, int residents, int reservedOutsideRoster = 0)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(capacity);
        ArgumentNullException.ThrowIfNull(unchanged);
        ArgumentNullException.ThrowIfNull(crewFamilies);
        ArgumentOutOfRangeException.ThrowIfNegative(residents);
        ArgumentOutOfRangeException.ThrowIfNegative(reservedOutsideRoster);
        catalog.ValidateBundle(resources);
        catalog.ValidateBundle(capacity);
        if (!capacity.Covers(resources))
            throw new ArgumentException("City stock exceeds declared capacity.", nameof(resources));

        var roster = new Dictionary<string, BuildingState>(StringComparer.Ordinal);
        long assigned = reservedOutsideRoster;
        foreach (var state in unchanged)
        {
            ArgumentNullException.ThrowIfNull(state);
            if (!catalog.BuildingsById.TryGetValue(state.BuildingId, out var definition) ||
                !roster.TryAdd(state.BuildingId, state))
                throw new ArgumentException("Unknown or duplicate building state.", nameof(unchanged));
            if (state.Count < 0 || state.AssignedWorkers < 0 ||
                (long)state.AssignedWorkers > (long)state.Count * definition.WorkplacesPerBuilding)
                throw new ArgumentException("Invalid standard building workforce.", nameof(unchanged));
            assigned += state.AssignedWorkers;
        }

        var portfolios = new Dictionary<string, CrewTeamPortfolio>(StringComparer.Ordinal);
        var crewSizes = new Dictionary<string, int>(StringComparer.Ordinal);
        var staffedModules = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var crew in crewFamilies)
        {
            ArgumentNullException.ThrowIfNull(crew);
            if (!catalog.BuildingsById.TryGetValue(crew.FamilyId, out var definition) ||
                definition.Recipe is null || definition.WorkplacesPerBuilding != 1 || definition.IsScenarioFixed ||
                !portfolios.TryAdd(crew.FamilyId, crew) || roster.ContainsKey(crew.FamilyId))
                throw new ArgumentException("Invalid, duplicate or overlapping crew family.", nameof(crewFamilies));
            crewSizes.Add(crew.FamilyId, crew.CrewSize);
            if (crew.CrewSize > 1) staffedModules.Add(crew.FamilyId, crew.StaffedModules);
            // The planner receives the number of productive modules as its
            // Count; persons are NOT silently converted to per-worker recipes.
            roster.Add(crew.FamilyId, new BuildingState(crew.FamilyId, crew.TotalModules, crew.AssignedWorkers));
            assigned += crew.AssignedWorkers;
        }
        if (roster.Count != catalog.Buildings.Length)
            throw new ArgumentException("Roster must cover every catalog building exactly once.", nameof(unchanged));
        if (assigned > residents)
            throw new InvalidOperationException("Insufficient population for all assigned and reserved workers.");

        var step = ProductionPlanner.Plan(catalog, resources, capacity, roster,
            includeDiagnostics: true, crewSizes: crewSizes, staffedModules: staffedModules);
        var statuses = ImmutableArray.CreateBuilder<CrewProductionStatus>(portfolios.Count);
        foreach (var (id, crew) in portfolios.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            var report = step.Buildings.Single(item => item.BuildingId == id);
            var recipe = catalog.BuildingsById[id].Recipe!;
            statuses.Add(new(id, crew.TotalModules, crew.AssignedWorkers, crew.StaffedModules,
                report.RunnableWorkers, crew.IncompleteCrewWorkers, crew.MissingForNextModule,
                recipe.Output.Scale(report.RunnableWorkers), recipe.Input.Scale(report.RunnableWorkers)));
        }
        return new(step.Resources, step.Buildings, statuses.MoveToImmutable());
    }
}
