using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>
/// E3b/3c: all v14 subsystems share one elapsed clock; no changes to the live v13 session.
/// </summary>
public sealed class V14CampaignClockTests
{
    private static readonly SchoolResearchRules School = new("food", ResourceAmount.FromUnits(10m),
        120, 1, 2, 100);
    private static readonly TechnologyCatalog Technologies = TechnologyCatalog.ProposedTierTree;
    private static readonly V14SchoolConstructionRules Construction = V14SchoolConstructionRules.Candidate;

    internal static V14CampaignSession Prepared()
    {
        var baseline = CivilGrowthScenario.Create();
        var original = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            School, Technologies, Construction).Snapshot();
        var advanced = original with
        {
            // This synthetic test colony reserves 3 initial production workers,
            // 2 additional food workers, 1 recruit and 4 school inhabitants.
            // Ten people are required; two small houses provide sixteen places.
            Population = 10,
            OtherBuildings = original.OtherBuildings.Select(item => item.BuildingId switch
            {
                "small-house" => item with { Count = 2 },
                "barracks" => item with { Count = 1 },
                _ => item
            }).ToImmutableArray(),
            ProductiveFamilies = original.ProductiveFamilies.Select(item => item.FamilyId is "fisher" or "food-store"
                ? item with { Base = new TierOccupancy(1, 1) } : item).ToImmutableArray(),
            PurchaseCounters = original.PurchaseCounters.Select(item => item.FamilyId switch
            {
                "small-house" => item with { Acquired = 2 },
                "barracks" or "fisher" or "food-store" or "school" => item with { Acquired = 1 },
                _ => item
            }).ToImmutableArray(),
            SchoolCount = 1,
            SchoolResearch = new SchoolResearchState(1, ImmutableArray.Create(0), 2, 2),
            Military = new MilitaryTrainingState(0, 1, 0, 0, ImmutableArray.Create(0))
        };
        var prepared = V14CampaignSession.Restore(baseline, advanced, School, Technologies, Construction);
        // Guard the fixture itself: no extra people may be fabricated by the clock.
        Assert.Equal(0, prepared.AvailablePopulation);
        return prepared;
    }

    internal static void Equivalent(V14CampaignSnapshot a, V14CampaignSnapshot b)
    {
        Assert.Equal(a.ElapsedTicks, b.ElapsedTicks);
        Assert.Equal(a.Resources, b.Resources);
        Assert.Equal(a.Population, b.Population);
        Assert.Equal(a.GrowthProgressSeconds, b.GrowthProgressSeconds);
        Assert.Equal(a.GrowthRemainderTwentieths, b.GrowthRemainderTwentieths);
        Assert.Equal(a.Military, b.Military);
        Assert.Equal(a.ResearchBalanceMilliPoints, b.ResearchBalanceMilliPoints);
        Assert.Equal(a.SchoolResearch.WaitingStudents, b.SchoolResearch.WaitingStudents);
        Assert.Equal(a.SchoolResearch.QualifiedResearchers, b.SchoolResearch.QualifiedResearchers);
        Assert.Equal(a.SchoolResearch.ActiveResearchers, b.SchoolResearch.ActiveResearchers);
        Assert.Equal(a.SchoolResearch.TrainingProgressSeconds.ToArray(),
            b.SchoolResearch.TrainingProgressSeconds.ToArray());
        Assert.Equal(a.UnlockedTechnologyIds.ToArray(), b.UnlockedTechnologyIds.ToArray());
        Assert.Equal(a.PurchaseCounters.ToArray(), b.PurchaseCounters.ToArray());
        Assert.Equal(a.OtherBuildings.ToArray(), b.OtherBuildings.ToArray());
        Assert.Equal(a.ProductiveFamilies.ToArray(), b.ProductiveFamilies.ToArray());
    }

    [Fact]
    public void PausedClockDoesNotAdvanceAndInvalidRequestsDoNotMutate()
    {
        var session = Prepared();
        var original = session.Snapshot();
        Assert.True(session.IsPaused);
        Assert.True(session.Advance(TimeSpan.FromMinutes(10)).Succeeded);
        Assert.Same(original, session.Snapshot());
        Assert.Equal(GameError.InvalidElapsedTime, session.Advance(TimeSpan.FromTicks(-1)).Error);
        Assert.Equal(GameError.AdvanceLimitExceeded, session.Advance(TimeSpan.FromDays(2)).Error);
        Assert.Same(original, session.Snapshot());
        session.Resume();
        Assert.True(session.Advance(TimeSpan.Zero).Succeeded);
        Assert.Same(original, session.Snapshot());
    }

    [Fact]
    public void WholeMinuteMatchesSixtyOneSecondAdvancesForAllSubsystems()
    {
        var single = Prepared();
        var repeated = Prepared();
        single.Resume();
        repeated.Resume();
        Assert.True(single.Advance(TimeSpan.FromSeconds(60)).Succeeded);
        for (var i = 0; i < 60; i++)
            Assert.True(repeated.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Equivalent(single.Snapshot(), repeated.Snapshot());
        Assert.Equal(1, single.Snapshot().Military.AvailableSoldiers);
        Assert.Equal(12_000, single.Snapshot().ResearchBalanceMilliPoints);
        Assert.Equal(60, single.Snapshot().SchoolResearch.TrainingProgressSeconds[0]);
        Assert.Empty(single.Snapshot().UnlockedTechnologyIds);
        Assert.True(single.Snapshot().Resources["food"].Units > 30m);
    }

    [Fact]
    public void TwoMinutesMatchIndividualStepsAcrossGraduationAndGrowth()
    {
        var longStep = Prepared();
        var shortSteps = Prepared();
        longStep.Resume();
        shortSteps.Resume();
        Assert.True(longStep.Advance(TimeSpan.FromMinutes(2)).Succeeded);
        for (var i = 0; i < 120; i++)
            Assert.True(shortSteps.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Equivalent(longStep.Snapshot(), shortSteps.Snapshot());
        Assert.Equal(3, longStep.Snapshot().SchoolResearch.QualifiedResearchers);
        Assert.Equal(1, longStep.Snapshot().SchoolResearch.TrainingStudents);
        Assert.Equal(0, longStep.Snapshot().SchoolResearch.WaitingStudents);
        Assert.Equal(0, Assert.Single(longStep.Snapshot().SchoolResearch.TrainingProgressSeconds));
        Assert.Equal(24_000, longStep.Snapshot().ResearchBalanceMilliPoints);
        Assert.Equal(2, longStep.Snapshot().SchoolResearch.ActiveResearchers);
        Assert.True(longStep.Snapshot().Population > 8);
    }

    [Fact]
    public void FractionalTicksCarryWithoutEarlyProductionOrEarlyTraining()
    {
        var halfSteps = Prepared();
        var wholeStep = Prepared();
        halfSteps.Resume();
        wholeStep.Resume();
        var original = halfSteps.Snapshot();
        Assert.True(halfSteps.Advance(TimeSpan.FromMilliseconds(500)).Succeeded);
        Assert.Equal(original.Resources, halfSteps.Snapshot().Resources);
        Assert.Equal(original.Military, halfSteps.Snapshot().Military);
        Assert.Equal(original.ResearchBalanceMilliPoints, halfSteps.Snapshot().ResearchBalanceMilliPoints);
        Assert.Equal(TimeSpan.FromMilliseconds(500).Ticks, halfSteps.Snapshot().ElapsedTicks);
        Assert.True(halfSteps.Advance(TimeSpan.FromMilliseconds(500)).Succeeded);
        Assert.True(wholeStep.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Equivalent(wholeStep.Snapshot(), halfSteps.Snapshot());
    }

    [Fact]
    public void PauseAndRestoreResumeTheSameWorkWithoutFreeResearch()
    {
        var original = Prepared();
        original.Resume();
        Assert.True(original.Advance(TimeSpan.FromSeconds(37)).Succeeded);
        original.Pause();
        var saved = original.Snapshot();
        Assert.True(original.Advance(TimeSpan.FromSeconds(30)).Succeeded);
        Assert.Same(saved, original.Snapshot());
        var resumed = V14CampaignSession.Restore(CivilGrowthScenario.Create(), saved,
            School, Technologies, Construction);
        Assert.True(resumed.IsPaused);
        resumed.Resume();
        Assert.True(resumed.Advance(TimeSpan.FromSeconds(83)).Succeeded);
        var uninterrupted = Prepared();
        uninterrupted.Resume();
        Assert.True(uninterrupted.Advance(TimeSpan.FromSeconds(120)).Succeeded);
        Equivalent(uninterrupted.Snapshot(), resumed.Snapshot());
    }

    [Fact]
    public void UnmodifiedV13ColonyAdvancesIdenticallyInIsolatedV14Clock()
    {
        var baseline = CivilGrowthScenario.Create();
        var historical = GameSession.Create(baseline);
        var advanced = V14CampaignSession.FromV13(baseline, historical.Snapshot(),
            School, Technologies, Construction);
        historical.Resume();
        advanced.Resume();
        Assert.True(historical.Advance(TimeSpan.FromSeconds(90)).Succeeded);
        Assert.True(advanced.Advance(TimeSpan.FromSeconds(90)).Succeeded);
        var oldState = historical.Snapshot();
        var newState = advanced.Snapshot();
        Assert.Equal(oldState.ElapsedTicks, newState.ElapsedTicks);
        Assert.Equal(oldState.Resources, newState.Resources);
        Assert.Equal(oldState.Population, newState.Population);
        Assert.Equal(oldState.GrowthProgressSeconds, newState.GrowthProgressSeconds);
        Assert.Equal(oldState.GrowthRemainderTwentieths, newState.GrowthRemainderTwentieths);
        Assert.Equal(oldState.Military, newState.Military);
        Assert.Equal(0, newState.ResearchBalanceMilliPoints);
    }

    [Fact]
    public void NumericOverflowIsRejectedWithoutPartialCommit()
    {
        var baseline = CivilGrowthScenario.Create();
        var source = Prepared().Snapshot() with { ElapsedTicks = long.MaxValue - 1 };
        var session = V14CampaignSession.Restore(baseline, source, School, Technologies, Construction);
        session.Resume();
        Assert.Equal(GameError.NumericLimit, session.Advance(TimeSpan.FromSeconds(1)).Error);
        Assert.Same(source, session.Snapshot());
    }

    [Fact]
    public void AdvancedProducerUsesActualWorkersRatherThanBuildingCount()
    {
        var baseline = CivilGrowthScenario.Create();
        var source = Prepared().Snapshot();
        var enlarged = source with
        {
            // Three initial producers + ten workers in the advanced fishing site.
            Population = 13,
            SchoolResearch = SchoolResearchState.Empty,
            Military = MilitaryTrainingState.IndividualEmpty,
            ProductiveFamilies = source.ProductiveFamilies.Select(item => item.FamilyId switch
            {
                "fisher" => item with { Base = default, Advanced = new TierOccupancy(1, 10) },
                "food-store" => item with { Base = new TierOccupancy(1, 0) },
                _ => item
            }).ToImmutableArray(),
            UnlockedTechnologyIds = ["alimentazione-1", "alimentazione-2"]
        };
        var session = V14CampaignSession.Restore(baseline, enlarged, School, Technologies, Construction);
        session.Resume();
        Assert.True(session.Advance(TimeSpan.FromSeconds(1)).Succeeded);
        Assert.Equal(source.Resources["fish"].Units + 10m, session.Snapshot().Resources["fish"].Units);
    }
}
