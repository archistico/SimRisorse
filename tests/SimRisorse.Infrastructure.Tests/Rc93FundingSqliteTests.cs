using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Infrastructure.Experimental;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class Rc93FundingSqliteTests
{
    private sealed class TemporaryDatabase : IDisposable
    {
        private readonly string _folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
            "simrisorse-rc93-" + Guid.NewGuid().ToString("N"));
        public TemporaryDatabase() => Directory.CreateDirectory(_folder);
        public string Path => System.IO.Path.Combine(_folder, "isolated-funding.sqlite3");
        public E4bCurveAFundingSqliteStore Store() => new(Path);
        public void Dispose() => Directory.Delete(_folder, recursive: true);
    }

    private static readonly ResourceBundle Capacity = ResourceBundle.Create(("planks", 100m), ("stone", 90m));
    private static E4bFundingInstruction Begin(ProductionTier tier = ProductionTier.Base) =>
        new(E4bFundingAction.BeginMint, tier);
    private static E4bFundingInstruction Deposit(ResourceBundle amount) =>
        new(E4bFundingAction.Deposit, Materials: amount);
    private static E4bFundingInstruction Deliver(ResourceBundle amount) =>
        new(E4bFundingAction.DeliverHypothetical, Materials: amount);
    private static E4bFundingApplyResult Apply(E4bCurveAFundingSqliteStore store,
        string request, long revision, E4bFundingInstruction instruction) =>
        store.Apply("s1", request, revision, instruction);

    [Fact]
    public void InitialStateSurvivesClosingAndReopeningTheIndependentDatabase()
    {
        using var temp = new TemporaryDatabase();
        temp.Store().CreateSession("s1", ResourceBundle.Create(("planks", 12m)), Capacity);
        var reloaded = temp.Store().Load("s1");
        Assert.Equal(0, reloaded.Revision);
        Assert.Equal(ResourceBundle.Create(("planks", 12m)), reloaded.State.Stock);
        Assert.True(reloaded.State.Reconciles());
    }

    [Fact]
    public void FrozenPriceAndPartialPaymentSurviveReopen()
    {
        using var temp = new TemporaryDatabase();
        var store = temp.Store();
        store.CreateSession("s1", ResourceBundle.Empty, Capacity);
        var started = Apply(store, "begin", 0, Begin());
        Assert.Equal(E4bFundingApplyStatus.Applied, started.Status);
        var quote = started.Session.State.Escrow!.Quote;
        var part = ProgressiveConstructionEscrow.Installment(quote,
            started.Session.State.RequiredPayments(ProductionTier.Base), 1);
        Assert.Equal(E4bFundingApplyStatus.Applied, Apply(store, "supply", 1, Deliver(part)).Status);
        Assert.Equal(E4bFundingApplyStatus.Applied, Apply(store, "pay", 2, Deposit(part)).Status);
        var after = temp.Store().Load("s1");
        Assert.Equal(3, after.Revision);
        Assert.Equal(quote, after.State.Escrow!.Quote);
        Assert.Equal(part, after.State.Escrowed);
        Assert.True(after.State.Reconciles());
    }

    [Fact]
    public void RetryingCommittedRequestAfterLaterOperationsNeverChargesTwice()
    {
        using var temp = new TemporaryDatabase();
        var store = temp.Store();
        store.CreateSession("s1", ResourceBundle.Create(("planks", 80m)), Capacity);
        Assert.Equal(E4bFundingApplyStatus.Applied, Apply(store, "begin", 0, Begin()).Status);
        var payment = ResourceBundle.Create(("planks", 10m));
        Assert.Equal(E4bFundingApplyStatus.Applied, Apply(store, "pay", 1, Deposit(payment)).Status);
        Assert.Equal(E4bFundingApplyStatus.Applied, Apply(store, "supply", 2,
            Deliver(ResourceBundle.Create(("planks", 1m)))).Status);
        var replay = Apply(temp.Store(), "pay", 1, Deposit(payment));
        Assert.Equal(E4bFundingApplyStatus.Replayed, replay.Status);
        Assert.Equal(3, replay.Session.Revision);
        Assert.Equal(payment, replay.Session.State.Escrowed);
        Assert.Equal(ResourceBundle.Create(("planks", 71m)), replay.Session.State.Stock);
    }

    [Fact]
    public void ReusingRequestIdWithDifferentPaymentIsRejectedWithoutMutation()
    {
        using var temp = new TemporaryDatabase();
        var store = temp.Store();
        store.CreateSession("s1", ResourceBundle.Create(("planks", 80m)), Capacity);
        Apply(store, "begin", 0, Begin());
        Apply(store, "pay", 1, Deposit(ResourceBundle.Create(("planks", 10m))));
        var result = Apply(store, "pay", 2, Deposit(ResourceBundle.Create(("planks", 11m))));
        Assert.Equal(E4bFundingApplyStatus.KeyConflict, result.Status);
        Assert.Equal(2, store.Load("s1").Revision);
    }

    [Fact]
    public void OptimisticRevisionStopsStaleWriter()
    {
        using var temp = new TemporaryDatabase();
        var store = temp.Store();
        store.CreateSession("s1", ResourceBundle.Empty, Capacity);
        Apply(store, "begin", 0, Begin());
        Assert.Equal(E4bFundingApplyStatus.StaleRevision,
            Apply(temp.Store(), "late", 0, Begin(ProductionTier.Large)).Status);
        Assert.Equal(1, store.Load("s1").Revision);
    }

    [Fact]
    public void RejectedOperationDoesNotCreateReceiptOrConsumeRequestId()
    {
        using var temp = new TemporaryDatabase();
        var store = temp.Store();
        store.CreateSession("s1", ResourceBundle.Create(("planks", 80m)), Capacity);
        var payment = Deposit(ResourceBundle.Create(("planks", 10m)));
        Assert.Equal(E4bFundingApplyStatus.Rejected, Apply(store, "pay", 0, payment).Status);
        Apply(store, "begin", 0, Begin());
        Assert.Equal(E4bFundingApplyStatus.Applied, Apply(store, "pay", 1, payment).Status);
        Assert.Equal(2, store.Load("s1").Revision);
    }

    [Fact]
    public void CancellationWithInsufficientCapacityPreservesEscrowAndStock()
    {
        using var temp = new TemporaryDatabase();
        var store = temp.Store();
        store.CreateSession("s1", ResourceBundle.Create(("planks", 80m)), Capacity);
        Apply(store, "begin", 0, Begin());
        Apply(store, "pay", 1, Deposit(ResourceBundle.Create(("planks", 20m))));
        Apply(store, "supply", 2, Deliver(ResourceBundle.Create(("planks", 40m))));
        Assert.Equal(E4bFundingApplyStatus.Rejected,
            Apply(store, "cancel", 3, new(E4bFundingAction.Cancel)).Status);
        var after = temp.Store().Load("s1");
        Assert.Equal(3, after.Revision);
        Assert.Equal(ResourceBundle.Create(("planks", 20m)), after.State.Escrowed);
        Assert.Equal(ResourceBundle.Create(("planks", 100m)), after.State.Stock);
        Assert.True(after.State.Reconciles());
    }

    [Fact]
    public void CancellationWhenSpaceIsAvailableRefundsExactlyOnce()
    {
        using var temp = new TemporaryDatabase();
        var store = temp.Store();
        store.CreateSession("s1", ResourceBundle.Create(("planks", 80m)), Capacity);
        Apply(store, "begin", 0, Begin());
        Apply(store, "pay", 1, Deposit(ResourceBundle.Create(("planks", 20m))));
        Assert.Equal(E4bFundingApplyStatus.Applied,
            Apply(store, "cancel", 2, new(E4bFundingAction.Cancel)).Status);
        var replay = Apply(temp.Store(), "cancel", 2, new(E4bFundingAction.Cancel));
        Assert.Equal(E4bFundingApplyStatus.Replayed, replay.Status);
        Assert.False(replay.Session.State.HasPendingPurchase);
        Assert.Equal(ResourceBundle.Create(("planks", 80m)), replay.Session.State.Stock);
        Assert.Equal(0, replay.Session.State.AcquiredMints);
    }

    [Fact]
    public void FullyPaidMintCanOnlyBeCompletedOnceAcrossRestart()
    {
        using var temp = new TemporaryDatabase();
        var store = temp.Store();
        var current = store.CreateSession("s1", ResourceBundle.Empty, Capacity);
        var opened = Apply(store, "begin", current.Revision, Begin());
        var quote = opened.Session.State.Escrow!.Quote;
        var count = opened.Session.State.RequiredPayments(ProductionTier.Base);
        Assert.InRange(count, 2L, 100_000L);
        for (long i = 1; i <= count; i++)
        {
            var part = ProgressiveConstructionEscrow.Installment(quote, count, i);
            var supplied = Apply(store, "supply" + i, opened.Session.Revision, Deliver(part));
            Assert.Equal(E4bFundingApplyStatus.Applied, supplied.Status);
            opened = Apply(store, "pay" + i, supplied.Session.Revision, Deposit(part));
            Assert.Equal(E4bFundingApplyStatus.Applied, opened.Status);
        }
        var final = Apply(temp.Store(), "complete", opened.Session.Revision, new(E4bFundingAction.Complete));
        Assert.Equal(E4bFundingApplyStatus.Applied, final.Status);
        Assert.Equal(1, final.Session.State.AcquiredMints);
        Assert.Equal(quote, final.Session.State.Committed);
        Assert.False(final.Session.State.HasPendingPurchase);
        Assert.Equal(E4bFundingApplyStatus.Replayed,
            Apply(store, "complete", opened.Session.Revision, new(E4bFundingAction.Complete)).Status);
        Assert.Equal(1, temp.Store().Load("s1").State.AcquiredMints);
    }

    [Fact]
    public void CorruptedCheckpointFailsClosedInsteadOfGrantingMaterials()
    {
        using var temp = new TemporaryDatabase();
        var store = temp.Store();
        store.CreateSession("s1", ResourceBundle.Empty, Capacity);
        using (var db = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = temp.Path, Pooling = false }.ToString()))
        {
            db.Open();
            using var command = db.CreateCommand();
            command.CommandText = "UPDATE funding_session SET state_json='{\"Version\":1}' WHERE session_id='s1';";
            Assert.Equal(1, command.ExecuteNonQuery());
        }
        Assert.Throws<InvalidDataException>(() => temp.Store().Load("s1"));
    }

    [Fact]
    public void ForeignSaveFormatIsRejectedAndBytesAreNotAltered()
    {
        using var temp = new TemporaryDatabase();
        using (var db = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = temp.Path, Pooling = false }.ToString()))
        {
            db.Open();
            using var command = db.CreateCommand();
            command.CommandText = "PRAGMA application_id = 305419896; PRAGMA user_version = 6; CREATE TABLE user_data (id INT);";
            command.ExecuteNonQuery();
        }
        var before = File.ReadAllBytes(temp.Path);
        Assert.Throws<InvalidDataException>(() => temp.Store());
        Assert.Equal(before, File.ReadAllBytes(temp.Path));
    }

    [Fact]
    public void SessionIdsCannotBeOverwrittenOrGuessedFromMissingRows()
    {
        using var temp = new TemporaryDatabase();
        var store = temp.Store();
        store.CreateSession("s1", ResourceBundle.Empty, Capacity);
        Assert.Throws<SqliteException>(() => store.CreateSession("s1", ResourceBundle.Empty, Capacity));
        Assert.Throws<KeyNotFoundException>(() => store.Load("missing"));
        Assert.Throws<ArgumentException>(() => store.Load("../save"));
        Assert.True(store.Load("s1").State.Reconciles());
    }
}
