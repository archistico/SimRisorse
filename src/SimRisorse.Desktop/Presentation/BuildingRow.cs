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
        "small-house" => "Offre spazio per nuovi abitanti. La crescita richiede cibo e tempo di gioco; la casa non impiega lavoratori.",
        "fisher" => "Raccoglie pesce da trasformare in cibo nel deposito alimentare.",
        "food-store" => "Trasforma il pesce in cibo per gli abitanti. Il cibo consente alla popolazione di crescere.",
        _ => "Assegna persone e controlla materiali e spazio per far lavorare la struttura."
    };
    public bool IsProducer => !_view.BlockReason.HasFlag(ProductionBlockReason.NotProducer);
    public string Recipe => _view.HousingPerBuilding > 0 ? $"+{_view.HousingPerBuilding} posti abitativi per edificio" :
        (_view.InputPerWorker.IsEmpty ? string.Empty : Format(_view.InputPerWorker) + " → ") + Format(_view.OutputPerWorker);
    public string Activity => !IsProducer ? $"+{_view.HousingPerBuilding} posti per edificio. Nessun lavoratore richiesto." :
        $"{_view.RunnableWorkers} di {Workers} lavoratori possono produrre nel prossimo secondo.";
    public string Status => _view.BlockReason.HasFlag(ProductionBlockReason.NotConstructed) ? "Da costruire" :
        !IsProducer ? $"{(long)Count * _view.HousingPerBuilding} posti abitativi" :
        _view.BlockReason.HasFlag(ProductionBlockReason.NoWorkers) ? "Nessun lavoratore" :
        _view.BlockReason.HasFlag(ProductionBlockReason.MissingInputs) && _view.BlockReason.HasFlag(ProductionBlockReason.StorageFull) ? "Materiali e spazio insufficienti" :
        _view.BlockReason.HasFlag(ProductionBlockReason.MissingInputs) ? "Materie prime insufficienti" :
        _view.BlockReason.HasFlag(ProductionBlockReason.StorageFull) ? "Deposito pieno" : "Pronto a produrre";
    public static string Format(IEnumerable<ResourceLine> lines, int multiplier = 1) => string.Join(" · ", lines.Select(item =>
        (item.Quantity * multiplier).ToString("0.###", CultureInfo.GetCultureInfo("it-IT")) + " " + item.Name.ToLowerInvariant()));
    public void Update(BuildingView updated) { _view = updated; Notify(string.Empty); }
}
