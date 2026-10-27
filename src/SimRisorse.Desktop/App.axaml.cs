using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SimRisorse.Application;
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
            var options = DesktopOptions.Parse(desktop.Args ?? []);
            var viewModel = new MainViewModel(new GameRuntime(options.Scenario), new SqliteGameSaveStore(options.SavePath));
            var window = new MainWindow(viewModel);
            desktop.MainWindow = window;
            if (options.SmokeTest)
                window.Opened += async (_, _) => await NativeSmokeAsync(desktop, window, viewModel);
        }
        base.OnFrameworkInitializationCompleted();
    }

    private static async Task NativeSmokeAsync(IClassicDesktopStyleApplicationLifetime desktop, MainWindow window, MainViewModel viewModel)
    {
        try
        {
            await window.Initialization;
            var growth = viewModel.HasPopulationGrowth;
            if (growth) viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "small-house");
            viewModel.ConstructCommand.Execute(null);
            if (!growth) viewModel.AssignCommand.Execute(null);
            viewModel.TogglePauseCommand.Execute(null);
            viewModel.Advance(TimeSpan.FromSeconds(growth ? 52 : 1));
            viewModel.Pause();
            var expected = viewModel.View.ElapsedTicks;
            if (!await viewModel.SaveAsync()) throw new InvalidOperationException("Native save failed.");
            viewModel.NewGameCommand.Execute(null);
            viewModel.ConfirmNewGameCommand.Execute(null);
            if (!await viewModel.LoadAsync()) throw new InvalidOperationException("Native load failed.");
            if (growth) viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "small-house");
            if (!viewModel.IsPaused || viewModel.View.ElapsedTicks != expected || viewModel.Selected.Count != 2 ||
                viewModel.Selected.Workers != (growth ? 0 : 2) || (growth && viewModel.View.PopulationState.ProgressSeconds != 59))
                throw new InvalidOperationException("Native resume differs from saved state.");
            if (growth)
            {
                viewModel.TogglePauseCommand.Execute(null);
                viewModel.Advance(TimeSpan.FromSeconds(1));
                viewModel.Pause();
                if (viewModel.View.Population != 9 || viewModel.View.PopulationState.ProgressSeconds != 0)
                    throw new InvalidOperationException("Native population growth differs after resume.");
            }
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
