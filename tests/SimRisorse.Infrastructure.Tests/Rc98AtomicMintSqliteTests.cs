using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Infrastructure.Experimental;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class Rc98AtomicMintSqliteTests
{
    private sealed class TemporaryFile : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(),
            "simrisorse-rc98-" + Guid.NewGuid().ToString("N"));
        public TemporaryFile() => Directory.CreateDirectory(_directory);
        public string FilePath => Path.Combine(_directory, "experimental.sqlite3");
        public void Dispose() => Directory.Delete(_directory, recursive: true);
    }

    private static E4bV15FundingCompositeSqliteStore Store(TemporaryFile file,
        V15BenchmarkFixture fx) => new(file.FilePath, fx.Baseline, fx.School,
            fx.Technologies, fx.Depot);

    private static (V15BenchmarkFixture Fx, TemporaryFile File,
        E4bV15FundingCompositeSqliteStore Db) Open()
    {
        var fx = V15BenchmarkFixture.Create();
        var file = new TemporaryFile();
        var db = Store(file, fx);
        db.Create(E4bV15MintCompletionFixture.PreProvisioned(fx));
        return (fx, file, db);
    }

    private static (E4bV15FundingCompositeResult Paid, ResourceBundle Quote) Pay(
        E4bV15FundingCompositeSqliteStore db, long revision = 0, string tag = "1")
    {
        var opened = db.Apply("begin-" + tag, revision,
            new(E4bFundingAction.BeginMint, ProductionTier.Base));
        Assert.Equal(E4bFundingApplyStatus.Applied, opened.Status);
        var quote = opened.State.Funding.Escrow!.Quote;
        var deposited = db.Apply("deposit-" + tag, opened.State.Revision,
            new(E4bFundingAction.Deposit, Materials: quote));
        Assert.Equal(E4bFundingApplyStatus.Applied, deposited.Status);
        return (deposited, quote);
    }

    [Fact]
    public void CompletedMintSurvivesRestartWithNoSecondWithdrawal()
    {
        var (fx, file, db) = Open();
        using (file)
        {
            var (paid, quote) = Pay(db);
            var remaining = paid.State.Campaign.CityCampaign.Resources;
            var finished = db.Apply("complete-1", paid.State.Revision, new(E4bFundingAction.Complete));
            Assert.Equal(E4bFundingApplyStatus.Applied, finished.Status);
            var reopened = Store(file, fx).Load();
            Assert.Equal(remaining, reopened.Campaign.CityCampaign.Resources);
            Assert.Equal(quote, reopened.Funding.Committed);
            Assert.Equal(1, reopened.Funding.AcquiredMints);
            Assert.Equal(1, reopened.Campaign.CityCampaign.ProductiveFamilies.Single(f => f.FamilyId == "mint").Base.Buildings);
        }
    }

    [Fact]
    public void CompletionReplayDoesNotConstructOrDebitAgain()
    {
        var (fx, file, db) = Open();
        using (file)
        {
            var (paid, _) = Pay(db);
            var first = db.Apply("complete-1", paid.State.Revision, new(E4bFundingAction.Complete));
            var replay = Store(file, fx).Apply("complete-1", paid.State.Revision,
                new(E4bFundingAction.Complete));
            Assert.Equal(E4bFundingApplyStatus.Replayed, replay.Status);
            Assert.Equal(first.State.Revision, replay.State.Revision);
            Assert.Equal(first.State.Funding.Committed, replay.State.Funding.Committed);
            Assert.Equal(first.State.Campaign.CityCampaign.Resources,
                replay.State.Campaign.CityCampaign.Resources);
            Assert.Equal(1, replay.State.Campaign.CityCampaign.ProductiveFamilies
                .Single(f => f.FamilyId == "mint").Base.Buildings);
        }
    }

    [Fact]
    public void NewCompletionWithStaleRevisionCannotConstructAgain()
    {
        var (_, file, db) = Open();
        using (file)
        {
            var (paid, _) = Pay(db);
            Assert.Equal(E4bFundingApplyStatus.Applied,
                db.Apply("complete", paid.State.Revision, new(E4bFundingAction.Complete)).Status);
            Assert.Equal(E4bFundingApplyStatus.StaleRevision,
                db.Apply("other-complete", paid.State.Revision, new(E4bFundingAction.Complete)).Status);
            Assert.Equal(1, db.Load().Funding.AcquiredMints);
        }
    }

    [Fact]
    public void CompletionBeforeFinalDepositIsRejectedAndCanBeRetried()
    {
        var (_, file, db) = Open();
        using (file)
        {
            var opened = db.Apply("begin", 0, new(E4bFundingAction.BeginMint, ProductionTier.Base));
            Assert.Equal(E4bFundingApplyStatus.Rejected,
                db.Apply("complete", opened.State.Revision, new(E4bFundingAction.Complete)).Status);
            var quote = opened.State.Funding.Escrow!.Quote;
            var paid = db.Apply("pay", 1, new(E4bFundingAction.Deposit, Materials: quote));
            Assert.Equal(E4bFundingApplyStatus.Applied,
                db.Apply("complete", paid.State.Revision, new(E4bFundingAction.Complete)).Status);
        }
    }

    [Fact]
    public void TwoMintPurchasesSurviveReopenWithIncreasingPrice()
    {
        var (fx, file, db) = Open();
        using (file)
        {
            var (first, quote1) = Pay(db);
            var completed = db.Apply("complete-1", first.State.Revision, new(E4bFundingAction.Complete));
            var (second, quote2) = Pay(Store(file, fx), completed.State.Revision, "2");
            Assert.NotEqual(quote1, quote2);
            Assert.Equal(E4bFundingApplyStatus.Applied,
                db.Apply("complete-2", second.State.Revision, new(E4bFundingAction.Complete)).Status);
            var reopened = Store(file, fx).Load();
            Assert.Equal(2, reopened.Funding.CompletedMints.Length);
            Assert.Equal(quote1 + quote2, reopened.Funding.Committed);
            Assert.Equal(2, reopened.Campaign.CityCampaign.ProductiveFamilies.Single(f => f.FamilyId == "mint").Base.Buildings);
        }
    }

    [Fact]
    public void CompletionKeyConflictCannotChangeLatestCheckpoint()
    {
        var (_, file, db) = Open();
        using (file)
        {
            var (paid, _) = Pay(db);
            Assert.Equal(E4bFundingApplyStatus.Applied,
                db.Apply("complete", paid.State.Revision, new(E4bFundingAction.Complete)).Status);
            var conflicting = db.Apply("complete", paid.State.Revision,
                new(E4bFundingAction.Cancel));
            Assert.Equal(E4bFundingApplyStatus.KeyConflict, conflicting.Status);
            Assert.Equal(1, db.Load().Funding.AcquiredMints);
        }
    }
}
