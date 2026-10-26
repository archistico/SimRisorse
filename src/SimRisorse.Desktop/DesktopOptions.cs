using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Content;

namespace SimRisorse.Desktop;

public sealed record DesktopOptions(GameScenario Scenario, string SavePath, bool SmokeTest)
{
    public static DesktopOptions Parse(string[] args)
    {
        string? path = null;
        var scenarioId = "civil-growth";
        var smoke = false;
        for (var index = 0; index < args.Length; index++)
        {
            var option = args[index];
            if (option == "--smoke-test") { smoke = true; continue; }
            if (option is not ("--save-path" or "--scenario") || index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("Use --scenario civil-growth|economy-foundation, --save-path PATH or --smoke-test.");
            var value = args[++index];
            if (option == "--save-path") path = value; else scenarioId = value;
        }
        var scenario = scenarioId switch
        {
            "civil-growth" => CivilGrowthScenario.Create(),
            "economy-foundation" => EconomyFoundationScenario.Create(),
            _ => throw new ArgumentException("Unknown scenario. Use civil-growth or economy-foundation.")
        };
        path ??= smoke
            ? Path.Combine(Path.GetTempPath(), "SimRisorse.NativeSmoke", Guid.NewGuid().ToString("N"), "game.sqlite3")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SimRisorse",
                scenarioId == "civil-growth" ? "civil-growth.sqlite3" : "game.sqlite3");
        return new(scenario, Path.GetFullPath(path), smoke);
    }
}
