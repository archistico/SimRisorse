using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Population;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Desktop.Presentation;

/// <summary>
/// UI parity with the validated v14 building workspace. This file only projects
/// v15 domain data to the existing icon/label/action conventions; production and
/// pricing remain owned by the v15 session and its original rules.
/// </summary>
public sealed partial class V15PreviewViewModel
{
    public string ScenarioName => "Crescita civile";
    public bool HasPopulationGrowth => _baseline.PopulationRules is not null;
    public bool HasAdvancedHousingRules => _baseline.PopulationRules?.AdvancedHousing is not null;
    public bool ShowBreadSupplyStatus => HasAdvancedHousingRules;
    private PopulationForecast Growth => V15CampaignGrowthProjection.Project(_baseline, _session.Snapshot());
    public string PopulationCount => $"{Population} / {Growth.HousingCapacity}";
    public string AvailableWorkersText => AvailableWorkers == 1 ? "1 libero" : $"{AvailableWorkers} liberi";
    public double HousingFillPercent => Growth.HousingCapacity == 0 ? 0d :
        Math.Clamp(100d * Population / Growth.HousingCapacity, 0d, 100d);
    public string FoodRequirement => Growth.RequiredFood.Units.ToString("0.###", Italian) + " Cibo/s";
    public string BreadRequirement => Growth.RequiredBread.Units.ToString("0.###", Italian) +
        $" Pane/s · {Growth.AdvancedResidents} residenti avanzati";
    public string BreadSupplyStatus => Growth.BlockReason == GrowthBlockReason.MissingBread
        ? "Pane insufficiente: controlla Forno e Deposito alimentare."
        : "Il Pane per i residenti avanzati viene riservato nel Deposito alimentare.";
    public string GrowthStatus
    {
        get
        {
            var status = Growth;
            if (status.BlockReason == GrowthBlockReason.MissingFood) return "Manca Cibo pronto: crescita sospesa";
            if (status.BlockReason == GrowthBlockReason.MissingBread) return "Manca Pane: crescita avanzata sospesa";
            if (status.BlockReason == GrowthBlockReason.HousingFull) return "Case piene: costruisci abitazioni";
            if (status.BlockReason != GrowthBlockReason.None) return "Popolazione fissa";
            var progress = status.ProgressSeconds + status.GrowthRemainderTwentieths /
                (decimal)HousingGrowthRules.UnitsPerSecond;
            var speed = status.GrowthRateTwentieths / (decimal)HousingGrowthRules.UnitsPerSecond;
            var position = $"{progress.ToString("0.##", Italian)} / {status.IntervalSeconds} s";
            if (speed == 1m) return "Prossimo abitante: " + position;
            var remaining = (long)(status.IntervalSeconds - status.ProgressSeconds) *
                HousingGrowthRules.UnitsPerSecond - status.GrowthRemainderTwentieths;
            var eta = (long)Math.Ceiling(remaining / (decimal)status.GrowthRateTwentieths);
            return $"Abitante: {position} · ×{speed.ToString("0.##", Italian)} · ~{eta}s";
        }
    }
    public double GrowthPercent => Growth.IntervalSeconds == 0 ? 0 : Math.Clamp(
        100d * (Growth.ProgressSeconds + Growth.GrowthRemainderTwentieths /
            (double)HousingGrowthRules.UnitsPerSecond) / Growth.IntervalSeconds, 0d, 100d);

    private string ResourceName(string id) => id switch
    {
        "pencils" => "Matite", "cattle" => "Bovini", "game" => "Selvaggina",
        "meat" => "Carne", "vegetables" => "Verdura", "fruit" => "Frutta",
        _ => _baseline.Catalog.ResourcesById.TryGetValue(id, out var definition) ? definition.Name : id
    };

    private ResourceDetailLine Line(string id, decimal amount, string? suffix = null) =>
        new(id, $"{amount.ToString("#,0.###", Italian)} {ResourceName(id)}{suffix}");

    private IReadOnlyList<ResourceDetailLine> Lines(ResourceBundle bundle, string? suffix = null) =>
        bundle.Amounts.Select(kv => Line(kv.Key, kv.Value.Units, suffix)).ToArray();

    private IReadOnlyList<ResourceDetailLine> CostLines(ResourceBundle bundle) => Lines(bundle);

    private bool BuildingIsFixed(V15BuildingRow row) => row.Kind == V15BuildingKind.Civic &&
        _baseline.Catalog.BuildingsById[row.SourceId].IsScenarioFixed;

    private string BuildDescription(V15BuildingRow row)
    {
        if (row.Kind == V15BuildingKind.FoodIndustry)
            return row.SourceId switch
            {
                "stable" => "Alleva Bovini utilizzando Grano e Acqua; gli animali restano nei recinti.",
                "hunter" => "Procura Selvaggina da destinare al Macellaio.",
                "butcher" => "Trasforma Bovini o Selvaggina in Carne. Le due ricette sono alternative, non simultanee.",
                "garden" => "Coltiva Verdura utilizzando Acqua.",
                "orchard" => "Coltiva Frutta utilizzando Acqua.",
                "food-depot" => "Riceve Pesce, Carne, Verdura, Frutta e Pane. Converte gli ingredienti in Cibo pronto e serve le razioni.",
                _ => string.Empty
            };
        if (row.Kind == V15BuildingKind.School)
            return "Forma ricercatori; i punti ricerca si accumulano e vengono spesi su richiesta.";
        var definition = _baseline.Catalog.BuildingsById[row.SourceId];
        if (BuildingIsFixed(row)) return "Centro iniziale della colonia. Non può essere costruito nuovamente.";
        if (row.Kind == V15BuildingKind.Producer)
            return $"{row.Name}. {row.Tier switch { ProductionTier.Large => 3, ProductionTier.Advanced => 10, _ => 1 }} posti per edificio; ogni addetto segue la ricetta della famiglia.";
        if (definition.HousingPerBuilding > 0)
            return $"Aggiunge {definition.HousingPerBuilding} posti abitativi per struttura.";
        return "Edificio civile della colonia.";
    }

    private IReadOnlyList<ResourceDetailLine> BuildingOutputLines(V15BuildingRow row)
    {
        if (row.Kind == V15BuildingKind.Producer)
            return Lines(_baseline.Catalog.BuildingsById[row.SourceId].Recipe!.Output, " /s");
        if (row.Kind != V15BuildingKind.FoodIndustry) return [];
        return row.SourceId switch
        {
            "stable" => [Line(_meat.CattleResourceId, _meat.CattlePerBatch.Units, " /s")],
            "hunter" => [Line(_meat.GameResourceId, _meat.GamePerHuntBatch.Units, " /s")],
            "butcher" => _meat.ButcherRecipes.Select(r => Line(_meat.MeatResourceId, r.MeatPerBatch.Units,
                $" /s (da {ResourceName(r.AnimalResourceId)})")).ToArray(),
            "garden" => [Line(_crops.Garden.CropResourceId, _crops.Garden.CropPerBatch.Units, " /s")],
            "orchard" => [Line(_crops.Orchard.CropResourceId, _crops.Orchard.CropPerBatch.Units, " /s")],
            "food-depot" => _depot.Recipes.Select(r => Line(_depot.ReadyFoodResourceId, r.FoodPerBatch.Units,
                $" /s (da {ResourceName(r.SourceResourceId)})")).ToArray(),
            _ => []
        };
    }

    private IReadOnlyList<ResourceDetailLine> BuildingInputLines(V15BuildingRow row)
    {
        if (row.Kind == V15BuildingKind.Producer)
            return Lines(_baseline.Catalog.BuildingsById[row.SourceId].Recipe!.Input, " /s");
        if (row.Kind != V15BuildingKind.FoodIndustry) return [];
        return row.SourceId switch
        {
            "stable" => Lines(_meat.FeedPerCattleBatch, " /s"),
            "hunter" => [],
            "butcher" => _meat.ButcherRecipes.Select(r => Line(r.AnimalResourceId, r.AnimalsPerBatch.Units, " /s (alternativa)")).ToArray(),
            "garden" => _crops.Garden.WaterPerBatch == ResourceAmount.Zero ? [] :
                [Line(_crops.WaterResourceId, _crops.Garden.WaterPerBatch.Units, " /s")],
            "orchard" => _crops.Orchard.WaterPerBatch == ResourceAmount.Zero ? [] :
                [Line(_crops.WaterResourceId, _crops.Orchard.WaterPerBatch.Units, " /s")],
            "food-depot" => _depot.Recipes.Select(r => Line(r.SourceResourceId, r.InputPerBatch.Units, " /s (alternativa)")).ToArray(),
            _ => []
        };
    }

    private IReadOnlyList<ResourceDetailLine> BuildingCapacityLines(V15BuildingRow row)
    {
        if (row.Kind == V15BuildingKind.FoodIndustry)
        {
            var cap = _foodCatalog.Sites.Single(site => site.Id == row.SourceId).StoragePerBuilding;
            IEnumerable<ResourceDetailLine> ready = cap.DepotReady == ResourceAmount.Zero
                ? Array.Empty<ResourceDetailLine>()
                : new[] { Line(_depot.ReadyFoodResourceId, cap.DepotReady.Units) };
            return Lines(cap.Animals + cap.Transit + cap.DepotRaw).Concat(ready).ToArray();
        }
        if (row.Kind == V15BuildingKind.School)
            return [new ResourceDetailLine("population", "1 posto formazione e 2 posti ricercatori per Scuola"),
                Line(V14SchoolSupplies.ResourceId, V14SchoolSupplies.SchoolPencilStorage)];
        var building = _baseline.Catalog.BuildingsById[row.SourceId];
        var lines = row.Kind == V15BuildingKind.Producer
            ? Lines(ResourceBundle.Create(building.Recipe!.Output.Amounts.Keys.Select(id =>
                (id, row.Tier switch { ProductionTier.Large => 150m, ProductionTier.Advanced => 500m, _ => 50m })).ToArray())).ToList()
            : Lines(building.StoragePerBuilding ?? ResourceBundle.Empty).ToList();
        if (row.Kind == V15BuildingKind.Civic && row.SourceId is "town-hall" or "warehouse")
        {
            var pencils = (building.StoragePerBuilding ?? ResourceBundle.Empty)["planks"].Units;
            if (pencils > 0) lines.Add(Line(V14SchoolSupplies.ResourceId, pencils));
        }
        if (building.HousingPerBuilding > 0)
            lines.Add(new ResourceDetailLine("population", $"{building.HousingPerBuilding} posti abitativi"));
        return lines;
    }

    private string BuildingRequirements(V15BuildingRow row, BuildQuote quote)
    {
        if (row.Kind == V15BuildingKind.FoodIndustry)
        {
            var site = _foodCatalog.Sites.Single(s => s.Id == row.SourceId);
            var missing = site.Prerequisites.Where(id => !HasConstructed(id))
                .Select(id => _foodCatalog.TryGet(id, out var other) && other is not null
                    ? other.Name : _baseline.Catalog.BuildingsById.TryGetValue(id, out var building)
                        ? building.Name : id).ToArray();
            return missing.Length == 0 ? string.Empty : "Per costruire servono prima: " + string.Join(", ", missing);
        }
        if (row.Kind == V15BuildingKind.School && !HasConstructed("food-depot"))
            return "Per costruire serve prima: Deposito alimentare";
        if (row.Kind == V15BuildingKind.Producer && row.Tier is { } tier && tier != ProductionTier.Base)
        {
            var tiers = V14TierConstructionRules.Candidate;
            var family = TieredProductionCatalog.FromCatalog(_baseline.Catalog, tiers.LargeMultiplier,
                tiers.AdvancedMultiplier).FirstOrDefault(item => item.FamilyId == row.SourceId);
            if (family is not null)
            {
                var unlocked = _session.Snapshot().CityCampaign.UnlockedTechnologyIds;
                var missing = family.Get(tier).RequiredTechnologies.Where(id => !unlocked.Contains(id))
                    .Select(id => _technologies.Technologies.FirstOrDefault(t => t.Id == id)?.Name ?? id).ToArray();
                if (missing.Length > 0) return "Per costruire serve ricerca: " + string.Join(", ", missing);
            }
        }
        if (!quote.Unlocked && !BuildingIsFixed(row)) return "Per costruire servono prima gli edifici o le tecnologie richieste.";
        return string.Empty;
    }

    private bool HasConstructed(string id)
    {
        var checkpoint = _session.Snapshot();
        if (id == "school") return checkpoint.CityCampaign.SchoolCount > 0;
        if (FoodIndustrySites.Ids.Contains(id))
            return FoodIndustrySites.Get(checkpoint.Industry.Buildings.Counts, id) > 0;
        return checkpoint.CityCampaign.OtherBuildings.Any(x => x.BuildingId == id && x.Count > 0) ||
            checkpoint.CityCampaign.ProductiveFamilies.Any(x => x.FamilyId == id &&
                (long)x.Base.Buildings + x.Large.Buildings + x.Advanced.Buildings > 0);
    }

    private string BuildDiagnosis(V15BuildingRow row, BuildQuote quote)
    {
        if (!quote.Unlocked || quote.Affordable || BuildingIsFixed(row)) return string.Empty;
        var resources = _session.Snapshot().CityCampaign.Resources;
        var missing = quote.Cost.Amounts.Where(kv => resources[kv.Key].Milliunits < kv.Value.Milliunits)
            .Select(kv => $"{ResourceName(kv.Key)} " +
                $"({resources[kv.Key].Units.ToString("0.###", Italian)} / {kv.Value.Units.ToString("0.###", Italian)})")
            .ToArray();
        return missing.Length > 0 ? "Spazio o risorse insufficienti. Mancano: " + string.Join(" · ", missing)
            : "Edificio non costruibile: " + ItalianReason(quote.Status);
    }
    private ResourceBundle FallbackQuote(V15BuildingRow row, int quantity)
    {
        if (BuildingIsFixed(row)) return ResourceBundle.Empty;
        var checkpoint = _session.Snapshot().CityCampaign;
        var purchased = checkpoint.PurchaseCounters.First(c => c.FamilyId == row.SourceId).Acquired;
        if (row.Kind == V15BuildingKind.Producer && row.Tier is { } tier)
        {
            var rules = V14TierConstructionRules.Candidate;
            var family = TieredProductionCatalog.FromCatalog(_baseline.Catalog, rules.LargeMultiplier,
                rules.AdvancedMultiplier).Single(x => x.FamilyId == row.SourceId);
            return ProgressiveConstructionPricing.QuoteNewBuildings(family.Source.Cost,
                family.Get(tier).CostMultiplier, rules.GrowthFactor, purchased, quantity);
        }
        if (row.Kind == V15BuildingKind.Civic)
            return ProgressiveConstructionPricing.QuoteNewBuildings(
                _baseline.Catalog.BuildingsById[row.SourceId].Cost, 1m,
                V14TierConstructionRules.Candidate.GrowthFactor, purchased, quantity);
        if (row.Kind == V15BuildingKind.School)
            return ProgressiveConstructionPricing.QuoteNewBuildings(
                V14SchoolConstructionRules.Candidate.Cost, 1m,
                V14SchoolConstructionRules.Candidate.GrowthFactor, purchased, quantity);
        return ResourceBundle.Empty;
    }

}
