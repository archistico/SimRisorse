using System.Globalization;
using System.Text.Json;
using SimRisorse.Application.Benchmarks;

namespace SimRisorse.Simulation;

/// <summary>RC87: reproducible no-grant first-research study with evidence files.</summary>
internal static class V15ResearchBootstrapCommand
{
    public static int Run(string[] args)
    {
        try
        {
            int? limit = null;
            string? output = null;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 1; i < args.Length; i += 2)
            {
                if (i + 1 >= args.Length || !seen.Add(args[i]) ||
                    string.IsNullOrWhiteSpace(args[i + 1]))
                    throw new ArgumentException("Usage: --audit-v15-research-bootstrap [--max-seconds 0..21600] [--output NEW_DIRECTORY].");
                switch (args[i])
                {
                    case "--max-seconds":
                        if (!int.TryParse(args[i + 1], NumberStyles.None, CultureInfo.InvariantCulture, out var seconds) ||
                            seconds is < 0 or > 21600)
                            throw new ArgumentException("Research bootstrap horizon must be within 0..21600 seconds.");
                        limit = seconds;
                        break;
                    case "--output": output = args[i + 1]; break;
                    default: throw new ArgumentException("Unknown argument: " + args[i]);
                }
            }
            var runs = new[]
            {
                V15IndustrialProgressionBenchmark.Run("V15-I01",
                    V15IndustrialProgressionBenchmark.PolicyResearchFirst, limit),
                V15IndustrialProgressionBenchmark.Run("V15-I01",
                    V15IndustrialProgressionBenchmark.PolicyBreadFirst, limit)
            };
            var audit = V15ResearchBootstrapAudit.Analyze(runs);
            var json = new JsonSerializerOptions { WriteIndented = true };
            if (output is not null)
            {
                var folder = Path.GetFullPath(output);
                var names = new[] { "rc87-research-bootstrap-results.json", "rc87-research-bootstrap-audit.json",
                    "rc87-research-bootstrap.csv", "rc87-research-bootstrap.md" };
                if (names.Any(name => File.Exists(Path.Combine(folder, name))))
                    throw new IOException("RC87 files already exist. Supply a new --output directory.");
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, names[0]), JsonSerializer.Serialize(runs, json));
                File.WriteAllText(Path.Combine(folder, names[1]), JsonSerializer.Serialize(audit, json));
                File.WriteAllText(Path.Combine(folder, names[2]), V15ResearchBootstrapAudit.ToCsv(audit));
                File.WriteAllText(Path.Combine(folder, names[3]), V15ResearchBootstrapAudit.ToMarkdown(audit));
            }
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                audit.Version, audit.EvidenceScope, audit.RunCount, audit.VerifiedRuns,
                audit.StartingPencilsMilliunits, audit.PencilsPerStudentMilliunits,
                audit.ExtractionCostMilliPoints, audit.BreadMinusResearchSeconds,
                Outcomes = audit.Observations.Select(row => new
                {
                    row.PolicyId, row.Outcome, row.Diagnostic, row.Verified
                }).ToArray()
            }, json));
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or
            IOException or UnauthorizedAccessException or OverflowException)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
