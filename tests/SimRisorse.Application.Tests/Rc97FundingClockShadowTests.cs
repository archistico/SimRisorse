using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class Rc97FundingClockShadowTests
{
    private static readonly ResourceBundle OnePlank = ResourceBundle.Create(("planks", 1m));

    private static (V15BenchmarkFixture Rules, E4bV15FundingShadow Shadow) New()
    {
        var fx = V15BenchmarkFixture.Create();
        return (fx, E4bV15FundingShadow.Open(fx.Baseline, fx.School,
            fx.Technologies, fx.Depot, fx.Start));
    }

    [Fact]
    public void RealSecondAdvancesNativeCityWithoutCreatingOrBuyingMint()
    {
        var (fx, start) = New();
        Assert.True(start.TryAdvanceClock(fx.Meat, fx.Crops, 0, out var next));
        Assert.Equal(TimeSpan.TicksPerSecond,
            next.Campaign.CityCampaign.ElapsedTicks - start.Campaign.CityCampaign.ElapsedTicks);
        Assert.Equal(1L, next.Revision);
        Assert.Equal(start.Funding.AcquiredMints, next.Funding.AcquiredMints);
        Assert.False(next.IsFunding);
        Assert.Equal(next.Campaign.CityCampaign.Resources, next.Funding.Stock);
        // This is a running v15 city, not a simulated elapsed-time counter:
        // the staffed initial stonecutter really adds one unit to city stock.
        Assert.Equal(1m, next.Campaign.CityCampaign.Resources["stone"].Units -
            start.Campaign.CityCampaign.Resources["stone"].Units);
        Assert.Equal(1m, next.Funding.RealClockCredits["stone"].Units);
        Assert.True(next.Funding.Reconciles());
    }

    [Fact]
    public void PendingEscrowIsUnaffectedByRealClock()
    {
        var (fx, start) = New();
        Assert.True(start.TryBeginMint(ProductionTier.Base, 0, out var begun));
        Assert.True(begun.TryDeposit(OnePlank, 1, out var paid));
        Assert.True(paid.TryAdvanceClock(fx.Meat, fx.Crops, 2, out var tick));
        Assert.Equal(OnePlank, tick.Funding.Escrowed);
        Assert.Equal(paid.Funding.Escrow!.Quote, tick.Funding.Escrow!.Quote);
        Assert.Equal(paid.Funding.AcquiredMints, tick.Funding.AcquiredMints);
        Assert.Equal(tick.Campaign.CityCampaign.Resources, tick.Funding.Stock);
        Assert.True(tick.Funding.Reconciles());
    }

    [Fact]
    public void ClockAfterDepositSurvivesCompoundCheckpointRestore()
    {
        var (fx, start) = New();
        Assert.True(start.TryBeginMint(ProductionTier.Base, 0, out var begun));
        Assert.True(begun.TryDeposit(OnePlank, 1, out var paid));
        Assert.True(paid.TryAdvanceClock(fx.Meat, fx.Crops, 2, out var tick));
        var recovered = E4bV15FundingShadow.Restore(tick.ExportCheckpoint(), fx.Baseline,
            fx.School, fx.Technologies, fx.Depot);
        Assert.Equal(tick.Revision, recovered.Revision);
        Assert.Equal(tick.Campaign, recovered.Campaign);
        Assert.Equal(tick.Funding.Escrowed, recovered.Funding.Escrowed);
        Assert.Equal(tick.Funding.RealClockCredits, recovered.Funding.RealClockCredits);
        Assert.Equal(tick.Funding.RealClockDebits, recovered.Funding.RealClockDebits);
    }

    [Fact]
    public void StaleRevisionDoesNotAdvanceRealClock()
    {
        var (fx, start) = New();
        Assert.False(start.TryAdvanceClock(fx.Meat, fx.Crops, 1, out var unchanged));
        Assert.Same(start, unchanged);
        Assert.Equal(0L, start.Campaign.CityCampaign.ElapsedTicks);
    }

    [Fact]
    public void ForgedRealClockLedgerDoesNotPassRestore()
    {
        var (fx, start) = New();
        var broken = start.ExportCheckpoint();
        broken = broken with { Funding = broken.Funding with
            { RealClockCredits = OnePlank, RealClockDebits = ResourceBundle.Empty } };
        Assert.Throws<ArgumentException>(() => E4bV15FundingShadow.Restore(broken,
            fx.Baseline, fx.School, fx.Technologies, fx.Depot));
    }

    [Fact]
    public void LegacyFundingCheckpointWithoutClockFieldsStillRestores()
    {
        var (fx, start) = New();
        var saved = start.ExportCheckpoint();
        saved = saved with { Funding = saved.Funding with
            { RealClockCredits = null, RealClockDebits = null } };
        var restored = E4bV15FundingShadow.Restore(saved,
            fx.Baseline, fx.School, fx.Technologies, fx.Depot);
        Assert.Equal(start.Campaign, restored.Campaign);
        Assert.True(restored.Funding.Reconciles());
    }
}
