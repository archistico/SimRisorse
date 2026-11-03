using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>Isolated E3b migration tests. The live scenario and the SQLite save remain v13.</summary>
public sealed class V14CampaignMigrationTests
{
    private static readonly SchoolResearchRules School = new("food", ResourceAmount.FromUnits(10m),
        120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;

    private static (SimRisorse.Core.Content.GameScenario Scenario, GameSnapshot Original,
        V14CampaignSnapshot Migrated) Migrate()
    {
        var scenario = CivilGrowthScenario.Create();
        var original = GameSession.Create(scenario).Snapshot() with { ElapsedTicks = TimeSpan.FromMinutes(9).Ticks };
        return (scenario, original, V14CampaignSnapshots.MigrateFromV13(scenario, original, School, Technologies));
    }

    [Fact]
    public void MigrationPreservesV13StocksPopulationMilitaryTimeAndMunicipio()
    {
        var (scenario, original, next) = Migrate();
        Assert.Equal(3, next.FormatVersion);
        Assert.Equal("civil-growth-14", next.RulesVersion);
        Assert.Equal(original.ScenarioId, next.ScenarioId);
        Assert.Equal(original.Population, next.Population);
        Assert.Equal(original.ElapsedTicks, next.ElapsedTicks);
        Assert.Same(original.Resources, next.Resources);
        Assert.Same(original.Military, next.Military);
        Assert.Equal(original.GrowthProgressSeconds, next.GrowthProgressSeconds);
        Assert.Equal(original.GrowthRemainderTwentieths, next.GrowthRemainderTwentieths);
        Assert.Contains(next.OtherBuildings, b => b.BuildingId == "town-hall" && b.Count == 1);
        Assert.Equal(16, next.ProductiveFamilies.Length);
        Assert.All(next.ProductiveFamilies, family =>
        {
            var old = original.Buildings.Single(b => b.BuildingId == family.FamilyId);
            Assert.Equal(new TierOccupancy(old.Count, old.AssignedWorkers), family.Base);
            Assert.Equal(new TierOccupancy(0, 0), family.Large);
            Assert.Equal(new TierOccupancy(0, 0), family.Advanced);
        });
        Assert.Equal(0, next.SchoolCount);
        Assert.Equal(0, next.SchoolResearch.ReservedPopulation);
        Assert.Empty(next.UnlockedTechnologyIds);
        Assert.Equal(0, next.ResearchBalanceMilliPoints);
        V14CampaignSnapshots.Validate(scenario, next, School, Technologies);
        Assert.Equal("civil-growth-13", original.RulesVersion); // migration has no side effects
    }

    [Fact]
    public void ExistingBuildingsInitializeMonotonicCountersAndSchoolStartsAtZero()
    {
        var (_, original, next) = Migrate();
        foreach (var legacy in original.Buildings.Where(b => b.BuildingId != "town-hall"))
            Assert.Equal(legacy.Count, next.PurchaseCounters.Single(c => c.FamilyId == legacy.BuildingId).Acquired);
        Assert.Equal(0, next.PurchaseCounters.Single(c => c.FamilyId == "school").Acquired);
        Assert.DoesNotContain(next.PurchaseCounters, c => c.FamilyId == "town-hall");
    }

    [Fact]
    public void UpgradeNeedsPurchasedTechnologyAndDoesNotChangeBuildingCountOrHistory()
    {
        var (scenario, _, next) = Migrate();
        var source = next.ProductiveFamilies.Single(f => f.FamilyId == "sawmill");
        // A single new sawmill must have paid its cost before a later upgrade.
        var built = source with { Base = new TierOccupancy(1, 1) };
        var owned = next with
        {
            ProductiveFamilies = next.ProductiveFamilies.Select(f => f.FamilyId == source.FamilyId ? built : f).ToImmutableArray(),
            PurchaseCounters = next.PurchaseCounters.Select(c => c.FamilyId == "sawmill"
                ? c with { Acquired = 1 } : c).ToImmutableArray()
        };
        V14CampaignSnapshots.Validate(scenario, owned, School, Technologies);
        var upgraded = owned with
        {
            ProductiveFamilies = owned.ProductiveFamilies.Select(f => f.FamilyId == built.FamilyId
                ? built with { Base = default, Large = new TierOccupancy(1, 1) } : f).ToImmutableArray()
        };
        Assert.Throws<ArgumentException>(() => V14CampaignSnapshots.Validate(scenario, upgraded, School, Technologies));
        var purchased = upgraded with { UnlockedTechnologyIds = ["lavorazioni-1"] };
        V14CampaignSnapshots.Validate(scenario, purchased, School, Technologies);
        Assert.Equal(1, purchased.PurchaseCounters.Single(c => c.FamilyId == "sawmill").Acquired);
    }

    [Fact]
    public void CorruptCountsDuplicateFamiliesAndMissingHistoryAreRejected()
    {
        var (scenario, _, next) = Migrate();
        Assert.Throws<ArgumentException>(() => V14CampaignSnapshots.Validate(scenario,
            next with { ProductiveFamilies = next.ProductiveFamilies.Add(next.ProductiveFamilies[0]) }, School, Technologies));
        Assert.Throws<ArgumentException>(() => V14CampaignSnapshots.Validate(scenario,
            next with { PurchaseCounters = next.PurchaseCounters.RemoveAt(0) }, School, Technologies));
        Assert.Throws<ArgumentException>(() => V14CampaignSnapshots.Validate(scenario,
            next with { RulesVersion = "civil-growth-13" }, School, Technologies));
        Assert.Throws<ArgumentException>(() => V14CampaignSnapshots.Validate(scenario,
            next with { OtherBuildings = next.OtherBuildings.Select(b => b.BuildingId == "town-hall"
                ? b with { Count = 2 } : b).ToImmutableArray() }, School, Technologies));
        Assert.Throws<ArgumentException>(() => V14CampaignSnapshots.Validate(scenario,
            next with { PurchaseCounters = next.PurchaseCounters.Select(c => c.FamilyId == "small-house"
                ? c with { Acquired = 0 } : c).ToImmutableArray() }, School, Technologies));
    }

    [Fact]
    public void ResearchCannotReassignMilitaryOrCivilianPopulation()
    {
        var (scenario, _, next) = Migrate();
        // A qualified researcher still counts as a resident and is reserved from other jobs.
        var researcher = next with
        {
            SchoolCount = 1,
            SchoolResearch = new SchoolResearchState(0, ImmutableArray<int>.Empty, 1, 1),
            PurchaseCounters = next.PurchaseCounters.Select(c => c.FamilyId == "school"
                ? c with { Acquired = 1 } : c).ToImmutableArray()
        };
        V14CampaignSnapshots.Validate(scenario, researcher, School, Technologies);
        Assert.Throws<ArgumentException>(() => V14CampaignSnapshots.Validate(scenario,
            researcher with { SchoolResearch = new SchoolResearchState(0, [], next.Population + 1, 1) },
            School, Technologies));
    }

    [Fact]
    public void SavingResearchPointsNeverAutoPurchasesTechnologies()
    {
        var (scenario, _, next) = Migrate();
        var accumulated = next with { ResearchBalanceMilliPoints = 100_000 };
        V14CampaignSnapshots.Validate(scenario, accumulated, School, Technologies);
        Assert.Empty(accumulated.UnlockedTechnologyIds);
        Assert.Equal(100_000, accumulated.ResearchBalanceMilliPoints);
        Assert.Equal(0, next.ResearchBalanceMilliPoints);
        Assert.Throws<ArgumentException>(() => V14CampaignSnapshots.Validate(scenario,
            accumulated with { UnlockedTechnologyIds = ["industria-2"] }, School, Technologies));
    }

    [Fact]
    public void HistoricalAndFutureCheckpointsCannotMasqueradeAsV13Migration()
    {
        var (scenario, original, _) = Migrate();
        Assert.Throws<ArgumentException>(() => V14CampaignSnapshots.MigrateFromV13(scenario,
            original with { RulesVersion = "civil-growth-12" }, School, Technologies));
        Assert.Throws<ArgumentException>(() => V14CampaignSnapshots.MigrateFromV13(scenario,
            original with { RulesVersion = "civil-growth-14" }, School, Technologies));
        Assert.Throws<ArgumentException>(() => V14CampaignSnapshots.MigrateFromV13(scenario,
            original with { Population = 1000000 }, School, Technologies));
    }
}
