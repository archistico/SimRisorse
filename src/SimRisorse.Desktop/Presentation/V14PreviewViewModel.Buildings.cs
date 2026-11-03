using System.Collections.ObjectModel;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Desktop.Presentation;

/// <summary>
/// The v14 catalog is presented exactly like the original building list:
/// each productive tier is an independent entry. Selecting an entry defines
/// its size; no size picker is needed or permitted in the building details.
/// </summary>
public sealed partial class V14PreviewViewModel
{
    private V14BuildingEntry? _selectedBuilding;
    public ObservableCollection<V14BuildingEntry> Buildings { get; } = new();

    public V14BuildingEntry? SelectedBuilding
    {
        get => _selectedBuilding;
        set
        {
            if (ReferenceEquals(_selectedBuilding, value)) return;
            _selectedBuilding = value;
            if (value is { Kind: V14BuildingKind.Producer, Tier: { } tier })
            {
                SelectedProducer = Producers.Single(item => item.Id == value.FamilyId);
                BuildTier = TierChoices.Single(item => item.Tier == tier);
                UpgradeFrom = BuildTier;
                UpgradeTo = TierChoices.Single(item => item.Tier == (tier == ProductionTier.Base
                    ? ProductionTier.Large : ProductionTier.Advanced));
            }
            else if (value is { Kind: V14BuildingKind.Civic })
                SelectedCivic = Civics.Single(item => item.Id == value.FamilyId);
            Notify();
            Notify(nameof(CanBuildSelected));
            Notify(nameof(CanAffordSelected));
            Notify(nameof(CanAffordFive));
            Notify(nameof(CanAffordTen));
            Notify(nameof(BuildFiveCost));
            Notify(nameof(BuildTenCost));
            Notify(nameof(ShowUpgradeNext));
            Notify(nameof(ShowUpgradeDirectlyToAdvanced));
            Notify(nameof(NextUpgradeRequirement));
            Notify(nameof(ShowNextUpgradeRequirement));
            Notify(nameof(CanAssignSelected));
            Notify(nameof(CanAssignSelectedOne));
            Notify(nameof(CanAssignSelectedFive));
            Notify(nameof(CanAssignSelectedTen));
            Notify(nameof(CanUpgradeNext));
            Notify(nameof(CanUpgradeDirectlyToAdvanced));
            Notify(nameof(NextUpgradeLabel));
            Notify(nameof(DirectUpgradeLabel));
            Notify(nameof(NextUpgradeCost));
            Notify(nameof(DirectUpgradeCost));
            BuildSelectedCommand.Refresh();
            BuildFiveCommand.Refresh();
            BuildTenCommand.Refresh();
            AssignSelectedWorkerCommand.Refresh();
            AssignFiveSelectedWorkersCommand.Refresh();
            AssignTenSelectedWorkersCommand.Refresh();
            ReleaseSelectedWorkerCommand.Refresh();
            UpgradeSelectedCommand.Refresh();
            UpgradeSelectedToAdvancedCommand.Refresh();
        }
    }

    // Locked entries remain selectable so their missing prerequisites are visible,
    // but never expose a "Costruisci" control before becoming unlocked.
    public bool CanBuildSelected => SelectedBuilding is { Kind: not V14BuildingKind.Fixed, IsUnlocked: true };
    public bool CanAffordSelected => CanBuildQuantity(1);
    public bool CanAffordFive => CanBuildQuantity(5);
    public bool CanAffordTen => CanBuildQuantity(10);
    public bool CanAssignSelected => SelectedBuilding is { Kind: V14BuildingKind.Producer, Count: > 0 };
    public bool CanAssignSelectedOne => CanAssignSelectedQuantity(1);
    public bool CanAssignSelectedFive => CanAssignSelectedQuantity(5);
    public bool CanAssignSelectedTen => CanAssignSelectedQuantity(10);

    private bool CanAssignSelectedQuantity(int quantity) => CanMutateColony && quantity > 0 &&
        SelectedBuilding is { Kind: V14BuildingKind.Producer, Tier: { }, Count: > 0 } selected &&
        (long)selected.Count * selected.Workplaces - selected.Workers >= quantity &&
        AvailablePopulation >= quantity;

    // Visibility is determined by ownership and technology. Resources only affect
    // CanExecute: otherwise the upgrade disappears precisely when materials run out.
    public bool ShowUpgradeNext => SelectedBuilding is
        { Kind: V14BuildingKind.Producer, Tier: ProductionTier.Base or ProductionTier.Large, Count: > 0 } selected &&
        IsUpgradeUnlocked(selected, selected.Tier == ProductionTier.Base ? ProductionTier.Large : ProductionTier.Advanced);
    public bool ShowUpgradeDirectlyToAdvanced => SelectedBuilding is
        { Kind: V14BuildingKind.Producer, Tier: ProductionTier.Base, Count: > 0 } selected &&
        IsUpgradeUnlocked(selected, ProductionTier.Advanced);
    public bool CanUpgradeNext => CanMutateColony && ShowUpgradeNext && CanUpgrade(SelectedBuilding!,
        SelectedBuilding!.Tier == ProductionTier.Base ? ProductionTier.Large : ProductionTier.Advanced);
    public bool CanUpgradeDirectlyToAdvanced => CanMutateColony && ShowUpgradeDirectlyToAdvanced &&
        CanUpgrade(SelectedBuilding!, ProductionTier.Advanced);

    private bool IsUpgradeUnlocked(V14BuildingEntry selected, ProductionTier destination) =>
        _families[selected.FamilyId].IsUnlocked(destination,
            _session.Snapshot().UnlockedTechnologyIds.ToHashSet(StringComparer.Ordinal));

    public string NextUpgradeRequirement
    {
        get
        {
            var selected = SelectedBuilding;
            if (selected is not { Kind: V14BuildingKind.Producer, Count: > 0 } || selected.Tier == ProductionTier.Advanced)
                return string.Empty;
            var dest = selected.Tier == ProductionTier.Base ? ProductionTier.Large : ProductionTier.Advanced;
            var required = _families[selected.FamilyId].Get(dest).RequiredTechnologies
                .Where(id => !_session.Snapshot().UnlockedTechnologyIds.Contains(id));
            var names = TechnologyNames(required);
            return names == "nessuno" ? string.Empty : "Per migliorare: ricerca " + names;
        }
    }
    public bool ShowNextUpgradeRequirement => NextUpgradeRequirement.Length > 0;

    // All three purchase buttons and their quoted costs use the identical
    // progressive, per-building rounded price calculation used by the session.
    private ResourceBundle QuoteBuild(V14BuildingEntry selected, int quantity)
    {
        var count = PurchaseCount(selected.FamilyId);
        return selected.Kind switch
        {
            V14BuildingKind.Producer => ProgressiveConstructionPricing.QuoteNewBuildings(
                _families[selected.FamilyId].Source.Cost,
                _families[selected.FamilyId].Get(selected.Tier!.Value).CostMultiplier,
                _tierRules.GrowthFactor, count, quantity),
            V14BuildingKind.Civic => ProgressiveConstructionPricing.QuoteNewBuildings(
                _baseline.Catalog.BuildingsById[selected.FamilyId].Cost, 1m,
                _tierRules.GrowthFactor, count, quantity),
            V14BuildingKind.School => ProgressiveConstructionPricing.QuoteNewBuildings(
                V14SchoolConstructionRules.Candidate.Cost, 1m,
                V14SchoolConstructionRules.Candidate.GrowthFactor, count, quantity),
            _ => throw new InvalidOperationException("Edificio non acquistabile.")
        };
    }

    private bool CanBuildQuantity(int quantity)
    {
        var selected = SelectedBuilding;
        if (!CanMutateColony || selected is not { IsUnlocked: true } || selected.Kind == V14BuildingKind.Fixed || quantity <= 0)
            return false;
        try { return _session.Snapshot().Resources.Covers(QuoteBuild(selected, quantity)); }
        catch (Exception exception) when (exception is OverflowException or ArgumentOutOfRangeException)
        { return false; }
    }

    public string BuildFiveCost => DescribeBuildBatch(5);
    public string BuildTenCost => DescribeBuildBatch(10);

    private string DescribeBuildBatch(int quantity)
    {
        if (SelectedBuilding is not { Kind: not V14BuildingKind.Fixed } selected) return string.Empty;
        try
        {
            var quote = QuoteBuild(selected, quantity);
            return $"+{quantity}: {FormatCost(quote)}" + ShortageExplanation(quote);
        }
        catch (Exception exception) when (exception is OverflowException or ArgumentOutOfRangeException)
        { return $"+{quantity}: costo non calcolabile (limite numerico raggiunto)."; }
    }

    private string ShortageExplanation(ResourceBundle quote)
    {
        var available = _session.Snapshot().Resources;
        var missing = quote.Amounts.Where(pair => available[pair.Key].Units < pair.Value.Units)
            .Select(pair => $"{_baseline.Catalog.ResourcesById[pair.Key].Name} " +
                $"({available[pair.Key].Units.ToString("#,0.###", Italian)} / {pair.Value.Units.ToString("#,0.###", Italian)})")
            .ToArray();
        return missing.Length == 0 ? string.Empty : " · Mancano: " + string.Join(" · ", missing);
    }

    private bool CanUpgrade(V14BuildingEntry selected, ProductionTier destination)
    {
        var family = _families[selected.FamilyId];
        if (!family.IsUnlocked(destination, _session.Snapshot().UnlockedTechnologyIds.ToHashSet(StringComparer.Ordinal)))
            return false;
        try
        {
            var quote = ProgressiveConstructionPricing.QuoteUpgrade(family.Source.Cost,
                family.Get(selected.Tier!.Value).CostMultiplier, family.Get(destination).CostMultiplier,
                _tierRules.GrowthFactor, PurchaseCount(selected.FamilyId));
            return _session.Snapshot().Resources.Covers(quote);
        }
        catch (Exception exception) when (exception is OverflowException or ArgumentOutOfRangeException)
        {
            return false;
        }
    }
    public string NextUpgradeLabel => SelectedBuilding?.Tier == ProductionTier.Base
        ? "Potenzia 1 → Grande" : "Potenzia 1 → Avanzato";
    public string DirectUpgradeLabel => "Potenzia 1 → Avanzato";
    public string NextUpgradeCost => UpgradeQuote(SelectedBuilding,
        SelectedBuilding?.Tier == ProductionTier.Base ? ProductionTier.Large : ProductionTier.Advanced);
    public string DirectUpgradeCost => UpgradeQuote(SelectedBuilding, ProductionTier.Advanced);

    public UiCommand BuildSelectedCommand { get; private set; } = null!;
    public UiCommand BuildFiveCommand { get; private set; } = null!;
    public UiCommand BuildTenCommand { get; private set; } = null!;
    public UiCommand AssignSelectedWorkerCommand { get; private set; } = null!;
    public UiCommand AssignFiveSelectedWorkersCommand { get; private set; } = null!;
    public UiCommand AssignTenSelectedWorkersCommand { get; private set; } = null!;
    public UiCommand ReleaseSelectedWorkerCommand { get; private set; } = null!;
    public UiCommand UpgradeSelectedCommand { get; private set; } = null!;
    public UiCommand UpgradeSelectedToAdvancedCommand { get; private set; } = null!;

    public string MilitaryOverview
    {
        get
        {
            var military = _session.Snapshot().Military;
            return $"{military.AvailableSoldiers} soldati disponibili · " +
                $"{military.TrainingRecruits} in addestramento · {military.WaitingRecruits} in attesa.";
        }
    }

    private void InitializeBuildingEntries()
    {
        foreach (var building in _baseline.Catalog.Buildings.OrderBy(item => item.Name, StringComparer.Ordinal))
        {
            if (_families.TryGetValue(building.Id, out var family))
            {
                foreach (var spec in family.Tiers)
                {
                    var title = spec.Tier switch
                    {
                        ProductionTier.Base => building.Name,
                        ProductionTier.Large => building.Name + " grande",
                        _ => building.Name + (building.Name.EndsWith('a') ? " avanzata" : " avanzato")
                    };
                    var output = ResourceLines(building.Recipe!.Output);
                    var input = ResourceLines(building.Recipe.Input);
                    var storageUnits = spec.Tier == ProductionTier.Base ? 50m :
                        spec.Tier == ProductionTier.Large ? 150m : 500m;
                    var storage = ResourceLines(ResourceBundle.Create(
                        building.Recipe.Output.Amounts.Keys.Select(id => (id, storageUnits)).ToArray()));
                    Buildings.Add(new V14BuildingEntry(building.Id, V14BuildingKind.Producer,
                        spec.Tier, title, building.Id, spec.Workplaces,
                        $"{title}. {spec.Workplaces} posti di lavoro. Ogni addetto produce e consuma secondo la ricetta originale.",
                        output, input, storage));
                }
            }
            else
            {
                var output = Array.Empty<ResourceDetailLine>();
                var storage = ResourceLines(building.StoragePerBuilding ?? ResourceBundle.Empty);
                if (building.Id is "town-hall" or "warehouse")
                {
                    // Matite follow the same per-building capacity as Legno lavorato.
                    // This v14-only line mirrors V14SchoolSupplies without changing v13.
                    var pencilStorage = (building.StoragePerBuilding ?? ResourceBundle.Empty)["planks"].Units;
                    if (pencilStorage > 0m)
                        storage = storage.Append(new ResourceDetailLine(V14SchoolSupplies.ResourceId,
                            $"{pencilStorage.ToString("#,0.###", Italian)} Matite")).ToArray();
                }
                if (building.HousingPerBuilding > 0)
                    storage = storage.Append(new ResourceDetailLine("population",
                        $"{building.HousingPerBuilding} posti abitativi")).ToArray();
                Buildings.Add(new V14BuildingEntry(building.Id, building.IsScenarioFixed
                    ? V14BuildingKind.Fixed : V14BuildingKind.Civic, null, building.Name,
                    building.Id, building.WorkplacesPerBuilding, building.IsScenarioFixed
                        ? "Centro iniziale della colonia. Non può essere costruito nuovamente."
                        : building.HousingPerBuilding > 0
                            ? $"Aggiunge {building.HousingPerBuilding} posti abitativi per struttura."
                            : "Edificio civile della colonia.", output, output, storage));
            }
        }
        Buildings.Add(new V14BuildingEntry(V14CampaignSnapshots.SchoolId, V14BuildingKind.School,
            null, "Scuola", "school", 0,
            "Forma gli abitanti in ricercatori. I punti ricerca si accumulano liberamente e si spendono quando vuoi.",
            [], [],
            [new ResourceDetailLine("population", "1 posto formazione e 2 posti ricercatori per Scuola"),
             new ResourceDetailLine(V14SchoolSupplies.ResourceId,
                 $"{V14SchoolSupplies.SchoolPencilStorage} Matite")]));
        // The old game uses a single alphabetical catalog; tiers remain adjacent.
        var sorted = Buildings.OrderBy(item => _baseline.Catalog.BuildingsById.TryGetValue(item.FamilyId, out var definition) ? definition.Name : item.Name, StringComparer.Ordinal)
            .ThenBy(item => item.Tier is null ? -1 : (int)item.Tier.Value).ToArray();
        Buildings.Clear();
        foreach (var entry in sorted) Buildings.Add(entry);
        _selectedBuilding = Buildings.FirstOrDefault(item => item.FamilyId == "town-hall") ?? Buildings.FirstOrDefault();
        Notify(nameof(SelectedBuilding));

        BuildSelectedCommand = new UiCommand(() => BuildSelectedQuantity(1), () => CanBuildQuantity(1));
        BuildFiveCommand = new UiCommand(() => BuildSelectedQuantity(5), () => CanBuildQuantity(5));
        BuildTenCommand = new UiCommand(() => BuildSelectedQuantity(10), () => CanBuildQuantity(10));
        AssignSelectedWorkerCommand = new UiCommand(() => AssignSelectedWorkers(1), () => CanAssignSelectedOne);
        AssignFiveSelectedWorkersCommand = new UiCommand(() => AssignSelectedWorkers(5), () => CanAssignSelectedFive);
        AssignTenSelectedWorkersCommand = new UiCommand(() => AssignSelectedWorkers(10), () => CanAssignSelectedTen);
        ReleaseSelectedWorkerCommand = new UiCommand(() =>
        {
            var selected = SelectedBuilding;
            if (selected is { Kind: V14BuildingKind.Producer, Tier: { } tier })
                Perform(() => _session.ReleaseProducerWorkers(selected.FamilyId, tier, 1).ToString(),
                    "Lavoratore liberato.");
        }, () => CanMutateColony && SelectedBuilding is { Kind: V14BuildingKind.Producer, Workers: > 0 });
        UpgradeSelectedCommand = new UiCommand(() => UpgradeSelected(
            SelectedBuilding?.Tier == ProductionTier.Base ? ProductionTier.Large : ProductionTier.Advanced),
            () => CanUpgradeNext);
        UpgradeSelectedToAdvancedCommand = new UiCommand(() => UpgradeSelected(ProductionTier.Advanced),
            () => CanUpgradeDirectlyToAdvanced);
    }

    // A batch is committed atomically by the same tier session used for +1.
    // Guards also re-check the live population and free workplaces on click.
    private void AssignSelectedWorkers(int quantity)
    {
        var selected = SelectedBuilding;
        if (!CanAssignSelectedQuantity(quantity) ||
            selected is not { Kind: V14BuildingKind.Producer, Tier: { } tier }) return;
        Perform(() => _session.AssignProducerWorkers(selected.FamilyId, tier, quantity).ToString(),
            quantity == 1 ? "Lavoratore assegnato." : $"{quantity} lavoratori assegnati.");
    }

    private void BuildSelectedQuantity(int quantity)
    {
        var selected = SelectedBuilding;
        if (selected is null || !CanBuildQuantity(quantity)) return;
        switch (selected.Kind)
        {
            case V14BuildingKind.Producer:
                Perform(() => _session.BuildProducer(selected.FamilyId, selected.Tier!.Value, quantity).Failure.ToString(),
                    $"{quantity} {selected.Name} costruiti.");
                break;
            case V14BuildingKind.Civic:
                Perform(() => _session.BuildCivicBuilding(selected.FamilyId, quantity).Failure.ToString(),
                    $"{quantity} {selected.Name} costruiti.");
                break;
            case V14BuildingKind.School:
                Perform(() => _session.BuildSchool(quantity).Failure.ToString(),
                    $"{quantity} Scuole costruite.");
                break;
        }
    }

    private void UpgradeSelected(ProductionTier destination)
    {
        var selected = SelectedBuilding;
        if (selected is null || selected.Kind != V14BuildingKind.Producer ||
            selected.Tier is null || selected.Tier.Value >= destination) return;
        var source = selected.Tier.Value;
        Perform(() => _session.UpgradeProducer(selected.FamilyId, source, destination).Failure.ToString(),
            $"{selected.Name} potenziato.");
    }

    private string UpgradeQuote(V14BuildingEntry? selected, ProductionTier destination)
    {
        if (selected is null || selected.Kind != V14BuildingKind.Producer ||
            selected.Tier is null || selected.Tier.Value >= destination)
            return string.Empty;
        var source = selected.Tier.Value;
        try
        {
            var quote = ProgressiveConstructionPricing.QuoteUpgrade(
                _families[selected.FamilyId].Source.Cost,
                _families[selected.FamilyId].Get(source).CostMultiplier,
                _families[selected.FamilyId].Get(destination).CostMultiplier,
                _tierRules.GrowthFactor, PurchaseCount(selected.FamilyId));
            return "Costo upgrade: " + FormatCost(quote) + ShortageExplanation(quote);
        }
        catch (Exception exception) when (exception is OverflowException or ArgumentOutOfRangeException)
        { return "Costo upgrade: non calcolabile (limite numerico raggiunto)."; }
    }

    private IReadOnlyList<ResourceDetailLine> ResourceLines(ResourceBundle bundle) => bundle.Amounts
        .Where(item => item.Value.Units > 0m)
        .Select(item => new ResourceDetailLine(item.Key,
            $"{item.Value.Units.ToString("#,0.###", Italian)} {_baseline.Catalog.ResourcesById[item.Key].Name}"))
        .ToArray();

    private ResourceBundle CalculateStorageCapacity(V14CampaignSnapshot snapshot)
    {
        var capacity = _baseline.Capacity;
        foreach (var definition in _baseline.Catalog.Buildings)
        {
            if (definition.StoragePerBuilding is not { } storage) continue;
            var family = snapshot.ProductiveFamilies.FirstOrDefault(item => item.FamilyId == definition.Id);
            var equivalents = family is null
                ? snapshot.OtherBuildings.Single(item => item.BuildingId == definition.Id).Count
                : checked(family.Base.Buildings + 3 * family.Large.Buildings + 10 * family.Advanced.Buildings);
            if (equivalents > 0) capacity += storage.Scale(equivalents);
        }
        return capacity + ResourceBundle.FromAmounts([
            KeyValuePair.Create(SimRisorse.Core.Research.V14SchoolSupplies.ResourceId,
                SimRisorse.Core.Research.V14SchoolSupplies.Capacity(_baseline, snapshot))]);
    }

    // The production planner reports one row for the entire family: Base, Grande
    // and Avanzato share the recipe and production priority. Do not falsely
    // attribute its runnable workers to an individual tier in this read model.
    private string FamilyProductionDiagnosis(V14BuildingEntry selected, BuildingProduction report,
        V14CampaignSnapshot snapshot, ResourceBundle capacity)
    {
        if (report.AssignedWorkers == 0 || report.RunnableWorkers >= report.AssignedWorkers)
            return string.Empty;

        var messages = new List<string>
        {
            $"Filiera {_baseline.Catalog.BuildingsById[selected.FamilyId].Name} (tutti i livelli): " +
            $"{report.RunnableWorkers}/{report.AssignedWorkers} addetti possono produrre nel prossimo secondo."
        };
        if (!report.MissingInputIds.IsDefaultOrEmpty)
            messages.Add("Materie prime insufficienti o già impegnate da altre strutture: " +
                string.Join(", ", report.MissingInputIds.Select(id => _baseline.Catalog.ResourcesById[id].Name)) + ".");
        if (!report.FullOutputIds.IsDefaultOrEmpty)
        {
            var recipe = _baseline.Catalog.BuildingsById[selected.FamilyId].Recipe!;
            foreach (var id in report.FullOutputIds)
            {
                var stock = snapshot.Resources[id].Units;
                var total = capacity[id].Units;
                var free = Math.Max(0m, total - stock);
                var batch = recipe.Output[id].Units;
                var name = _baseline.Catalog.ResourcesById[id].Name;
                string units(decimal value) => value.ToString("#,0.###", Italian);
                messages.Add($"Spazio insufficiente per {name}: {units(stock)} / {units(total)} " +
                    $"(liberi {units(free)}; servono {units(batch)} posti per lotto e addetto).");
            }
            messages.Add("Per aumentare la capacità, costruisci un Magazzino (+500 per ogni risorsa) " +
                "o un altro edificio che produce la risorsa interessata.");
            if (report.FullOutputIds.Contains("bread"))
                messages.Add("Il Deposito alimentare conserva Cibo, non Pane: " +
                    "per il Pane servono Forni, Magazzini o la capacità condivisa del Municipio.");
        }
        messages.Add("La previsione tiene conto delle razioni degli abitanti e della priorità tra strutture.");
        return string.Join(" ", messages);
    }

    private static string AppendDiagnosis(string first, string additional) => additional.Length == 0
        ? first : first.Length == 0 ? additional : first + " " + additional;

    private void RefreshBuildingEntries(V14CampaignSnapshot snapshot)
    {
        var researched = snapshot.UnlockedTechnologyIds.ToHashSet(StringComparer.Ordinal);
        var production = _economyPreview?.Buildings.ToDictionary(item => item.BuildingId, StringComparer.Ordinal);
        var capacity = CalculateStorageCapacity(snapshot);
        foreach (var item in Buildings)
        {
            var count = 0;
            var workers = 0;
            var unlocked = true;
            string requirements = "";
            ResourceBundle? cost = null;
            switch (item.Kind)
            {
                case V14BuildingKind.Producer:
                {
                    var family = snapshot.ProductiveFamilies.Single(entry => entry.FamilyId == item.FamilyId);
                    var occupancy = item.Tier switch
                    {
                        ProductionTier.Base => family.Base,
                        ProductionTier.Large => family.Large,
                        _ => family.Advanced
                    };
                    count = occupancy.Buildings;
                    workers = occupancy.AssignedWorkers;
                    var definition = _families[item.FamilyId];
                    var required = definition.Get(item.Tier!.Value).RequiredTechnologies;
                    unlocked = definition.IsUnlocked(item.Tier.Value, researched);
                    if (!unlocked) requirements = "Ricerca richiesta: " + TechnologyNames(required);
                    var missingBuildings = definition.Source.Prerequisites.Where(id => !Owns(id, snapshot)).ToArray();
                    if (missingBuildings.Length > 0)
                    {
                        requirements = string.Join(" ", new[] { requirements,
                            "Costruire prima: " + BuildingNames(missingBuildings) }.Where(value => value.Length > 0));
                        unlocked = false;
                    }
                    cost = QuoteBuild(item, 1);
                    break;
                }
                case V14BuildingKind.Civic:
                case V14BuildingKind.Fixed:
                {
                    var current = snapshot.OtherBuildings.Single(entry => entry.BuildingId == item.FamilyId);
                    count = current.Count;
                    workers = current.AssignedWorkers;
                    if (item.Kind != V14BuildingKind.Fixed)
                    {
                        var definition = _baseline.Catalog.BuildingsById[item.FamilyId];
                        var missingBuildings = definition.Prerequisites.Where(id => !Owns(id, snapshot)).ToArray();
                        if (missingBuildings.Length > 0)
                        {
                            requirements = "Costruire prima: " + BuildingNames(missingBuildings);
                            unlocked = false;
                        }
                        cost = QuoteBuild(item, 1);
                    }
                    break;
                }
                case V14BuildingKind.School:
                    count = snapshot.SchoolCount;
                    cost = QuoteBuild(item, 1);
                    var schoolMissing = V14SchoolConstructionRules.Candidate.Prerequisites
                        .Where(id => !Owns(id, snapshot)).ToArray();
                    if (schoolMissing.Length > 0)
                    {
                        requirements = "Costruire prima: " + BuildingNames(schoolMissing);
                        unlocked = false;
                    }
                    break;
            }
            var price = cost is null ? "Non costruibile" : FormatCost(cost);
            var diagnosis = !unlocked ? requirements : cost is not null && !snapshot.Resources.Covers(cost)
                ? "Risorse insufficienti: " + string.Join(" · ", cost.Amounts
                    .Where(pair => snapshot.Resources[pair.Key].Units < pair.Value.Units)
                    .Select(pair => $"{_baseline.Catalog.ResourcesById[pair.Key].Name} " +
                        $"({snapshot.Resources[pair.Key].Units.ToString("#,0.###", Italian)} / {pair.Value.Units.ToString("#,0.###", Italian)})")) +
                    ". Nessun materiale sarà consumato finché il costo completo non sarà disponibile."
                : "";
            var status = !unlocked ? "Bloccato · " + requirements : item.Kind == V14BuildingKind.Fixed
                ? "Edificio iniziale" : count == 0 ? "Da costruire" : item.Kind == V14BuildingKind.Producer
                ? $"{workers}/{(long)count * item.Workplaces} lavoratori" : $"{count} costruiti";
            if (item.Kind == V14BuildingKind.Producer && count > 0)
            {
                if (workers == 0)
                    diagnosis = AppendDiagnosis(diagnosis,
                        "Nessun lavoratore assegnato a questo livello: assegna abitanti liberi per produrre.");
                else if (production is not null && production.TryGetValue(item.FamilyId, out var report))
                {
                    if (report.RunnableWorkers < report.AssignedWorkers)
                    {
                        status += $" · filiera {report.RunnableWorkers}/{report.AssignedWorkers} attivi";
                        diagnosis = AppendDiagnosis(diagnosis, FamilyProductionDiagnosis(item, report, snapshot, capacity));
                    }
                    else status += " · produzione pronta";
                }
            }
            item.Update(count, workers, status, price, requirements, unlocked,
                cost is null ? [] : ResourceLines(cost), diagnosis);
        }
        Notify(nameof(MilitaryOverview));
        Notify(nameof(CanBuildSelected));
        Notify(nameof(CanAffordSelected));
        Notify(nameof(CanAffordFive));
        Notify(nameof(CanAffordTen));
        Notify(nameof(BuildFiveCost));
        Notify(nameof(BuildTenCost));
        Notify(nameof(ShowUpgradeNext));
        Notify(nameof(ShowUpgradeDirectlyToAdvanced));
        Notify(nameof(NextUpgradeRequirement));
        Notify(nameof(ShowNextUpgradeRequirement));
        Notify(nameof(CanAssignSelected));
        Notify(nameof(CanAssignSelectedOne));
        Notify(nameof(CanAssignSelectedFive));
        Notify(nameof(CanAssignSelectedTen));
        Notify(nameof(CanUpgradeNext));
        Notify(nameof(CanUpgradeDirectlyToAdvanced));
        Notify(nameof(NextUpgradeLabel));
        Notify(nameof(NextUpgradeCost));
        Notify(nameof(DirectUpgradeCost));
        BuildSelectedCommand.Refresh();
        BuildFiveCommand.Refresh();
        BuildTenCommand.Refresh();
        AssignSelectedWorkerCommand.Refresh();
        AssignFiveSelectedWorkersCommand.Refresh();
        AssignTenSelectedWorkersCommand.Refresh();
        ReleaseSelectedWorkerCommand.Refresh();
        UpgradeSelectedCommand.Refresh();
        UpgradeSelectedToAdvancedCommand.Refresh();
    }
}

public enum V14BuildingKind { Fixed, Producer, Civic, School }

public sealed class V14BuildingEntry(
    string familyId, V14BuildingKind kind, ProductionTier? tier, string name,
    string glyphKey, int workplaces, string description,
    IReadOnlyList<ResourceDetailLine> outputLines,
    IReadOnlyList<ResourceDetailLine> inputLines,
    IReadOnlyList<ResourceDetailLine> capacityLines) : ObservableObject
{
    private int _count;
    private int _workers;
    private string _status = string.Empty;
    private string _price = string.Empty;
    private string _requirements = string.Empty;
    private bool _unlocked;
    private string _diagnosis = "";
    private IReadOnlyList<ResourceDetailLine> _costLines = [];
    public string FamilyId { get; } = familyId;
    public V14BuildingKind Kind { get; } = kind;
    public ProductionTier? Tier { get; } = tier;
    public string Name { get; } = name;
    public string GlyphKey { get; } = glyphKey;
    public string Description { get; } = description;
    public int Workplaces { get; } = workplaces;
    public IReadOnlyList<ResourceDetailLine> OutputLines { get; } = outputLines;
    public IReadOnlyList<ResourceDetailLine> InputLines { get; } = inputLines;
    public IReadOnlyList<ResourceDetailLine> CapacityLines { get; } = capacityLines;
    public bool IsProducer => Kind == V14BuildingKind.Producer;
    public bool HasCost => Kind != V14BuildingKind.Fixed;
    public bool HasInputs => InputLines.Count > 0;
    public bool HasCapacity => CapacityLines.Count > 0;
    public int Count => _count;
    public int Workers => _workers;
    public string Staff => IsProducer ? $"{Workers} / {Count * Workplaces}" : "—";
    public string Status => _status;
    public string Price => _price;
    public string Requirements => _requirements;
    public bool HasRequirements => _requirements.Length > 0;
    public bool IsUnlocked => _unlocked;
    // Only dim the content of a locked entry. Never disable the ListBoxItem:
    // it must remain selectable so the player can read the prerequisites.
    public double RowOpacity => _unlocked ? 1d : 0.48d;
    public IReadOnlyList<ResourceDetailLine> CostLines => _costLines;
    public string Diagnosis => _diagnosis;
    public bool HasDiagnosis => _diagnosis.Length > 0;
    public void Update(int count, int workers, string status, string price,
        string requirements, bool unlocked, IReadOnlyList<ResourceDetailLine> costLines,
        string diagnosis)
    {
        if (count == _count && workers == _workers && status == _status && price == _price &&
            requirements == _requirements && unlocked == _unlocked && diagnosis == _diagnosis) return;
        _count = count;
        _workers = workers;
        _status = status;
        _price = price;
        _requirements = requirements;
        _unlocked = unlocked;
        _diagnosis = diagnosis;
        _costLines = costLines;
        Notify(string.Empty);
    }
}
