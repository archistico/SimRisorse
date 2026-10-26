using System.Collections.Immutable;

namespace SimRisorse.StackProbe.Model;

public sealed record BuildingState(BuildingKind Kind, int Count, int Workers);

public sealed record ProbeSnapshot(int FormatVersion, string ScenarioId, string RulesVersion,
    long ElapsedTicks, ResourceAmounts Resources, ImmutableArray<BuildingState> Buildings);

public interface IProbeSnapshotStore
{
    void Save(ProbeSnapshot snapshot);
    ProbeSnapshot Load();
}
