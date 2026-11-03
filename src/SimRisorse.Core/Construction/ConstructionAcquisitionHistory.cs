using System.Collections.Immutable;
using SimRisorse.Core.Content;

namespace SimRisorse.Core.Construction;

/// <summary>
/// Monotone purchase counters per building family. Creating an initial history does not
/// charge existing buildings; upgrading a building does not count as a new acquisition.
/// This type is not yet attached to the v13 session or its saved snapshots.
/// </summary>
public sealed class ConstructionAcquisitionHistory
{
    private readonly ImmutableSortedDictionary<string, int> _acquired;

    private ConstructionAcquisitionHistory(ImmutableSortedDictionary<string, int> acquired) => _acquired = acquired;

    public static ConstructionAcquisitionHistory Empty { get; } = new(
        ImmutableSortedDictionary.Create<string, int>(StringComparer.Ordinal));

    public static ConstructionAcquisitionHistory FromOwned(IEnumerable<(string FamilyId, int Count)> owned)
    {
        ArgumentNullException.ThrowIfNull(owned);
        var builder = ImmutableSortedDictionary.CreateBuilder<string, int>(StringComparer.Ordinal);
        foreach (var (familyId, count) in owned)
        {
            ContentIdentifier.Validate(familyId, nameof(owned));
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (builder.ContainsKey(familyId))
                throw new ArgumentException($"Duplicate building family: {familyId}.", nameof(owned));
            builder.Add(familyId, count);
        }
        return new(builder.ToImmutable());
    }

    public int Acquired(string familyId)
    {
        ContentIdentifier.Validate(familyId, nameof(familyId));
        return _acquired.TryGetValue(familyId, out var count) ? count : 0;
    }

    /// <summary>Return a new history only after an external, atomic construction succeeds.</summary>
    public ConstructionAcquisitionHistory RegisterPurchase(string familyId, int quantity)
    {
        ContentIdentifier.Validate(familyId, nameof(familyId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        return new(_acquired.SetItem(familyId, checked(Acquired(familyId) + quantity)));
    }

    public ImmutableSortedDictionary<string, int> SnapshotCounts() => _acquired;
}
