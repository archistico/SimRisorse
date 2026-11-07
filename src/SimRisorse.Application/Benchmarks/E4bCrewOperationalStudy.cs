using System.Globalization;
using System.Text;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;

namespace SimRisorse.Application.Benchmarks;

public sealed record E4bCrewOperationalRow(string Scenario, ProductionTier Tier,
    int Buildings, int AssignedWorkers, int OwnedModules, int StaffedModules,
    int RunnableModules, int IncompleteCrewWorkers, int MissingForNextModule,
    ProductionBlockReason BlockReason, ResourceBundle Consumed, ResourceBundle Produced);

public sealed record E4bCrewOperationalReport(string Version, string Mode,
    string CatalogRulesVersion, int CrewSize, IReadOnlyList<E4bCrewOperationalRow> Rows);

/// <summary>
/// RC80: controlled counterfactual using the live mint recipe and the production
/// planner itself, but an isolated 10-person crew portfolio. This is NOT a
/// v15 natural run: resources, storage and available population are declared
/// laboratory inputs, rather than earned through the game economy.
/// </summary>
public sealed class E4bCrewOperationalStudy
{
    public const string Version = "e4b-crew-10-projection-1";

    public E4bCrewOperationalReport Run()
    {
        var baseline = V15BenchmarkFixture.Create().Baseline;
        var catalog = baseline.Catalog;
        var standard = baseline.InitialBuildings.Where(b => b.BuildingId != "mint")
            .Select(b => b with { AssignedWorkers = 0 }).ToArray();
        var stock = ResourceBundle.Create(("gold", 20m), ("coal", 20m));
        var capacity = ResourceBundle.FromAmounts(catalog.Resources.Select(r =>
            KeyValuePair.Create(r.Id, ResourceAmount.FromUnits(1000m))));
        var rows = new List<E4bCrewOperationalRow>();

        void Measure(string label, ProductionTier tier, int buildings, int workers,
            ResourceBundle? alternativeStock = null, ResourceBundle? alternativeCapacity = null)
        {
            var portfolio = tier switch
            {
                ProductionTier.Base => CrewTeamPortfolio.Create("mint", 10, new(buildings, workers)),
                ProductionTier.Large => CrewTeamPortfolio.Create("mint", 10, default, new(buildings, workers)),
                ProductionTier.Advanced => CrewTeamPortfolio.Create("mint", 10, default, default, new(buildings, workers)),
                _ => throw new ArgumentOutOfRangeException(nameof(tier))
            };
            var tick = CrewProductionProjection.SimulateSecond(catalog,
                alternativeStock ?? stock, alternativeCapacity ?? capacity,
                standard, [portfolio], residents: 200);
            var status = tick.CrewStatus.Single();
            var mint = tick.Production.Single(p => p.BuildingId == "mint");
            rows.Add(new(label, tier, buildings, workers, status.OwnedModules,
                status.StaffedModules, status.RunnableModules, status.IncompleteCrewWorkers,
                status.MissingForNextModule, mint.BlockReason, status.Consumed, status.Produced));
        }

        foreach (var staff in new[] { 0, 9, 10, 19, 20 })
            Measure($"two-base-{staff}", ProductionTier.Base, 2, staff);
        Measure("large-29", ProductionTier.Large, 1, 29);
        Measure("large-30", ProductionTier.Large, 1, 30);
        Measure("advanced-100", ProductionTier.Advanced, 1, 100);
        Measure("mint-no-gold", ProductionTier.Base, 1, 10,
            ResourceBundle.Create(("coal", 20m)));
        Measure("coin-storage-full", ProductionTier.Base, 1, 10,
            alternativeCapacity: ResourceBundle.FromAmounts(catalog.Resources.Select(r =>
                KeyValuePair.Create(r.Id, ResourceAmount.FromUnits(r.Id == "coins" ? 2m : 1000m)))));
        return new(Version, "controlled-projection/not-applied", catalog.RulesVersion, 10, rows);
    }

    public static string ToCsv(E4bCrewOperationalReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("scenario,tier,buildings,assigned,modules,staffed,runnable,incomplete,missing_next,reason,gold_consumed,coal_consumed,coins_produced\n");
        foreach (var row in report.Rows)
            sb.Append(row.Scenario).Append(',').Append(row.Tier).Append(',').Append(row.Buildings).Append(',')
              .Append(row.AssignedWorkers).Append(',').Append(row.OwnedModules).Append(',')
              .Append(row.StaffedModules).Append(',').Append(row.RunnableModules).Append(',')
              .Append(row.IncompleteCrewWorkers).Append(',').Append(row.MissingForNextModule).Append(',')
              .Append(row.BlockReason.ToString().Replace(", ", "+", StringComparison.Ordinal)).Append(',')
              .Append(row.Consumed["gold"].Units.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(row.Consumed["coal"].Units.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(row.Produced["coins"].Units.ToString(CultureInfo.InvariantCulture)).Append('\n');
        return sb.ToString();
    }

    public static string ToMarkdown(E4bCrewOperationalReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var sb = new StringBuilder("# RC80 — Zecca con squadre minime (proiezione, NON partita)\n\n");
        sb.AppendLine("Misura controllata su un solo tick produttivo di un secondo: ricetta attuale della Zecca, 10 abitanti per modulo, tier da 1/3/10 moduli. Oro e Carbone sono concessi nella fixture, non ottenuti tramite la filiera.");
        sb.AppendLine("I 200 residenti sono un budget sintetico, NON un risultato demografico. Nessun prezzo, salvataggio, GUI, clock v15 o bilanciamento è stato modificato.\n");
        sb.AppendLine("| Scenario | Tier | Zecche | Addetti | Moduli attivi | Squadre incomplete (persone) | Mancano per prossimo modulo | Monete/s | Vincoli |");
        sb.AppendLine("| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |");
        foreach (var row in report.Rows)
            sb.Append("| ").Append(row.Scenario).Append(" | ").Append(row.Tier).Append(" | ")
              .Append(row.Buildings).Append(" | ").Append(row.AssignedWorkers).Append(" | ")
              .Append(row.RunnableModules).Append(" | ").Append(row.IncompleteCrewWorkers).Append(" | ")
              .Append(row.MissingForNextModule).Append(" | ")
              .Append(row.Produced["coins"].Units.ToString(CultureInfo.InvariantCulture)).Append(" | ")
              .Append(row.BlockReason).AppendLine(" |");
        sb.AppendLine("\nResta da verificare: budget abitanti realmente disponibili, food clock, tier & persistenza, gestione delle assunzioni/upgrade, pagamento e risorse ottenute naturalmente, scenario economico A/B/C. **Non** è una prova di progresso giocabile.");
        return sb.ToString();
    }
}
