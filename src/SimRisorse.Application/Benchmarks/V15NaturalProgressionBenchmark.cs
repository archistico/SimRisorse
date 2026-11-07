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
/// RC73 / E4a.1: a reproducible, UNFUNDED v15 progression reference.
/// Starts from the same fresh civil-growth scenario and in-memory v15 food
/// preparation used by the desktop. Every purchase/staffing action goes through
/// the real v15 session; all waiting is advanced through the actual v15 clock.
/// This is a declared scripted policy, not an optimal human player or a claim
/// that the milestones are reachable. No stock, workers or research are granted.
/// </summary>
public static class V15NaturalProgressionBenchmark
{
    public const string PolicyId = "v15-scripted-natural-reference-1";
    public const int DecisionIntervalSeconds = 60;

    private sealed record Definition(string Id, string Goal, int Horizon, string[] Steps, string ProducedId);

    private static readonly Definition[] Definitions =
    [
        new("V15-N01", "first-new-depot-prepared-food", 14400,
            ["build-fisher", "staff-fisher", "build-depot", "staff-depot"], "food"),
        new("V15-N02", "first-bread-produced", 28800,
            ["build-fisher", "staff-fisher", "build-depot", "staff-depot",
             "build-small-house", "build-farm", "staff-farm", "build-well", "staff-well",
             "build-mill", "staff-mill", "build-bakery", "staff-bakery"], "bread"),
        new("V15-N03", "first-hunter-game-produced", 14400,
            ["build-hunter", "staff-hunter"], "game")
    ];

    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);

    private static (SchoolResearchRules School, TechnologyCatalog Technologies,
        FoodConversionPolicy Depot, MeatChainPolicy Meat, HorticulturePolicy Crops) Rules() =>
        (new SchoolResearchRules("food", U(10m), 120, 1, 2, 100, pencilsPerStudent: 2),
         TechnologyCatalog.ProposedTierTree,
         UnifiedFoodInputs.CreatePolicy([
             new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
             new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
             new("bread", U(1m), U(1m), 4)]),
         new MeatChainPolicy("grain", "water", "cattle", "game", "meat",
             ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(1m), U(1m),
             [new("cattle", U(1m), U(2m), 0), new("game", U(1m), U(1m), 1)]),
         new HorticulturePolicy("water", "vegetables", "fruit",
             new("vegetables", U(2m), U(1m), 0), new("fruit", U(3m), U(1m), 1)));

    public static IReadOnlyList<V15NaturalProgressionRun> RunAll(int? maxSeconds = null) =>
        Definitions.Select(definition => Run(definition.Id, maxSeconds)).ToArray();

    /// <summary>
    /// maxSeconds may shorten a diagnostic run; it can never extend the declared
    /// scenario horizon. A shortened run is labelled diagnostic, not measured.
    /// The reported completion time is null unless the milestone was observed.
    /// </summary>
    public static V15NaturalProgressionRun Run(string scenarioId, int? maxSeconds = null)
    {
        var definition = Definitions.SingleOrDefault(item => item.Id == scenarioId);
        if (definition is null)
            throw new ArgumentException("Unknown natural v15 benchmark scenario.", nameof(scenarioId));
        if (maxSeconds is < 0)
            throw new ArgumentOutOfRangeException(nameof(maxSeconds));
        var effectiveHorizon = Math.Min(definition.Horizon, maxSeconds ?? definition.Horizon);
        var diagnostic = effectiveHorizon < definition.Horizon;

        var baseline = CivilGrowthScenario.Create();
        var rules = Rules();
        var initial = V15FoodCampaignCheckpoints.NewCampaign(
            baseline, rules.School, rules.Technologies, rules.Depot);
        var physicalAtStart = V15StockAuditor.PhysicalTotals(initial);
        var initialAmounts = physicalAtStart.Amounts.ToDictionary(item => item.Key,
            item => item.Value.Milliunits, StringComparer.Ordinal);
        var events = new List<V15NaturalProgressionEvent>();
        var samples = new List<V15NaturalProgressionSample>();
        var current = initial;
        var seconds = 0;
        var nextStep = 0;
        var lastFailure = string.Empty;
        var outcome = "timeout";
        var stopReason = string.Empty;
        int? completion = null;

        while (true)
        {
            // Commands run against the current authoritative checkpoint. No
            // resources, population or technology are added by the benchmark.
            var session = new V15FoodIndustryCampaignSession(current, baseline, rules.School,
                rules.Technologies, rules.Depot);
            while (nextStep < definition.Steps.Length)
            {
                var code = definition.Steps[nextStep];
                var attempt = TryAction(session, code);
                if (!attempt.Success)
                {
                    if (attempt.Failure != lastFailure)
                    {
                        events.Add(new(seconds, code, attempt.Terminal ? "blocked" : "waiting",
                            attempt.Failure, 0, 0));
                        lastFailure = attempt.Failure;
                    }
                    if (attempt.Terminal)
                    {
                        outcome = "blocked";
                        stopReason = $"{code}: {attempt.Failure}";
                    }
                    break;
                }
                events.Add(new(seconds, code, "executed", string.Empty,
                    attempt.Cost["planks"].Milliunits, attempt.Cost["stone"].Milliunits));
                nextStep++;
                lastFailure = string.Empty;
            }
            current = session.Snapshot();
            if (seconds == 0 || seconds % DecisionIntervalSeconds == 0 || outcome == "blocked")
                samples.Add(Sample(current, seconds,
                    nextStep < definition.Steps.Length ? definition.Steps[nextStep] : "await-production"));

            if (outcome == "blocked" || seconds >= effectiveHorizon)
                break;

            // The scripted policy attempts actions every 60 seconds while
            // waiting. Once all sites are staffed, inspect EACH physical second
            // so the first actual production event cannot be missed.
            var batch = Math.Min(nextStep == definition.Steps.Length ? 1 : DecisionIntervalSeconds,
                effectiveHorizon - seconds);
            var advance = V15UnifiedCampaignClock.Plan(current, baseline, rules.School,
                rules.Technologies, FoodIndustryConstructionCatalog.Candidate,
                rules.Meat, rules.Crops, rules.Depot, TimeSpan.FromSeconds(batch));
            if (advance.SecondsAdvanced != batch)
                throw new InvalidOperationException("Benchmark clock did not advance the requested seconds.");
            if (batch == 1 && !V15StockAuditor.ReconcileSecond(current, advance).Balanced)
                throw new InvalidOperationException("Benchmark physical stock reconciliation failed.");
            current = advance.Next;
            seconds += batch;
            var gross = definition.ProducedId == "food" ? advance.ReadyFoodProduced :
                advance.FirstSecondProduction[definition.ProducedId];
            if (nextStep == definition.Steps.Length && gross.Milliunits > 0)
            {
                outcome = "pass";
                completion = seconds;
                stopReason = $"Actual {definition.ProducedId} production observed";
                events.Add(new(seconds, definition.Goal, "milestone", stopReason, 0, 0));
                samples.Add(Sample(current, seconds, "completed"));
                break;
            }
        }
        if (outcome == "timeout")
            stopReason = $"No milestone by {effectiveHorizon}s; next step: " +
                (nextStep < definition.Steps.Length ? definition.Steps[nextStep] : "await-production") +
                (lastFailure.Length == 0 ? string.Empty : $"; {lastFailure}");
        if (samples.Count == 0 || samples[^1].ElapsedSeconds != seconds)
            samples.Add(Sample(current, seconds,
                nextStep < definition.Steps.Length ? definition.Steps[nextStep] : "await-production"));

        return new(definition.Id, V15FoodCampaignCheckpoints.RulesVersion,
            baseline.Catalog.RulesVersion, PolicyId, "fresh-game/no-grants",
            definition.Goal, definition.Horizon, effectiveHorizon, diagnostic,
            outcome, completion, stopReason, initial.CityCampaign.Population,
            initialAmounts, events, samples);
    }

    private static V15NaturalProgressionSample Sample(V15FoodCampaignCheckpoint checkpoint,
        int seconds, string nextStep)
    {
        var city = checkpoint.CityCampaign;
        var total = V15StockAuditor.PhysicalTotals(checkpoint);
        var free = V15FoodCampaignCheckpoints.AvailableWorkers(checkpoint);
        return new(seconds, nextStep, city.Population, free,
            total["planks"].Milliunits, total["stone"].Milliunits,
            total["fish"].Milliunits, total["bread"].Milliunits,
            total["food"].Milliunits, total["game"].Milliunits,
            FoodIndustrySites.Get(checkpoint.Industry.Buildings.Counts, "food-depot"),
            FoodIndustrySites.Get(checkpoint.Industry.Buildings.Counts, "hunter"),
            city.ProductiveFamilies.Single(f => f.FamilyId == "bakery").Base.Buildings);
    }

    private readonly record struct Attempt(bool Success, string Failure, bool Terminal,
        ResourceBundle Cost);

    private static Attempt TryAction(V15FoodIndustryCampaignSession session, string action)
    {
        static Attempt Tier(V14TierActionResult result) => new(result.Succeeded,
            result.Failure.ToString(), result.Failure is not
                (V14TierActionFailure.None or V14TierActionFailure.InsufficientResources or
                 V14TierActionFailure.InsufficientPopulation), result.Cost);
        static Attempt Staff(V14TierActionFailure failure) => new(failure == V14TierActionFailure.None,
            failure.ToString(), failure is not
                (V14TierActionFailure.None or V14TierActionFailure.InsufficientPopulation), ResourceBundle.Empty);
        static Attempt City(V14CivicConstructionResult result) => new(result.Succeeded,
            result.Failure.ToString(), result.Failure is not
                (V14CivicConstructionFailure.None or V14CivicConstructionFailure.InsufficientResources), result.Cost);
        static Attempt Food(FoodIndustryBuildResult result) => new(result.Succeeded,
            result.Failure.ToString(), result.Failure is not
                (FoodIndustryActionFailure.None or FoodIndustryActionFailure.InsufficientResources or
                 FoodIndustryActionFailure.InsufficientPopulation), result.Cost);
        static Attempt FoodStaff(FoodIndustryActionFailure failure) => new(
            failure == FoodIndustryActionFailure.None, failure.ToString(),
            failure is not (FoodIndustryActionFailure.None or FoodIndustryActionFailure.InsufficientPopulation),
            ResourceBundle.Empty);
        return action switch
        {
            "build-fisher" => Tier(session.BuildCityProducer("fisher", ProductionTier.Base, 1)),
            "staff-fisher" => Staff(session.AssignCityProducerWorkers("fisher", ProductionTier.Base, 1)),
            "build-depot" => Food(session.Build("food-depot", 1)),
            "staff-depot" => FoodStaff(session.AssignWorkers("food-depot", 1)),
            "build-small-house" => City(session.BuildCityBuilding("small-house", 1)),
            "build-farm" => Tier(session.BuildCityProducer("farm", ProductionTier.Base, 1)),
            "staff-farm" => Staff(session.AssignCityProducerWorkers("farm", ProductionTier.Base, 1)),
            "build-well" => Tier(session.BuildCityProducer("well", ProductionTier.Base, 1)),
            "staff-well" => Staff(session.AssignCityProducerWorkers("well", ProductionTier.Base, 1)),
            "build-mill" => Tier(session.BuildCityProducer("mill", ProductionTier.Base, 1)),
            "staff-mill" => Staff(session.AssignCityProducerWorkers("mill", ProductionTier.Base, 1)),
            "build-bakery" => Tier(session.BuildCityProducer("bakery", ProductionTier.Base, 1)),
            "staff-bakery" => Staff(session.AssignCityProducerWorkers("bakery", ProductionTier.Base, 1)),
            "build-hunter" => Food(session.Build("hunter", 1)),
            "staff-hunter" => FoodStaff(session.AssignWorkers("hunter", 1)),
            _ => throw new ArgumentException($"Unknown benchmark action: {action}.", nameof(action))
        };
    }

    public static string SamplesCsv(IEnumerable<V15NaturalProgressionRun> runs)
    {
        var csv = new StringBuilder("scenario_id,rules_version,policy_id,goal,horizon_seconds,effective_horizon_seconds,diagnostic,outcome,completion_seconds,elapsed_seconds,next_action,population,free_workers,planks_milli,stone_milli,fish_milli,bread_milli,food_milli,game_milli,depots,hunters,bakeries\n");
        foreach (var run in runs)
            foreach (var s in run.Samples)
                csv.AppendLine(string.Join(",", new[] {
                    Escape(run.ScenarioId), Escape(run.RulesVersion), Escape(run.PolicyId),
                    Escape(run.Goal), N(run.DeclaredHorizonSeconds), N(run.EffectiveHorizonSeconds),
                    run.Diagnostic ? "true" : "false", Escape(run.Outcome), N(run.CompletionSeconds),
                    N(s.ElapsedSeconds), Escape(s.NextAction), N(s.Population), N(s.FreeWorkers),
                    N(s.PlanksMilliunits), N(s.StoneMilliunits), N(s.FishMilliunits),
                    N(s.BreadMilliunits), N(s.PreparedFoodMilliunits), N(s.GameMilliunits),
                    N(s.DepotCount), N(s.HunterCount), N(s.BakeryCount)
                }));
        return csv.ToString();
    }

    public static string EventsCsv(IEnumerable<V15NaturalProgressionRun> runs)
    {
        var csv = new StringBuilder("scenario_id,elapsed_seconds,action,result,reason,planks_spent_milli,stone_spent_milli\n");
        foreach (var run in runs)
            foreach (var e in run.Events)
                csv.AppendLine(string.Join(",", new[] { Escape(run.ScenarioId), N(e.ElapsedSeconds),
                    Escape(e.Action), Escape(e.Result), Escape(e.Reason),
                    N(e.PlanksSpentMilliunits), N(e.StoneSpentMilliunits) }));
        return csv.ToString();
    }

    private static string N(long? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "";
    private static string Escape(string value) => value.IndexOfAny(['"', ',', '\r', '\n']) < 0 ?
        value : '"' + value.Replace("\"", "\"\"") + '"';
}

public sealed record V15NaturalProgressionEvent(int ElapsedSeconds, string Action,
    string Result, string Reason, long PlanksSpentMilliunits, long StoneSpentMilliunits);

public sealed record V15NaturalProgressionSample(int ElapsedSeconds, string NextAction,
    int Population, long FreeWorkers, long PlanksMilliunits, long StoneMilliunits,
    long FishMilliunits, long BreadMilliunits, long PreparedFoodMilliunits,
    long GameMilliunits, long DepotCount, long HunterCount, int BakeryCount);

public sealed record V15NaturalProgressionRun(string ScenarioId, string RulesVersion,
    string BaselineRulesVersion, string PolicyId, string Fixture, string Goal,
    int DeclaredHorizonSeconds, int EffectiveHorizonSeconds, bool Diagnostic,
    string Outcome, int? CompletionSeconds, string StopReason,
    int InitialPopulation, IReadOnlyDictionary<string, long> InitialPhysicalStockMilliunits,
    IReadOnlyList<V15NaturalProgressionEvent> Events,
    IReadOnlyList<V15NaturalProgressionSample> Samples);
