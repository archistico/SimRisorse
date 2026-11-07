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
        ProgressiveConstructionEscrow? escrow)
    {
        InitialStock = initialStock;
        Stock = stock;
        Capacity = capacity;
        HypotheticalDelivered = hypotheticalDelivered;
        Committed = committed;
        AcquiredMints = acquiredMints;
        PendingTier = pendingTier;
        Escrow = escrow;
    }

    public const string Mode = "curve-A/opt-in/in-memory-only/not-playable";
    public ResourceBundle InitialStock { get; }
    public ResourceBundle Stock { get; }
    public ResourceBundle Capacity { get; }
    public ResourceBundle HypotheticalDelivered { get; }
    public ResourceBundle Committed { get; }
    public int AcquiredMints { get; }
    public ProductionTier? PendingTier { get; }
    public ProgressiveConstructionEscrow? Escrow { get; }
    public ResourceBundle Escrowed => Escrow?.Paid ?? ResourceBundle.Empty;
    public ResourceBundle Outstanding => Escrow?.Outstanding ?? ResourceBundle.Empty;
    public bool HasPendingPurchase => Escrow is not null;

    public const int CheckpointVersion = 1;

    public E4bFundingCheckpoint ExportCheckpoint() => new(CheckpointVersion, InitialStock, Stock,
        Capacity, HypotheticalDelivered, Committed, AcquiredMints, PendingTier,
        Escrow?.Quote, Escrow?.Paid);

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
        var restored = new E4bCurveAFundingSandbox(checkpoint.InitialStock, checkpoint.Stock,
            checkpoint.Capacity, checkpoint.HypotheticalDelivered, checkpoint.Committed,
            checkpoint.AcquiredMints, checkpoint.PendingTier, escrow);
        if (!restored.Reconciles())
            throw new ArgumentException("Financing balances do not reconcile or exceed storage.", nameof(checkpoint));
        return restored;
    }

    public static E4bCurveAFundingSandbox Open(ResourceBundle initialStock,
        ResourceBundle physicalCapacity, int alreadyAcquiredMints = 0)
    {
        ArgumentNullException.ThrowIfNull(initialStock);
        ArgumentNullException.ThrowIfNull(physicalCapacity);
        ArgumentOutOfRangeException.ThrowIfNegative(alreadyAcquiredMints);
        if (!physicalCapacity.Covers(initialStock))
            throw new ArgumentException("Initial stock exceeds physical storage capacity.", nameof(initialStock));
        return new(initialStock, initialStock, physicalCapacity, ResourceBundle.Empty,
            ResourceBundle.Empty, alreadyAcquiredMints, null, null);
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
    public bool TryComplete(out E4bCurveAFundingSandbox next)
    {
        next = this;
        if (Escrow is null || !Escrow.IsFullyPaid || PendingTier is null ||
            AcquiredMints == int.MaxValue) return false;
        try
        {
            var committed = Committed + Escrow.Paid;
            next = new(InitialStock, Stock, Capacity, HypotheticalDelivered,
                committed, AcquiredMints + 1, null, null);
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
            return (InitialStock + HypotheticalDelivered).Equals(
                Stock + Escrowed + Committed) && Capacity.Covers(Stock) &&
                (PendingTier is null) == (Escrow is null);
        }
        catch (OverflowException) { return false; }
    }

    private E4bCurveAFundingSandbox Copy(ResourceBundle? stock = null,
        ResourceBundle? hypotheticalDelivered = null, ProductionTier? pendingTier = null,
        ProgressiveConstructionEscrow? escrow = null, bool clearPurchase = false) =>
        new(InitialStock, stock ?? Stock, Capacity,
            hypotheticalDelivered ?? HypotheticalDelivered, Committed, AcquiredMints,
            clearPurchase ? null : pendingTier ?? PendingTier,
            clearPurchase ? null : escrow ?? Escrow);
}
