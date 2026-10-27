using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.Core.Population;

/// <summary>Additional ration for residents beyond the capacity of the basic housing type.</summary>
public sealed record AdvancedHousingRules
{
    public AdvancedHousingRules(string basicHousingBuildingId, string breadResourceId, ResourceAmount breadPerPersonPerSecond)
    {
        ContentIdentifier.Validate(basicHousingBuildingId, nameof(basicHousingBuildingId));
        ContentIdentifier.Validate(breadResourceId, nameof(breadResourceId));
        if (breadPerPersonPerSecond == ResourceAmount.Zero ||
            breadPerPersonPerSecond.Milliunits > long.MaxValue / int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(breadPerPersonPerSecond));
        BasicHousingBuildingId = basicHousingBuildingId;
        BreadResourceId = breadResourceId;
        BreadPerPersonPerSecond = breadPerPersonPerSecond;
    }

    public string BasicHousingBuildingId { get; }
    public string BreadResourceId { get; }
    public ResourceAmount BreadPerPersonPerSecond { get; }
}
