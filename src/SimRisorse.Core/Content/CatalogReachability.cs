using System.Collections.Immutable;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Content;

public enum CatalogIssueKind
{
    /// <summary>A prerequisite building can never be constructed.</summary>
    PrerequisiteUnreachable,
    /// <summary>A cost resource is neither renewable nor covered by the initial stock.</summary>
    CostNotObtainable,
    /// <summary>A cost amount exceeds the base capacity and no reachable building expands that resource.</summary>
    CostExceedsStorage,
    /// <summary>The building can be owned, but a recipe input is never renewable.</summary>
    InputNotObtainable,
    /// <summary>The building can be owned, but one worker's input or output exceeds every reachable capacity.</summary>
    RecipeExceedsStorage
}

/// <param name="Subject">The building or resource identifier that causes the issue.</param>
public sealed record CatalogIssue(string BuildingId, CatalogIssueKind Kind, string Subject);

public sealed record ReachabilityReport(ImmutableArray<string> ReachableBuildings, ImmutableArray<string> RenewableResources,
    ImmutableArray<CatalogIssue> Issues)
{
    /// <summary>Every building can be constructed and every recipe can eventually run.</summary>
    public bool IsComplete => Issues.IsEmpty;
}

/// <summary>
/// Static, optimistic check that a scenario has no unlock blocked by a catalog error. It ignores time, people and
/// contention: a positive answer means "possible with enough play time", not "balanced". Initial stocks may pay for
/// a cost but never make a recipe input renewable, so self-feeding cycles that need a starting stock are reported.
/// </summary>
public static class CatalogReachability
{
    public static ReachabilityReport Analyze(GameScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var catalog = scenario.Catalog;
        var owned = scenario.InitialBuildings.Where(item => item.Count > 0).Select(item => item.BuildingId);
        var reachable = new HashSet<string>(owned, StringComparer.Ordinal);
        var renewable = new HashSet<string>(StringComparer.Ordinal);
        var expandable = new HashSet<string>(StringComparer.Ordinal);
        bool Fits(string resource, ResourceAmount amount) =>
            amount.Milliunits <= scenario.Capacity[resource].Milliunits || expandable.Contains(resource);
        bool Payable(KeyValuePair<string, ResourceAmount> cost) =>
            (renewable.Contains(cost.Key) || scenario.InitialResources[cost.Key].Milliunits >= cost.Value.Milliunits) && Fits(cost.Key, cost.Value);
        bool Runs(ProductionRecipe recipe) =>
            recipe.Input.Amounts.All(item => renewable.Contains(item.Key) && Fits(item.Key, item.Value)) &&
            recipe.Output.Amounts.All(item => Fits(item.Key, item.Value));

        for (var changed = true; changed;)
        {
            changed = false;
            foreach (var building in catalog.Buildings)
            {
                if (!reachable.Contains(building.Id) && building.Prerequisites.All(reachable.Contains) && building.Cost.Amounts.All(Payable))
                    changed |= reachable.Add(building.Id);
                if (!reachable.Contains(building.Id)) continue;
                if (building.StoragePerBuilding is { } storage)
                    foreach (var id in storage.Amounts.Keys) changed |= expandable.Add(id);
                if (building.Recipe is { } recipe && Runs(recipe))
                    foreach (var id in recipe.Output.Amounts.Keys) changed |= renewable.Add(id);
            }
        }

        var issues = ImmutableArray.CreateBuilder<CatalogIssue>();
        foreach (var building in catalog.Buildings)
        {
            if (!reachable.Contains(building.Id))
            {
                var prerequisite = building.Prerequisites.FirstOrDefault(id => !reachable.Contains(id));
                if (prerequisite is not null) { issues.Add(new(building.Id, CatalogIssueKind.PrerequisiteUnreachable, prerequisite)); continue; }
                var cost = building.Cost.Amounts.First(item => !Payable(item));
                issues.Add(new(building.Id, Fits(cost.Key, cost.Value) ? CatalogIssueKind.CostNotObtainable : CatalogIssueKind.CostExceedsStorage, cost.Key));
            }
            else if (building.Recipe is { } recipe && !Runs(recipe))
            {
                var input = recipe.Input.Amounts.Where(item => !renewable.Contains(item.Key)).Select(item => item.Key).FirstOrDefault();
                issues.Add(input is not null
                    ? new(building.Id, CatalogIssueKind.InputNotObtainable, input)
                    : new(building.Id, CatalogIssueKind.RecipeExceedsStorage,
                        recipe.Input.Amounts.Concat(recipe.Output.Amounts).First(item => !Fits(item.Key, item.Value)).Key));
            }
        }
        return new(
            [.. catalog.Buildings.Select(item => item.Id).Where(reachable.Contains)],
            [.. catalog.Resources.Select(item => item.Id).Where(renewable.Contains)],
            issues.ToImmutable());
    }
}
