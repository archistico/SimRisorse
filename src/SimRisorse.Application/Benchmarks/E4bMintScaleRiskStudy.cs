using System.Globalization;
using System.Text;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;

namespace SimRisorse.Application.Benchmarks;

/// <summary>
/// RC83: analytical comparison of one and two fully staffed mints. The complete
/// supply-chain investment is not the same as the quotation for the mint alone.
/// Warehouse-route failures are evidence of a limitation of THIS route, not a
/// proof that a game is impossible. No prices, runtime rules or saves change.
/// </summary>
public sealed record E4bMintScaleRiskRow(
    string CurveId,
    ProductionTier Tier,
    int ModulesPerMint,
    int CrewSize,
    E4bInvestmentRow OneMint,
    E4bInvestmentRow TwoMints,
    ResourceBundle SecondMintQuote,
    ResourceBundle TwoMintsAtomicQuote,
    ResourceBundle AdditionalSupplyChainCost,
    IReadOnlyDictionary<string, int> AdditionalBuildings,
    int AdditionalMintWorkers,
    int AdditionalResidents,
    int AdditionalSupportWorkers,
    int AdditionalLargeHouses,
    E4bStorageAudit SecondMintWarehouseRoute,
    E4bStorageAudit AtomicTwoMintsWarehouseRoute,
    long AtomicEscrowInstallmentsAtStartingCapacity,
    long SecondMintEscrowInstallmentsAtStartingCapacity,
    decimal FirstMintStonecutterEquivalents,
    decimal SecondMintStonecutterEquivalents,
    string FirstMintTargetBand,
    string SecondMintTargetBand);

public sealed record E4bMintScaleRiskReport(
    string Version,
    string Mode,
    string StorageScope,
    string WorkforceScope,
    ResourceBundle StartingCapacity,
    IReadOnlyList<E4bMintScaleRiskRow> Rows);

public sealed class E4bMintScaleRiskStudy
{
    public const string Version = "e4b-mint-scale-risk-1";
    public const string Mode = "analytical-proposal/not-applied";

    /// <summary>Target requested for the DIRECT mint price, expressed as the
    /// recipe/material work equivalent of an initial stonecutter quotation.
    /// It does not measure time to completion, all prerequisites or player ROI.</summary>
    public static string TargetBand(decimal stonecutterEquivalents) => stonecutterEquivalents switch
    {
        < 0m => throw new ArgumentOutOfRangeException(nameof(stonecutterEquivalents)),
        < 100m => "below-100",
        <= 1000m => "within-100-to-1000",
        _ => "above-1000"
    };

    public E4bMintScaleRiskReport Run()
    {
        var study = new E4bEconomicCurveStudy();
        var capacity = study.AuditWarehouseRoute(E4bEconomicCurveStudy.Curves[0],
            ResourceBundle.Empty).Capacity;
        var rows = new List<E4bMintScaleRiskRow>();
        foreach (var curve in E4bEconomicCurveStudy.Curves)
        {
            var firstStonecutter = study.Quote(curve, "stonecutter", ProductionTier.Base);
            var benchmarkWork = study.ReferenceMaterialWorkerSeconds(firstStonecutter);
            if (benchmarkWork <= 0m)
                throw new InvalidOperationException("Stonecutter work reference must be positive.");

            foreach (var tier in Enum.GetValues<ProductionTier>())
            {
                var one = study.Investment(curve, 1, tier);
                var two = study.Investment(curve, 2, tier);
                var firstQuote = study.Quote(curve, "mint", tier);
                var secondQuote = study.Quote(curve, "mint", tier, 1);
                var atomicQuote = study.Quote(curve, "mint", tier, 0, 2);
                if (!atomicQuote.Equals(firstQuote + secondQuote) ||
                    !two.ConstructionWithoutWarehouses.Covers(one.ConstructionWithoutWarehouses))
                    throw new InvalidOperationException("Progressive construction costs did not reconcile.");

                var required = new SortedDictionary<string, int>(StringComparer.Ordinal);
                foreach (var (id, count) in two.RequiredBuildings)
                {
                    var delta = count - one.RequiredBuildings.GetValueOrDefault(id);
                    if (delta < 0)
                        throw new InvalidOperationException("A second mint reduced a supply-chain requirement.");
                    if (delta > 0) required.Add(id, delta);
                }
                if (one.RequiredBuildings.Keys.Except(two.RequiredBuildings.Keys).Any())
                    throw new InvalidOperationException("A second mint removed a prerequisite.");

                // Check the same complete-crew invariant as the RC81 runtime:
                // a second mint must acquire additional workers, not reuse the
                // ten people assigned to the first one.
                var modules = E4bEconomicCurveStudy.Modules(tier);
                var staffed = tier switch
                {
                    ProductionTier.Base => CrewTeamPortfolio.Create("mint", 10,
                        new(2, checked(modules * 20))),
                    ProductionTier.Large => CrewTeamPortfolio.Create("mint", 10, default,
                        new(2, checked(modules * 20))),
                    ProductionTier.Advanced => CrewTeamPortfolio.Create("mint", 10, default, default,
                        new(2, checked(modules * 20))),
                    _ => throw new ArgumentOutOfRangeException(nameof(tier))
                };
                if (staffed.StaffedModules != two.Workforce.Modules ||
                    staffed.AssignedWorkers != two.Workforce.MintWorkers)
                    throw new InvalidOperationException("Mint staffing and workforce budgets disagree.");

                var extraMintWorkers = two.Workforce.MintWorkers - one.Workforce.MintWorkers;
                var extraResidents = two.Workforce.RequiredResidents - one.Workforce.RequiredResidents;
                if (extraResidents < extraMintWorkers)
                    throw new InvalidOperationException("The workforce budget omits additional mint workers.");

                var firstRatio = study.ReferenceMaterialWorkerSeconds(firstQuote) / benchmarkWork;
                var secondRatio = study.ReferenceMaterialWorkerSeconds(secondQuote) / benchmarkWork;
                rows.Add(new(curve.Id, tier, modules, 10, one, two,
                    secondQuote, atomicQuote,
                    two.ConstructionWithoutWarehouses - one.ConstructionWithoutWarehouses,
                    required, extraMintWorkers, extraResidents, extraResidents - extraMintWorkers,
                    two.Workforce.AdditionalLargeHouses - one.Workforce.AdditionalLargeHouses,
                    study.AuditWarehouseRoute(curve, secondQuote),
                    study.AuditWarehouseRoute(curve, atomicQuote),
                    ProgressiveConstructionEscrow.MinimumInstallments(atomicQuote, capacity),
                    ProgressiveConstructionEscrow.MinimumInstallments(secondQuote, capacity),
                    firstRatio, secondRatio, TargetBand(firstRatio), TargetBand(secondRatio)));
            }
        }
        return new(Version, Mode, "warehouse-only/512-step-cap/assume-materials-earnable",
            "full-teams/analytical-food-and-housing-budget/not-measured",
            capacity, rows);
    }

    public static string ToCsv(E4bMintScaleRiskReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("curve,tier,modules_per_mint,mint_workers_one,mint_workers_two,extra_mint_workers,extra_residents,extra_support_workers,extra_large_houses,second_mint_warehouse_route,atomic_two_warehouse_route,second_mint_route_warehouses,atomic_two_route_warehouses,escrow_second_mint_at_start,escrow_atomic_two_at_start,one_mint_work_equivalents,second_mint_work_equivalents,one_mint_target,second_mint_target,second_mint_planks,second_mint_stone,extra_chain_planks,extra_chain_stone\n");
        foreach (var r in report.Rows)
        {
            sb.Append(r.CurveId).Append(',').Append(r.Tier).Append(',')
              .Append(r.ModulesPerMint).Append(',')
              .Append(r.OneMint.Workforce.MintWorkers).Append(',')
              .Append(r.TwoMints.Workforce.MintWorkers).Append(',')
              .Append(r.AdditionalMintWorkers).Append(',')
              .Append(r.AdditionalResidents).Append(',')
              .Append(r.AdditionalSupportWorkers).Append(',')
              .Append(r.AdditionalLargeHouses).Append(',')
              .Append(r.SecondMintWarehouseRoute.Status).Append(',')
              .Append(r.AtomicTwoMintsWarehouseRoute.Status).Append(',')
              .Append(r.SecondMintWarehouseRoute.Warehouses).Append(',')
              .Append(r.AtomicTwoMintsWarehouseRoute.Warehouses).Append(',')
              .Append(r.SecondMintEscrowInstallmentsAtStartingCapacity).Append(',')
              .Append(r.AtomicEscrowInstallmentsAtStartingCapacity).Append(',')
              .Append(r.FirstMintStonecutterEquivalents.ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
              .Append(r.SecondMintStonecutterEquivalents.ToString("0.###", CultureInfo.InvariantCulture)).Append(',')
              .Append(r.FirstMintTargetBand).Append(',')
              .Append(r.SecondMintTargetBand).Append(',')
              .Append(r.SecondMintQuote["planks"].Units.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(r.SecondMintQuote["stone"].Units.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(r.AdditionalSupplyChainCost["planks"].Units.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(r.AdditionalSupplyChainCost["stone"].Units.ToString(CultureInfo.InvariantCulture)).Append('\n');
        }
        return sb.ToString();
    }

    public static string ToMarkdown(E4bMintScaleRiskReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("# RC83 — Rischi della seconda Zecca e dei tier (solo studio)\n\n");
        sb.AppendLine("Nessuna modifica a prezzi, curve, partite, salvataggi o GUI. Le equivalenze con il Tagliapietre confrontano **solo il lavoro materiale stimato per il prezzo diretto**, non la filiera completa né i tempi di gioco.");
        sb.AppendLine("La finestra 100–1000 Tagliapietre è un **obiettivo progettuale da valutare**, non un criterio già approvato o una prova di giocabilità.\n");
        sb.AppendLine("| Curva | Tier | Operatori Zecca 1 → 2 | Residenti aggiuntivi (di cui supporto) | Pagamento della seconda Zecca | Lotto atomico x2 | Rate minime seconda / lotto (capienza iniziale) | Equivalenti prima / seconda Zecca | Fascia seconda |");
        sb.AppendLine("| --- | --- | ---: | ---: | --- | --- | ---: | ---: | --- |");
        foreach (var r in report.Rows)
            sb.Append("| ").Append(r.CurveId).Append(" | ").Append(r.Tier).Append(" | ")
              .Append(r.OneMint.Workforce.MintWorkers).Append(" → ").Append(r.TwoMints.Workforce.MintWorkers)
              .Append(" | +").Append(r.AdditionalResidents).Append(" (+").Append(r.AdditionalSupportWorkers)
              .Append(") | ").Append(r.SecondMintWarehouseRoute.Status)
              .Append(" | ").Append(r.AtomicTwoMintsWarehouseRoute.Status)
              .Append(" | ").Append(r.SecondMintEscrowInstallmentsAtStartingCapacity).Append(" / ")
              .Append(r.AtomicEscrowInstallmentsAtStartingCapacity).Append(" | ")
              .Append(r.FirstMintStonecutterEquivalents.ToString("0.#", CultureInfo.InvariantCulture))
              .Append(" / ").Append(r.SecondMintStonecutterEquivalents.ToString("0.#", CultureInfo.InvariantCulture))
              .Append(" | ").Append(r.SecondMintTargetBand).AppendLine(" |");
        sb.AppendLine("\n**Interpretazione:** il percorso Magazzini simulato amplia la capacità solo quando si può già pagare il Magazzino successivo. Un suo arresto non dimostra un softlock universale. Le rate sono un limite inferiore di sola capienza; non sono materiali prodotti né una funzione attiva nella campagna.");
        sb.AppendLine("Le proiezioni delle popolazioni/filiera sono budget statici; vanno confrontati con run naturali integrali MintTen. Nessun run temporale RC83 è stato effettuato da questo report.");
        return sb.ToString();
    }
}
