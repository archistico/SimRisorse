using System.Globalization;
using System.Text;
using SimRisorse.Core.Food;

namespace SimRisorse.Application.Benchmarks;

/// <summary>RC84: compare OBSERVED natural MintTen/Legacy event ledgers with
/// the distinct, UNAPPLIED price/warehouse proposals. Never turn a proposed
/// warehouse route or a single scripted timeout into a gameplay softlock.</summary>
public sealed record E4bMintObservedShortage(string Action, string ResourceId,
    int FirstObservedSecond, long MissingMilliunitsAtFirstObservation, int Observations);

public sealed record E4bMintNaturalEvidenceRow(
    string ScenarioId, string PolicyId, string CrewContract, string Outcome,
    bool Diagnostic, string Classification, int ObservedSeconds, int? CompletionSeconds,
    int? MintBuiltSecond, int? FullCrewAssignedSecond, int? CoinsMilestoneSecond,
    long MinimumFreeWorkers, long FinalFreeWorkers, bool StaffingWaitObserved,
    int CapacityPressureSamples, bool HasStockTelemetry,
    IReadOnlyList<string> SaturatedResourceIds,
    IReadOnlyList<string> ObservedWaitConstraints,
    IReadOnlyList<E4bMintObservedShortage> MeasuredMaterialShortages,
    string PendingAction);

public sealed record E4bMintUnappliedHypothesis(
    string CurveId, string Tier, int WorkersForOneMint, int WorkersForTwoMints,
    int ExtraRequiredResidents, string SecondMintWarehouseRoute,
    string AtomicTwoMintWarehouseRoute, long MinimumInstallmentsForSecondAtInitialCapacity,
    string SecondMintWorkTargetBand);

public sealed record E4bMintNaturalFeasibilityReport(
    string Version, string EvidenceScope, string ProposalScope,
    string InputSha256, int ObservedRuns, int DiagnosticRuns, int FullHorizonRuns,
    int VerifiedMintTenMilestones, bool CompleteNaturalPolicyPairObserved,
    IReadOnlyList<E4bMintNaturalEvidenceRow> Observations,
    IReadOnlyList<E4bMintUnappliedHypothesis> EconomicHypotheses);

public static class E4bMintNaturalFeasibilityAudit
{
    public const string Version = "e4b-mint-natural-feasibility-1";
    public const string EvidenceScope = "actual-v15-natural-legacy-prices/no-grants";
    public const string ProposalScope = "analytical-A-B-C/not-applied/not-measured";

    public static E4bMintNaturalFeasibilityReport Analyze(
        IReadOnlyList<V15IndustrialProgressionRun> runs)
    {
        ArgumentNullException.ThrowIfNull(runs);
        // Existing ledgers enforce ordering, non-negative receipts, true milestones,
        // consistent diagnostic horizons and unique scenario/policy identifiers.
        _ = V15IndustrialProgressionAnalysis.Analyze(runs);
        var constraints = V15IndustrialConstraintAnalysis.Analyze(runs);
        var first = runs[0];
        var observed = new List<E4bMintNaturalEvidenceRow>();
        var mintContracts = 0;
        foreach (var run in runs)
        {
            var contract = run.RulesVersion switch
            {
                V15CrewRules.MintTenRulesVersion when run.ScenarioId == "E4B-M01" => "MintTen",
                V15FoodCampaignCheckpoints.RulesVersion when run.ScenarioId == "V15-I03" => "Legacy",
                _ => throw new ArgumentException("Only versioned MintTen and its Legacy control are accepted.", nameof(runs))
            };
            if (contract == "MintTen") mintContracts++;
            if (run.Goal != "first-coins-after-research" ||
                run.DeclaredHorizonSeconds != first.DeclaredHorizonSeconds ||
                run.BaselineRulesVersion != first.BaselineRulesVersion ||
                run.ScenarioBaselineId != first.ScenarioBaselineId ||
                run.InitialPhysicalStockMilliunits.Count != first.InitialPhysicalStockMilliunits.Count ||
                run.InitialPhysicalStockMilliunits.Any(p =>
                    !first.InitialPhysicalStockMilliunits.TryGetValue(p.Key, out var value) || value != p.Value))
                throw new ArgumentException("Natural runs do not share the same starting rules, horizon and inventory.", nameof(runs));

            var crew = run.Events.FirstOrDefault(e =>
                e.Action == (contract == "MintTen" ? "staff-mint-team" : "staff-mint") &&
                e.Result == "executed");
            var built = run.Events.FirstOrDefault(e => e.Action == "build-mint" && e.Result == "executed");
            var milestone = run.Events.FirstOrDefault(e => e.Action == run.Goal && e.Result == "milestone");
            if ((contract == "MintTen" && run.Events.Any(e => e.Action == "staff-mint")) ||
                (contract == "Legacy" && run.Events.Any(e => e.Action == "staff-mint-team")) ||
                (crew is not null && built is null) ||
                (crew is not null && built is not null &&
                    crew.ElapsedSeconds < built.ElapsedSeconds) ||
                (milestone is not null && (crew is null || built is null ||
                    milestone.ElapsedSeconds < crew.ElapsedSeconds)) ||
                (run.Outcome == "pass" && (built is null || crew is null || milestone is null)))
                throw new ArgumentException("Mint production evidence violates the declared staffing contract.", nameof(runs));

            var constraint = constraints.Runs.Single(r => r.ScenarioId == run.ScenarioId && r.PolicyId == run.PolicyId);
            var missing = constraint.Observations.Where(o => o.Evidence == "measured-shortage" &&
                    o.ResourceId is not null && o.DeficitMilliunits is > 0)
                .OrderBy(o => o.FirstObservedSecond).ThenBy(o => o.ResourceId, StringComparer.Ordinal)
                .Select(o => new E4bMintObservedShortage(o.Action, o.ResourceId!,
                    o.FirstObservedSecond, o.DeficitMilliunits!.Value, o.Occurrences)).ToArray();
            var observedSaturation = constraint.Observations
                .Where(o => o.Evidence == "capacity-sample" && o.ResourceId is not null)
                .Select(o => o.ResourceId!).Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal).ToArray();
            var waitKinds = constraint.Observations
                .Where(o => o.Evidence is "measured-shortage" or "failure-code-only")
                .Select(o => o.Constraint).Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal).ToArray();
            var classification = run.Diagnostic ? "diagnostic-only" : run.Outcome switch
            {
                "pass" => "observed-natural-mint-production",
                "blocked" => "script-policy-blocked/not-global-softlock",
                _ => "script-policy-timeout/not-global-softlock"
            };
            observed.Add(new(run.ScenarioId, run.PolicyId, contract, run.Outcome,
                run.Diagnostic, classification, run.Samples[^1].ElapsedSeconds,
                run.CompletionSeconds, built?.ElapsedSeconds, crew?.ElapsedSeconds,
                milestone?.ElapsedSeconds, constraint.MinimumObservedFreeWorkers,
                run.Samples[^1].FreeWorkers,
                run.Events.Any(e => e.Action == (contract == "MintTen" ? "staff-mint-team" : "staff-mint") &&
                    e.Result == "waiting"),
                constraint.SaturatedStockSamples, constraint.HasStockTelemetry,
                observedSaturation, waitKinds, missing, run.Samples[^1].NextAction));
        }
        if (mintContracts == 0)
            throw new ArgumentException("At least one MintTen observation is required.", nameof(runs));
        var scale = new E4bMintScaleRiskStudy().Run();
        var hypotheses = scale.Rows.Select(row => new E4bMintUnappliedHypothesis(
            row.CurveId, row.Tier.ToString(), row.OneMint.Workforce.MintWorkers,
            row.TwoMints.Workforce.MintWorkers, row.AdditionalResidents,
            row.SecondMintWarehouseRoute.Status, row.AtomicTwoMintsWarehouseRoute.Status,
            row.SecondMintEscrowInstallmentsAtStartingCapacity, row.SecondMintTargetBand)).ToArray();
        // The historical RC82 pair is *specifically* research-first AND
        // bread-first. A third experimental policy must not fabricate that pair.
        var completedStandardPolicies = observed.Where(r => r.CrewContract == "MintTen" &&
                !r.Diagnostic && r.Outcome == "pass" && r.CoinsMilestoneSecond.HasValue)
            .Select(r => r.PolicyId).ToHashSet(StringComparer.Ordinal);
        var paired = completedStandardPolicies.Contains(V15IndustrialProgressionBenchmark.PolicyResearchFirst) &&
            completedStandardPolicies.Contains(V15IndustrialProgressionBenchmark.PolicyBreadFirst);
        return new(Version, EvidenceScope, ProposalScope, "in-memory/not-file-provenance",
            observed.Count, observed.Count(r => r.Diagnostic),
            observed.Count(r => !r.Diagnostic),
            observed.Count(r => r.CrewContract == "MintTen" && !r.Diagnostic &&
                r.Outcome == "pass" && r.CoinsMilestoneSecond.HasValue),
            paired, observed, hypotheses);
    }

    public static string ToCsv(E4bMintNaturalFeasibilityReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("scenario_id,policy_id,crew_contract,outcome,diagnostic,classification,observed_seconds,completion_seconds,mint_built_second,crew_assigned_second,first_coins_second,min_free_workers,final_free_workers,staffing_wait,capacity_pressure_samples,has_stock_telemetry,observed_wait_constraints,pending_action\n");
        foreach (var row in report.Observations)
            sb.AppendLine(string.Join(",", Escape(row.ScenarioId), Escape(row.PolicyId),
                Escape(row.CrewContract), Escape(row.Outcome),
                row.Diagnostic ? "true" : "false", Escape(row.Classification),
                Num(row.ObservedSeconds), Num(row.CompletionSeconds), Num(row.MintBuiltSecond),
                Num(row.FullCrewAssignedSecond), Num(row.CoinsMilestoneSecond),
                Num(row.MinimumFreeWorkers), Num(row.FinalFreeWorkers),
                row.StaffingWaitObserved ? "true" : "false", Num(row.CapacityPressureSamples),
                row.HasStockTelemetry ? "true" : "false",
                Escape(string.Join(";", row.ObservedWaitConstraints)), Escape(row.PendingAction)));
        return sb.ToString();
    }

    public static string ToMarkdown(E4bMintNaturalFeasibilityReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("# RC84 — Fattibilità Zecca: fatti osservati e ipotesi separate\n\n");
        sb.AppendLine($"Origine: `{report.EvidenceScope}`. SHA-256 input: `{report.InputSha256}`. " +
            $"Run: {report.ObservedRuns}; diagnostici: {report.DiagnosticRuns}; " +
            $"a orizzonte pieno: {report.FullHorizonRuns}; milestone MintTen complete: {report.VerifiedMintTenMilestones}.\n");
        sb.AppendLine("**Un timeout della strategia non è un softlock globale.** Le attese registrate non garantiscono una durata esatta. Magazzino saturo è una possibile pressione, NON la prova che abbia impedito la produzione. Le proposte A/B/C non erano attive durante questi run.\n");
        sb.AppendLine("| Contratto | Strategia | Esito | Classificazione | Secondi | Costruzione | Squadra assegnata | Monete | Attese riscontrate |");
        sb.AppendLine("| --- | --- | --- | --- | ---: | ---: | ---: | ---: | --- |");
        foreach (var row in report.Observations)
            sb.AppendLine($"| {Md(row.CrewContract)} | {Md(row.PolicyId)} | {Md(row.Outcome)} | " +
                $"{Md(row.Classification)} | {row.ObservedSeconds} | {Num(row.MintBuiltSecond)} | " +
                $"{Num(row.FullCrewAssignedSecond)} | {Num(row.CoinsMilestoneSecond)} | " +
                $"{Md(string.Join(", ", row.ObservedWaitConstraints))} |");
        foreach (var row in report.Observations)
        {
            sb.AppendLine($"\n### {Md(row.CrewContract)} / {Md(row.PolicyId)}\n");
            sb.AppendLine($"Lavoratori liberi minimo/finale: {row.MinimumFreeWorkers}/{row.FinalFreeWorkers}. " +
                $"Campioni di saturazione: {row.CapacityPressureSamples}; telemetria stock: " +
                $"{(row.HasStockTelemetry ? "presente" : "assente")} " +
                $"({string.Join(", ", row.SaturatedResourceIds)}). " +
                $"Prossima azione: `{Md(row.PendingAction)}`.\n");
            foreach (var shortage in row.MeasuredMaterialShortages)
                sb.AppendLine($"- Deficit misurato all'azione `{Md(shortage.Action)}`: " +
                    $"{Md(shortage.ResourceId)} = {shortage.MissingMilliunitsAtFirstObservation} millesimi, " +
                    $"t={shortage.FirstObservedSecond}s (non cumulabile nel tempo).");
        }
        sb.AppendLine("\n## Ipotesi RC83, NON verificate nella partita\n");
        sb.AppendLine($"Ambito: `{report.ProposalScope}`. Requisiti e magazzini sono proiezioni, non esiti di esecuzione.\n");
        sb.AppendLine("| Curva | Tier | Addetti 1→2 Zecche | Residenti extra | Percorso Magazzini seconda | Lotto atomico x2 | Fascia costo seconda |");
        sb.AppendLine("| --- | --- | ---: | ---: | --- | --- | --- |");
        foreach (var hypothesis in report.EconomicHypotheses)
            sb.AppendLine($"| {Md(hypothesis.CurveId)} | {Md(hypothesis.Tier)} | " +
                $"{hypothesis.WorkersForOneMint}→{hypothesis.WorkersForTwoMints} | " +
                $"+{hypothesis.ExtraRequiredResidents} | {Md(hypothesis.SecondMintWarehouseRoute)} | " +
                $"{Md(hypothesis.AtomicTwoMintWarehouseRoute)} | {Md(hypothesis.SecondMintWorkTargetBand)} |");
        sb.AppendLine("\n**Decisione ancora aperta:** curva economica, capacità di stoccaggio e finanziamento. " +
            "Per attribuire uno stallo di progettazione occorrono run integrali, confronto fra strategie e verifica dei prerequisiti, non soltanto questo report.");
        return sb.ToString();
    }

    private static string Num(long? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "";
    private static string Num(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "";
    private static string Num(long value) => value.ToString(CultureInfo.InvariantCulture);
    private static string Num(int value) => value.ToString(CultureInfo.InvariantCulture);
    private static string Escape(string value) => value.IndexOfAny(['"', ',', '\r', '\n']) < 0
        ? value : '"' + value.Replace("\"", "\"\"") + '"';
    private static string Md(string value) => value.Replace("|", "\\|").Replace("`", "'").Replace("\n", " ");
}
