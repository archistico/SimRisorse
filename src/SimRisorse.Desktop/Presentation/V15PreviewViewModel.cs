using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using SimRisorse.Application.Persistence;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Population;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using SimRisorse.Infrastructure.Persistence;

namespace SimRisorse.Desktop.Presentation;

/// <summary>
/// RC63: the default v15 presenter, with v14 UI parity. It never opens the v14 store, never drives a v14
/// session clock and never alters the historical seven-column resource UI.
/// Actions delegate to the validated v15 domain session, not to a second economy.
/// </summary>
public sealed partial class V15PreviewViewModel : ObservableObject
{
    private static readonly CultureInfo Italian = CultureInfo.GetCultureInfo("it-IT");
    private readonly GameScenario _baseline;
    private readonly V15SqliteFoodSaveStore _store;
    private readonly SchoolResearchRules _school;
    private readonly TechnologyCatalog _technologies;
    private readonly FoodConversionPolicy _depot;
    private readonly MeatChainPolicy _meat;
    private readonly HorticulturePolicy _crops;
    private readonly FoodIndustryConstructionCatalog _foodCatalog = FoodIndustryConstructionCatalog.Candidate;
    private V15FoodIndustryCampaignSession _session;
    private V15BuildingRow? _selectedBuilding;
    private bool _foodIndustryFilterActive;
    private V15ResourceRow? _selectedResource;
    private V15TechnologyRow? _selectedTechnology;
    private bool _initialized;
    private bool _saveAllowed = true;
    private bool _autoSaveAllowed = true;
    private bool _newGameConfirmation;
    private string _message = "Partita v15 separata: il salvataggio v14 non viene toccato.";
    private int _militaryQuantity = 1;
    private int _schoolQuantity = 1;

    public V15PreviewViewModel(GameScenario baseline, V15SqliteFoodSaveStore store,
        SchoolResearchRules school, TechnologyCatalog technologies,
        FoodConversionPolicy depot, MeatChainPolicy meat, HorticulturePolicy crops)
    {
        _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _school = school ?? throw new ArgumentNullException(nameof(school));
        _technologies = technologies ?? throw new ArgumentNullException(nameof(technologies));
        _depot = depot ?? throw new ArgumentNullException(nameof(depot));
        _meat = meat ?? throw new ArgumentNullException(nameof(meat));
        _crops = crops ?? throw new ArgumentNullException(nameof(crops));
        _session = FreshSession();
        Buildings = new();
        VisibleBuildings = new();
        Resources = new();
        Technologies = new();
        ToggleFoodIndustryFilterCommand = new(() =>
        {
            _foodIndustryFilterActive = !_foodIndustryFilterActive;
            RefreshVisibleBuildings();
            SelectedBuilding = VisibleBuildings.FirstOrDefault();
            Notify(nameof(FoodIndustryFilterActive));
            Notify(nameof(FoodIndustryFilterLabel));
        });
        TogglePauseCommand = new(() => { if (_session.IsPaused) _session.Resume(); else _session.Pause(); Refresh(); },
            () => !_newGameConfirmation);
        BuildCommand = new(() => Act(() => BuildSelected(1), "Edificio costruito."),
            () => CanAffordSelected);
        BuildFiveCommand = new(() => Act(() => BuildSelected(5), "Cinque edifici costruiti."),
            () => CanAffordFive);
        BuildTenCommand = new(() => Act(() => BuildSelected(10), "Dieci edifici costruiti."),
            () => CanAffordTen);
        UpgradeCommand = new(() => Act(() => UpgradeSelected(NextTier()), "Edificio aggiornato al livello successivo."),
            () => CanUpgradeSelected);
        UpgradeDirectAdvancedCommand = new(() => Act(() => UpgradeSelected(ProductionTier.Advanced), "Edificio aggiornato al livello avanzato."),
            () => CanUpgradeDirectlyToAdvanced);
        AssignWorkerCommand = new(() => Act(() => ChangeWorker(true, 1), "Lavoratore assegnato."),
            () => CanAssignWorker);
        AssignFiveWorkersCommand = new(() => Act(() => ChangeWorker(true, 5), "Cinque lavoratori assegnati."),
            () => CanAssignSelectedFive);
        AssignTenWorkersCommand = new(() => Act(() => ChangeWorker(true, 10), "Dieci lavoratori assegnati."),
            () => CanAssignSelectedTen);
        ReleaseWorkerCommand = new(() => Act(() => ChangeWorker(false, 1), "Lavoratore liberato."),
            () => CanReleaseWorker);
        BuildSchoolCommand = new(() => Act(() => _session.BuildSchool(1).Failure.ToString(), "Scuola costruita."),
            () => CanBuildSchool);
        EnrollCommand = new(() => Act(() => _session.EnrollStudents(SchoolQuantity).ToString(), "Studenti iscritti."),
            () => CanChange && Trial().EnrollStudents(SchoolQuantity) == SchoolActionFailure.None);
        CancelStudentCommand = new(() => Act(() => _session.CancelStudents(SchoolQuantity).ToString(), "Iscrizione annullata."),
            () => CanChange && Trial().CancelStudents(SchoolQuantity) == SchoolActionFailure.None);
        AssignResearcherCommand = new(() => Act(() => _session.AssignResearchers(SchoolQuantity).ToString(), "Ricercatore assegnato."),
            () => CanChange && Trial().AssignResearchers(SchoolQuantity) == SchoolActionFailure.None);
        UnassignResearcherCommand = new(() => Act(() => _session.UnassignResearchers(SchoolQuantity).ToString(), "Ricercatore sospeso."),
            () => CanChange && Trial().UnassignResearchers(SchoolQuantity) == SchoolActionFailure.None);
        ReturnResearcherCommand = new(() => Act(() => _session.ReturnResearchersToCivilian(SchoolQuantity).ToString(), "Ricercatore tornato civile."),
            () => CanChange && Trial().ReturnResearchersToCivilian(SchoolQuantity) == SchoolActionFailure.None);
        CraftPencilsCommand = new(() => Act(() => _session.CraftPencils(SchoolQuantity).ToString(), "Matite prodotte."),
            () => CanChange && Trial().CraftPencils(SchoolQuantity) == PencilCraftFailure.None);
        BuyTechnologyCommand = new(() => {
            var tech = SelectedTechnology;
            if (tech is not null) Act(() => _session.BuyTechnology(tech.Id).ToString(), "Tecnologia acquistata.");
        }, () => CanBuySelectedTechnology);
        EnlistCommand = new(() => Act(() => _session.Enlist(MilitaryQuantity).Error.ToString(), "Reclute arruolate."),
            () => CanChange && Trial().Enlist(MilitaryQuantity).Succeeded);
        CancelRecruitCommand = new(() => Act(() => _session.CancelRecruits(MilitaryQuantity).Error.ToString(), "Reclutamento annullato."),
            () => CanChange && Trial().CancelRecruits(MilitaryQuantity).Succeeded);
        // The classic GUI keeps discharge deferred until expeditions are supported.
        DischargeCommand = new(() => { }, () => false);
        NewGameCommand = new(() => { _session.Pause(); _newGameConfirmation = true; Refresh(); }, () => !_newGameConfirmation);
        ConfirmNewGameCommand = new(() => { _session = FreshSession(); _newGameConfirmation = false;
            _autoSaveAllowed = false; _saveAllowed = true; _message = "Nuova colonia v15: salva manualmente per sostituire il checkpoint precedente."; Refresh(); },
            () => _newGameConfirmation);
        CancelNewGameCommand = new(() => { _newGameConfirmation = false; Refresh(); }, () => _newGameConfirmation);
        SaveCommand = new(() => Save(), () => CanSave);
        LoadCommand = new(() => Load(), () => _initialized && !_newGameConfirmation && File.Exists(_store.DatabasePath));
        Refresh();
    }

    public static V15PreviewViewModel CreateForPreview(string savePath)
    {
        var scenario = CivilGrowthScenario.Create();
        var school = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m),
            120, 1, 2, 100, pencilsPerStudent: 2);
        var technologies = TechnologyCatalog.ProposedTierTree;
        // Development candidate rates, shared by UI and the single unified clock.
        var depot = UnifiedFoodInputs.CreatePolicy([
            new("fish", U(1m), U(1m), 0), new("meat", U(1m), U(1m), 1),
            new("vegetables", U(1m), U(1m), 2), new("fruit", U(1m), U(1m), 3),
            new("bread", U(1m), U(1m), 4)
        ]);
        var meat = new MeatChainPolicy("grain", "water", "cattle", "game", "meat",
            ResourceBundle.Create(("grain", 1m), ("water", 1m)), U(1m), U(1m),
            [new("cattle", U(1m), U(2m), 0), new("game", U(1m), U(1m), 1)]);
        var crops = new HorticulturePolicy("water", "vegetables", "fruit",
            new("vegetables", U(2m), U(1m), 0), new("fruit", U(3m), U(1m), 1));
        var store = new V15SqliteFoodSaveStore(savePath, scenario, school, technologies, depot);
        return new(scenario, store, school, technologies, depot, meat, crops);
    }

    private static ResourceAmount U(decimal units) => ResourceAmount.FromUnits(units);

    private V15FoodIndustryCampaignSession FreshSession()
    {
        var city = V14CampaignSnapshots.MigrateFromV13(_baseline,
            GameSession.Create(_baseline).Snapshot(), _school, _technologies);
        var checkpoint = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(city, _depot), _depot);
        return new(checkpoint, _baseline, _school, _technologies, _depot, _foodCatalog);
    }

    public string DatabasePath => _store.DatabasePath;
    public bool IsPaused => _session.IsPaused;
    public bool CanSave => _initialized && _saveAllowed && !_newGameConfirmation;
    public bool CanAutoSave => CanSave && _autoSaveAllowed;
    private bool CanChange => !_newGameConfirmation;
    public string Clock => TimeSpan.FromTicks(_session.Snapshot().CityCampaign.ElapsedTicks)
        .ToString(@"d\.hh\:mm\:ss", Italian);
    public string PauseLabel => IsPaused ? "▶ Riprendi" : "Ⅱ Pausa";
    public int Population => _session.Snapshot().CityCampaign.Population;
    public long AvailableWorkers => _session.AvailableWorkers;
    public string PopulationStatus => $"{Population} abitanti · {AvailableWorkers} liberi";
    public string ResearchBalance => (_session.Snapshot().CityCampaign.ResearchBalanceMilliPoints / 1000m)
        .ToString("N2", Italian) + " punti";
    public string ResearchRate => (_session.Snapshot().CityCampaign.SchoolResearch.ActiveResearchers *
        _school.MilliPointsPerResearcherPerSecond / 1000m).ToString("N2", Italian) + " punti/s";
    public string SchoolStatus
    {
        get
        {
            var city = _session.Snapshot().CityCampaign;
            var state = city.SchoolResearch;
            return $"Scuole: {city.SchoolCount} · In attesa: {state.WaitingStudents} · " +
                $"In formazione: {state.TrainingStudents} · Qualificati: {state.QualifiedResearchers} · " +
                $"Attivi: {state.ActiveResearchers}";
        }
    }
    public string MilitaryStatus
    {
        get
        {
            var state = _session.Snapshot().CityCampaign.Military;
            return $"Reclute in attesa: {state.WaitingRecruits} · In addestramento: {state.TrainingRecruits} · Soldati: {state.AvailableSoldiers}";
        }
    }
    public string Message => _message;
    public bool IsNewGameConfirmationVisible => _newGameConfirmation;
    public ObservableCollection<V15BuildingRow> Buildings { get; }
    // Keep the full catalog in Buildings; filter only the list shown by the v15 UI.
    public ObservableCollection<V15BuildingRow> VisibleBuildings { get; }
    public bool FoodIndustryFilterActive => _foodIndustryFilterActive;
    public string FoodIndustryFilterLabel => _foodIndustryFilterActive
        ? "Tutti gli edifici" : "Filiere alimentari (6)";
    public ObservableCollection<V15ResourceRow> Resources { get; }
    public ObservableCollection<V15TechnologyRow> Technologies { get; }
    public IEnumerable<V15ResourceRow> ResourceHighlights =>
        new[] { "food", "planks", "stone" }.Select(id => Resources.Single(row => row.Id == id));

    public V15BuildingRow? SelectedBuilding
    {
        get => _selectedBuilding;
        set { if (ReferenceEquals(_selectedBuilding, value)) return; _selectedBuilding = value;
            Notify(); RefreshSelections(); }
    }
    public V15ResourceRow? SelectedResource
    {
        get => _selectedResource;
        set { if (ReferenceEquals(_selectedResource, value)) return; _selectedResource = value; Notify();
            Notify(nameof(SelectedResourceStorageDetails)); }
    }
    public V15TechnologyRow? SelectedTechnology
    {
        get => _selectedTechnology;
        set { if (ReferenceEquals(_selectedTechnology, value)) return; _selectedTechnology = value;
            Notify(); RefreshSelections(); }
    }
    public string SelectedResourceStorageDetails => SelectedResource?.CapacityBreakdown ??
        "Capienza aggregata di tutti i depositi fisici; nessuna scorta viene conteggiata due volte.";
    public string SelectedBuildingSummary => SelectedBuilding is null ? "Seleziona un edificio." :
        $"{SelectedBuilding.Name}\n{SelectedBuilding.Count} costruiti · " +
        $"{SelectedBuilding.Workers} addetti\n{SelectedBuilding.Cost}\n{SelectedBuilding.Status}";
    public string SelectedBuildingCapacity
    {
        get
        {
            var selected = SelectedBuilding;
            if (selected is null) return "—";
            ResourceBundle storage;
            if (selected.Kind == V15BuildingKind.FoodIndustry)
            {
                var site = _foodCatalog.Sites.Single(s => s.Id == selected.SourceId);
                var cap = site.StoragePerBuilding;
                storage = cap.Animals + cap.Transit + cap.DepotRaw + ResourceBundle.FromAmounts([
                    KeyValuePair.Create("food", cap.DepotReady)]);
            }
            else if (_baseline.Catalog.BuildingsById.TryGetValue(selected.SourceId, out var definition))
                storage = definition.StoragePerBuilding ?? ResourceBundle.Empty;
            else storage = ResourceBundle.Empty;
            return storage.Amounts.IsEmpty ? "Nessuna capacità diretta" : FormatCost(storage);
        }
    }
    public string SelectedBuildingProduction => SelectedBuilding?.SourceId switch
    {
        "stable" => "Bovini · usa Grano e Acqua",
        "hunter" => "Selvaggina · consegna al Macellaio",
        "butcher" => "Carne · usa Bovini o Selvaggina",
        "garden" => "Verdura · usa Acqua",
        "orchard" => "Frutta · usa Acqua",
        "food-depot" => "Cibo pronto · usa Pesce, Carne, Verdura, Frutta e Pane",
        _ => "Produzione e consumo secondo la ricetta della famiglia, se presente."
    };
    public string SelectedTechnologySummary => SelectedTechnology is null ? "Seleziona una ricerca." :
        $"{SelectedTechnology.Name}\n{SelectedTechnology.Status}\nCosto: {SelectedTechnology.Cost}";
    public bool CanBuildSelected => CanChange && SelectedBuilding is { Unlocked: true, IsFixed: false };
    public bool CanAffordSelected => CanBuildSelected && SelectedBuilding?.Affordable == true;
    public bool CanAffordFive => CanBuildSelected && Quote(SelectedBuilding!, 5).Affordable;
    public bool CanAffordTen => CanBuildSelected && Quote(SelectedBuilding!, 10).Affordable;
    public string BuildFiveCost => SelectedBuilding is null ? string.Empty : "5 edifici: " + FormatCost(Quote(SelectedBuilding, 5).Cost);
    public string BuildTenCost => SelectedBuilding is null ? string.Empty : "10 edifici: " + FormatCost(Quote(SelectedBuilding, 10).Cost);
    public bool CanAssignSelected => SelectedBuilding is { Kind: V15BuildingKind.Producer or V15BuildingKind.FoodIndustry, Count: > 0 };
    public bool CanAssignSelectedOne => CanAssignSelectedQuantity(1);
    public bool CanAssignSelectedFive => CanAssignSelectedQuantity(5);
    public bool CanAssignSelectedTen => CanAssignSelectedQuantity(10);
    private bool CanAssignSelectedQuantity(int quantity) => CanChange && CanAssignSelected &&
        SelectedBuilding!.Workers <= SelectedBuilding.Workplaces - quantity && AvailableWorkers >= quantity;
    public bool ShowUpgradeNext => ShowUpgradeSelected;
    public bool CanUpgradeNext => CanUpgradeSelected;
    public bool ShowUpgradeDirectlyToAdvanced => SelectedBuilding is
        { Kind: V15BuildingKind.Producer, Count: > 0, Tier: ProductionTier.Base } row &&
        UpgradeQuote(row, ProductionTier.Advanced).Failure is V14TierActionFailure.None or V14TierActionFailure.InsufficientResources;
    public bool CanUpgradeDirectlyToAdvanced => CanChange && ShowUpgradeDirectlyToAdvanced &&
        UpgradeQuote(SelectedBuilding!, ProductionTier.Advanced).Succeeded;
    public string NextUpgradeLabel => SelectedBuilding?.Tier == ProductionTier.Base ? "Potenzia 1 → Grande" : "Potenzia 1 → Avanzato";
    public string DirectUpgradeLabel => "Potenzia 1 → Avanzato";
    public string NextUpgradeCost => SelectedBuilding is not
        { Kind: V15BuildingKind.Producer, Count: > 0, Tier: ProductionTier.Base or ProductionTier.Large }
        ? string.Empty : FormatCost(UpgradeQuote(SelectedBuilding, NextTier()).Cost);
    public string DirectUpgradeCost => SelectedBuilding is not
        { Kind: V15BuildingKind.Producer, Count: > 0, Tier: ProductionTier.Base }
        ? string.Empty : FormatCost(UpgradeQuote(SelectedBuilding, ProductionTier.Advanced).Cost);
    public string NextUpgradeRequirement => string.Empty;
    public bool ShowNextUpgradeRequirement => false;
    private ProductionTier NextTier() => SelectedBuilding?.Tier == ProductionTier.Base ? ProductionTier.Large : ProductionTier.Advanced;
    public bool ShowUpgradeSelected => SelectedBuilding is
        { Kind: V15BuildingKind.Producer, Count: > 0, Tier: ProductionTier.Base or ProductionTier.Large } row &&
        UpgradeQuote(row).Failure is V14TierActionFailure.None or V14TierActionFailure.InsufficientResources;
    public bool CanUpgradeSelected => CanChange && SelectedBuilding is
        { Kind: V15BuildingKind.Producer, Count: > 0, Tier: ProductionTier.Base or ProductionTier.Large } row &&
        UpgradeQuote(row).Succeeded;
    public bool CanBuildSchool => CanChange && Buildings.FirstOrDefault(b => b.Kind == V15BuildingKind.School)?.Affordable == true;
    public bool CanAssignWorker => CanChange && SelectedBuilding is { Kind: V15BuildingKind.Producer or V15BuildingKind.FoodIndustry } b &&
        b.Workers < b.Workplaces && AvailableWorkers > 0;
    public bool CanReleaseWorker => CanChange && SelectedBuilding is { Kind: V15BuildingKind.Producer or V15BuildingKind.FoodIndustry, Workers: > 0 };
    public bool CanBuySelectedTechnology => CanChange && SelectedTechnology?.CanBuy == true;

    public UiCommand ToggleFoodIndustryFilterCommand { get; }
    public UiCommand TogglePauseCommand { get; }
    public UiCommand BuildCommand { get; }
    public UiCommand BuildFiveCommand { get; }
    public UiCommand BuildTenCommand { get; }
    public UiCommand UpgradeCommand { get; }
    public UiCommand UpgradeDirectAdvancedCommand { get; }
    public UiCommand AssignWorkerCommand { get; }
    public UiCommand AssignFiveWorkersCommand { get; }
    public UiCommand AssignTenWorkersCommand { get; }
    public UiCommand ReleaseWorkerCommand { get; }
    public UiCommand BuildSchoolCommand { get; }
    public UiCommand EnrollCommand { get; }
    public UiCommand CancelStudentCommand { get; }
    public UiCommand AssignResearcherCommand { get; }
    public UiCommand UnassignResearcherCommand { get; }
    public UiCommand ReturnResearcherCommand { get; }
    public UiCommand CraftPencilsCommand { get; }
    public UiCommand BuyTechnologyCommand { get; }
    public UiCommand EnlistCommand { get; }
    public UiCommand CancelRecruitCommand { get; }
    public UiCommand DischargeCommand { get; }
    public UiCommand NewGameCommand { get; }
    public UiCommand ConfirmNewGameCommand { get; }
    public UiCommand CancelNewGameCommand { get; }
    public UiCommand SaveCommand { get; }
    public UiCommand LoadCommand { get; }

    private V15FoodIndustryCampaignSession Trial() => new(_session.Snapshot(), _baseline,
        _school, _technologies, _depot, _foodCatalog);

    private sealed record BuildQuote(bool Unlocked, bool Affordable, ResourceBundle Cost, string Status);

    private BuildQuote Quote(V15BuildingRow row, int quantity)
    {
        // Quote against a disposable session so pricing, technology locks and
        // prerequisites are exactly those enforced by the actual domain commands.
        try
        {
            if (row.Kind == V15BuildingKind.FoodIndustry)
            {
                var result = _session.CheckBuild(row.SourceId, quantity);
                var cost = result.Cost;
                if (cost.Equals(ResourceBundle.Empty))
                    cost = _foodCatalog.Quote(_session.Snapshot().Industry.Buildings, row.SourceId, quantity);
                return new(result.Failure is FoodIndustryActionFailure.None or FoodIndustryActionFailure.InsufficientResources,
                    result.Succeeded, cost, result.Failure.ToString());
            }
            if (row.Kind == V15BuildingKind.Producer && row.Tier is { } tier)
            {
                var result = Trial().BuildCityProducer(row.SourceId, tier, quantity);
                return new(result.Failure is V14TierActionFailure.None or V14TierActionFailure.InsufficientResources,
                    result.Succeeded, result.Cost.Amounts.IsEmpty ? FallbackQuote(row, quantity) : result.Cost,
                    result.Failure.ToString());
            }
            if (row.Kind == V15BuildingKind.Civic)
            {
                var result = Trial().BuildCityBuilding(row.SourceId, quantity);
                return new(result.Failure is V14CivicConstructionFailure.None or V14CivicConstructionFailure.InsufficientResources,
                    result.Succeeded, result.Cost.Amounts.IsEmpty ? FallbackQuote(row, quantity) : result.Cost,
                    result.Failure.ToString());
            }
            if (row.Kind == V15BuildingKind.School)
            {
                var result = Trial().BuildSchool(quantity);
                return new(result.Failure is V14SchoolConstructionFailure.None or V14SchoolConstructionFailure.InsufficientResources,
                    result.Succeeded, result.Cost.Amounts.IsEmpty ? FallbackQuote(row, quantity) : result.Cost,
                    result.Failure.ToString());
            }
        }
        catch (Exception ex) when (ex is OverflowException or ArgumentOutOfRangeException or InvalidOperationException)
        { return new(false, false, ResourceBundle.Empty, "NumericLimit"); }
        return new(false, false, ResourceBundle.Empty, "Non disponibile");
    }

    private string BuildSelected(int quantity)
    {
        var row = SelectedBuilding;
        if (row is null) return "Nessun edificio selezionato";
        return row.Kind switch
        {
            V15BuildingKind.FoodIndustry => _session.Build(row.SourceId, quantity).Failure.ToString(),
            V15BuildingKind.Producer when row.Tier is { } tier =>
                _session.BuildCityProducer(row.SourceId, tier, quantity).Failure.ToString(),
            V15BuildingKind.Civic => _session.BuildCityBuilding(row.SourceId, quantity).Failure.ToString(),
            V15BuildingKind.School => _session.BuildSchool(quantity).Failure.ToString(),
            _ => "Non disponibile"
        };
    }

    private V14TierActionResult UpgradeQuote(V15BuildingRow row, ProductionTier? destination = null) =>
        Trial().UpgradeCityProducer(row.SourceId, row.Tier!.Value,
            destination ?? (row.Tier == ProductionTier.Base ? ProductionTier.Large : ProductionTier.Advanced));

    private string UpgradeSelected(ProductionTier destination)
    {
        var row = SelectedBuilding;
        if (row is not { Kind: V15BuildingKind.Producer, Tier: ProductionTier.Base or ProductionTier.Large })
            return "Non disponibile";
        return _session.UpgradeCityProducer(row.SourceId, row.Tier!.Value, destination).Failure.ToString();
    }

    private string ChangeWorker(bool assign, int quantity)
    {
        var row = SelectedBuilding;
        if (row is null) return "Nessun edificio selezionato";
        return row.Kind switch
        {
            V15BuildingKind.FoodIndustry => (assign ? _session.AssignWorkers(row.SourceId, quantity) :
                _session.ReleaseWorkers(row.SourceId, quantity)).ToString(),
            V15BuildingKind.Producer when row.Tier is { } tier => (assign ?
                _session.AssignCityProducerWorkers(row.SourceId, tier, quantity) :
                _session.ReleaseCityProducerWorkers(row.SourceId, tier, quantity)).ToString(),
            _ => "Non disponibile"
        };
    }

    private void Act(Func<string> action, string success)
    {
        if (!CanChange) return;
        try
        {
            var outcome = action();
            _message = outcome == "None" ? success : "Operazione non eseguita: " + outcome;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or OverflowException)
        { _message = "Operazione rifiutata: " + ex.Message; }
        Refresh();
    }

    public void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        if (File.Exists(_store.DatabasePath)) Load();
        else { _message = "Nuova partita v15 in pausa. Salva per creare il database separato."; Refresh(); }
    }

    public bool Save()
    {
        if (!CanSave) return false;
        try
        {
            _store.Save(_session.Snapshot());
            _autoSaveAllowed = true;
            _message = "Partita v15 salvata: " + _store.DatabasePath;
            Refresh();
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or GameSaveException)
        { _message = "Salvataggio v15 non riuscito: " + ex.Message; Refresh(); return false; }
    }

    public bool Load()
    {
        if (!_initialized || _newGameConfirmation) return false;
        try
        {
            var restored = _store.Load();
            _session = new(restored, _baseline, _school, _technologies, _depot, _foodCatalog);
            _saveAllowed = true;
            _autoSaveAllowed = true;
            _message = "Partita v15 caricata in pausa.";
            Refresh();
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or GameSaveException)
        {
            // An unknown database must never be replaced by a fresh campaign.
            _saveAllowed = false;
            _message = "Caricamento bloccato: " + ex.Message;
            Refresh();
            return false;
        }
    }

    public void Advance(TimeSpan elapsed)
    {
        if (_session.IsPaused || _newGameConfirmation) return;
        var previousSecond = _session.Snapshot().CityCampaign.ElapsedTicks / TimeSpan.TicksPerSecond;
        try
        {
            var result = _session.Advance(elapsed, _meat, _crops);
            if (!result.Succeeded) { _session.Pause(); _message = "Clock fermato: " + result.Error; }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or OverflowException)
        { _session.Pause(); _message = "Clock fermato: " + ex.Message; }
        // View projections are relatively expensive; no visible quantity changes
        // between two whole seconds. Pause/actions always refresh separately.
        if (_session.IsPaused || _session.Snapshot().CityCampaign.ElapsedTicks / TimeSpan.TicksPerSecond != previousSecond)
            Refresh();
    }

    public void Interrupt() { _session.Pause(); Refresh(); }

    private void RefreshSelections()
    {
        Notify(nameof(SelectedBuildingSummary));
        Notify(nameof(BuildFiveCost)); Notify(nameof(BuildTenCost));
        Notify(nameof(CanAffordFive)); Notify(nameof(CanAffordTen));
        Notify(nameof(CanAssignSelected)); Notify(nameof(CanAssignSelectedOne));
        Notify(nameof(CanAssignSelectedFive)); Notify(nameof(CanAssignSelectedTen));
        Notify(nameof(ShowUpgradeNext)); Notify(nameof(CanUpgradeNext));
        Notify(nameof(ShowUpgradeDirectlyToAdvanced)); Notify(nameof(CanUpgradeDirectlyToAdvanced));
        Notify(nameof(NextUpgradeCost)); Notify(nameof(DirectUpgradeCost));
        Notify(nameof(NextUpgradeLabel)); Notify(nameof(DirectUpgradeLabel));
        Notify(nameof(SelectedBuildingCapacity));
        Notify(nameof(SelectedBuildingProduction));
        Notify(nameof(SelectedTechnologySummary));
        Notify(nameof(CanBuildSelected));
        Notify(nameof(CanUpgradeSelected));
        Notify(nameof(ShowUpgradeSelected));
        Notify(nameof(CanBuildSchool));
        Notify(nameof(CanAffordSelected));
        Notify(nameof(CanAssignWorker));
        Notify(nameof(CanReleaseWorker));
        Notify(nameof(CanBuySelectedTechnology));
        BuildCommand.Refresh(); BuildFiveCommand.Refresh(); BuildTenCommand.Refresh();
        UpgradeCommand.Refresh(); UpgradeDirectAdvancedCommand.Refresh();
        AssignWorkerCommand.Refresh(); AssignFiveWorkersCommand.Refresh(); AssignTenWorkersCommand.Refresh();
        ReleaseWorkerCommand.Refresh(); BuyTechnologyCommand.Refresh();
    }

    private string FormatCost(ResourceBundle cost) => cost.Amounts.IsEmpty ? "Costo non disponibile" :
        string.Join(", ", cost.Amounts.Select(kv => $"{kv.Value.Units.ToString("0.###", Italian)} {ResourceName(kv.Key)}"));

    private static string ItalianReason(string failure) => failure switch
    {
        "None" => "Disponibile",
        "InsufficientResources" => "Risorse insufficienti",
        "MissingPrerequisite" => "Richiede altri edifici",
        "MissingResearch" => "Richiede una tecnologia",
        "FixedBuilding" => "Edificio iniziale fisso",
        "NotImplemented" => "Ricerca non ancora implementata",
        "AlreadyUnlocked" => "Ricerca acquistata",
        "InsufficientResearchPoints" => "Punti ricerca insufficienti",
        "MissingBuilding" => "Richiede un edificio",
        _ => failure
    };

    private void RefreshVisibleBuildings()
    {
        var selected = Buildings.Where(row => !_foodIndustryFilterActive ||
            row.Kind == V15BuildingKind.FoodIndustry).ToArray();
        if (VisibleBuildings.Count == selected.Length &&
            VisibleBuildings.SequenceEqual(selected)) return;
        VisibleBuildings.Clear();
        foreach (var row in selected) VisibleBuildings.Add(row);
    }

    public void Refresh()
    {
        var checkpoint = _session.Snapshot();
        var projection = _session.CityCatalog;
        // The projection does not mutate the paused/running session. Gross flows
        // are measured by the real v15 clock for the NEXT complete second.
        var economy = V15EconomyProjector.Project(checkpoint, _baseline, _school,
            _technologies, _foodCatalog, _meat, _crops, _depot);
        var buildingsById = Buildings.ToDictionary(r => r.Id, StringComparer.Ordinal);
        foreach (var entry in projection.Buildings)
        {
            if (!buildingsById.TryGetValue(entry.Id, out var row))
            {
                row = new V15BuildingRow(entry);
                Buildings.Add(row);
            }
            row.Update(entry);
            var quote = Quote(row, 1);
            row.UpdateQuote(quote.Unlocked, quote.Affordable, FormatCost(quote.Cost), ItalianReason(quote.Status),
                CostLines(quote.Cost), BuildDiagnosis(row, quote));
            row.UpdateDetails(BuildDescription(row), BuildingOutputLines(row), BuildingInputLines(row), BuildingCapacityLines(row),
                BuildingRequirements(row, quote), BuildingIsFixed(row));
        }
        // Catalog identity is stable; the matching row instances survive refreshes.
        RefreshVisibleBuildings();
        if (_selectedBuilding is null && VisibleBuildings.Count > 0)
            SelectedBuilding = VisibleBuildings[0];
        var resourcesById = Resources.ToDictionary(r => r.Id, StringComparer.Ordinal);
        foreach (var resource in projection.Resources)
        {
            if (!resourcesById.TryGetValue(resource.Id, out var row))
            {
                row = new V15ResourceRow(resource);
                Resources.Add(row);
            }
            row.Update(resource, economy.Production[resource.Id].Units,
                economy.Consumption[resource.Id].Units);
        }
        if (_selectedResource is null && Resources.Count > 0) SelectedResource = Resources[0];
        var technologiesById = Technologies.ToDictionary(t => t.Id, StringComparer.Ordinal);
        foreach (var definition in _technologies.Technologies)
        {
            if (!technologiesById.TryGetValue(definition.Id, out var row))
            {
                row = new V15TechnologyRow(definition);
                Technologies.Add(row);
            }
            var failure = Trial().BuyTechnology(definition.Id);
            row.Update(failure == TechnologyPurchaseFailure.None, ItalianReason(failure.ToString()));
        }
        if (_selectedTechnology is null && Technologies.Count > 0) SelectedTechnology = Technologies[0];
        Notify(nameof(Clock)); Notify(nameof(IsPaused)); Notify(nameof(PauseLabel));
        Notify(nameof(Population)); Notify(nameof(AvailableWorkers)); Notify(nameof(PopulationStatus));
        Notify(nameof(PopulationCount)); Notify(nameof(AvailableWorkersText));
        Notify(nameof(HousingFillPercent)); Notify(nameof(GrowthStatus)); Notify(nameof(GrowthPercent));
        Notify(nameof(FoodRequirement)); Notify(nameof(BreadRequirement)); Notify(nameof(BreadSupplyStatus));
        Notify(nameof(ResearchBalance)); Notify(nameof(ResearchRate));
        Notify(nameof(SchoolStatus)); Notify(nameof(MilitaryStatus));
        RefreshParityPanels();
        Notify(nameof(Message)); Notify(nameof(IsNewGameConfirmationVisible));
        Notify(nameof(ResourceHighlights)); Notify(nameof(SelectedResourceStorageDetails));
        RefreshSelections();
        TogglePauseCommand.Refresh(); BuildSchoolCommand.Refresh(); EnrollCommand.Refresh();
        CancelStudentCommand.Refresh(); AssignResearcherCommand.Refresh(); UnassignResearcherCommand.Refresh();
        CraftPencilsCommand.Refresh(); ReturnResearcherCommand.Refresh(); EnlistCommand.Refresh(); CancelRecruitCommand.Refresh();
        DischargeCommand.Refresh(); UnassignResearcherCommand.Refresh();
        NewGameCommand.Refresh(); ConfirmNewGameCommand.Refresh(); CancelNewGameCommand.Refresh();
        SaveCommand.Refresh(); LoadCommand.Refresh();
    }
}

/// <summary>Reusable display row: tier is part of the identity, not a combo box.</summary>
public sealed class V15BuildingRow(V15BuildingEntry initial) : ObservableObject
{
    private V15BuildingEntry _entry = initial;
    private bool _unlocked;
    private bool _affordable;
    private string _cost = string.Empty;
    private string _status = string.Empty;
    private string _description = string.Empty;
    private string _requirements = string.Empty;
    private string _diagnosis = string.Empty;
    private bool _fixed;
    private IReadOnlyList<ResourceDetailLine> _costLines = [];
    private IReadOnlyList<ResourceDetailLine> _inputLines = [];
    private IReadOnlyList<ResourceDetailLine> _outputLines = [];
    private IReadOnlyList<ResourceDetailLine> _capacityLines = [];
    public string Id => _entry.Id;
    public string SourceId => _entry.SourceId;
    public string GlyphKey => _entry.SourceId;
    public string Name => _entry.Name;
    public V15BuildingKind Kind => _entry.Kind;
    public ProductionTier? Tier => _entry.Tier;
    public long Count => _entry.Owned;
    public long Workers => _entry.AssignedWorkers;
    public long Workplaces => _entry.Kind == V15BuildingKind.FoodIndustry ? Count :
        Count * (_entry.Tier switch { ProductionTier.Large => 3, ProductionTier.Advanced => 10, _ => 1 });
    public string Workforce => $"{Workers} / {Workplaces} addetti";
    public string Staff => Kind is V15BuildingKind.Producer or V15BuildingKind.FoodIndustry ? $"{Workers} / {Workplaces}" : "—";
    public bool IsFixed => _fixed;
    public double RowOpacity => Unlocked || IsFixed ? 1d : 0.48d;
    public bool HasCost => !IsFixed;
    public string Price => _cost;
    public string Description => _description;
    public string Requirements => _requirements;
    public bool HasRequirements => _requirements.Length > 0;
    public string Diagnosis => _diagnosis;
    public bool HasDiagnosis => _diagnosis.Length > 0;
    public bool IsProducer => Kind is V15BuildingKind.Producer or V15BuildingKind.FoodIndustry;
    public IReadOnlyList<ResourceDetailLine> CostLines => _costLines;
    public IReadOnlyList<ResourceDetailLine> OutputLines => _outputLines;
    public IReadOnlyList<ResourceDetailLine> InputLines => _inputLines;
    public IReadOnlyList<ResourceDetailLine> CapacityLines => _capacityLines;
    public bool HasInputs => _inputLines.Count > 0;
    public bool HasCapacity => _capacityLines.Count > 0;
    public string Summary => $"{Count} costruiti · {Workforce}";
    public bool Unlocked => _unlocked;
    public bool Affordable => _affordable;
    public string Cost => _cost;
    public string Status => _status;
    public void Update(V15BuildingEntry entry) { _entry = entry; Notify(string.Empty); }
    public void UpdateQuote(bool unlocked, bool affordable, string cost, string status,
        IReadOnlyList<ResourceDetailLine> costLines, string diagnosis)
    {
        _unlocked = unlocked; _affordable = affordable; _cost = cost; _status = status;
        _costLines = costLines; _diagnosis = diagnosis;
        Notify(string.Empty);
    }
    public void UpdateDetails(string description, IReadOnlyList<ResourceDetailLine> output,
        IReadOnlyList<ResourceDetailLine> input, IReadOnlyList<ResourceDetailLine> capacity,
        string requirements, bool isFixed)
    {
        _description = description; _outputLines = output; _inputLines = input;
        _capacityLines = capacity; _requirements = requirements; _fixed = isFixed;
        Notify(string.Empty);
    }
}

/// <summary>Seven original columns, based on actual next-second v15 flows.</summary>
public sealed class V15ResourceRow(V15ResourceEntry initial) : ObservableObject
{
    private V15ResourceEntry _entry = initial;
    private decimal _productionPerSecond;
    private decimal _consumptionPerSecond;
    private static readonly CultureInfo ItalianRows = CultureInfo.GetCultureInfo("it-IT");
    private static readonly IImmutableBrush SurplusBrush = new ImmutableSolidColorBrush(Color.Parse("#176858"));
    private static readonly IImmutableBrush DeficitBrush = new ImmutableSolidColorBrush(Color.Parse("#A44732"));
    public string Id => _entry.Id;
    public string Name => _entry.Name;
    public string Quantity => decimal.Round(_entry.Quantity.Units, 0,
        MidpointRounding.AwayFromZero).ToString("#,0", ItalianRows);
    public string QuantityRounded => Quantity;
    public string Capacity => _entry.Capacity.Units.ToString("#,0.###", ItalianRows);
    public decimal ProductionPerSecond => _productionPerSecond;
    public decimal ConsumptionPerSecond => _consumptionPerSecond;
    public decimal NetPerSecond => _productionPerSecond - _consumptionPerSecond;
    public string Production => _productionPerSecond.ToString("0.###", ItalianRows) + " /s";
    public string Consumption => _consumptionPerSecond.ToString("0.###", ItalianRows) + " /s";
    public string NetRate => NetPerSecond.ToString("+0.###;-0.###;±0", ItalianRows) + " /s";
    public IBrush NetForeground => NetPerSecond < 0m ? DeficitBrush : SurplusBrush;
    public string TimeToFull
    {
        get
        {
            if (_entry.Capacity == ResourceAmount.Zero) return "—";
            if (_entry.Quantity.Milliunits >= _entry.Capacity.Milliunits) return "00:00:00";
            if (NetPerSecond <= 0m) return "—";
            // Local estimate under a CONSTANT next-second net flow, not a forecast
            // of future input replenishment or global throughput.
            var seconds = decimal.Ceiling((_entry.Capacity.Units - _entry.Quantity.Units) / NetPerSecond);
            var hours = decimal.Floor(seconds / 3600m);
            var minutes = decimal.Floor((seconds % 3600m) / 60m);
            return $"{hours:00}:{minutes:00}:{seconds % 60m:00}";
        }
    }
    public string Stock => _entry.Quantity.Units.ToString("#,0.###", ItalianRows) + " / " + Capacity;
    public double FillPercent => _entry.Capacity.Units <= 0m ? 0d :
        Math.Clamp((double)(_entry.Quantity.Units / _entry.Capacity.Units * 100m), 0d, 100d);
    public string CapacityBreakdown => $"{Name}: {Stock}. Capienza: " +
        (_entry.CapacitySources.IsDefaultOrEmpty ? "nessun compartimento disponibile" :
            string.Join(" + ", _entry.CapacitySources.Select(segment =>
                $"{segment.Source} {segment.Amount.Units.ToString("#,0.###", ItalianRows)}")));
    public void Update(V15ResourceEntry entry, decimal productionPerSecond, decimal consumptionPerSecond)
    {
        _entry = entry;
        _productionPerSecond = productionPerSecond;
        _consumptionPerSecond = consumptionPerSecond;
        Notify(string.Empty);
    }
}

public sealed class V15TechnologyRow(TechnologyDefinition technology) : ObservableObject
{
    private bool _canBuy;
    private string _status = string.Empty;
    public string Id => technology.Id;
    public string Name => technology.Name;
    public string Cost => technology.Cost.Units.ToString("N2", CultureInfo.GetCultureInfo("it-IT")) + " punti";
    public bool CanBuy => _canBuy;
    public string Status => _status;
    public string Label => Cost + " · " + Status;
    public void Update(bool canBuy, string status) { _canBuy = canBuy; _status = status; Notify(string.Empty); }
}
