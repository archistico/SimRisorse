namespace SimRisorse.Desktop.Presentation;

/// <summary>
/// Shared pixel-art presentation for technologies and buildings unlocked by their real tier rules.
/// Never infer an unlock from the name of the technology alone.
/// </summary>
public static class TechnologyPresentation
{
    public static string GlyphForTechnology(string id) => id switch
    {
        _ when id.StartsWith("lavorazioni-", StringComparison.Ordinal) => "sawmill",
        _ when id.StartsWith("alimentazione-", StringComparison.Ordinal) => "bread",
        _ when id.StartsWith("estrazione-", StringComparison.Ordinal) => "coal-mine",
        _ when id.StartsWith("industria-", StringComparison.Ordinal) => "forge",
        _ when id.StartsWith("logistica-", StringComparison.Ordinal) => "warehouse",
        _ when id.StartsWith("navigazione-", StringComparison.Ordinal) => "ship",
        _ => "research"
    };

    public static IReadOnlyList<TechnologyUnlockedBuildingRow> BuildingUnlocks(
        IEnumerable<string> largeFamilies, IEnumerable<string> advancedFamilies,
        Func<string, string> buildingName, string? extraLarge = null, string? extraAdvanced = null)
    {
        ArgumentNullException.ThrowIfNull(largeFamilies);
        ArgumentNullException.ThrowIfNull(advancedFamilies);
        ArgumentNullException.ThrowIfNull(buildingName);
        var entries = new List<TechnologyUnlockedBuildingRow>();
        AddTier(largeFamilies, extraLarge, "Grande");
        AddTier(advancedFamilies, extraAdvanced, "Avanzato");

        void AddTier(IEnumerable<string> families, string? extraId, string tier)
        {
            var ids = extraId is null ? families : families.Append(extraId);
            foreach (var id in ids.Distinct(StringComparer.Ordinal)
                .OrderBy(buildingName, StringComparer.Ordinal))
            {
                var glyph = id is "food-depot-large" or "food-depot-advanced" ? "food-depot" : id;
                entries.Add(new TechnologyUnlockedBuildingRow(buildingName(id), tier,
                    glyph));
            }
        }
        return entries;
    }
}

/// <summary>One icon and label per actually unlocked building tier.</summary>
public sealed record TechnologyUnlockedBuildingRow(string BuildingName, string Tier, string GlyphKey)
{
    public string Label => $"{BuildingName} · {Tier}";
}
