using System.Globalization;
using System.Text;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Core.Economy;

namespace SimRisorse.Application.Benchmarks;

/// <summary>
/// E4a-D02: unmodified civil-growth-14 starts. This is an explicit, deterministic
/// reference policy, NOT an optimum player strategy. There are no funded snapshots,
/// resource grants, skipped requirements or artificial technologies/population.
/// Runs that do not reach their objective report timeout/blocked, never a guessed time.
/// </summary>
public static class E4aNaturalBenchmark
{
    public const string PolicyId = "natural-reference-v1";
    public const int SampleSeconds = 60;

    private static readonly (string Id, string Strategy, string Goal, int Horizon)[] Scenarios =
    [
        ("E4A-01", "manual-reference", "first-food-production", 14400),
        ("E4A-02", "growth-first", "bread-and-advanced-resident", 28800),
        ("E4A-03", "research-first", "first-student-enrolled", 28800),
        ("E4A-04", "research-first", "first-technology-purchased", 43200),
        ("E4A-05", "research-first", "first-large-sawmill", 57600)
    ];

    private static readonly string[] Steps =
    [
        "build fisher Base", "build food-store Base", "assign fisher worker", "assign food-store worker",
        "build second small-house", "build farm Base", "build well Base", "build mill Base",
        "build bakery Base", "assign farm worker", "assign well worker", "assign mill worker",
        "assign bakery worker", "build large-house", "build coal-mine Base", "assign coal-mine worker",
        "build School", "craft four pencils", "enroll one student", "wait for qualification",
        "assign one researcher", "wait for 100 research points", "buy Lavorazioni I",
        "upgrade sawmill Base to Grande"
    ];

    private static readonly SchoolResearchRules SchoolRules = new("food", ResourceAmount.FromUnits(10m),
        trainingSeconds: 120, trainingSeatsPerSchool: 1, researcherPlacesPerSchool: 2,
        milliPointsPerResearcherPerSecond: 100, pencilsPerStudent: 2);

    public static IReadOnlyList<E4aNaturalRun> RunAll() => Scenarios.Select(item => Run(item.Id)).ToArray();

    /// <summary>
    /// maxSeconds is an optional smaller diagnostic horizon, not a clock acceleration.
    /// Production still advances by genuine v14 ticks, in batches of up to 60 seconds.
    /// </summary>
    public static E4aNaturalRun Run(string scenarioId, int? maxSeconds = null)
    {
        var descriptor = Scenarios.SingleOrDefault(item => item.Id == scenarioId);
        if (descriptor.Id is null)
            throw new ArgumentException("Unknown natural E4a scenario.", nameof(scenarioId));
        if (maxSeconds is < 0)
            throw new ArgumentOutOfRangeException(nameof(maxSeconds));
        var horizon = Math.Min(descriptor.Horizon, maxSeconds ?? descriptor.Horizon);
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot();
        var campaign = V14CampaignSession.FromV13(scenario, initial, SchoolRules,
            TechnologyCatalog.ProposedTierTree, V14SchoolConstructionRules.Candidate);
        var starting = campaign.Snapshot();
        var initialStock = new E4aNaturalInitialStock(starting.Population,
            starting.Resources["planks"].Units, starting.Resources["stone"].Units,
            starting.Resources["food"].Units, starting.Resources["coal"].Units,
            starting.Resources["pencils"].Units);

        var events = new List<E4aNaturalEvent>();
        var samples = new List<E4aNaturalSample>();
        var step = 0;
        var previousFailure = "";
        var lastReason = "";
        var outcome = "timeout";
        var seconds = 0;
        campaign.Resume();

        while (true)
        {
            // At every clock point perform as many legal actions as the unchanged
            // current stock and population permit. Failed actions are atomic.
            while (step < Steps.Length)
            {
                var result = TryStep(campaign, step);
                if (result.Success)
                {
                    events.Add(new E4aNaturalEvent(seconds, Steps[step], "executed", ""));
                    step++;
                    previousFailure = "";
                    lastReason = "";
                    continue;
                }
                lastReason = result.Failure;
                if (previousFailure != result.Failure)
                {
                    events.Add(new E4aNaturalEvent(seconds, Steps[step],
                        result.Terminal ? "blocked" : "waiting", result.Failure));
                    previousFailure = result.Failure;
                }
                if (result.Terminal) outcome = "blocked";
                break;
            }

            var sample = TakeSample(campaign, seconds, step);
            samples.Add(sample);
            if (GoalAchieved(descriptor.Id, sample, campaign))
            {
                outcome = "pass";
                lastReason = "objective reached";
                break;
            }
            if (outcome == "blocked" || seconds >= horizon)
                break;

            var batch = Math.Min(SampleSeconds, horizon - seconds);
            var advance = campaign.Advance(TimeSpan.FromSeconds(batch));
            if (!advance.Succeeded)
            {
                outcome = "blocked";
                lastReason = $"Advance failed: {advance.Error}";
                events.Add(new E4aNaturalEvent(seconds, "advance clock", "blocked", lastReason));
                break;
            }
            seconds += batch;
        }
        campaign.Pause();
        return new E4aNaturalRun(descriptor.Id, V14CampaignSnapshots.RulesVersion,
            "natural: no stock/population/research grants", descriptor.Strategy, PolicyId,
            descriptor.Goal, horizon, outcome == "pass" ? (int?)seconds : null, outcome,
            outcome == "timeout" ? $"time limit {horizon}s; step: {Steps[Math.Min(step, Steps.Length - 1)]}; {lastReason}" : lastReason,
            initialStock, events, samples);
    }

    private static bool GoalAchieved(string scenarioId, E4aNaturalSample sample, V14CampaignSession campaign)
    {
        var snap = campaign.Snapshot();
        return scenarioId switch
        {
            "E4A-01" => sample.ElapsedSeconds > 0 && sample.FoodUnits > 30m &&
                         snap.ProductiveFamilies.Single(item => item.FamilyId == "food-store").Base.AssignedWorkers > 0,
            "E4A-02" => sample.BreadUnits > 0m && sample.LargeHouses > 0 &&
                         sample.Population > sample.SmallHouses * 8,
            "E4A-03" => sample.SchoolCount > 0 &&
                         sample.StudentsWaiting + sample.StudentsTraining + sample.ResearchersQualified > 0,
            "E4A-04" => snap.UnlockedTechnologyIds.Contains("lavorazioni-1"),
            "E4A-05" => snap.ProductiveFamilies.Single(item => item.FamilyId == "sawmill").Large.Buildings > 0,
            _ => false
        };
    }

    private static E4aNaturalSample TakeSample(V14CampaignSession session, int seconds, int step)
    {
        var snap = session.Snapshot();
        var students = snap.SchoolResearch;
        return new E4aNaturalSample(seconds, step == Steps.Length ? "completed" : Steps[step],
            snap.Population, session.AvailablePopulation,
            snap.OtherBuildings.Single(item => item.BuildingId == "small-house").Count,
            snap.OtherBuildings.Single(item => item.BuildingId == "large-house").Count,
            snap.SchoolCount, students.WaitingStudents, students.TrainingStudents,
            students.QualifiedResearchers, students.ActiveResearchers,
            snap.Resources["planks"].Units, snap.Resources["stone"].Units,
            snap.Resources["food"].Units, snap.Resources["bread"].Units,
            snap.Resources["coal"].Units, snap.Resources["pencils"].Units,
            snap.ResearchBalanceMilliPoints);
    }

    private readonly record struct StepResult(bool Success, string Failure = "", bool Terminal = false);

    private static StepResult TryStep(V14CampaignSession session, int step)
    {
        static StepResult Tier(V14TierActionResult result) =>
            new(result.Succeeded, result.Failure.ToString(),
                result.Failure is not (V14TierActionFailure.None or
                    V14TierActionFailure.InsufficientResources or V14TierActionFailure.InsufficientPopulation));
        static StepResult Staff(V14TierActionFailure result) =>
            new(result == V14TierActionFailure.None, result.ToString(),
                result is not (V14TierActionFailure.None or V14TierActionFailure.InsufficientPopulation));
        static StepResult Civic(V14CivicConstructionResult result) =>
            new(result.Succeeded, result.Failure.ToString(),
                result.Failure is not (V14CivicConstructionFailure.None or V14CivicConstructionFailure.InsufficientResources));
        static StepResult School(V14SchoolConstructionResult result) =>
            new(result.Succeeded, result.Failure.ToString(),
                result.Failure is not (V14SchoolConstructionFailure.None or V14SchoolConstructionFailure.InsufficientResources));
        static StepResult Pencil(PencilCraftFailure result) =>
            new(result == PencilCraftFailure.None, result.ToString(),
                result is not (PencilCraftFailure.None or PencilCraftFailure.InsufficientMaterials or
                    PencilCraftFailure.InsufficientStorage));
        static StepResult Students(SchoolActionFailure result) =>
            new(result == SchoolActionFailure.None, result.ToString(),
                result is not (SchoolActionFailure.None or SchoolActionFailure.InsufficientPopulation or
                    SchoolActionFailure.InsufficientFood or SchoolActionFailure.InsufficientPencils or
                    SchoolActionFailure.InsufficientQualifiedResearchers or SchoolActionFailure.InsufficientResearchPlaces));

        return step switch
        {
            0 => Tier(session.BuildProducer("fisher", ProductionTier.Base, 1)),
            1 => Tier(session.BuildProducer("food-store", ProductionTier.Base, 1)),
            2 => Staff(session.AssignProducerWorkers("fisher", ProductionTier.Base, 1)),
            3 => Staff(session.AssignProducerWorkers("food-store", ProductionTier.Base, 1)),
            4 => Civic(session.BuildCivicBuilding("small-house", 1)),
            5 => Tier(session.BuildProducer("farm", ProductionTier.Base, 1)),
            6 => Tier(session.BuildProducer("well", ProductionTier.Base, 1)),
            7 => Tier(session.BuildProducer("mill", ProductionTier.Base, 1)),
            8 => Tier(session.BuildProducer("bakery", ProductionTier.Base, 1)),
            9 => Staff(session.AssignProducerWorkers("farm", ProductionTier.Base, 1)),
            10 => Staff(session.AssignProducerWorkers("well", ProductionTier.Base, 1)),
            11 => Staff(session.AssignProducerWorkers("mill", ProductionTier.Base, 1)),
            12 => Staff(session.AssignProducerWorkers("bakery", ProductionTier.Base, 1)),
            13 => Civic(session.BuildCivicBuilding("large-house", 1)),
            14 => Tier(session.BuildProducer("coal-mine", ProductionTier.Base, 1)),
            15 => Staff(session.AssignProducerWorkers("coal-mine", ProductionTier.Base, 1)),
            16 => School(session.BuildSchool(1)),
            17 => Pencil(session.CraftPencils(1)),
            18 => Students(session.EnrollStudents(1)),
            19 => new StepResult(session.Snapshot().SchoolResearch.QualifiedResearchers > 0, "student still in training"),
            20 => Students(session.AssignResearchers(1)),
            21 => new StepResult(session.Snapshot().ResearchBalanceMilliPoints >= 100000,
                "waiting for 100 research points"),
            22 => Technology(session.BuyTechnology("lavorazioni-1")),
            23 => Tier(session.UpgradeProducer("sawmill", ProductionTier.Base, ProductionTier.Large)),
            _ => new StepResult(false, "Unknown benchmark step", true)
        };

        static StepResult Technology(TechnologyPurchaseFailure result) =>
            new(result == TechnologyPurchaseFailure.None, result.ToString(),
                result != TechnologyPurchaseFailure.None);
    }

    public static string ToCsv(IEnumerable<E4aNaturalRun> runs)
    {
        ArgumentNullException.ThrowIfNull(runs);
        var csv = new StringBuilder();
        csv.AppendLine("scenario_id,rules_version,fixture,policy_id,strategy,objective,elapsed_seconds,stage,population,available_population,small_houses,large_houses,schools,students_waiting,students_training,researchers_qualified,researchers_active,planks_units,stone_units,food_units,bread_units,coal_units,pencils_units,research_millipoints,outcome,completion_seconds");
        foreach (var run in runs)
            foreach (var sample in run.Samples)
            {
                var fields = new[]
                {
                    run.ScenarioId, run.RulesVersion, run.Fixture, run.PolicyId, run.Strategy,
                    run.Objective, sample.ElapsedSeconds.ToString(CultureInfo.InvariantCulture), sample.Stage,
                    sample.Population.ToString(CultureInfo.InvariantCulture),
                    sample.AvailablePopulation.ToString(CultureInfo.InvariantCulture),
                    sample.SmallHouses.ToString(CultureInfo.InvariantCulture),
                    sample.LargeHouses.ToString(CultureInfo.InvariantCulture),
                    sample.SchoolCount.ToString(CultureInfo.InvariantCulture),
                    sample.StudentsWaiting.ToString(CultureInfo.InvariantCulture),
                    sample.StudentsTraining.ToString(CultureInfo.InvariantCulture),
                    sample.ResearchersQualified.ToString(CultureInfo.InvariantCulture),
                    sample.ResearchersActive.ToString(CultureInfo.InvariantCulture),
                    sample.PlanksUnits.ToString(CultureInfo.InvariantCulture),
                    sample.StoneUnits.ToString(CultureInfo.InvariantCulture),
                    sample.FoodUnits.ToString(CultureInfo.InvariantCulture),
                    sample.BreadUnits.ToString(CultureInfo.InvariantCulture),
                    sample.CoalUnits.ToString(CultureInfo.InvariantCulture),
                    sample.PencilsUnits.ToString(CultureInfo.InvariantCulture),
                    sample.ResearchMilliPoints.ToString(CultureInfo.InvariantCulture),
                    sample.ElapsedSeconds == run.Samples[^1].ElapsedSeconds ? run.Outcome : "running",
                    run.CompletionSeconds?.ToString(CultureInfo.InvariantCulture) ?? ""
                };
                csv.AppendLine(string.Join(",", fields.Select(Escape)));
            }
        return csv.ToString();
    }

    private static string Escape(string text) =>
        text.IndexOfAny(['"', ',', '\n', '\r']) >= 0
            ? '"' + text.Replace("\"", "\"\"") + '"'
            : text;
}

public sealed record E4aNaturalInitialStock(int Population, decimal PlanksUnits, decimal StoneUnits,
    decimal FoodUnits, decimal CoalUnits, decimal PencilsUnits);
public sealed record E4aNaturalEvent(int ElapsedSeconds, string Stage, string Outcome, string Failure);
public sealed record E4aNaturalSample(int ElapsedSeconds, string Stage, int Population, int AvailablePopulation,
    int SmallHouses, int LargeHouses, int SchoolCount, int StudentsWaiting, int StudentsTraining,
    int ResearchersQualified, int ResearchersActive, decimal PlanksUnits, decimal StoneUnits,
    decimal FoodUnits, decimal BreadUnits, decimal CoalUnits, decimal PencilsUnits, long ResearchMilliPoints);
public sealed record E4aNaturalRun(string ScenarioId, string RulesVersion, string Fixture, string Strategy,
    string PolicyId, string Objective, int HorizonSeconds, int? CompletionSeconds, string Outcome,
    string StopReason, E4aNaturalInitialStock InitialStock,
    IReadOnlyList<E4aNaturalEvent> Events, IReadOnlyList<E4aNaturalSample> Samples);
