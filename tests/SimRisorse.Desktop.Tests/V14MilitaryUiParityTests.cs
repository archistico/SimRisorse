using System.Collections.Immutable;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Desktop.Tests;

/// <summary>
/// G1-D34: the military hints must agree with the quantity, the command state,
/// and a pending new-game confirmation, without touching v13 or military rules.
/// </summary>
public sealed class V14MilitaryUiParityTests
{
    private static V14PreviewViewModel Fresh() =>
        V14PreviewViewModel.CreateForPreview(Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.MilitaryHints." + Guid.NewGuid().ToString("N") + ".sqlite3"));

    private static V14PreviewViewModel WithBarracksAndFiveSwords(string path)
    {
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m),
            120, 1, 2, 100, pencilsPerStudent: 2);
        var technologies = TechnologyCatalog.ProposedTierTree;
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            school, technologies, V14SchoolConstructionRules.Candidate).Snapshot();
        var checkpoint = initial with
        {
            Resources = initial.Resources + ResourceBundle.Create(("swords", 5m)),
            OtherBuildings = initial.OtherBuildings.Select(item => item.BuildingId == "barracks"
                ? item with { Count = 1 } : item).ToImmutableArray(),
            PurchaseCounters = initial.PurchaseCounters.Select(item => item.FamilyId == "barracks"
                ? item with { Acquired = 1 } : item).ToImmutableArray()
        };
        var store = new V14SqliteSaveStore(path, baseline, school, technologies);
        store.Save(checkpoint);
        var view = new V14PreviewViewModel(baseline, store, school, technologies);
        view.Initialize();
        return view;
    }

    [Fact]
    public void MilitaryHintsExplainTemporaryNewGameBlockAndRestoreAfterCancel()
    {
        var view = Fresh();
        view.Initialize();
        Assert.Contains("Caserma", view.MilitaryRecruitmentHint);
        Assert.Contains("Reclute annullabili insufficienti per −1", view.MilitaryCancelHint);
        view.NewGameCommand.Execute(null);
        Assert.False(view.EnlistCommand.CanExecute(null));
        Assert.False(view.CancelRecruitsCommand.CanExecute(null));
        Assert.Contains("sospeso", view.MilitaryRecruitmentHint);
        Assert.Contains("sospeso", view.MilitaryCancelHint);
        Assert.Contains("sospesi", view.MilitaryLimitsNote);
        view.CancelNewGameCommand.Execute(null);
        Assert.Contains("Caserma", view.MilitaryRecruitmentHint);
        Assert.DoesNotContain("sospesi", view.MilitaryLimitsNote);
    }

    [Fact]
    public void MilitaryBatchHintsReportActualSwordAndRecruitsShortages()
    {
        var directory = Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.MilitaryBatch." + Guid.NewGuid().ToString("N"));
        try
        {
            var view = WithBarracksAndFiveSwords(Path.Combine(directory, "preview-v14.sqlite3"));
            view.MilitaryQuantity = 5;
            Assert.False(view.EnlistCommand.CanExecute(null));
            Assert.Contains("Spade insufficienti per +5", view.MilitaryRecruitmentHint);
            Assert.Contains("disponibili 5", view.MilitaryRecruitmentHint);
            Assert.Contains("necessarie 25", view.MilitaryRecruitmentHint);

            view.MilitaryQuantity = 1;
            Assert.Contains("Costo per 1 recluta: 5 Spade", view.MilitaryWeaponsStatus);
            Assert.True(view.EnlistCommand.CanExecute(null));
            Assert.Contains("Arruolamento possibile", view.MilitaryRecruitmentHint);
            view.EnlistCommand.Execute(null);
            Assert.Equal(1, view.MilitaryCancellable);
            Assert.True(view.CancelRecruitsCommand.CanExecute(null));

            view.MilitaryQuantity = 5;
            Assert.False(view.CancelRecruitsCommand.CanExecute(null));
            Assert.Contains("Reclute annullabili insufficienti per −5", view.MilitaryCancelHint);
            Assert.Contains("disponibili 1", view.MilitaryCancelHint);
            view.MilitaryQuantity = 1;
            view.CancelRecruitsCommand.Execute(null);
            Assert.Equal(0, view.MilitaryCancellable);
            Assert.Equal(5m, view.AvailableSwords);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
    }

    [AvaloniaFact]
    public void MilitaryPanelUpdatesHintsAndEnabledStateWhenQuantityAndConfirmationChange()
    {
        var directory = Path.Combine(Path.GetTempPath(),
            "SimRisorse.UI.MilitaryVisual." + Guid.NewGuid().ToString("N"));
        V14PreviewWindow? window = null;
        try
        {
            var view = WithBarracksAndFiveSwords(Path.Combine(directory, "preview-v14.sqlite3"));
            window = new V14PreviewWindow(view, startTimer: false) { Width = 1040, Height = 740 };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.FindControl<TabControl>("V14Sections")!.SelectedIndex = 2;
            Dispatcher.UIThread.RunJobs();
            var enlist = window.FindControl<Button>("EnlistButton")!;
            var hint = window.FindControl<TextBlock>("MilitaryRecruitmentHintText")!;
            Assert.True(enlist.IsEffectivelyEnabled);
            Assert.Contains("Arruolamento possibile", hint.Text);

            view.NewGameCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.False(enlist.IsEffectivelyEnabled);
            Assert.Contains("sospeso", hint.Text);
            view.CancelNewGameCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.True(enlist.IsEffectivelyEnabled);
            Assert.Contains("Arruolamento possibile", hint.Text);

            view.MilitaryQuantity = 5;
            Dispatcher.UIThread.RunJobs();
            Assert.False(enlist.IsEffectivelyEnabled);
            Assert.Contains("Spade insufficienti per +5", hint.Text);
        }
        finally
        {
            if (window is { IsVisible: true }) window.CloseWithoutSaving();
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }
}
