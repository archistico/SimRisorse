using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Scenarios;

/// <summary>Known explicit conversions between rules versions of the same scenario. Unknown pairs are left unchanged.</summary>
internal static class ScenarioUpgrades
{
    public static GameSnapshot Apply(GameSnapshot snapshot, string targetRulesVersion)
    {
        while (snapshot.RulesVersion != targetRulesVersion)
        {
            GameSnapshot? next = (snapshot.ScenarioId, snapshot.RulesVersion) switch
            {
                (CivilGrowthScenario.Id, _) => CivilGrowthScenario.UpgradeOneStep(snapshot),
                _ => null
            };
            if (next is null || next.RulesVersion == snapshot.RulesVersion) return snapshot;
            snapshot = next;
        }
        return snapshot;
    }
}
