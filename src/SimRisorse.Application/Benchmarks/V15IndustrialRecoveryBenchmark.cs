using System.Globalization;
using System.Text;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;

namespace SimRisorse.Application.Benchmarks;

public sealed record V15ProductionLossSample(int Second, string FamilyId, string ResourceId,
    int AssignedWorkers, int RunnableWorkers, long NominalMilliunits, long ProducedMilliunits,
    long StorageOnlyUnproducedMilliunits, long InputsOnlyUnproducedMilliunits,
    long MixedUnproducedMilliunits, long OtherUnproducedMilliunits,
    ProductionBlockReason BlockReason, IReadOnlyList<string> MissingInputIds,
    IReadOnlyList<string> FullOutputIds);

public sealed record V15ProductionLossTotal(string FamilyId, string ResourceId,
    long NominalMilliunits, long ProducedMilliunits, long StorageOnlyUnproducedMilliunits,
    long InputsOnlyUnproducedMilliunits, long MixedUnproducedMilliunits,
    long OtherUnproducedMilliunits, int StorageOnlySeconds, int InputsOnlySeconds, int MixedSeconds);

public sealed record V15RecoveryAction(int Second, string Action, bool Succeeded, string Failure,
    IReadOnlyDictionary<string, long> PaidMilliunits);

public sealed record V15IndustrialRecoveryRun(string ScenarioId, string PolicyId, string Version,
    string RulesVersion, string BaselineRulesVersion, string Fixture, int GrowthMultiplierCap,
    int DeclaredHorizonSeconds, int EffectiveHorizonSeconds, bool Diagnostic,
    string TargetResourceId, int? FirstProductionSecond, long ProducedTargetMilliunits,
    long MealsServed, long MealsMissed, V15FoodCampaignCheckpoint InitialCheckpoint,
    V15FoodCampaignCheckpoint FinalCheckpoint, IReadOnlyList<V15RecoveryAction> Actions,
    IReadOnlyList<V15ProductionLossSample> Samples, IReadOnlyList<V15ProductionLossTotal> Totals);

/// <summary>
/// RC77/E4a.3: explicitly funded, controlled production experiments, NOT natural
/// progression measurements. Each policy starts from the same immutable state,
/// uses real city commands and advances the real v15 clock one second at a time.
/// Unproduced output means assigned-worker potential, never destroyed inventory.
/// Mixed input/storage constraints remain unattributed to either individual cause.
/// </summary>
public static class V15IndustrialRecoveryBenchmark
{
    public const string Version = "v15-controlled-industrial-recovery-1";
    public const int HorizonSeconds = 120;
    public static IReadOnlyList<string> ScenarioIds { get; } =
        Array.AsReadOnly(new[] { "V15-R01", "V15-R02", "V15-R03" });
    private static readonly string[] Families = ["coal-mine", "iron-smelter", "gold-smelter", "forge", "mint"];

    public static V15BenchmarkFixture CreateFixture(string scenarioId)
    {
        if (!ScenarioIds.Contains(scenarioId))
            throw new ArgumentException("Unknown controlled recovery scenario.", nameof(scenarioId));
        var setup = V15BenchmarkFixture.Create();
        var inherited = setup.Start.CityCampaign;
        // All industrial sites and buffers below are declared TEST grants.
        // Forty residents occupy five small houses, so growth is blocked by
        // housing and does not confound the policy comparison. Food covers 120s.
        var counts = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["iron-mine"] = 1, ["coal-mine"] = 3, ["gold-mine"] = 1,
            ["iron-smelter"] = 1, ["gold-smelter"] = 1, ["forge"] = 1, ["mint"] = 1
        };
        int Staff(string id) => scenarioId switch
        {
            "V15-R01" => id == "forge" ? 1 : 0,
            "V15-R02" => id == "mint" ? 1 : 0,
            _ => Families.Contains(id) ? 1 : 0
        };
        var city = inherited with
        {
            Population = 40,
            OtherBuildings = [.. inherited.OtherBuildings.Select(b =>
                b.BuildingId == "small-house" ? b with { Count = 5 } : b)],
            ProductiveFamilies = [.. inherited.ProductiveFamilies.Select(f => f with
            {
                Base = new TierOccupancy(counts.GetValueOrDefault(f.FamilyId, f.Base.Buildings), Staff(f.FamilyId))
            })],
            PurchaseCounters = [.. inherited.PurchaseCounters.Select(c => c with
            {
                Acquired = c.FamilyId == "small-house" ? 5 : counts.GetValueOrDefault(c.FamilyId, c.Acquired)
            })],
            Resources = ResourceBundle.Create(("planks", 1000m), ("stone", 1000m),
                ("iron-ore", 300m), ("gold-ore", 150m), ("iron", 200m), ("gold", 200m),
                ("coal", scenarioId == "V15-R03" ? 0m : 200m))
        };
        if (scenarioId is "V15-R01" or "V15-R02")
        {
            var id = scenarioId == "V15-R01" ? "swords" : "coins";
            city = city with { Resources = city.Resources + ResourceBundle.FromAmounts([
                KeyValuePair.Create(id, V15CityStorage.Calculate(setup.Baseline, city)[id])]) };
        }
        var start = setup.Start with
        {
            CityCampaign = city,
            Depot = new FoodDepotStock(ResourceBundle.Empty, ResourceAmount.FromUnits(100m)),
            Industry = setup.Start.Industry with
            {
                Buildings = new FoodIndustryBuildings(new FoodIndustryCrew(0, 0, 0, 0, 0, 1),
                    new FoodIndustryCrew(0, 0, 0, 0, 0, 0))
            }
        };
        V15FoodCampaignCheckpoints.Validate(start, setup.Baseline, setup.School, setup.Technologies, setup.Depot);
        return setup with { Start = start };
    }

    public static IReadOnlyList<V15IndustrialRecoveryRun> RunAll(int? maxSeconds = null) =>
        ScenarioIds.SelectMany(id => RunScenario(id, maxSeconds)).ToArray();

    public static IReadOnlyList<V15IndustrialRecoveryRun> RunScenario(string scenarioId, int? maxSeconds = null)
    {
        if (maxSeconds is < 0 or > HorizonSeconds)
            throw new ArgumentOutOfRangeException(nameof(maxSeconds), "Use an integer between 0 and 120 seconds.");
        var setup = CreateFixture(scenarioId);
        return new[] { Run(setup, scenarioId, false, maxSeconds ?? HorizonSeconds),
            Run(setup, scenarioId, true, maxSeconds ?? HorizonSeconds) };
    }

    private static V15IndustrialRecoveryRun Run(V15BenchmarkFixture setup, string scenarioId,
        bool recovery, int seconds)
    {
        var state = setup.Start;
        var actions = new List<V15RecoveryAction>();
        var samples = new List<V15ProductionLossSample>();
        var target = scenarioId == "V15-R01" ? "swords" : "coins";
        var policy = !recovery ? "baseline" : scenarioId == "V15-R03"
            ? "assign-coal-workers" : "build-warehouse";
        long served = 0, missed = 0;
        int? first = null;
        long producedTarget = 0;
        // A zero-second diagnostic observes the initial state; it does not
        // execute recovery commands outside the measured interval.
        if (recovery && seconds > 0)
        {
            var session = new V15FoodIndustryCampaignSession(state, setup.Baseline,
                setup.School, setup.Technologies, setup.Depot);
            if (scenarioId == "V15-R03")
            {
                var result = session.AssignCityProducerWorkers("coal-mine", ProductionTier.Base, 2);
                actions.Add(new(0, policy, result == SimRisorse.Core.Sessions.V14TierActionFailure.None,
                    result.ToString(), new Dictionary<string, long>()));
            }
            else
            {
                var result = session.BuildCityBuilding("warehouse", 1);
                actions.Add(new(0, policy, result.Succeeded, result.Failure.ToString(),
                    result.Succeeded ? Amounts(result.Cost) : new Dictionary<string, long>()));
            }
            state = session.Snapshot();
        }
        for (int second = 1; second <= seconds; second++)
        {
            var plan = V15UnifiedCampaignClock.Plan(state, setup.Baseline, setup.School, setup.Technologies,
                FoodIndustryConstructionCatalog.Candidate, setup.Meat, setup.Crops, setup.Depot,
                TimeSpan.FromSeconds(1), includeProductionDiagnostics: true);
            samples.AddRange(MeasureSecond(second, state, plan, setup.Baseline));
            var produced = plan.FirstSecondProduction[target].Milliunits;
            producedTarget = checked(producedTarget + produced);
            if (produced > 0) first ??= second;
            served += plan.MealsServed;
            missed += plan.MealsMissed;
            state = plan.Next;
        }
        var totals = samples.GroupBy(s => (s.FamilyId, s.ResourceId)).Select(g =>
            new V15ProductionLossTotal(g.Key.FamilyId, g.Key.ResourceId,
                g.Sum(s => s.NominalMilliunits), g.Sum(s => s.ProducedMilliunits),
                g.Sum(s => s.StorageOnlyUnproducedMilliunits), g.Sum(s => s.InputsOnlyUnproducedMilliunits),
                g.Sum(s => s.MixedUnproducedMilliunits), g.Sum(s => s.OtherUnproducedMilliunits),
                g.Count(s => s.StorageOnlyUnproducedMilliunits > 0),
                g.Count(s => s.InputsOnlyUnproducedMilliunits > 0), g.Count(s => s.MixedUnproducedMilliunits > 0)))
            .ToArray();
        return new(scenarioId, policy, Version, V15FoodCampaignCheckpoints.RulesVersion,
            setup.Baseline.Catalog.RulesVersion, "controlled/declared-grants", V15CampaignGrowthProjection.MaximumGrowthMultiplier,
            HorizonSeconds, seconds, seconds < HorizonSeconds, target, first, producedTarget,
            served, missed, setup.Start, state, actions, samples, totals);
    }

    public static IReadOnlyList<V15ProductionLossSample> MeasureSecond(int second,
        V15FoodCampaignCheckpoint before, V15CampaignAdvancePlan plan, GameScenario baseline)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(second, 1);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(baseline);
        if (!V15StockAuditor.ReconcileSecond(before, plan).Balanced)
            throw new InvalidOperationException("Recovery experiment failed physical stock reconciliation.");
        if (plan.FirstSecondBuildings is null)
            throw new ArgumentException("Production diagnostics were not requested; loss is unmeasured.", nameof(plan));
        var result = new List<V15ProductionLossSample>();
        foreach (var report in plan.FirstSecondBuildings.Where(b => Families.Contains(b.BuildingId)))
        {
            var recipe = baseline.Catalog.BuildingsById[report.BuildingId].Recipe!;
            foreach (var (id, quantity) in recipe.Output.Amounts)
            {
                var nominal = checked(quantity.Milliunits * report.AssignedWorkers);
                var actual = checked(quantity.Milliunits * report.RunnableWorkers);
                var unproduced = checked(nominal - actual);
                if (unproduced < 0) throw new InvalidOperationException("Runnable workers exceed assigned workers.");
                var storage = report.BlockReason.HasFlag(ProductionBlockReason.StorageFull);
                var inputs = report.BlockReason.HasFlag(ProductionBlockReason.MissingInputs);
                result.Add(new(second, report.BuildingId, id, report.AssignedWorkers, report.RunnableWorkers,
                    nominal, actual, storage && !inputs ? unproduced : 0,
                    inputs && !storage ? unproduced : 0, storage && inputs ? unproduced : 0,
                    !storage && !inputs ? unproduced : 0, report.BlockReason,
                    report.MissingInputIds, report.FullOutputIds));
            }
        }
        return result;
    }

    private static IReadOnlyDictionary<string, long> Amounts(ResourceBundle bundle) =>
        bundle.Amounts.ToDictionary(kv => kv.Key, kv => kv.Value.Milliunits, StringComparer.Ordinal);

    public static string SamplesCsv(IEnumerable<V15IndustrialRecoveryRun> runs)
    {
        var csv = new StringBuilder("scenario_id,policy_id,experiment_version,rules_version,fixture,declared_horizon_seconds,effective_horizon_seconds,diagnostic,second,family_id,resource_id,assigned_workers,runnable_workers,nominal_milli,produced_milli,storage_only_unproduced_milli,inputs_only_unproduced_milli,mixed_unproduced_milli,other_unproduced_milli,block_reason,missing_inputs,full_outputs\n");
        foreach (var run in runs)
            foreach (var s in run.Samples)
                csv.AppendLine(string.Join(",", run.ScenarioId, run.PolicyId, run.Version, run.RulesVersion,
                    run.Fixture, N(run.DeclaredHorizonSeconds), N(run.EffectiveHorizonSeconds),
                    run.Diagnostic ? "true" : "false", N(s.Second), s.FamilyId, s.ResourceId,
                    N(s.AssignedWorkers), N(s.RunnableWorkers), N(s.NominalMilliunits), N(s.ProducedMilliunits),
                    N(s.StorageOnlyUnproducedMilliunits), N(s.InputsOnlyUnproducedMilliunits),
                    N(s.MixedUnproducedMilliunits), N(s.OtherUnproducedMilliunits),
                    Escape(s.BlockReason.ToString()), string.Join("|", s.MissingInputIds), string.Join("|", s.FullOutputIds)));
        return csv.ToString();
    }

    public static string ActionsCsv(IEnumerable<V15IndustrialRecoveryRun> runs)
    {
        var csv = new StringBuilder("scenario_id,policy_id,experiment_version,rules_version,fixture,diagnostic,second,action,succeeded,failure,resource_id,paid_milli\n");
        foreach (var run in runs)
            foreach (var action in run.Actions)
            {
                var paid = action.PaidMilliunits.Count == 0
                    ? new Dictionary<string, long> { [string.Empty] = 0 } : action.PaidMilliunits;
                foreach (var (id, amount) in paid)
                    csv.AppendLine(string.Join(",", run.ScenarioId, run.PolicyId, run.Version, run.RulesVersion,
                        run.Fixture, run.Diagnostic ? "true" : "false", N(action.Second), action.Action,
                        action.Succeeded ? "true" : "false", action.Failure, id, N(amount)));
            }
        return csv.ToString();
    }

    public static string ToMarkdown(IEnumerable<V15IndustrialRecoveryRun> runs)
    {
        var text = new StringBuilder("# RC77 / E4a.3 — Produzione non realizzata e recupero controllato\n\n");
        text.AppendLine("Scenari **controlled/declared-grants**, non partite naturali: edifici, abitanti e scorte iniziali sono forniti e dichiarati nei checkpoint JSON.\n");
        text.AppendLine("Il potenziale nominale usa gli addetti assegnati. La differenza e' output non prodotto, non scorta distrutta. I vincoli misti non sono attribuiti separatamente a input o spazio. Nessun esito dimostra un deadlock o un tempo di progressione naturale.\n");
        text.AppendLine("| Scenario | Politica | Secondi | Diagnostica | Prima produzione | Output target (millesimi) | Spesa effettiva |\n| --- | --- | ---: | --- | ---: | ---: | --- |");
        foreach (var run in runs)
        {
            var paid = string.Join("; ", run.Actions.SelectMany(a => a.PaidMilliunits)
                .Select(kv => $"{kv.Key}={N(kv.Value)} millesimi"));
            text.AppendLine($"| {run.ScenarioId} | {run.PolicyId} | {N(run.EffectiveHorizonSeconds)} | " +
                $"{run.Diagnostic} | {(run.FirstProductionSecond is int first ? N(first) : "non osservata")} | " +
                $"{run.TargetResourceId}: {N(run.ProducedTargetMilliunits)} | {(paid.Length == 0 ? "0" : paid)} |");
        }
        foreach (var run in runs)
        {
            text.AppendLine($"\n## {run.ScenarioId} / {run.PolicyId}\n");
            foreach (var action in run.Actions)
                text.AppendLine($"Azione t={N(action.Second)}: {action.Action}; riuscita={action.Succeeded}; esito={action.Failure}.\n");
            text.AppendLine("| Famiglia | Risorsa | Nominale | Prodotto | Non prodotto: solo spazio | Solo input | Misto | Altro |\n| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |");
            foreach (var row in run.Totals)
                text.AppendLine($"| {row.FamilyId} | {row.ResourceId} | {N(row.NominalMilliunits)} | " +
                    $"{N(row.ProducedMilliunits)} | {N(row.StorageOnlyUnproducedMilliunits)} | " +
                    $"{N(row.InputsOnlyUnproducedMilliunits)} | {N(row.MixedUnproducedMilliunits)} | {N(row.OtherUnproducedMilliunits)} |");
        }
        text.AppendLine("\nTutti gli output e le spese nelle tabelle sono in millesimi. La produzione e' lorda: i trasferimenti e le scorte iniziali non vengono conteggiati come produzione.\n");
        return text.ToString();
    }

    private static string N(long value) => value.ToString(CultureInfo.InvariantCulture);
    private static string Escape(string value) => value.Contains(',') ? '"' + value.Replace("\"", "\"\"") + '"' : value;
}
