using SimRisorse.Core.Research;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class ResearchAccountingTests
{
    [Fact]
    public void PointsAccumulateWithoutSelectingAResearchProject()
    {
        var balance = ResearchAccounting.Accrue(ResearchPoints.Zero, 2, 500, 100);
        Assert.Equal(100m, balance.Units);
        Assert.Equal(ResearchPoints.FromUnits(100m), balance);
    }

    [Fact]
    public void SpendingIsOptionalAndPreservesUnspentKnowledge()
    {
        var initial = ResearchPoints.FromUnits(125.25m);
        Assert.True(ResearchAccounting.TrySpend(initial, ResearchPoints.FromUnits(100m), out var remainder));
        Assert.Equal(25.25m, remainder.Units);
        Assert.Equal(125.25m, initial.Units);
        Assert.Equal(35.25m, ResearchAccounting.Accrue(remainder, 1, 100, 100).Units);
    }

    [Fact]
    public void InsufficientBalanceCannotUnlockAnythingOrBecomeNegative()
    {
        var balance = ResearchPoints.FromUnits(99.999m);
        Assert.False(ResearchAccounting.TrySpend(balance, ResearchPoints.FromUnits(100m), out var unchanged));
        Assert.Equal(balance, unchanged);
    }

    [Fact]
    public void NoResearchersOrNoElapsedTimeCannotAccruePoints()
    {
        var balance = ResearchPoints.FromUnits(2m);
        Assert.Equal(balance, ResearchAccounting.Accrue(balance, 0, 100, 100));
        Assert.Equal(balance, ResearchAccounting.Accrue(balance, 2, 0, 100));
    }

    [Fact]
    public void InvalidValuesAndOverflowAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ResearchPoints.FromUnits(-1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => ResearchPoints.FromUnits(0.0001m));
        Assert.Throws<ArgumentOutOfRangeException>(() => ResearchAccounting.Accrue(ResearchPoints.Zero, -1, 1, 100));
        Assert.Throws<OverflowException>(() => ResearchAccounting.Accrue(ResearchPoints.Zero, int.MaxValue, long.MaxValue, 100));
    }
}
