using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>Preflight of the future food-depot migration; NEVER writes user saves.</summary>
public sealed class F1D05FoodMigrationTests
{
    private static readonly SchoolResearchRules School = new(
        "food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;

    private static FoodConversionPolicy Policy() => UnifiedFoodInputs.CreatePolicy([
        new("fish", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 0),
        new("bread", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 4),
        new("meat", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 1),
        new("fruit", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 2),
        new("vegetables", ResourceAmount.FromUnits(1m), ResourceAmount.FromUnits(1m), 3)
    ]);

    private static V14CampaignSnapshot Snapshot()
    {
        var scenario = CivilGrowthScenario.Create();
        return V14CampaignSnapshots.MigrateFromV13(scenario,
            GameSession.Create(scenario).Snapshot(), School, Technologies);
    }

    [Fact]
    public void StartingFoodMovesToDepotOnlyOnceWithoutTouchingNonFoodResources()
    {
        var source = Snapshot();
        var plan = V14FoodStockMigration.Prepare(source, Policy());
        Assert.Same(source, plan.SourceCheckpoint);
        Assert.Equal(source.Resources["food"], plan.DepotAfterTransfer.ReadyFood);
        Assert.Equal(source.Resources["fish"], plan.DepotAfterTransfer.RawFood["fish"]);
        Assert.Equal(source.Resources["bread"], plan.DepotAfterTransfer.RawFood["bread"]);
        Assert.Equal(ResourceAmount.Zero, plan.CityAfterTransfer["food"]);
        Assert.Equal(ResourceAmount.Zero, plan.CityAfterTransfer["fish"]);
        Assert.Equal(ResourceAmount.Zero, plan.CityAfterTransfer["bread"]);
        Assert.Equal(source.Resources["stone"], plan.CityAfterTransfer["stone"]);
        Assert.Equal(source.Resources["planks"], plan.CityAfterTransfer["planks"]);
        Assert.Equal(source.Resources, V14FoodStockMigration.RestoreLegacyResources(plan, Policy()));
        Assert.True(source.Resources["food"].Milliunits > 0); // Legacy Cibo remains in the source checkpoint.
    }

    [Fact]
    public void FractionalStocksAreTransferredAndRestoredExactly()
    {
        var initial = Snapshot();
        var extra = ResourceBundle.Create(("fish", 0.375m), ("bread", 1.125m), ("food", 0.875m));
        var source = initial with { Resources = initial.Resources + extra };
        var plan = V14FoodStockMigration.Prepare(source, Policy());
        Assert.Equal(source.Resources["fish"], plan.DepotAfterTransfer.RawFood["fish"]);
        Assert.Equal(source.Resources["bread"], plan.DepotAfterTransfer.RawFood["bread"]);
        Assert.Equal(source.Resources["food"], plan.DepotAfterTransfer.ReadyFood);
        Assert.Equal(source.Resources, V14FoodStockMigration.RestoreLegacyResources(plan, Policy()));
    }

    [Fact]
    public void BreadRemainsRawAndIsNeverPreconvertedIntoPreparedFood()
    {
        var source = Snapshot() with { Resources = ResourceBundle.Create(
            ("bread", 6m), ("fish", 4m), ("food", 2m), ("stone", 7m)) };
        var plan = V14FoodStockMigration.Prepare(source, Policy());
        Assert.Equal(6m, plan.DepotAfterTransfer.RawFood["bread"].Units);
        Assert.Equal(2m, plan.DepotAfterTransfer.ReadyFood.Units);
        Assert.Equal(4m, plan.DepotAfterTransfer.RawFood["fish"].Units);
        Assert.Equal(7m, plan.CityAfterTransfer["stone"].Units);
    }

    [Fact]
    public void EmptyHistoricalFoodStockMigratesWithoutCreatingFood()
    {
        var source = Snapshot() with { Resources = ResourceBundle.Create(("stone", 17m)) };
        var plan = V14FoodStockMigration.Prepare(source, Policy());
        Assert.Equal(ResourceBundle.Empty, plan.DepotAfterTransfer.RawFood);
        Assert.Equal(ResourceAmount.Zero, plan.DepotAfterTransfer.ReadyFood);
        Assert.Equal(source.Resources, plan.CityAfterTransfer);
        Assert.Equal(source.Resources, V14FoodStockMigration.RestoreLegacyResources(plan, Policy()));
    }

    [Fact]
    public void CapacityPreflightReportsOverfillWithoutReducingAnyStock()
    {
        var source = Snapshot() with { Resources = ResourceBundle.Create(
            ("fish", 9m), ("bread", 11m), ("food", 30m), ("planks", 8m)) };
        var plan = V14FoodStockMigration.Prepare(source, Policy());
        var check = V14FoodStockMigration.AssessCapacity(plan, Policy(),
            ResourceBundle.Create(("fish", 5m), ("bread", 7m)), ResourceAmount.FromUnits(10m));
        Assert.False(check.Fits);
        Assert.Equal(4m, check.RawShortfall["fish"].Units);
        Assert.Equal(4m, check.RawShortfall["bread"].Units);
        Assert.Equal(20m, check.ReadyFoodShortfall.Units);
        Assert.Equal(11m, plan.DepotAfterTransfer.RawFood["bread"].Units);
        Assert.Equal(30m, plan.DepotAfterTransfer.ReadyFood.Units);
        Assert.Equal(source.Resources, V14FoodStockMigration.RestoreLegacyResources(plan, Policy()));
    }

    [Fact]
    public void SufficientCapacitiesProduceNoShortfall()
    {
        var plan = V14FoodStockMigration.Prepare(Snapshot(), Policy());
        var check = V14FoodStockMigration.AssessCapacity(plan, Policy(), ResourceBundle.Create(
            ("fish", 1000m), ("bread", 1000m), ("meat", 1000m),
            ("fruit", 1000m), ("vegetables", 1000m)), ResourceAmount.FromUnits(1000m));
        Assert.True(check.Fits);
        Assert.Equal(ResourceBundle.Empty, check.RawShortfall);
    }

    [Fact]
    public void TamperedPlanThatCountsFoodTwiceOrLosesCityStockIsRejected()
    {
        var plan = V14FoodStockMigration.Prepare(Snapshot(), Policy());
        var duplicated = plan with { CityAfterTransfer = plan.CityAfterTransfer + ResourceBundle.Create(("food", 1m)) };
        Assert.Throws<ArgumentException>(() => V14FoodStockMigration.RestoreLegacyResources(duplicated, Policy()));
        var reduced = plan with { CityAfterTransfer = ResourceBundle.Empty };
        Assert.Throws<ArgumentException>(() => V14FoodStockMigration.RestoreLegacyResources(reduced, Policy()));
    }

    [Fact]
    public void MigrationDoesNotAcceptDifferentRulesOrAlreadyMigratedFormat()
    {
        var source = Snapshot();
        Assert.Throws<ArgumentException>(() => V14FoodStockMigration.Prepare(
            source with { RulesVersion = "civil-growth-15" }, Policy()));
        Assert.Throws<ArgumentException>(() => V14FoodStockMigration.Prepare(
            source with { FormatVersion = 4 }, Policy()));
    }
}
