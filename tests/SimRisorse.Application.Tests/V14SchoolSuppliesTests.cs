using System.Collections.Immutable;
using SimRisorse.Application.Scenarios;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class V14SchoolSuppliesTests
{
    private static readonly SchoolResearchRules PencilRules = new("food", ResourceAmount.FromUnits(10m),
        120, 1, 2, 100, pencilsPerStudent: 2);

    private static (GameScenario Baseline, V14CampaignSession Session) Ready(decimal coal = 10m)
    {
        var baseline = CivilGrowthScenario.Create();
        var tech = TechnologyCatalog.ProposedTierTree;
        var initial = V14CampaignSession.FromV13(baseline, GameSession.Create(baseline).Snapshot(),
            PencilRules, tech, V14SchoolConstructionRules.Candidate).Snapshot();
        // Focused fixture: a school and the charcoal supply exist; no expensive construction needed.
        var prepared = initial with
        {
            SchoolCount = 1,
            PurchaseCounters = initial.PurchaseCounters.Select(item => item.FamilyId == "school"
                ? item with { Acquired = 1 } : item).ToImmutableArray(),
            Resources = initial.Resources + ResourceBundle.Create(("coal", coal))
        };
        return (baseline, V14CampaignSession.Restore(baseline, prepared,
            PencilRules, tech, V14SchoolConstructionRules.Candidate));
    }

    [Fact]
    public void FourPencilsNeedOnePlankAndOneCoalAndTwoAreRequiredForEnrollment()
    {
        var (_, session) = Ready();
        var start = session.Snapshot();
        Assert.Equal(SchoolActionFailure.InsufficientPencils, session.EnrollStudents(1));
        Assert.Same(start, session.Snapshot());
        Assert.Equal(PencilCraftFailure.None, session.CraftPencils(1));
        var crafted = session.Snapshot();
        Assert.Equal(4m, crafted.Resources["pencils"].Units);
        Assert.Equal(start.Resources["coal"].Units - 1m, crafted.Resources["coal"].Units);
        Assert.Equal(start.Resources["planks"].Units - 1m, crafted.Resources["planks"].Units);
        Assert.Equal(SchoolActionFailure.None, session.EnrollStudents(1));
        Assert.Equal(2m, session.Snapshot().Resources["pencils"].Units);
        Assert.Equal(start.Resources["food"].Units - 10m, session.Snapshot().Resources["food"].Units);
        Assert.Equal(1, session.Snapshot().PencilPaidStudents);
        Assert.Equal(1, session.Snapshot().SchoolResearch.TrainingStudents);
    }

    [Fact]
    public void CancelRefundsOnlyPrepaidPencilsAndNoResourceIsMintedByOldStudents()
    {
        var (baseline, session) = Ready();
        Assert.Equal(PencilCraftFailure.None, session.CraftPencils(1));
        var before = session.Snapshot().Resources;
        Assert.Equal(SchoolActionFailure.None, session.EnrollStudents(1));
        Assert.Equal(SchoolActionFailure.None, session.CancelStudents(1));
        Assert.Equal(before, session.Snapshot().Resources);
        Assert.Equal(0, session.Snapshot().PencilPaidStudents);

        var oldRules = new SchoolResearchRules("food", ResourceAmount.FromUnits(10m), 120, 1, 2, 100);
        var original = Ready().Session.Snapshot();
        var legacyEnrollment = SchoolResearch.Enroll(oldRules, original.SchoolResearch,
            original.Resources, original.Population, 0, 0, 1, 1);
        Assert.True(legacyEnrollment.Succeeded);
        var legacy = V14CampaignSession.Restore(baseline, original with
        {
            Resources = legacyEnrollment.Resources,
            SchoolResearch = legacyEnrollment.State,
            PencilPaidStudents = 0
        }, PencilRules, TechnologyCatalog.ProposedTierTree, V14SchoolConstructionRules.Candidate);
        Assert.Equal(SchoolActionFailure.None, legacy.CancelStudents(1));
        Assert.Equal(0m, legacy.Snapshot().Resources["pencils"].Units);
        Assert.Equal(original.Resources["food"], legacy.Snapshot().Resources["food"]);
    }

    [Fact]
    public void ProductionCannotRunWithoutSchoolInputsOrRoomAndIsAtomic()
    {
        var (baseline, session) = Ready(coal: 0);
        var zero = session.Snapshot();
        Assert.Equal(PencilCraftFailure.InsufficientMaterials, session.CraftPencils(1));
        Assert.Same(zero, session.Snapshot());
        Assert.Equal(PencilCraftFailure.InvalidQuantity, session.CraftPencils(0));
        var without = V14CampaignSession.Restore(baseline, zero with
        {
            SchoolCount = 0,
            PurchaseCounters = zero.PurchaseCounters.Select(item => item.FamilyId == "school"
                ? item with { Acquired = 0 } : item).ToImmutableArray()
        }, PencilRules, TechnologyCatalog.ProposedTierTree, V14SchoolConstructionRules.Candidate);
        Assert.Equal(PencilCraftFailure.MissingSchool, without.CraftPencils(1));

        var (_, supplies) = Ready();
        var full = supplies.Snapshot();
        var capacity = V14SchoolSupplies.Capacity(baseline, full).Units;
        var nearlyFull = V14CampaignSession.Restore(baseline, full with
        {
            Resources = full.Resources + ResourceBundle.Create(("pencils", capacity - 1m))
        }, PencilRules, TechnologyCatalog.ProposedTierTree, V14SchoolConstructionRules.Candidate);
        var before = nearlyFull.Snapshot();
        Assert.Equal(PencilCraftFailure.InsufficientStorage, nearlyFull.CraftPencils(1));
        Assert.Same(before, nearlyFull.Snapshot());
    }

    [Fact]
    public void PencilsSurviveTicksAndGraduationClearsRefundEligibility()
    {
        var (_, session) = Ready();
        Assert.Equal(PencilCraftFailure.None, session.CraftPencils(1));
        Assert.Equal(SchoolActionFailure.None, session.EnrollStudents(1));
        session.Resume();
        Assert.Equal(GameError.None, session.Advance(TimeSpan.FromSeconds(60)).Error);
        Assert.Equal(2m, session.Snapshot().Resources["pencils"].Units);
        Assert.Equal(1, session.Snapshot().PencilPaidStudents);
        Assert.Equal(60, session.Snapshot().SchoolResearch.TrainingProgressSeconds[0]);
        Assert.Equal(GameError.None, session.Advance(TimeSpan.FromSeconds(60)).Error);
        Assert.Equal(2m, session.Snapshot().Resources["pencils"].Units);
        Assert.Equal(0, session.Snapshot().PencilPaidStudents);
        Assert.Equal(1, session.Snapshot().SchoolResearch.QualifiedResearchers);
    }
}
