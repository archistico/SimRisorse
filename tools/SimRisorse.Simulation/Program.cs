using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimRisorse.Application;
using SimRisorse.Application.Features.Sessions.AdvanceTime;
using SimRisorse.Application.Features.Sessions.GetGameState;
using SimRisorse.Application.Features.Sessions.SetPause;
using SimRisorse.Application.Scenarios;

if (args is ["--help"])
{
    Console.WriteLine("SimRisorse.Simulation [--seconds N | --hours N] (maximum 168 hours; default 60 seconds)");
    return 0;
}
var seconds = 60m;
if (args.Length != 0)
{
    if (args.Length != 2 || args[0] is not ("--seconds" or "--hours") ||
        !decimal.TryParse(args[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value) ||
        value < 0 || value > (args[0] == "--hours" ? 168m : 604800m))
    {
        Console.Error.WriteLine("Use --seconds N or --hours N with a nonnegative duration of at most 168 hours.");
        return 2;
    }
    seconds = args[0] == "--hours" ? value * 3600m : value;
}
var ticks = seconds * TimeSpan.TicksPerSecond;
if (ticks != decimal.Truncate(ticks))
{
    Console.Error.WriteLine("Duration must be representable in 100-nanosecond ticks.");
    return 2;
}
var runtime = new GameRuntime(EconomyFoundationScenario.Create());
new SetPauseHandler(runtime).Handle(new(false));
var remainingTicks = (long)ticks;
var advance = new AdvanceTimeHandler(runtime);
while (remainingTicks > 0)
{
    var batch = Math.Min(remainingTicks, TimeSpan.TicksPerHour);
    var result = advance.Handle(new(TimeSpan.FromTicks(batch)));
    if (!result.Succeeded) { Console.Error.WriteLine(result.Error); return 1; }
    remainingTicks -= batch;
}
new SetPauseHandler(runtime).Handle(new(true));
var view = new GetGameStateHandler(runtime).Handle();
Console.WriteLine(JsonSerializer.Serialize(view, new JsonSerializerOptions
{
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() }
}));
return 0;
