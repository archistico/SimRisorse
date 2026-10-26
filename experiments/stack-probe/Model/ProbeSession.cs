using System.Collections.Immutable;

namespace SimRisorse.StackProbe.Model;

// An isolated M1 experiment, not the production Core.
public sealed class ProbeSession
{
    private readonly Dictionary<BuildingKind, BuildingState> _buildings;

    private ProbeSession(ProbeSnapshot snapshot)
    {
        Validate(snapshot);
        Resources = snapshot.Resources;
        ElapsedTicks = snapshot.ElapsedTicks;
        _buildings = snapshot.Buildings.ToDictionary(item => item.Kind);
    }

    public ResourceAmounts Resources { get; private set; }
    public long ElapsedTicks { get; private set; }
    public bool IsPaused { get; private set; } = true;
    public int AvailableWorkers => ProbeCatalog.Population - _buildings.Values.Sum(item => item.Workers);
    public BuildingState GetBuilding(BuildingKind kind) => _buildings[kind];

    public static ProbeSession Create() => new(new(ProbeCatalog.FormatVersion, ProbeCatalog.ScenarioId,
        ProbeCatalog.RulesVersion, 0, new(0, 120, 80),
        ProbeCatalog.Buildings.Select(item => new BuildingState(item.Kind, 1, 1)).ToImmutableArray()));

    public static ProbeSession Restore(ProbeSnapshot snapshot) => new(snapshot);
    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;

    public ProbeSnapshot Snapshot() => new(ProbeCatalog.FormatVersion, ProbeCatalog.ScenarioId,
        ProbeCatalog.RulesVersion, ElapsedTicks, Resources,
        ProbeCatalog.Buildings.Select(item => _buildings[item.Kind]).ToImmutableArray());

    public ActionResult Construct(BuildingKind kind)
    {
        var definition = ProbeCatalog.Get(kind);
        if (!Resources.Covers(definition.Cost)) return ActionResult.InsufficientResources;
        var current = _buildings[kind];
        var nextCount = checked(current.Count + 1);
        Resources -= definition.Cost;
        _buildings[kind] = current with { Count = nextCount };
        return ActionResult.Success;
    }

    public ActionResult AssignWorker(BuildingKind kind)
    {
        var current = _buildings[kind];
        if (current.Workers >= current.Count) return ActionResult.NoWorkplace;
        if (AvailableWorkers == 0) return ActionResult.NoAvailableWorker;
        _buildings[kind] = current with { Workers = current.Workers + 1 };
        return ActionResult.Success;
    }

    public ActionResult ReleaseWorker(BuildingKind kind)
    {
        var current = _buildings[kind];
        if (current.Workers == 0) return ActionResult.NoAssignedWorker;
        _buildings[kind] = current with { Workers = current.Workers - 1 };
        return ActionResult.Success;
    }

    public void Advance(TimeSpan elapsed)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(elapsed, TimeSpan.Zero);
        if (IsPaused) return;
        var nextTicks = checked(ElapsedTicks + elapsed.Ticks);
        var steps = nextTicks / TimeSpan.TicksPerSecond - ElapsedTicks / TimeSpan.TicksPerSecond;
        for (long step = 0; step < steps; step++) Step();
        ElapsedTicks = nextTicks;
    }

    public int RunnableWorkers(BuildingKind kind)
    {
        var definition = ProbeCatalog.Get(kind);
        return LimitCycles(_buildings[kind].Workers, definition, Resources, ProbeCatalog.Capacity - Resources);
    }

    private void Step()
    {
        var available = Resources;
        var freeSpace = ProbeCatalog.Capacity - Resources;
        var delta = new ResourceAmounts();
        foreach (var definition in ProbeCatalog.Buildings)
        {
            var cycles = LimitCycles(_buildings[definition.Kind].Workers, definition, available, freeSpace);
            var consumed = definition.Input * cycles;
            var produced = definition.Output * cycles;
            available -= consumed;
            freeSpace -= produced;
            delta = delta - consumed + produced;
        }
        Resources += delta;
    }

    private static int LimitCycles(int workers, BuildingDefinition definition,
        ResourceAmounts available, ResourceAmounts freeSpace)
    {
        long cycles = workers;
        foreach (var kind in Enum.GetValues<ResourceKind>())
        {
            if (definition.Input[kind] > 0) cycles = Math.Min(cycles, available[kind] / definition.Input[kind]);
            if (definition.Output[kind] > 0) cycles = Math.Min(cycles, freeSpace[kind] / definition.Output[kind]);
        }
        return checked((int)cycles);
    }

    public static void Validate(ProbeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.FormatVersion != ProbeCatalog.FormatVersion || snapshot.ScenarioId != ProbeCatalog.ScenarioId ||
            snapshot.RulesVersion != ProbeCatalog.RulesVersion)
            throw new InvalidDataException("Formato, scenario o versione delle regole non supportati dalla prova.");
        if (snapshot.ElapsedTicks < 0 || !snapshot.Resources.Covers(new()) || !ProbeCatalog.Capacity.Covers(snapshot.Resources))
            throw new InvalidDataException("Tempo o scorte non validi.");
        if (snapshot.Buildings.IsDefault || snapshot.Buildings.Length != ProbeCatalog.Buildings.Count ||
            snapshot.Buildings.Any(item => item is null) ||
            snapshot.Buildings.Select(item => item.Kind).Distinct().Count() != ProbeCatalog.Buildings.Count ||
            snapshot.Buildings.Any(item => !Enum.IsDefined(item.Kind) || item.Count < 0 || item.Workers < 0 || item.Workers > item.Count) ||
            snapshot.Buildings.Sum(item => (long)item.Workers) > ProbeCatalog.Population)
            throw new InvalidDataException("Edifici o assegnazioni non validi.");
    }
}
