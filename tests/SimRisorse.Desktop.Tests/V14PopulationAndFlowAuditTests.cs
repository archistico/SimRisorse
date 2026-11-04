using SimRisorse.Application.ReadModels;
using SimRisorse.Desktop.Presentation;
using Xunit;

namespace SimRisorse.Desktop.Tests;

/// <summary>
/// G1-D40: retain the original growth presentation and protect resource-flow ETAs.
/// The economy is not rebalanced by these UI-focused regression tests.
/// </summary>
public sealed class V14PopulationAndFlowAuditTests
{
    private static V14PreviewViewModel Fresh() =>
        V14PreviewViewModel.CreateForPreview(Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.PopulationAudit." + Guid.NewGuid().ToString("N") + ".sqlite3"));

    [Fact]
    public void FullInitialHousingExplainsWhyPopulationCannotGrow()
    {
        var vm = Fresh();
        vm.Initialize();
        Assert.Equal(8, vm.Population);
        Assert.Contains("Case piene", vm.GrowthStatus);
        Assert.Equal(100d, vm.HousingFillPercent);
        Assert.Equal(0d, vm.GrowthPercent);
    }

    [Fact]
    public void AdditionalHousingShowsExactFractionalProgressSpeedAndRemainingTimeAfterTick()
    {
        var vm = Fresh();
        vm.Initialize();
        vm.SelectedCivic = vm.Civics.Single(item => item.Id == "small-house");
        vm.BuildCivicCommand.Execute(null);
        Assert.Contains("×1,15", vm.GrowthStatus);
        Assert.Contains("~53s", vm.GrowthStatus);
        Assert.Equal(8, vm.Population);

        vm.TogglePauseCommand.Execute(null);
        vm.Advance(TimeSpan.FromSeconds(1));
        Assert.Contains("1,15 / 60 s", vm.GrowthStatus);
        Assert.Contains("×1,15", vm.GrowthStatus);
        Assert.Contains("~52s", vm.GrowthStatus);
        Assert.InRange(vm.GrowthPercent, 1.91d, 1.92d);
        Assert.Equal(8, vm.Population);
    }

    [Fact]
    public void ResourceTimeToFullUsesNetRateAndHandlesDeficitsFullAndNoCapacity()
    {
        static ResourceRow Row(decimal owned, decimal capacity, decimal produced, decimal consumed) =>
            new(new ResourceView("food", "Cibo", owned, capacity, produced, consumed));

        Assert.Equal("00:00:05", Row(5m, 10m, 2m, 1m).TimeToFull);
        Assert.Equal("00:00:03", Row(5m, 10m, 3m, 1m).TimeToFull);
        Assert.Equal("—", Row(5m, 10m, 1m, 2m).TimeToFull);
        Assert.Equal("—", Row(5m, 10m, 1m, 1m).TimeToFull);
        Assert.Equal("00:00:00", Row(10m, 10m, 0m, 1m).TimeToFull);
        Assert.Equal("—", Row(0m, 0m, 1m, 0m).TimeToFull);
        Assert.Equal("01:00:00", Row(0m, 3600m, 2m, 1m).TimeToFull);
    }
}
