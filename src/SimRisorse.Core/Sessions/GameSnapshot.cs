using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;

namespace SimRisorse.Core.Sessions;

/// <param name="Military">Recruits and soldiers; present exactly when the scenario has military rules.</param>
public sealed record GameSnapshot(int FormatVersion, string ScenarioId, string RulesVersion,
    long ElapsedTicks, int Population, ResourceBundle Resources, ImmutableArray<BuildingState> Buildings,
    int? GrowthProgressSeconds = null, int GrowthRemainderTwentieths = 0, MilitaryState? Military = null);
