using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>Deterministic command-stream replay across a genuine isolated SQLite v3 checkpoint.</summary>
public sealed class V14ReplayPersistenceTests
{
    private static readonly SchoolResearchRules School = new("food", ResourceAmount.FromUnits(10m),
        120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly V14SchoolConstructionRules SchoolBuilding = V14SchoolConstructionRules.Candidate;

    private static (GameScenario Baseline, V14CampaignSession Session) Seed()
    {
        var baseline = CivilGrowthScenario.Create();
        var original = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            School, Technologies, SchoolBuilding).Snapshot();
        // Represents a previously accumulated 100-point balance, not a queued technology.
        // The additional resources remain strictly below the starter shared-storage limit.
        var funded = original with
        {
            Resources = original.Resources + ResourceBundle.Create(
                ("planks", 650m), ("stone", 520m), ("food", 70m)),
            ResearchBalanceMilliPoints = 100_000
        };
        return (baseline, V14CampaignSession.Restore(baseline, funded,
            School, Technologies, SchoolBuilding));
    }

    private static void BeforeCheckpoint(V14CampaignSession session)
    {
        Assert.True(session.BuildCivicBuilding("small-house", 1).Succeeded);
        Assert.True(session.BuildCivicBuilding("warehouse", 1).Succeeded);
        Assert.True(session.BuildProducer("fisher", ProductionTier.Base, 1).Succeeded);
        Assert.True(session.BuildProducer("food-store", ProductionTier.Base, 1).Succeeded);
        Assert.True(session.BuildSchool(1).Succeeded);
        Assert.Equal(SchoolActionFailure.None, session.EnrollStudents(1));
        session.Resume();
        Assert.True(session.Advance(TimeSpan.FromSeconds(120)).Succeeded);
        Assert.Equal(1, session.Snapshot().SchoolResearch.QualifiedResearchers);
        Assert.Equal(SchoolActionFailure.None, session.AssignResearchers(1));
        Assert.True(session.Advance(TimeSpan.FromSeconds(9)).Succeeded);
        Assert.Equal(100_900L, session.Snapshot().ResearchBalanceMilliPoints);
    }

    private static void AfterCheckpoint(V14CampaignSession session, bool splitClock)
    {
        Assert.Equal(TechnologyPurchaseFailure.None, session.BuyTechnology("lavorazioni-1"));
        Assert.Equal(900L, session.Snapshot().ResearchBalanceMilliPoints);
        Assert.True(session.BuildProducer("sawmill", ProductionTier.Large, 1).Succeeded);
        Assert.Equal(V14TierActionFailure.None,
            session.AssignProducerWorkers("sawmill", ProductionTier.Large, 2));
        Assert.True(session.UpgradeProducer("sawmill", ProductionTier.Base, ProductionTier.Large).Succeeded);
        Assert.True(session.BuildCivicBuilding("small-house", 1).Succeeded);
        session.Resume();
        if (splitClock)
        {
            for (var i = 0; i < 3; i++)
                Assert.True(session.Advance(TimeSpan.FromSeconds(15)).Succeeded);
        }
        else Assert.True(session.Advance(TimeSpan.FromSeconds(45)).Succeeded);
    }

    private static void Equivalent(V14CampaignSnapshot expected, V14CampaignSnapshot actual)
    {
        Assert.Equal(expected.FormatVersion, actual.FormatVersion);
        Assert.Equal(expected.ScenarioId, actual.ScenarioId);
        Assert.Equal(expected.RulesVersion, actual.RulesVersion);
        Assert.Equal(expected.ElapsedTicks, actual.ElapsedTicks);
        Assert.Equal(expected.Population, actual.Population);
        Assert.Equal(expected.Resources, actual.Resources);
        Assert.Equal(expected.OtherBuildings.OrderBy(b => b.BuildingId, StringComparer.Ordinal).ToArray(),
            actual.OtherBuildings.OrderBy(b => b.BuildingId, StringComparer.Ordinal).ToArray());
        Assert.Equal(expected.ProductiveFamilies.OrderBy(f => f.FamilyId, StringComparer.Ordinal).ToArray(),
            actual.ProductiveFamilies.OrderBy(f => f.FamilyId, StringComparer.Ordinal).ToArray());
        Assert.Equal(expected.PurchaseCounters.OrderBy(c => c.FamilyId, StringComparer.Ordinal).ToArray(),
            actual.PurchaseCounters.OrderBy(c => c.FamilyId, StringComparer.Ordinal).ToArray());
        Assert.Equal(expected.SchoolCount, actual.SchoolCount);
        Assert.Equal(expected.GrowthProgressSeconds, actual.GrowthProgressSeconds);
        Assert.Equal(expected.GrowthRemainderTwentieths, actual.GrowthRemainderTwentieths);
        Assert.Equal(expected.Military, actual.Military);
        Assert.Equal(expected.SchoolResearch.WaitingStudents, actual.SchoolResearch.WaitingStudents);
        Assert.Equal(expected.SchoolResearch.TrainingProgressSeconds.ToArray(),
            actual.SchoolResearch.TrainingProgressSeconds.ToArray());
        Assert.Equal(expected.SchoolResearch.QualifiedResearchers, actual.SchoolResearch.QualifiedResearchers);
        Assert.Equal(expected.SchoolResearch.ActiveResearchers, actual.SchoolResearch.ActiveResearchers);
        Assert.Equal(expected.ResearchBalanceMilliPoints, actual.ResearchBalanceMilliPoints);
        Assert.Equal(expected.UnlockedTechnologyIds.OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            actual.UnlockedTechnologyIds.OrderBy(id => id, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void MixedConstructionTrainingResearchUpgradeAndClockReplayMatchesAfterSqliteRestore()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-v14-replay-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var (baseline, uninterrupted) = Seed();
            var (_, interrupted) = Seed();
            BeforeCheckpoint(uninterrupted);
            BeforeCheckpoint(interrupted);
            var saved = interrupted.Snapshot();
            var store = new V14SqliteSaveStore(Path.Combine(folder, "replay.sqlite3"),
                baseline, School, Technologies);
            store.Save(saved);
            var restored = V14CampaignSession.Restore(baseline, store.Load(),
                School, Technologies, SchoolBuilding);
            Assert.True(restored.IsPaused);
            Equivalent(saved, restored.Snapshot());
            AfterCheckpoint(uninterrupted, splitClock: false);
            AfterCheckpoint(restored, splitClock: true);
            Equivalent(uninterrupted.Snapshot(), restored.Snapshot());
            Assert.Equal(3, restored.Snapshot().OtherBuildings.Single(b => b.BuildingId == "small-house").Count);
            Assert.Equal(3, restored.Snapshot().PurchaseCounters.Single(b => b.FamilyId == "small-house").Acquired);
            Assert.Equal(2, restored.Snapshot().PurchaseCounters.Single(b => b.FamilyId == "sawmill").Acquired);
            Assert.Single(restored.Snapshot().UnlockedTechnologyIds);
            // A second checkpoint also preserves the full post-replay state.
            store.Save(restored.Snapshot());
            Equivalent(restored.Snapshot(), store.Load());
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public void RestoredHouseAcquisitionContinuesEscalatingAndRejectedOrdersArePure()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-v14-civic-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var (baseline, session) = Seed();
            Assert.True(session.BuildCivicBuilding("small-house", 1).Succeeded);
            var store = new V14SqliteSaveStore(Path.Combine(folder, "purchase.sqlite3"),
                baseline, School, Technologies);
            store.Save(session.Snapshot());
            var restored = V14CampaignSession.Restore(baseline, store.Load(), School,
                Technologies, SchoolBuilding);
            var before = restored.Snapshot();
            Assert.Equal(V14CivicConstructionFailure.MissingPrerequisite,
                restored.BuildCivicBuilding("large-house", 1).Failure);
            Assert.Equal(V14CivicConstructionFailure.FixedBuilding,
                restored.BuildCivicBuilding("town-hall", 1).Failure);
            Assert.Same(before, restored.Snapshot());
            var second = restored.BuildCivicBuilding("small-house", 1);
            Assert.True(second.Succeeded);
            Assert.Equal(ResourceBundle.Create(("planks", 106m), ("stone", 53m)), second.Cost);
            Assert.Equal(3, restored.Snapshot().PurchaseCounters.Single(c => c.FamilyId == "small-house").Acquired);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}
