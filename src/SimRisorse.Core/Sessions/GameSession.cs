using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Population;

namespace SimRisorse.Core.Sessions;

/// <summary>Single-owner aggregate. Concurrent callers must use the application runtime.</summary>
public sealed class GameSession
{
    public const int SnapshotFormatVersion = 2;
    public static bool SupportsSnapshotFormat(int version) => version is 1 or SnapshotFormatVersion;
    public static TimeSpan MaximumAdvance { get; } = TimeSpan.FromHours(24);
    private readonly Dictionary<string, BuildingState> _buildings;

    private GameSession(GameScenario scenario, GameSnapshot snapshot)
    {
        Scenario = scenario;
        Resources = snapshot.Resources;
        ElapsedTicks = snapshot.ElapsedTicks;
        Population = snapshot.Population;
        GrowthProgressSeconds = snapshot.GrowthProgressSeconds ?? 0;
        GrowthRemainderTwentieths = snapshot.GrowthRemainderTwentieths;
        MilitaryState = snapshot.Military ?? MilitaryTrainingState.Empty;
        _buildings = snapshot.Buildings.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
    }

    public GameScenario Scenario { get; }
    public ResourceBundle Resources { get; private set; }
    public long ElapsedTicks { get; private set; }
    public bool IsPaused { get; private set; } = true;
    public int Population { get; private set; }
    public int GrowthProgressSeconds { get; private set; }
    public int GrowthRemainderTwentieths { get; private set; }
    public MilitaryTrainingState MilitaryState { get; private set; }
    public int HousingCapacity => Scenario.HousingCapacity(_buildings.Values);
    public ResourceBundle StorageCapacity => Scenario.StorageCapacity(_buildings.Values);
    public int AvailableWorkers => Population - checked((int)(_buildings.Values.Sum(item => (long)item.AssignedWorkers) +
        MilitaryState.EnlistedPopulation));

    public static GameSession Create(GameScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        return new(scenario, new(scenario.PopulationRules is null ? 1 : SnapshotFormatVersion, scenario.Id, scenario.Catalog.RulesVersion, 0,
            scenario.Population, scenario.InitialResources, scenario.InitialBuildings, scenario.PopulationRules is null ? null : 0,
            Military: scenario.MilitaryRules is null ? null :
                scenario.MilitaryRules.IndividualTraining ? MilitaryTrainingState.IndividualEmpty : MilitaryTrainingState.Empty));
    }

    public static GameSession Restore(GameScenario scenario, GameSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.FormatVersion != (scenario.PopulationRules is null ? 1 : SnapshotFormatVersion) || snapshot.ScenarioId != scenario.Id ||
            snapshot.RulesVersion != scenario.Catalog.RulesVersion || snapshot.ElapsedTicks < 0 || snapshot.Population < scenario.Population)
            throw new ArgumentException("Snapshot identity, version, population or elapsed time is invalid.", nameof(snapshot));
        scenario.Catalog.ValidateBundle(snapshot.Resources);
        GameScenario.ValidateBuildings(scenario.Catalog, snapshot.Buildings, snapshot.Population);
        if (snapshot.Buildings.Length != scenario.Catalog.Buildings.Length)
            throw new ArgumentException("Snapshot must contain every building type, including zero counts.", nameof(snapshot));
        foreach (var fixedBuilding in scenario.Catalog.Buildings.Where(item => item.IsScenarioFixed))
        {
            var expected = scenario.InitialBuildings.Single(item => item.BuildingId == fixedBuilding.Id);
            var actual = snapshot.Buildings.Single(item => item.BuildingId == fixedBuilding.Id);
            if (actual != expected)
                throw new ArgumentException("A fixed starting structure cannot be altered.", nameof(snapshot));
        }
        if (scenario.MilitaryRules is { } militaryRules)
        {
            if (snapshot.Military is null)
                throw new ArgumentException("Military state is required for this rules version.", nameof(snapshot));
            MilitaryTraining.Validate(snapshot.Military, militaryRules,
                snapshot.Buildings.Single(item => item.BuildingId == militaryRules.BarracksId).Count);
            if (snapshot.Buildings.Sum(item => (long)item.AssignedWorkers) + snapshot.Military.EnlistedPopulation > snapshot.Population)
                throw new ArgumentException("Civilian and military assignments exceed population.", nameof(snapshot));
        }
        else if (snapshot.Military is not null)
            throw new ArgumentException("Historical rules must not contain military state.", nameof(snapshot));
        ResourceBundle capacity;
        try { capacity = scenario.StorageCapacity(snapshot.Buildings); }
        catch (OverflowException exception) { throw new ArgumentException("Snapshot storage exceeds the numeric limit.", nameof(snapshot), exception); }
        if (!capacity.Covers(snapshot.Resources)) throw new ArgumentException("Snapshot exceeds storage capacity.", nameof(snapshot));
        if (scenario.PopulationRules is { } rules)
        {
            if (snapshot.Population > scenario.HousingCapacity(snapshot.Buildings) || snapshot.GrowthProgressSeconds is not { } progress ||
                progress < 0 || progress >= rules.GrowthIntervalSeconds ||
                snapshot.GrowthRemainderTwentieths is < 0 or >= HousingGrowthRules.UnitsPerSecond ||
                (rules.HousingGrowth is null && snapshot.GrowthRemainderTwentieths != 0))
                throw new ArgumentException("Invalid population or growth progress.", nameof(snapshot));
        }
        else if (snapshot.Population != scenario.Population || snapshot.GrowthProgressSeconds is not null || snapshot.GrowthRemainderTwentieths != 0)
            throw new ArgumentException("Fixed population snapshots cannot contain growth.", nameof(snapshot));
        return new(scenario, snapshot);
    }

    public GameSnapshot Snapshot() => new(Scenario.PopulationRules is null ? 1 : SnapshotFormatVersion, Scenario.Id, Scenario.Catalog.RulesVersion,
        ElapsedTicks, Population, Resources,
        Scenario.Catalog.Buildings.Select(item => _buildings[item.Id]).ToImmutableArray(),
        Scenario.PopulationRules is null ? null : GrowthProgressSeconds, GrowthRemainderTwentieths,
        Scenario.MilitaryRules is null ? null : MilitaryState);

    private static GameActionResult ResultOf(RecruitmentFailure failure) => new(failure switch
    {
        RecruitmentFailure.None => GameError.None,
        RecruitmentFailure.InvalidQuantity => GameError.InvalidQuantity,
        RecruitmentFailure.MissingBarracks => GameError.MissingBarracks,
        RecruitmentFailure.InsufficientPopulation => GameError.InsufficientPopulation,
        RecruitmentFailure.InsufficientSwords => GameError.InsufficientSwords,
        RecruitmentFailure.InsufficientSoldiers => GameError.InsufficientSoldiers,
        RecruitmentFailure.InsufficientQueuedRecruits => GameError.InsufficientQueuedRecruits,
        RecruitmentFailure.InsufficientStorage => GameError.InsufficientStorage,
        RecruitmentFailure.DischargeUnavailable => GameError.DischargeUnavailable,
        _ => GameError.NumericLimit
    });

    private GameActionResult ApplyMilitary(RecruitmentAttempt attempt)
    {
        if (!attempt.Succeeded) return ResultOf(attempt.Failure);
        Resources = attempt.Resources;
        MilitaryState = attempt.State;
        return GameActionResult.Success;
    }

    public GameActionResult Enlist(int count)
    {
        if (Scenario.MilitaryRules is not { } rules) return new(GameError.MissingBarracks);
        return ApplyMilitary(MilitaryTraining.Enlist(rules, MilitaryState, Resources, Population,
            checked((int)_buildings.Values.Sum(item => (long)item.AssignedWorkers)),
            _buildings[rules.BarracksId].Count, count));
    }

    public GameActionResult DischargeSoldiers(int count)
    {
        if (Scenario.MilitaryRules is not { } rules) return new(GameError.MissingBarracks);
        if (rules.IndividualTraining) return new(GameError.DischargeUnavailable);
        return ApplyMilitary(MilitaryTraining.Discharge(rules, MilitaryState, Resources,
            _buildings[rules.BarracksId].Count, count));
    }

    public GameActionResult CancelQueuedRecruits(int count)
    {
        if (Scenario.MilitaryRules is not { } rules) return new(GameError.MissingBarracks);
        return ApplyMilitary(MilitaryTraining.CancelWaiting(rules, MilitaryState, Resources,
            StorageCapacity, _buildings[rules.BarracksId].Count, count));
    }

    private PopulationForecast PopulationPlan() => PopulationPlanner.Plan(Scenario.PopulationRules, Population, HousingCapacity,
        Scenario.BasicHousingCapacity(_buildings.Values), GrowthProgressSeconds, GrowthRemainderTwentieths,
        Scenario.PopulationRules?.HousingGrowth is { } growth
            ? growth.Rate(_buildings[growth.SmallHouseId].Count, _buildings[growth.LargeHouseId].Count)
            : HousingGrowthRules.UnitsPerSecond, Resources);

    public EconomyForecast ForecastEconomy()
    {
        var population = PopulationPlan();
        var production = ProductionPlanner.Plan(Scenario.Catalog, Resources, StorageCapacity, _buildings,
            PopulationPlanner.Consumption(population), includeDiagnostics: true);
        return new(production.Buildings, population);
    }

    public ImmutableArray<BuildingProduction> ForecastProduction() => ForecastEconomy().Buildings;

    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;

    public GameActionResult Construct(string buildingId, int count)
    {
        if (count <= 0) return new(GameError.InvalidQuantity);
        if (buildingId is null || !Scenario.Catalog.BuildingsById.TryGetValue(buildingId, out var definition)) return new(GameError.UnknownBuilding);
        if (definition.IsScenarioFixed) return new(GameError.FixedBuilding);
        if (definition.Prerequisites.Any(id => _buildings[id].Count == 0)) return new(GameError.MissingPrerequisite);
        if (Scenario.PopulationRules is not null && (long)count * definition.HousingPerBuilding > int.MaxValue - HousingCapacity)
            return new(GameError.NumericLimit);
        ResourceBundle cost;
        int nextCount;
        try
        {
            cost = definition.Cost.Scale(count);
            nextCount = checked(_buildings[buildingId].Count + count);
            if (definition.StoragePerBuilding is { } storage) _ = StorageCapacity + storage.Scale(count);
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
        var capacity = StorageCapacity;
        for (long step = 0; step < steps; step++)
        {
            var population = PopulationPlan();
            Resources = ProductionPlanner.Plan(Scenario.Catalog, Resources, capacity, _buildings, PopulationPlanner.Consumption(population)).Resources;
            if (population.BlockReason == GrowthBlockReason.None)
            {
                var accrued = (long)GrowthProgressSeconds * HousingGrowthRules.UnitsPerSecond +
                    GrowthRemainderTwentieths + population.GrowthRateTwentieths;
                var threshold = (long)population.IntervalSeconds * HousingGrowthRules.UnitsPerSecond;
                if (accrued >= threshold)
                {
                    Population++;
                    accrued -= threshold;
                }
                GrowthProgressSeconds = (int)(accrued / HousingGrowthRules.UnitsPerSecond);
                GrowthRemainderTwentieths = (int)(accrued % HousingGrowthRules.UnitsPerSecond);
            }
            if (Scenario.MilitaryRules is { } military)
                MilitaryState = MilitaryTraining.Advance(military, MilitaryState,
                    _buildings[military.BarracksId].Count, 1);
        }
        ElapsedTicks = nextTicks;
        return GameActionResult.Success;
    }
}
