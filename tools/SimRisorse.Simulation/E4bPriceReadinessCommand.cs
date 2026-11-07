using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application.Benchmarks;

namespace SimRisorse.Simulation;

/// <summary>RC89 non-invasive pricing preflight; optional RC88 real receipts.</summary>
internal static class E4bPriceReadinessCommand
{
    public static int Run(string[] args)
    {
        try
        {
            string? input = null;
            string? output = null;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            if ((args.Length - 1) % 2 != 0)
                throw new ArgumentException("Usage: --study-e4b-price-readiness [--input RC88_RESULTS_JSON] [--output NEW_DIRECTORY].");
            for (var i = 1; i < args.Length; i += 2)
            {
                if (string.IsNullOrWhiteSpace(args[i + 1]) || !seen.Add(args[i]))
                    throw new ArgumentException("Empty or repeated RC89 argument.");
                switch (args[i])
                {
                    case "--input": input = args[i + 1]; break;
                    case "--output": output = args[i + 1]; break;
                    default: throw new ArgumentException("Unknown RC89 argument: " + args[i]);
                }
            }
            var json = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            V15IndustrialProgressionRun[]? imported = null;
            string? inputHash = null;
            if (input is not null)
            {
                var bytes = File.ReadAllBytes(Path.GetFullPath(input));
                inputHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                imported = JsonSerializer.Deserialize<V15IndustrialProgressionRun[]>(bytes, json) ??
                    throw new ArgumentException("RC89 input must be an array of industrial receipts.");
            }
            var report = new E4bPriceReadinessStudy().Run(imported);
            if (output is not null)
            {
                var folder = Path.GetFullPath(output);
                var names = new[] { "rc89-price-readiness.json", "rc89-price-readiness.csv",
                    "rc89-price-readiness.md", "rc89-source.json" };
                if (names.Any(name => File.Exists(Path.Combine(folder, name))))
                    throw new IOException("RC89 outputs already exist. Choose a new directory.");
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, names[0]), JsonSerializer.Serialize(report, json));
                File.WriteAllText(Path.Combine(folder, names[1]), E4bPriceReadinessStudy.ToCsv(report));
                File.WriteAllText(Path.Combine(folder, names[2]), E4bPriceReadinessStudy.ToMarkdown(report));
                File.WriteAllText(Path.Combine(folder, names[3]), JsonSerializer.Serialize(new
                {
                    ImportedInputSha256 = inputHash, report.EvidenceScope,
                    InputKind = input is null ? "no-observed-runs" : "offline-RC88-receipts",
                    report.ApprovalStatus
                }, json));
            }
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                report.Version, report.Mode, report.ApprovalStatus,
                Rows = report.Rows.Count, Curves = report.Rows.Select(r => r.CurveId).Distinct().Count(),
                Tiers = report.Rows.Select(r => r.Tier).Distinct().Count(),
                report.ImportedRuns, report.VerifiedNaturalRuns,
                report.VerifiedMintTenRuns, report.DiagnosticRuns, InputSha256 = inputHash
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
