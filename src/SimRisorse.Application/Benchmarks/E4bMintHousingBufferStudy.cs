using System.Globalization;
using System.Text;
using SimRisorse.Core.Food;

namespace SimRisorse.Application.Benchmarks;

/// <summary>
/// RC85: an explicitly costed, unfunded alternative to bread-first. The policy
/// purchases two extra small houses before the gold chain, allowing normal
/// population growth to determine whether the ten-person mint crew is viable.
/// Observed outcomes are never interpreted as proof of a global softlock.
/// </summary>
public sealed record E4bMintHousingBufferRow(
    string CrewContract, string BaselinePolicyId, string AlternativePolicyId,
    string BaselineOutcome, string HousingOutcome, bool Diagnostic,
    string ComparisonStatus, int? BaselineCompletionSeconds,
    int? HousingCompletionSeconds, int? HousingMinusBaselineSeconds,
    int BaselineSmallHousesPurchased, int HousingSmallHousesPurchased,
    int AdditionalSmallHousesPurchased, long BaselinePlanksSpentMilliunits,
    long HousingPlanksSpentMilliunits, long BaselineStoneSpentMilliunits,
    long HousingStoneSpentMilliunits, bool HousingStaffingWaitObserved);

public sealed record E4bMintHousingBufferReport(
    string Version, string EvidenceScope, string InputSha256, int NaturalRunCount,
    int PairedContracts, int ComparableCompletedContracts,
    IReadOnlyList<E4bMintHousingBufferRow> Comparisons);

public static class E4bMintHousingBufferStudy
{
    public const string Version = "e4b-mint-housing-buffer-1";
    public const string EvidenceScope = "actual-v15-natural-legacy-prices/no-grants/no-economy-changes";

    public static E4bMintHousingBufferReport Analyze(
        IReadOnlyList<V15IndustrialProgressionRun> runs)
    {
        ArgumentNullException.ThrowIfNull(runs);
        // Preserve RC84 cross-run consistency checks (starting stock, saved
        // workforce rules, milestone, provenance, ordering and uniqueness).
        var audited = E4bMintNaturalFeasibilityAudit.Analyze(runs);
        var pairs = new List<E4bMintHousingBufferRow>();
        foreach (var contract in new[] { "MintTen", "Legacy" })
        {
            var group = audited.Observations.Where(row => row.CrewContract == contract).ToArray();
            var buffered = group.SingleOrDefault(row =>
                row.PolicyId == V15IndustrialProgressionBenchmark.PolicyHousingBuffer);
            if (buffered is null) continue;
            var baseline = group.SingleOrDefault(row =>
                row.PolicyId == V15IndustrialProgressionBenchmark.PolicyBreadFirst);
            if (baseline is null)
                throw new ArgumentException("A housing-buffer run requires a matching bread-first control.", nameof(runs));

            var sourceBuffer = runs.Single(run => run.ScenarioId == buffered.ScenarioId &&
                run.PolicyId == buffered.PolicyId);
            var sourceBaseline = runs.Single(run => run.ScenarioId == baseline.ScenarioId &&
                run.PolicyId == baseline.PolicyId);
            var boughtBuffer = BuiltSmallHouses(sourceBuffer);
            var boughtBaseline = BuiltSmallHouses(sourceBaseline);
            // A full successful housing-buffer path must actually have paid for
            // all three small houses (one baseline + two extra); event logs cannot
            // masquerade as an observed alternative after silently skipping them.
            if (sourceBuffer.Outcome == "pass" &&
                boughtBuffer != 1 + V15IndustrialProgressionBenchmark.HousingBufferExtraSmallHouses)
                throw new ArgumentException("Successful housing-buffer report is missing its house purchases.", nameof(runs));

            var comparable = !baseline.Diagnostic && !buffered.Diagnostic &&
                baseline.Outcome == "pass" && buffered.Outcome == "pass" &&
                baseline.CompletionSeconds.HasValue && buffered.CompletionSeconds.HasValue;
            int? delta = comparable
                ? checked(buffered.CompletionSeconds!.Value - baseline.CompletionSeconds!.Value)
                : null;
            var status = baseline.Diagnostic || buffered.Diagnostic
                ? "diagnostic-only/no-time-comparison"
                : comparable
                    ? "observed-both-complete/scripted-only"
                    : "inconclusive/not-global-softlock";
            pairs.Add(new(contract,
                V15IndustrialProgressionBenchmark.PolicyBreadFirst,
                V15IndustrialProgressionBenchmark.PolicyHousingBuffer,
                baseline.Outcome, buffered.Outcome, baseline.Diagnostic || buffered.Diagnostic,
                status, baseline.CompletionSeconds, buffered.CompletionSeconds, delta,
                boughtBaseline, boughtBuffer, boughtBuffer - boughtBaseline,
                Spent(sourceBaseline, "planks"), Spent(sourceBuffer, "planks"),
                Spent(sourceBaseline, "stone"), Spent(sourceBuffer, "stone"),
                buffered.StaffingWaitObserved));
        }
        if (!pairs.Any(row => row.CrewContract == "MintTen"))
            throw new ArgumentException("The ten-worker mint must have both housing and baseline observations.", nameof(runs));
        return new(Version, EvidenceScope, "in-memory/not-file-provenance",
            runs.Count, pairs.Count, pairs.Count(row => row.HousingMinusBaselineSeconds.HasValue), pairs);
    }

    private static int BuiltSmallHouses(V15IndustrialProgressionRun run) =>
        run.Events.Count(e => e.Action == "build-small-house" && e.Result == "executed");

    private static long Spent(V15IndustrialProgressionRun run, string resource) =>
        run.Events.Where(e => e.Result == "executed")
            .Aggregate(0L, (total, e) => checked(total + e.ResourceCostsMilliunits.GetValueOrDefault(resource)));

    public static string ToCsv(E4bMintHousingBufferReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var csv = new StringBuilder("crew_contract,baseline_policy,housing_policy,baseline_outcome,housing_outcome,diagnostic,status,baseline_completion_seconds,housing_completion_seconds,housing_minus_baseline_seconds,baseline_houses_bought,housing_houses_bought,additional_houses_bought,baseline_planks_spent_milli,housing_planks_spent_milli,baseline_stone_spent_milli,housing_stone_spent_milli,housing_staffing_wait\n");
        foreach (var row in report.Comparisons)
            csv.AppendLine(string.Join(",", Esc(row.CrewContract), Esc(row.BaselinePolicyId),
                Esc(row.AlternativePolicyId), Esc(row.BaselineOutcome), Esc(row.HousingOutcome),
                row.Diagnostic ? "true" : "false", Esc(row.ComparisonStatus),
                Num(row.BaselineCompletionSeconds), Num(row.HousingCompletionSeconds),
                Num(row.HousingMinusBaselineSeconds), Num(row.BaselineSmallHousesPurchased),
                Num(row.HousingSmallHousesPurchased), Num(row.AdditionalSmallHousesPurchased),
                Num(row.BaselinePlanksSpentMilliunits), Num(row.HousingPlanksSpentMilliunits),
                Num(row.BaselineStoneSpentMilliunits), Num(row.HousingStoneSpentMilliunits),
                row.HousingStaffingWaitObserved ? "true" : "false"));
        return csv.ToString();
    }

    public static string ToMarkdown(E4bMintHousingBufferReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var md = new StringBuilder("# RC85 — Strategia abitativa per la Zecca MintTen\n\n");
        md.AppendLine($"Ambito: `{report.EvidenceScope}`. SHA-256 input: `{report.InputSha256}`. " +
            $"Run: {report.NaturalRunCount}; coppie: {report.PairedContracts}; " +
            $"confronti completi: {report.ComparableCompletedContracts}.\n");
        md.AppendLine("**Non sono stati applicati nuovi prezzi né risorse gratuite.** " +
            "La politica housing-buffer tenta due piccole case aggiuntive, pagate realmente, " +
            "prima della filiera Oro. Un esito di timeout NON dimostra un softlock globale.\n");
        md.AppendLine("| Regime | Esito bread-first | Esito housing-buffer | " +
            "Case acquistate (base→buffer) | Differenza di tempo (s) | Classificazione |");
        md.AppendLine("| --- | --- | --- | ---: | ---: | --- |");
        foreach (var row in report.Comparisons)
            md.AppendLine($"| {row.CrewContract} | {row.BaselineOutcome} | {row.HousingOutcome} | " +
                $"{row.BaselineSmallHousesPurchased}→{row.HousingSmallHousesPurchased} | " +
                $"{Num(row.HousingMinusBaselineSeconds)} | {row.ComparisonStatus} |");
        md.AppendLine("\nI pagamenti in legname e pietra delle strategie comprendono **tutti** gli " +
            "acquisti realmente eseguiti, non solo le nuove case:\n");
        foreach (var row in report.Comparisons)
            md.AppendLine($"- {row.CrewContract}: legname {row.BaselinePlanksSpentMilliunits}→" +
                $"{row.HousingPlanksSpentMilliunits} millesimi, pietra {row.BaselineStoneSpentMilliunits}→" +
                $"{row.HousingStoneSpentMilliunits} millesimi; attesa di squadra nella variante: " +
                $"{(row.HousingStaffingWaitObserved ? "osservata" : "non osservata")}. " +
                "Solo le milestone reali consentono una differenza di tempi.");
        md.AppendLine("\nQuesta prova confronta strategie scripted a prezzi legacy. " +
            "Non misura la giocabilità delle curve economiche A/B/C non applicate.");
        return md.ToString();
    }

    private static string Num(int? n) => n?.ToString(CultureInfo.InvariantCulture) ?? "";
    private static string Num(long n) => n.ToString(CultureInfo.InvariantCulture);
    private static string Num(int n) => n.ToString(CultureInfo.InvariantCulture);
    private static string Esc(string s) => s.IndexOfAny(['\"', ',', '\r', '\n']) < 0
        ? s : '"' + s.Replace("\"", "\"\"") + '"';
}
