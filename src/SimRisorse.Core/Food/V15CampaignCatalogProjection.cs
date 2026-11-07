using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Core.Food;

/// <summary>Player-facing v15 building identities. The historical food-store is
/// neither constructible nor shown here; ordinary v15 checkpoints omit it.</summary>
public enum V15BuildingKind { Civic, Producer, FoodIndustry, School }

public sealed record V15BuildingEntry(string Id, string Name, V15BuildingKind Kind,
    string SourceId, ProductionTier? Tier, long Owned, long AssignedWorkers,
    int CrewSize = 1, long CompleteModules = 0);

/// <summary>Single resource row across the city, animal pens, transit and the depot.
/// Totals never count an inventory twice. Capacities are sums of the independent bins.</summary>
public sealed record V15CapacityContribution(string Source, ResourceAmount Amount);

public sealed record V15ResourceEntry(string Id, string Name,
    ResourceAmount Quantity, ResourceAmount Capacity,
    ImmutableArray<V15CapacityContribution> CapacitySources = default);

public sealed record V15CampaignCatalogView(ImmutableArray<V15BuildingEntry> Buildings,
    ImmutableArray<V15ResourceEntry> Resources);

public static class V15CampaignCatalogProjection
{
    public static V15CampaignCatalogView Project(GameScenario baseline,
        V15FoodCampaignCheckpoint checkpoint, FoodIndustryConstructionCatalog foodCatalog)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(checkpoint);
        ArgumentNullException.ThrowIfNull(foodCatalog);
        var city = checkpoint.CityCampaign;
        var rows = ImmutableArray.CreateBuilder<V15BuildingEntry>();
        foreach (var definition in baseline.Catalog.Buildings.OrderBy(b => b.Name, StringComparer.Ordinal))
        {
            if (definition.Id == V15UnifiedCampaignClock.DisabledLegacyProducerId) continue;
            var family = city.ProductiveFamilies.FirstOrDefault(f => f.FamilyId == definition.Id);
            if (family is not null)
            {
                foreach (var tier in new[] { ProductionTier.Base, ProductionTier.Large, ProductionTier.Advanced })
                {
                    var occupancy = tier switch
                    {
                        ProductionTier.Base => family.Base,
                        ProductionTier.Large => family.Large,
                        _ => family.Advanced
                    };
                    var suffix = tier switch
                    {
                        ProductionTier.Base => string.Empty,
                        ProductionTier.Large => "-large",
                        _ => "-advanced"
                    };
                    var label = tier switch
                    {
                        ProductionTier.Base => definition.Name,
                        ProductionTier.Large => definition.Name + " grande",
                        _ => definition.Name + " avanzato"
                    };
                    var crewSize = V15CrewRules.CrewSize(checkpoint.CrewMode, definition.Id);
                    rows.Add(new(definition.Id + suffix, label, V15BuildingKind.Producer,
                        definition.Id, tier, occupancy.Buildings, occupancy.AssignedWorkers,
                        crewSize, occupancy.AssignedWorkers / crewSize));
                }
            }
            else
            {
                var own = city.OtherBuildings.Single(b => b.BuildingId == definition.Id);
                rows.Add(new(definition.Id, definition.Name, V15BuildingKind.Civic,
                    definition.Id, null, own.Count, own.AssignedWorkers));
            }
        }
        rows.Add(new(V14CampaignSnapshots.SchoolId, "Scuola", V15BuildingKind.School,
            V14CampaignSnapshots.SchoolId, null, city.SchoolCount, 0));
        foreach (var site in foodCatalog.Sites)
        {
            rows.Add(new(site.Id, site.Name, V15BuildingKind.FoodIndustry, site.Id, null,
                FoodIndustrySites.Get(checkpoint.Industry.Buildings.Counts, site.Id),
                FoodIndustrySites.Get(checkpoint.Industry.Buildings.Assigned, site.Id)));
        }
        rows.Add(new("food-depot-large", "Deposito alimentare grande", V15BuildingKind.FoodIndustry,
            "food-depot-large", ProductionTier.Large, checkpoint.Industry.Buildings.Counts.DepotLarge,
            checkpoint.Industry.Buildings.Assigned.DepotLarge));
        rows.Add(new("food-depot-advanced", "Deposito alimentare avanzato", V15BuildingKind.FoodIndustry,
            "food-depot-advanced", ProductionTier.Advanced, checkpoint.Industry.Buildings.Counts.DepotAdvanced,
            checkpoint.Industry.Buildings.Assigned.DepotAdvanced));

        var cityCap = V15CityStorage.Calculate(baseline, city);
        var foodCap = foodCatalog.CapacitiesFor(checkpoint.Industry.Buildings);
        var labels = baseline.Catalog.Resources.ToDictionary(r => r.Id, r => r.Name, StringComparer.Ordinal);
        labels[V14SchoolSupplies.ResourceId] = "Matite";
        labels["cattle"] = "Bovini";
        labels["game"] = "Selvaggina";
        labels["meat"] = "Carne";
        labels["fruit"] = "Frutta";
        labels["vegetables"] = "Verdura";
        var resources = labels.Select(item =>
        {
            var id = item.Key;
            var quantity = city.Resources[id] + checkpoint.Industry.Animals[id] +
                checkpoint.Industry.Transit[id] + checkpoint.Depot.RawFood[id];
            var capacity = cityCap[id] + foodCap.Animals[id] + foodCap.Transit[id] +
                foodCap.DepotRaw[id];
            if (id == "food")
            {
                quantity = checkpoint.Depot.ReadyFood;
                capacity = foodCap.DepotReady;
            }
            // Name the physical owner of each capacity once; the ready-food
            // inventory belongs exclusively to the new depot.
            var sources = id == "food"
                ? ImmutableArray.Create(new V15CapacityContribution("Deposito · Cibo pronto", foodCap.DepotReady))
                : new[]
                {
                    new V15CapacityContribution("Città / edifici", cityCap[id]),
                    new V15CapacityContribution("Recinti animali", foodCap.Animals[id]),
                    new V15CapacityContribution("Lavorazioni intermedie", foodCap.Transit[id]),
                    new V15CapacityContribution("Deposito · ingredienti", foodCap.DepotRaw[id])
                }.Where(source => source.Amount != ResourceAmount.Zero).ToImmutableArray();
            return new V15ResourceEntry(id, item.Value, quantity, capacity, sources);
        }).OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.Id, StringComparer.Ordinal).ToImmutableArray();
        return new(rows.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.Id, StringComparer.Ordinal).ToImmutableArray(), resources);
    }
}
