using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Core.Food;

/// <summary>
/// Generation-6 boundary: a normal v15 checkpoint contains only the 15 live
/// productive families. The retired v14 family is reconstructed exclusively in
/// a disposable v14 diagnostic/import projection, never in a new v15 save.
/// Inherited snapshots with occupied retired buildings are kept until the
/// explicit retirement command; they cannot be silently persisted.
/// </summary>
public static class V15LegacyProducerAdapter
{
    public const string RetiredFamilyId = "food-store";


    /// <summary>Native v15 roster cleanup. Retired historical buildings are
    /// kept until an explicit retirement; empty placeholders are removed.
    /// This overload never creates a v14 DTO.</summary>
    public static V15CityCampaignSnapshot WithoutEmptySlot(V15CityCampaignSnapshot city)
    {
        ArgumentNullException.ThrowIfNull(city);
        if (city.ProductiveFamilies.IsDefault)
            throw new ArgumentException("Invalid v15 production portfolio.", nameof(city));
        var retired = city.ProductiveFamilies.Where(f =>
            f is not null && f.FamilyId == RetiredFamilyId).ToArray();
        if (retired.Length != 1) return city;
        var f = retired[0];
        if (f.Base.Buildings != 0 || f.Base.AssignedWorkers != 0 ||
            f.Large.Buildings != 0 || f.Large.AssignedWorkers != 0 ||
            f.Advanced.Buildings != 0 || f.Advanced.AssignedWorkers != 0)
            return city;
        return city with { ProductiveFamilies = city.ProductiveFamilies
            .Where(item => item.FamilyId != RetiredFamilyId).ToImmutableArray() };
    }

    /// <summary>For compatibility import tests only. Normal native operations
    /// use the live 15-family roster without ever padding the retired producer.</summary>
    public static V15CityCampaignSnapshot ForV14Rules(V15CityCampaignSnapshot city)
    {
        ArgumentNullException.ThrowIfNull(city);
        if (city.ProductiveFamilies.IsDefault)
            throw new ArgumentException("Invalid v15 production portfolio.", nameof(city));
        if (city.ProductiveFamilies.Any(f => f is not null && f.FamilyId == RetiredFamilyId))
            return city;
        return city with { ProductiveFamilies = city.ProductiveFamilies.Add(
            new V15ProductiveFamilySnapshot(RetiredFamilyId, default, default, default)) };
    }

    /// <summary>Native-generation check before SQLite serialization. Refuse
    /// occupied retired sites instead of silently discarding their history.</summary>
    public static bool CanOmit(V15CityCampaignSnapshot city)
    {
        ArgumentNullException.ThrowIfNull(city);
        if (city.ProductiveFamilies.IsDefault) return false;
        var matches = city.ProductiveFamilies.Where(f =>
            f is not null && f.FamilyId == RetiredFamilyId).ToArray();
        if (matches.Length == 0)
            return city.ProductiveFamilies.Length == TieredProductionCatalog.KnownFamilyIds.Length - 1;
        if (matches.Length != 1) return false;
        var f = matches[0];
        return f.Base.Buildings == 0 && f.Base.AssignedWorkers == 0 &&
            f.Large.Buildings == 0 && f.Large.AssignedWorkers == 0 &&
            f.Advanced.Buildings == 0 && f.Advanced.AssignedWorkers == 0;
    }

    /// <summary>Only an empty retired family may be omitted from v15 storage.
    /// Reject instead of silently discarding buildings or occupied workers.</summary>
    public static bool CanOmit(V14CampaignSnapshot city)
    {
        ArgumentNullException.ThrowIfNull(city);
        if (city.ProductiveFamilies.IsDefault) return false;
        var matches = city.ProductiveFamilies.Where(f =>
            f is not null && f.FamilyId == RetiredFamilyId).ToArray();
        if (matches.Length == 0)
            return city.ProductiveFamilies.Length == TieredProductionCatalog.KnownFamilyIds.Length - 1;
        if (matches.Length != 1) return false;
        var family = matches[0];
        return family.Base.Buildings == 0 && family.Base.AssignedWorkers == 0 &&
            family.Large.Buildings == 0 && family.Large.AssignedWorkers == 0 &&
            family.Advanced.Buildings == 0 && family.Advanced.AssignedWorkers == 0;
    }

    /// <summary>Remove the empty retired producer from the working v15 roster.
    /// An occupied historical family is deliberately left alone: only explicit
    /// retirement may discard its buildings/workers. All other state is retained.
    /// </summary>
    public static V14CampaignSnapshot WithoutEmptySlot(V14CampaignSnapshot city)
    {
        ArgumentNullException.ThrowIfNull(city);
        if (city.ProductiveFamilies.IsDefault)
            throw new ArgumentException("Invalid production portfolio.", nameof(city));
        var old = city.ProductiveFamilies.Where(f =>
            f is not null && f.FamilyId == RetiredFamilyId).ToArray();
        if (old.Length == 0) return city;
        if (old.Length != 1 || !CanOmit(city)) return city;
        return city with { ProductiveFamilies = city.ProductiveFamilies
            .Where(f => f.FamilyId != RetiredFamilyId).ToImmutableArray() };
    }

    /// <summary>Disposable projection for legacy imports and diagnostics only.
    /// Ordinary v15 validation and native city commands do not call it.</summary>
    public static V14CampaignSnapshot ForV14Rules(V14CampaignSnapshot city)
    {
        ArgumentNullException.ThrowIfNull(city);
        if (city.ProductiveFamilies.IsDefault)
            throw new ArgumentException("Invalid production portfolio.", nameof(city));
        return city.ProductiveFamilies.Any(f => f is not null && f.FamilyId == RetiredFamilyId)
            ? city
            : city with { ProductiveFamilies = RestoreEmptyAdapterSlot(city.ProductiveFamilies) };
    }

    /// <summary>The only v14-shaped representation of a generation-6 save.
    /// This adapter never creates resources, workers, buildings or research.</summary>
    public static ImmutableArray<V14FamilySnapshot> RestoreEmptyAdapterSlot(
        ImmutableArray<V14FamilySnapshot> activeFamilies)
    {
        if (activeFamilies.IsDefault || activeFamilies.Any(f => f is null || f.FamilyId == RetiredFamilyId))
            throw new ArgumentException("Invalid v15 production portfolio.", nameof(activeFamilies));
        return activeFamilies.Append(new V14FamilySnapshot(RetiredFamilyId,
            default, default, default))
            .OrderBy(f => f.FamilyId, StringComparer.Ordinal).ToImmutableArray();
    }
}
