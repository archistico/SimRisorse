using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;

namespace SimRisorse.Simulation;

/// <summary>RC97 deterministic, actual native v15 clock smoke on an isolated
/// in-memory city shadow. It does not access player saves or create buildings.</summary>
internal static class E4bV15FundingClockCommand
{
    public static int Run(string[] args)
    {
        if (args is not ["--study-e4b-v15-funding-clock"]) return 2;
        try
        {
            var fx = V15BenchmarkFixture.Create();
            var original = fx.Start;
            var state = E4bV15FundingShadow.Open(fx.Baseline, fx.School,
                fx.Technologies, fx.Depot, original);
            if (!state.TryBeginMint(ProductionTier.Base, 0, out var begun) ||
                !begun.TryDeposit(ResourceBundle.Create(("planks", 1m)), 1, out var paid) ||
                !paid.TryAdvanceClock(fx.Meat, fx.Crops, 2, out var clocked))
                throw new InvalidOperationException("RC97 real clock shadow transition failed.");
            var restored = E4bV15FundingShadow.Restore(clocked.ExportCheckpoint(),
                fx.Baseline, fx.School, fx.Technologies, fx.Depot);
            if (!restored.TryCancel(3, out var canceled))
                throw new InvalidOperationException("RC97 refund after clock was rejected.");
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                Version = "rc97-v15-funding-clock-1",
                Mode = E4bV15FundingShadow.Mode,
                StartingRevision = state.Revision,
                DepositedRevision = paid.Revision,
                ClockRevision = restored.Revision,
                CancelRevision = canceled.Revision,
                NativeElapsedTicks = restored.Campaign.CityCampaign.ElapsedTicks - original.CityCampaign.ElapsedTicks,
                EscrowSurvivesClock = restored.Funding.Escrowed.Equals(ResourceBundle.Create(("planks", 1m))),
                CityInventoryReconciles = restored.Funding.Stock.Equals(restored.Campaign.CityCampaign.Resources),
                ClockLedgerReconciles = restored.Funding.Reconciles(),
                RefundAfterClockReconciles = canceled.Funding.Reconciles() && !canceled.IsFunding,
                NoMintGranted = canceled.Funding.AcquiredMints == state.Funding.AcquiredMints,
                PlayerSnapshotUnmodified = original.Equals(fx.Start),
                ApprovalStatus = "not-approved/not-playable/no-player-save-changes"
            }));
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or OverflowException)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
