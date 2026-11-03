using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class ProgressiveConstructionPricingTests
{
    private static readonly ResourceBundle Sawmill = ResourceBundle.Create(("planks", 40m), ("stone", 20m));

    [Fact]
    public void FirstPurchaseAndAlreadyOwnedSawmillUseDistinctPrices()
    {
        Assert.Equal(Sawmill, ProgressiveConstructionPricing.QuoteNewBuildings(Sawmill, 1m, 1.15m, 0, 1));
        Assert.Equal(ResourceBundle.Create(("planks", 46m), ("stone", 23m)),
            ProgressiveConstructionPricing.QuoteNewBuildings(Sawmill, 1m, 1.15m, 1, 1));
    }

    [Fact]
    public void FiveBuildingsAreChargedIndividuallyAndNotAtFiveTimesTheFirstPrice()
    {
        var quote = ProgressiveConstructionPricing.QuoteNewBuildings(Sawmill, 1m, 1.15m, 1, 5);
        Assert.Equal(ResourceBundle.Create(("planks", 311m), ("stone", 157m)), quote);
        Assert.NotEqual(ProgressiveConstructionPricing.QuoteNewBuildings(Sawmill, 1m, 1.15m, 1, 1).Scale(5), quote);
    }

    [Fact]
    public void UpgradesChargeOnlyTheDifferenceAndDoNotChangeFamilyHistory()
    {
        var history = ConstructionAcquisitionHistory.FromOwned([("sawmill", 1), ("storehouse", 2)]);
        Assert.Equal(ResourceBundle.Create(("planks", 74m), ("stone", 37m)),
            ProgressiveConstructionPricing.QuoteUpgrade(Sawmill, 1m, 2.6m, 1.15m, history.Acquired("sawmill")));
        Assert.Equal(1, history.Acquired("sawmill"));
        Assert.Equal(2, history.Acquired("storehouse"));
    }

    [Fact]
    public void AdvancedTierCanBeBoughtDirectlyAndUpgradedWithTheSameFamilyCounter()
    {
        var direct = ProgressiveConstructionPricing.QuoteNewBuildings(Sawmill, 6.5m, 1.15m, 1, 1);
        Assert.Equal(ResourceBundle.Create(("planks", 299m), ("stone", 150m)), direct);
        var directUpgrade = ProgressiveConstructionPricing.QuoteUpgrade(Sawmill, 1m, 6.5m, 1.15m, 1);
        Assert.Equal(ResourceBundle.Create(("planks", 253m), ("stone", 127m)), directUpgrade);
    }

    [Fact]
    public void BuildingFamilyCountersAreIndependentAndImmutable()
    {
        var initial = ConstructionAcquisitionHistory.FromOwned([("sawmill", 1), ("mine", 0)]);
        var after = initial.RegisterPurchase("sawmill", 2);
        Assert.Equal(1, initial.Acquired("sawmill"));
        Assert.Equal(3, after.Acquired("sawmill"));
        Assert.Equal(0, after.Acquired("mine"));
        Assert.Equal(0, after.Acquired("unknown"));
    }

    [Fact]
    public void QuoteIsUnchangedByHistoryUnlessARealPurchaseIsRegistered()
    {
        var history = ConstructionAcquisitionHistory.FromOwned([("sawmill", 1)]);
        var first = ProgressiveConstructionPricing.QuoteNewBuildings(Sawmill, 1m, 1.15m, history.Acquired("sawmill"), 1);
        var insufficientMaterials = ResourceBundle.Create(("planks", 1m));
        Assert.False(insufficientMaterials.Covers(first));
        Assert.Equal(1, history.Acquired("sawmill"));
        var purchased = history.RegisterPurchase("sawmill", 1);
        Assert.Equal(ResourceBundle.Create(("planks", 53m), ("stone", 27m)),
            ProgressiveConstructionPricing.QuoteNewBuildings(Sawmill, 1m, 1.15m, purchased.Acquired("sawmill"), 1));
    }

    [Fact]
    public void PurchaseUpdatesResourcesAndHistoryTogetherOnlyOnSuccess()
    {
        var history = ConstructionAcquisitionHistory.FromOwned([("sawmill", 1)]);
        var stock = ResourceBundle.Create(("planks", 100m), ("stone", 100m));
        var rejected = ProgressiveConstructionPurchase.TryPurchase("sawmill", Sawmill, 1m,
            1.15m, 5, stock, history);
        Assert.False(rejected.Succeeded);
        Assert.Same(stock, rejected.RemainingResources);
        Assert.Same(history, rejected.History);
        Assert.Equal(1, history.Acquired("sawmill"));

        var accepted = ProgressiveConstructionPurchase.TryPurchase("sawmill", Sawmill, 1m,
            1.15m, 1, stock, history);
        Assert.True(accepted.Succeeded);
        Assert.Equal(ResourceBundle.Create(("planks", 54m), ("stone", 77m)), accepted.RemainingResources);
        Assert.Equal(2, accepted.History.Acquired("sawmill"));
        Assert.Equal(1, history.Acquired("sawmill"));
    }

    [Fact]
    public void BulkPriceMatchesSplitPurchasesAtSubsequentCounters()
    {
        var single = ProgressiveConstructionPricing.QuoteNewBuildings(Sawmill, 1m, 1.15m, 1, 1);
        var rest = ProgressiveConstructionPricing.QuoteNewBuildings(Sawmill, 1m, 1.15m, 2, 4);
        var bulk = ProgressiveConstructionPricing.QuoteNewBuildings(Sawmill, 1m, 1.15m, 1, 5);
        Assert.Equal(single + rest, bulk);
    }

    [Fact]
    public void StableFactorAvoidsUnnecessaryProgressionAndPricesStayInteger()
    {
        var quote = ProgressiveConstructionPricing.QuoteNewBuildings(
            ResourceBundle.Create(("planks", 1.125m)), 2.6m, 1m, 500, 5);
        Assert.Equal(ResourceBundle.Create(("planks", 15m)), quote);
    }

    [Fact]
    public void RejectInvalidPricesAndHistoryInsteadOfClampingThem()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveConstructionPricing.QuoteNewBuildings(Sawmill, 1m, 0.99m, 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveConstructionPricing.QuoteNewBuildings(Sawmill, 1m, 1.15m, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProgressiveConstructionPricing.QuoteUpgrade(Sawmill, 2.6m, 1m, 1.15m, 1));
        Assert.Throws<ArgumentException>(() =>
            ConstructionAcquisitionHistory.FromOwned([("sawmill", 1), ("sawmill", 2)]));
    }

    [Fact]
    public void LargeQuotesFailWithNumericErrorRatherThanWrappingOrInventingResources()
    {
        Assert.Throws<OverflowException>(() =>
            ProgressiveConstructionPricing.QuoteNewBuildings(Sawmill, 1m, 1.15m, 5000, 1));
        var history = ConstructionAcquisitionHistory.FromOwned([("sawmill", int.MaxValue)]);
        Assert.Throws<OverflowException>(() => history.RegisterPurchase("sawmill", 1));
        Assert.Equal(int.MaxValue, history.Acquired("sawmill"));
    }
}
