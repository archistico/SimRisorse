using System.Globalization;
using System.Text;
using SimRisorse.Core.Food;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Benchmarks;

/// <summary>
/// RC88: checks the order and actual command/clock receipts of the industrial
/// progression. A diagnostic or timeout is not evidence of a softlock.
/// Analyses recorded runs; does not change the campaign or advance time.
/// </summary>
public static class V15IndustrialChainAudit
{
    public const string Version = "rc88-industrial-chain-evidence-1";
    public const string EvidenceScope = "natural-v15/command-and-clock-receipts/no-grants";

    public static V15IndustrialChainReport Analyze(IEnumerable<V15IndustrialProgressionRun> runs)
    {
        ArgumentNullException.ThrowIfNull(runs);
        var source = runs.ToArray();
        if (source.Length == 0 || source.Any(r => r is null))
            throw new ArgumentException("At least one valid industrial run is required.", nameof(runs));
        var fixture = V15BenchmarkFixture.Create();
        var extraction = fixture.Technologies.Technologies.Single(t => t.Id == "estrazione-1");
        var industry = fixture.Technologies.Technologies.Single(t => t.Id == "industria-1");
        if (!extraction.Available || extraction.Prerequisites.Length != 0 ||
            !industry.Available || !industry.Prerequisites.SequenceEqual(["estrazione-1"]))
            throw new InvalidOperationException("The first industrial technology dependency contract changed.");
        // ResourceBundle is a sparse vector: zero-quantity output resources
        // (notably swords and coins in a fresh city) have no dictionary entry.
        // Keep the exact sparse provenance check, but read quantities through
        // the bundle indexer, whose documented contract returns zero if absent.
        var baselineStock = V15StockAuditor.PhysicalTotals(fixture.Start);
        var initial = baselineStock.Amounts;
        var rows = new List<V15IndustrialChainObservation>();
        var identities = new HashSet<string>(StringComparer.Ordinal);
        foreach (var run in source)
        {
            var mintTen = run.RulesVersion == V15CrewRules.SavedRulesVersion(V15CrewMode.MintTen);
            var mode = mintTen ? V15CrewMode.MintTen : V15CrewMode.Legacy;
            var scenario = run.ScenarioId == "E4B-M01" && mintTen ? "V15-I03" : run.ScenarioId;
            var goal = scenario switch
            {
                "V15-I02" => "first-swords-after-research",
                "V15-I03" => "first-coins-after-research",
                _ => throw new ArgumentException("Only real sword/mint industrial runs can be audited.", nameof(runs))
            };
            if (mintTen && scenario != "V15-I03" ||
                run.RulesVersion != V15CrewRules.SavedRulesVersion(mode) ||
                run.ScenarioId != (mintTen ? "E4B-M01" : scenario) ||
                run.Goal != goal ||
                run.PolicyId is not (V15IndustrialProgressionBenchmark.PolicyResearchFirst or
                    V15IndustrialProgressionBenchmark.PolicyBreadFirst or
                    V15IndustrialProgressionBenchmark.PolicyHousingBuffer) ||
                run.PolicyId == V15IndustrialProgressionBenchmark.PolicyHousingBuffer && scenario != "V15-I03" ||
                run.Fixture != "fresh-game/no-grants" ||
                run.BaselineRulesVersion != fixture.Baseline.Catalog.RulesVersion ||
                run.ScenarioBaselineId != fixture.Start.CityCampaign.ScenarioId ||
                run.InitialPhysicalStockMilliunits is null ||
                run.InitialPhysicalStockMilliunits.Count != initial.Count ||
                initial.Any(p => !run.InitialPhysicalStockMilliunits.TryGetValue(p.Key, out var v) ||
                    v != p.Value.Milliunits) ||
                run.DeclaredHorizonSeconds != (scenario == "V15-I02" ? 43200 : 57600) ||
                run.EffectiveHorizonSeconds < 0 ||
                run.EffectiveHorizonSeconds > run.DeclaredHorizonSeconds ||
                run.Diagnostic != (run.EffectiveHorizonSeconds < run.DeclaredHorizonSeconds) ||
                run.Outcome is not ("pass" or "timeout" or "blocked") ||
                run.Events is null || run.Samples is null || run.Samples.Count == 0 ||
                run.Events.Any(e => e is null) || run.Samples.Any(s => s is null) ||
                run.Samples[0].ElapsedSeconds != 0 ||
                run.Samples[0].SwordsMilliunits != baselineStock["swords"].Milliunits ||
                run.Samples[0].CoinsMilliunits != baselineStock["coins"].Milliunits ||
                run.Samples[0].ExtractionOneUnlocked ||
                run.Samples[^1].ElapsedSeconds > run.EffectiveHorizonSeconds ||
                !Nondecreasing(run.Events.Select(e => e.ElapsedSeconds)) ||
                !Nondecreasing(run.Samples.Select(s => s.ElapsedSeconds)) ||
                run.Events.Any(e => e.ElapsedSeconds > run.EffectiveHorizonSeconds ||
                    e.Result is not ("executed" or "waiting" or "blocked" or "milestone") ||
                    e.ResourceCostsMilliunits is null || e.ResearchSpentMilliPoints < 0 ||
                    e.ProducedMilliunits < 0 || e.ResourceCostsMilliunits.Any(c => c.Value < 0) ||
                    e.MissingResourceMilliunits is not null &&
                        e.MissingResourceMilliunits.Any(c => c.Value < 0)) ||
                run.Samples.Any(s => s.SwordsMilliunits < 0 || s.CoinsMilliunits < 0 ||
                    s.ResearchBalanceMilliPoints < 0 || s.FreeWorkers < 0))
                throw new ArgumentException("Invalid industrial run provenance, clock or receipt values.", nameof(runs));

            var unique = string.Join("/", run.ScenarioId, run.RulesVersion, run.PolicyId);
            if (!identities.Add(unique))
                throw new ArgumentException("Duplicate industrial run for a policy and workforce regime.", nameof(runs));
            var steps = V15IndustrialProgressionBenchmark.Steps(scenario, run.PolicyId, mode);
            var cursor = 0;
            var blocked = false;
            var milestones = 0;
            var observedClockOutput = 0L;
            foreach (var e in run.Events)
            {
                if (e.Result == "milestone")
                {
                    milestones++;
                    if (cursor != steps.Count || e.Action != goal ||
                        run.Outcome != "pass" || e.ResearchSpentMilliPoints != 0 ||
                        e.ResourceCostsMilliunits.Count != 0 || e.ProducedMilliunits <= 0 ||
                        run.Events[^1] != e)
                        throw new ArgumentException("An industrial milestone has no valid final clock receipt.", nameof(runs));
                    observedClockOutput = e.ProducedMilliunits;
                    continue;
                }
                if (blocked || milestones != 0 || cursor >= steps.Count ||
                    e.Action != steps[cursor] || e.ProducedMilliunits != 0)
                    throw new ArgumentException("The observed action order does not match the declared policy.", nameof(runs));
                if (e.Result == "blocked")
                    blocked = true;
                if (e.Result != "executed")
                {
                    if (e.ResearchSpentMilliPoints != 0 || e.ResourceCostsMilliunits.Count != 0 ||
                        string.IsNullOrWhiteSpace(e.Reason))
                        throw new ArgumentException("A rejected command cannot record a payment.", nameof(runs));
                    continue;
                }
                var expectedPoints = e.Action switch
                {
                    "buy-tech-estrazione-1" => extraction.Cost.MilliPoints,
                    "buy-tech-industria-1" => industry.Cost.MilliPoints,
                    _ => 0L
                };
                if (e.ResearchSpentMilliPoints != expectedPoints ||
                    expectedPoints != 0 && e.ResourceCostsMilliunits.Count != 0 ||
                    e.Action.StartsWith("build-", StringComparison.Ordinal) &&
                        !e.ResourceCostsMilliunits.Any(p => p.Value > 0) ||
                    e.Action.StartsWith("staff-", StringComparison.Ordinal) &&
                        e.ResourceCostsMilliunits.Count != 0 ||
                    e.Action == "assign-researcher" && e.ResourceCostsMilliunits.Count != 0)
                    throw new ArgumentException("Industrial command costs or research points are inconsistent.", nameof(runs));
                if (e.Action == "build-school" && !MatchesCost(e,
                        V14SchoolConstructionRules.Candidate.Cost.Amounts.ToDictionary(p => p.Key,
                            p => p.Value.Milliunits, StringComparer.Ordinal)))
                    throw new ArgumentException("The school construction receipt differs from the catalog.", nameof(runs));
                if (e.Action == "enroll-student" && !MatchesCost(e,
                        fixture.School.TrainingCost(1).Amounts.ToDictionary(p => p.Key,
                            p => p.Value.Milliunits, StringComparer.Ordinal)))
                    throw new ArgumentException("The student was not paid for with the actual materials.", nameof(runs));
                cursor++;
            }
            var output = scenario == "V15-I02" ? "swords" : "coins";
            var actualStock = output == "swords" ? run.Samples[^1].SwordsMilliunits :
                run.Samples[^1].CoinsMilliunits;
            var initialStock = baselineStock[output].Milliunits;
            var verified = run.Outcome == "pass" && !blocked && milestones == 1 &&
                cursor == steps.Count && run.CompletionSeconds is > 0 &&
                run.CompletionSeconds == run.Samples[^1].ElapsedSeconds &&
                run.CompletionSeconds == run.Events[^1].ElapsedSeconds &&
                run.Samples[^1].NextAction == "completed" &&
                run.Samples[^1].ExtractionOneUnlocked &&
                actualStock > initialStock && observedClockOutput > 0 &&
                run.Events.Any(e => e.Action == "buy-tech-estrazione-1" && e.Result == "executed") &&
                run.Events.Any(e => e.Action == "buy-tech-industria-1" && e.Result == "executed");
            if (run.Outcome == "pass" && !verified ||
                run.Outcome != "pass" && (run.CompletionSeconds is not null || milestones > 0) ||
                run.Outcome == "timeout" && run.Samples[^1].ElapsedSeconds != run.EffectiveHorizonSeconds ||
                run.Outcome == "blocked" && (!blocked || run.Events[^1].Result != "blocked") ||
                run.Outcome != "blocked" && blocked ||
                run.Outcome != "pass" && run.Samples[^1].NextAction == "completed")
                throw new ArgumentException("Industrial outcome is not supported by chronological command/clock evidence.", nameof(runs));
            rows.Add(new(scenario, run.PolicyId, run.RulesVersion, run.Outcome, run.Diagnostic,
                run.CompletionSeconds, verified, cursor, steps.Count, observedClockOutput,
                run.Events.Count(e => e.Result == "waiting"),
                run.Outcome == "pass" ? "completed" : cursor < steps.Count ? steps[cursor] : "await-production",
                run.Events.Any(e => e.Action == "buy-tech-estrazione-1" && e.Result == "executed"),
                run.Events.Any(e => e.Action == "buy-tech-industria-1" && e.Result == "executed"),
                actualStock));
        }
        var comparisons = new List<V15IndustrialChainComparison>();
        foreach (var group in rows.GroupBy(r => (r.ScenarioId, r.RulesVersion)))
        {
            var research = group.SingleOrDefault(r => r.PolicyId == V15IndustrialProgressionBenchmark.PolicyResearchFirst);
            var bread = group.SingleOrDefault(r => r.PolicyId == V15IndustrialProgressionBenchmark.PolicyBreadFirst);
            if (research is null || bread is null) continue;
            long? delta = research is { Verified: true, Diagnostic: false } &&
                bread is { Verified: true, Diagnostic: false }
                ? checked((long)bread.CompletionSeconds!.Value - research.CompletionSeconds!.Value) : null;
            comparisons.Add(new(group.Key.ScenarioId, group.Key.RulesVersion, delta));
        }
        return new(Version, EvidenceScope, rows.Count, rows.Count(r => r.Verified),
            rows.Count(r => r.Diagnostic), rows, comparisons);
    }

    private static bool MatchesCost(V15IndustrialProgressionEvent receipt,
        IReadOnlyDictionary<string, long> expected) =>
        receipt.ResourceCostsMilliunits.Count == expected.Count &&
        expected.All(p => receipt.ResourceCostsMilliunits.TryGetValue(p.Key, out var spent) &&
            spent == p.Value);

    private static bool Nondecreasing(IEnumerable<int> seconds)
    {
        var previous = -1;
        foreach (var value in seconds)
        {
            if (value < 0 || value < previous) return false;
            previous = value;
        }
        return true;
    }

    public static string ToCsv(V15IndustrialChainReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("scenario,policy,rules,outcome,diagnostic,verified,completion_seconds,executed_steps,total_steps,clock_output_milli,waiting_events,pending_action,extraction_purchased,industry_purchased,final_stock_milli\n");
        foreach (var r in report.Observations)
            sb.AppendLine(string.Join(",", r.ScenarioId, r.PolicyId, r.RulesVersion, r.Outcome,
                B(r.Diagnostic), B(r.Verified), r.CompletionSeconds?.ToString(CultureInfo.InvariantCulture) ?? "",
                N(r.ExecutedSteps), N(r.TotalSteps), N(r.ClockOutputMilliunits), N(r.WaitingEvents),
                r.PendingAction, B(r.ExtractionPurchased), B(r.IndustryPurchased), N(r.FinalStockMilliunits)));
        return sb.ToString();
    }

    public static string ToMarkdown(V15IndustrialChainReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("# RC88 — Audit naturale della filiera industriale v15\n\n");
        sb.AppendLine($"Origine: `{report.EvidenceScope}`. Run: {report.RunCount}; " +
            $"ricevute complete: {report.VerifiedRuns}; run diagnostici: {report.DiagnosticRuns}.\n");
        sb.AppendLine("Il `pass` richiede acquisti di ricerca, costruzione/assegnazione della filiera, " +
            "output positivo registrato dal clock e stock finale coerente. Un `timeout` non prova un softlock.\n");
        sb.AppendLine("| Scenario | Strategia | Regime | Esito | Passi | Attesa | Output clock (millesimi) | Completamento (s) |");
        sb.AppendLine("|---|---|---|---|---:|---|---:|---:|");
        foreach (var r in report.Observations)
            sb.AppendLine($"| {r.ScenarioId} | {r.PolicyId} | {r.RulesVersion} | " +
                $"{r.Outcome}{(r.Diagnostic ? " (diagnostico)" : "")}{(r.Verified ? " verificato" : "")} | " +
                $"{r.ExecutedSteps}/{r.TotalSteps} | {r.PendingAction} | {N(r.ClockOutputMilliunits)} | " +
                $"{(r.Verified ? r.CompletionSeconds?.ToString(CultureInfo.InvariantCulture) : "—")} |");
        sb.AppendLine("\nDifferenze temporali bread-first meno research-first: disponibili " +
            "solo per due completamenti **non diagnostici** e realmente verificati.\n");
        foreach (var compare in report.Comparisons)
            sb.AppendLine($"- {compare.ScenarioId} / {compare.RulesVersion}: " +
                (compare.BreadMinusResearchSeconds is long value ? $"{value} s" : "non determinabile"));
        sb.AppendLine("\nL'analisi riguarda i percorsi programmati, non dimostra la raggiungibilità " +
            "di strategie alternative e non modifica i prezzi o il salvataggio.");
        return sb.ToString();
    }

    private static string B(bool value) => value ? "true" : "false";
    private static string N(long value) => value.ToString(CultureInfo.InvariantCulture);
}

public sealed record V15IndustrialChainObservation(string ScenarioId, string PolicyId,
    string RulesVersion, string Outcome, bool Diagnostic, int? CompletionSeconds,
    bool Verified, int ExecutedSteps, int TotalSteps, long ClockOutputMilliunits,
    int WaitingEvents, string PendingAction, bool ExtractionPurchased,
    bool IndustryPurchased, long FinalStockMilliunits);

public sealed record V15IndustrialChainComparison(string ScenarioId, string RulesVersion,
    long? BreadMinusResearchSeconds);

public sealed record V15IndustrialChainReport(string Version, string EvidenceScope,
    int RunCount, int VerifiedRuns, int DiagnosticRuns,
    IReadOnlyList<V15IndustrialChainObservation> Observations,
    IReadOnlyList<V15IndustrialChainComparison> Comparisons);
