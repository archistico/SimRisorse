using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;

namespace SimRisorse.Simulation;

/// <summary>
/// M5.3c controlled capacity experiment: all swords and civilians are supplied up front.
/// This isolates the training queue; it is NOT an estimate of the time to fund an army
/// from the first-turn colony. The self-funded campaign remains --strategy military.
/// </summary>
public static class MilitaryCapacityBenchmark
{
    public sealed record Profile(int Barracks, int TargetSoldiers, int SwordsSpent,
        int PlanksForBarracks, int StoneForBarracks, int TrainingSeconds,
        int FirstSoldierSecond, int SoldiersAt60Seconds, int CiviliansReserved,
        int FreeCiviliansAfterEnlistment, int PeakWaitingRecruits);

    public sealed record Report(string RulesVersion, string Model,
        int TrainingSecondsPerRecruit, int SeatsPerBarracks, int SwordsPerRecruit,
        int ReferencePopulation, int ReferenceAssignedWorkers,
        string FundingAssumption, IReadOnlyList<Profile> Profiles);

    public static Report Run()
    {
        var rules = MilitaryTrainingRules.IndividualBarracks;
        const int population = 60;
        const int assignedWorkers = 15;
        var profiles = new List<Profile>();
        foreach (var barracks in new[] { 1, 2, 4 })
        foreach (var target in new[] { 5, 10, 20 })
        {
            var startingResources = ResourceBundle.Create((rules.SwordResourceId, 1000m));
            var start = MilitaryTraining.Enlist(rules, MilitaryTrainingState.IndividualEmpty,
                startingResources, population, assignedWorkers, barracks, target);
            if (!start.Succeeded) throw new InvalidOperationException(
                $"Controlled enlistment failed: {barracks} barracks, {target} people ({start.Failure}).");
            var swordsSpent = startingResources[rules.SwordResourceId].Units -
                start.Resources[rules.SwordResourceId].Units;
            if (swordsSpent != (decimal)target * rules.SwordsPerRecruit ||
                start.State.EnlistedPopulation != target)
                throw new InvalidOperationException("Enlistment did not conserve people or swords.");

            var expectedSeconds = ((target + barracks * rules.SeatsPerBarracks - 1) /
                (barracks * rules.SeatsPerBarracks)) * rules.TrainingSecondsPerGroup;
            var state = start.State;
            var firstSoldier = 0;
            var atSixty = 0;
            for (var second = 1; second <= expectedSeconds; second++)
            {
                state = MilitaryTraining.Advance(rules, state, barracks, 1);
                if (state.EnlistedPopulation != target)
                    throw new InvalidOperationException("Training changed the number of residents.");
                if (firstSoldier == 0 && state.AvailableSoldiers > 0) firstSoldier = second;
                if (second == 60) atSixty = state.AvailableSoldiers;
                if (second < expectedSeconds && state.AvailableSoldiers == target)
                    throw new InvalidOperationException("Training completed before the expected boundary.");
            }
            if (expectedSeconds < 60)
                atSixty = MilitaryTraining.Advance(rules, start.State, barracks, 60).AvailableSoldiers;
            var singleStep = MilitaryTraining.Advance(rules, start.State, barracks, expectedSeconds);
            if (!singleStep.Equals(state) || state.AvailableSoldiers != target ||
                state.WaitingRecruits != 0 || state.TrainingRecruits != 0 ||
                firstSoldier != rules.TrainingSecondsPerGroup)
                throw new InvalidOperationException("Training is not deterministic or does not finish at the expected time.");
            profiles.Add(new Profile(barracks, target, (int)swordsSpent,
                checked((int)rules.BarracksCost["planks"].Units * barracks),
                checked((int)rules.BarracksCost["stone"].Units * barracks),
                expectedSeconds, firstSoldier, atSixty, target,
                population - assignedWorkers - target, start.State.WaitingRecruits));
        }
        return new Report("civil-growth-13", "controlled-training-only",
            rules.TrainingSecondsPerGroup, rules.SeatsPerBarracks, rules.SwordsPerRecruit,
            population, assignedWorkers,
            "All swords and civilians available at t=0; excludes production, construction, housing, and food supply.",
            profiles);
    }
}
