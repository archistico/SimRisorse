using System.Collections.ObjectModel;
using System.Diagnostics;
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
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Desktop.Presentation;

public sealed class MainViewModel : ObservableObject
{
    private readonly GameRuntime _runtime;
    private readonly GetGameStateHandler _read;
    private readonly SaveGameHandler _save;
    private readonly LoadGameHandler _load;
    private GameView _view;
    private BuildingRow _selected;
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
        Resources = new(_view.Resources.Select(item => new ResourceRow(item)));
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
            new StartGameHandler(runtime).Handle(new(EconomyFoundationScenario.Create()));
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
    public ObservableCollection<BuildingRow> Buildings { get; }
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
    public string Population => $"{_view.AvailableWorkers} disponibili / {_view.Population} abitanti";
    public string BusyState => _loading ? "Caricamento in corso…" : _busy ? "Salvataggio in corso…" :
        _autoSaveAllowed ? "Salvataggio ogni minuto e alla chiusura" : "Salvataggio automatico sospeso";
    public string SelectedCost => BuildingRow.Format(Selected.View.Cost, BatchSize);
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
                _message = load ? "Partita ripresa in pausa. Il tempo trascorso a gioco chiuso non viene conteggiato." : "Partita salvata.";
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
