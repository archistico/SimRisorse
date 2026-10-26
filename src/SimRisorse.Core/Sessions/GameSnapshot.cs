using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Sessions;

public sealed record GameSnapshot(int FormatVersion, string ScenarioId, string RulesVersion,
    long ElapsedTicks, int Population, ResourceBundle Resources, ImmutableArray<BuildingState> Buildings,
    int? GrowthProgressSeconds = null);
