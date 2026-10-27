using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace SimRisorse.Desktop.Presentation;

/// <summary>
/// Original 8×8 pictograms rendered as sharp rectangles. Keys identify catalog items
/// or interface indicators; all graphics live exclusively in Desktop.
/// </summary>
public sealed class PixelGlyph : Control
{
    public static readonly StyledProperty<string> KeyProperty =
        AvaloniaProperty.Register<PixelGlyph, string>(nameof(Key), string.Empty);

    // Supports() can initialize this type outside the UI thread; shared brushes must be immutable.
    private static readonly IReadOnlyDictionary<char, IImmutableBrush> Colors = new Dictionary<char, IImmutableBrush>
    {
        ['#'] = new ImmutableSolidColorBrush(Color.Parse("#263349")), // outline
        ['W'] = new ImmutableSolidColorBrush(Color.Parse("#F2D49B")), // highlight
        ['G'] = new ImmutableSolidColorBrush(Color.Parse("#6AA85C")), // vegetation
        ['Y'] = new ImmutableSolidColorBrush(Color.Parse("#E6AC52")), // wheat, roof, bread
        ['T'] = new ImmutableSolidColorBrush(Color.Parse("#9A6947")), // wood, brick
        ['B'] = new ImmutableSolidColorBrush(Color.Parse("#66BED0")), // water, metal shine
        ['R'] = new ImmutableSolidColorBrush(Color.Parse("#D66A57")), // fish and details
        ['C'] = new ImmutableSolidColorBrush(Color.Parse("#8F9BA5"))  // stone, metal
    };

    // Each picture is exactly eight rows of eight cells. '.' is transparent.
    private static readonly IReadOnlyDictionary<string, string[]> Sprites = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["population"] = ["..####..", "..#WW#..", "..#WW#..", "...##...", ".#BBBB#.", ".#BBBB#.", "..#..#..", "..#..#.."],
        ["logs"] = ["........", ".######.", ".#TTTT#.", ".#TWWT#.", ".######.", "..#TT#..", "..####..", "........"],
        ["planks"] = ["........", "........", ".######.", ".#TTTT#.", ".######.", ".#TTTT#.", ".######.", "........"],
        ["stone"] = ["........", "...###..", "..#CCC#.", ".#CCWCC#", ".#CCCCC#", "..#CCC#.", "...###..", "........"],
        ["iron-ore"] = ["........", "...###..", "..#TCC#.", ".#CTWTC#", ".#TTCCC#", "..#CTC#.", "...###..", "........"],
        ["coal"] = ["........", "...###..", "..##C##.", ".##C####", ".#######", "..#####.", "...###..", "........"],
        ["iron"] = ["........", "..BBBB..", ".#WWBB#.", "#BBBBBB#", "########", ".######.", "........", "........"],
        ["gold-ore"] = ["........", "...###..", "..#YCC#.", ".#CYWYC#", ".#YYCCC#", "..#CYC#.", "...###..", "........"],
        ["gold"] = ["........", "..YYYY..", ".#WWYY#.", "#YYYYYY#", "########", ".######.", "........", "........"],
        ["fish"] = ["........", "........", "..####..", ".#RRRR##", "#RRWRRR#", ".#RRRR##", "..####..", "........"],
        ["food"] = ["........", "..G..G..", "..#GG#..", ".#YYYY#.", ".#YYYY#.", "..#TT#..", "...##...", "........"],
        ["grain"] = ["...Y....", "..YYY...", "...Y.Y..", "..YYY...", "...Y.Y..", "..YYY...", "...T....", "...T...."],
        ["water"] = ["...BB...", "...BB...", "..BBBB..", ".BBBBBB.", ".BBBWBB.", ".BBBBBB.", "..####..", "........"],
        ["flour"] = ["........", "..####..", "..#WW#..", ".#WWWW#.", ".#WWWW#.", ".#WWWW#.", "..####..", "........"],
        ["bread"] = ["........", "..YYYY..", ".#YYYY#.", "#YYWYYYY", "#YYYYYYY", ".#TTTT#.", "..####..", "........"],
        ["tree-feller"] = ["...GG...", "..GGGG..", ".GGGGGG.", "..GGGG..", "...TT...", "...TT...", ".######.", "........"],
        ["sawmill"] = ["........", "..#####.", ".#YYYY#.", "###YY###", ".#TTTT#.", ".#T##T#.", ".######.", "........"],
        ["stonecutter"] = ["........", "...BB...", "..#BB#..", "...##...", ".#CCCC#.", ".#CCCC#.", "..####..", "........"],
        ["small-house"] = ["...YY...", "..YYYY..", ".YYYYYY.", "########", ".#TTTT#.", ".#T##T#.", ".#T##T#.", ".######."],
        ["large-house"] = ["..YYYY..", ".YYYYYY.", "########", "#TWWTWW#", "#T##T###", "#TWWTWW#", "#TT##TT#", "########"],
        ["fisher"] = ["...B....", "...B....", "...B....", "..#B#...", ".#RRR##.", "#RRWRRR#", ".######.", "........"],
        ["food-store"] = ["..YYYY..", ".YYYYYY.", "########", "#GG##GG#", "#GYYYYG#", "#GYYYYG#", "########", "........"],
        ["farm"] = ["...YY...", "..YYYY..", ".######.", ".#TTTT#.", ".#T##T#.", ".#TTTT#.", "GGGGGGGG", "........"],
        ["well"] = [".######.", "..#CC#..", "..#BB#..", "..#BB#..", ".#BBBB#.", ".#BBBB#.", "..####..", "........"],
        ["mill"] = ["...#....", ".#.#.#..", "..###...", "########", "..###...", ".#.#.#..", "...#....", "..###..."],
        ["bakery"] = ["..######", ".#TTTTT#", ".#TYYYT#", ".#YYYYY#", ".#YWWYY#", ".#TTTTT#", "..######", "........"],
        ["warehouse"] = ["..YYYY..", ".YYYYYY.", "########", "#CCCCCC#", "#C####C#", "#CTTTTC#", "########", "........"],
        ["iron-mine"] = ["..CCCC..", ".CCTTCC.", "CCTTTTCC", "CT####TC", "CT#TT#TC", "CT#TT#TC", "########", "........"],
        ["coal-mine"] = ["..CCCC..", ".CCTTCC.", "CCTTTTCC", "CT####TC", "CT#CC#TC", "CT####TC", "########", "........"],
        ["iron-smelter"] = [".....CC.", ".....CC.", ".######.", ".#CCCC#.", ".#RYYR#.", ".#RWWR#.", ".######.", "........"],
        ["gold-mine"] = ["..CCCC..", ".CCYYCC.", "CCYYYYCC", "CY####YC", "CY#YY#YC", "CY#YY#YC", "########", "........"],
        ["gold-smelter"] = [".....YY.", ".....YY.", ".######.", ".#CYYC#.", ".#RWWR#.", ".#YYYY#.", ".######.", "........"]
    };

    static PixelGlyph() => AffectsRender<PixelGlyph>(KeyProperty);

    public string Key
    {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public static bool Supports(string key) => key is not null && Sprites.ContainsKey(key);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (!Sprites.TryGetValue(Key ?? string.Empty, out var pixels)) return;
        var cell = Math.Min(Bounds.Width, Bounds.Height) / 8d;
        if (cell <= 0) return;
        var originX = (Bounds.Width - 8 * cell) / 2;
        var originY = (Bounds.Height - 8 * cell) / 2;
        for (var y = 0; y < pixels.Length; y++)
        {
            for (var x = 0; x < pixels[y].Length; x++)
            {
                if (Colors.TryGetValue(pixels[y][x], out var color))
                    context.FillRectangle(color, new Rect(originX + x * cell, originY + y * cell, cell, cell));
            }
        }
    }
}
