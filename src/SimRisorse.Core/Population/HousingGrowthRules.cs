using SimRisorse.Core.Content;

namespace SimRisorse.Core.Population;

/// <summary>
/// Accelerated births without fractional floating-point arithmetic: a normal second is 20 units,
/// an additional small house adds 3 and a large house adds 6, up to 40 units per second.
/// </summary>
public sealed record HousingGrowthRules
{
    public const int UnitsPerSecond = 20;
    public const int MaximumUnitsPerSecond = 40;

    public HousingGrowthRules(string smallHouseId, string largeHouseId)
    {
        ContentIdentifier.Validate(smallHouseId, nameof(smallHouseId));
        ContentIdentifier.Validate(largeHouseId, nameof(largeHouseId));
        if (smallHouseId == largeHouseId) throw new ArgumentException("Housing types must be distinct.");
        SmallHouseId = smallHouseId;
        LargeHouseId = largeHouseId;
    }

    public string SmallHouseId { get; }
    public string LargeHouseId { get; }

    public int Rate(int smallHouseCount, int largeHouseCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(smallHouseCount);
        ArgumentOutOfRangeException.ThrowIfNegative(largeHouseCount);
        var units = UnitsPerSecond + 3L * Math.Max(0, smallHouseCount - 1) + 6L * largeHouseCount;
        return (int)Math.Min(units, MaximumUnitsPerSecond);
    }
}
