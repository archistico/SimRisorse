using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Population;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Core.Food;

/// <summary>
/// RC51 / F1-D05d.3b. One deterministic, transactional clock for an experimental
/// v15 campaign. The retired food-store is not inserted into the building
/// roster; its historical fish->food recipe is NEVER run. An explicitly
/// imported, still-occupied historical producer is preserved but suppressed.
/// All ready food and bread rations are owned and consumed by the new depot.
/// Legacy industry, housing, individual military and school research share this tick.
/// </summary>
public sealed record V15CampaignAdvancePlan(V15FoodCampaignCheckpoint Next,
    long SecondsAdvanced, long MealsServed, long MealsMissed,
    ResourceAmount ReadyFoodProduced,
    ResourceBundle FirstSecondProduction, ResourceBundle FirstSecondConsumption,
    IReadOnlyList<BuildingProduction>? FirstSecondBuildings = null,
    IReadOnlyList<ResourceFlowContributor>? FirstSecondContributors = null);

public static class V15UnifiedCampaignClock
{
    public const string DisabledLegacyProducerId = V15LegacyProducerAdapter.RetiredFamilyId;
    private static readonly IReadOnlySet<string> DisabledLegacyProducers =
        new HashSet<string>(StringComparer.Ordinal) { DisabledLegacyProducerId };

    public static V15CampaignAdvancePlan Plan(
        V15FoodCampaignCheckpoint start, GameScenario baseline,
        SchoolResearchRules schoolRules, TechnologyCatalog technologies,
        FoodIndustryConstructionCatalog foodCatalog, MeatChainPolicy meatPolicy,
        HorticulturePolicy cropsPolicy, FoodConversionPolicy depotPolicy,
        TimeSpan elapsed, bool includeProductionDiagnostics = false)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(schoolRules);
        ArgumentNullException.ThrowIfNull(technologies);
        ArgumentNullException.ThrowIfNull(foodCatalog);
        ArgumentNullException.ThrowIfNull(meatPolicy);
        ArgumentNullException.ThrowIfNull(cropsPolicy);
        ArgumentNullException.ThrowIfNull(depotPolicy);
        if (elapsed < TimeSpan.Zero || elapsed > GameSession.MaximumAdvance)
            throw new ArgumentOutOfRangeException(nameof(elapsed));
        foodCatalog.ValidateAgainst(baseline);
        FoodIndustryTickPlanner.ValidatePolicies(meatPolicy, cropsPolicy, depotPolicy);
        V15FoodCampaignCheckpoints.Validate(start, baseline, schoolRules, technologies, depotPolicy);
        if (baseline.PopulationRules is not { } populationRules ||
            baseline.MilitaryRules is not { } militaryRules ||
            populationRules.FoodResourceId != depotPolicy.ReadyFoodResourceId ||
            populationRules.AdvancedHousing?.BreadResourceId != depotPolicy.BreadResourceId)
            throw new ArgumentException("Population must use the canonical depot food and bread.", nameof(baseline));
        // Guard against another legacy recipe reintroducing ready food from the city.
        if (baseline.Catalog.Buildings.Any(b =>
            b.Recipe is { } recipe && recipe.Output[depotPolicy.ReadyFoodResourceId] != ResourceAmount.Zero &&
            b.Id != DisabledLegacyProducerId))
            throw new ArgumentException("A second city producer of ready food is not allowed.", nameof(baseline));

        var nextTicks = checked(start.CityCampaign.ElapsedTicks + elapsed.Ticks);
        var wholeSeconds = nextTicks / TimeSpan.TicksPerSecond -
            start.CityCampaign.ElapsedTicks / TimeSpan.TicksPerSecond;
        if (wholeSeconds == 0)
            return new(start with { CityCampaign = start.CityCampaign with { ElapsedTicks = nextTicks } },
                0, 0, 0, ResourceAmount.Zero, ResourceBundle.Empty, ResourceBundle.Empty);

        var city = start.CityCampaign;
        var buildings = city.OtherBuildings.ToDictionary(b => b.BuildingId, StringComparer.Ordinal);
        var crewSizes = new Dictionary<string, int>(StringComparer.Ordinal);
        var staffedModules = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var family in city.ProductiveFamilies)
        {
            var crewSize = V15CrewRules.CrewSize(start.CrewMode, family.FamilyId);
            var count = crewSize == 1
                ? checked(family.Base.Buildings + family.Large.Buildings + family.Advanced.Buildings)
                : checked(checked(family.Base.Buildings + checked(3 * family.Large.Buildings)) +
                    checked(10 * family.Advanced.Buildings));
            buildings.Add(family.FamilyId, new BuildingState(family.FamilyId, count,
                checked(family.Base.AssignedWorkers + family.Large.AssignedWorkers + family.Advanced.AssignedWorkers)));
            if (crewSize > 1)
            {
                crewSizes.Add(family.FamilyId, crewSize);
                staffedModules.Add(family.FamilyId, V15CrewRules.StaffedModules(family, crewSize));
            }
        }
        // No artificial food-store slot: ProductionPlanner skips explicitly
        // suppressed producers before looking up building state. A genuinely
        // imported occupied legacy producer is kept, but cannot run its recipe.
        var housingCapacity = baseline.HousingCapacity(buildings.Values);
        var basicHousing = baseline.BasicHousingCapacity(buildings.Values);
        var barracks = buildings[militaryRules.BarracksId].Count;
        var growthRate = V15CampaignGrowthProjection.GrowthRate(populationRules.HousingGrowth, buildings);
        var capacity = V15CityStorage.Calculate(baseline, city);
        var foodCapacities = foodCatalog.CapacitiesFor(start.Industry.Buildings);
        var personnel = start.Industry.Buildings.PlannerPersonnel(city.Population,
            V15FoodCampaignCheckpoints.BusyOutsideFood(city));

        var resources = city.Resources;
        var animals = start.Industry.Animals;
        var transit = start.Industry.Transit;
        var depot = start.Depot;
        var population = city.Population;
        var growthSeconds = city.GrowthProgressSeconds;
        var growthRemainder = city.GrowthRemainderTwentieths;
        var military = city.Military;
        var school = city.SchoolResearch;
        var pencilPaidStudents = city.PencilPaidStudents;
        var ledger = ResearchLedger.Restore(ResearchPoints.FromMilliPoints(city.ResearchBalanceMilliPoints),
            city.UnlockedTechnologyIds, technologies);
        long mealsServed = 0;
        long mealsMissed = 0;
        var producedReadyFood = ResourceAmount.Zero;
        // Gross flows for the first complete second, recorded at the same execution
        // sites as the real clock. This is a measurement, not an approximate
        // reconstruction from the final inventory (deliveries are transfers).
        var firstProduced = ResourceBundle.Empty;
        var firstConsumed = ResourceBundle.Empty;
        IReadOnlyList<BuildingProduction>? firstBuildings = null;
        // Gather the per-source ledger at the SAME recipe/meal boundaries as the
        // gross totals. This is diagnostic data, never an independent forecast.
        var sourceFlows = new Dictionary<string, ResourceFlowContributor>(StringComparer.Ordinal);
        static string SiteCount(long count) => $"{count} {(count == 1 ? "edificio" : "edifici")}";
        static string ActiveModules(long count) => $"{count} {(count == 1 ? "unità attiva" : "unità attive")}";
        void RecordSource(string key, string label, ResourceBundle production, ResourceBundle consumption)
        {
            if (production.Amounts.IsEmpty && consumption.Amounts.IsEmpty) return;
            if (sourceFlows.TryGetValue(key, out var previous))
                sourceFlows[key] = previous with
                {
                    Production = previous.Production + production,
                    Consumption = previous.Consumption + consumption
                };
            else sourceFlows.Add(key, new ResourceFlowContributor(label, production, consumption));
        }


        for (long i = 0; i < wholeSeconds; i++)
        {
            // Produce the active historical industries only. Fish, grain, water,
            // flour and bread enter the city; food-store cannot run and need
            // not have an entry in this roster.
            var pencilStock = resources[V14SchoolSupplies.ResourceId];
            var industry = ProductionPlanner.Plan(baseline.Catalog, resources, capacity, buildings,
                includeDiagnostics: includeProductionDiagnostics && i == 0,
                suppressedProducerIds: DisabledLegacyProducers, crewSizes: crewSizes,
                staffedModules: staffedModules);
            resources = industry.Resources;
            if (i == 0)
            {
                if (includeProductionDiagnostics) firstBuildings = industry.Buildings;
                foreach (var report in industry.Buildings)
                {
                    if (report.RunnableWorkers == 0 ||
                        baseline.Catalog.BuildingsById[report.BuildingId].Recipe is not { } recipe)
                        continue;
                    var production = recipe.Output.Scale(report.RunnableWorkers);
                    var consumption = recipe.Input.Scale(report.RunnableWorkers);
                    firstProduced += production;
                    firstConsumed += consumption;
                    var family = city.ProductiveFamilies.FirstOrDefault(item => item.FamilyId == report.BuildingId);
                    var owned = family is null ? buildings[report.BuildingId].Count :
                        checked(family.Base.Buildings + family.Large.Buildings + family.Advanced.Buildings);
                    var name = baseline.Catalog.BuildingsById[report.BuildingId].Name;
                    RecordSource("city:" + report.BuildingId,
                        $"{name} ({SiteCount(owned)}, {ActiveModules(report.RunnableWorkers)})",
                        production, consumption);
                }
            }
            if (pencilStock != ResourceAmount.Zero)
                resources += ResourceBundle.FromAmounts([
                    KeyValuePair.Create(V14SchoolSupplies.ResourceId, pencilStock)]);

            // All new food supply stages, processing and meals share the same second.
            // Population consumes whole ready-food and bread rations, or neither.
            var advancedResidents = Math.Max(0, population - basicHousing);
            var ration = populationRules.FoodPerPersonPerSecond * population;
            var breadRation = populationRules.AdvancedHousing is { } advanced
                ? advanced.BreadPerPersonPerSecond * advancedResidents : ResourceAmount.Zero;
            // Protect enough bread for a possible arriving advanced resident as well.
            var arrivalBreadReserve = populationRules.AdvancedHousing is { } housing
                ? housing.BreadPerPersonPerSecond * Math.Max(0, population + (population < housingCapacity ? 1 : 0) - basicHousing)
                : ResourceAmount.Zero;
            var tick = FoodIndustryTickPlanner.PlanTick(meatPolicy, cropsPolicy, depotPolicy,
                new FoodIndustryStocks(resources, animals, transit, depot), foodCapacities,
                personnel, new FoodIndustryMeals(arrivalBreadReserve, ration, breadRation));
            resources = tick.Next.City;
            animals = tick.Next.Animals;
            transit = tick.Next.Transit;
            depot = tick.Next.Depot;
            producedReadyFood += tick.Depot.Processing.ProducedFood;
            if (i == 0)
            {
                // Physical births/harvests and actual recipe inputs only. Depot
                // receipts and animal/transit transfers have no gross flow.
                firstProduced += tick.Meat.Produced + tick.Horticulture.Produced;
                firstConsumed += tick.Meat.Consumed + tick.Horticulture.Consumed;
                firstProduced += ResourceBundle.FromAmounts([
                    KeyValuePair.Create(depotPolicy.ReadyFoodResourceId,
                        tick.Depot.Processing.ProducedFood)]);
                firstConsumed += tick.Depot.Processing.ConsumedIngredients;
                var mealConsumption = ResourceBundle.FromAmounts([
                    KeyValuePair.Create(depotPolicy.ReadyFoodResourceId,
                        tick.Depot.Meal.ConsumedReadyFood),
                    KeyValuePair.Create(depotPolicy.BreadResourceId,
                        tick.Depot.Meal.ConsumedBread)]);
                firstConsumed += mealConsumption;
                foreach (var run in tick.Meat.Runs)
                {
                    var siteId = run.Stage switch
                    {
                        MeatChainStage.Stable => "stable",
                        MeatChainStage.Hunting => "hunter",
                        _ => "butcher"
                    };
                    var site = foodCatalog.Sites.First(item => item.Id == siteId);
                    var count = FoodIndustrySites.Get(start.Industry.Buildings.Counts, siteId);
                    RecordSource("food:" + siteId, $"{site.Name} ({SiteCount(count)})", run.Outputs, run.Inputs);
                }
                foreach (var run in tick.Horticulture.Runs)
                {
                    var siteId = run.CropResourceId == cropsPolicy.VegetablesResourceId ? "garden" : "orchard";
                    var site = foodCatalog.Sites.First(item => item.Id == siteId);
                    var count = FoodIndustrySites.Get(start.Industry.Buildings.Counts, siteId);
                    RecordSource("food:" + siteId, $"{site.Name} ({SiteCount(count)})", run.Outputs, run.Inputs);
                }
                var depotSite = foodCatalog.Sites.First(item => item.Id == "food-depot");
                var depotCount = start.Industry.Buildings.Counts.Depot +
                    start.Industry.Buildings.Counts.DepotLarge +
                    start.Industry.Buildings.Counts.DepotAdvanced;
                RecordSource("food:food-depot", $"{depotSite.Name} ({SiteCount(depotCount)})",
                    ResourceBundle.FromAmounts([
                        KeyValuePair.Create(depotPolicy.ReadyFoodResourceId, tick.Depot.Processing.ProducedFood)]),
                    tick.Depot.Processing.ConsumedIngredients);
                RecordSource("people:meal", $"Abitanti ({population})", ResourceBundle.Empty, mealConsumption);
            }
            if (tick.Depot.Meal.Served) mealsServed++;
            else mealsMissed++;

            // Forecast against the stock BEFORE the atomic meal (which has already
            // been deducted once by the depot). No v14 city food deduction is made.
            var beforeMeal = tick.Depot.Processing.NextStock;
            var edible = ResourceBundle.FromAmounts([
                KeyValuePair.Create(depotPolicy.ReadyFoodResourceId, beforeMeal.ReadyFood),
                KeyValuePair.Create(depotPolicy.BreadResourceId,
                    beforeMeal.RawFood[depotPolicy.BreadResourceId])]);
            var growth = PopulationPlanner.Plan(populationRules, population,
                housingCapacity, basicHousing, growthSeconds, growthRemainder, growthRate, edible);
            if (tick.Depot.Meal.Served && growth.BlockReason == GrowthBlockReason.None)
            {
                var accrued = (long)growthSeconds * HousingGrowthRules.UnitsPerSecond +
                    growthRemainder + growth.GrowthRateTwentieths;
                var threshold = (long)growth.IntervalSeconds * HousingGrowthRules.UnitsPerSecond;
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
            var schoolStep = SchoolResearch.Advance(schoolRules, school, ledger, city.SchoolCount, 1);
            school = schoolStep.State;
            var graduates = pendingBefore - (school.WaitingStudents + school.TrainingStudents);
            var legacyBefore = pendingBefore - pencilPaidStudents;
            pencilPaidStudents -= Math.Max(0, graduates - legacyBefore);
            ledger = schoolStep.Ledger;
        }

        var finalCity = city with
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
        var next = start with
        {
            CityCampaign = finalCity,
            Depot = depot,
            Industry = start.Industry with { Animals = animals, Transit = transit }
        };
        V15FoodCampaignCheckpoints.Validate(next, baseline, schoolRules, technologies, depotPolicy);
        return new(next, wholeSeconds, mealsServed, mealsMissed, producedReadyFood,
            firstProduced, firstConsumed, firstBuildings, sourceFlows.Values.ToArray());
    }

}
