using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class AcceleratedHousingGrowthTests
{
    [Fact]
    public void EachExtraSmallHouseAddsFifteenPercentAndRetainsFractionalProgress()
    {
        var scenario = CivilGrowthScenario.Create();
        var game = GameSession.Create(scenario);
        Assert.Equal(GrowthBlockReason.HousingFull, game.ForecastEconomy().Population.BlockReason);
        Assert.Equal(20, game.ForecastEconomy().Population.GrowthRateTwentieths);
        Assert.True(game.Construct("small-house", 1).Succeeded);
        Assert.Equal(23, game.ForecastEconomy().Population.GrowthRateTwentieths);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(52));
        Assert.Equal(8, game.Population);
        Assert.Equal(59, game.GrowthProgressSeconds);
        Assert.Equal(16, game.GrowthRemainderTwentieths);
        game.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(9, game.Population);
        Assert.Equal(0, game.GrowthProgressSeconds);
        Assert.Equal(19, game.GrowthRemainderTwentieths);
        Assert.Equal(0.09m, game.ForecastEconomy().Population.RequiredFood.Units);
    }

    [Fact]
    public void LargeHouseAddsThirtyPercentAndAllHousesTogetherStopAtDoubleSpeed()
    {
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot();
        GameSnapshot WithHousing(int small, int large) => initial with
        {
            Resources = ResourceBundle.Create(("food", 100m), ("bread", 100m)),
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId switch
            {
                "small-house" => item with { Count = small },
                "large-house" => item with { Count = large },
                _ => item
            })]
        };

        var large = GameSession.Restore(scenario, WithHousing(1, 1));
        Assert.Equal(26, large.ForecastEconomy().Population.GrowthRateTwentieths);
        large.Resume();
        large.Advance(TimeSpan.FromSeconds(46));
        Assert.Equal(8, large.Population);
        large.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(9, large.Population);
        Assert.Equal(1, large.GrowthProgressSeconds);
        Assert.Equal(2, large.GrowthRemainderTwentieths);

        var capped = GameSession.Restore(scenario, WithHousing(10, 4));
        Assert.Equal(40, capped.ForecastEconomy().Population.GrowthRateTwentieths);
        capped.Resume();
        capped.Advance(TimeSpan.FromSeconds(29));
        Assert.Equal(8, capped.Population);
        capped.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(9, capped.Population);
        Assert.Equal(0, capped.GrowthRemainderTwentieths);
    }

    [Fact]
    public void BuildingDuringProgressChangesOnlyTheRateAndPauseAndRestoreAreExact()
    {
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot() with
        {
            Resources = ResourceBundle.Create(("food", 100m), ("planks", 1000m), ("stone", 1000m))
        };
        var game = GameSession.Restore(scenario, initial);
        Assert.True(game.Construct("small-house", 1).Succeeded);
        game.Resume();
        game.Advance(TimeSpan.FromSeconds(20));
        Assert.Equal(23, game.GrowthProgressSeconds);
        Assert.True(game.Construct("small-house", 1).Succeeded);
        Assert.Equal(26, game.ForecastEconomy().Population.GrowthRateTwentieths);
        var checkpoint = game.Snapshot();
        var resumed = GameSession.Restore(scenario, checkpoint);
        resumed.Advance(TimeSpan.FromHours(3)); // Paused: no offline births or fractional changes.
        Equal(checkpoint, resumed.Snapshot());
        resumed.Resume();
        game.Advance(TimeSpan.FromSeconds(28));
        resumed.Advance(TimeSpan.FromSeconds(11));
        resumed.Advance(TimeSpan.FromSeconds(17));
        Equal(game.Snapshot(), resumed.Snapshot());
        Assert.Equal(8, game.Population);
        game.Advance(TimeSpan.FromSeconds(1));
        resumed.Advance(TimeSpan.FromSeconds(1));
        Equal(game.Snapshot(), resumed.Snapshot());
        Assert.Equal(9, game.Population);
        Assert.Equal(0, game.GrowthProgressSeconds);
        Assert.Equal(14, game.GrowthRemainderTwentieths);
    }

    [Fact]
    public void StarvationAndFullHousingFreezeBothPartsOfTheGrowthProgress()
    {
        var scenario = CivilGrowthScenario.Create();
        var initial = GameSession.Create(scenario).Snapshot();
        var saved = initial with
        {
            Population = 9, GrowthProgressSeconds = 12, GrowthRemainderTwentieths = 7,
            Resources = ResourceBundle.Empty,
            Buildings = [.. initial.Buildings.Select(item => item.BuildingId == "small-house" ? item with { Count = 2 } : item)]
        };
        var hungry = GameSession.Restore(scenario, saved);
        Assert.Equal(GrowthBlockReason.MissingFood, hungry.ForecastEconomy().Population.BlockReason);
        hungry.Resume();
        hungry.Advance(TimeSpan.FromSeconds(100));
        Assert.Equal(9, hungry.Population);
        Assert.Equal(12, hungry.GrowthProgressSeconds);
        Assert.Equal(7, hungry.GrowthRemainderTwentieths);

        var full = GameSession.Restore(scenario, saved with
        {
            Population = 16, Resources = ResourceBundle.Create(("food", 100m))
        });
        Assert.Equal(GrowthBlockReason.HousingFull, full.ForecastEconomy().Population.BlockReason);
        full.Resume();
        full.Advance(TimeSpan.FromSeconds(100));
        Assert.Equal(12, full.GrowthProgressSeconds);
        Assert.Equal(7, full.GrowthRemainderTwentieths);
    }

    private static void Equal(GameSnapshot expected, GameSnapshot actual)
    {
        Assert.Equal(expected.ElapsedTicks, actual.ElapsedTicks);
        Assert.Equal(expected.Population, actual.Population);
        Assert.Equal(expected.GrowthProgressSeconds, actual.GrowthProgressSeconds);
        Assert.Equal(expected.GrowthRemainderTwentieths, actual.GrowthRemainderTwentieths);
        Assert.Equal(expected.Resources, actual.Resources);
        Assert.Equal(expected.Buildings.ToArray(), actual.Buildings.ToArray());
    }
}
