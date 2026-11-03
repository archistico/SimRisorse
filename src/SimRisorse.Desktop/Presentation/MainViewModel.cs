using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using SimRisorse.Application;
using SimRisorse.Application.Features.Construction.ConstructBuildings;
using SimRisorse.Application.Features.Military.EnlistRecruits;
using SimRisorse.Application.Features.Military.DischargeSoldiers;
using SimRisorse.Application.Features.Military.CancelRecruits;
using SimRisorse.Application.Features.Persistence.LoadGame;
using SimRisorse.Application.Features.Persistence.SaveGame;
using SimRisorse.Application.Features.Population.AssignWorkers;
using SimRisorse.Application.Features.Population.ReleaseWorkers;
using SimRisorse.Application.Features.Sessions.AdvanceTime;
using SimRisorse.Application.Features.Sessions.GetGameState;
using SimRisorse.Application.Features.Sessions.SetPause;
using SimRisorse.Application.Features.Sessions.StartGame;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.ReadModels;
using SimRisorse.Core.Sessions;
using SimRisorse.Core.Population;

namespace SimRisorse.Desktop.Presentation;

public sealed class MainViewModel : ObservableObject
{
    private static readonly CultureInfo Italian = CultureInfo.GetCultureInfo("it-IT");
    private static readonly StringComparer ResourceNameComparer = StringComparer.Create(Italian, ignoreCase: true);
    private readonly GameRuntime _runtime;
    private readonly GetGameStateHandler _read;
    private readonly SaveGameHandler _save;
    private readonly LoadGameHandler _load;
    private GameView _view;
    private BuildingRow _selected;
    private ResourceRow? _selectedResource;
    private string _message = "La colonia è in pausa. Organizza i lavoratori e avvia la produzione quando vuoi.";
    private bool _busy, _loading, _closing, _initialized, _autoSaveAllowed;
    private bool _newGameConfirmation, _closeFailure;
    private int _batchSize = 1;
    private int _militaryQuantity = 1;
    private Task<bool>? _pendingOperation;

    public MainViewModel(GameRuntime runtime, IGameSaveStore store)
    {
        _runtime = runtime;
        _read = new(runtime);
        _save = new(runtime, store);
        _load = new(runtime, store);
        _view = _read.Handle();
        Resources = new(_view.Resources.OrderBy(item => item.Name, ResourceNameComparer)
            .ThenBy(item => item.Id, StringComparer.Ordinal).Select(item => new ResourceRow(item)));
        Buildings = new(_view.Buildings.Select(item => new BuildingRow(item, _view)));
        _selected = Buildings[0];
        TogglePauseCommand = new(() =>
        {
            SynchronizeTime?.Invoke();
            new SetPauseHandler(runtime).Handle(new(!_view.IsPaused));
            Refresh();
        }, () => !_closing && !_loading && !_newGameConfirmation);
        ConstructCommand = Action(() => new ConstructBuildingsHandler(runtime).Handle(new(Selected.Id, BatchSize)), CanConstruct);
        AssignCommand = Action(() => new AssignWorkersHandler(runtime).Handle(new(Selected.Id, BatchSize)),
            () => _view.AvailableWorkers >= BatchSize && Selected.View.Workplaces - Selected.Workers >= BatchSize);
        ReleaseCommand = Action(() => new ReleaseWorkersHandler(runtime).Handle(new(Selected.Id, BatchSize)), () => Selected.Workers >= BatchSize);
        EnlistCommand = MilitaryAction(() => new EnlistRecruitsHandler(runtime).Handle(new(MilitaryQuantity)),
            () => HasMilitaryRules && MilitaryBarracks > 0 && _view.AvailableWorkers >= MilitaryQuantity &&
                  AvailableSwords >= MilitarySwordCost, "Reclute arruolate. Le Spade sono state consumate una sola volta.");
        DischargeCommand = MilitaryAction(() => new DischargeSoldiersHandler(runtime).Handle(new(MilitaryQuantity)),
            () => _view.Military is { AllowsDischarge: true } army && army.AvailableSoldiers >= MilitaryQuantity,
            "Soldati congedati. Sono di nuovo civili liberi; le Spade non vengono restituite.");
        CancelRecruitsCommand = MilitaryAction(() => new CancelRecruitsHandler(runtime).Handle(new(MilitaryQuantity)),
            () => _view.Military is { } army && (long)army.WaitingRecruits + army.TrainingRecruits >= MilitaryQuantity &&
                  FreeSwordStorage >= MilitarySwordCost,
            "Reclute annullate. Spade restituite nel magazzino.");
        NewGameCommand = new(() => { SynchronizeTime?.Invoke(); Pause(); _newGameConfirmation = true; Refresh(); }, () => !_busy && !_closing);
        ConfirmNewGameCommand = new(() =>
        {
            new StartGameHandler(runtime).Handle(new());
            _newGameConfirmation = false;
            _autoSaveAllowed = true;
            _message = "Nuova colonia pronta. La partita è in pausa.";
            Refresh();
        }, () => !_busy && !_closing && _newGameConfirmation);
        CancelNewGameCommand = new(() => { _newGameConfirmation = false; Refresh(); });
        SaveCommand = new(async () => { await SaveAsync(); }, UnexpectedError, () => !_busy && !_closing);
        LoadCommand = new(async () => { await LoadAsync(); }, UnexpectedError, () => !_busy && !_closing);
        Refresh();
    }

    public Action? SynchronizeTime { get; set; }
    public ObservableCollection<ResourceRow> Resources { get; }
    // The table is alphabetical by its Italian labels; summary cards retain their established order.
    private IEnumerable<ResourceRow> ResourcesInCatalogOrder => Resources.OrderBy(item => item.Id, StringComparer.Ordinal);
    public IEnumerable<ResourceRow> ResourceHighlights => HasAdvancedResidents
        ? ResourcesInCatalogOrder.Where(item => item.Id is "food" or "bread" or "planks") : HasPopulationGrowth
        ? ResourcesInCatalogOrder.Where(item => item.Id is "food" or "planks" or "stone") : ResourcesInCatalogOrder.Take(3);
    public ObservableCollection<BuildingRow> Buildings { get; }
    public ResourceRow? SelectedResource
    {
        get => _selectedResource;
        set { if (_selectedResource != value) { _selectedResource = value; Notify(); Notify(nameof(SelectedResourceStorageDetails)); } }
    }
    // Used both by the selected row and by the Capacity-column tooltip.
    // No permanent explanatory box is needed below the table.
    public string SelectedResourceStorageDetails => SelectedResource?.CapacityBreakdown ??
        "Passa il puntatore sulla capacità di una risorsa per vedere il calcolo dello spazio.";

    private string CapacityBreakdown(ResourceView stock)
    {
        var contributions = _view.Buildings.Select(building => new
        {
            Building = building,
            PerBuilding = building.StoragePerBuilding.FirstOrDefault(item => item.ResourceId == stock.Id)?.Quantity ?? 0m
        }).Where(item => item.PerBuilding > 0m).ToArray();
        var initialCapacity = stock.Capacity - contributions.Sum(item => item.PerBuilding * item.Building.Count);
        var free = Math.Max(0m, stock.Capacity - stock.Quantity);
        string Amount(decimal number) => number.ToString("#,0.###", Italian);

        var breakdown = new List<string>();
        if (initialCapacity > 0m) breakdown.Add($"{Amount(initialCapacity)} iniziale");
        foreach (var item in contributions.Where(item => item.Building.Count > 0 || item.Building.Id == "warehouse"))
        {
            var label = item.Building.Id switch
            {
                "warehouse" => item.Building.Count == 1 ? "Magazzino" : "Magazzini",
                "bakery" => item.Building.Count == 1 ? "Forno" : "Forni",
                "town-hall" => "Municipio",
                "small-house" => item.Building.Count == 1 ? "Casa piccola" : "Case piccole",
                "large-house" => item.Building.Count == 1 ? "Casa grande" : "Case grandi",
                _ => item.Building.Name
            };
            breakdown.Add($"{item.Building.Count} {label} × {Amount(item.PerBuilding)}");
        }
        var explanation = $"{stock.Name}: {Amount(stock.Quantity)} / {Amount(stock.Capacity)}; " +
            $"spazio libero {Amount(free)}. Capienza: {string.Join(" + ", breakdown)}. " +
            "Per aumentarla, costruisci un Magazzino (+500 per ogni risorsa) oppure un edificio che conservi questa risorsa.";
        if (stock.Id is "bread" or "swords" or "coins")
        {
            var producerId = stock.Id switch { "bread" => "bakery", "swords" => "forge", _ => "mint" };
            var producer = _view.Buildings.FirstOrDefault(item => item.Id == producerId);
            var batch = producer?.OutputPerWorker.FirstOrDefault(item => item.ResourceId == stock.Id)?.Quantity ?? 0m;
            if (batch > 0m)
            {
                var producerLabel = stock.Id switch { "bread" => "al Forno", "swords" => "alla Forgia", _ => "alla Zecca" };
                explanation += $" Ogni addetto {producerLabel} richiede almeno {Amount(batch)} posti liberi per un lotto.";
            }
        }
        return explanation;
    }
    public IReadOnlyList<int> BatchSizes { get; } = Array.AsReadOnly(new[] { 1, 5, 10 });
    public BuildingRow Selected { get => _selected; set { if (value is not null && value != _selected) { _selected = value; Refresh(); } } }
    public int BatchSize { get => _batchSize; set { if (BatchSizes.Contains(value)) { _batchSize = value; Refresh(); } } }
    public int MilitaryQuantity
    {
        get => _militaryQuantity;
        set { if (BatchSizes.Contains(value) && _militaryQuantity != value) { _militaryQuantity = value; Refresh(); } }
    }
    public bool HasMilitaryRules => _view.Military is not null;
    private bool HasIndividualTraining => _view.Military is { } military &&
        !military.IndividualProgressSeconds.IsDefault;
    public int MilitaryBarracks => _view.Buildings.FirstOrDefault(item => item.Id == "barracks")?.Count ?? 0;
    public int WaitingRecruits => _view.Military?.WaitingRecruits ?? 0;
    public int TrainingRecruits => _view.Military?.TrainingRecruits ?? 0;
    public int AvailableSoldiers => _view.Military?.AvailableSoldiers ?? 0;
    public int MilitaryPeople => WaitingRecruits + TrainingRecruits + AvailableSoldiers;
    public int AssignedWorkersCount => _view.Population - _view.AvailableWorkers - MilitaryPeople;
    public long MilitarySeats => _view.Military?.Seats ?? 0;
    public long FreeMilitarySeats => Math.Max(0, MilitarySeats - TrainingRecruits);
    public int MilitarySwordCost => (_view.Military?.SwordsPerRecruit ?? 0) * MilitaryQuantity;
    public decimal AvailableSwords => _view.Resources.FirstOrDefault(item => item.Id == "swords")?.Quantity ?? 0m;
    public int MilitaryMaximumEnlistable => _view.Military is not { } military || MilitaryBarracks == 0 ||
        military.SwordsPerRecruit <= 0 ? 0 :
        (int)Math.Min(Math.Max(0, _view.AvailableWorkers),
            Math.Min((decimal)int.MaxValue, Math.Floor(AvailableSwords / military.SwordsPerRecruit)));
    public int MilitaryMaximumCancellable => _view.Military is not { } military ||
        military.SwordsPerRecruit <= 0 ? 0 :
        (int)Math.Min((decimal)WaitingRecruits + (HasIndividualTraining ? TrainingRecruits : 0),
            Math.Min((decimal)int.MaxValue, Math.Floor(FreeSwordStorage / military.SwordsPerRecruit)));
    public string MilitaryLimitsNote => $"Arruolabili ora: {MilitaryMaximumEnlistable} · " +
        $"annullabili con rimborso: {MilitaryMaximumCancellable}.";
    public string MilitarySeatsExplanation => HasIndividualTraining
        ? "Una Caserma richiede la Forgia e ospita una recluta per volta."
        : "Una Caserma richiede la Forgia e ospita due reclute in addestramento.";
    public string MilitaryArmamentExplanation => HasIndividualTraining
        ? "La Forgia produce le Spade. Ogni recluta ne consuma cinque; se annullata le Spade sono restituite. Il congedo è rinviato."
        : "La Forgia produce le Spade. Ogni recluta ne consuma due all'arruolamento; il congedo non le restituisce.";
    public string MilitaryTrainingHeading => HasIndividualTraining
        ? "FORMAZIONE · 30 SECONDI PER RECLUTA" : "FORMAZIONE · 60 SECONDI PER GRUPPO";
    public string MilitaryQueueExplanation => HasIndividualTraining
        ? "I posti liberi avviano la prima recluta in attesa. Ciascuna progredisce individualmente e l'annullamento perde il tempo già svolto."
        : "Le reclute eccedenti attendono in ordine di arrivo. Il gruppo successivo inizia quando termina quello attivo.";
    public decimal FreeSwordStorage
    {
        get
        {
            var swords = _view.Resources.FirstOrDefault(item => item.Id == "swords");
            return swords is null ? 0m : Math.Max(0m, swords.Capacity - swords.Quantity);
        }
    }
    public string MilitaryRoster => $"{_view.Population} abitanti = {AssignedWorkersCount} lavoratori + {_view.AvailableWorkers} liberi + " +
        $"{WaitingRecruits} in coda + {TrainingRecruits} in addestramento + {AvailableSoldiers} soldati";
    public string MilitaryCapacityStatus => $"{MilitaryBarracks} {(MilitaryBarracks == 1 ? "Caserma" : "Caserme")} · " +
        $"{TrainingRecruits}/{MilitarySeats} posti occupati · {FreeMilitarySeats} posti liberi";
    public string MilitaryWeaponsStatus => $"Spade disponibili: {AvailableSwords.ToString("#,0.###", Italian)} · " +
        $"Costo per {MilitaryQuantity} {(MilitaryQuantity == 1 ? "recluta" : "reclute")}: {MilitarySwordCost} Spade";
    public string MilitaryTrainingStatus => _view.Military is not { } army || army.TrainingRecruits == 0
        ? "Nessuna recluta in addestramento."
        : !army.IndividualProgressSeconds.IsDefault
            ? $"{army.TrainingRecruits} reclute in addestramento individuale · prima pronta tra " +
              TimeSpan.FromSeconds(army.TrainingSecondsPerGroup - army.TrainingProgressSeconds).ToString(@"mm\:ss", Italian) + "."
            : $"Gruppo attivo: {army.TrainingRecruits} reclute · mancano " +
              TimeSpan.FromSeconds(army.TrainingSecondsPerGroup - army.TrainingProgressSeconds).ToString(@"mm\:ss", Italian) + " " +
              $"(su {army.TrainingSecondsPerGroup} s per gruppo).";
    public double MilitaryTrainingPercent => _view.Military is { TrainingRecruits: > 0 } army
        ? 100d * army.TrainingProgressSeconds / army.TrainingSecondsPerGroup : 0d;
    public string MilitaryRecruitmentHint => MilitaryBarracks == 0
        ? "Per arruolare costruisci una Caserma dalla scheda Edifici (richiede prima la Forgia)."
        : _view.AvailableWorkers < MilitaryQuantity
            ? $"Abitanti liberi insufficienti: disponibili {_view.AvailableWorkers}, ne servono {MilitaryQuantity}. " +
              "Libera lavoratori o aumenta gli alloggi e attendi nuovi abitanti."
            : AvailableSwords < MilitarySwordCost
                ? $"Spade insufficienti: disponibili {AvailableSwords.ToString("#,0.###", Italian)}, ne servono {MilitarySwordCost}. " +
                  "Assegna lavoratori alla Forgia, controllando Ferro, Carbone e magazzini."
                : "Arruolamento possibile. Le reclute occupano gli alloggi e consumano le razioni normali.";
    public string MilitaryCancelHint => (long)WaitingRecruits + (HasIndividualTraining ? TrainingRecruits : 0) < MilitaryQuantity
        ? $"Reclute annullabili insufficienti: disponibili {WaitingRecruits + (HasIndividualTraining ? TrainingRecruits : 0)}."
        : FreeSwordStorage < MilitarySwordCost
            ? $"Spazio insufficiente per restituire {MilitarySwordCost} Spade: liberi " +
              $"{FreeSwordStorage.ToString("#,0.###", Italian)} posti. Costruisci un Magazzino o libera spazio per le Spade."
            : HasIndividualTraining
                ? "Annulla prima la coda e poi l'addestramento meno avanzato. Le Spade tornano nel magazzino."
                : "Le reclute in coda possono tornare civili. Le loro Spade vengono restituite per intero.";
    public string MilitaryDischargeHint => _view.Military is { AllowsDischarge: false }
        ? "Congedo rinviato fino all'introduzione delle missioni: i soldati restano assegnati."
        : AvailableSoldiers < MilitaryQuantity
        ? $"Soldati disponibili per il congedo: {AvailableSoldiers}. Le reclute in formazione non possono essere congedate."
        : "I soldati congedati tornano civili liberi. Le Spade già consumate non vengono rimborsate.";
    public string EnlistLabel => $"Arruola +{MilitaryQuantity}";
    public string DischargeLabel => $"Congeda −{MilitaryQuantity}";
    public string CancelRecruitsLabel => HasIndividualTraining
        ? $"Annulla reclute −{MilitaryQuantity}" : $"Annulla coda −{MilitaryQuantity}";
    public GameView View => _view;
    public bool IsPaused => _view.IsPaused;
    public bool IsBusy => _busy;
    public bool IsNewGameConfirmationVisible => _newGameConfirmation;
    public bool IsCloseFailureVisible => _closeFailure;
    public string Message => _message;
    public string Clock
    {
        get
        {
            var elapsed = TimeSpan.FromTicks(_view.ElapsedTicks);
            return FormattableString.Invariant($"{_view.ElapsedTicks / TimeSpan.TicksPerHour:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}");
        }
    }
    public string PlayState => IsPaused ? "IN PAUSA" : "IN CORSO";
    public string PauseAction => IsPaused ? "Avvia produzione" : "Metti in pausa";
    public bool HasPopulationGrowth => _view.PopulationState.IntervalSeconds > 0;
    public bool HasAdvancedHousingRules => _view.PopulationState.BreadResourceId is not null;
    public bool HasAdvancedResidents => _view.PopulationState.AdvancedResidents > 0;
    public bool ShowBreadSupplyStatus => _view.BreadSupply.State != BreadSupplyState.Hidden;
    public string ScenarioName => HasPopulationGrowth ? "Crescita civile" : "Economia di base";
    public string Population => HasPopulationGrowth
        ? $"{_view.Population} / {_view.PopulationState.HousingCapacity} abitanti · {_view.AvailableWorkers} liberi"
        : $"{_view.AvailableWorkers} disponibili / {_view.Population} abitanti";
    public string GrowthStatus => _view.PopulationState.BlockReason switch
    {
        GrowthBlockReason.MissingFood => "Manca cibo: crescita sospesa",
        GrowthBlockReason.MissingBread => "Manca pane: crescita avanzata sospesa",
        GrowthBlockReason.HousingFull => "Case piene: costruisci abitazioni",
        GrowthBlockReason.None => GrowthActiveStatus(),
        _ => "Popolazione fissa"
    };
    private string GrowthActiveStatus()
    {
        var growth = _view.PopulationState;
        var progress = growth.ProgressSeconds + growth.GrowthRemainderTwentieths / (decimal)HousingGrowthRules.UnitsPerSecond;
        var speed = growth.GrowthRateTwentieths / (decimal)HousingGrowthRules.UnitsPerSecond;
        var remainingUnits = (long)(growth.IntervalSeconds - growth.ProgressSeconds) * HousingGrowthRules.UnitsPerSecond - growth.GrowthRemainderTwentieths;
        var seconds = (long)Math.Ceiling(remainingUnits / (decimal)growth.GrowthRateTwentieths);
        var position = $"{progress.ToString("0.##", Italian)} / {growth.IntervalSeconds} s";
        return speed == 1m ? $"Prossimo abitante: {position}" :
            $"Abitante: {position} · ×{speed.ToString("0.##", Italian)} · ~{seconds}s";
    }
    public string FoodRequirement => $"{_view.PopulationState.RequiredFoodPerSecond:0.###} cibo/s";
    public string BreadRequirement => $"{_view.PopulationState.RequiredBreadPerSecond:0.###} pane/s · {_view.PopulationState.AdvancedResidents} residenti avanzati";
    public string BreadSupplyStatus => BreadSupplyStatusFormatter.Format(_view.BreadSupply);

    public double GrowthPercent => HasPopulationGrowth
        ? 100d * (_view.PopulationState.ProgressSeconds + _view.PopulationState.GrowthRemainderTwentieths / (double)HousingGrowthRules.UnitsPerSecond) /
          _view.PopulationState.IntervalSeconds : 0d;
    public string BusyState => _loading ? "Caricamento in corso…" : _busy ? "Salvataggio in corso…" :
        _autoSaveAllowed ? "Salvataggio ogni minuto e alla chiusura" : "Salvataggio automatico sospeso";
    public string SelectedCost => BuildingRow.Format(Selected.View.Cost, BatchSize);
    public string ConstructionCostHeading => BatchSize == 1 ? "COSTO DI COSTRUZIONE" : $"COSTO PER {BatchSize} EDIFICI";
    public IEnumerable<ResourceDetailLine> SelectedCostLines => Selected.View.Cost.Select(cost =>
        new ResourceDetailLine(cost.ResourceId, BuildingRow.Format([cost], BatchSize)));
    public string PopulationCount => HasPopulationGrowth
        ? $"{_view.Population} / {_view.PopulationState.HousingCapacity}" : $"{_view.Population}";
    public string AvailableWorkersText => _view.AvailableWorkers == 1 ? "1 libero" : $"{_view.AvailableWorkers} liberi";
    public double HousingFillPercent => HasPopulationGrowth && _view.PopulationState.HousingCapacity > 0
        ? 100d * _view.Population / _view.PopulationState.HousingCapacity : 0d;
    public string BuildLabel => $"Costruisci +{BatchSize}";
    public UiCommand TogglePauseCommand { get; }
    public UiCommand ConstructCommand { get; }
    public UiCommand AssignCommand { get; }
    public UiCommand ReleaseCommand { get; }
    public UiCommand EnlistCommand { get; }
    public UiCommand DischargeCommand { get; }
    public UiCommand CancelRecruitsCommand { get; }
    public UiCommand NewGameCommand { get; }
    public UiCommand ConfirmNewGameCommand { get; }
    public UiCommand CancelNewGameCommand { get; }
    public AsyncUiCommand SaveCommand { get; }
    public AsyncUiCommand LoadCommand { get; }

    private bool CanConstruct() => !Selected.IsScenarioFixed && Selected.View.MissingPrerequisites.IsEmpty && Selected.Count <= int.MaxValue - BatchSize &&
        (!HasPopulationGrowth || (long)BatchSize * Selected.View.HousingPerBuilding <= int.MaxValue - _view.PopulationState.HousingCapacity) &&
        Selected.View.Cost.All(cost => _view.Resources.Single(item => item.Id == cost.ResourceId).Quantity >= cost.Quantity * BatchSize);

    private UiCommand Action(Func<CommandResult> action, Func<bool> condition) => new(() =>
    {
        SynchronizeTime?.Invoke();
        var result = action();
        _message = result.Succeeded ? "Ordine completato." : result.Error switch
        {
            GameError.InsufficientResources => "Materiali insufficienti. Nessuna risorsa spesa.",
            GameError.InsufficientPopulation => "Non ci sono abbastanza abitanti disponibili.",
            GameError.InsufficientWorkplaces => "Servono altri posti di lavoro.",
            GameError.FixedBuilding => "Il Municipio è una struttura iniziale unica e non può essere costruito.",
            GameError.MissingPrerequisite => "Manca una struttura necessaria.",
            _ => "L'ordine non può essere eseguito con le quantità richieste."
        };
        Refresh();
    }, () => !_busy && !_closing && !_newGameConfirmation && condition());

    private UiCommand MilitaryAction(Func<CommandResult> action, Func<bool> condition, string successMessage) => new(() =>
    {
        SynchronizeTime?.Invoke();
        var result = action();
        _message = result.Succeeded ? successMessage : result.Error switch
        {
            GameError.MissingBarracks => "Costruisci una Caserma per arruolare.",
            GameError.InsufficientSwords => "Spade insufficienti: aumenta la produzione della Forgia.",
            GameError.InsufficientPopulation => "Abitanti liberi insufficienti. Libera lavoratori o amplia gli alloggi.",
            GameError.InsufficientSoldiers => "Non ci sono abbastanza soldati addestrati da congedare.",
            GameError.InsufficientQueuedRecruits => HasIndividualTraining
                ? "Non ci sono abbastanza reclute in coda o in addestramento da annullare."
                : "Si possono annullare solo le reclute ancora in coda.",
            GameError.InsufficientStorage => "Magazzino delle Spade pieno: crea spazio prima di annullare reclute.",
            _ => "Ordine militare non eseguibile: nessuna risorsa o persona è stata modificata."
        };
        Refresh();
    }, () => !_busy && !_closing && !_newGameConfirmation && condition());

    public void Advance(TimeSpan elapsed)
    {
        if (!IsPaused)
        {
            var result = new AdvanceTimeHandler(_runtime).Handle(new(elapsed));
            if (!result.Succeeded) { Pause(); _message = "Avanzamento interrotto. La partita è in pausa."; }
        }
        Refresh();
    }

    public void Pause()
    {
        if (!_read.Handle().IsPaused) new SetPauseHandler(_runtime).Handle(new(true));
        Refresh();
    }

    public void Interrupt()
    {
        Pause();
        _message = "Partita sospesa. Riprendi quando vuoi: il tempo dell'interruzione non viene conteggiato.";
        Refresh();
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;
        await PersistAsync(load: true, initial: true);
    }

    public Task<bool> SaveAsync() => PersistAsync(load: false);
    public Task<bool> LoadAsync() => PersistAsync(load: true);
    public Task<bool> AutoSaveAsync() => _autoSaveAllowed && !_newGameConfirmation ? PersistAsync(load: false) : Task.FromResult(false);

    private Task<bool> PersistAsync(bool load, bool initial = false)
    {
        if (_busy || _closing) return Task.FromResult(false);
        SynchronizeTime?.Invoke();
        _pendingOperation = PersistCoreAsync(load, initial);
        return _pendingOperation;
    }

    private async Task<bool> PersistCoreAsync(bool load, bool initial)
    {
        _busy = true;
        _loading = load;
        if (load) Pause();
        Refresh();
        try
        {
            var result = load ? await _load.HandleAsync() : await _save.HandleAsync();
            if (result.Succeeded)
            {
                _autoSaveAllowed = true;
                _closeFailure = false;
                _message = !load ? "Partita salvata." : result.UpgradedFromRules is { } previous
                    ? $"Partita ripresa in pausa e aggiornata dalle regole {previous} a {result.View.RulesVersion}: " +
                      "le strutture aggiunte vengono inizializzate secondo le nuove regole; le vecchie scorte restano invariate. " +
                      "Al prossimo salvataggio il file originale viene conservato come copia accanto alla partita."
                    : "Partita ripresa in pausa. Il tempo trascorso a gioco chiuso non viene conteggiato.";
            }
            else if (initial && result.Error == GameSaveError.NotFound)
            {
                _autoSaveAllowed = true;
                _message = "La tua nuova colonia è pronta. Organizza i lavoratori e avvia la produzione.";
            }
            else
            {
                _message = result.Error switch
                {
                    GameSaveError.NotFound => "Non c'è ancora una partita salvata.",
                    GameSaveError.UnsupportedFormat or GameSaveError.IncompatibleRules => "Il salvataggio usa un formato o regole diversi. Non è stato caricato.",
                    GameSaveError.InvalidData => "Il salvataggio non è valido. La partita attuale è stata conservata.",
                    GameSaveError.Superseded => "Caricamento scartato: nel frattempo la partita è cambiata.",
                    GameSaveError.Canceled => "Operazione annullata.",
                    _ => "Il salvataggio non è accessibile. Controlla lo spazio disponibile e riprova."
                };
            }
            return result.Succeeded || (initial && result.Error == GameSaveError.NotFound);
        }
        finally
        {
            _busy = _loading = false;
            _pendingOperation = null;
            Refresh();
        }
    }

    public async Task<bool> PrepareCloseAsync()
    {
        _closing = true;
        Pause();
        try
        {
            var pending = _pendingOperation;
            if (pending is not null) await pending;
            Pause();
            if (!_autoSaveAllowed) return true;
            var saved = await PersistCoreAsync(load: false, initial: false);
            if (!saved)
            {
                _closeFailure = true;
                _message = "Chiusura interrotta: la partita non è stata salvata. Riprova oppure scegli di uscire senza salvare.";
            }
            return saved;
        }
        finally { _closing = false; Refresh(); }
    }

    public void UnexpectedError(Exception exception)
    {
        Trace.TraceError(exception.ToString());
        Pause();
        _message = "Operazione interrotta per un errore inatteso. La partita è in pausa.";
        Refresh();
    }

    public void ReportCloseFailure(Exception exception) { _closeFailure = true; UnexpectedError(exception); }

    private void Refresh()
    {
        _view = _read.Handle();
        foreach (var resource in Resources)
        {
            var stock = _view.Resources.Single(item => item.Id == resource.Id);
            resource.Update(stock, CapacityBreakdown(stock));
        }
        foreach (var building in Buildings) building.Update(_view.Buildings.Single(item => item.Id == building.Id), _view);
        Notify(string.Empty);
        foreach (var command in new[] { TogglePauseCommand, ConstructCommand, AssignCommand, ReleaseCommand,
            EnlistCommand, DischargeCommand, CancelRecruitsCommand, NewGameCommand, ConfirmNewGameCommand, CancelNewGameCommand }) command.Refresh();
        SaveCommand.Refresh();
        LoadCommand.Refresh();
    }
}
