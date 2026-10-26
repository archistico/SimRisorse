using System.Globalization;

namespace SimRisorse.Core.Economy;

/// <summary>Nonnegative fixed-point resource amount, in thousandths of a unit.</summary>
public readonly record struct ResourceAmount : IComparable<ResourceAmount>
{
    public const long Scale = 1000;
    private ResourceAmount(long milliunits) => Milliunits = milliunits;
    public long Milliunits { get; }
    public decimal Units => (decimal)Milliunits / Scale;
    public static ResourceAmount Zero => default;

    public static ResourceAmount FromUnits(decimal units)
    {
        if (units < 0 || units > (decimal)long.MaxValue / Scale)
            throw new ArgumentOutOfRangeException(nameof(units));
        var scaled = units * Scale;
        if (scaled != decimal.Truncate(scaled))
            throw new ArgumentOutOfRangeException(nameof(units), "At most three decimal places are supported.");
        return new((long)scaled);
    }

    public static ResourceAmount FromMilliunits(long milliunits)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(milliunits);
        return new(milliunits);
    }

    public static ResourceAmount operator +(ResourceAmount left, ResourceAmount right) => new(checked(left.Milliunits + right.Milliunits));
    public static ResourceAmount operator -(ResourceAmount left, ResourceAmount right) => FromMilliunits(checked(left.Milliunits - right.Milliunits));
    public static ResourceAmount operator *(ResourceAmount amount, long multiplier)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(multiplier);
        return new(checked(amount.Milliunits * multiplier));
    }

    public int CompareTo(ResourceAmount other) => Milliunits.CompareTo(other.Milliunits);
    public override string ToString() => Units.ToString("0.###", CultureInfo.InvariantCulture);
}
