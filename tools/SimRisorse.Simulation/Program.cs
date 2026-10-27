using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application;
using SimRisorse.Application.Features.Construction.ConstructBuildings;
using SimRisorse.Application.Features.Population.AssignWorkers;
using SimRisorse.Application.Features.Sessions.AdvanceTime;
using SimRisorse.Application.Features.Sessions.GetGameState;
using SimRisorse.Application.Features.Sessions.SetPause;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Content;
using SimRisorse.Simulation;

if (args is ["--help"])
{
    Console.WriteLine("SimRisorse.Simulation [--seconds N | --hours N] [--scenario economy-foundation|civil-growth] [--strategy bootstrap|recovery|storage|bread|advanced|industry|gold]");
    Console.WriteLine("Maximum 168 hours; default 60 seconds. Civil growth defaults to bootstrap; foundation has no strategy.");
    Console.WriteLine("SimRisorse.Simulation --check-catalog [--scenario economy-foundation|civil-growth]: exit 1 if an unlock is unreachable.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-housing: historical M4 comparison; --benchmark-housing-accelerated: new M4.10 comparison.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-bread-scale: compares one, two full, a delayed full and a lean bread chain at 220 residents.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-bread-high-density: stress comparison at 320 residents with sufficient housing and food.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-industry-scale: five controlled coal-allocation profiles under civil-growth-7.");
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
var legacyCivilRules = false;
var options = new HashSet<string>(StringComparer.Ordinal);
for (var index = 0; index < args.Length; index += 2)
{
    var option = args[index];
    if (index + 1 >= args.Length || !options.Add(option)) return Usage();
    var value = args[index + 1];
    if (option == "--rules") { if (value != "legacy") return Usage(); legacyCivilRules = true; continue; }
    if (option == "--scenario") { scenarioId = value; continue; }
    if (option == "--strategy") { strategy = value; continue; }
    if (option is not ("--seconds" or "--hours") || (options.Contains("--hours") && options.Contains("--seconds")) ||
        !decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var duration) ||
        duration < 0 || duration > (option == "--hours" ? 168m : 604800m)) return Usage();
    seconds = option == "--hours" ? duration * 3600m : duration;
}
if (scenarioId is not ("economy-foundation" or "civil-growth") ||
    (legacyCivilRules && scenarioId != "civil-growth") ||
    ((strategy is "industry" or "gold") && legacyCivilRules) ||
    (strategy is not null && (scenarioId != "civil-growth" || strategy is not ("bootstrap" or "recovery" or "storage" or "bread" or "advanced" or "industry" or "gold")))) return Usage();
if (scenarioId == "civil-growth") strategy ??= "bootstrap";

static int Usage()
{
    Console.Error.WriteLine("Use --seconds N or --hours N (maximum 168h), --scenario economy-foundation|civil-growth and optionally --strategy bootstrap|recovery|storage|bread|advanced|industry|gold for civil-growth. Industry and gold require current rules.");
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
    ? (legacyCivilRules ? CivilGrowthScenario.CreateLegacyForBenchmarks() : CivilGrowthScenario.Create())
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
        if ((strategy is "bread" or "advanced" or "industry" or "gold") && ticks >= breadTicks)
        {
            AdvanceTo(breadTicks);
            foreach (var id in new[] { "farm", "farm", "well", "mill", "bakery" }) Build(runtime, id, staff: true);
        }
        var largeHouseTicks = 760L * TimeSpan.TicksPerSecond;
        if ((strategy is "advanced" or "industry" or "gold") && ticks >= largeHouseTicks)
        {
            AdvanceTo(largeHouseTicks);
            Build(runtime, "large-house");
        }
        var industryTicks = 1100L * TimeSpan.TicksPerSecond;
        if ((strategy is "industry" or "gold") && ticks >= industryTicks)
        {
            AdvanceTo(industryTicks);
            foreach (var id in new[] { "iron-mine", "coal-mine", "iron-smelter" }) Build(runtime, id, staff: true);
        }
        var goldTicks = 1500L * TimeSpan.TicksPerSecond;
        if (strategy == "gold" && ticks >= goldTicks)
        {
            AdvanceTo(goldTicks);
            foreach (var id in new[] { "gold-mine", "gold-smelter" }) Build(runtime, id, staff: true);
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
