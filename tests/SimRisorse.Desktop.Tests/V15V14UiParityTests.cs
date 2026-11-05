using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Food;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using Xunit;

namespace SimRisorse.Desktop.Tests;

/// <summary>
/// RC63: the v14 presentation contract is a regression barrier for v15.
/// New food buildings extend the existing UI; they do not replace it.
/// </summary>
public sealed class V15V14UiParityTests
{
    private static V15PreviewViewModel Fresh() => V15PreviewViewModel.CreateForPreview(
        Path.Combine(Path.GetTempPath(), "SimRisorse.RC63." + Guid.NewGuid().ToString("N") + ".sqlite3"));

    [Fact]
    public void SidebarRestoresGrowthAndHousingIndicatorsWithoutAdvancingTheClock()
    {
        var vm = Fresh();
        var before = vm.Clock;
        Assert.True(vm.HasPopulationGrowth);
        Assert.True(vm.HasAdvancedHousingRules);
        Assert.Contains("/", vm.PopulationCount);
        Assert.Contains("liberi", vm.AvailableWorkersText);
        Assert.InRange(vm.GrowthPercent, 0, 100);
        Assert.InRange(vm.HousingFillPercent, 0, 100);
        Assert.NotEmpty(vm.GrowthStatus);
        Assert.Contains("Cibo/s", vm.FoodRequirement);
        Assert.Contains("Pane/s", vm.BreadRequirement);
        Assert.Equal(before, vm.Clock);
    }

    [Fact]
    public void EveryNewFoodSiteHasRealIconLinesForOutputInputAndItsOwnCapacity()
    {
        var vm = Fresh();
        foreach (var siteId in FoodIndustrySites.Ids)
        {
            var site = vm.Buildings.Single(b => b.Id == siteId);
            Assert.True(PixelGlyph.Supports(site.GlyphKey));
            Assert.True(site.IsProducer);
            Assert.NotEmpty(site.Description);
            Assert.NotEmpty(site.OutputLines);
            Assert.NotEmpty(site.CapacityLines);
            Assert.All(site.OutputLines.Concat(site.InputLines).Concat(site.CapacityLines),
                line => Assert.True(PixelGlyph.Supports(line.ResourceId), line.ResourceId));
        }
        var depot = vm.Buildings.Single(b => b.Id == "food-depot");
        Assert.Contains(depot.OutputLines, line => line.ResourceId == "food");
        Assert.Contains(depot.InputLines, line => line.ResourceId == "fish");
        Assert.Contains(depot.CapacityLines, line => line.ResourceId == "food");
    }

    [Fact]
    public void LockedBuildingsRemainSelectableButDimAndNeverOfferConstruction()
    {
        var vm = Fresh();
        var depot = vm.Buildings.Single(row => row.Id == "food-depot");
        Assert.False(depot.Unlocked);
        Assert.Equal(0.48d, depot.RowOpacity);
        Assert.True(depot.HasRequirements);
        Assert.Contains("Pescatore", depot.Requirements);
        Assert.NotEmpty(depot.CostLines); // price visible even when prerequisite is missing
        vm.SelectedBuilding = depot;
        Assert.Same(depot, vm.SelectedBuilding);
        Assert.False(vm.CanBuildSelected);
        Assert.False(vm.BuildCommand.CanExecute(null));
        Assert.False(vm.BuildFiveCommand.CanExecute(null));
        Assert.False(vm.BuildTenCommand.CanExecute(null));
    }

    [Fact]
    public void BaseLargeAdvancedRemainSeparateWithLockedResearchShown()
    {
        var vm = Fresh();
        var baseRow = vm.Buildings.Single(b => b.Id == "sawmill");
        var large = vm.Buildings.Single(b => b.Id == "sawmill-large");
        var advanced = vm.Buildings.Single(b => b.Id == "sawmill-advanced");
        Assert.Equal(ProductionTier.Base, baseRow.Tier);
        Assert.Equal(ProductionTier.Large, large.Tier);
        Assert.Equal(ProductionTier.Advanced, advanced.Tier);
        Assert.True(large.HasRequirements);
        Assert.True(advanced.HasRequirements);
        Assert.Equal(0.48d, advanced.RowOpacity);
        Assert.NotEmpty(advanced.InputLines);
        Assert.NotEmpty(advanced.OutputLines);
    }

    [Fact]
    public void BatchButtonsKeepTheV14QuantityOptionsAndAreNotEnabledWithoutMaterials()
    {
        var vm = Fresh();
        vm.SelectedBuilding = vm.Buildings.Single(row => row.Id == "hunter");
        Assert.Equal(3, vm.BatchSizes.Count);
        Assert.Equal(new[] { 1, 5, 10 }, vm.BatchSizes);
        Assert.False(vm.CanAssignSelected);
        Assert.False(vm.CanAssignSelectedFive);
        Assert.False(vm.CanAssignSelectedTen);
        Assert.False(vm.AssignFiveWorkersCommand.CanExecute(null));
        Assert.False(vm.AssignTenWorkersCommand.CanExecute(null));
    }

    [Fact]
    public void ResourceRowsPreserveSevenColumnsFormattingAndPhysicalCapacityTooltip()
    {
        var vm = Fresh();
        Assert.Equal(vm.Resources.Count, vm.Resources.Select(row => row.Name).Distinct().Count());
        Assert.All(vm.Resources, row =>
        {
            Assert.NotEmpty(row.QuantityRounded);
            Assert.NotEmpty(row.Capacity);
            Assert.EndsWith("/s", row.Production);
            Assert.EndsWith("/s", row.Consumption);
            Assert.EndsWith("/s", row.NetRate);
            Assert.NotEmpty(row.TimeToFull);
            Assert.Contains("Capienza:", row.CapacityBreakdown);
        });
    }

    [Fact]
    public void SchoolAndMilitaryRetainOriginalBatchSelectorsAndProgressObservations()
    {
        var vm = Fresh();
        vm.SchoolQuantity = 5;
        vm.MilitaryQuantity = 10;
        Assert.Contains("+5", vm.SchoolEnrollLabel);
        Assert.Contains("−5", vm.SchoolCancelLabel);
        Assert.Contains("+10", vm.EnlistLabel);
        Assert.Contains("−10", vm.CancelRecruitsLabel);
        Assert.NotEmpty(vm.SchoolRoster);
        Assert.NotEmpty(vm.MilitaryRoster);
        Assert.InRange(vm.SchoolTrainingPercent, 0, 100);
        Assert.InRange(vm.MilitaryTrainingPercent, 0, 100);
        Assert.False(vm.DischargeCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void ClassicBuildingWorkspaceAndSidebarAppearInActualV15Window()
    {
        var vm = Fresh();
        var window = new V15PreviewWindow(vm, startTimer: false);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window.FindControl<ProgressBar>("GrowthProgressBar"));
            Assert.NotNull(window.FindControl<TextBlock>("GrowthStatusText"));
            Assert.NotNull(window.FindControl<Grid>("V15BuildingConstructionActions"));
            Assert.NotNull(window.FindControl<Grid>("V15BuildingWorkerActions"));
            Assert.NotNull(window.FindControl<ScrollViewer>("V15BuildingDiagnosisScroll"));
            Assert.NotNull(window.FindControl<Button>("V15BuildTen"));
            Assert.NotNull(window.FindControl<Button>("V15AssignTenWorkers"));
            var row = vm.Buildings.Single(b => b.Id == "food-depot");
            vm.SelectedBuilding = row;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(row, window.FindControl<ListBox>("V15Buildings")!.SelectedItem);
            Assert.False(window.FindControl<Grid>("V15BuildingConstructionActions")!.IsVisible);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public void OldFiveSectionsAndSelectionsSurviveRefreshingV15()
    {
        var vm = Fresh();
        var window = new V15PreviewWindow(vm, startTimer: false);
        try
        {
            window.Show();
            var tabs = window.FindControl<TabControl>("V15Sections")!;
            var nav = window.FindControl<ListBox>("V15NavigationMenu")!;
            Assert.Equal(5, tabs.Items.Count);
            Assert.Equal(5, nav.Items.Count);
            var row = vm.Buildings.Single(b => b.Id == "butcher");
            vm.SelectedBuilding = row;
            for (int i = 0; i < 5; i++)
            {
                nav.SelectedIndex = i;
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(i, tabs.SelectedIndex);
            }
            vm.Refresh();
            Assert.Same(row, vm.SelectedBuilding);
        }
        finally { window.CloseWithoutSaving(); }
    }
}
