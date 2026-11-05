using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SimRisorse.Desktop.Lifecycle;
using SimRisorse.Desktop.Presentation;

namespace SimRisorse.Desktop.Views;

/// <summary>Default v15 game with the approved v14 presentation. Never opens a v14 store or auto-converts a file.</summary>
public partial class V15PreviewWindow : Window
{
    private readonly DispatcherTimer _timer;
    private readonly DispatcherTimer _autoSaveTimer;
    private readonly PlayClock? _clock;
    private bool _allowClose;

    public V15PreviewWindow()
    {
        _timer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
        { Interval = TimeSpan.FromMilliseconds(100) };
        _autoSaveTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
        { Interval = TimeSpan.FromMinutes(1) };
        AvaloniaXamlLoader.Load(this);
    }

    public V15PreviewWindow(V15PreviewViewModel viewModel, bool startTimer = true) : this()
    {
        DataContext = viewModel;
        _clock = new PlayClock(viewModel.Advance, viewModel.Interrupt);
        Opened += (_, _) =>
        {
            viewModel.Initialize();
            _clock.Reset();
            if (startTimer)
            {
                _timer.Tick += (_, _) => _clock.Pulse();
                _autoSaveTimer.Tick += (_, _) =>
                {
                    _clock.Pulse();
                    if (viewModel.CanAutoSave) viewModel.Save();
                };
                _timer.Start();
                _autoSaveTimer.Start();
            }
        };
        Closing += (_, args) =>
        {
            if (_allowClose) return;
            _clock.Pulse();
            if (viewModel.IsNewGameConfirmationVisible)
                viewModel.CancelNewGameCommand.Execute(null);
            viewModel.Interrupt();
            if (viewModel.CanAutoSave && !viewModel.Save()) args.Cancel = true;
        };
        Closed += (_, _) =>
        {
            _timer.Stop();
            _autoSaveTimer.Stop();
            Win32Properties.RemoveWndProcHookCallback(this, PowerMessage);
        };
        Win32Properties.AddWndProcHookCallback(this, PowerMessage);
    }

    private void OnBuildingSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        var details = this.FindControl<ScrollViewer>("V15BuildingDetailScroll");
        if (details is not null) details.Offset = default;
    }

    private IntPtr PowerMessage(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == 0x218 && wParam.ToInt64() is 4 or 7 or 18) _clock?.Suspend();
        return IntPtr.Zero;
    }

    public void CloseWithoutSaving() { _allowClose = true; Close(); }
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty && WindowState == WindowState.Minimized)
            _clock?.Suspend();
    }
}
