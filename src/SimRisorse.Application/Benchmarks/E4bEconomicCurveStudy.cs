using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;

namespace SimRisorse.Application.Benchmarks;

public enum E4bCurveKind { Epoch, DepthWithMaterials, Hybrid }
public sealed record E4bCurveDefinition(string Id, E4bCurveKind Kind, decimal ProgressionBase,
    decimal CopyGrowth, decimal LargeMultiplier, decimal AdvancedMultiplier);
public sealed record E4bStorageAudit(string Status, int Warehouses, ResourceBundle LargestPayment,
    ResourceBundle Capacity, ResourceBundle WarehousePayments, ResourceBundle? BlockedWarehouseQuote);
public sealed record E4bCrewProjection(int OwnedModules, int AssignedWorkers, int StaffedModules,
    int RunnableModules, int ReservedInactiveWorkers, long GoldConsumedMilliunits,
    long CoalConsumedMilliunits, long CoinsProducedMilliunits);
public sealed record E4bWorkforceBudget(int Modules, int MintWorkers, int GoldMineWorkers,
    int GoldSmelterWorkers, int CoalMineWorkers, int FisherWorkers, int DepotWorkers,
    int FarmWorkers, int WellWorkers, int MillWorkers, int BakeryWorkers,
    int FoundationAndResearchWorkers, int RequiredResidents, int AdditionalLargeHouses,
    int HousingCapacity, decimal RequiredFoodPerSecond, decimal RequiredBreadPerSecond);
public sealed record E4bQuoteRow(string CurveId, string FamilyId, ProductionTier Tier,
    int Epoch, int DependencyDepth, ResourceBundle First, ResourceBundle Second,
    ResourceBundle BatchFive, ResourceBundle BatchTen,
    E4bStorageAudit BatchFiveLiquidity, E4bStorageAudit BatchTenLiquidity);
public sealed record E4bUpgradeRow(string CurveId, ResourceBundle DirectAdvanced,
    ResourceBundle PaidViaBaseLargeAdvanced, ResourceBundle LargestUpgradePayment,
    bool SameCounterTelescopes, bool NextCounterPathNoCheaper,
    E4bStorageAudit DirectLiquidity, E4bStorageAudit UpgradeLiquidity);
public sealed record E4bInvestmentRow(string CurveId, int MintBuildings, ProductionTier Tier,
    ResourceBundle CurrentMintPayment, ResourceBundle ConstructionWithoutWarehouses,
    ResourceBundle? ConstructionWithWarehouses, IReadOnlyDictionary<string, int> RequiredBuildings,
    E4bStorageAudit Liquidity, E4bWorkforceBudget Workforce, decimal ConstructionWorkerSeconds,
    decimal StonecutterWorkEquivalents, decimal PlankOnlyStonecutterEquivalents,
    decimal StoneOnlyStonecutterEquivalents, decimal RequiredResearchPoints,
    decimal ReferenceResearchWorkerSeconds);
public sealed record E4bCurveStudyReport(string Version, string Mode, string TierModuleAssumption,
    IReadOnlyList<E4bCurveDefinition> Curves, IReadOnlyList<E4bQuoteRow> Quotes,
    IReadOnlyList<E4bUpgradeRow> Upgrades, IReadOnlyList<E4bInvestmentRow> Investments,
    V15IndustrialComparisonReport? MeasuredCurrentBaseline);

/// <summary>
/// RC78/E4b.1: isolated numerical proposals. No candidate is installed in a game
/// catalog, clock, UI or save. Prices use the existing quotation engine; liquidity
/// simulates only a warehouse expansion route, assuming materials can be earned.
/// Work quantities are gross reference worker-seconds, never completion times.
/// </summary>
public sealed class E4bEconomicCurveStudy
{
    public const string Version = "e4b-three-curves-study-1";
    public static IReadOnlyList<E4bCurveDefinition> Curves { get; } = Array.AsReadOnly(new[]
    {
        new E4bCurveDefinition("A", E4bCurveKind.Epoch, 2m, 1.08m, 2.6m, 6.5m),
        new E4bCurveDefinition("B", E4bCurveKind.DepthWithMaterials, 1.8m, 1.10m, 2.6m, 6.5m),
        new E4bCurveDefinition("C", E4bCurveKind.Hybrid, 2.7m, 1.15m, 3m, 10m)
    });
    private sealed record Node(string Id, ResourceBundle Cost, string[] Prerequisites,
        bool Tiered, int Epoch);
    private readonly V15BenchmarkFixture _setup = V15BenchmarkFixture.Create();
    private readonly Dictionary<string, Node> _nodes;
    private readonly Dictionary<string, int> _initialCounts;
    private readonly Dictionary<string, int> _depths = new(StringComparer.Ordinal);
    private readonly ResourceBundle _startingCapacity;

    public E4bEconomicCurveStudy()
    {
        _nodes = _setup.Baseline.Catalog.Buildings.Where(b => b.Id != "food-store")
            .ToDictionary(b => b.Id, b => new Node(b.Id, b.Cost, b.Prerequisites.ToArray(),
                b.Recipe is not null, Epoch(b.Id)), StringComparer.Ordinal);
        foreach (var site in FoodIndustryConstructionCatalog.Candidate.Sites)
            _nodes.Add(site.Id, new(site.Id, site.BaseCost, site.Prerequisites.ToArray(), false, 1));
        _nodes.Add("school", new("school", ResourceBundle.Create(("planks", 120m), ("stone", 80m)),
            ["food-depot"], false, 1));
        _initialCounts = _setup.Baseline.InitialBuildings.ToDictionary(b => b.BuildingId,
            b => b.Count, StringComparer.Ordinal);
        _startingCapacity = V15CityStorage.Calculate(_setup.Baseline, _setup.Start.CityCampaign);
    }

    private static int Epoch(string id) => id switch
    {
        "mint" or "forge" => 4,
        "gold-smelter" or "iron-smelter" => 3,
        "coal-mine" or "iron-mine" or "gold-mine" => 2,
        "mill" or "bakery" or "large-house" => 1,
        _ => 0
    };

    public int DependencyDepth(string id)
    {
        if (_depths.TryGetValue(id, out var known)) return known;
        var active = new HashSet<string>(StringComparer.Ordinal);
        int Visit(string key)
        {
            if (_depths.TryGetValue(key, out var found)) return found;
            if (!active.Add(key)) throw new InvalidOperationException("Circular building prerequisites.");
            var node = NodeFor(key);
            var depth = node.Prerequisites.Length == 0 ? 0 : checked(1 + node.Prerequisites.Max(Visit));
            active.Remove(key);
            _depths[key] = depth;
            return depth;
        }
        return Visit(id);
    }

    private Node NodeFor(string id) => _nodes.TryGetValue(id, out var node) ? node
        : throw new ArgumentException("Unknown study building.", nameof(id));

    public ResourceBundle EntryCost(E4bCurveDefinition curve, string id)
    {
        ArgumentNullException.ThrowIfNull(curve);
        if (!Enum.IsDefined(curve.Kind) || curve.ProgressionBase < 1m || curve.CopyGrowth < 1m ||
            curve.LargeMultiplier <= 1m || curve.AdvancedMultiplier <= curve.LargeMultiplier)
            throw new ArgumentException("Invalid economic curve proposal.", nameof(curve));
        var node = NodeFor(id);
        var exponent = curve.Kind == E4bCurveKind.DepthWithMaterials ? DependencyDepth(id) : node.Epoch;
        var multiplier = 1m;
        for (int i = 0; i < exponent; i++) multiplier = checked(multiplier * curve.ProgressionBase);
        // Entry vectors are defined in whole units. Copy/tier prices are then
        // rounded per building by the same engine used for current quotations.
        var entry = ResourceBundle.FromAmounts(node.Cost.Amounts.Select(kv =>
            KeyValuePair.Create(kv.Key, ResourceAmount.FromUnits(decimal.Ceiling(checked(kv.Value.Units * multiplier))))));
        if (curve.Kind == E4bCurveKind.DepthWithMaterials)
            entry += id switch
            {
                "mint" => ResourceBundle.Create(("iron", 40m), ("gold", 20m)),
                "forge" => ResourceBundle.Create(("iron", 40m)),
                _ => ResourceBundle.Empty
            };
        if (curve.Kind == E4bCurveKind.Hybrid)
            entry += id switch
            {
                "mint" => ResourceBundle.Create(("iron", 120m), ("gold", 80m)),
                "forge" => ResourceBundle.Create(("iron", 80m)),
                _ => ResourceBundle.Empty
            };
        return entry;
    }

    private decimal TierMultiplier(E4bCurveDefinition curve, string id, ProductionTier tier)
    {
        if (!NodeFor(id).Tiered && tier != ProductionTier.Base)
            throw new ArgumentException("This study building has no productive tiers.", nameof(tier));
        return tier switch
        {
            ProductionTier.Base => 1m,
            ProductionTier.Large => curve.LargeMultiplier,
            ProductionTier.Advanced => curve.AdvancedMultiplier,
            _ => throw new ArgumentOutOfRangeException(nameof(tier))
        };
    }

    public ResourceBundle Quote(E4bCurveDefinition curve, string id, ProductionTier tier,
        int previouslyAcquired = 0, int quantity = 1) => ProgressiveConstructionPricing.QuoteNewBuildings(
            EntryCost(curve, id), TierMultiplier(curve, id, tier), curve.CopyGrowth, previouslyAcquired, quantity);

    public ResourceBundle Upgrade(E4bCurveDefinition curve, string id, ProductionTier from,
        ProductionTier to, int acquired) => ProgressiveConstructionPricing.QuoteUpgrade(EntryCost(curve, id),
            TierMultiplier(curve, id, from), TierMultiplier(curve, id, to), curve.CopyGrowth, acquired);

    public E4bStorageAudit AuditWarehouseRoute(E4bCurveDefinition curve, ResourceBundle payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        var capacity = _startingCapacity;
        var paid = ResourceBundle.Empty;
        var warehouseStorage = _setup.Baseline.Catalog.BuildingsById["warehouse"].StoragePerBuilding!;
        for (int count = 0; count < 512; count++)
        {
            if (capacity.Covers(payment)) return new("fits-warehouse-route", count, payment, capacity, paid, null);
            var price = Quote(curve, "warehouse", ProductionTier.Base, count);
            if (!capacity.Covers(price))
                return new("warehouse-payment-exceeds-capacity", count, payment, capacity, paid, price);
            capacity += warehouseStorage;
            paid += price;
        }
        return new("route-limit-unmeasured", 512, payment, capacity, paid, null);
    }

    public static int Modules(ProductionTier tier) => tier switch
    {
        ProductionTier.Base => 1, ProductionTier.Large => 3, ProductionTier.Advanced => 10,
        _ => throw new ArgumentOutOfRangeException(nameof(tier))
    };

    public static E4bCrewProjection ProjectMintCrew(int buildings, ProductionTier tier, int assigned,
        long goldMilliunits, long coalMilliunits, long coinSpaceMilliunits)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(buildings);
        ArgumentOutOfRangeException.ThrowIfNegative(assigned);
        ArgumentOutOfRangeException.ThrowIfNegative(goldMilliunits);
        ArgumentOutOfRangeException.ThrowIfNegative(coalMilliunits);
        ArgumentOutOfRangeException.ThrowIfNegative(coinSpaceMilliunits);
        var modules = checked(buildings * Modules(tier));
        if (assigned > checked(modules * 10)) throw new ArgumentOutOfRangeException(nameof(assigned));
        var staffed = Math.Min(modules, assigned / 10);
        var runnable = checked((int)Math.Min(staffed, Math.Min(goldMilliunits / 1000,
            Math.Min(coalMilliunits / 1000, coinSpaceMilliunits / 3000))));
        return new(modules, assigned, staffed, runnable, assigned - runnable * 10,
            checked(runnable * 1000L), checked(runnable * 1000L), checked(runnable * 3000L));
    }

    public static E4bWorkforceBudget Workforce(int mintBuildings, ProductionTier tier)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(mintBuildings);
        var modules = checked(mintBuildings * Modules(tier));
        var industrial = checked(modules * 13); // mint 10, gold mine 1, smelter 1, coal mine 1
        var population = checked(industrial + 4); // 3 foundation workers + 1 researcher
        int fish = 0, farm = 0, well = 0, mill = 0, bakery = 0;
        decimal food = 0, bread = 0;
        for (int i = 0; i < 100; i++)
        {
            food = population * .010m;
            bread = Math.Max(0, population - 16) * .015m; // two basic houses; expand with large houses
            fish = checked((int)decimal.Ceiling(food)); // 1 fish / second and 1 prepared food / depot worker
            farm = checked((int)decimal.Ceiling(bread * 2m / 3m));
            well = mill = bakery = checked((int)decimal.Ceiling(bread / 3m));
            var next = checked(industrial + 4 + 2 * fish + farm + well + mill + bakery);
            if (next == population)
            {
                var houses = checked((int)decimal.Ceiling(Math.Max(0, population - 48) / 32m));
                return new(modules, checked(modules * 10), modules, modules, modules,
                    fish, fish, farm, well, mill, bakery, 4, population, houses,
                    checked(48 + houses * 32), food, bread);
            }
            population = next;
        }
        throw new InvalidOperationException("Workforce budget did not converge.");
    }

    private string ProducerFor(string resource) => _setup.Baseline.Catalog.Buildings
        .Single(b => b.Id != "food-store" && b.Recipe?.Output[resource].Milliunits > 0).Id;

    public decimal ReferenceMaterialWorkerSeconds(ResourceBundle demand)
    {
        decimal Work(string resource, decimal units, HashSet<string> active)
        {
            if (!active.Add(resource)) throw new InvalidOperationException("Circular production recipe.");
            var recipe = _setup.Baseline.Catalog.BuildingsById[ProducerFor(resource)].Recipe!;
            var workers = units / recipe.Output[resource].Units;
            var total = workers;
            foreach (var (input, quantity) in recipe.Input.Amounts)
                total = checked(total + Work(input, checked(workers * quantity.Units), active));
            active.Remove(resource);
            return total;
        }
        return demand.Amounts.Sum(kv => Work(kv.Key, kv.Value.Units, new HashSet<string>(StringComparer.Ordinal)));
    }

    private static ResourceBundle Largest(IEnumerable<ResourceBundle> payments) => ResourceBundle.FromAmounts(
        payments.SelectMany(p => p.Amounts).GroupBy(kv => kv.Key)
            .Select(g => KeyValuePair.Create(g.Key, ResourceAmount.FromMilliunits(g.Max(kv => kv.Value.Milliunits)))));

    public E4bInvestmentRow Investment(E4bCurveDefinition curve, int mintBuildings, ProductionTier tier)
    {
        var staff = Workforce(mintBuildings, tier);
        var required = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["mint"] = mintBuildings, ["gold-mine"] = staff.GoldMineWorkers,
            ["gold-smelter"] = staff.GoldSmelterWorkers, ["coal-mine"] = staff.CoalMineWorkers,
            ["small-house"] = 2, ["large-house"] = 1 + staff.AdditionalLargeHouses,
            ["school"] = 1, ["fisher"] = staff.FisherWorkers, ["food-depot"] = staff.DepotWorkers,
            ["farm"] = staff.FarmWorkers, ["well"] = staff.WellWorkers,
            ["mill"] = staff.MillWorkers, ["bakery"] = staff.BakeryWorkers
        };
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var active = new HashSet<string>(StringComparer.Ordinal);
        void Include(string id)
        {
            required[id] = Math.Max(1, required.GetValueOrDefault(id));
            if (visited.Contains(id)) return;
            if (!active.Add(id)) throw new InvalidOperationException("Circular material/construction dependency.");
            foreach (var prerequisite in NodeFor(id).Prerequisites) Include(prerequisite);
            foreach (var material in EntryCost(curve, id).Amounts.Keys)
            {
                var producer = ProducerFor(material);
                // Existing foundation producers bootstrap their own building
                // materials. Unowned advanced producers must precede payment.
                if (_initialCounts.GetValueOrDefault(producer) == 0) Include(producer);
            }
            active.Remove(id);
            visited.Add(id);
        }
        foreach (var id in required.Keys.ToArray()) Include(id);
        var costs = ResourceBundle.Empty;
        var payments = new List<ResourceBundle>();
        foreach (var (id, count) in required.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var initial = _initialCounts.GetValueOrDefault(id);
            for (int index = initial; index < count; index++)
            {
                var cost = Quote(curve, id, id == "mint" ? tier : ProductionTier.Base, index);
                costs += cost;
                payments.Add(cost);
            }
        }
        var storage = AuditWarehouseRoute(curve, Largest(payments));
        var withWarehouses = storage.Status == "fits-warehouse-route" ? costs + storage.WarehousePayments : null;
        var work = ReferenceMaterialWorkerSeconds(costs);
        var reference = ReferenceMaterialWorkerSeconds(_setup.Baseline.Catalog.BuildingsById["stonecutter"].Cost);
        var research = tier == ProductionTier.Advanced ? 800m : 350m;
        return new(curve.Id, mintBuildings, tier, Quote(curve, "mint", tier, mintBuildings - 1),
            costs, withWarehouses, new SortedDictionary<string, int>(required, StringComparer.Ordinal), storage,
            staff, work, work / reference, costs["planks"].Units / 30m, costs["stone"].Units / 10m,
            research, research / .1m);
    }

    public E4bCurveStudyReport Run(IReadOnlyList<V15IndustrialProgressionRun>? measuredBaseline = null)
    {
        V15IndustrialComparisonReport? measured = null;
        if (measuredBaseline is not null)
        {
            measured = V15IndustrialProgressionAnalysis.Analyze(measuredBaseline);
            if (measured.RunCount != 6 || measured.Diagnostics != 0 || measured.Passed != 6 ||
                measuredBaseline.Select(r => r.ScenarioId).Distinct().OrderBy(id => id)
                    .SequenceEqual(new[] { "V15-I01", "V15-I02", "V15-I03" }) == false)
                throw new ArgumentException("Supply the six observed full-horizon RC77-M01 industrial runs.", nameof(measuredBaseline));
        }
        var quotes = new List<E4bQuoteRow>();
        var investments = new List<E4bInvestmentRow>();
        var upgrades = new List<E4bUpgradeRow>();
        foreach (var curve in Curves)
        {
            foreach (var id in new[] { "stonecutter", "farm", "mill", "bakery", "coal-mine", "iron-smelter", "gold-smelter", "forge", "mint" })
                foreach (var tier in Enum.GetValues<ProductionTier>())
                    quotes.Add(new(curve.Id, id, tier, NodeFor(id).Epoch, DependencyDepth(id),
                        Quote(curve, id, tier), Quote(curve, id, tier, 1),
                        Quote(curve, id, tier, 0, 5), Quote(curve, id, tier, 0, 10),
                        AuditWarehouseRoute(curve, Quote(curve, id, tier, 0, 5)),
                        AuditWarehouseRoute(curve, Quote(curve, id, tier, 0, 10))));
            foreach (var count in new[] { 1, 2 })
                foreach (var tier in Enum.GetValues<ProductionTier>()) investments.Add(Investment(curve, count, tier));
            var first = Quote(curve, "mint", ProductionTier.Base);
            var direct = Quote(curve, "mint", ProductionTier.Advanced);
            var baseLarge = Upgrade(curve, "mint", ProductionTier.Base, ProductionTier.Large, 1);
            var largeAdvanced = Upgrade(curve, "mint", ProductionTier.Large, ProductionTier.Advanced, 1);
            var paid = first + baseLarge + largeAdvanced;
            var fixedCounter = first + Upgrade(curve, "mint", ProductionTier.Base, ProductionTier.Large, 0) +
                Upgrade(curve, "mint", ProductionTier.Large, ProductionTier.Advanced, 0);
            upgrades.Add(new(curve.Id, direct, paid, Largest([first, baseLarge, largeAdvanced]),
                fixedCounter.Equals(direct), paid.Covers(direct), AuditWarehouseRoute(curve, direct),
                AuditWarehouseRoute(curve, Largest([first, baseLarge, largeAdvanced]))));
        }
        return new(Version, "analytical-proposal/not-applied", "candidate-modules-1-3-10/crew-10",
            Curves, quotes, upgrades, investments, measured);
    }
}
