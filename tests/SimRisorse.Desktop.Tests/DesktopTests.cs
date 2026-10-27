using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
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
        var historical = DesktopOptions.Parse(["--scenario", "economy-foundation"]);
        Assert.Equal("civil-growth", current.Scenario.Id);
        Assert.EndsWith("civil-growth.sqlite3", current.SavePath);
        Assert.EndsWith("game.sqlite3", historical.SavePath);
        Assert.NotEqual(current.SavePath, historical.SavePath);
        var smoke = DesktopOptions.Parse(["--smoke-test"]);
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
            Buildings = [.. current.Buildings.Where(item => !CivilGrowthScenario.Changes.Any(change => change.AddedBuildings.Contains(item.BuildingId)))]
        });
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new SqliteGameSaveStore(path));
        await viewModel.InitializeAsync();
        Assert.Contains("civil-growth-1 a civil-growth-5", viewModel.Message);
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "warehouse");
        Assert.Equal("+500 capacità per ogni risorsa per edificio", viewModel.Selected.Recipe);
        Assert.Equal("Da costruire", viewModel.Selected.Status);
        Assert.False(viewModel.Selected.IsProducer);
        Assert.False(viewModel.AssignCommand.CanExecute(null));
        Assert.True(await viewModel.SaveAsync());
        Assert.True(File.Exists(Path.Combine(directory.PathName, "civil-growth.civil-growth-1.backup.sqlite3")));
    }

    [AvaloniaFact]
    public async Task WarehouseRaisesCapacityWhilePreservingTheSmallerBreadBaseCapacity()
    {
        var viewModel = new MainViewModel(new GameRuntime(CivilGrowthScenario.Create()), new MemoryStore());
        await viewModel.InitializeAsync();
        viewModel.Selected = viewModel.Buildings.Single(item => item.Id == "warehouse");
        viewModel.ConstructCommand.Execute(null);
        Assert.Equal("+500 capacità per ogni risorsa", viewModel.Selected.Status);
        Assert.All(viewModel.Resources.Where(item => item.Id != "bread"), item => Assert.Equal("1.500", item.Capacity));
        Assert.Equal("600", viewModel.Resources.Single(item => item.Id == "bread").Capacity);
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
        Assert.Equal(8, viewModel.Resources.Count(item => item.Capacity == "1.000"));
        Assert.Equal("100", viewModel.Resources.Single(item => item.Id == "bread").Capacity);
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
            Assert.Equal(Color.Parse("#259D80"), Assert.IsAssignableFrom<ISolidColorBrush>(build.Background).Color);
            Assert.Equal(originalGeometry, stableControls.Select(control =>
                (Position: control.TranslatePoint(default, window)!.Value, Size: control.Bounds.Size)).ToArray());

            // Il puntatore resta sul comando mentre l'acquisto esaurisce le risorse.
            // In questo stato il bottone disabilitato non deve mantenere l'hover verde.
            viewModel.ConstructCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.False(viewModel.ConstructCommand.CanExecute(null));
            Assert.False(build.IsEffectivelyEnabled);
            Assert.Equal(Color.Parse("#B8C2BC"), Assert.IsAssignableFrom<ISolidColorBrush>(build.Background).Color);
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
                Resources = SimRisorse.Core.Economy.ResourceBundle.Create(("food", 100m), ("bread", 100m),
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
        Assert.Contains("Forno in attesa: deposito quasi pieno", viewModel.BreadSupplyStatus);
        Assert.DoesNotContain("Capacità nominale dei Forni insufficiente", viewModel.BreadSupplyStatus);

        var window = new MainWindow(viewModel, startTimer: false, initialize: false) { Width = 1040, Height = 740 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var supply = window.FindControl<TextBlock>("BreadSupplyStatusText")!;
            Assert.True(supply.IsEffectivelyVisible);
            Assert.Contains("deposito quasi pieno", supply.Text!);
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
                var scroll = window.FindControl<ScrollViewer>("ResourcesScroll")!;
                Assert.InRange(scroll.TranslatePoint(default, window)!.Value.Y + scroll.Bounds.Height, 0, window.ClientSize.Height);
            }
            var growthBar = window.FindControl<ProgressBar>("GrowthProgressBar")!;
            var growthPosition = growthBar.TranslatePoint(default, window)!.Value;
            Assert.InRange(growthPosition.X, 22, 185 - growthBar.Bounds.Width);
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
            Assert.Equal("+2 /s", viewModel.Resources.Single(item => item.Id == "logs").NetRate);
            Assert.Equal("±0 /s", viewModel.Resources.Single(item => item.Id == "planks").NetRate);
            viewModel.TogglePauseCommand.Execute(null);
            viewModel.Advance(TimeSpan.FromSeconds(1));
            Assert.Equal("±0 /s", viewModel.Resources.Single(item => item.Id == "logs").NetRate);
            Assert.Equal("+1 /s", viewModel.Resources.Single(item => item.Id == "planks").NetRate);
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
            Assert.True(window.FindControl<Button>("DiscardCloseButton")!.IsVisible);
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
