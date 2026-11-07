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
        ['Y'] = new ImmutableSolidColorBrush(Color.Parse("#E6AC52")), // wheat, roof
        ['O'] = new ImmutableSolidColorBrush(Color.Parse("#BE8852")), // bread crust
        ['I'] = new ImmutableSolidColorBrush(Color.Parse("#FFFDF4")), // cow's white coat
        ['P'] = new ImmutableSolidColorBrush(Color.Parse("#D99D8C")), // cow's muzzle
        ['L'] = new ImmutableSolidColorBrush(Color.Parse("#BDC5CA")), // upper stone face
        ['D'] = new ImmutableSolidColorBrush(Color.Parse("#626F7C")), // shaded stone face
        ['T'] = new ImmutableSolidColorBrush(Color.Parse("#9A6947")), // wood, brick
        ['B'] = new ImmutableSolidColorBrush(Color.Parse("#66BED0")), // water, metal shine
        ['R'] = new ImmutableSolidColorBrush(Color.Parse("#D66A57")), // fish and details
        ['C'] = new ImmutableSolidColorBrush(Color.Parse("#8F9BA5"))  // stone, metal
    };

    // Each picture is exactly eight rows of eight cells. '.' is transparent.
    private static readonly IReadOnlyDictionary<string, string[]> Sprites = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["population"] = ["..####..", "..#WW#..", "..#WW#..", "...##...", ".#BBBB#.", ".#BBBB#.", "..#..#..", "..#..#.."],
        ["town-hall"] = ["...YY...", "..YYYY..", ".######.", "########", "#C#WW#C#", "#C####C#", "#C#TT#C#", "########"],
        ["logs"] = ["........", ".######.", ".#TTTT#.", ".#TWWT#.", ".######.", "..#TT#..", "..####..", "........"],
        ["planks"] = ["........", "........", ".######.", ".#TTTT#.", ".######.", ".#TTTT#.", ".######.", "........"],
        ["stone"] = ["........", "...###..", "..#CCC#.", ".#CCWCC#", ".#CCCCC#", "..#CCC#.", "...###..", "........"],
        ["iron-ore"] = ["........", "...###..", "..#TCC#.", ".#CTWTC#", ".#TTCCC#", "..#CTC#.", "...###..", "........"],
        ["coal"] = ["........", "...###..", "..##C##.", ".##C####", ".#######", "..#####.", "...###..", "........"],
        ["iron"] = ["........", "..BBBB..", ".#WWBB#.", "#BBBBBB#", "########", ".######.", "........", "........"],
        ["gold-ore"] = ["........", "...###..", "..#YCC#.", ".#CYWYC#", ".#YYCCC#", "..#CYC#.", "...###..", "........"],
        ["gold"] = ["........", "..YYYY..", ".#WWYY#.", "#YYYYYY#", "########", ".######.", "........", "........"],
        ["swords"] = ["......C.", ".....CC.", "....CC..", "...CC...", "..##....", ".#TT#...", "#TT#....", ".##....."],
        ["coins"] = ["........", "..YYYY..", ".Y####Y.", "Y#YWWY#Y", "Y#YWWY#Y", ".Y####Y.", "..YYYY..", "........"],
        ["fish"] = ["........", "........", "..####..", ".#RRRR##", "#RRWRRR#", ".#RRRR##", "..####..", "........"],
        ["food"] = ["........", "..G..G..", "..#GG#..", ".#YYYY#.", ".#YYYY#.", "..#TT#..", "...##...", "........"],
        ["grain"] = ["...Y....", "..YYY...", "...Y.Y..", "..YYY...", "...Y.Y..", "..YYY...", "...T....", "...T...."],
        ["water"] = ["...BB...", "...BB...", "..BBBB..", ".BBBBBB.", ".BBBWBB.", ".BBBBBB.", "..####..", "........"],
        ["flour"] = ["........", "..####..", "..#WW#..", ".#WWWW#.", ".#WWWW#.", ".#WWWW#.", "..####..", "........"],
        ["bread"] = ["........", "..OOOO..", ".#OOOO#.", "#OOWOOOO", "#OOOOOOO", ".#TTTT#.", "..####..", "........"],
        ["tree-feller"] = ["...GG...", "..GGGG..", ".GGGGGG.", "..GGGG..", "...TT...", "...TT...", ".######.", "........"],
        ["sawmill"] = ["........", "..#####.", ".#YYYY#.", "###YY###", ".#TTTT#.", ".#T##T#.", ".######.", "........"],
        ["stonecutter"] = ["...##...", "..#LL#..", ".#LLLL#.", "#LLL#DD#", "#CCC#DD#", "#CCC#DD#", ".#CC#D#.", "..###..."],
        ["small-house"] = ["...YY...", "..YYYY..", ".YYYYYY.", "########", ".#TTTT#.", ".#T##T#.", ".#T##T#.", ".######."],
        ["large-house"] = ["..YYYY..", ".YYYYYY.", "########", "#TWWTWW#", "#T##T###", "#TWWTWW#", "#TT##TT#", "########"],
        ["fisher"] = ["........", "...B....", "..####.#", ".#BBB###", "#BWBBBB#", ".#BBB###", "..####.#", "........"],
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
        ["gold-smelter"] = [".....YY.", ".....YY.", ".######.", ".#CYYC#.", ".#RWWR#.", ".#YYYY#.", ".######.", "........"],
        ["forge"] = [".....RR.", ".....RR.", ".######.", ".#CTTC#.", ".#RRRR#.", ".#CWWC#.", ".######.", "........"],
        ["mint"] = ["..YYYY..", ".Y####Y.", "########", "#CYYYYC#", "#C#WW#C#", "#CYYYYC#", "########", "........"],
        ["pencils"] = ["..YY....", "..YY.TT.", "..##.TT.", "..TT.##.", "..TT.TT.", "..TT.TT.", "..TT.TT.", "..##.##."],
        ["school"] = ["..YYYY..", ".######.", "##WWWW##", "#C#WW#C#", "#C####C#", "#C#BB#C#", "#C#TT#C#", "########"],
        ["research"] = ["..####..", ".#BBBB#.", "#BBWWBB#", "#BBWWBB#", ".#BBBB#.", "..####..", "...##...", "...##..."],
        ["ship"] = ["...##...", "...#W...", "...#WW..", "...#WWW.", "..#####.", ".#TTTTT#", "..#BBBB#", "...####."],
        ["barracks"] = ["..####..", ".#WWWW#.", "########", "#CBBBBC#", "#C#WW#C#", "#C#WW#C#", "########", "........"],
        // RC54 food industries and intermediate inventories keep the same 8x8 pixel grammar.
        ["stable"] = ["..YYYY..", ".######.", "########", "#TTWWTT#", "#TT##TT#", "#TTTTTT#", "########", "........"],
        ["hunter"] = ["...#....", "..#T#...", ".#TTT#..", "..#T#...", "...#....", "..#G#...", ".GGGGG..", "........"],
        ["butcher"] = ["..####..", ".#YYYY#.", "########", "#RRRRRR#", "#RWWWWR#", "#RRRRRR#", "########", "........"],
        ["garden"] = ["........", ".GG..GG.", "GGGGGGGG", "..G..G..", ".#G##G#.", ".#TTTT#.", "..####..", "........"],
        ["orchard"] = ["..GGGG..", ".GGYYGG.", "GGGYYGGG", ".GGGGGG.", "...TT...", "...TT...", "..####..", "........"],
        ["food-depot"] = ["..YYYY..", ".######.", "########", "#GY##YG#", "#GYWWYG#", "#GY##YG#", "########", "........"],
        ["cattle"] = [".W....W.", "#I####I#", ".#IIII#.", ".I#II#I.", ".#IPPI#.", ".#P##P#.", "..#PP#..", "...##..."],
        ["game"] = [".#....#.", ".#C..C#.", "..#CC#..", ".#CCCC#.", ".#CWWC#.", "..#CC#..", "..#..#..", "........"],
        ["meat"] = ["........", "..####..", ".#RRRR#.", "#RRWWRR#", "#RRRRRR#", ".#RRRR#.", "..####..", "........"],
        ["vegetables"] = ["...GG...", "..GGGG..", "...GG...", ".#GGGG#.", "#GGWWGG#", "#GGGGGG#", ".######.", "........"],
        ["fruit"] = ["...GG...", "..GGGG..", "...TT...", ".#RRRR#.", "#RRWWRR#", "#RRRRRR#", ".######.", "........"]
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
