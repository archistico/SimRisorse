using System.Collections.Immutable;
using System.Text.Json;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class SchoolResearchTests
{
    private static readonly SchoolResearchRules Rules = new("food", ResourceAmount.FromUnits(10m),
        trainingSeconds: 120, trainingSeatsPerSchool: 1, researcherPlacesPerSchool: 2,
        milliPointsPerResearcherPerSecond: 100);

    [Fact]
    public void EnrollmentUsesAvailableResidentsConsumesFoodAndReservesStudents()
    {
        var stock = ResourceBundle.Create(("food", 100m));
        var enrolled = SchoolResearch.Enroll(Rules, SchoolResearchState.Empty, stock,
            totalPopulation: 8, assignedCivilianWorkers: 3, militaryPopulation: 1, schools: 2, students: 3);
        Assert.True(enrolled.Succeeded);
        Assert.Equal(70m, enrolled.Resources["food"].Units);
        Assert.Equal(1, enrolled.State.WaitingStudents);
        Assert.Equal(2, enrolled.State.TrainingStudents);
        Assert.Equal(3, enrolled.State.ReservedPopulation);
        Assert.Equal(0, enrolled.State.QualifiedResearchers);
        Assert.Equal(100m, stock["food"].Units);
        Assert.Equal(0, SchoolResearchState.Empty.ReservedPopulation);
        var noMore = SchoolResearch.Enroll(Rules, enrolled.State, enrolled.Resources,
            8, 3, 1, 2, 2);
        Assert.Equal(SchoolActionFailure.InsufficientPopulation, noMore.Failure);
        Assert.Same(enrolled.Resources, noMore.Resources);
        Assert.Same(enrolled.State, noMore.State);
    }

    [Fact]
    public void CannotEnrollWithoutASchoolOrWithoutEnoughFood()
    {
        var stock = ResourceBundle.Create(("food", 9m));
        var missing = SchoolResearch.Enroll(Rules, SchoolResearchState.Empty, stock, 8, 0, 0, 0, 1);
        Assert.Equal(SchoolActionFailure.MissingSchool, missing.Failure);
        var food = SchoolResearch.Enroll(Rules, SchoolResearchState.Empty, stock, 8, 0, 0, 1, 1);
        Assert.Equal(SchoolActionFailure.InsufficientFood, food.Failure);
        var invalid = SchoolResearch.Enroll(Rules, SchoolResearchState.Empty, stock, 8, 0, 0, 1, 0);
        Assert.Equal(SchoolActionFailure.InvalidQuantity, invalid.Failure);
    }

    [Fact]
    public void SchoolSeatsTrainInParallelButQueueRemainsSequential()
    {
        var initial = SchoolResearch.Enroll(Rules, SchoolResearchState.Empty,
            ResourceBundle.Create(("food", 50m)), 6, 0, 0, 2, 3).State;
        var half = SchoolResearch.Advance(Rules, initial, ResearchLedger.Empty, 2, 60);
        Assert.True(half.State.TrainingProgressSeconds.SequenceEqual(new[] { 60, 60 }));
        Assert.Equal(1, half.State.WaitingStudents);
        var complete = SchoolResearch.Advance(Rules, half.State, half.Ledger, 2, 60);
        Assert.Equal(2, complete.State.QualifiedResearchers);
        Assert.Equal(0, complete.State.WaitingStudents);
        Assert.True(complete.State.TrainingProgressSeconds.SequenceEqual(new[] { 0 }));
        Assert.Equal(0, complete.State.ActiveResearchers);
        Assert.Equal(0m, complete.Ledger.Balance.Units); // not auto-assigned
        var another = SchoolResearch.Advance(Rules, complete.State, complete.Ledger, 2, 120);
        Assert.Equal(3, another.State.QualifiedResearchers);
        Assert.Equal(0, another.State.TrainingStudents);
        Assert.Equal(3, another.State.ReservedPopulation);
    }

    [Fact]
    public void TrainedPeopleOnlyProduceAfterVoluntaryAssignment()
    {
        var initial = SchoolResearch.Enroll(Rules, SchoolResearchState.Empty,
            ResourceBundle.Create(("food", 40m)), 5, 0, 0, 2, 2).State;
        var graduated = SchoolResearch.Advance(Rules, initial, ResearchLedger.Empty, 2, 120);
        var assigned = SchoolResearch.Assign(Rules, graduated.State, 2, 2);
        Assert.True(assigned.Succeeded);
        var accrued = SchoolResearch.Advance(Rules, assigned.State, graduated.Ledger, 2, 500);
        Assert.Equal(100m, accrued.Ledger.Balance.Units);
        Assert.Empty(accrued.Ledger.UnlockedTechnologies);
        Assert.Equal(2, accrued.State.ActiveResearchers);
        Assert.Equal(2, accrued.State.ReservedPopulation);
    }

    [Fact]
    public void QualifiedPeopleCanBeUnassignedAndThenReturnedToCivilianPool()
    {
        var state = new SchoolResearchState(0, ImmutableArray<int>.Empty, 3, 2);
        var before = SchoolResearch.ReturnToCivilian(Rules, state, 2, 2);
        Assert.Equal(SchoolActionFailure.InsufficientQualifiedResearchers, before.Failure);
        Assert.Same(state, before.State);
        var unassigned = SchoolResearch.Unassign(Rules, state, 2, 1);
        Assert.True(unassigned.Succeeded);
        var civilian = SchoolResearch.ReturnToCivilian(Rules, unassigned.State, 2, 2);
        Assert.True(civilian.Succeeded);
        Assert.Equal(1, civilian.State.ReservedPopulation);
        Assert.Equal(1, civilian.State.QualifiedResearchers);
        Assert.Equal(1, civilian.State.ActiveResearchers);
        Assert.Equal(3, state.ReservedPopulation);
    }

    [Fact]
    public void StaffingCannotExceedSchoolResearchPlacesOrQualifiedPeople()
    {
        var state = new SchoolResearchState(0, ImmutableArray<int>.Empty, 5, 0);
        var tooManyPlaces = SchoolResearch.Assign(Rules, state, 1, 3);
        Assert.Equal(SchoolActionFailure.InsufficientResearchPlaces, tooManyPlaces.Failure);
        Assert.Same(state, tooManyPlaces.State);
        var tooManyScientists = SchoolResearch.Assign(Rules, state with { QualifiedResearchers = 1 }, 1, 2);
        Assert.Equal(SchoolActionFailure.InsufficientQualifiedResearchers, tooManyScientists.Failure);
        Assert.Equal(SchoolActionFailure.InvalidQuantity, SchoolResearch.Assign(Rules, state, 1, 0).Failure);
    }

    [Fact]
    public void CancelWaitingThenLeastAdvancedStudentsRefundsWithoutPartialMutation()
    {
        var enrolled = SchoolResearch.Enroll(Rules, SchoolResearchState.Empty,
            ResourceBundle.Create(("food", 100m)), 10, 0, 0, 2, 3);
        var mid = SchoolResearch.Advance(Rules, enrolled.State, ResearchLedger.Empty, 2, 35).State;
        var insufficientStorage = SchoolResearch.CancelTraining(Rules, mid, enrolled.Resources,
            ResourceBundle.Create(("food", 75m)), 2, 2);
        Assert.Equal(SchoolActionFailure.InsufficientStorage, insufficientStorage.Failure);
        Assert.Same(mid, insufficientStorage.State);
        Assert.Same(enrolled.Resources, insufficientStorage.Resources);
        var success = SchoolResearch.CancelTraining(Rules, mid, enrolled.Resources,
            ResourceBundle.Create(("food", 100m)), 2, 2);
        Assert.True(success.Succeeded);
        Assert.Equal(90m, success.Resources["food"].Units);
        Assert.Equal(1, success.State.ReservedPopulation);
        Assert.True(success.State.TrainingProgressSeconds.SequenceEqual(new[] { 35 }));
    }

    [Fact]
    public void SaveRoundTripPreservesCreditsTechnologiesAndPartialTraining()
    {
        var started = SchoolResearch.Enroll(Rules, SchoolResearchState.Empty,
            ResourceBundle.Create(("food", 100m)), 8, 1, 1, 2, 3);
        var partial = SchoolResearch.Advance(Rules, started.State, ResearchLedger.Empty, 2, 41);
        var ledger = ResearchLedger.Empty.Accrue(1, 1500, 100);
        var spent = TechnologyPurchases.TryUnlock(TechnologyCatalog.ProposedTierTree, ledger,
            "lavorazioni-1", new HashSet<string>(StringComparer.Ordinal));
        Assert.True(spent.Succeeded);
        var checkpoint = ResearchPrototypeCheckpoints.Capture(spent.Ledger, partial.State, Rules, 2);
        var json = JsonSerializer.Serialize(checkpoint);
        var imported = JsonSerializer.Deserialize<ResearchPrototypeCheckpoint>(json)!;
        var restored = ResearchPrototypeCheckpoints.Restore(imported,
            TechnologyCatalog.ProposedTierTree, Rules, 2, 8, 1, 1);
        Assert.Equal(50m, restored.Ledger.Balance.Units);
        Assert.Contains("lavorazioni-1", restored.Ledger.UnlockedTechnologies);
        Assert.Equal(1, restored.School.WaitingStudents);
        Assert.True(restored.School.TrainingProgressSeconds.SequenceEqual(new[] { 41, 41 }));
        Assert.Equal(3, restored.School.ReservedPopulation);
    }

    [Fact]
    public void BrokenCheckpointsAreRejectedRatherThanDuplicatingPopulation()
    {
        var valid = ResearchPrototypeCheckpoints.Capture(ResearchLedger.Empty,
            new SchoolResearchState(1, [11], 1, 1), Rules, 1);
        Assert.Throws<ArgumentException>(() => ResearchPrototypeCheckpoints.Restore(
            valid, TechnologyCatalog.ProposedTierTree, Rules, 1, 2, 1, 0));
        Assert.Throws<ArgumentException>(() => ResearchPrototypeCheckpoints.Restore(
            valid with { ModelId = "civil-growth-13" }, TechnologyCatalog.ProposedTierTree, Rules, 1, 5, 0, 0));
        Assert.Throws<ArgumentException>(() => ResearchPrototypeCheckpoints.Restore(
            valid with { TrainingProgressSeconds = [120] }, TechnologyCatalog.ProposedTierTree, Rules, 1, 5, 0, 0));
        Assert.Throws<ArgumentException>(() => ResearchPrototypeCheckpoints.Restore(
            valid with { UnlockedTechnologyIds = ["industria-2"] }, TechnologyCatalog.ProposedTierTree, Rules, 1, 5, 0, 0));
    }

    [Fact]
    public void SplitAndCombinedAdvanceAgreeWithoutLossOfFractionalResearch()
    {
        var initial = SchoolResearch.Enroll(Rules, SchoolResearchState.Empty,
            ResourceBundle.Create(("food", 100m)), 8, 0, 0, 2, 3).State;
        var once = SchoolResearch.Advance(Rules, initial, ResearchLedger.Empty, 2, 240);
        var step1 = SchoolResearch.Advance(Rules, initial, ResearchLedger.Empty, 2, 97);
        var step2 = SchoolResearch.Advance(Rules, step1.State, step1.Ledger, 2, 143);
        Assert.Equal(once.State.WaitingStudents, step2.State.WaitingStudents);
        Assert.Equal(once.State.QualifiedResearchers, step2.State.QualifiedResearchers);
        Assert.True(once.State.TrainingProgressSeconds.SequenceEqual(step2.State.TrainingProgressSeconds));
        Assert.Equal(once.Ledger.Balance, step2.Ledger.Balance);
    }

    [Fact]
    public void InvalidTrainingAndResearchValuesAreNotSilentlyNormalized()
    {
        Assert.Throws<ArgumentException>(() => SchoolResearch.Validate(Rules,
            new SchoolResearchState(0, [3, 50], 0, 0), 2));
        Assert.Throws<ArgumentException>(() => SchoolResearch.Validate(Rules,
            new SchoolResearchState(1, [0], 0, 0), 2));
        Assert.Throws<ArgumentException>(() => SchoolResearch.Validate(Rules,
            new SchoolResearchState(0, ImmutableArray<int>.Empty, 1, 2), 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SchoolResearchRules("food", ResourceAmount.FromUnits(1m),
            120, 1, 2, 0));
        Assert.Throws<ArgumentException>(() => SchoolResearch.Enroll(Rules, SchoolResearchState.Empty,
            ResourceBundle.Create(("food", 100m)), 2, 2, 1, 1, 1));
    }
}
