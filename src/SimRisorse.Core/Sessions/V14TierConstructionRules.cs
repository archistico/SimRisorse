using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Sessions;

/// <summary>
/// Experimental tier-price policy. Values are supplied to the isolated v14 session;
/// they do not alter the playable civil-growth-13 ruleset.
/// </summary>
public sealed class V14TierConstructionRules
{
    public V14TierConstructionRules(decimal growthFactor, decimal largeMultiplier, decimal advancedMultiplier)
    {
        if (growthFactor < 1m || largeMultiplier <= 1m || advancedMultiplier <= largeMultiplier)
            throw new ArgumentOutOfRangeException(nameof(growthFactor),
                "Growth and size multipliers must be monotone and positive.");
        GrowthFactor = growthFactor;
        LargeMultiplier = largeMultiplier;
        AdvancedMultiplier = advancedMultiplier;
    }

    public decimal GrowthFactor { get; }
    public decimal LargeMultiplier { get; }
    public decimal AdvancedMultiplier { get; }

    /// <summary>Candidate balancing values, not an approved player-facing rule.</summary>
    public static V14TierConstructionRules Candidate { get; } = new(1.15m, 2.6m, 6.5m);
}

public enum V14TierActionFailure
{
    None,
    UnknownFamily,
    InvalidTier,
    InvalidQuantity,
    MissingPrerequisite,
    MissingResearch,
    MissingSourceBuilding,
    InsufficientResources,
    InsufficientPopulation,
    InsufficientWorkplaces,
    InsufficientAssignedWorkers,
    NumericLimit
}

/// <summary>Result for new buildings and upgrades; the quoted cost is never a partial debit.</summary>
public sealed record V14TierActionResult(V14TierActionFailure Failure, ResourceBundle Cost)
{
    public bool Succeeded => Failure == V14TierActionFailure.None;
}
