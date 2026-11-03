using System.Collections.Immutable;
using SimRisorse.Core.Content;

namespace SimRisorse.Core.Research;

/// <summary>Immutable knowledge balance and unlocked technology identities.</summary>
public sealed class ResearchLedger
{
    private ResearchLedger(ResearchPoints balance, ImmutableHashSet<string> unlocked)
    {
        Balance = balance;
        UnlockedTechnologies = unlocked;
    }

    public ResearchPoints Balance { get; }
    public ImmutableHashSet<string> UnlockedTechnologies { get; }
    public static ResearchLedger Empty { get; } = new(ResearchPoints.Zero,
        ImmutableHashSet.Create<string>(StringComparer.Ordinal));

    public ResearchLedger Accrue(int activeResearchers, long seconds, long milliPointsPerResearcherPerSecond) =>
        new(ResearchAccounting.Accrue(Balance, activeResearchers, seconds, milliPointsPerResearcherPerSecond),
            UnlockedTechnologies);

    public static ResearchLedger Restore(ResearchPoints balance, IEnumerable<string> unlockedIds, TechnologyCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(unlockedIds);
        var builder = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
        foreach (var id in unlockedIds)
        {
            ContentIdentifier.Validate(id, nameof(unlockedIds));
            if (!catalog.TryGet(id, out var tech) || tech is null || !tech.Available || !builder.Add(id))
                throw new ArgumentException("Unknown, duplicated or unavailable unlocked technology.", nameof(unlockedIds));
        }
        var unlocked = builder.ToImmutable();
        foreach (var id in unlocked)
        {
            _ = catalog.TryGet(id, out var tech);
            if (tech!.Prerequisites.Any(required => !unlocked.Contains(required)))
                throw new ArgumentException("Unlocked technology is missing a prerequisite.", nameof(unlockedIds));
        }
        return new(balance, unlocked);
    }

    internal ResearchLedger Unlock(TechnologyDefinition technology, ResearchPoints remainder) =>
        new(remainder, UnlockedTechnologies.Add(technology.Id));
}

public enum TechnologyPurchaseFailure
{
    None, UnknownTechnology, NotImplemented, AlreadyUnlocked, MissingPrerequisite,
    MissingBuilding, InsufficientResearchPoints
}

public sealed record TechnologyPurchaseResult(TechnologyPurchaseFailure Failure, ResearchLedger Ledger)
{
    public bool Succeeded => Failure == TechnologyPurchaseFailure.None;
}

/// <summary>Explicit voluntary purchases. Accumulation NEVER selects or unlocks technologies.</summary>
public static class TechnologyPurchases
{
    public static TechnologyPurchaseResult TryUnlock(TechnologyCatalog catalog, ResearchLedger ledger,
        string technologyId, IReadOnlySet<string> ownedBuildingIds)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(ledger);
        ArgumentNullException.ThrowIfNull(ownedBuildingIds);
        if (!catalog.TryGet(technologyId, out var tech) || tech is null)
            return new(TechnologyPurchaseFailure.UnknownTechnology, ledger);
        if (!tech.Available) return new(TechnologyPurchaseFailure.NotImplemented, ledger);
        if (ledger.UnlockedTechnologies.Contains(tech.Id))
            return new(TechnologyPurchaseFailure.AlreadyUnlocked, ledger);
        if (tech.Prerequisites.Any(id => !ledger.UnlockedTechnologies.Contains(id)))
            return new(TechnologyPurchaseFailure.MissingPrerequisite, ledger);
        if (tech.RequiredBuildings.Any(id => !ownedBuildingIds.Contains(id)))
            return new(TechnologyPurchaseFailure.MissingBuilding, ledger);
        if (!ResearchAccounting.TrySpend(ledger.Balance, tech.Cost, out var remainder))
            return new(TechnologyPurchaseFailure.InsufficientResearchPoints, ledger);
        return new(TechnologyPurchaseFailure.None, ledger.Unlock(tech, remainder));
    }
}
