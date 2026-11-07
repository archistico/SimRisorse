using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using Xunit;

namespace SimRisorse.Desktop.Tests;

public sealed class Rc90_3TechnologyIconsTests
{
    private static V15PreviewViewModel V15() => V15PreviewViewModel.CreateForPreview(
        Path.Combine(Path.GetTempPath(), "rc90-3-v15-" + Guid.NewGuid().ToString("N") + ".sqlite3"));

    private static V14PreviewViewModel V14() => V14PreviewViewModel.CreateForPreview(
        Path.Combine(Path.GetTempPath(), "rc90-3-v14-" + Guid.NewGuid().ToString("N") + ".sqlite3"));

    [Theory]
    [InlineData("lavorazioni-1", "sawmill")]
    [InlineData("alimentazione-2", "bread")]
    [InlineData("estrazione-1", "coal-mine")]
    [InlineData("industria-2", "forge")]
    [InlineData("logistica-1", "warehouse")]
    [InlineData("navigazione-1", "ship")]
    public void EachResearchBranchHasItsOwnExistingPixelGlyph(string technologyId, string glyph)
    {
        Assert.Equal(glyph, TechnologyPresentation.GlyphForTechnology(technologyId));
        Assert.True(PixelGlyph.Supports(glyph));
        Assert.Equal(glyph, V15().Technologies.Single(row => row.Id == technologyId).GlyphKey);
        Assert.Equal(glyph, V14().Technologies.Single(row => row.Id == technologyId).GlyphKey);
    }

    [Fact]
    public void V15ListsOnlyRealUnlocksWithCatalogBuildingIconsAndFoodDepotTiers()
    {
        var vm = V15();
        foreach (var research in vm.Technologies)
        {
            vm.SelectedTechnology = research;
            Assert.All(vm.SelectedTechnologyUnlocks, row =>
            {
                Assert.True(PixelGlyph.Supports(row.GlyphKey), $"No icon for {row.Label}");
                Assert.Contains(row.Tier, new[] { "Grande", "Avanzato" });
            });
            Assert.Equal(vm.SelectedTechnologyUnlocks.Count > 0, vm.HasSelectedTechnologyUnlocks);
            Assert.DoesNotContain(vm.SelectedTechnologyUnlocks, row => row.GlyphKey == "food-store");
            if (research.Id is "logistica-1" or "logistica-2" or "navigazione-1")
                Assert.Empty(vm.SelectedTechnologyUnlocks);
        }
        vm.SelectedTechnology = vm.Technologies.Single(row => row.Id == "alimentazione-1");
        Assert.Contains(vm.SelectedTechnologyUnlocks, row => row.Label == "Deposito alimentare · Grande" &&
            row.GlyphKey == "food-depot");
        vm.SelectedTechnology = vm.Technologies.Single(row => row.Id == "alimentazione-2");
        Assert.Contains(vm.SelectedTechnologyUnlocks, row => row.Label == "Deposito alimentare · Avanzato" &&
            row.GlyphKey == "food-depot");
        Assert.DoesNotContain(vm.SelectedTechnologyUnlocks, row => row.Tier == "Grande");
    }

    [Fact]
    public void V14KeepsSeparateTiersAndNeverPromisesUnavailableResearchUnlocks()
    {
        var vm = V14();
        foreach (var tech in vm.Technologies)
        {
            vm.SelectedTechnology = tech;
            Assert.Equal(vm.SelectedTechnologyUnlocks.Count > 0, vm.HasSelectedTechnologyUnlocks);
            Assert.All(vm.SelectedTechnologyUnlocks, row => Assert.True(PixelGlyph.Supports(row.GlyphKey)));
            if (tech.Id.StartsWith("logistica-", StringComparison.Ordinal) || tech.Id == "navigazione-1")
                Assert.Empty(vm.SelectedTechnologyUnlocks);
        }
    }

    [AvaloniaFact]
    public void V15ResearchTabRendersTechnologyAndBuildingGlyphsAndRebindsWhenSelected()
    {
        var vm = V15();
        var window = new V15PreviewWindow(vm, startTimer: false) { Width = 1240, Height = 850 };
        try
        {
            window.Show();
            window.FindControl<TabControl>("V15Sections")!.SelectedIndex = 4;
            vm.SelectedTechnology = vm.Technologies.Single(row => row.Id == "alimentazione-1");
            Dispatcher.UIThread.RunJobs();
            var list = window.FindControl<ListBox>("V15Technologies")!;
            Assert.Contains(list.GetVisualDescendants().OfType<PixelGlyph>(), glyph => glyph.Key == "bread");
            var unlocks = window.FindControl<ItemsControl>("V15TechnologyUnlocks")!;
            Assert.True(unlocks.IsEffectivelyVisible);
            Assert.Contains(unlocks.GetVisualDescendants().OfType<PixelGlyph>(), glyph => glyph.Key == "food-depot");
            Assert.Contains(unlocks.GetVisualDescendants().OfType<TextBlock>(), text =>
                text.Text == "Deposito alimentare · Grande");
            vm.SelectedTechnology = vm.Technologies.Single(row => row.Id == "alimentazione-2");
            Dispatcher.UIThread.RunJobs();
            Assert.Contains(unlocks.GetVisualDescendants().OfType<TextBlock>(), text =>
                text.Text == "Deposito alimentare · Avanzato");
            vm.SelectedTechnology = vm.Technologies.Single(row => row.Id == "logistica-1");
            Dispatcher.UIThread.RunJobs();
            Assert.False(unlocks.IsEffectivelyVisible);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public void V14ResearchTabRendersIconsAndUnlocksWithoutChangingLayout()
    {
        var vm = V14();
        var window = new V14PreviewWindow(vm, startTimer: false) { Width = 1240, Height = 850 };
        try
        {
            window.Show();
            window.FindControl<TabControl>("V14Sections")!.SelectedIndex = 4;
            vm.SelectedTechnology = vm.Technologies.Single(row => row.Id == "lavorazioni-1");
            Dispatcher.UIThread.RunJobs();
            var list = window.FindControl<ListBox>("V14Technologies")!;
            Assert.Contains(list.GetVisualDescendants().OfType<PixelGlyph>(), glyph => glyph.Key == "sawmill");
            var unlocks = window.FindControl<ItemsControl>("V14TechnologyUnlocks")!;
            Assert.True(unlocks.IsEffectivelyVisible);
            Assert.Contains(unlocks.GetVisualDescendants().OfType<PixelGlyph>(), glyph => glyph.Key == "sawmill");
            Assert.Contains(unlocks.GetVisualDescendants().OfType<TextBlock>(), text =>
                text.Text == "Segheria · Grande");
        }
        finally { window.CloseWithoutSaving(); }
    }
}
