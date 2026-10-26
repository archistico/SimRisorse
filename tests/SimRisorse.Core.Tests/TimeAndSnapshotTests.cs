using System.Collections.Immutable;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class TimeAndSnapshotTests
{
    [Theory]
    [InlineData(60)]
    [InlineData(28800)]
    public void BatchAndSubsecondAdvanceProduceIdenticalStates(int seconds)
    {
        var scenario = TestScenarios.Economy();
        var batch = GameSession.Create(scenario);
        var split = GameSession.Create(scenario);
        batch.Resume();
        split.Resume();
        Assert.True(batch.Advance(TimeSpan.FromSeconds(seconds)).Succeeded);
        for (var index = 0; index < seconds * 2; index++) split.Advance(TimeSpan.FromMilliseconds(500));
        TestScenarios.EqualState(batch.Snapshot(), split.Snapshot());
    }

    [Fact]
    public void RestoreRetainsTheFractionalSecondButAlwaysStartsPaused()
    {
        var scenario = TestScenarios.Economy();
        var game = GameSession.Create(scenario);
        game.Resume();
        game.Advance(TimeSpan.FromMilliseconds(500));
        var restored = GameSession.Restore(scenario, game.Snapshot());
        restored.Advance(TimeSpan.FromHours(8));
        Assert.True(restored.IsPaused);
        Assert.Equal(TimeSpan.TicksPerSecond / 2, restored.ElapsedTicks);
        restored.Resume();
        restored.Advance(TimeSpan.FromMilliseconds(500));
        Assert.Equal(2m, restored.Resources["logs"].Units);
        Assert.Equal(TimeSpan.TicksPerSecond, restored.ElapsedTicks);
    }

    [Fact]
    public void SnapshotCannotBeRestoredAgainstDifferentRulesOrInvalidAssignments()
    {
        var scenario = TestScenarios.Economy();
        var valid = GameSession.Create(scenario).Snapshot();
        var invalid = new[]
        {
            valid with { FormatVersion = 2 }, valid with { RulesVersion = "different" },
            valid with { ScenarioId = "different" }, valid with { Population = 9 },
            valid with { ElapsedTicks = -1 }, valid with { Resources = ResourceBundle.Create(("logs", 1001m)) },
            valid with { Buildings = [] },
            valid with { Buildings = [new("tree-feller", 1, 2), new("sawmill", 1, 1), new("stonecutter", 1, 1)] },
            valid with { Buildings = [new("tree-feller", 8, 8), new("sawmill", 1, 1), new("stonecutter", 1, 1)] },
            valid with { Buildings = [new("tree-feller", 1, 1), new("tree-feller", 1, 1), new("stonecutter", 1, 1)] }
        };
        foreach (var snapshot in invalid) Assert.Throws<ArgumentException>(() => GameSession.Restore(scenario, snapshot));
    }

    [Fact]
    public void RandomizedOrdersRetainBoundsAndOldSnapshotsRemainUnchanged()
    {
        var scenario = TestScenarios.Economy();
        var game = GameSession.Create(scenario);
        var oldSnapshot = game.Snapshot();
        var random = new Random(7341);
        game.Resume();
        for (var index = 0; index < 500; index++)
        {
            var id = scenario.Catalog.Buildings[random.Next(3)].Id;
            switch (random.Next(4))
            {
                case 0: game.Construct(id, random.Next(1, 4)); break;
                case 1: game.AssignWorkers(id, random.Next(1, 4)); break;
                case 2: game.ReleaseWorkers(id, random.Next(1, 4)); break;
                default: game.Advance(TimeSpan.FromMilliseconds(random.Next(1, 4000))); break;
            }
            var snapshot = game.Snapshot();
            Assert.InRange(snapshot.Buildings.Sum(item => item.AssignedWorkers), 0, scenario.Population);
            Assert.True(scenario.Capacity.Covers(snapshot.Resources));
            foreach (var state in snapshot.Buildings)
                Assert.InRange((long)state.AssignedWorkers, 0, (long)state.Count * scenario.Catalog.BuildingsById[state.BuildingId].WorkplacesPerBuilding);
            GameSession.Restore(scenario, snapshot);
        }
        Assert.Equal(120m, oldSnapshot.Resources["planks"].Units);
        Assert.Equal(1, oldSnapshot.Buildings.Single(item => item.BuildingId == "tree-feller").Count);
    }
}
