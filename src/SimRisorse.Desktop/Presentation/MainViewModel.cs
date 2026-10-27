using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using SimRisorse.Application;
using SimRisorse.Application.Features.Construction.ConstructBuildings;
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
        Buildings = new(_view.Buildings.Select(item => new BuildingRow(item)));
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
        set { if (_selectedResource != value) { _selectedResource = value; Notify(); } }
    }
    public IReadOnlyList<int> BatchSizes { get; } = Array.AsReadOnly(new[] { 1, 5, 10 });
    public BuildingRow Selected { get => _selected; set { if (value is not null && value != _selected) { _selected = value; Refresh(); } } }
    public int BatchSize { get => _batchSize; set { if (BatchSizes.Contains(value)) { _batchSize = value; Refresh(); } } }
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
    public string BreadSupplyStatus
    {
        get
        {
            var diagnosis = _view.BreadSupply.State switch
            {
                BreadSupplyState.Hidden => string.Empty,
                BreadSupplyState.FirstAdvancedArrivalNeedsStock =>
                    "Serve Pane in scorta per il primo residente avanzato. Controlla Forno, filiera e lavoratori.",
                BreadSupplyState.NominalCapacityInsufficient =>
                    $"Capacità nominale dei Forni insufficiente: −{_view.BreadSupply.NominalShortfallPerSecond.ToString("0.###", Italian)} Pane/s" +
                    (_view.BreadSupply.AdditionalAssignedWorkers is int workers && workers > 0
                        ? $" · +{workers} {(workers == 1 ? "addetto" : "addetti")}"
                        : string.Empty),
                BreadSupplyState.SufficientNextSecond => "Produzione Pane sufficiente nel prossimo secondo",
                BreadSupplyState.MissingInputs => "Filiera Pane limitata: input insufficienti",
                BreadSupplyState.StorageFull => "Forno in attesa: deposito quasi pieno",
                _ => "Filiera Pane limitata nel prossimo secondo"
            };
            return _view.BreadSupply.ReserveSecondsWithoutProduction is { } seconds
                ? $"{diagnosis}\nScorta senza produzione: {FormatReserve(seconds)} (consumo attuale)"
                : diagnosis;
        }
    }

    private static string FormatReserve(decimal seconds)
    {
        if (seconds < 60m) return $"{seconds:0} s";
        var minutes = decimal.Floor(seconds / 60m);
        if (minutes < 60m) return $"{minutes:0} min {seconds - minutes * 60m:0} s";
        var hours = decimal.Floor(minutes / 60m);
        return $"{hours:0} h {minutes - hours * 60m:0} min";
    }
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
    public UiCommand NewGameCommand { get; }
    public UiCommand ConfirmNewGameCommand { get; }
    public UiCommand CancelNewGameCommand { get; }
    public AsyncUiCommand SaveCommand { get; }
    public AsyncUiCommand LoadCommand { get; }

    private bool CanConstruct() => Selected.View.MissingPrerequisites.IsEmpty && Selected.Count <= int.MaxValue - BatchSize &&
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
            GameError.MissingPrerequisite => "Manca una struttura necessaria.",
            _ => "L'ordine non può essere eseguito con le quantità richieste."
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
                      "le nuove strutture e le nuove risorse partono da zero; le vecchie scorte restano invariate. " +
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
        foreach (var resource in Resources) resource.Update(_view.Resources.Single(item => item.Id == resource.Id));
        foreach (var building in Buildings) building.Update(_view.Buildings.Single(item => item.Id == building.Id));
        Notify(string.Empty);
        foreach (var command in new[] { TogglePauseCommand, ConstructCommand, AssignCommand, ReleaseCommand, NewGameCommand, ConfirmNewGameCommand, CancelNewGameCommand }) command.Refresh();
        SaveCommand.Refresh();
        LoadCommand.Refresh();
    }
}
