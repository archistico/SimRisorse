using Avalonia;

namespace SimRisorse.Desktop;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        try { return AppBuilder.Configure<App>().UsePlatformDetect().StartWithClassicDesktopLifetime(args); }
        catch (ArgumentException exception) { Console.Error.WriteLine(exception.Message); return 2; }
    }
}
