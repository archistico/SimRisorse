using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using SimRisorse.StackProbe.Desktop;
using SimRisorse.StackProbe.Model;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(SimRisorse.StackProbe.Tests.TestAppBuilder))]

namespace SimRisorse.StackProbe.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}

public sealed class DesktopTests
{
    [AvaloniaFact]
    public void BuildingSelectionOrdersAndClockAreBoundToTheWindow()
    {
        using var database = new TemporaryDatabase();
        var viewModel = new ProbeViewModel(database.Store);
        var window = new MainWindow(viewModel, startTimer: false);
        try
        {
            window.Show();
            Click(window, "BuildButton");
            Assert.Equal(2, viewModel.Selected.Count);
            Click(window, "AssignButton");
            Assert.Equal(2, viewModel.Selected.Workers);
            Click(window, "PauseButton");
            viewModel.Advance(TimeSpan.FromSeconds(1));
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("00:00:01", window.FindControl<TextBlock>("ClockText")!.Text);
            Assert.Equal("100", window.FindControl<TextBlock>("PlanksText")!.Text);
            window.FindControl<ListBox>("BuildingsList")!.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(BuildingKind.Sawmill, viewModel.Selected.Kind);
            Assert.Equal("IN CORSO", viewModel.PlayState);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task DesktopSaveLoadResumesPausedAndAFailedLoadKeepsTheCurrentSession()
    {
        using var database = new TemporaryDatabase();
        var viewModel = new ProbeViewModel(database.Store);
        viewModel.TogglePauseCommand.Execute(null);
        viewModel.Advance(TimeSpan.FromMilliseconds(1500));
        var saved = JsonSerializer.Serialize(viewModel.Snapshot);
        await viewModel.SaveAsync();
        Assert.Contains("salvata", viewModel.Message);
        viewModel.Advance(TimeSpan.FromMilliseconds(1500));
        await viewModel.LoadAsync();
        Assert.Equal(saved, JsonSerializer.Serialize(viewModel.Snapshot));
        Assert.Equal("IN PAUSA", viewModel.PlayState);
        database.Execute("UPDATE save_metadata SET rules_version='incompatible' WHERE slot=1;");
        await viewModel.LoadAsync();
        Assert.Equal(saved, JsonSerializer.Serialize(viewModel.Snapshot));
        Assert.Contains("non riuscita", viewModel.Message);
    }

    [AvaloniaFact]
    public async Task ClockDisplaysTotalHoursBeyondOneDay()
    {
        using var database = new TemporaryDatabase();
        database.Store.Save(ProbeSession.Create().Snapshot() with { ElapsedTicks = TimeSpan.FromHours(32).Ticks });
        var viewModel = new ProbeViewModel(database.Store);
        await viewModel.LoadAsync();
        Assert.Equal("32:00:00", viewModel.Clock);
    }

    [AvaloniaFact]
    public async Task StorageWorkDoesNotBlockTheDispatcher()
    {
        using var database = new TemporaryDatabase();
        using var gate = new BlockingStore(database.Store);
        var viewModel = new ProbeViewModel(gate);
        viewModel.TogglePauseCommand.Execute(null);
        var saveTask = viewModel.SaveAsync();
        try
        {
            Assert.True(await Task.Run(() => gate.Started.Wait(TimeSpan.FromSeconds(5))));
            viewModel.Advance(TimeSpan.FromSeconds(1));
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("00:00:01", viewModel.Clock);
            Assert.False(viewModel.SaveCommand.CanExecute(null));
        }
        finally { gate.Release.Set(); await saveTask; }
        Assert.Contains("salvata", viewModel.Message);
        Assert.Equal(0, database.Store.Load().ElapsedTicks);
    }

    [AvaloniaTheory]
    [InlineData(1180, 790, "main-window.png")]
    [InlineData(1020, 720, "compact-window.png")]
    public void WindowRendersAtSupportedSizes(int width, int height, string filename)
    {
        using var database = new TemporaryDatabase();
        var window = new MainWindow(new(database.Store), startTimer: false) { Width = width, Height = height };
        try
        {
            window.Show();
            using var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            Assert.True(frame.PixelSize.Width >= width);
            Assert.True(frame.PixelSize.Height >= height);
            foreach (var name in new[] { "BuildButton", "AssignButton", "ReleaseButton", "PauseButton", "SaveButton", "LoadButton" })
            {
                var button = window.FindControl<Button>(name)!;
                var position = button.TranslatePoint(default, window)!.Value;
                Assert.True(position.Y >= 0 && position.Y + button.Bounds.Height <= window.ClientSize.Height, name);
            }
            var directory = Environment.GetEnvironmentVariable("SIMRISORSE_PROBE_ARTIFACTS");
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
                frame.Save(Path.Combine(directory, filename));
            }
        }
        finally { window.Close(); }
    }

    private static void Click(Window window, string name)
    {
        Dispatcher.UIThread.RunJobs();
        var button = window.FindControl<Button>(name)!;
        Assert.True(button.IsEnabled);
        var point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class BlockingStore(IProbeSnapshotStore inner) : IProbeSnapshotStore, IDisposable
    {
        public ManualResetEventSlim Started { get; } = new();
        public ManualResetEventSlim Release { get; } = new();
        public void Save(ProbeSnapshot snapshot)
        {
            Started.Set();
            if (!Release.Wait(TimeSpan.FromSeconds(10))) throw new IOException("Storage gate timed out.");
            inner.Save(snapshot);
        }
        public ProbeSnapshot Load() => inner.Load();
        public void Dispose() { Started.Dispose(); Release.Dispose(); }
    }
}
