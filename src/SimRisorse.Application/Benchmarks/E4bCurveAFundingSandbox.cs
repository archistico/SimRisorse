using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;

namespace SimRisorse.Application.Benchmarks;

/// <summary>
/// RC92: explicitly selected, in-memory ONLY funding sandbox for Curve A mint
/// purchases. No live construction, jobs, technology unlocks, SQLite writes,
/// or player inventories are modified. This is an integration contract, not
/// an enabled game rule. External material deliveries are hypothetical.
/// </summary>
public sealed class E4bCurveAFundingSandbox
{
    private static readonly E4bCurveDefinition CurveA = E4bEconomicCurveStudy.Curves
        .Single(curve => curve.Id == "A");

    private E4bCurveAFundingSandbox(ResourceBundle initialStock, ResourceBundle stock,
        ResourceBundle capacity, ResourceBundle hypotheticalDelivered,
        ResourceBundle committed, int acquiredMints, ProductionTier? pendingTier,
        ProgressiveConstructionEscrow? escrow, ResourceBundle realClockCredits,
        ResourceBundle realClockDebits, E4bMintOrigin origin,
        ImmutableArray<E4bCompletedMint> completedMints)
    {
        InitialStock = initialStock;
        Stock = stock;
        Capacity = capacity;
        HypotheticalDelivered = hypotheticalDelivered;
        Committed = committed;
        AcquiredMints = acquiredMints;
        PendingTier = pendingTier;
        Escrow = escrow;
        RealClockCredits = realClockCredits;
        RealClockDebits = realClockDebits;
        MintOrigin = origin;
        CompletedMints = completedMints;
    }

    public const string Mode = "curve-A/opt-in/in-memory-only/not-playable";
    public ResourceBundle InitialStock { get; }
    public ResourceBundle Stock { get; }
    public ResourceBundle Capacity { get; }
    public ResourceBundle HypotheticalDelivered { get; }
    public ResourceBundle Committed { get; }
    // Measured changes to the city inventory from the real v15 clock only.
    // Food transfers in other inventories are tracked by the native checkpoint.
    public ResourceBundle RealClockCredits { get; }
    public ResourceBundle RealClockDebits { get; }
    public E4bMintOrigin MintOrigin { get; }
    public ImmutableArray<E4bCompletedMint> CompletedMints { get; }
    public int AcquiredMints { get; }
    public ProductionTier? PendingTier { get; }
    public ProgressiveConstructionEscrow? Escrow { get; }
    public ResourceBundle Escrowed => Escrow?.Paid ?? ResourceBundle.Empty;
    public ResourceBundle Outstanding => Escrow?.Outstanding ?? ResourceBundle.Empty;
    public bool HasPendingPurchase => Escrow is not null;

    public const int CheckpointVersion = 1;

    public E4bFundingCheckpoint ExportCheckpoint() => new(CheckpointVersion, InitialStock, Stock,
        Capacity, HypotheticalDelivered, Committed, AcquiredMints, PendingTier,
        Escrow?.Quote, Escrow?.Paid, RealClockCredits, RealClockDebits,
        MintOrigin, CompletedMints);

    /// <summary>Reject corrupt, inconsistent or repriced checkpoints before exposing
    /// any material balance to callers. The pending quote must still match Curve A.</summary>
    public static E4bCurveAFundingSandbox Restore(E4bFundingCheckpoint checkpoint)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        if (checkpoint.Version != CheckpointVersion || checkpoint.AcquiredMints < 0 ||
            checkpoint.InitialStock is null || checkpoint.Stock is null ||
            checkpoint.Capacity is null || checkpoint.HypotheticalDelivered is null ||
            checkpoint.Committed is null ||
            (checkpoint.PendingTier is null) != (checkpoint.PendingQuote is null) ||
            (checkpoint.PendingTier is null) != (checkpoint.PendingPaid is null))
            throw new ArgumentException("Unrecognized or incomplete financing checkpoint.", nameof(checkpoint));
        if (!checkpoint.Capacity.Covers(checkpoint.InitialStock))
            throw new ArgumentException("Initial stock exceeds checkpoint capacity.", nameof(checkpoint));
        ProgressiveConstructionEscrow? escrow = null;
        if (checkpoint.PendingTier is { } tier)
        {
            if (!Enum.IsDefined(tier))
                throw new ArgumentException("Invalid pending construction tier.", nameof(checkpoint));
            ResourceBundle expected;
            try { expected = new E4bEconomicCurveStudy().Quote(CurveA, "mint", tier, checkpoint.AcquiredMints); }
            catch (OverflowException ex) { throw new ArgumentException("Pending quote exceeds numerical limits.", nameof(checkpoint), ex); }
            if (!expected.Equals(checkpoint.PendingQuote))
                throw new ArgumentException("Pending quotation differs from the frozen Curve A price.", nameof(checkpoint));
            escrow = ProgressiveConstructionEscrow.Restore(checkpoint.PendingQuote!, checkpoint.PendingPaid!);
        }
        // Old RC93/RC95 experimental rows omitted the clock ledgers entirely.
        if ((checkpoint.RealClockCredits is null) != (checkpoint.RealClockDebits is null))
            throw new ArgumentException("Incomplete real-clock reconciliation ledger.", nameof(checkpoint));
        var receipts = checkpoint.CompletedMints.IsDefault
            ? ImmutableArray<E4bCompletedMint>.Empty : checkpoint.CompletedMints;
        if (checkpoint.MintOrigin is null &&
            (!checkpoint.Committed.Equals(ResourceBundle.Empty) || !receipts.IsEmpty))
            throw new ArgumentException("Completed financing requires an acquisition origin.", nameof(checkpoint));
        var origin = checkpoint.MintOrigin ?? new E4bMintOrigin(checkpoint.AcquiredMints, 0, 0, 0);
        if (origin.Acquired < 0 || origin.Base < 0 || origin.Large < 0 || origin.Advanced < 0 ||
            origin.Acquired > checkpoint.AcquiredMints ||
            (long)origin.Acquired + receipts.Length != checkpoint.AcquiredMints)
            throw new ArgumentException("Invalid financing acquisition history.", nameof(checkpoint));
        var committedFromReceipts = ResourceBundle.Empty;
        for (var i = 0; i < receipts.Length; i++)
        {
            var receipt = receipts[i];
            if (receipt is null || !Enum.IsDefined(receipt.Tier) ||
                receipt.PreviouslyAcquired != (long)origin.Acquired + i || receipt.Quote is null)
                throw new ArgumentException("Invalid completed mint receipt.", nameof(checkpoint));
            try
            {
                var expected = new E4bEconomicCurveStudy().Quote(CurveA,
                    "mint", receipt.Tier, receipt.PreviouslyAcquired);
                if (!expected.Equals(receipt.Quote))
                    throw new ArgumentException("Completed price differs from Curve A.", nameof(checkpoint));
                committedFromReceipts += receipt.Quote;
            }
            catch (OverflowException e)
            {
                throw new ArgumentException("Completed mint receipts overflow.", nameof(checkpoint), e);
            }
        }
        if (!committedFromReceipts.Equals(checkpoint.Committed))
            throw new ArgumentException("Committed materials do not match completed purchases.", nameof(checkpoint));
        var restored = new E4bCurveAFundingSandbox(checkpoint.InitialStock, checkpoint.Stock,
            checkpoint.Capacity, checkpoint.HypotheticalDelivered, checkpoint.Committed,
            checkpoint.AcquiredMints, checkpoint.PendingTier, escrow,
            checkpoint.RealClockCredits ?? ResourceBundle.Empty,
            checkpoint.RealClockDebits ?? ResourceBundle.Empty, origin, receipts);
        if (!restored.Reconciles())
            throw new ArgumentException("Financing balances do not reconcile or exceed storage.", nameof(checkpoint));
        return restored;
    }

    public static E4bCurveAFundingSandbox Open(ResourceBundle initialStock,
        ResourceBundle physicalCapacity, int alreadyAcquiredMints = 0, E4bMintOrigin? mintOrigin = null)
    {
        ArgumentNullException.ThrowIfNull(initialStock);
        ArgumentNullException.ThrowIfNull(physicalCapacity);
        ArgumentOutOfRangeException.ThrowIfNegative(alreadyAcquiredMints);
        var origin = mintOrigin ?? new E4bMintOrigin(alreadyAcquiredMints, 0, 0, 0);
        if (origin.Acquired != alreadyAcquiredMints || origin.Base < 0 ||
            origin.Large < 0 || origin.Advanced < 0)
            throw new ArgumentException("Invalid original mint acquisition history.", nameof(mintOrigin));
        if (!physicalCapacity.Covers(initialStock))
            throw new ArgumentException("Initial stock exceeds physical storage capacity.", nameof(initialStock));
        return new(initialStock, initialStock, physicalCapacity, ResourceBundle.Empty,
            ResourceBundle.Empty, alreadyAcquiredMints, null, null,
            ResourceBundle.Empty, ResourceBundle.Empty, origin, ImmutableArray<E4bCompletedMint>.Empty);
    }

    public ResourceBundle QuoteNextMint(ProductionTier tier)
    {
        if (!Enum.IsDefined(tier))
            throw new ArgumentOutOfRangeException(nameof(tier));
        // Family counter is frozen for the entire pending purchase; it advances
        // only when the fully funded quote is explicitly completed.
        return new E4bEconomicCurveStudy().Quote(CurveA, "mint", tier, AcquiredMints);
    }

    public long RequiredPayments(ProductionTier tier) =>
        ProgressiveConstructionEscrow.MinimumInstallments(QuoteNextMint(tier), Capacity);

    public bool TryBeginMint(ProductionTier tier, out E4bCurveAFundingSandbox next)
    {
        next = this;
        if (HasPendingPurchase || !Enum.IsDefined(tier)) return false;
        try
        {
            var quote = QuoteNextMint(tier);
            if (quote.Equals(ResourceBundle.Empty)) return false;
            next = Copy(pendingTier: tier, escrow: ProgressiveConstructionEscrow.Start(quote));
            return true;
        }
        catch (OverflowException) { return false; }
    }

    /// <summary>Hypothetical earning/delivery, not production by a real clock.
    /// Physical storage is enforced even while a construction is pending.</summary>
    public bool TryDeliverHypothetical(ResourceBundle materials, out E4bCurveAFundingSandbox next)
    {
        ArgumentNullException.ThrowIfNull(materials);
        next = this;
        if (materials.Equals(ResourceBundle.Empty)) return false;
        try
        {
            var stock = Stock + materials;
            if (!Capacity.Covers(stock)) return false;
            var delivered = HypotheticalDelivered + materials;
            // Keep the complete conservation ledger representable in long milliunits.
            _ = InitialStock + delivered;
            next = Copy(stock: stock, hypotheticalDelivered: delivered);
            return true;
        }
        catch (OverflowException) { return false; }
    }

    public bool TryDeposit(ResourceBundle payment, out E4bCurveAFundingSandbox next)
    {
        ArgumentNullException.ThrowIfNull(payment);
        next = this;
        if (Escrow is null || !Escrow.TryDeposit(Stock, payment, out var funded, out var stock))
            return false;
        next = Copy(stock: stock, escrow: funded);
        return true;
    }

    /// <summary>Completion makes a sandbox purchase only. No actual building,
    /// storage or workplaces are granted to the live v15 campaign.</summary>
    public bool TryComplete(out E4bCurveAFundingSandbox next) =>
        TryCompleteWithCapacity(Capacity, out next);

    /// <summary>RC98: same single debit as the deposit, but retain a verifiable
    /// completion receipt and the NEW physical capacity after the building exists.
    /// This operation cannot create the city building by itself.</summary>
    internal bool TryCompleteWithCapacity(ResourceBundle nextCapacity,
        out E4bCurveAFundingSandbox next)
    {
        ArgumentNullException.ThrowIfNull(nextCapacity);
        next = this;
        if (Escrow is null || !Escrow.IsFullyPaid || PendingTier is null ||
            AcquiredMints == int.MaxValue || !nextCapacity.Covers(Stock)) return false;
        try
        {
            var committed = Committed + Escrow.Paid;
            var receipts = CompletedMints.Add(new E4bCompletedMint(PendingTier.Value,
                AcquiredMints, Escrow.Quote));
            var candidate = new E4bCurveAFundingSandbox(InitialStock, Stock, nextCapacity,
                HypotheticalDelivered, committed, AcquiredMints + 1, null, null,
                RealClockCredits, RealClockDebits, MintOrigin, receipts);
            if (!candidate.Reconciles()) return false;
            next = candidate;
            return true;
        }
        catch (OverflowException) { return false; }
    }

    /// <summary>Full cancellation only. If the refund would exceed the physical
    /// storage capacity, nothing changes and the escrow remains available.</summary>
    public bool TryCancel(out E4bCurveAFundingSandbox next)
    {
        next = this;
        if (Escrow is null || !Escrow.TryRefund(Stock, Capacity, out var refunded)) return false;
        next = Copy(stock: refunded, clearPurchase: true);
        return true;
    }

    /// <summary>Checked conservation including pending and completed payments.</summary>
    public bool Reconciles()
    {
        try
        {
            return (InitialStock + HypotheticalDelivered + RealClockCredits).Equals(
                Stock + Escrowed + Committed + RealClockDebits) && Capacity.Covers(Stock) &&
                (PendingTier is null) == (Escrow is null);
        }
        catch (OverflowException) { return false; }
    }

    /// <summary>RC97: accept only a stock delta measured by the validated native
    /// v15 clock. This method is called by the non-playable shadow after Plan().
    /// Escrow and price never change during a clock tick. No synthetic delivery
    /// or sandbox building completion is authorized by this operation.</summary>
    internal bool TryRecordRealClock(ResourceBundle previousCityStock,
        ResourceBundle nextCityStock, ResourceBundle nextCityCapacity,
        out E4bCurveAFundingSandbox next)
    {
        ArgumentNullException.ThrowIfNull(previousCityStock);
        ArgumentNullException.ThrowIfNull(nextCityStock);
        ArgumentNullException.ThrowIfNull(nextCityCapacity);
        next = this;
        if (!Stock.Equals(previousCityStock) || !Capacity.Equals(nextCityCapacity) ||
            !Capacity.Covers(nextCityStock)) return false;
        try
        {
            var credits = new List<KeyValuePair<string, ResourceAmount>>();
            var debits = new List<KeyValuePair<string, ResourceAmount>>();
            foreach (var id in previousCityStock.Amounts.Keys.Concat(nextCityStock.Amounts.Keys)
                .Distinct(StringComparer.Ordinal))
            {
                var before = previousCityStock[id].Milliunits;
                var after = nextCityStock[id].Milliunits;
                if (after > before) credits.Add(KeyValuePair.Create(id,
                    ResourceAmount.FromMilliunits(checked(after - before))));
                else if (before > after) debits.Add(KeyValuePair.Create(id,
                    ResourceAmount.FromMilliunits(checked(before - after))));
            }
            next = new E4bCurveAFundingSandbox(InitialStock, nextCityStock, Capacity,
                HypotheticalDelivered, Committed, AcquiredMints, PendingTier, Escrow,
                RealClockCredits + ResourceBundle.FromAmounts(credits),
                RealClockDebits + ResourceBundle.FromAmounts(debits), MintOrigin, CompletedMints);
            if (!next.Reconciles()) { next = this; return false; }
            return true;
        }
        catch (OverflowException) { next = this; return false; }
    }

    private E4bCurveAFundingSandbox Copy(ResourceBundle? stock = null,
        ResourceBundle? hypotheticalDelivered = null, ProductionTier? pendingTier = null,
        ProgressiveConstructionEscrow? escrow = null, bool clearPurchase = false) =>
        new(InitialStock, stock ?? Stock, Capacity,
            hypotheticalDelivered ?? HypotheticalDelivered, Committed, AcquiredMints,
            clearPurchase ? null : pendingTier ?? PendingTier,
            clearPurchase ? null : escrow ?? Escrow, RealClockCredits, RealClockDebits,
            MintOrigin, CompletedMints);
}
