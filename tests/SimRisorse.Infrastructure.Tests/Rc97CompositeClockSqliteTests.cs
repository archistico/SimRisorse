using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Infrastructure.Experimental;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class Rc97CompositeClockSqliteTests
{
    private sealed class TestFile : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(),
            "simrisorse-rc97-" + Guid.NewGuid().ToString("N"));
        public TestFile() => Directory.CreateDirectory(_directory);
        public string Pathname => Path.Combine(_directory, "composite.sqlite3");
        public void Dispose() => Directory.Delete(_directory, recursive: true);
    }

    private static E4bV15FundingCompositeSqliteStore Store(TestFile file, V15BenchmarkFixture fx) =>
        new(file.Pathname, fx.Baseline, fx.School, fx.Technologies, fx.Depot);
    private static readonly ResourceBundle OnePlank = ResourceBundle.Create(("planks", 1m));
    private static E4bFundingInstruction Begin => new(E4bFundingAction.BeginMint, ProductionTier.Base);
    private static E4bFundingInstruction Deposit => new(E4bFundingAction.Deposit, Materials: OnePlank);

    [Fact]
    public void ClockAndItsReceiptSurviveReopenAndReplayDoesNotTickTwice()
    {
        var fx = V15BenchmarkFixture.Create();
        using var file = new TestFile();
        var db = Store(file, fx);
        db.Create(fx.Start);
        var applied = db.AdvanceClock("tick-1", 0, fx.Meat, fx.Crops);
        Assert.Equal(E4bFundingApplyStatus.Applied, applied.Status);
        var reopened = Store(file, fx);
        Assert.Equal(1L, reopened.Load().Revision);
        Assert.Equal(E4bFundingApplyStatus.Replayed,
            reopened.AdvanceClock("tick-1", 0, fx.Meat, fx.Crops).Status);
        Assert.Equal(applied.State.Campaign.CityCampaign.ElapsedTicks,
            reopened.Load().Campaign.CityCampaign.ElapsedTicks);
        Assert.Equal(applied.State.Funding.Stock, reopened.Load().Funding.Stock);
    }

    [Fact]
    public void ASecondClockRequestAdvancesOneMoreRealSecond()
    {
        var fx = V15BenchmarkFixture.Create();
        using var file = new TestFile();
        var db = Store(file, fx);
        db.Create(fx.Start);
        Assert.Equal(E4bFundingApplyStatus.Applied,
            db.AdvanceClock("tick-1", 0, fx.Meat, fx.Crops).Status);
        var second = db.AdvanceClock("tick-2", 1, fx.Meat, fx.Crops);
        Assert.Equal(E4bFundingApplyStatus.Applied, second.Status);
        Assert.Equal(TimeSpan.TicksPerSecond * 2, second.State.Campaign.CityCampaign.ElapsedTicks);
        Assert.True(second.State.Funding.Reconciles());
    }

    [Fact]
    public void ClockAndPaymentCannotReuseTheSameRequestId()
    {
        var fx = V15BenchmarkFixture.Create();
        using var file = new TestFile();
        var db = Store(file, fx);
        db.Create(fx.Start);
        db.AdvanceClock("once", 0, fx.Meat, fx.Crops);
        Assert.Equal(E4bFundingApplyStatus.KeyConflict, db.Apply("once", 1, Begin).Status);
        Assert.Equal(1L, db.Load().Revision);
    }

    [Fact]
    public void ClockStaleRevisionIsRejectedWithoutReceipt()
    {
        var fx = V15BenchmarkFixture.Create();
        using var file = new TestFile();
        var db = Store(file, fx);
        db.Create(fx.Start);
        Assert.Equal(E4bFundingApplyStatus.StaleRevision,
            db.AdvanceClock("future", 1, fx.Meat, fx.Crops).Status);
        Assert.Equal(E4bFundingApplyStatus.Applied,
            db.AdvanceClock("future", 0, fx.Meat, fx.Crops).Status);
    }

    [Fact]
    public void DepositsRemainReservedDuringSavedClockAdvancement()
    {
        var fx = V15BenchmarkFixture.Create();
        using var file = new TestFile();
        var db = Store(file, fx);
        db.Create(fx.Start);
        Assert.Equal(E4bFundingApplyStatus.Applied, db.Apply("begin", 0, Begin).Status);
        Assert.Equal(E4bFundingApplyStatus.Applied, db.Apply("pay", 1, Deposit).Status);
        var tick = db.AdvanceClock("tick", 2, fx.Meat, fx.Crops);
        Assert.Equal(E4bFundingApplyStatus.Applied, tick.Status);
        var recovered = Store(file, fx).Load();
        Assert.Equal(OnePlank, recovered.Funding.Escrowed);
        Assert.Equal(recovered.Campaign.CityCampaign.Resources, recovered.Funding.Stock);
        Assert.True(recovered.Funding.Reconciles());
    }

    [Fact]
    public void CancellationAfterClockRefundsMaterialsExactlyOnce()
    {
        var fx = V15BenchmarkFixture.Create();
        using var file = new TestFile();
        var db = Store(file, fx);
        db.Create(fx.Start);
        db.Apply("begin", 0, Begin);
        db.Apply("pay", 1, Deposit);
        var tick = db.AdvanceClock("tick", 2, fx.Meat, fx.Crops);
        Assert.Equal(E4bFundingApplyStatus.Applied, tick.Status);
        var canceled = db.Apply("cancel", 3, new(E4bFundingAction.Cancel));
        Assert.Equal(E4bFundingApplyStatus.Applied, canceled.Status);
        Assert.False(canceled.State.IsFunding);
        Assert.Equal(tick.State.Campaign.CityCampaign.Resources + OnePlank,
            canceled.State.Campaign.CityCampaign.Resources);
        Assert.Equal(E4bFundingApplyStatus.Replayed,
            Store(file, fx).Apply("cancel", 3, new(E4bFundingAction.Cancel)).Status);
    }
}
