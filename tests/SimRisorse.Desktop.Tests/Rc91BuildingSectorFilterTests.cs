using SimRisorse.Desktop.Presentation;
using SimRisorse.Core.Construction;
using Xunit;

namespace SimRisorse.Desktop.Tests;

/// <summary>RC91: the sector bar is presentation-only and keeps every catalog tier accessible.</summary>
public sealed class Rc91BuildingSectorFilterTests
{
    private static V15PreviewViewModel Fresh() => V15PreviewViewModel.CreateForPreview(
        Path.Combine(Path.GetTempPath(), "SimRisorse.RC91.Filter." + Guid.NewGuid().ToString("N") + ".sqlite3"));

    private static void Pick(V15PreviewViewModel vm, V15BuildingSector sector) =>
        vm.BuildingFilters.Single(option => option.Sector == sector).SelectCommand.Execute(null);

    [Fact]
    public void AllFilterIsActiveByDefaultAndShowsEveryUniqueEntry()
    {
        var vm = Fresh();
        Assert.Equal(V15BuildingSector.All, vm.ActiveBuildingSector);
        Assert.Equal(vm.Buildings.Count, vm.VisibleBuildings.Count);
        Assert.Equal(vm.Buildings.Count, vm.VisibleBuildings.Select(row => row.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Single(vm.BuildingFilters, option => option.IsSelected);
        Assert.True(vm.BuildingFilters.Single(option => option.Sector == V15BuildingSector.All).IsSelected);
    }

    [Theory]
    [InlineData("food-depot", V15BuildingSector.Food)]
    [InlineData("food-depot-large", V15BuildingSector.Food)]
    [InlineData("food-depot-advanced", V15BuildingSector.Food)]
    [InlineData("fisher", V15BuildingSector.Food)]
    [InlineData("coal-mine", V15BuildingSector.Extraction)]
    [InlineData("stonecutter-advanced", V15BuildingSector.Extraction)]
    [InlineData("mint-advanced", V15BuildingSector.Industry)]
    [InlineData("sawmill-large", V15BuildingSector.Processing)]
    [InlineData("warehouse", V15BuildingSector.Logistics)]
    [InlineData("small-house", V15BuildingSector.Housing)]
    [InlineData("town-hall", V15BuildingSector.Civic)]
    [InlineData("school", V15BuildingSector.Civic)]
    [InlineData("barracks", V15BuildingSector.Military)]
    public void RepresentativeBuildingsBelongToTheirRequestedSector(string id, V15BuildingSector expected)
    {
        var vm = Fresh();
        var row = Assert.Single(vm.Buildings, entry => entry.Id == id);
        Assert.Equal(expected, V15BuildingSectors.For(row));
    }

    [Fact]
    public void EveryBuildingBelongsToExactlyOneNamedSectorAndTheCatalogIsFullyCovered()
    {
        var vm = Fresh();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var option in vm.BuildingFilters.Where(option => option.Sector != V15BuildingSector.All))
        {
            Pick(vm, option.Sector);
            foreach (var row in vm.VisibleBuildings)
            {
                Assert.Equal(option.Sector, V15BuildingSectors.For(row));
                Assert.True(ids.Add(row.Id), $"Duplicate entry in sector classification: {row.Id}");
            }
        }
        Assert.Equal(vm.Buildings.Count, ids.Count);
        Assert.DoesNotContain(vm.Buildings, row => V15BuildingSectors.For(row) == V15BuildingSector.Other);
    }

    [Fact]
    public void FiltersIncludeLockedBuildingsAndAllThreeProductionTiers()
    {
        var vm = Fresh();
        Pick(vm, V15BuildingSector.Industry);
        Assert.Contains(vm.VisibleBuildings, row => row.Id == "mint" && !row.Unlocked);
        Assert.Contains(vm.VisibleBuildings, row => row.Id == "mint-large" && row.Tier == ProductionTier.Large);
        Assert.Contains(vm.VisibleBuildings, row => row.Id == "mint-advanced" && row.Tier == ProductionTier.Advanced);
        Assert.Equal(12, vm.VisibleBuildings.Count); // iron/gold smelters, forge, mint × three tiers
    }

    [Fact]
    public void SwitchingToMatchingSectorKeepsSelectedBuildingAndDetails()
    {
        var vm = Fresh();
        vm.SelectedBuilding = vm.Buildings.Single(row => row.Id == "mint-large");
        var selected = vm.SelectedBuilding;
        Pick(vm, V15BuildingSector.Industry);
        Assert.Same(selected, vm.SelectedBuilding);
        Assert.Contains(selected!, vm.VisibleBuildings);
        Pick(vm, V15BuildingSector.All);
        Assert.Same(selected, vm.SelectedBuilding);
    }

    [Fact]
    public void SwitchingToUnrelatedSectorSelectsAVisibleBuilding()
    {
        var vm = Fresh();
        vm.SelectedBuilding = vm.Buildings.Single(row => row.Id == "mint-large");
        Pick(vm, V15BuildingSector.Logistics);
        Assert.Single(vm.VisibleBuildings);
        Assert.Same(vm.VisibleBuildings[0], vm.SelectedBuilding);
        Assert.Equal("warehouse", vm.SelectedBuilding!.Id);
    }

    [Fact]
    public void RepeatingTheActiveFilterDoesNotResetSelection()
    {
        var vm = Fresh();
        Pick(vm, V15BuildingSector.Food);
        vm.SelectedBuilding = vm.VisibleBuildings.Single(row => row.Id == "bakery");
        Pick(vm, V15BuildingSector.Food);
        Assert.Equal("bakery", vm.SelectedBuilding!.Id);
    }

    [Fact]
    public void RefreshDoesNotDropFilterOrSelectedRow()
    {
        var vm = Fresh();
        Pick(vm, V15BuildingSector.Extraction);
        vm.SelectedBuilding = vm.VisibleBuildings.Single(row => row.Id == "coal-mine");
        vm.Refresh();
        Assert.Equal(V15BuildingSector.Extraction, vm.ActiveBuildingSector);
        Assert.Equal("coal-mine", vm.SelectedBuilding!.Id);
        Assert.All(vm.VisibleBuildings, row => Assert.Equal(V15BuildingSector.Extraction, V15BuildingSectors.For(row)));
    }

    [Fact]
    public void ExactlyOneButtonHasActivePresentationAfterEveryChange()
    {
        var vm = Fresh();
        foreach (var option in vm.BuildingFilters)
        {
            Pick(vm, option.Sector);
            Assert.Single(vm.BuildingFilters, button => button.IsSelected);
            Assert.True(option.IsSelected);
            Assert.All(vm.BuildingFilters.Where(other => other.Sector != option.Sector), other =>
            {
                Assert.False(other.IsSelected);
                Assert.NotEqual(option.Background, other.Background);
                Assert.NotEqual(option.Foreground, other.Foreground);
                Assert.NotEqual(option.Border, other.Border);
            });
        }
    }
}
