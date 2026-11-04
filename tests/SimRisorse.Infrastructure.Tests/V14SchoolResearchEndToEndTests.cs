using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>
/// P1-D49: exercise real v14 commands and the SQLite v3 checkpoint together.
/// The only injected resources are a declared, capacity-bounded starting fixture;
/// training, research points, unlocks and upgraded buildings are earned normally.
/// </summary>
public sealed class V14SchoolResearchEndToEndTests
{
    private static readonly SchoolResearchRules School = new("food", ResourceAmount.FromUnits(10m),
        trainingSeconds: 120, trainingSeatsPerSchool: 1, researcherPlacesPerSchool: 2,
        milliPointsPerResearcherPerSecond: 100, pencilsPerStudent: 2);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly V14SchoolConstructionRules Construction = V14SchoolConstructionRules.Candidate;

    private static (SimRisorse.Core.Content.GameScenario Baseline, V14CampaignSession Session) Ready()
    {
        var scenario = CivilGrowthScenario.Create();
        var initial = V14CampaignSession.FromV13(scenario, GameSession.Create(scenario).Snapshot(),
            School, Technologies, Construction).Snapshot();
        // Fund ONLY existing catalog resources, below the Municipio's 1,000-unit
        // per-resource cap. The test subsequently purchases every required building.
        var funded = initial with
        {
            Resources = initial.Resources + ResourceBundle.Create(
                ("planks", 600m), ("stone", 300m), ("coal", 10m))
        };
        var session = V14CampaignSession.Restore(scenario, funded, School, Technologies, Construction);
        Assert.True(session.BuildProducer("fisher", ProductionTier.Base, 1).Succeeded);
        Assert.True(session.BuildProducer("food-store", ProductionTier.Base, 1).Succeeded);
        Assert.True(session.BuildSchool(1).Succeeded);
        return (scenario, session);
    }

    private static void AssertSameCampaign(V14CampaignSnapshot expected, V14CampaignSnapshot actual)
    {
        Assert.Equal(expected.ElapsedTicks, actual.ElapsedTicks);
        Assert.Equal(expected.Population, actual.Population);
        Assert.Equal(expected.Resources, actual.Resources);
        Assert.Equal(expected.SchoolCount, actual.SchoolCount);
        Assert.Equal(expected.SchoolResearch.WaitingStudents, actual.SchoolResearch.WaitingStudents);
        Assert.Equal(expected.SchoolResearch.TrainingProgressSeconds.ToArray(),
            actual.SchoolResearch.TrainingProgressSeconds.ToArray());
        Assert.Equal(expected.SchoolResearch.QualifiedResearchers,
            actual.SchoolResearch.QualifiedResearchers);
        Assert.Equal(expected.SchoolResearch.ActiveResearchers,
            actual.SchoolResearch.ActiveResearchers);
        Assert.Equal(expected.PencilPaidStudents, actual.PencilPaidStudents);
        Assert.Equal(expected.ResearchBalanceMilliPoints, actual.ResearchBalanceMilliPoints);
        // These collections represent keyed sets, not position-sensitive sequences.
        // SQLite can return the same complete records in a different row order.
        // Preserve positional comparison above for training progress: student order matters.
        Assert.Equal(expected.UnlockedTechnologyIds.OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            actual.UnlockedTechnologyIds.OrderBy(id => id, StringComparer.Ordinal).ToArray());
        Assert.Equal(expected.ProductiveFamilies.OrderBy(f => f.FamilyId, StringComparer.Ordinal).ToArray(),
            actual.ProductiveFamilies.OrderBy(f => f.FamilyId, StringComparer.Ordinal).ToArray());
        Assert.Equal(expected.OtherBuildings.OrderBy(b => b.BuildingId, StringComparer.Ordinal).ToArray(),
            actual.OtherBuildings.OrderBy(b => b.BuildingId, StringComparer.Ordinal).ToArray());
        Assert.Equal(expected.PurchaseCounters.OrderBy(c => c.FamilyId, StringComparer.Ordinal).ToArray(),
            actual.PurchaseCounters.OrderBy(c => c.FamilyId, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void PencilsToStudentToResearchToLargeTierSurvivesTwoSqliteRoundTrips()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-p1-d49-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var (scenario, original) = Ready();
            var store = new V14SqliteSaveStore(Path.Combine(folder, "campaign.sqlite3"),
                scenario, School, Technologies);
            Assert.Empty(original.Snapshot().UnlockedTechnologyIds);
            var freeBeforeEnrollment = original.AvailablePopulation;
            Assert.Equal(PencilCraftFailure.None, original.CraftPencils(1));
            Assert.Equal(SchoolActionFailure.None, original.EnrollStudents(1));
            Assert.Equal(2m, original.Snapshot().Resources["pencils"].Units);
            Assert.Equal(1, original.Snapshot().PencilPaidStudents);
            Assert.Equal(freeBeforeEnrollment - 1, original.AvailablePopulation);
            Assert.Equal(SchoolActionFailure.InsufficientQualifiedResearchers,
                original.AssignResearchers(1));
            Assert.Equal(TechnologyPurchaseFailure.InsufficientResearchPoints,
                original.BuyTechnology("lavorazioni-1"));
            Assert.Equal(V14TierActionFailure.MissingResearch,
                original.UpgradeProducer("sawmill", ProductionTier.Base, ProductionTier.Large).Failure);

            original.Resume();
            Assert.True(original.Advance(TimeSpan.FromSeconds(120)).Succeeded);
            Assert.Equal(1, original.Snapshot().SchoolResearch.QualifiedResearchers);
            Assert.Equal(0, original.Snapshot().SchoolResearch.ActiveResearchers);
            Assert.Equal(0, original.Snapshot().PencilPaidStudents);
            Assert.Equal(freeBeforeEnrollment - 1, original.AvailablePopulation);
            Assert.Equal(0L, original.Snapshot().ResearchBalanceMilliPoints);
            Assert.Equal(SchoolActionFailure.None, original.AssignResearchers(1));
            Assert.True(original.Advance(TimeSpan.FromSeconds(999)).Succeeded);
            Assert.Equal(99_900L, original.Snapshot().ResearchBalanceMilliPoints);
            Assert.Equal(TechnologyPurchaseFailure.InsufficientResearchPoints,
                original.BuyTechnology("lavorazioni-1"));

            original.Pause();
            store.Save(original.Snapshot());
            var firstCheckpoint = store.Load();
            AssertSameCampaign(original.Snapshot(), firstCheckpoint);
            var resumed = V14CampaignSession.Restore(scenario, firstCheckpoint,
                School, Technologies, Construction);
            Assert.True(resumed.IsPaused);
            Assert.True(resumed.Advance(TimeSpan.FromSeconds(30)).Succeeded);
            AssertSameCampaign(firstCheckpoint, resumed.Snapshot());

            original.Resume();
            resumed.Resume();
            Assert.True(original.Advance(TimeSpan.FromSeconds(1)).Succeeded);
            Assert.True(resumed.Advance(TimeSpan.FromSeconds(1)).Succeeded);
            AssertSameCampaign(original.Snapshot(), resumed.Snapshot());
            Assert.Equal(100_000L, resumed.Snapshot().ResearchBalanceMilliPoints);
            Assert.Equal(TechnologyPurchaseFailure.None, resumed.BuyTechnology("lavorazioni-1"));
            Assert.Equal(0L, resumed.Snapshot().ResearchBalanceMilliPoints);
            Assert.Contains("lavorazioni-1", resumed.Snapshot().UnlockedTechnologyIds);
            Assert.True(resumed.UpgradeProducer("sawmill", ProductionTier.Base, ProductionTier.Large).Succeeded);
            var upgraded = resumed.Snapshot().ProductiveFamilies.Single(item => item.FamilyId == "sawmill");
            Assert.Equal(0, upgraded.Base.Buildings);
            Assert.Equal(1, upgraded.Large.Buildings);
            Assert.Equal(1, upgraded.Large.AssignedWorkers);
            Assert.Equal(freeBeforeEnrollment - 1, resumed.AvailablePopulation);
            Assert.Equal(1, resumed.Snapshot().PurchaseCounters.Single(item => item.FamilyId == "sawmill").Acquired);

            store.Save(resumed.Snapshot());
            var secondCheckpoint = store.Load();
            AssertSameCampaign(resumed.Snapshot(), secondCheckpoint);
            var recovered = V14CampaignSession.Restore(scenario, secondCheckpoint,
                School, Technologies, Construction);
            Assert.True(recovered.IsPaused);
            Assert.Equal(TechnologyPurchaseFailure.AlreadyUnlocked,
                recovered.BuyTechnology("lavorazioni-1"));
            AssertSameCampaign(secondCheckpoint, recovered.Snapshot());
            Assert.True(File.Exists(store.DatabasePath));
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public void PartialTrainingSavedPausedDoesNotGraduateOrAccrueResearchUntilResumed()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-p1-d49-partial-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var (scenario, session) = Ready();
            Assert.Equal(PencilCraftFailure.None, session.CraftPencils(1));
            Assert.Equal(SchoolActionFailure.None, session.EnrollStudents(1));
            session.Resume();
            Assert.True(session.Advance(TimeSpan.FromSeconds(45)).Succeeded);
            Assert.Equal(45, Assert.Single(session.Snapshot().SchoolResearch.TrainingProgressSeconds));
            session.Pause();
            var store = new V14SqliteSaveStore(Path.Combine(folder, "partial.sqlite3"),
                scenario, School, Technologies);
            store.Save(session.Snapshot());
            var loaded = V14CampaignSession.Restore(scenario, store.Load(),
                School, Technologies, Construction);
            Assert.True(loaded.IsPaused);
            var before = loaded.Snapshot();
            Assert.True(loaded.Advance(TimeSpan.FromHours(1)).Succeeded);
            Assert.Same(before, loaded.Snapshot());
            loaded.Resume();
            Assert.True(loaded.Advance(TimeSpan.FromSeconds(75)).Succeeded);
            Assert.Equal(1, loaded.Snapshot().SchoolResearch.QualifiedResearchers);
            Assert.Equal(0, loaded.Snapshot().SchoolResearch.TrainingStudents);
            Assert.Equal(0, loaded.Snapshot().PencilPaidStudents);
            Assert.Equal(0L, loaded.Snapshot().ResearchBalanceMilliPoints);
            Assert.Equal(SchoolActionFailure.None, loaded.AssignResearchers(1));
            Assert.True(loaded.Advance(TimeSpan.FromSeconds(1)).Succeeded);
            Assert.Equal(100L, loaded.Snapshot().ResearchBalanceMilliPoints);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public void InsufficientSuppliesAndResearchNeverPartiallyChangeCampaign()
    {
        var (scenario, session) = Ready();
        var before = session.Snapshot();
        Assert.Equal(SchoolActionFailure.InsufficientPencils, session.EnrollStudents(1));
        Assert.Same(before, session.Snapshot());
        Assert.Equal(TechnologyPurchaseFailure.InsufficientResearchPoints,
            session.BuyTechnology("lavorazioni-1"));
        Assert.Same(before, session.Snapshot());
        Assert.Equal(V14TierActionFailure.MissingResearch,
            session.BuildProducer("sawmill", ProductionTier.Large, 1).Failure);
        Assert.Same(before, session.Snapshot());
        // A valid checkpoint still admits normal progression after every failed attempt.
        var restored = V14CampaignSession.Restore(scenario, before,
            School, Technologies, Construction);
        Assert.Equal(PencilCraftFailure.None, restored.CraftPencils(1));
        Assert.Equal(SchoolActionFailure.None, restored.EnrollStudents(1));
    }
}
