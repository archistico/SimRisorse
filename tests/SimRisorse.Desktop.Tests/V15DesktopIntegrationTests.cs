using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using SimRisorse.Desktop;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Food;
using SimRisorse.Core.Economy;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Desktop.Tests;

/// <summary>RC54: opt-in v15 window and isolated persistent UI route.</summary>
public sealed class V15DesktopIntegrationTests
{
    private static string NewPath() => Path.Combine(Path.GetTempPath(),
        "SimRisorse.RC54." + Guid.NewGuid().ToString("N"), "v15.sqlite3");

    [Fact]
    public void DefaultV15AndExplicitV14UseDistinctReservedSlots()
    {
        var normal = DesktopOptions.Parse([]);
        var v15 = DesktopOptions.Parse(["--v15"]);
        var v14 = DesktopOptions.Parse(["--v14"]);
        Assert.True(normal.UseV15);
        Assert.False(normal.UseV14);
        Assert.True(v15.UseV15);
        Assert.False(v15.UseV14);
        Assert.Equal(normal.SavePath, v15.SavePath);
        Assert.EndsWith("civil-growth-v15.sqlite3", normal.SavePath);
        Assert.True(v14.UseV14);
        Assert.False(v14.UseV15);
        Assert.EndsWith("civil-growth-v14.sqlite3", v14.SavePath);
        Assert.NotEqual(normal.SavePath, v14.SavePath);
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--v14", "--v15"]));
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--legacy-v13", "--v15"]));
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--smoke-test", "--v15"]));
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--v15", "--scenario", "civil-growth"]));
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--v15", "--save-path", v14.SavePath]));
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--v14", "--save-path", normal.SavePath]));
    }

    [Fact]
    public void EveryV15FoodSiteAndIntermediateHasAnEightBitGlyph()
    {
        foreach (var id in FoodIndustrySites.Ids.Concat(
            ["cattle", "game", "meat", "vegetables", "fruit"]))
            Assert.True(PixelGlyph.Supports(id), $"Missing pixel glyph for {id}");
    }

    [Fact]
    public void FirstV15LaunchIsPausedAndExposesSixNewSitesAndDirectTierEntries()
    {
        var path = NewPath();
        try
        {
            var vm = V15PreviewViewModel.CreateForPreview(path);
            vm.Initialize();
            Assert.True(vm.IsPaused);
            Assert.False(File.Exists(path));
            Assert.Equal(8, vm.Population);
            foreach (var site in FoodIndustrySites.Ids)
                Assert.Single(vm.Buildings, row => row.Id == site && row.Kind == V15BuildingKind.FoodIndustry);
            Assert.DoesNotContain(vm.Buildings, row => row.Id == "food-store");
            Assert.Contains(vm.Buildings, row => row.Id == "sawmill-large" && row.Tier == ProductionTier.Large);
            Assert.Contains(vm.Buildings, row => row.Id == "sawmill-advanced" && row.Tier == ProductionTier.Advanced);
            Assert.Equal(1, vm.Resources.Count(row => row.Id == "food"));
            Assert.Contains(vm.Resources, row => row.Id == "cattle");
            Assert.Contains(vm.Resources, row => row.Id == "vegetables");
            Assert.Contains(vm.Resources, row => row.Id == "pencils");
            Assert.Equal("2", vm.Resources.Single(row => row.Id == "pencils").QuantityRounded);
            Assert.Equal("100", vm.Resources.Single(row => row.Id == "coal").QuantityRounded);
            vm.SelectedBuilding = vm.Buildings.Single(row => row.Id == "food-depot");
            Assert.False(vm.CanBuildSelected); // Pescatore is missing.
            Assert.False(vm.BuildCommand.CanExecute(null));
            Assert.Equal(0L, vm.SelectedBuilding!.Count);
            Assert.False(vm.CanAssignWorker);
        }
        finally { if (Directory.Exists(Path.GetDirectoryName(path))) Directory.Delete(Path.GetDirectoryName(path)!, true); }
    }

    [AvaloniaFact]
    public void ConstructionCostUsesIconLinesOnlyAndDoesNotRepeatTextQuote()
    {
        var vm = V15PreviewViewModel.CreateForPreview(NewPath());
        var window = new V15PreviewWindow(vm, startTimer: false);
        try
        {
            window.Show();
            vm.SelectedBuilding = vm.Buildings.Single(b => b.Id == "sawmill");
            Dispatcher.UIThread.RunJobs();
            Assert.Null(window.FindControl<TextBlock>("V15BuildingCost"));
            var lines = window.FindControl<ItemsControl>("V15BuildingCostLines");
            Assert.NotNull(lines);
            Assert.True(lines!.IsEffectivelyVisible);
            Assert.NotEmpty(vm.SelectedBuilding!.CostLines);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [Fact]
    public void FoodSectorIncludesAllIndustrySitesAndTheHistoricFoodChain()
    {
        var path = NewPath();
        try
        {
            var vm = V15PreviewViewModel.CreateForPreview(path);
            vm.Initialize();
            Assert.Equal(V15BuildingSector.All, vm.ActiveBuildingSector);
            Assert.Equal(vm.Buildings.Count, vm.VisibleBuildings.Count);
            vm.BuildingFilters.Single(option => option.Sector == V15BuildingSector.Food).SelectCommand.Execute(null);
            Assert.Equal(V15BuildingSector.Food, vm.ActiveBuildingSector);
            Assert.Equal(23, vm.VisibleBuildings.Count); // six sites + two depot tiers + five food-producing families × three tiers
            foreach (var site in FoodIndustrySites.Ids)
                Assert.Single(vm.VisibleBuildings, row => row.Id == site);
            Assert.Contains(vm.VisibleBuildings, row => row.Id == "food-depot-large");
            Assert.Contains(vm.VisibleBuildings, row => row.Id == "food-depot-advanced");
            Assert.Contains(vm.VisibleBuildings, row => row.Id == "bakery-advanced");
            Assert.All(vm.VisibleBuildings, row => Assert.Equal(V15BuildingSector.Food, V15BuildingSectors.For(row)));
            vm.Refresh();
            Assert.Equal(23, vm.VisibleBuildings.Count);
            vm.BuildingFilters.Single(option => option.Sector == V15BuildingSector.All).SelectCommand.Execute(null);
            Assert.Equal(V15BuildingSector.All, vm.ActiveBuildingSector);
            Assert.Equal(vm.Buildings.Count, vm.VisibleBuildings.Count);
        }
        finally { if (Directory.Exists(Path.GetDirectoryName(path))) Directory.Delete(Path.GetDirectoryName(path)!, true); }
    }

    [Fact]
    public void SaveAndLoadV15DoNotTouchAnAdjacentV14File()
    {
        var path = NewPath();
        var directory = Path.GetDirectoryName(path)!;
        try
        {
            Directory.CreateDirectory(directory);
            var v14 = Path.Combine(directory, "v14.sqlite3");
            var original = new byte[] { 1, 7, 11, 30, 91 };
            File.WriteAllBytes(v14, original);
            var vm = V15PreviewViewModel.CreateForPreview(path);
            vm.Initialize();
            Assert.True(vm.Save());
            Assert.True(File.Exists(path));
            Assert.True(vm.Load());
            Assert.True(vm.IsPaused);
            Assert.Equal("2", vm.Resources.Single(row => row.Id == "pencils").QuantityRounded);
            Assert.Equal("100", vm.Resources.Single(row => row.Id == "coal").QuantityRounded);
            Assert.Equal(8, vm.Population);
            Assert.Equal(original, File.ReadAllBytes(v14));
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public void ExistingCheckpointWithNoPencilsIsNotRefilledOnLoad()
    {
        var path = NewPath();
        try
        {
            var setup = V15BenchmarkFixture.Create();
            var withoutPencils = setup.Start with
            {
                CityCampaign = setup.Start.CityCampaign with
                {
                    Resources = setup.Start.CityCampaign.Resources -
                        ResourceBundle.Create(("pencils", 2m), ("coal", 100m))
                }
            };
            var store = new V15SqliteFoodSaveStore(path, setup.Baseline,
                setup.School, setup.Technologies, setup.Depot);
            store.Save(withoutPencils);
            var vm = V15PreviewViewModel.CreateForPreview(path);
            vm.Initialize(); // Auto-load from an existing file, not FreshSession.
            Assert.Equal("0", vm.Resources.Single(row => row.Id == "pencils").QuantityRounded);
            Assert.Equal("0", vm.Resources.Single(row => row.Id == "coal").QuantityRounded);
            Assert.True(vm.Load());
            Assert.Equal("0", vm.Resources.Single(row => row.Id == "pencils").QuantityRounded);
            Assert.Equal("0", vm.Resources.Single(row => row.Id == "coal").QuantityRounded);
            vm.NewGameCommand.Execute(null);
            vm.ConfirmNewGameCommand.Execute(null);
            Assert.Equal("2", vm.Resources.Single(row => row.Id == "pencils").QuantityRounded);
            Assert.Equal("100", vm.Resources.Single(row => row.Id == "coal").QuantityRounded);
            // An unsaved new game must not overwrite the old checkpoint.
            Assert.Equal(ResourceAmount.Zero, store.Load().CityCampaign.Resources["pencils"]);
            Assert.Equal(ResourceAmount.Zero, store.Load().CityCampaign.Resources["coal"]);
        }
        finally { if (Directory.Exists(Path.GetDirectoryName(path))) Directory.Delete(Path.GetDirectoryName(path)!, true); }
    }

    [Fact]
    public void NewColonyNeverOverwritesExistingCheckpointBeforeExplicitSave()
    {
        var path = NewPath();
        try
        {
            var vm = V15PreviewViewModel.CreateForPreview(path);
            vm.Initialize();
            Assert.True(vm.Save());
            var original = File.ReadAllBytes(path);
            vm.NewGameCommand.Execute(null);
            Assert.True(vm.IsNewGameConfirmationVisible);
            vm.ConfirmNewGameCommand.Execute(null);
            Assert.True(vm.IsPaused);
            Assert.False(vm.CanAutoSave);
            Assert.Equal(original, File.ReadAllBytes(path));
            Assert.True(vm.Save());
            Assert.True(vm.CanAutoSave);
        }
        finally { if (Directory.Exists(Path.GetDirectoryName(path))) Directory.Delete(Path.GetDirectoryName(path)!, true); }
    }

    [Fact]
    public void CorruptV15CheckpointBlocksAutosaveInsteadOfBeingReplaced()
    {
        var path = NewPath();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var corrupt = new byte[] { 1, 2, 3, 4, 5, 6 };
            File.WriteAllBytes(path, corrupt);
            var vm = V15PreviewViewModel.CreateForPreview(path);
            vm.Initialize();
            Assert.False(vm.CanSave);
            Assert.False(vm.CanAutoSave);
            Assert.False(vm.Save());
            Assert.Equal(corrupt, File.ReadAllBytes(path));
        }
        finally { if (Directory.Exists(Path.GetDirectoryName(path))) Directory.Delete(Path.GetDirectoryName(path)!, true); }
    }

    [AvaloniaFact]
    public void V15WindowUsesOriginalFiveSectionNavigationAndSevenResourceColumns()
    {
        var path = NewPath();
        V15PreviewWindow? window = null;
        try
        {
            var vm = V15PreviewViewModel.CreateForPreview(path);
            window = new V15PreviewWindow(vm, startTimer: false);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var nav = window.FindControl<ListBox>("V15NavigationMenu")!;
            var tabs = window.FindControl<TabControl>("V15Sections")!;
            Assert.Equal(5, nav.Items.Count);
            Assert.Equal(5, tabs.Items.Count);
            Assert.Equal(7, window.FindControl<Grid>("V15ResourceTableHeader")!.ColumnDefinitions.Count);
            var building = vm.Buildings.Single(b => b.Id == "hunter");
            vm.SelectedBuilding = building;
            nav.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, tabs.SelectedIndex);
            nav.SelectedIndex = 0;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(building, window.FindControl<ListBox>("V15Buildings")!.SelectedItem);
        }
        finally
        {
            if (window is { IsVisible: true }) window.CloseWithoutSaving();
            if (Directory.Exists(Path.GetDirectoryName(path))) Directory.Delete(Path.GetDirectoryName(path)!, true);
        }
    }

    [Fact]
    public void RefreshPreservesSelectedRowsAndDoesNotAdvancePausedGame()
    {
        var vm = V15PreviewViewModel.CreateForPreview(NewPath());
        var selectedBuilding = vm.Buildings.Single(b => b.Id == "garden");
        var selectedResource = vm.Resources.Single(r => r.Id == "bread");
        var selectedTechnology = vm.Technologies.Single(t => t.Id == "lavorazioni-1");
        vm.SelectedBuilding = selectedBuilding;
        vm.SelectedResource = selectedResource;
        vm.SelectedTechnology = selectedTechnology;
        var previousClock = vm.Clock;
        vm.Advance(TimeSpan.FromSeconds(30));
        vm.Refresh();
        Assert.Equal(previousClock, vm.Clock);
        Assert.Same(selectedBuilding, vm.SelectedBuilding);
        Assert.Same(selectedResource, vm.SelectedResource);
        Assert.Same(selectedTechnology, vm.SelectedTechnology);
    }
}
