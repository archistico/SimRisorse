using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SimRisorse.Desktop.Lifecycle;
using SimRisorse.Desktop.Presentation;

namespace SimRisorse.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel? _viewModel;
    private readonly PlayClock? _clock;
    private readonly DispatcherTimer _timer;
    private readonly DispatcherTimer _autoSaveTimer;
    private bool _allowClose, _closing;
    public Task Initialization { get; private set; } = Task.CompletedTask;

    public MainWindow()
    {
        _timer = new(DispatcherPriority.Background, Dispatcher) { Interval = TimeSpan.FromMilliseconds(100) };
        _autoSaveTimer = new(DispatcherPriority.Background, Dispatcher) { Interval = TimeSpan.FromMinutes(1) };
        AvaloniaXamlLoader.Load(this);
    }

    public MainWindow(MainViewModel viewModel, bool startTimer = true, bool initialize = true) : this()
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        _clock = new(viewModel.Advance, viewModel.Interrupt);
        if (startTimer)
        {
            viewModel.SynchronizeTime = _clock.Pulse;
            _timer.Tick += (_, _) => _clock.Pulse();
            _autoSaveTimer.Tick += async (_, _) =>
            {
                try { await viewModel.AutoSaveAsync(); }
                catch (Exception exception) { viewModel.UnexpectedError(exception); }
            };
        }
        Opened += (_, _) =>
        {
            _clock.Reset();
            if (startTimer) { _timer.Start(); _autoSaveTimer.Start(); }
            if (initialize) Initialization = InitializeAsync();
        };
        Closing += OnClosing;
        Closed += (_, _) => { _timer.Stop(); _autoSaveTimer.Stop(); viewModel.SynchronizeTime = null; Win32Properties.RemoveWndProcHookCallback(this, PowerMessage); };
        Win32Properties.AddWndProcHookCallback(this, PowerMessage);
    }

    private async Task InitializeAsync()
    {
        try { await _viewModel!.InitializeAsync(); }
        catch (Exception exception) { _viewModel!.UnexpectedError(exception); }
        finally { _clock?.Reset(); }
    }

    private async void OnClosing(object? sender, WindowClosingEventArgs args)
    {
        if (_allowClose || _viewModel is null) return;
        args.Cancel = true;
        if (_closing) return;
        _closing = true;
        _clock?.Pulse();
        try
        {
            if (await _viewModel.PrepareCloseAsync()) { _allowClose = true; Close(); }
        }
        catch (Exception exception) { _viewModel.ReportCloseFailure(exception); }
        finally { _closing = false; }
    }

    public void CloseWithoutSaving() { _allowClose = true; Close(); }
    internal void AllowImmediateClose() => _allowClose = true;
    private void DiscardCloseClick(object? sender, RoutedEventArgs args) => CloseWithoutSaving();

    private IntPtr PowerMessage(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        // WM_POWERBROADCAST: suspend, resume-suspend and automatic resume.
        if (message == 0x218 && wParam.ToInt64() is 4 or 7 or 18) _clock?.Suspend();
        return IntPtr.Zero;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty && WindowState == WindowState.Minimized) _clock?.Suspend();
    }
}
