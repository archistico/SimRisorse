using System.Collections.Immutable;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;

namespace SimRisorse.Application.Benchmarks;

/// <summary>
/// RC98 laboratory fixture ONLY. A pre-provisioned city with warehouses,
/// unlocked research and a gold smelter is manufactured for deterministic
/// transaction tests. These materials are NOT earned in a natural game and
/// the fixture must never be presented as a progress/viability benchmark.
/// </summary>
public static class E4bV15MintCompletionFixture
{
    public static V15FoodCampaignCheckpoint PreProvisioned(V15BenchmarkFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        var city = fixture.Start.CityCampaign;
        var updated = city with
        {
            Resources = city.Resources + ResourceBundle.Create(("planks", 9000m), ("stone", 8000m)),
            OtherBuildings = city.OtherBuildings.Select(b => b.BuildingId == "warehouse"
                ? b with { Count = 20 } : b).ToImmutableArray(),
            ProductiveFamilies = city.ProductiveFamilies.Select(f => f.FamilyId == "gold-smelter"
                ? f with { Base = f.Base with { Buildings = 1 } } : f).ToImmutableArray(),
            PurchaseCounters = city.PurchaseCounters.Select(c => c.FamilyId switch
            {
                "warehouse" => c with { Acquired = 20 },
                "gold-smelter" => c with { Acquired = 1 },
                _ => c
            }).ToImmutableArray(),
            UnlockedTechnologyIds = ["estrazione-1", "industria-1"]
        };
        var prepared = fixture.Start with { CityCampaign = updated };
        V15FoodCampaignCheckpoints.Validate(prepared, fixture.Baseline, fixture.School,
            fixture.Technologies, fixture.Depot);
        return prepared;
    }
}
