using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application.Benchmarks;

namespace SimRisorse.Simulation;

/// <summary>RC83: isolated 1-to-2 mint scaling risk matrix, without running or editing a game.</summary>
internal static class E4bMintScaleRiskCommand
{
    public static int Run(string[] args)
    {
        try
        {
            if (args.Length != 1 && (args.Length != 3 || args[1] != "--output" ||
                string.IsNullOrWhiteSpace(args[2])))
                throw new ArgumentException("Usage: --study-e4b-mint-scale [--output DIRECTORY].");
            var report = new E4bMintScaleRiskStudy().Run();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            if (args.Length == 3)
            {
                var directory = Path.GetFullPath(args[2]);
                var names = new[] { "e4b-mint-scale.json", "e4b-mint-scale.csv", "e4b-mint-scale.md" };
                if (names.Any(name => File.Exists(Path.Combine(directory, name))))
                    throw new IOException("Mint scale study outputs already exist; choose a new directory.");
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, names[0]), JsonSerializer.Serialize(report, options));
                File.WriteAllText(Path.Combine(directory, names[1]), E4bMintScaleRiskStudy.ToCsv(report));
                File.WriteAllText(Path.Combine(directory, names[2]), E4bMintScaleRiskStudy.ToMarkdown(report));
            }
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                report.Version, report.Mode, report.StorageScope, report.WorkforceScope,
                Rows = report.Rows.Count, Curves = report.Rows.Select(r => r.CurveId).Distinct().Count(),
                Tiers = report.Rows.Select(r => r.Tier).Distinct().Count()
            }, options));
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
