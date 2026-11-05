using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application.Benchmarks;

namespace SimRisorse.Simulation;

internal static class E4bCurveStudyCommand
{
    public static int Run(string[] args)
    {
        try
        {
            if ((args.Length - 1) % 2 != 0) throw new ArgumentException("Options require a value.");
            string? baseline = null, output = null;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 1; i < args.Length; i += 2)
            {
                if (!seen.Add(args[i]) || string.IsNullOrWhiteSpace(args[i + 1]))
                    throw new ArgumentException("Duplicate or empty study option.");
                switch (args[i])
                {
                    case "--baseline": baseline = args[i + 1]; break;
                    case "--output": output = args[i + 1]; break;
                    default: throw new ArgumentException("Unknown study option.");
                }
            }
            var names = new[] { "e4b-curves.json", "e4b-quotes.csv", "e4b-investments.csv", "e4b-curves.md", "baseline-source.json" };
            if (output is not null && names.Any(n => File.Exists(Path.Combine(output, n))))
                throw new IOException("Study outputs already exist; choose a new directory.");
            var json = new JsonSerializerOptions { WriteIndented = true,
                Converters = { new JsonStringEnumConverter() } };
            V15IndustrialProgressionRun[]? runs = null;
            string? sourceHash = null;
            if (baseline is not null)
            {
                baseline = Path.GetFullPath(Directory.Exists(baseline)
                    ? Path.Combine(baseline, "e4a-v15-industry-results.json") : baseline);
                var bytes = File.ReadAllBytes(baseline);
                sourceHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                runs = JsonSerializer.Deserialize<V15IndustrialProgressionRun[]>(bytes, json)
                    ?? throw new ArgumentException("No measured baseline runs.");
            }
            var report = new E4bEconomicCurveStudy().Run(runs);
            if (output is not null)
            {
                Directory.CreateDirectory(output);
                File.WriteAllText(Path.Combine(output, names[0]), JsonSerializer.Serialize(report, json));
                File.WriteAllText(Path.Combine(output, names[1]), E4bCurveStudyExport.QuotesCsv(report));
                File.WriteAllText(Path.Combine(output, names[2]), E4bCurveStudyExport.InvestmentsCsv(report));
                File.WriteAllText(Path.Combine(output, names[3]), E4bCurveStudyExport.ToMarkdown(report));
                File.WriteAllText(Path.Combine(output, names[4]), JsonSerializer.Serialize(new { Path = baseline, Sha256 = sourceHash }, json));
            }
            Console.WriteLine(JsonSerializer.Serialize(new { report.Version, report.Mode,
                Curves = report.Curves.Count, Quotes = report.Quotes.Count, Investments = report.Investments.Count,
                MeasuredBaselineRuns = report.MeasuredCurrentBaseline?.RunCount ?? 0 }, json));
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException or
            UnauthorizedAccessException or OverflowException or JsonException)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
