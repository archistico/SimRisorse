using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Food;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC76-HF01: benchmarks must use the same publicly accessible,
/// authoritative workforce projection as the live v15 campaign session.
/// </summary>
public sealed class E4aV15WorkforceProjectionRegressionTests
{
    [Fact]
    public void FreshCampaignBenchmarkAndLiveSessionAgreeAfterInitialActions()
    {
        var fixture = V15BenchmarkFixture.Create();
        var session = new V15FoodIndustryCampaignSession(fixture.Start,
            fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot);
        var run = V15NaturalProgressionBenchmark.Run("V15-N01", maxSeconds: 0);

        Assert.Equal(5L, session.AvailableWorkers);
        Assert.Equal(session.AvailableWorkers,
            V15FoodCampaignCheckpoints.AvailableWorkers(fixture.Start));

        // The zero-second sample is taken AFTER the policy's instant commands.
        // Apply those same commands to compare equivalent campaign states.
        Assert.Equal(new[] { "build-fisher", "staff-fisher", "build-depot", "staff-depot" },
            run.Events.Select(e => e.Action).ToArray());
        Assert.All(run.Events, e =>
        {
            Assert.Equal(0, e.ElapsedSeconds);
            Assert.Equal("executed", e.Result);
        });
        Assert.True(session.BuildCityProducer("fisher", ProductionTier.Base, 1).Succeeded);
        Assert.Equal(V14TierActionFailure.None,
            session.AssignCityProducerWorkers("fisher", ProductionTier.Base, 1));
        Assert.True(session.Build("food-depot", 1).Succeeded);
        Assert.Equal(FoodIndustryActionFailure.None, session.AssignWorkers("food-depot", 1));

        var sample = Assert.Single(run.Samples);
        Assert.Equal(3L, session.AvailableWorkers);
        Assert.Equal(session.AvailableWorkers,
            V15FoodCampaignCheckpoints.AvailableWorkers(session.Snapshot()));
        Assert.Equal(session.AvailableWorkers, sample.FreeWorkers);
    }

    [Fact]
    public void CombinedFoodCrewsAndHistoricalIndustriesAreCountedExactlyOnce()
    {
        var fixture = V15CombinedFoodStressScenario.Create();
        var session = new V15FoodIndustryCampaignSession(fixture.Initial,
            fixture.Baseline, fixture.School, fixture.Technologies, fixture.Depot);

        Assert.Equal(24, fixture.Initial.CityCampaign.Population);
        Assert.Equal(6L, fixture.Initial.Industry.Buildings.Assigned.Total);
        Assert.Equal(2L, V15FoodCampaignCheckpoints.AvailableWorkers(fixture.Initial));
        Assert.Equal(session.AvailableWorkers,
            V15FoodCampaignCheckpoints.AvailableWorkers(fixture.Initial));
    }
}
