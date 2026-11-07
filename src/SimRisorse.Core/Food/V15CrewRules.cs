using SimRisorse.Core.Construction;

namespace SimRisorse.Core.Food;

/// <summary>
/// RC81: the saved campaign selects its staffing contract explicitly. Legacy
/// campaigns remain unchanged; the mint-ten ruleset is an opt-in gameplay mode.
/// Neither a desktop option nor a change to economic prices is implied here.
/// </summary>
public enum V15CrewMode { Legacy = 0, MintTen = 1 }

public static class V15CrewRules
{
    public const string MintId = "mint";
    public const int MintCrewSize = 10;
    public const string MintTenRulesVersion = "civil-growth-15-mint-crew10";

    public static int CrewSize(V15CrewMode mode, string familyId) => mode switch
    {
        V15CrewMode.Legacy => 1,
        V15CrewMode.MintTen => familyId == MintId ? MintCrewSize : 1,
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };

    public static string SavedRulesVersion(V15CrewMode mode) => mode switch
    {
        V15CrewMode.Legacy => V15FoodCampaignCheckpoints.RulesVersion,
        V15CrewMode.MintTen => MintTenRulesVersion,
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };

    public static V15CrewMode FromSavedRulesVersion(string value) => value switch
    {
        V15FoodCampaignCheckpoints.RulesVersion => V15CrewMode.Legacy,
        MintTenRulesVersion => V15CrewMode.MintTen,
        _ => throw new ArgumentException("Unknown v15 crew rules version.", nameof(value))
    };

    public static int StaffedModules(V15ProductiveFamilySnapshot family, int crewSize) =>
        CrewTeamPortfolio.Create(family.FamilyId, crewSize,
            family.Base, family.Large, family.Advanced).StaffedModules;
}
