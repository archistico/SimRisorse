using System.Globalization;
using System.Text;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;

namespace SimRisorse.Application.Benchmarks;

public sealed record E4bCurveAFundingRow(ProductionTier Tier, int PurchaseIndex,
    ResourceBundle FrozenQuote, long RequiredPayments, ResourceBundle Paid,
    ResourceBundle StockAfter, int AcquiredMintsAfter, bool Reconciles);

public sealed record E4bCurveAFundingReport(string Version, string Mode,
    string InputProvenance, ResourceBundle StartingPhysicalCapacity,
    IReadOnlyList<E4bCurveAFundingRow> Rows, string ApprovalStatus);

/// <summary>Finances six controlled purchase demonstrations. Each replenishment
/// is EXPLICITLY hypothetical. No natural resource output or game completion
/// is claimed, even when the escrow has been settled in this sandbox.</summary>
public static class E4bCurveAFundingDemonstration
{
    public const string Version = "rc92-curve-a-funding-1";

    public static E4bCurveAFundingReport Run()
    {
        var fixture = V15BenchmarkFixture.Create();
        var capacity = V15CityStorage.Calculate(fixture.Baseline, fixture.Start.CityCampaign);
        var rows = new List<E4bCurveAFundingRow>();
        foreach (var tier in new[] { ProductionTier.Base, ProductionTier.Large, ProductionTier.Advanced })
        {
            // Fresh sandbox per tier: the two purchases share one family counter.
            var state = E4bCurveAFundingSandbox.Open(ResourceBundle.Empty, capacity);
            for (var copy = 1; copy <= 2; copy++)
            {
                var quote = state.QuoteNextMint(tier);
                var count = state.RequiredPayments(tier);
                if (count > 100000)
                    throw new InvalidOperationException("Demonstration requires too many hypothetical transfers.");
                if (!state.TryBeginMint(tier, out var started))
                    throw new InvalidOperationException("Unable to begin the frozen mint quotation.");
                state = started;
                for (long i = 1; i <= count; i++)
                {
                    var payment = ProgressiveConstructionEscrow.Installment(quote, count, i);
                    if (!state.TryDeliverHypothetical(payment, out var supplied) ||
                        !supplied.TryDeposit(payment, out var funded))
                        throw new InvalidOperationException("A hypothetical installment does not fit storage.");
                    state = funded;
                    if (!state.Reconciles())
                        throw new InvalidOperationException("Conservation failed while funding a mint.");
                }
                var paid = state.Escrowed;
                if (!state.TryComplete(out var complete) || !complete.Reconciles() ||
                    !paid.Equals(quote) || complete.AcquiredMints != copy)
                    throw new InvalidOperationException("Mint escrow completion did not reconcile.");
                state = complete;
                rows.Add(new(tier, copy, quote, count, paid,
                    state.Stock, state.AcquiredMints, state.Reconciles()));
            }
        }
        return new(Version, E4bCurveAFundingSandbox.Mode,
            "hypothetical-material-deliveries/no-natural-clock/no-live-prices",
            capacity, rows, "not-approved/runtime-persistence-and-natural-progression-pending");
    }

    public static string ToCsv(E4bCurveAFundingReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("tier,purchase_index,quote,required_payments,paid,acquired_after,reconciles\n");
        foreach (var row in report.Rows)
            sb.Append(row.Tier).Append(',').Append(row.PurchaseIndex).Append(',')
              .Append(E4bCurveStudyExport.Materials(row.FrozenQuote)).Append(',')
              .Append(row.RequiredPayments.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(E4bCurveStudyExport.Materials(row.Paid)).Append(',')
              .Append(row.AcquiredMintsAfter).Append(',')
              .Append(row.Reconciles ? "true" : "false").Append('\n');
        return sb.ToString();
    }

    public static string ToMarkdown(E4bCurveAFundingReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("# RC92 — Curva A, deposito progressivo sperimentale\n\n");
        sb.AppendLine("**Non attivo in partita.** Pagamenti e rifornimenti di questo report sono ipotetici e non costituiscono prove della progressione naturale.");
        sb.AppendLine("Nessun edificio, posto di lavoro o capacità è concesso nel motore v15. Nessun salvataggio è cambiato.");
        sb.AppendLine("\n| Livello | Copia | Preventivo bloccato | Versamenti | Contatore dopo | Conservazione |\n|---|---:|---|---:|---:|---|");
        foreach (var row in report.Rows)
            sb.Append("| ").Append(row.Tier).Append(" | ").Append(row.PurchaseIndex)
              .Append(" | ").Append(E4bCurveStudyExport.Materials(row.FrozenQuote))
              .Append(" | ").Append(row.RequiredPayments).Append(" | ")
              .Append(row.AcquiredMintsAfter).Append(" | ")
              .Append(row.Reconciles ? "ok" : "errore").AppendLine(" |");
        sb.AppendLine("\n**Prima dell'attivazione reale:** servono persistenza transazionale del deposito, regole di annullamento, integrazione con prerequisiti, clock, acquisti e test naturali con i nuovi prezzi.");
        return sb.ToString();
    }
}
