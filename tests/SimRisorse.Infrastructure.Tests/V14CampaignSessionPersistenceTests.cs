using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>Exercises the new isolated session through the separate SQLite v3 repository.</summary>
public sealed class V14CampaignSessionPersistenceTests
{
    [Fact]
    public void SchoolEnrollmentAndTechnologyPurchaseSurviveSqliteV3RoundTrip()
    {
        var directory = Path.Combine(Path.GetTempPath(), "simrisorse-e3b3-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var database = Path.Combine(directory, "candidate-v14.sqlite3");
            var baseline = CivilGrowthScenario.Create();
            var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
            var technologies = TechnologyCatalog.ProposedTierTree;
            var session = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
                school, technologies, V14SchoolConstructionRules.Candidate);
            var start = session.Snapshot();
            var prepared = start with
            {
                Resources = start.Resources + ResourceBundle.Create(("planks", 500m), ("stone", 400m)),
                ProductiveFamilies = start.ProductiveFamilies.Select(item => item.FamilyId == "food-store"
                    ? item with { Base = new TierOccupancy(1, 0) } : item).ToImmutableArray(),
                PurchaseCounters = start.PurchaseCounters.Select(item => item.FamilyId == "food-store"
                    ? item with { Acquired = 1 } : item).ToImmutableArray(),
                ResearchBalanceMilliPoints = 100_000
            };
            session = V14CampaignSession.Restore(baseline, prepared, school, technologies,
                V14SchoolConstructionRules.Candidate);
            Assert.True(session.BuildSchool(1).Succeeded);
            Assert.Equal(SchoolActionFailure.None, session.EnrollStudents(2));
            Assert.Equal(TechnologyPurchaseFailure.None, session.BuyTechnology("lavorazioni-1"));
            var expected = session.Snapshot();
            var store = new V14SqliteSaveStore(database, baseline, school, technologies);
            store.Save(expected);
            var restored = V14CampaignSession.Restore(baseline, store.Load(), school, technologies,
                V14SchoolConstructionRules.Candidate);
            Assert.Equal(expected.Resources, restored.Snapshot().Resources);
            Assert.Equal(expected.SchoolCount, restored.Snapshot().SchoolCount);
            Assert.Equal(expected.SchoolResearch.TrainingStudents, restored.Snapshot().SchoolResearch.TrainingStudents);
            Assert.Equal(expected.SchoolResearch.WaitingStudents, restored.Snapshot().SchoolResearch.WaitingStudents);
            Assert.Equal(expected.ResearchBalanceMilliPoints, restored.Snapshot().ResearchBalanceMilliPoints);
            Assert.Equal(expected.UnlockedTechnologyIds.ToArray(), restored.Snapshot().UnlockedTechnologyIds.ToArray());
            Assert.Equal(expected.PurchaseCounters.OrderBy(item => item.FamilyId, StringComparer.Ordinal).ToArray(),
                restored.Snapshot().PurchaseCounters.OrderBy(item => item.FamilyId, StringComparer.Ordinal).ToArray());
            Assert.Equal(session.AvailablePopulation, restored.AvailablePopulation);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void MidTrainingCheckpointResumesExactlyAtGraduationAfterSqliteRoundTrip()
    {
        var directory = Path.Combine(Path.GetTempPath(), "simrisorse-clock-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var baseline = CivilGrowthScenario.Create();
            var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
            var technologies = TechnologyCatalog.ProposedTierTree;
            var construction = V14SchoolConstructionRules.Candidate;
            var original = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
                school, technologies, construction).Snapshot();
            var prepared = original with
            {
                Resources = original.Resources + ResourceBundle.Create(("planks", 500m), ("stone", 400m)),
                ProductiveFamilies = original.ProductiveFamilies.Select(item => item.FamilyId == "food-store"
                    ? item with { Base = new TierOccupancy(1, 0) } : item).ToImmutableArray(),
                PurchaseCounters = original.PurchaseCounters.Select(item => item.FamilyId == "food-store"
                    ? item with { Acquired = 1 } : item).ToImmutableArray()
            };
            V14CampaignSession Create() => V14CampaignSession.Restore(baseline, prepared,
                school, technologies, construction);
            var partial = Create();
            Assert.True(partial.BuildSchool(1).Succeeded);
            Assert.Equal(SchoolActionFailure.None, partial.EnrollStudents(1));
            var start = partial.Snapshot();
            partial.Resume();
            Assert.True(partial.Advance(TimeSpan.FromSeconds(37)).Succeeded);
            var store = new V14SqliteSaveStore(Path.Combine(directory, "school-clock.sqlite3"),
                baseline, school, technologies);
            store.Save(partial.Snapshot());
            var restored = V14CampaignSession.Restore(baseline, store.Load(), school, technologies, construction);
            Assert.True(restored.IsPaused);
            Assert.Equal(37, Assert.Single(restored.Snapshot().SchoolResearch.TrainingProgressSeconds));
            restored.Resume();
            Assert.True(restored.Advance(TimeSpan.FromSeconds(83)).Succeeded);

            var continuous = V14CampaignSession.Restore(baseline, start, school, technologies, construction);
            continuous.Resume();
            Assert.True(continuous.Advance(TimeSpan.FromSeconds(120)).Succeeded);
            Assert.Equal(continuous.Snapshot().ElapsedTicks, restored.Snapshot().ElapsedTicks);
            Assert.Equal(continuous.Snapshot().Resources, restored.Snapshot().Resources);
            Assert.Equal(continuous.Snapshot().Population, restored.Snapshot().Population);
            Assert.Equal(continuous.Snapshot().SchoolResearch.QualifiedResearchers,
                restored.Snapshot().SchoolResearch.QualifiedResearchers);
            Assert.Equal(continuous.Snapshot().SchoolResearch.TrainingProgressSeconds.ToArray(),
                restored.Snapshot().SchoolResearch.TrainingProgressSeconds.ToArray());
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

}
