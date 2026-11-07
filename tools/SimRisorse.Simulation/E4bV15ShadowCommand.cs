using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;

namespace SimRisorse.Simulation;

/// <summary>RC94 smoke: one genuine v15 new-game snapshot, without changing any
/// player session, save, construction price or resource production.</summary>
internal static class E4bV15ShadowCommand
{
    public static int Run(string[] args)
    {
        if (args is not ["--study-e4b-v15-shadow"]) return 2;
        try
        {
            var fixture = V15BenchmarkFixture.Create();
            var original = fixture.Start;
            var initial = E4bV15FundingShadow.Open(fixture.Baseline, fixture.School,
                fixture.Technologies, fixture.Depot, original);
            if (!initial.TryBeginMint(ProductionTier.Base, 0, out var opened) ||
                !opened.TryDeposit(ResourceBundle.Create(("planks", 1m)), 1, out var paid))
                throw new InvalidOperationException("The RC94 staged funding smoke was rejected.");
            var reloaded = E4bV15FundingShadow.Restore(paid.ExportCheckpoint(),
                fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot);
            if (!reloaded.TryCancel(2, out var canceled))
                throw new InvalidOperationException("The RC94 refund smoke was rejected.");
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                Version = "rc94-v15-shadow-1",
                Mode = E4bV15FundingShadow.Mode,
                InitialRevision = initial.Revision,
                DepositedRevision = paid.Revision,
                RefundedRevision = canceled.Revision,
                ShadowDepositReconciles = paid.Funding.Reconciles(),
                FrozenQuoteSurvivesRestore = paid.Funding.Escrow!.Quote.Equals(reloaded.Funding.Escrow!.Quote),
                CityAndEscrowMatch = paid.Campaign.CityCampaign.Resources.Equals(paid.Funding.Stock),
                RefundRestoresInventory = canceled.Campaign.CityCampaign.Resources.Equals(original.CityCampaign.Resources),
                LiveSnapshotUnmodified = original.CityCampaign.Resources.Equals(fixture.Start.CityCampaign.Resources) &&
                    initial.Campaign.Equals(fixture.Start),
                NoConstructionGranted = canceled.Funding.AcquiredMints == initial.Funding.AcquiredMints,
                ApprovalStatus = "not-approved/not-playable/no-player-save-changes"
            }));
            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException or
            InvalidOperationException or OverflowException)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }
}
