using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class MilitaryTrainingTests
{
    private static readonly MilitaryTrainingRules Rules = MilitaryTrainingRules.SimpleBarracks;

    private static ResourceBundle Swords(decimal count) => ResourceBundle.Create(("swords", count));

    private static RecruitmentAttempt Enlist(int quantity, decimal swords = 100m,
        int people = 40, int workers = 10, int barracks = 1,
        MilitaryTrainingState? state = null) =>
        MilitaryTraining.Enlist(Rules, state ?? MilitaryTrainingState.Empty,
            Swords(swords), people, workers, barracks, quantity);

    [Fact]
    public void ApprovedSimpleContractKeepsConstructionWeaponsAndTrainingValuesTogether()
    {
        Assert.Equal("barracks", Rules.BarracksId);
        Assert.Equal("forge", Rules.PrerequisiteId);
        Assert.Equal("swords", Rules.SwordResourceId);
        Assert.Equal(280m, Rules.BarracksCost["planks"].Units);
        Assert.Equal(200m, Rules.BarracksCost["stone"].Units);
        Assert.Equal(2, Rules.SwordsPerRecruit);
        Assert.Equal(2, Rules.SeatsPerBarracks);
        Assert.Equal(60, Rules.TrainingSecondsPerGroup);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonpositiveOrdersAreRejectedWithoutChangingResourcesOrPopulationReservations(int count)
    {
        var result = Enlist(count);
        Assert.Equal(RecruitmentFailure.InvalidQuantity, result.Failure);
        Assert.Equal(Swords(100m), result.Resources);
        Assert.Equal(MilitaryTrainingState.Empty, result.State);
    }

    [Fact]
    public void BarracksAreMandatoryEvenWhenWeaponsAndWorkersAreAvailable()
    {
        var result = Enlist(1, barracks: 0);
        Assert.Equal(RecruitmentFailure.MissingBarracks, result.Failure);
        Assert.Equal(Swords(100m), result.Resources);
        Assert.Equal(MilitaryTrainingState.Empty, result.State);
    }

    [Fact]
    public void RejectedWeaponAndWorkerOrdersAreAtomic()
    {
        var missingSwords = Enlist(3, swords: 5m);
        Assert.Equal(RecruitmentFailure.InsufficientSwords, missingSwords.Failure);
        Assert.Equal(Swords(5m), missingSwords.Resources);
        Assert.Equal(MilitaryTrainingState.Empty, missingSwords.State);

        var missingCivilians = Enlist(6, people: 20, workers: 15);
        Assert.Equal(RecruitmentFailure.InsufficientPopulation, missingCivilians.Failure);
        Assert.Equal(Swords(100m), missingCivilians.Resources);
        Assert.Equal(MilitaryTrainingState.Empty, missingCivilians.State);
    }

    [Fact]
    public void EnlistingTwentyReservesPeopleAndConsumesFortySwordsExactlyOnce()
    {
        var result = Enlist(20, swords: 40m, people: 40, workers: 15);
        Assert.True(result.Succeeded);
        Assert.Equal(Swords(0m), result.Resources);
        Assert.Equal(new MilitaryTrainingState(18, 2, 0, 0), result.State);
        Assert.Equal(20L, result.State.EnlistedPopulation);

        var afterTraining = MilitaryTraining.Advance(Rules, result.State, 1, 600);
        Assert.Equal(new MilitaryTrainingState(0, 0, 20, 0), afterTraining);
        Assert.Equal(20L, afterTraining.EnlistedPopulation);
        Assert.Equal(Swords(0m), result.Resources); // Completing training never pays again.
    }

    [Fact]
    public void PeopleAlreadyInTheMilitaryCannotBeRecruitedAgain()
    {
        var first = Enlist(20, swords: 100m, people: 30, workers: 10);
        Assert.True(first.Succeeded);
        var second = MilitaryTraining.Enlist(Rules, first.State, first.Resources,
            population: 30, assignedWorkers: 10, barracks: 1, count: 1);
        Assert.Equal(RecruitmentFailure.InsufficientPopulation, second.Failure);
        Assert.Equal(first.State, second.State);
        Assert.Equal(first.Resources, second.Resources);
    }

    [Fact]
    public void LateRecruitsNeverJoinAnAlreadyRunningGroup()
    {
        var first = Enlist(1);
        var afterThirty = MilitaryTraining.Advance(Rules, first.State, 1, 30);
        var second = MilitaryTraining.Enlist(Rules, afterThirty, first.Resources,
            population: 40, assignedWorkers: 10, barracks: 1, count: 1);
        Assert.True(second.Succeeded);
        Assert.Equal(new MilitaryTrainingState(1, 1, 0, 30), second.State);

        var atSixty = MilitaryTraining.Advance(Rules, second.State, 1, 30);
        Assert.Equal(new MilitaryTrainingState(0, 1, 1, 0), atSixty);
        Assert.Equal(new MilitaryTrainingState(0, 0, 2, 0), MilitaryTraining.Advance(Rules, atSixty, 1, 60));
    }

    [Fact]
    public void BoundariesAtFiftyNineAndSixtySecondsCompleteExactlyOneGroup()
    {
        var recruited = Enlist(3);
        var atFiftyNine = MilitaryTraining.Advance(Rules, recruited.State, 1, 59);
        Assert.Equal(new MilitaryTrainingState(1, 2, 0, 59), atFiftyNine);
        var atSixty = MilitaryTraining.Advance(Rules, atFiftyNine, 1, 1);
        Assert.Equal(new MilitaryTrainingState(0, 1, 2, 0), atSixty);
        Assert.Equal(new MilitaryTrainingState(0, 1, 2, 1), MilitaryTraining.Advance(Rules, atSixty, 1, 1));
    }

    [Fact]
    public void TwoBarracksFinishThirtyRecruitsInEightFullGroups()
    {
        var recruited = Enlist(30, swords: 60m, people: 40, workers: 10, barracks: 2);
        Assert.True(recruited.Succeeded);
        Assert.Equal(new MilitaryTrainingState(26, 4, 0, 0), recruited.State);
        Assert.Equal(new MilitaryTrainingState(0, 2, 28, 59),
            MilitaryTraining.Advance(Rules, recruited.State, 2, 479));
        Assert.Equal(new MilitaryTrainingState(0, 0, 30, 0), MilitaryTraining.Advance(Rules, recruited.State, 2, 480));
    }

    [Fact]
    public void NewBarracksIncreaseSeatsOnlyForTheNextGroup()
    {
        var initial = Enlist(6).State;
        var partway = MilitaryTraining.Advance(Rules, initial, 1, 30);
        Assert.Equal(new MilitaryTrainingState(4, 2, 0, 30), partway);
        var next = MilitaryTraining.Advance(Rules, partway, 2, 30);
        Assert.Equal(new MilitaryTrainingState(0, 4, 2, 0), next);
        Assert.Equal(new MilitaryTrainingState(0, 0, 6, 0), MilitaryTraining.Advance(Rules, next, 2, 60));
    }

    [Fact]
    public void BatchAndSplitTrainingStepsProduceIdenticalQueueAndSoldiers()
    {
        var initial = Enlist(17).State;
        var whole = MilitaryTraining.Advance(Rules, initial, 1, 421);
        var split = initial;
        foreach (var seconds in new[] { 5, 55, 1, 120, 160, 80 })
            split = MilitaryTraining.Advance(Rules, split, 1, seconds);
        Assert.Equal(whole, split);
        Assert.Equal(421, new[] { 5, 55, 1, 120, 160, 80 }.Sum());
        Assert.Equal(initial.EnlistedPopulation, whole.EnlistedPopulation);
    }

    [Fact]
    public void IdleQueuesDoNotInventSoldiersOrProgress()
    {
        Assert.Equal(MilitaryTrainingState.Empty, MilitaryTraining.Advance(Rules, MilitaryTrainingState.Empty, 1, 86400));
        Assert.Equal(MilitaryTrainingState.Empty, MilitaryTraining.Advance(Rules, MilitaryTrainingState.Empty, 0, 0));
    }

    [Fact]
    public void InvalidRestoredMilitaryStateIsRejectedBeforeAnyMutation()
    {
        var invalid = new[]
        {
            new MilitaryTrainingState(-1, 0, 0, 0),
            new MilitaryTrainingState(0, 0, 0, 1),
            new MilitaryTrainingState(0, 1, 0, 60),
            new MilitaryTrainingState(0, 3, 0, 10),
            new MilitaryTrainingState(1, 0, 0, 0),
            new MilitaryTrainingState(1, 0, 1, 0)
        };
        for (var i = 0; i < invalid.Length; i++)
            Assert.Throws<ArgumentException>(() => MilitaryTraining.Validate(invalid[i], Rules, i == 4 ? 0 : 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => MilitaryTraining.Advance(Rules, MilitaryTrainingState.Empty, 1, -1));
    }

    [Fact]
    public void ImpossibleWeaponAmountFailsBeforeAnyResourceChanges()
    {
        var huge = new MilitaryTrainingRules("barracks", "forge", "swords", Rules.BarracksCost,
            int.MaxValue, 2, 60);
        var stock = Swords(20m);
        var attempted = MilitaryTraining.Enlist(huge, MilitaryTrainingState.Empty, stock,
            int.MaxValue, 0, 1, int.MaxValue);
        Assert.Equal(RecruitmentFailure.NumericLimit, attempted.Failure);
        Assert.Equal(stock, attempted.Resources);
        Assert.Equal(MilitaryTrainingState.Empty, attempted.State);
    }

    [Fact]
    public void WaitingCancellationRefundsExactlyTwoSwordsPerPerson()
    {
        var recruited = Enlist(4, swords: 12m);
        Assert.Equal(new MilitaryTrainingState(2, 2, 0, 0), recruited.State);
        var result = MilitaryTraining.CancelWaiting(Rules, recruited.State, recruited.Resources,
            Swords(12m), 1, 2);
        Assert.True(result.Succeeded);
        Assert.Equal(Swords(8m), result.Resources);
        Assert.Equal(new MilitaryTrainingState(0, 2, 0, 0), result.State);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(3)]
    public void InvalidQueueCancellationCannotChangeStocks(int count)
    {
        var recruited = Enlist(3);
        var result = MilitaryTraining.CancelWaiting(Rules, recruited.State, recruited.Resources,
            Swords(100m), 1, count);
        Assert.False(result.Succeeded);
        Assert.Equal(recruited.Resources, result.Resources);
        Assert.Equal(recruited.State, result.State);
    }

    [Fact]
    public void InsufficientRefundStorageBlocksTheEntireCancellation()
    {
        var enlisted = Enlist(3, swords: 100m);
        var crowded = Swords(100m);
        var result = MilitaryTraining.CancelWaiting(Rules, enlisted.State, crowded, crowded, 1, 1);
        Assert.Equal(RecruitmentFailure.InsufficientStorage, result.Failure);
        Assert.Equal(crowded, result.Resources);
        Assert.Equal(enlisted.State, result.State);
    }

    [Fact]
    public void DischargeIsOnlyPossibleForTrainedSoldiersAndNeverRefundsWeapons()
    {
        var enlisted = Enlist(3);
        var early = MilitaryTraining.Discharge(Rules, enlisted.State, enlisted.Resources, 1, 1);
        Assert.Equal(RecruitmentFailure.InsufficientSoldiers, early.Failure);
        var ready = MilitaryTraining.Advance(Rules, enlisted.State, 1, 120);
        var discharged = MilitaryTraining.Discharge(Rules, ready, enlisted.Resources, 1, 2);
        Assert.True(discharged.Succeeded);
        Assert.Equal(new MilitaryTrainingState(0, 0, 1, 0), discharged.State);
        Assert.Equal(enlisted.Resources, discharged.Resources);
    }
}
