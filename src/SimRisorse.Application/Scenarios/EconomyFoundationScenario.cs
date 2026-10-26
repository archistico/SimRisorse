using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;

namespace SimRisorse.Application.Scenarios;

/// <summary>Small reference content; military and final victory are outside this scenario.</summary>
public static class EconomyFoundationScenario
{
    public static GameScenario Create()
    {
        var catalog = new GameCatalog("economy-foundation-1",
            [new("logs", "Tronchi"), new("planks", "Legno lavorato"), new("stone", "Pietra")],
            [
                new("tree-feller", "Taglialegna", ResourceBundle.Create(("planks", 20m), ("stone", 10m)), 1,
                    new(ResourceBundle.Empty, ResourceBundle.Create(("logs", 2m))), 10, []),
                new("sawmill", "Segheria", ResourceBundle.Create(("planks", 40m), ("stone", 20m)), 1,
                    new(ResourceBundle.Create(("logs", 2m)), ResourceBundle.Create(("planks", 1m))), 20, ["tree-feller"]),
                new("stonecutter", "Tagliapietra", ResourceBundle.Create(("planks", 30m), ("stone", 10m)), 1,
                    new(ResourceBundle.Empty, ResourceBundle.Create(("stone", 1m))), 30, [])
            ]);
        return new("economy-foundation", catalog, 8,
            ResourceBundle.Create(("logs", 1000m), ("planks", 1000m), ("stone", 1000m)),
            ResourceBundle.Create(("planks", 120m), ("stone", 80m)),
            [new("tree-feller", 1, 1), new("sawmill", 1, 1), new("stonecutter", 1, 1)]);
    }
}
