using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Scenarios;

/// <summary>A declared conversion between consecutive rules versions, including new building types starting at zero.</summary>
public sealed record RulesChange(string From, string To, ImmutableArray<string> AddedBuildings);

/// <summary>Civilian progression slices. Values are provisional scenario rules.</summary>
public static class CivilGrowthScenario
{
    public const string Id = "civil-growth";
    public const string RulesVersion = "civil-growth-7";

    /// <summary>Every earlier rules version that can still be loaded, oldest first.</summary>
    public static ImmutableArray<RulesChange> Changes { get; } =
    [
        new("civil-growth-1", "civil-growth-2", ["warehouse"]),
        new("civil-growth-2", "civil-growth-3", ["farm", "well", "mill", "bakery"]),
        // The bakery's output changes from generic food to the new bread resource.
        // Old generic food stocks remain generic food: their provenance is unknown.
        new("civil-growth-3", "civil-growth-4", ["large-house"]),
        new("civil-growth-4", "civil-growth-5", []),
        new("civil-growth-5", "civil-growth-6", ["iron-mine", "coal-mine", "iron-smelter"]),
        new("civil-growth-6", RulesVersion, ["gold-mine", "gold-smelter"])
    ];

    public static GameScenario Create() => CreateCore(RulesVersion, accelerated: true, industry: true, gold: true);

    /// <summary>Historical rules, retained for comparing the already published M4 benchmarks.</summary>
    public static GameScenario CreateLegacyForBenchmarks() => CreateCore("civil-growth-4", accelerated: false);

    /// <summary>The M4 accelerated baseline stays fixed when later content is added.</summary>
    public static GameScenario CreateAcceleratedForBenchmarks() => CreateCore("civil-growth-5", accelerated: true);

    /// <summary>The first iron industry remains available for compatibility checks.</summary>
    public static GameScenario CreateIronForBenchmarks() => CreateCore("civil-growth-6", accelerated: true, industry: true);

    private static GameScenario CreateCore(string rulesVersion, bool accelerated, bool industry = false, bool gold = false)
    {
        var foundation = EconomyFoundationScenario.Create();
        ResourceDefinition[] industrialResources = industry
            ? [new("iron-ore", "Minerale di ferro"), new("coal", "Carbone"), new("iron", "Ferro")]
            : [];
        ResourceDefinition[] goldResources = gold
            ? [new("gold-ore", "Minerale d'oro"), new("gold", "Oro")]
            : [];
        BuildingDefinition[] industrialBuildings = industry
            ? [
                new("iron-mine", "Miniera di ferro", ResourceBundle.Create(("planks", 120m), ("stone", 80m)), 1,
                    new(ResourceBundle.Empty, ResourceBundle.Create(("iron-ore", 2m))), 80, ["large-house"]),
                new("coal-mine", "Miniera di carbone", ResourceBundle.Create(("planks", 130m), ("stone", 90m)), 1,
                    new(ResourceBundle.Empty, ResourceBundle.Create(("coal", 2m))), 81, ["large-house"]),
                new("iron-smelter", "Fonderia di ferro", ResourceBundle.Create(("planks", 200m), ("stone", 150m)), 1,
                    new(ResourceBundle.Create(("iron-ore", 2m), ("coal", 1m)), ResourceBundle.Create(("iron", 1m))),
                    90, ["iron-mine", "coal-mine"])
            ]
            : [];
        BuildingDefinition[] goldBuildings = gold
            ? [
                new("gold-mine", "Miniera d'oro", ResourceBundle.Create(("planks", 180m), ("stone", 120m)), 1,
                    new(ResourceBundle.Empty, ResourceBundle.Create(("gold-ore", 1m))), 82, ["large-house"]),
                new("gold-smelter", "Fonderia d'oro", ResourceBundle.Create(("planks", 220m), ("stone", 170m)), 1,
                    new(ResourceBundle.Create(("gold-ore", 1m), ("coal", 1m)), ResourceBundle.Create(("gold", 1m))),
                    91, ["gold-mine", "coal-mine"])
            ]
            : [];
        ResourceDefinition[] resources =
        [
            .. foundation.Catalog.Resources, new("fish", "Pesce"), new("food", "Cibo"),
            new("grain", "Grano"), new("water", "Acqua"), new("flour", "Farina"), new("bread", "Pane"),
            .. industrialResources, .. goldResources
        ];
        var catalog = new GameCatalog(rulesVersion, resources,
            [
                .. foundation.Catalog.Buildings,
                new("small-house", "Casa piccola", ResourceBundle.Create(("planks", 80m), ("stone", 40m)), 0,
                    null, 35, [], HousingPerBuilding: 8),
                new("large-house", "Casa grande", ResourceBundle.Create(("planks", 160m), ("stone", 80m)), 0,
                    null, 36, ["bakery"], HousingPerBuilding: 32),
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
                    new(ResourceBundle.Create(("flour", 1m), ("water", 1m)), ResourceBundle.Create(("bread", 3m))), 70, ["mill", "well"]),
                .. industrialBuildings, .. goldBuildings
            ]);
        return new(Id, catalog, 8,
            foundation.Capacity + ResourceBundle.Create(("fish", 1000m), ("food", 1000m), ("grain", 1000m),
                ("water", 1000m), ("flour", 1000m), ("bread", 100m)) +
                ResourceBundle.Create([.. industrialResources.Concat(goldResources).Select(item => (item.Id, 1000m))]),
            foundation.InitialResources + ResourceBundle.Create(("food", 30m)),
            [.. foundation.InitialBuildings, new("small-house", 1, 0)],
            new PopulationRules("food", ResourceAmount.FromUnits(0.010m), 60,
                new AdvancedHousingRules("small-house", "bread", ResourceAmount.FromUnits(0.015m)),
                accelerated ? new HousingGrowthRules("small-house", "large-house") : null));
    }

    /// <summary>
    /// Explicit one-step conversion of an older checkpoint: the declared new building types start at zero.
    /// Stocks, people, time and both parts of growth progress are carried over unchanged.
    /// Null when no step is declared or a checkpoint claims stocks introduced by later rules.
    /// A duplicate building in an older checkpoint is rejected by snapshot validation after conversion.
    /// </summary>
    internal static GameSnapshot? UpgradeOneStep(GameSnapshot snapshot)
    {
        var change = Changes.FirstOrDefault(item => item.From == snapshot.RulesVersion);
        if (change is null || snapshot.Buildings.IsDefault || snapshot.Resources is null) return null;
        if (change.From == "civil-growth-3" && snapshot.Resources["bread"] != ResourceAmount.Zero)
            return null;
        if (change.From == "civil-growth-5" && (snapshot.Resources["iron-ore"] != ResourceAmount.Zero ||
            snapshot.Resources["coal"] != ResourceAmount.Zero || snapshot.Resources["iron"] != ResourceAmount.Zero))
            return null;
        if (change.From == "civil-growth-6" && (snapshot.Resources["gold-ore"] != ResourceAmount.Zero || snapshot.Resources["gold"] != ResourceAmount.Zero))
            return null;
        // Rules -3 -> -4 changes the bakery recipe, not already stored stock.
        // Never guess whether old food originally came from the bakery or the fish chain.
        return snapshot with
        {
            RulesVersion = change.To,
            Buildings = [.. snapshot.Buildings, .. change.AddedBuildings.Select(id => new BuildingState(id, 0, 0))]
        };
    }
}
