using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Core.Food;

/// <summary>
/// F1-D05e.2b.2b: v15 city transactions executed on the authoritative checkpoint.
/// Pricing, tier rules and school recipes remain shared with the approved model;
/// V14CampaignSession is never opened to perform a v15 player command.
/// Every successful action validates the complete city, staff and food checkpoint
/// before committing; rejected requests keep the original Snapshot reference.
/// </summary>
public sealed partial class V15FoodIndustryCampaignSession
{
    private static bool IsTier(ProductionTier tier) =>
        tier is ProductionTier.Base or ProductionTier.Large or ProductionTier.Advanced;

    private TieredBuildingPortfolio NativePortfolio(string familyId)
    {
        var snapshot = _current.CityCampaign.ProductiveFamilies.Single(f => f.FamilyId == familyId);
        return TieredBuildingPortfolio.Create(snapshot.FamilyId, snapshot.Base,
            snapshot.Large, snapshot.Advanced);
    }

    private ImmutableArray<V15ProductiveFamilySnapshot> NativeReplace(TieredBuildingPortfolio replacement) =>
        _current.CityCampaign.ProductiveFamilies.Select(f => f.FamilyId == replacement.FamilyId
            ? new V15ProductiveFamilySnapshot(replacement.FamilyId, replacement.Base,
                replacement.Large, replacement.Advanced) : f).ToImmutableArray();

    private ImmutableArray<V15PurchaseCounter> NativeCounter(string familyId, int acquired) =>
        _current.CityCampaign.PurchaseCounters.Select(c => c.FamilyId == familyId
            ? c with { Acquired = acquired } : c).ToImmutableArray();

    private ConstructionAcquisitionHistory NativeHistory() =>
        ConstructionAcquisitionHistory.FromOwned(_current.CityCampaign.PurchaseCounters
            .Select(c => (c.FamilyId, c.Acquired)));

    private static V14TierActionResult TierResult(V14TierActionFailure failure,
        ResourceBundle? cost = null) => new(failure, cost ?? ResourceBundle.Empty);

    private static V14TierActionFailure PurchaseFailure(TieredPurchaseFailure failure) => failure switch
    {
        TieredPurchaseFailure.MissingResearch => V14TierActionFailure.MissingResearch,
        TieredPurchaseFailure.MissingSourceBuilding => V14TierActionFailure.MissingSourceBuilding,
        TieredPurchaseFailure.InsufficientResources => V14TierActionFailure.InsufficientResources,
        _ => throw new ArgumentOutOfRangeException(nameof(failure))
    };

    public V14CivicConstructionResult BuildCityBuilding(string? buildingId, int quantity)
    {
        static V14CivicConstructionResult Fail(V14CivicConstructionFailure failure,
            ResourceBundle? cost = null) => new(failure, cost ?? ResourceBundle.Empty);
        if (buildingId == V15LegacyProducerAdapter.RetiredFamilyId)
            return Fail(V14CivicConstructionFailure.UnknownBuilding);
        if (quantity <= 0) return Fail(V14CivicConstructionFailure.InvalidQuantity);
        if (buildingId is null || !_baseline.Catalog.BuildingsById.TryGetValue(buildingId, out var definition))
            return Fail(V14CivicConstructionFailure.UnknownBuilding);
        if (definition.IsScenarioFixed) return Fail(V14CivicConstructionFailure.FixedBuilding);
        if (definition.Recipe is not null || _nativeFamilies.ContainsKey(buildingId))
            return Fail(V14CivicConstructionFailure.TieredBuilding);
        if (definition.Prerequisites.Any(id => !HasPrerequisite(id)))
            return Fail(V14CivicConstructionFailure.MissingPrerequisite);
        var city = _current.CityCampaign;
        try
        {
            var old = city.OtherBuildings.Single(b => b.BuildingId == buildingId);
            var count = checked(old.Count + quantity);
            var history = city.PurchaseCounters.Single(c => c.FamilyId == buildingId);
            var acquired = checked(history.Acquired + quantity);
            if (definition.HousingPerBuilding > 0)
            {
                var housing = city.OtherBuildings.Sum(b => (long)b.Count *
                    _baseline.Catalog.BuildingsById[b.BuildingId].HousingPerBuilding);
                if (housing + (long)quantity * definition.HousingPerBuilding > int.MaxValue)
                    return Fail(V14CivicConstructionFailure.NumericLimit);
            }
            if (definition.StoragePerBuilding is { } storage)
                _ = V15CityStorage.Calculate(_baseline, city) + storage.Scale(quantity);
            var cost = ProgressiveConstructionPricing.QuoteNewBuildings(definition.Cost, 1m,
                V15TierRules.GrowthFactor, history.Acquired, quantity);
            if (!city.Resources.Covers(cost))
                return Fail(V14CivicConstructionFailure.InsufficientResources, cost);
            CommitCity(city with
            {
                Resources = city.Resources - cost,
                OtherBuildings = city.OtherBuildings.Select(b => b.BuildingId == buildingId
                    ? b with { Count = count } : b).ToImmutableArray(),
                PurchaseCounters = NativeCounter(buildingId, acquired)
            });
            return Fail(V14CivicConstructionFailure.None, cost);
        }
        catch (Exception e) when (e is OverflowException or ArgumentOutOfRangeException)
        {
            return Fail(V14CivicConstructionFailure.NumericLimit);
        }
    }

    public V14TierActionResult BuildCityProducer(string? familyId, ProductionTier tier, int quantity)
    {
        if (familyId == V15LegacyProducerAdapter.RetiredFamilyId)
            return TierResult(V14TierActionFailure.UnknownFamily);
        if (quantity <= 0) return TierResult(V14TierActionFailure.InvalidQuantity);
        if (!IsTier(tier)) return TierResult(V14TierActionFailure.InvalidTier);
        if (familyId is null || !_nativeFamilies.TryGetValue(familyId, out var definition))
            return TierResult(V14TierActionFailure.UnknownFamily);
        if (definition.Source.Prerequisites.Any(id => !HasPrerequisite(id)))
            return TierResult(V14TierActionFailure.MissingPrerequisite);
        var city = _current.CityCampaign;
        try
        {
            var attempt = TieredConstruction.TryBuild(definition, NativePortfolio(familyId), tier,
                quantity, V15TierRules.GrowthFactor,
                city.UnlockedTechnologyIds.ToHashSet(StringComparer.Ordinal), city.Resources, NativeHistory());
            if (!attempt.Succeeded) return TierResult(PurchaseFailure(attempt.Failure), attempt.Cost);
            CommitCity(city with
            {
                Resources = attempt.Resources,
                ProductiveFamilies = NativeReplace(attempt.Portfolio),
                PurchaseCounters = NativeCounter(familyId, attempt.History.Acquired(familyId))
            });
            return TierResult(V14TierActionFailure.None, attempt.Cost);
        }
        catch (Exception e) when (e is OverflowException or ArgumentOutOfRangeException)
        {
            return TierResult(V14TierActionFailure.NumericLimit);
        }
    }

    public V14TierActionResult UpgradeCityProducer(string? familyId, ProductionTier from, ProductionTier to)
    {
        if (familyId == V15LegacyProducerAdapter.RetiredFamilyId)
            return TierResult(V14TierActionFailure.UnknownFamily);
        if (!IsTier(from) || !IsTier(to) || from >= to)
            return TierResult(V14TierActionFailure.InvalidTier);
        if (familyId is null || !_nativeFamilies.TryGetValue(familyId, out var definition))
            return TierResult(V14TierActionFailure.UnknownFamily);
        if (definition.Source.Prerequisites.Any(id => !HasPrerequisite(id)))
            return TierResult(V14TierActionFailure.MissingPrerequisite);
        var city = _current.CityCampaign;
        try
        {
            var attempt = TieredConstruction.TryUpgrade(definition, NativePortfolio(familyId),
                from, to, V15TierRules.GrowthFactor,
                city.UnlockedTechnologyIds.ToHashSet(StringComparer.Ordinal), city.Resources, NativeHistory());
            if (!attempt.Succeeded) return TierResult(PurchaseFailure(attempt.Failure), attempt.Cost);
            CommitCity(city with
            {
                Resources = attempt.Resources,
                ProductiveFamilies = NativeReplace(attempt.Portfolio)
                // Upgrades intentionally do not advance the acquisition counter.
            });
            return TierResult(V14TierActionFailure.None, attempt.Cost);
        }
        catch (Exception e) when (e is OverflowException or ArgumentOutOfRangeException)
        {
            return TierResult(V14TierActionFailure.NumericLimit);
        }
    }

    public V14TierActionFailure AssignCityProducerWorkers(string? familyId,
        ProductionTier tier, int workers) => NativeStaff(familyId, tier, workers, release: false);

    public V14TierActionFailure ReleaseCityProducerWorkers(string? familyId,
        ProductionTier tier, int workers) => NativeStaff(familyId, tier, workers, release: true);

    private V14TierActionFailure NativeStaff(string? familyId, ProductionTier tier,
        int workers, bool release)
    {
        if (familyId == V15LegacyProducerAdapter.RetiredFamilyId)
            return V14TierActionFailure.UnknownFamily;
        if (workers <= 0) return V14TierActionFailure.InvalidQuantity;
        if (!IsTier(tier)) return V14TierActionFailure.InvalidTier;
        if (familyId is null || !_nativeFamilies.ContainsKey(familyId))
            return V14TierActionFailure.UnknownFamily;
        try
        {
            var portfolio = NativePortfolio(familyId);
            var occupancy = portfolio.At(tier);
            if (release && workers > occupancy.AssignedWorkers)
                return V14TierActionFailure.InsufficientAssignedWorkers;
            var vacancies = (long)occupancy.Buildings * (tier switch
            {
                ProductionTier.Base => 1,
                ProductionTier.Large => 3,
                _ => 10
            }) - occupancy.AssignedWorkers;
            if (!release && workers > vacancies)
                return V14TierActionFailure.InsufficientWorkplaces;
            if (!release && workers > AvailableWorkers)
                return V14TierActionFailure.InsufficientPopulation;
            var next = release ? portfolio.Release(tier, workers) :
                portfolio.Assign(tier, workers, checked((int)AvailableWorkers));
            CommitCity(_current.CityCampaign with { ProductiveFamilies = NativeReplace(next) });
            return V14TierActionFailure.None;
        }
        catch (OverflowException)
        {
            return V14TierActionFailure.NumericLimit;
        }
    }

    public V14SchoolConstructionResult BuildSchool(int quantity)
    {
        static V14SchoolConstructionResult Fail(V14SchoolConstructionFailure failure,
            ResourceBundle? cost = null) => new(failure, cost ?? ResourceBundle.Empty);
        if (quantity <= 0) return Fail(V14SchoolConstructionFailure.InvalidQuantity);
        if (!HasPrerequisite("food-depot") ||
            V15SchoolBuilding.Prerequisites.Any(id => !HasPrerequisite(id)))
            return Fail(V14SchoolConstructionFailure.MissingPrerequisite);
        var city = _current.CityCampaign;
        try
        {
            var history = city.PurchaseCounters.Single(c => c.FamilyId == V14CampaignSnapshots.SchoolId);
            var cost = ProgressiveConstructionPricing.QuoteNewBuildings(V15SchoolBuilding.Cost,
                1m, V15SchoolBuilding.GrowthFactor, history.Acquired, quantity);
            if (!city.Resources.Covers(cost))
                return Fail(V14SchoolConstructionFailure.InsufficientResources, cost);
            CommitCity(city with
            {
                SchoolCount = checked(city.SchoolCount + quantity),
                Resources = city.Resources - cost,
                PurchaseCounters = NativeCounter(V14CampaignSnapshots.SchoolId,
                    checked(history.Acquired + quantity))
            });
            return Fail(V14SchoolConstructionFailure.None, cost);
        }
        catch (Exception e) when (e is OverflowException or ArgumentOutOfRangeException)
        {
            return Fail(V14SchoolConstructionFailure.NumericLimit);
        }
    }

    public PencilCraftFailure CheckPencilCraft(int batches) => NativePencilCraft(batches).Failure;

    public PencilCraftFailure CraftPencils(int batches)
    {
        var outcome = NativePencilCraft(batches);
        if (outcome.Failure != PencilCraftFailure.None) return outcome.Failure;
        CommitCity(_current.CityCampaign with { Resources = outcome.Resources! });
        return PencilCraftFailure.None;
    }

    private (PencilCraftFailure Failure, ResourceBundle? Resources) NativePencilCraft(int batches)
    {
        if (batches <= 0) return (PencilCraftFailure.InvalidQuantity, null);
        var city = _current.CityCampaign;
        if (city.SchoolCount == 0) return (PencilCraftFailure.MissingSchool, null);
        try
        {
            var inputs = V14SchoolSupplies.Inputs(batches);
            if (!city.Resources.Covers(inputs)) return (PencilCraftFailure.InsufficientMaterials, null);
            var result = city.Resources - inputs + V14SchoolSupplies.Output(batches);
            if (!V15CityStorage.Calculate(_baseline, city).Covers(result))
                return (PencilCraftFailure.InsufficientStorage, null);
            return (PencilCraftFailure.None, result);
        }
        catch (Exception e) when (e is OverflowException or ArgumentOutOfRangeException)
        {
            return (PencilCraftFailure.NumericLimit, null);
        }
    }
}
