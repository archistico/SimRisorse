using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application;
using SimRisorse.Application.Features.Construction.ConstructBuildings;
using SimRisorse.Application.Features.Military.EnlistRecruits;
using SimRisorse.Application.Features.Population.AssignWorkers;
using SimRisorse.Application.Features.Sessions.AdvanceTime;
using SimRisorse.Application.Features.Sessions.GetGameState;
using SimRisorse.Application.Features.Sessions.SetPause;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Content;
using SimRisorse.Simulation;

if (args is ["--help"])
{
    Console.WriteLine("SimRisorse.Simulation [--seconds N | --hours N] [--scenario economy-foundation|civil-growth] [--strategy bootstrap|recovery|storage|bread|advanced|industry|gold|forge|mint|crafting|military]");
    Console.WriteLine("Maximum 168 hours; default 60 seconds. Civil growth defaults to bootstrap; foundation has no strategy.");
    Console.WriteLine("--rules legacy: frozen civil-growth-4; --rules historical-v9: frozen civil-growth-9 for bootstrap/industry/gold reference trajectories.");
    Console.WriteLine("SimRisorse.Simulation --check-catalog [--scenario economy-foundation|civil-growth]: exit 1 if an unlock is unreachable.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-housing: historical M4 comparison; --benchmark-housing-accelerated: new M4.10 comparison.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-bread-scale: compares one, two full, a delayed full and a lean bread chain at 220 residents.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-bread-high-density: stress comparison at 320 residents with sufficient housing and food.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-industry-scale: five controlled coal-allocation profiles under civil-growth-7.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-forge-mint-coal: three v9 coal strategies at 1800/3600 seconds, with replay and ledger checks.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-forge-mint-campaign: three real-start v9 routes to 240 swords/360 coins over 2400 seconds.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-military-capacity: controlled v13 comparison of 5/10/20 recruits and 1/2/4 barracks.");
    return 0;
}
var json = new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
if (args is ["--benchmark-housing"])
{
    try { Console.WriteLine(JsonSerializer.Serialize(HousingBenchmark.Run(), json)); return 0; }
    catch (InvalidOperationException exception) { Console.Error.WriteLine(exception.Message); return 1; }
}
if (args is ["--benchmark-housing-accelerated"])
{
    try { Console.WriteLine(JsonSerializer.Serialize(HousingBenchmark.Run(accelerated: true), json)); return 0; }
    catch (InvalidOperationException exception) { Console.Error.WriteLine(exception.Message); return 1; }
}
if (args is ["--benchmark-bread-scale"])
{
    try { Console.WriteLine(JsonSerializer.Serialize(BreadScaleBenchmark.Run(), json)); return 0; }
    catch (InvalidOperationException exception) { Console.Error.WriteLine(exception.Message); return 1; }
}
if (args is ["--benchmark-bread-high-density"])
{
    try { Console.WriteLine(JsonSerializer.Serialize(BreadScaleBenchmark.RunHighDensity(), json)); return 0; }
    catch (InvalidOperationException exception) { Console.Error.WriteLine(exception.Message); return 1; }
}
if (args is ["--benchmark-industry-scale"])
{
    try { Console.WriteLine(JsonSerializer.Serialize(IndustryScaleBenchmark.Run(), json)); return 0; }
    catch (InvalidOperationException exception) { Console.Error.WriteLine(exception.Message); return 1; }
}
if (args is ["--benchmark-forge-mint-coal"])
{
    try { Console.WriteLine(JsonSerializer.Serialize(ForgeMintCoalBenchmark.Run(), json)); return 0; }
    catch (InvalidOperationException exception) { Console.Error.WriteLine(exception.Message); return 1; }
}
if (args is ["--benchmark-forge-mint-campaign"])
{
    try { Console.WriteLine(JsonSerializer.Serialize(ForgeMintCampaignBenchmark.Run(), json)); return 0; }
    catch (InvalidOperationException exception) { Console.Error.WriteLine(exception.Message); return 1; }
}
if (args is ["--benchmark-military-capacity"])
{
    try { Console.WriteLine(JsonSerializer.Serialize(MilitaryCapacityBenchmark.Run(), json)); return 0; }
    catch (InvalidOperationException exception) { Console.Error.WriteLine(exception.Message); return 1; }
}
if (args is ["--check-catalog", .. var checkArguments])
{
    var checkedScenario = checkArguments switch
    {
        [] or ["--scenario", "civil-growth"] => CivilGrowthScenario.Create(),
        ["--scenario", "economy-foundation"] => EconomyFoundationScenario.Create(),
        _ => null
    };
    if (checkedScenario is null) return Usage();
    var report = CatalogReachability.Analyze(checkedScenario);
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        ScenarioId = checkedScenario.Id, checkedScenario.Catalog.RulesVersion, report.IsComplete,
        report.ReachableBuildings, report.RenewableResources, report.Issues
    }, json));
    return report.IsComplete ? 0 : 1;
}
var seconds = 60m;
var scenarioId = "economy-foundation";
string? strategy = null;
string? rulesPreset = null;
var options = new HashSet<string>(StringComparer.Ordinal);
for (var index = 0; index < args.Length; index += 2)
{
    var option = args[index];
    if (index + 1 >= args.Length || !options.Add(option)) return Usage();
    var value = args[index + 1];
    if (option == "--rules") { if (value is not ("legacy" or "historical-v9")) return Usage(); rulesPreset = value; continue; }
    if (option == "--scenario") { scenarioId = value; continue; }
    if (option == "--strategy") { strategy = value; continue; }
    if (option is not ("--seconds" or "--hours") || (options.Contains("--hours") && options.Contains("--seconds")) ||
        !decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var duration) ||
        duration < 0 || duration > (option == "--hours" ? 168m : 604800m)) return Usage();
    seconds = option == "--hours" ? duration * 3600m : duration;
}
if (scenarioId is not ("economy-foundation" or "civil-growth") ||
    (rulesPreset is not null && scenarioId != "civil-growth") ||
    (rulesPreset == "legacy" && strategy is ("industry" or "gold" or "forge" or "mint" or "crafting" or "military")) ||
    (rulesPreset == "historical-v9" && strategy is not (null or "bootstrap" or "industry" or "gold")) ||
    (strategy is not null && (scenarioId != "civil-growth" || strategy is not ("bootstrap" or "recovery" or "storage" or "bread" or "advanced" or "industry" or "gold" or "forge" or "mint" or "crafting" or "military")))) return Usage();
if (scenarioId == "civil-growth") strategy ??= "bootstrap";

static int Usage()
{
    Console.Error.WriteLine("Use --seconds N or --hours N (maximum 168h), --scenario economy-foundation|civil-growth; optional --strategy bootstrap|recovery|storage|bread|advanced|industry|gold|forge|mint|crafting|military and --rules legacy|historical-v9. Historical-v9 supports bootstrap, industry and gold; military requires current rules.");
    return 2;
}

static void Build(GameRuntime runtime, string id, bool staff = false)
{
    if (!new ConstructBuildingsHandler(runtime).Handle(new(id, 1)).Succeeded ||
        (staff && !new AssignWorkersHandler(runtime).Handle(new(id, 1)).Succeeded))
        throw new InvalidOperationException($"The reference strategy could not build/staff {id}.");
}

static void Food(GameRuntime runtime)
{
    Build(runtime, "fisher", staff: true);
    Build(runtime, "food-store", staff: true);
}

var ticks = seconds * TimeSpan.TicksPerSecond;
if (ticks != decimal.Truncate(ticks))
{
    Console.Error.WriteLine("Duration must be representable in 100-nanosecond ticks.");
    return 2;
}
var runtime = new GameRuntime(scenarioId == "civil-growth"
    ? rulesPreset switch
    {
        "legacy" => CivilGrowthScenario.CreateLegacyForBenchmarks(),
        "historical-v9" => CivilGrowthScenario.CreateMintForBenchmarks(),
        _ => CivilGrowthScenario.Create()
    }
    : EconomyFoundationScenario.Create());
new SetPauseHandler(runtime).Handle(new(false));
var advance = new AdvanceTimeHandler(runtime);
void AdvanceTo(long target)
{
    var remainingTicks = target - new GetGameStateHandler(runtime).Handle().ElapsedTicks;
    while (remainingTicks > 0)
    {
        var batch = Math.Min(remainingTicks, TimeSpan.TicksPerHour);
        var result = advance.Handle(new(TimeSpan.FromTicks(batch)));
        if (!result.Succeeded) throw new InvalidOperationException(result.Error.ToString());
        remainingTicks -= batch;
    }
}
try
{
    if (strategy is not null)
    {
        // storage follows bootstrap, then expands the full stocks at second 1200.
        var recovery = strategy == "recovery";
        if (recovery) Build(runtime, "small-house"); else Food(runtime);
        var eventTicks = (recovery ? 600L : 61L) * TimeSpan.TicksPerSecond;
        if (ticks >= eventTicks)
        {
            AdvanceTo(eventTicks);
            if (recovery) Food(runtime); else Build(runtime, "small-house");
        }
        var storageTicks = 1200L * TimeSpan.TicksPerSecond;
        if (strategy == "storage" && ticks >= storageTicks)
        {
            AdvanceTo(storageTicks);
            Build(runtime, "warehouse");
        }
        // bread follows bootstrap, then starts the whole grain/water/flour chain at second 600.
        var breadTicks = 600L * TimeSpan.TicksPerSecond;
        if ((strategy is "bread" or "advanced" or "industry" or "gold" or "forge" or "mint" or "crafting" or "military") && ticks >= breadTicks)
        {
            AdvanceTo(breadTicks);
            foreach (var id in new[] { "farm", "farm", "well", "mill", "bakery" }) Build(runtime, id, staff: true);
        }
        var largeHouseTicks = 760L * TimeSpan.TicksPerSecond;
        if ((strategy is "advanced" or "industry" or "gold" or "forge" or "mint" or "crafting" or "military") && ticks >= largeHouseTicks)
        {
            AdvanceTo(largeHouseTicks);
            Build(runtime, "large-house");
        }
        var industryTicks = 1100L * TimeSpan.TicksPerSecond;
        if ((strategy is "industry" or "gold" or "forge" or "mint" or "crafting" or "military") && ticks >= industryTicks)
        {
            AdvanceTo(industryTicks);
            foreach (var id in new[] { "iron-mine", "coal-mine", "iron-smelter" }) Build(runtime, id, staff: true);
        }
        var forgeTicks = 1400L * TimeSpan.TicksPerSecond;
        if (strategy == "forge" && ticks >= forgeTicks)
        {
            AdvanceTo(forgeTicks);
            Build(runtime, "forge", staff: true);
        }
        var goldTicks = 1500L * TimeSpan.TicksPerSecond;
        if ((strategy is "gold" or "mint" or "crafting" or "military") && ticks >= goldTicks)
        {
            AdvanceTo(goldTicks);
            foreach (var id in new[] { "gold-mine", "gold-smelter" }) Build(runtime, id, staff: true);
        }
        var mintTicks = 1800L * TimeSpan.TicksPerSecond;
        if (strategy == "mint" && ticks >= mintTicks)
        {
            AdvanceTo(mintTicks);
            Build(runtime, "mint", staff: true);
        }
        var craftingTicks = 2200L * TimeSpan.TicksPerSecond;
        if ((strategy is "crafting" or "military") && ticks >= craftingTicks)
        {
            AdvanceTo(craftingTicks);
            foreach (var id in new[] { "coal-mine", "forge", "mint" }) Build(runtime, id, staff: true);
        }
        // Recover the fork's deterministic military progression from real starting stock.
        // Wait for the barracks to be affordable, then enlist up to 20 people as swords/civilians permit.
        var militaryTicks = 2320L * TimeSpan.TicksPerSecond;
        if (strategy == "military" && ticks >= militaryTicks)
        {
            AdvanceTo(militaryTicks);
            var built = false;
            var enlisted = 0;
            var construct = new ConstructBuildingsHandler(runtime);
            var enlist = new EnlistRecruitsHandler(runtime);
            for (var second = militaryTicks; ; second += TimeSpan.TicksPerSecond)
            {
                if (!built) built = construct.Handle(new("barracks", 1)).Succeeded;
                while (built && enlisted < 20 && enlist.Handle(new(1)).Succeeded) enlisted++;
                if ((built && enlisted == 20) || second + TimeSpan.TicksPerSecond > (long)ticks) break;
                AdvanceTo(second + TimeSpan.TicksPerSecond);
            }
        }
    }
    AdvanceTo((long)ticks);
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
new SetPauseHandler(runtime).Handle(new(true));
var view = new GetGameStateHandler(runtime).Handle();
Console.WriteLine(JsonSerializer.Serialize(view, json));
return 0;
