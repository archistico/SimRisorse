using System.Collections.Immutable;
using SimRisorse.Core.Content;

namespace SimRisorse.Core.Economy;

/// <summary>Immutable sparse resource vector. Missing resources have quantity zero.</summary>
public sealed class ResourceBundle : IEquatable<ResourceBundle>
{
    private ResourceBundle(ImmutableSortedDictionary<string, ResourceAmount> amounts) => Amounts = amounts;
    public ImmutableSortedDictionary<string, ResourceAmount> Amounts { get; }
    public static ResourceBundle Empty { get; } = new(ImmutableSortedDictionary.Create<string, ResourceAmount>(StringComparer.Ordinal));
    public ResourceAmount this[string resourceId] => Amounts.GetValueOrDefault(resourceId);

    public static ResourceBundle Create(params (string Id, decimal Units)[] amounts) =>
        FromAmounts(amounts.Select(item => KeyValuePair.Create(item.Id, ResourceAmount.FromUnits(item.Units))));

    public static ResourceBundle FromAmounts(IEnumerable<KeyValuePair<string, ResourceAmount>> amounts)
    {
        ArgumentNullException.ThrowIfNull(amounts);
        var builder = ImmutableSortedDictionary.CreateBuilder<string, ResourceAmount>(StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (id, amount) in amounts)
        {
            ContentIdentifier.Validate(id, nameof(amounts));
            if (!seen.Add(id)) throw new ArgumentException($"Duplicate resource: {id}.", nameof(amounts));
            if (amount != ResourceAmount.Zero) builder.Add(id, amount);
        }
        return new(builder.ToImmutable());
    }

    public bool Covers(ResourceBundle other) => other.Amounts.All(item => this[item.Key].Milliunits >= item.Value.Milliunits);

    public ResourceBundle Scale(long multiplier)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(multiplier);
        return FromAmounts(Amounts.Select(item => KeyValuePair.Create(item.Key, item.Value * multiplier)));
    }

    public static ResourceBundle operator +(ResourceBundle left, ResourceBundle right)
    {
        var builder = left.Amounts.ToBuilder();
        foreach (var (id, amount) in right.Amounts) builder[id] = left[id] + amount;
        return new(builder.ToImmutable());
    }

    public static ResourceBundle operator -(ResourceBundle left, ResourceBundle right)
    {
        var builder = left.Amounts.ToBuilder();
        foreach (var (id, amount) in right.Amounts)
        {
            var remaining = left[id] - amount;
            if (remaining == ResourceAmount.Zero) builder.Remove(id); else builder[id] = remaining;
        }
        return new(builder.ToImmutable());
    }

    public bool Equals(ResourceBundle? other) => other is not null && Amounts.SequenceEqual(other.Amounts);
    public override bool Equals(object? obj) => obj is ResourceBundle other && Equals(other);
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var item in Amounts) { hash.Add(item.Key, StringComparer.Ordinal); hash.Add(item.Value); }
        return hash.ToHashCode();
    }
}
