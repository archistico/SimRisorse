using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application.Benchmarks;

namespace SimRisorse.Simulation;

/// <summary>RC84: offline-only review of a saved natural MintTen report.
/// Never advances a clock, modifies saves or claims that A/B/C were applied.</summary>
internal static class E4bMintNaturalFeasibilityCommand
{
    public static int Run(string[] args)
    {
        try
        {
            string? input = null;
            string? output = null;
            var used = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 1; index < args.Length; index += 2)
            {
                if (index + 1 >= args.Length || !used.Add(args[index]) ||
                    string.IsNullOrWhiteSpace(args[index + 1]))
                    throw new ArgumentException("Usage: --analyze-e4b-mint-ten --input RESULTS_JSON [--output NEW_DIRECTORY].");
                switch (args[index])
                {
                    case "--input": input = args[index + 1]; break;
                    case "--output": output = args[index + 1]; break;
                    default: throw new ArgumentException("Unknown option: " + args[index]);
                }
            }
            if (input is null)
                throw new ArgumentException("An existing --input RESULTS_JSON is required.");
            var inputFile = Path.GetFullPath(input);
            var sourceBytes = File.ReadAllBytes(inputFile);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            var runs = JsonSerializer.Deserialize<V15IndustrialProgressionRun[]>(sourceBytes, options)
                ?? throw new ArgumentException("Natural benchmark input must be a nonempty JSON array.");
            var audit = E4bMintNaturalFeasibilityAudit.Analyze(runs) with
            {
                InputSha256 = Convert.ToHexString(SHA256.HashData(sourceBytes)).ToLowerInvariant()
            };
            if (output is not null)
            {
                var folder = Path.GetFullPath(output);
                var names = new[] { "e4b-mint-feasibility.json", "e4b-mint-feasibility.csv",
                    "e4b-mint-feasibility.md" };
                if (names.Any(name => File.Exists(Path.Combine(folder, name))))
                    throw new IOException("RC84 outputs already exist; choose a new directory.");
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, names[0]), JsonSerializer.Serialize(audit, options));
                File.WriteAllText(Path.Combine(folder, names[1]), E4bMintNaturalFeasibilityAudit.ToCsv(audit));
                File.WriteAllText(Path.Combine(folder, names[2]), E4bMintNaturalFeasibilityAudit.ToMarkdown(audit));
            }
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                audit.Version, audit.EvidenceScope, audit.ProposalScope,
                audit.InputSha256, audit.ObservedRuns, audit.DiagnosticRuns,
                audit.FullHorizonRuns, audit.VerifiedMintTenMilestones,
                audit.CompleteNaturalPolicyPairObserved,
                Hypotheses = audit.EconomicHypotheses.Count
            }, options));
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
