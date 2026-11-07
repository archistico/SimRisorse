using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Core.Research;

/// <summary>
/// Experimental v14 supplies: school workshop crafts four pencils from one plank and one coal.
/// The v13 resource catalog remains immutable. Pencil stock is stored in the ordinary v14
/// resource_stock table and is strictly validated by the v14 checkpoint validator.
/// </summary>
public static class V14SchoolSupplies
{
    public const string ResourceId = "pencils";
    public const int PencilsPerBatch = 4;
    public const int SchoolPencilStorage = 50;
    // Starter set provided once by the Municipio when creating a new v15 campaign.
    public const int MunicipalStarterPencils = 2;
    public static ResourceBundle Inputs(int batches) => ResourceBundle.Create(
        ("planks", batches), ("coal", batches));
    public static ResourceBundle Output(int batches) => ResourceBundle.Create(
        (ResourceId, checked((decimal)batches * PencilsPerBatch)));

    /// <summary>Municipio +1000, Magazzino +500, Scuola +50. Other v13 storage is unchanged.</summary>
    public static ResourceAmount Capacity(GameScenario baseline, V14CampaignSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(snapshot);
        var initial = baseline.Capacity["planks"].Units;
        foreach (var building in snapshot.OtherBuildings)
        {
            if (building.BuildingId is not ("town-hall" or "warehouse")) continue;
            var perBuilding = baseline.Catalog.BuildingsById[building.BuildingId].StoragePerBuilding;
            initial = checked(initial + building.Count * ((perBuilding is null ? 0m : perBuilding["planks"].Units)));
        }
        return ResourceAmount.FromUnits(checked(initial + (decimal)SchoolPencilStorage * snapshot.SchoolCount));
    }
}

public enum PencilCraftFailure
{
    None, InvalidQuantity, MissingSchool, InsufficientMaterials, InsufficientStorage, NumericLimit
}
