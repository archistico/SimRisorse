using System.Globalization;
using System.Text;

namespace SimRisorse.Application.Benchmarks;

/// <summary>Audit recorded advanced strategies without simulating or awarding grants.
/// Outputs are evidence summaries; no 'optimal' policy can be inferred from one run.</summary>
public static class V15IndustrialProgressionAnalysis
{
    public const string Version = "e4a-v15-industry-analysis-1";

    public static V15IndustrialComparisonReport Analyze(IReadOnlyList<V15IndustrialProgressionRun> runs)
    {
        ArgumentNullException.ThrowIfNull(runs);
        if (runs.Count == 0) throw new ArgumentException("At least one run is required.", nameof(runs));
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var rows = new List<V15IndustrialComparisonRow>();
        foreach (var run in runs)
        {
            Validate(run);
            if (!seen.Add(run.ScenarioId + "/" + run.PolicyId))
                throw new ArgumentException("Duplicate scenario/policy pair.", nameof(runs));
            var costs = new SortedDictionary<string, long>(StringComparer.Ordinal);
            var researchCost = 0L;
            foreach (var evt in run.Events.Where(e => e.Result == "executed"))
            {
                researchCost = checked(researchCost + evt.ResearchSpentMilliPoints);
                foreach (var entry in evt.ResourceCostsMilliunits)
                    costs[entry.Key] = checked(costs.GetValueOrDefault(entry.Key) + entry.Value);
            }
            var waiting = new SortedDictionary<string, long>(StringComparer.Ordinal);
            var waitSeconds = 0L;
            var last = run.Samples[^1];
            for (var i = 0; i < run.Events.Count; i++)
            {
                var e = run.Events[i];
                if (e.Result != "waiting") continue;
                var end = last.ElapsedSeconds;
                for (var j = i + 1; j < run.Events.Count; j++)
                    if (run.Events[j].Action == e.Action &&
                        run.Events[j].Result is ("waiting" or "executed" or "blocked"))
                    {
                        end = run.Events[j].ElapsedSeconds;
                        break;
                    }
                var duration = Math.Max(0L, (long)end - e.ElapsedSeconds);
                waitSeconds = checked(waitSeconds + duration);
                var constraint = Constraint(e.Reason);
                waiting[constraint] = checked(waiting.GetValueOrDefault(constraint) + duration);
            }
            var classification = run.Diagnostic ? "diagnostic-only" : run.Outcome switch
            {
                "pass" => "observed-scripted-milestone",
                "blocked" => "policy-blocked",
                _ => "policy-horizon-exceeded"
            };
            rows.Add(new(run.ScenarioId, run.PolicyId, run.Outcome,
                run.Diagnostic, classification, run.CompletionSeconds, last.ElapsedSeconds,
                waitSeconds, waiting, run.Events.Count(e => e.Result == "executed"),
                costs, researchCost, run.Samples.Min(s => s.FreeWorkers),
                run.Samples[^1].NextAction, run.StopReason));
        }
        var paired = new List<V15IndustrialPolicyComparison>();
        foreach (var group in rows.GroupBy(r => r.ScenarioId))
        {
            var research = group.SingleOrDefault(r => r.PolicyId == V15IndustrialProgressionBenchmark.PolicyResearchFirst);
            var bread = group.SingleOrDefault(r => r.PolicyId == V15IndustrialProgressionBenchmark.PolicyBreadFirst);
            if (research is null || bread is null) continue;
            // Only like-for-like full-horizon, observed milestones receive a time difference.
            long? difference = !research.Diagnostic && !bread.Diagnostic &&
                research.CompletionSeconds.HasValue && bread.CompletionSeconds.HasValue
                ? checked((long)bread.CompletionSeconds.Value - research.CompletionSeconds.Value) : null;
            paired.Add(new(group.Key, difference, difference is null ?
                "Non confrontabile: completamento osservato a orizzonte pieno richiesto per entrambe le politiche." :
                "Differenza osservata fra due politiche scripted; non prova l'ottimalita'."));
        }
        return new(Version, rows.Count, rows.Count(r => r.Outcome == "pass"),
            rows.Count(r => r.Diagnostic), rows, paired);
    }

    private static string Constraint(string reason) => reason switch
    {
        "InsufficientResources" or "InsufficientMaterials" => "materials",
        "InsufficientPopulation" => "workers",
        "InsufficientFood" => "prepared-food",
        "InsufficientPencils" => "pencils",
        "InsufficientQualifiedResearchers" => "training",
        "InsufficientResearchPoints" => "research-points",
        _ => "other"
    };

    private static void Validate(V15IndustrialProgressionRun? run)
    {
        if (run is null || string.IsNullOrWhiteSpace(run.ScenarioId) ||
            run.PolicyId is not (V15IndustrialProgressionBenchmark.PolicyResearchFirst or
                V15IndustrialProgressionBenchmark.PolicyBreadFirst) ||
            run.Fixture != "fresh-game/no-grants" || string.IsNullOrWhiteSpace(run.Goal) ||
            run.Events is null || run.Samples is null || run.Samples.Count == 0 ||
            run.InitialPhysicalStockMilliunits is null ||
            run.DeclaredHorizonSeconds <= 0 || run.EffectiveHorizonSeconds < 0 ||
            run.EffectiveHorizonSeconds > run.DeclaredHorizonSeconds ||
            run.Diagnostic != (run.EffectiveHorizonSeconds < run.DeclaredHorizonSeconds) ||
            run.Outcome is not ("pass" or "timeout" or "blocked"))
            throw new ArgumentException("Invalid advanced progression report.", nameof(run));
        if (run.Samples[0].ElapsedSeconds != 0 ||
            !Ordered(run.Samples.Select(s => s.ElapsedSeconds)) ||
            !Ordered(run.Events.Select(e => e.ElapsedSeconds)) ||
            run.Samples.Any(s => s.ElapsedSeconds > run.EffectiveHorizonSeconds ||
                s.ElapsedSeconds < 0 || s.FreeWorkers < 0 || s.ResearchBalanceMilliPoints < 0) ||
            run.Events.Any(e => e.ElapsedSeconds < 0 || e.ElapsedSeconds > run.EffectiveHorizonSeconds ||
                e.Result is not ("waiting" or "executed" or "blocked" or "milestone") ||
                e.ResourceCostsMilliunits is null || e.ResearchSpentMilliPoints < 0 ||
                e.ResourceCostsMilliunits.Any(p => string.IsNullOrWhiteSpace(p.Key) || p.Value < 0) ||
                (e.Result != "executed" &&
                    (e.ResourceCostsMilliunits.Count != 0 || e.ResearchSpentMilliPoints != 0))) ||
            run.InitialPhysicalStockMilliunits.Any(p => p.Value < 0) ||
            run.Events.Any(e => e.MissingResourceMilliunits is not null &&
                ((e.Result is not ("waiting" or "blocked") && e.MissingResourceMilliunits.Count > 0) ||
                e.MissingResourceMilliunits.Any(p => string.IsNullOrWhiteSpace(p.Key) || p.Value <= 0))))
            throw new ArgumentException("Malformed, unsorted or negative advanced progression ledger.", nameof(run));
        var last = run.Samples[^1].ElapsedSeconds;
        if (run.Outcome == "pass")
        {
            if (run.CompletionSeconds is null || run.CompletionSeconds.Value != last ||
                !run.Events.Any(e => e.Result == "milestone" && e.ElapsedSeconds == last))
                throw new ArgumentException("Completion must have an observed milestone.", nameof(run));
        }
        else if (run.CompletionSeconds is not null || run.Events.Any(e => e.Result == "milestone") ||
            run.Outcome == "timeout" && last != run.EffectiveHorizonSeconds ||
            run.Outcome == "blocked" && !run.Events.Any(e => e.Result == "blocked"))
            throw new ArgumentException("Outcome contradicts the advanced event log.", nameof(run));
    }

    private static bool Ordered(IEnumerable<int> positions)
    {
        var previous = -1;
        foreach (var position in positions)
        {
            if (position < previous) return false;
            previous = position;
        }
        return true;
    }

    public static string ToCsv(V15IndustrialComparisonReport report)
    {
        var sb = new StringBuilder("scenario_id,policy_id,outcome,diagnostic,classification,completion_seconds,observed_seconds,waiting_seconds,actions_executed,min_free_workers,resource_id,spent_milli,research_spent_millipoints,pending_action,stop_reason\n");
        foreach (var row in report.Scenarios)
        {
            IEnumerable<KeyValuePair<string, long>> costs = row.ResourceCostsMilliunits.Count == 0
                ? new[] { new KeyValuePair<string, long>("", 0) }
                : row.ResourceCostsMilliunits.OrderBy(p => p.Key, StringComparer.Ordinal);
            var first = true;
            foreach (var cost in costs)
            {
                // The material ledger is normalized by resource. Research is
                // a run-level total and must not be multiplied by row count.
                sb.AppendLine(string.Join(",", Esc(row.ScenarioId), Esc(row.PolicyId),
                    Esc(row.Outcome), row.Diagnostic ? "true" : "false", Esc(row.Classification),
                    Num(row.CompletionSeconds), Num(row.ObservedSeconds), Num(row.WaitingSeconds),
                    Num(row.ExecutedActions), Num(row.MinFreeWorkers), Esc(cost.Key), Num(cost.Value),
                    first ? Num(row.ResearchSpentMilliPoints) : "", Esc(row.PendingAction), Esc(row.StopReason)));
                first = false;
            }
        }
        return sb.ToString();
    }

    public static string ToMarkdown(V15IndustrialComparisonReport report)
    {
        var sb = new StringBuilder("# E4a.2b — Benchmark industriali v15\n\n");
        sb.AppendLine($"Analisi `{report.AnalysisVersion}` · prove: {report.RunCount} · " +
            $"obiettivi osservati: {report.Passed} · diagnostiche: {report.Diagnostics}.\n");
        sb.AppendLine("**Cautela:** una strategia scripted non rappresenta un giocatore ottimale; " +
            "timeout e blocked non dimostrano uno stallo strutturale. Costi = materiali **effettivamente pagati** " +
            "dalle azioni registrate, non consumi produttivi; punti ricerca indicati a parte.\n");
        sb.AppendLine("| Scenario | Strategia | Esito | Secondi | Attese (s) | Materiali pagati (millesimi) | Ricerca spesa (millipunti) |");
        sb.AppendLine("|---|---|---|---:|---:|---|---:|");
        foreach (var row in report.Scenarios)
        {
            var costs = row.ResourceCostsMilliunits.Count == 0 ? "nessuno" : string.Join("; ",
                row.ResourceCostsMilliunits.OrderBy(p => p.Key, StringComparer.Ordinal)
                    .Select(p => $"{p.Key}={p.Value}"));
            sb.AppendLine($"| {row.ScenarioId} | {row.PolicyId} | {row.Classification} | " +
                $"{(row.CompletionSeconds?.ToString(CultureInfo.InvariantCulture) ?? "—")} | " +
                $"{row.WaitingSeconds} | {costs} | {row.ResearchSpentMilliPoints} |");
        }
        sb.AppendLine("\n## Confronto di strategia\n");
        foreach (var comparison in report.Comparisons)
            sb.AppendLine($"- **{comparison.ScenarioId}:** " +
                (comparison.BreadFirstMinusResearchFirstSeconds is long delta
                    ? $"bread-first meno research-first = {delta} s. " : "Nessuna differenza temporale dimostrata. ") +
                comparison.EvidenceNote);
        return sb.ToString();
    }

    private static string Num(long? n) => n?.ToString(CultureInfo.InvariantCulture) ?? "";
    private static string Esc(string value) => value.IndexOfAny(['"', ',', '\r', '\n']) < 0 ?
        value : '"' + value.Replace("\"", "\"\"") + '"';
}

public sealed record V15IndustrialComparisonRow(string ScenarioId, string PolicyId, string Outcome,
    bool Diagnostic, string Classification, int? CompletionSeconds, int ObservedSeconds,
    long WaitingSeconds, IReadOnlyDictionary<string, long> WaitingSecondsByConstraint,
    int ExecutedActions, IReadOnlyDictionary<string, long> ResourceCostsMilliunits,
    long ResearchSpentMilliPoints, long MinFreeWorkers, string PendingAction, string StopReason);

public sealed record V15IndustrialPolicyComparison(string ScenarioId,
    long? BreadFirstMinusResearchFirstSeconds, string EvidenceNote);

public sealed record V15IndustrialComparisonReport(string AnalysisVersion, int RunCount,
    int Passed, int Diagnostics, IReadOnlyList<V15IndustrialComparisonRow> Scenarios,
    IReadOnlyList<V15IndustrialPolicyComparison> Comparisons);
