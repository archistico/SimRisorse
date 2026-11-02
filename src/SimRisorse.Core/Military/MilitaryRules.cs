using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Military;

/// <summary>
/// Recruitment contract: enlisting spends equipment immediately; every owned barracks trains a fixed number of
/// recruits at once, each for a whole number of valid game seconds. Recruits and soldiers remain residents.
/// </summary>
public sealed record MilitaryRules
{
    public MilitaryRules(string barracksBuildingId, string swordsResourceId, ResourceAmount swordsPerRecruit,
        int trainingSeconds, int recruitsPerBarracks = 1)
    {
        ContentIdentifier.Validate(barracksBuildingId, nameof(barracksBuildingId));
        ContentIdentifier.Validate(swordsResourceId, nameof(swordsResourceId));
        if (swordsPerRecruit == ResourceAmount.Zero || swordsPerRecruit.Milliunits > long.MaxValue / int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(swordsPerRecruit));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(trainingSeconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(recruitsPerBarracks);
        BarracksBuildingId = barracksBuildingId;
        SwordsResourceId = swordsResourceId;
        SwordsPerRecruit = swordsPerRecruit;
        TrainingSeconds = trainingSeconds;
        RecruitsPerBarracks = recruitsPerBarracks;
    }

    public string BarracksBuildingId { get; }
    public string SwordsResourceId { get; }
    public ResourceAmount SwordsPerRecruit { get; }
    public int TrainingSeconds { get; }
    public int RecruitsPerBarracks { get; }
}
