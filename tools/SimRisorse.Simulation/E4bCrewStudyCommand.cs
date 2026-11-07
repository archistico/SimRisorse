using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application.Benchmarks;

namespace SimRisorse.Simulation;

internal static class E4bCrewStudyCommand
{
    public static int Run(string[] args)
    {
        try
        {
            if (args.Length != 1 && (args.Length != 3 || args[1] != "--output" ||
                string.IsNullOrWhiteSpace(args[2])))
                throw new ArgumentException("Usage: --study-e4b-crews [--output DIRECTORY].");
            var report = new E4bCrewOperationalStudy().Run();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            if (args.Length == 3)
            {
                var directory = Path.GetFullPath(args[2]);
                var names = new[] { "e4b-crews.json", "e4b-crews.csv", "e4b-crews.md" };
                if (names.Any(n => File.Exists(Path.Combine(directory, n))))
                    throw new IOException("Crew study outputs already exist; choose another directory.");
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, names[0]), JsonSerializer.Serialize(report, options));
                File.WriteAllText(Path.Combine(directory, names[1]), E4bCrewOperationalStudy.ToCsv(report));
                File.WriteAllText(Path.Combine(directory, names[2]), E4bCrewOperationalStudy.ToMarkdown(report));
            }
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                report.Version, report.Mode, report.CatalogRulesVersion, report.CrewSize,
                Rows = report.Rows.Count
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
