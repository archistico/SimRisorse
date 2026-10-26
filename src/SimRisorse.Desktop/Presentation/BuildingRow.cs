using System.Globalization;
using SimRisorse.Application.ReadModels;
using SimRisorse.Core.Economy;

namespace SimRisorse.Desktop.Presentation;

public sealed class BuildingRow(BuildingView view) : ObservableObject
{
    private BuildingView _view = view;
    public BuildingView View => _view;
    public string Id => _view.Id;
    public string Name => _view.Name;
    public int Count => _view.Count;
    public int Workers => _view.AssignedWorkers;
    public string Staff => $"{Workers} / {_view.Workplaces}";
    public string Initials => new(Name.Where(char.IsUpper).Take(2).ToArray());
    public string Description => Id switch
    {
        "tree-feller" => "Fornisce i tronchi che alimentano le segherie. Più persone assegnate aumentano la raccolta.",
        "sawmill" => "Trasforma i tronchi in tavole per costruire e ampliare la colonia.",
        "stonecutter" => "Estrae la pietra necessaria agli investimenti e alle nuove costruzioni.",
        _ => "Assegna persone e controlla materiali e spazio per far lavorare la struttura."
    };
    public string Recipe => (_view.InputPerWorker.IsEmpty ? string.Empty : Format(_view.InputPerWorker) + " → ") + Format(_view.OutputPerWorker);
    public string Activity => $"{_view.RunnableWorkers} di {Workers} lavoratori possono produrre nel prossimo secondo.";
    public string Status => _view.BlockReason.HasFlag(ProductionBlockReason.NotConstructed) ? "Da costruire" :
        _view.BlockReason.HasFlag(ProductionBlockReason.NoWorkers) ? "Nessun lavoratore" :
        _view.BlockReason.HasFlag(ProductionBlockReason.MissingInputs) && _view.BlockReason.HasFlag(ProductionBlockReason.StorageFull) ? "Materiali e spazio insufficienti" :
        _view.BlockReason.HasFlag(ProductionBlockReason.MissingInputs) ? "Materie prime insufficienti" :
        _view.BlockReason.HasFlag(ProductionBlockReason.StorageFull) ? "Deposito pieno" : "Pronto a produrre";
    public static string Format(IEnumerable<ResourceLine> lines, int multiplier = 1) => string.Join(" · ", lines.Select(item =>
        (item.Quantity * multiplier).ToString("0.###", CultureInfo.GetCultureInfo("it-IT")) + " " + item.Name.ToLowerInvariant()));
    public void Update(BuildingView updated) { _view = updated; Notify(string.Empty); }
}
