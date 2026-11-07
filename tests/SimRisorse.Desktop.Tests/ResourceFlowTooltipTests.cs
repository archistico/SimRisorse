using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SimRisorse.Application.ReadModels;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using Xunit;

namespace SimRisorse.Desktop.Tests;

public sealed class ResourceFlowTooltipTests
{
    private static ResourceFlowContributor Source(string label, decimal produced, decimal consumed) =>
        new(label, ResourceBundle.Create(("fish", produced)),
            ResourceBundle.Create(("fish", consumed)));

    [Fact]
    public void TooltipsExplainEachSourceAndGrossTotalsWithoutChangingNet()
    {
        var entry = new V15ResourceEntry("fish", "Pesce", ResourceAmount.Zero,
            ResourceAmount.FromUnits(100m));
        var row = new V15ResourceRow(entry);
        row.Update(entry, 3m, 1m, [Source("Pescatore (2 edifici, 2 moduli attivi)", 2m, 0m),
            Source("Peschereccio (1 edificio)", 1m, 0m),
            Source("Deposito alimentare (1 edificio)", 0m, 1m)]);
        Assert.Contains("Pescatore (2 edifici, 2 moduli attivi): 2 /s", row.ProductionBreakdown);
        Assert.Contains("Peschereccio (1 edificio): 1 /s", row.ProductionBreakdown);
        Assert.Contains("Totale: 3 /s", row.ProductionBreakdown);
        Assert.Contains("Deposito alimentare (1 edificio): 1 /s", row.ConsumptionBreakdown);
        Assert.Contains("Totale: 1 /s", row.ConsumptionBreakdown);
        Assert.DoesNotContain("Deposito alimentare", row.ProductionBreakdown);
        Assert.Equal("+2 /s", row.NetRate);
    }

    [Fact]
    public void EmptyInventoriesAndNoActiveWorkersAreNotCountedAsProduction()
    {
        var entry = new V15ResourceEntry("fish", "Pesce", ResourceAmount.Zero,
            ResourceAmount.FromUnits(100m));
        var row = new V15ResourceRow(entry);
        row.Update(entry, 0m, 0m, []);
        Assert.Contains("Nessun produttore attivo", row.ProductionBreakdown);
        Assert.Contains("Nessun consumo attivo", row.ConsumptionBreakdown);
        Assert.Contains("Totale: 0 /s", row.ProductionBreakdown);
        Assert.DoesNotContain("non attribuito", row.ProductionBreakdown);
    }

    [Fact]
    public void RefreshReplacesOldSourcesOnTheSameRow()
    {
        var entry = new V15ResourceEntry("fish", "Pesce", ResourceAmount.Zero,
            ResourceAmount.FromUnits(100m));
        var row = new V15ResourceRow(entry);
        row.Update(entry, 1m, 0m, [Source("Prima fonte", 1m, 0m)]);
        Assert.Contains("Prima fonte", row.ProductionBreakdown);
        row.Update(entry, 2m, 0m, [Source("Seconda fonte", 2m, 0m)]);
        Assert.Contains("Seconda fonte", row.ProductionBreakdown);
        Assert.DoesNotContain("Prima fonte", row.ProductionBreakdown);
    }

    [Fact]
    public void MissingContributorIsExplicitInsteadOfInventingItsOrigin()
    {
        var text = ResourceFlowBreakdown.Format("Pesce", "fish", 2m, true,
            [Source("Pescatore", 1m, 0m)]);
        Assert.Contains("Flusso non attribuito: 1 /s", text);
        Assert.Contains("Totale: 2 /s", text);
    }

    [Fact]
    public void HistoricalResourceRowsHaveTheSamePerColumnTooltips()
    {
        var row = new ResourceRow(new ResourceView("fish", "Pesce", 4m, 12m, 1.25m, .5m));
        row.Update(new ResourceView("fish", "Pesce", 4m, 12m, 1.25m, .5m),
            "capienza", [Source("Pescatore", 1.25m, .5m)]);
        Assert.Contains("Pescatore: 1,25 /s", row.ProductionBreakdown);
        Assert.Contains("Pescatore: 0,5 /s", row.ConsumptionBreakdown);
        Assert.Equal("+0,75 /s", row.NetRate);
        Assert.Equal("capienza", row.CapacityBreakdown);
    }

    [Fact]
    public void NaturalV15ResourceRowsReconcileWithoutUnattributedFlows()
    {
        var vm = V15PreviewViewModel.CreateForPreview(Path.Combine(Path.GetTempPath(),
            "SimRisorse.Tooltip." + Guid.NewGuid().ToString("N"), "v15.sqlite3"));
        var before = vm.Clock;
        foreach (var row in vm.Resources)
        {
            Assert.Contains("Totale: ", row.ProductionBreakdown);
            Assert.Contains("Totale: ", row.ConsumptionBreakdown);
            Assert.DoesNotContain("non attribuito", row.ProductionBreakdown);
            Assert.DoesNotContain("non attribuito", row.ConsumptionBreakdown);
        }
        vm.Refresh();
        Assert.Equal(before, vm.Clock);
    }

    [Fact]
    public void NaturalV14ResourceRowsHaveNamedSourceBreakdowns()
    {
        var vm = V14PreviewViewModel.CreateForPreview(Path.Combine(Path.GetTempPath(),
            "SimRisorse.Tooltip." + Guid.NewGuid().ToString("N"), "v14.sqlite3"));
        vm.Initialize();
        foreach (var row in vm.Resources)
        {
            Assert.Contains("Totale: ", row.ProductionBreakdown);
            Assert.Contains("Totale: ", row.ConsumptionBreakdown);
            Assert.DoesNotContain("non attribuito", row.ProductionBreakdown);
            Assert.DoesNotContain("non attribuito", row.ConsumptionBreakdown);
        }
    }

    [AvaloniaFact]
    public void V15RenderedResourceCellsHaveSeparateHoverTipsAndSevenColumns()
    {
        var vm = V15PreviewViewModel.CreateForPreview(Path.Combine(Path.GetTempPath(),
            "SimRisorse.TooltipUI." + Guid.NewGuid().ToString("N"), "v15.sqlite3"));
        var window = new V15PreviewWindow(vm, startTimer: false);
        try
        {
            window.Show();
            window.FindControl<TabControl>("V15Sections")!.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            var header = window.FindControl<Grid>("V15ResourceTableHeader")!;
            Assert.Equal(7, header.ColumnDefinitions.Count);
            var tips = window.FindControl<ListBox>("V15Resources")!
                .GetVisualDescendants().OfType<TextBlock>()
                .Select(block => ToolTip.GetTip(block)).OfType<string>().ToArray();
            Assert.Contains(tips, text => text.StartsWith("PRODUZIONE ·", StringComparison.Ordinal));
            Assert.Contains(tips, text => text.StartsWith("CONSUMO ·", StringComparison.Ordinal));
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public void V14RenderedResourceCellsHaveSeparateHoverTipsAndSevenColumns()
    {
        var vm = V14PreviewViewModel.CreateForPreview(Path.Combine(Path.GetTempPath(),
            "SimRisorse.TooltipUI." + Guid.NewGuid().ToString("N"), "v14.sqlite3"));
        var window = new V14PreviewWindow(vm, startTimer: false);
        try
        {
            window.Show();
            window.FindControl<TabControl>("V14Sections")!.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            var header = window.FindControl<Grid>("V14ResourceTableHeader")!;
            Assert.Equal(7, header.ColumnDefinitions.Count);
            var tips = window.FindControl<ListBox>("V14Resources")!
                .GetVisualDescendants().OfType<TextBlock>()
                .Select(block => ToolTip.GetTip(block)).OfType<string>().ToArray();
            Assert.Contains(tips, text => text.StartsWith("PRODUZIONE ·", StringComparison.Ordinal));
            Assert.Contains(tips, text => text.StartsWith("CONSUMO ·", StringComparison.Ordinal));
        }
        finally { window.CloseWithoutSaving(); }
    }
}
