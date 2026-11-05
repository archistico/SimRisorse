using System.Collections.Immutable;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SimRisorse.Application;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.ReadModels;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Sessions;
using SimRisorse.Desktop;
using SimRisorse.Desktop.Lifecycle;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(SimRisorse.Desktop.Tests.TestAppBuilder))]

namespace SimRisorse.Desktop.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UseSkia().UseHarfBuzz()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}

public sealed class DesktopTests
{
    [AvaloniaFact]
    public async Task TownHallIsVisibleAsAStartingStructureWithoutConstructionControls()
    {
        var vm = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await vm.InitializeAsync();
        var townHall = vm.Buildings.Single(item => item.Id == "town-hall");
        Assert.True(PixelGlyph.Supports("town-hall"));
        Assert.Equal(1, townHall.Count);
        Assert.True(townHall.IsScenarioFixed);
        Assert.Equal("+1.000 posti per ogni risorsa", Assert.Single(townHall.CapacityLines).Text);
        vm.Selected = townHall;
        Assert.False(vm.ConstructCommand.CanExecute(null));
        var window = new MainWindow(vm, startTimer: false, initialize: false);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.False(window.FindControl<Button>("BuildButton")!.IsEffectivelyVisible);
            Assert.False(window.FindControl<ComboBox>("BatchSelector")!.IsEffectivelyVisible);
            Assert.False(window.FindControl<Button>("AssignButton")!.IsEffectivelyVisible);
            Assert.False(window.FindControl<Button>("ReleaseButton")!.IsEffectivelyVisible);
            Assert.True(window.FindControl<ItemsControl>("BuildingCapacityList")!.IsEffectivelyVisible);
        }
        finally { window.CloseWithoutSaving(); }
    }

    private static GameSnapshot MilitaryFixture(decimal swords = 40m)
    {
        var initial = GameSession.Create(CivilGrowthScenario.CreateTownHallForBenchmarks()).Snapshot();
        return initial with
        {
            Population = 40,
            Resources = SimRisorse.Core.Economy.ResourceBundle.Create(("planks", 1000m), ("stone", 1000m),
                ("food", 100m), ("bread", 100m), ("swords", swords)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId is "large-house" or "forge" or "barracks"
                ? item with { Count = 1 } : item)]
        };
    }

    [Fact]
    public async Task BarracksDescriptionShowsSeatsTrainingAndPopulationNotHousingPlaces()
    {
        var vm = new MainViewModel(new GameRuntime(CivilGrowthScenario.CreateTownHallForBenchmarks()),
            new MemoryStore { Snapshot = MilitaryFixture() });
        await vm.InitializeAsync();
        var barracks = vm.Buildings.Single(item => item.Id == "barracks");
        Assert.True(PixelGlyph.Supports("barracks"));
        Assert.Equal("+2 posti di addestramento per Caserma", barracks.Recipe);
        Assert.Contains("2 posti di addestramento", barracks.Status);
        Assert.DoesNotContain("abitativi", barracks.Status);
        Assert.Contains("60 secondi", barracks.Description);
        Assert.Contains("senza lavoratori", barracks.Description);
        Assert.Contains("Esercito", barracks.Activity);
        Assert.False(barracks.IsProducer);
        Assert.Equal("0 / 0", barracks.Staff);
    }

    [Fact]
    public async Task MilitaryUiExplainsWhenQueueRefundIsBlockedByFullSwordStorage()
    {
        var seed = MilitaryFixture(1070m) with
        {
            Military = new SimRisorse.Core.Military.MilitaryTrainingState(1, 2, 0, 0)
        };
        var vm = new MainViewModel(new GameRuntime(CivilGrowthScenario.CreateTownHallForBenchmarks()),
            new MemoryStore { Snapshot = seed });
        await vm.InitializeAsync();
        Assert.False(vm.CancelRecruitsCommand.CanExecute(null));
        Assert.Contains("Spazio insufficiente", vm.MilitaryCancelHint);
        Assert.Contains("Magazzino", vm.MilitaryCancelHint);
        Assert.Equal(3, vm.MilitaryPeople);
        Assert.Equal(34, vm.View.AvailableWorkers);
    }

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public async Task MilitaryNavigationAndCommandsRemainUsableAcrossBothDesktopSizes(int width, int height)
    {
        var vm = new MainViewModel(new GameRuntime(CivilGrowthScenario.CreateTownHallForBenchmarks()),
            new MemoryStore { Snapshot = MilitaryFixture() });
        await vm.InitializeAsync();
        var window = new MainWindow(vm, startTimer: false, initialize: false)
        { Width = width, Height = height };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var navigation = window.FindControl<ListBoxItem>("MilitaryNavigation")!;
            Assert.True(vm.HasMilitaryRules);
            Assert.True(navigation.IsVisible);
            var point = navigation.TranslatePoint(new Point(navigation.Bounds.Width / 2,
                navigation.Bounds.Height / 2), window)!.Value;
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(2, window.FindControl<TabControl>("Sections")!.SelectedIndex);
            Assert.True(window.FindControl<TextBlock>("MilitaryRosterText")!.IsEffectivelyVisible);
            Assert.Contains("40 abitanti", window.FindControl<TextBlock>("MilitaryRosterText")!.Text);
            Assert.True(window.FindControl<PixelGlyph>("PopulationGlyph")!.IsEffectivelyVisible);
            Assert.True(window.FindControl<Button>("EnlistButton")!.IsEffectivelyEnabled);
            Assert.False(window.FindControl<Button>("DischargeButton")!.IsEffectivelyEnabled);
            Assert.False(window.FindControl<Button>("CancelQueueButton")!.IsEffectivelyEnabled);

            var selector = window.FindControl<ComboBox>("MilitaryQuantitySelector")!;
            selector.SelectedItem = 5;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(10, vm.MilitarySwordCost);
            Click(window, "EnlistButton");
            Assert.Equal((3, 2, 0), (vm.WaitingRecruits, vm.TrainingRecruits, vm.AvailableSoldiers));
            Assert.Equal(30m, vm.AvailableSwords);
            Assert.Equal(32, vm.View.AvailableWorkers);

            selector.SelectedItem = 1;
            Dispatcher.UIThread.RunJobs();
            Click(window, "CancelQueueButton");
            Assert.Equal((2, 2, 0), (vm.WaitingRecruits, vm.TrainingRecruits, vm.AvailableSoldiers));
            Assert.Equal(32m, vm.AvailableSwords);

            Click(window, "PauseButton");
            vm.Advance(TimeSpan.FromSeconds(30));
            Dispatcher.UIThread.RunJobs();
            Assert.Contains("00:30", window.FindControl<TextBlock>("MilitaryTrainingText")!.Text);
            Assert.InRange(window.FindControl<ProgressBar>("MilitaryProgressBar")!.Value, 49, 51);
            vm.Advance(TimeSpan.FromSeconds(30));
            Dispatcher.UIThread.RunJobs();
            Assert.Equal((0, 2, 2), (vm.WaitingRecruits, vm.TrainingRecruits, vm.AvailableSoldiers));
            Click(window, "DischargeButton");
            Assert.Equal(1, vm.AvailableSoldiers);
            Assert.Equal(34, vm.View.AvailableWorkers);
            Assert.Contains("non vengono rimborsate", vm.MilitaryDischargeHint);
            Assert.Contains("1", window.FindControl<TextBlock>("MilitarySoldiersCount")!.Text);

            foreach (var name in new[] { "EnlistButton", "CancelQueueButton", "DischargeButton", "MilitaryQuantitySelector" })
            {
                var control = window.FindControl<Control>(name)!;
                Assert.True(control.IsEffectivelyVisible);
                var position = control.TranslatePoint(default, window)!.Value;
                Assert.InRange(position.X, 0, window.ClientSize.Width - control.Bounds.Width);
                Assert.InRange(position.Y, 0, window.ClientSize.Height - control.Bounds.Height);
            }
            var buttons = new[] { "EnlistButton", "CancelQueueButton", "DischargeButton" }
                .Select(name => window.FindControl<Button>(name)!).ToArray();
            for (var index = 1; index < buttons.Length; index++)
            {
                var left = buttons[index - 1].TranslatePoint(default, window)!.Value.X + buttons[index - 1].Bounds.Width;
                var right = buttons[index].TranslatePoint(default, window)!.Value.X;
                Assert.True(left + 1 <= right, "I comandi militari non devono sovrapporsi.");
            }
            using var screenshot = window.CaptureRenderedFrame();
            Assert.NotNull(screenshot);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [Fact]
    public async Task V13ArmyTabShowsIndividualTrainingAndAllowsCancellationButNotDischarge()
    {
        var scenario = CivilGrowthScenario.Create();
        var start = GameSession.Create(scenario).Snapshot();
        var snapshot = start with
        {
            Population = 40,
            Resources = SimRisorse.Core.Economy.ResourceBundle.Create(("planks", 1000m),
                ("stone", 1000m), ("food", 100m), ("bread", 100m), ("swords", 100m)),
            Buildings = [.. start.Buildings.Select(item => item.BuildingId is "large-house" or "forge" or "barracks"
                ? item with { Count = 1 } : item)]
        };
        var vm = new MainViewModel(new GameRuntime(scenario), new MemoryStore { Snapshot = snapshot });
        await vm.InitializeAsync();
        var barracks = vm.Buildings.Single(item => item.Id == "barracks");
        Assert.Equal("+1 posto di addestramento per Caserma", barracks.Recipe);
        Assert.Contains("260 Tavole", barracks.Description);
        Assert.Contains("30 secondi", barracks.Description);
        Assert.Contains("5 Spade", vm.MilitaryWeaponsStatus);
        Assert.False(vm.DischargeCommand.CanExecute(null));
        Assert.True(vm.EnlistCommand.CanExecute(null));
        vm.MilitaryQuantity = 5;
        vm.EnlistCommand.Execute(null);
        Assert.Equal((4, 1, 0), (vm.WaitingRecruits, vm.TrainingRecruits, vm.AvailableSoldiers));
        Assert.Equal(75m, vm.AvailableSwords);
        Assert.Contains("Annulla reclute", vm.CancelRecruitsLabel);
        Assert.True(vm.CancelRecruitsCommand.CanExecute(null));
        vm.CancelRecruitsCommand.Execute(null);
        Assert.Equal(0, vm.TrainingRecruits);
        Assert.Equal(0, vm.WaitingRecruits);
        Assert.Equal(100m, vm.AvailableSwords);
    }

    [AvaloniaFact]
    public async Task HistoricalFoundationKeepsMilitaryNavigationHidden()
    {
        var vm = new MainViewModel(new GameRuntime(EconomyFoundationScenario.Create()), new MemoryStore());
        await vm.InitializeAsync();
        var window = new MainWindow(vm, startTimer: false, initialize: false);
        try
        {
            Assert.False(vm.HasMilitaryRules);
            Assert.False(window.FindControl<ListBoxItem>("MilitaryNavigation")!.IsVisible);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.False(vm.HasMilitaryRules);
            Assert.False(window.FindControl<ListBoxItem>("MilitaryNavigation")!.IsVisible);
            Assert.False(window.FindControl<ListBoxItem>("MilitaryNavigation")!.IsEffectivelyVisible);
            Assert.False(window.FindControl<TabItem>("MilitaryTab")!.IsEffectivelyVisible);
            window.FindControl<TabControl>("Sections")!.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.True(window.FindControl<ItemsControl>("ResourcesTable")!.IsEffectivelyVisible);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [Fact]
    public async Task BreadStorageExplainsItsCapacityItsProducerBatchAndWhichBuildingExpandsIt()
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        viewModel.SelectedResource = viewModel.Resources.Single(item => item.Id == "bread");

        Assert.Contains("Pane: 0 / 1.010", viewModel.SelectedResourceStorageDetails);
        Assert.Contains("1 Municipio × 1.000", viewModel.SelectedResourceStorageDetails);
        Assert.Contains("1 Casa piccola × 10", viewModel.SelectedResourceStorageDetails);
        Assert.Contains("0 Magazzini × 500", viewModel.SelectedResourceStorageDetails);
        Assert.Contains("3 posti liberi", viewModel.SelectedResourceStorageDetails);

        var game = viewModel.View;
        var bread = game.Resources.Single(item => item.Id == "bread") with { Quantity = 1009m };
        var capacityReached = game with
        {
            Resources = game.Resources.Select(item => item.Id == "bread" ? bread : item).ToImmutableArray()
        };
        var baker = game.Buildings.Single(item => item.Id == "bakery") with
        {
            Count = 1, AssignedWorkers = 1, BlockReason = SimRisorse.Core.Economy.ProductionBlockReason.StorageFull,
            FullOutputNames = ["Pane"]
        };
        var diagnosis = new BuildingRow(baker, capacityReached).Diagnosis;
        Assert.Contains("Pane: 1.009 / 1.010", diagnosis);
        Assert.Contains("liberi 1", diagnosis);
        Assert.Contains("servono 3 posti per lotto", diagnosis);
        Assert.Contains("costruisci un Magazzino", diagnosis);
        Assert.Contains("Deposito alimentare produce Cibo", diagnosis);

        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "warehouse");
        viewModel.ConstructCommand.Execute(null);
        Assert.Equal(1, viewModel.Selected.Count);
        Assert.Contains("Pane: 0 / 1.510", viewModel.SelectedResourceStorageDetails);
        Assert.Contains("1 Municipio × 1.000", viewModel.SelectedResourceStorageDetails);
        Assert.Contains("1 Magazzino × 500", viewModel.SelectedResourceStorageDetails);
        Assert.Equal(viewModel.SelectedResource!.CapacityBreakdown, viewModel.SelectedResourceStorageDetails);
        Assert.Contains("Non conserva il Pane", viewModel.Buildings.Single(item => item.Id == "food-store").Description);
    }

    [Fact]
    public async Task ProducerWithOnSiteStorageStillShowsItsRecipeAndMissingInputs()
    {
        var vm = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await vm.InitializeAsync();
        var bakery = vm.View.Buildings.Single(item => item.Id == "bakery") with
        {
            Count = 1,
            AssignedWorkers = 1,
            BlockReason = SimRisorse.Core.Economy.ProductionBlockReason.MissingInputs,
            MissingInputNames = ["Farina"]
        };
        var row = new BuildingRow(bakery, vm.View);

        Assert.True(row.IsProducer);
        Assert.True(row.HasStorage);
        Assert.True(row.HasInputs);
        Assert.Equal("Materie prime insufficienti", row.Status);
        Assert.Equal("1 farina · 1 acqua → 3 pane", row.Recipe);
        Assert.Contains("Farina", row.Diagnosis);
        Assert.Contains(row.CapacityLines, item => item.ResourceId == "bread" && item.Text.Contains("+50 Pane"));
    }

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public async Task EveryBuildingShowsCapacityWithSameIconAndFontAsCostsAndProduction(int width, int height)
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = width, Height = height };
        try
        {
            window.Show();
            var capacity = window.FindControl<ItemsControl>("BuildingCapacityList")!;
            var costs = window.FindControl<ItemsControl>("ConstructionCostList")!;
            foreach (var (id, icons, labels) in new (string Id, string[] Icons, string[] Labels)[]
            {
                ("warehouse", ["warehouse"], ["+500 posti per ogni risorsa"]),
                ("small-house", ["population", "bread", "food"], ["+8 posti abitativi", "+10 Pane", "+10 Cibo"]),
                ("large-house", ["population", "bread", "food"], ["+32 posti abitativi", "+25 Pane", "+25 Cibo"]),
                ("barracks", ["barracks", "swords"], ["+1 posto di addestramento", "+20 Spade"]),
                ("bakery", ["bread"], ["+50 Pane"]),
                ("food-store", ["food"], ["+50 Cibo"])
            })
            {
                viewModel.Selected = viewModel.Buildings.Single(item => item.Id == id);
                Dispatcher.UIThread.RunJobs();
                var lines = capacity.GetVisualDescendants().OfType<TextBlock>().ToArray();
                var glyphs = capacity.GetVisualDescendants().OfType<PixelGlyph>().ToArray();
                var costText = costs.GetVisualDescendants().OfType<TextBlock>().First();
                Assert.Equal(icons, glyphs.Select(item => item.Key));
                Assert.Equal(labels.Length, lines.Length);
                for (var index = 0; index < lines.Length; index++)
                {
                    Assert.Contains(labels[index], lines[index].Text!);
                    Assert.Equal(14d, glyphs[index].Width);
                    Assert.Equal(14d, glyphs[index].Height);
                    Assert.Equal(costText.FontFamily, lines[index].FontFamily);
                    Assert.Equal(costText.FontSize, lines[index].FontSize);
                    Assert.Equal(costText.FontWeight, lines[index].FontWeight);
                }
            }
        }
        finally { window.CloseWithoutSaving(); }
    }

    [Fact]
    public void ResourceTableRoundsOnlyDisplayedQuantityAndEstimatesTimeToFullFromNetRate()
    {
        var row = new ResourceRow(new ResourceView("logs", "Tronchi", 12.5m, 100m, 2m, 0.5m));
        Assert.Equal("13", row.QuantityRounded);
        Assert.Equal("12,5", row.Quantity);
        Assert.Equal("100", row.Capacity);
        Assert.Equal("00:00:59", row.TimeToFull); // (100 - 12,5) / 1,5 = 58,33... s

        row.Update(new ResourceView("logs", "Tronchi", 99.5m, 100m, 0.75m, 0m));
        Assert.Equal("100", row.QuantityRounded);
        Assert.Equal("00:00:01", row.TimeToFull);
        row.Update(new ResourceView("logs", "Tronchi", 100m, 100m, 0m, 0m));
        Assert.Equal("00:00:00", row.TimeToFull);
        row.Update(new ResourceView("logs", "Tronchi", 20m, 100m, 1m, 1m));
        Assert.Equal("—", row.TimeToFull);
        row.Update(new ResourceView("logs", "Tronchi", 20m, 100m, 0.5m, 1m));
        Assert.Equal("—", row.TimeToFull);
        row.Update(new ResourceView("logs", "Tronchi", 0m, 0m, 1m, 0m));
        Assert.Equal("—", row.TimeToFull);
        row.Update(new ResourceView("logs", "Tronchi", 0m, 100m, 0.001m, 0m));
        Assert.Equal("27:46:40", row.TimeToFull); // Le ore totali non si azzerano dopo 24h.
        row.Update(new ResourceView("logs", "Tronchi", 1234.5m, 2000m, 0m, 0m));
        Assert.Equal("1.235", row.QuantityRounded);
    }

    [Fact]
    public async Task RateBrushesAreImmutableAndSafeAcrossHeadlessUiThreads()
    {
        // Un normale SolidColorBrush statico resta associato al Dispatcher del
        // primo test: quello successivo fallisce durante il rendering del testo.
        var surplus = new ResourceRow(new ResourceView("logs", "Tronchi", 1m, 10m, 2m, 1m)).NetForeground;
        var deficit = new ResourceRow(new ResourceView("bread", "Pane", 1m, 10m, 1m, 2m)).NetForeground;
        Assert.IsAssignableFrom<IImmutableBrush>(surplus);
        Assert.IsAssignableFrom<IImmutableBrush>(deficit);

        await Task.Run(() =>
        {
            Assert.Equal(Color.Parse("#176858"), Assert.IsAssignableFrom<ISolidColorBrush>(surplus).Color);
            Assert.Equal(Color.Parse("#A44732"), Assert.IsAssignableFrom<ISolidColorBrush>(deficit).Color);
            // Forza anche la lettura dell'attributo usato dal compositor
            // nel punto preciso dell'eccezione riportata dal test Windows.
            _ = surplus.Transform;
            _ = deficit.Transform;
        }, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ResourceTableIsAlphabeticalByItalianNameWithoutChangingHighlightsOrUpdates(bool civil)
    {
        var scenario = civil ? CivilGrowthScenario.Create() : EconomyFoundationScenario.Create();
        var viewModel = new MainViewModel(new GameRuntime(scenario), new MemoryStore());
        await viewModel.InitializeAsync();

        var italian = StringComparer.Create(CultureInfo.GetCultureInfo("it-IT"), ignoreCase: true);
        var expected = scenario.Catalog.Resources.OrderBy(item => item.Name, italian)
            .ThenBy(item => item.Id, StringComparer.Ordinal).Select(item => item.Id).ToArray();
        Assert.Equal(expected, viewModel.Resources.Select(item => item.Id));
        Assert.Equal(scenario.Catalog.Resources.Select(item => item.Id), viewModel.View.Resources.Select(item => item.Id));
        string[] expectedHighlights = civil ? ["food", "planks", "stone"] : ["logs", "planks", "stone"];
        Assert.Equal(expectedHighlights, viewModel.ResourceHighlights.Select(item => item.Id));

        var rows = viewModel.Resources.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var initialQuantities = rows.ToDictionary(item => item.Key, item => item.Value.Quantity, StringComparer.Ordinal);
        viewModel.TogglePauseCommand.Execute(null);
        viewModel.Advance(TimeSpan.FromSeconds(2));
        viewModel.Pause();

        Assert.Equal(expected, viewModel.Resources.Select(item => item.Id));
        Assert.All(viewModel.Resources, row => Assert.Same(rows[row.Id], row));
        Assert.Contains(viewModel.Resources, row => row.Quantity != initialQuantities[row.Id]);
    }

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public async Task SidebarNavigationAndSummaryCardsKeepConstructionDetailsInReadingOrder(int width, int height)
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = width, Height = height };
        try
        {
            window.Show();
            viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "tree-feller");
            Dispatcher.UIThread.RunJobs();
            var description = window.FindControl<TextBlock>("BuildingDescriptionText")!;
            var costs = window.FindControl<ItemsControl>("ConstructionCostList")!;
            var output = window.FindControl<ItemsControl>("ProductionOutputList")!;
            TextBlock OutputText() => Assert.Single(output.GetVisualDescendants().OfType<TextBlock>());
            string[] CostTexts() => costs.GetVisualDescendants().OfType<TextBlock>().Select(item => item.Text!).ToArray();
            Assert.Contains("20 legno lavorato", CostTexts());
            Assert.Contains("10 pietra", CostTexts());
            Assert.Equal("2 tronchi", OutputText().Text);
            var costText = costs.GetVisualDescendants().OfType<TextBlock>().First();
            Assert.Equal(costText.FontFamily, OutputText().FontFamily);
            Assert.Equal(costText.FontSize, OutputText().FontSize);
            Assert.Equal(costText.FontWeight, OutputText().FontWeight);
            Assert.Equal("logs", Assert.Single(output.GetVisualDescendants().OfType<PixelGlyph>()).Key);
            viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "iron-smelter");
            Dispatcher.UIThread.RunJobs();
            var inputs = window.FindControl<ItemsControl>("ProductionInputList")!;
            var inputTexts = inputs.GetVisualDescendants().OfType<TextBlock>().ToArray();
            var inputGlyphs = inputs.GetVisualDescendants().OfType<PixelGlyph>().ToArray();
            // Dopo il cambio edificio i vecchi TextBlock sono staccati dall'albero
            // visuale e perdono i font ereditati: confrontare i controlli attivi.
            var activeCostText = costs.GetVisualDescendants().OfType<TextBlock>().First();
            var activeOutputText = OutputText();
            Assert.True(inputs.IsEffectivelyVisible);
            Assert.Equal(viewModel.Selected.View.InputPerWorker.Length, inputTexts.Length);
            Assert.Equal(viewModel.Selected.View.InputPerWorker.Select(item => item.ResourceId), inputGlyphs.Select(item => item.Key));
            Assert.Equal(activeCostText.FontFamily, activeOutputText.FontFamily);
            Assert.Equal(activeCostText.FontSize, activeOutputText.FontSize);
            Assert.Equal(activeCostText.FontWeight, activeOutputText.FontWeight);
            Assert.All(inputTexts, item =>
            {
                Assert.Equal(activeCostText.FontFamily, item.FontFamily);
                Assert.Equal(activeCostText.FontSize, item.FontSize);
                Assert.Equal(activeCostText.FontWeight, item.FontWeight);
            });
            Assert.All(inputGlyphs, item =>
            {
                Assert.Equal(14d, item.Width);
                Assert.Equal(14d, item.Height);
            });
            viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "tree-feller");
            Dispatcher.UIThread.RunJobs();
            Assert.True(window.FindControl<PixelGlyph>("PopulationGlyph")!.IsEffectivelyVisible);
            Assert.True(description.TranslatePoint(default, window)!.Value.Y + description.Bounds.Height <=
                costs.TranslatePoint(default, window)!.Value.Y);
            Assert.True(costs.TranslatePoint(default, window)!.Value.Y + costs.Bounds.Height <=
                output.TranslatePoint(default, window)!.Value.Y);
            Assert.True(output.TranslatePoint(default, window)!.Value.Y + output.Bounds.Height <
                window.FindControl<ComboBox>("BatchSelector")!.TranslatePoint(default, window)!.Value.Y);

            window.FindControl<ComboBox>("BatchSelector")!.SelectedItem = 5;
            Dispatcher.UIThread.RunJobs();
            Assert.Contains("100 legno lavorato", CostTexts());
            Assert.Contains("50 pietra", CostTexts());
            Assert.Equal("COSTO PER 5 EDIFICI", window.FindControl<TextBlock>("ConstructionCostText")!.Text);
            Assert.Equal("2 tronchi", OutputText().Text);
            window.FindControl<ComboBox>("BatchSelector")!.SelectedItem = 1;
            viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "small-house");
            Click(window, "BuildButton");
            Assert.Equal("8 / 16", window.FindControl<TextBlock>("PopulationCountText")!.Text);
            Assert.Equal("5 liberi", window.FindControl<TextBlock>("AvailableWorkersText")!.Text);
            viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "tree-feller");
            Click(window, "BuildButton");
            Click(window, "AssignButton");
            Assert.Equal("4 liberi", window.FindControl<TextBlock>("AvailableWorkersText")!.Text);
            var population = window.FindControl<Border>("PopulationCard")!;
            var resources = window.FindControl<ItemsControl>("ResourceHighlights")!;
            Assert.Equal(population.TranslatePoint(default, window)!.Value.Y, resources.TranslatePoint(default, window)!.Value.Y);
            foreach (var bar in resources.GetVisualDescendants().OfType<ProgressBar>()
                .Concat(population.GetVisualDescendants().OfType<ProgressBar>()))
            {
                var card = bar.GetVisualAncestors().OfType<Border>().First();
                var position = bar.TranslatePoint(default, card)!.Value;
                Assert.InRange(position.X, 0, card.Bounds.Width - bar.Bounds.Width);
            }

            foreach (var (name, index) in new[] { ("ResourcesNavigation", 1), ("BuildingsNavigation", 0) })
            {
                var navigation = window.FindControl<ListBoxItem>(name)!;
                var position = navigation.TranslatePoint(default, window)!.Value;
                Assert.InRange(position.X + navigation.Bounds.Width, 0, window.FindControl<Border>("SidebarPanel")!.Bounds.Width);
                var point = navigation.TranslatePoint(new Point(navigation.Bounds.Width / 2, navigation.Bounds.Height / 2), window)!.Value;
                window.MouseDown(point, MouseButton.Left);
                window.MouseUp(point, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(index, window.FindControl<TabControl>("Sections")!.SelectedIndex);
                Assert.True(population.IsEffectivelyVisible);
                Assert.True(resources.IsEffectivelyVisible);
                if (index == 1)
                {
                    Assert.True(window.FindControl<ItemsControl>("ResourcesTable")!.IsEffectivelyVisible);
                    var header = window.FindControl<Grid>("ResourceTableHeader")!;
                    Assert.Equal(7, header.ColumnDefinitions.Count);
                    Assert.True(header.Bounds.Width <= ResourceScroll(window).Bounds.Width);
                }
            }
            using var image = window.CaptureRenderedFrame();
            Assert.NotNull(image);
            var artifacts = Environment.GetEnvironmentVariable("SIMRISORSE_DESKTOP_ARTIFACTS");
            if (!string.IsNullOrWhiteSpace(artifacts))
            {
                Directory.CreateDirectory(artifacts);
                image.Save(Path.Combine(artifacts, $"structure-details-{width}.png"), PngBitmapEncoderOptions.Default);
            }
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaTheory]
    [InlineData(1040, 740, 0, "industry-buildings-compact.png", false)]
    [InlineData(1240, 850, 0, "industry-buildings.png", false)]
    [InlineData(1040, 740, 1, "industry-resources-compact.png", false)]
    [InlineData(1240, 850, 1, "industry-resources.png", false)]
    [InlineData(1040, 740, 0, "gold-buildings-compact.png", true)]
    [InlineData(1240, 850, 0, "gold-buildings.png", true)]
    [InlineData(1040, 740, 1, "gold-resources-compact.png", true)]
    [InlineData(1240, 850, 1, "gold-resources.png", true)]
    public async Task IndustrialCommandsShowPrerequisitesProductionAndScrollableResources(int width, int height, int section, string file, bool includeGold)
    {
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot();
        var store = new MemoryStore { Snapshot = initial with
        {
            Resources = SimRisorse.Core.Economy.ResourceBundle.Create(("planks", 1000m), ("stone", 1000m), ("food", 100m), ("bread", 100m)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId is "large-house" or "bakery" ? item with { Count = 1 } : item)]
        } };
        var viewModel = new MainViewModel(new GameRuntime(scenario), store);
        await viewModel.InitializeAsync();
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == (includeGold ? "gold-smelter" : "iron-smelter"));
        Assert.Contains(includeGold ? "Miniera d'oro, Miniera di carbone" : "Miniera di ferro, Miniera di carbone", viewModel.Selected.Diagnosis);
        Assert.False(viewModel.ConstructCommand.CanExecute(null));
        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = width, Height = height };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var diagnosis = window.FindControl<TextBlock>("ProductionDiagnosisText")!;
            var build = window.FindControl<Button>("BuildButton")!;
            Assert.True(diagnosis.Bounds.Height > 0);
            Assert.True(diagnosis.TranslatePoint(default, window)!.Value.Y + diagnosis.Bounds.Height <=
                build.TranslatePoint(default, window)!.Value.Y);
            foreach (var id in includeGold ? new[] { "iron-mine", "coal-mine", "iron-smelter", "gold-mine", "gold-smelter" }
                : new[] { "iron-mine", "coal-mine", "iron-smelter" })
            {
                viewModel.Selected = viewModel.Buildings.Single(item => item.Id == id);
                Assert.True(viewModel.ConstructCommand.CanExecute(null));
                viewModel.ConstructCommand.Execute(null);
                viewModel.AssignCommand.Execute(null);
                Assert.Equal(1, viewModel.Selected.Count);
                Assert.Equal(1, viewModel.Selected.Workers);
            }
            viewModel.TogglePauseCommand.Execute(null);
            viewModel.Advance(TimeSpan.FromSeconds(2));
            viewModel.Pause();
            Assert.Equal(1m, viewModel.View.Resources.Single(item => item.Id == "iron").Quantity);
            if (includeGold) Assert.Equal(1m, viewModel.View.Resources.Single(item => item.Id == "gold").Quantity);
            Assert.Equal("Pronto a produrre", viewModel.Selected.Status);
            window.FindControl<ListBox>("BuildingsList")!.ScrollIntoView(viewModel.Selected);
            window.FindControl<TabControl>("Sections")!.SelectedIndex = section;
            Dispatcher.UIThread.RunJobs();
            foreach (var name in section == 0 ? new[] { "PauseButton", "SaveButton", "BuildButton", "AssignButton" } : new[] { "PauseButton", "SaveButton" })
            {
                var button = window.FindControl<Button>(name)!;
                Assert.True(button.IsEffectivelyVisible);
                var point = button.TranslatePoint(default, window)!.Value;
                Assert.InRange(point.Y, 0, window.ClientSize.Height - button.Bounds.Height);
            }
            Assert.Equal(16, viewModel.Resources.Count);
            if (section == 1)
            {
                var scroll = ResourceScroll(window);
                Assert.InRange(scroll.TranslatePoint(default, window)!.Value.Y + scroll.Bounds.Height, 0, window.ClientSize.Height);
            }
            using var image = window.CaptureRenderedFrame();
            Assert.NotNull(image);
            var artifacts = Environment.GetEnvironmentVariable("SIMRISORSE_DESKTOP_ARTIFACTS");
            if (!string.IsNullOrWhiteSpace(artifacts))
            {
                Directory.CreateDirectory(artifacts);
                image.Save(Path.Combine(artifacts, file), PngBitmapEncoderOptions.Default);
            }
        }
        finally { window.CloseWithoutSaving(); }
    }

    [Fact]
    public void OriginalRetroGlyphsCoverTheEntireCivilCatalog()
    {
        var catalog = CivilGrowthScenario.Create().Catalog;
        Assert.All(catalog.Buildings, building => Assert.True(PixelGlyph.Supports(building.Id),
            $"Missing pixel pictogram for building {building.Id}."));
        Assert.All(catalog.Resources, resource => Assert.True(PixelGlyph.Supports(resource.Id),
            $"Missing pixel pictogram for resource {resource.Id}."));
        Assert.False(PixelGlyph.Supports("not-a-catalog-key"));
    }

    [Fact]
    public async Task ForgeAndSwordsAppearWithDescriptionStorageAndRetroPictograms()
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        Assert.Contains("Forgia", viewModel.Buildings.Select(item => item.Name));
        var forge = viewModel.Buildings.Single(item => item.Id == "forge");
        Assert.Equal(240m, forge.View.Cost.Single(item => item.ResourceId == "planks").Quantity);
        Assert.Contains("prima alle Fonderie", forge.Description);
        Assert.Contains("Fonderia di ferro", forge.Diagnosis);
        Assert.True(PixelGlyph.Supports("forge"));
        Assert.True(PixelGlyph.Supports("swords"));

        viewModel.SelectedResource = viewModel.Resources.Single(item => item.Id == "swords");
        Assert.Contains("Spade: 0 / 1.000", viewModel.SelectedResourceStorageDetails);
        Assert.Contains("1 Municipio × 1.000", viewModel.SelectedResourceStorageDetails);
        Assert.Contains("Forgia richiede almeno 2 posti liberi", viewModel.SelectedResourceStorageDetails);
    }

    [Fact]
    public async Task MintAndCoinsShowCorrectInputsStorageBatchAndRetroPictograms()
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        var mint = viewModel.Buildings.Single(item => item.Id == "mint");
        Assert.Equal("Zecca", mint.Name);
        Assert.Equal(210m, mint.View.Cost.Single(item => item.ResourceId == "planks").Quantity);
        Assert.Equal(160m, mint.View.Cost.Single(item => item.ResourceId == "stone").Quantity);
        Assert.Contains("pagamenti specifici", mint.Description);
        Assert.Contains("Fonderia d'oro", mint.Diagnosis);
        Assert.Contains(mint.View.InputPerWorker, item => item.ResourceId == "coal" && item.Quantity == 1m);
        Assert.Contains(mint.View.InputPerWorker, item => item.ResourceId == "gold" && item.Quantity == 1m);
        Assert.Contains(mint.View.OutputPerWorker, item => item.ResourceId == "coins" && item.Quantity == 3m);
        Assert.True(PixelGlyph.Supports("mint"));
        Assert.True(PixelGlyph.Supports("coins"));

        viewModel.SelectedResource = viewModel.Resources.Single(item => item.Id == "coins");
        Assert.Contains("Monete: 0 / 1.000", viewModel.SelectedResourceStorageDetails);
        Assert.Contains("1 Municipio × 1.000", viewModel.SelectedResourceStorageDetails);
        Assert.Contains("Zecca richiede almeno 3 posti liberi", viewModel.SelectedResourceStorageDetails);
    }

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public async Task RetroGlyphPrototypeKeepsResourcesAndBuildingsVisibleInBothLayouts(int width, int height)
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        Assert.Contains("15%", viewModel.Buildings.Single(item => item.Id == "small-house").Description);
        Assert.Contains("30%", viewModel.Buildings.Single(item => item.Id == "large-house").Description);
        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = width, Height = height };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var glyphs = window.GetVisualDescendants().OfType<PixelGlyph>()
                .Where(glyph => glyph.IsEffectivelyVisible).ToArray();
            Assert.Contains(glyphs, glyph => glyph.Key == "small-house" && glyph.Bounds.Width >= 24);
            Assert.Contains(glyphs, glyph => glyph.Key == "food" && glyph.Bounds.Height == 16);
            Assert.All(glyphs, glyph => Assert.True(glyph.Bounds.Width > 0 && glyph.Bounds.Height > 0));

            // The experimental visuals never take away the fixed building diagnosis.
            viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "bakery");
            Dispatcher.UIThread.RunJobs();
            var diagnosis = window.FindControl<TextBlock>("ProductionDiagnosisText")!;
            var build = window.FindControl<Button>("BuildButton")!;
            Assert.Contains("Per costruire servono prima:", diagnosis.Text!);
            Assert.True(diagnosis.TranslatePoint(default, window)!.Value.Y + diagnosis.Bounds.Height <=
                build.TranslatePoint(default, window)!.Value.Y);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public async Task AcceleratedGrowthStatusShowsSpeedFractionAndAccurateTimeWithoutLayoutSideEffects()
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "small-house");
        viewModel.ConstructCommand.Execute(null);
        Assert.Contains("×1,15", viewModel.GrowthStatus);
        Assert.Contains("~53s", viewModel.GrowthStatus);
        viewModel.TogglePauseCommand.Execute(null);
        viewModel.Advance(TimeSpan.FromSeconds(52));
        Assert.Equal(8, viewModel.View.Population);
        Assert.Contains("59,8 / 60", viewModel.GrowthStatus);
        Assert.InRange(viewModel.GrowthPercent, 99, 100);
        viewModel.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(9, viewModel.View.Population);
        Assert.Contains("0,95 / 60", viewModel.GrowthStatus);
    }

    [Fact]
    public void ScenariosHaveDistinctPersonalSlotsAndSmokeTestsUsePrivatePaths()
    {
        var current = DesktopOptions.Parse([]);
        var legacy = DesktopOptions.Parse(["--legacy-v13"]);
        var historical = DesktopOptions.Parse(["--scenario", "economy-foundation"]);
        Assert.Equal("civil-growth", current.Scenario.Id);
        Assert.True(current.UseV15);
        Assert.False(current.UseV14);
        Assert.EndsWith("civil-growth-v15.sqlite3", current.SavePath);
        Assert.EndsWith("civil-growth.sqlite3", legacy.SavePath);
        Assert.EndsWith("game.sqlite3", historical.SavePath);
        Assert.NotEqual(current.SavePath, historical.SavePath);
        Assert.NotEqual(current.SavePath, legacy.SavePath);
        var smoke = DesktopOptions.Parse(["--smoke-test"]);
        Assert.False(smoke.UseV14);
        Assert.NotEqual(current.SavePath, smoke.SavePath);
        Assert.NotEqual(historical.SavePath, smoke.SavePath);
        Assert.Contains("SimRisorse.NativeSmoke", smoke.SavePath);
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--scenario", "unknown"]));
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--save-path", "--smoke-test"]));
    }

    [AvaloniaFact]
    public async Task HousesGrowthAndFoodConsumptionRemainVisibleAndResumeExactly()
    {
        using var directory = new TemporaryDirectory();
        var store = new SqliteGameSaveStore(Path.Combine(directory.PathName, "civil.sqlite3"));
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.CreateLegacyForBenchmarks()), store);
        await viewModel.InitializeAsync();
        var window = new MainWindow(viewModel, startTimer: false, initialize: false);
        try
        {
            window.Show();
            Assert.Contains("8 / 8", viewModel.Population);
            Assert.Contains("Case piene", window.FindControl<TextBlock>("GrowthStatusText")!.Text!);
            viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "small-house");
            Assert.Contains("posti abitativi", viewModel.Selected.Recipe);
            Assert.Contains("posti abitativi", viewModel.Selected.Status);
            Assert.False(window.FindControl<Button>("AssignButton")!.IsEffectivelyEnabled);
            Click(window, "BuildButton");
            Assert.Contains("8 / 16", viewModel.Population);
            Click(window, "PauseButton");
            viewModel.Advance(TimeSpan.FromMilliseconds(59500));
            Assert.Contains("59 / 60", viewModel.GrowthStatus);
            Assert.True(await viewModel.SaveAsync());
            viewModel.NewGameCommand.Execute(null);
            viewModel.ConfirmNewGameCommand.Execute(null);
            Assert.Equal("civil-growth", viewModel.View.ScenarioId);
            Assert.Contains("8 / 8", viewModel.Population);
            Assert.True(await viewModel.LoadAsync());
            Assert.Contains("59 / 60", viewModel.GrowthStatus);
            Assert.True(viewModel.IsPaused);
            Click(window, "PauseButton");
            viewModel.Advance(TimeSpan.FromMilliseconds(500));
            Assert.Equal(9, viewModel.View.Population);
            Assert.Equal(0.09m, viewModel.View.Resources.Single(item => item.Id == "food").ConsumptionPerSecond);
            Assert.Equal(25.2m, viewModel.View.Resources.Single(item => item.Id == "food").Quantity);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public async Task PreviousCivilSaveIsConvertedExplainedAndKeptAsBackup()
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.PathName, "civil-growth.sqlite3");
        var current = GameSession.Create(CivilGrowthScenario.Create()).Snapshot();
        new SqliteGameSaveStore(path).Save(current with
        {
            RulesVersion = "civil-growth-1",
            Military = null,
            Buildings = [.. current.Buildings.Where(item => !CivilGrowthScenario.Changes.Any(change => change.AddedBuildings.Contains(item.BuildingId)))]
        });
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new SqliteGameSaveStore(path));
        await viewModel.InitializeAsync();
        Assert.Contains($"civil-growth-1 a {CivilGrowthScenario.RulesVersion}", viewModel.Message);
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "warehouse");
        Assert.Equal("+500 capacità per ogni risorsa per edificio", viewModel.Selected.Recipe);
        Assert.Equal("Da costruire", viewModel.Selected.Status);
        Assert.False(viewModel.Selected.IsProducer);
        Assert.False(viewModel.AssignCommand.CanExecute(null));
        Assert.True(await viewModel.SaveAsync());
        Assert.True(File.Exists(Path.Combine(directory.PathName, "civil-growth.civil-growth-1.backup.sqlite3")));
    }

    [AvaloniaFact]
    public async Task WarehouseRaisesEveryResourceCapacityByTheSameAmount()
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        var previous = viewModel.View.Resources.ToDictionary(item => item.Id, item => item.Capacity);
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "warehouse");
        viewModel.ConstructCommand.Execute(null);
        Assert.Equal("+500 capacità per ogni risorsa", viewModel.Selected.Status);
        Assert.All(viewModel.View.Resources, item => Assert.Equal(previous[item.Id] + 500m, item.Capacity));
        Assert.Equal("1.550", viewModel.Resources.Single(item => item.Id == "planks").Capacity);
        Assert.Equal("1.510", viewModel.Resources.Single(item => item.Id == "bread").Capacity);
        Assert.Contains("Nessun lavoratore", viewModel.Selected.Activity);
    }

    [AvaloniaFact]
    public async Task BreadChainShowsRecipesAndBlocksTheBakeryUntilItsSuppliersExist()
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "bakery");
        Assert.Equal("1 farina · 1 acqua → 3 pane", viewModel.Selected.Recipe);
        Assert.Equal(new[] { "Mulino", "Pozzo" }, viewModel.Selected.View.MissingPrerequisites);
        Assert.Contains("Mulino, Pozzo", viewModel.Selected.Diagnosis);
        Assert.False(viewModel.ConstructCommand.CanExecute(null));
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "mill");
        Assert.Equal("2 grano → 1 farina", viewModel.Selected.Recipe);
        Assert.Equal(viewModel.Resources.Count - 5, viewModel.Resources.Count(item => item.Capacity == "1.000"));
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "large-house");
        Assert.Equal(new[] { "Forno" }, viewModel.Selected.View.MissingPrerequisites);
        Assert.Equal("+32 posti abitativi per edificio", viewModel.Selected.Recipe);
        Assert.False(viewModel.ConstructCommand.CanExecute(null));
    }

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public async Task MissingConstructionPrerequisitesStayVisibleAboveTheBuildAction(int width, int height)
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "bakery");
        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = width, Height = height };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var diagnosis = window.FindControl<TextBlock>("ProductionDiagnosisText")!;
            var panel = window.FindControl<Border>("BuildingDiagnosisPanel")!;
            var build = window.FindControl<Button>("BuildButton")!;
            Assert.True(panel.IsEffectivelyVisible);
            Assert.Contains("Per costruire servono prima: Mulino, Pozzo", diagnosis.Text);
            Assert.False(build.IsEffectivelyEnabled);
            var position = diagnosis.TranslatePoint(default, window)!.Value;
            var buildPosition = build.TranslatePoint(default, window)!.Value;
            Assert.True(diagnosis.Bounds.Height > 0, "The prerequisites must receive visible space.");
            Assert.True(position.Y + diagnosis.Bounds.Height <= buildPosition.Y,
                "The complete prerequisites must be visible above the action buttons without scrolling.");
            Assert.InRange(position.Y, 0, window.ClientSize.Height - diagnosis.Bounds.Height);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public async Task BuildHoverAndDisabledStatesUseDistinctBackgroundsWithoutMovingTheLayout(int width, int height)
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "small-house");
        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = width, Height = height };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var build = window.FindControl<Button>("BuildButton")!;
            var batch = window.FindControl<ComboBox>("BatchSelector")!;
            var cost = window.FindControl<TextBlock>("ConstructionCostText")!;
            Control[] stableControls = [batch, cost, build];
            var originalGeometry = stableControls.Select(control =>
                (Position: control.TranslatePoint(default, window)!.Value, Size: control.Bounds.Size)).ToArray();
            Assert.True(build.IsEffectivelyEnabled);
            Assert.Equal(Color.Parse("#176858"), Assert.IsAssignableFrom<ISolidColorBrush>(build.Background).Color);
            var pointer = build.TranslatePoint(new Point(build.Bounds.Width / 2, build.Bounds.Height / 2), window)!.Value;
            window.MouseMove(pointer);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(Color.Parse("#E6D5AC"), Assert.IsAssignableFrom<ISolidColorBrush>(build.Background).Color);
            var presenter = build.GetVisualDescendants().OfType<Avalonia.Controls.Presenters.ContentPresenter>()
                .Single(item => item.Name == "PART_ContentPresenter");
            Assert.Equal(Color.Parse("#E6D5AC"), Assert.IsAssignableFrom<ISolidColorBrush>(presenter.Background).Color);
            Assert.Equal(Color.Parse("#263349"), Assert.IsAssignableFrom<ISolidColorBrush>(presenter.Foreground).Color);
            Assert.Equal(originalGeometry, stableControls.Select(control =>
                (Position: control.TranslatePoint(default, window)!.Value, Size: control.Bounds.Size)).ToArray());

            // Il puntatore resta sul comando mentre l'acquisto esaurisce le risorse.
            // In questo stato il bottone disabilitato non deve mantenere l'hover ocra.
            viewModel.ConstructCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.False(viewModel.ConstructCommand.CanExecute(null));
            Assert.False(build.IsEffectivelyEnabled);
            Assert.Equal(Color.Parse("#B8C2BC"), Assert.IsAssignableFrom<ISolidColorBrush>(build.Background).Color);

            var save = window.FindControl<Button>("SaveButton")!;
            window.MouseMove(save.TranslatePoint(new Point(save.Bounds.Width / 2, save.Bounds.Height / 2), window)!.Value);
            Dispatcher.UIThread.RunJobs();
            var savePresenter = save.GetVisualDescendants().OfType<Avalonia.Controls.Presenters.ContentPresenter>()
                .Single(item => item.Name == "PART_ContentPresenter");
            Assert.Equal(Color.Parse("#E6D5AC"), Assert.IsAssignableFrom<ISolidColorBrush>(savePresenter.Background).Color);
            Assert.Equal(originalGeometry, stableControls.Select(control =>
                (Position: control.TranslatePoint(default, window)!.Value, Size: control.Bounds.Size)).ToArray());

            // Anche un prerequisito mancante usa lo stato disabilitato esplicito.
            viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "bakery");
            Dispatcher.UIThread.RunJobs();
            Assert.False(build.IsEffectivelyEnabled);
            Assert.Equal(Color.Parse("#B8C2BC"), Assert.IsAssignableFrom<ISolidColorBrush>(build.Background).Color);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public async Task AdvancedHousingShowsBreadRationAndResidentCountsWithoutChangingTheSavedPopulation()
    {
        var scenario = CivilGrowthScenario.CreateLegacyForBenchmarks();
        var snapshot = GameSession.Create(scenario).Snapshot();
        var store = new MemoryStore
        {
            Snapshot = snapshot with
            {
                Population = 17,
                Resources = SimRisorse.Core.Economy.ResourceBundle.Create(("food", 100m), ("bread", 20m)),
                Buildings = [.. snapshot.Buildings.Select(item => item.BuildingId switch
                {
                    "large-house" => item with { Count = 1 },
                    "bakery" => item with { Count = 1 },
                    _ => item
                })]
            }
        };
        var viewModel = new MainViewModel(new GameRuntime(scenario), store);
        await viewModel.InitializeAsync();
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "large-house");
        Assert.Equal(17, viewModel.View.Population);
        Assert.Equal(9, viewModel.View.PopulationState.AdvancedResidents);
        Assert.Equal(0.135m, viewModel.View.PopulationState.RequiredBreadPerSecond);
        Assert.Equal(40, viewModel.View.PopulationState.HousingCapacity);
        Assert.True(viewModel.HasAdvancedResidents);
        Assert.Equal(BreadSupplyState.NominalCapacityInsufficient, viewModel.View.BreadSupply.State);
        Assert.Contains("Capacità nominale dei Forni insufficiente", viewModel.BreadSupplyStatus);
        Assert.Contains("−0,135 Pane/s", viewModel.BreadSupplyStatus);
        Assert.Contains("+1 addetto", viewModel.BreadSupplyStatus);
        Assert.Contains("2 min 28 s", viewModel.BreadSupplyStatus);

        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = 1040, Height = 740 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var ration = window.FindControl<TextBlock>("BreadRequirementText")!;
            Assert.True(ration.IsEffectivelyVisible);
            Assert.Contains("pane/s", ration.Text!);
            Assert.Contains("residenti avanzati", ration.Text!);
            var supply = window.FindControl<TextBlock>("BreadSupplyStatusText")!;
            Assert.True(supply.IsEffectivelyVisible);
            Assert.Contains("2 min 28 s", supply.Text!);
            Assert.Contains("−0,135 Pane/s", supply.Text!);
            var note = window.FindControl<TextBlock>("GrowthPauseNote")!;
            var supplyBottom = supply.TranslatePoint(default, window)!.Value.Y + supply.Bounds.Height;
            var noteTop = note.TranslatePoint(default, window)!.Value.Y;
            Assert.True(supplyBottom <= noteTop, "Quantified bread deficit overlaps the pause explanation.");
            Assert.InRange(noteTop + note.Bounds.Height, 0, window.ClientSize.Height);
            Assert.Contains("+32 posti abitativi", viewModel.Selected.Recipe);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public async Task FullBreadStockIsExplainedAsAStorageLimitNotAsInsufficientOvenCapacity()
    {
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot();
        var store = new MemoryStore
        {
            Snapshot = initial with
            {
                Population = 9,
                Resources = SimRisorse.Core.Economy.ResourceBundle.Create(("food", 100m), ("bread", 1085m),
                    ("flour", 1m), ("water", 1m)),
                Buildings = [.. initial.Buildings.Select(item => item.BuildingId switch
                {
                    "large-house" => item with { Count = 1 },
                    "farm" => item with { Count = 2 },
                    "well" or "mill" => item with { Count = 1 },
                    "bakery" => item with { Count = 1, AssignedWorkers = 1 },
                    _ => item
                })]
            }
        };
        var viewModel = new MainViewModel(new GameRuntime(scenario), store);
        await viewModel.InitializeAsync();
        Assert.Equal(0m, viewModel.View.Resources.Single(item => item.Id == "bread").ProductionPerSecond);
        Assert.Equal(BreadSupplyState.StorageFull, viewModel.View.BreadSupply.State);
        Assert.Contains("Forno fermo: non c'è spazio per un lotto di Pane", viewModel.BreadSupplyStatus);
        Assert.Contains("Costruisci un Magazzino", viewModel.BreadSupplyStatus);
        Assert.DoesNotContain("Capacità nominale dei Forni insufficiente", viewModel.BreadSupplyStatus);

        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = 1040, Height = 740 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var supply = window.FindControl<TextBlock>("BreadSupplyStatusText")!;
            Assert.True(supply.IsEffectivelyVisible);
            Assert.Contains("Costruisci un Magazzino", supply.Text!);
            var note = window.FindControl<TextBlock>("GrowthPauseNote")!;
            var supplyBottom = supply.TranslatePoint(default, window)!.Value.Y + supply.Bounds.Height;
            var noteTop = note.TranslatePoint(default, window)!.Value.Y;
            Assert.True(supplyBottom <= noteTop, "Bread status overlaps the growth pause explanation.");
            Assert.InRange(noteTop + note.Bounds.Height, 0, window.ClientSize.Height);
            using var image = window.CaptureRenderedFrame();
            Assert.NotNull(image);
            var artifacts = Environment.GetEnvironmentVariable("SIMRISORSE_DESKTOP_ARTIFACTS");
            if (!string.IsNullOrWhiteSpace(artifacts))
            {
                Directory.CreateDirectory(artifacts);
                image.Save(Path.Combine(artifacts, "civil-advanced-bread-compact.png"), PngBitmapEncoderOptions.Default);
            }
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public async Task FirstAdvancedArrivalExplainsMissingBreadAndUsesNewProductionOnlyNextSecond()
    {
        var scenario = CivilGrowthScenario.CreateLegacyForBenchmarks();
        var initial = GameSession.Create(scenario).Snapshot();
        var store = new MemoryStore
        {
            Snapshot = initial with
            {
                Population = 16,
                GrowthProgressSeconds = 59,
                Resources = SimRisorse.Core.Economy.ResourceBundle.Create(("food", 30m), ("flour", 1m), ("water", 1m)),
                Buildings = [.. initial.Buildings.Select(item => item.BuildingId switch
                {
                    "small-house" => item with { Count = 2 },
                    "large-house" => item with { Count = 1 },
                    "farm" => item with { Count = 2 },
                    "well" or "mill" => item with { Count = 1 },
                    "bakery" => item with { Count = 1, AssignedWorkers = 1 },
                    _ => item
                })]
            }
        };
        var viewModel = new MainViewModel(new GameRuntime(scenario), store);
        await viewModel.InitializeAsync();
        Assert.False(viewModel.HasAdvancedResidents);
        Assert.True(viewModel.ShowBreadSupplyStatus);
        Assert.Equal(BreadSupplyState.FirstAdvancedArrivalNeedsStock, viewModel.View.BreadSupply.State);
        Assert.Contains("Manca pane", viewModel.GrowthStatus);
        Assert.Contains("primo residente avanzato", viewModel.BreadSupplyStatus);

        var window = new MainWindow(viewModel, startTimer: false, initialize: false);
        try
        {
            window.Show();
            var supply = window.FindControl<TextBlock>("BreadSupplyStatusText")!;
            Assert.True(supply.IsEffectivelyVisible);
            Assert.Contains("primo residente avanzato", supply.Text!);

            viewModel.TogglePauseCommand.Execute(null);
            viewModel.Advance(TimeSpan.FromSeconds(1));
            Assert.Equal(16, viewModel.View.Population);
            Assert.Equal(59, viewModel.View.PopulationState.ProgressSeconds);
            Assert.Equal(3m, viewModel.View.Resources.Single(item => item.Id == "bread").Quantity);
            Assert.Equal(29.84m, viewModel.View.Resources.Single(item => item.Id == "food").Quantity);
            Assert.False(viewModel.ShowBreadSupplyStatus);

            viewModel.Advance(TimeSpan.FromSeconds(1));
            Assert.Equal(17, viewModel.View.Population);
            Assert.Equal(0, viewModel.View.PopulationState.ProgressSeconds);
            Assert.Equal(3m, viewModel.View.Resources.Single(item => item.Id == "bread").Quantity);
            Assert.True(viewModel.ShowBreadSupplyStatus);
            Assert.Contains("input insufficienti", viewModel.BreadSupplyStatus);

            viewModel.Advance(TimeSpan.FromSeconds(1));
            Assert.Equal(17, viewModel.View.Population);
            Assert.Equal(2.985m, viewModel.View.Resources.Single(item => item.Id == "bread").Quantity);
            Assert.Equal(29.51m, viewModel.View.Resources.Single(item => item.Id == "food").Quantity);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public async Task SelectedStructureExplainsTheSpecificResourceBlockingProduction()
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "fisher");
        viewModel.ConstructCommand.Execute(null);
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "food-store");
        viewModel.ConstructCommand.Execute(null);
        Assert.Contains("Assegna", viewModel.Selected.Diagnosis);
        viewModel.AssignCommand.Execute(null);

        var window = new MainWindow(viewModel, startTimer: false, initialize: false);
        try
        {
            window.Show();
            Assert.Contains("Pesce", viewModel.Selected.Diagnosis);
            var detail = window.FindControl<TextBlock>("ProductionDiagnosisText")!;
            Assert.True(detail.IsEffectivelyVisible);
            Assert.Contains("Pesce", detail.Text!);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public async Task FoodShortageIsExplainedWithoutRemovingWorkers()
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        viewModel.TogglePauseCommand.Execute(null);
        viewModel.Advance(TimeSpan.FromSeconds(376));
        Assert.Equal(8, viewModel.View.Population);
        Assert.Equal(5, viewModel.View.AvailableWorkers);
        Assert.Contains("Manca cibo", viewModel.GrowthStatus);
    }

    [AvaloniaTheory]
    [InlineData(1240, 850, 0, "civil-buildings.png")]
    [InlineData(1040, 740, 0, "civil-buildings-compact.png")]
    [InlineData(1240, 850, 1, "civil-resources.png")]
    [InlineData(1040, 740, 1, "civil-resources-compact.png")]
    public async Task CivilScreensRenderHousingAndFoodWithPrimaryControlsVisible(int width, int height, int section, string file)
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "small-house");
        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = width, Height = height };
        try
        {
            window.Show();
            window.FindControl<TabControl>("Sections")!.SelectedIndex = section;
            Dispatcher.UIThread.RunJobs();
            foreach (var name in section == 0 ? new[] { "BuildButton", "AssignButton", "PauseButton", "SaveButton" } : new[] { "PauseButton", "SaveButton" })
            {
                var button = window.FindControl<Button>(name)!;
                var point = button.TranslatePoint(default, window)!.Value;
                Assert.InRange(point.Y, 0, window.ClientSize.Height - button.Bounds.Height);
            }
            Assert.True(window.FindControl<TextBlock>("GrowthStatusText")!.IsEffectivelyVisible);
            if (section == 1)
            {
                // Eight resources no longer fit at the compact size: the table must scroll inside the window.
                var scroll = ResourceScroll(window);
                Assert.InRange(scroll.TranslatePoint(default, window)!.Value.Y + scroll.Bounds.Height, 0, window.ClientSize.Height);
            }
            var growthBar = window.FindControl<ProgressBar>("GrowthProgressBar")!;
            var growthPosition = growthBar.TranslatePoint(default, window)!.Value;
            Assert.InRange(growthPosition.X, 22, window.FindControl<Border>("SidebarPanel")!.Bounds.Width - growthBar.Bounds.Width);
            using var image = window.CaptureRenderedFrame();
            Assert.NotNull(image);
            var artifacts = Environment.GetEnvironmentVariable("SIMRISORSE_DESKTOP_ARTIFACTS");
            if (!string.IsNullOrWhiteSpace(artifacts))
            {
                Directory.CreateDirectory(artifacts);
                image.Save(Path.Combine(artifacts, file), PngBitmapEncoderOptions.Default);
            }
        }
        finally { window.CloseWithoutSaving(); }
    }

    [Fact]
    public void DesktopDoesNotCallTheSQLiteDriverDirectly() => Assert.DoesNotContain(
        typeof(MainViewModel).Assembly.GetReferencedAssemblies(), reference => reference.Name!.StartsWith("Microsoft.Data.Sqlite", StringComparison.Ordinal));

    [AvaloniaFact]
    public async Task SavingKeepsTheClockAndPauseControlResponsive()
    {
        using var store = new BlockingStore();
        var viewModel = Create(store);
        viewModel.TogglePauseCommand.Execute(null);
        var save = viewModel.SaveAsync();
        try
        {
            await store.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            viewModel.Advance(TimeSpan.FromSeconds(1));
            Assert.Equal("00:00:01", viewModel.Clock);
            Assert.True(viewModel.TogglePauseCommand.CanExecute(null));
            viewModel.TogglePauseCommand.Execute(null);
            Assert.True(viewModel.IsPaused);
        }
        finally { store.Release.Set(); await save; }
        Assert.False(viewModel.IsBusy);
    }

    [AvaloniaFact]
    public void BulkOrdersUseTheDefinitiveRuntimeAndRespectPopulation()
    {
        var viewModel = Create(new MemoryStore());
        var window = new MainWindow(viewModel, startTimer: false, initialize: false);
        try
        {
            window.Show();
            window.FindControl<ComboBox>("BatchSelector")!.SelectedItem = 5;
            Click(window, "BuildButton");
            Assert.Equal(6, viewModel.Selected.Count);
            Click(window, "AssignButton");
            Assert.Equal(6, viewModel.Selected.Workers);
            Assert.Equal(0, viewModel.View.AvailableWorkers);
            Assert.False(window.FindControl<Button>("AssignButton")!.IsEffectivelyEnabled);
            Click(window, "PauseButton");
            viewModel.Advance(TimeSpan.FromSeconds(1));
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("00:00:01", window.FindControl<TextBlock>("ClockText")!.Text);
            Assert.Equal(12m, viewModel.View.Resources.Single(item => item.Id == "logs").Quantity);
            Assert.Equal(20m, viewModel.View.Resources.Single(item => item.Id == "planks").Quantity);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public void ResourceTabShowsExplicitRatesFromTheForecast()
    {
        var viewModel = Create(new MemoryStore());
        var window = new MainWindow(viewModel, startTimer: false, initialize: false);
        try
        {
            window.Show();
            window.FindControl<TabControl>("Sections")!.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.True(window.FindControl<ItemsControl>("ResourcesTable")!.IsEffectivelyVisible);
            var header = window.FindControl<Grid>("ResourceTableHeader")!;
            var row = window.FindControl<ItemsControl>("ResourcesTable")!.GetVisualDescendants()
                .OfType<Grid>().First(item => item.ColumnDefinitions.Count == 7 &&
                    item.GetVisualDescendants().OfType<PixelGlyph>().Any());
            Assert.Equal(7, header.ColumnDefinitions.Count);
            foreach (var grid in new[] { header, row })
            {
                var numericCells = grid.Children.OfType<TextBlock>().Where(item => Grid.GetColumn(item) > 0).ToArray();
                Assert.Equal(6, numericCells.Length);
                Assert.All(numericCells, cell =>
                {
                    Assert.Equal(Avalonia.Layout.HorizontalAlignment.Right, cell.HorizontalAlignment);
                    Assert.Equal(TextAlignment.Right, cell.TextAlignment);
                });
            }
            Assert.Contains(row.Children.OfType<TextBlock>(), cell => Grid.GetColumn(cell) == 6 &&
                cell.Text == viewModel.Resources.First().TimeToFull);
            Assert.Contains(row.Children.OfType<TextBlock>(), cell => Grid.GetColumn(cell) == 1 &&
                cell.Text == viewModel.Resources.First().QuantityRounded);
            Assert.Equal("+2 /s", viewModel.Resources.Single(item => item.Id == "logs").NetRate);
            Assert.Equal("±0 /s", viewModel.Resources.Single(item => item.Id == "planks").NetRate);
            viewModel.TogglePauseCommand.Execute(null);
            viewModel.Advance(TimeSpan.FromSeconds(1));
            Assert.Equal("±0 /s", viewModel.Resources.Single(item => item.Id == "logs").NetRate);
            Assert.Equal("+1 /s", viewModel.Resources.Single(item => item.Id == "planks").NetRate);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public async Task ResourcesCanBeSelectedInOchreAndBothListsKeepCellsAwayFromScrollbars(int width, int height)
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = width, Height = height };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var buildings = window.FindControl<ListBox>("BuildingsList")!;
            Assert.Same(viewModel.Selected, buildings.SelectedItem);
            AssertListGutter(window, buildings);

            window.FindControl<TabControl>("Sections")!.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            var resources = window.FindControl<ListBox>("ResourcesTable")!;
            var firstRow = viewModel.Resources.First();
            var item = resources.GetVisualDescendants().OfType<ListBoxItem>()
                .First(candidate => ReferenceEquals(candidate.DataContext, firstRow));

            var point = item.TranslatePoint(new Point(item.Bounds.Width / 2, item.Bounds.Height / 2), window)!.Value;
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.Same(firstRow, viewModel.SelectedResource);
            Assert.Same(firstRow, resources.SelectedItem);
            Assert.True(item.IsSelected);
            var ochrePresenter = item.GetVisualDescendants().OfType<Avalonia.Controls.Presenters.ContentPresenter>()
                .FirstOrDefault(control => control.Background is ISolidColorBrush brush &&
                    brush.Color == Color.Parse("#E6D5AC"));
            Assert.NotNull(ochrePresenter);
            AssertListGutter(window, resources);

            viewModel.TogglePauseCommand.Execute(null);
            viewModel.Advance(TimeSpan.FromSeconds(2));
            Assert.Same(firstRow, viewModel.SelectedResource);
            Assert.True(item.IsSelected);
            Assert.Contains(firstRow.Name, firstRow.CapacityBreakdown);
            viewModel.SelectedResource = viewModel.Resources.Single(row => row.Id == "bread");
            Dispatcher.UIThread.RunJobs();
            Assert.Contains("Pane:", viewModel.SelectedResource!.CapacityBreakdown);
            Assert.Contains("Magazzini", viewModel.SelectedResource!.CapacityBreakdown);
            var selectedItem = resources.GetVisualDescendants().OfType<ListBoxItem>()
                .First(candidate => ReferenceEquals(candidate.DataContext, viewModel.SelectedResource));
            var capacityCell = selectedItem.GetVisualDescendants().OfType<Grid>()
                .SelectMany(grid => grid.Children.OfType<TextBlock>())
                .First(cell => Grid.GetColumn(cell) == 2);
            Assert.Equal(viewModel.SelectedResource!.CapacityBreakdown, ToolTip.GetTip(capacityCell));
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public void NewGameRequiresConfirmationAndCancelPreservesTheSession()
    {
        var viewModel = Create(new MemoryStore());
        viewModel.ConstructCommand.Execute(null);
        viewModel.NewGameCommand.Execute(null);
        Assert.True(viewModel.IsNewGameConfirmationVisible);
        Assert.Equal(2, viewModel.Selected.Count);
        viewModel.CancelNewGameCommand.Execute(null);
        Assert.Equal(2, viewModel.Selected.Count);
        viewModel.NewGameCommand.Execute(null);
        viewModel.ConfirmNewGameCommand.Execute(null);
        Assert.Equal(1, viewModel.Selected.Count);
        Assert.True(viewModel.IsPaused);
    }

    [AvaloniaFact]
    public async Task ClosingSavesAndAnotherWindowRestoresTheExactPausedCheckpoint()
    {
        using var directory = new TemporaryDirectory();
        var store = new SqliteGameSaveStore(Path.Combine(directory.PathName, "game.sqlite3"));
        var first = Create(store);
        await first.InitializeAsync();
        var window = new MainWindow(first, startTimer: false, initialize: false);
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        window.Closed += (_, _) => closed.TrySetResult();
        try
        {
            window.Show();
            Click(window, "BuildButton");
            Click(window, "AssignButton");
            first.TogglePauseCommand.Execute(null);
            first.Advance(TimeSpan.FromMilliseconds(1750));
            first.Pause();
            var expected = first.View;
            window.Close();
            await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var restored = Create(store);
            await restored.InitializeAsync();
            Assert.True(restored.IsPaused);
            Assert.Equal(expected.ElapsedTicks, restored.View.ElapsedTicks);
            Assert.Equal(2, restored.Selected.Count);
            Assert.Equal(2, restored.Selected.Workers);
            restored.Advance(TimeSpan.FromHours(8));
            Assert.Equal(expected.ElapsedTicks, restored.View.ElapsedTicks);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public async Task FailedCloseKeepsTheWindowAndTheUnsavedStateAvailable()
    {
        var store = new MemoryStore { FailWrites = true };
        var viewModel = Create(store);
        await viewModel.InitializeAsync();
        var window = new MainWindow(viewModel, startTimer: false, initialize: false);
        try
        {
            window.Show();
            Click(window, "BuildButton");
            window.Close();
            await UntilAsync(() => viewModel.IsCloseFailureVisible);
            Assert.True(window.IsVisible);
            Assert.Equal(2, viewModel.Selected.Count);
            Assert.True(viewModel.IsPaused);
            Assert.Null(window.FindControl<Button>("DiscardCloseButton")); // No destructive-exit UI.
            store.FailWrites = false;
            Assert.True(await viewModel.SaveAsync());
            Assert.False(viewModel.IsCloseFailureVisible);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public async Task IncompatibleStartupDoesNotOverwriteTheExistingSaveOnClose()
    {
        var store = new MemoryStore { LoadError = GameSaveError.UnsupportedFormat };
        var viewModel = Create(store);
        await viewModel.InitializeAsync();
        Assert.False(await viewModel.AutoSaveAsync());
        Assert.True(await viewModel.PrepareCloseAsync());
        Assert.Equal(0, store.WriteCount);
    }

    [AvaloniaFact]
    public async Task PeriodicCheckpointPreservesRunningStateAndSkipsNewGameConfirmation()
    {
        var store = new MemoryStore();
        var viewModel = Create(store);
        await viewModel.InitializeAsync();
        viewModel.TogglePauseCommand.Execute(null);
        viewModel.Advance(TimeSpan.FromMilliseconds(500));
        Assert.True(await viewModel.AutoSaveAsync());
        Assert.False(viewModel.IsPaused);
        Assert.Equal(TimeSpan.FromMilliseconds(500).Ticks, store.Snapshot!.ElapsedTicks);
        viewModel.NewGameCommand.Execute(null);
        Assert.False(await viewModel.AutoSaveAsync());
        Assert.Equal(1, store.WriteCount);
    }

    [AvaloniaFact]
    public async Task ClockDisplaysTotalHoursAndPreciseResourceQuantities()
    {
        var state = GameSession.Create(EconomyFoundationScenario.Create()).Snapshot() with
        { ElapsedTicks = TimeSpan.FromHours(32).Ticks, Resources = SimRisorse.Core.Economy.ResourceBundle.Create(("logs", 1.234m)) };
        var viewModel = Create(new MemoryStore { Snapshot = state });
        await viewModel.InitializeAsync();
        Assert.Equal("32:00:00", viewModel.Clock);
        Assert.Equal("1,234", viewModel.Resources.Single(item => item.Id == "logs").Quantity);
    }

    [AvaloniaFact]
    public void MonotonicClockDiscardsSuspensionAndCountsOnlyActiveTime()
    {
        var viewModel = Create(new MemoryStore());
        var time = new ManualTime();
        var clock = new PlayClock(viewModel.Advance, viewModel.Interrupt, time);
        viewModel.SynchronizeTime = clock.Pulse;
        clock.Reset();
        viewModel.TogglePauseCommand.Execute(null);
        time.Advance(TimeSpan.FromSeconds(1)); clock.Pulse();
        Assert.Equal(TimeSpan.TicksPerSecond, viewModel.View.ElapsedTicks);
        time.Advance(TimeSpan.FromHours(8)); clock.Pulse();
        Assert.True(viewModel.IsPaused);
        Assert.Equal(TimeSpan.TicksPerSecond, viewModel.View.ElapsedTicks);
        viewModel.TogglePauseCommand.Execute(null);
        time.Advance(TimeSpan.FromMilliseconds(500)); clock.Pulse();
        Assert.Equal(TimeSpan.FromMilliseconds(1500).Ticks, viewModel.View.ElapsedTicks);
        clock.Suspend();
        Assert.True(viewModel.IsPaused);
    }

    [AvaloniaFact]
    public void MinimizingPausesTheSession()
    {
        var viewModel = Create(new MemoryStore());
        var window = new MainWindow(viewModel, startTimer: false, initialize: false);
        try
        {
            window.Show();
            viewModel.TogglePauseCommand.Execute(null);
            window.WindowState = WindowState.Minimized;
            Assert.True(viewModel.IsPaused);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaTheory]
    [InlineData(1240, 850, 0, "buildings.png")]
    [InlineData(1040, 740, 0, "buildings-compact.png")]
    [InlineData(1240, 850, 1, "resources.png")]
    [InlineData(1040, 740, 1, "resources-compact.png")]
    public async Task ScreensRenderWithVisiblePrimaryControls(int width, int height, int section, string file)
    {
        var viewModel = Create(new MemoryStore());
        await viewModel.InitializeAsync();
        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = width, Height = height };
        try
        {
            window.Show();
            window.FindControl<TabControl>("Sections")!.SelectedIndex = section;
            Dispatcher.UIThread.RunJobs();
            using var image = window.CaptureRenderedFrame();
            Assert.NotNull(image);
            Assert.True(image.PixelSize.Width >= width);
            foreach (var name in section == 0 ? new[] { "BuildButton", "AssignButton", "ReleaseButton", "PauseButton", "SaveButton", "LoadButton" } : new[] { "PauseButton", "SaveButton", "LoadButton" })
            {
                var button = window.FindControl<Button>(name)!;
                var position = button.TranslatePoint(default, window)!.Value;
                Assert.InRange(position.Y, 0, window.ClientSize.Height - button.Bounds.Height);
            }
            var artifacts = Environment.GetEnvironmentVariable("SIMRISORSE_DESKTOP_ARTIFACTS");
            if (!string.IsNullOrWhiteSpace(artifacts)) { Directory.CreateDirectory(artifacts); image.Save(Path.Combine(artifacts, file), PngBitmapEncoderOptions.Default); }
        }
        finally { window.CloseWithoutSaving(); }
    }

    private static MainViewModel Create(IGameSaveStore store) => new(new GameRuntime(EconomyFoundationScenario.Create()), store);
    private static ScrollViewer ResourceScroll(MainWindow window) => window.FindControl<ListBox>("ResourcesTable")!
        .GetVisualDescendants().OfType<ScrollViewer>().First();
    private static void AssertListGutter(MainWindow window, ListBox list)
    {
        var scroll = list.GetVisualDescendants().OfType<ScrollViewer>().First();
        var first = list.GetVisualDescendants().OfType<ListBoxItem>().First(item => item.Bounds.Width > 0);
        var right = first.TranslatePoint(new Point(first.Bounds.Width, 0), window)!.Value.X;
        var scrollbar = scroll.GetVisualDescendants().OfType<ScrollBar>()
            .FirstOrDefault(item => item.Orientation == Avalonia.Layout.Orientation.Vertical &&
                item.IsEffectivelyVisible && item.Bounds.Width > 0);
        var limit = scrollbar is null
            ? scroll.TranslatePoint(new Point(scroll.Bounds.Width - 18, 0), window)!.Value.X
            : scrollbar.TranslatePoint(default, window)!.Value.X - 2;
        Assert.True(right <= limit, $"La riga termina a {right:0.##}, oltre il limite della scrollbar {limit:0.##}.");
        if (list.Name == "ResourcesTable")
        {
            var header = window.FindControl<Grid>("ResourceTableHeader")!;
            var headerRight = header.TranslatePoint(new Point(header.Bounds.Width, 0), window)!.Value.X;
            Assert.True(headerRight <= limit,
                $"L'intestazione 'Si riempie tra' termina a {headerRight:0.##}, oltre il limite della scrollbar {limit:0.##}.");
        }
    }
    private static void Click(Window window, string name)
    {
        Dispatcher.UIThread.RunJobs();
        var button = window.FindControl<Button>(name)!;
        Assert.True(button.IsEffectivelyEnabled);
        var point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
        window.MouseDown(point, MouseButton.Left); window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }
    private static async Task UntilAsync(Func<bool> predicate)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!predicate()) await Task.Delay(10, timeout.Token);
    }

    private sealed class ManualTime : TimeProvider
    {
        private long _ticks;
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override long GetTimestamp() => _ticks;
        public void Advance(TimeSpan elapsed) => _ticks += elapsed.Ticks;
    }

    private sealed class MemoryStore : IGameSaveStore
    {
        public GameSnapshot? Snapshot { get; set; }
        public bool FailWrites { get; set; }
        public GameSaveError? LoadError { get; init; }
        public int WriteCount { get; private set; }
        public GameSnapshot Load() => LoadError is { } error ? throw new GameSaveException(error, "Unavailable") :
            Snapshot ?? throw new GameSaveException(GameSaveError.NotFound, "Missing");
        public void Save(GameSnapshot snapshot)
        {
            WriteCount++;
            if (FailWrites) throw new GameSaveException(GameSaveError.Unavailable, "Unavailable");
            Snapshot = snapshot;
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public string PathName { get; } = Path.Combine(Path.GetTempPath(), "SimRisorse.M3.DesktopTests", Guid.NewGuid().ToString("N"));
        public void Dispose() { if (Directory.Exists(PathName)) Directory.Delete(PathName, recursive: true); }
    }

    private sealed class BlockingStore : IGameSaveStore, IDisposable
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public ManualResetEventSlim Release { get; } = new();
        public void Save(GameSnapshot snapshot) { Started.TrySetResult(); if (!Release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException(); }
        public GameSnapshot Load() => throw new GameSaveException(GameSaveError.NotFound, "Missing");
        public void Dispose() => Release.Dispose();
    }
}
