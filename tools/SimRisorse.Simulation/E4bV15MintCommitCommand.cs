using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;

namespace SimRisorse.Simulation;

/// <summary>RC98 synthetic *pre-provisioned* city fixture; tests the single
/// composite completion contract, never a natural playthrough or player save.</summary>
internal static class E4bV15MintCommitCommand
{
    public static int Run(string[] args)
    {
        if (args is not ["--study-e4b-v15-mint-commit"]) return 2;
        try
        {
            var fx = V15BenchmarkFixture.Create();
            var initial = E4bV15MintCompletionFixture.PreProvisioned(fx);
            var state = E4bV15FundingShadow.Open(fx.Baseline, fx.School,
                fx.Technologies, fx.Depot, initial);
            var quotes = new List<long>();
            var preservedStocks = true;
            for (var copy = 0; copy < 2; copy++)
            {
                if (!state.TryBeginMint(ProductionTier.Base, state.Revision, out var begun))
                    throw new InvalidOperationException("Could not open RC98 laboratory funding.");
                var price = begun.Funding.Escrow!.Quote;
                quotes.Add(price["planks"].Milliunits);
                if (!begun.TryDeposit(price, begun.Revision, out var funded) ||
                    funded.Readiness() != E4bV15FundingReadiness.ReadyForFutureAtomicCommit ||
                    !funded.TryCompleteMint(funded.Revision, out var completed))
                    throw new InvalidOperationException("RC98 laboratory mint completion failed.");
                preservedStocks &= funded.Campaign.CityCampaign.Resources.Equals(
                    completed.Campaign.CityCampaign.Resources);
                state = completed;
            }
            var roundTrip = E4bV15FundingShadow.Restore(state.ExportCheckpoint(),
                fx.Baseline, fx.School, fx.Technologies, fx.Depot);
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                Version = "rc98-v15-mint-commit-1",
                Mode = E4bV15FundingShadow.Mode,
                InputProvenance = "synthetic-pre-provisioned-laboratory-not-natural-progression",
                MintBuildings = roundTrip.Campaign.CityCampaign.ProductiveFamilies
                    .Single(f => f.FamilyId == "mint").Base.Buildings,
                HistoricalAcquisitions = roundTrip.Funding.AcquiredMints,
                CompletionReceipts = roundTrip.Funding.CompletedMints.Length,
                QuoteIncreases = quotes.Count == 2 && quotes[1] > quotes[0],
                NoSecondWithdrawal = preservedStocks,
                NoPendingFunding = !roundTrip.IsFunding,
                FullMaterialConservation = roundTrip.Funding.Reconciles(),
                OriginalFixtureUnmodified = initial.CityCampaign.ProductiveFamilies
                    .Single(f => f.FamilyId == "mint").Base.Buildings == 0,
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
