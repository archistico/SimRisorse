namespace SimRisorse.Core.Research;

/// <summary>Research is intangible: a separate nonnegative balance in 1/1000 points.</summary>
public readonly record struct ResearchPoints
{
    private ResearchPoints(long milliPoints) => MilliPoints = milliPoints;

    public long MilliPoints { get; }
    public decimal Units => MilliPoints / 1000m;
    public static ResearchPoints Zero => default;

    public static ResearchPoints FromMilliPoints(long milliPoints)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(milliPoints);
        return new(milliPoints);
    }

    public static ResearchPoints FromUnits(decimal units)
    {
        if (units < 0 || units > (decimal)long.MaxValue / 1000m)
            throw new ArgumentOutOfRangeException(nameof(units));
        var raw = units * 1000m;
        if (raw != decimal.Truncate(raw))
            throw new ArgumentOutOfRangeException(nameof(units), "At most three decimal places are supported.");
        return new((long)raw);
    }
}
