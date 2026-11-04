using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using Xunit;

namespace SimRisorse.Desktop.Tests;

/// <summary>
/// G1-D42: keyboard focus must be visible without altering the selection fill
/// or the layout. Sidebar arrows must still switch the corresponding tab.
/// The Windows TAB/Shift+TAB and screen-reader gate remains manual (G1-D43).
/// </summary>
public sealed class V14KeyboardFocusAuditTests
{
    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public void KeyboardFocusMarksSidebarAndArrowDownOpensResourceSection(int width, int height)
    {
        var path = Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.Focus.Menu." + Guid.NewGuid().ToString("N") + ".sqlite3");
        var vm = V14PreviewViewModel.CreateForPreview(path);
        var window = new V14PreviewWindow(vm, startTimer: false) { Width = width, Height = height };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var navigation = window.FindControl<ListBox>("V14NavigationMenu")!;
            var sections = window.FindControl<TabControl>("V14Sections")!;
            var first = Assert.IsType<ListBoxItem>(navigation.ContainerFromIndex(0));
            var firstPresenter = Presenter(first);
            var baseline = (first.Bounds, firstPresenter.Bounds);
            Assert.True(first.Focus());
            Dispatcher.UIThread.RunJobs();
            Assert.True(first.IsFocused);
            Assert.Equal(Color.Parse("#F2D49B"), BrushColor(firstPresenter.BorderBrush));
            Assert.Equal(baseline, (first.Bounds, firstPresenter.Bounds));

            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            window.KeyRelease(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, navigation.SelectedIndex);
            Assert.Equal(1, sections.SelectedIndex);
            Assert.True(window.FindControl<ListBox>("V14Resources")!.IsEffectivelyVisible);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public void FocusedResourceRetainsSelectionOchreAndDoesNotReflowRow(int width, int height)
    {
        var path = Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.Focus.Resources." + Guid.NewGuid().ToString("N") + ".sqlite3");
        var vm = V14PreviewViewModel.CreateForPreview(path);
        var window = new V14PreviewWindow(vm, startTimer: false) { Width = width, Height = height };
        try
        {
            window.Show();
            window.FindControl<ListBox>("V14NavigationMenu")!.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            var resources = window.FindControl<ListBox>("V14Resources")!;
            resources.SelectedIndex = 0;
            Dispatcher.UIThread.RunJobs();
            var entry = Assert.IsType<ListBoxItem>(resources.ContainerFromIndex(0));
            var presenter = Presenter(entry);
            var baseline = (entry.Bounds, presenter.Bounds);
            var selectedResource = vm.SelectedResource;
            Assert.NotNull(selectedResource);
            Assert.True(entry.Focus());
            Dispatcher.UIThread.RunJobs();
            Assert.True(entry.IsFocused);
            Assert.Equal(Color.Parse("#9A6947"), BrushColor(presenter.BorderBrush));
            Assert.Equal(Color.Parse("#E6D5AC"), BrushColor(presenter.Background));
            Assert.Equal(baseline, (entry.Bounds, presenter.Bounds));
            Assert.Same(selectedResource, vm.SelectedResource);
        }
        finally { window.CloseWithoutSaving(); }
    }

    private static ContentPresenter Presenter(ListBoxItem item) =>
        item.GetVisualDescendants().OfType<ContentPresenter>()
            .Single(child => child.Name == "PART_ContentPresenter");

    private static Color BrushColor(IBrush? brush) =>
        Assert.IsAssignableFrom<ISolidColorBrush>(brush).Color;
}
