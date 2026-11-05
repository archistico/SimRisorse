using System.Globalization;
using System.Text;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Benchmarks;

/// <summary>
/// RC75/E4a.2b: declared, reproducible advanced v15 policies, starting from the
/// unmodified fresh campaign. There are NO resource, worker or research grants.
/// A policy failure or timeout is NOT proof of an economic deadlock.
/// </summary>
public static class V15IndustrialProgressionBenchmark
{
    public const string PolicyResearchFirst = "research-first";
    public const string PolicyBreadFirst = "bread-first";
    public const int DecisionIntervalSeconds = 60;

    private sealed record Definition(string Id, string Goal, string OutputResource, int Horizon);
    private static readonly Definition[] Definitions =
    [
        new("V15-I01", "unlock-estrazione-1", "", 21600),
        new("V15-I02", "first-swords-after-research", "swords", 43200),
        new("V15-I03", "first-coins-after-research", "coins", 57600)
    ];

    private static readonly string[] Foundation =
    [
        "build-fisher", "staff-fisher", "build-depot", "staff-depot", "build-small-house"
    ];
    // The actual desktop requires two pencils per student. Pencils consume
    // planks and COAL, whose mine requires a large house (and thus bread).
    // A research-first strategy can build the school early, but cannot train
    // researchers until the bread/coal chain has been completed.
    private static readonly string[] Education =
    [
        "craft-pencils", "enroll-student", "assign-researcher", "buy-tech-estrazione-1"
    ];
    private static readonly string[] Coal = ["build-coal-mine", "staff-coal-mine"];
    private static readonly string[] Bread =
    [
        "build-farm", "staff-farm", "build-well", "staff-well", "build-mill", "staff-mill",
        "build-bakery", "staff-bakery", "build-large-house"
    ];
    private static readonly string[] Iron =
    [
        "build-iron-mine", "staff-iron-mine", "build-iron-smelter", "staff-iron-smelter", "build-forge", "staff-forge"
    ];
    private static readonly string[] Gold =
    [
        "build-gold-mine", "staff-gold-mine", "build-gold-smelter", "staff-gold-smelter", "build-mint", "staff-mint"
    ];

    /// <summary>A strategy is an explicit order of the SAME required actions.</summary>
    public static IReadOnlyList<string> Steps(string scenarioId, string policyId)
    {
        var definition = DefinitionFor(scenarioId);
        if (policyId is not (PolicyResearchFirst or PolicyBreadFirst))
            throw new ArgumentException("Unknown advanced v15 benchmark policy.", nameof(policyId));
        string[] industry = definition.Id switch
        {
            "V15-I02" => Iron,
            "V15-I03" => Gold,
            _ => []
        };
        // All policies execute the same prerequisite actions. "research-first"
        // invests in the School earlier; coal and pencils necessarily precede
        // actual training under the approved desktop school rules.
        string[] education = definition.Id == "V15-I01"
            ? Education : [.. Education, "buy-tech-industria-1"];
        return policyId == PolicyResearchFirst
            ? [.. Foundation, "build-school", .. Bread, .. Coal, .. education, .. industry]
            : [.. Foundation, .. Bread, .. Coal, "build-school", .. education, .. industry];
    }

    public static IReadOnlyList<V15IndustrialProgressionRun> RunAll(int? maxSeconds = null)
    {
        var result = new List<V15IndustrialProgressionRun>();
        foreach (var definition in Definitions)
            foreach (var policy in new[] { PolicyResearchFirst, PolicyBreadFirst })
                result.Add(Run(definition.Id, policy, maxSeconds));
        return result;
    }

    public static V15IndustrialProgressionRun Run(string scenarioId, string policyId,
        int? maxSeconds = null)
    {
        var definition = DefinitionFor(scenarioId);
        var steps = Steps(scenarioId, policyId);
        if (maxSeconds is < 0)
            throw new ArgumentOutOfRangeException(nameof(maxSeconds));
        var horizon = Math.Min(definition.Horizon, maxSeconds ?? definition.Horizon);
        var diagnostic = horizon < definition.Horizon;
        var setup = V15BenchmarkFixture.Create();
        var checkpoint = setup.Start;
        var initial = V15StockAuditor.PhysicalTotals(checkpoint).Amounts.ToDictionary(
            p => p.Key, p => p.Value.Milliunits, StringComparer.Ordinal);
        var actions = new List<V15IndustrialProgressionEvent>();
        var samples = new List<V15IndustrialProgressionSample>();
        var next = 0;
        var elapsed = 0;
        var outcome = "timeout";
        var reason = string.Empty;
        var lastWait = string.Empty;
        int? completion = null;

        while (true)
        {
            var session = new V15FoodIndustryCampaignSession(checkpoint, setup.Baseline,
                setup.School, setup.Technologies, setup.Depot);
            while (next < steps.Count)
            {
                var action = steps[next];
                var before = session.Snapshot();
                var attempted = Execute(session, setup.School, action);
                if (!attempted.Success)
                {
                    var why = attempted.Failure;
                    if (attempted.Terminal)
                    {
                        actions.Add(new(elapsed, action, "blocked", why,
                            new Dictionary<string, long>(StringComparer.Ordinal), 0,
                            MissingResources(before, setup.School, action, why, attempted.Cost)));
                        outcome = "blocked";
                        reason = $"{action}: {why}";
                    }
                    else if (lastWait != action + ":" + why)
                    {
                        actions.Add(new(elapsed, action, "waiting", why,
                            new Dictionary<string, long>(StringComparer.Ordinal), 0,
                            MissingResources(before, setup.School, action, why, attempted.Cost)));
                        lastWait = action + ":" + why;
                    }
                    break;
                }
                // Charge actual successful construction/enrolment receipts, not
                // estimated catalogue costs. Research points are a separate unit.
                var after = session.Snapshot();
                var researchSpent = action.StartsWith("buy-tech-", StringComparison.Ordinal)
                    ? checked(before.CityCampaign.ResearchBalanceMilliPoints -
                        after.CityCampaign.ResearchBalanceMilliPoints) : 0;
                actions.Add(new(elapsed, action, "executed", string.Empty,
                    attempted.Cost.Amounts.Where(p => p.Value.Milliunits != 0)
                        .ToDictionary(p => p.Key, p => p.Value.Milliunits, StringComparer.Ordinal),
                    researchSpent));
                next++;
                lastWait = string.Empty;
            }
            checkpoint = session.Snapshot();
            if (elapsed == 0 || elapsed % DecisionIntervalSeconds == 0 || outcome == "blocked")
                samples.Add(Sample(checkpoint, setup.Baseline, elapsed,
                    next < steps.Count ? steps[next] : "await-production"));
            if (outcome == "blocked") break;
            if (next == steps.Count && definition.OutputResource.Length == 0 &&
                checkpoint.CityCampaign.UnlockedTechnologyIds.Contains("estrazione-1"))
            {
                outcome = "pass";
                completion = elapsed;
                reason = "estrazione-1 unlocked through the real research purchase";
                actions.Add(new(elapsed, definition.Goal, "milestone", reason,
                    new Dictionary<string, long>(StringComparer.Ordinal), 0));
                if (samples[^1].ElapsedSeconds == elapsed)
                    samples[^1] = Sample(checkpoint, setup.Baseline, elapsed, "completed");
                else
                    samples.Add(Sample(checkpoint, setup.Baseline, elapsed, "completed"));
                break;
            }
            if (elapsed >= horizon) break;

            var batch = Math.Min(next == steps.Count ? 1 : DecisionIntervalSeconds,
                horizon - elapsed);
            var plan = V15UnifiedCampaignClock.Plan(checkpoint, setup.Baseline,
                setup.School, setup.Technologies, FoodIndustryConstructionCatalog.Candidate,
                setup.Meat, setup.Crops, setup.Depot, TimeSpan.FromSeconds(batch));
            if (plan.SecondsAdvanced != batch)
                throw new InvalidOperationException("The advanced benchmark clock did not advance as requested.");
            if (batch == 1 && !V15StockAuditor.ReconcileSecond(checkpoint, plan).Balanced)
                throw new InvalidOperationException("Advanced benchmark resource reconciliation failed.");
            checkpoint = plan.Next;
            elapsed += batch;
            if (next == steps.Count && definition.OutputResource.Length != 0 &&
                checkpoint.CityCampaign.UnlockedTechnologyIds.Contains("industria-1") &&
                plan.FirstSecondProduction[definition.OutputResource].Milliunits > 0)
            {
                outcome = "pass";
                completion = elapsed;
                reason = $"Real {definition.OutputResource} output observed with industria-1 unlocked";
                actions.Add(new(elapsed, definition.Goal, "milestone", reason,
                    new Dictionary<string, long>(StringComparer.Ordinal), 0));
                samples.Add(Sample(checkpoint, setup.Baseline, elapsed, "completed"));
                break;
            }
        }
        if (outcome == "timeout")
            reason = $"Not observed by {horizon}s; pending: " +
                (next < steps.Count ? steps[next] : "actual output") +
                (lastWait.Length == 0 ? "" : $" ({lastWait})");
        if (samples.Count == 0 || samples[^1].ElapsedSeconds != elapsed)
            samples.Add(Sample(checkpoint, setup.Baseline, elapsed,
                next < steps.Count ? steps[next] : "await-production"));

        return new(definition.Id, policyId, V15FoodCampaignCheckpoints.RulesVersion,
            setup.Baseline.Catalog.RulesVersion, "fresh-game/no-grants", definition.Goal,
            definition.Horizon, horizon, diagnostic, outcome, completion, reason,
            checkpoint.CityCampaign.ScenarioId, initial, actions, samples);
    }

    private static Definition DefinitionFor(string scenarioId) =>
        Definitions.SingleOrDefault(d => d.Id == scenarioId) ??
        throw new ArgumentException("Unknown advanced v15 benchmark scenario.", nameof(scenarioId));

    private static V15IndustrialProgressionSample Sample(V15FoodCampaignCheckpoint checkpoint,
        SimRisorse.Core.Content.GameScenario baseline, int seconds, string nextAction)
    {
        var city = checkpoint.CityCampaign;
        var physical = V15StockAuditor.PhysicalTotals(checkpoint);
        var free = V15FoodCampaignCheckpoints.AvailableWorkers(checkpoint);
        return new(seconds, nextAction, city.Population, free,
            city.SchoolCount, city.SchoolResearch.QualifiedResearchers,
            city.SchoolResearch.ActiveResearchers, city.ResearchBalanceMilliPoints,
            city.UnlockedTechnologyIds.Contains("estrazione-1"),
            physical["swords"].Milliunits, physical["coins"].Milliunits,
            V15IndustrialStockProbe.Capture(city.Resources,
                V15CityStorage.Calculate(baseline, city)));
    }

    /// <summary>Measured shortage at the moment a scripted action is rejected.
    /// Empty means that the failure did not provide a measurable quote; it never
    /// proves that no input is missing. Food is owned by the depot, not the city.
    /// </summary>
    private static IReadOnlyDictionary<string, long> MissingResources(
        V15FoodCampaignCheckpoint before, SchoolResearchRules school,
        string action, string failure, ResourceBundle quoted)
    {
        if (failure is not ("InsufficientResources" or "InsufficientMaterials" or
            "InsufficientFood" or "InsufficientPencils"))
            return new Dictionary<string, long>(StringComparer.Ordinal);
        var expected = action == "enroll-student" ? school.TrainingCost(1) : quoted;
        var missing = new SortedDictionary<string, long>(StringComparer.Ordinal);
        foreach (var entry in expected.Amounts)
        {
            // Training failures are specific: avoid presenting other resources as
            // the cause of the particular rejected operation.
            if (failure == "InsufficientFood" && entry.Key != school.FoodResourceId ||
                failure == "InsufficientPencils" && entry.Key != V14SchoolSupplies.ResourceId)
                continue;
            var current = entry.Key == school.FoodResourceId
                ? before.Depot.ReadyFood.Milliunits
                : before.CityCampaign.Resources[entry.Key].Milliunits;
            var gap = entry.Value.Milliunits - current;
            if (gap > 0) missing.Add(entry.Key, gap);
        }
        return missing;
    }

    private readonly record struct Attempt(bool Success, bool Terminal, string Failure, ResourceBundle Cost);

    private static Attempt Execute(V15FoodIndustryCampaignSession session,
        SchoolResearchRules school, string action)
    {
        static Attempt Tier(V14TierActionResult result) =>
            new(result.Succeeded, result.Failure is not
                (V14TierActionFailure.None or V14TierActionFailure.InsufficientResources or
                 V14TierActionFailure.InsufficientPopulation),
                result.Failure.ToString(), result.Cost);
        static Attempt Staff(V14TierActionFailure result) =>
            new(result == V14TierActionFailure.None, result is not
                (V14TierActionFailure.None or V14TierActionFailure.InsufficientPopulation),
                result.ToString(), ResourceBundle.Empty);
        static Attempt City(V14CivicConstructionResult result) =>
            new(result.Succeeded, result.Failure is not
                (V14CivicConstructionFailure.None or V14CivicConstructionFailure.InsufficientResources),
                result.Failure.ToString(), result.Cost);
        static Attempt Food(FoodIndustryBuildResult result) =>
            new(result.Succeeded, result.Failure is not
                (FoodIndustryActionFailure.None or FoodIndustryActionFailure.InsufficientResources or
                 FoodIndustryActionFailure.InsufficientPopulation),
                result.Failure.ToString(), result.Cost);
        static Attempt FoodStaff(FoodIndustryActionFailure result) =>
            new(result == FoodIndustryActionFailure.None, result is not
                (FoodIndustryActionFailure.None or FoodIndustryActionFailure.InsufficientPopulation),
                result.ToString(), ResourceBundle.Empty);
        static Attempt School(V14SchoolConstructionResult result) =>
            new(result.Succeeded, result.Failure is not
                (V14SchoolConstructionFailure.None or V14SchoolConstructionFailure.InsufficientResources),
                result.Failure.ToString(), result.Cost);
        static Attempt Enroll(SchoolActionFailure result, SchoolResearchRules rules) =>
            new(result == SchoolActionFailure.None, result is not
                (SchoolActionFailure.None or SchoolActionFailure.InsufficientPopulation or
                 SchoolActionFailure.InsufficientFood or SchoolActionFailure.InsufficientPencils or
                 SchoolActionFailure.InsufficientQualifiedResearchers),
                result.ToString(), result == SchoolActionFailure.None
                    ? rules.TrainingCost(1) : ResourceBundle.Empty);
        static Attempt Pencils(PencilCraftFailure result) =>
            new(result == PencilCraftFailure.None, result is not
                (PencilCraftFailure.None or PencilCraftFailure.InsufficientMaterials),
                result.ToString(), result == PencilCraftFailure.None
                    ? V14SchoolSupplies.Inputs(1) : ResourceBundle.Empty);
        static Attempt Assign(SchoolActionFailure result) =>
            new(result == SchoolActionFailure.None, result is not
                (SchoolActionFailure.None or SchoolActionFailure.InsufficientQualifiedResearchers),
                result.ToString(), ResourceBundle.Empty);
        static Attempt Buy(TechnologyPurchaseFailure result) =>
            new(result == TechnologyPurchaseFailure.None, result is not
                (TechnologyPurchaseFailure.None or TechnologyPurchaseFailure.InsufficientResearchPoints),
                result.ToString(), ResourceBundle.Empty);

        return action switch
        {
            "build-fisher" => Tier(session.BuildCityProducer("fisher", ProductionTier.Base, 1)),
            "staff-fisher" => Staff(session.AssignCityProducerWorkers("fisher", ProductionTier.Base, 1)),
            "build-depot" => Food(session.Build("food-depot", 1)),
            "staff-depot" => FoodStaff(session.AssignWorkers("food-depot", 1)),
            "build-small-house" => City(session.BuildCityBuilding("small-house", 1)),
            "build-large-house" => City(session.BuildCityBuilding("large-house", 1)),
            "build-school" => School(session.BuildSchool(1)),
            "craft-pencils" => Pencils(session.CraftPencils(1)),
            "enroll-student" => Enroll(session.EnrollStudents(1), school),
            "assign-researcher" => Assign(session.AssignResearchers(1)),
            "buy-tech-estrazione-1" => Buy(session.BuyTechnology("estrazione-1")),
            "buy-tech-industria-1" => Buy(session.BuyTechnology("industria-1")),
            "build-farm" => Tier(session.BuildCityProducer("farm", ProductionTier.Base, 1)),
            "staff-farm" => Staff(session.AssignCityProducerWorkers("farm", ProductionTier.Base, 1)),
            "build-well" => Tier(session.BuildCityProducer("well", ProductionTier.Base, 1)),
            "staff-well" => Staff(session.AssignCityProducerWorkers("well", ProductionTier.Base, 1)),
            "build-mill" => Tier(session.BuildCityProducer("mill", ProductionTier.Base, 1)),
            "staff-mill" => Staff(session.AssignCityProducerWorkers("mill", ProductionTier.Base, 1)),
            "build-bakery" => Tier(session.BuildCityProducer("bakery", ProductionTier.Base, 1)),
            "staff-bakery" => Staff(session.AssignCityProducerWorkers("bakery", ProductionTier.Base, 1)),
            "build-iron-mine" => Tier(session.BuildCityProducer("iron-mine", ProductionTier.Base, 1)),
            "staff-iron-mine" => Staff(session.AssignCityProducerWorkers("iron-mine", ProductionTier.Base, 1)),
            "build-coal-mine" => Tier(session.BuildCityProducer("coal-mine", ProductionTier.Base, 1)),
            "staff-coal-mine" => Staff(session.AssignCityProducerWorkers("coal-mine", ProductionTier.Base, 1)),
            "build-iron-smelter" => Tier(session.BuildCityProducer("iron-smelter", ProductionTier.Base, 1)),
            "staff-iron-smelter" => Staff(session.AssignCityProducerWorkers("iron-smelter", ProductionTier.Base, 1)),
            "build-forge" => Tier(session.BuildCityProducer("forge", ProductionTier.Base, 1)),
            "staff-forge" => Staff(session.AssignCityProducerWorkers("forge", ProductionTier.Base, 1)),
            "build-gold-mine" => Tier(session.BuildCityProducer("gold-mine", ProductionTier.Base, 1)),
            "staff-gold-mine" => Staff(session.AssignCityProducerWorkers("gold-mine", ProductionTier.Base, 1)),
            "build-gold-smelter" => Tier(session.BuildCityProducer("gold-smelter", ProductionTier.Base, 1)),
            "staff-gold-smelter" => Staff(session.AssignCityProducerWorkers("gold-smelter", ProductionTier.Base, 1)),
            "build-mint" => Tier(session.BuildCityProducer("mint", ProductionTier.Base, 1)),
            "staff-mint" => Staff(session.AssignCityProducerWorkers("mint", ProductionTier.Base, 1)),
            _ => throw new ArgumentException($"Unknown benchmark action: {action}.", nameof(action))
        };
    }

    public static string EventsCsv(IEnumerable<V15IndustrialProgressionRun> runs)
    {
        var csv = new StringBuilder("scenario_id,policy_id,elapsed_seconds,action,result,reason,resource_id,spent_milli,research_spent_millipoints\n");
        foreach (var run in runs)
            foreach (var e in run.Events)
            {
                if (e.ResourceCostsMilliunits.Count == 0)
                    csv.AppendLine(string.Join(",", Escape(run.ScenarioId), Escape(run.PolicyId),
                        N(e.ElapsedSeconds), Escape(e.Action), Escape(e.Result), Escape(e.Reason),
                        "", "0", N(e.ResearchSpentMilliPoints)));
                else foreach (var entry in e.ResourceCostsMilliunits.OrderBy(p => p.Key, StringComparer.Ordinal))
                    csv.AppendLine(string.Join(",", Escape(run.ScenarioId), Escape(run.PolicyId),
                        N(e.ElapsedSeconds), Escape(e.Action), Escape(e.Result), Escape(e.Reason),
                        Escape(entry.Key), N(entry.Value), N(e.ResearchSpentMilliPoints)));
            }
        return csv.ToString();
    }

    public static string SamplesCsv(IEnumerable<V15IndustrialProgressionRun> runs)
    {
        var csv = new StringBuilder("scenario_id,policy_id,elapsed_seconds,next_action,population,free_workers,schools,qualified_researchers,active_researchers,research_balance_millipoints,estrazione1_unlocked,swords_milli,coins_milli\n");
        foreach (var run in runs)
            foreach (var s in run.Samples)
                csv.AppendLine(string.Join(",", Escape(run.ScenarioId), Escape(run.PolicyId),
                    N(s.ElapsedSeconds), Escape(s.NextAction), N(s.Population), N(s.FreeWorkers),
                    N(s.Schools), N(s.QualifiedResearchers), N(s.ActiveResearchers),
                    N(s.ResearchBalanceMilliPoints), s.ExtractionOneUnlocked ? "true" : "false",
                    N(s.SwordsMilliunits), N(s.CoinsMilliunits)));
        return csv.ToString();
    }

    private static string N(long value) => value.ToString(CultureInfo.InvariantCulture);
    private static string Escape(string value) => value.IndexOfAny(['"', ',', '\r', '\n']) < 0
        ? value : '"' + value.Replace("\"", "\"\"") + '"';
}

public sealed record V15IndustrialProgressionEvent(int ElapsedSeconds, string Action,
    string Result, string Reason, IReadOnlyDictionary<string, long> ResourceCostsMilliunits,
    long ResearchSpentMilliPoints,
    IReadOnlyDictionary<string, long>? MissingResourceMilliunits = null);

public sealed record V15IndustrialProgressionSample(int ElapsedSeconds, string NextAction,
    int Population, long FreeWorkers, int Schools, int QualifiedResearchers,
    int ActiveResearchers, long ResearchBalanceMilliPoints, bool ExtractionOneUnlocked,
    long SwordsMilliunits, long CoinsMilliunits,
    V15IndustrialStockProbe? IndustrialStocks = null);

public sealed record V15IndustrialProgressionRun(string ScenarioId, string PolicyId,
    string RulesVersion, string BaselineRulesVersion, string Fixture, string Goal,
    int DeclaredHorizonSeconds, int EffectiveHorizonSeconds, bool Diagnostic,
    string Outcome, int? CompletionSeconds, string StopReason, string ScenarioBaselineId,
    IReadOnlyDictionary<string, long> InitialPhysicalStockMilliunits,
    IReadOnlyList<V15IndustrialProgressionEvent> Events,
    IReadOnlyList<V15IndustrialProgressionSample> Samples);
