using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using SimRisorse.Core.Construction;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using Xunit;

namespace SimRisorse.Desktop.Tests;

/// <summary>
/// G1-D38: switching between the five sidebar sections and refreshing the game
/// must never discard list selection or leave a section disconnected from its tab.
/// No gameplay rules or visual layout are changed by these checks.
/// </summary>
public sealed class V14NavigationSelectionAuditTests
{
    private static V14PreviewViewModel Fresh(string path) =>
        V14PreviewViewModel.CreateForPreview(path);

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public void SidebarAndTabsStaySynchronizedWithoutLosingSelections(int width, int height)
    {
        var directory = Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.Navigation." + Guid.NewGuid().ToString("N"));
        V14PreviewWindow? window = null;
        try
        {
            var vm = Fresh(Path.Combine(directory, "preview-v14.sqlite3"));
            window = new V14PreviewWindow(vm, startTimer: false) { Width = width, Height = height };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var tabs = window.FindControl<TabControl>("V14Sections")!;
            var navigation = window.FindControl<ListBox>("V14NavigationMenu")!;
            var selectedBuilding = vm.Buildings.Single(item =>
                item.FamilyId == "sawmill" && item.Tier == ProductionTier.Base);
            var selectedResource = vm.Resources.Single(item => item.Id == "pencils");
            var selectedTechnology = vm.Technologies.Single(item => item.Id == "lavorazioni-1");
            vm.SelectedBuilding = selectedBuilding;
            vm.SelectedResource = selectedResource;
            vm.SelectedTechnology = selectedTechnology;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(5, tabs.Items.Count);
            Assert.Equal(5, navigation.Items.Count);
            for (var index = 0; index < 5; index++)
            {
                navigation.SelectedIndex = index;
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(index, tabs.SelectedIndex);
                Assert.Equal(index, navigation.SelectedIndex);
            }
            for (var index = 4; index >= 0; index--)
            {
                tabs.SelectedIndex = index;
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(index, navigation.SelectedIndex);
            }

            vm.TogglePauseCommand.Execute(null);
            vm.Advance(TimeSpan.FromSeconds(2));
            Dispatcher.UIThread.RunJobs();
            Assert.Same(selectedBuilding, vm.SelectedBuilding);
            Assert.Same(selectedResource, vm.SelectedResource);
            Assert.Same(selectedTechnology, vm.SelectedTechnology);
            Assert.Same(selectedBuilding, window.FindControl<ListBox>("V14Buildings")!.SelectedItem);

            navigation.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(selectedResource, window.FindControl<ListBox>("V14Resources")!.SelectedItem);
            Assert.True(window.FindControl<ListBox>("V14Resources")!.IsEffectivelyVisible);
            navigation.SelectedIndex = 4;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(selectedTechnology, window.FindControl<ListBox>("V14Technologies")!.SelectedItem);
            Assert.True(window.FindControl<ListBox>("V14Technologies")!.IsEffectivelyVisible);
        }
        finally
        {
            if (window is { IsVisible: true }) window.CloseWithoutSaving();
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ExplicitSaveAndLoadKeepAllThreeSelectedRowsAndLiveReadModels()
    {
        var directory = Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.SelectionReload." + Guid.NewGuid().ToString("N"));
        try
        {
            var vm = Fresh(Path.Combine(directory, "preview-v14.sqlite3"));
            vm.Initialize();
            var building = vm.Buildings.Single(item =>
                item.FamilyId == "sawmill" && item.Tier == ProductionTier.Base);
            var resource = vm.Resources.Single(item => item.Id == "pencils");
            var technology = vm.Technologies.Single(item => item.Id == "lavorazioni-1");
            vm.SelectedBuilding = building;
            vm.SelectedResource = resource;
            vm.SelectedTechnology = technology;
            Assert.True(vm.Save());
            vm.TogglePauseCommand.Execute(null);
            vm.Advance(TimeSpan.FromSeconds(2));
            Assert.True(vm.Load());
            Assert.Same(building, vm.SelectedBuilding);
            Assert.Same(resource, vm.SelectedResource);
            Assert.Same(technology, vm.SelectedTechnology);
            Assert.Same(building, vm.Buildings.Single(item =>
                item.FamilyId == "sawmill" && item.Tier == ProductionTier.Base));
            Assert.Same(resource, vm.Resources.Single(item => item.Id == "pencils"));
            Assert.Same(technology, vm.Technologies.Single(item => item.Id == "lavorazioni-1"));
            Assert.Equal(resource.CapacityBreakdown, vm.SelectedResourceStorageDetails);
            Assert.Contains(technology.Status, vm.SelectedTechnologySummary);
            Assert.True(vm.IsPaused);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }
}
