using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>
/// The same decisions at the same simulation instants must not depend on how the
/// caller groups elapsed time into Advance calls.
/// </summary>
public sealed class AdvancedHousingReplayTests
{
    [Fact]
    public void AdvancedHousingProducesTheSameStateWithSecondTicksOrIrregularTimeBatches()
    {
        var singleSeconds = PrepareFirstAdvancedResident(irregular: false);
        var irregular = PrepareFirstAdvancedResident(irregular: true);
        Equal(singleSeconds.Snapshot(), irregular.Snapshot());

        // The checkpoint is at 879.5 s: population 17, with 59 valid seconds
        // accumulated toward the next resident. The next half-second is decisive.
        Assert.Equal(17, singleSeconds.Population);
        Assert.Equal(59, singleSeconds.GrowthProgressSeconds);
        Assert.True(singleSeconds.Resources["bread"].Units > 0m);
        AdvanceBothTo(TimeSpan.FromSeconds(880));
        Assert.Equal(18, singleSeconds.Population);
        Assert.Equal(0, singleSeconds.GrowthProgressSeconds);

        AdvanceBothTo(TimeSpan.FromSeconds(3000));
        Assert.Equal(48, singleSeconds.Population);
        Assert.True(singleSeconds.Construct("large-house", 1).Succeeded);
        Assert.True(irregular.Construct("large-house", 1).Succeeded);
        Equal(singleSeconds.Snapshot(), irregular.Snapshot());

        AdvanceBothTo(TimeSpan.FromHours(2));
        Assert.Equal(80, singleSeconds.Population);
        Assert.Equal(80, singleSeconds.HousingCapacity);
        Assert.Equal(16, singleSeconds.ForecastEconomy().Population.BasicHousingCapacity);
        Assert.True(singleSeconds.Resources["bread"].Units > 0m);

        void AdvanceBothTo(TimeSpan time)
        {
            AdvanceTo(singleSeconds, time, irregular: false);
            AdvanceTo(irregular, time, irregular: true);
            Equal(singleSeconds.Snapshot(), irregular.Snapshot());
        }
    }

    [Fact]
    public void MidSecondRestorationAndPausePreserveBreadStocksGrowthAndFutureArrivals()
    {
        var uninterrupted = PrepareFirstAdvancedResident(irregular: false);
        var saved = uninterrupted.Snapshot();
        var restored = GameSession.Restore(uninterrupted.Scenario, saved);
        Assert.True(restored.IsPaused);
        Assert.True(restored.Advance(TimeSpan.FromHours(8)).Succeeded);
        Equal(saved, restored.Snapshot());

        restored.Resume();
        AdvanceTo(uninterrupted, TimeSpan.FromSeconds(880), irregular: false);
        AdvanceTo(restored, TimeSpan.FromSeconds(880), irregular: true);
        Equal(uninterrupted.Snapshot(), restored.Snapshot());
        Assert.Equal(18, restored.Population);

        AdvanceTo(uninterrupted, TimeSpan.FromSeconds(3000), irregular: false);
        AdvanceTo(restored, TimeSpan.FromSeconds(3000), irregular: true);
        Equal(uninterrupted.Snapshot(), restored.Snapshot());
        Assert.True(uninterrupted.Construct("large-house", 1).Succeeded);
        Assert.True(restored.Construct("large-house", 1).Succeeded);

        AdvanceTo(uninterrupted, TimeSpan.FromHours(2), irregular: false);
        AdvanceTo(restored, TimeSpan.FromHours(2), irregular: true);
        Equal(uninterrupted.Snapshot(), restored.Snapshot());
        Assert.Equal(80, restored.Population);
    }

    private static GameSession PrepareFirstAdvancedResident(bool irregular)
    {
        // Preserve the fixed-60-second M4 historical replay as a regression fixture.
        var game = GameSession.Create(CivilGrowthScenario.CreateLegacyForBenchmarks());
        BuildAndStaff(game, "fisher");
        BuildAndStaff(game, "food-store");
        game.Resume();
        AdvanceTo(game, TimeSpan.FromSeconds(61), irregular);
        Assert.True(game.Construct("small-house", 1).Succeeded);

        AdvanceTo(game, TimeSpan.FromSeconds(600), irregular);
        Assert.Equal(16, game.Population);
        foreach (var id in new[] { "farm", "farm", "well", "mill", "bakery" })
            BuildAndStaff(game, id);

        AdvanceTo(game, TimeSpan.FromSeconds(760), irregular);
        Assert.Equal(99m, game.Resources["bread"].Units);
        Assert.True(game.Construct("large-house", 1).Succeeded);
        AdvanceTo(game, TimeSpan.FromMilliseconds(879500), irregular);
        return game;
    }

    private static void BuildAndStaff(GameSession game, string id)
    {
        Assert.True(game.Construct(id, 1).Succeeded);
        Assert.True(game.AssignWorkers(id, 1).Succeeded);
    }

    private static void AdvanceTo(GameSession game, TimeSpan time, bool irregular)
    {
        long[] chunks = irregular
            ? [TimeSpan.FromSeconds(73).Ticks, TimeSpan.FromMilliseconds(137).Ticks,
                TimeSpan.FromMilliseconds(18750).Ticks, TimeSpan.FromMilliseconds(250).Ticks,
                TimeSpan.FromSeconds(91).Ticks]
            : [TimeSpan.TicksPerSecond];
        var chunk = 0;
        while (game.ElapsedTicks < time.Ticks)
        {
            var step = Math.Min(chunks[chunk++ % chunks.Length], time.Ticks - game.ElapsedTicks);
            Assert.True(game.Advance(TimeSpan.FromTicks(step)).Succeeded);
        }
        Assert.Equal(time.Ticks, game.ElapsedTicks);
    }

    private static void Equal(GameSnapshot expected, GameSnapshot actual)
    {
        Assert.Equal(expected.FormatVersion, actual.FormatVersion);
        Assert.Equal(expected.ScenarioId, actual.ScenarioId);
        Assert.Equal(expected.RulesVersion, actual.RulesVersion);
        Assert.Equal(expected.ElapsedTicks, actual.ElapsedTicks);
        Assert.Equal(expected.Population, actual.Population);
        Assert.Equal(expected.GrowthProgressSeconds, actual.GrowthProgressSeconds);
        Assert.Equal(expected.GrowthRemainderTwentieths, actual.GrowthRemainderTwentieths);
        Assert.Equal(expected.Resources, actual.Resources);
        Assert.Equal(expected.Buildings.ToArray(), actual.Buildings.ToArray());
    }
}
