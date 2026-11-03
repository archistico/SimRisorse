using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>E3b/3e candidate non-productive building commands: v13 remains untouched.</summary>
public sealed class V14CivicConstructionTests
{
    private static readonly SchoolResearchRules School = new("food", ResourceAmount.FromUnits(10m),
        120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;

    private static V14CampaignSession Session(bool funded = false,
        V14TierConstructionRules? pricing = null)
    {
        var baseline = CivilGrowthScenario.Create();
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            School, Technologies, V14SchoolConstructionRules.Candidate).Snapshot();
        return V14CampaignSession.Restore(baseline, funded ? initial with
        {
            Resources = initial.Resources + ResourceBundle.Create(("planks", 650m), ("stone", 520m))
        } : initial, School, Technologies, V14SchoolConstructionRules.Candidate, pricing);
    }

    private static int Owned(V14CampaignSession session, string id) =>
        session.Snapshot().OtherBuildings.Single(item => item.BuildingId == id).Count;

    private static int Acquired(V14CampaignSession session, string id) =>
        session.Snapshot().PurchaseCounters.Single(item => item.FamilyId == id).Acquired;

    [Fact]
    public void InitialHouseHasHistoricalCounterAndNextPriceIsEscalated()
    {
        var session = Session();
        Assert.Equal(1, Owned(session, "small-house"));
        Assert.Equal(1, Acquired(session, "small-house"));
        var before = session.Snapshot();
        var result = session.BuildCivicBuilding("small-house", 1);
        Assert.True(result.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 92m), ("stone", 46m)), result.Cost);
        Assert.Equal(2, Owned(session, "small-house"));
        Assert.Equal(2, Acquired(session, "small-house"));
        Assert.Equal(before.Population, session.Snapshot().Population);
        Assert.Equal(before.Resources - result.Cost, session.Snapshot().Resources);
    }

    [Fact]
    public void BulkHousePurchaseMatchesSequentialCostsAndPreservesHistory()
    {
        var bulk = Session(funded: true);
        var sequential = Session(funded: true);
        var group = bulk.BuildCivicBuilding("small-house", 2);
        var first = sequential.BuildCivicBuilding("small-house", 1);
        var second = sequential.BuildCivicBuilding("small-house", 1);
        Assert.True(group.Succeeded);
        Assert.True(first.Succeeded);
        Assert.True(second.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 198m), ("stone", 99m)), group.Cost);
        Assert.Equal(first.Cost + second.Cost, group.Cost);
        Assert.Equal(bulk.Snapshot().Resources, sequential.Snapshot().Resources);
        Assert.Equal(3, Owned(bulk, "small-house"));
        Assert.Equal(3, Acquired(bulk, "small-house"));
        Assert.Equal(Acquired(bulk, "small-house"), Acquired(sequential, "small-house"));
    }

    [Fact]
    public void InsufficientResourcesCannotPartiallyBuildOrAdvanceCounters()
    {
        var session = Session();
        var previous = session.Snapshot();
        var attempt = session.BuildCivicBuilding("small-house", 2);
        Assert.Equal(V14CivicConstructionFailure.InsufficientResources, attempt.Failure);
        Assert.Equal(ResourceBundle.Create(("planks", 198m), ("stone", 99m)), attempt.Cost);
        Assert.Same(previous, session.Snapshot());
    }

    [Fact]
    public void UnknownFixedTieredAndPrerequisiteFailuresArePure()
    {
        var session = Session(funded: true);
        var before = session.Snapshot();
        Assert.Equal(V14CivicConstructionFailure.InvalidQuantity, session.BuildCivicBuilding("warehouse", 0).Failure);
        Assert.Equal(V14CivicConstructionFailure.UnknownBuilding, session.BuildCivicBuilding("school", 1).Failure);
        Assert.Equal(V14CivicConstructionFailure.UnknownBuilding, session.BuildCivicBuilding("missing", 1).Failure);
        Assert.Equal(V14CivicConstructionFailure.FixedBuilding, session.BuildCivicBuilding("town-hall", 1).Failure);
        Assert.Equal(V14CivicConstructionFailure.TieredBuilding, session.BuildCivicBuilding("sawmill", 1).Failure);
        Assert.Equal(V14CivicConstructionFailure.MissingPrerequisite, session.BuildCivicBuilding("large-house", 1).Failure);
        Assert.Equal(V14CivicConstructionFailure.MissingPrerequisite, session.BuildCivicBuilding("barracks", 1).Failure);
        Assert.Same(before, session.Snapshot());
    }

    [Fact]
    public void WarehouseExtendsSharedStorageAndHouseRaisesHousingLimit()
    {
        var baseline = CivilGrowthScenario.Create();
        var session = Session(funded: true);
        var original = session.Snapshot();
        Assert.Throws<ArgumentException>(() => V14CampaignSession.Restore(baseline,
            original with { Resources = ResourceBundle.Create(("food", 1200m)) },
            School, Technologies, V14SchoolConstructionRules.Candidate));
        Assert.True(session.BuildCivicBuilding("warehouse", 1).Succeeded);
        Assert.True(session.BuildCivicBuilding("small-house", 2).Succeeded);
        Assert.Equal(1, Owned(session, "warehouse"));
        Assert.Equal(3, Owned(session, "small-house"));
        var expanded = session.Snapshot();
        var loaded = V14CampaignSession.Restore(baseline, expanded with
        {
            Resources = ResourceBundle.Create(("food", 1200m)),
            Population = 17
        }, School, Technologies, V14SchoolConstructionRules.Candidate);
        Assert.Equal(17, loaded.Snapshot().Population);
        Assert.Equal(24, Owned(loaded, "small-house") * 8);
    }

    [Fact]
    public void GrowthFactorCanBeInjectedForCivicWithoutChangingV13()
    {
        var session = Session(pricing: new V14TierConstructionRules(1m, 2.6m, 6.5m));
        var price = session.BuildCivicBuilding("small-house", 1);
        Assert.True(price.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 80m), ("stone", 40m)), price.Cost);
        Assert.Equal(2, Acquired(session, "small-house"));
    }

    [Fact]
    public void BarracksCanBeBuiltAfterForgeAndKeepMilitaryState()
    {
        var baseline = CivilGrowthScenario.Create();
        var original = Session(funded: true).Snapshot();
        var seeded = original with
        {
            ProductiveFamilies = original.ProductiveFamilies.Select(item => item.FamilyId == "forge"
                ? item with { Base = new SimRisorse.Core.Construction.TierOccupancy(1, 0) } : item).ToImmutableArray(),
            PurchaseCounters = original.PurchaseCounters.Select(item => item.FamilyId == "forge"
                ? item with { Acquired = 1 } : item).ToImmutableArray()
        };
        var session = V14CampaignSession.Restore(baseline, seeded,
            School, Technologies, V14SchoolConstructionRules.Candidate);
        var result = session.BuildCivicBuilding("barracks", 1);
        Assert.True(result.Succeeded);
        Assert.Equal(1, Owned(session, "barracks"));
        Assert.Equal(1, Acquired(session, "barracks"));
        Assert.Equal(seeded.Military, session.Snapshot().Military);
    }
}
