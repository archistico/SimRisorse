using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>Versioned SQLite v3 round-trip of a constructed, staffed and upgraded v14 colony.</summary>
public sealed class V14TierSessionPersistenceTests
{
    [Fact]
    public void ConstructedAndUpgradedTiersRetainStaffingAcquisitionHistoryAndProduction()
    {
        var directory = Path.Combine(Path.GetTempPath(), "simrisorse-tier-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var baseline = CivilGrowthScenario.Create();
            var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
            var technologies = TechnologyCatalog.ProposedTierTree;
            var rules = V14SchoolConstructionRules.Candidate;
            var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
                school, technologies, rules).Snapshot();
            var seeded = initial with
            {
                Resources = initial.Resources + ResourceBundle.Create(("planks", 500m), ("stone", 400m)),
                UnlockedTechnologyIds = ImmutableArray.Create("lavorazioni-1", "lavorazioni-2")
            };
            var session = V14CampaignSession.Restore(baseline, seeded, school, technologies, rules);
            Assert.True(session.BuildProducer("sawmill", ProductionTier.Large, 1).Succeeded);
            Assert.Equal(V14TierActionFailure.None,
                session.AssignProducerWorkers("sawmill", ProductionTier.Large, 2));
            Assert.True(session.UpgradeProducer("sawmill", ProductionTier.Base, ProductionTier.Advanced).Succeeded);
            var beforeSave = session.Snapshot();
            var database = Path.Combine(directory, "tier-v14.sqlite3");
            var store = new V14SqliteSaveStore(database, baseline, school, technologies);
            store.Save(beforeSave);
            var loaded = V14CampaignSession.Restore(baseline, store.Load(), school, technologies, rules);
            Assert.True(loaded.IsPaused);
            Assert.Equal(beforeSave.Resources, loaded.Snapshot().Resources);
            Assert.Equal(beforeSave.ProductiveFamilies.ToArray(), loaded.Snapshot().ProductiveFamilies.ToArray());
            Assert.Equal(beforeSave.PurchaseCounters.ToArray(), loaded.Snapshot().PurchaseCounters.ToArray());
            Assert.Equal(beforeSave.UnlockedTechnologyIds.ToArray(), loaded.Snapshot().UnlockedTechnologyIds.ToArray());
            Assert.Equal(session.AvailablePopulation, loaded.AvailablePopulation);
            var mill = loaded.Snapshot().ProductiveFamilies.Single(item => item.FamilyId == "sawmill");
            Assert.Equal(new TierOccupancy(1, 2), mill.Large);
            Assert.Equal(new TierOccupancy(1, 1), mill.Advanced);
            Assert.Equal(2, loaded.Snapshot().PurchaseCounters.Single(item => item.FamilyId == "sawmill").Acquired);
            session.Resume();
            loaded.Resume();
            Assert.True(session.Advance(TimeSpan.FromSeconds(20)).Succeeded);
            Assert.True(loaded.Advance(TimeSpan.FromSeconds(20)).Succeeded);
            Assert.Equal(session.Snapshot().Resources, loaded.Snapshot().Resources);
            Assert.Equal(session.Snapshot().ElapsedTicks, loaded.Snapshot().ElapsedTicks);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
