using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>E3b/3d: the isolated v14 session owns construction, upgrade and workforce commits.</summary>
public sealed class V14TierSessionTests
{
    private static readonly SchoolResearchRules School = new("food", ResourceAmount.FromUnits(10m),
        120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;

    private static V14CampaignSession Create(params string[] technologies)
    {
        var baseline = CivilGrowthScenario.Create();
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            School, Technologies, V14SchoolConstructionRules.Candidate).Snapshot();
        return V14CampaignSession.Restore(baseline,
            initial with { UnlockedTechnologyIds = technologies.ToImmutableArray() },
            School, Technologies, V14SchoolConstructionRules.Candidate);
    }

    private static V14CampaignSession Funded(params string[] technologies)
    {
        var baseline = CivilGrowthScenario.Create();
        var source = Create(technologies).Snapshot();
        return V14CampaignSession.Restore(baseline, source with
        {
            Resources = source.Resources + ResourceBundle.Create(("planks", 500m), ("stone", 400m))
        }, School, Technologies, V14SchoolConstructionRules.Candidate);
    }

    private static V14FamilySnapshot Family(V14CampaignSession session, string id) =>
        session.Snapshot().ProductiveFamilies.Single(item => item.FamilyId == id);

    private static int Acquired(V14CampaignSession session, string id) =>
        session.Snapshot().PurchaseCounters.Single(item => item.FamilyId == id).Acquired;

    [Fact]
    public void MigratedSawmillHasHistoricalCounterAndNextBaseHasEscalatedPrice()
    {
        var session = Create();
        Assert.Equal(new TierOccupancy(1, 1), Family(session, "sawmill").Base);
        Assert.Equal(1, Acquired(session, "sawmill"));
        var before = session.Snapshot();
        var result = session.BuildProducer("sawmill", ProductionTier.Base, 1);
        Assert.True(result.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 46m), ("stone", 23m)), result.Cost);
        Assert.Equal(before.Resources - result.Cost, session.Snapshot().Resources);
        Assert.Equal(new TierOccupancy(2, 1), Family(session, "sawmill").Base);
        Assert.Equal(2, Acquired(session, "sawmill"));
        Assert.Equal(before.Population, session.Snapshot().Population);
        Assert.Equal(before.ElapsedTicks, session.Snapshot().ElapsedTicks);
    }

    [Fact]
    public void LockedLargeAndAdvancedCannotSpendOrChangeCounts()
    {
        var session = Funded();
        var before = session.Snapshot();
        Assert.Equal(V14TierActionFailure.MissingResearch,
            session.BuildProducer("sawmill", ProductionTier.Large, 1).Failure);
        Assert.Equal(V14TierActionFailure.MissingResearch,
            session.BuildProducer("sawmill", ProductionTier.Advanced, 1).Failure);
        Assert.Equal(V14TierActionFailure.MissingResearch,
            session.UpgradeProducer("sawmill", ProductionTier.Base, ProductionTier.Large).Failure);
        Assert.Same(before, session.Snapshot());
    }

    [Fact]
    public void DirectLargeAndAdvancedUseFamilyCounterAndLeaveSlotsEmpty()
    {
        var large = Create("lavorazioni-1");
        var result = large.BuildProducer("sawmill", ProductionTier.Large, 1);
        Assert.True(result.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 120m), ("stone", 60m)), result.Cost);
        Assert.Equal(new TierOccupancy(1, 0), Family(large, "sawmill").Large);
        Assert.Equal(2, Acquired(large, "sawmill"));
        Assert.Equal(5, large.AvailablePopulation);

        var advanced = Funded("lavorazioni-1", "lavorazioni-2");
        var buy = advanced.BuildProducer("sawmill", ProductionTier.Advanced, 1);
        Assert.True(buy.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 299m), ("stone", 150m)), buy.Cost);
        Assert.Equal(new TierOccupancy(1, 0), Family(advanced, "sawmill").Advanced);
        Assert.Equal(2, Acquired(advanced, "sawmill"));
        Assert.Equal(5, advanced.AvailablePopulation);
    }

    [Fact]
    public void DirectAdvancedAndTwoUpgradePathsChargeSameTotalWithoutNewAcquisitions()
    {
        var direct = Funded("lavorazioni-1", "lavorazioni-2");
        var staged = Funded("lavorazioni-1", "lavorazioni-2");
        var directBefore = direct.Snapshot().Resources;
        var directPrice = direct.UpgradeProducer("sawmill", ProductionTier.Base, ProductionTier.Advanced);
        var first = staged.UpgradeProducer("sawmill", ProductionTier.Base, ProductionTier.Large);
        var second = staged.UpgradeProducer("sawmill", ProductionTier.Large, ProductionTier.Advanced);
        Assert.True(directPrice.Succeeded);
        Assert.True(first.Succeeded);
        Assert.True(second.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 253m), ("stone", 127m)), directPrice.Cost);
        Assert.Equal(directPrice.Cost, first.Cost + second.Cost);
        Assert.Equal(directBefore - directPrice.Cost, direct.Snapshot().Resources);
        Assert.Equal(direct.Snapshot().Resources, staged.Snapshot().Resources);
        Assert.Equal(Family(direct, "sawmill"), Family(staged, "sawmill"));
        Assert.Equal(new TierOccupancy(1, 1), Family(staged, "sawmill").Advanced);
        Assert.Equal(1, Acquired(direct, "sawmill"));
        Assert.Equal(1, Acquired(staged, "sawmill"));
    }

    [Fact]
    public void GroupPurchaseFailureIsAtomicAndSuccessfulGroupChargesSequentially()
    {
        var session = Create();
        var before = session.Snapshot();
        var failed = session.BuildProducer("sawmill", ProductionTier.Base, 10);
        Assert.Equal(V14TierActionFailure.InsufficientResources, failed.Failure);
        Assert.True(failed.Cost["planks"].Units > before.Resources["planks"].Units);
        Assert.Same(before, session.Snapshot());
        var funded = Funded();
        var two = funded.BuildProducer("sawmill", ProductionTier.Base, 2);
        Assert.True(two.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 99m), ("stone", 50m)), two.Cost);
        Assert.Equal(3, Acquired(funded, "sawmill"));
        Assert.Equal(3, Family(funded, "sawmill").Base.Buildings);
    }

    [Fact]
    public void MissingConstructionPrerequisitesRemainRequiredEvenWithResources()
    {
        var session = Funded();
        var start = session.Snapshot();
        Assert.Equal(V14TierActionFailure.MissingPrerequisite,
            session.BuildProducer("food-store", ProductionTier.Base, 1).Failure);
        Assert.Same(start, session.Snapshot());
        Assert.True(session.BuildProducer("fisher", ProductionTier.Base, 1).Succeeded);
        Assert.True(session.BuildProducer("food-store", ProductionTier.Base, 1).Succeeded);
        Assert.Equal(1, Acquired(session, "fisher"));
        Assert.Equal(1, Acquired(session, "food-store"));
    }

    [Fact]
    public void WorkerAssignmentsUseFreePeopleAndTierSlotsOnly()
    {
        var session = Funded("lavorazioni-1");
        Assert.True(session.BuildProducer("sawmill", ProductionTier.Large, 1).Succeeded);
        Assert.Equal(V14TierActionFailure.None,
            session.AssignProducerWorkers("sawmill", ProductionTier.Large, 3));
        Assert.Equal(new TierOccupancy(1, 3), Family(session, "sawmill").Large);
        Assert.Equal(2, session.AvailablePopulation);
        var before = session.Snapshot();
        Assert.Equal(V14TierActionFailure.InsufficientWorkplaces,
            session.AssignProducerWorkers("sawmill", ProductionTier.Large, 1));
        Assert.Equal(V14TierActionFailure.InsufficientWorkplaces,
            session.AssignProducerWorkers("fisher", ProductionTier.Base, 3));
        Assert.Same(before, session.Snapshot());
        Assert.Equal(V14TierActionFailure.None,
            session.ReleaseProducerWorkers("sawmill", ProductionTier.Large, 2));
        Assert.Equal(4, session.AvailablePopulation);
        Assert.Equal(1, Family(session, "sawmill").Large.AssignedWorkers);
    }

    [Fact]
    public void WorkerScarcityWithStudentsAndSoldiersIsEnforced()
    {
        var session = V14CampaignClockTests.Prepared();
        Assert.Equal(0, session.AvailablePopulation);
        Assert.True(session.BuildProducer("sawmill", ProductionTier.Base, 1).Succeeded);
        var before = session.Snapshot();
        Assert.Equal(V14TierActionFailure.InsufficientPopulation,
            session.AssignProducerWorkers("sawmill", ProductionTier.Base, 1));
        Assert.Same(before, session.Snapshot());
        Assert.Equal(V14TierActionFailure.None,
            session.ReleaseProducerWorkers("sawmill", ProductionTier.Base, 1));
        Assert.Equal(V14TierActionFailure.None,
            session.AssignProducerWorkers("sawmill", ProductionTier.Base, 1));
        Assert.Equal(0, session.AvailablePopulation);
    }

    [Fact]
    public void UpgradeTransfersOnlyExistingWorkersAndNewVacanciesProduceNoBonus()
    {
        var session = Funded("lavorazioni-1");
        var oldFree = session.AvailablePopulation;
        var result = session.UpgradeProducer("sawmill", ProductionTier.Base, ProductionTier.Large);
        Assert.True(result.Succeeded);
        Assert.Equal(new TierOccupancy(0, 0), Family(session, "sawmill").Base);
        Assert.Equal(new TierOccupancy(1, 1), Family(session, "sawmill").Large);
        Assert.Equal(oldFree, session.AvailablePopulation);
        Assert.Equal(V14TierActionFailure.None,
            session.AssignProducerWorkers("sawmill", ProductionTier.Large, 2));
        Assert.Equal(oldFree - 2, session.AvailablePopulation);
    }

    [Fact]
    public void InvalidInputsAndUnstaffingNeverMutate()
    {
        var session = Funded();
        var start = session.Snapshot();
        Assert.Equal(V14TierActionFailure.InvalidQuantity, session.BuildProducer("sawmill", ProductionTier.Base, 0).Failure);
        Assert.Equal(V14TierActionFailure.UnknownFamily, session.BuildProducer("warehouse", ProductionTier.Base, 1).Failure);
        Assert.Equal(V14TierActionFailure.InvalidTier, session.BuildProducer("sawmill", (ProductionTier)99, 1).Failure);
        Assert.Equal(V14TierActionFailure.InvalidTier,
            session.UpgradeProducer("sawmill", ProductionTier.Base, ProductionTier.Base).Failure);
        Assert.Equal(V14TierActionFailure.MissingResearch,
            session.UpgradeProducer("sawmill", ProductionTier.Large, ProductionTier.Advanced).Failure);
        Assert.Equal(V14TierActionFailure.InvalidQuantity, session.AssignProducerWorkers("sawmill", ProductionTier.Base, 0));
        Assert.Equal(V14TierActionFailure.InsufficientAssignedWorkers,
            session.ReleaseProducerWorkers("sawmill", ProductionTier.Base, 2));
        Assert.Same(start, session.Snapshot());
        var allTech = Funded("lavorazioni-1", "lavorazioni-2");
        var fullyUnlocked = allTech.Snapshot();
        Assert.Equal(V14TierActionFailure.MissingSourceBuilding,
            allTech.UpgradeProducer("sawmill", ProductionTier.Large, ProductionTier.Advanced).Failure);
        Assert.Same(fullyUnlocked, allTech.Snapshot());
        Assert.Throws<ArgumentOutOfRangeException>(() => new V14TierConstructionRules(0.99m, 2.6m, 6.5m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new V14TierConstructionRules(1.15m, 1m, 6.5m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new V14TierConstructionRules(1.15m, 2.6m, 2.6m));
    }

    [Fact]
    public void ConstructionPricingParametersCanBeInjectedWithoutChangingV13()
    {
        var baseline = CivilGrowthScenario.Create();
        var source = Create().Snapshot();
        var parameters = new V14TierConstructionRules(1m, 2.2m, 5m);
        var session = V14CampaignSession.Restore(baseline, source, School, Technologies,
            V14SchoolConstructionRules.Candidate, parameters);
        var buy = session.BuildProducer("sawmill", ProductionTier.Base, 1);
        Assert.True(buy.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 40m), ("stone", 20m)), buy.Cost);
        Assert.Equal("civil-growth-13", baseline.Catalog.RulesVersion);
        Assert.Equal("civil-growth-14", session.Snapshot().RulesVersion);
    }
}
