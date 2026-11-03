using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>E3b/3 candidate session. The active v13 GameSession must remain unchanged.</summary>
public sealed class V14CampaignSessionTests
{
    private static readonly SchoolResearchRules School = new("food", ResourceAmount.FromUnits(10m),
        120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly V14SchoolConstructionRules Construction = V14SchoolConstructionRules.Candidate;

    private static (SimRisorse.Core.Content.GameScenario Baseline, GameSnapshot Legacy,
        V14CampaignSession Session) Initial()
    {
        var baseline = CivilGrowthScenario.Create();
        var legacy = GameSession.Create(baseline).Snapshot();
        return (baseline, legacy, V14CampaignSession.FromV13(
            baseline, legacy, School, Technologies, Construction));
    }

    private static V14CampaignSession ReadyForSchool()
    {
        var (baseline, _, session) = Initial();
        var original = session.Snapshot();
        var prepared = original with
        {
            Resources = original.Resources + ResourceBundle.Create(("planks", 500m), ("stone", 400m)),
            ProductiveFamilies = original.ProductiveFamilies.Select(item => item.FamilyId == "food-store"
                ? item with { Base = new TierOccupancy(1, 0) } : item).ToImmutableArray(),
            PurchaseCounters = original.PurchaseCounters.Select(item => item.FamilyId == "food-store"
                ? item with { Acquired = 1 } : item).ToImmutableArray()
        };
        return V14CampaignSession.Restore(baseline, prepared, School, Technologies, Construction);
    }

    [Fact]
    public void BootstrapAndRestorePreserveCheckpointWithoutChangingV13()
    {
        var (baseline, legacy, session) = Initial();
        var saved = session.Snapshot();
        Assert.Equal("civil-growth-14", saved.RulesVersion);
        Assert.Equal("civil-growth-13", legacy.RulesVersion);
        Assert.Equal(legacy.Population, saved.Population);
        Assert.Equal(legacy.ElapsedTicks, saved.ElapsedTicks);
        Assert.Equal(legacy.Resources, saved.Resources);
        Assert.Equal(0, session.ResearchBalance.MilliPoints);
        var restored = V14CampaignSession.Restore(baseline, saved, School, Technologies, Construction);
        Assert.Same(saved, restored.Snapshot());
        Assert.Throws<ArgumentException>(() => V14CampaignSession.Restore(baseline,
            saved with { RulesVersion = "civil-growth-13" }, School, Technologies, Construction));
    }

    [Fact]
    public void BuildingSchoolRequiresPrerequisitesAndDoesNotSpendOnFailure()
    {
        var (_, _, session) = Initial();
        var before = session.Snapshot();
        Assert.Equal(V14SchoolConstructionFailure.MissingPrerequisite, session.BuildSchool(1).Failure);
        Assert.Equal(V14SchoolConstructionFailure.InvalidQuantity, session.BuildSchool(0).Failure);
        Assert.Same(before, session.Snapshot());
        Assert.Equal(SchoolActionFailure.MissingSchool, session.EnrollStudents(1));
        Assert.Same(before, session.Snapshot());
    }

    [Fact]
    public void NewSchoolsUseMonotoneFamilyPricingAndAtomicPurchase()
    {
        var session = ReadyForSchool();
        var first = session.BuildSchool(1);
        Assert.True(first.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 120m), ("stone", 80m)), first.Cost);
        Assert.Equal(1, session.Snapshot().SchoolCount);
        Assert.Equal(1, session.Snapshot().PurchaseCounters.Single(item => item.FamilyId == "school").Acquired);
        var second = session.BuildSchool(1);
        Assert.True(second.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 138m), ("stone", 92m)), second.Cost);
        var after = session.Snapshot();
        Assert.Equal(2, after.SchoolCount);
        Assert.Equal(2, after.PurchaseCounters.Single(item => item.FamilyId == "school").Acquired);
        Assert.Equal(V14SchoolConstructionFailure.InsufficientResources, session.BuildSchool(20).Failure);
        Assert.Same(after, session.Snapshot());
    }

    [Fact]
    public void EnrollmentAndCancelReservePopulationAndRefundFoodOnce()
    {
        var session = ReadyForSchool();
        Assert.True(session.BuildSchool(1).Succeeded);
        var before = session.Snapshot();
        var free = session.AvailablePopulation;
        Assert.Equal(SchoolActionFailure.None, session.EnrollStudents(2));
        Assert.Equal(free - 2, session.AvailablePopulation);
        Assert.Equal(before.Resources["food"].Units - 20m, session.Snapshot().Resources["food"].Units);
        Assert.Equal(1, session.Snapshot().SchoolResearch.TrainingStudents);
        Assert.Equal(1, session.Snapshot().SchoolResearch.WaitingStudents);
        var inTraining = session.Snapshot();
        Assert.Equal(SchoolActionFailure.InsufficientPopulation, session.EnrollStudents(100));
        Assert.Same(inTraining, session.Snapshot());
        Assert.Equal(SchoolActionFailure.None, session.CancelStudents(2));
        Assert.Equal(before.Resources, session.Snapshot().Resources);
        Assert.Equal(free, session.AvailablePopulation);
        Assert.Equal(SchoolActionFailure.InsufficientStudents, session.CancelStudents(1));
    }

    [Fact]
    public void QualifiedAndActiveResearchersAreNotCountedTwice()
    {
        var (baseline, _, _) = Initial();
        var candidate = ReadyForSchool();
        Assert.True(candidate.BuildSchool(1).Succeeded);
        var s = candidate.Snapshot() with
        {
            SchoolResearch = new SchoolResearchState(0, ImmutableArray<int>.Empty, 2, 0)
        };
        var session = V14CampaignSession.Restore(baseline, s, School, Technologies, Construction);
        var free = session.AvailablePopulation;
        Assert.Equal(SchoolActionFailure.None, session.AssignResearchers(2));
        Assert.Equal(free, session.AvailablePopulation);
        var active = session.Snapshot();
        Assert.Equal(SchoolActionFailure.InsufficientQualifiedResearchers,
            session.ReturnResearchersToCivilian(1));
        Assert.Same(active, session.Snapshot());
        Assert.Equal(SchoolActionFailure.None, session.UnassignResearchers(1));
        Assert.Equal(SchoolActionFailure.None, session.ReturnResearchersToCivilian(1));
        Assert.Equal(free + 1, session.AvailablePopulation);
        Assert.Equal(1, session.Snapshot().SchoolResearch.ActiveResearchers);
        Assert.Equal(1, session.Snapshot().SchoolResearch.QualifiedResearchers);
    }

    [Fact]
    public void ResearchIsSpentOnlyOnExplicitPurchaseAndPrerequisitesAreChecked()
    {
        var (baseline, _, session) = Initial();
        var prepared = session.Snapshot() with { ResearchBalanceMilliPoints = 350_000 };
        var loaded = V14CampaignSession.Restore(baseline, prepared, School, Technologies, Construction);
        Assert.Equal(TechnologyPurchaseFailure.MissingPrerequisite, loaded.BuyTechnology("lavorazioni-2"));
        Assert.Equal(TechnologyPurchaseFailure.NotImplemented, loaded.BuyTechnology("logistica-1"));
        Assert.Equal(350_000, loaded.ResearchBalance.MilliPoints);
        Assert.Equal(TechnologyPurchaseFailure.None, loaded.BuyTechnology("lavorazioni-1"));
        Assert.Equal(250_000, loaded.ResearchBalance.MilliPoints);
        Assert.Contains("lavorazioni-1", loaded.Snapshot().UnlockedTechnologyIds);
        var purchased = loaded.Snapshot();
        Assert.Equal(TechnologyPurchaseFailure.AlreadyUnlocked, loaded.BuyTechnology("lavorazioni-1"));
        Assert.Same(purchased, loaded.Snapshot());
        Assert.Equal(TechnologyPurchaseFailure.InsufficientResearchPoints, loaded.BuyTechnology("lavorazioni-2"));
        Assert.Same(purchased, loaded.Snapshot());
    }
}
