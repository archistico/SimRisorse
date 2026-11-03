using System.Collections.Immutable;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class IndividualRecruitmentTests
{
    private static readonly MilitaryTrainingRules Rules = MilitaryTrainingRules.IndividualBarracks;
    private static readonly ResourceBundle Storage = ResourceBundle.Create(("swords", 1000m));

    [Fact]
    public void IndividualRulesUseOneSeatThirtySecondsFiveSwordsAndLowerBarracksCost()
    {
        Assert.True(Rules.IndividualTraining);
        Assert.Equal(1, Rules.SeatsPerBarracks);
        Assert.Equal(30, Rules.TrainingSecondsPerGroup);
        Assert.Equal(5, Rules.SwordsPerRecruit);
        Assert.Equal(260m, Rules.BarracksCost["planks"].Units);
        Assert.Equal(200m, Rules.BarracksCost["stone"].Units);
        Assert.False(MilitaryTrainingRules.SimpleBarracks.IndividualTraining);
    }

    [Fact]
    public void EnlistTrainAndCancelDuringTrainingWithoutPartialRefund()
    {
        var equipment = ResourceBundle.Create(("swords", 100m));
        var enlisted = MilitaryTraining.Enlist(Rules, MilitaryTrainingState.IndividualEmpty,
            equipment, population: 40, assignedWorkers: 3, barracks: 2, count: 5);
        Assert.True(enlisted.Succeeded);
        Assert.Equal(75m, enlisted.Resources["swords"].Units);
        Assert.Equal((3, 2, 0), (enlisted.State.WaitingRecruits,
            enlisted.State.TrainingRecruits, enlisted.State.AvailableSoldiers));
        var midway = MilitaryTraining.Advance(Rules, enlisted.State, barracks: 2, seconds: 15);
        Assert.Equal(new[] { 15, 15 }, midway.IndividualProgressSeconds);
        var cancelled = MilitaryTraining.CancelWaiting(Rules, midway, enlisted.Resources,
            Storage, barracks: 2, count: 4);
        Assert.True(cancelled.Succeeded);
        Assert.Equal(95m, cancelled.Resources["swords"].Units);
        Assert.Equal((0, 1, 0, 15), (cancelled.State.WaitingRecruits,
            cancelled.State.TrainingRecruits, cancelled.State.AvailableSoldiers,
            cancelled.State.TrainingProgressSeconds));
        var trained = MilitaryTraining.Advance(Rules, cancelled.State, barracks: 2, seconds: 15);
        Assert.Equal((0, 0, 1), (trained.WaitingRecruits,
            trained.TrainingRecruits, trained.AvailableSoldiers));
        Assert.True(trained.HasIndividualProgress);
    }

    [Fact]
    public void StaggeredRecruitsCompleteIndividuallyAndTrainingIsDeterministic()
    {
        var first = MilitaryTraining.Enlist(Rules, MilitaryTrainingState.IndividualEmpty,
            ResourceBundle.Create(("swords", 50m)), 40, 3, 2, 1);
        var atTen = MilitaryTraining.Advance(Rules, first.State, 2, 10);
        var second = MilitaryTraining.Enlist(Rules, atTen, first.Resources, 40, 3, 2, 1);
        Assert.Equal(new[] { 10, 0 }, second.State.IndividualProgressSeconds);
        var afterTwenty = MilitaryTraining.Advance(Rules, second.State, 2, 20);
        Assert.Equal(1, afterTwenty.AvailableSoldiers);
        Assert.Equal(new[] { 20 }, afterTwenty.IndividualProgressSeconds);
        var finished = MilitaryTraining.Advance(Rules, afterTwenty, 2, 10);
        Assert.Equal(2, finished.AvailableSoldiers);
        Assert.Empty(finished.IndividualProgressSeconds);
        Assert.Equal(finished,
            MilitaryTraining.Advance(Rules, second.State, 2, 30));
    }

    [Fact]
    public void FullWarehouseAndInsufficientCapacityRejectCancellationAtomically()
    {
        var enlisted = MilitaryTraining.Enlist(Rules, MilitaryTrainingState.IndividualEmpty,
            ResourceBundle.Create(("swords", 20m)), 40, 3, 1, 2);
        var full = ResourceBundle.Create(("swords", 1000m));
        var rejected = MilitaryTraining.CancelWaiting(Rules, enlisted.State, full, Storage, 1, 1);
        Assert.Equal(RecruitmentFailure.InsufficientStorage, rejected.Failure);
        Assert.Equal(enlisted.State, rejected.State);
        Assert.Equal(full, rejected.Resources);
        Assert.Equal(RecruitmentFailure.InsufficientQueuedRecruits,
            MilitaryTraining.CancelWaiting(Rules, enlisted.State, enlisted.Resources, Storage, 1, 3).Failure);
    }

    [Fact]
    public void LegacyConversionPreservesPeopleAndNormalizesPercentProgress()
    {
        var old = new MilitaryTrainingState(3, 2, 4, 30);
        var migrated = IndividualRecruitment.UpgradeLegacy(old, barracks: 1);
        Assert.Equal((4, 1, 4, 15), (migrated.WaitingRecruits,
            migrated.TrainingRecruits, migrated.AvailableSoldiers,
            migrated.TrainingProgressSeconds));
        Assert.Equal(old.EnlistedPopulation, migrated.EnlistedPopulation);
        Assert.Equal(15, Assert.Single(migrated.IndividualProgressSeconds));
        IndividualRecruitment.Validate(Rules, migrated, 1);
        Assert.Throws<ArgumentException>(() => MilitaryTraining.Validate(migrated,
            MilitaryTrainingRules.SimpleBarracks, 1));
    }

    [Fact]
    public void ForgedIndividualCheckpointIsRejected()
    {
        Assert.Throws<ArgumentException>(() => IndividualRecruitment.Validate(Rules,
            new(1, 1, 0, 31, ImmutableArray.Create(31)), 1));
        Assert.Throws<ArgumentException>(() => IndividualRecruitment.Validate(Rules,
            new(1, 0, 0, 0, ImmutableArray<int>.Empty), 1));
        Assert.Throws<ArgumentException>(() => IndividualRecruitment.Validate(Rules,
            new(0, 2, 0, 10, ImmutableArray.Create(10, 12)), 2));
    }
}
