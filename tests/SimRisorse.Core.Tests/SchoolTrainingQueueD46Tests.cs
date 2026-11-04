using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using Xunit;

namespace SimRisorse.Core.Tests;

/// <summary>P1-D46: queued students occupy population but not a school seat until graduation.</summary>
public sealed class SchoolTrainingQueueD46Tests
{
    [Fact]
    public void ParallelSeatsStartWaitingStudentsAfterGraduationWithoutAutoAssigningResearchers()
    {
        var rules = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m),
            trainingSeconds: 120, trainingSeatsPerSchool: 1, researcherPlacesPerSchool: 2,
            milliPointsPerResearcherPerSecond: 100, pencilsPerStudent: 2);
        var stock = ResourceBundle.Create(("food", 100m), ("pencils", 8m));
        var enrolled = SchoolResearch.Enroll(rules, SchoolResearchState.Empty, stock,
            totalPopulation: 8, assignedCivilianWorkers: 0, militaryPopulation: 0,
            schools: 2, students: 4);
        Assert.True(enrolled.Succeeded);
        Assert.Equal(2, enrolled.State.TrainingStudents);
        Assert.Equal(2, enrolled.State.WaitingStudents);
        Assert.Equal(4, enrolled.State.ReservedPopulation);
        Assert.Equal(0m, enrolled.Resources["pencils"].Units);
        Assert.Equal(60m, enrolled.Resources["food"].Units);

        var first = SchoolResearch.Advance(rules, enrolled.State, ResearchLedger.Empty, 2, 120);
        Assert.Equal(2, first.State.QualifiedResearchers);
        Assert.Equal(0, first.State.ActiveResearchers);
        Assert.Equal(2, first.State.TrainingStudents);
        Assert.Equal(0, first.State.WaitingStudents);
        Assert.Equal(4, first.State.ReservedPopulation);
        Assert.True(first.State.TrainingProgressSeconds.SequenceEqual(new[] { 0, 0 }));
        Assert.Equal(0m, first.Ledger.Balance.Units);

        var second = SchoolResearch.Advance(rules, first.State, first.Ledger, 2, 120);
        Assert.Equal(4, second.State.QualifiedResearchers);
        Assert.Equal(0, second.State.TrainingStudents);
        Assert.Equal(4, second.State.ReservedPopulation);
        Assert.Equal(0, second.State.ActiveResearchers);
        Assert.Equal(0m, second.Ledger.Balance.Units);
    }
}
