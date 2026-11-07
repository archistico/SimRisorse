using SimRisorse.Core.Construction;
using SimRisorse.Core.Food;
using SimRisorse.Desktop.Presentation;
using Xunit;

namespace SimRisorse.Desktop.Tests;

public sealed class Rc90TechnologyUnlockPresentationTests
{
    private static V15PreviewViewModel Fresh() => V15PreviewViewModel.CreateForPreview(
        Path.Combine(Path.GetTempPath(), "rc90-ui-" + Guid.NewGuid().ToString("N") + ".sqlite3"));

    [Theory]
    [InlineData("alimentazione-1", "Grande", "Deposito alimentare")]
    [InlineData("alimentazione-2", "Avanzato", "Deposito alimentare")]
    [InlineData("estrazione-1", "Grande", "Miniera")]
    [InlineData("estrazione-2", "Avanzato", "Miniera")]
    [InlineData("industria-1", "Grande", "Zecca")]
    [InlineData("industria-2", "Avanzato", "Zecca")]
    [InlineData("lavorazioni-1", "Grande", "Segheria")]
    [InlineData("lavorazioni-2", "Avanzato", "Segheria")]
    public void EveryAvailableTechnologyDescribesRealBuildingUnlocks(string id, string tier, string name)
    {
        var vm = Fresh();
        vm.SelectedTechnology = vm.Technologies.Single(t => t.Id == id);
        Assert.Contains(vm.SelectedTechnologyUnlocks, row => row.Tier == tier &&
            row.BuildingName.Contains(name, StringComparison.Ordinal) && PixelGlyph.Supports(row.GlyphKey));
        Assert.DoesNotContain("Sblocca edifici Grande:", vm.SelectedTechnologySummary);
        Assert.DoesNotContain("Sblocca edifici Avanzato:", vm.SelectedTechnologySummary);
    }

    [Fact]
    public void DeferredTechnologiesAreNotAdvertisedAsHavingWorkingUnlocks()
    {
        var vm = Fresh();
        vm.SelectedTechnology = vm.Technologies.Single(t => t.Id == "logistica-1");
        Assert.Contains("non ancora implementata", vm.SelectedTechnologySummary);
        Assert.Empty(vm.SelectedTechnologyUnlocks);
        Assert.False(vm.HasSelectedTechnologyUnlocks);
    }

    [Fact]
    public void FoodDepotTiersUseTheApprovedFoodIconAndShowActualCapacities()
    {
        var vm = Fresh();
        var large = vm.Buildings.Single(b => b.Id == "food-depot-large");
        var advanced = vm.Buildings.Single(b => b.Id == "food-depot-advanced");
        Assert.Equal("food-depot", large.GlyphKey);
        Assert.Equal("food-depot", advanced.GlyphKey);
        Assert.True(PixelGlyph.Supports(large.GlyphKey));
        Assert.Equal(ProductionTier.Large, large.Tier);
        Assert.Equal(ProductionTier.Advanced, advanced.Tier);
        Assert.False(large.Unlocked);
        Assert.False(advanced.Unlocked);
        Assert.Equal(0.48d, large.RowOpacity);
        Assert.Contains("Alimentazione I", large.Requirements);
        Assert.Contains("Alimentazione II", advanced.Requirements);
        Assert.Contains(large.CapacityLines, r => r.ResourceId == "food" && r.Text.StartsWith("300", StringComparison.Ordinal));
        Assert.Contains(advanced.CapacityLines, r => r.ResourceId == "food" && r.Text.StartsWith("1.000", StringComparison.Ordinal));
        Assert.NotEmpty(large.OutputLines);
        Assert.NotEmpty(advanced.InputLines);
        Assert.Equal(0, large.Count);
        Assert.Equal(0, advanced.Count);
    }

    [Fact]
    public void TechnologyDetailsKeepPrerequisitesVisibleWhenPresent()
    {
        var vm = Fresh();
        vm.SelectedTechnology = vm.Technologies.Single(t => t.Id == "alimentazione-2");
        Assert.Contains("Richiede ricerca: Alimentazione I", vm.SelectedTechnologySummary);
    }
}
