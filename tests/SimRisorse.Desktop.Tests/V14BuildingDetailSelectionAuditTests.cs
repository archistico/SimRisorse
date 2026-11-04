using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using SimRisorse.Core.Construction;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using Xunit;

namespace SimRisorse.Desktop.Tests;

/// <summary>
/// G1-D41: selecting another building must start its detail and diagnosis at
/// the beginning without resetting the current building on normal row updates.
/// </summary>
public sealed class V14BuildingDetailSelectionAuditTests
{
    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public void SelectingAnotherBuildingResetsBothScrollPositions(int width, int height)
    {
        var view = V14PreviewViewModel.CreateForPreview(Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.BuildingSelection." + Guid.NewGuid().ToString("N") + ".sqlite3"));
        var window = new V14PreviewWindow(view, startTimer: false) { Width = width, Height = height };
        try
        {
            window.Show();
            var first = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Base);
            var second = view.Buildings.Single(item => item.FamilyId == "stonecutter" &&
                item.Tier == ProductionTier.Base);
            AddLongDetail(first);
            AddLongDetail(second);
            var list = window.FindControl<ListBox>("V14Buildings")!;
            list.SelectedItem = first;
            Dispatcher.UIThread.RunJobs();

            var mainScroll = window.FindControl<ScrollViewer>("V14BuildingDetailScroll")!;
            var diagnosisScroll = window.FindControl<ScrollViewer>("V14BuildingDiagnosisScroll")!;
            Assert.True(mainScroll.Extent.Height > mainScroll.Viewport.Height,
                "The synthetic prerequisites must require scrolling in the main detail.");
            Assert.True(diagnosisScroll.Extent.Height > diagnosisScroll.Viewport.Height,
                "The synthetic diagnosis must require its own scrolling.");
            mainScroll.Offset = new Vector(0, 60);
            diagnosisScroll.Offset = new Vector(0, 40);
            Dispatcher.UIThread.RunJobs();
            Assert.True(mainScroll.Offset.Y > 0);
            Assert.True(diagnosisScroll.Offset.Y > 0);

            // Simulate an actual change in the building list, not a forced refresh.
            list.SelectedItem = second;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(second, view.SelectedBuilding);
            Assert.Same(second, list.SelectedItem);
            Assert.True(mainScroll.Extent.Height > mainScroll.Viewport.Height);
            Assert.True(diagnosisScroll.Extent.Height > diagnosisScroll.Viewport.Height);
            Assert.Equal(0d, mainScroll.Offset.Y);
            Assert.Equal(0d, diagnosisScroll.Offset.Y);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public void RefreshingTheSameBuildingDoesNotDiscardTheReadersScrollPosition()
    {
        var view = V14PreviewViewModel.CreateForPreview(Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.DetailRefresh." + Guid.NewGuid().ToString("N") + ".sqlite3"));
        var window = new V14PreviewWindow(view, startTimer: false) { Width = 1040, Height = 740 };
        try
        {
            window.Show();
            var building = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Base);
            AddLongDetail(building);
            view.SelectedBuilding = building;
            Dispatcher.UIThread.RunJobs();
            var mainScroll = window.FindControl<ScrollViewer>("V14BuildingDetailScroll")!;
            var diagnosisScroll = window.FindControl<ScrollViewer>("V14BuildingDiagnosisScroll")!;
            Assert.True(mainScroll.Extent.Height > mainScroll.Viewport.Height);
            Assert.True(diagnosisScroll.Extent.Height > diagnosisScroll.Viewport.Height);
            mainScroll.Offset = new Vector(0, 60);
            diagnosisScroll.Offset = new Vector(0, 40);
            Dispatcher.UIThread.RunJobs();
            Assert.True(mainScroll.Offset.Y > 0);
            Assert.True(diagnosisScroll.Offset.Y > 0);
            var beforeMain = mainScroll.Offset.Y;
            var beforeDiagnosis = diagnosisScroll.Offset.Y;

            // Same selection with updated status: no SelectionChanged event should
            // be raised and the user must retain the reading position.
            view.SelectedBuilding = building;
            building.Update(building.Count, building.Workers, building.Status + " ",
                building.Price, building.Requirements, building.IsUnlocked,
                building.CostLines, building.Diagnosis);
            Dispatcher.UIThread.RunJobs();
            Assert.Same(building, view.SelectedBuilding);
            Assert.Equal(beforeMain, mainScroll.Offset.Y);
            Assert.Equal(beforeDiagnosis, diagnosisScroll.Offset.Y);
        }
        finally { window.CloseWithoutSaving(); }
    }

    private static void AddLongDetail(V14BuildingEntry entry)
    {
        var requirements = string.Join(" ", Enumerable.Repeat(
            "Prerequisiti dettagliati per la verifica dello scorrimento del pannello.", 80));
        var diagnosis = string.Join(" ", Enumerable.Repeat(
            "Materiali insufficienti e capacità produttiva da controllare.", 28));
        entry.Update(entry.Count, entry.Workers, entry.Status, entry.Price, requirements,
            entry.IsUnlocked, entry.CostLines, diagnosis);
    }
}
