using System.Globalization;
using SimRisorse.Application.ReadModels;
using SimRisorse.Core.Economy;

namespace SimRisorse.Desktop.Presentation;

public sealed class BuildingRow(BuildingView view, GameView gameView) : ObservableObject
{
    private BuildingView _view = view;
    private GameView _gameView = gameView;
    private static readonly CultureInfo Italian = CultureInfo.GetCultureInfo("it-IT");
    private bool IndividualTraining => _gameView.Military is { } military &&
        !military.IndividualProgressSeconds.IsDefault;
    public BuildingView View => _view;
    public string Id => _view.Id;
    public string Name => _view.Name;
    public int Count => _view.Count;
    public int Workers => _view.AssignedWorkers;
    public string Staff => $"{Workers} / {_view.Workplaces}";
    public string Initials => new(Name.Where(char.IsUpper).Take(2).ToArray());
    public string Description => Id switch
    {
        "town-hall" => "Centro della colonia, presente fin dall'inizio. Fornisce una sola volta 120 Tavole, 80 Pietra e 30 Cibo e offre 1.000 posti per ogni risorsa. Non può essere costruito o demolito.",
        "tree-feller" => "Fornisce i tronchi che alimentano le segherie. Più persone assegnate aumentano la raccolta.",
        "sawmill" => "Trasforma i tronchi in tavole per costruire e ampliare la colonia.",
        "stonecutter" => "Estrae la pietra necessaria agli investimenti e alle nuove costruzioni.",
        "small-house" => "Offre 8 posti abitativi e 10 posti di scorta per Cibo e Pane. Ogni Casa piccola oltre la prima accelera gli arrivi del 15%, fino al limite complessivo di 2×. Richiede Cibo; non impiega lavoratori.",
        "large-house" => "Offre 32 posti abitativi e 25 posti di scorta per Cibo e Pane; accelera gli arrivi del 30%, fino al limite complessivo di 2×. Richiede un Forno costruito e Pane per gli abitanti avanzati, oltre al Cibo.",
        "fisher" => "Raccoglie pesce da trasformare in cibo nel deposito alimentare.",
        "food-store" => "Trasforma il Pesce in Cibo, necessario per la crescita degli abitanti, e conserva fino a 50 unità aggiuntive di Cibo per struttura. Non conserva il Pane.",
        "farm" => "Coltiva il grano da macinare nel mulino. Due fattorie riforniscono un mulino a pieno ritmo.",
        "well" => "Fornisce l'acqua necessaria al forno per impastare il pane.",
        "mill" => "Macina il grano in farina per il forno.",
        "bakery" => "Trasforma Farina e Acqua in Pane per gli abitanti delle Case grandi. Ogni Forno aggiunge 50 posti per il Pane; il Municipio e i Magazzini offrono spazio condiviso. Il Cibo di base resta necessario per tutti.",
        "warehouse" => "Aumenta di 500 unità la capacità di ogni risorsa. Gli edifici produttivi conservano piccole quantità dei propri prodotti; il Deposito alimentare aggiunge spazio soltanto per il Cibo.",
        "iron-mine" => "Estrae minerale di ferro per la fonderia. Richiede una Casa grande costruita e un lavoratore assegnato.",
        "coal-mine" => "Estrae il carbone usato dalla fonderia. Richiede una Casa grande costruita e un lavoratore assegnato.",
        "iron-smelter" => "Fonde il minerale di ferro usando carbone. Le due miniere devono essere costruite; i materiali estratti sono utilizzabili dal secondo successivo.",
        "gold-mine" => "Estrae minerale d'oro. Richiede una Casa grande costruita e un lavoratore assegnato.",
        "gold-smelter" => "Fonde il minerale d'oro usando carbone. Condivide il carbone con il ferro, che ha priorità quando le scorte non bastano.",
        "forge" => "Trasforma 1 Ferro e 1 Carbone in 2 Spade per addetto al secondo. Il carbone va prima alle Fonderie di ferro e d'oro: libera i loro addetti o potenzia le miniere se la Forgia resta senza. Ogni recluta richiede " +
            (IndividualTraining ? "5" : "2") + " Spade, consumate all'arruolamento.",
        "mint" => "Trasforma 1 Oro e 1 Carbone in 3 Monete per addetto al secondo. Il carbone va prima alle Fonderie e alla Forgia: puoi liberare addetti o costruire altre Miniere di carbone. Le Monete serviranno per pagamenti specifici, non per tutte le costruzioni.",
        "barracks" => IndividualTraining
            ? "La Caserma costa 260 Tavole e 200 Pietra e richiede una Forgia. Offre 1 posto individuale senza lavoratori; ogni recluta completa 30 secondi di gioco. È possibile annullare anche gli addestramenti. Reclute e soldati rimangono residenti e consumano le razioni normali."
            : "La Caserma costa 280 Tavole e 200 Pietra e richiede una Forgia. Offre 2 posti di addestramento senza lavoratori assegnati. Ogni gruppo impiega 60 secondi di gioco; le reclute successive attendono in coda. Reclute e soldati usano gli stessi alloggi e le stesse razioni degli altri abitanti.",
        _ => "Assegna persone e controlla materiali e spazio per far lavorare la struttura."
    };
    public bool IsProducer => !_view.BlockReason.HasFlag(ProductionBlockReason.NotProducer);
    public bool HasStorage => !_view.StoragePerBuilding.IsEmpty;
    public bool IsScenarioFixed => _view.IsScenarioFixed;
    public bool IsBuildable => !IsScenarioFixed;
    public string OutputHeading => IsProducer ? "PRODUCE PER ADDETTO / S" : "EFFETTO";
    public string Output => IsProducer ? Format(_view.OutputPerWorker) : Recipe;
    public IEnumerable<ResourceDetailLine> OutputLines => _view.OutputPerWorker.Select(output =>
        new ResourceDetailLine(output.ResourceId, Format([output])));
    public bool HasInputs => !_view.InputPerWorker.IsEmpty;
    public IEnumerable<ResourceDetailLine> ConsumptionLines => _view.InputPerWorker.Select(input =>
        new ResourceDetailLine(input.ResourceId, Format([input])));
    // The same 14px glyph + 12px text template is shared with costs, production and consumption.
    // Storage and housing/training are independent effects, all rendered with the same icon/text template.
    public IEnumerable<ResourceDetailLine> CapacityLines
    {
        get
        {
            if (Id == "town-hall") return [new ResourceDetailLine(Id, "+1.000 posti per ogni risorsa")];
            if (Id == "warehouse") return [new ResourceDetailLine(Id, "+500 posti per ogni risorsa")];
            var lines = new List<ResourceDetailLine>();
            if (Id == "barracks") lines.Add(new ResourceDetailLine(Id, IndividualTraining ? "+1 posto di addestramento per Caserma" : "+2 posti di addestramento per Caserma"));
            if (_view.HousingPerBuilding > 0)
                lines.Add(new ResourceDetailLine("population", $"+{_view.HousingPerBuilding} posti abitativi per edificio"));
            lines.AddRange(_view.StoragePerBuilding.Select(item => new ResourceDetailLine(item.ResourceId,
                $"+{item.Quantity.ToString("#,0.###", Italian)} {item.Name} di capacità")));
            if (lines.Count == 0) lines.Add(new ResourceDetailLine(Id, "Nessuna capacità aggiuntiva"));
            return lines;
        }
    }
    public string Recipe => Id == "barracks" ? (IndividualTraining ? "+1 posto di addestramento per Caserma" : "+2 posti di addestramento per Caserma") :
        _view.HousingPerBuilding > 0 ? $"+{_view.HousingPerBuilding} posti abitativi per edificio" :
        !IsProducer && HasStorage ? $"{StorageEffect(1)} per edificio" :
        (_view.InputPerWorker.IsEmpty ? string.Empty : Format(_view.InputPerWorker) + " → ") + Format(_view.OutputPerWorker);
    public string Activity => Id == "barracks" ? (IndividualTraining ? "Nessun addetto richiesto. Gestisci arruolamento e annullamenti nella scheda Esercito; il congedo è rinviato." : "Nessun addetto richiesto. Gestisci arruolamento, addestramento e congedi nella scheda Esercito.") :
        IsScenarioFixed ? "Il Municipio è una struttura iniziale permanente. Non richiede lavoratori." :
        !IsProducer ? (Id == "warehouse" ? "Capacità disponibile appena costruito. Nessun lavoratore richiesto." :
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
            {
                foreach (var output in _view.OutputPerWorker.Where(item => _view.FullOutputNames.Contains(item.Name)))
                {
                    var stock = _gameView.Resources.First(item => item.Id == output.ResourceId);
                    var free = Math.Max(0m, stock.Capacity - stock.Quantity);
                    messages.Add($"Spazio insufficiente per {output.Name}: {stock.Quantity.ToString("#,0.###", Italian)} / " +
                        $"{stock.Capacity.ToString("#,0.###", Italian)} (liberi {free.ToString("#,0.###", Italian)}; " +
                        $"servono {output.Quantity.ToString("#,0.###", Italian)} posti per lotto e addetto).");
                }
                var warehouse = _gameView.Buildings.FirstOrDefault(item => item.Id == "warehouse");
                if (warehouse is not null)
                {
                    messages.Add("Per aumentare la capacità costruisci un Magazzino (+500 per ogni risorsa).");
                    foreach (var output in _view.OutputPerWorker.Where(item => _view.FullOutputNames.Contains(item.Name)))
                    {
                        var local = _view.StoragePerBuilding.FirstOrDefault(item => item.ResourceId == output.ResourceId)?.Quantity ?? 0m;
                        if (local > 0m)
                            messages.Add($"Anche un altro {Name} aggiunge {local.ToString("#,0.###", Italian)} posti per {output.Name}.");
                    }
                    if (_view.FullOutputNames.Contains("Pane"))
                        messages.Add("Il Deposito alimentare produce Cibo dal Pesce: non immagazzina Pane.");
                }
            }
            if (messages.Count > 0)
                messages.Add("La previsione considera il consumo degli abitanti e le prenotazioni delle strutture con priorità maggiore.");
            return string.Join(" ", messages);
        }
    }
    public bool HasDiagnosis => Diagnosis.Length > 0;
    public string Status => _view.BlockReason.HasFlag(ProductionBlockReason.NotConstructed) ? "Da costruire" :
        IsScenarioFixed ? "Struttura iniziale · capacità condivisa" :
        Id == "barracks" ? $"{(long)Count * (IndividualTraining ? 1 : 2)} posti di addestramento · {_gameView.Military?.TrainingRecruits ?? 0} occupati" :
        Id == "warehouse" ? StorageEffect(Count) :
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
    public void Update(BuildingView updated, GameView gameView) { _view = updated; _gameView = gameView; Notify(string.Empty); }
}
