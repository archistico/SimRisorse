using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Population;
using SimRisorse.Core.Research;

namespace SimRisorse.Core.Sessions;

/// <summary>
/// Isolated v14 one-second simulation adapter over the existing v13 production,
/// population and military planners. A whole Advance is computed on local values;
/// no state is committed until the final checkpoint passes validation.
/// </summary>
internal static class V14CampaignClock
{
    public static V14CampaignSnapshot Advance(GameScenario baseline, V14CampaignSnapshot source,
        SchoolResearchRules schoolRules, TechnologyCatalog technologies,
        TimeSpan elapsed, ResourceBundle capacity)
    {
        var nextTicks = checked(source.ElapsedTicks + elapsed.Ticks);
        var seconds = nextTicks / TimeSpan.TicksPerSecond - source.ElapsedTicks / TimeSpan.TicksPerSecond;
        if (seconds == 0)
            return source with { ElapsedTicks = nextTicks };

        // Production recipes remain defined once per family. Tier sizes change only
        // worker and storage capacity; the existing deterministic recipe order is reused.
        var buildings = source.OtherBuildings.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        foreach (var family in source.ProductiveFamilies)
        {
            var count = checked(family.Base.Buildings + family.Large.Buildings + family.Advanced.Buildings);
            var workers = checked(family.Base.AssignedWorkers + family.Large.AssignedWorkers +
                family.Advanced.AssignedWorkers);
            buildings.Add(family.FamilyId, new BuildingState(family.FamilyId, count, workers));
        }
        var housing = baseline.HousingCapacity(buildings.Values);
        var basicHousing = baseline.BasicHousingCapacity(buildings.Values);
        var growthRules = baseline.PopulationRules!;
        var militaryRules = baseline.MilitaryRules!;
        var barracks = buildings[militaryRules.BarracksId].Count;
        var growthRate = growthRules.HousingGrowth is { } faster
            ? faster.Rate(buildings[faster.SmallHouseId].Count, buildings[faster.LargeHouseId].Count)
            : HousingGrowthRules.UnitsPerSecond;

        var resources = source.Resources;
        var population = source.Population;
        var growthSeconds = source.GrowthProgressSeconds;
        var growthRemainder = source.GrowthRemainderTwentieths;
        var military = source.Military;
        var school = source.SchoolResearch;
        var pencilPaidStudents = source.PencilPaidStudents;
        var ledger = ResearchLedger.Restore(ResearchPoints.FromMilliPoints(source.ResearchBalanceMilliPoints),
            source.UnlockedTechnologyIds, technologies);

        for (long i = 0; i < seconds; i++)
        {
            // Match the existing GameSession's per-second order. School research
            // does not consume food a second time: its enrollment was prepaid.
            var forecast = PopulationPlanner.Plan(growthRules, population, housing, basicHousing,
                growthSeconds, growthRemainder, growthRate, resources);
            var pencilStock = resources[V14SchoolSupplies.ResourceId];
            resources = ProductionPlanner.Plan(baseline.Catalog, resources, capacity, buildings,
                PopulationPlanner.Consumption(forecast)).Resources;
            if (pencilStock != ResourceAmount.Zero)
                resources += ResourceBundle.FromAmounts([
                    KeyValuePair.Create(V14SchoolSupplies.ResourceId, pencilStock)]);
            if (forecast.BlockReason == GrowthBlockReason.None)
            {
                var accrued = (long)growthSeconds * HousingGrowthRules.UnitsPerSecond +
                    growthRemainder + forecast.GrowthRateTwentieths;
                var threshold = (long)forecast.IntervalSeconds * HousingGrowthRules.UnitsPerSecond;
                if (accrued >= threshold)
                {
                    population = checked(population + 1);
                    accrued -= threshold;
                }
                growthSeconds = checked((int)(accrued / HousingGrowthRules.UnitsPerSecond));
                growthRemainder = checked((int)(accrued % HousingGrowthRules.UnitsPerSecond));
            }
            military = MilitaryTraining.Advance(militaryRules, military, barracks, 1);
            var pendingBefore = school.WaitingStudents + school.TrainingStudents;
            var schoolStep = SchoolResearch.Advance(schoolRules, school, ledger, source.SchoolCount, 1);
            school = schoolStep.State;
            var graduates = pendingBefore - (school.WaitingStudents + school.TrainingStudents);
            var legacyBefore = pendingBefore - pencilPaidStudents;
            pencilPaidStudents -= Math.Max(0, graduates - legacyBefore);
            ledger = schoolStep.Ledger;
        }

        return source with
        {
            ElapsedTicks = nextTicks,
            Resources = resources,
            Population = population,
            GrowthProgressSeconds = growthSeconds,
            GrowthRemainderTwentieths = growthRemainder,
            Military = military,
            SchoolResearch = school,
            PencilPaidStudents = pencilPaidStudents,
            ResearchBalanceMilliPoints = ledger.Balance.MilliPoints
        };
    }
}
