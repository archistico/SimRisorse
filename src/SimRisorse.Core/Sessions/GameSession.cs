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
        _buildings = snapshot.Buildings.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        Military = snapshot.Military;
    }

    public GameScenario Scenario { get; }
    public ResourceBundle Resources { get; private set; }
    public long ElapsedTicks { get; private set; }
    public bool IsPaused { get; private set; } = true;
    public int Population { get; private set; }
    public int GrowthProgressSeconds { get; private set; }
    public int GrowthRemainderTwentieths { get; private set; }
    public int HousingCapacity => Scenario.HousingCapacity(_buildings.Values);
    public ResourceBundle StorageCapacity => Scenario.StorageCapacity(_buildings.Values);
    /// <summary>Recruits and soldiers; null when the scenario has no military rules.</summary>
    public MilitaryState? Military { get; private set; }
    /// <summary>Civilians neither employed nor enlisted.</summary>
    public int AvailableWorkers => checked((int)(Population - _buildings.Values.Sum(item => (long)item.AssignedWorkers) - (Military?.People ?? 0)));
    /// <summary>Recruits that the owned barracks can train at the same time.</summary>
    public int TrainingPlaces => Scenario.MilitaryRules is { } rules
        ? checked(_buildings[rules.BarracksBuildingId].Count * rules.RecruitsPerBarracks) : 0;

    public static GameSession Create(GameScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        return new(scenario, new(scenario.PopulationRules is null ? 1 : SnapshotFormatVersion, scenario.Id, scenario.Catalog.RulesVersion, 0,
            scenario.Population, scenario.InitialResources, scenario.InitialBuildings, scenario.PopulationRules is null ? null : 0,
            Military: scenario.MilitaryRules is null ? null : MilitaryState.Empty));
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
        ValidateMilitary(scenario, snapshot);
        return new(scenario, snapshot);
    }

    public GameSnapshot Snapshot() => new(Scenario.PopulationRules is null ? 1 : SnapshotFormatVersion, Scenario.Id, Scenario.Catalog.RulesVersion,
        ElapsedTicks, Population, Resources,
        Scenario.Catalog.Buildings.Select(item => _buildings[item.Id]).ToImmutableArray(),
        Scenario.PopulationRules is null ? null : GrowthProgressSeconds, GrowthRemainderTwentieths, Military);

    /// <summary>
    /// Military checkpoints must be normalized: no place stays free while recruits wait, progress lies inside one
    /// training period, most advanced first, and employed plus enlisted people never exceed the population.
    /// </summary>
    private static void ValidateMilitary(GameScenario scenario, GameSnapshot snapshot)
    {
        if (scenario.MilitaryRules is not { } rules)
        {
            if (snapshot.Military is not null) throw new ArgumentException("This scenario has no military state.", nameof(snapshot));
            return;
        }
        if (snapshot.Military is not { } military || military.TrainingProgressSeconds.IsDefault ||
            military.QueuedRecruits < 0 || military.Soldiers < 0)
            throw new ArgumentException("Military state is missing or invalid.", nameof(snapshot));
        var places = (long)snapshot.Buildings.First(item => item.BuildingId == rules.BarracksBuildingId).Count * rules.RecruitsPerBarracks;
        var training = military.TrainingProgressSeconds;
        if (training.Length > places || (military.QueuedRecruits > 0 && training.Length != places) ||
            training.Any(progress => progress < 0 || progress >= rules.TrainingSeconds) ||
            training.Zip(training.Skip(1)).Any(pair => pair.First < pair.Second))
            throw new ArgumentException("Training places are inconsistent.", nameof(snapshot));
        var employed = snapshot.Buildings.Sum(item => (long)item.AssignedWorkers);
        if (employed + military.People > snapshot.Population)
            throw new ArgumentException("Workers and enlisted people exceed the population.", nameof(snapshot));
    }

    /// <summary>Moves waiting recruits into free places; a recruit who enters now completes its first second at the next step.</summary>
    private static MilitaryState FillPlaces(MilitaryState military, int places)
    {
        var free = Math.Min(places - military.TrainingRecruits, military.QueuedRecruits);
        if (free <= 0) return military;
        return military with
        {
            QueuedRecruits = military.QueuedRecruits - free,
            TrainingProgressSeconds = military.TrainingProgressSeconds.AddRange(Enumerable.Repeat(0, free))
        };
    }

    /// <summary>Spends the equipment and enlists free civilians at once; they stay residents while waiting and training.</summary>
    public GameActionResult Enlist(int count)
    {
        if (Scenario.MilitaryRules is not { } rules || Military is not { } military) return new(GameError.MilitaryUnavailable);
        if (count <= 0) return new(GameError.InvalidQuantity);
        if (_buildings[rules.BarracksBuildingId].Count == 0) return new(GameError.MissingPrerequisite);
        if (count > AvailableWorkers) return new(GameError.InsufficientPopulation);
        ResourceBundle cost;
        int queued;
        try
        {
            cost = ResourceBundle.FromAmounts([KeyValuePair.Create(rules.SwordsResourceId, rules.SwordsPerRecruit * count)]);
            queued = checked(military.QueuedRecruits + count);
        }
        catch (OverflowException) { return new(GameError.NumericLimit); }
        if (!Resources.Covers(cost)) return new(GameError.InsufficientResources);
        Resources -= cost;
        Military = FillPlaces(military with { QueuedRecruits = queued }, TrainingPlaces);
        return GameActionResult.Success;
    }

    /// <summary>
    /// Returns recruits to the free civilians with their equipment. Waiting recruits are cancelled first, the most
    /// recently enlisted first; then recruits in training with the least progress, whose completed seconds are lost.
    /// Refused without changes when the returned equipment would exceed storage.
    /// </summary>
    public GameActionResult CancelRecruits(int count)
    {
        if (Scenario.MilitaryRules is not { } rules || Military is not { } military) return new(GameError.MilitaryUnavailable);
        if (count <= 0) return new(GameError.InvalidQuantity);
        if (count > military.Recruits) return new(GameError.InsufficientRecruits);
        ResourceBundle refund, next;
        try
        {
            refund = ResourceBundle.FromAmounts([KeyValuePair.Create(rules.SwordsResourceId, rules.SwordsPerRecruit * count)]);
            next = Resources + refund;
        }
        catch (OverflowException) { return new(GameError.NumericLimit); }
        if (!StorageCapacity.Covers(next)) return new(GameError.InsufficientStorage);
        var fromQueue = Math.Min(count, military.QueuedRecruits);
        var fromTraining = count - fromQueue;
        Resources = next;
        Military = military with
        {
            QueuedRecruits = military.QueuedRecruits - fromQueue,
            TrainingProgressSeconds = military.TrainingProgressSeconds.RemoveRange(military.TrainingRecruits - fromTraining, fromTraining)
        };
        return GameActionResult.Success;
    }

    /// <summary>One valid second for every place in use; completed recruits become soldiers and free their place.</summary>
    private void TrainOneSecond()
    {
        if (Scenario.MilitaryRules is not { } rules || Military is not { } military || military.TrainingRecruits == 0) return;
        var advanced = military.TrainingProgressSeconds.Select(progress => progress + 1).ToArray();
        var completed = advanced.Count(progress => progress >= rules.TrainingSeconds);
        Military = FillPlaces(military with
        {
            TrainingProgressSeconds = [.. advanced.Where(progress => progress < rules.TrainingSeconds)],
            Soldiers = checked(military.Soldiers + completed)
        }, TrainingPlaces);
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
            if (Scenario.MilitaryRules is { } recruitment && recruitment.BarracksBuildingId == buildingId)
                _ = checked(nextCount * recruitment.RecruitsPerBarracks);
        }
        catch (OverflowException) { return new(GameError.NumericLimit); }
        if (!Resources.Covers(cost)) return new(GameError.InsufficientResources);
        var nextResources = Resources - cost;
        _buildings[buildingId] = _buildings[buildingId] with { Count = nextCount };
        Resources = nextResources;
        if (Military is { } military && Scenario.MilitaryRules?.BarracksBuildingId == buildingId)
            Military = FillPlaces(military, TrainingPlaces);
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
            TrainOneSecond();
        }
        ElapsedTicks = nextTicks;
        return GameActionResult.Success;
    }
}
