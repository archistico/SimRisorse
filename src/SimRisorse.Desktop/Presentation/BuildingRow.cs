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
        "small-house" => "Offre 8 posti abitativi. Ogni Casa piccola oltre la prima accelera gli arrivi del 15%, fino al limite complessivo di 2×. Richiede Cibo; non impiega lavoratori.",
        "large-house" => "Offre 32 posti abitativi e accelera gli arrivi del 30%, fino al limite complessivo di 2×. Richiede un Forno costruito e Pane per gli abitanti avanzati, oltre al Cibo.",
        "fisher" => "Raccoglie pesce da trasformare in cibo nel deposito alimentare.",
        "food-store" => "Trasforma il pesce in cibo per gli abitanti. Il cibo consente alla popolazione di crescere.",
        "farm" => "Coltiva il grano da macinare nel mulino. Due fattorie riforniscono un mulino a pieno ritmo.",
        "well" => "Fornisce l'acqua necessaria al forno per impastare il pane.",
        "mill" => "Macina il grano in farina per il forno.",
        "bakery" => "Trasforma farina e acqua in Pane per gli abitanti delle case grandi. Il Cibo di base resta necessario per tutti.",
        "warehouse" => "Amplia lo spazio di ogni risorsa. Quando una scorta raggiunge la capacità la sua produzione si ferma: un magazzino la fa ripartire.",
        "iron-mine" => "Estrae minerale di ferro per la fonderia. Richiede una Casa grande costruita e un lavoratore assegnato.",
        "coal-mine" => "Estrae il carbone usato dalla fonderia. Richiede una Casa grande costruita e un lavoratore assegnato.",
        "iron-smelter" => "Fonde il minerale di ferro usando carbone. Le due miniere devono essere costruite; i materiali estratti sono utilizzabili dal secondo successivo.",
        "gold-mine" => "Estrae minerale d'oro. Richiede una Casa grande costruita e un lavoratore assegnato.",
        "gold-smelter" => "Fonde il minerale d'oro usando carbone. Condivide il carbone con il ferro, che ha priorità quando le scorte non bastano.",
        _ => "Assegna persone e controlla materiali e spazio per far lavorare la struttura."
    };
    public bool IsProducer => !_view.BlockReason.HasFlag(ProductionBlockReason.NotProducer);
    public bool IsStorage => !_view.StoragePerBuilding.IsEmpty;
    public string OutputHeading => IsProducer ? "PRODUCE PER ADDETTO / S" : "EFFETTO";
    public string Output => IsProducer ? Format(_view.OutputPerWorker) : Recipe;
    public IEnumerable<ResourceDetailLine> OutputLines => _view.OutputPerWorker.Select(output =>
        new ResourceDetailLine(output.ResourceId, Format([output])));
    public bool HasInputs => !_view.InputPerWorker.IsEmpty;
    public string Consumption => "Consuma: " + Format(_view.InputPerWorker) + " /s";
    public string Recipe => _view.HousingPerBuilding > 0 ? $"+{_view.HousingPerBuilding} posti abitativi per edificio" :
        IsStorage ? $"{StorageEffect(1)} per edificio" :
        (_view.InputPerWorker.IsEmpty ? string.Empty : Format(_view.InputPerWorker) + " → ") + Format(_view.OutputPerWorker);
    public string Activity => !IsProducer ? (IsStorage ? "Capacità disponibile appena costruito. Nessun lavoratore richiesto." :
        $"+{_view.HousingPerBuilding} posti per edificio. Nessun lavoratore richiesto.") :
        $"{_view.RunnableWorkers} di {Workers} lavoratori possono produrre nel prossimo secondo.";
    public string Diagnosis
    {
        get
        {
            if (Count == 0) return _view.MissingPrerequisites.IsEmpty ? string.Empty :
                $"Per costruire servono prima: {string.Join(", ", _view.MissingPrerequisites)}.";
            if (!IsProducer) return string.Empty;
            if (Workers == 0) return "Assegna almeno un lavoratore per avviare la produzione.";
            var messages = new List<string>();
            if (!_view.MissingInputNames.IsEmpty)
                messages.Add($"Materie prime insufficienti: {string.Join(", ", _view.MissingInputNames)}.");
            if (!_view.FullOutputNames.IsEmpty)
                messages.Add($"Spazio insufficiente per: {string.Join(", ", _view.FullOutputNames)}.");
            if (messages.Count > 0)
                messages.Add("La previsione considera il consumo degli abitanti e le prenotazioni delle strutture con priorità maggiore.");
            return string.Join(" ", messages);
        }
    }
    public bool HasDiagnosis => Diagnosis.Length > 0;
    public string Status => _view.BlockReason.HasFlag(ProductionBlockReason.NotConstructed) ? "Da costruire" :
        IsStorage ? StorageEffect(Count) :
        !IsProducer ? $"{(long)Count * _view.HousingPerBuilding} posti abitativi" :
        _view.BlockReason.HasFlag(ProductionBlockReason.NoWorkers) ? "Nessun lavoratore" :
        _view.RunnableWorkers > 0 && _view.RunnableWorkers < Workers ? $"Produzione parziale: {_view.RunnableWorkers}/{Workers}" :
        _view.BlockReason.HasFlag(ProductionBlockReason.MissingInputs) && _view.BlockReason.HasFlag(ProductionBlockReason.StorageFull) ? "Materiali e spazio insufficienti" :
        _view.BlockReason.HasFlag(ProductionBlockReason.MissingInputs) ? "Materie prime insufficienti" :
        _view.BlockReason.HasFlag(ProductionBlockReason.StorageFull) ? "Deposito pieno" : "Pronto a produrre";
    public static string Format(IEnumerable<ResourceLine> lines, int multiplier = 1) => string.Join(" · ", lines.Select(item =>
        (item.Quantity * multiplier).ToString("0.###", CultureInfo.GetCultureInfo("it-IT")) + " " + item.Name.ToLowerInvariant()));
    private string StorageEffect(int count) => _view.StoragePerBuilding.Select(item => item.Quantity).Distinct().Count() == 1
        ? "+" + (_view.StoragePerBuilding[0].Quantity * count).ToString("#,0.###", CultureInfo.GetCultureInfo("it-IT")) + " capacità per ogni risorsa"
        : "+" + Format(_view.StoragePerBuilding, count) + " di capacità";
    public void Update(BuildingView updated) { _view = updated; Notify(string.Empty); }
}
