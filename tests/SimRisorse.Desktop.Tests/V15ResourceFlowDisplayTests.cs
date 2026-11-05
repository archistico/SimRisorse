using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Desktop.Presentation;
using Xunit;

namespace SimRisorse.Desktop.Tests;

public sealed class V15ResourceFlowDisplayTests
{
    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);

    private static V15ResourceRow Row(decimal amount, decimal capacity, decimal production, decimal consumption)
    {
        var entry = new V15ResourceEntry("fish", "Pesce", U(amount), U(capacity),
            [new V15CapacityContribution("Città", U(capacity))]);
        var row = new V15ResourceRow(entry);
        row.Update(entry, production, consumption);
        return row;
    }

    [Fact]
    public void GrossAndNetRatesUseItalianFormattingAndPreserveTheSevenColumnContract()
    {
        var row = Row(1.234m, 25m, 2.500m, 0.125m);
        Assert.Equal("1", row.Quantity);
        Assert.Equal("25", row.Capacity);
        Assert.Equal("2,5 /s", row.Production);
        Assert.Equal("0,125 /s", row.Consumption);
        Assert.Equal("+2,375 /s", row.NetRate);
        Assert.Contains("Città 25", row.CapacityBreakdown);
    }

    [Theory]
    [InlineData(3, 10, 2, 0, "00:00:04")]
    [InlineData(3, 10, 2, 3, "—")]
    [InlineData(3, 10, 0, 0, "—")]
    [InlineData(10, 10, 0, 2, "00:00:00")]
    [InlineData(10, 0, 1, 0, "—")]
    [InlineData(0, 7200, 2, 0, "01:00:00")]
    public void EtaRespectsCapacityNetFlowAndZeroStorage(decimal stock, decimal capacity,
        decimal production, decimal consumption, string expected)
    {
        Assert.Equal(expected, Row(stock, capacity, production, consumption).TimeToFull);
    }

    [Fact]
    public void RefreshChangesRatesWithoutRecreatingTheResourceRow()
    {
        var initial = new V15ResourceEntry("food", "Cibo", U(1m), U(10m));
        var row = new V15ResourceRow(initial);
        row.Update(initial, 1m, .080m);
        Assert.Equal("00:00:10", row.TimeToFull);
        row.Update(initial with { Quantity = U(9m) }, 0m, .080m);
        Assert.Equal("-0,08 /s", row.NetRate);
        Assert.Equal("—", row.TimeToFull);
    }

    [Fact]
    public void ViewModelRefreshUsesRealFlowAndDoesNotAdvanceTime()
    {
        var path = Path.Combine(Path.GetTempPath(), "SimRisorse.RC56." + Guid.NewGuid().ToString("N"), "v15.sqlite3");
        var vm = V15PreviewViewModel.CreateForPreview(path);
        var originalClock = vm.Clock;
        var food = vm.Resources.Single(r => r.Id == "food");
        Assert.NotEqual("—", food.Production);
        Assert.NotEqual("—", food.Consumption);
        Assert.NotEqual("—", food.NetRate);
        Assert.Equal(food.ProductionPerSecond - food.ConsumptionPerSecond, food.NetPerSecond);
        vm.Refresh();
        Assert.Same(food, vm.Resources.Single(r => r.Id == "food"));
        Assert.Equal(originalClock, vm.Clock);
    }
}
