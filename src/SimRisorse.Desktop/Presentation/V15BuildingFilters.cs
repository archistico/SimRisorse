using Avalonia.Media;
using Avalonia.Media.Immutable;
using SimRisorse.Core.Food;

namespace SimRisorse.Desktop.Presentation;

/// <summary>
/// Presentation-only classification for the v15 building list. A production tier always
/// belongs to its family's sector; filters do not change the catalog or save format.
/// Unrecognised future buildings remain visible in Tutti and Altro.
/// </summary>
public enum V15BuildingSector
{
    All,
    Food,
    Extraction,
    Industry,
    Processing,
    Logistics,
    Housing,
    Civic,
    Military,
    Other
}

public static class V15BuildingSectors
{
    public static V15BuildingSector For(V15BuildingRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        if (row.Kind == V15BuildingKind.FoodIndustry) return V15BuildingSector.Food;
        if (row.Kind == V15BuildingKind.School) return V15BuildingSector.Civic;
        return row.SourceId switch
        {
            "fisher" or "farm" or "well" or "mill" or "bakery" => V15BuildingSector.Food,
            "tree-feller" or "stonecutter" or "iron-mine" or "coal-mine" or "gold-mine" => V15BuildingSector.Extraction,
            "iron-smelter" or "gold-smelter" or "forge" or "mint" => V15BuildingSector.Industry,
            "sawmill" => V15BuildingSector.Processing,
            "warehouse" => V15BuildingSector.Logistics,
            "small-house" or "large-house" => V15BuildingSector.Housing,
            "town-hall" => V15BuildingSector.Civic,
            "barracks" => V15BuildingSector.Military,
            _ => V15BuildingSector.Other
        };
    }
}

/// <summary>A small 8-bit-style filter button with an explicit, accessible selection state.</summary>
public sealed class V15BuildingFilterOption : ObservableObject
{
    private static readonly IBrush SelectedBackground = new ImmutableSolidColorBrush(Color.Parse("#176858"));
    private static readonly IBrush SelectedForeground = new ImmutableSolidColorBrush(Color.Parse("#FFFFFF"));
    private static readonly IBrush NormalBackground = new ImmutableSolidColorBrush(Color.Parse("#F1E6C9"));
    private static readonly IBrush NormalForeground = new ImmutableSolidColorBrush(Color.Parse("#263349"));
    private static readonly IBrush SelectedBorder = new ImmutableSolidColorBrush(Color.Parse("#176858"));
    private static readonly IBrush NormalBorder = new ImmutableSolidColorBrush(Color.Parse("#697787"));
    private bool _selected;

    public V15BuildingFilterOption(V15BuildingSector sector, string label, Action<V15BuildingSector> select)
    {
        Sector = sector;
        Label = label;
        SelectCommand = new UiCommand(() => select(sector));
    }

    public V15BuildingSector Sector { get; }
    public string Label { get; }
    public UiCommand SelectCommand { get; }
    public bool IsSelected => _selected;
    public IBrush Background => _selected ? SelectedBackground : NormalBackground;
    public IBrush Foreground => _selected ? SelectedForeground : NormalForeground;
    public IBrush Border => _selected ? SelectedBorder : NormalBorder;

    internal void UpdateSelection(bool selected)
    {
        if (_selected == selected) return;
        _selected = selected;
        Notify(nameof(IsSelected));
        Notify(nameof(Background));
        Notify(nameof(Foreground));
        Notify(nameof(Border));
    }
}
