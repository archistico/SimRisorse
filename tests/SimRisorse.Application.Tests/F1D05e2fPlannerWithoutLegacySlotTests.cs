using System.Collections.Immutable;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>
/// RC71: the shared production planner must not need a phantom food-store
/// BuildingState when v15 explicitly suppresses that historical producer.
/// Both the physical inventory and the diagnostic first-second flows remain
/// controlled by the original deterministic production rules.
/// </summary>
public sealed class F1D05e2fPlannerWithoutLegacySlotTests
{
    private const string RetiredId = V15LegacyProducerAdapter.RetiredFamilyId;

    [Fact]
    public void CombinedClockRunsWithoutRetiredFamilyInNativeCity()
    {
        var scenario = V15CombinedFoodStressScenario.Create();
        Assert.Equal(15, scenario.Initial.CityCampaign.ProductiveFamilies.Length);
        Assert.DoesNotContain(scenario.Initial.CityCampaign.ProductiveFamilies,
            family => family.FamilyId == RetiredId);

        var step = scenario.Plan(scenario.Initial, TimeSpan.FromSeconds(1));
        Assert.Equal(1, step.SecondsAdvanced);
        Assert.True(V15StockAuditor.ReconcileSecond(scenario.Initial, step).Balanced);
        Assert.Equal(15, step.Next.CityCampaign.ProductiveFamilies.Length);
        Assert.DoesNotContain(step.Next.CityCampaign.ProductiveFamilies,
            family => family.FamilyId == RetiredId);
        Assert.Equal(ResourceAmount.Zero, step.Next.CityCampaign.Resources["food"]);
        Assert.True(step.FirstSecondProduction["fish"].Milliunits > 0);
        Assert.True(step.FirstSecondProduction["food"].Milliunits > 0);
    }

    [Fact]
    public void ExplicitlyImportedOccupiedRetiredProducerIsPreservedButNeverRuns()
    {
        var scenario = V15CombinedFoodStressScenario.Create();
        var initial = scenario.Initial;
        var city = initial.CityCampaign;
        var historical = city with
        {
            ProductiveFamilies = city.ProductiveFamilies.Add(new V15ProductiveFamilySnapshot(
                RetiredId, new TierOccupancy(1, 1), default, default)),
            PurchaseCounters = city.PurchaseCounters.Select(counter =>
                counter.FamilyId == RetiredId
                    ? counter with { Acquired = Math.Max(1, counter.Acquired) }
                    : counter).ToImmutableArray()
        };
        var imported = initial with { CityCampaign = historical };
        V15FoodCampaignCheckpoints.Validate(imported, scenario.Baseline,
            scenario.School, scenario.Technologies, scenario.Depot);

        var normal = scenario.Plan(initial, TimeSpan.FromSeconds(1));
        var withRetired = scenario.Plan(imported, TimeSpan.FromSeconds(1));
        Assert.Equal(normal.FirstSecondProduction, withRetired.FirstSecondProduction);
        Assert.Equal(normal.FirstSecondConsumption, withRetired.FirstSecondConsumption);
        Assert.Equal(normal.Next.CityCampaign.Resources, withRetired.Next.CityCampaign.Resources);
        Assert.Equal(normal.Next.Depot, withRetired.Next.Depot);
        Assert.Equal(normal.MealsServed, withRetired.MealsServed);
        Assert.Equal(1, withRetired.Next.CityCampaign.ProductiveFamilies
            .Single(family => family.FamilyId == RetiredId).Base.AssignedWorkers);
        Assert.Equal(ResourceAmount.Zero, withRetired.Next.CityCampaign.Resources["food"]);
        Assert.True(V15StockAuditor.ReconcileSecond(imported, withRetired).Balanced);
    }

    [Fact]
    public void SuppressionPreservesFractionalAndSplitTickDeterminism()
    {
        var scenario = V15CombinedFoodStressScenario.Create();
        var initial = scenario.Initial;
        var continuous = scenario.Plan(initial, TimeSpan.FromSeconds(3.5)).Next;
        var partial = scenario.Plan(initial, TimeSpan.FromMilliseconds(500)).Next;
        Assert.Equal(initial.CityCampaign.Resources, partial.CityCampaign.Resources);
        var split = scenario.Plan(partial, TimeSpan.FromSeconds(3)).Next;

        Assert.Equal(continuous.CityCampaign.Resources, split.CityCampaign.Resources);
        Assert.Equal(continuous.CityCampaign.ElapsedTicks, split.CityCampaign.ElapsedTicks);
        Assert.Equal(continuous.CityCampaign.Population, split.CityCampaign.Population);
        Assert.Equal(continuous.Depot, split.Depot);
        Assert.Equal(continuous.Industry, split.Industry);
        Assert.DoesNotContain(split.CityCampaign.ProductiveFamilies,
            family => family.FamilyId == RetiredId);
    }

    [Fact]
    public void OmittingAnActiveProducerIsStillRejectedByV15Validation()
    {
        var scenario = V15CombinedFoodStressScenario.Create();
        var city = scenario.Initial.CityCampaign;
        var incomplete = scenario.Initial with
        {
            CityCampaign = city with { ProductiveFamilies = city.ProductiveFamilies.RemoveAt(0) }
        };
        Assert.Throws<ArgumentException>(() => scenario.Plan(incomplete, TimeSpan.FromSeconds(1)));
        Assert.Equal(15, scenario.Initial.CityCampaign.ProductiveFamilies.Length);
    }
}
