using System.Collections.Immutable;
using SimRisorse.Core.Sessions;
using SimRisorse.Core.Economy;

namespace SimRisorse.Application.ReadModels;

internal static class GameViewProjector
{
    public static GameView Project(GameSession session)
    {
        var snapshot = session.Snapshot();
        var scenario = session.Scenario;
        var buildings = snapshot.Buildings.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        var economy = session.ForecastEconomy();
        var population = economy.Population;
        var forecast = economy.Buildings.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        var capacity = session.StorageCapacity;
        ImmutableArray<ResourceLine> Lines(ResourceBundle bundle) => bundle.Amounts.Select(item =>
            new ResourceLine(item.Key, scenario.Catalog.ResourcesById[item.Key].Name, item.Value.Units)).ToImmutableArray();
        ImmutableArray<string> Names(ImmutableArray<string> ids) => ids.Select(id => scenario.Catalog.ResourcesById[id].Name).ToImmutableArray();
        decimal Flow(string resourceId, bool output) => scenario.Catalog.Buildings.Sum(item =>
            (item.Recipe is null ? 0m : (output ? item.Recipe.Output : item.Recipe.Input)[resourceId].Units) * forecast[item.Id].RunnableWorkers);
        var resourceViews = scenario.Catalog.Resources.Select(item => new ResourceView(item.Id, item.Name,
                snapshot.Resources[item.Id].Units, capacity[item.Id].Units, Flow(item.Id, true),
                Flow(item.Id, false) + (item.Id == population.FoodResourceId ? population.ConsumedFood.Units : 0m) +
                    (item.Id == population.BreadResourceId ? population.ConsumedBread.Units : 0m))).ToImmutableArray();
        var buildingViews = scenario.Catalog.Buildings.Select(item => new BuildingView(item.Id, item.Name, buildings[item.Id].Count,
                buildings[item.Id].AssignedWorkers, (long)buildings[item.Id].Count * item.WorkplacesPerBuilding,
                item.AllocationPriority, forecast[item.Id].RunnableWorkers, forecast[item.Id].BlockReason,
                Lines(item.Cost), Lines(item.Recipe?.Input ?? ResourceBundle.Empty), Lines(item.Recipe?.Output ?? ResourceBundle.Empty),
                item.Prerequisites.Where(id => buildings[id].Count == 0).Select(id => scenario.Catalog.BuildingsById[id].Name).ToImmutableArray(),
                item.HousingPerBuilding, Lines(item.StoragePerBuilding ?? ResourceBundle.Empty),
                Names(forecast[item.Id].MissingInputIds), Names(forecast[item.Id].FullOutputIds))).ToImmutableArray();
        return new(scenario.Id, scenario.Catalog.RulesVersion, session.ElapsedTicks, session.IsPaused,
            session.Population, session.AvailableWorkers, resourceViews, buildingViews,
            new(population.HousingCapacity, population.ProgressSeconds, population.IntervalSeconds, population.FoodResourceId,
                population.RequiredFood.Units, population.ConsumedFood.Units, population.BlockReason,
                population.BasicHousingCapacity, population.AdvancedResidents, population.BreadResourceId,
                population.RequiredBread.Units, population.ConsumedBread.Units,
                population.GrowthRemainderTwentieths, population.GrowthRateTwentieths),
            BreadSupplyProjector.Project(session, population, resourceViews, buildingViews),
            ProjectMilitary(session, snapshot));
    }

    private static MilitaryView? ProjectMilitary(GameSession session, GameSnapshot snapshot)
    {
        if (session.Scenario.MilitaryRules is not { } rules || session.Military is not { } military) return null;
        var employed = checked((int)snapshot.Buildings.Sum(item => (long)item.AssignedWorkers));
        var places = session.TrainingPlaces;
        var equipped = session.Resources[rules.SwordsResourceId].Milliunits / rules.SwordsPerRecruit.Milliunits;
        var maximum = places == 0 ? 0 : (int)Math.Min(session.AvailableWorkers, equipped);
        var space = session.StorageCapacity[rules.SwordsResourceId].Milliunits - session.Resources[rules.SwordsResourceId].Milliunits;
        var cancellable = (int)Math.Min(military.Recruits, Math.Max(0, space) / rules.SwordsPerRecruit.Milliunits);
        var remaining = military.TrainingProgressSeconds.Select(progress => rules.TrainingSeconds - progress).ToImmutableArray();
        return new(rules.BarracksBuildingId, rules.SwordsResourceId, session.Scenario.Catalog.ResourcesById[rules.SwordsResourceId].Name,
            rules.SwordsPerRecruit.Units, rules.TrainingSeconds, rules.RecruitsPerBarracks, places,
            military.QueuedRecruits, remaining, military.Soldiers, employed, maximum, cancellable)
        {
            SecondsUntilAllTrained = SecondsUntilAllTrained(remaining, military.QueuedRecruits, rules.TrainingSeconds)
        };
    }

    /// <summary>
    /// Every place is busy while recruits wait, and a freed place starts the next recruit at once. Remaining times are at
    /// most one period, so the queue is served in whole waves: the k-th waiting recruit (0-based) uses the place that
    /// frees (k mod places)-th soonest, k / places periods later.
    /// </summary>
    private static long SecondsUntilAllTrained(ImmutableArray<int> remaining, int queued, int period)
    {
        if (remaining.IsEmpty) return 0;
        if (queued == 0) return remaining.Max();
        var sorted = remaining.Sort();
        var last = queued - 1L;
        return sorted[(int)(last % sorted.Length)] + (last / sorted.Length + 1) * period;
    }
}
