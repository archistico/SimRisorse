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

/// <summary>P1-D46: progress shown for each occupied School seat must match saved and advanced state.</summary>
public sealed class V14SchoolIndividualProgressTests
{
    private static (V14PreviewViewModel View, string Directory) Prepared()
    {
        var directory = Path.Combine(Path.GetTempPath(), "SimRisorse.P1.D46." + Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "campaign-v14.sqlite3");
        var baseline = CivilGrowthScenario.Create();
        var rules = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m),
            120, 1, 2, 100, pencilsPerStudent: 2);
        var technologies = TechnologyCatalog.ProposedTierTree;
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            rules, technologies, V14SchoolConstructionRules.Candidate).Snapshot();
        var saved = initial with
        {
            SchoolCount = 2,
            SchoolResearch = new SchoolResearchState(1, [90, 30], 0, 0),
            PurchaseCounters = initial.PurchaseCounters.Select(item => item.FamilyId == "school"
                ? item with { Acquired = 2 } : item).ToImmutableArray()
        };
        var store = new V14SqliteSaveStore(path, baseline, rules, technologies);
        store.Save(saved);
        var view = new V14PreviewViewModel(baseline, store, rules, technologies);
        view.Initialize();
        return (view, directory);
    }

    [Fact]
    public void DetailShowsBothStudentsAndRefreshesWhenFirstGraduates()
    {
        var (view, directory) = Prepared();
        try
        {
            Assert.Equal("1", view.SchoolWaitingLabel);
            Assert.Equal("2", view.SchoolTrainingLabel);
            Assert.Contains("Allievo 1: 90/120 s · 00:30 rimanenti", view.SchoolTrainingDetails);
            Assert.Contains("Allievo 2: 30/120 s · 01:30 rimanenti", view.SchoolTrainingDetails);
            Assert.Equal(75d, view.SchoolTrainingPercent);

            view.TogglePauseCommand.Execute(null);
            view.Advance(TimeSpan.FromSeconds(30));
            Assert.Equal("0", view.SchoolWaitingLabel);
            Assert.Equal("2", view.SchoolTrainingLabel);
            Assert.Equal("1", view.SchoolReadyLabel);
            Assert.Contains("Allievo 1: 60/120 s · 01:00 rimanenti", view.SchoolTrainingDetails);
            Assert.Contains("Allievo 2: 0/120 s · 02:00 rimanenti", view.SchoolTrainingDetails);
            Assert.Equal(50d, view.SchoolTrainingPercent);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
    }

    [AvaloniaFact]
    public void SchoolPanelBindsIndividualProgressWithoutMovingTrainingActions()
    {
        var (view, directory) = Prepared();
        V14PreviewWindow? window = null;
        try
        {
            window = new V14PreviewWindow(view, startTimer: false) { Width = 1040, Height = 740 };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.FindControl<TabControl>("V14Sections")!.SelectedIndex = 3;
            Dispatcher.UIThread.RunJobs();
            var details = window.FindControl<TextBlock>("V14SchoolIndividualProgress")!;
            Assert.Contains("Allievo 1: 90/120 s", details.Text);
            Assert.Contains("Allievo 2: 30/120 s", details.Text);
            Assert.True(window.FindControl<Button>("V14EnrollStudent")!.IsEffectivelyVisible);
            Assert.True(window.FindControl<Button>("V14AssignResearcher")!.IsEffectivelyVisible);
        }
        finally
        {
            if (window is { IsVisible: true }) window.CloseWithoutSaving();
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }
}
