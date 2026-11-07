using System.Globalization;
using System.Text;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;

namespace SimRisorse.Application.Benchmarks;

public sealed record E4bPriceReadinessRow(
    string CurveId, ProductionTier Tier, int WorkersForOneMint, int WorkersForTwoMints,
    ResourceBundle FirstMint, ResourceBundle SecondMint, ResourceBundle AtomicTwoMints,
    decimal FirstStonecutterEquivalents, decimal SecondStonecutterEquivalents,
    string FirstTargetBand, string SecondTargetBand,
    bool FirstFitsInitialCapacity, bool SecondFitsInitialCapacity, bool AtomicTwoFitsInitialCapacity,
    E4bStorageAudit FirstWarehouseRoute, E4bStorageAudit SecondWarehouseRoute,
    E4bStorageAudit AtomicWarehouseRoute,
    long FirstEscrowInstallments, long SecondEscrowInstallments, long AtomicEscrowInstallments,
    IReadOnlyList<string> FirstCapacityGaps, IReadOnlyList<string> SecondCapacityGaps,
    IReadOnlyList<string> AtomicCapacityGaps, string ReviewPriority);

public sealed record E4bPriceReadinessReport(
    string Version, string Mode, string EvidenceScope, ResourceBundle InitialCapacity,
    IReadOnlyList<E4bPriceReadinessRow> Rows, int ImportedRuns,
    int VerifiedNaturalRuns, int VerifiedMintTenRuns, int DiagnosticRuns,
    string ApprovalStatus);

/// <summary>
/// RC89: explicit pre-activation gates for the 3x3 proposed mint prices.
/// Reuses the quotation engine and the bounded warehouse-route study; never
/// changes live game prices or upgrades a simulated clock event to evidence.
/// The availability of physical materials and actual completion times remain unproved.
/// </summary>
public sealed class E4bPriceReadinessStudy
{
    public const string Version = "rc89-e4b-price-readiness-1";
    public const string Mode = "analytical-proposal/not-applied";
    public const string ApprovalStatus = "not-approved/pricing-and-financing-policy-pending";

    /// <summary>Resource ids with a positive deficit at the stated capacity.
    /// Missing entries are zero, as required by sparse ResourceBundle.</summary>
    public static IReadOnlyList<string> CapacityGaps(ResourceBundle quote, ResourceBundle capacity)
    {
        ArgumentNullException.ThrowIfNull(quote);
        ArgumentNullException.ThrowIfNull(capacity);
        return quote.Amounts.Where(p => p.Value.Milliunits > capacity[p.Key].Milliunits)
            .Select(p => p.Key).OrderBy(id => id, StringComparer.Ordinal).ToArray();
    }

    /// <summary>A warehouse-route failure is not a universal deadlock.
    /// The direct Base target band is a design goal, not an acceptance threshold
    /// for Large and Advanced or a replacement for natural-run evidence.</summary>
    public static string Priority(ProductionTier tier, string firstTarget,
        E4bStorageAudit firstRoute, E4bStorageAudit secondRoute)
    {
        ArgumentNullException.ThrowIfNull(firstRoute);
        ArgumentNullException.ThrowIfNull(secondRoute);
        if (tier == ProductionTier.Base && firstTarget != "within-100-to-1000")
            return "recalibrate-base-price";
        if (firstRoute.Status != "fits-warehouse-route" ||
            secondRoute.Status != "fits-warehouse-route")
            return "review-progressive-funding";
        return "needs-natural-playtest";
    }

    public E4bPriceReadinessReport Run(IReadOnlyList<V15IndustrialProgressionRun>? recordedRuns = null)
    {
        var imported = 0;
        var verified = 0;
        var verifiedMintTen = 0;
        var diagnostics = 0;
        if (recordedRuns is not null)
        {
            // Always delegate verification to the receipts auditor. Never trust
            // user-provided Outcome = pass or a short horizon on its own.
            var evidence = V15IndustrialChainAudit.Analyze(recordedRuns);
            imported = evidence.RunCount;
            verified = evidence.VerifiedRuns;
            verifiedMintTen = evidence.Observations.Count(row => row.Verified &&
                row.RulesVersion == V15CrewRules.SavedRulesVersion(V15CrewMode.MintTen));
            diagnostics = evidence.DiagnosticRuns;
        }

        var scale = new E4bMintScaleRiskStudy().Run();
        var study = new E4bEconomicCurveStudy();
        var capacity = scale.StartingCapacity;
        var rows = new List<E4bPriceReadinessRow>();
        foreach (var r in scale.Rows)
        {
            var curve = E4bEconomicCurveStudy.Curves.Single(c => c.Id == r.CurveId);
            var first = study.Quote(curve, "mint", r.Tier);
            var firstRoute = study.AuditWarehouseRoute(curve, first);
            var firstGaps = CapacityGaps(first, capacity);
            var secondGaps = CapacityGaps(r.SecondMintQuote, capacity);
            var atomicGaps = CapacityGaps(r.TwoMintsAtomicQuote, capacity);
            if (!first.Equals(r.OneMint.CurrentMintPayment) ||
                !r.TwoMintsAtomicQuote.Equals(first + r.SecondMintQuote) ||
                r.AdditionalMintWorkers != r.ModulesPerMint * 10)
                throw new InvalidOperationException("Mint price, crew and acquisition counters diverged.");
            rows.Add(new(r.CurveId, r.Tier,
                r.OneMint.Workforce.MintWorkers, r.TwoMints.Workforce.MintWorkers,
                first, r.SecondMintQuote, r.TwoMintsAtomicQuote,
                r.FirstMintStonecutterEquivalents, r.SecondMintStonecutterEquivalents,
                r.FirstMintTargetBand, r.SecondMintTargetBand,
                firstGaps.Count == 0, secondGaps.Count == 0, atomicGaps.Count == 0,
                firstRoute, r.SecondMintWarehouseRoute, r.AtomicTwoMintsWarehouseRoute,
                ProgressiveConstructionEscrow.MinimumInstallments(first, capacity),
                r.SecondMintEscrowInstallmentsAtStartingCapacity,
                r.AtomicEscrowInstallmentsAtStartingCapacity,
                firstGaps, secondGaps, atomicGaps,
                Priority(r.Tier, r.FirstMintTargetBand, firstRoute, r.SecondMintWarehouseRoute)));
        }
        return new(Version, Mode,
            "RC88-receipts-if-imported/current-prices-only; A-B-C-are-unplayed-counterfactuals",
            capacity, rows, imported, verified, verifiedMintTen, diagnostics, ApprovalStatus);
    }

    private static string N(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    private static string B(bool value) => value ? "true" : "false";

    public static string ToCsv(E4bPriceReadinessReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("curve,tier,first_quote,second_quote,atomic_two_quote,mint_workers_one,mint_workers_two,first_stonecutter_equivalents,second_stonecutter_equivalents,first_target_band,second_target_band,first_fits_initial,second_fits_initial,atomic_two_fits_initial,first_warehouse_route,second_warehouse_route,atomic_warehouse_route,first_escrow_installments,second_escrow_installments,atomic_escrow_installments,first_capacity_gaps,second_capacity_gaps,atomic_capacity_gaps,priority\n");
        foreach (var r in report.Rows)
            sb.AppendLine(string.Join(",", r.CurveId, r.Tier,
                E4bCurveStudyExport.Materials(r.FirstMint),
                E4bCurveStudyExport.Materials(r.SecondMint),
                E4bCurveStudyExport.Materials(r.AtomicTwoMints),
                r.WorkersForOneMint, r.WorkersForTwoMints,
                N(r.FirstStonecutterEquivalents), N(r.SecondStonecutterEquivalents),
                r.FirstTargetBand, r.SecondTargetBand, B(r.FirstFitsInitialCapacity),
                B(r.SecondFitsInitialCapacity), B(r.AtomicTwoFitsInitialCapacity),
                r.FirstWarehouseRoute.Status, r.SecondWarehouseRoute.Status,
                r.AtomicWarehouseRoute.Status, r.FirstEscrowInstallments,
                r.SecondEscrowInstallments, r.AtomicEscrowInstallments,
                string.Join("+", r.FirstCapacityGaps), string.Join("+", r.SecondCapacityGaps),
                string.Join("+", r.AtomicCapacityGaps), r.ReviewPriority));
        return sb.ToString();
    }

    public static string ToMarkdown(E4bPriceReadinessReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("# RC89 — Prontezza delle curve economiche (solo studio)\n\n");
        sb.AppendLine("**Nessun prezzo, salvataggio o comportamento della partita è stato cambiato.** Lo studio combina RC78/RC79/RC83 e, se forniti, solo i run industriali con ricevute verificate RC88. Non simula le proposte A/B/C dentro una partita.");
        sb.AppendLine($"\nDati importati: {report.ImportedRuns}; verificati a prezzi correnti: {report.VerifiedNaturalRuns}; MintTen verificati: {report.VerifiedMintTenRuns}; diagnostici: {report.DiagnosticRuns}. **Approvazione: {report.ApprovalStatus}.**");
        sb.AppendLine("\n| Curva | Livello | Addetti 1/2 | Preventivi prima / seconda | Equiv. Tagliapietre 1/2 | Prima / seconda / lotto x2 in capienza iniziale | Fuori capienza iniziale (prima / seconda) | Percorso Magazzini prima / seconda | Rate minime prima / seconda / lotto | Priorità |\n|---|---|---|---|---|---|---|---|---|---|");
        foreach (var r in report.Rows)
            sb.AppendLine($"| {r.CurveId} | {r.Tier} | {r.WorkersForOneMint}/{r.WorkersForTwoMints} | {E4bCurveStudyExport.Materials(r.FirstMint)} / {E4bCurveStudyExport.Materials(r.SecondMint)} | {N(r.FirstStonecutterEquivalents)}/{N(r.SecondStonecutterEquivalents)} | {B(r.FirstFitsInitialCapacity)}/{B(r.SecondFitsInitialCapacity)}/{B(r.AtomicTwoFitsInitialCapacity)} | {string.Join("+", r.FirstCapacityGaps)} / {string.Join("+", r.SecondCapacityGaps)} | {r.FirstWarehouseRoute.Status}/{r.SecondWarehouseRoute.Status} | {r.FirstEscrowInstallments}/{r.SecondEscrowInstallments}/{r.AtomicEscrowInstallments} | {r.ReviewPriority} |");
        sb.AppendLine("\nIl criterio di 100–1000 Tagliapietre è applicato **soltanto alla prima Zecca Base** e rimane qualitativo: equivale al lavoro nominale sui materiali del preventivo diretto, non ai tempi della filiera completa.");
        sb.AppendLine("`fits-warehouse-route` significa soltanto che il percorso limitato di Magazzini, ipotizzando materiali disponibili, consente il pagamento. `warehouse-payment-exceeds-capacity` non dimostra che la campagna sia impossibile: esistono altre capacità, lotti sequenziali e l'escrow (ancora NON attivo). Le rate sono un limite inferiore di capienza, non attestano ricavi o materiali futuri.");
        sb.AppendLine("\n**Decisione ancora necessaria:** coefficienti definitivi, costo delle copie, pagamento atomico/sequenziale/escrow, capacità raggiungibili e validazione della progressione naturale MintTen con i prezzi nuovi. Timeout e run diagnostici non equivalgono a softlock né a completamento.");
        return sb.ToString();
    }
}
