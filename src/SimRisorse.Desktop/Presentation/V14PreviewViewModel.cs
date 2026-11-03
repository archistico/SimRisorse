using System.Collections.ObjectModel;
using System.Globalization;
using SimRisorse.Application.Scenarios;
using SimRisorse.Application.Persistence;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;

namespace SimRisorse.Desktop.Presentation;

/// <summary>
/// Explicit opt-in preview of the isolated v14 ruleset. It never instantiates the live
/// v13 GameRuntime and never writes to a v13 database. All commands run on the UI thread.
/// </summary>
public sealed partial class V14PreviewViewModel : ObservableObject
{
    private static readonly CultureInfo Italian = CultureInfo.GetCultureInfo("it-IT");
    private readonly GameScenario _baseline;
    private readonly V14SqliteSaveStore _store;
    private readonly SchoolResearchRules _schoolRules;
    private readonly TechnologyCatalog _technologies;
    private readonly V14TierConstructionRules _tierRules = V14TierConstructionRules.Candidate;
    private readonly Dictionary<string, TieredProductionFamily> _families;
    private V14CampaignSession _session;
    private V14TechnologyRow? _selectedTechnology;
    private V14ProducerRow? _selectedProducer;
    private V14CivicChoice? _selectedCivic;
    private V14TierChoice _buildTier;
    private V14TierChoice _upgradeFrom;
    private V14TierChoice _upgradeTo;
    private string _message = "Anteprima v14 isolata: nessun salvataggio v13 sarà modificato.";
    private bool _initialized;
    private bool _saveAllowed = true;
    // A newly confirmed colony must not silently replace the previous checkpoint
    // via the periodic or closing autosave. An explicit successful Save arms autosave.
    private bool _autoSaveAllowed = true;
    private bool _newGameConfirmation;
    // Match the classic GUI: a pending new-game confirmation blocks colony mutations.
    // This is a presentation gate; the save store and simulation rules are unchanged.
    private bool CanMutateColony => !_newGameConfirmation;

    public V14PreviewViewModel(GameScenario baseline, V14SqliteSaveStore store,
        SchoolResearchRules schoolRules, TechnologyCatalog technologies)
    {
        _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _schoolRules = schoolRules ?? throw new ArgumentNullException(nameof(schoolRules));
        _technologies = technologies ?? throw new ArgumentNullException(nameof(technologies));
        _families = TieredProductionCatalog.FromCatalog(baseline.Catalog, _tierRules.LargeMultiplier,
            _tierRules.AdvancedMultiplier).ToDictionary(item => item.FamilyId, StringComparer.Ordinal);
        _session = FreshSession();
        TierChoices =
        [
            new(ProductionTier.Base, "Base · 1 lavoratore"),
            new(ProductionTier.Large, "Grande · 3 lavoratori"),
            new(ProductionTier.Advanced, "Avanzato · 10 lavoratori")
        ];
        _buildTier = TierChoices[0];
        _upgradeFrom = TierChoices[0];
        _upgradeTo = TierChoices[1];
        Technologies = new();
        Producers = new();
        Civics = new(baseline.Catalog.Buildings
            .Where(item => !item.IsScenarioFixed && item.Recipe is null)
            .OrderBy(item => item.Name, StringComparer.Ordinal)
            .Select(item => new V14CivicChoice(item.Id, item.Name)));
        _selectedCivic = Civics.FirstOrDefault();
        // Match the classic window: a pending new-game confirmation holds the
        // simulation paused until the player confirms or cancels.
        TogglePauseCommand = new(() => { if (_session.IsPaused) _session.Resume(); else _session.Pause(); Refresh(); },
            () => !_newGameConfirmation);
        BuildSchoolCommand = new(() => Perform(() => _session.BuildSchool(1).Failure.ToString(), "Scuola costruita."),
            () => CanMutateColony);
        EnrollCommand = new(() => Perform(() => _session.EnrollStudents(SchoolQuantity).ToString(), "Studenti iscritti."),
            () => CanMutateColony && SchoolCount > 0 && AvailablePopulation >= SchoolQuantity && SchoolCanAffordEnrollment);
        CancelStudentCommand = new(() => Perform(() => _session.CancelStudents(SchoolQuantity).ToString(), "Formazione annullata; materiali restituiti."),
            () => CanMutateColony && SchoolCancellable >= SchoolQuantity && SchoolCanRefund);
        AssignResearcherCommand = new(() => Perform(() => _session.AssignResearchers(SchoolQuantity).ToString(), "Ricercatori assegnati."),
            () => CanMutateColony && SchoolAvailableResearchers >= SchoolQuantity && SchoolFreeResearchPlaces >= SchoolQuantity);
        UnassignResearcherCommand = new(() => Perform(() => _session.UnassignResearchers(SchoolQuantity).ToString(), "Ricercatori sospesi."),
            () => CanMutateColony && ActiveResearchers >= SchoolQuantity);
        ReturnResearcherCommand = new(() => Perform(() => _session.ReturnResearchersToCivilian(SchoolQuantity).ToString(), "Ricercatori tornati civili."),
            () => CanMutateColony && SchoolAvailableResearchers >= SchoolQuantity);
        CraftPencilsCommand = new(() => Perform(() => _session.CraftPencils(SchoolQuantity).ToString(), "Matite prodotte e depositate."),
            () => CanMutateColony && SchoolCanCraft);
        BuyTechnologyCommand = new(() =>
        {
            var selected = SelectedTechnology;
            if (selected is not null)
                Perform(() => _session.BuyTechnology(selected.Id).ToString(),
                    $"Ricerca {selected.Name} acquistata.");
        }, () => CanMutateColony && CanBuySelectedTechnology);
        BuildProducerCommand = new(() =>
        {
            var selected = SelectedProducer;
            if (selected is not null)
                Perform(() => _session.BuildProducer(selected.Id, BuildTier.Tier, 1).Failure.ToString(),
                    "Edificio produttivo costruito.");
        }, () => CanMutateColony && SelectedProducer is not null);
        UpgradeProducerCommand = new(() =>
        {
            var selected = SelectedProducer;
            if (selected is not null)
                Perform(() => _session.UpgradeProducer(selected.Id, UpgradeFrom.Tier,
                    UpgradeTo.Tier).Failure.ToString(), "Edificio potenziato.");
        }, () => CanMutateColony && SelectedProducer is not null);
        AssignWorkerCommand = new(() =>
        {
            var selected = SelectedProducer;
            if (selected is not null)
                Perform(() => _session.AssignProducerWorkers(selected.Id, BuildTier.Tier, 1).ToString(),
                    "Lavoratore assegnato.");
        }, () => CanMutateColony && SelectedProducer is not null);
        ReleaseWorkerCommand = new(() =>
        {
            var selected = SelectedProducer;
            if (selected is not null)
                Perform(() => _session.ReleaseProducerWorkers(selected.Id, BuildTier.Tier, 1).ToString(),
                    "Lavoratore liberato.");
        }, () => CanMutateColony && SelectedProducer is not null);
        BuildCivicCommand = new(() =>
        {
            var selected = SelectedCivic;
            if (selected is not null)
                Perform(() => _session.BuildCivicBuilding(selected.Id, 1).Failure.ToString(),
                    "Edificio civile costruito.");
        }, () => CanMutateColony && SelectedCivic is not null);
        EnlistCommand = new(() => Perform(() => _session.Enlist(MilitaryQuantity).Error.ToString(), "Reclute arruolate."),
            () => CanMutateColony && MilitaryBarracks > 0 && AvailablePopulation >= MilitaryQuantity && AvailableSwords >= MilitarySwordCost);
        CancelRecruitsCommand = new(() => Perform(() => _session.CancelRecruits(MilitaryQuantity).Error.ToString(), "Reclute annullate; Spade restituite."),
            () => CanMutateColony && MilitaryCancellable >= MilitaryQuantity && FreeSwordStorage >= MilitarySwordCost);
        DischargeCommand = new(() => Perform(() => _session.DischargeSoldiers(MilitaryQuantity).Error.ToString(), "Soldati congedati."),
            () => CanMutateColony && _baseline.MilitaryRules is { IndividualTraining: false } && AvailableSoldiers >= MilitaryQuantity);
        NewGameCommand = new(() => { _session.Pause(); _newGameConfirmation = true; Refresh(); },
            () => !_newGameConfirmation);
        ConfirmNewGameCommand = new(() => { _session = FreshSession(); _newGameConfirmation = false;
            _autoSaveAllowed = false; // Manual Save is required before autosave can overwrite the old checkpoint.
            _message = "Nuova colonia in pausa. Salvataggio automatico sospeso: premi Salva v14 per sostituire il checkpoint precedente."; Refresh(); },
            () => _newGameConfirmation);
        CancelNewGameCommand = new(() => { _newGameConfirmation = false; Refresh(); },
            () => _newGameConfirmation);
        SaveCommand = new(() => Save(), () => CanSave);
        LoadCommand = new(() => Load(), () => _initialized && !_newGameConfirmation);
        InitializeBuildingEntries();
        Refresh();
    }

    public static V14PreviewViewModel CreateForPreview(string savePath)
    {
        var scenario = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100, pencilsPerStudent: 2);
        var tech = TechnologyCatalog.ProposedTierTree;
        return new(scenario, new V14SqliteSaveStore(savePath, scenario, school, tech), school, tech);
    }

    private V14CampaignSession FreshSession() => V14CampaignSession.FromV13(_baseline,
        GameSession.Create(_baseline).Snapshot(), _schoolRules, _technologies,
        V14SchoolConstructionRules.Candidate, _tierRules);

    /// <summary>Only load on opening. A corrupt/legacy file blocks Save, not silently overwritten.</summary>
    public void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        if (File.Exists(_store.DatabasePath)) Load();
        else { _message = "Nuova anteprima v14 in pausa. Salvataggio separato: " + _store.DatabasePath; Refresh(); }
    }

    public string DatabasePath => _store.DatabasePath;
    public bool IsPaused => _session.IsPaused;
    public bool CanSave => _saveAllowed && _initialized && !_newGameConfirmation;
    // Manual Save remains available after confirming a new colony; periodic/closing
    // saves resume only after the player deliberately commits that new checkpoint.
    public bool CanAutoSave => CanSave && _autoSaveAllowed;
    public string PauseLabel => IsPaused ? "▶ Riprendi" : "Ⅱ Pausa";
    public string Clock => TimeSpan.FromTicks(_session.Snapshot().ElapsedTicks).ToString(@"d\.hh\:mm\:ss", Italian);
    public int Population => _session.Snapshot().Population;
    public int AvailablePopulation => _session.AvailablePopulation;
    public int SchoolCount => _session.Snapshot().SchoolCount;
    public string Students => $"{_session.Snapshot().SchoolResearch.WaitingStudents} in attesa · {_session.Snapshot().SchoolResearch.TrainingStudents} in formazione";
    public int QualifiedResearchers => _session.Snapshot().SchoolResearch.QualifiedResearchers;
    public int ActiveResearchers => _session.Snapshot().SchoolResearch.ActiveResearchers;
    public string ResearchBalance => _session.ResearchBalance.Units.ToString("N2", Italian) + " punti";
    public string ResearchRate => (_session.Snapshot().SchoolResearch.ActiveResearchers *
        _schoolRules.MilliPointsPerResearcherPerSecond / 1000m).ToString("N2", Italian) + " punti/s";
    public string SchoolRequirements => "Costo iniziale: 120 Legno lavorato, 80 Pietra (crescita proposta +15%). Richiede Casa piccola e Deposito alimentare.";
    public string SchoolTrainingInfo => $"{_schoolRules.TrainingSeatsPerSchool} posto formazione/Scuola · {_schoolRules.TrainingSeconds} s, {_schoolRules.FoodPerStudent.Units:0} Cibo e {_schoolRules.PencilsPerStudent} Matite per persona · {_schoolRules.ResearcherPlacesPerSchool} posti ricerca/Scuola.";
    public string Message => _message;
    public string GameMode => "ANTEPRIMA SPERIMENTALE · civil-growth-14 · SQLite v3 separato";
    public ObservableCollection<V14TechnologyRow> Technologies { get; }
    public ObservableCollection<V14ProducerRow> Producers { get; }
    public ObservableCollection<V14CivicChoice> Civics { get; }
    public IReadOnlyList<V14TierChoice> TierChoices { get; }
    public ObservableCollection<ResourceRow> Resources { get; } = new();
    public IEnumerable<ResourceRow> ResourceHighlights => HasAdvancedResidents
        ? Resources.Where(item => item.Id is "food" or "bread" or "planks")
        : Resources.Where(item => item.Id is "food" or "planks" or "stone");

    public bool CanBuySelectedTechnology => CanMutateColony && SelectedTechnology?.CanPurchase == true;

    public V14TechnologyRow? SelectedTechnology
    {
        get => _selectedTechnology;
        set { _selectedTechnology = value; Notify(); Notify(nameof(SelectedTechnologySummary));
            Notify(nameof(CanBuySelectedTechnology)); BuyTechnologyCommand.Refresh(); }
    }
    public V14ProducerRow? SelectedProducer
    {
        get => _selectedProducer;
        set { _selectedProducer = value; Notify(); Notify(nameof(SelectedProducerSummary)); Notify(nameof(SelectedBuildCost)); Notify(nameof(SelectedUpgradeCost));
            BuildProducerCommand.Refresh(); UpgradeProducerCommand.Refresh(); AssignWorkerCommand.Refresh(); ReleaseWorkerCommand.Refresh(); }
    }
    public V14CivicChoice? SelectedCivic
    {
        get => _selectedCivic;
        set { _selectedCivic = value; Notify(); Notify(nameof(SelectedCivicCost)); BuildCivicCommand.Refresh(); }
    }
    public V14TierChoice BuildTier
    {
        get => _buildTier;
        set { if (value is null) return; _buildTier = value; Notify(); Notify(nameof(SelectedBuildCost)); }
    }
    public V14TierChoice UpgradeFrom
    {
        get => _upgradeFrom;
        set { if (value is null) return; _upgradeFrom = value; Notify(); Notify(nameof(SelectedUpgradeCost)); }
    }
    public V14TierChoice UpgradeTo
    {
        get => _upgradeTo;
        set { if (value is null) return; _upgradeTo = value; Notify(); Notify(nameof(SelectedUpgradeCost)); }
    }

    public string SelectedTechnologySummary => SelectedTechnology is null ? "Seleziona una tecnologia." :
        $"{SelectedTechnology.Name}: {SelectedTechnology.Status} · {SelectedTechnology.Cost} punti.\n" +
        $"Prerequisiti ricerca: {TechnologyNames(SelectedTechnology.Prerequisites)}. Edifici: {BuildingNames(SelectedTechnology.RequiredBuildings)}.";
    public string SelectedProducerSummary => SelectedProducer is null ? "Seleziona una struttura produttiva." :
        $"{SelectedProducer.Name}: {SelectedProducer.Description}\n" +
        $"Prerequisiti edifici: {BuildingNames(_families[SelectedProducer.Id].Source.Prerequisites)}.\n" +
        $"Ricerca per Grande: {TechnologyNames(_families[SelectedProducer.Id].Get(ProductionTier.Large).RequiredTechnologies)}; " +
        $"per Avanzato: {TechnologyNames(_families[SelectedProducer.Id].Get(ProductionTier.Advanced).RequiredTechnologies)}.";
    public string SelectedBuildCost => DescribeQuote(() =>
    {
        var item = _families[SelectedProducer!.Id];
        return ProgressiveConstructionPricing.QuoteNewBuildings(item.Source.Cost, item.Get(BuildTier.Tier).CostMultiplier,
            _tierRules.GrowthFactor, PurchaseCount(item.FamilyId), 1);
    }, "Nuova costruzione");
    public string SelectedUpgradeCost => DescribeQuote(() =>
    {
        var item = _families[SelectedProducer!.Id];
        return ProgressiveConstructionPricing.QuoteUpgrade(item.Source.Cost, item.Get(UpgradeFrom.Tier).CostMultiplier,
            item.Get(UpgradeTo.Tier).CostMultiplier, _tierRules.GrowthFactor, PurchaseCount(item.FamilyId));
    }, "Potenziamento");
    public string SelectedCivicCost => DescribeQuote(() =>
        ProgressiveConstructionPricing.QuoteNewBuildings(_baseline.Catalog.BuildingsById[SelectedCivic!.Id].Cost,
            1m, _tierRules.GrowthFactor, PurchaseCount(SelectedCivic.Id), 1), "Edificio civile");

    private int PurchaseCount(string id) => _session.Snapshot().PurchaseCounters.Single(item => item.FamilyId == id).Acquired;
    private string DescribeQuote(Func<ResourceBundle> quote, string label)
    {
        try { return label + ": " + FormatCost(quote()); }
        catch (Exception exception) when (exception is OverflowException or ArgumentOutOfRangeException or KeyNotFoundException or NullReferenceException)
        { return label + ": non disponibile per la selezione attuale."; }
    }
    private string FormatCost(ResourceBundle bundle) => bundle.Amounts.Count == 0 ? "gratuito" : string.Join(" · ",
        bundle.Amounts.Select(item => $"{item.Value.Units.ToString("N0", Italian)} {_baseline.Catalog.ResourcesById[item.Key].Name}"));
    private string BuildingNames(IEnumerable<string> ids) => Names(ids.Select(id =>
        _baseline.Catalog.BuildingsById.TryGetValue(id, out var building) ? building.Name : id));
    private string TechnologyNames(IEnumerable<string> ids) => Names(ids.Select(id =>
        _technologies.TryGet(id, out var tech) && tech is not null ? tech.Name : id));
    private static string Names(IEnumerable<string> names)
    {
        var text = string.Join(", ", names);
        return text.Length == 0 ? "nessuno" : text;
    }

    private void Perform(Func<string> action, string success)
    {
        if (!CanMutateColony) return;
        try
        {
            var failure = action();
            _message = failure == "None" ? success : "Operazione non eseguita: " + ExplainFailure(failure);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        { _message = "Operazione rifiutata: " + exception.Message; }
        Refresh();
    }

    private static string ExplainFailure(string value) => value switch
    {
        "MissingPrerequisite" => "mancano gli edifici richiesti.",
        "MissingResearch" => "prima occorre acquistare la ricerca necessaria.",
        "MissingSchool" => "prima occorre costruire la Scuola.",
        "InsufficientResources" => "risorse insufficienti.",
        "InsufficientFood" => "Cibo insufficiente.",
        "InsufficientPencils" => "Matite insufficienti: produci Matite nel laboratorio della Scuola.",
        "InsufficientMaterials" => "servono Legno lavorato e Carbone per produrre Matite.",
        "InsufficientPopulation" => "non ci sono abitanti liberi sufficienti.",
        "InsufficientResearchPlaces" => "posti di ricerca insufficienti.",
        "InsufficientWorkplaces" => "posti di lavoro insufficienti nel livello selezionato.",
        "InsufficientAssignedWorkers" => "non ci sono abbastanza lavoratori assegnati.",
        "InsufficientResearchPoints" => "punti ricerca insufficienti.",
        "NotImplemented" => "la tecnologia non ha ancora un effetto implementato.",
        "MissingBuilding" => "manca un edificio richiesto.",
        "MissingSourceBuilding" => "non esiste un edificio di quel livello da potenziare.",
        "Unavailable" => "la tecnologia non ha ancora un effetto implementato.",
        "AlreadyUnlocked" => "tecnologia già sbloccata.",
        "InvalidTier" => "selezionare un livello di destinazione superiore a quello di partenza.",
        "InsufficientSwords" => "Spade insufficienti: aumenta la produzione della Forgia.",
        "InsufficientQueuedRecruits" => "reclute in attesa/formazione insufficienti.",
        "InsufficientStorage" => "magazzino pieno: libera spazio prima dell'operazione o del rimborso.",
        "MissingBarracks" => "prima occorre costruire una Caserma.",
        "DischargeUnavailable" => "il congedo resta rinviato fino all'introduzione delle missioni.",
        _ => value
    };

    public void Advance(TimeSpan elapsed)
    {
        try
        {
            var result = _session.Advance(elapsed);
            if (!result.Succeeded)
            {
                _session.Pause();
                _message = "Simulazione in pausa: " + result.Error;
            }
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            _session.Pause();
            _message = "Simulazione in pausa: " + exception.Message;
        }
        // Keep row instances stable, preserving selection and scroll position.
        Refresh();
    }

    public void Interrupt()
    {
        _session.Pause();
        _message = "Simulazione sospesa per evitare progressi durante la sospensione del sistema.";
        Refresh();
    }

    public bool Save()
    {
        if (_newGameConfirmation)
        {
            _message = "Chiudi la conferma di nuova partita prima di salvare.";
            Refresh();
            return false;
        }
        if (!CanSave)
        {
            _message = "Salvataggio v14 bloccato: controlla il database esistente; nessun file è stato sovrascritto.";
            Refresh();
            return false;
        }
        try
        {
            _store.Save(_session.Snapshot());
            _autoSaveAllowed = true;
            _message = "Anteprima v14 salvata separatamente in: " + _store.DatabasePath;
            Refresh();
            return true;
        }
        catch (Exception exception) when (exception is GameSaveException or IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
            _message = "Salvataggio non riuscito: " + exception.Message;
            Refresh();
            return false;
        }
    }

    public bool Load()
    {
        if (_newGameConfirmation)
        {
            _message = "Chiudi la conferma di nuova partita prima di caricare.";
            Refresh();
            return false;
        }
        try
        {
            var snapshot = _store.Load();
            var restored = V14CampaignSession.Restore(_baseline, snapshot, _schoolRules,
                _technologies, V14SchoolConstructionRules.Candidate, _tierRules);
            _session = restored; // Only a complete, validated load replaces the current game.
            _saveAllowed = true;
            _autoSaveAllowed = true;
            _message = "Salvataggio sperimentale v14 caricato. La sessione riparte in pausa.";
            Refresh();
            return true;
        }
        catch (Exception exception) when (exception is GameSaveException or IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
            if (_initialized && File.Exists(_store.DatabasePath)) _saveAllowed = false;
            _message = "Caricamento v14 rifiutato: " + exception.Message +
                ". Per sicurezza non verrà sovrascritto questo database.";
            Refresh();
            return false;
        }
    }

    private void Refresh()
    {
        var state = _session.Snapshot();
        if (Technologies.Count == 0)
        {
            foreach (var tech in _technologies.Technologies.OrderBy(item => item.Name, StringComparer.Ordinal))
                Technologies.Add(new(tech.Id, tech.Name, tech.Cost.Units.ToString("N2", Italian),
                    tech.Prerequisites, tech.RequiredBuildings));
            _selectedTechnology = Technologies.FirstOrDefault();
            Notify(nameof(SelectedTechnology));
        }
        foreach (var row in Technologies)
        {
            var tech = _technologies.Technologies.Single(item => item.Id == row.Id);
            string status;
            var canPurchase = false;
            if (state.UnlockedTechnologyIds.Contains(tech.Id)) status = "Acquistata";
            else if (!tech.Available) status = "Non disponibile (nessun effetto)";
            else if (tech.Prerequisites.Any(id => !state.UnlockedTechnologyIds.Contains(id))) status = "Richiede tecnologie precedenti";
            else if (tech.RequiredBuildings.Any(id => !Owns(id, state))) status = "Richiede edifici";
            else if (state.ResearchBalanceMilliPoints < tech.Cost.MilliPoints) status = "Punti insufficienti";
            else { status = "Acquistabile"; canPurchase = true; }
            row.Status = status;
            row.CanPurchase = canPurchase;
        }
        if (Producers.Count == 0)
        {
            foreach (var family in state.ProductiveFamilies.OrderBy(item => _baseline.Catalog.BuildingsById[item.FamilyId].Name, StringComparer.Ordinal))
                Producers.Add(new(family.FamilyId, _baseline.Catalog.BuildingsById[family.FamilyId].Name));
            _selectedProducer = Producers.FirstOrDefault();
            Notify(nameof(SelectedProducer));
        }
        foreach (var row in Producers)
        {
            var family = state.ProductiveFamilies.Single(item => item.FamilyId == row.Id);
            row.Description = $"Base {family.Base.Buildings} ({family.Base.AssignedWorkers}/{family.Base.Buildings}) · " +
                $"Grande {family.Large.Buildings} ({family.Large.AssignedWorkers}/{family.Large.Buildings * 3}) · " +
                $"Avanzato {family.Advanced.Buildings} ({family.Advanced.AssignedWorkers}/{family.Advanced.Buildings * 10})";
        }
        RefreshResourceRows(state);
        RefreshBuildingEntries(state);
        RefreshParity();
        Notify(nameof(Clock)); Notify(nameof(Population)); Notify(nameof(AvailablePopulation));
        Notify(nameof(SchoolCount)); Notify(nameof(Students)); Notify(nameof(QualifiedResearchers));
        Notify(nameof(ActiveResearchers)); Notify(nameof(ResearchBalance)); Notify(nameof(ResearchRate));
        RefreshSchoolPanel();
        Notify(nameof(PauseLabel)); Notify(nameof(IsPaused)); Notify(nameof(Message));
        Notify(nameof(SelectedTechnologySummary)); Notify(nameof(CanBuySelectedTechnology));
        Notify(nameof(SelectedProducerSummary));
        Notify(nameof(SelectedBuildCost)); Notify(nameof(SelectedUpgradeCost)); Notify(nameof(SelectedCivicCost));
        Notify(nameof(CanSave)); Notify(nameof(CanAutoSave));
        foreach (var command in AllCommands()) command.Refresh();
    }

    private static bool Owns(string id, V14CampaignSnapshot snapshot) =>
        snapshot.OtherBuildings.Any(item => item.BuildingId == id && item.Count > 0) ||
        snapshot.ProductiveFamilies.Any(item => item.FamilyId == id &&
            (long)item.Base.Buildings + item.Large.Buildings + item.Advanced.Buildings > 0) ||
        id == V14CampaignSnapshots.SchoolId && snapshot.SchoolCount > 0;

    private IEnumerable<UiCommand> AllCommands() =>
    [
        TogglePauseCommand, BuildSchoolCommand, EnrollCommand, CancelStudentCommand, CraftPencilsCommand,
        AssignResearcherCommand, UnassignResearcherCommand, ReturnResearcherCommand,
        BuyTechnologyCommand, BuildProducerCommand, UpgradeProducerCommand,
        AssignWorkerCommand, ReleaseWorkerCommand, BuildCivicCommand, SaveCommand, LoadCommand,
        BuildSelectedCommand, BuildFiveCommand, BuildTenCommand,
        AssignSelectedWorkerCommand, AssignFiveSelectedWorkersCommand, AssignTenSelectedWorkersCommand,
        ReleaseSelectedWorkerCommand, UpgradeSelectedCommand, UpgradeSelectedToAdvancedCommand,
        EnlistCommand, CancelRecruitsCommand, DischargeCommand,
        NewGameCommand, ConfirmNewGameCommand, CancelNewGameCommand
    ];

    public UiCommand TogglePauseCommand { get; }
    public UiCommand BuildSchoolCommand { get; }
    public UiCommand CraftPencilsCommand { get; }
    public UiCommand EnrollCommand { get; }
    public UiCommand CancelStudentCommand { get; }
    public UiCommand AssignResearcherCommand { get; }
    public UiCommand UnassignResearcherCommand { get; }
    public UiCommand ReturnResearcherCommand { get; }
    public UiCommand BuyTechnologyCommand { get; }
    public UiCommand BuildProducerCommand { get; }
    public UiCommand UpgradeProducerCommand { get; }
    public UiCommand AssignWorkerCommand { get; }
    public UiCommand ReleaseWorkerCommand { get; }
    public UiCommand BuildCivicCommand { get; }
    public UiCommand SaveCommand { get; }
    public UiCommand LoadCommand { get; }
    public UiCommand EnlistCommand { get; }
    public UiCommand CancelRecruitsCommand { get; }
    public UiCommand DischargeCommand { get; }
    public UiCommand NewGameCommand { get; }
    public UiCommand ConfirmNewGameCommand { get; }
    public UiCommand CancelNewGameCommand { get; }
}

public sealed class V14TechnologyRow(string id, string name, string cost,
    IEnumerable<string> prerequisites, IEnumerable<string> requiredBuildings) : ObservableObject
{
    private string _status = string.Empty;
    public string Id { get; } = id;
    public string Name { get; } = name;
    public string Cost { get; } = cost;
    // Updated by the same prerequisites, ownership and exact milli-point checks as Status.
    public bool CanPurchase { get; internal set; }
    public IEnumerable<string> Prerequisites { get; } = prerequisites;
    public IEnumerable<string> RequiredBuildings { get; } = requiredBuildings;
    public string Status
    {
        get => _status;
        set { if (_status == value) return; _status = value; Notify(); Notify(nameof(Label)); }
    }
    public string Label => $"{Name} · {Cost} punti · {Status}";
}
public sealed class V14ProducerRow(string id, string name) : ObservableObject
{
    private string _description = string.Empty;
    public string Id { get; } = id;
    public string Name { get; } = name;
    public string Description
    {
        get => _description;
        set { if (_description == value) return; _description = value; Notify(); }
    }
}
public sealed record V14TierChoice(ProductionTier Tier, string Label)
{
    public override string ToString() => Label;
}
public sealed record V14CivicChoice(string Id, string Name)
{
    public override string ToString() => Name;
}
