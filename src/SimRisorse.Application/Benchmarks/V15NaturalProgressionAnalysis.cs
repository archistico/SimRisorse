using System.Globalization;
using System.Text;

namespace SimRisorse.Application.Benchmarks;

/// <summary>
/// RC74 / E4a.2: evidence-based postprocessor for RC73 natural progression runs.
/// It never simulates a game or declares a structural deadlock from a scripted
/// strategy's timeout. Costs are only the two resources recorded in RC73 events.
/// </summary>
public static class V15NaturalProgressionAnalysis
{
    public const string AnalysisVersion = "e4a-v15-analysis-1";

    public static V15ProgressionReport Analyze(IReadOnlyList<V15NaturalProgressionRun> runs)
    {
        ArgumentNullException.ThrowIfNull(runs);
        if (runs.Count == 0)
            throw new ArgumentException("At least one natural benchmark result is required.", nameof(runs));

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var analyzed = new List<V15ProgressionScenarioAnalysis>(runs.Count);
        foreach (var run in runs)
        {
            Validate(run);
            if (!seen.Add(run.ScenarioId))
                throw new ArgumentException($"Duplicate scenario in analysis input: {run.ScenarioId}.", nameof(runs));
            analyzed.Add(AnalyzeOne(run));
        }

        return new(AnalysisVersion, analyzed.Count,
            analyzed.Count(row => row.Outcome == "pass"),
            analyzed.Count(row => row.Outcome == "timeout"),
            analyzed.Count(row => row.Outcome == "blocked"),
            analyzed.Count(row => row.Diagnostic), analyzed);
    }

    private static V15ProgressionScenarioAnalysis AnalyzeOne(V15NaturalProgressionRun run)
    {
        var final = run.Samples[^1];
        var waitSeconds = 0L;
        var waitingByConstraint = new Dictionary<string, long>(StringComparer.Ordinal);
        for (var i = 0; i < run.Events.Count; i++)
        {
            var entry = run.Events[i];
            if (entry.Result != "waiting") continue;
            var end = final.ElapsedSeconds;
            for (var j = i + 1; j < run.Events.Count; j++)
            {
                // A new wait reason or a resolution closes the prior interval.
                if (run.Events[j].Action == entry.Action &&
                    run.Events[j].Result is ("waiting" or "executed" or "blocked"))
                {
                    end = run.Events[j].ElapsedSeconds;
                    break;
                }
            }
            var seconds = Math.Max(0L, (long)end - entry.ElapsedSeconds);
            waitSeconds = checked(waitSeconds + seconds);
            var constraint = Constraint(entry.Reason);
            waitingByConstraint[constraint] = checked(
                waitingByConstraint.GetValueOrDefault(constraint) + seconds);
        }

        var lastFailed = run.Events.LastOrDefault(e => e.Result is "waiting" or "blocked");
        var primaryConstraint = run.Outcome == "pass" || final.NextAction == "await-production" ||
            lastFailed is null ? "none" : Constraint(lastFailed.Reason);
        var classification = run.Diagnostic ? "diagnostic-only" : run.Outcome switch
        {
            "pass" => "milestone-observed",
            "blocked" => "script-policy-blocked",
            _ when final.NextAction == "await-production" => "production-not-observed-within-horizon",
            _ => "script-policy-timeout"
        };

        var executed = run.Events.Where(e => e.Result == "executed").ToArray();
        var notes = run.Diagnostic
            ? "Shortened horizon: not a full-horizon gameplay measurement."
            : run.Outcome == "pass"
                ? "Milestone observed under this scripted policy; not an optimal-player time."
                : "Outcome is specific to this scripted policy and horizon; structural softlock NOT demonstrated.";
        return new(run.ScenarioId, run.Goal, run.PolicyId, run.Outcome, run.Diagnostic,
            run.DeclaredHorizonSeconds, run.EffectiveHorizonSeconds, final.ElapsedSeconds,
            run.CompletionSeconds, classification, primaryConstraint, waitSeconds,
            waitingByConstraint.OrderBy(p => p.Key, StringComparer.Ordinal)
                .ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal),
            executed.Length,
            executed.Sum(e => (decimal)e.PlanksSpentMilliunits),
            executed.Sum(e => (decimal)e.StoneSpentMilliunits),
            run.Samples.Min(s => s.FreeWorkers), final.FreeWorkers,
            final.PlanksMilliunits, final.StoneMilliunits, final.NextAction,
            run.StopReason, notes);
    }

    private static string Constraint(string reason)
    {
        if (reason.Contains("InsufficientResources", StringComparison.OrdinalIgnoreCase))
            return "resources";
        if (reason.Contains("InsufficientPopulation", StringComparison.OrdinalIgnoreCase))
            return "population";
        if (reason.Contains("Locked", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("Prerequisite", StringComparison.OrdinalIgnoreCase) ||
            reason.Contains("Required", StringComparison.OrdinalIgnoreCase))
            return "prerequisite-or-technology";
        return "other-or-unknown";
    }

    private static void Validate(V15NaturalProgressionRun? run)
    {
        if (run is null || string.IsNullOrWhiteSpace(run.ScenarioId) ||
            string.IsNullOrWhiteSpace(run.Goal) || string.IsNullOrWhiteSpace(run.PolicyId) ||
            run.Fixture != "fresh-game/no-grants" ||
            run.Events is null || run.Samples is null || run.Samples.Count == 0 ||
            run.InitialPhysicalStockMilliunits is null ||
            run.DeclaredHorizonSeconds <= 0 || run.EffectiveHorizonSeconds < 0 ||
            run.EffectiveHorizonSeconds > run.DeclaredHorizonSeconds ||
            run.Diagnostic != (run.EffectiveHorizonSeconds < run.DeclaredHorizonSeconds) ||
            run.Outcome is not ("pass" or "timeout" or "blocked"))
            throw new ArgumentException("Invalid natural benchmark report contract.", nameof(run));

        if (run.Samples[0] is null || run.Samples[0].ElapsedSeconds != 0 ||
            run.Samples.Any(s => s is null || s.ElapsedSeconds < 0 || s.FreeWorkers < 0 ||
                s.ElapsedSeconds > run.EffectiveHorizonSeconds) ||
            run.Events.Any(e => e is null || e.ElapsedSeconds < 0 ||
                e.ElapsedSeconds > run.EffectiveHorizonSeconds ||
                e.PlanksSpentMilliunits < 0 || e.StoneSpentMilliunits < 0 ||
                e.Action is null || e.Reason is null ||
                e.Result is not ("waiting" or "blocked" or "executed" or "milestone")) ||
            !IsOrdered(run.Samples.Select(s => s.ElapsedSeconds)) ||
            !IsOrdered(run.Events.Select(e => e.ElapsedSeconds)))
            throw new ArgumentException("Events or samples are missing, unsorted or outside the run horizon.", nameof(run));

        var finalSeconds = run.Samples[^1].ElapsedSeconds;
        if (run.Outcome == "pass")
        {
            if (run.CompletionSeconds is null or <= 0 ||
                run.CompletionSeconds.Value != finalSeconds ||
                !run.Events.Any(e => e.Result == "milestone" &&
                    e.ElapsedSeconds == run.CompletionSeconds.Value))
                throw new ArgumentException("A completed milestone needs observed production evidence.", nameof(run));
        }
        else if (run.CompletionSeconds is not null || run.Events.Any(e => e.Result == "milestone") ||
            (run.Outcome == "timeout" && finalSeconds != run.EffectiveHorizonSeconds) ||
            (run.Outcome == "blocked" && !run.Events.Any(e => e.Result == "blocked")))
            throw new ArgumentException("The incomplete outcome contradicts the event log.", nameof(run));
    }

    private static bool IsOrdered(IEnumerable<int> seconds)
    {
        var previous = -1;
        foreach (var second in seconds)
        {
            if (second < previous) return false;
            previous = second;
        }
        return true;
    }

    public static string ToCsv(V15ProgressionReport report)
    {
        var sb = new StringBuilder("scenario_id,policy_id,outcome,diagnostic,classification,primary_constraint,completion_seconds,observed_seconds,wait_seconds,actions_executed,planks_spent_milli,stone_spent_milli,min_free_workers,last_free_workers,next_action,stop_reason,evidence_note\n");
        foreach (var row in report.Scenarios)
            sb.AppendLine(string.Join(",", new[] {
                Csv(row.ScenarioId), Csv(row.PolicyId), Csv(row.Outcome),
                row.Diagnostic ? "true" : "false", Csv(row.Classification), Csv(row.PrimaryConstraint),
                N(row.CompletionSeconds), N(row.ObservedSeconds), N(row.WaitingSeconds),
                N(row.ExecutedActions), N(row.PlanksSpentMilliunits), N(row.StoneSpentMilliunits),
                N(row.MinimumFreeWorkers), N(row.FinalFreeWorkers), Csv(row.NextAction),
                Csv(row.StopReason), Csv(row.EvidenceNote)
            }));
        return sb.ToString();
    }

    public static string ToMarkdown(V15ProgressionReport report)
    {
        var sb = new StringBuilder("# E4a v15 — Analisi dei benchmark naturali\n\n");
        sb.AppendLine($"Analizzatore: `{report.AnalysisVersion}`; scenari: {report.ScenarioCount}; " +
            $"pass: {report.Completed}; timeout: {report.TimedOut}; blocked: {report.Blocked}; " +
            $"diagnostici: {report.DiagnosticCount}.\n");
        sb.AppendLine("**Avvertenza:** un timeout o un blocco della politica scripted non prova " +
            "un softlock strutturale. I costi sono **parziali**: il log RC73 registra " +
            "soltanto Legname lavorato (planks) e Pietra (stone), in milliunità. " +
            "Le attese dipendono dalla frequenza di decisione (60 s). " +
            "Non confrontare run diagnostiche con benchmark a orizzonte pieno.\n");
        sb.AppendLine("| Scenario | Esito | Valutazione | Tempo osservato (s) | Attesa (s) | Passo finale |");
        sb.AppendLine("| --- | --- | --- | ---: | ---: | --- |");
        foreach (var row in report.Scenarios)
            sb.AppendLine($"| {Md(row.ScenarioId)} | {Md(row.Outcome)} | {Md(row.Classification)} | " +
                $"{row.ObservedSeconds} | {row.WaitingSeconds} | {Md(row.NextAction)} |");
        sb.AppendLine("\n## Interpretazione\n");
        foreach (var row in report.Scenarios)
            sb.AppendLine($"- **{Md(row.ScenarioId)}** — {Md(row.EvidenceNote)} " +
                $"Ultimo vincolo osservato: `{Md(row.PrimaryConstraint)}`. " +
                $"Spesa tracciata: planks {N(row.PlanksSpentMilliunits)}, " +
                $"stone {N(row.StoneSpentMilliunits)} milliunità. " +
                $"Motivo: {Md(row.StopReason)}.");
        sb.AppendLine("\n## Prossime misure richieste\n\n" +
            "Eseguire il gate F1, poi tutti gli scenari a orizzonte pieno. " +
            "Per ogni `timeout` ripetere con strategia alternativa e inventario del " +
            "prossimo acquisto prima di attribuire la causa a un collo di bottiglia " +
            "dell'economia. Tracciare anche i costi delle risorse non ancora presenti " +
            "negli eventi RC73 prima di calcolare il costo completo di una filiera.\n");
        return sb.ToString();
    }

    private static string N(int value) => value.ToString(CultureInfo.InvariantCulture);
    private static string N(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "";
    private static string N(long value) => value.ToString(CultureInfo.InvariantCulture);
    private static string N(long? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "";
    private static string N(decimal value) => value.ToString(CultureInfo.InvariantCulture);
    private static string Csv(string value) => value.IndexOfAny(['"', ',', '\r', '\n']) < 0 ?
        value : '"' + value.Replace("\"", "\"\"") + '"';
    private static string Md(string value) => value.Replace("|", "\\|").Replace("\r", " ")
        .Replace("\n", " ").Replace("`", "'");
}

public sealed record V15ProgressionScenarioAnalysis(
    string ScenarioId, string Goal, string PolicyId, string Outcome, bool Diagnostic,
    int DeclaredHorizonSeconds, int EffectiveHorizonSeconds, int ObservedSeconds,
    int? CompletionSeconds, string Classification, string PrimaryConstraint,
    long WaitingSeconds, IReadOnlyDictionary<string, long> WaitingSecondsByConstraint,
    int ExecutedActions, decimal PlanksSpentMilliunits, decimal StoneSpentMilliunits,
    long MinimumFreeWorkers, long FinalFreeWorkers, long FinalPlanksMilliunits,
    long FinalStoneMilliunits, string NextAction, string StopReason, string EvidenceNote);

public sealed record V15ProgressionReport(string AnalysisVersion, int ScenarioCount,
    int Completed, int TimedOut, int Blocked, int DiagnosticCount,
    IReadOnlyList<V15ProgressionScenarioAnalysis> Scenarios);
