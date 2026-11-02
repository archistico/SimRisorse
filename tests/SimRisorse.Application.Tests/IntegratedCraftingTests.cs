using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class IntegratedCraftingTests
{
    [Fact]
    public void NewColonyPaysForBothChainsAndResumesTheirSharedProductionFromAHalfSecondCheckpoint()
    {
        var game = IronIndustryTests.StartIndustry();
        void Build(string id)
        {
            Assert.True(game.Construct(id, 1).Succeeded);
            Assert.True(game.AssignWorkers(id, 1).Succeeded);
        }
        Assert.True(game.Advance(TimeSpan.FromSeconds(400)).Succeeded);
        Build("gold-mine");
        Build("gold-smelter");
        Assert.True(game.Advance(TimeSpan.FromSeconds(700)).Succeeded);
        var before = game.Resources;
        foreach (var id in new[] { "coal-mine", "forge", "mint" }) Build(id);
        Assert.Equal(before["planks"].Units - 580m, game.Resources["planks"].Units);
        Assert.Equal(before["stone"].Units - 430m, game.Resources["stone"].Units);
        var purchased = game.Resources;
        Assert.True(game.Advance(TimeSpan.FromMilliseconds(500)).Succeeded);
        Assert.Equal(purchased, game.Resources);
        var resumed = GameSession.Restore(game.Scenario, game.Snapshot());
        Assert.True(resumed.IsPaused);
        Assert.True(resumed.Advance(TimeSpan.FromHours(1)).Succeeded);
        Assert.Equal(game.ElapsedTicks, resumed.ElapsedTicks);
        resumed.Resume();
        Assert.True(game.Advance(TimeSpan.FromMilliseconds(119500)).Succeeded);
        for (var part = 0; part < 239; part++)
            Assert.True(resumed.Advance(TimeSpan.FromMilliseconds(500)).Succeeded);
        Assert.Equal(2320 * TimeSpan.TicksPerSecond, game.ElapsedTicks);
        Assert.Equal(240m, game.Resources["swords"].Units);
        Assert.Equal(360m, game.Resources["coins"].Units);
        Assert.Equal(game.Resources, resumed.Resources);
        Assert.Equal(game.ElapsedTicks, resumed.ElapsedTicks);
        Assert.Equal(game.Population, resumed.Population);
        Assert.Equal(game.GrowthProgressSeconds, resumed.GrowthProgressSeconds);
        Assert.Equal(game.GrowthRemainderTwentieths, resumed.GrowthRemainderTwentieths);
        Assert.Equal(game.Snapshot().Buildings.ToArray(), resumed.Snapshot().Buildings.ToArray());
    }
}
