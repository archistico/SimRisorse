using System.Globalization;
using SimRisorse.Application.ReadModels;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Desktop.Presentation;

/// <summary>
/// V14 presentation parity with the classic resource, population and military panels.
/// Display projections are read-only; only explicit commands mutate the session.
/// </summary>
public sealed partial class V14PreviewViewModel
{
    private static readonly StringComparer ResourceNameComparer = StringComparer.Create(Italian, ignoreCase: true);
    private V14EconomyPreview? _economyPreview;
    private BreadSupplyView _breadSupply = new(BreadSupplyState.Hidden, null);
    private ResourceRow? _selectedResource;
    private int _militaryQuantity = 1;

    public ResourceRow? SelectedResource
    {
        get => _selectedResource;
        set
        {
            if (ReferenceEquals(_selectedResource, value)) return;
            _selectedResource = value;
            Notify();
            Notify(nameof(SelectedResourceStorageDetails));
        }
    }
    public string SelectedResourceStorageDetails => SelectedResource?.CapacityBreakdown ??
        "Passa il puntatore sulla capacità di una risorsa per vedere il calcolo dello spazio.";

    private void RefreshResourceRows(V14CampaignSnapshot snapshot)
    {
        var preview = _session.ForecastEconomy();
        _economyPreview = preview;
        var capacity = CalculateStorageCapacity(snapshot);
        var rates = preview.Resources.ToDictionary(item => item.Id, StringComparer.Ordinal);
        if (Resources.Count == 0)
        {
            foreach (var resource in _baseline.Catalog.Resources
                         .Append(new SimRisorse.Core.Content.ResourceDefinition("pencils", "Matite"))
                         .OrderBy(item => item.Name, ResourceNameComparer)
                         .ThenBy(item => item.Id, StringComparer.Ordinal))
            {
                var rate = rates[resource.Id];
                Resources.Add(new ResourceRow(new ResourceView(resource.Id, resource.Name,
                    snapshot.Resources[resource.Id].Units, capacity[resource.Id].Units,
                    rate.ProductionPerSecond, rate.ConsumptionPerSecond)));
            }
        }
        var resourceViews = new List<ResourceView>(Resources.Count);
        foreach (var row in Resources)
        {
            var rate = rates[row.Id];
            var resource = new ResourceView(row.Id, row.Name, snapshot.Resources[row.Id].Units,
                capacity[row.Id].Units, rate.ProductionPerSecond, rate.ConsumptionPerSecond);
            row.Update(resource, CapacityBreakdown(snapshot, row.Id, capacity[row.Id].Units));
            resourceViews.Add(resource);
        }
        _breadSupply = V14BreadSupplyProjector.Project(_baseline, snapshot, preview, resourceViews);
        Notify(nameof(ResourceHighlights));
    }

    private string CapacityBreakdown(V14CampaignSnapshot snapshot, string resourceId, decimal total)
    {
        var parts = new List<string>();
        string number(decimal value) => value.ToString("#,0.###", Italian);
        if (resourceId == "pencils")
        {
            return $"Matite: {number(snapshot.Resources[resourceId].Units)} / {number(total)}. " +
                "Capienza: Municipio (+1000), Magazzino (+500 ciascuno), Scuola (+50 ciascuna). " +
                "Nella Scuola puoi produrre 4 Matite con 1 Legno lavorato e 1 Carbone.";
        }
        var initial = _baseline.Capacity[resourceId].Units;
        if (initial > 0) parts.Add($"{number(initial)} iniziale");
        foreach (var building in snapshot.OtherBuildings)
        {
            if (building.Count == 0) continue;
            var definition = _baseline.Catalog.BuildingsById[building.BuildingId];
            var contribution = (definition.StoragePerBuilding ?? SimRisorse.Core.Economy.ResourceBundle.Empty)[resourceId].Units;
            if (contribution > 0m)
                parts.Add($"{building.Count} {definition.Name} × {number(contribution)}");
        }
        foreach (var family in snapshot.ProductiveFamilies)
        {
            var definition = _baseline.Catalog.BuildingsById[family.FamilyId];
            var contribution = (definition.StoragePerBuilding ?? SimRisorse.Core.Economy.ResourceBundle.Empty)[resourceId].Units;
            if (contribution <= 0m) continue;
            if (family.Base.Buildings > 0)
                parts.Add($"{family.Base.Buildings} {definition.Name} × {number(contribution)}");
            if (family.Large.Buildings > 0)
                parts.Add($"{family.Large.Buildings} {definition.Name} grandi × {number(contribution * 3m)}");
            if (family.Advanced.Buildings > 0)
                parts.Add($"{family.Advanced.Buildings} {definition.Name} avanzati × {number(contribution * 10m)}");
        }
        var owned = snapshot.Resources[resourceId].Units;
        var free = Math.Max(0m, total - owned);
        return $"{_baseline.Catalog.ResourcesById[resourceId].Name}: {number(owned)} / {number(total)}; " +
            $"spazio libero {number(free)}. Capienza: {string.Join(" + ", parts)}. " +
            "Per aumentarla, costruisci un Magazzino (+500 per risorsa) oppure un edificio produttivo che immagazzini questa risorsa.";
    }

    private PopulationForecast Growth => _economyPreview?.Population ?? _session.ForecastEconomy().Population;
    public bool HasPopulationGrowth => _baseline.PopulationRules is not null;
    public bool HasAdvancedHousingRules => _baseline.PopulationRules?.AdvancedHousing is not null;
    public bool HasAdvancedResidents => Growth.AdvancedResidents > 0;
    public bool ShowBreadSupplyStatus => _breadSupply.State != BreadSupplyState.Hidden;
    public string ScenarioName => "Crescita civile";
    public string PlayState => IsPaused ? "IN PAUSA" : "IN CORSO";
    public string PauseAction => IsPaused ? "Avvia produzione" : "Metti in pausa";
    public string PopulationCount => $"{Population} / {Growth.HousingCapacity}";
    public string AvailableWorkersText => AvailablePopulation == 1 ? "1 libero" : $"{AvailablePopulation} liberi";
    public double HousingFillPercent => Growth.HousingCapacity > 0
        ? 100d * Population / Growth.HousingCapacity : 0d;
    public string FoodRequirement => Growth.RequiredFood.Units.ToString("0.###", Italian) + " Cibo/s";
    public string BreadRequirement => Growth.RequiredBread.Units.ToString("0.###", Italian) +
        $" Pane/s · {Growth.AdvancedResidents} residenti avanzati";
    public string GrowthStatus => Growth.BlockReason switch
    {
        GrowthBlockReason.MissingFood => "Manca cibo: crescita sospesa",
        GrowthBlockReason.MissingBread => "Manca pane: crescita avanzata sospesa",
        GrowthBlockReason.HousingFull => "Case piene: costruisci abitazioni",
        GrowthBlockReason.None => $"Prossimo abitante: {Growth.ProgressSeconds} / {Growth.IntervalSeconds} s · " +
            $"×{(Growth.GrowthRateTwentieths / (decimal)HousingGrowthRules.UnitsPerSecond).ToString("0.##", Italian)}",
        _ => "Popolazione fissa"
    };
    public double GrowthPercent => Growth.IntervalSeconds > 0
        ? 100d * (Growth.ProgressSeconds + Growth.GrowthRemainderTwentieths / (double)HousingGrowthRules.UnitsPerSecond) /
            Growth.IntervalSeconds : 0d;
    public string BreadSupplyStatus => BreadSupplyStatusFormatter.Format(_breadSupply);

    public IReadOnlyList<int> BatchSizes { get; } = Array.AsReadOnly(new[] { 1, 5, 10 });
    public int MilitaryQuantity
    {
        get => _militaryQuantity;
        set
        {
            if (!BatchSizes.Contains(value) || _militaryQuantity == value) return;
            _militaryQuantity = value;
            RefreshParity();
        }
    }
    public int MilitaryBarracks => _session.Snapshot().OtherBuildings.Single(item => item.BuildingId == "barracks").Count;
    public int WaitingRecruits => _session.Snapshot().Military.WaitingRecruits;
    public int TrainingRecruits => _session.Snapshot().Military.TrainingRecruits;
    public int AvailableSoldiers => _session.Snapshot().Military.AvailableSoldiers;
    public int MilitaryPeople => WaitingRecruits + TrainingRecruits + AvailableSoldiers;
    public long MilitarySeats => (long)MilitaryBarracks * (_baseline.MilitaryRules?.SeatsPerBarracks ?? 0);
    public long FreeMilitarySeats => Math.Max(0L, MilitarySeats - TrainingRecruits);
    public int MilitarySwordCost => (_baseline.MilitaryRules?.SwordsPerRecruit ?? 0) * MilitaryQuantity;
    public decimal AvailableSwords => _session.Snapshot().Resources["swords"].Units;
    public decimal FreeSwordStorage => Math.Max(0m, CalculateStorageCapacity(_session.Snapshot())["swords"].Units - AvailableSwords);
    public int MilitaryCancellable => checked(WaitingRecruits + TrainingRecruits);
    public string MilitaryLimitsNote => $"Arruolabili ora: " +
        $"{Math.Max(0, Math.Min(AvailablePopulation, MilitaryBarracks == 0 ? 0 : (int)(AvailableSwords / (_baseline.MilitaryRules?.SwordsPerRecruit ?? 5))))}" +
        $" · annullabili con rimborso: {Math.Min(MilitaryCancellable, (int)(FreeSwordStorage / (_baseline.MilitaryRules?.SwordsPerRecruit ?? 5)))}.";
    public string MilitaryRoster =>
        $"{Population} abitanti = {Population - AvailablePopulation - MilitaryPeople - _session.Snapshot().SchoolResearch.ReservedPopulation}" +
        $" lavoratori + {AvailablePopulation} liberi + {_session.Snapshot().SchoolResearch.ReservedPopulation} studenti/ricercatori + " +
        $"{WaitingRecruits} in coda + {TrainingRecruits} in addestramento + {AvailableSoldiers} soldati";
    public string MilitaryCapacityStatus => $"{MilitaryBarracks} {(MilitaryBarracks == 1 ? "Caserma" : "Caserme")} · " +
        $"{TrainingRecruits}/{MilitarySeats} posti occupati · {FreeMilitarySeats} posti liberi";
    public string MilitarySeatsExplanation => "Una Caserma richiede la Forgia e ospita una recluta per volta.";
    public string MilitaryWeaponsStatus => $"Spade disponibili: {AvailableSwords.ToString("#,0.###", Italian)} · " +
        $"Costo per {MilitaryQuantity} reclute: {MilitarySwordCost} Spade";
    public string MilitaryArmamentExplanation => "Ogni recluta consuma cinque Spade. All'annullamento tornano nel magazzino; il congedo resta rinviato.";
    public string MilitaryTrainingHeading => "FORMAZIONE · 30 SECONDI PER RECLUTA";
    public string MilitaryQueueExplanation => "Le reclute eccedenti attendono in coda. L'annullamento perde il tempo già svolto.";
    public string MilitaryTrainingStatus => TrainingRecruits == 0 ? "Nessuna recluta in addestramento." :
        $"{TrainingRecruits} reclute in addestramento individuale · prima pronta tra " +
        TimeSpan.FromSeconds((_baseline.MilitaryRules?.TrainingSecondsPerGroup ?? 30) -
            _session.Snapshot().Military.TrainingProgressSeconds).ToString(@"mm\:ss", Italian) + ".";
    public double MilitaryTrainingPercent => TrainingRecruits > 0
        ? 100d * _session.Snapshot().Military.TrainingProgressSeconds / (_baseline.MilitaryRules?.TrainingSecondsPerGroup ?? 30) : 0d;
    public string MilitaryRecruitmentHint => MilitaryBarracks == 0
        ? "Costruisci una Caserma dalla scheda Edifici (richiede prima la Forgia)."
        : AvailablePopulation < MilitaryQuantity ? "Abitanti liberi insufficienti: libera lavoratori o attendi nuova popolazione."
        : AvailableSwords < MilitarySwordCost ? "Spade insufficienti: aumenta produzione e spazio della Forgia."
        : "Arruolamento possibile: le reclute occupano alloggi e consumano le razioni normali.";
    public string MilitaryCancelHint => MilitaryCancellable < MilitaryQuantity ? "Reclute annullabili insufficienti."
        : FreeSwordStorage < MilitarySwordCost ? "Spazio insufficiente per restituire le Spade: costruisci un Magazzino."
        : "Annulla prima la coda e poi gli allievi meno avanzati, restituendo le Spade.";
    public string MilitaryDischargeHint => "Congedo rinviato fino alle missioni: i soldati non possono ancora essere congedati.";
    public string EnlistLabel => $"Arruola +{MilitaryQuantity}";
    public string CancelRecruitsLabel => $"Annulla reclute −{MilitaryQuantity}";
    public string DischargeLabel => $"Congeda −{MilitaryQuantity}";
    public bool IsNewGameConfirmationVisible => _newGameConfirmation;

    private void RefreshParity()
    {
        string[] properties =
        [
            nameof(SelectedResourceStorageDetails), nameof(HasPopulationGrowth), nameof(HasAdvancedHousingRules),
            nameof(HasAdvancedResidents), nameof(ShowBreadSupplyStatus), nameof(ScenarioName), nameof(PlayState),
            nameof(PauseAction), nameof(PopulationCount), nameof(AvailableWorkersText), nameof(HousingFillPercent),
            nameof(FoodRequirement), nameof(BreadRequirement), nameof(GrowthStatus), nameof(GrowthPercent),
            nameof(BreadSupplyStatus), nameof(MilitaryBarracks), nameof(WaitingRecruits), nameof(TrainingRecruits),
            nameof(AvailableSoldiers), nameof(MilitarySeats), nameof(MilitaryCancellable), nameof(MilitaryLimitsNote),
            nameof(MilitaryRoster), nameof(MilitaryCapacityStatus), nameof(MilitarySeatsExplanation),
            nameof(MilitaryWeaponsStatus), nameof(MilitaryArmamentExplanation), nameof(MilitaryTrainingHeading),
            nameof(MilitaryQueueExplanation), nameof(MilitaryTrainingStatus), nameof(MilitaryTrainingPercent),
            nameof(MilitaryRecruitmentHint), nameof(MilitaryCancelHint), nameof(MilitaryDischargeHint),
            nameof(EnlistLabel), nameof(CancelRecruitsLabel), nameof(DischargeLabel),
            nameof(MilitaryQuantity), nameof(IsNewGameConfirmationVisible)
        ];
        foreach (var name in properties) Notify(name);
        EnlistCommand.Refresh(); CancelRecruitsCommand.Refresh(); DischargeCommand.Refresh();
        NewGameCommand.Refresh(); ConfirmNewGameCommand.Refresh(); CancelNewGameCommand.Refresh();
    }
}
