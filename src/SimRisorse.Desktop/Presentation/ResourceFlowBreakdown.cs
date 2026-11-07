using System.Globalization;
using SimRisorse.Application.ReadModels;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Desktop.Presentation;

/// <summary>
/// Read-only tooltip projection. Every line describes activity measured for the
/// NEXT second, not a nominal recipe or an estimate of future production.
/// </summary>
public static class ResourceFlowBreakdown
{
    private static readonly CultureInfo Italian = CultureInfo.GetCultureInfo("it-IT");

    public static string Format(string name, string resourceId, decimal total, bool production,
        IReadOnlyList<ResourceFlowContributor>? contributors)
    {
        var heading = production ? "PRODUZIONE" : "CONSUMO";
        var lines = new List<string> { $"{heading} · {name} (prossimo secondo)" };
        var contributed = 0m;
        if (contributors is not null)
        {
            foreach (var item in contributors.OrderBy(item => item.Source, StringComparer.Ordinal))
            {
                var rate = (production ? item.Production : item.Consumption)[resourceId].Units;
                if (rate <= 0m) continue;
                contributed += rate;
                lines.Add($"• {item.Source}: {Number(rate)} /s");
            }
            if (contributed == 0m && total == 0m)
                lines.Add(production ? "Nessun produttore attivo." : "Nessun consumo attivo.");
            // Keep the displayed total authoritative even for an incomplete
            // diagnostic source list. Never silently claim a false sum.
            if (contributed != total)
                lines.Add($"• Flusso non attribuito: {Number(total - contributed)} /s");
        }
        else
            lines.Add("Dettaglio dei contributi non disponibile.");
        lines.Add($"Totale: {Number(total)} /s");
        return string.Join(Environment.NewLine, lines);
    }

    private static string Number(decimal value) => value.ToString("#,0.###", Italian);
    private static string Sites(long count) => $"{count} {(count == 1 ? "edificio" : "edifici")}";
    private static string Active(long count) => $"{count} {(count == 1 ? "unità attiva" : "unità attive")}";

    public static IReadOnlyList<ResourceFlowContributor> FromV14(GameScenario scenario,
        V14CampaignSnapshot snapshot, V14EconomyPreview economy)
    {
        var result = new List<ResourceFlowContributor>();
        foreach (var report in economy.Buildings)
        {
            if (report.RunnableWorkers <= 0 ||
                scenario.Catalog.BuildingsById[report.BuildingId].Recipe is not { } recipe) continue;
            var owned = snapshot.ProductiveFamilies.FirstOrDefault(item => item.FamilyId == report.BuildingId);
            var count = owned is null
                ? snapshot.OtherBuildings.First(item => item.BuildingId == report.BuildingId).Count
                : checked(owned.Base.Buildings + owned.Large.Buildings + owned.Advanced.Buildings);
            var name = scenario.Catalog.BuildingsById[report.BuildingId].Name;
            result.Add(new($"{name} ({Sites(count)}, {Active(report.RunnableWorkers)})",
                recipe.Output.Scale(report.RunnableWorkers), recipe.Input.Scale(report.RunnableWorkers)));
        }
        var population = economy.Population;
        var consumed = new List<KeyValuePair<string, ResourceAmount>>();
        if (population.FoodResourceId is { } food && population.ConsumedFood != ResourceAmount.Zero)
            consumed.Add(KeyValuePair.Create(food, population.ConsumedFood));
        if (population.BreadResourceId is { } bread && population.ConsumedBread != ResourceAmount.Zero)
            consumed.Add(KeyValuePair.Create(bread, population.ConsumedBread));
        if (consumed.Count > 0)
            result.Add(new($"Abitanti ({snapshot.Population})", ResourceBundle.Empty,
                ResourceBundle.FromAmounts(consumed)));
        return result;
    }

    public static IReadOnlyList<ResourceFlowContributor> FromGameView(GameView view)
    {
        var result = new List<ResourceFlowContributor>();
        foreach (var building in view.Buildings)
        {
            if (building.RunnableWorkers <= 0) continue;
            var produced = ResourceBundle.Create(building.OutputPerWorker
                .Select(item => (item.ResourceId, item.Quantity * building.RunnableWorkers)).ToArray());
            var consumed = ResourceBundle.Create(building.InputPerWorker
                .Select(item => (item.ResourceId, item.Quantity * building.RunnableWorkers)).ToArray());
            if (produced.Amounts.IsEmpty && consumed.Amounts.IsEmpty) continue;
            result.Add(new($"{building.Name} ({Sites(building.Count)}, {building.RunnableWorkers} addetti attivi)",
                produced, consumed));
        }
        var meals = new List<(string Id, decimal Units)>();
        if (view.PopulationState.FoodResourceId is { } food && view.PopulationState.ConsumedFoodPerSecond > 0m)
            meals.Add((food, view.PopulationState.ConsumedFoodPerSecond));
        if (view.PopulationState.BreadResourceId is { } bread && view.PopulationState.ConsumedBreadPerSecond > 0m)
            meals.Add((bread, view.PopulationState.ConsumedBreadPerSecond));
        if (meals.Count > 0)
            result.Add(new($"Abitanti ({view.Population})", ResourceBundle.Empty,
                ResourceBundle.Create(meals.ToArray())));
        return result;
    }
}
