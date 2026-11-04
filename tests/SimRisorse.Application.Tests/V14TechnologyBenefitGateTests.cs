using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class V14TechnologyBenefitGateTests
{
    [Fact]
    public void StartingAV14SessionRejectsPurchasableResearchThatCannotUnlockAnyTier()
    {
        var scenario = CivilGrowthScenario.Create();
        var catalog = TechnologyCatalog.ProposedTierTree;
        var invalid = new TechnologyCatalog(catalog.Technologies.Select(item => item.Id != "logistica-1"
            ? item
            : new TechnologyDefinition(item.Id, item.Name, item.Cost, true,
                item.Prerequisites, item.RequiredBuildings, item.UnlockedFamilies)));
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m),
            120, 1, 2, 100, pencilsPerStudent: 2);
        Assert.Throws<ArgumentException>(() => V14CampaignSession.FromV13(scenario,
            GameSession.Create(scenario).Snapshot(), school, invalid, V14SchoolConstructionRules.Candidate));
    }
}
