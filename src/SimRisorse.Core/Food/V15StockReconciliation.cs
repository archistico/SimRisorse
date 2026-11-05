using System.Collections.Immutable;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Food;

/// <summary>
/// F1-D05e: independent, physical-inventory audit of a single v15 clock second.
/// The ledger counts city, animal pens, intermediate stock, depot ingredients and
/// prepared food exactly once. Movement between those owners is not production.
/// This diagnostic never mutates the campaign or recalculates any recipe.
/// </summary>
public sealed record V15StockBalanceRow(string ResourceId, long BeforeMilliunits,
    long ProducedMilliunits, long ConsumedMilliunits, long ExpectedMilliunits,
    long ActualMilliunits)
{
    public bool Balanced => ExpectedMilliunits == ActualMilliunits;
    public long DifferenceMilliunits => checked(ActualMilliunits - ExpectedMilliunits);
}

public sealed record V15StockReconciliation(ImmutableArray<V15StockBalanceRow> Rows)
{
    public bool Balanced => Rows.All(row => row.Balanced);
    public ImmutableArray<V15StockBalanceRow> Differences =>
        [.. Rows.Where(row => !row.Balanced)];
}

public static class V15StockAuditor
{
    public static ResourceBundle PhysicalTotals(V15FoodCampaignCheckpoint checkpoint)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        ArgumentNullException.ThrowIfNull(checkpoint.CityCampaign);
        ArgumentNullException.ThrowIfNull(checkpoint.Industry);
        ArgumentNullException.ThrowIfNull(checkpoint.Depot);
        ArgumentNullException.ThrowIfNull(checkpoint.Depot.RawFood);
        // Only the depot owns ready food. An invalid duplicate city stock remains
        // visible in the sum, so a reconciliation cannot silently conceal it.
        return checkpoint.CityCampaign.Resources + checkpoint.Industry.Animals +
            checkpoint.Industry.Transit + checkpoint.Depot.RawFood +
            ResourceBundle.FromAmounts([
                KeyValuePair.Create(UnifiedFoodInputs.ReadyFood, checkpoint.Depot.ReadyFood)]);
    }

    /// <summary>
    /// Compare total physical stock before/after precisely one elapsed second
    /// with the gross flows emitted by that very second of the unified clock.
    /// For a longer advance, audit each second separately; the plan only exposes
    /// the first second's gross flows and must not be mistaken for interval totals.
    /// </summary>
    public static V15StockReconciliation ReconcileSecond(
        V15FoodCampaignCheckpoint before, V15CampaignAdvancePlan advance)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(advance);
        if (advance.SecondsAdvanced != 1 ||
            checked(advance.Next.CityCampaign.ElapsedTicks - before.CityCampaign.ElapsedTicks) != TimeSpan.TicksPerSecond)
            throw new ArgumentException("A single complete simulated second is required.", nameof(advance));

        var original = PhysicalTotals(before);
        var result = PhysicalTotals(advance.Next);
        var generated = advance.FirstSecondProduction;
        var spent = advance.FirstSecondConsumption;
        var ids = original.Amounts.Keys.Concat(result.Amounts.Keys)
            .Concat(generated.Amounts.Keys).Concat(spent.Amounts.Keys)
            .Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal);
        var rows = ids.Select(id =>
        {
            var initial = original[id].Milliunits;
            var produced = generated[id].Milliunits;
            var consumed = spent[id].Milliunits;
            return new V15StockBalanceRow(id, initial, produced, consumed,
                checked(checked(initial + produced) - consumed), result[id].Milliunits);
        });
        return new([.. rows]);
    }
}
