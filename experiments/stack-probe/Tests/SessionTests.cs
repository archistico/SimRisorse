using System.Collections.Immutable;
using System.Text.Json;
using SimRisorse.StackProbe.Model;
using Xunit;

namespace SimRisorse.StackProbe.Tests;

public sealed class SessionTests
{
    [Theory]
    [InlineData(1, 2, 120, 81)]
    [InlineData(2, 2, 121, 82)]
    [InlineData(60, 2, 179, 140)]
    public void ProductionMatchesTheScenarioTimeline(int seconds, long logs, long planks, long stone)
    {
        var session = ProbeSession.Create();
        session.Resume();
        session.Advance(TimeSpan.FromSeconds(seconds));
        Assert.Equal(new ResourceAmounts(logs, planks, stone), session.Resources);
    }

    [Fact]
    public void PausedTimeDoesNotCountAndSubsecondProgressSurvivesRestoration()
    {
        var session = ProbeSession.Create();
        session.Advance(TimeSpan.FromHours(10));
        Assert.Equal(0, session.ElapsedTicks);
        session.Resume();
        session.Advance(TimeSpan.FromMilliseconds(500));
        Assert.Equal(new ResourceAmounts(0, 120, 80), session.Resources);
        var restored = ProbeSession.Restore(session.Snapshot());
        restored.Advance(TimeSpan.FromDays(1));
        Assert.Equal(session.ElapsedTicks, restored.ElapsedTicks);
        restored.Resume();
        restored.Advance(TimeSpan.FromMilliseconds(500));
        Assert.Equal(new ResourceAmounts(2, 120, 81), restored.Resources);
        Assert.Equal(TimeSpan.TicksPerSecond, restored.ElapsedTicks);
    }

    [Fact]
    public void FailedPurchaseDoesNotSpendAnythingOrCreateAnEdifice()
    {
        var session = ProbeSession.Create();
        for (var index = 0; index < 3; index++) Assert.Equal(ActionResult.Success, session.Construct(BuildingKind.Sawmill));
        var before = JsonSerializer.Serialize(session.Snapshot());
        Assert.Equal(ActionResult.InsufficientResources, session.Construct(BuildingKind.Sawmill));
        Assert.Equal(before, JsonSerializer.Serialize(session.Snapshot()));
        Assert.Equal(1, session.GetBuilding(BuildingKind.Sawmill).Workers);
    }

    [Fact]
    public void WorkplacesAndPopulationAreBothEnforced()
    {
        var session = ProbeSession.Create();
        Assert.Equal(ActionResult.NoWorkplace, session.AssignWorker(BuildingKind.TreeFeller));
        for (var index = 0; index < 6; index++) Assert.Equal(ActionResult.Success, session.Construct(BuildingKind.TreeFeller));
        for (var index = 0; index < 5; index++) Assert.Equal(ActionResult.Success, session.AssignWorker(BuildingKind.TreeFeller));
        Assert.Equal(0, session.AvailableWorkers);
        Assert.Equal(ActionResult.NoAvailableWorker, session.AssignWorker(BuildingKind.TreeFeller));
        Assert.Equal(ActionResult.Success, session.ReleaseWorker(BuildingKind.Sawmill));
        Assert.Equal(ActionResult.Success, session.AssignWorker(BuildingKind.TreeFeller));
        Assert.Equal(ActionResult.NoAssignedWorker, session.ReleaseWorker(BuildingKind.Sawmill));
        ProbeSession.Validate(session.Snapshot());
    }

    [Fact]
    public void ATransformationCannotProduceWithoutInputs()
    {
        var session = ProbeSession.Create();
        session.ReleaseWorker(BuildingKind.TreeFeller);
        session.Resume();
        session.Advance(TimeSpan.FromSeconds(10));
        Assert.Equal(new ResourceAmounts(0, 120, 90), session.Resources);
    }

    [Fact]
    public void AFullOutputStoreStopsItsInputConsumption()
    {
        var initial = ProbeSession.Create().Snapshot() with { Resources = new(50, 1000, 80) };
        var session = ProbeSession.Restore(initial);
        session.Resume();
        session.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(new ResourceAmounts(52, 1000, 81), session.Resources);
    }

    [Fact]
    public void InputShortageRunsOnlyCompleteWorkerRecipes()
    {
        var initial = ProbeSession.Create().Snapshot() with
        {
            Resources = new(3, 0, 80),
            Buildings = [new(BuildingKind.TreeFeller, 1, 0), new(BuildingKind.Sawmill, 2, 2), new(BuildingKind.Stonecutter, 1, 1)]
        };
        var session = ProbeSession.Restore(initial);
        session.Resume();
        session.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(new ResourceAmounts(1, 1, 81), session.Resources);
    }

    [Theory]
    [InlineData(60)]
    [InlineData(28800)]
    public void DifferentAdvanceChunkSizesProduceTheSameResult(int seconds)
    {
        var batch = ProbeSession.Create();
        var split = ProbeSession.Create();
        batch.Resume();
        split.Resume();
        batch.Advance(TimeSpan.FromSeconds(seconds));
        for (var index = 0; index < seconds * 2; index++) split.Advance(TimeSpan.FromMilliseconds(500));
        Assert.Equal(JsonSerializer.Serialize(batch.Snapshot()), JsonSerializer.Serialize(split.Snapshot()));
        ProbeSession.Validate(batch.Snapshot());
    }

    [Fact]
    public void CorruptOrUnsupportedSnapshotsAreRejected()
    {
        var valid = ProbeSession.Create().Snapshot();
        var invalid = new[]
        {
            valid with { RulesVersion = "unknown" },
            valid with { FormatVersion = 2 },
            valid with { ElapsedTicks = -1 },
            valid with { Resources = new(-1, 120, 80) },
            valid with { Resources = new(0, 1001, 80) },
            valid with { Buildings = ImmutableArray<BuildingState>.Empty },
            valid with { Buildings = [new(BuildingKind.TreeFeller, 1, 2), new(BuildingKind.Sawmill, 1, 1), new(BuildingKind.Stonecutter, 1, 1)] }
        };
        foreach (var snapshot in invalid) Assert.Throws<InvalidDataException>(() => ProbeSession.Restore(snapshot));
    }
}
