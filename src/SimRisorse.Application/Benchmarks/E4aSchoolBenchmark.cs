using System.Globalization;
using System.Text;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Benchmarks;

/// <summary>
/// E4a-01: measured (not simulated-on-paper) comparison of one versus two schools.
/// Both runs start from the same declared controlled stock injection and use genuine
/// v14 construction, crafting, enrollment and Advance commands. They are NOT evidence
/// that school construction is reachable from an unmodified natural campaign.
/// </summary>
public static class E4aSchoolBenchmark
{
    private const int TargetGraduates = 2;
    private const int SampleSeconds = 30;
    private const int MaximumSeconds = 1800;
    private const string Fixture = "controlled: +600 planks, +400 stone, +10 coal; no research/tech grants";

    private static readonly SchoolResearchRules SchoolRules = new("food", ResourceAmount.FromUnits(10m),
        trainingSeconds: 120, trainingSeatsPerSchool: 1, researcherPlacesPerSchool: 2,
        milliPointsPerResearcherPerSecond: 100, pencilsPerStudent: 2);

    public static IReadOnlyList<E4aSchoolRun> Run() => [RunOne("E4A-06", 1), RunOne("E4A-07", 2)];

    private static E4aSchoolRun RunOne(string scenarioId, int schools)
    {
        var scenario = CivilGrowthScenario.Create();
        var initial = V14CampaignSession.FromV13(scenario, GameSession.Create(scenario).Snapshot(),
            SchoolRules, TechnologyCatalog.ProposedTierTree, V14SchoolConstructionRules.Candidate).Snapshot();
        var funded = initial with
        {
            Resources = initial.Resources + ResourceBundle.Create(
                ("planks", 600m), ("stone", 400m), ("coal", 10m))
        };
        var campaign = V14CampaignSession.Restore(scenario, funded, SchoolRules,
            TechnologyCatalog.ProposedTierTree, V14SchoolConstructionRules.Candidate);
        var actions = new List<string>();
        void Check(bool succeeded, string description)
        {
            if (!succeeded)
                throw new InvalidOperationException($"{scenarioId}: setup failed at '{description}'.");
            actions.Add(description);
        }

        // Identical starting stock, and no artificial population, graduates, research or unlocks.
        Check(campaign.BuildProducer("fisher", ProductionTier.Base, 1).Succeeded, "build fisher Base x1");
        Check(campaign.BuildProducer("food-store", ProductionTier.Base, 1).Succeeded,
            "build food-store Base x1");
        Check(campaign.BuildSchool(schools).Succeeded, $"build school x{schools}");
        Check(campaign.CraftPencils(1) == PencilCraftFailure.None,
            "craft pencils: 1 plank + 1 coal -> 4 pencils");
        Check(campaign.EnrollStudents(TargetGraduates) == SchoolActionFailure.None,
            "enroll 2 students: prepaid food and 2 pencils per student");

        var samples = new List<E4aSchoolSample>();
        E4aSchoolSample TakeSample()
        {
            var snap = campaign.Snapshot();
            var school = snap.SchoolResearch;
            return new E4aSchoolSample(
                checked((int)(snap.ElapsedTicks / TimeSpan.TicksPerSecond)),
                snap.Population, campaign.AvailablePopulation, school.WaitingStudents,
                school.TrainingStudents, school.QualifiedResearchers, school.ActiveResearchers,
                snap.Resources["food"].Units, snap.Resources["planks"].Units,
                snap.Resources["stone"].Units, snap.Resources["coal"].Units,
                snap.Resources["pencils"].Units, snap.ResearchBalanceMilliPoints);
        }

        samples.Add(TakeSample());
        int? firstQualified = null;
        int? allQualified = null;
        campaign.Resume();
        while (samples[^1].QualifiedResearchers < TargetGraduates &&
               samples[^1].ElapsedSeconds < MaximumSeconds)
        {
            var advance = campaign.Advance(TimeSpan.FromSeconds(SampleSeconds));
            if (!advance.Succeeded)
                throw new InvalidOperationException($"{scenarioId}: Advance failed: {advance.Error}.");
            var sample = TakeSample();
            samples.Add(sample);
            if (sample.QualifiedResearchers > 0 && firstQualified is null)
                firstQualified = sample.ElapsedSeconds;
            if (sample.QualifiedResearchers >= TargetGraduates)
                allQualified = sample.ElapsedSeconds;
        }
        campaign.Pause();

        return new E4aSchoolRun(scenarioId, "civil-growth-14", Fixture,
            schools == 1 ? "school-one" : "school-two", schools,
            TargetGraduates, firstQualified, allQualified,
            allQualified.HasValue ? "pass" : "timeout", actions, samples);
    }

    /// <summary>
    /// Exact measured samples in an invariant CSV; output contains no fabricated build hash
    /// or SQLite checkpoint identifier (neither is available in this in-memory benchmark).
    /// </summary>
    public static string ToCsv(IEnumerable<E4aSchoolRun> runs)
    {
        ArgumentNullException.ThrowIfNull(runs);
        var csv = new StringBuilder();
        csv.AppendLine("scenario_id,rules_version,fixture,strategy,school_count,elapsed_seconds,population,available_population,students_waiting,students_training,researchers_qualified,researchers_active,food_units,planks_units,stone_units,coal_units,pencils_units,research_millipoints,first_qualified_seconds,all_qualified_seconds,outcome");
        foreach (var run in runs)
        {
            foreach (var sample in run.Samples)
            {
                string[] fields =
                [
                    run.ScenarioId, run.RulesVersion, run.Fixture, run.Strategy,
                    run.SchoolCount.ToString(CultureInfo.InvariantCulture),
                    sample.ElapsedSeconds.ToString(CultureInfo.InvariantCulture),
                    sample.Population.ToString(CultureInfo.InvariantCulture),
                    sample.AvailablePopulation.ToString(CultureInfo.InvariantCulture),
                    sample.WaitingStudents.ToString(CultureInfo.InvariantCulture),
                    sample.TrainingStudents.ToString(CultureInfo.InvariantCulture),
                    sample.QualifiedResearchers.ToString(CultureInfo.InvariantCulture),
                    sample.ActiveResearchers.ToString(CultureInfo.InvariantCulture),
                    sample.FoodUnits.ToString(CultureInfo.InvariantCulture),
                    sample.PlanksUnits.ToString(CultureInfo.InvariantCulture),
                    sample.StoneUnits.ToString(CultureInfo.InvariantCulture),
                    sample.CoalUnits.ToString(CultureInfo.InvariantCulture),
                    sample.PencilsUnits.ToString(CultureInfo.InvariantCulture),
                    sample.ResearchMilliPoints.ToString(CultureInfo.InvariantCulture),
                    run.FirstQualifiedSeconds?.ToString(CultureInfo.InvariantCulture) ?? "",
                    run.AllQualifiedSeconds?.ToString(CultureInfo.InvariantCulture) ?? "",
                    sample.ElapsedSeconds == run.Samples[^1].ElapsedSeconds ? run.Outcome : "running"
                ];
                csv.AppendLine(string.Join(",", fields.Select(Escape)));
            }
        }
        return csv.ToString();
    }

    private static string Escape(string text) =>
        text.Contains('"') || text.Contains(',') || text.Contains('\n') || text.Contains('\r')
            ? '"' + text.Replace("\"", "\"\"") + '"'
            : text;
}

public sealed record E4aSchoolSample(
    int ElapsedSeconds, int Population, int AvailablePopulation,
    int WaitingStudents, int TrainingStudents, int QualifiedResearchers, int ActiveResearchers,
    decimal FoodUnits, decimal PlanksUnits, decimal StoneUnits, decimal CoalUnits,
    decimal PencilsUnits, long ResearchMilliPoints);

public sealed record E4aSchoolRun(
    string ScenarioId, string RulesVersion, string Fixture, string Strategy,
    int SchoolCount, int EnrolledStudents, int? FirstQualifiedSeconds, int? AllQualifiedSeconds,
    string Outcome, IReadOnlyList<string> SetupActions, IReadOnlyList<E4aSchoolSample> Samples);
