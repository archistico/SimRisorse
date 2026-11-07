using System.Globalization;
using System.Text.Json;
using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using Xunit;

namespace SimRisorse.Application.Tests;

public sealed class E4bCrewOperationalStudyTests
{
    [Fact]
    public void ControlledScenariosCoverThresholdsAndThreeTierSizes()
    {
        var report = new E4bCrewOperationalStudy().Run();
        Assert.Equal("controlled-projection/not-applied", report.Mode);
        Assert.Equal(10, report.CrewSize);
        Assert.Equal(10, report.Rows.Count);
        Assert.Equal(10, report.Rows.Select(r => r.Scenario).Distinct().Count());
        Assert.Equal(new[] { 0, 0, 1, 1, 2 }, report.Rows.Take(5).Select(r => r.RunnableModules));
        Assert.Equal(2, report.Rows.Single(r => r.Scenario == "two-base-20").OwnedModules);
        Assert.Equal(3, report.Rows.Single(r => r.Scenario == "large-30").RunnableModules);
        Assert.Equal(10, report.Rows.Single(r => r.Scenario == "advanced-100").RunnableModules);
    }

    [Fact]
    public void MeasuredRowsUseRealGoldCoalAndCoinRecipeWithoutPartialInputs()
    {
        var report = new E4bCrewOperationalStudy().Run();
        foreach (var row in report.Rows)
        {
            Assert.Equal(row.RunnableModules, (int)row.Consumed["gold"].Units);
            Assert.Equal(row.RunnableModules, (int)row.Consumed["coal"].Units);
            Assert.Equal(3m * row.RunnableModules, row.Produced["coins"].Units);
        }
        Assert.True(report.Rows.Single(r => r.Scenario == "mint-no-gold").BlockReason
            .HasFlag(ProductionBlockReason.MissingInputs));
        Assert.True(report.Rows.Single(r => r.Scenario == "coin-storage-full").BlockReason
            .HasFlag(ProductionBlockReason.StorageFull));
        Assert.Equal(ResourceBundle.Empty, report.Rows.Single(r => r.Scenario == "two-base-9").Consumed);
    }

    [Fact]
    public void ExistingCatalogAndNormalProductionRulesRemainUnchanged()
    {
        var baseline = V15BenchmarkFixture.Create().Baseline;
        var before = JsonSerializer.Serialize(baseline.Catalog.Buildings);
        var report = new E4bCrewOperationalStudy().Run();
        Assert.Equal(baseline.Catalog.RulesVersion, report.CatalogRulesVersion);
        Assert.Equal(1, baseline.Catalog.BuildingsById["mint"].WorkplacesPerBuilding);
        Assert.Equal(before, JsonSerializer.Serialize(V15BenchmarkFixture.Create().Baseline.Catalog.Buildings));
    }

    [Fact]
    public void ReportsAreReproducibleAndInvariantUnderItalianCulture()
    {
        var first = new E4bCrewOperationalStudy().Run();
        var second = new E4bCrewOperationalStudy().Run();
        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        var csv = E4bCrewOperationalStudy.ToCsv(first);
        var markdown = E4bCrewOperationalStudy.ToMarkdown(first);
        Assert.Equal(11, csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.All(csv.Split('\n', StringSplitOptions.RemoveEmptyEntries),
            line => Assert.Equal(13, line.Split(',').Length));
        Assert.Contains("NON partita", markdown);
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");
            Assert.Equal(csv, E4bCrewOperationalStudy.ToCsv(second));
            Assert.Equal(markdown, E4bCrewOperationalStudy.ToMarkdown(second));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }
}
