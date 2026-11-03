using System.Collections.Immutable;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.VisualTree;
using System.Globalization;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Sessions;
using SimRisorse.Application.Scenarios;
using SimRisorse.Infrastructure.Persistence;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using Xunit;

namespace SimRisorse.Desktop.Tests;

/// <summary>Permanent regression barrier: v14 may add to, but must not simplify, v13 panels.</summary>
public sealed class V14UiParityRegressionTests
{
    private static V14PreviewViewModel Fresh() =>
        V14PreviewViewModel.CreateForPreview(Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.Parity." + Guid.NewGuid().ToString("N") + ".sqlite3"));

    [Fact]
    public void V14ResourcesUseOriginalResourceRowWithFlowsAndCapacityBreakdown()
    {
        var view = Fresh();
        view.Initialize();
        Assert.Equal(17, view.Resources.Count);
        Assert.Contains(view.Resources, item => item.Id == "pencils" && item.Name == "Matite");
        Assert.All(view.Resources, item =>
        {
            Assert.IsType<ResourceRow>(item);
            Assert.NotEqual(string.Empty, item.QuantityRounded);
            Assert.EndsWith("/s", item.Production);
            Assert.EndsWith("/s", item.Consumption);
            Assert.EndsWith("/s", item.NetRate);
            Assert.NotEqual(string.Empty, item.TimeToFull);
            Assert.Contains("Capienza:", item.CapacityBreakdown, StringComparison.Ordinal);
        });
        Assert.Equal(view.Resources.OrderBy(item => item.Name, StringComparer.Create(
            System.Globalization.CultureInfo.GetCultureInfo("it-IT"), true)).Select(item => item.Id),
            view.Resources.Select(item => item.Id));
    }

    [Fact]
    public void V14FirstAdvancedArrivalShowsTheSameDetailedBreadWarningAsV13()
    {
        var baseline = CivilGrowthScenario.Create();
        var school = new SimRisorse.Core.Research.SchoolResearchRules("food",
            SimRisorse.Core.Economy.ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
        var tech = SimRisorse.Core.Research.TechnologyCatalog.ProposedTierTree;
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            school, tech, V14SchoolConstructionRules.Candidate).Snapshot();
        var checkpoint = initial with
        {
            Resources = SimRisorse.Core.Economy.ResourceBundle.Empty,
            OtherBuildings = initial.OtherBuildings.Select(item => item.BuildingId == "large-house"
                ? item with { Count = 1 } : item).ToImmutableArray(),
            PurchaseCounters = initial.PurchaseCounters.Select(item => item.FamilyId == "large-house"
                ? item with { Acquired = 1 } : item).ToImmutableArray()
        };
        var path = Path.Combine(Path.GetTempPath(), "SimRisorse.UI.Bread." + Guid.NewGuid().ToString("N") + ".sqlite3");
        try
        {
            var store = new V14SqliteSaveStore(path, baseline, school, tech);
            store.Save(checkpoint);
            var view = new V14PreviewViewModel(baseline, store, school, tech);
            view.Initialize();
            Assert.True(view.ShowBreadSupplyStatus);
            Assert.Contains("primo residente avanzato", view.BreadSupplyStatus);
            Assert.Equal("0", view.Resources.Single(item => item.Id == "bread").QuantityRounded);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void LockedBuildingsCannotBeBuiltAndAffordableBuildingsKeepBatchCommands()
    {
        var view = Fresh();
        view.Initialize();
        view.SelectedBuilding = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
            item.Tier == ProductionTier.Large);
        var before = view.Resources.Select(item => (item.Id, item.Quantity)).ToArray();
        Assert.False(view.CanBuildSelected);
        Assert.False(view.BuildSelectedCommand.CanExecute(null));
        Assert.False(view.BuildFiveCommand.CanExecute(null));
        view.BuildSelectedCommand.Execute(null);
        Assert.Equal(before, view.Resources.Select(item => (item.Id, item.Quantity)));
        Assert.Contains("Ricerca richiesta", view.SelectedBuilding.Requirements);
        view.SelectedBuilding = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
            item.Tier == ProductionTier.Base);
        Assert.True(view.CanBuildSelected);
        Assert.True(view.BuildSelectedCommand.CanExecute(null));
        Assert.False(view.BuildFiveCommand.CanExecute(null));
        Assert.False(view.BuildTenCommand.CanExecute(null));
        view.BuildSelectedCommand.Execute(null);
        Assert.Equal(2, view.SelectedBuilding.Count);
    }

    [Fact]
    public void BatchQuotesShowCumulativeProgressivePricesAndMissingMaterials()
    {
        var view = Fresh();
        view.Initialize();
        view.SelectedBuilding = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
            item.Tier == ProductionTier.Base);
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            school, TechnologyCatalog.ProposedTierTree, V14SchoolConstructionRules.Candidate).Snapshot();
        var previous = initial.PurchaseCounters.Single(item => item.FamilyId == "sawmill").Acquired;
        var cost = baseline.Catalog.BuildingsById["sawmill"].Cost;
        var quoteOne = ProgressiveConstructionPricing.QuoteNewBuildings(cost, 1m, 1.15m, previous, 1);
        var quoteFive = ProgressiveConstructionPricing.QuoteNewBuildings(cost, 1m, 1.15m, previous, 5);
        var quoteTen = ProgressiveConstructionPricing.QuoteNewBuildings(cost, 1m, 1.15m, previous, 10);
        var italian = CultureInfo.GetCultureInfo("it-IT");
        foreach (var item in cost.Amounts)
        {
            var resourceName = baseline.Catalog.ResourcesById[item.Key].Name;
            Assert.Contains(quoteFive[item.Key].Units.ToString("N0", italian) + " " + resourceName, view.BuildFiveCost);
            Assert.Contains(quoteTen[item.Key].Units.ToString("N0", italian) + " " + resourceName, view.BuildTenCost);
            Assert.True(quoteFive[item.Key].Units > quoteOne[item.Key].Units * 5m);
        }
        Assert.Contains("Mancano:", view.BuildFiveCost);
        Assert.Contains("Mancano:", view.BuildTenCost);
        Assert.False(view.BuildFiveCommand.CanExecute(null));
        Assert.False(view.BuildTenCommand.CanExecute(null));
    }

    [Fact]
    public void BatchQuotesUpdateAfterPurchaseWithoutChangingSelection()
    {
        var view = Fresh();
        view.Initialize();
        var selected = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
            item.Tier == ProductionTier.Base);
        view.SelectedBuilding = selected;
        var oldFive = view.BuildFiveCost;
        var oldTen = view.BuildTenCost;
        Assert.True(view.BuildSelectedCommand.CanExecute(null));
        view.BuildSelectedCommand.Execute(null);
        Assert.Same(selected, view.SelectedBuilding);
        Assert.Equal(2, selected.Count);
        Assert.NotEqual(oldFive, view.BuildFiveCost);
        Assert.NotEqual(oldTen, view.BuildTenCost);
    }

    [AvaloniaFact]
    public void UpgradeWithResearchButWithoutMaterialsRemainsVisibleAndDisabled()
    {
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
        var technologies = TechnologyCatalog.ProposedTierTree;
        var checkpoint = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            school, technologies, V14SchoolConstructionRules.Candidate).Snapshot() with
        {
            Resources = ResourceBundle.Empty,
            UnlockedTechnologyIds = ["lavorazioni-1", "lavorazioni-2"]
        };
        var path = Path.Combine(Path.GetTempPath(), "SimRisorse.UI.Upgrade." + Guid.NewGuid().ToString("N") + ".sqlite3");
        try
        {
            var store = new V14SqliteSaveStore(path, baseline, school, technologies);
            store.Save(checkpoint);
            var view = new V14PreviewViewModel(baseline, store, school, technologies);
            view.Initialize();
            view.SelectedBuilding = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Base);
            Assert.True(view.ShowUpgradeNext);
            Assert.True(view.ShowUpgradeDirectlyToAdvanced);
            Assert.False(view.CanUpgradeNext);
            Assert.False(view.CanUpgradeDirectlyToAdvanced);
            Assert.False(view.UpgradeSelectedCommand.CanExecute(null));
            Assert.False(view.UpgradeSelectedToAdvancedCommand.CanExecute(null));
            Assert.Contains("Mancano:", view.NextUpgradeCost);
            Assert.Contains("Mancano:", view.DirectUpgradeCost);
            Assert.Equal(string.Empty, view.NextUpgradeRequirement);
            var window = new V14PreviewWindow(view, startTimer: false);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                var nextButton = window.FindControl<Button>("V14UpgradeNext")!;
                var directButton = window.FindControl<Button>("V14UpgradeAdvanced")!;
                Assert.True(nextButton.IsEffectivelyVisible);
                Assert.True(directButton.IsEffectivelyVisible);
                Assert.Same(view.UpgradeSelectedCommand, nextButton.Command);
                Assert.Same(view.UpgradeSelectedToAdvancedCommand, directButton.Command);
                // Avalonia 12: ICommand.CanExecute influences IsEffectivelyEnabled
                // through Button.IsEnabledCore; the user-set IsEnabled may remain true.
                Assert.False(nextButton.IsEffectivelyEnabled);
                Assert.False(directButton.IsEffectivelyEnabled);
            }
            finally { window.CloseWithoutSaving(); }
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [AvaloniaFact]
    public void V14BuildPanelDisplaysBatchQuotesWithoutChangingSevenColumnResources()
    {
        var view = Fresh();
        var window = new V14PreviewWindow(view, startTimer: false);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            view.SelectedBuilding = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Base);
            Dispatcher.UIThread.RunJobs();
            Assert.True(window.FindControl<TextBlock>("V14BuildFiveCost")!.IsEffectivelyVisible);
            Assert.StartsWith("+5:", window.FindControl<TextBlock>("V14BuildFiveCost")!.Text);
            Assert.StartsWith("+10:", window.FindControl<TextBlock>("V14BuildTenCost")!.Text);
            var sections = window.FindControl<TabControl>("V14Sections")!;
            sections.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(7, window.FindControl<Grid>("V14ResourceTableHeader")!.ColumnDefinitions.Count);
            sections.SelectedIndex = 0;
            Dispatcher.UIThread.RunJobs();
            view.SelectedBuilding = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Large);
            Dispatcher.UIThread.RunJobs();
            Assert.False(window.FindControl<Button>("V14BuildSelected")!.IsEffectivelyVisible);
            Assert.False(window.FindControl<TextBlock>("V14BuildFiveCost")!.IsEffectivelyVisible);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [Fact]
    public void SchoolMissingPrerequisitesHideBuildButKeepVisibleCostAndExplanation()
    {
        var view = Fresh();
        view.Initialize();
        view.SelectedBuilding = view.Buildings.Single(item => item.Kind == V14BuildingKind.School);
        Assert.False(view.CanBuildSelected);
        Assert.True(view.SelectedBuilding.HasCost);
        Assert.Contains("Costruire prima", view.SelectedBuilding.Requirements);
        Assert.NotEmpty(view.SelectedBuilding.CostLines);
    }

    [Fact]
    public void ExistingSawmillDiagnosesMissingLogsInsteadOfOnlyShowingWorkerCount()
    {
        var view = Fresh();
        view.Initialize();
        var sawmill = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
            item.Tier == ProductionTier.Base);
        Assert.Equal(1, sawmill.Workers);
        Assert.Contains("filiera 0/1 attivi", sawmill.Status);
        Assert.Contains("Tronchi", sawmill.Diagnosis);
        Assert.Contains("Materie prime insufficienti", sawmill.Diagnosis);
        Assert.Contains("prossimo secondo", sawmill.Diagnosis);
    }

    [Fact]
    public void SaturatedBreadStoreExplainsWhichStorageCanActuallyHelp()
    {
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
        var tech = TechnologyCatalog.ProposedTierTree;
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            school, tech, V14SchoolConstructionRules.Candidate).Snapshot();
        var required = new HashSet<string>(StringComparer.Ordinal) { "farm", "well", "mill", "bakery" };
        var families = initial.ProductiveFamilies.Select(family => !required.Contains(family.FamilyId)
            ? family : family with { Base = new TierOccupancy(1, family.FamilyId == "bakery" ? 1 : 0) })
            .ToImmutableArray();
        var capBread = baseline.Catalog.BuildingsById["town-hall"].StoragePerBuilding!["bread"].Units +
            baseline.Catalog.BuildingsById["small-house"].StoragePerBuilding!["bread"].Units +
            baseline.Catalog.BuildingsById["bakery"].StoragePerBuilding!["bread"].Units;
        var checkpoint = initial with
        {
            ProductiveFamilies = families,
            PurchaseCounters = initial.PurchaseCounters.Select(item => required.Contains(item.FamilyId)
                ? item with { Acquired = Math.Max(1, item.Acquired) } : item).ToImmutableArray(),
            Resources = ResourceBundle.Create(("bread", capBread), ("flour", 1m), ("water", 1m))
        };
        var path = Path.Combine(Path.GetTempPath(), "SimRisorse.UI.BreadStore." + Guid.NewGuid().ToString("N") + ".sqlite3");
        try
        {
            var store = new V14SqliteSaveStore(path, baseline, school, tech);
            store.Save(checkpoint);
            var view = new V14PreviewViewModel(baseline, store, school, tech);
            view.Initialize();
            var oven = view.Buildings.Single(item => item.FamilyId == "bakery" &&
                item.Tier == ProductionTier.Base);
            Assert.Contains("Spazio insufficiente per Pane", oven.Diagnosis);
            Assert.Contains("servono 3 posti", oven.Diagnosis);
            Assert.Contains("Deposito alimentare conserva Cibo, non Pane", oven.Diagnosis);
            Assert.Contains("Magazzino", oven.Diagnosis);
            Assert.Contains("filiera 0/1 attivi", oven.Status);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void ConstructionShortageDoesNotHideExistingProductionBlock()
    {
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
        var tech = TechnologyCatalog.ProposedTierTree;
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            school, tech, V14SchoolConstructionRules.Candidate).Snapshot();
        var checkpoint = initial with
        {
            Resources = ResourceBundle.Empty,
            ProductiveFamilies = initial.ProductiveFamilies.Select(item => item.FamilyId == "sawmill"
                ? item with { Large = new TierOccupancy(1, 1) } : item).ToImmutableArray(),
            PurchaseCounters = initial.PurchaseCounters.Select(item => item.FamilyId == "sawmill"
                ? item with { Acquired = 2 } : item).ToImmutableArray(),
            UnlockedTechnologyIds = ["lavorazioni-1"]
        };
        var path = Path.Combine(Path.GetTempPath(), "SimRisorse.UI.BothBlocks." + Guid.NewGuid().ToString("N") + ".sqlite3");
        try
        {
            var store = new V14SqliteSaveStore(path, baseline, school, tech);
            store.Save(checkpoint);
            var view = new V14PreviewViewModel(baseline, store, school, tech);
            view.Initialize();
            var sawmill = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Base);
            Assert.Contains("Risorse insufficienti", sawmill.Diagnosis);
            Assert.Contains("Tronchi", sawmill.Diagnosis);
            Assert.Contains("Filiera Segheria", sawmill.Diagnosis);
            Assert.Contains("filiera 0/2 attivi", sawmill.Status);
            var large = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Large);
            Assert.Equal(1, large.Workers);
            Assert.True(large.IsUnlocked);
            Assert.Contains("0/2 addetti", large.Diagnosis); // Family forecast, not a tier-specific claim.
            view.SelectedBuilding = sawmill;
            Assert.False(view.BuildSelectedCommand.CanExecute(null));
            Assert.False(view.BuildFiveCommand.CanExecute(null));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [AvaloniaFact]
    public void ProductionBlockAppearsInClassicStyleBuildingDiagnosisPanel()
    {
        var view = Fresh();
        var window = new V14PreviewWindow(view, startTimer: false);
        try
        {
            window.Show();
            view.SelectedBuilding = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Base);
            Dispatcher.UIThread.RunJobs();
            var panel = window.FindControl<Border>("V14BuildingDiagnosisPanel")!;
            Assert.True(panel.IsEffectivelyVisible);
            Assert.True(view.SelectedBuilding.HasDiagnosis);
            Assert.Contains("Tronchi", view.SelectedBuilding.Diagnosis);
            Assert.Contains("Tronchi", ((TextBlock)panel.Child!).Text);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [Fact]
    public void NewGameRequiresExplicitConfirmationAndDoesNotTouchTheSaveFile()
    {
        var view = Fresh();
        view.Initialize();
        Assert.False(view.IsNewGameConfirmationVisible);
        view.NewGameCommand.Execute(null);
        Assert.True(view.IsNewGameConfirmationVisible);
        view.CancelNewGameCommand.Execute(null);
        Assert.False(view.IsNewGameConfirmationVisible);
        Assert.False(File.Exists(view.DatabasePath));
    }

    [AvaloniaFact]
    public void SchoolPanelUsesMilitaryStyleProgressAndHasNoDiscardExit()
    {
        var view = Fresh();
        var window = new V14PreviewWindow(view, startTimer: false) { Width = 1040, Height = 740 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var tabs = window.FindControl<TabControl>("V14Sections")!;
            tabs.SelectedIndex = 3;
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window.FindControl<ProgressBar>("V14SchoolProgressBar"));
            Assert.NotNull(window.FindControl<ScrollViewer>("V14SchoolScroll"));
            Assert.NotNull(window.FindControl<ComboBox>("V14SchoolQuantitySelector"));
            Assert.NotNull(window.FindControl<Button>("V14CraftPencils"));
            Assert.False(window.FindControl<Button>("V14CraftPencils")!.IsEffectivelyEnabled);
            Assert.Null(window.FindControl<Button>("V14CloseWithoutSave"));
            Assert.Null(window.FindControl<Button>("V14BuildSchool"));
            Assert.Equal(0d, view.SchoolTrainingPercent);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public void ClassicResourcePageHasAllSevenOriginalColumnsAndMilitaryActions()
    {
        var view = Fresh();
        var window = new V14PreviewWindow(view, startTimer: false) { Width = 1240, Height = 850 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var pages = window.FindControl<TabControl>("V14Sections")!;
            pages.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            var header = window.FindControl<Grid>("V14ResourceTableHeader")!;
            Assert.Equal(7, header.ColumnDefinitions.Count);
            var resourceList = window.FindControl<ListBox>("V14Resources")!;
            Assert.True(resourceList.IsEffectivelyVisible);
            resourceList.SelectedItem = view.Resources.Single(item => item.Id == "bread");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("bread", view.SelectedResource?.Id);
            Assert.Contains("Capienza:", view.SelectedResource!.CapacityBreakdown);
            pages.SelectedIndex = 2;
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(window.FindControl<Button>("EnlistButton"));
            Assert.NotNull(window.FindControl<Button>("CancelQueueButton"));
            Assert.NotNull(window.FindControl<Button>("DischargeButton"));
            Assert.NotNull(window.FindControl<ProgressBar>("MilitaryProgressBar"));
            Assert.False(view.EnlistCommand.CanExecute(null)); // No barracks at game start.
        }
        finally { window.CloseWithoutSaving(); }
    }

    [AvaloniaFact]
    public void LegacyCheckpointCannotBeOverwrittenAndCannotTrapPreviewWindowOpen()
    {
        var path = Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.Parity.Legacy." + Guid.NewGuid().ToString("N") + ".sqlite3");
        new SqliteGameSaveStore(path).Save(GameSession.Create(CivilGrowthScenario.Create()).Snapshot());
        var original = File.ReadAllBytes(path);
        var view = V14PreviewViewModel.CreateForPreview(path);
        var window = new V14PreviewWindow(view, startTimer: false);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.False(view.CanSave);
            window.Close();
            Dispatcher.UIThread.RunJobs();
            Assert.False(window.IsVisible);
            Assert.Equal(original, File.ReadAllBytes(path));
        }
        finally
        {
            if (window.IsVisible) window.CloseWithoutSaving();
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [AvaloniaFact]
    public void LockedTierHasNoBuildButtonButTheBaseTierDoes()
    {
        var view = Fresh();
        var window = new V14PreviewWindow(view, startTimer: false);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            view.SelectedBuilding = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Advanced);
            Dispatcher.UIThread.RunJobs();
            Assert.False(window.FindControl<Button>("V14BuildSelected")!.IsEffectivelyVisible);
            Assert.Contains("Ricerca richiesta", view.SelectedBuilding!.Requirements);
            view.SelectedBuilding = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Base);
            Dispatcher.UIThread.RunJobs();
            Assert.True(window.FindControl<Button>("V14BuildSelected")!.IsEffectivelyVisible);
            Assert.NotNull(window.FindControl<Button>("V14BuildFive"));
            Assert.NotNull(window.FindControl<Button>("V14BuildTen"));
        }
        finally { window.CloseWithoutSaving(); }
    }
    [Fact]
    public void LockedBuildingRowsAreMutedButStillExposePrerequisitesInDetails()
    {
        var view = Fresh();
        view.Initialize();
        var locked = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
            item.Tier == ProductionTier.Large);
        var unlocked = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
            item.Tier == ProductionTier.Base);
        Assert.False(locked.IsUnlocked);
        Assert.Equal(0.48d, locked.RowOpacity);
        Assert.True(unlocked.IsUnlocked);
        Assert.Equal(1d, unlocked.RowOpacity);

        // Selection is independent from construction permission.
        view.SelectedBuilding = locked;
        Assert.Same(locked, view.SelectedBuilding);
        Assert.Contains("Ricerca richiesta", locked.Requirements);
        Assert.True(locked.HasRequirements);
        Assert.False(view.CanBuildSelected);
        Assert.False(view.BuildSelectedCommand.CanExecute(null));
    }

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public void ConstructionAndWorkerRowsHaveSeparateLabelsAndThreePositiveBatchButtons(int width, int height)
    {
        var view = Fresh();
        var window = new V14PreviewWindow(view, startTimer: false) { Width = width, Height = height };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var locked = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Large);
            var buildings = window.FindControl<ListBox>("V14Buildings")!;
            buildings.SelectedItem = locked;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(locked, view.SelectedBuilding);
            Assert.True(buildings.IsEnabled);
            Assert.False(window.FindControl<Grid>("V14BuildingConstructionActions")!.IsEffectivelyVisible);
            Assert.False(window.FindControl<Button>("V14BuildSelected")!.IsEffectivelyVisible);

            view.SelectedBuilding = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Base);
            Dispatcher.UIThread.RunJobs();
            var construction = window.FindControl<Grid>("V14BuildingConstructionActions")!;
            var staffing = window.FindControl<Grid>("V14BuildingWorkerActions")!;
            var main = window.FindControl<Button>("V14BuildSelected")!;
            var five = window.FindControl<Button>("V14BuildFive")!;
            var ten = window.FindControl<Button>("V14BuildTen")!;
            var release = window.FindControl<Button>("V14ReleaseWorker")!;
            var assign = window.FindControl<Button>("V14AssignWorker")!;
            var assignFive = window.FindControl<Button>("V14AssignFiveWorkers")!;
            var assignTen = window.FindControl<Button>("V14AssignTenWorkers")!;
            var buildLabel = window.FindControl<TextBlock>("V14BuildActionLabel")!;
            var workerLabel = window.FindControl<TextBlock>("V14WorkerActionLabel")!;
            Assert.Equal("Costruisci", buildLabel.Text);
            Assert.Equal("Lavoratore", workerLabel.Text);
            Assert.Equal("+1", main.Content);
            Assert.Equal("+5", five.Content);
            Assert.Equal("+10", ten.Content);
            Assert.Equal("−1", release.Content); // No regression: workers can still be released.
            Assert.Equal("+1", assign.Content);
            Assert.Equal("+5", assignFive.Content);
            Assert.Equal("+10", assignTen.Content);
            Assert.Equal(4, construction.ColumnDefinitions.Count);
            Assert.Equal(4, staffing.ColumnDefinitions.Count);
            Assert.True(construction.IsEffectivelyVisible);
            Assert.True(staffing.IsEffectivelyVisible);
            Assert.True(construction.Bounds.Width > 300d);
            Assert.True(staffing.Bounds.Width > 300d);
            Assert.Same(construction, main.Parent);
            Assert.Same(construction, five.Parent);
            Assert.Same(construction, ten.Parent);
            Assert.Same(staffing, assign.Parent);
            Assert.Same(staffing, assignFive.Parent);
            Assert.Same(staffing, assignTen.Parent);
            Assert.Equal(1, Grid.GetColumn(main));
            Assert.Equal(2, Grid.GetColumn(five));
            Assert.Equal(3, Grid.GetColumn(ten));
            Assert.Equal(1, Grid.GetColumn(assign));
            Assert.Equal(2, Grid.GetColumn(assignFive));
            Assert.Equal(3, Grid.GetColumn(assignTen));
            Assert.Same(view.AssignSelectedWorkerCommand, assign.Command);
            Assert.Same(view.AssignFiveSelectedWorkersCommand, assignFive.Command);
            Assert.Same(view.AssignTenSelectedWorkersCommand, assignTen.Command);
            Assert.True(main.IsEffectivelyEnabled);
            Assert.False(five.IsEnabled); // Initial stock covers +1, not +5.
            Assert.False(ten.IsEnabled);
            Assert.False(assign.IsEnabled); // Initial sawmill has its only job occupied.
            Assert.False(assignFive.IsEnabled);
            Assert.False(assignTen.IsEnabled);
        }
        finally { window.CloseWithoutSaving(); }
    }

    [Fact]
    public void WorkerBatchUsesAvailablePeopleAndWorkplacesWithoutPartialAssignments()
    {
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m),
            120, 1, 2, 100, pencilsPerStudent: 2);
        var tech = TechnologyCatalog.ProposedTierTree;
        var original = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            school, tech, V14SchoolConstructionRules.Candidate).Snapshot();
        // The base campaign starts with one staffed sawmill and five free inhabitants.
        // Add five unstaffed buildings; keep counters consistent with the snapshot.
        var state = original with
        {
            ProductiveFamilies = original.ProductiveFamilies.Select(item => item.FamilyId == "sawmill"
                ? item with { Base = new TierOccupancy(6, 1) } : item).ToImmutableArray(),
            PurchaseCounters = original.PurchaseCounters.Select(item => item.FamilyId == "sawmill"
                ? item with { Acquired = 6 } : item).ToImmutableArray()
        };
        var path = Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.WorkerBatch." + Guid.NewGuid().ToString("N") + ".sqlite3");
        try
        {
            var store = new V14SqliteSaveStore(path, baseline, school, tech);
            store.Save(state);
            var view = new V14PreviewViewModel(baseline, store, school, tech);
            view.Initialize();
            view.SelectedBuilding = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Base);
            Assert.True(view.CanAssignSelectedFive);
            Assert.False(view.CanAssignSelectedTen);
            Assert.True(view.AssignFiveSelectedWorkersCommand.CanExecute(null));
            Assert.False(view.AssignTenSelectedWorkersCommand.CanExecute(null));
            Assert.Equal(1, view.SelectedBuilding.Workers);
            view.AssignTenSelectedWorkersCommand.Execute(null); // Rejected without a partial commit.
            Assert.Equal(1, view.SelectedBuilding.Workers);
            view.AssignFiveSelectedWorkersCommand.Execute(null);
            Assert.Equal(6, view.SelectedBuilding.Workers);
            Assert.Equal(0, view.AvailablePopulation);
            Assert.False(view.CanAssignSelectedOne);
            Assert.False(view.CanAssignSelectedFive);
            Assert.False(view.CanAssignSelectedTen);
            view.ReleaseSelectedWorkerCommand.Execute(null);
            Assert.Equal(5, view.SelectedBuilding.Workers);
            Assert.True(view.CanAssignSelectedOne);
            Assert.False(view.CanAssignSelectedFive);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [AvaloniaFact]
    public void MissingResourcesMakeBuildButtonVisiblyDisabledWithoutHidingTheDetails()
    {
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
        var technologies = TechnologyCatalog.ProposedTierTree;
        var checkpoint = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            school, technologies, V14SchoolConstructionRules.Candidate).Snapshot() with
        {
            Resources = ResourceBundle.Empty
        };
        var path = Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.DisabledBuild." + Guid.NewGuid().ToString("N") + ".sqlite3");
        try
        {
            var store = new V14SqliteSaveStore(path, baseline, school, technologies);
            store.Save(checkpoint);
            var view = new V14PreviewViewModel(baseline, store, school, technologies);
            view.Initialize();
            view.SelectedBuilding = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Base);
            Assert.True(view.CanBuildSelected); // Unlocked and shown.
            Assert.False(view.CanAffordSelected);
            Assert.False(view.CanAffordFive);
            Assert.False(view.CanAffordTen);
            Assert.Contains("Risorse insufficienti", view.SelectedBuilding.Diagnosis);
            var window = new V14PreviewWindow(view, startTimer: false);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                var build = window.FindControl<Button>("V14BuildSelected")!;
                Assert.Equal("+1", build.Content);
                Assert.True(build.IsEffectivelyVisible);
                Assert.Same(view.BuildSelectedCommand, build.Command);
                Assert.False(build.IsEnabled); // Explicit visual state, not just CanExecute.
                Assert.False(build.IsEffectivelyEnabled);
                Assert.Equal(Color.Parse("#B8C2BC"),
                    Assert.IsAssignableFrom<ISolidColorBrush>(build.Background).Color);
                Assert.False(window.FindControl<Button>("V14BuildFive")!.IsEnabled);
                Assert.False(window.FindControl<Button>("V14BuildTen")!.IsEnabled);
                Assert.True(window.FindControl<ItemsControl>("V14BuildingCostLines")!.IsEffectivelyVisible);
            }
            finally { window.CloseWithoutSaving(); }
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [AvaloniaFact]
    public void ResearchPurchaseButtonIsGreyUntilPointsAreAvailableAndNeverMovesOnHover()
    {
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m),
            120, 1, 2, 100, pencilsPerStudent: 2);
        var catalog = TechnologyCatalog.ProposedTierTree;
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            school, catalog, V14SchoolConstructionRules.Candidate).Snapshot();
        var folder = Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.ResearchDisabled." + Guid.NewGuid().ToString("N"));
        var path = Path.Combine(folder, "preview-v14.sqlite3");
        try
        {
            var store = new V14SqliteSaveStore(path, baseline, school, catalog);
            store.Save(initial with { ResearchBalanceMilliPoints = 99_999L });
            var view = new V14PreviewViewModel(baseline, store, school, catalog);
            var window = new V14PreviewWindow(view, startTimer: false);
            try
            {
                window.Show();
                window.FindControl<TabControl>("V14Sections")!.SelectedIndex = 4;
                view.SelectedTechnology = view.Technologies.Single(item => item.Id == "lavorazioni-1");
                Dispatcher.UIThread.RunJobs();
                var button = window.FindControl<Button>("V14BuyTechnology")!;
                Assert.True(button.IsEffectivelyVisible);
                Assert.Same(view.BuyTechnologyCommand, button.Command);
                Assert.False(button.IsEnabled); // Explicit visual state as well as ICommand guard.
                Assert.False(button.IsEffectivelyEnabled);
                Assert.Equal(Color.Parse("#B8C2BC"),
                    Assert.IsAssignableFrom<ISolidColorBrush>(button.Background).Color);
                window.MouseMove(button.TranslatePoint(
                    new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value);
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(Color.Parse("#B8C2BC"),
                    Assert.IsAssignableFrom<ISolidColorBrush>(button.Background).Color);
                window.MouseMove(new Point(0, 0));
                Dispatcher.UIThread.RunJobs();

                store.Save(initial with { ResearchBalanceMilliPoints = 100_000L });
                Assert.True(view.Load());
                Dispatcher.UIThread.RunJobs();
                Assert.True(button.IsEnabled);
                Assert.True(button.IsEffectivelyEnabled);
                Assert.Equal(Color.Parse("#176858"),
                    Assert.IsAssignableFrom<ISolidColorBrush>(button.Background).Color);
                var position = button.TranslatePoint(default, window)!.Value;
                var size = button.Bounds.Size;
                window.MouseMove(button.TranslatePoint(
                    new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value);
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(Color.Parse("#E6D5AC"),
                    Assert.IsAssignableFrom<ISolidColorBrush>(button.Background).Color);
                Assert.Equal(position, button.TranslatePoint(default, window)!.Value);
                Assert.Equal(size, button.Bounds.Size);
                view.BuyTechnologyCommand.Execute(null);
                Dispatcher.UIThread.RunJobs();
                Assert.False(button.IsEnabled);
                Assert.False(button.IsEffectivelyEnabled);
            }
            finally { window.CloseWithoutSaving(); }
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true); }
    }

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public void V14BuildHoverIsDistinctAndDoesNotMoveTheActions(int width, int height)
    {
        var view = Fresh();
        var window = new V14PreviewWindow(view, startTimer: false) { Width = width, Height = height };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            view.SelectedBuilding = view.Buildings.Single(item => item.FamilyId == "sawmill" &&
                item.Tier == ProductionTier.Base);
            Dispatcher.UIThread.RunJobs();
            var main = window.FindControl<Button>("V14BuildSelected")!;
            var buildingRow = window.FindControl<Grid>("V14BuildingConstructionActions")!;
            var workerRow = window.FindControl<Grid>("V14BuildingWorkerActions")!;
            Assert.True(main.IsEffectivelyEnabled);
            Assert.True(buildingRow.Bounds.Width > 300d, "Three construction controls need their dedicated row.");
            Assert.Equal(Color.Parse("#176858"),
                Assert.IsAssignableFrom<ISolidColorBrush>(main.Background).Color);
            Control[] stable = [main, buildingRow, workerRow];
            var before = stable.Select(control =>
                (Position: control.TranslatePoint(default, window)!.Value, Size: control.Bounds.Size)).ToArray();
            window.MouseMove(main.TranslatePoint(
                new Point(main.Bounds.Width / 2, main.Bounds.Height / 2), window)!.Value);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(Color.Parse("#E6D5AC"),
                Assert.IsAssignableFrom<ISolidColorBrush>(main.Background).Color);
            var presenter = main.GetVisualDescendants().OfType<Avalonia.Controls.Presenters.ContentPresenter>()
                .Single(item => item.Name == "PART_ContentPresenter");
            Assert.Equal(Color.Parse("#263349"),
                Assert.IsAssignableFrom<ISolidColorBrush>(presenter.Foreground).Color);
            Assert.Equal(before, stable.Select(control =>
                (Position: control.TranslatePoint(default, window)!.Value, Size: control.Bounds.Size)).ToArray());
        }
        finally { window.CloseWithoutSaving(); }
    }

}
