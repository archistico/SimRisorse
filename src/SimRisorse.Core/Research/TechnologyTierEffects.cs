using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;

namespace SimRisorse.Core.Research;

/// <summary>
/// Direct, implemented benefit of a technology in the current v14 productive-tier rules.
/// Grande uses its own research; Avanzato uses the additional research beyond Grande.
/// An earlier prerequisite is not presented as if it alone unlocked Avanzato.
/// </summary>
public sealed record TechnologyTierBenefits(ImmutableArray<string> LargeFamilies,
    ImmutableArray<string> AdvancedFamilies)
{
    public bool HasEffect => !LargeFamilies.IsEmpty || !AdvancedFamilies.IsEmpty;
    public ImmutableHashSet<string> FamilyIds => LargeFamilies.Concat(AdvancedFamilies)
        .ToImmutableHashSet(StringComparer.Ordinal);
}

/// <summary>
/// Checks the technology descriptions against actual tier rules. In particular, an available
/// technology may never charge points while unlocking no implemented productive tier.
/// </summary>
public static class TechnologyTierEffects
{
    public static TechnologyTierBenefits ForTechnology(string technologyId,
        IEnumerable<TieredProductionFamily> families)
    {
        ContentIdentifier.Validate(technologyId, nameof(technologyId));
        ArgumentNullException.ThrowIfNull(families);
        var large = ImmutableArray.CreateBuilder<string>();
        var advanced = ImmutableArray.CreateBuilder<string>();
        foreach (var family in families)
        {
            ArgumentNullException.ThrowIfNull(family);
            var largeRequirements = family.Get(ProductionTier.Large).RequiredTechnologies;
            var advancedRequirements = family.Get(ProductionTier.Advanced).RequiredTechnologies;
            if (largeRequirements.Contains(technologyId, StringComparer.Ordinal))
                large.Add(family.FamilyId);
            if (advancedRequirements.Contains(technologyId, StringComparer.Ordinal) &&
                !largeRequirements.Contains(technologyId, StringComparer.Ordinal))
                advanced.Add(family.FamilyId);
        }
        return new(large.OrderBy(id => id, StringComparer.Ordinal).ToImmutableArray(),
            advanced.OrderBy(id => id, StringComparer.Ordinal).ToImmutableArray());
    }

    public static void ValidateCatalog(TechnologyCatalog catalog,
        IEnumerable<TieredProductionFamily> families)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(families);
        var allFamilies = families.ToArray();
        if (allFamilies.Select(item => item.FamilyId).Distinct(StringComparer.Ordinal).Count() != allFamilies.Length)
            throw new ArgumentException("Duplicate productive family in technology effects.", nameof(families));

        foreach (var family in allFamilies)
        {
            var large = family.Get(ProductionTier.Large).RequiredTechnologies;
            var advanced = family.Get(ProductionTier.Advanced).RequiredTechnologies;
            var advancedOnly = advanced.Except(large, StringComparer.Ordinal).ToArray();
            if (large.IsDefaultOrEmpty || advancedOnly.Length != 1 ||
                large.Any(id => !advanced.Contains(id, StringComparer.Ordinal)))
                throw new ArgumentException($"Invalid tier research path for {family.FamilyId}.", nameof(families));

            foreach (var id in large.Concat(advancedOnly))
                if (!catalog.TryGet(id, out var technology) || technology is null || !technology.Available)
                    throw new ArgumentException($"Tier of {family.FamilyId} requires an unavailable technology: {id}.", nameof(catalog));

            // An advanced-tier purchase must imply its Grande-tier prerequisites; otherwise a
            // player could pay for research with no immediate benefit, contrary to the tree.
            foreach (var advancedId in advancedOnly)
                foreach (var largeId in large)
                    if (!DependsOn(catalog, advancedId, largeId))
                        throw new ArgumentException($"{advancedId} must depend on {largeId} for {family.FamilyId}.", nameof(catalog));
        }

        foreach (var technology in catalog.Technologies)
        {
            var actual = ForTechnology(technology.Id, allFamilies);
            if (technology.Available)
            {
                if (!actual.HasEffect ||
                    !actual.FamilyIds.SetEquals(technology.UnlockedFamilies))
                    throw new ArgumentException($"Purchasable technology {technology.Id} has no matching implemented tier benefit.", nameof(catalog));
            }
            else if (actual.HasEffect)
                throw new ArgumentException($"Unavailable technology {technology.Id} is required by an implemented tier.", nameof(catalog));
        }
    }

    private static bool DependsOn(TechnologyCatalog catalog, string technologyId, string prerequisiteId)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>();
        pending.Push(technologyId);
        while (pending.Count > 0)
        {
            var id = pending.Pop();
            if (!visited.Add(id)) continue;
            if (!catalog.TryGet(id, out var technology) || technology is null) continue;
            foreach (var required in technology.Prerequisites)
            {
                if (required == prerequisiteId) return true;
                pending.Push(required);
            }
        }
        return false;
    }
}
