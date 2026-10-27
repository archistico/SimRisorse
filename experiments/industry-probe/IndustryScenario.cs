using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.IndustryProbe;

internal static class IndustryScenario
{
    // Hypotheses from the M5.1 dossier, scoped exclusively to this executable.
    internal static GameScenario Create(bool expensive)
    {
        var civil = CivilGrowthScenario.Create();
        var multiplier = expensive ? 1.5m : 1m;
        ResourceBundle Cost(decimal planks, decimal stone) =>
            ResourceBundle.Create(("planks", planks * multiplier), ("stone", stone * multiplier));
        ResourceDefinition[] resources =
        [
            .. civil.Catalog.Resources,
            new("iron-ore", "Minerale di ferro"), new("coal", "Carbone"), new("iron", "Ferro"),
            new("gold-ore", "Minerale d'oro"), new("gold", "Oro")
        ];
        var storage = ResourceBundle.Create([.. resources.Select(item => (item.Id, 500m))]);
        var catalog = new GameCatalog(expensive ? "industry-hypothesis-b-1" : "industry-hypothesis-a-1", resources,
        [
            .. civil.Catalog.Buildings.Select(item => item.Id == "warehouse" ? item with { StoragePerBuilding = storage } : item),
            new("iron-mine", "Miniera di ferro", Cost(120m, 80m), 1,
                new(ResourceBundle.Empty, ResourceBundle.Create(("iron-ore", 2m))), 80, ["large-house"]),
            new("coal-mine", "Miniera di carbone", Cost(130m, 90m), 1,
                new(ResourceBundle.Empty, ResourceBundle.Create(("coal", 2m))), 81, ["large-house"]),
            new("iron-smelter", "Fonderia di ferro", Cost(200m, 150m), 1,
                new(ResourceBundle.Create(("iron-ore", 2m), ("coal", 1m)), ResourceBundle.Create(("iron", 1m))),
                90, ["iron-mine", "coal-mine"]),
            new("gold-mine", "Miniera d'oro", Cost(180m, 120m), 1,
                new(ResourceBundle.Empty, ResourceBundle.Create(("gold-ore", 1m))), 82, ["large-house"]),
            new("gold-smelter", "Fonderia d'oro", Cost(220m, 170m), 1,
                new(ResourceBundle.Create(("gold-ore", 1m), ("coal", 1m)), ResourceBundle.Create(("gold", 1m))),
                91, ["gold-mine", "coal-mine"])
        ]);
        BuildingState[] initial =
        [
            new("tree-feller", 1, 1), new("sawmill", 1, 1), new("stonecutter", 1, 1),
            new("small-house", 1, 0), new("large-house", 1, 0),
            new("fisher", 1, 1), new("food-store", 1, 1),
            new("farm", 2, 2), new("well", 1, 1), new("mill", 1, 1), new("bakery", 1, 1)
        ];
        return new("industry-probe", catalog, 40,
            civil.Capacity + ResourceBundle.Create(("iron-ore", 1000m), ("coal", 1000m), ("iron", 1000m),
                ("gold-ore", 1000m), ("gold", 1000m)),
            ResourceBundle.Create(("food", 100m), ("bread", 100m)), initial, civil.PopulationRules);
    }
}
