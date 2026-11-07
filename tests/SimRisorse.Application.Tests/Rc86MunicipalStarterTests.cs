using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;
using Xunit;

namespace SimRisorse.Application.Tests;

/// <summary>RC86: one-time new-colony seed, not a resource granted on checkpoint load.</summary>
public sealed class Rc86MunicipalStarterTests
{
    [Fact]
    public void FreshV15MunicipioStartsWithTwoPencilsAndOneHundredCoal()
    {
        var setup = V15BenchmarkFixture.Create();
        var starting = setup.Start;
        Assert.Equal(2, V14SchoolSupplies.MunicipalStarterPencils);
        Assert.Equal(2m, starting.CityCampaign.Resources[V14SchoolSupplies.ResourceId].Units);
        Assert.Equal(100, V15FoodCampaignCheckpoints.MunicipalStarterCoal);
        Assert.Equal(100m, starting.CityCampaign.Resources["coal"].Units);
        Assert.Equal(1000m, V15CityStorage.Calculate(setup.Baseline, starting.CityCampaign)["coal"].Units);
        Assert.Equal(0, starting.CityCampaign.SchoolCount);
        Assert.Equal(0, starting.CityCampaign.SchoolResearch.ReservedPopulation);
        Assert.Equal(0, starting.CityCampaign.ResearchBalanceMilliPoints);
        V15FoodCampaignCheckpoints.Validate(starting, setup.Baseline,
            setup.School, setup.Technologies, setup.Depot);
    }

    [Fact]
    public void FirstStudentUsesStarterPencilsWhileCoalRemainsForWorkshop()
    {
        var setup = V15BenchmarkFixture.Create();
        var pencils = setup.Start.CityCampaign.Resources;
        Assert.True(pencils.Covers(ResourceBundle.Create(("pencils", 2m))));
        Assert.False(pencils.Covers(ResourceBundle.Create(("pencils", 4m))));
        Assert.Equal(2m, setup.School.TrainingCost(1)["pencils"].Units);
        Assert.Equal(4m, setup.School.TrainingCost(2)["pencils"].Units);
        // Isolated school rule: enrolling the first student consumes pencils,
        // not the initial coal reserve needed for later pencil crafting.
        var suppliedFood = pencils + ResourceBundle.Create((setup.School.FoodResourceId, 10m));
        var enrolled = SchoolResearch.Enroll(setup.School, SchoolResearchState.Empty,
            suppliedFood, totalPopulation: setup.Start.CityCampaign.Population,
            assignedCivilianWorkers: 3, militaryPopulation: 0, schools: 1, students: 1);
        Assert.True(enrolled.Succeeded);
        Assert.Equal(ResourceAmount.Zero, enrolled.Resources[V14SchoolSupplies.ResourceId]);
        Assert.Equal(100m, enrolled.Resources["coal"].Units);
        Assert.Equal(1, enrolled.State.TrainingStudents);
    }

    [Fact]
    public void MunicipioCoalCanFuelTheSchoolWorkshopBeforeExtractionResearch()
    {
        var setup = V15BenchmarkFixture.Create();
        // The school needs the usual building chain; extra construction
        // materials here are within real municipal storage capacity.
        var stocked = setup.Start with
        {
            CityCampaign = setup.Start.CityCampaign with
            {
                Resources = setup.Start.CityCampaign.Resources +
                    ResourceBundle.Create(("planks", 500m), ("stone", 500m))
            }
        };
        var game = new V15FoodIndustryCampaignSession(stocked, setup.Baseline,
            setup.School, setup.Technologies, setup.Depot);
        Assert.True(game.BuildCityProducer("fisher", ProductionTier.Base, 1).Succeeded);
        Assert.True(game.Build("food-depot", 1).Succeeded);
        Assert.True(game.BuildSchool(1).Succeeded);
        Assert.DoesNotContain("estrazione-1", game.Snapshot().CityCampaign.UnlockedTechnologyIds);
        Assert.Equal(PencilCraftFailure.None, game.CheckPencilCraft(1));
        var before = game.Snapshot().CityCampaign.Resources;
        Assert.Equal(100m, before["coal"].Units);
        Assert.Equal(PencilCraftFailure.None, game.CraftPencils(1));
        Assert.Equal(before - V14SchoolSupplies.Inputs(1) + V14SchoolSupplies.Output(1),
            game.Snapshot().CityCampaign.Resources);
        Assert.Equal(99m, game.Snapshot().CityCampaign.Resources["coal"].Units);
        Assert.Equal(6m, game.Snapshot().CityCampaign.Resources["pencils"].Units);
        Assert.DoesNotContain("estrazione-1", game.Snapshot().CityCampaign.UnlockedTechnologyIds);
    }

    [Fact]
    public void ExistingMigrationPathDoesNotInventStarterPencilsOrCoal()
    {
        var setup = V15BenchmarkFixture.Create();
        var inherited = V14CampaignSnapshots.MigrateFromV13(setup.Baseline,
            GameSession.Create(setup.Baseline).Snapshot(), setup.School, setup.Technologies);
        var imported = V15FoodCampaignCheckpoints.FromMigrationPlan(
            V14FoodStockMigration.Prepare(inherited, setup.Depot), setup.Depot);
        Assert.Equal(ResourceAmount.Zero, imported.CityCampaign.Resources[V14SchoolSupplies.ResourceId]);
        Assert.Equal(ResourceAmount.Zero, imported.CityCampaign.Resources["coal"]);
        Assert.Equal(2m, setup.Start.CityCampaign.Resources[V14SchoolSupplies.ResourceId].Units);
        Assert.Equal(100m, setup.Start.CityCampaign.Resources["coal"].Units);
    }

    [Theory]
    [InlineData(V15IndustrialProgressionBenchmark.PolicyResearchFirst)]
    [InlineData(V15IndustrialProgressionBenchmark.PolicyBreadFirst)]
    [InlineData(V15IndustrialProgressionBenchmark.PolicyHousingBuffer)]
    public void IndustrialPolicyBuysExtractionBeforeAttemptingCoal(string policy)
    {
        var steps = V15IndustrialProgressionBenchmark.Steps("V15-I03", policy);
        var list = steps.ToList();
        Assert.DoesNotContain("craft-pencils", list);
        Assert.True(list.IndexOf("enroll-student") < list.IndexOf("buy-tech-estrazione-1"));
        Assert.True(list.IndexOf("buy-tech-estrazione-1") < list.IndexOf("build-coal-mine"));
        Assert.True(list.IndexOf("build-coal-mine") < list.IndexOf("build-gold-mine"));
    }

    [Fact]
    public void NaturalAndIndustrialBenchmarksUseSameMunicipalInventory()
    {
        var natural = V15NaturalProgressionBenchmark.Run("V15-N01", maxSeconds: 0);
        var advanced = V15IndustrialProgressionBenchmark.Run("V15-I03",
            V15IndustrialProgressionBenchmark.PolicyResearchFirst, maxSeconds: 0);
        Assert.Equal(2000L, natural.InitialPhysicalStockMilliunits["pencils"]);
        Assert.Equal(100000L, natural.InitialPhysicalStockMilliunits["coal"]);
        Assert.Equal(natural.InitialPhysicalStockMilliunits.OrderBy(p => p.Key),
            advanced.InitialPhysicalStockMilliunits.OrderBy(p => p.Key));
    }
}
