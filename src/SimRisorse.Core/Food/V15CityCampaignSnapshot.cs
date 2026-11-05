using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Core.Food;

/// <summary>
/// RC68: v15-owned civil contract with v15-owned productive portfolio and
/// acquisition history types. The legacy-shaped SQLite bridge is confined to
/// FromInherited/ToInherited; no legacy producer is manufactured here.
/// </summary>
public sealed record V15CityCampaignSnapshot(
    string ScenarioId, long ElapsedTicks, int Population, ResourceBundle Resources,
    ImmutableArray<BuildingState> OtherBuildings,
    ImmutableArray<V15ProductiveFamilySnapshot> ProductiveFamilies,
    ImmutableArray<V15PurchaseCounter> PurchaseCounters,
    int SchoolCount, int GrowthProgressSeconds, int GrowthRemainderTwentieths,
    MilitaryTrainingState Military, SchoolResearchState SchoolResearch,
    long ResearchBalanceMilliPoints, ImmutableArray<string> UnlockedTechnologyIds,
    int PencilPaidStudents)
{
    /// <summary>Strict boundary for already-decoded legacy-shaped city data.
    /// Never interpret a foreign or changed v14 DTO format as v15 data.</summary>
    public static V15CityCampaignSnapshot FromInherited(V14CampaignSnapshot source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.FormatVersion != V14CampaignSnapshots.FormatVersion ||
            source.RulesVersion != V14CampaignSnapshots.RulesVersion)
            throw new ArgumentException("Unsupported inherited civil checkpoint version.", nameof(source));
        if (source.ProductiveFamilies.IsDefault || source.PurchaseCounters.IsDefault)
            throw new ArgumentException("Incomplete inherited production or acquisition history.", nameof(source));
        return new(source.ScenarioId, source.ElapsedTicks, source.Population,
            source.Resources, source.OtherBuildings,
            source.ProductiveFamilies.Select(V15ProductiveFamilySnapshot.FromInherited).ToImmutableArray(),
            source.PurchaseCounters.Select(V15PurchaseCounter.FromInherited).ToImmutableArray(),
            source.SchoolCount, source.GrowthProgressSeconds,
            source.GrowthRemainderTwentieths, source.Military, source.SchoolResearch,
            source.ResearchBalanceMilliPoints, source.UnlockedTechnologyIds,
            source.PencilPaidStudents);
    }

    /// <summary>Checked conversion for legacy fixtures and opt-in imports.
    /// Not used for normal v15 commands or simulation steps.</summary>
    public static implicit operator V15CityCampaignSnapshot(V14CampaignSnapshot source) =>
        FromInherited(source);

    /// <summary>Temporary serialization/clock bridge while those consumers are
    /// migrated. Does not add a food-store family or change any inventory.</summary>
    public V14CampaignSnapshot ToInherited()
    {
        if (ProductiveFamilies.IsDefault || PurchaseCounters.IsDefault)
            throw new ArgumentException("Incomplete v15 production or acquisition history.");
        return new V14CampaignSnapshot(
            V14CampaignSnapshots.FormatVersion, ScenarioId, V14CampaignSnapshots.RulesVersion,
            ElapsedTicks, Population, Resources, OtherBuildings,
            ProductiveFamilies.Select(f => f.ToInherited()).ToImmutableArray(),
            PurchaseCounters.Select(c => c.ToInherited()).ToImmutableArray(),
            SchoolCount, GrowthProgressSeconds, GrowthRemainderTwentieths,
            Military, SchoolResearch, ResearchBalanceMilliPoints,
            UnlockedTechnologyIds, PencilPaidStudents);
    }
}
