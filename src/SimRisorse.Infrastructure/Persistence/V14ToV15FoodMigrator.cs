using System.Security.Cryptography;
using SimRisorse.Application.Persistence;
using SimRisorse.Core.Content;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Infrastructure.Persistence;

/// <summary>Read-only analysis of a v14 food checkpoint before a copy-on-write conversion.</summary>
public sealed record V14ToV15FoodPreview(
    V14FoodStockMigrationPlan Plan,
    FoodMigrationCapacityCheck Capacity,
    string SourceFileSha256);

/// <summary>Explicit copy result. AlreadyPresent means that the existing v15 file was
/// verified as identical to a fresh migration; it was not overwritten.</summary>
public sealed record V14ToV15FoodMigrationReceipt(
    string SourcePath,
    string DestinationPath,
    string SourceFileSha256,
    FoodMigrationCapacityCheck Capacity,
    bool AlreadyPresent);

/// <summary>
/// F1-D05c: conservative, opt-in v14 -> v15 food migration. Does not switch the
/// active game, edit or delete v14, or overwrite any existing v15 checkpoint.
/// A new v15 is built and verified at a temporary path before an exclusive move.
/// An identical existing destination is treated as an idempotent no-op.
/// </summary>
public sealed class V14ToV15FoodMigrator
{
    private readonly V14SqliteSaveStore _source;
    private readonly V15SqliteFoodSaveStore _destination;
    private readonly GameScenario _baseline;
    private readonly SchoolResearchRules _school;
    private readonly TechnologyCatalog _technologies;
    private readonly FoodConversionPolicy _policy;

    public V14ToV15FoodMigrator(string sourceV14Path, string destinationV15Path,
        GameScenario baselineV13, SchoolResearchRules schoolRules,
        TechnologyCatalog technologies, FoodConversionPolicy policy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceV14Path);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationV15Path);
        _baseline = baselineV13 ?? throw new ArgumentNullException(nameof(baselineV13));
        _school = schoolRules ?? throw new ArgumentNullException(nameof(schoolRules));
        _technologies = technologies ?? throw new ArgumentNullException(nameof(technologies));
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _source = new V14SqliteSaveStore(sourceV14Path, _baseline, _school, _technologies);
        _destination = new V15SqliteFoodSaveStore(destinationV15Path, _baseline, _school, _technologies, _policy);
        EnsureDifferentFiles();
    }

    public V14ToV15FoodPreview Preview(ResourceBundle rawCapacities, ResourceAmount readyFoodCapacity)
    {
        ArgumentNullException.ThrowIfNull(rawCapacities);
        EnsureDifferentFiles();
        var firstHash = SourceHash();
        var plan = _source.PreviewFoodMigration(_policy);
        var capacity = V14FoodStockMigration.AssessCapacity(plan, _policy, rawCapacities, readyFoodCapacity);
        if (!string.Equals(firstHash, SourceHash(), StringComparison.Ordinal))
            throw new GameSaveException(GameSaveError.Superseded,
                "The v14 source file changed during the food migration preview.");
        return new(plan, capacity, firstHash);
    }

    /// <summary>
    /// Creates a new v15 file only after full on-disk round-trip verification.
    /// The v14 database remains untouched and is the rollback checkpoint.
    /// Call with the game closed: concurrent SQLite WAL writers cannot be
    /// made safe merely by hashing the main database file.
    /// Over-capacity legacy stocks are copied intact and reported, never clipped.
    /// </summary>
    public V14ToV15FoodMigrationReceipt MigrateCopy(
        ResourceBundle rawCapacities, ResourceAmount readyFoodCapacity)
    {
        EnsureDifferentFiles();
        var preview = Preview(rawCapacities, readyFoodCapacity);
        var expected = V15FoodCampaignCheckpoints.FromMigrationPlan(preview.Plan, _policy);
        VerifySourceUnchanged(preview);

        var target = _destination.DatabasePath;
        if (File.Exists(target))
        {
            // Idempotency is permitted only for the *same complete* converted
            // snapshot. Never treat any other v15 campaign as safe to overwrite.
            var existing = _destination.Load();
            AssertSameCheckpoint(expected, existing);
            VerifySourceUnchanged(preview);
            return new(_source.DatabasePath, target, preview.SourceFileSha256,
                preview.Capacity, AlreadyPresent: true);
        }

        var directory = Path.GetDirectoryName(target)!;
        string? stagedPath = null;
        try
        {
            Directory.CreateDirectory(directory);
            stagedPath = Path.Combine(directory,
                $".{Path.GetFileName(target)}.{Guid.NewGuid():N}.import.sqlite3");
            var staged = new V15SqliteFoodSaveStore(stagedPath,
                _baseline, _school, _technologies, _policy);
            staged.Save(expected);
            AssertSameCheckpoint(expected, staged.Load());
            VerifySourceUnchanged(preview);
            // File.Move without overwrite is an exclusive promotion: a new file
            // created by another caller is never silently replaced.
            File.Move(stagedPath, target);
            stagedPath = null;
            AssertSameCheckpoint(expected, _destination.Load());
            return new(_source.DatabasePath, target, preview.SourceFileSha256,
                preview.Capacity, AlreadyPresent: false);
        }
        catch (IOException ex)
        {
            throw new GameSaveException(GameSaveError.Unavailable,
                "Cannot promote the verified v15 food checkpoint without overwriting another file.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new GameSaveException(GameSaveError.Unavailable,
                "The v15 destination is not writable.", ex);
        }
        finally
        {
            if (stagedPath is not null)
            {
                try { File.Delete(stagedPath); }
                catch (IOException) { /* Preserve the primary error; no user data is deleted. */ }
                catch (UnauthorizedAccessException) { /* Preserve the primary error. */ }
            }
        }
    }

    private void VerifySourceUnchanged(V14ToV15FoodPreview preview)
    {
        var current = _source.Load();
        if (!SameCampaign(preview.Plan.SourceCheckpoint, current) ||
            !string.Equals(preview.SourceFileSha256, SourceHash(), StringComparison.Ordinal))
            throw new GameSaveException(GameSaveError.Superseded,
                "The original v14 checkpoint changed; close the game and restart migration.");
    }

    private void AssertSameCheckpoint(V15FoodCampaignCheckpoint expected, V15FoodCampaignCheckpoint actual)
    {
        if (!actual.Industry.IsEmpty ||
            !SameCampaign(expected.CityCampaign.ToInherited(), actual.CityCampaign.ToInherited()) ||
            !expected.Depot.RawFood.Equals(actual.Depot.RawFood) ||
            expected.Depot.ReadyFood != actual.Depot.ReadyFood)
            throw new GameSaveException(GameSaveError.Superseded,
                "The destination is not the same complete v14-to-v15 food migration; no overwrite is allowed.");
        var projected = V15FoodCampaignCheckpoints.ToLegacyPreview(actual,
            _baseline, _school, _technologies, _policy);
        if (!SameCampaign(projected, _source.Load()))
            throw new GameSaveException(GameSaveError.Superseded,
                "The migrated checkpoint cannot reconstruct the complete current v14 campaign.");
    }

    private static bool SameCampaign(V14CampaignSnapshot a, V14CampaignSnapshot b) =>
        a.FormatVersion == b.FormatVersion &&
        string.Equals(a.ScenarioId, b.ScenarioId, StringComparison.Ordinal) &&
        string.Equals(a.RulesVersion, b.RulesVersion, StringComparison.Ordinal) &&
        a.ElapsedTicks == b.ElapsedTicks && a.Population == b.Population &&
        a.Resources.Equals(b.Resources) && a.SchoolCount == b.SchoolCount &&
        a.GrowthProgressSeconds == b.GrowthProgressSeconds &&
        a.GrowthRemainderTwentieths == b.GrowthRemainderTwentieths &&
        Equals(a.Military, b.Military) &&
        a.SchoolResearch.WaitingStudents == b.SchoolResearch.WaitingStudents &&
        a.SchoolResearch.QualifiedResearchers == b.SchoolResearch.QualifiedResearchers &&
        a.SchoolResearch.ActiveResearchers == b.SchoolResearch.ActiveResearchers &&
        a.SchoolResearch.TrainingProgressSeconds.SequenceEqual(b.SchoolResearch.TrainingProgressSeconds) &&
        a.ResearchBalanceMilliPoints == b.ResearchBalanceMilliPoints &&
        a.PencilPaidStudents == b.PencilPaidStudents &&
        a.OtherBuildings.OrderBy(x => x.BuildingId, StringComparer.Ordinal)
            .SequenceEqual(b.OtherBuildings.OrderBy(x => x.BuildingId, StringComparer.Ordinal)) &&
        a.ProductiveFamilies.OrderBy(x => x.FamilyId, StringComparer.Ordinal)
            .SequenceEqual(b.ProductiveFamilies.OrderBy(x => x.FamilyId, StringComparer.Ordinal)) &&
        a.PurchaseCounters.OrderBy(x => x.FamilyId, StringComparer.Ordinal)
            .SequenceEqual(b.PurchaseCounters.OrderBy(x => x.FamilyId, StringComparer.Ordinal)) &&
        a.UnlockedTechnologyIds.OrderBy(x => x, StringComparer.Ordinal)
            .SequenceEqual(b.UnlockedTechnologyIds.OrderBy(x => x, StringComparer.Ordinal));

    private string SourceHash()
    {
        if (!File.Exists(_source.DatabasePath))
            throw new GameSaveException(GameSaveError.NotFound, "The original v14 checkpoint does not exist.");
        try
        {
            using var stream = new FileStream(_source.DatabasePath, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return Convert.ToHexString(SHA256.HashData(stream));
        }
        catch (IOException ex)
        {
            throw new GameSaveException(GameSaveError.Unavailable, "Cannot fingerprint the original v14 database.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new GameSaveException(GameSaveError.Unavailable, "Cannot read the original v14 database.", ex);
        }
    }

    private void EnsureDifferentFiles()
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (string.Equals(_source.DatabasePath, _destination.DatabasePath, comparison))
            throw new GameSaveException(GameSaveError.UnsupportedFormat,
                "v14 and v15 must use two different SQLite paths.");
    }
}
