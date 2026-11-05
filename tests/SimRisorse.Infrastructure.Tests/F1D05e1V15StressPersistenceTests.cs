using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

/// <summary>RC57: physical inventory and split-clock preservation across actual SQLite boundaries.</summary>
public sealed class F1D05e1V15StressPersistenceTests
{
    private static ResourceAmount U(decimal value) => ResourceAmount.FromUnits(value);
    private static readonly SchoolResearchRules School = new("food", U(10m), 120, 1, 2, 100);
    private static readonly TechnologyCatalog Tech = TechnologyCatalog.ProposedTierTree;
    private static readonly MeatChainPolicy Meat = new("grain", "water", "cattle", "game", "meat",
        ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(1m), U(1m),
        [new("cattle", U(1m), U(2m), 0), new("game", U(1m), U(1m), 1)]);
    private static readonly HorticulturePolicy Crops = new("water", "vegetables", "fruit",
        new("vegetables", U(2m), U(1m), 0), new("fruit", U(3m), U(1m), 1));
    private static readonly FoodConversionPolicy Depot = UnifiedFoodInputs.CreatePolicy([
        new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
        new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
        new("bread", U(1m), U(1m), 4)
    ]);

    private static V15FoodCampaignCheckpoint Start()
    {
        var scenario = CivilGrowthScenario.Create();
        var city = V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), School, Tech);
        city = city with
        {
            ProductiveFamilies = [.. city.ProductiveFamilies.Select(f => f.FamilyId == "fisher"
                ? f with { Base = new TierOccupancy(1, 1) } : f)],
            PurchaseCounters = [.. city.PurchaseCounters.Select(c => c.FamilyId == "fisher"
                ? c with { Acquired = 1 } : c)]
        };
        var start = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, Depot), Depot);
        return start with
        {
            Depot = new FoodDepotStock(ResourceBundle.Empty, ResourceAmount.Zero),
            Industry = start.Industry with
            {
                Buildings = new FoodIndustryBuildings(new FoodIndustryCrew(0, 0, 0, 0, 0, 1),
                    new FoodIndustryCrew(0, 0, 0, 0, 0, 1))
            }
        };
    }

    private static V15CampaignAdvancePlan Plan(V15FoodCampaignCheckpoint start, TimeSpan elapsed) =>
        V15UnifiedCampaignClock.Plan(start, CivilGrowthScenario.Create(), School,
            Tech, FoodIndustryConstructionCatalog.Candidate, Meat, Crops, Depot, elapsed);

    private static void AssertSame(V15FoodCampaignCheckpoint expected, V15FoodCampaignCheckpoint actual)
    {
        Assert.Equal(expected.CityCampaign.ElapsedTicks, actual.CityCampaign.ElapsedTicks);
        Assert.Equal(expected.CityCampaign.Resources, actual.CityCampaign.Resources);
        Assert.Equal(expected.CityCampaign.Population, actual.CityCampaign.Population);
        Assert.Equal(expected.CityCampaign.GrowthProgressSeconds, actual.CityCampaign.GrowthProgressSeconds);
        Assert.Equal(expected.CityCampaign.GrowthRemainderTwentieths, actual.CityCampaign.GrowthRemainderTwentieths);
        Assert.Equal(expected.CityCampaign.Military, actual.CityCampaign.Military);
        Assert.Equal(expected.CityCampaign.SchoolResearch.WaitingStudents,
            actual.CityCampaign.SchoolResearch.WaitingStudents);
        Assert.Equal(expected.CityCampaign.SchoolResearch.QualifiedResearchers,
            actual.CityCampaign.SchoolResearch.QualifiedResearchers);
        Assert.Equal(expected.CityCampaign.SchoolResearch.ActiveResearchers,
            actual.CityCampaign.SchoolResearch.ActiveResearchers);
        Assert.Equal(expected.CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray(),
            actual.CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray());
        Assert.Equal(expected.CityCampaign.ResearchBalanceMilliPoints, actual.CityCampaign.ResearchBalanceMilliPoints);
        Assert.Equal(expected.CityCampaign.PencilPaidStudents, actual.CityCampaign.PencilPaidStudents);
        Assert.Equal(expected.CityCampaign.SchoolCount, actual.CityCampaign.SchoolCount);
        // SQLite loads keyed sets in ordinal ID order, whereas the in-memory
        // scenario keeps catalog order. Compare their complete values by key;
        // order is meaningful only for individual training-progress sequences.
        Assert.Equal(expected.CityCampaign.OtherBuildings.OrderBy(b => b.BuildingId, StringComparer.Ordinal).ToArray(),
            actual.CityCampaign.OtherBuildings.OrderBy(b => b.BuildingId, StringComparer.Ordinal).ToArray());
        Assert.Equal(expected.CityCampaign.ProductiveFamilies.OrderBy(f => f.FamilyId, StringComparer.Ordinal).ToArray(),
            actual.CityCampaign.ProductiveFamilies.OrderBy(f => f.FamilyId, StringComparer.Ordinal).ToArray());
        Assert.Equal(expected.CityCampaign.PurchaseCounters.OrderBy(c => c.FamilyId, StringComparer.Ordinal).ToArray(),
            actual.CityCampaign.PurchaseCounters.OrderBy(c => c.FamilyId, StringComparer.Ordinal).ToArray());
        Assert.Equal(expected.CityCampaign.UnlockedTechnologyIds.OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            actual.CityCampaign.UnlockedTechnologyIds.OrderBy(id => id, StringComparer.Ordinal).ToArray());
        Assert.Equal(expected.Industry, actual.Industry);
        Assert.Equal(expected.Depot, actual.Depot);
        Assert.Equal(V15StockAuditor.PhysicalTotals(expected), V15StockAuditor.PhysicalTotals(actual));
    }

    [Fact]
    public void KeyedCampaignCollectionsCanBeReorderedWithoutChangingCheckpointMeaning()
    {
        var initial = Start();
        var original = initial.CityCampaign;
        var reordered = initial with
        {
            CityCampaign = original with
            {
                OtherBuildings = [.. original.OtherBuildings.Reverse()],
                ProductiveFamilies = [.. original.ProductiveFamilies.Reverse()],
                PurchaseCounters = [.. original.PurchaseCounters.Reverse()],
                UnlockedTechnologyIds = [.. original.UnlockedTechnologyIds.Reverse()]
            }
        };
        AssertSame(initial, reordered);
    }

    [Fact]
    public void MultipleSqliteRestartsWithFractionalTicksMatchUninterruptedRun()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-rc57-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var store = new V15SqliteFoodSaveStore(Path.Combine(folder, "food.sqlite3"),
                CivilGrowthScenario.Create(), School, Tech, Depot);
            var initial = Start();
            var reference = Plan(initial, TimeSpan.FromSeconds(90.75)).Next;
            var intervals = new[] { TimeSpan.FromSeconds(18.25), TimeSpan.FromSeconds(19.5),
                TimeSpan.FromSeconds(23), TimeSpan.FromSeconds(30) };
            var current = initial;
            foreach (var interval in intervals)
            {
                current = Plan(current, interval).Next;
                store.Save(current);
                current = store.Load();
            }
            AssertSame(reference, current);
        }
        finally { Directory.Delete(folder, recursive: true); }
    }

    [Fact]
    public void SQLitePreservesCityAndDepotIngredientsWithDistinctIntermediateOwners()
    {
        var folder = Path.Combine(Path.GetTempPath(), "simrisorse-rc57-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var store = new V15SqliteFoodSaveStore(Path.Combine(folder, "food.sqlite3"),
                CivilGrowthScenario.Create(), School, Tech, Depot);
            var initial = Start();
            var start = initial with
            {
                CityCampaign = initial.CityCampaign with
                {
                    Resources = ResourceBundle.Create(("fish", 2m), ("bread", 3m))
                },
                Depot = new FoodDepotStock(ResourceBundle.Create(("fish", 4m), ("bread", 5m)), U(9m)),
                Industry = initial.Industry with
                {
                    Animals = ResourceBundle.Create(("cattle", 1m), ("game", 2m)),
                    Transit = ResourceBundle.Create(("meat", 3m), ("vegetables", 4m), ("fruit", 5m))
                }
            };
            store.Save(start);
            var loaded = store.Load();
            AssertSame(start, loaded);
            Assert.Equal(U(6m), V15StockAuditor.PhysicalTotals(loaded)["fish"]);
            Assert.Equal(U(8m), V15StockAuditor.PhysicalTotals(loaded)["bread"]);
            var tick = Plan(loaded, TimeSpan.FromSeconds(1));
            Assert.True(V15StockAuditor.ReconcileSecond(loaded, tick).Balanced);
            store.Save(tick.Next);
            AssertSame(tick.Next, store.Load());
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}
