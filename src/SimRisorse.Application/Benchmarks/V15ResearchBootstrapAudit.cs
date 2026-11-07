using System.Globalization;
using System.Text;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Benchmarks;

/// <summary>
/// RC87: evidence audit of the first research unlock using actual V15-I01
/// campaign command receipts. It never awards resources, advances a clock,
/// or interprets a scripted timeout as proof of an unreachable campaign.
/// </summary>
public static class V15ResearchBootstrapAudit
{
    public const string Version = "rc87-bootstrap-evidence-1";
    private const string Scenario = "V15-I01";
    private const string Goal = "unlock-estrazione-1";
    private const string Purchase = "buy-tech-estrazione-1";

    public static V15ResearchBootstrapReport Analyze(IEnumerable<V15IndustrialProgressionRun> runs)
    {
        ArgumentNullException.ThrowIfNull(runs);
        var fixture = V15BenchmarkFixture.Create();
        var technology = fixture.Technologies.Technologies.Single(t => t.Id == "estrazione-1");
        if (!technology.Available || technology.Prerequisites.Length != 0 ||
            technology.RequiredBuildings.Length != 0)
            throw new InvalidOperationException("Estrazione I is no longer a first-tier research purchase.");
        var pencilsNeeded = fixture.School.TrainingCost(1)[V14SchoolSupplies.ResourceId].Milliunits;
        var pencilsInitially = fixture.Start.CityCampaign.Resources[V14SchoolSupplies.ResourceId].Milliunits;
        if (pencilsInitially != pencilsNeeded || pencilsInitially <= 0 ||
            fixture.Start.CityCampaign.Resources["coal"].Units !=
                V15FoodCampaignCheckpoints.MunicipalStarterCoal ||
            V14SchoolSupplies.Inputs(1)["coal"].Milliunits <= 0)
            throw new InvalidOperationException("The municipal research bootstrap contract has changed.");
        var expectedInitialStock = V15StockAuditor.PhysicalTotals(fixture.Start).Amounts;
        var source = runs.ToArray();
        if (source.Any(run => run is null))
            throw new ArgumentException("A bootstrap evidence row cannot be null.", nameof(runs));
        if (source.Length == 0)
            throw new ArgumentException("At least one observed V15-I01 run is required.", nameof(runs));
        if (source.Select(run => run.PolicyId).Distinct(StringComparer.Ordinal).Count() != source.Length)
            throw new ArgumentException("A policy may appear only once.", nameof(runs));

        var observations = new List<V15ResearchBootstrapObservation>();
        foreach (var run in source)
        {
            if (run is null || run.ScenarioId != Scenario || run.Goal != Goal ||
                run.PolicyId is not (V15IndustrialProgressionBenchmark.PolicyResearchFirst or
                    V15IndustrialProgressionBenchmark.PolicyBreadFirst) ||
                run.RulesVersion != "civil-growth-15" ||
                run.BaselineRulesVersion != fixture.Baseline.Catalog.RulesVersion ||
                run.ScenarioBaselineId != fixture.Start.CityCampaign.ScenarioId ||
                run.Fixture != "fresh-game/no-grants" ||
                run.InitialPhysicalStockMilliunits is null || run.Events is null ||
                run.Samples is null || run.Samples.Count == 0 ||
                run.Outcome is not ("pass" or "timeout" or "blocked") ||
                run.DeclaredHorizonSeconds <= 0 || run.EffectiveHorizonSeconds < 0 ||
                run.EffectiveHorizonSeconds > run.DeclaredHorizonSeconds ||
                run.Diagnostic != (run.EffectiveHorizonSeconds < run.DeclaredHorizonSeconds) ||
                run.InitialPhysicalStockMilliunits.Count != expectedInitialStock.Count ||
                expectedInitialStock.Any(entry =>
                    !run.InitialPhysicalStockMilliunits.TryGetValue(entry.Key, out var actualStart) ||
                    actualStart != entry.Value.Milliunits) ||
                run.Samples[0].ElapsedSeconds != 0 ||
                run.Samples[^1].ElapsedSeconds > run.EffectiveHorizonSeconds ||
                !InOrder(run.Events.Select(e => e.ElapsedSeconds)) ||
                !InOrder(run.Samples.Select(s => s.ElapsedSeconds)) ||
                run.Events.Any(e => e.Result is not ("executed" or "waiting" or "blocked" or "milestone") ||
                    e.ResourceCostsMilliunits is null || e.ResearchSpentMilliPoints < 0 ||
                    e.ResourceCostsMilliunits.Any(p => p.Value < 0) ||
                    e.ElapsedSeconds > run.EffectiveHorizonSeconds) ||
                run.Events.Any(e => e.Action is "craft-pencils" or "build-coal-mine" or "staff-coal-mine"))
                throw new ArgumentException("Invalid or unrelated natural bootstrap evidence.", nameof(runs));

            var school = Executed(run, "build-school");
            var enrolled = Executed(run, "enroll-student");
            var assigned = Executed(run, "assign-researcher");
            var purchase = Executed(run, Purchase);
            if (new[] { school, enrolled, assigned, purchase }.Any(group => group.Length > 1))
                throw new ArgumentException("Duplicate research milestones in one natural run.", nameof(runs));
            // Every accepted receipt must agree with the normal gameplay
            // purchase quote: no free school, uncharged food or hidden grant.
            if (school.Length == 1 && !MatchesCost(school[0], V14SchoolConstructionRules.Candidate.Cost) ||
                enrolled.Length == 1 && !MatchesCost(enrolled[0], fixture.School.TrainingCost(1)) ||
                purchase.Length == 1 && purchase[0].ResourceCostsMilliunits.Count != 0)
                throw new ArgumentException("The school, training or technology receipt is incomplete.", nameof(runs));
            var actualPencils = enrolled.Length == 1 &&
                enrolled[0].ResourceCostsMilliunits.TryGetValue("pencils", out var pencilSpend)
                ? pencilSpend : 0;
            var actualPoints = purchase.Length == 1 ? purchase[0].ResearchSpentMilliPoints : 0;
            if (enrolled.Length == 1 && actualPencils != pencilsNeeded ||
                purchase.Length == 1 && actualPoints != technology.Cost.MilliPoints)
                throw new ArgumentException("The recorded school or technology payment differs from the real rules.", nameof(runs));
            var sequence = school.Length == 1 && enrolled.Length == 1 &&
                assigned.Length == 1 && purchase.Length == 1 &&
                school[0].ElapsedSeconds <= enrolled[0].ElapsedSeconds &&
                enrolled[0].ElapsedSeconds <= assigned[0].ElapsedSeconds &&
                assigned[0].ElapsedSeconds <= purchase[0].ElapsedSeconds &&
                OrderedActions(run, "build-school", "enroll-student", "assign-researcher", Purchase);
            // With one fresh student and one active researcher, the saved
            // research balance starts at zero. Training and generation must
            // consume actual elapsed clock time; a zero-time receipt is not proof.
            var minimumResearchSeconds = checked((technology.Cost.MilliPoints +
                fixture.School.MilliPointsPerResearcherPerSecond - 1) /
                fixture.School.MilliPointsPerResearcherPerSecond);
            var temporalEvidence = enrolled.Length == 1 && assigned.Length == 1 && purchase.Length == 1 &&
                (long)assigned[0].ElapsedSeconds - enrolled[0].ElapsedSeconds >= fixture.School.TrainingSeconds &&
                (long)purchase[0].ElapsedSeconds - assigned[0].ElapsedSeconds >= minimumResearchSeconds;
            var milestone = run.Events.Where(e => e.Action == Goal && e.Result == "milestone").ToArray();
            var clockEvidence = run.Samples[^1].ExtractionOneUnlocked &&
                run.Samples[^1].Schools >= 1 &&
                run.Samples[^1].ActiveResearchers >= 1 &&
                run.Samples[^1].QualifiedResearchers >= 1;
            var verified = sequence && temporalEvidence && clockEvidence && milestone.Length == 1 &&
                run.Outcome == "pass" && run.CompletionSeconds == milestone[0].ElapsedSeconds &&
                run.CompletionSeconds == run.Samples[^1].ElapsedSeconds;
            if (run.Outcome == "pass" && !verified ||
                run.Outcome != "pass" && (run.CompletionSeconds is not null || milestone.Length > 0) ||
                run.Outcome == "timeout" && run.Samples[^1].ElapsedSeconds != run.EffectiveHorizonSeconds ||
                run.Outcome == "blocked" && !run.Events.Any(e => e.Result == "blocked") ||
                milestone.Length > 1)
                throw new ArgumentException("Reported outcome is not supported by an actual research receipt and clock evidence.", nameof(runs));

            observations.Add(new(run.PolicyId, run.Outcome, run.Diagnostic,
                run.CompletionSeconds, school.Length == 1, enrolled.Length == 1,
                actualPencils, assigned.Length == 1, purchase.Length == 1,
                actualPoints, run.Samples[^1].ExtractionOneUnlocked,
                verified, run.Events.Count(e => e.Result == "waiting"),
                run.Samples[^1].NextAction));
        }
        var research = observations.SingleOrDefault(o =>
            o.PolicyId == V15IndustrialProgressionBenchmark.PolicyResearchFirst);
        var bread = observations.SingleOrDefault(o =>
            o.PolicyId == V15IndustrialProgressionBenchmark.PolicyBreadFirst);
        long? difference = research is { Verified: true, Diagnostic: false } &&
            bread is { Verified: true, Diagnostic: false }
            ? checked((long)bread.CompletionSeconds!.Value - research.CompletionSeconds!.Value)
            : null;
        return new(Version, "natural-v15/real-receipts/no-grants", pencilsInitially,
            pencilsNeeded, technology.Cost.MilliPoints, source.Length,
            observations.Count(o => o.Verified), observations, difference);
    }

    private static bool MatchesCost(V15IndustrialProgressionEvent item, ResourceBundle cost) =>
        item.ResourceCostsMilliunits.Count == cost.Amounts.Count &&
        cost.Amounts.All(entry => item.ResourceCostsMilliunits.TryGetValue(entry.Key, out var actual) &&
            actual == entry.Value.Milliunits);

    private static V15IndustrialProgressionEvent[] Executed(V15IndustrialProgressionRun run, string action) =>
        run.Events.Where(e => e.Action == action && e.Result == "executed").ToArray();

    private static bool OrderedActions(V15IndustrialProgressionRun run, params string[] actions)
    {
        var last = -1;
        foreach (var action in actions)
        {
            var index = -1;
            for (var i = 0; i < run.Events.Count; i++)
                if (run.Events[i].Action == action && run.Events[i].Result == "executed")
                {
                    index = i;
                    break;
                }
            if (index <= last) return false;
            last = index;
        }
        return true;
    }

    private static bool InOrder(IEnumerable<int> seconds)
    {
        var previous = -1;
        foreach (var second in seconds)
        {
            if (second < 0 || second < previous) return false;
            previous = second;
        }
        return true;
    }

    public static string ToCsv(V15ResearchBootstrapReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("policy,outcome,diagnostic,school_built,student_enrolled,pencils_spent_milli,researcher_assigned,technology_purchased,research_spent_millipoints,extraction_observed,verified,completion_seconds,waiting_events,pending_action\n");
        foreach (var row in report.Observations)
            sb.AppendLine(string.Join(",", row.PolicyId, row.Outcome,
                row.Diagnostic ? "true" : "false", row.SchoolBuilt ? "true" : "false",
                row.StudentEnrolled ? "true" : "false", row.PencilsSpentMilliunits.ToString(CultureInfo.InvariantCulture),
                row.ResearcherAssigned ? "true" : "false", row.TechnologyPurchased ? "true" : "false",
                row.ResearchSpentMilliPoints.ToString(CultureInfo.InvariantCulture),
                row.ExtractionObserved ? "true" : "false", row.Verified ? "true" : "false",
                row.CompletionSeconds?.ToString(CultureInfo.InvariantCulture) ?? "",
                row.WaitingEvents.ToString(CultureInfo.InvariantCulture), row.PendingAction));
        return sb.ToString();
    }

    public static string ToMarkdown(V15ResearchBootstrapReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("# RC87 — Ricerca iniziale v15: ricevute e sblocco Estrazione I\n\n");
        sb.AppendLine($"Origine: `{report.EvidenceScope}`. Matite iniziali: {report.StartingPencilsMilliunits} millesimi; " +
            $"costo del primo studente: {report.PencilsPerStudentMilliunits} millesimi; " +
            $"costo Estrazione I: {report.ExtractionCostMilliPoints} millipunti.\n");
        sb.AppendLine("La miniera e la filiera industriale sono successive all'obiettivo V15-I01: " +
            "non possono ritardare artificiosamente il riconoscimento dell'acquisto della tecnologia. " +
            "Una strategia incompleta o a tempo ridotto NON prova un softlock.\n");
        sb.AppendLine("| Strategia | Esito | Scuola | Studente | Ricercatore | Ricerca acquistata | Evidenza completa | Tempo osservato (s) |");
        sb.AppendLine("|---|---|---|---|---|---|---|---:|");
        foreach (var row in report.Observations)
            sb.AppendLine($"| {row.PolicyId} | {row.Outcome}{(row.Diagnostic ? " (diagnostico)" : "")} | " +
                $"{(row.SchoolBuilt ? "sì" : "no")} | {(row.StudentEnrolled ? "sì" : "no")} | " +
                $"{(row.ResearcherAssigned ? "sì" : "no")} | {(row.TechnologyPurchased ? "sì" : "no")} | " +
                $"{(row.Verified ? "sì" : "no")} | {(row.Verified ? row.CompletionSeconds?.ToString(CultureInfo.InvariantCulture) : "—")} |");
        sb.AppendLine("\nConfronto bread-first meno research-first: " +
            (report.BreadMinusResearchSeconds is long delta ? $"{delta} s (due milestone reali)" :
                "non determinabile, servono due completamenti reali non diagnostici") + ".");
        return sb.ToString();
    }
}

public sealed record V15ResearchBootstrapObservation(string PolicyId, string Outcome, bool Diagnostic,
    int? CompletionSeconds, bool SchoolBuilt, bool StudentEnrolled, long PencilsSpentMilliunits,
    bool ResearcherAssigned, bool TechnologyPurchased, long ResearchSpentMilliPoints,
    bool ExtractionObserved, bool Verified, int WaitingEvents, string PendingAction);

public sealed record V15ResearchBootstrapReport(string Version, string EvidenceScope,
    long StartingPencilsMilliunits, long PencilsPerStudentMilliunits, long ExtractionCostMilliPoints,
    int RunCount, int VerifiedRuns, IReadOnlyList<V15ResearchBootstrapObservation> Observations,
    long? BreadMinusResearchSeconds);
