using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Military;

/// <summary>
/// Military parameters are scenario data, not implicit economy production.
/// M5.3 rules are scenario data, shared by recruitment, session replay and persistence.
/// </summary>
public sealed class MilitaryTrainingRules
{
    public static MilitaryTrainingRules SimpleBarracks { get; } = new(
        "barracks", "forge", "swords", ResourceBundle.Create(("planks", 280m), ("stone", 200m)),
        swordsPerRecruit: 2, seatsPerBarracks: 2, trainingSecondsPerGroup: 60);

    public static MilitaryTrainingRules IndividualBarracks { get; } = new(
        "barracks", "forge", "swords", ResourceBundle.Create(("planks", 260m), ("stone", 200m)),
        swordsPerRecruit: 5, seatsPerBarracks: 1, trainingSecondsPerGroup: 30, individualTraining: true);

    public MilitaryTrainingRules(string barracksId, string prerequisiteId, string swordResourceId,
        ResourceBundle barracksCost, int swordsPerRecruit, int seatsPerBarracks, int trainingSecondsPerGroup,
        bool individualTraining = false)
    {
        ContentIdentifier.Validate(barracksId, nameof(barracksId));
        ContentIdentifier.Validate(prerequisiteId, nameof(prerequisiteId));
        ContentIdentifier.Validate(swordResourceId, nameof(swordResourceId));
        ArgumentNullException.ThrowIfNull(barracksCost);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(swordsPerRecruit);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seatsPerBarracks);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(trainingSecondsPerGroup);
        BarracksId = barracksId;
        PrerequisiteId = prerequisiteId;
        SwordResourceId = swordResourceId;
        BarracksCost = barracksCost;
        SwordsPerRecruit = swordsPerRecruit;
        SeatsPerBarracks = seatsPerBarracks;
        TrainingSecondsPerGroup = trainingSecondsPerGroup;
        IndividualTraining = individualTraining;
    }

    public string BarracksId { get; }
    public string PrerequisiteId { get; }
    public string SwordResourceId { get; }
    public ResourceBundle BarracksCost { get; }
    public int SwordsPerRecruit { get; }
    public int SeatsPerBarracks { get; }
    public int TrainingSecondsPerGroup { get; }
    public bool IndividualTraining { get; }
}
