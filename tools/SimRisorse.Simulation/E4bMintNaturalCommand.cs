using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application.Benchmarks;

namespace SimRisorse.Simulation;

/// <summary>RC82: opt-in, non-financed v15 natural benchmark for a full mint crew.
/// The historical E4a runner and all existing player saves are untouched.</summary>
internal static class E4bMintNaturalCommand
{
    private const string Prefix = "e4b-mint-ten";

    public static int Run(string[] args)
    {
        try
        {
            string? policy = null;
            string? output = null;
            int? maxSeconds = null;
            var compareLegacy = false;
            var includeHousingBuffer = false;
            var used = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 1; i < args.Length; i++)
            {
                var option = args[i];
                if (!used.Add(option)) throw new ArgumentException("Duplicate option: " + option);
                if (option == "--compare-legacy")
                {
                    compareLegacy = true;
                    continue;
                }
                if (option == "--include-housing-buffer")
                {
                    includeHousingBuffer = true;
                    continue;
                }
                if (option is not ("--policy" or "--max-seconds" or "--output") ||
                    ++i >= args.Length || string.IsNullOrWhiteSpace(args[i]))
                    throw new ArgumentException("Invalid or missing argument for: " + option);
                switch (option)
                {
                    case "--policy":
                        policy = args[i];
                        break;
                    case "--max-seconds":
                        if (!int.TryParse(args[i], NumberStyles.None, CultureInfo.InvariantCulture,
                            out var seconds))
                            throw new ArgumentException("--max-seconds must be a nonnegative integer.");
                        maxSeconds = seconds;
                        break;
                    case "--output":
                        output = args[i];
                        break;
                }
            }
            if (policy is not (null or V15IndustrialProgressionBenchmark.PolicyResearchFirst or
                V15IndustrialProgressionBenchmark.PolicyBreadFirst or
                V15IndustrialProgressionBenchmark.PolicyHousingBuffer))
                throw new ArgumentException("Unknown mint benchmark policy.");
            if (policy is not null && includeHousingBuffer)
                throw new ArgumentException("Choose either --policy or --include-housing-buffer, not both.");
            var policies = policy is null
                ? includeHousingBuffer
                    ? new[] { V15IndustrialProgressionBenchmark.PolicyResearchFirst,
                        V15IndustrialProgressionBenchmark.PolicyBreadFirst,
                        V15IndustrialProgressionBenchmark.PolicyHousingBuffer }
                    : new[] { V15IndustrialProgressionBenchmark.PolicyResearchFirst,
                        V15IndustrialProgressionBenchmark.PolicyBreadFirst }
                : new[] { policy! };
            var runs = new List<V15IndustrialProgressionRun>();
            foreach (var current in policies)
            {
                runs.Add(V15IndustrialProgressionBenchmark.RunMintTen(current, maxSeconds));
                if (compareLegacy)
                    runs.Add(V15IndustrialProgressionBenchmark.Run("V15-I03", current, maxSeconds));
            }
            var analysis = V15IndustrialProgressionAnalysis.Analyze(runs);
            var constraints = V15IndustrialConstraintAnalysis.Analyze(runs);
            var json = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            var report = JsonSerializer.Serialize(runs, json);
            if (output is not null)
            {
                var folder = Path.GetFullPath(output);
                var suffixes = new[] { "results.json", "events.csv", "samples.csv",
                    "analysis.json", "analysis.csv", "analysis.md",
                    "constraints.json", "constraints.csv", "constraints.md" };
                var paths = suffixes.Select(s => Path.Combine(folder, Prefix + "-" + s)).ToArray();
                if (paths.Any(File.Exists))
                    throw new IOException("Mint benchmark outputs already exist; choose a new directory.");
                Directory.CreateDirectory(folder);
                File.WriteAllText(paths[0], report);
                File.WriteAllText(paths[1], V15IndustrialProgressionBenchmark.EventsCsv(runs));
                File.WriteAllText(paths[2], V15IndustrialProgressionBenchmark.SamplesCsv(runs));
                File.WriteAllText(paths[3], JsonSerializer.Serialize(analysis, json));
                File.WriteAllText(paths[4], V15IndustrialProgressionAnalysis.ToCsv(analysis));
                File.WriteAllText(paths[5], V15IndustrialProgressionAnalysis.ToMarkdown(analysis));
                File.WriteAllText(paths[6], JsonSerializer.Serialize(constraints, json));
                File.WriteAllText(paths[7], V15IndustrialConstraintAnalysis.ToCsv(constraints));
                File.WriteAllText(paths[8], V15IndustrialConstraintAnalysis.ToMarkdown(constraints));
            }
            Console.WriteLine(report);
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or
            OverflowException or IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
