using System.Text.Json;
using SimRisorse.IndustryProbe;

if (args is ["--help"])
{
    Console.WriteLine("IndustryProbe: experimental industrial cost hypotheses A/B; no arguments writes the audited comparison as JSON.");
    Console.WriteLine("Uses a synthetic, already established civilian colony. Does not load or save games, or change civil-growth rules.");
    return 0;
}
if (args.Length != 0)
{
    Console.Error.WriteLine("Use no arguments for the comparison, or --help.");
    return 2;
}
try
{
    Console.WriteLine(JsonSerializer.Serialize(IndustryBenchmark.Run(), new JsonSerializerOptions { WriteIndented = true }));
    return 0;
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
