using SimRisorse.Core.Construction;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Core.Food;

/// <summary>
/// V15-owned productive family and acquisition history, independent from the
/// v14 DTO. Occupancies are intentionally the already-tested tier value type.
/// No normalization, sorting or filtering takes place at the bridge.
/// </summary>
public sealed record V15ProductiveFamilySnapshot(string FamilyId,
    TierOccupancy Base, TierOccupancy Large, TierOccupancy Advanced)
{
    public static V15ProductiveFamilySnapshot FromInherited(V14FamilySnapshot family)
    {
        ArgumentNullException.ThrowIfNull(family);
        return new(family.FamilyId, family.Base, family.Large, family.Advanced);
    }

    public V14FamilySnapshot ToInherited() => new(FamilyId, Base, Large, Advanced);
}

/// <summary>Acquisition counts are historical, not the current owned count:
/// withdrawing buildings or upgrading does not erase past purchases.</summary>
public sealed record V15PurchaseCounter(string FamilyId, int Acquired)
{
    public static V15PurchaseCounter FromInherited(V14PurchaseCounter counter)
    {
        ArgumentNullException.ThrowIfNull(counter);
        return new(counter.FamilyId, counter.Acquired);
    }

    public V14PurchaseCounter ToInherited() => new(FamilyId, Acquired);
}
