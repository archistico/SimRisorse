using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace SimRisorse.StackProbe.Desktop;

public partial class MainWindow : Window
{
    private readonly ProbeViewModel? _viewModel;
    private readonly Stopwatch _watch = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(100) };

    public MainWindow() => AvaloniaXamlLoader.Load(this);

    public MainWindow(ProbeViewModel viewModel, bool startTimer = true) : this()
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        if (startTimer)
        {
            viewModel.SynchronizeTime = ReadClock;
            _timer.Tick += (_, _) => ReadClock();
            Opened += (_, _) => { _watch.Start(); _timer.Start(); };
        }
        Closing += (_, _) => { if (_watch.IsRunning) ReadClock(); viewModel.Pause(); _timer.Stop(); };
        Closed += (_, _) => { viewModel.SynchronizeTime = null; };
    }

    private void ReadClock()
    {
        var elapsed = _watch.Elapsed;
        _watch.Restart();
        _viewModel?.Advance(elapsed);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty && WindowState == WindowState.Minimized && _viewModel is not null)
            _viewModel.Pause();
    }
}
