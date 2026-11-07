using System.Globalization;
using System.Text;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;

namespace SimRisorse.Application.Benchmarks;

public sealed record E4bFinancingComparisonRow(
    string CurveId, ProductionTier Tier, int Quantity,
    ResourceBundle AtomicQuote, ResourceBundle LargestSingleQuote,
    E4bStorageAudit AtomicWarehouseRoute, E4bStorageAudit SequentialWarehouseRoute,
    long StartingCapacityEscrowInstallments, long SequentialCapacityEscrowInstallments);

public sealed record E4bFinancingComparisonReport(
    string Version, string Mode, ResourceBundle InitialCapacity,
    IReadOnlyList<E4bFinancingComparisonRow> Rows);

/// <summary>
/// RC79 comparison of three financing mechanisms; an isolated, evidence-free
/// feasibility test, not measured gameplay. No new game construction mechanism
/// is enabled. Sequential purchases preserve the progressive acquisition
/// counter; deposits preserve one atomic batch purchase until fully paid.
/// </summary>
public sealed class E4bFinancingFeasibilityStudy
{
    public const string Version = "e4b-financing-options-1";

    public E4bFinancingComparisonReport Run()
    {
        var study = new E4bEconomicCurveStudy();
        var initial = study.AuditWarehouseRoute(E4bEconomicCurveStudy.Curves[0], ResourceBundle.Empty).Capacity;
        var rows = new List<E4bFinancingComparisonRow>();
        foreach (var curve in E4bEconomicCurveStudy.Curves)
            foreach (var tier in Enum.GetValues<ProductionTier>())
                foreach (var count in new[] { 1, 5, 10 })
                {
                    var atomic = study.Quote(curve, "mint", tier, 0, count);
                    // Monotonically rising prices: the last individual quote
                    // determines whether all sequential purchases fit storage.
                    var largestSingle = study.Quote(curve, "mint", tier, count - 1);
                    var atomicRoute = study.AuditWarehouseRoute(curve, atomic);
                    var sequentialRoute = study.AuditWarehouseRoute(curve, largestSingle);
                    rows.Add(new(curve.Id, tier, count, atomic, largestSingle,
                        atomicRoute, sequentialRoute,
                        ProgressiveConstructionEscrow.MinimumInstallments(atomic, initial),
                        ProgressiveConstructionEscrow.MinimumInstallments(atomic, sequentialRoute.Capacity)));
                }
        return new(Version, "analytical-proposal/not-applied", initial, rows);
    }

    public static string ToCsv(E4bFinancingComparisonReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("curve,tier,quantity,atomic_route,sequential_route,atomic_warehouses,sequential_warehouses,initial_escrow_installments,sequential_capacity_escrow_installments,atomic_planks,atomic_stone,largest_single_planks,largest_single_stone\n");
        foreach (var row in report.Rows)
        {
            sb.Append(row.CurveId).Append(',').Append(row.Tier).Append(',').Append(row.Quantity).Append(',')
              .Append(row.AtomicWarehouseRoute.Status).Append(',').Append(row.SequentialWarehouseRoute.Status).Append(',')
              .Append(row.AtomicWarehouseRoute.Warehouses).Append(',').Append(row.SequentialWarehouseRoute.Warehouses).Append(',')
              .Append(row.StartingCapacityEscrowInstallments).Append(',').Append(row.SequentialCapacityEscrowInstallments).Append(',')
              .Append(row.AtomicQuote["planks"].Units.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(row.AtomicQuote["stone"].Units.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(row.LargestSingleQuote["planks"].Units.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(row.LargestSingleQuote["stone"].Units.ToString(CultureInfo.InvariantCulture)).Append('\n');
        }
        return sb.ToString();
    }

    public static string ToMarkdown(E4bFinancingComparisonReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("# E4b / RC79 — Opzioni di finanziamento (solo studio)\n\n");
        sb.AppendLine("La costruzione a rate è una **proposta non attivata**: materiali pagati in escrow separato, nessun edificio prima del saldo. La fattibilità di stoccaggio non dimostra disponibilità, tempo o raggiungibilità dei materiali.");
        sb.AppendLine("La via sequenziale richiede il pagamento della singola copia più costosa (storico progressivo), non del lotto atomico. Il percorso Magazzini è quello limitato dello studio RC78, non una prova di assenza di alternative.\n");
        sb.AppendLine("| Curva | Tier | Copie | Lotto atomico | Copie sequenziali | Rate minime con capacità iniziale | Rate minime alla capacità del percorso sequenziale |");
        sb.AppendLine("| --- | --- | ---: | --- | --- | ---: | ---: |");
        foreach (var row in report.Rows)
            sb.Append("| ").Append(row.CurveId).Append(" | ").Append(row.Tier).Append(" | ")
              .Append(row.Quantity).Append(" | ").Append(row.AtomicWarehouseRoute.Status)
              .Append(" (" ).Append(row.AtomicWarehouseRoute.Warehouses).Append(" mag.) | ")
              .Append(row.SequentialWarehouseRoute.Status).Append(" (")
              .Append(row.SequentialWarehouseRoute.Warehouses).Append(" mag.) | ")
              .Append(row.StartingCapacityEscrowInstallments).Append(" | ")
              .Append(row.SequentialCapacityEscrowInstallments).AppendLine(" |");
        sb.AppendLine("\nLimiti: rate pagate con risorse prodotte successivamente, conservazione rigorosa del valore, annullamento/rimborso solo se c'è capienza, persistenza transazionale necessaria. Tempi, ROI, interfaccia, politiche di costruzione e salvataggi non implementati.");
        return sb.ToString();
    }
}
