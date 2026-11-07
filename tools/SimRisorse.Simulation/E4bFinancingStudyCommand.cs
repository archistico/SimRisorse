using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application.Benchmarks;

namespace SimRisorse.Simulation;

internal static class E4bFinancingStudyCommand
{
    public static int Run(string[] args)
    {
        try
        {
            if (args.Length != 1 && (args.Length != 3 || args[1] != "--output" ||
                string.IsNullOrWhiteSpace(args[2])))
                throw new ArgumentException("Usage: --study-e4b-financing [--output DIRECTORY].");

            var report = new E4bFinancingFeasibilityStudy().Run();
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            if (args.Length == 3)
            {
                var directory = Path.GetFullPath(args[2]);
                var filenames = new[] { "e4b-financing.json", "e4b-financing.csv", "e4b-financing.md" };
                if (filenames.Any(name => File.Exists(Path.Combine(directory, name))))
                    throw new IOException("Financing study outputs already exist; choose a new directory.");
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, filenames[0]), JsonSerializer.Serialize(report, jsonOptions));
                File.WriteAllText(Path.Combine(directory, filenames[1]), E4bFinancingFeasibilityStudy.ToCsv(report));
                File.WriteAllText(Path.Combine(directory, filenames[2]), E4bFinancingFeasibilityStudy.ToMarkdown(report));
            }
            Console.WriteLine(JsonSerializer.Serialize(new { report.Version, report.Mode,
                Options = "atomic/sequential/escrow", Rows = report.Rows.Count }, jsonOptions));
            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or
            OverflowException or IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }
}
