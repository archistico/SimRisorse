using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application.Benchmarks;

namespace SimRisorse.Simulation;

internal static class V15IndustrialRecoveryCommand
{
    public static int Run(string[] args)
    {
        try
        {
            if ((args.Length - 1) % 2 != 0) throw new ArgumentException("Options require a value.");
            string? scenario = null, output = null;
            int? maxSeconds = null;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 1; i < args.Length; i += 2)
            {
                if (!seen.Add(args[i])) throw new ArgumentException("Duplicate option.");
                switch (args[i])
                {
                    case "--scenario":
                        scenario = args[i + 1];
                        if (!V15IndustrialRecoveryBenchmark.ScenarioIds.Contains(scenario))
                            throw new ArgumentException("Use V15-R01, V15-R02 or V15-R03.");
                        break;
                    case "--output":
                        output = args[i + 1];
                        if (string.IsNullOrWhiteSpace(output)) throw new ArgumentException("Output directory is empty.");
                        break;
                    case "--max-seconds":
                        if (!int.TryParse(args[i + 1], NumberStyles.None, CultureInfo.InvariantCulture, out var seconds) ||
                            seconds is < 0 or > V15IndustrialRecoveryBenchmark.HorizonSeconds)
                            throw new ArgumentException("Use a diagnostic duration between 0 and 120 seconds.");
                        maxSeconds = seconds;
                        break;
                    default: throw new ArgumentException("Unknown recovery benchmark option.");
                }
            }
            var names = new[] { "e4a-v15-recovery-results.json", "e4a-v15-recovery-samples.csv",
                "e4a-v15-recovery-actions.csv", "e4a-v15-recovery.md" };
            if (output is not null && names.Any(name => File.Exists(Path.Combine(output, name))))
                throw new IOException("Recovery benchmark outputs already exist; choose a new directory.");

            var runs = scenario is null ? V15IndustrialRecoveryBenchmark.RunAll(maxSeconds)
                : V15IndustrialRecoveryBenchmark.RunScenario(scenario, maxSeconds);
            var json = new JsonSerializerOptions { WriteIndented = true,
                Converters = { new JsonStringEnumConverter() } };
            if (output is not null)
            {
                Directory.CreateDirectory(output);
                File.WriteAllText(Path.Combine(output, names[0]), JsonSerializer.Serialize(runs, json));
                File.WriteAllText(Path.Combine(output, names[1]), V15IndustrialRecoveryBenchmark.SamplesCsv(runs));
                File.WriteAllText(Path.Combine(output, names[2]), V15IndustrialRecoveryBenchmark.ActionsCsv(runs));
                File.WriteAllText(Path.Combine(output, names[3]), V15IndustrialRecoveryBenchmark.ToMarkdown(runs));
            }
            // Console stays small; complete checkpoints and second-by-second
            // measurements live in the explicitly requested output files.
            Console.WriteLine(JsonSerializer.Serialize(runs.Select(run => new
            {
                run.ScenarioId, run.PolicyId, run.Version, run.RulesVersion, run.Fixture,
                run.DeclaredHorizonSeconds, run.EffectiveHorizonSeconds, run.Diagnostic,
                run.TargetResourceId, run.FirstProductionSecond, run.ProducedTargetMilliunits,
                run.MealsServed, run.MealsMissed, run.Actions, run.Totals
            }), json));
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
