using Microsoft.Data.Sqlite;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Application.Persistence;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Infrastructure.Experimental;
using SimRisorse.Infrastructure.Persistence;
using Xunit;

namespace SimRisorse.Infrastructure.Tests;

public sealed class Rc95CompositeFundingSqliteTests
{
    private sealed class IsolatedFile : IDisposable
    {
        private readonly string _folder = Path.Combine(Path.GetTempPath(),
            "simrisorse-rc95-" + Guid.NewGuid().ToString("N"));
        public IsolatedFile() => Directory.CreateDirectory(_folder);
        public string FilePath => Path.Combine(_folder, "experimental-composite.sqlite3");
        public void Dispose() => Directory.Delete(_folder, true);
    }

    private static readonly ResourceBundle Plank = ResourceBundle.Create(("planks", 1m));
    private static E4bFundingInstruction Begin() => new(E4bFundingAction.BeginMint, ProductionTier.Base);
    private static E4bFundingInstruction Deposit() => new(E4bFundingAction.Deposit, Materials: Plank);
    private static E4bFundingInstruction Cancel() => new(E4bFundingAction.Cancel);
    private static E4bV15FundingCompositeSqliteStore Open(IsolatedFile path, V15BenchmarkFixture fixture) =>
        new(path.FilePath, fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot);

    [Fact]
    public void CompleteV15CityAndEmptyEscrowSurviveDatabaseReopen()
    {
        var fixture = V15BenchmarkFixture.Create();
        using var file = new IsolatedFile();
        var state = Open(file, fixture).Create(fixture.Start);
        var restored = Open(file, fixture).Load();
        Assert.Equal(0L, restored.Revision);
        Assert.Equal(state.Campaign.CityCampaign.Resources, restored.Campaign.CityCampaign.Resources);
        Assert.Equal(state.Campaign.CityCampaign.Population, restored.Campaign.CityCampaign.Population);
        Assert.Equal(state.Campaign.CrewMode, restored.Campaign.CrewMode);
        Assert.Equal(fixture.Start.CityCampaign.Resources, restored.Funding.Stock);
        Assert.False(restored.IsFunding);
        // The regular player-save loader must refuse the experimental container.
        Assert.Throws<GameSaveException>(() => new V15SqliteFoodSaveStore(file.FilePath,
            fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot).Load());
    }

    [Fact]
    public void PartialEscrowAndCityInventorySurviveReopenWithoutDoubleDebit()
    {
        var fixture = V15BenchmarkFixture.Create();
        using var file = new IsolatedFile();
        var store = Open(file, fixture);
        store.Create(fixture.Start);
        Assert.Equal(E4bFundingApplyStatus.Applied, store.Apply("begin", 0, Begin()).Status);
        var paid = store.Apply("pay", 1, Deposit());
        Assert.Equal(E4bFundingApplyStatus.Applied, paid.Status);
        var reopened = Open(file, fixture).Load();
        Assert.Equal(2L, reopened.Revision);
        Assert.Equal(Plank, reopened.Funding.Escrowed);
        Assert.Equal(fixture.Start.CityCampaign.Resources - Plank, reopened.Campaign.CityCampaign.Resources);
        Assert.Equal(reopened.Funding.Stock, reopened.Campaign.CityCampaign.Resources);
        Assert.True(reopened.Funding.Reconciles());
    }

    [Fact]
    public void ReplayedRequestDoesNotChargeTwiceAfterRestart()
    {
        var fixture = V15BenchmarkFixture.Create();
        using var file = new IsolatedFile();
        var store = Open(file, fixture);
        store.Create(fixture.Start);
        store.Apply("begin", 0, Begin());
        store.Apply("pay", 1, Deposit());
        var reply = Open(file, fixture).Apply("pay", 1, Deposit());
        Assert.Equal(E4bFundingApplyStatus.Replayed, reply.Status);
        Assert.Equal(2L, reply.State.Revision);
        Assert.Equal(Plank, reply.State.Funding.Escrowed);
    }

    [Fact]
    public void ReusedKeyWithDifferentPayloadCannotChangeEscrow()
    {
        var fixture = V15BenchmarkFixture.Create();
        using var file = new IsolatedFile();
        var store = Open(file, fixture);
        store.Create(fixture.Start);
        store.Apply("begin", 0, Begin());
        store.Apply("pay", 1, Deposit());
        var other = new E4bFundingInstruction(E4bFundingAction.Deposit,
            Materials: ResourceBundle.Create(("planks", 2m)));
        Assert.Equal(E4bFundingApplyStatus.KeyConflict, store.Apply("pay", 2, other).Status);
        Assert.Equal(2L, store.Load().Revision);
    }

    [Fact]
    public void StaleRevisionAndRejectedPaymentsCreateNoReceipts()
    {
        var fixture = V15BenchmarkFixture.Create();
        using var file = new IsolatedFile();
        var store = Open(file, fixture);
        store.Create(fixture.Start);
        Assert.Equal(E4bFundingApplyStatus.Rejected, store.Apply("pay", 0, Deposit()).Status);
        store.Apply("begin", 0, Begin());
        Assert.Equal(E4bFundingApplyStatus.StaleRevision, store.Apply("stale", 0, Deposit()).Status);
        Assert.Equal(E4bFundingApplyStatus.Applied, store.Apply("pay", 1, Deposit()).Status);
        Assert.Equal(2L, store.Load().Revision);
    }

    [Fact]
    public void CancelRefundsSavedMaterialsOnceAndKeepsCityCounter()
    {
        var fixture = V15BenchmarkFixture.Create();
        using var file = new IsolatedFile();
        var store = Open(file, fixture);
        store.Create(fixture.Start);
        store.Apply("begin", 0, Begin());
        store.Apply("pay", 1, Deposit());
        var canceled = store.Apply("cancel", 2, Cancel());
        Assert.Equal(E4bFundingApplyStatus.Applied, canceled.Status);
        Assert.False(canceled.State.IsFunding);
        Assert.Equal(fixture.Start.CityCampaign.Resources, canceled.State.Campaign.CityCampaign.Resources);
        Assert.Equal(0, canceled.State.Funding.AcquiredMints);
        Assert.Equal(E4bFundingApplyStatus.Replayed, Open(file, fixture).Apply("cancel", 2, Cancel()).Status);
    }

    [Fact]
    public void UnsupportedFakeDeliveriesAndUnfundedCompletionNeverTouchDatabase()
    {
        var fixture = V15BenchmarkFixture.Create();
        using var file = new IsolatedFile();
        var store = Open(file, fixture);
        store.Create(fixture.Start);
        Assert.Throws<ArgumentException>(() => store.Apply("fake", 0,
            new(E4bFundingAction.DeliverHypothetical, Materials: Plank)));
        Assert.Equal(E4bFundingApplyStatus.Rejected, store.Apply("complete", 0,
            new(E4bFundingAction.Complete)).Status);
        Assert.Equal(0L, store.Load().Revision);
    }

    [Fact]
    public void NativeSaveFileIsNeverConvertedToCompositeSchema()
    {
        var fixture = V15BenchmarkFixture.Create();
        using var file = new IsolatedFile();
        new V15SqliteFoodSaveStore(file.FilePath, fixture.Baseline, fixture.School,
            fixture.Technologies, fixture.Depot).Save(fixture.Start);
        var before = System.IO.File.ReadAllBytes(file.FilePath);
        Assert.Throws<InvalidDataException>(() => Open(file, fixture));
        Assert.Equal(before, System.IO.File.ReadAllBytes(file.FilePath));
    }

    [Fact]
    public void FundingOnlyRc93FileIsNeverAcceptedAsComposite()
    {
        var fixture = V15BenchmarkFixture.Create();
        using var file = new IsolatedFile();
        _ = new E4bCurveAFundingSqliteStore(file.FilePath);
        Assert.Throws<InvalidDataException>(() => Open(file, fixture));
    }

    [Fact]
    public void CorruptEscrowIsRejectedInsteadOfCreditingInventory()
    {
        var fixture = V15BenchmarkFixture.Create();
        using var file = new IsolatedFile();
        var store = Open(file, fixture);
        store.Create(fixture.Start);
        store.Apply("begin", 0, Begin());
        store.Apply("pay", 1, Deposit());
        using (var db = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = file.FilePath, Pooling = false }.ToString()))
        {
            db.Open();
            using var cmd = db.CreateCommand();
            cmd.CommandText = "UPDATE composite_state SET funding_json='{}' WHERE slot=1;";
            Assert.Equal(1, cmd.ExecuteNonQuery());
        }
        Assert.Throws<InvalidDataException>(() => Open(file, fixture).Load());
    }


    [Fact]
    public void TamperedNativeImageFailsBeforeAcceptingAnotherPayment()
    {
        var fixture = V15BenchmarkFixture.Create();
        using var file = new IsolatedFile();
        var store = Open(file, fixture);
        store.Create(fixture.Start);
        store.Apply("begin", 0, Begin());
        using (var db = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource = file.FilePath, Pooling = false }.ToString()))
        {
            db.Open();
            using var cmd = db.CreateCommand();
            cmd.CommandText = "UPDATE composite_state SET native_v15_image=x'00112233' WHERE slot=1;";
            Assert.Equal(1, cmd.ExecuteNonQuery());
        }
        Assert.Throws<GameSaveException>(() => Open(file, fixture).Apply("pay", 1, Deposit()));
    }

    [Fact]
    public void CannotOverwriteAnExistingCompositeSession()
    {
        var fixture = V15BenchmarkFixture.Create();
        using var file = new IsolatedFile();
        var store = Open(file, fixture);
        store.Create(fixture.Start);
        Assert.Throws<SqliteException>(() => store.Create(fixture.Start));
        Assert.Equal(0L, store.Load().Revision);
    }
}
