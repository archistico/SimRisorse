using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Construction;

/// <summary>
/// RC79 experimental, opt-in construction financing. A paid deposit is removed
/// from city inventory and held as a separate material liability. It grants no
/// building/workplace/storage until the entire quotation has been funded.
/// This pure model is NOT connected to a playable campaign or SQLite.
/// </summary>
public sealed record ProgressiveConstructionEscrow
{
    private ProgressiveConstructionEscrow(ResourceBundle quote, ResourceBundle paid)
    {
        Quote = quote;
        Paid = paid;
    }

    public ResourceBundle Quote { get; }
    public ResourceBundle Paid { get; }
    public ResourceBundle Outstanding => Quote - Paid;
    public bool IsFullyPaid => Outstanding.Equals(ResourceBundle.Empty);

    public static ProgressiveConstructionEscrow Start(ResourceBundle quote)
    {
        ArgumentNullException.ThrowIfNull(quote);
        if (quote.Equals(ResourceBundle.Empty))
            throw new ArgumentException("A construction deposit must have a nonzero quotation.", nameof(quote));
        return new(quote, ResourceBundle.Empty);
    }

    /// <summary>Reconstitutes an escrow without creating a payment. Always validates
    /// that the deposited materials are a subset of the frozen quotation.</summary>
    public static ProgressiveConstructionEscrow Restore(ResourceBundle quote, ResourceBundle paid)
    {
        ArgumentNullException.ThrowIfNull(quote);
        ArgumentNullException.ThrowIfNull(paid);
        if (quote.Equals(ResourceBundle.Empty) || !quote.Covers(paid))
            throw new ArgumentException("Escrow amount exceeds its nonzero quotation.", nameof(paid));
        return new(quote, paid);
    }

    /// <summary>
    /// Attempts an atomic transfer from city stock into the escrow. Rejected
    /// transfers never mutate either value. Callers must serialize the escrow
    /// and stock transactionally before enabling this in a real game.
    /// </summary>
    public bool TryDeposit(ResourceBundle cityStock, ResourceBundle payment,
        out ProgressiveConstructionEscrow next, out ResourceBundle remainingCityStock)
    {
        ArgumentNullException.ThrowIfNull(cityStock);
        ArgumentNullException.ThrowIfNull(payment);
        next = this;
        remainingCityStock = cityStock;
        if (payment.Equals(ResourceBundle.Empty) || !Outstanding.Covers(payment) ||
            !cityStock.Covers(payment)) return false;
        next = new(Quote, Paid + payment);
        remainingCityStock = cityStock - payment;
        return true;
    }

    /// <summary>
    /// Cancellation must not overflow city storage. Reject the refund if there
    /// is insufficient space, leaving the escrow available for later handling.
    /// Never silently discard the already-paid materials.
    /// </summary>
    public bool TryRefund(ResourceBundle cityStock, ResourceBundle cityCapacity,
        out ResourceBundle refundedCityStock)
    {
        ArgumentNullException.ThrowIfNull(cityStock);
        ArgumentNullException.ThrowIfNull(cityCapacity);
        refundedCityStock = cityStock;
        if (!cityCapacity.Covers(cityStock)) return false;
        try
        {
            var candidate = cityStock + Paid;
            if (!cityCapacity.Covers(candidate)) return false;
            refundedCityStock = candidate;
            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    /// <summary>
    /// Minimal number of instalments such that every resource in every payment
    /// can fit a declared city capacity. This is *capacity only*: it does not
    /// guarantee resources can be produced, delivered, or financed over time.
    /// Zero capacity for a required resource makes the quotation impossible.
    /// </summary>
    public static long MinimumInstallments(ResourceBundle quote, ResourceBundle capacity)
    {
        ArgumentNullException.ThrowIfNull(quote);
        ArgumentNullException.ThrowIfNull(capacity);
        long count = quote.Equals(ResourceBundle.Empty) ? 0 : 1;
        foreach (var (id, amount) in quote.Amounts)
        {
            var limit = capacity[id].Milliunits;
            if (limit == 0) throw new InvalidOperationException($"No storage capacity for required material '{id}'.");
            var needed = amount.Milliunits / limit + (amount.Milliunits % limit == 0 ? 0 : 1);
            count = Math.Max(count, needed);
        }
        return count;
    }

    /// <summary>
    /// The 1-based installment of a deterministic, exactly reconciling schedule.
    /// No list of potentially millions of installments is allocated. Payments
    /// differ by no more than one milliunit per resource, with the remainder
    /// assigned to earlier installments. They sum exactly to the quote.
    /// </summary>
    public static ResourceBundle Installment(ResourceBundle quote, long count, long index)
    {
        ArgumentNullException.ThrowIfNull(quote);
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (index < 1 || index > count) throw new ArgumentOutOfRangeException(nameof(index));
        return ResourceBundle.FromAmounts(quote.Amounts.Select(pair =>
        {
            var amount = pair.Value.Milliunits;
            var part = amount / count + (index <= amount % count ? 1 : 0);
            return KeyValuePair.Create(pair.Key, ResourceAmount.FromMilliunits(part));
        }));
    }
}
