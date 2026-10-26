using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SimRisorse.Application;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Sessions;
using SimRisorse.Desktop;
using SimRisorse.Desktop.Lifecycle;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(SimRisorse.Desktop.Tests.TestAppBuilder))]

namespace SimRisorse.Desktop.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UseSkia().UseHarfBuzz()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}

public sealed class DesktopTests
{
    [Fact]
    public void ScenariosHaveDistinctPersonalSlotsAndSmokeTestsUsePrivatePaths()
    {
        var current = DesktopOptions.Parse([]);
        var historical = DesktopOptions.Parse(["--scenario", "economy-foundation"]);
        Assert.Equal("civil-growth", current.Scenario.Id);
        Assert.EndsWith("civil-growth.sqlite3", current.SavePath);
        Assert.EndsWith("game.sqlite3", historical.SavePath);
        Assert.NotEqual(current.SavePath, historical.SavePath);
        var smoke = DesktopOptions.Parse(["--smoke-test"]);
        Assert.NotEqual(current.SavePath, smoke.SavePath);
        Assert.NotEqual(historical.SavePath, smoke.SavePath);
        Assert.Contains("SimRisorse.NativeSmoke", smoke.SavePath);
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--scenario", "unknown"]));
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--save-path", "--smoke-test"]));
    }

    [AvaloniaFact]
    public async Task HousesGrowthAndFoodConsumptionRemainVisibleAndResumeExactly()
    {
        using var directory = new TemporaryDirectory();
        var store = new SqliteGameSaveStore(Path.Combine(directory.PathName, "civil.sqlite3"));
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), store);
        await viewModel.InitializeAsync();
        var window = new MainWindow(viewModel, startTimer: false, initialize: false);
        try
        {
            window.Show();
            Assert.Contains("8 / 8", viewModel.Population);
            Assert.Contains("Case piene", window.FindControl<TextBlock>("GrowthStatusText")!.Text!);
            viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "small-house");
            Assert.Contains("posti abitativi", viewModel.Selected.Recipe);
            Assert.Contains("posti abitativi", viewModel.Selected.Status);
            Assert.False(window.FindControl<Button>("AssignButton")!.IsEffectivelyEnabled);
            Click(window, "BuildButton");
            Assert.Contains("8 / 16", viewModel.Population);
            Click(window, "PauseButton");
            viewModel.Advance(TimeSpan.FromMilliseconds(59500));
            Assert.Contains("59 / 60", viewModel.GrowthStatus);
            Assert.True(await viewModel.SaveAsync());
            viewModel.NewGameCommand.Execute(null);
            viewModel.ConfirmNewGameCommand.Execute(null);
            Assert.Equal("civil-growth", viewModel.View.ScenarioId);
            Assert.Contains("8 / 8", viewModel.Population);
            Assert.True(await viewModel.LoadAsync());
            Assert.Contains("59 / 60", viewModel.GrowthStatus);
            Assert.True(viewModel.IsPaused);
            Click(window, "PauseButton");
            viewModel.Advance(TimeSpan.FromMilliseconds(500));
            Assert.Equal(9, viewModel.View.Population);
            Assert.Equal(0.09m, viewModel.View.Resources.Single(item => item.Id == "food").ConsumptionPerSecond);
            Assert.Equal(25.2m, viewModel.View.Resources.Single(item => item.Id == "food").Quantity);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public async Task FoodShortageIsExplainedWithoutRemovingWorkers()
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        viewModel.TogglePauseCommand.Execute(null);
        viewModel.Advance(TimeSpan.FromSeconds(376));
        Assert.Equal(8, viewModel.View.Population);
        Assert.Equal(5, viewModel.View.AvailableWorkers);
        Assert.Contains("Manca cibo", viewModel.GrowthStatus);
    }

    [AvaloniaTheory]
    [InlineData(1240, 850, 0, "civil-buildings.png")]
    [InlineData(1040, 740, 0, "civil-buildings-compact.png")]
    [InlineData(1240, 850, 1, "civil-resources.png")]
    [InlineData(1040, 740, 1, "civil-resources-compact.png")]
    public async Task CivilScreensRenderHousingAndFoodWithPrimaryControlsVisible(int width, int height, int section, string file)
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "small-house");
        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = width, Height = height };
        try
        {
            window.Show();
            window.FindControl<TabControl>("Sections")!.SelectedIndex = section;
            Dispatcher.UIThread.RunJobs();
            foreach (var name in section == 0 ? new[] { "BuildButton", "AssignButton", "PauseButton", "SaveButton" } : new[] { "PauseButton", "SaveButton" })
            {
                var button = window.FindControl<Button>(name)!;
                var point = button.TranslatePoint(default, window)!.Value;
                Assert.InRange(point.Y, 0, window.ClientSize.Height - button.Bounds.Height);
            }
            Assert.True(window.FindControl<TextBlock>("GrowthStatusText")!.IsEffectivelyVisible);
            var growthBar = window.FindControl<ProgressBar>("GrowthProgressBar")!;
            var growthPosition = growthBar.TranslatePoint(default, window)!.Value;
            Assert.InRange(growthPosition.X, 22, 185 - growthBar.Bounds.Width);
            using var image = window.CaptureRenderedFrame();
            Assert.NotNull(image);
            var artifacts = Environment.GetEnvironmentVariable("SIMRISORSE_DESKTOP_ARTIFACTS");
            if (!string.IsNullOrWhiteSpace(artifacts))
            {
                Directory.CreateDirectory(artifacts);
                image.Save(Path.Combine(artifacts, file), PngBitmapEncoderOptions.Default);
            }
        }
        finally { window.CloseWithoutSaving(); }
    }

    [Fact]
    public void DesktopDoesNotCallTheSQLiteDriverDirectly() => Assert.DoesNotContain(
        typeof(MainViewModel).Assembly.GetReferencedAssemblies(), reference => reference.Name!.StartsWith("Microsoft.Data.Sqlite", StringComparison.Ordinal));

    [AvaloniaFact]
    public async Task SavingKeepsTheClockAndPauseControlResponsive()
    {
        using var store = new BlockingStore();
        var viewModel = Create(store);
        viewModel.TogglePauseCommand.Execute(null);
        var save = viewModel.SaveAsync();
        try
        {
            await store.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            viewModel.Advance(TimeSpan.FromSeconds(1));
            Assert.Equal("00:00:01", viewModel.Clock);
            Assert.True(viewModel.TogglePauseCommand.CanExecute(null));
            viewModel.TogglePauseCommand.Execute(null);
            Assert.True(viewModel.IsPaused);
        }
        finally { store.Release.Set(); await save; }
        Assert.False(viewModel.IsBusy);
    }

    [AvaloniaFact]
    public void BulkOrdersUseTheDefinitiveRuntimeAndRespectPopulation()
    {
        var viewModel = Create(new MemoryStore());
        var window = new MainWindow(viewModel, startTimer: false, initialize: false);
        try
        {
            window.Show();
            window.FindControl<ComboBox>("BatchSelector")!.SelectedItem = 5;
            Click(window, "BuildButton");
            Assert.Equal(6, viewModel.Selected.Count);
            Click(window, "AssignButton");
            Assert.Equal(6, viewModel.Selected.Workers);
            Assert.Equal(0, viewModel.View.AvailableWorkers);
            Assert.False(window.FindControl<Button>("AssignButton")!.IsEffectivelyEnabled);
            Click(window, "PauseButton");
            viewModel.Advance(TimeSpan.FromSeconds(1));
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("00:00:01", window.FindControl<TextBlock>("ClockText")!.Text);
            Assert.Equal(12m, viewModel.View.Resources.Single(item => item.Id == "logs").Quantity);
            Assert.Equal(20m, viewModel.View.Resources.Single(item => item.Id == "planks").Quantity);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public void ResourceTabShowsExplicitRatesFromTheForecast()
    {
        var viewModel = Create(new MemoryStore());
        var window = new MainWindow(viewModel, startTimer: false, initialize: false);
        try
        {
            window.Show();
            window.FindControl<TabControl>("Sections")!.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.True(window.FindControl<ItemsControl>("ResourcesTable")!.IsEffectivelyVisible);
            Assert.Equal("+2 /s", viewModel.Resources.Single(item => item.Id == "logs").NetRate);
            Assert.Equal("±0 /s", viewModel.Resources.Single(item => item.Id == "planks").NetRate);
            viewModel.TogglePauseCommand.Execute(null);
            viewModel.Advance(TimeSpan.FromSeconds(1));
            Assert.Equal("±0 /s", viewModel.Resources.Single(item => item.Id == "logs").NetRate);
            Assert.Equal("+1 /s", viewModel.Resources.Single(item => item.Id == "planks").NetRate);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public void NewGameRequiresConfirmationAndCancelPreservesTheSession()
    {
        var viewModel = Create(new MemoryStore());
        viewModel.ConstructCommand.Execute(null);
        viewModel.NewGameCommand.Execute(null);
        Assert.True(viewModel.IsNewGameConfirmationVisible);
        Assert.Equal(2, viewModel.Selected.Count);
        viewModel.CancelNewGameCommand.Execute(null);
        Assert.Equal(2, viewModel.Selected.Count);
        viewModel.NewGameCommand.Execute(null);
        viewModel.ConfirmNewGameCommand.Execute(null);
        Assert.Equal(1, viewModel.Selected.Count);
        Assert.True(viewModel.IsPaused);
    }

    [AvaloniaFact]
    public async Task ClosingSavesAndAnotherWindowRestoresTheExactPausedCheckpoint()
    {
        using var directory = new TemporaryDirectory();
        var store = new SqliteGameSaveStore(Path.Combine(directory.PathName, "game.sqlite3"));
        var first = Create(store);
        await first.InitializeAsync();
        var window = new MainWindow(first, startTimer: false, initialize: false);
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        window.Closed += (_, _) => closed.TrySetResult();
        try
        {
            window.Show();
            Click(window, "BuildButton");
            Click(window, "AssignButton");
            first.TogglePauseCommand.Execute(null);
            first.Advance(TimeSpan.FromMilliseconds(1750));
            first.Pause();
            var expected = first.View;
            window.Close();
            await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var restored = Create(store);
            await restored.InitializeAsync();
            Assert.True(restored.IsPaused);
            Assert.Equal(expected.ElapsedTicks, restored.View.ElapsedTicks);
            Assert.Equal(2, restored.Selected.Count);
            Assert.Equal(2, restored.Selected.Workers);
            restored.Advance(TimeSpan.FromHours(8));
            Assert.Equal(expected.ElapsedTicks, restored.View.ElapsedTicks);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public async Task FailedCloseKeepsTheWindowAndTheUnsavedStateAvailable()
    {
        var store = new MemoryStore { FailWrites = true };
        var viewModel = Create(store);
        await viewModel.InitializeAsync();
        var window = new MainWindow(viewModel, startTimer: false, initialize: false);
        try
        {
            window.Show();
            Click(window, "BuildButton");
            window.Close();
            await UntilAsync(() => viewModel.IsCloseFailureVisible);
            Assert.True(window.IsVisible);
            Assert.Equal(2, viewModel.Selected.Count);
            Assert.True(viewModel.IsPaused);
            Assert.True(window.FindControl<Button>("DiscardCloseButton")!.IsVisible);
            store.FailWrites = false;
            Assert.True(await viewModel.SaveAsync());
            Assert.False(viewModel.IsCloseFailureVisible);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public async Task IncompatibleStartupDoesNotOverwriteTheExistingSaveOnClose()
    {
        var store = new MemoryStore { LoadError = GameSaveError.UnsupportedFormat };
        var viewModel = Create(store);
        await viewModel.InitializeAsync();
        Assert.False(await viewModel.AutoSaveAsync());
        Assert.True(await viewModel.PrepareCloseAsync());
        Assert.Equal(0, store.WriteCount);
    }

    [AvaloniaFact]
    public async Task PeriodicCheckpointPreservesRunningStateAndSkipsNewGameConfirmation()
    {
        var store = new MemoryStore();
        var viewModel = Create(store);
        await viewModel.InitializeAsync();
        viewModel.TogglePauseCommand.Execute(null);
        viewModel.Advance(TimeSpan.FromMilliseconds(500));
        Assert.True(await viewModel.AutoSaveAsync());
        Assert.False(viewModel.IsPaused);
        Assert.Equal(TimeSpan.FromMilliseconds(500).Ticks, store.Snapshot!.ElapsedTicks);
        viewModel.NewGameCommand.Execute(null);
        Assert.False(await viewModel.AutoSaveAsync());
        Assert.Equal(1, store.WriteCount);
    }

    [AvaloniaFact]
    public async Task ClockDisplaysTotalHoursAndPreciseResourceQuantities()
    {
        var state = GameSession.Create(EconomyFoundationScenario.Create()).Snapshot() with
        { ElapsedTicks = TimeSpan.FromHours(32).Ticks, Resources = SimRisorse.Core.Economy.ResourceBundle.Create(("logs", 1.234m)) };
        var viewModel = Create(new MemoryStore { Snapshot = state });
        await viewModel.InitializeAsync();
        Assert.Equal("32:00:00", viewModel.Clock);
        Assert.Equal("1,234", viewModel.Resources.Single(item => item.Id == "logs").Quantity);
    }

    [AvaloniaFact]
    public void MonotonicClockDiscardsSuspensionAndCountsOnlyActiveTime()
    {
        var viewModel = Create(new MemoryStore());
        var time = new ManualTime();
        var clock = new PlayClock(viewModel.Advance, viewModel.Interrupt, time);
        viewModel.SynchronizeTime = clock.Pulse;
        clock.Reset();
        viewModel.TogglePauseCommand.Execute(null);
        time.Advance(TimeSpan.FromSeconds(1)); clock.Pulse();
        Assert.Equal(TimeSpan.TicksPerSecond, viewModel.View.ElapsedTicks);
        time.Advance(TimeSpan.FromHours(8)); clock.Pulse();
        Assert.True(viewModel.IsPaused);
        Assert.Equal(TimeSpan.TicksPerSecond, viewModel.View.ElapsedTicks);
        viewModel.TogglePauseCommand.Execute(null);
        time.Advance(TimeSpan.FromMilliseconds(500)); clock.Pulse();
        Assert.Equal(TimeSpan.FromMilliseconds(1500).Ticks, viewModel.View.ElapsedTicks);
        clock.Suspend();
        Assert.True(viewModel.IsPaused);
    }

    [AvaloniaFact]
    public void MinimizingPausesTheSession()
    {
        var viewModel = Create(new MemoryStore());
        var window = new MainWindow(viewModel, startTimer: false, initialize: false);
        try
        {
            window.Show();
            viewModel.TogglePauseCommand.Execute(null);
            window.WindowState = WindowState.Minimized;
            Assert.True(viewModel.IsPaused);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaTheory]
    [InlineData(1240, 850, 0, "buildings.png")]
    [InlineData(1040, 740, 0, "buildings-compact.png")]
    [InlineData(1240, 850, 1, "resources.png")]
    [InlineData(1040, 740, 1, "resources-compact.png")]
    public async Task ScreensRenderWithVisiblePrimaryControls(int width, int height, int section, string file)
    {
        var viewModel = Create(new MemoryStore());
        await viewModel.InitializeAsync();
        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = width, Height = height };
        try
        {
            window.Show();
            window.FindControl<TabControl>("Sections")!.SelectedIndex = section;
            Dispatcher.UIThread.RunJobs();
            using var image = window.CaptureRenderedFrame();
            Assert.NotNull(image);
            Assert.True(image.PixelSize.Width >= width);
            foreach (var name in section == 0 ? new[] { "BuildButton", "AssignButton", "ReleaseButton", "PauseButton", "SaveButton", "LoadButton" } : new[] { "PauseButton", "SaveButton", "LoadButton" })
            {
                var button = window.FindControl<Button>(name)!;
                var position = button.TranslatePoint(default, window)!.Value;
                Assert.InRange(position.Y, 0, window.ClientSize.Height - button.Bounds.Height);
            }
            var artifacts = Environment.GetEnvironmentVariable("SIMRISORSE_DESKTOP_ARTIFACTS");
            if (!string.IsNullOrWhiteSpace(artifacts)) { Directory.CreateDirectory(artifacts); image.Save(Path.Combine(artifacts, file), PngBitmapEncoderOptions.Default); }
        }
        finally { window.CloseWithoutSaving(); }
    }

    private static MainViewModel Create(IGameSaveStore store) => new(new GameRuntime(EconomyFoundationScenario.Create()), store);
    private static void Click(Window window, string name)
    {
        Dispatcher.UIThread.RunJobs();
        var button = window.FindControl<Button>(name)!;
        Assert.True(button.IsEffectivelyEnabled);
        var point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
        window.MouseDown(point, MouseButton.Left); window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }
    private static async Task UntilAsync(Func<bool> predicate)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!predicate()) await Task.Delay(10, timeout.Token);
    }

    private sealed class ManualTime : TimeProvider
    {
        private long _ticks;
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override long GetTimestamp() => _ticks;
        public void Advance(TimeSpan elapsed) => _ticks += elapsed.Ticks;
    }

    private sealed class MemoryStore : IGameSaveStore
    {
        public GameSnapshot? Snapshot { get; set; }
        public bool FailWrites { get; set; }
        public GameSaveError? LoadError { get; init; }
        public int WriteCount { get; private set; }
        public GameSnapshot Load() => LoadError is { } error ? throw new GameSaveException(error, "Unavailable") :
            Snapshot ?? throw new GameSaveException(GameSaveError.NotFound, "Missing");
        public void Save(GameSnapshot snapshot)
        {
            WriteCount++;
            if (FailWrites) throw new GameSaveException(GameSaveError.Unavailable, "Unavailable");
            Snapshot = snapshot;
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public string PathName { get; } = Path.Combine(Path.GetTempPath(), "SimRisorse.M3.DesktopTests", Guid.NewGuid().ToString("N"));
        public void Dispose() { if (Directory.Exists(PathName)) Directory.Delete(PathName, recursive: true); }
    }

    private sealed class BlockingStore : IGameSaveStore, IDisposable
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public ManualResetEventSlim Release { get; } = new();
        public void Save(GameSnapshot snapshot) { Started.TrySetResult(); if (!Release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException(); }
        public GameSnapshot Load() => throw new GameSaveException(GameSaveError.NotFound, "Missing");
        public void Dispose() => Release.Dispose();
    }
}
