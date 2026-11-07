using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using Xunit;

namespace SimRisorse.Core.Tests;

public sealed class ProgressiveConstructionEscrowTests
{
    [Fact]
    public void DepositsRemoveOnlyPaidMaterialsAndDoNotGrantAnything()
    {
        var quote = ResourceBundle.Create(("planks", 12m), ("stone", 6m));
        var escrow = ProgressiveConstructionEscrow.Start(quote);
        var first = ResourceBundle.Create(("planks", 7m), ("stone", 2m));
        Assert.True(escrow.TryDeposit(ResourceBundle.Create(("planks", 9m), ("stone", 4m)), first,
            out var funded, out var city));
        Assert.Equal(ResourceBundle.Empty, escrow.Paid);
        Assert.Equal(quote, escrow.Outstanding);
        Assert.Equal(first, funded.Paid);
        Assert.Equal(ResourceBundle.Create(("planks", 5m), ("stone", 4m)), funded.Outstanding);
        Assert.Equal(ResourceBundle.Create(("planks", 2m), ("stone", 2m)), city);
        Assert.False(funded.IsFullyPaid);
        Assert.True(funded.TryDeposit(ResourceBundle.Create(("planks", 5m), ("stone", 4m)),
            funded.Outstanding, out var complete, out var empty));
        Assert.True(complete.IsFullyPaid);
        Assert.Equal(ResourceBundle.Empty, empty);
        Assert.Equal(quote, complete.Paid);
    }

    [Fact]
    public void RejectedDepositsKeepBothSnapshotsUnchanged()
    {
        var start = ProgressiveConstructionEscrow.Start(ResourceBundle.Create(("stone", 5m)));
        var stock = ResourceBundle.Create(("stone", 4m));
        foreach (var invalid in new[]
        {
            ResourceBundle.Empty,
            ResourceBundle.Create(("stone", 6m)),
            ResourceBundle.Create(("stone", 4m), ("planks", 1m))
        })
        {
            Assert.False(start.TryDeposit(stock, invalid, out var unchanged, out var city));
            Assert.Same(start, unchanged);
            Assert.Same(stock, city);
        }
        Assert.False(start.TryDeposit(stock, ResourceBundle.Create(("stone", 5m)),
            out var stillOpen, out var stillStock));
        Assert.Same(start, stillOpen);
        Assert.Same(stock, stillStock);
        Assert.Throws<ArgumentException>(() => ProgressiveConstructionEscrow.Start(ResourceBundle.Empty));
    }

    [Fact]
    public void RefundCannotDiscardDepositsWhenCityIsFull()
    {
        var original = ProgressiveConstructionEscrow.Start(ResourceBundle.Create(("planks", 20m)));
        Assert.True(original.TryDeposit(ResourceBundle.Create(("planks", 15m)),
            ResourceBundle.Create(("planks", 15m)), out var funded, out _));
        var stock = ResourceBundle.Create(("planks", 7m));
        var capacity = ResourceBundle.Create(("planks", 20m));
        Assert.False(funded.TryRefund(stock, capacity, out var unchanged));
        Assert.Same(stock, unchanged);
        Assert.Equal(ResourceBundle.Create(("planks", 15m)), funded.Paid);
        Assert.True(funded.TryRefund(ResourceBundle.Create(("planks", 5m)), capacity, out var refunded));
        Assert.Equal(capacity, refunded);
    }

    [Fact]
    public void RefundRejectsAlreadyInvalidCityStock()
    {
        var escrow = ProgressiveConstructionEscrow.Start(ResourceBundle.Create(("planks", 2m)));
        Assert.False(escrow.TryRefund(ResourceBundle.Create(("planks", 6m)),
            ResourceBundle.Create(("planks", 5m)), out _));
    }

    [Theory]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(21, 10, 3)]
    [InlineData(1, 10, 1)]
    public void MinimumInstallmentsUseCeilingWithoutRoundingLoss(long required, long capacity, long expected)
    {
        var quote = ResourceBundle.FromAmounts([KeyValuePair.Create("stone", ResourceAmount.FromMilliunits(required))]);
        var space = ResourceBundle.FromAmounts([KeyValuePair.Create("stone", ResourceAmount.FromMilliunits(capacity))]);
        Assert.Equal(expected, ProgressiveConstructionEscrow.MinimumInstallments(quote, space));
    }

    [Fact]
    public void MultipleResourcesAndMillipointsReconcileExactly()
    {
        var quote = ResourceBundle.Create(("planks", 17.003m), ("stone", 12.001m), ("gold", .007m));
        var capacity = ResourceBundle.Create(("planks", 4m), ("stone", 2.5m), ("gold", .003m));
        var count = ProgressiveConstructionEscrow.MinimumInstallments(quote, capacity);
        Assert.Equal(5L, count);
        var sum = ResourceBundle.Empty;
        for (var i = 1L; i <= count; i++)
        {
            var instalment = ProgressiveConstructionEscrow.Installment(quote, count, i);
            Assert.True(capacity.Covers(instalment));
            sum += instalment;
        }
        Assert.Equal(quote, sum);
    }

    [Fact]
    public void ZeroRequiredCapacityIsExplicitFailureAndEmptyQuoteNeedsNoRate()
    {
        Assert.Throws<InvalidOperationException>(() => ProgressiveConstructionEscrow.MinimumInstallments(
            ResourceBundle.Create(("iron", 1m)), ResourceBundle.Create(("stone", 200m))));
        Assert.Equal(0L, ProgressiveConstructionEscrow.MinimumInstallments(ResourceBundle.Empty, ResourceBundle.Empty));
    }

    [Fact]
    public void InstallmentSupportsHugeSchedulesWithoutAllocatingThem()
    {
        var quote = ResourceBundle.FromAmounts([KeyValuePair.Create("stone",
            ResourceAmount.FromMilliunits(long.MaxValue - 1))]);
        var capacity = ResourceBundle.FromAmounts([KeyValuePair.Create("stone", ResourceAmount.FromMilliunits(1))]);
        var count = ProgressiveConstructionEscrow.MinimumInstallments(quote, capacity);
        Assert.Equal(long.MaxValue - 1, count);
        Assert.True(capacity.Covers(ProgressiveConstructionEscrow.Installment(quote, count, 1)));
        Assert.True(capacity.Covers(ProgressiveConstructionEscrow.Installment(quote, count, count)));
    }

    [Fact]
    public void InstallmentRejectsInvalidIndexOrCount()
    {
        var quote = ResourceBundle.Create(("stone", 3m));
        Assert.Throws<ArgumentOutOfRangeException>(() => ProgressiveConstructionEscrow.Installment(quote, 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ProgressiveConstructionEscrow.Installment(quote, 2, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ProgressiveConstructionEscrow.Installment(quote, 2, 3));
    }
}
