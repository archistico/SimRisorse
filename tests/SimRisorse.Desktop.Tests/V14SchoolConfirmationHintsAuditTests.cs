using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Desktop.Views;
using Xunit;

namespace SimRisorse.Desktop.Tests;

/// <summary>
/// G1-D39: Scuola hints must agree with the same new-game confirmation
/// that temporarily disables enrolling, cancelling and pencil crafting.
/// </summary>
public sealed class V14SchoolConfirmationHintsAuditTests
{
    private static string TemporarySavePath() => Path.Combine(Path.GetTempPath(),
        "SimRisorse.UI.SchoolConfirmation." + Guid.NewGuid().ToString("N") + ".sqlite3");

    [Fact]
    public void AllSchoolActionHintsReportConfirmationAndRestoreAfterCancel()
    {
        var view = V14PreviewViewModel.CreateForPreview(TemporarySavePath());
        view.Initialize();
        var craftHint = view.SchoolCraftHint;
        var enrollHint = view.SchoolEnrollmentHint;
        var cancelHint = view.SchoolCancelHint;

        view.NewGameCommand.Execute(null);
        Assert.False(view.CraftPencilsCommand.CanExecute(null));
        Assert.False(view.EnrollCommand.CanExecute(null));
        Assert.False(view.CancelStudentCommand.CanExecute(null));
        Assert.Contains("sospesa", view.SchoolCraftHint);
        Assert.Contains("sospese", view.SchoolEnrollmentHint);
        Assert.Contains("sospeso", view.SchoolCancelHint);
        Assert.Contains("conferma di nuova partita", view.SchoolResearchHint);

        view.CancelNewGameCommand.Execute(null);
        Assert.Equal(craftHint, view.SchoolCraftHint);
        Assert.Equal(enrollHint, view.SchoolEnrollmentHint);
        Assert.Equal(cancelHint, view.SchoolCancelHint);
    }

    [AvaloniaFact]
    public void SchoolCraftHintAndButtonReflectConfirmationInWindow()
    {
        var path = TemporarySavePath();
        V14PreviewWindow? window = null;
        try
        {
            var view = V14PreviewViewModel.CreateForPreview(path);
            window = new V14PreviewWindow(view, startTimer: false) { Width = 1040, Height = 740 };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var tabs = window.FindControl<TabControl>("V14Sections")!;
            tabs.SelectedIndex = 3; // Scuola
            Dispatcher.UIThread.RunJobs();
            var button = window.FindControl<Button>("V14CraftPencils")!;
            var hint = window.FindControl<TextBlock>("V14CraftPencilsHint")!;
            Assert.Contains("Costruisci prima una Scuola", hint.Text);
            Assert.False(button.IsEffectivelyEnabled);

            view.NewGameCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.Contains("Produzione Matite sospesa", hint.Text);
            Assert.False(button.IsEffectivelyEnabled);

            view.CancelNewGameCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.Contains("Costruisci prima una Scuola", hint.Text);
            Assert.False(button.IsEffectivelyEnabled);
        }
        finally
        {
            if (window is { IsVisible: true }) window.CloseWithoutSaving();
        }
    }
}
