using System.Globalization;
using System.Text.Json;
using SimRisorse.Application.Benchmarks;

namespace SimRisorse.Simulation;

/// <summary>RC88: reproducible research-to-industrial-output audit; offline mode
/// accepts unmodified JSON from the existing industrial/mint runners.</summary>
internal static class V15IndustrialChainCommand
{
    public static int Run(string[] args)
    {
        try
        {
            int? limit = 0; // Explicit diagnostic default; full runs are opt-in.
            string? input = null;
            string? output = null;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 1; i < args.Length; i += 2)
            {
                if (i + 1 >= args.Length || !seen.Add(args[i]) ||
                    string.IsNullOrWhiteSpace(args[i + 1]))
                    throw new ArgumentException("Usage: --audit-v15-industry-chain [--max-seconds 0..57600 | --input RESULTS_JSON] [--output NEW_DIRECTORY].");
                switch (args[i])
                {
                    case "--max-seconds":
                        if (!int.TryParse(args[i + 1], NumberStyles.None, CultureInfo.InvariantCulture,
                                out var seconds) || seconds is < 0 or > 57600)
                            throw new ArgumentException("Industrial audit horizon must be 0..57600 seconds.");
                        limit = seconds;
                        break;
                    case "--input": input = args[i + 1]; break;
                    case "--output": output = args[i + 1]; break;
                    default: throw new ArgumentException("Unknown argument: " + args[i]);
                }
            }
            if (input is not null && seen.Contains("--max-seconds"))
                throw new ArgumentException("Offline input cannot be mixed with a new simulation horizon.");
            var json = new JsonSerializerOptions { WriteIndented = true };
            V15IndustrialProgressionRun[] runs;
            if (input is not null)
            {
                runs = JsonSerializer.Deserialize<V15IndustrialProgressionRun[]>(File.ReadAllText(input)) ??
                    throw new ArgumentException("The supplied JSON does not contain an industrial run array.");
            }
            else
            {
                runs = [
                    V15IndustrialProgressionBenchmark.Run("V15-I02", "research-first", limit),
                    V15IndustrialProgressionBenchmark.Run("V15-I02", "bread-first", limit),
                    V15IndustrialProgressionBenchmark.Run("V15-I03", "research-first", limit),
                    V15IndustrialProgressionBenchmark.Run("V15-I03", "bread-first", limit),
                    V15IndustrialProgressionBenchmark.RunMintTen("research-first", limit),
                    V15IndustrialProgressionBenchmark.RunMintTen("bread-first", limit),
                    V15IndustrialProgressionBenchmark.RunMintTen("housing-buffer", limit)
                ];
            }
            var audit = V15IndustrialChainAudit.Analyze(runs);
            if (output is not null)
            {
                var folder = Path.GetFullPath(output);
                var names = new[] { "rc88-industry-chain-results.json", "rc88-industry-chain-audit.json",
                    "rc88-industry-chain.csv", "rc88-industry-chain.md" };
                if (names.Any(name => File.Exists(Path.Combine(folder, name))))
                    throw new IOException("RC88 output already exists. Choose a fresh directory.");
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, names[0]), JsonSerializer.Serialize(runs, json));
                File.WriteAllText(Path.Combine(folder, names[1]), JsonSerializer.Serialize(audit, json));
                File.WriteAllText(Path.Combine(folder, names[2]), V15IndustrialChainAudit.ToCsv(audit));
                File.WriteAllText(Path.Combine(folder, names[3]), V15IndustrialChainAudit.ToMarkdown(audit));
            }
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                audit.Version, audit.EvidenceScope, audit.RunCount,
                audit.VerifiedRuns, audit.DiagnosticRuns,
                Outcomes = audit.Observations.Select(row => new
                {
                    row.ScenarioId, row.PolicyId, row.RulesVersion,
                    row.Outcome, row.Diagnostic, row.Verified, row.PendingAction
                }).ToArray()
            }, json));
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or
            IOException or UnauthorizedAccessException or JsonException or OverflowException or
            NotSupportedException)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
