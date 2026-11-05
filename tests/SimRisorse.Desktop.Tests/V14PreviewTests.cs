using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Desktop;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Desktop.Tests;

public sealed class V14PreviewTests
{
    [Fact]
    public void ExplicitV14LaunchPreservesTheExistingV14SaveSlot()
    {
        var normal = DesktopOptions.Parse(["--v14"]);
        var oldShortcut = DesktopOptions.Parse(["--experimental-v14"]);
        var explicitV14 = DesktopOptions.Parse(["--v14"]);
        Assert.True(normal.UseV14);
        Assert.True(oldShortcut.UseV14);
        Assert.True(explicitV14.UseV14);
        Assert.Equal(normal.SavePath, oldShortcut.SavePath);
        Assert.Equal(normal.SavePath, explicitV14.SavePath);
        Assert.EndsWith("civil-growth-v14.sqlite3", normal.SavePath);
        Assert.Contains("experiments", normal.SavePath, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("civil-growth-13", normal.Scenario.Catalog.RulesVersion);
    }

    [Fact]
    public void LegacyModeIsExplicitAndKeepsTheV13SaveSlot()
    {
        var legacy = DesktopOptions.Parse(["--legacy-v13"]);
        var historicShortcut = DesktopOptions.Parse(["--scenario", "civil-growth"]);
        var normal = DesktopOptions.Parse([]);
        Assert.False(legacy.UseV14);
        Assert.False(historicShortcut.UseV14);
        Assert.Equal(legacy.SavePath, historicShortcut.SavePath);
        Assert.EndsWith("civil-growth.sqlite3", legacy.SavePath);
        Assert.NotEqual(legacy.SavePath, normal.SavePath);
    }

    [Fact]
    public void LegacySmokeAndHistoricalScenarioStayOnTheirOriginalRuntime()
    {
        var smoke = DesktopOptions.Parse(["--smoke-test"]);
        var historical = DesktopOptions.Parse(["--scenario", "economy-foundation"]);
        Assert.False(smoke.UseV14);
        Assert.True(smoke.SmokeTest);
        Assert.False(historical.UseV14);
        Assert.EndsWith("game.sqlite3", historical.SavePath);
        Assert.NotEqual(smoke.SavePath, DesktopOptions.Parse([]).SavePath);
    }

    [Fact]
    public void BothModesRejectTheOtherModesReservedSavePaths()
    {
        var v14 = DesktopOptions.Parse(["--v14"]);
        var v13 = DesktopOptions.Parse(["--legacy-v13"]);
        var historical = DesktopOptions.Parse(["--scenario", "economy-foundation"]);
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--v14", "--save-path", v13.SavePath]));
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--v14", "--save-path", historical.SavePath]));
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--legacy-v13", "--save-path", v14.SavePath]));
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse([
            "--scenario", "civil-growth", "--save-path", v14.SavePath]));
    }

    [Fact]
    public void ConflictingModeOptionsFailWithoutOpeningAnySave()
    {
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--legacy-v13", "--v14"]));
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--experimental-v14", "--smoke-test"]));
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse([
            "--v14", "--scenario", "economy-foundation"]));
        Assert.Throws<ArgumentException>(() => DesktopOptions.Parse(["--unknown-option"]));
    }

    [Fact]
    public void FirstLaunchIsPausedAndNeverStartsAnyResearchAutomatically()
    {
        using var sandbox = new TestDirectory();
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        vm.Initialize();
        Assert.True(vm.IsPaused);
        Assert.Equal("0,00 punti", vm.ResearchBalance);
        Assert.Equal(0, vm.SchoolCount);
        Assert.Equal(8, vm.Population);
        Assert.Equal(16, vm.Producers.Count);
        Assert.Equal(11, vm.Technologies.Count);
        Assert.Contains(vm.Technologies, item => item.Id == "logistica-1" &&
            item.Status.Contains("Non disponibile", StringComparison.Ordinal));
        Assert.False(File.Exists(sandbox.V14)); // Preview opening does not write a checkpoint.
        var initialTime = vm.Clock;
        vm.Advance(TimeSpan.FromSeconds(40));
        Assert.Equal(initialTime, vm.Clock); // Paused by default.
    }

    [Theory]
    [InlineData("lavorazioni-1", "Sblocca edifici Grande:", "Segheria")]
    [InlineData("lavorazioni-2", "Sblocca edifici Avanzato:", "Segheria")]
    [InlineData("alimentazione-1", "Sblocca edifici Grande:", "Forno")]
    public void TechnologyDetailsShowTheRealBuildingTierBenefit(string technologyId,
        string expectedEffect, string expectedBuilding)
    {
        using var sandbox = new TestDirectory();
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        vm.SelectedTechnology = vm.Technologies.Single(item => item.Id == technologyId);
        Assert.Contains(expectedEffect, vm.SelectedTechnologySummary);
        Assert.Contains(expectedBuilding, vm.SelectedTechnologySummary);
        Assert.Contains("Prerequisiti ricerca:", vm.SelectedTechnologySummary);
    }

    [Fact]
    public void DeferredResearchShowsNoImplementedBenefitAndRemainsUnpurchasable()
    {
        using var sandbox = new TestDirectory();
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        vm.SelectedTechnology = vm.Technologies.Single(item => item.Id == "logistica-1");
        Assert.Contains("Effetto non implementato", vm.SelectedTechnologySummary);
        Assert.False(vm.CanBuySelectedTechnology);
        Assert.False(vm.BuyTechnologyCommand.CanExecute(null));
    }

    [Fact]
    public void TechnologyCostDisplayHasTwoDecimalsButPurchaseUsesExactMilliPoints()
    {
        using var sandbox = new TestDirectory();
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m),
            120, 1, 2, 100, pencilsPerStudent: 2);
        var catalog = TechnologyCatalog.ProposedTierTree;
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            school, catalog, V14SchoolConstructionRules.Candidate).Snapshot();
        var store = new V14SqliteSaveStore(sandbox.V14, baseline, school, catalog);
        // 99.999 is deliberately displayed as 100,00; purchasing MUST still require 100.000.
        store.Save(initial with { ResearchBalanceMilliPoints = 99_999L });
        var vm = new V14PreviewViewModel(baseline, store, school, catalog);
        vm.Initialize();
        var basic = vm.Technologies.Single(item => item.Id == "lavorazioni-1");
        Assert.Equal("100,00", basic.Cost);
        vm.SelectedTechnology = basic;
        Assert.Equal("100,00 punti", vm.ResearchBalance);
        Assert.Equal("0,00 punti/s", vm.ResearchRate);
        Assert.Equal("Punti insufficienti", basic.Status);
        Assert.False(vm.CanBuySelectedTechnology);
        Assert.False(vm.BuyTechnologyCommand.CanExecute(null));
        vm.BuyTechnologyCommand.Execute(null);
        Assert.Equal("Punti insufficienti", basic.Status);

        store.Save(initial with { ResearchBalanceMilliPoints = 100_000L });
        Assert.True(vm.Load());
        Assert.True(vm.CanBuySelectedTechnology);
        Assert.True(vm.BuyTechnologyCommand.CanExecute(null));
        Assert.Equal("Acquistabile", basic.Status);
        vm.BuyTechnologyCommand.Execute(null);
        Assert.Equal("Acquistata", basic.Status);
        Assert.Equal("0,00 punti", vm.ResearchBalance);
        Assert.False(vm.CanBuySelectedTechnology);
        Assert.False(vm.BuyTechnologyCommand.CanExecute(null));

        // Additional blockers must never leave an enabled purchase action.
        vm.SelectedTechnology = vm.Technologies.Single(item => item.Id == "logistica-1");
        Assert.False(vm.CanBuySelectedTechnology);
        Assert.False(vm.BuyTechnologyCommand.CanExecute(null));
        vm.SelectedTechnology = vm.Technologies.Single(item => item.Id == "lavorazioni-2");
        Assert.False(vm.CanBuySelectedTechnology);
        Assert.False(vm.BuyTechnologyCommand.CanExecute(null));
    }

    [Fact]
    public void SchoolReadModelShowsPencilRequirementsAndBatchCosts()
    {
        using var sandbox = new TestDirectory();
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        vm.Initialize();
        Assert.False(vm.EnrollCommand.CanExecute(null));
        Assert.False(vm.CraftPencilsCommand.CanExecute(null));
        Assert.Contains("2 Matite", vm.SchoolSuppliesStatus);
        Assert.Equal("0", vm.Resources.Single(item => item.Id == "pencils").QuantityRounded);
        Assert.Equal("1.000", vm.Resources.Single(item => item.Id == "pencils").Capacity);
        vm.SchoolQuantity = 5;
        Assert.Contains("10 Matite", vm.SchoolSuppliesStatus);
        Assert.Equal("Produci +20 Matite", vm.SchoolCraftLabel);
        Assert.Equal("Iscrivi +5", vm.SchoolEnrollLabel);
        Assert.Contains("Nessuno studente", vm.SchoolTrainingStatus);
        Assert.Equal(0d, vm.SchoolTrainingPercent);
    }

    [Fact]
    public void RejectedSchoolAndDeferredTechnologyDoNotChangeResourcesOrUnlocks()
    {
        using var sandbox = new TestDirectory();
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        vm.Initialize();
        var before = vm.Resources.Select(item => (item.Id, item.Quantity)).ToArray();
        vm.BuildSchoolCommand.Execute(null);
        Assert.Equal(0, vm.SchoolCount);
        Assert.Contains("mancano", vm.Message, StringComparison.OrdinalIgnoreCase);
        vm.SelectedTechnology = vm.Technologies.Single(item => item.Id == "logistica-1");
        vm.BuyTechnologyCommand.Execute(null);
        Assert.Equal("0,00 punti", vm.ResearchBalance);
        Assert.Equal("Non disponibile (nessun effetto)", vm.SelectedTechnology!.Status);
        Assert.Equal(before, vm.Resources.Select(item => (item.Id, item.Quantity)).ToArray());
    }

    [Fact]
    public void SaveLoadAndSimulationPreserveTechnologiesAndSelections()
    {
        using var sandbox = new TestDirectory();
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        vm.Initialize();
        var tech = vm.Technologies.Single(item => item.Id == "lavorazioni-1");
        var producer = vm.Producers.Single(item => item.Id == "sawmill");
        vm.SelectedTechnology = tech;
        vm.SelectedProducer = producer;
        Assert.True(vm.Save());
        Assert.True(File.Exists(sandbox.V14));
        vm.TogglePauseCommand.Execute(null);
        vm.Advance(TimeSpan.FromSeconds(5));
        Assert.Same(tech, vm.SelectedTechnology);
        Assert.Same(producer, vm.SelectedProducer);
        Assert.NotEqual("0.00:00:00", vm.Clock);
        Assert.True(vm.Load());
        Assert.True(vm.IsPaused);
        Assert.Equal("0.00:00:00", vm.Clock);
        Assert.True(vm.CanSave);
        var reopened = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        reopened.Initialize();
        Assert.True(reopened.IsPaused);
        Assert.Equal(vm.Population, reopened.Population);
        Assert.Equal(vm.ResearchBalance, reopened.ResearchBalance);
    }

    [Fact]
    public void ProducingBuildStaffingAndUpgradeWithoutResearchUseSessionRules()
    {
        using var sandbox = new TestDirectory();
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        vm.Initialize();
        vm.SelectedProducer = vm.Producers.Single(item => item.Id == "sawmill");
        Assert.Contains("46", vm.SelectedBuildCost, StringComparison.Ordinal);
        Assert.Contains("23", vm.SelectedBuildCost, StringComparison.Ordinal);
        vm.BuildProducerCommand.Execute(null);
        Assert.Contains("costruito", vm.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Base 2", vm.SelectedProducer!.Description, StringComparison.Ordinal);
        vm.AssignWorkerCommand.Execute(null);
        Assert.Contains("assegnato", vm.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(4, vm.AvailablePopulation);
        vm.ReleaseWorkerCommand.Execute(null);
        Assert.Equal(5, vm.AvailablePopulation);
        var resources = vm.Resources.Select(item => (item.Id, item.Quantity)).ToArray();
        vm.UpgradeProducerCommand.Execute(null);
        Assert.Contains("ricerca", vm.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Base 2", vm.SelectedProducer!.Description, StringComparison.Ordinal);
        Assert.Equal(resources, vm.Resources.Select(item => (item.Id, item.Quantity)).ToArray());
        Assert.True(vm.Save());
        var loaded = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        loaded.Initialize();
        Assert.Contains("Base 2", loaded.Producers.Single(item => item.Id == "sawmill").Description, StringComparison.Ordinal);
    }

    [Fact]
    public void CivicConstructionDisplaysSuccessAndDoesNotConjureNewResidents()
    {
        using var sandbox = new TestDirectory();
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        vm.Initialize();
        var originalPopulation = vm.Population;
        vm.SelectedCivic = vm.Civics.Single(item => item.Id == "small-house");
        Assert.Contains("92", vm.SelectedCivicCost, StringComparison.Ordinal);
        vm.BuildCivicCommand.Execute(null);
        Assert.Contains("costruito", vm.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(originalPopulation, vm.Population);
        Assert.Contains("106", vm.SelectedCivicCost, StringComparison.Ordinal);
    }

    [Fact]
    public void CorruptOrLegacyFileIsNeverOverwrittenByPreview()
    {
        using var sandbox = new TestDirectory();
        var legacyPath = sandbox.V14;
        var old = new SqliteGameSaveStore(legacyPath);
        old.Save(GameSession.Create(CivilGrowthScenario.Create()).Snapshot());
        var before = File.ReadAllBytes(legacyPath);
        var vm = V14PreviewViewModel.CreateForPreview(legacyPath);
        vm.Initialize();
        Assert.False(vm.CanSave);
        Assert.False(vm.Save());
        Assert.Equal(before, File.ReadAllBytes(legacyPath));
        Assert.Contains("bloccato", vm.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PreviewSaveDoesNotModifyAnExistingV13Checkpoint()
    {
        using var sandbox = new TestDirectory();
        var originalV13 = Path.Combine(sandbox.Folder, "civil-growth.sqlite3");
        new SqliteGameSaveStore(originalV13).Save(GameSession.Create(CivilGrowthScenario.Create()).Snapshot());
        var before = File.ReadAllBytes(originalV13);
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        vm.Initialize();
        Assert.True(vm.Save());
        Assert.Equal(before, File.ReadAllBytes(originalV13));
        Assert.NotEqual(before, File.ReadAllBytes(sandbox.V14));
    }

    [Fact]
    public void ProductiveTiersAreSeparateBuildingRowsAndSelectionDefinesTheTier()
    {
        using var sandbox = new TestDirectory();
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        vm.Initialize();
        Assert.Equal(54, vm.Buildings.Count); // 16 families × 3, 5 historical civics, 1 school
        var sawmills = vm.Buildings.Where(item => item.FamilyId == "sawmill").ToArray();
        Assert.Equal(3, sawmills.Length);
        Assert.Equal(new[] { ProductionTier.Base, ProductionTier.Large, ProductionTier.Advanced },
            sawmills.Select(item => item.Tier!.Value));
        Assert.Equal("Segheria", sawmills[0].Name);
        Assert.Equal("Segheria grande", sawmills[1].Name);
        Assert.Equal("Segheria avanzata", sawmills[2].Name);
        Assert.Contains("50", sawmills[0].CapacityLines.Single().Text);
        Assert.Contains("150", sawmills[1].CapacityLines.Single().Text);
        Assert.Contains("500", sawmills[2].CapacityLines.Single().Text);
        vm.SelectedBuilding = sawmills[1];
        Assert.Equal(ProductionTier.Large, vm.BuildTier.Tier);
        Assert.Contains("120", vm.SelectedBuilding!.Price, StringComparison.Ordinal);
        Assert.Contains("60", vm.SelectedBuilding!.Price, StringComparison.Ordinal);
        var before = vm.Resources.Select(item => (item.Id, item.Quantity)).ToArray();
        Assert.False(vm.CanBuildSelected);
        Assert.False(vm.BuildSelectedCommand.CanExecute(null));
        Assert.Contains("Ricerca richiesta", vm.SelectedBuilding.Requirements, StringComparison.OrdinalIgnoreCase);
        vm.BuildSelectedCommand.Execute(null);
        Assert.Equal(before, vm.Resources.Select(item => (item.Id, item.Quantity)));
        Assert.Same(sawmills[1], vm.SelectedBuilding);
        vm.SelectedBuilding = sawmills[0];
        vm.BuildSelectedCommand.Execute(null);
        Assert.Equal(2, sawmills[0].Count);
        Assert.Equal(0, sawmills[1].Count);
        Assert.Same(sawmills[0], vm.SelectedBuilding);
    }

    [Fact]
    public void SchoolIsAvailableFromTheExistingBuildingListAndResearchFromItsOwnMenu()
    {
        using var sandbox = new TestDirectory();
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        vm.Initialize();
        var school = vm.Buildings.Single(item => item.FamilyId == "school");
        Assert.Equal(V14BuildingKind.School, school.Kind);
        Assert.Contains("120", school.Price, StringComparison.Ordinal);
        vm.SelectedBuilding = school;
        Assert.False(vm.CanBuildSelected);
        Assert.False(vm.BuildSelectedCommand.CanExecute(null));
        Assert.Contains("Costruire prima", school.Requirements, StringComparison.OrdinalIgnoreCase);
        vm.BuildSelectedCommand.Execute(null);
        Assert.Equal(0, vm.SchoolCount);
        Assert.Equal(11, vm.Technologies.Count);
        Assert.Equal("0,00 punti", vm.ResearchBalance);
    }

    [Fact]
    public void UpgradeHasExplicitDestinationButtonsWithoutChangingSelectionOrPurchaseHistory()
    {
        using var sandbox = new TestDirectory();
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        vm.Initialize();
        var baseSawmill = vm.Buildings.Single(item => item.FamilyId == "sawmill" && item.Tier == ProductionTier.Base);
        vm.SelectedBuilding = baseSawmill;
        Assert.False(vm.CanUpgradeNext);
        Assert.False(vm.CanUpgradeDirectlyToAdvanced);
        Assert.Contains("Grande", vm.NextUpgradeLabel, StringComparison.Ordinal);
        Assert.Contains("ricerca", vm.NextUpgradeRequirement, StringComparison.OrdinalIgnoreCase);
        var before = vm.Resources.Select(item => (item.Id, item.Quantity)).ToArray();
        vm.UpgradeSelectedCommand.Execute(null);
        Assert.Equal(before, vm.Resources.Select(item => (item.Id, item.Quantity)));
        Assert.Same(baseSawmill, vm.SelectedBuilding);
    }

    [AvaloniaFact]
    public void ExperimentalWindowUsesOriginalSidebarAndBuildingDetailsWithoutTierComboboxes()
    {
        using var sandbox = new TestDirectory();
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        var window = new V14PreviewWindow(vm, startTimer: false) { Width = 1240, Height = 850 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var tabs = window.FindControl<TabControl>("V14Sections")!;
            var sidebar = window.FindControl<ListBox>("V14NavigationMenu")!;
            Assert.Equal(5, sidebar.Items.Count);
            Assert.Equal(0, tabs.SelectedIndex);
            Assert.NotNull(window.FindControl<ListBox>("V14Buildings")!.SelectedItem);
            Assert.True(window.FindControl<Button>("V14BuildSelected")!.IsEffectivelyVisible == false); // initial Municipio
            Assert.Null(window.FindControl<ComboBox>("V14BuildTier"));
            Assert.Null(window.FindControl<ComboBox>("V14UpgradeFrom"));
            Assert.Null(window.FindControl<ComboBox>("V14UpgradeTo"));
            var large = vm.Buildings.Single(item => item.FamilyId == "sawmill" && item.Tier == ProductionTier.Large);
            vm.SelectedBuilding = large;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Segheria grande", vm.SelectedBuilding!.Name);
            Assert.False(window.FindControl<Button>("V14BuildSelected")!.IsEffectivelyVisible);
            Assert.Contains("Ricerca richiesta", large.Requirements, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("120", window.FindControl<TextBlock>("V14BuildingCost")!.Text);
            Assert.NotNull(window.FindControl<ItemsControl>("V14BuildingCostLines"));
            var basic = vm.Buildings.Single(item => item.FamilyId == "sawmill" && item.Tier == ProductionTier.Base);
            vm.SelectedBuilding = basic;
            Dispatcher.UIThread.RunJobs();
            Assert.True(window.FindControl<Button>("V14BuildSelected")!.IsEffectivelyVisible);
            tabs.SelectedIndex = 3;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(3, sidebar.SelectedIndex);
            // The school is built from Edifici, never from the School operations tab.
            Assert.Null(window.FindControl<Button>("V14BuildSchool"));
            Assert.Null(window.FindControl<Button>("V14CloseWithoutSave"));
            Assert.True(window.FindControl<Button>("V14CraftPencils")!.IsEffectivelyVisible);
            Assert.True(window.FindControl<Button>("V14EnrollStudent")!.IsEffectivelyVisible);
            Assert.NotNull(window.FindControl<ProgressBar>("V14SchoolProgressBar"));
            Assert.NotNull(window.FindControl<ComboBox>("V14SchoolQuantitySelector"));
            Assert.True(window.FindControl<Button>("V14CancelStudent")!.IsEffectivelyVisible);
            Assert.True(window.FindControl<Button>("V14AssignResearcher")!.IsEffectivelyVisible);
            Assert.True(window.FindControl<Button>("V14UnassignResearcher")!.IsEffectivelyVisible);
            Assert.True(window.FindControl<Button>("V14ReturnResearcher")!.IsEffectivelyVisible);
            tabs.SelectedIndex = 4;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(4, sidebar.SelectedIndex);
            Assert.True(window.FindControl<Button>("V14BuyTechnology")!.IsEffectivelyVisible);
            Assert.NotNull(window.FindControl<ListBox>("V14Technologies")!.SelectedItem);
            tabs.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window.FindControl<ListBox>("V14Resources"));
            tabs.SelectedIndex = 0;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(vm.SelectedBuilding, window.FindControl<ListBox>("V14Buildings")!.SelectedItem);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public void PreviewWindowOnNormalCloseWritesOnlyDedicatedV14File()
    {
        using var sandbox = new TestDirectory();
        var vm = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        var window = new V14PreviewWindow(vm, startTimer: false);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.Close();
        Dispatcher.UIThread.RunJobs();
        Assert.True(File.Exists(sandbox.V14));
        var reopened = V14PreviewViewModel.CreateForPreview(sandbox.V14);
        reopened.Initialize();
        Assert.True(reopened.CanSave);
        Assert.True(reopened.IsPaused);
    }

    private sealed class TestDirectory : IDisposable
    {
        public string Folder { get; } = Path.Combine(Path.GetTempPath(), "SimRisorse.V14Preview.Tests", Guid.NewGuid().ToString("N"));
        public string V14 => Path.Combine(Folder, "preview-v14.sqlite3");
        public void Dispose() { if (Directory.Exists(Folder)) Directory.Delete(Folder, recursive: true); }
    }
}
