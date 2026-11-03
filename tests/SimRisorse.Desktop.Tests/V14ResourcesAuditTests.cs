using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using Xunit;

namespace SimRisorse.Desktop.Tests;

/// <summary>
/// G1-D35: lock the seven-column resource layout and scrollbar/selection behaviour
/// to the already accepted classic UI, without changing the resource table XAML.
/// </summary>
public sealed class V14ResourcesAuditTests
{
    private static V14PreviewViewModel Fresh() =>
        V14PreviewViewModel.CreateForPreview(Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.ResourcesAudit." + Guid.NewGuid().ToString("N") + ".sqlite3"));

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public void ResourceTableKeepsSevenRightAlignedColumnsSelectionAndScrollbarGutter(int width, int height)
    {
        var vm = Fresh();
        var window = new V14PreviewWindow(vm, startTimer: false) { Width = width, Height = height };
        try
        {
            window.Show();
            window.FindControl<TabControl>("V14Sections")!.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();

            var list = window.FindControl<ListBox>("V14Resources")!;
            var header = window.FindControl<Grid>("V14ResourceTableHeader")!;
            Assert.True(list.IsEffectivelyVisible);
            Assert.Equal(17, vm.Resources.Count);
            Assert.Equal(7, header.ColumnDefinitions.Count);

            // Reuse the real seven-column grid: the final ETA cell must never
            // extend over the vertical scrollbar at either supported viewport.
            var row = list.GetVisualDescendants().OfType<Grid>().First(grid =>
                grid.ColumnDefinitions.Count == 7 &&
                grid.GetVisualDescendants().OfType<PixelGlyph>().Any());
            var scroll = list.GetVisualDescendants().OfType<ScrollViewer>().First();
            var vertical = scroll.GetVisualDescendants().OfType<ScrollBar>()
                .FirstOrDefault(bar => bar.Orientation == Avalonia.Layout.Orientation.Vertical &&
                    bar.IsEffectivelyVisible && bar.Bounds.Width > 0);
            var limit = vertical is null
                ? scroll.TranslatePoint(new Point(scroll.Bounds.Width - 18, 0), window)!.Value.X
                : vertical.TranslatePoint(default, window)!.Value.X - 2;
            var headerRight = header.TranslatePoint(new Point(header.Bounds.Width, 0), window)!.Value.X;
            var rowItem = row.GetVisualAncestors().OfType<ListBoxItem>().First();
            var rowRight = rowItem.TranslatePoint(new Point(rowItem.Bounds.Width, 0), window)!.Value.X;
            Assert.True(headerRight <= limit,
                $"Resource header reaches {headerRight:0.##} beyond scrollbar boundary {limit:0.##}.");
            Assert.True(rowRight <= limit,
                $"Resource row reaches {rowRight:0.##} beyond scrollbar boundary {limit:0.##}.");

            foreach (var grid in new[] { header, row })
            {
                var numeric = grid.Children.OfType<TextBlock>()
                    .Where(cell => Grid.GetColumn(cell) > 0).ToArray();
                Assert.Equal(6, numeric.Length);
                Assert.All(numeric, cell =>
                {
                    Assert.Equal(Avalonia.Layout.HorizontalAlignment.Right, cell.HorizontalAlignment);
                    Assert.Equal(TextAlignment.Right, cell.TextAlignment);
                });
            }

            var selected = vm.Resources.Single(resource => resource.Id == "food");
            list.SelectedItem = selected;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(selected, vm.SelectedResource);
            var selectedItem = list.GetVisualDescendants().OfType<ListBoxItem>()
                .First(item => ReferenceEquals(item.DataContext, selected));
            Assert.True(selectedItem.IsSelected);
            Assert.Contains(selectedItem.GetVisualDescendants().OfType<Avalonia.Controls.Presenters.ContentPresenter>(),
                presenter => presenter.Background is ISolidColorBrush brush &&
                    brush.Color == Color.Parse("#E6D5AC"));

            var cells = selectedItem.GetVisualDescendants().OfType<Grid>()
                .Where(grid => grid.ColumnDefinitions.Count == 7)
                .SelectMany(grid => grid.Children.OfType<TextBlock>()).ToArray();
            Assert.Equal(selected.QuantityRounded, cells.Single(cell => Grid.GetColumn(cell) == 1).Text);
            Assert.Equal(selected.CapacityBreakdown,
                ToolTip.GetTip(cells.Single(cell => Grid.GetColumn(cell) == 2)));
            Assert.Equal(selected.TimeToFull, cells.Single(cell => Grid.GetColumn(cell) == 6).Text);

            vm.TogglePauseCommand.Execute(null);
            vm.Advance(TimeSpan.FromSeconds(3));
            Dispatcher.UIThread.RunJobs();
            Assert.Same(selected, vm.SelectedResource);
            Assert.Same(selected, list.SelectedItem);
            Assert.True(selectedItem.IsSelected);
            Assert.Equal(selected.CapacityBreakdown,
                ToolTip.GetTip(cells.Single(cell => Grid.GetColumn(cell) == 2)));
        }
        finally { window.CloseWithoutSaving(); }
    }

    [Fact]
    public void ResourceRowsKeepIdentityAlphabeticalOrderAndSelectedDetailsAcrossRefresh()
    {
        var vm = Fresh();
        vm.Initialize();
        var comparer = StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("it-IT"), true);
        var initialRows = vm.Resources.ToDictionary(row => row.Id, StringComparer.Ordinal);
        Assert.Equal(vm.Resources.OrderBy(row => row.Name, comparer).ThenBy(row => row.Id, StringComparer.Ordinal)
            .Select(row => row.Id), vm.Resources.Select(row => row.Id));

        vm.SelectedResource = vm.Resources.Single(row => row.Id == "pencils");
        var pencil = vm.SelectedResource!;
        var changes = 0;
        pencil.PropertyChanged += (_, _) => changes++;
        vm.TogglePauseCommand.Execute(null);
        vm.Advance(TimeSpan.FromSeconds(2));

        Assert.True(changes > 0, "The selected row must notify the view when stock/capacity values refresh.");
        Assert.All(vm.Resources, row => Assert.Same(initialRows[row.Id], row));
        Assert.Same(pencil, vm.SelectedResource);
        Assert.Equal(pencil.CapacityBreakdown, vm.SelectedResourceStorageDetails);
        Assert.Contains("1 Municipio × 1.000", pencil.CapacityBreakdown);
        Assert.Equal(17, vm.Resources.Count);
    }
}
