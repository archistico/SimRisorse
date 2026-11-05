using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Population;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class V15HousingGrowthTests
{
    private static V15FoodCampaignCheckpoint Seed(V15BenchmarkFixture setup, int small, int large) =>
        setup.Start with
        {
            CityCampaign = setup.Start.CityCampaign with
            {
                OtherBuildings = [.. setup.Start.CityCampaign.OtherBuildings.Select(b => b.BuildingId switch
                {
                    "small-house" => b with { Count = small },
                    "large-house" => b with { Count = large },
                    _ => b
                })],
                PurchaseCounters = [.. setup.Start.CityCampaign.PurchaseCounters.Select(c => c.FamilyId switch
                {
                    "small-house" => c with { Acquired = small },
                    "large-house" => c with { Acquired = large },
                    _ => c
                })]
            },
            Depot = new FoodDepotStock(ResourceBundle.Create(("bread", 100m)), ResourceAmount.FromUnits(100m))
        };

    private static V15FoodIndustryCampaignSession Session(V15BenchmarkFixture setup, V15FoodCampaignCheckpoint state) =>
        new(state, setup.Baseline, setup.School, setup.Technologies, setup.Depot);

    [Theory]
    [InlineData(1, 0, 20)]
    [InlineData(2, 0, 23)]
    [InlineData(1, 1, 26)]
    [InlineData(8, 0, 41)]
    [InlineData(127, 0, 398)]
    [InlineData(128, 0, 400)]
    [InlineData(1, 64, 400)]
    public void ForecastAndClockShareTheHousingBonusUpToTwentyfold(int small, int large, int expectedRate)
    {
        var setup = V15BenchmarkFixture.Create();
        var state = Seed(setup, small, large);
        var forecast = V15CampaignGrowthProjection.Project(setup.Baseline, state);
        var full = small == 1 && large == 0;
        Assert.Equal(full ? GrowthBlockReason.HousingFull : GrowthBlockReason.None, forecast.BlockReason);
        Assert.Equal(expectedRate, forecast.GrowthRateTwentieths);
        var session = Session(setup, state);
        session.Resume();
        Assert.True(session.Advance(TimeSpan.FromSeconds(1), setup.Meat, setup.Crops).Succeeded);
        var after = session.Snapshot().CityCampaign;
        Assert.Equal(state.CityCampaign.Population, after.Population);
        Assert.Equal(full ? 0 : expectedRate, after.GrowthProgressSeconds * HousingGrowthRules.UnitsPerSecond +
            after.GrowthRemainderTwentieths);
    }

    [Fact]
    public void TwentyfoldGrowthAndFractionalClockResumeMatchContinuousPlay()
    {
        var setup = V15BenchmarkFixture.Create();
        var state = Seed(setup, 128, 0);
        var continuous = Session(setup, state);
        continuous.Resume();
        Assert.True(continuous.Advance(TimeSpan.FromSeconds(6), setup.Meat, setup.Crops).Succeeded);

        var split = Session(setup, state);
        split.Resume();
        Assert.True(split.Advance(TimeSpan.FromSeconds(2), setup.Meat, setup.Crops).Succeeded);
        Assert.Equal(8, split.Snapshot().CityCampaign.Population);
        Assert.Equal(40, split.Snapshot().CityCampaign.GrowthProgressSeconds);
        Assert.True(split.Advance(TimeSpan.FromSeconds(1), setup.Meat, setup.Crops).Succeeded);
        Assert.Equal(9, split.Snapshot().CityCampaign.Population);
        Assert.Equal(0, split.Snapshot().CityCampaign.GrowthProgressSeconds);
        Assert.True(split.Advance(TimeSpan.FromMilliseconds(1500), setup.Meat, setup.Crops).Succeeded);

        var checkpoint = split.Snapshot();
        var restored = Session(setup, checkpoint);
        Assert.True(restored.IsPaused);
        Assert.True(restored.Advance(TimeSpan.FromHours(1), setup.Meat, setup.Crops).Succeeded);
        Assert.Equal(checkpoint, restored.Snapshot());
        restored.Resume();
        Assert.True(restored.Advance(TimeSpan.FromMilliseconds(500), setup.Meat, setup.Crops).Succeeded);
        Assert.True(restored.Advance(TimeSpan.FromSeconds(1), setup.Meat, setup.Crops).Succeeded);

        var expected = continuous.Snapshot();
        var actual = restored.Snapshot();
        Assert.Equal(10, actual.CityCampaign.Population);
        Assert.Equal(expected.CityCampaign.ElapsedTicks, actual.CityCampaign.ElapsedTicks);
        Assert.Equal(expected.CityCampaign.Population, actual.CityCampaign.Population);
        Assert.Equal(expected.CityCampaign.GrowthProgressSeconds, actual.CityCampaign.GrowthProgressSeconds);
        Assert.Equal(expected.CityCampaign.GrowthRemainderTwentieths, actual.CityCampaign.GrowthRemainderTwentieths);
        Assert.Equal(V15StockAuditor.PhysicalTotals(expected), V15StockAuditor.PhysicalTotals(actual));
    }

    [Theory]
    [InlineData("food", GrowthBlockReason.MissingFood)]
    [InlineData("housing", GrowthBlockReason.HousingFull)]
    [InlineData("bread", GrowthBlockReason.MissingBread)]
    public void TwentyfoldBonusStillRequiresFoodBreadAndFreeHousing(string constraint, GrowthBlockReason reason)
    {
        var setup = V15BenchmarkFixture.Create();
        var state = constraint == "bread" ? Seed(setup, 1, 64) : Seed(setup, 128, 0);
        state = state with
        {
            CityCampaign = state.CityCampaign with
            {
                Population = constraint switch { "housing" => 1024, "bread" => 9, _ => 8 },
                GrowthProgressSeconds = 19
            },
            Depot = state.Depot with
            {
                ReadyFood = constraint == "food" ? ResourceAmount.Zero : state.Depot.ReadyFood,
                RawFood = constraint == "bread" ? ResourceBundle.Empty : state.Depot.RawFood
            }
        };
        var forecast = V15CampaignGrowthProjection.Project(setup.Baseline, state);
        Assert.Equal(400, forecast.GrowthRateTwentieths);
        Assert.Equal(reason, forecast.BlockReason);
        var session = Session(setup, state);
        session.Resume();
        Assert.True(session.Advance(TimeSpan.FromSeconds(5), setup.Meat, setup.Crops).Succeeded);
        Assert.Equal(state.CityCampaign.Population, session.Snapshot().CityCampaign.Population);
        Assert.Equal(19, session.Snapshot().CityCampaign.GrowthProgressSeconds);
        Assert.Equal(0, session.Snapshot().CityCampaign.GrowthRemainderTwentieths);
    }
}
