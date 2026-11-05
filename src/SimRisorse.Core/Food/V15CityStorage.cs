using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Core.Food;

/// <summary>Shared city-only capacity projection for the v15 clock and city commands.
/// The historical food capacity is NOT prepared-food capacity: only the new depot owns that.</summary>
public static class V15CityStorage
{
    // The v14 overload is a temporary boundary for older callers, not a v14
    // validation or a source of historical food capacity.
    public static ResourceBundle Calculate(GameScenario baseline, V14CampaignSnapshot city) =>
        Calculate(baseline, V15CityCampaignSnapshot.FromInherited(city));

    public static ResourceBundle Calculate(GameScenario baseline, V15CityCampaignSnapshot city)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(city);
        var capacity = baseline.Capacity;
        foreach (var definition in baseline.Catalog.Buildings)
        {
            // The inherited catalog still names the retired v14 food-store.
            // A native v15 campaign has no such building; its prepared-food
            // storage comes exclusively from the new depot, not the city.
            if (definition.Id == V15LegacyProducerAdapter.RetiredFamilyId) continue;
            if (definition.StoragePerBuilding is not { } perBuilding) continue;
            var productive = city.ProductiveFamilies.FirstOrDefault(f => f.FamilyId == definition.Id);
            var equivalents = productive is not null
                ? checked(productive.Base.Buildings + checked(3 * productive.Large.Buildings) +
                    checked(10 * productive.Advanced.Buildings))
                : city.OtherBuildings.Single(b => b.BuildingId == definition.Id).Count;
            if (equivalents > 0) capacity += perBuilding.Scale(equivalents);
        }
        // Preserve the approved v14 pencil-capacity formula without creating
        // a legacy civic snapshot just to calculate one resource capacity:
        // baseline plank capacity + town-hall/warehouse plank storage +
        // 50 pencils per school. This belongs to the v15 city's real buildings.
        var pencils = baseline.Capacity["planks"].Units;
        foreach (var building in city.OtherBuildings)
        {
            if (building.BuildingId is not ("town-hall" or "warehouse")) continue;
            var storage = baseline.Catalog.BuildingsById[building.BuildingId].StoragePerBuilding;
            pencils = checked(pencils + building.Count *
                (storage is null ? 0m : storage["planks"].Units));
        }
        pencils = checked(pencils + (decimal)V14SchoolSupplies.SchoolPencilStorage * city.SchoolCount);
        return capacity + ResourceBundle.FromAmounts([
            KeyValuePair.Create(V14SchoolSupplies.ResourceId, ResourceAmount.FromUnits(pencils))]);
    }
}
