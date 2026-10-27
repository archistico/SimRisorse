using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;

namespace SimRisorse.Core.Economy;

[Flags]
public enum ProductionBlockReason { None = 0, NotConstructed = 1, NoWorkers = 2, MissingInputs = 4, StorageFull = 8, NotProducer = 16 }

public sealed record BuildingProduction(string BuildingId, int AssignedWorkers, int RunnableWorkers, ProductionBlockReason BlockReason,
    ImmutableArray<string> MissingInputIds, ImmutableArray<string> FullOutputIds);
internal sealed record ProductionPlan(ResourceBundle Resources, ImmutableArray<BuildingProduction> Buildings);

internal static class ProductionPlanner
{
    public static ProductionPlan Plan(GameCatalog catalog, ResourceBundle resources, ResourceBundle capacity,
        IReadOnlyDictionary<string, BuildingState> buildings, ResourceBundle? reservedConsumption = null,
        bool includeDiagnostics = false)
    {
        var remaining = reservedConsumption is null ? resources : resources - reservedConsumption;
        var available = catalog.Resources.ToDictionary(item => item.Id, item => remaining[item.Id].Milliunits, StringComparer.Ordinal);
        var space = catalog.Resources.ToDictionary(item => item.Id, item => capacity[item.Id].Milliunits - resources[item.Id].Milliunits, StringComparer.Ordinal);
        var output = catalog.Resources.ToDictionary(item => item.Id, _ => 0L, StringComparer.Ordinal);
        var reports = ImmutableArray.CreateBuilder<BuildingProduction>(catalog.Buildings.Length);
        foreach (var definition in catalog.Buildings)
        {
            var state = buildings[definition.Id];
            var reason = ProductionBlockReason.None;
            // The second-by-second simulation does not need presentation diagnostics.
            var missingInputs = includeDiagnostics ? ImmutableArray.CreateBuilder<string>() : null;
            var fullOutputs = includeDiagnostics ? ImmutableArray.CreateBuilder<string>() : null;
            long runnable = state.AssignedWorkers;
            if (state.Count == 0) reason |= ProductionBlockReason.NotConstructed;
            if (state.AssignedWorkers == 0) reason |= ProductionBlockReason.NoWorkers;
            if (definition.Recipe is not { } recipe)
            {
                reports.Add(new(definition.Id, state.AssignedWorkers, 0, reason | ProductionBlockReason.NotProducer,
                    ImmutableArray<string>.Empty, ImmutableArray<string>.Empty));
                continue;
            }
            foreach (var (id, amount) in recipe.Input.Amounts)
            {
                var supported = available[id] / amount.Milliunits;
                if (supported < state.AssignedWorkers)
                {
                    reason |= ProductionBlockReason.MissingInputs;
                    missingInputs?.Add(id);
                }
                runnable = Math.Min(runnable, supported);
            }
            foreach (var (id, amount) in recipe.Output.Amounts)
            {
                var supported = space[id] / amount.Milliunits;
                if (supported < state.AssignedWorkers)
                {
                    reason |= ProductionBlockReason.StorageFull;
                    fullOutputs?.Add(id);
                }
                runnable = Math.Min(runnable, supported);
            }
            foreach (var (id, amount) in recipe.Input.Amounts) available[id] -= checked(amount.Milliunits * runnable);
            foreach (var (id, amount) in recipe.Output.Amounts)
            {
                var produced = checked(amount.Milliunits * runnable);
                space[id] -= produced;
                output[id] += produced;
            }
            reports.Add(new(definition.Id, state.AssignedWorkers, checked((int)runnable), reason,
                missingInputs?.ToImmutable() ?? ImmutableArray<string>.Empty,
                fullOutputs?.ToImmutable() ?? ImmutableArray<string>.Empty));
        }
        var stocks = ResourceBundle.FromAmounts(catalog.Resources.Select(item =>
            KeyValuePair.Create(item.Id, ResourceAmount.FromMilliunits(checked(available[item.Id] + output[item.Id])))));
        return new(stocks, reports.MoveToImmutable());
    }
}
