using System.Collections.Immutable;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Military;

/// <summary>
/// All three groups are part of the same population; available civilians are
/// calculated by the caller after subtracting assigned workers and these people.
/// </summary>
public sealed record MilitaryTrainingState(int WaitingRecruits, int TrainingRecruits,
    int AvailableSoldiers, int TrainingProgressSeconds,
    ImmutableArray<int> IndividualProgressSeconds = default)
{
    public static MilitaryTrainingState Empty { get; } = new(0, 0, 0, 0);
    public long EnlistedPopulation => (long)WaitingRecruits + TrainingRecruits + AvailableSoldiers;
    // Default means historical group training; initialized (including empty) means individual training.
    public bool HasIndividualProgress => !IndividualProgressSeconds.IsDefault;
    public static MilitaryTrainingState IndividualEmpty { get; } = new(0, 0, 0, 0, ImmutableArray<int>.Empty);

    public bool Equals(MilitaryTrainingState? other) => other is not null &&
        WaitingRecruits == other.WaitingRecruits && TrainingRecruits == other.TrainingRecruits &&
        AvailableSoldiers == other.AvailableSoldiers && TrainingProgressSeconds == other.TrainingProgressSeconds &&
        (IndividualProgressSeconds.IsDefault && other.IndividualProgressSeconds.IsDefault ||
         !IndividualProgressSeconds.IsDefault && !other.IndividualProgressSeconds.IsDefault &&
         IndividualProgressSeconds.SequenceEqual(other.IndividualProgressSeconds));

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(WaitingRecruits); hash.Add(TrainingRecruits);
        hash.Add(AvailableSoldiers); hash.Add(TrainingProgressSeconds);
        hash.Add(IndividualProgressSeconds.IsDefault);
        if (!IndividualProgressSeconds.IsDefault)
            foreach (var item in IndividualProgressSeconds) hash.Add(item);
        return hash.ToHashCode();
    }
}

public enum RecruitmentFailure
{
    None, InvalidQuantity, MissingBarracks, InsufficientPopulation, InsufficientSwords, NumericLimit,
    InsufficientSoldiers, InsufficientQueuedRecruits, InsufficientStorage, DischargeUnavailable
}

/// <summary>A rejected order returns its original immutable inputs unchanged.</summary>
public sealed record RecruitmentAttempt(RecruitmentFailure Failure, ResourceBundle Resources,
    MilitaryTrainingState State)
{
    public bool Succeeded => Failure == RecruitmentFailure.None;
}

/// <summary>
/// Pure, deterministic group-based recruitment. Does not mutate game sessions,
/// create residents or advance time while a game is paused. The v10 adapter
/// will call these operations atomically from the session aggregate.
/// </summary>
public static class MilitaryTraining
{
    public static RecruitmentAttempt Discharge(MilitaryTrainingRules rules, MilitaryTrainingState state,
        ResourceBundle resources, int barracks, int count)
    {
        ArgumentNullException.ThrowIfNull(resources);
        Validate(state, rules, barracks);
        if (rules.IndividualTraining) return new(RecruitmentFailure.DischargeUnavailable, resources, state);
        if (count <= 0) return new(RecruitmentFailure.InvalidQuantity, resources, state);
        if (count > state.AvailableSoldiers) return new(RecruitmentFailure.InsufficientSoldiers, resources, state);
        return new(RecruitmentFailure.None, resources,
            state with { AvailableSoldiers = state.AvailableSoldiers - count });
    }

    /// <summary>Only waiting recruits may be cancelled; sword refunds must fit entirely.</summary>
    public static RecruitmentAttempt CancelWaiting(MilitaryTrainingRules rules, MilitaryTrainingState state,
        ResourceBundle resources, ResourceBundle capacity, int barracks, int count)
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(capacity);
        Validate(state, rules, barracks);
        if (rules.IndividualTraining)
            return IndividualRecruitment.Cancel(rules, state, resources, capacity, barracks, count);
        RecruitmentAttempt Fail(RecruitmentFailure failure) => new(failure, resources, state);
        if (count <= 0) return Fail(RecruitmentFailure.InvalidQuantity);
        if (count > state.WaitingRecruits) return Fail(RecruitmentFailure.InsufficientQueuedRecruits);
        ResourceBundle refund;
        ResourceBundle refundedResources;
        try
        {
            refund = ResourceBundle.Create((rules.SwordResourceId, (decimal)rules.SwordsPerRecruit * count));
            refundedResources = resources + refund;
        }
        catch (Exception exception) when (exception is OverflowException or ArgumentOutOfRangeException)
        { return Fail(RecruitmentFailure.NumericLimit); }
        if (!capacity.Covers(refundedResources)) return Fail(RecruitmentFailure.InsufficientStorage);
        return new(RecruitmentFailure.None, refundedResources,
            state with { WaitingRecruits = state.WaitingRecruits - count });
    }

    public static RecruitmentAttempt Enlist(MilitaryTrainingRules rules, MilitaryTrainingState state,
        ResourceBundle resources, int population, int assignedWorkers, int barracks, int count)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resources);
        Validate(state, rules, barracks);
        if (rules.IndividualTraining)
            return IndividualRecruitment.Enlist(rules, state, resources, population, assignedWorkers, barracks, count);
        if (population < 0 || assignedWorkers < 0 || assignedWorkers > population ||
            state.EnlistedPopulation > population - (long)assignedWorkers)
            throw new ArgumentException("Population and existing military assignments are inconsistent.");

        RecruitmentAttempt Fail(RecruitmentFailure failure) => new(failure, resources, state);
        if (count <= 0) return Fail(RecruitmentFailure.InvalidQuantity);
        if (barracks == 0) return Fail(RecruitmentFailure.MissingBarracks);
        if (count > population - (long)assignedWorkers - state.EnlistedPopulation)
            return Fail(RecruitmentFailure.InsufficientPopulation);

        ResourceBundle swords;
        try { swords = ResourceBundle.Create((rules.SwordResourceId, (decimal)rules.SwordsPerRecruit * count)); }
        catch (Exception exception) when (exception is OverflowException or ArgumentOutOfRangeException)
        { return Fail(RecruitmentFailure.NumericLimit); }
        if (!resources.Covers(swords)) return Fail(RecruitmentFailure.InsufficientSwords);

        MilitaryTrainingState next;
        try
        {
            var seats = (long)barracks * rules.SeatsPerBarracks;
            var start = state.TrainingRecruits == 0 && state.WaitingRecruits == 0
                ? (int)Math.Min(count, seats) : 0;
            next = state with
            {
                TrainingRecruits = checked(state.TrainingRecruits + start),
                WaitingRecruits = checked(state.WaitingRecruits + count - start)
            };
        }
        catch (OverflowException) { return Fail(RecruitmentFailure.NumericLimit); }
        return new(RecruitmentFailure.None, resources - swords, next);
    }

    /// <summary>
    /// A new request never joins a running group. Groups start immediately at
    /// completion boundaries, even when exactly zero seconds remain to advance.
    /// </summary>
    public static MilitaryTrainingState Advance(MilitaryTrainingRules rules,
        MilitaryTrainingState state, int barracks, int seconds)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(state);
        if (seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
        Validate(state, rules, barracks);
        if (rules.IndividualTraining) return IndividualRecruitment.Advance(rules, state, barracks, seconds);
        if (seconds == 0 || (state.WaitingRecruits == 0 && state.TrainingRecruits == 0)) return state;

        var remaining = seconds;
        var current = state;
        var seats = (long)barracks * rules.SeatsPerBarracks;
        while (remaining > 0)
        {
            if (current.TrainingRecruits == 0)
            {
                if (current.WaitingRecruits == 0) break;
                current = StartNextGroup(current, seats);
            }

            var required = rules.TrainingSecondsPerGroup - current.TrainingProgressSeconds;
            if (remaining < required)
                return current with { TrainingProgressSeconds = current.TrainingProgressSeconds + remaining };

            remaining -= required;
            current = current with
            {
                AvailableSoldiers = checked(current.AvailableSoldiers + current.TrainingRecruits),
                TrainingRecruits = 0,
                TrainingProgressSeconds = 0
            };
            // The next group starts at the same boundary, never one second late.
            if (current.WaitingRecruits > 0) current = StartNextGroup(current, seats);
        }
        return current;
    }

    private static MilitaryTrainingState StartNextGroup(MilitaryTrainingState state, long seats)
    {
        var size = (int)Math.Min(state.WaitingRecruits, seats);
        return state with { WaitingRecruits = state.WaitingRecruits - size, TrainingRecruits = size };
    }

    public static void Validate(MilitaryTrainingState state, MilitaryTrainingRules rules, int barracks)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentOutOfRangeException.ThrowIfNegative(barracks);
        if (rules.IndividualTraining) { IndividualRecruitment.Validate(rules, state, barracks); return; }
        if (state.HasIndividualProgress) throw new ArgumentException("Individual progress cannot appear in historical military rules.", nameof(state));
        if (state.WaitingRecruits < 0 || state.TrainingRecruits < 0 || state.AvailableSoldiers < 0 ||
            state.EnlistedPopulation > int.MaxValue || state.TrainingProgressSeconds < 0 ||
            state.TrainingProgressSeconds >= rules.TrainingSecondsPerGroup ||
            (state.TrainingRecruits == 0 && state.TrainingProgressSeconds != 0) ||
            (state.WaitingRecruits > 0 && state.TrainingRecruits == 0) ||
            state.TrainingRecruits > (long)barracks * rules.SeatsPerBarracks ||
            (barracks == 0 && state.WaitingRecruits != 0))
            throw new ArgumentException("Military group, capacity or training progress is invalid.", nameof(state));
    }
}
