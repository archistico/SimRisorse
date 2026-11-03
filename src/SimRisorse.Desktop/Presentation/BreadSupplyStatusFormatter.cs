using System.Globalization;
using SimRisorse.Application.ReadModels;

namespace SimRisorse.Desktop.Presentation;

/// <summary>Original v13 messages, shared verbatim with the v14 sidebar.</summary>
internal static class BreadSupplyStatusFormatter
{
    private static readonly CultureInfo Italian = CultureInfo.GetCultureInfo("it-IT");

    public static string Format(BreadSupplyView supply)
    {
        var diagnosis = supply.State switch
        {
            BreadSupplyState.Hidden => string.Empty,
            BreadSupplyState.FirstAdvancedArrivalNeedsStock =>
                "Serve Pane in scorta per il primo residente avanzato. Controlla Forno, filiera e lavoratori.",
            BreadSupplyState.NominalCapacityInsufficient =>
                $"Capacità nominale dei Forni insufficiente: −{supply.NominalShortfallPerSecond.ToString("0.###", Italian)} Pane/s" +
                (supply.AdditionalAssignedWorkers is int workers && workers > 0
                    ? $" · +{workers} {(workers == 1 ? "addetto" : "addetti")}" : string.Empty),
            BreadSupplyState.SufficientNextSecond => "Produzione Pane sufficiente nel prossimo secondo",
            BreadSupplyState.MissingInputs => "Filiera Pane limitata: input insufficienti",
            BreadSupplyState.StorageFull => "Forno fermo: non c'è spazio per un lotto di Pane. Costruisci un Magazzino (+500 per ogni risorsa) oppure un altro Forno (+50 Pane).",
            _ => "Filiera Pane limitata nel prossimo secondo"
        };
        return supply.ReserveSecondsWithoutProduction is { } seconds
            ? $"{diagnosis}\nScorta senza produzione: {FormatReserve(seconds)} (consumo attuale)"
            : diagnosis;
    }

    private static string FormatReserve(decimal seconds)
    {
        if (seconds < 60m) return $"{seconds:0} s";
        var minutes = decimal.Floor(seconds / 60m);
        if (minutes < 60m) return $"{minutes:0} min {seconds - minutes * 60m:0} s";
        var hours = decimal.Floor(minutes / 60m);
        return $"{hours:0} h {minutes - hours * 60m:0} min";
    }
}
