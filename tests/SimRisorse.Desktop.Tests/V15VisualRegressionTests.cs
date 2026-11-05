using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using Xunit;

namespace SimRisorse.Desktop.Tests;

public sealed class V15VisualRegressionTests
{
    private static V15PreviewViewModel Fresh() => V15PreviewViewModel.CreateForPreview(
        Path.Combine(Path.GetTempPath(), "SimRisorse.V15.Visual." + Guid.NewGuid().ToString("N") + ".sqlite3"));

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public void FoodFilterAndColumnHeadingsRemainReadableInBothFilterStates(int width, int height)
    {
        var vm = Fresh();
        var window = new V15PreviewWindow(vm, startTimer: false) { Width = width, Height = height };
        try
        {
            window.Show();
            foreach (var filtered in new[] { false, true })
            {
                if (filtered) vm.ToggleFoodIndustryFilterCommand.Execute(null);
                Dispatcher.UIThread.RunJobs();
                var headings = new[] { "STRUTTURA", "EDIFICI", "PERSONALE" }
                    .Select(label => window.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Text == label)).ToArray();
                var filter = window.FindControl<Button>("V15FoodIndustryFilter")!;
                var rectangles = headings.Select(text =>
                {
                    Assert.True(text.Bounds.Width >= text.TextLayout.Width - 0.5,
                        $"Heading {text.Text} has insufficient space: {text.Bounds.Width} < {text.TextLayout.Width}.");
                    return new Rect(text.TranslatePoint(default, window)!.Value, text.Bounds.Size);
                }).Append(new Rect(filter.TranslatePoint(default, window)!.Value, filter.Bounds.Size)).ToArray();
                for (int i = 0; i < rectangles.Length; i++)
                    for (int j = i + 1; j < rectangles.Length; j++)
                        Assert.False(rectangles[i].Intersects(rectangles[j]), $"Header controls {i} and {j} overlap.");
                Capture(window, $"v15-buildings{(filtered ? "-filtered" : "")}-{width}.png");
            }
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public void SchoolAndMilitarySupplyDescriptionsWrapInsideTheirCards(int width, int height)
    {
        var vm = Fresh();
        vm.SchoolQuantity = 10;
        vm.MilitaryQuantity = 10;
        var window = new V15PreviewWindow(vm, startTimer: false) { Width = width, Height = height };
        try
        {
            window.Show();
            var navigation = window.FindControl<ListBox>("V15NavigationMenu")!;
            foreach (var (section, text, name) in new[] {
                (2, vm.MilitaryWeaponsStatus, "military"), (3, vm.SchoolSuppliesStatus, "school") })
            {
                navigation.SelectedIndex = section;
                Dispatcher.UIThread.RunJobs();
                var description = window.GetVisualDescendants().OfType<TextBlock>().Single(t => t.Text == text);
                var card = description.GetVisualAncestors().OfType<Border>().First();
                var right = description.TranslatePoint(new Point(description.Bounds.Width, 0), card)!.Value.X;
                Assert.True(right <= card.Bounds.Width - card.Padding.Right - card.BorderThickness.Right + 0.5,
                    $"The {name} supply description extends beyond its card: {right} / {card.Bounds.Width}.");
                Assert.True(description.TextLayout.Width <= description.Bounds.Width + 0.5);
                if (width == 1040) Assert.True(description.TextLayout.TextLines.Count > 1);
                Capture(window, $"v15-{name}-{width}.png");
            }
        }
        finally { window.CloseWithoutSaving(); }
    }

    [Fact]
    public void BuildingAndBatchQuotesUseTheSameItalianResourceNamesAsIconLines()
    {
        var vm = Fresh();
        vm.SelectedBuilding = vm.Buildings.Single(b => b.Id == "sawmill");
        foreach (var quote in new[] { vm.SelectedBuilding.Price, vm.BuildFiveCost, vm.BuildTenCost })
        {
            Assert.Contains("Legno lavorato", quote);
            Assert.Contains("Pietra", quote);
            Assert.DoesNotContain("planks", quote);
            Assert.DoesNotContain("stone", quote);
        }
    }

    private static void Capture(Window window, string file)
    {
        using var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        var directory = Environment.GetEnvironmentVariable("SIMRISORSE_DESKTOP_ARTIFACTS");
        if (string.IsNullOrWhiteSpace(directory)) return;
        Directory.CreateDirectory(directory);
        frame.Save(Path.Combine(directory, file), PngBitmapEncoderOptions.Default);
    }
}
