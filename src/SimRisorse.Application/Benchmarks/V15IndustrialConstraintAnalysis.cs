using System.Globalization;
using System.Text;
using SimRisorse.Core.Economy;

namespace SimRisorse.Application.Benchmarks;

/// <summary>
/// RC76/E4a.2c: observable stock/capacity measurements for the critical industrial
/// inputs. This is a snapshot of CITY inventory and CITY capacity, not a model of
/// storage or throughput inside the food depot. Primitive fields permit value
/// comparisons across deterministic runs and SQLite restarts.
/// </summary>
public sealed record V15IndustrialStockProbe(
    long PlanksStoredMilliunits, long PlanksCapacityMilliunits,
    long StoneStoredMilliunits, long StoneCapacityMilliunits,
    long CoalStoredMilliunits, long CoalCapacityMilliunits,
    long IronOreStoredMilliunits, long IronOreCapacityMilliunits,
    long IronStoredMilliunits, long IronCapacityMilliunits,
    long GoldOreStoredMilliunits, long GoldOreCapacityMilliunits,
    long GoldStoredMilliunits, long GoldCapacityMilliunits)
{
    public static V15IndustrialStockProbe Capture(ResourceBundle stock, ResourceBundle capacity)
    {
        ArgumentNullException.ThrowIfNull(stock);
        ArgumentNullException.ThrowIfNull(capacity);
        return new(stock["planks"].Milliunits, capacity["planks"].Milliunits,
            stock["stone"].Milliunits, capacity["stone"].Milliunits,
            stock["coal"].Milliunits, capacity["coal"].Milliunits,
            stock["iron-ore"].Milliunits, capacity["iron-ore"].Milliunits,
            stock["iron"].Milliunits, capacity["iron"].Milliunits,
            stock["gold-ore"].Milliunits, capacity["gold-ore"].Milliunits,
            stock["gold"].Milliunits, capacity["gold"].Milliunits);
    }

    public IEnumerable<(string Id, long Stock, long Capacity)> Items()
    {
        yield return ("planks", PlanksStoredMilliunits, PlanksCapacityMilliunits);
        yield return ("stone", StoneStoredMilliunits, StoneCapacityMilliunits);
        yield return ("coal", CoalStoredMilliunits, CoalCapacityMilliunits);
        yield return ("iron-ore", IronOreStoredMilliunits, IronOreCapacityMilliunits);
        yield return ("iron", IronStoredMilliunits, IronCapacityMilliunits);
        yield return ("gold-ore", GoldOreStoredMilliunits, GoldOreCapacityMilliunits);
        yield return ("gold", GoldStoredMilliunits, GoldCapacityMilliunits);
    }
}

public sealed record V15IndustrialConstraintObservation(string ScenarioId, string PolicyId,
    string Evidence, string Constraint, string Action, string? ResourceId,
    int FirstObservedSecond, long? DeficitMilliunits, int Occurrences);

public sealed record V15IndustrialConstraintRow(string ScenarioId, string PolicyId,
    string Outcome, bool Diagnostic, long MinimumObservedFreeWorkers,
    int WaitEvents, int SaturatedStockSamples, bool HasStockTelemetry,
    IReadOnlyList<V15IndustrialConstraintObservation> Observations,
    string Conclusion);

public sealed record V15IndustrialConstraintReport(string Version, int RunCount,
    IReadOnlyList<V15IndustrialConstraintRow> Runs);

/// <summary>
/// Distinguishes measured shortages and observed stock saturation from hypotheses.
/// No unsatisfied script proves a structural deadlock; a full-height inventory
/// sample does not prove its capacity stopped production.
/// </summary>
public static class V15IndustrialConstraintAnalysis
{
    public const string Version = "e4a-v15-constraint-evidence-1";

    public static V15IndustrialConstraintReport Analyze(IReadOnlyList<V15IndustrialProgressionRun> runs)
    {
        // Reuse the full ledger and milestone validation (including duplicate pairs).
        _ = V15IndustrialProgressionAnalysis.Analyze(runs);
        var result = new List<V15IndustrialConstraintRow>();
        foreach (var run in runs)
        {
            var observations = new List<V15IndustrialConstraintObservation>();
            var waiting = run.Events.Where(e => e.Result is "waiting" or "blocked").ToArray();
            foreach (var group in waiting.GroupBy(e => (e.Action, e.Reason)))
            {
                var first = group.First();
                var kind = Classify(first.Reason);
                // Shortages are exact at the measured attempt, but NOT additive
                // over time. A repeated shortage of 10 does not mean 20 missing.
                var measured = group.Where(e => e.MissingResourceMilliunits is { Count: > 0 })
                    .ToArray();
                foreach (var resource in measured.SelectMany(e => e.MissingResourceMilliunits!.Keys)
                    .Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal))
                {
                    var firstGap = measured.First(e => e.MissingResourceMilliunits!.ContainsKey(resource));
                    observations.Add(new(run.ScenarioId, run.PolicyId, "measured-shortage", kind,
                        first.Action, resource, firstGap.ElapsedSeconds,
                        firstGap.MissingResourceMilliunits![resource],
                        measured.Count(e => e.MissingResourceMilliunits!.ContainsKey(resource))));
                }
                // Missing telemetry from RC75 runs must remain explicit.
                if (measured.Length == 0)
                    observations.Add(new(run.ScenarioId, run.PolicyId, "failure-code-only", kind,
                        first.Action, null, first.ElapsedSeconds, null, group.Count()));
            }

            var capacityObservations = 0;
            foreach (var sample in run.Samples)
            {
                if (sample.IndustrialStocks is null) continue;
                foreach (var (id, stock, capacity) in sample.IndustrialStocks.Items())
                {
                    if (stock < 0 || capacity < 0)
                        throw new ArgumentException("Negative industrial stock telemetry.", nameof(runs));
                    // Existing over-capacity stocks can be imported, so a value
                    // >= capacity is recorded as pressure, never auto-diagnosed.
                    if (capacity == 0 || stock < capacity) continue;
                    capacityObservations++;
                    observations.Add(new(run.ScenarioId, run.PolicyId, "capacity-sample",
                        "possible-storage-pressure", sample.NextAction, id,
                        sample.ElapsedSeconds, null, 1));
                }
            }

            result.Add(new(run.ScenarioId, run.PolicyId, run.Outcome, run.Diagnostic,
                run.Samples.Min(s => s.FreeWorkers), waiting.Count(e => e.Result == "waiting"),
                capacityObservations, run.Samples.Any(s => s.IndustrialStocks is not null),
                observations, run.Diagnostic
                    ? "Diagnostica abbreviata: nessuna conclusione sulla progressione."
                    : run.Outcome == "pass"
                        ? "Obiettivo raggiunto dalla strategia osservata; non dimostra ottimalita'."
                        : "Obiettivo non osservato: verificare strategia, tempi e prerequisiti prima di dichiarare uno stallo."));
        }
        return new(Version, result.Count, result);
    }

    private static string Classify(string failure) => failure switch
    {
        "InsufficientResources" or "InsufficientMaterials" => "construction-materials",
        "InsufficientFood" => "depot-food",
        "InsufficientPencils" => "school-pencils",
        "InsufficientPopulation" => "workforce-or-population",
        "InsufficientResearchPoints" => "research-points",
        "InsufficientQualifiedResearchers" => "researcher-training",
        "MissingPrerequisite" or "MissingSourceBuilding" => "construction-prerequisite",
        "MissingResearch" => "technology-prerequisite",
        _ => "other-or-script-policy"
    };

    public static string ToCsv(V15IndustrialConstraintReport report)
    {
        var csv = new StringBuilder("scenario_id,policy_id,evidence,constraint,action,resource_id,first_second,deficit_milli,occurrences\n");
        foreach (var row in report.Runs)
            foreach (var item in row.Observations)
                csv.AppendLine(string.Join(",", Escape(item.ScenarioId), Escape(item.PolicyId),
                    Escape(item.Evidence), Escape(item.Constraint), Escape(item.Action),
                    Escape(item.ResourceId ?? ""), N(item.FirstObservedSecond), N(item.DeficitMilliunits),
                    N(item.Occurrences)));
        return csv.ToString();
    }

    public static string ToMarkdown(V15IndustrialConstraintReport report)
    {
        var markdown = new StringBuilder("# E4a.2c — Vincoli della progressione industriale v15\n\n");
        markdown.AppendLine("**Evidenza, non diagnosi automatica.** Una saturazione osservata non prova un blocco produttivo; un timeout della strategia non prova uno stallo strutturale.\n");
        foreach (var row in report.Runs)
        {
            markdown.AppendLine($"## {row.ScenarioId} / {row.PolicyId}\n");
            markdown.AppendLine($"Esito: `{row.Outcome}` · diagnostica: `{row.Diagnostic}` · " +
                $"lavoratori liberi minimi osservati: {row.MinimumObservedFreeWorkers} · " +
                $"campioni di saturazione: {row.SaturatedStockSamples}.\n");
            markdown.AppendLine(row.Conclusion + "\n");
            if (!row.HasStockTelemetry)
                markdown.AppendLine("Nessuna telemetria di capienza disponibile in questo report (formato RC75): non inferire assenza di saturazione.\n");
            foreach (var observation in row.Observations)
                markdown.AppendLine($"- `{observation.Evidence}` / `{observation.Constraint}`: " +
                    $"{observation.Action}, {observation.ResourceId ?? "risorsa non identificata"}, " +
                    $"t={observation.FirstObservedSecond}s" +
                    (observation.DeficitMilliunits is long gap ? $", mancano {gap} millesimi" : "") + ".");
            if (row.Observations.Count == 0) markdown.AppendLine("Nessuna criticita' direttamente osservata nei dati disponibili.");
            markdown.AppendLine();
        }
        markdown.AppendLine("## Diagnosi successive consigliate\n");
        markdown.AppendLine("Verificare la specifica ricetta a monte per ogni risorsa mancante; " +
            "aumentare stoccaggio soltanto dopo aver confermato la saturazione come causa di produzione persa; " +
            "confrontare strategie alternative a orizzonte pieno prima di variare costi o requisiti.\n");
        return markdown.ToString();
    }

    private static string N(long? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "";
    private static string Escape(string text) => text.IndexOfAny(['"', ',', '\r', '\n']) < 0
        ? text : '"' + text.Replace("\"", "\"\"") + '"';
}
