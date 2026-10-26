using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Scenarios;

/// <summary>A declared conversion between consecutive rules versions: only new building types, starting at zero.</summary>
public sealed record RulesChange(string From, string To, ImmutableArray<string> AddedBuildings);

/// <summary>Civilian progression slices. Values are provisional scenario rules.</summary>
public static class CivilGrowthScenario
{
    public const string Id = "civil-growth";
    public const string RulesVersion = "civil-growth-3";

    /// <summary>Every earlier rules version that can still be loaded, oldest first.</summary>
    public static ImmutableArray<RulesChange> Changes { get; } =
    [
        new("civil-growth-1", "civil-growth-2", ["warehouse"]),
        new("civil-growth-2", RulesVersion, ["farm", "well", "mill", "bakery"])
    ];

    public static GameScenario Create()
    {
        var foundation = EconomyFoundationScenario.Create();
        ResourceDefinition[] resources =
        [
            .. foundation.Catalog.Resources, new("fish", "Pesce"), new("food", "Cibo"),
            new("grain", "Grano"), new("water", "Acqua"), new("flour", "Farina")
        ];
        var catalog = new GameCatalog(RulesVersion, resources,
            [
                .. foundation.Catalog.Buildings,
                new("small-house", "Casa piccola", ResourceBundle.Create(("planks", 80m), ("stone", 40m)), 0,
                    null, 35, [], HousingPerBuilding: 8),
                new("warehouse", "Magazzino", ResourceBundle.Create(("planks", 100m), ("stone", 60m)), 0,
                    null, 37, [], StoragePerBuilding: ResourceBundle.Create([.. resources.Select(item => (item.Id, 500m))])),
                new("fisher", "Pescatore", ResourceBundle.Create(("planks", 40m), ("stone", 20m)), 1,
                    new(ResourceBundle.Empty, ResourceBundle.Create(("fish", 1m))), 40, []),
                new("food-store", "Deposito alimentare", ResourceBundle.Create(("planks", 60m), ("stone", 30m)), 1,
                    new(ResourceBundle.Create(("fish", 1m)), ResourceBundle.Create(("food", 1m))), 50, ["fisher"]),
                new("farm", "Fattoria", ResourceBundle.Create(("planks", 60m), ("stone", 30m)), 1,
                    new(ResourceBundle.Empty, ResourceBundle.Create(("grain", 1m))), 55, []),
                new("well", "Pozzo", ResourceBundle.Create(("planks", 30m), ("stone", 40m)), 1,
                    new(ResourceBundle.Empty, ResourceBundle.Create(("water", 1m))), 56, []),
                new("mill", "Mulino", ResourceBundle.Create(("planks", 120m), ("stone", 80m)), 1,
                    new(ResourceBundle.Create(("grain", 2m)), ResourceBundle.Create(("flour", 1m))), 60, ["farm"]),
                new("bakery", "Forno", ResourceBundle.Create(("planks", 150m), ("stone", 100m)), 1,
                    new(ResourceBundle.Create(("flour", 1m), ("water", 1m)), ResourceBundle.Create(("food", 3m))), 70, ["mill", "well"])
            ]);
        return new(Id, catalog, 8,
            foundation.Capacity + ResourceBundle.Create(("fish", 1000m), ("food", 1000m), ("grain", 1000m), ("water", 1000m), ("flour", 1000m)),
            foundation.InitialResources + ResourceBundle.Create(("food", 30m)),
            [.. foundation.InitialBuildings, new("small-house", 1, 0)],
            new PopulationRules("food", ResourceAmount.FromUnits(0.010m), 60));
    }

    /// <summary>
    /// Explicit one-step conversion of an older checkpoint: the declared new building types start at zero.
    /// Stocks, people, time and growth progress are carried over unchanged. Null when no step is declared.
    /// </summary>
    internal static GameSnapshot? UpgradeOneStep(GameSnapshot snapshot)
    {
        var change = Changes.FirstOrDefault(item => item.From == snapshot.RulesVersion);
        if (change is null || snapshot.Buildings.IsDefault) return null;
        return snapshot with
        {
            RulesVersion = change.To,
            Buildings = [.. snapshot.Buildings, .. change.AddedBuildings.Select(id => new BuildingState(id, 0, 0))]
        };
    }
}
