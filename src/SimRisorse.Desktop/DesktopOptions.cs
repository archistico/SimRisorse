using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Content;

namespace SimRisorse.Desktop;

/// <summary>
/// v14 is the default desktop. The legacy v13 application is an explicit compatibility
/// mode and retains its own SQLite slot. No route silently migrates a save file.
/// </summary>
public sealed record DesktopOptions(GameScenario Scenario, string SavePath, bool SmokeTest, bool UseV14 = true)
{
    private const string Usage = "Use --legacy-v13, --v14 (or --experimental-v14), " +
        "--scenario civil-growth|economy-foundation, --save-path PATH or --smoke-test.";

    public static DesktopOptions Parse(string[] args)
    {
        string? path = null;
        var scenarioId = "civil-growth";
        var smoke = false;
        var legacyV13 = false;
        var explicitV14 = false;
        var scenarioSpecified = false;
        for (var index = 0; index < args.Length; index++)
        {
            var option = args[index];
            if (option == "--smoke-test") { smoke = true; continue; }
            if (option == "--legacy-v13") { legacyV13 = true; continue; }
            // Keep the original option working for existing shortcuts and scripts.
            if (option is "--v14" or "--experimental-v14") { explicitV14 = true; continue; }
            if (option is not ("--save-path" or "--scenario") ||
                index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException(Usage);
            var value = args[++index];
            if (option == "--save-path") path = value;
            else { scenarioId = value; scenarioSpecified = true; }
        }

        var scenario = scenarioId switch
        {
            "civil-growth" => CivilGrowthScenario.Create(),
            "economy-foundation" => EconomyFoundationScenario.Create(),
            _ => throw new ArgumentException("Unknown scenario. Use civil-growth or economy-foundation.")
        };
        if (explicitV14 && (legacyV13 || smoke || scenarioId != "civil-growth"))
            throw new ArgumentException("v14 cannot be combined with legacy v13, smoke-test or economy-foundation.");

        // An explicitly selected --scenario keeps the historic v13 behaviour so
        // previously published scripts and native smoke probes do not change meaning.
        var useV14 = explicitV14 || (!legacyV13 && !smoke && !scenarioSpecified);
        var localAppData = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "SimRisorse");
        // Retain the exact v14 path used in RC9-RC30: promotion must not make an
        // existing v14 campaign appear lost by silently selecting a new database.
        var v14Slot = Path.Combine(localAppData, "experiments", "civil-growth-v14.sqlite3");
        var v13CivilSlot = Path.Combine(localAppData, "civil-growth.sqlite3");
        var v13HistoricalSlot = Path.Combine(localAppData, "game.sqlite3");
        path ??= useV14 ? v14Slot : smoke
            ? Path.Combine(Path.GetTempPath(), "SimRisorse.NativeSmoke", Guid.NewGuid().ToString("N"), "game.sqlite3")
            : scenarioId == "civil-growth" ? v13CivilSlot : v13HistoricalSlot;

        var normalizedPath = Path.GetFullPath(path);
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        bool Matches(string reserved) => string.Equals(normalizedPath, Path.GetFullPath(reserved), comparison);
        if (useV14 && (Matches(v13CivilSlot) || Matches(v13HistoricalSlot)))
            throw new ArgumentException("The v14 save path must not be a legacy v13 save path.");
        if (!useV14 && Matches(v14Slot))
            throw new ArgumentException("The legacy v13 save path must not be the v14 save path.");

        return new(scenario, normalizedPath, smoke, useV14);
    }
}
