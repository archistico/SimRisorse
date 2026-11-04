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
/// D37: a long production/stock diagnosis must not push the building actions
/// out of the detail pane; the scroll belongs to the diagnosis itself.
/// </summary>
public sealed class V14BuildingDetailScrollAuditTests
{
    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public void LongDiagnosisScrollsWhileConstructionAndWorkersRemainVisible(int width, int height)
    {
        var view = V14PreviewViewModel.CreateForPreview(Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.BuildingScroll." + Guid.NewGuid().ToString("N") + ".sqlite3"));
        var window = new V14PreviewWindow(view, startTimer: false) { Width = width, Height = height };
        try
        {
            window.Show();
            var selected = view.Buildings.Single(entry => entry.FamilyId == "sawmill" &&
                entry.Tier == ProductionTier.Base);
            view.SelectedBuilding = selected;
            // A deterministic, deliberately verbose diagnostic exercises the layout,
            // not any of the simulation's resource/price algorithms.
            var longDiagnosis = string.Join(" ", Enumerable.Repeat(
                "Produzione limitata: materie prime insufficienti e magazzino pieno.", 18));
            selected.Update(selected.Count, selected.Workers, selected.Status, selected.Price,
                selected.Requirements, selected.IsUnlocked, selected.CostLines, longDiagnosis);
            Dispatcher.UIThread.RunJobs();

            var scroll = window.FindControl<ScrollViewer>("V14BuildingDiagnosisScroll")!;
            var panel = window.FindControl<Border>("V14BuildingDiagnosisPanel")!;
            var build = window.FindControl<Grid>("V14BuildingConstructionActions")!;
            var workers = window.FindControl<Grid>("V14BuildingWorkerActions")!;
            Assert.True(scroll.IsEffectivelyVisible);
            Assert.True(panel.IsEffectivelyVisible);
            Assert.True(scroll.Bounds.Height <= 104.5,
                $"Diagnosis height {scroll.Bounds.Height} exceeded its 104 px cap.");
            Assert.True(scroll.Extent.Height > scroll.Viewport.Height,
                "Long diagnosis should be scrollable rather than fully expanded.");
            Assert.True(build.IsEffectivelyVisible);
            Assert.True(workers.IsEffectivelyVisible);
            Assert.True(build.Bounds.Height > 0);
            Assert.True(workers.Bounds.Height > 0);
            var before = view.SelectedBuilding;
            scroll.Offset = new Vector(0, Math.Max(0, scroll.Extent.Height - scroll.Viewport.Height));
            Dispatcher.UIThread.RunJobs();
            Assert.Same(before, view.SelectedBuilding);
            Assert.True(scroll.Offset.Y > 0);
        }
        finally { window.CloseWithoutSaving(); }
    }
}
