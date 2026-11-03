using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Population;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Scenarios;

/// <summary>A declared conversion between consecutive rules versions, including new building types starting at zero.</summary>
public sealed record RulesChange(string From, string To, ImmutableArray<string> AddedBuildings);

/// <summary>Civilian progression slices. Values are provisional scenario rules.</summary>
public static class CivilGrowthScenario
{
    public const string Id = "civil-growth";
    public const string RulesVersion = "civil-growth-13";

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
        new("civil-growth-6", "civil-growth-7", ["gold-mine", "gold-smelter"]),
        new("civil-growth-7", "civil-growth-8", ["forge"]),
        new("civil-growth-8", "civil-growth-9", ["mint"]),
        new("civil-growth-9", "civil-growth-10", ["barracks"]),
        // M5.3 remains unchanged; v11 only equalizes bread's initial storage with other resources.
        new("civil-growth-10", "civil-growth-11", []),
        // The Town Hall exists in every v12 colony, including converted checkpoints.
        new("civil-growth-11", "civil-growth-12", ["town-hall"]),
        new("civil-growth-12", RulesVersion, [])
    ];

    public static GameScenario Create() => CreateCore(RulesVersion, accelerated: true, industry: true, gold: true,
        forge: true, mint: true, barracks: true, uniformBreadCapacity: true, distributedStorage: true, individualMilitary: true);

    /// <summary>Frozen v12 baseline with Municipio, group training and distributed storage.</summary>
    public static GameScenario CreateTownHallForBenchmarks() => CreateCore("civil-growth-12", accelerated: true,
        industry: true, gold: true, forge: true, mint: true, barracks: true,
        uniformBreadCapacity: true, distributedStorage: true);

    /// <summary>Frozen v11 capacity rules, used for reproducible upgrades and regression tests.</summary>
    public static GameScenario CreateUniformStorageForBenchmarks() => CreateCore("civil-growth-11", accelerated: true,
        industry: true, gold: true, forge: true, mint: true, barracks: true, uniformBreadCapacity: true);

    /// <summary>Frozen v10 rules: military state is present, bread starts at 100 storage units.</summary>
    public static GameScenario CreateBarracksForBenchmarks() => CreateCore("civil-growth-10", accelerated: true, industry: true, gold: true, forge: true, mint: true, barracks: true);

    /// <summary>Frozen M5.2 catalog for reproducible mint benchmarks and v9 migration.</summary>
    public static GameScenario CreateMintForBenchmarks() => CreateCore("civil-growth-9", accelerated: true, industry: true, gold: true, forge: true, mint: true);

    /// <summary>Frozen M5.2a catalog for v8 save migration and forge-specific regression tests.</summary>
    public static GameScenario CreateForgeForBenchmarks() => CreateCore("civil-growth-8", accelerated: true, industry: true, gold: true, forge: true);

    /// <summary>Frozen M5.1b/c catalog, used for historical coal contention benchmarks and v7 save migration tests.</summary>
    public static GameScenario CreateGoldForBenchmarks() => CreateCore("civil-growth-7", accelerated: true, industry: true, gold: true);

    /// <summary>Historical rules, retained for comparing the already published M4 benchmarks.</summary>
    public static GameScenario CreateLegacyForBenchmarks() => CreateCore("civil-growth-4", accelerated: false);

    /// <summary>The M4 accelerated baseline stays fixed when later content is added.</summary>
    public static GameScenario CreateAcceleratedForBenchmarks() => CreateCore("civil-growth-5", accelerated: true);

    /// <summary>The first iron industry remains available for compatibility checks.</summary>
    public static GameScenario CreateIronForBenchmarks() => CreateCore("civil-growth-6", accelerated: true, industry: true);

    private static GameScenario CreateCore(string rulesVersion, bool accelerated, bool industry = false, bool gold = false,
        bool forge = false, bool mint = false, bool barracks = false, bool uniformBreadCapacity = false,
        bool distributedStorage = false, bool individualMilitary = false)
    {
        var foundation = EconomyFoundationScenario.Create();
        ResourceDefinition[] industrialResources = industry
            ? [new("iron-ore", "Minerale di ferro"), new("coal", "Carbone"), new("iron", "Ferro")]
            : [];
        ResourceDefinition[] goldResources = gold
            ? [new("gold-ore", "Minerale d'oro"), new("gold", "Oro")]
            : [];
        ResourceDefinition[] forgedResources = forge ? [new("swords", "Spade")] : [];
        ResourceDefinition[] mintedResources = mint ? [new("coins", "Monete")] : [];
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
        BuildingDefinition[] forgedBuildings = forge
            ? [new("forge", "Forgia", ResourceBundle.Create(("planks", 240m), ("stone", 180m)), 1,
                new(ResourceBundle.Create(("iron", 1m), ("coal", 1m)), ResourceBundle.Create(("swords", 2m))),
                100, ["iron-smelter"])]
            : [];
        BuildingDefinition[] mintedBuildings = mint
            ? [new("mint", "Zecca", ResourceBundle.Create(("planks", 210m), ("stone", 160m)), 1,
                new(ResourceBundle.Create(("gold", 1m), ("coal", 1m)), ResourceBundle.Create(("coins", 3m))),
                101, ["gold-smelter"])]
            : [];
        BuildingDefinition[] militaryBuildings = barracks
            ? [new("barracks", "Caserma", (individualMilitary ? MilitaryTrainingRules.IndividualBarracks : MilitaryTrainingRules.SimpleBarracks).BarracksCost,
                0, null, 110, ["forge"])]
            : [];
        ResourceDefinition[] resources =
        [
            .. foundation.Catalog.Resources, new("fish", "Pesce"), new("food", "Cibo"),
            new("grain", "Grano"), new("water", "Acqua"), new("flour", "Farina"), new("bread", "Pane"),
            .. industrialResources, .. goldResources, .. forgedResources, .. mintedResources
        ];
        BuildingDefinition[] buildings =
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
                .. industrialBuildings, .. goldBuildings, .. forgedBuildings, .. mintedBuildings, .. militaryBuildings
            ];
        if (distributedStorage)
        {
            buildings =
            [
                new("town-hall", "Municipio", ResourceBundle.Empty, 0, null, 0, [],
                    StoragePerBuilding: ResourceBundle.Create([.. resources.Select(item => (item.Id, 1000m))]),
                    IsScenarioFixed: true),
                .. buildings.Select(WithLocalStorage)
            ];
        }
        var catalog = new GameCatalog(rulesVersion, resources, buildings);
        return new(Id, catalog, 8,
            distributedStorage ? ResourceBundle.Empty : foundation.Capacity + ResourceBundle.Create(("fish", 1000m), ("food", 1000m), ("grain", 1000m),
                ("water", 1000m), ("flour", 1000m), ("bread", uniformBreadCapacity ? 1000m : 100m)) +
                ResourceBundle.Create([.. industrialResources.Concat(goldResources).Concat(forgedResources).Concat(mintedResources).Select(item => (item.Id, 1000m))]),
            foundation.InitialResources + ResourceBundle.Create(("food", 30m)),
            [.. foundation.InitialBuildings, new("small-house", 1, 0),
                .. (distributedStorage ? new[] { new BuildingState("town-hall", 1, 0) } : Array.Empty<BuildingState>())],
            new PopulationRules("food", ResourceAmount.FromUnits(0.010m), 60,
                new AdvancedHousingRules("small-house", "bread", ResourceAmount.FromUnits(0.015m)),
                accelerated ? new HousingGrowthRules("small-house", "large-house") : null),
            barracks ? (individualMilitary ? MilitaryTrainingRules.IndividualBarracks : MilitaryTrainingRules.SimpleBarracks) : null);
    }

    /// <summary>Small on-site stores; all stock remains in the shared city inventory.</summary>
    private static BuildingDefinition WithLocalStorage(BuildingDefinition building)
    {
        if (building.Recipe is { } recipe)
            return building with
            {
                StoragePerBuilding = ResourceBundle.Create([.. recipe.Output.Amounts.Keys.Select(id => (id, 50m))])
            };
        return building.Id switch
        {
            "small-house" => building with { StoragePerBuilding = ResourceBundle.Create(("food", 10m), ("bread", 10m)) },
            "large-house" => building with { StoragePerBuilding = ResourceBundle.Create(("food", 25m), ("bread", 25m)) },
            "barracks" => building with { StoragePerBuilding = ResourceBundle.Create(("swords", 20m)) },
            _ => building
        };
    }

    /// <summary>
    /// Explicit one-step conversion of an older checkpoint: new buildings start at zero,
    /// except the permanent Town Hall introduced by v12, which starts at one.
    /// Stocks, people, time and both parts of growth progress are carried over unchanged.
    /// Null when no step is declared or a checkpoint claims stocks introduced by later rules.
    /// A duplicate building in an older checkpoint is rejected by snapshot validation after conversion.
    /// </summary>
    internal static GameSnapshot? UpgradeOneStep(GameSnapshot snapshot)
    {
        // A separate 03/11 branch reused the v10 identifier with an incompatible individual-training schema.
        // The loader distinguishes it by its concrete progress rows: never reinterpret it as historical v10.
        if (snapshot.RulesVersion == "civil-growth-10" && snapshot.Military?.HasIndividualProgress == true)
        {
            if (snapshot.Buildings.Any(item => item.BuildingId == "town-hall")) return null;
            return snapshot with
            {
                RulesVersion = RulesVersion,
                Buildings = [.. snapshot.Buildings, new BuildingState("town-hall", 1, 0)]
            };
        }
        var change = Changes.FirstOrDefault(item => item.From == snapshot.RulesVersion);
        if (change is null || snapshot.Buildings.IsDefault || snapshot.Resources is null) return null;
        if (change.From == "civil-growth-3" && snapshot.Resources["bread"] != ResourceAmount.Zero)
            return null;
        if (change.From == "civil-growth-5" && (snapshot.Resources["iron-ore"] != ResourceAmount.Zero ||
            snapshot.Resources["coal"] != ResourceAmount.Zero || snapshot.Resources["iron"] != ResourceAmount.Zero))
            return null;
        if (change.From == "civil-growth-6" && (snapshot.Resources["gold-ore"] != ResourceAmount.Zero || snapshot.Resources["gold"] != ResourceAmount.Zero))
            return null;
        if (change.From == "civil-growth-7" && snapshot.Resources["swords"] != ResourceAmount.Zero)
            return null;
        if (change.From == "civil-growth-8" && snapshot.Resources["coins"] != ResourceAmount.Zero)
            return null;
        if (change.From == "civil-growth-9" && snapshot.Military is not null)
            return null;
        MilitaryTrainingState? military = snapshot.Military;
        if (change.From == "civil-growth-12")
        {
            if (military is null) return null;
            var barracks = snapshot.Buildings.SingleOrDefault(item => item.BuildingId == "barracks");
            if (barracks is null) return null;
            military = IndividualRecruitment.UpgradeLegacy(military, barracks.Count);
        }
        // Rules -3 -> -4 changes the bakery recipe, not already stored stock.
        // Never guess whether old food originally came from the bakery or the fish chain.
        return snapshot with
        {
            RulesVersion = change.To,
            Buildings = [.. snapshot.Buildings, .. change.AddedBuildings.Select(id =>
                new BuildingState(id, id == "town-hall" ? 1 : 0, 0))],
            Military = change.To == "civil-growth-10" ? MilitaryTrainingState.Empty : military
        };
    }
}
