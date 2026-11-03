using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Content;

namespace SimRisorse.Desktop;

public sealed record DesktopOptions(GameScenario Scenario, string SavePath, bool SmokeTest, bool ExperimentalV14 = false)
{
    public static DesktopOptions Parse(string[] args)
    {
        string? path = null;
        var scenarioId = "civil-growth";
        var smoke = false;
        var experimentalV14 = false;
        for (var index = 0; index < args.Length; index++)
        {
            var option = args[index];
            if (option == "--smoke-test") { smoke = true; continue; }
            if (option == "--experimental-v14") { experimentalV14 = true; continue; }
            if (option is not ("--save-path" or "--scenario") || index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("Use --scenario civil-growth|economy-foundation, --save-path PATH, --smoke-test or --experimental-v14.");
            var value = args[++index];
            if (option == "--save-path") path = value; else scenarioId = value;
        }
        var scenario = scenarioId switch
        {
            "civil-growth" => CivilGrowthScenario.Create(),
            "economy-foundation" => EconomyFoundationScenario.Create(),
            _ => throw new ArgumentException("Unknown scenario. Use civil-growth or economy-foundation.")
        };
        if (experimentalV14 && (scenarioId != "civil-growth" || smoke))
            throw new ArgumentException("Experimental v14 requires --scenario civil-growth and cannot use --smoke-test.");
        path ??= experimentalV14
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SimRisorse", "experiments", "civil-growth-v14.sqlite3")
            : smoke
            ? Path.Combine(Path.GetTempPath(), "SimRisorse.NativeSmoke", Guid.NewGuid().ToString("N"), "game.sqlite3")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SimRisorse",
                scenarioId == "civil-growth" ? "civil-growth.sqlite3" : "game.sqlite3");
        var normalizedPath = Path.GetFullPath(path);
        if (experimentalV14)
        {
            var livePath = Path.GetFullPath(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SimRisorse", "civil-growth.sqlite3"));
            if (string.Equals(normalizedPath, livePath,
                    OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                throw new ArgumentException("The experimental v14 save path must not be the live v13 save path.");
        }
        return new(scenario, normalizedPath, smoke, experimentalV14);
    }
}
