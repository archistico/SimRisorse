using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SimRisorse.StackProbe.Persistence;

namespace SimRisorse.StackProbe.Desktop;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SimRisorse", "StackProbe", "save.sqlite3");
            var viewModel = new ProbeViewModel(new SqliteProbeSnapshotStore(path));
            desktop.MainWindow = new MainWindow(viewModel);
            if (desktop.Args?.Contains("--smoke-test", StringComparer.Ordinal) == true)
            {
                desktop.MainWindow.Opened += (_, _) => DispatcherTimer.RunOnce(() =>
                {
                    viewModel.TogglePauseCommand.Execute(null);
                    viewModel.ConstructCommand.Execute(null);
                    viewModel.Advance(TimeSpan.FromSeconds(1));
                    if (viewModel.Snapshot.ElapsedTicks < TimeSpan.TicksPerSecond ||
                        viewModel.Selected.Count != 2)
                        desktop.Shutdown(1);
                    else
                        desktop.Shutdown(0);
                }, TimeSpan.FromMilliseconds(300));
            }
        }
        base.OnFrameworkInitializationCompleted();
    }
}
