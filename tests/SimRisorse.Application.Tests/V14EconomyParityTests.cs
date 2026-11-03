using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>UI projection and enlisted population use exactly the validated v14 simulation state.</summary>
public sealed class V14EconomyParityTests
{
    private static V14CampaignSession Fresh()
    {
        var baseline = CivilGrowthScenario.Create();
        return V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100),
            TechnologyCatalog.ProposedTierTree, V14SchoolConstructionRules.Candidate);
    }

    [Fact]
    public void ForecastIsReadOnlyAndContainsTheSixteenLegacyFlowsPlusPencils()
    {
        var session = Fresh();
        var before = session.Snapshot();
        var projection = session.ForecastEconomy();
        Assert.Equal(17, projection.Resources.Length);
        Assert.Contains(projection.Resources, item => item.Id == "pencils" && item.ProductionPerSecond == 0m);
        Assert.Contains(projection.Resources, item => item.Id == "food");
        Assert.All(projection.Resources, item =>
        {
            Assert.True(item.ProductionPerSecond >= 0m);
            Assert.True(item.ConsumptionPerSecond >= 0m);
        });
        Assert.Same(before, session.Snapshot());
        var second = session.ForecastEconomy();
        Assert.True(projection.Resources.SequenceEqual(second.Resources));
        Assert.Equal(projection.Population, second.Population);
    }

    [Fact]
    public void IndividualRecruitmentAndCancellationPreserveStockAndFreePopulation()
    {
        var baseline = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
        var technologies = TechnologyCatalog.ProposedTierTree;
        var original = Fresh().Snapshot();
        var prepared = original with
        {
            Resources = original.Resources + ResourceBundle.Create(("swords", 10m)),
            OtherBuildings = original.OtherBuildings.Select(item => item.BuildingId == "barracks"
                ? item with { Count = 1 } : item).ToImmutableArray(),
            PurchaseCounters = original.PurchaseCounters.Select(item => item.FamilyId == "barracks"
                ? item with { Acquired = 1 } : item).ToImmutableArray()
        };
        var session = V14CampaignSession.Restore(baseline, prepared, school, technologies,
            V14SchoolConstructionRules.Candidate);
        var free = session.AvailablePopulation;
        Assert.Equal(GameError.None, session.Enlist(1).Error);
        Assert.Equal(free - 1, session.AvailablePopulation);
        Assert.Equal(5m, session.Snapshot().Resources["swords"].Units);
        Assert.Equal(GameError.None, session.CancelRecruits(1).Error);
        Assert.Equal(free, session.AvailablePopulation);
        Assert.Equal(10m, session.Snapshot().Resources["swords"].Units);
        Assert.Equal(0L, session.Snapshot().Military.EnlistedPopulation);
    }
}
