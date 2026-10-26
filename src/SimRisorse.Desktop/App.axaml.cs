using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SimRisorse.Application;
using SimRisorse.Application.Scenarios;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using SimRisorse.Infrastructure.Persistence;

namespace SimRisorse.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var args = desktop.Args ?? [];
            var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SimRisorse", "game.sqlite3");
            var pathIndex = Array.IndexOf(args, "--save-path");
            if (pathIndex >= 0 && pathIndex + 1 < args.Length) savePath = args[pathIndex + 1];
            else if (args.Contains("--smoke-test", StringComparer.Ordinal))
                savePath = Path.Combine(Path.GetTempPath(), "SimRisorse.NativeSmoke", Guid.NewGuid().ToString("N"), "game.sqlite3");
            var viewModel = new MainViewModel(new GameRuntime(EconomyFoundationScenario.Create()), new SqliteGameSaveStore(savePath));
            var window = new MainWindow(viewModel);
            desktop.MainWindow = window;
            if (args.Contains("--smoke-test", StringComparer.Ordinal))
                window.Opened += async (_, _) => await NativeSmokeAsync(desktop, window, viewModel);
        }
        base.OnFrameworkInitializationCompleted();
    }

    private static async Task NativeSmokeAsync(IClassicDesktopStyleApplicationLifetime desktop, MainWindow window, MainViewModel viewModel)
    {
        try
        {
            await window.Initialization;
            viewModel.ConstructCommand.Execute(null);
            viewModel.AssignCommand.Execute(null);
            viewModel.TogglePauseCommand.Execute(null);
            viewModel.Advance(TimeSpan.FromSeconds(1));
            viewModel.Pause();
            var expected = viewModel.View.ElapsedTicks;
            if (!await viewModel.SaveAsync()) throw new InvalidOperationException("Native save failed.");
            viewModel.NewGameCommand.Execute(null);
            viewModel.ConfirmNewGameCommand.Execute(null);
            if (!await viewModel.LoadAsync()) throw new InvalidOperationException("Native load failed.");
            if (!viewModel.IsPaused || viewModel.View.ElapsedTicks != expected || viewModel.Selected.Count != 2 || viewModel.Selected.Workers != 2)
                throw new InvalidOperationException("Native resume differs from saved state.");
            if (OperatingSystem.IsWindows())
            {
                viewModel.TogglePauseCommand.Execute(null);
                SendMessage(window.TryGetPlatformHandle()!.Handle, 0x218, new IntPtr(4), IntPtr.Zero);
                if (!viewModel.IsPaused) throw new InvalidOperationException("Native suspend notification was ignored.");
            }
            window.Close(); // Normal asynchronous close path must save successfully.
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            window.AllowImmediateClose();
            desktop.Shutdown(1);
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
}
