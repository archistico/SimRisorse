using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Desktop.Tests;

/// <summary>
/// G1-D36: upgrade actions must use the same explicit disabled/hover states
/// as construction actions, while remaining visible when only materials are missing.
/// </summary>
public sealed class V14BuildingUpgradeAuditTests
{
    private static readonly SchoolResearchRules School = new("food", ResourceAmount.FromUnits(10m),
        120, 1, 2, 100, pencilsPerStudent: 2);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;

    [AvaloniaTheory]
    [InlineData(1040, 740)]
    [InlineData(1240, 850)]
    public void UpgradeButtonsAreVisibleButGreyAndStayInPlaceWithoutMaterials(int width, int height)
    {
        var baseline = CivilGrowthScenario.Create();
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            School, Technologies, V14SchoolConstructionRules.Candidate).Snapshot();
        var checkpoint = initial with
        {
            Resources = ResourceBundle.Empty,
            UnlockedTechnologyIds = ["lavorazioni-1", "lavorazioni-2"]
        };
        var directory = Path.Combine(Path.GetTempPath(), "SimRisorse.UI.UpgradeAudit." + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new V14SqliteSaveStore(Path.Combine(directory, "preview-v14.sqlite3"),
                baseline, School, Technologies);
            store.Save(checkpoint);
            var vm = new V14PreviewViewModel(baseline, store, School, Technologies);
            var window = new V14PreviewWindow(vm, startTimer: false) { Width = width, Height = height };
            try
            {
                window.Show();
                vm.SelectedBuilding = vm.Buildings.Single(item => item.FamilyId == "sawmill" &&
                    item.Tier == ProductionTier.Base);
                Dispatcher.UIThread.RunJobs();
                var next = window.FindControl<Button>("V14UpgradeNext")!;
                var direct = window.FindControl<Button>("V14UpgradeAdvanced")!;
                Assert.True(vm.ShowUpgradeNext);
                Assert.True(vm.ShowUpgradeDirectlyToAdvanced);
                Assert.Contains("Mancano:", vm.NextUpgradeCost);
                Assert.Contains("Mancano:", vm.DirectUpgradeCost);
                Assert.Same(vm.UpgradeSelectedCommand, next.Command);
                Assert.Same(vm.UpgradeSelectedToAdvancedCommand, direct.Command);
                foreach (var button in new[] { next, direct })
                {
                    Assert.True(button.IsEffectivelyVisible);
                    Assert.False(button.IsEnabled); // Explicit UI state, not only ICommand.CanExecute.
                    Assert.False(button.IsEffectivelyEnabled);
                    Assert.Equal(Color.Parse("#B8C2BC"),
                        Assert.IsAssignableFrom<ISolidColorBrush>(button.Background).Color);
                    var before = (button.TranslatePoint(default, window)!.Value, button.Bounds.Size);
                    window.MouseMove(button.TranslatePoint(
                        new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value);
                    Dispatcher.UIThread.RunJobs();
                    Assert.Equal(Color.Parse("#B8C2BC"),
                        Assert.IsAssignableFrom<ISolidColorBrush>(button.Background).Color);
                    Assert.Equal(before, (button.TranslatePoint(default, window)!.Value, button.Bounds.Size));
                }
                var selection = vm.SelectedBuilding;
                vm.UpgradeSelectedCommand.Execute(null);
                vm.UpgradeSelectedToAdvancedCommand.Execute(null);
                Assert.Same(selection, vm.SelectedBuilding);
                Assert.Equal(1, selection!.Count);
                Assert.Equal(0, vm.Buildings.Single(item => item.FamilyId == "sawmill" &&
                    item.Tier == ProductionTier.Large).Count);
            }
            finally { window.CloseWithoutSaving(); }
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
    }

    [AvaloniaFact]
    public void UpgradeBecomesGreenOnlyWithFullQuoteAndTransfersOneBuildingWithoutChangingSelection()
    {
        var baseline = CivilGrowthScenario.Create();
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            School, Technologies, V14SchoolConstructionRules.Candidate).Snapshot();
        var purchased = initial.PurchaseCounters.Single(item => item.FamilyId == "sawmill").Acquired;
        var rules = V14TierConstructionRules.Candidate;
        var quote = ProgressiveConstructionPricing.QuoteUpgrade(
            baseline.Catalog.BuildingsById["sawmill"].Cost, 1m, rules.LargeMultiplier,
            rules.GrowthFactor, purchased);
        var checkpoint = initial with
        {
            Resources = ResourceBundle.Empty,
            UnlockedTechnologyIds = ["lavorazioni-1", "lavorazioni-2"]
        };
        var directory = Path.Combine(Path.GetTempPath(), "SimRisorse.UI.UpgradeAvailable." + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new V14SqliteSaveStore(Path.Combine(directory, "preview-v14.sqlite3"),
                baseline, School, Technologies);
            store.Save(checkpoint);
            var vm = new V14PreviewViewModel(baseline, store, School, Technologies);
            var window = new V14PreviewWindow(vm, startTimer: false);
            try
            {
                window.Show();
                var selected = vm.Buildings.Single(item => item.FamilyId == "sawmill" &&
                    item.Tier == ProductionTier.Base);
                vm.SelectedBuilding = selected;
                Dispatcher.UIThread.RunJobs();
                var button = window.FindControl<Button>("V14UpgradeNext")!;
                Assert.False(button.IsEnabled);

                // Reload the same colony with exactly the upgrade quotation, no spare materials.
                store.Save(checkpoint with { Resources = quote });
                Assert.True(vm.Load());
                Dispatcher.UIThread.RunJobs();
                Assert.Same(selected, vm.SelectedBuilding);
                Assert.True(vm.CanUpgradeNext);
                Assert.True(button.IsEnabled);
                Assert.True(button.IsEffectivelyEnabled);
                Assert.Equal(Color.Parse("#176858"),
                    Assert.IsAssignableFrom<ISolidColorBrush>(button.Background).Color);
                var before = (button.TranslatePoint(default, window)!.Value, button.Bounds.Size);
                window.MouseMove(button.TranslatePoint(
                    new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value);
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(Color.Parse("#E6D5AC"),
                    Assert.IsAssignableFrom<ISolidColorBrush>(button.Background).Color);
                Assert.Equal(before, (button.TranslatePoint(default, window)!.Value, button.Bounds.Size));

                vm.UpgradeSelectedCommand.Execute(null);
                Dispatcher.UIThread.RunJobs();
                Assert.Same(selected, vm.SelectedBuilding);
                Assert.Equal(0, selected.Count);
                Assert.Equal(1, vm.Buildings.Single(item => item.FamilyId == "sawmill" &&
                    item.Tier == ProductionTier.Large).Count);
                Assert.False(button.IsEnabled);
                Assert.False(button.IsEffectivelyVisible);
                Assert.Equal("0", vm.Resources.Single(item => item.Id == "planks").QuantityRounded);
            }
            finally { window.CloseWithoutSaving(); }
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
    }
}
