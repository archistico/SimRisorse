using System.Collections.Immutable;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Research;

/// <summary>Candidate rules supplied by the future v14 ruleset, never by a v13 scenario.</summary>
public sealed class SchoolResearchRules
{
    public SchoolResearchRules(string foodResourceId, ResourceAmount foodPerStudent, int trainingSeconds,
        int trainingSeatsPerSchool, int researcherPlacesPerSchool, long milliPointsPerResearcherPerSecond,
        int pencilsPerStudent = 0)
    {
        ContentIdentifier.Validate(foodResourceId, nameof(foodResourceId));
        if (foodPerStudent == ResourceAmount.Zero) throw new ArgumentOutOfRangeException(nameof(foodPerStudent));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(trainingSeconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(trainingSeatsPerSchool);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(researcherPlacesPerSchool);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(milliPointsPerResearcherPerSecond);
        ArgumentOutOfRangeException.ThrowIfNegative(pencilsPerStudent);
        FoodResourceId = foodResourceId;
        FoodPerStudent = foodPerStudent;
        TrainingSeconds = trainingSeconds;
        TrainingSeatsPerSchool = trainingSeatsPerSchool;
        ResearcherPlacesPerSchool = researcherPlacesPerSchool;
        MilliPointsPerResearcherPerSecond = milliPointsPerResearcherPerSecond;
        PencilsPerStudent = pencilsPerStudent;
    }

    public string FoodResourceId { get; }
    public ResourceAmount FoodPerStudent { get; }
    public int TrainingSeconds { get; }
    public int TrainingSeatsPerSchool { get; }
    public int ResearcherPlacesPerSchool { get; }
    public long MilliPointsPerResearcherPerSecond { get; }
    public int PencilsPerStudent { get; }

    public ResourceBundle TrainingCost(int students)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(students);
        var materials = new List<KeyValuePair<string, ResourceAmount>>
        {
            KeyValuePair.Create(FoodResourceId, FoodPerStudent * students)
        };
        if (PencilsPerStudent > 0)
            materials.Add(KeyValuePair.Create(V14SchoolSupplies.ResourceId,
                ResourceAmount.FromUnits(checked((decimal)PencilsPerStudent * students))));
        return ResourceBundle.FromAmounts(materials);
    }
}

/// <summary>
/// All students and qualified researchers remain inhabitants, but are reserved from civil work
/// and recruitment. Active researchers are a subset of QualifiedResearchers, not extra people.
/// Progress order is from most advanced to least advanced.
/// </summary>
public sealed record SchoolResearchState(int WaitingStudents, ImmutableArray<int> TrainingProgressSeconds,
    int QualifiedResearchers, int ActiveResearchers)
{
    public static SchoolResearchState Empty { get; } = new(0, ImmutableArray<int>.Empty, 0, 0);
    public int TrainingStudents => TrainingProgressSeconds.Length;
    public int ReservedPopulation => checked(checked(WaitingStudents + TrainingStudents) + QualifiedResearchers);
}

public enum SchoolActionFailure
{
    None, InvalidQuantity, MissingSchool, InsufficientPopulation, InsufficientFood, InsufficientPencils,
    InsufficientStudents, InsufficientQualifiedResearchers, InsufficientResearchPlaces,
    InsufficientStorage, NumericLimit
}

public sealed record SchoolEnrollmentResult(SchoolActionFailure Failure, ResourceBundle Resources, SchoolResearchState State)
{
    public bool Succeeded => Failure == SchoolActionFailure.None;
}

public sealed record SchoolStaffingResult(SchoolActionFailure Failure, SchoolResearchState State)
{
    public bool Succeeded => Failure == SchoolActionFailure.None;
}

public sealed record SchoolAdvanceResult(SchoolResearchState State, ResearchLedger Ledger);

/// <summary>
/// Pure isolated E3 prototype. Neither GameSession nor SQLite calls this yet.
/// Each successful command returns a complete replacement value; failures leave inputs intact.
/// </summary>
public static class SchoolResearch
{
    public static void Validate(SchoolResearchRules rules, SchoolResearchState state, int schools)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentOutOfRangeException.ThrowIfNegative(schools);
        var capacity = checked((long)schools * rules.TrainingSeatsPerSchool);
        var researchCapacity = checked((long)schools * rules.ResearcherPlacesPerSchool);
        if (state.WaitingStudents < 0 || state.QualifiedResearchers < 0 || state.ActiveResearchers < 0 ||
            state.TrainingProgressSeconds.IsDefault || state.TrainingStudents > capacity ||
            state.ActiveResearchers > state.QualifiedResearchers || state.ActiveResearchers > researchCapacity ||
            (state.WaitingStudents > 0 && state.TrainingStudents != capacity) ||
            (schools == 0 && state.WaitingStudents > 0))
            throw new ArgumentException("Invalid School research state.", nameof(state));
        var previous = rules.TrainingSeconds;
        foreach (var progress in state.TrainingProgressSeconds)
        {
            if (progress < 0 || progress >= rules.TrainingSeconds || progress > previous)
                throw new ArgumentException("School progress is unordered or out of bounds.", nameof(state));
            previous = progress;
        }
        try { _ = state.ReservedPopulation; }
        catch (OverflowException e) { throw new ArgumentException("School population exceeds limits.", nameof(state), e); }
    }

    public static SchoolEnrollmentResult Enroll(SchoolResearchRules rules, SchoolResearchState state,
        ResourceBundle resources, int totalPopulation, int assignedCivilianWorkers,
        int militaryPopulation, int schools, int students)
    {
        ArgumentNullException.ThrowIfNull(resources);
        Validate(rules, state, schools);
        if (totalPopulation < 0 || assignedCivilianWorkers < 0 || militaryPopulation < 0 ||
            (long)assignedCivilianWorkers + militaryPopulation + state.ReservedPopulation > totalPopulation)
            throw new ArgumentException("Inconsistent population reservations.");
        SchoolEnrollmentResult Fail(SchoolActionFailure reason) => new(reason, resources, state);
        if (students <= 0) return Fail(SchoolActionFailure.InvalidQuantity);
        if (schools == 0) return Fail(SchoolActionFailure.MissingSchool);
        if (students > (long)totalPopulation - assignedCivilianWorkers - militaryPopulation - state.ReservedPopulation)
            return Fail(SchoolActionFailure.InsufficientPopulation);
        try
        {
            var cost = rules.TrainingCost(students);
            if (resources[rules.FoodResourceId].Milliunits < cost[rules.FoodResourceId].Milliunits)
                return Fail(SchoolActionFailure.InsufficientFood);
            if (resources[V14SchoolSupplies.ResourceId].Milliunits < cost[V14SchoolSupplies.ResourceId].Milliunits)
                return Fail(SchoolActionFailure.InsufficientPencils);
            var queued = checked(state.WaitingStudents + students);
            var next = Fill(state with { WaitingStudents = queued }, (long)schools * rules.TrainingSeatsPerSchool);
            Validate(rules, next, schools);
            return new(SchoolActionFailure.None, resources - cost, next);
        }
        catch (Exception e) when (e is OverflowException or ArgumentOutOfRangeException)
        { return Fail(SchoolActionFailure.NumericLimit); }
    }

    /// <summary>Cancel waiting students first, then the least progressed trainees. Full food refund must fit storage.</summary>
    public static SchoolEnrollmentResult CancelTraining(SchoolResearchRules rules, SchoolResearchState state,
        ResourceBundle resources, ResourceBundle storageCapacity, int schools, int students,
        int pencilsRefundableStudents = -1)
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(storageCapacity);
        Validate(rules, state, schools);
        SchoolEnrollmentResult Fail(SchoolActionFailure reason) => new(reason, resources, state);
        if (students <= 0 || pencilsRefundableStudents > students || pencilsRefundableStudents < -1)
            return Fail(SchoolActionFailure.InvalidQuantity);
        if (students > (long)state.WaitingStudents + state.TrainingStudents)
            return Fail(SchoolActionFailure.InsufficientStudents);
        try
        {
            // Older v14 saves may contain students enrolled before pencils existed.
            // Only documented prepaid students are eligible for a pencil refund.
            var pencilCount = pencilsRefundableStudents < 0 ? students : pencilsRefundableStudents;
            var refund = resources + ResourceBundle.FromAmounts([
                KeyValuePair.Create(rules.FoodResourceId, rules.FoodPerStudent * students)]);
            if (rules.PencilsPerStudent > 0 && pencilCount > 0)
                refund += ResourceBundle.Create((V14SchoolSupplies.ResourceId,
                    checked((decimal)rules.PencilsPerStudent * pencilCount)));
            if (!storageCapacity.Covers(refund)) return Fail(SchoolActionFailure.InsufficientStorage);
            var fromWaiting = Math.Min(students, state.WaitingStudents);
            var fromTraining = students - fromWaiting;
            var remaining = state.TrainingProgressSeconds.RemoveRange(state.TrainingStudents - fromTraining, fromTraining);
            var next = Fill(state with
            {
                WaitingStudents = state.WaitingStudents - fromWaiting,
                TrainingProgressSeconds = remaining
            }, (long)schools * rules.TrainingSeatsPerSchool);
            Validate(rules, next, schools);
            return new(SchoolActionFailure.None, refund, next);
        }
        catch (Exception e) when (e is OverflowException or ArgumentOutOfRangeException)
        { return Fail(SchoolActionFailure.NumericLimit); }
    }

    public static SchoolStaffingResult Assign(SchoolResearchRules rules, SchoolResearchState state,
        int schools, int researchers)
    {
        Validate(rules, state, schools);
        if (researchers <= 0) return new(SchoolActionFailure.InvalidQuantity, state);
        if (researchers > state.QualifiedResearchers - state.ActiveResearchers)
            return new(SchoolActionFailure.InsufficientQualifiedResearchers, state);
        if ((long)state.ActiveResearchers + researchers > (long)schools * rules.ResearcherPlacesPerSchool)
            return new(SchoolActionFailure.InsufficientResearchPlaces, state);
        return new(SchoolActionFailure.None, state with { ActiveResearchers = checked(state.ActiveResearchers + researchers) });
    }

    public static SchoolStaffingResult Unassign(SchoolResearchRules rules, SchoolResearchState state,
        int schools, int researchers)
    {
        Validate(rules, state, schools);
        if (researchers <= 0) return new(SchoolActionFailure.InvalidQuantity, state);
        if (researchers > state.ActiveResearchers)
            return new(SchoolActionFailure.InsufficientQualifiedResearchers, state);
        return new(SchoolActionFailure.None, state with { ActiveResearchers = state.ActiveResearchers - researchers });
    }

    /// <summary>Return only inactive, already qualified researchers to the civilian pool.</summary>
    public static SchoolStaffingResult ReturnToCivilian(SchoolResearchRules rules, SchoolResearchState state,
        int schools, int researchers)
    {
        Validate(rules, state, schools);
        if (researchers <= 0) return new(SchoolActionFailure.InvalidQuantity, state);
        if (researchers > state.QualifiedResearchers - state.ActiveResearchers)
            return new(SchoolActionFailure.InsufficientQualifiedResearchers, state);
        return new(SchoolActionFailure.None, state with { QualifiedResearchers = state.QualifiedResearchers - researchers });
    }

    /// <summary>
    /// Training is parallel across seats and sequential per seat. New graduates stay idle until
    /// the player assigns them. Thus the research rate is constant during this Advance call.
    /// </summary>
    public static SchoolAdvanceResult Advance(SchoolResearchRules rules, SchoolResearchState state,
        ResearchLedger ledger, int schools, int elapsedSeconds)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        ArgumentOutOfRangeException.ThrowIfNegative(elapsedSeconds);
        Validate(rules, state, schools);
        var remaining = elapsedSeconds;
        var current = state;
        var seats = (long)schools * rules.TrainingSeatsPerSchool;
        while (remaining > 0 && current.TrainingStudents > 0)
        {
            var progress = current.TrainingProgressSeconds;
            var step = Math.Min(remaining, rules.TrainingSeconds - progress[0]);
            var advanced = progress.Select(value => value + step).ToImmutableArray();
            remaining -= step;
            var completed = advanced.Count(value => value >= rules.TrainingSeconds);
            var pending = advanced.Skip(completed).ToImmutableArray();
            current = Fill(current with
            {
                TrainingProgressSeconds = pending,
                QualifiedResearchers = checked(current.QualifiedResearchers + completed)
            }, seats);
        }
        Validate(rules, current, schools);
        return new(current, ledger.Accrue(state.ActiveResearchers, elapsedSeconds,
            rules.MilliPointsPerResearcherPerSecond));
    }

    private static SchoolResearchState Fill(SchoolResearchState state, long capacity)
    {
        var take = (int)Math.Min((long)state.WaitingStudents, capacity - state.TrainingStudents);
        if (take <= 0) return state;
        return state with
        {
            WaitingStudents = state.WaitingStudents - take,
            TrainingProgressSeconds = state.TrainingProgressSeconds.AddRange(Enumerable.Repeat(0, take))
        };
    }
}
