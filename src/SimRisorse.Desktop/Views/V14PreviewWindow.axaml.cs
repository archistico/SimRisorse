using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SimRisorse.Desktop.Lifecycle;
using SimRisorse.Desktop.Presentation;

namespace SimRisorse.Desktop.Views;

/// <summary>Default v14 desktop window. The legacy MainWindow remains available by explicit option.</summary>
public partial class V14PreviewWindow : Window
{
    private readonly DispatcherTimer _timer;
    private readonly DispatcherTimer _autoSaveTimer;
    private readonly PlayClock? _clock;
    private bool _allowClose;

    public V14PreviewWindow()
    {
        _timer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
        { Interval = TimeSpan.FromMilliseconds(100) };
        _autoSaveTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
        { Interval = TimeSpan.FromMinutes(1) };
        AvaloniaXamlLoader.Load(this);
    }

    private void OnBuildingSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        // The TabControl can recreate the Edifici content when changing pages:
        // connect through XAML, not through a reference cached at window startup.
        // A selection notification may also arrive while the template is loading.
        var detail = this.FindControl<ScrollViewer>("V14BuildingDetailScroll");
        if (detail is not null) detail.Offset = default;
        var diagnosis = this.FindControl<ScrollViewer>("V14BuildingDiagnosisScroll");
        if (diagnosis is not null) diagnosis.Offset = default;
        // Do not call Focus(): keyboard navigation remains in the building list.
    }

    public V14PreviewWindow(V14PreviewViewModel viewModel, bool startTimer = true) : this()
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
            // Closing a pending confirmation means canceling it: preserve the
            // existing colony and allow its ordinary closing autosave.
            if (viewModel.IsNewGameConfirmationVisible)
                viewModel.CancelNewGameCommand.Execute(null);
            viewModel.Interrupt();
            // An unrecognised or corrupt v14 checkpoint must never be overwritten;
            // equally, a blocked Save must not trap the player in a window that
            // cannot be closed. No write is attempted when saving is disallowed.
            // A confirmed NEW colony is not automatically persisted: its previous
            // checkpoint stays intact until the player manually chooses Save v14.
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

    private IntPtr PowerMessage(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        // Match the legacy window: suspend / resume notifications always pause the game.
        if (message == 0x218 && wParam.ToInt64() is 4 or 7 or 18) _clock?.Suspend();
        return IntPtr.Zero;
    }

    public void CloseWithoutSaving() { _allowClose = true; Close(); }
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty && WindowState == WindowState.Minimized)
        {
            _clock?.Suspend();
        }
    }
}
