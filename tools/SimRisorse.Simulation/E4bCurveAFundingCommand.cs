using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application.Benchmarks;

namespace SimRisorse.Simulation;

/// <summary>RC92: reproducible sandbox transfers, not real game production.</summary>
internal static class E4bCurveAFundingCommand
{
    public static int Run(string[] args)
    {
        try
        {
            string? output = null;
            if (args.Length == 3 && args[1] == "--output" &&
                !string.IsNullOrWhiteSpace(args[2])) output = args[2];
            else if (args.Length != 1)
                throw new ArgumentException("Usage: --study-e4b-curve-a-funding [--output NEW_DIRECTORY].");

            var report = E4bCurveAFundingDemonstration.Run();
            var json = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            if (output is not null)
            {
                var folder = Path.GetFullPath(output);
                var names = new[] { "rc92-curve-a-funding.json", "rc92-curve-a-funding.csv",
                    "rc92-curve-a-funding.md" };
                if (names.Any(name => File.Exists(Path.Combine(folder, name))))
                    throw new IOException("RC92 reports already exist; choose a new directory.");
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, names[0]), JsonSerializer.Serialize(report, json));
                File.WriteAllText(Path.Combine(folder, names[1]), E4bCurveAFundingDemonstration.ToCsv(report));
                File.WriteAllText(Path.Combine(folder, names[2]), E4bCurveAFundingDemonstration.ToMarkdown(report));
            }
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                report.Version, report.Mode, report.InputProvenance,
                Rows = report.Rows.Count,
                Reconciled = report.Rows.Count(row => row.Reconciles),
                report.ApprovalStatus
            }, json));
            return 0;
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException or
            OverflowException or IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine(e.Message);
            return 1;
        }
    }
}
