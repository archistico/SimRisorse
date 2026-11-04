using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Desktop.Presentation;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Desktop.Tests;

/// <summary>P1-D47: the Scuola workshop display and enabled state must match the session.</summary>
public sealed class V14PencilWorkshopPreviewTests
{
    private static readonly SchoolResearchRules Rules = new("food", ResourceAmount.FromUnits(10m),
        120, 1, 2, 100, pencilsPerStudent: 2);

    private static (string Folder, V14PreviewViewModel View) CreateWithStocks(
        decimal planks, decimal coal, decimal pencils)
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-p1d47-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var baseline = CivilGrowthScenario.Create();
        var technology = TechnologyCatalog.ProposedTierTree;
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            Rules, technology, V14SchoolConstructionRules.Candidate).Snapshot();
        var stocked = initial.Resources;
        var originalPlanks = stocked["planks"].Units;
        if (originalPlanks > 0m)
            stocked -= ResourceBundle.Create(("planks", originalPlanks));
        stocked += ResourceBundle.Create(("planks", planks), ("coal", coal), ("pencils", pencils));
        var candidate = initial with
        {
            SchoolCount = 1,
            PurchaseCounters = initial.PurchaseCounters.Select(item => item.FamilyId == "school"
                ? item with { Acquired = 1 } : item).ToImmutableArray(),
            Resources = stocked
        };
        var store = new V14SqliteSaveStore(Path.Combine(folder, "v14.sqlite3"), baseline, Rules, technology);
        store.Save(candidate);
        var view = new V14PreviewViewModel(baseline, store, Rules, technology);
        view.Initialize();
        return (folder, view);
    }

    [Fact]
    public void FiveBatchHintReportsBothMissingMaterialsAndCommandRemainsDisabled()
    {
        var (folder, view) = CreateWithStocks(0m, 3m, 0m);
        try
        {
            view.SchoolQuantity = 5;
            Assert.False(view.SchoolCanCraft);
            Assert.False(view.CraftPencilsCommand.CanExecute(null));
            Assert.Contains("Legno lavorato insufficiente: 0/5", view.SchoolCraftHint);
            Assert.Contains("Carbone insufficiente: 3/5", view.SchoolCraftHint);
            Assert.Contains("mancano 2", view.SchoolCraftHint);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public void StorageHintMatchesBatchAndOneBatchCanStillBeCrafted()
    {
        var baseline = CivilGrowthScenario.Create();
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            Rules, TechnologyCatalog.ProposedTierTree, V14SchoolConstructionRules.Candidate).Snapshot();
        var capacity = V14SchoolSupplies.Capacity(baseline, initial with { SchoolCount = 1 }).Units;
        var (folder, view) = CreateWithStocks(20m, 20m, capacity - 10m);
        try
        {
            view.SchoolQuantity = 5;
            Assert.False(view.SchoolCanCraft);
            Assert.False(view.CraftPencilsCommand.CanExecute(null));
            Assert.Contains("Spazio Matite insufficiente", view.SchoolCraftHint);
            Assert.Contains("10 libero", view.SchoolCraftHint);
            Assert.Contains("20 necessario", view.SchoolCraftHint);

            view.SchoolQuantity = 1;
            Assert.True(view.SchoolCanCraft);
            Assert.True(view.CraftPencilsCommand.CanExecute(null));
            Assert.Contains("Puoi produrre 4 Matite", view.SchoolCraftHint);
            view.CraftPencilsCommand.Execute(null);
            Assert.Equal(capacity - 6m, view.SchoolPencils);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}
