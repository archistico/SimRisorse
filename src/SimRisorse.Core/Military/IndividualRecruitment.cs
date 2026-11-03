using System.Collections.Immutable;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Military;

/// <summary>Immutable, deterministic individual training. Historical group snapshots are not reinterpreted.</summary>
public static class IndividualRecruitment
{
    private static MilitaryTrainingState Fill(MilitaryTrainingState state, long seats)
    {
        var count = (int)Math.Min(state.WaitingRecruits, seats - state.TrainingRecruits);
        if (count <= 0) return state;
        var progress = state.IndividualProgressSeconds.AddRange(Enumerable.Repeat(0, count));
        return state with
        {
            WaitingRecruits = state.WaitingRecruits - count,
            TrainingRecruits = progress.Length,
            TrainingProgressSeconds = progress.Length == 0 ? 0 : progress[0],
            IndividualProgressSeconds = progress
        };
    }

    public static void Validate(MilitaryTrainingRules rules, MilitaryTrainingState state, int barracks)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentOutOfRangeException.ThrowIfNegative(barracks);
        if (!rules.IndividualTraining || !state.HasIndividualProgress || state.WaitingRecruits < 0 ||
            state.TrainingRecruits < 0 || state.AvailableSoldiers < 0 || state.EnlistedPopulation > int.MaxValue ||
            state.TrainingRecruits != state.IndividualProgressSeconds.Length ||
            state.TrainingRecruits > (long)barracks * rules.SeatsPerBarracks ||
            (state.WaitingRecruits > 0 && state.TrainingRecruits != (long)barracks * rules.SeatsPerBarracks) ||
            (barracks == 0 && state.WaitingRecruits > 0) ||
            (state.TrainingRecruits == 0 ? state.TrainingProgressSeconds != 0 :
                state.TrainingProgressSeconds != state.IndividualProgressSeconds[0]))
            throw new ArgumentException("Invalid individual recruitment checkpoint.", nameof(state));
        var previous = rules.TrainingSecondsPerGroup;
        foreach (var current in state.IndividualProgressSeconds)
        {
            if (current < 0 || current >= rules.TrainingSecondsPerGroup || current > previous)
                throw new ArgumentException("Individual progress is not ordered or is out of range.", nameof(state));
            previous = current;
        }
    }

    public static RecruitmentAttempt Enlist(MilitaryTrainingRules rules, MilitaryTrainingState state,
        ResourceBundle resources, int population, int assignedWorkers, int barracks, int count)
    {
        Validate(rules, state, barracks);
        RecruitmentAttempt Fail(RecruitmentFailure reason) => new(reason, resources, state);
        if (population < 0 || assignedWorkers < 0 || assignedWorkers > population ||
            state.EnlistedPopulation > population - (long)assignedWorkers)
            throw new ArgumentException("Invalid civilian or military population.");
        if (count <= 0) return Fail(RecruitmentFailure.InvalidQuantity);
        if (barracks == 0) return Fail(RecruitmentFailure.MissingBarracks);
        if (count > population - (long)assignedWorkers - state.EnlistedPopulation)
            return Fail(RecruitmentFailure.InsufficientPopulation);
        try
        {
            var equipment = ResourceBundle.Create((rules.SwordResourceId, checked((decimal)rules.SwordsPerRecruit * count)));
            if (!resources.Covers(equipment)) return Fail(RecruitmentFailure.InsufficientSwords);
            var queued = checked(state.WaitingRecruits + count);
            var next = Fill(state with { WaitingRecruits = queued }, (long)barracks * rules.SeatsPerBarracks);
            Validate(rules, next, barracks);
            return new(RecruitmentFailure.None, resources - equipment, next);
        }
        catch (Exception e) when (e is OverflowException or ArgumentOutOfRangeException)
        { return Fail(RecruitmentFailure.NumericLimit); }
    }

    /// <summary>Cancel from waiting queue first, then least-progressed trainees; refund must fit atomically.</summary>
    public static RecruitmentAttempt Cancel(MilitaryTrainingRules rules, MilitaryTrainingState state,
        ResourceBundle resources, ResourceBundle capacity, int barracks, int count)
    {
        Validate(rules, state, barracks);
        RecruitmentAttempt Fail(RecruitmentFailure reason) => new(reason, resources, state);
        if (count <= 0) return Fail(RecruitmentFailure.InvalidQuantity);
        if (count > (long)state.WaitingRecruits + state.TrainingRecruits)
            return Fail(RecruitmentFailure.InsufficientQueuedRecruits);
        try
        {
            var equipment = ResourceBundle.Create((rules.SwordResourceId, checked((decimal)rules.SwordsPerRecruit * count)));
            var refund = resources + equipment;
            if (!capacity.Covers(refund)) return Fail(RecruitmentFailure.InsufficientStorage);
            var fromWaiting = Math.Min(count, state.WaitingRecruits);
            var fromTraining = count - fromWaiting;
            var remaining = state.IndividualProgressSeconds.RemoveRange(state.TrainingRecruits - fromTraining, fromTraining);
            var next = Fill(state with
            {
                WaitingRecruits = state.WaitingRecruits - fromWaiting,
                IndividualProgressSeconds = remaining,
                TrainingRecruits = remaining.Length,
                TrainingProgressSeconds = remaining.Length == 0 ? 0 : remaining[0]
            }, (long)barracks * rules.SeatsPerBarracks);
            Validate(rules, next, barracks);
            return new(RecruitmentFailure.None, refund, next);
        }
        catch (Exception e) when (e is OverflowException or ArgumentOutOfRangeException)
        { return Fail(RecruitmentFailure.NumericLimit); }
    }

    public static MilitaryTrainingState Advance(MilitaryTrainingRules rules, MilitaryTrainingState state, int barracks, int seconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(seconds);
        Validate(rules, state, barracks);
        var current = state;
        var remaining = seconds;
        var seats = (long)barracks * rules.SeatsPerBarracks;
        while (remaining > 0 && current.TrainingRecruits > 0)
        {
            var progress = current.IndividualProgressSeconds;
            var step = Math.Min(remaining, rules.TrainingSecondsPerGroup - progress[0]);
            var advanced = progress.Select(value => value + step).ToImmutableArray();
            remaining -= step;
            var finished = advanced.Count(value => value >= rules.TrainingSecondsPerGroup);
            var pending = advanced.Skip(finished).ToImmutableArray();
            current = Fill(current with
            {
                AvailableSoldiers = checked(current.AvailableSoldiers + finished),
                IndividualProgressSeconds = pending,
                TrainingRecruits = pending.Length,
                TrainingProgressSeconds = pending.Length == 0 ? 0 : pending[0]
            }, seats);
        }
        Validate(rules, current, barracks);
        return current;
    }

    /// <summary>
    /// v12 -> v13: preserve every person and the fraction of a group's elapsed training.
    /// When the old group has more trainees than the new seat count, excess trainees return to the queue.
    /// Their partial progress cannot be retained without occupying a seat; this is declared explicitly.
    /// </summary>
    public static MilitaryTrainingState UpgradeLegacy(MilitaryTrainingState historical, int barracks)
    {
        MilitaryTraining.Validate(historical, MilitaryTrainingRules.SimpleBarracks, barracks);
        var rules = MilitaryTrainingRules.IndividualBarracks;
        var seats = (long)barracks * rules.SeatsPerBarracks;
        var active = (int)Math.Min(historical.TrainingRecruits, seats);
        var leftover = checked(historical.WaitingRecruits + historical.TrainingRecruits - active);
        var adjusted = Math.Min(rules.TrainingSecondsPerGroup - 1,
            historical.TrainingProgressSeconds * rules.TrainingSecondsPerGroup / MilitaryTrainingRules.SimpleBarracks.TrainingSecondsPerGroup);
        var progress = Enumerable.Repeat(adjusted, active).ToImmutableArray();
        var next = new MilitaryTrainingState(leftover, active, historical.AvailableSoldiers,
            active == 0 ? 0 : adjusted, progress);
        return Fill(next, seats);
    }
}
