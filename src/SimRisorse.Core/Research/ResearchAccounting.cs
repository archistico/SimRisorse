namespace SimRisorse.Core.Research;

/// <summary>
/// Free accumulation and on-demand spending, independent of material warehouses.
/// The research rate is a ruleset parameter; no technology is automatically started.
/// This pure service is not yet active in a GameSession.
/// </summary>
public static class ResearchAccounting
{
    public static ResearchPoints Accrue(ResearchPoints balance, int activeResearchers,
        long elapsedWholeSeconds, long milliPointsPerResearcherPerSecond)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(activeResearchers);
        ArgumentOutOfRangeException.ThrowIfNegative(elapsedWholeSeconds);
        ArgumentOutOfRangeException.ThrowIfNegative(milliPointsPerResearcherPerSecond);
        var produced = checked(checked((long)activeResearchers * elapsedWholeSeconds) * milliPointsPerResearcherPerSecond);
        return ResearchPoints.FromMilliPoints(checked(balance.MilliPoints + produced));
    }

    /// <summary>Failure leaves the original immutable balance unchanged.</summary>
    public static bool TrySpend(ResearchPoints balance, ResearchPoints price, out ResearchPoints remaining)
    {
        if (balance.MilliPoints < price.MilliPoints)
        {
            remaining = balance;
            return false;
        }
        remaining = ResearchPoints.FromMilliPoints(balance.MilliPoints - price.MilliPoints);
        return true;
    }
}
