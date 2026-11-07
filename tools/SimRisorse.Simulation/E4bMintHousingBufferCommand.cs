using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application.Benchmarks;

namespace SimRisorse.Simulation;

/// <summary>RC85: offline housing-buffer vs bread-first comparison.
/// Accepts only genuine natural run ledgers and never advances the clock.</summary>
internal static class E4bMintHousingBufferCommand
{
    public static int Run(string[] args)
    {
        try
        {
            string? input = null;
            string? output = null;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 1; i < args.Length; i += 2)
            {
                if (i + 1 >= args.Length || !seen.Add(args[i]) ||
                    string.IsNullOrWhiteSpace(args[i + 1]))
                    throw new ArgumentException("Usage: --review-e4b-housing-buffer --input RESULTS_JSON [--output NEW_DIRECTORY].");
                switch (args[i])
                {
                    case "--input": input = args[i + 1]; break;
                    case "--output": output = args[i + 1]; break;
                    default: throw new ArgumentException("Unknown option: " + args[i]);
                }
            }
            if (input is null)
                throw new ArgumentException("An existing --input RESULTS_JSON is required.");
            var bytes = File.ReadAllBytes(Path.GetFullPath(input));
            var json = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            var runs = JsonSerializer.Deserialize<V15IndustrialProgressionRun[]>(bytes, json)
                ?? throw new ArgumentException("Housing buffer input must be a nonempty array.");
            var report = E4bMintHousingBufferStudy.Analyze(runs) with
            {
                InputSha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()
            };
            if (output is not null)
            {
                var folder = Path.GetFullPath(output);
                var names = new[] { "e4b-housing-buffer.json", "e4b-housing-buffer.csv",
                    "e4b-housing-buffer.md" };
                if (names.Any(name => File.Exists(Path.Combine(folder, name))))
                    throw new IOException("RC85 report already exists; choose a new directory.");
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, names[0]), JsonSerializer.Serialize(report, json));
                File.WriteAllText(Path.Combine(folder, names[1]), E4bMintHousingBufferStudy.ToCsv(report));
                File.WriteAllText(Path.Combine(folder, names[2]), E4bMintHousingBufferStudy.ToMarkdown(report));
            }
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                report.Version, report.EvidenceScope, report.InputSha256,
                report.NaturalRunCount, report.PairedContracts,
                report.ComparableCompletedContracts
            }, json));
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or
            OverflowException or IOException or UnauthorizedAccessException or JsonException or NotSupportedException)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
