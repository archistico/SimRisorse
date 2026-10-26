using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Sessions;

/// <summary>Single-owner aggregate. Concurrent callers must use the application runtime.</summary>
public sealed class GameSession
{
    public const int SnapshotFormatVersion = 1;
    public static TimeSpan MaximumAdvance { get; } = TimeSpan.FromHours(24);
    private readonly Dictionary<string, BuildingState> _buildings;

    private GameSession(GameScenario scenario, GameSnapshot snapshot)
    {
        Scenario = scenario;
        Resources = snapshot.Resources;
        ElapsedTicks = snapshot.ElapsedTicks;
        _buildings = snapshot.Buildings.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
    }

    public GameScenario Scenario { get; }
    public ResourceBundle Resources { get; private set; }
    public long ElapsedTicks { get; private set; }
    public bool IsPaused { get; private set; } = true;
    public int AvailableWorkers => Scenario.Population - checked((int)_buildings.Values.Sum(item => (long)item.AssignedWorkers));

    public static GameSession Create(GameScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        return new(scenario, new(SnapshotFormatVersion, scenario.Id, scenario.Catalog.RulesVersion, 0,
            scenario.Population, scenario.InitialResources, scenario.InitialBuildings));
    }

    public static GameSession Restore(GameScenario scenario, GameSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.FormatVersion != SnapshotFormatVersion || snapshot.ScenarioId != scenario.Id ||
            snapshot.RulesVersion != scenario.Catalog.RulesVersion || snapshot.ElapsedTicks < 0 || snapshot.Population != scenario.Population)
            throw new ArgumentException("Snapshot identity, version, population or elapsed time is invalid.", nameof(snapshot));
        scenario.Catalog.ValidateBundle(snapshot.Resources);
        if (!scenario.Capacity.Covers(snapshot.Resources)) throw new ArgumentException("Snapshot exceeds storage capacity.", nameof(snapshot));
        GameScenario.ValidateBuildings(scenario.Catalog, snapshot.Buildings, snapshot.Population);
        if (snapshot.Buildings.Length != scenario.Catalog.Buildings.Length)
            throw new ArgumentException("Snapshot must contain every building type, including zero counts.", nameof(snapshot));
        return new(scenario, snapshot);
    }

    public GameSnapshot Snapshot() => new(SnapshotFormatVersion, Scenario.Id, Scenario.Catalog.RulesVersion,
        ElapsedTicks, Scenario.Population, Resources,
        Scenario.Catalog.Buildings.Select(item => _buildings[item.Id]).ToImmutableArray());

    public ImmutableArray<BuildingProduction> ForecastProduction() =>
        ProductionPlanner.Plan(Scenario.Catalog, Resources, Scenario.Capacity, _buildings).Buildings;

    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;

    public GameActionResult Construct(string buildingId, int count)
    {
        if (count <= 0) return new(GameError.InvalidQuantity);
        if (buildingId is null || !Scenario.Catalog.BuildingsById.TryGetValue(buildingId, out var definition)) return new(GameError.UnknownBuilding);
        if (definition.Prerequisites.Any(id => _buildings[id].Count == 0)) return new(GameError.MissingPrerequisite);
        ResourceBundle cost;
        int nextCount;
        try
        {
            cost = definition.Cost.Scale(count);
            nextCount = checked(_buildings[buildingId].Count + count);
        }
        catch (OverflowException) { return new(GameError.NumericLimit); }
        if (!Resources.Covers(cost)) return new(GameError.InsufficientResources);
        var nextResources = Resources - cost;
        _buildings[buildingId] = _buildings[buildingId] with { Count = nextCount };
        Resources = nextResources;
        return GameActionResult.Success;
    }

    public GameActionResult AssignWorkers(string buildingId, int count)
    {
        if (count <= 0) return new(GameError.InvalidQuantity);
        if (buildingId is null || !Scenario.Catalog.BuildingsById.TryGetValue(buildingId, out var definition)) return new(GameError.UnknownBuilding);
        var current = _buildings[buildingId];
        if ((long)current.AssignedWorkers + count > (long)current.Count * definition.WorkplacesPerBuilding)
            return new(GameError.InsufficientWorkplaces);
        if (count > AvailableWorkers) return new(GameError.InsufficientPopulation);
        _buildings[buildingId] = current with { AssignedWorkers = checked(current.AssignedWorkers + count) };
        return GameActionResult.Success;
    }

    public GameActionResult ReleaseWorkers(string buildingId, int count)
    {
        if (count <= 0) return new(GameError.InvalidQuantity);
        if (buildingId is null || !_buildings.TryGetValue(buildingId, out var current)) return new(GameError.UnknownBuilding);
        if (count > current.AssignedWorkers) return new(GameError.InsufficientAssignedWorkers);
        _buildings[buildingId] = current with { AssignedWorkers = current.AssignedWorkers - count };
        return GameActionResult.Success;
    }

    public GameActionResult Advance(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero) return new(GameError.InvalidElapsedTime);
        if (elapsed > MaximumAdvance) return new(GameError.AdvanceLimitExceeded);
        if (IsPaused) return GameActionResult.Success;
        long nextTicks;
        try { nextTicks = checked(ElapsedTicks + elapsed.Ticks); }
        catch (OverflowException) { return new(GameError.NumericLimit); }
        var steps = nextTicks / TimeSpan.TicksPerSecond - ElapsedTicks / TimeSpan.TicksPerSecond;
        for (long step = 0; step < steps; step++)
            Resources = ProductionPlanner.Plan(Scenario.Catalog, Resources, Scenario.Capacity, _buildings).Resources;
        ElapsedTicks = nextTicks;
        return GameActionResult.Success;
    }
}
