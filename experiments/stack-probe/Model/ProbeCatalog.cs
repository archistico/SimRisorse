namespace SimRisorse.StackProbe.Model;

public enum ResourceKind { Logs, Planks, Stone }
public enum BuildingKind { TreeFeller, Sawmill, Stonecutter }
public enum ActionResult { Success, InsufficientResources, NoWorkplace, NoAvailableWorker, NoAssignedWorker }

public readonly record struct ResourceAmounts(long Logs, long Planks, long Stone)
{
    public long this[ResourceKind kind] => kind switch
    {
        ResourceKind.Logs => Logs,
        ResourceKind.Planks => Planks,
        ResourceKind.Stone => Stone,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public static ResourceAmounts operator +(ResourceAmounts a, ResourceAmounts b) =>
        new(checked(a.Logs + b.Logs), checked(a.Planks + b.Planks), checked(a.Stone + b.Stone));

    public static ResourceAmounts operator -(ResourceAmounts a, ResourceAmounts b) =>
        new(checked(a.Logs - b.Logs), checked(a.Planks - b.Planks), checked(a.Stone - b.Stone));

    public static ResourceAmounts operator *(ResourceAmounts a, long multiplier) =>
        new(checked(a.Logs * multiplier), checked(a.Planks * multiplier), checked(a.Stone * multiplier));

    public bool Covers(ResourceAmounts other) => Logs >= other.Logs && Planks >= other.Planks && Stone >= other.Stone;
}

public sealed record BuildingDefinition(BuildingKind Kind, string Name, ResourceAmounts Cost,
    ResourceAmounts Input, ResourceAmounts Output);

public static class ProbeCatalog
{
    public const string ScenarioId = "economy-probe";
    public const string RulesVersion = "probe-1";
    public const int Population = 8;
    public const int FormatVersion = 1;
    public static ResourceAmounts Capacity { get; } = new(1000, 1000, 1000);

    // This explicit order is also the allocation order for each economic step.
    public static IReadOnlyList<BuildingDefinition> Buildings { get; } = Array.AsReadOnly<BuildingDefinition>(
    [
        new(BuildingKind.TreeFeller, "Taglialegna", new(0, 20, 10), new(), new(2, 0, 0)),
        new(BuildingKind.Sawmill, "Segheria", new(0, 40, 20), new(2, 0, 0), new(0, 1, 0)),
        new(BuildingKind.Stonecutter, "Tagliapietra", new(0, 30, 10), new(), new(0, 0, 1))
    ]);

    public static BuildingDefinition Get(BuildingKind kind) =>
        Buildings.FirstOrDefault(item => item.Kind == kind) ?? throw new ArgumentOutOfRangeException(nameof(kind));
}
