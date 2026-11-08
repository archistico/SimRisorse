using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application;
using SimRisorse.Application.Benchmarks;
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
    Console.WriteLine("SimRisorse.Simulation --audit-v15 --seconds N | --hours N [--profile combined]: optional full-chain stress (max 48h), no personal saves.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-e4a-school [--output DIRECTORY]: controlled v14 E4A-06/07, one vs two schools.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-e4a-natural [--scenario E4A-01..05] [--output DIRECTORY]: unfunded v14 progression; JSON/CSV.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-e4a-v15-natural [--scenario V15-N01|V15-N02|V15-N03] [--max-seconds N] [--output DIRECTORY]: separate unfunded v15 progression; JSON + samples/events CSV. Shortened horizons are diagnostic.");
    Console.WriteLine("SimRisorse.Simulation --analyze-e4a-v15-natural --input RESULTS_JSON [--output DIRECTORY]: evidence-based postprocessing of RC73 JSON. No simulation; CSV, Markdown, JSON.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-e4a-v15-industry [--scenario V15-I01|V15-I02|V15-I03] [--policy research-first|bread-first] [--max-seconds N] [--output DIRECTORY]: unfunded school/research/forge/mint strategies with full material receipts.");
    Console.WriteLine("SimRisorse.Simulation --analyze-e4a-v15-industry --input RESULTS_JSON [--output DIRECTORY]: economic analysis and measured RC76 shortage/capacity evidence; never simulates or modifies saves.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-e4a-v15-recovery [--scenario V15-R01|V15-R02|V15-R03] [--max-seconds 0..120] [--output DIRECTORY]: controlled, declared-grant production-loss and recovery experiments; not natural progression.");
    Console.WriteLine("SimRisorse.Simulation --study-e4b-curves [--baseline INDUSTRY_JSON_OR_DIRECTORY] [--output DIRECTORY]: three analytical proposals; no live rules changed.");
    Console.WriteLine("SimRisorse.Simulation --study-e4b-financing [--output DIRECTORY]: compare atomic, sequential, and escrow capacity requirements; no live rules changed.");
    Console.WriteLine("SimRisorse.Simulation --study-e4b-crews [--output DIRECTORY]: study the 10-worker mint team using live recipe in a controlled projection, not gameplay.");
    Console.WriteLine("SimRisorse.Simulation --study-e4b-mint-scale [--output DIRECTORY]: 1 vs 2 mints, tier/crew/filiera/prezzi/capienza; studio isolato. No game changes.");
    Console.WriteLine("SimRisorse.Simulation --study-e4b-price-readiness [--input RC88_RESULTS_JSON] [--output NEW_DIRECTORY]: preflight curve A/B/C, capacità, magazzini, rate e ricevute industriali; non attiva i prezzi.");
    Console.WriteLine("SimRisorse.Simulation --study-e4b-curve-a-funding [--output NEW_DIRECTORY]: opt-in memory sandbox, 6 mint funding demonstrations with hypothetical replenishments; no gameplay or save changes.");
    Console.WriteLine("SimRisorse.Simulation --study-e4b-v15-shadow: RC94 composite v15/escrow preview, debit/restore/cancel, no player-save changes.");
    Console.WriteLine("SimRisorse.Simulation --study-e4b-v15-funding-clock: RC97 real one-second v15 clock with reserved materials, no gameplay/save changes.");
    Console.WriteLine("SimRisorse.Simulation --study-e4b-v15-mint-commit: RC98 isolated mint completion using SYNTHETIC pre-provisioned materials; not a natural playthrough.");
    Console.WriteLine("SimRisorse.Simulation --analyze-e4b-mint-ten --input RESULTS_JSON [--output NEW_DIRECTORY]: audit osservazioni MintTen e confronto separato con proposte A/B/C; nessun clock avanzato.");
    Console.WriteLine("SimRisorse.Simulation --review-e4b-housing-buffer --input RESULTS_JSON [--output NEW_DIRECTORY]: confronto offline e pagamenti effettivi housing-buffer vs bread-first, non modifica salvataggi.");
    Console.WriteLine("SimRisorse.Simulation --audit-v15-industry-chain [--max-seconds 0..57600 | --input RESULTS_JSON] [--output NEW_DIRECTORY]: audit ricevute ricerca, filiera, produzione Spade/Monete; 0 secondi predefinito = diagnostica.");
    Console.WriteLine("SimRisorse.Simulation --audit-v15-research-bootstrap [--max-seconds 0..21600] [--output NEW_DIRECTORY]: verifica la prima ricerca su campagne reali, senza grants; output in JSON/CSV/Markdown. 0 secondi = solo diagnostica.");
    Console.WriteLine("SimRisorse.Simulation --benchmark-e4b-mint-ten [--policy research-first|bread-first|housing-buffer | --include-housing-buffer] [--max-seconds N] [--compare-legacy] [--output DIRECTORY]: natural v15 mint path with ten workers, without grants. Short runs are diagnostic only.");
    return 0;
}
var json = new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
if (args.Length > 0 && args[0] == "--audit-v15-industry-chain")
    return V15IndustrialChainCommand.Run(args);
if (args.Length > 0 && args[0] == "--audit-v15-research-bootstrap")
    return V15ResearchBootstrapCommand.Run(args);
if (args.Length > 0 && args[0] == "--study-e4b-curves")
    return E4bCurveStudyCommand.Run(args);
if (args.Length > 0 && args[0] == "--study-e4b-financing")
    return E4bFinancingStudyCommand.Run(args);
if (args.Length > 0 && args[0] == "--study-e4b-crews")
    return E4bCrewStudyCommand.Run(args);
if (args.Length > 0 && args[0] == "--study-e4b-mint-scale")
    return E4bMintScaleRiskCommand.Run(args);
if (args.Length > 0 && args[0] == "--study-e4b-price-readiness")
    return E4bPriceReadinessCommand.Run(args);
if (args.Length > 0 && args[0] == "--study-e4b-curve-a-funding")
    return E4bCurveAFundingCommand.Run(args);
if (args.Length > 0 && args[0] == "--study-e4b-v15-funding-clock")
    return E4bV15FundingClockCommand.Run(args);
if (args.Length > 0 && args[0] == "--study-e4b-v15-mint-commit")
    return E4bV15MintCommitCommand.Run(args);
if (args.Length > 0 && args[0] == "--study-e4b-v15-shadow")
    return E4bV15ShadowCommand.Run(args);
if (args.Length > 0 && args[0] == "--analyze-e4b-mint-ten")
    return E4bMintNaturalFeasibilityCommand.Run(args);
if (args.Length > 0 && args[0] == "--review-e4b-housing-buffer")
    return E4bMintHousingBufferCommand.Run(args);
if (args.Length > 0 && args[0] == "--benchmark-e4b-mint-ten")
    return E4bMintNaturalCommand.Run(args);
if (args.Length > 0 && args[0] == "--benchmark-e4a-v15-recovery")
    return V15IndustrialRecoveryCommand.Run(args);
if (args.Length > 0 && args[0] == "--audit-v15")
{
    var combined = args is ["--audit-v15", _, _, "--profile", "combined"];
    if (!(args.Length == 3 || combined) || args[1] is not ("--seconds" or "--hours") ||
        !int.TryParse(args[2], NumberStyles.None, CultureInfo.InvariantCulture, out var duration) ||
        duration < 1 || duration > (args[1] == "--hours" ? 48 : 172800)) return Usage();
    try
    {
        var secondsToAudit = args[1] == "--hours" ? checked(duration * 3600) : duration;
        var report = combined
            ? JsonSerializer.Serialize(V15CombinedStressAudit.Run(secondsToAudit), json)
            : JsonSerializer.Serialize(V15StressAudit.Run(secondsToAudit), json);
        Console.WriteLine(report);
        return 0;
    }
    catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OverflowException)
    {
        Console.Error.WriteLine(ex.Message);
        return 1;
    }
}

if (args.Length > 0 && args[0] == "--benchmark-e4a-v15-industry")
{
    string? requestedScenario = null;
    string? requestedPolicy = null;
    string? outputDirectory = null;
    int? maxSeconds = null;
    var seen = new HashSet<string>(StringComparer.Ordinal);
    if ((args.Length - 1) % 2 != 0) return Usage();
    for (var i = 1; i < args.Length; i += 2)
    {
        if (!seen.Add(args[i])) return Usage();
        if (args[i] == "--scenario") requestedScenario = args[i + 1];
        else if (args[i] == "--policy") requestedPolicy = args[i + 1];
        else if (args[i] == "--output") outputDirectory = args[i + 1];
        else if (args[i] == "--max-seconds")
        {
            if (!int.TryParse(args[i + 1], NumberStyles.None, CultureInfo.InvariantCulture, out var limit) ||
                limit < 0) return Usage();
            maxSeconds = limit;
        }
        else return Usage();
    }
    if (requestedScenario is "" || requestedPolicy is "" || outputDirectory is "" ||
        requestedPolicy is not (null or V15IndustrialProgressionBenchmark.PolicyResearchFirst or
            V15IndustrialProgressionBenchmark.PolicyBreadFirst)) return Usage();
    try
    {
        IReadOnlyList<V15IndustrialProgressionRun> runs = requestedScenario is not null && requestedPolicy is not null
            ? [V15IndustrialProgressionBenchmark.Run(requestedScenario, requestedPolicy, maxSeconds)]
            : requestedScenario is null && requestedPolicy is null
                ? V15IndustrialProgressionBenchmark.RunAll(maxSeconds)
                : (requestedScenario is null
                    ? new[] { "V15-I01", "V15-I02", "V15-I03" }.Select(id =>
                        V15IndustrialProgressionBenchmark.Run(id, requestedPolicy!, maxSeconds)).ToArray()
                    : new[] { V15IndustrialProgressionBenchmark.PolicyResearchFirst,
                        V15IndustrialProgressionBenchmark.PolicyBreadFirst }.Select(policy =>
                        V15IndustrialProgressionBenchmark.Run(requestedScenario, policy, maxSeconds)).ToArray());
        var analysis = V15IndustrialProgressionAnalysis.Analyze(runs);
        var constraints = V15IndustrialConstraintAnalysis.Analyze(runs);
        var report = JsonSerializer.Serialize(runs, json);
        if (outputDirectory is not null)
        {
            var outputs = new[] {
                Path.Combine(outputDirectory, "e4a-v15-industry-results.json"),
                Path.Combine(outputDirectory, "e4a-v15-industry-events.csv"),
                Path.Combine(outputDirectory, "e4a-v15-industry-samples.csv"),
                Path.Combine(outputDirectory, "e4a-v15-industry-analysis.json"),
                Path.Combine(outputDirectory, "e4a-v15-industry-analysis.csv"),
                Path.Combine(outputDirectory, "e4a-v15-industry-analysis.md"),
                Path.Combine(outputDirectory, "e4a-v15-industry-constraints.json"),
                Path.Combine(outputDirectory, "e4a-v15-industry-constraints.csv"),
                Path.Combine(outputDirectory, "e4a-v15-industry-constraints.md") };
            if (outputs.Any(File.Exists))
            {
                Console.Error.WriteLine("Industrial benchmark outputs already exist; choose a new directory.");
                return 1;
            }
            Directory.CreateDirectory(outputDirectory);
            File.WriteAllText(outputs[0], report);
            File.WriteAllText(outputs[1], V15IndustrialProgressionBenchmark.EventsCsv(runs));
            File.WriteAllText(outputs[2], V15IndustrialProgressionBenchmark.SamplesCsv(runs));
            File.WriteAllText(outputs[3], JsonSerializer.Serialize(analysis, json));
            File.WriteAllText(outputs[4], V15IndustrialProgressionAnalysis.ToCsv(analysis));
            File.WriteAllText(outputs[5], V15IndustrialProgressionAnalysis.ToMarkdown(analysis));
            File.WriteAllText(outputs[6], JsonSerializer.Serialize(constraints, json));
            File.WriteAllText(outputs[7], V15IndustrialConstraintAnalysis.ToCsv(constraints));
            File.WriteAllText(outputs[8], V15IndustrialConstraintAnalysis.ToMarkdown(constraints));
        }
        Console.WriteLine(report);
        return 0;
    }
    catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or
        IOException or UnauthorizedAccessException or OverflowException)
    {
        Console.Error.WriteLine(exception.Message);
        return 1;
    }
}

if (args.Length > 0 && args[0] == "--analyze-e4a-v15-industry")
{
    string? input = null;
    string? outputDirectory = null;
    var seen = new HashSet<string>(StringComparer.Ordinal);
    if ((args.Length - 1) % 2 != 0) return Usage();
    for (var i = 1; i < args.Length; i += 2)
    {
        if (!seen.Add(args[i])) return Usage();
        if (args[i] == "--input") input = args[i + 1];
        else if (args[i] == "--output") outputDirectory = args[i + 1];
        else return Usage();
    }
    if (string.IsNullOrWhiteSpace(input) || outputDirectory is "") return Usage();
    try
    {
        var source = Directory.Exists(input)
            ? Path.Combine(input, "e4a-v15-industry-results.json") : input;
        var runs = JsonSerializer.Deserialize<V15IndustrialProgressionRun[]>(
            File.ReadAllText(source, System.Text.Encoding.UTF8), json)
            ?? throw new InvalidDataException("No industrial progression runs in input.");
        var analysis = V15IndustrialProgressionAnalysis.Analyze(runs);
        var constraints = V15IndustrialConstraintAnalysis.Analyze(runs);
        var report = JsonSerializer.Serialize(analysis, json);
        if (outputDirectory is not null)
        {
            var outputs = new[] {
                Path.Combine(outputDirectory, "e4a-v15-industry-analysis.json"),
                Path.Combine(outputDirectory, "e4a-v15-industry-analysis.csv"),
                Path.Combine(outputDirectory, "e4a-v15-industry-analysis.md"),
                Path.Combine(outputDirectory, "e4a-v15-industry-constraints.json"),
                Path.Combine(outputDirectory, "e4a-v15-industry-constraints.csv"),
                Path.Combine(outputDirectory, "e4a-v15-industry-constraints.md") };
            if (outputs.Any(File.Exists))
            {
                Console.Error.WriteLine("Analysis outputs already exist; choose a new directory.");
                return 1;
            }
            Directory.CreateDirectory(outputDirectory);
            File.WriteAllText(outputs[0], report);
            File.WriteAllText(outputs[1], V15IndustrialProgressionAnalysis.ToCsv(analysis));
            File.WriteAllText(outputs[2], V15IndustrialProgressionAnalysis.ToMarkdown(analysis));
            File.WriteAllText(outputs[3], JsonSerializer.Serialize(constraints, json));
            File.WriteAllText(outputs[4], V15IndustrialConstraintAnalysis.ToCsv(constraints));
            File.WriteAllText(outputs[5], V15IndustrialConstraintAnalysis.ToMarkdown(constraints));
        }
        Console.WriteLine(report);
        return 0;
    }
    catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or
        JsonException or InvalidDataException or IOException or UnauthorizedAccessException or OverflowException)
    {
        Console.Error.WriteLine(exception.Message);
        return 1;
    }
}

if (args.Length > 0 && args[0] == "--analyze-e4a-v15-natural")
{
    string? input = null;
    string? outputDirectory = null;
    var seen = new HashSet<string>(StringComparer.Ordinal);
    if ((args.Length - 1) % 2 != 0) return Usage();
    for (var i = 1; i < args.Length; i += 2)
    {
        if (!seen.Add(args[i])) return Usage();
        if (args[i] == "--input") input = args[i + 1];
        else if (args[i] == "--output") outputDirectory = args[i + 1];
        else return Usage();
    }
    if (string.IsNullOrWhiteSpace(input) ||
        outputDirectory is not null && string.IsNullOrWhiteSpace(outputDirectory)) return Usage();
    try
    {
        var source = Directory.Exists(input)
            ? Path.Combine(input, "e4a-v15-natural-results.json") : input;
        var runs = JsonSerializer.Deserialize<V15NaturalProgressionRun[]>(
            File.ReadAllText(source, System.Text.Encoding.UTF8), json)
            ?? throw new InvalidDataException("Natural benchmark input has no scenario runs.");
        var analysis = V15NaturalProgressionAnalysis.Analyze(runs);
        var report = JsonSerializer.Serialize(analysis, json);
        if (outputDirectory is not null)
        {
            var jsonPath = Path.Combine(outputDirectory, "e4a-v15-analysis.json");
            var csvPath = Path.Combine(outputDirectory, "e4a-v15-analysis.csv");
            var markdownPath = Path.Combine(outputDirectory, "e4a-v15-analysis.md");
            // Do not create/replace any output if even one named file already exists.
            if (File.Exists(jsonPath) || File.Exists(csvPath) || File.Exists(markdownPath))
            {
                Console.Error.WriteLine("Analysis files already exist; choose a new output directory.");
                return 1;
            }
            Directory.CreateDirectory(outputDirectory);
            File.WriteAllText(jsonPath, report);
            File.WriteAllText(csvPath, V15NaturalProgressionAnalysis.ToCsv(analysis));
            File.WriteAllText(markdownPath, V15NaturalProgressionAnalysis.ToMarkdown(analysis));
        }
        Console.WriteLine(report);
        return 0; // An observed timeout is a valid measurement, not a process failure.
    }
    catch (Exception exception) when (exception is JsonException or InvalidDataException or
        InvalidOperationException or ArgumentException or IOException or UnauthorizedAccessException or
        OverflowException)
    {
        Console.Error.WriteLine(exception.Message);
        return 1;
    }
}

if (args.Length > 0 && args[0] == "--benchmark-e4a-v15-natural")
{
    string? requestedScenario = null;
    string? outputDirectory = null;
    int? maxSeconds = null;
    var seen = new HashSet<string>(StringComparer.Ordinal);
    if ((args.Length - 1) % 2 != 0) return Usage();
    for (var i = 1; i < args.Length; i += 2)
    {
        if (!seen.Add(args[i])) return Usage();
        if (args[i] == "--scenario") requestedScenario = args[i + 1];
        else if (args[i] == "--output") outputDirectory = args[i + 1];
        else if (args[i] == "--max-seconds")
        {
            if (!int.TryParse(args[i + 1], NumberStyles.None, CultureInfo.InvariantCulture, out var limit) ||
                limit < 0) return Usage();
            maxSeconds = limit;
        }
        else return Usage();
    }
    if (requestedScenario is "" || string.IsNullOrWhiteSpace(outputDirectory) && outputDirectory is not null)
        return Usage();
    try
    {
        IReadOnlyList<V15NaturalProgressionRun> runs = requestedScenario is null
            ? V15NaturalProgressionBenchmark.RunAll(maxSeconds)
            : [V15NaturalProgressionBenchmark.Run(requestedScenario, maxSeconds)];
        var report = JsonSerializer.Serialize(runs, json);
        if (outputDirectory is not null)
        {
            Directory.CreateDirectory(outputDirectory);
            var jsonPath = Path.Combine(outputDirectory, "e4a-v15-natural-results.json");
            var samplesPath = Path.Combine(outputDirectory, "e4a-v15-natural-samples.csv");
            var eventsPath = Path.Combine(outputDirectory, "e4a-v15-natural-events.csv");
            if (File.Exists(jsonPath) || File.Exists(samplesPath) || File.Exists(eventsPath))
            {
                Console.Error.WriteLine("Benchmark files already exist; choose a new output directory.");
                return 1;
            }
            File.WriteAllText(jsonPath, report);
            File.WriteAllText(samplesPath, V15NaturalProgressionBenchmark.SamplesCsv(runs));
            File.WriteAllText(eventsPath, V15NaturalProgressionBenchmark.EventsCsv(runs));
        }
        Console.WriteLine(report);
        // A measured timeout or game-design softlock is not a CLI malfunction.
        // The outcome and stop reason remain explicit in the emitted data.
        return 0;
    }
    catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or
        IOException or UnauthorizedAccessException or OverflowException)
    {
        Console.Error.WriteLine(exception.Message);
        return 1;
    }
}

if (args.Length > 0 && args[0] == "--benchmark-e4a-natural")
{
    string? requestedScenario = null;
    string? outputDirectory = null;
    var seenOptions = new HashSet<string>(StringComparer.Ordinal);
    if ((args.Length - 1) % 2 != 0) return Usage();
    for (var i = 1; i < args.Length; i += 2)
    {
        if (!seenOptions.Add(args[i])) return Usage();
        if (args[i] == "--scenario") requestedScenario = args[i + 1];
        else if (args[i] == "--output") outputDirectory = args[i + 1];
        else return Usage();
    }
    if (requestedScenario is "" || outputDirectory is "") return Usage();
    try
    {
        IReadOnlyList<E4aNaturalRun> runs = requestedScenario is null
            ? E4aNaturalBenchmark.RunAll()
            : new[] { E4aNaturalBenchmark.Run(requestedScenario) };
        var report = JsonSerializer.Serialize(runs, json);
        if (outputDirectory is not null)
        {
            Directory.CreateDirectory(outputDirectory);
            var jsonPath = Path.Combine(outputDirectory, "e4a-natural-results.json");
            var csvPath = Path.Combine(outputDirectory, "e4a-natural-results.csv");
            if (File.Exists(jsonPath) || File.Exists(csvPath))
            {
                Console.Error.WriteLine("The requested output already exists: choose a new directory for this run.");
                return 1;
            }
            File.WriteAllText(jsonPath, report);
            File.WriteAllText(csvPath, E4aNaturalBenchmark.ToCsv(runs));
        }
        Console.WriteLine(report);
        return runs.All(run => run.Outcome == "pass") ? 0 : 1;
    }
    catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or
        IOException or UnauthorizedAccessException or OverflowException)
    {
        Console.Error.WriteLine(exception.Message);
        return 1;
    }
}
if (args is ["--benchmark-e4a-school"] or ["--benchmark-e4a-school", "--output", _])
{
    try
    {
        var runs = E4aSchoolBenchmark.Run();
        var report = JsonSerializer.Serialize(runs, json);
        if (args.Length == 3)
        {
            if (string.IsNullOrWhiteSpace(args[2])) return Usage();
            Directory.CreateDirectory(args[2]);
            var jsonPath = Path.Combine(args[2], "e4a-school-results.json");
            var csvPath = Path.Combine(args[2], "e4a-school-results.csv");
            if (File.Exists(jsonPath) || File.Exists(csvPath))
            {
                Console.Error.WriteLine("The requested output already exists: choose a new directory for this run.");
                return 1;
            }
            File.WriteAllText(jsonPath, report);
            File.WriteAllText(csvPath, E4aSchoolBenchmark.ToCsv(runs));
        }
        Console.WriteLine(report);
        return runs.All(run => run.Outcome == "pass") ? 0 : 1;
    }
    catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or IOException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine(exception.Message);
        return 1;
    }
}
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
