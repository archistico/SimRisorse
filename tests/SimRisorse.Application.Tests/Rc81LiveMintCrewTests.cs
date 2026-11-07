using System.Collections.Immutable;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>Only controlled fixtures are seeded; no benchmark calls these natural runs.</summary>
public sealed class Rc81LiveMintCrewTests
{
    private static readonly ResourceBundle Inputs = ResourceBundle.Create(("gold", 6m), ("coal", 6m));

    private static (V15BenchmarkFixture Fixture, V15FoodCampaignCheckpoint Checkpoint) Prepare(
        int mintedWorkers, int mintCount = 1)
    {
        var fx = V15BenchmarkFixture.Create();
        var city = fx.Start.CityCampaign;
        city = city with
        {
            Population = 30,
            Resources = city.Resources + Inputs,
            OtherBuildings = city.OtherBuildings.Select(b => b.BuildingId == "small-house"
                ? b with { Count = 4 } : b).ToImmutableArray(),
            ProductiveFamilies = city.ProductiveFamilies.Select(f => f.FamilyId == "mint"
                ? f with { Base = new TierOccupancy(mintCount, mintedWorkers) } : f).ToImmutableArray(),
            PurchaseCounters = city.PurchaseCounters.Select(c => c.FamilyId switch
            {
                "mint" => c with { Acquired = mintCount },
                "small-house" => c with { Acquired = 4 },
                _ => c
            }).ToImmutableArray()
        };
        return (fx, fx.Start with { CityCampaign = city, CrewMode = V15CrewMode.MintTen });
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(9, 0)]
    [InlineData(10, 1)]
    [InlineData(19, 1)]
    [InlineData(20, 2)]
    public void LiveClockConsumesInputsOnlyForCompletedMintTeams(int staff, int expectedModules)
    {
        var (fx, checkpoint) = Prepare(staff, mintCount: 2);
        V15FoodCampaignCheckpoints.Validate(checkpoint, fx.Baseline,
            fx.School, fx.Technologies, fx.Depot);
        var step = V15UnifiedCampaignClock.Plan(checkpoint, fx.Baseline, fx.School,
            fx.Technologies, FoodIndustryConstructionCatalog.Candidate, fx.Meat,
            fx.Crops, fx.Depot, TimeSpan.FromSeconds(1), includeProductionDiagnostics: true);
        var mint = Assert.Single(step.FirstSecondBuildings!, b => b.BuildingId == "mint");
        Assert.Equal(staff, mint.AssignedWorkers);
        Assert.Equal(expectedModules, mint.RunnableWorkers);
        // Check the resources consumed by the clock, not the absolute starter stock:
        // RC94.1 gives every new v15 city 100 coal from the Municipio.
        Assert.Equal((decimal)expectedModules,
            checkpoint.CityCampaign.Resources["gold"].Units - step.Next.CityCampaign.Resources["gold"].Units);
        Assert.Equal((decimal)expectedModules,
            checkpoint.CityCampaign.Resources["coal"].Units - step.Next.CityCampaign.Resources["coal"].Units);
        Assert.Equal(3m * expectedModules, step.Next.CityCampaign.Resources["coins"].Units);
        Assert.Equal(V15CrewMode.MintTen, step.Next.CrewMode);
    }

    [Fact]
    public void SessionAllowsPartialCrewsButDeniesExcessPopulationAndWorkplacesAtomically()
    {
        var (fx, start) = Prepare(9);
        var game = new V15FoodIndustryCampaignSession(start, fx.Baseline,
            fx.School, fx.Technologies, fx.Depot);
        var first = game.Snapshot();
        Assert.Equal(V15CrewMode.MintTen, game.CrewMode);
        Assert.Equal(V14TierActionFailure.InsufficientWorkplaces,
            game.AssignCityProducerWorkers("mint", ProductionTier.Base, 2));
        Assert.Same(first, game.Snapshot());
        Assert.Equal(V14TierActionFailure.None,
            game.AssignCityProducerWorkers("mint", ProductionTier.Base, 1));
        var row = game.CityCatalog.Buildings.Single(b => b.Id == "mint");
        Assert.Equal(10, row.CrewSize);
        Assert.Equal(1, row.CompleteModules);
        Assert.Equal(10, row.AssignedWorkers);
        Assert.Equal(V14TierActionFailure.InsufficientWorkplaces,
            game.AssignCityProducerWorkers("mint", ProductionTier.Base, 1));
        Assert.Equal(V14TierActionFailure.None,
            game.ReleaseCityProducerWorkers("mint", ProductionTier.Base, 1));
        Assert.Equal(9, game.CityCatalog.Buildings.Single(b => b.Id == "mint").AssignedWorkers);
    }

    [Fact]
    public void SwitchingCampaignRulesIsExplicitAndDoesNotRewriteResources()
    {
        var (fx, mint) = Prepare(1);
        var old = mint with { CrewMode = V15CrewMode.Legacy };
        var game = new V15FoodIndustryCampaignSession(old, fx.Baseline,
            fx.School, fx.Technologies, fx.Depot);
        var before = game.Snapshot();
        game.EnableMintCrews();
        Assert.Equal(V15CrewMode.MintTen, game.CrewMode);
        Assert.Equal(before.CityCampaign.Resources, game.Snapshot().CityCampaign.Resources);
        Assert.Equal(before.Depot, game.Snapshot().Depot);
        var changed = game.Snapshot();
        game.EnableMintCrews();
        Assert.Same(changed, game.Snapshot());
        Assert.Equal(0, game.CityCatalog.Buildings.Single(b => b.Id == "mint").CompleteModules);
    }

    [Fact]
    public void PausedCampaignDoesNotProduceOrCommitUntilResumed()
    {
        var (fx, start) = Prepare(10);
        var game = new V15FoodIndustryCampaignSession(start, fx.Baseline,
            fx.School, fx.Technologies, fx.Depot);
        Assert.True(game.IsPaused);
        var before = game.Snapshot();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1), fx.Meat, fx.Crops).Succeeded);
        Assert.Same(before, game.Snapshot());
        game.Resume();
        Assert.True(game.Advance(TimeSpan.FromSeconds(1), fx.Meat, fx.Crops).Succeeded);
        Assert.Equal(3m, game.Snapshot().CityCampaign.Resources["coins"].Units);
        game.Pause();
        var paused = game.Snapshot();
        Assert.True(game.Advance(TimeSpan.FromSeconds(2), fx.Meat, fx.Crops).Succeeded);
        Assert.Same(paused, game.Snapshot());
    }

    [Fact]
    public void UnmodifiedV15GameRetainsLegacyMintAndOtherProduction()
    {
        var (fx, mint) = Prepare(1);
        var old = mint with { CrewMode = V15CrewMode.Legacy };
        var tick = V15UnifiedCampaignClock.Plan(old, fx.Baseline, fx.School,
            fx.Technologies, FoodIndustryConstructionCatalog.Candidate, fx.Meat,
            fx.Crops, fx.Depot, TimeSpan.FromSeconds(1), includeProductionDiagnostics: true);
        Assert.Equal(3m, tick.Next.CityCampaign.Resources["coins"].Units);
        Assert.Equal(1, Assert.Single(tick.FirstSecondBuildings!, b => b.BuildingId == "mint").RunnableWorkers);
    }

    [Fact]
    public void InvalidCheckpointWithTooManyWorkersIsRejectedBeforeClockAndCommands()
    {
        var (fx, valid) = Prepare(10);
        var invalid = valid with { CityCampaign = valid.CityCampaign with
        {
            ProductiveFamilies = valid.CityCampaign.ProductiveFamilies.Select(f => f.FamilyId == "mint"
                ? f with { Base = new TierOccupancy(1, 11) } : f).ToImmutableArray()
        }};
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            V15FoodCampaignCheckpoints.Validate(invalid, fx.Baseline, fx.School, fx.Technologies, fx.Depot));
    }
}
