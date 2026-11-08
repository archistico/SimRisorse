using SimRisorse.Application.Benchmarks;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Infrastructure.Persistence;

namespace SimRisorse.Infrastructure.Experimental;

/// <summary>
/// RC99 opt-in, non-destructive format bridge for the isolated Curve A laboratory.
/// Import reads an existing playable generation-6 v15 save but NEVER changes it.
/// Export writes a NEW ordinary playable v15 save only when no materials remain
/// pending in escrow. It does not enable financing in the normal game or UI.
/// The composite database must be retained separately for its payment receipts.
/// </summary>
public sealed class E4bV15FundingSaveBridge
{
    private readonly GameScenario _baseline;
    private readonly SchoolResearchRules _school;
    private readonly TechnologyCatalog _technologies;
    private readonly FoodConversionPolicy _food;

    public E4bV15FundingSaveBridge(GameScenario baseline, SchoolResearchRules school,
        TechnologyCatalog technologies, FoodConversionPolicy food)
    {
        _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        _school = school ?? throw new ArgumentNullException(nameof(school));
        _technologies = technologies ?? throw new ArgumentNullException(nameof(technologies));
        _food = food ?? throw new ArgumentNullException(nameof(food));
    }

    /// <summary>
    /// Read a normal native save and create a NEW laboratory composite database.
    /// A private stage is validated and moved into place only after successful
    /// initialization. No file is overwritten and the input save is read-only.
    /// </summary>
    public E4bV15FundingShadow ImportNativeToNewComposite(string nativeSavePath,
        string compositePath)
    {
        var (source, target) = DistinctPaths(nativeSavePath, compositePath);
        RequireFreshTarget(target);
        var campaign = Native(source).Load();
        var stage = TemporarySibling(target);
        try
        {
            var stagingStore = Composite(stage);
            stagingStore.Create(campaign);
            var verified = stagingStore.Load(); // Validate the image before publication.
            File.Move(stage, target); // No overwrite, including concurrent target creation.
            return verified;
        }
        finally { DeleteStage(stage); }
    }

    /// <summary>
    /// Export one validated snapshot into a NEW ordinary native save. A pending
    /// financing is not representable in generation 6, so reject it rather than
    /// silently discarding the player's already deposited materials.
    /// Completed buildings are included in the exported city; the experimental
    /// escrow/operation receipts stay ONLY in the original composite database.
    /// </summary>
    public V15FoodCampaignCheckpoint ExportToNewNativeSave(string compositePath,
        string nativeSavePath)
    {
        var (source, target) = DistinctPaths(compositePath, nativeSavePath);
        RequireFreshTarget(target);
        if (!File.Exists(source)) throw new FileNotFoundException("The composite source does not exist.", source);
        var shadow = Composite(source).Load();
        if (shadow.IsFunding || !shadow.Funding.Escrowed.Equals(ResourceBundle.Empty) ||
            !shadow.Funding.Reconciles())
            throw new InvalidOperationException(
                "Pending or unbalanced funding cannot be exported to a native v15 save.");
        var stage = TemporarySibling(target);
        try
        {
            Native(stage).Save(shadow.Campaign); // Native Save validates and verifies a round-trip.
            var verified = Native(stage).Load();
            if (!EquivalentEssentials(shadow.Campaign, verified))
                throw new InvalidDataException("The staged native city is not equivalent to the laboratory city.");
            File.Move(stage, target); // Never overwrite a previous player save.
            return verified;
        }
        finally { DeleteStage(stage); }
    }

    private V15SqliteFoodSaveStore Native(string path) =>
        new(path, _baseline, _school, _technologies, _food);

    private E4bV15FundingCompositeSqliteStore Composite(string path) =>
        new(path, _baseline, _school, _technologies, _food);

    private static bool EquivalentEssentials(V15FoodCampaignCheckpoint before,
        V15FoodCampaignCheckpoint after)
    {
        var a = before.CityCampaign;
        var b = after.CityCampaign;
        return before.CrewMode == after.CrewMode &&
            a.ScenarioId == b.ScenarioId && a.ElapsedTicks == b.ElapsedTicks &&
            a.Population == b.Population && a.Resources.Equals(b.Resources) &&
            a.SchoolCount == b.SchoolCount &&
            a.ProductiveFamilies.OrderBy(x => x.FamilyId, StringComparer.Ordinal)
                .SequenceEqual(b.ProductiveFamilies.OrderBy(x => x.FamilyId, StringComparer.Ordinal)) &&
            a.PurchaseCounters.OrderBy(x => x.FamilyId, StringComparer.Ordinal)
                .SequenceEqual(b.PurchaseCounters.OrderBy(x => x.FamilyId, StringComparer.Ordinal)) &&
            before.Depot.Equals(after.Depot) && before.Industry.Equals(after.Industry);
    }

    private static (string Source, string Target) DistinctPaths(string sourcePath,
        string targetPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
        var source = Path.GetFullPath(sourcePath);
        var target = Path.GetFullPath(targetPath);
        // Windows is the supported desktop target. A case-insensitive comparison
        // also prevents unsafe aliasing when tests run on other systems.
        if (string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Source and destination must be different files.", nameof(targetPath));
        return (source, target);
    }

    private static void RequireFreshTarget(string target)
    {
        if (!Directory.Exists(Path.GetDirectoryName(target)))
            throw new DirectoryNotFoundException("Create the destination directory first.");
        if (File.Exists(target) || Directory.Exists(target))
            throw new IOException("The destination already exists; no existing save may be replaced.");
    }

    private static string TemporarySibling(string target) => Path.Combine(
        Path.GetDirectoryName(target)!, ".rc99-bridge-" + Guid.NewGuid().ToString("N") + ".sqlite3");

    private static void DeleteStage(string path)
    {
        // Never mask the original validation or I/O error during rollback.
        try { if (File.Exists(path)) File.Delete(path); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
