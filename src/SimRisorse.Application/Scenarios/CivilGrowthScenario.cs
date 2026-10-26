using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;

namespace SimRisorse.Application.Scenarios;

/// <summary>First civilian progression slice. Values are provisional scenario rules.</summary>
public static class CivilGrowthScenario
{
    public static GameScenario Create()
    {
        var foundation = EconomyFoundationScenario.Create();
        var catalog = new GameCatalog("civil-growth-1",
            [.. foundation.Catalog.Resources, new("fish", "Pesce"), new("food", "Cibo")],
            [
                .. foundation.Catalog.Buildings,
                new("small-house", "Casa piccola", ResourceBundle.Create(("planks", 80m), ("stone", 40m)), 0,
                    null, 35, [], HousingPerBuilding: 8),
                new("fisher", "Pescatore", ResourceBundle.Create(("planks", 40m), ("stone", 20m)), 1,
                    new(ResourceBundle.Empty, ResourceBundle.Create(("fish", 1m))), 40, []),
                new("food-store", "Deposito alimentare", ResourceBundle.Create(("planks", 60m), ("stone", 30m)), 1,
                    new(ResourceBundle.Create(("fish", 1m)), ResourceBundle.Create(("food", 1m))), 50, ["fisher"])
            ]);
        return new("civil-growth", catalog, 8,
            foundation.Capacity + ResourceBundle.Create(("fish", 1000m), ("food", 1000m)),
            foundation.InitialResources + ResourceBundle.Create(("food", 30m)),
            [.. foundation.InitialBuildings, new("small-house", 1, 0)],
            new PopulationRules("food", ResourceAmount.FromUnits(0.010m), 60));
    }
}
