using System.Collections.ObjectModel;
using System.Globalization;
using SimRisorse.StackProbe.Model;

namespace SimRisorse.StackProbe.Desktop;

public sealed class ProbeViewModel : ObservableObject
{
    private readonly IProbeSnapshotStore _store;
    private ProbeSession _session = ProbeSession.Create();
    private BuildingRow _selected;
    private bool _isBusy;
    private string _message = "La colonia è in pausa. Puoi organizzare i lavoratori prima di avviare la produzione.";

    public ProbeViewModel(IProbeSnapshotStore store)
    {
        _store = store;
        Buildings = new(ProbeCatalog.Buildings.Select(definition => new BuildingRow(definition)));
        _selected = Buildings[0];
        TogglePauseCommand = Command(() => { if (_session.IsPaused) _session.Resume(); else _session.Pause(); });
        ConstructCommand = Command(() => Report(_session.Construct(Selected.Kind)), () => _session.Resources.Covers(ProbeCatalog.Get(Selected.Kind).Cost));
        AssignCommand = Command(() => Report(_session.AssignWorker(Selected.Kind)),
            () => _session.AvailableWorkers > 0 && Selected.Workers < Selected.Count);
        ReleaseCommand = Command(() => Report(_session.ReleaseWorker(Selected.Kind)), () => Selected.Workers > 0);
        NewGameCommand = Command(() => { _session = ProbeSession.Create(); _message = "Nuova colonia pronta. La partita è in pausa."; });
        SaveCommand = new(async () => await SaveAsync(), () => !_isBusy);
        LoadCommand = new(async () => await LoadAsync(), () => !_isBusy);
        Refresh();
    }

    // Set only by the desktop adapter to flush elapsed time before an action.
    public Action? SynchronizeTime { get; set; }
    public ObservableCollection<BuildingRow> Buildings { get; }
    public BuildingRow Selected
    {
        get => _selected;
        set { if (value is not null && value != _selected) { _selected = value; Refresh(); } }
    }
    public ProbeSnapshot Snapshot => _session.Snapshot();
    public string Message => _message;
    public string Clock
    {
        get
        {
            var elapsed = TimeSpan.FromTicks(_session.ElapsedTicks);
            return FormattableString.Invariant($"{_session.ElapsedTicks / TimeSpan.TicksPerHour:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}");
        }
    }
    public string PlayState => _session.IsPaused ? "IN PAUSA" : "IN CORSO";
    public string PauseAction => _session.IsPaused ? "Avvia produzione" : "Metti in pausa";
    public string Logs => _session.Resources.Logs.ToString("N0", CultureInfo.GetCultureInfo("it-IT"));
    public string Planks => _session.Resources.Planks.ToString("N0", CultureInfo.GetCultureInfo("it-IT"));
    public string Stone => _session.Resources.Stone.ToString("N0", CultureInfo.GetCultureInfo("it-IT"));
    public string Population => $"{_session.AvailableWorkers} disponibili / {ProbeCatalog.Population} abitanti";
    public string BusyState => _isBusy ? "Operazione in corso…" : "Salvataggio manuale";
    public UiCommand TogglePauseCommand { get; }
    public UiCommand ConstructCommand { get; }
    public UiCommand AssignCommand { get; }
    public UiCommand ReleaseCommand { get; }
    public UiCommand NewGameCommand { get; }
    public UiCommand SaveCommand { get; }
    public UiCommand LoadCommand { get; }

    private UiCommand Command(Action action, Func<bool>? condition = null) => new(() =>
    {
        SynchronizeTime?.Invoke();
        action();
        Refresh();
    }, () => !_isBusy && (condition?.Invoke() ?? true));

    private void Report(ActionResult result) => _message = result switch
    {
        ActionResult.Success => "Ordine completato.",
        ActionResult.InsufficientResources => "Materiali insufficienti: nessuna risorsa è stata spesa.",
        ActionResult.NoWorkplace => "Costruisci un altro edificio per aumentare i posti di lavoro.",
        ActionResult.NoAvailableWorker => "Non ci sono abitanti disponibili.",
        _ => "Nessun lavoratore da rimuovere."
    };

    public void Advance(TimeSpan elapsed)
    {
        if (elapsed > TimeSpan.FromSeconds(2))
        {
            Pause();
            _message = "Partita sospesa dopo un'interruzione. Riprendi quando vuoi.";
        }
        else _session.Advance(elapsed);
        Refresh();
    }

    public void Pause()
    {
        _session.Pause();
        Refresh();
    }

    public async Task SaveAsync()
    {
        if (_isBusy) return;
        SynchronizeTime?.Invoke();
        var snapshot = _session.Snapshot();
        await RunStorageAsync(() =>
        {
            _store.Save(snapshot);
            return (ProbeSession?)null;
        }, "Partita salvata. Puoi chiudere e riprenderla in seguito.");
    }

    public async Task LoadAsync()
    {
        if (_isBusy) return;
        SynchronizeTime?.Invoke();
        Pause();
        await RunStorageAsync(() => ProbeSession.Restore(_store.Load()), "Partita caricata in pausa, senza avanzamento del tempo.");
    }

    private async Task RunStorageAsync(Func<ProbeSession?> operation, string successMessage)
    {
        _isBusy = true;
        Refresh();
        try
        {
            var restored = await Task.Run(operation);
            if (restored is not null) _session = restored;
            _message = successMessage;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            _message = $"Operazione non riuscita: {exception.Message}";
        }
        finally
        {
            _isBusy = false;
            Refresh();
        }
    }

    private void Refresh()
    {
        foreach (var building in Buildings) building.Update(_session);
        Notify(string.Empty);
        foreach (var command in new[] { TogglePauseCommand, ConstructCommand, AssignCommand, ReleaseCommand, NewGameCommand, SaveCommand, LoadCommand })
            command.Refresh();
    }
}
