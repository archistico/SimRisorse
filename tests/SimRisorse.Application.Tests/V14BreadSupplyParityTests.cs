using System.Collections.Immutable;
using SimRisorse.Application.ReadModels;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Population;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>v14 must provide the same bread-chain explanation as the established v13 read model.</summary>
public sealed class V14BreadSupplyParityTests
{
    private static readonly SchoolResearchRules School = new("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;

    private static (V14CampaignSession Session, SimRisorse.Core.Content.GameScenario Scenario) Prepare(
        int population, bool bakery, ResourceBundle resources)
    {
        var scenario = CivilGrowthScenario.Create();
        var original = V14CampaignSession.FromV13(scenario, GameSession.Create(scenario).Snapshot(),
            School, Technologies, V14SchoolConstructionRules.Candidate).Snapshot();
        var prepared = original with
        {
            Population = population,
            Resources = resources,
            OtherBuildings = original.OtherBuildings.Select(item => item.BuildingId == "large-house"
                ? item with { Count = 1 } : item).ToImmutableArray(),
            ProductiveFamilies = original.ProductiveFamilies.Select(item => item.FamilyId == "bakery" && bakery
                ? item with { Base = new TierOccupancy(1, 1) } : item).ToImmutableArray(),
            PurchaseCounters = original.PurchaseCounters.Select(item => item.FamilyId == "large-house" ||
                (item.FamilyId == "bakery" && bakery) ? item with { Acquired = 1 } : item).ToImmutableArray()
        };
        return (V14CampaignSession.Restore(scenario, prepared, School, Technologies,
            V14SchoolConstructionRules.Candidate), scenario);
    }

    private static BreadSupplyView Diagnose(V14CampaignSession session,
        SimRisorse.Core.Content.GameScenario scenario)
    {
        var state = session.Snapshot();
        var forecast = session.ForecastEconomy();
        var rates = forecast.Resources.ToDictionary(item => item.Id, StringComparer.Ordinal);
        // Capacity is part of the actual v14 presentation contract; the bread
        // projection only uses stock and next-second rates, never a guessed supply.
        var resources = scenario.Catalog.Resources.Select(item => new ResourceView(item.Id, item.Name,
            state.Resources[item.Id].Units, 0m,
            rates[item.Id].ProductionPerSecond, rates[item.Id].ConsumptionPerSecond)).ToArray();
        return V14BreadSupplyProjector.Project(scenario, state, forecast, resources);
    }

    [Fact]
    public void FirstAdvancedArrivalShowsBreadShortageEvenWhenFoodTakesPrecedence()
    {
        var (session, scenario) = Prepare(8, bakery: false, ResourceBundle.Empty);
        Assert.Equal(GrowthBlockReason.MissingFood, session.ForecastEconomy().Population.BlockReason);
        var diagnosis = Diagnose(session, scenario);
        Assert.Equal(BreadSupplyState.FirstAdvancedArrivalNeedsStock, diagnosis.State);
        Assert.Null(diagnosis.ReserveSecondsWithoutProduction);
        Assert.Equal(0m, diagnosis.RequiredPerSecond);
        Assert.Null(diagnosis.ChainCapacity);
    }

    [Fact]
    public void MissingRecipeInputsAndWorkingRecipeHaveDistinctStates()
    {
        var (limited, scenario) = Prepare(9, bakery: true,
            ResourceBundle.Create(("food", 100m), ("bread", 20m)));
        var missing = Diagnose(limited, scenario);
        Assert.Equal(BreadSupplyState.MissingInputs, missing.State);
        Assert.Equal(0m, missing.NextSecondProductionPerSecond);
        Assert.Equal(3m, missing.NominalProductionPerSecond);
        Assert.NotNull(missing.ChainCapacity);

        var (working, sameScenario) = Prepare(9, bakery: true,
            ResourceBundle.Create(("food", 100m), ("bread", 20m), ("flour", 1m), ("water", 1m)));
        var supplied = Diagnose(working, sameScenario);
        Assert.Equal(BreadSupplyState.SufficientNextSecond, supplied.State);
        Assert.Equal(3m, supplied.NextSecondProductionPerSecond);
    }

    [Fact]
    public void FullBreadInventoryIsReportedAsStorageNotNominalShortage()
    {
        var (session, scenario) = Prepare(9, bakery: true,
            ResourceBundle.Create(("food", 100m), ("bread", 1085m), ("flour", 1m), ("water", 1m)));
        var supply = Diagnose(session, scenario);
        Assert.Equal(BreadSupplyState.StorageFull, supply.State);
        Assert.Equal(0m, supply.NextSecondProductionPerSecond);
        Assert.Equal(0m, supply.NominalShortfallPerSecond);
    }
}
