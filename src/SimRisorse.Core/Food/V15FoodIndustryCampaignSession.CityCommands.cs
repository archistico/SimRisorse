using System.Collections.Immutable;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Military;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Core.Food;

/// <summary>RC52 / F1-D05d.4a: city, school, research and military commands for
/// the v15 headless campaign. Each command stages a complete checkpoint and
/// commits only after v15 validates both staff pools and all inventories.</summary>
public sealed partial class V15FoodIndustryCampaignSession
{
    // Reuse the approved policy values but execute civic commands directly against
    // the v15 checkpoint. No disposable V14CampaignSession is instantiated.
    private static readonly V14TierConstructionRules V15TierRules = V14TierConstructionRules.Candidate;
    private static readonly V14SchoolConstructionRules V15SchoolBuilding = new(
        V14SchoolConstructionRules.Candidate.Cost,
        V14SchoolConstructionRules.Candidate.GrowthFactor, ["small-house"]);

    private void CommitCity(V15CityCampaignSnapshot city) =>
        Commit(_current with { CityCampaign = city });

    /// <summary>Explicit one-way cleanup of an obsolete producer's buildings and
    /// staff. No stock or research is modified and no historical purchase is refunded.
    /// Once empty, the retired family disappears from the v15 checkpoint;
    /// it is reconstructed only in temporary v14 validation/command projections.</summary>
    public GameActionResult RetireLegacyFoodStore()
    {
        var city = _current.CityCampaign;
        var old = city.ProductiveFamilies.FirstOrDefault(f =>
            f.FamilyId == V15UnifiedCampaignClock.DisabledLegacyProducerId);
        if (old is null ||
            old.Base == default && old.Large == default && old.Advanced == default)
            return GameActionResult.Success;
        CommitCity(city with { ProductiveFamilies = [.. city.ProductiveFamilies.Select(f =>
            f.FamilyId == V15UnifiedCampaignClock.DisabledLegacyProducerId
                ? f with { Base = default, Large = default, Advanced = default }
                : f)] });
        return GameActionResult.Success;
    }

    // Civic/producer/school actions live in V15NativeCityCommands.cs. The
    // public command contract is unchanged for the validated desktop ViewModel.

    public SchoolActionFailure EnrollStudents(int students)
    {
        var city = _current.CityCampaign;
        if (students <= 0) return SchoolActionFailure.InvalidQuantity;
        try
        {
            var borrowedFood = CityWithTemporaryFood();
            var outsideSchool = V15FoodCampaignCheckpoints.BusyOutsideFood(city) -
                city.Military.EnlistedPopulation - city.SchoolResearch.ReservedPopulation +
                _current.Industry.Buildings.Assigned.Total;
            var result = SchoolResearch.Enroll(_school, city.SchoolResearch, borrowedFood,
                city.Population, checked((int)outsideSchool),
                checked((int)city.Military.EnlistedPopulation), city.SchoolCount, students);
            if (!result.Succeeded) return result.Failure;
            var next = _current with
            {
                CityCampaign = city with
                {
                    Resources = RemoveTemporaryFood(result.Resources),
                    SchoolResearch = result.State,
                    PencilPaidStudents = checked(city.PencilPaidStudents +
                        (_school.PencilsPerStudent > 0 ? students : 0))
                },
                Depot = _current.Depot with { ReadyFood = result.Resources[_depotPolicy.ReadyFoodResourceId] }
            };
            Commit(next);
            return SchoolActionFailure.None;
        }
        catch (Exception e) when (e is OverflowException or ArgumentOutOfRangeException)
        { return SchoolActionFailure.NumericLimit; }
    }

    public SchoolActionFailure CancelStudents(int students)
    {
        var city = _current.CityCampaign;
        if (students <= 0) return SchoolActionFailure.InvalidQuantity;
        try
        {
            var prepaid = Math.Min(students, city.PencilPaidStudents);
            var capacity = V15CityStorage.Calculate(_baseline, city);
            // Rebuild with the physical depot's prepared-food capacity, never the
            // obsolete city's food warehouse capacity.
            capacity = ResourceBundle.FromAmounts(capacity.Amounts
                .Where(item => item.Key != _depotPolicy.ReadyFoodResourceId)
                .Append(KeyValuePair.Create(_depotPolicy.ReadyFoodResourceId, Capacities.DepotReady)));
            var result = SchoolResearch.CancelTraining(_school, city.SchoolResearch,
                CityWithTemporaryFood(), capacity, city.SchoolCount, students, prepaid);
            if (!result.Succeeded) return result.Failure;
            Commit(_current with
            {
                CityCampaign = city with
                {
                    Resources = RemoveTemporaryFood(result.Resources),
                    SchoolResearch = result.State,
                    PencilPaidStudents = city.PencilPaidStudents - prepaid
                },
                Depot = _current.Depot with { ReadyFood = result.Resources[_depotPolicy.ReadyFoodResourceId] }
            });
            return SchoolActionFailure.None;
        }
        catch (Exception e) when (e is OverflowException or ArgumentOutOfRangeException)
        { return SchoolActionFailure.NumericLimit; }
    }

    private ResourceBundle CityWithTemporaryFood() =>
        _current.CityCampaign.Resources + ResourceBundle.FromAmounts([
            KeyValuePair.Create(_depotPolicy.ReadyFoodResourceId, _current.Depot.ReadyFood)]);

    private ResourceBundle RemoveTemporaryFood(ResourceBundle resources) =>
        resources - ResourceBundle.FromAmounts([
            KeyValuePair.Create(_depotPolicy.ReadyFoodResourceId,
                resources[_depotPolicy.ReadyFoodResourceId])]);

    public SchoolActionFailure AssignResearchers(int count) => UpdateResearchers(
        SchoolResearch.Assign(_school, _current.CityCampaign.SchoolResearch,
            _current.CityCampaign.SchoolCount, count));

    public SchoolActionFailure UnassignResearchers(int count) => UpdateResearchers(
        SchoolResearch.Unassign(_school, _current.CityCampaign.SchoolResearch,
            _current.CityCampaign.SchoolCount, count));

    public SchoolActionFailure ReturnResearchersToCivilian(int count) => UpdateResearchers(
        SchoolResearch.ReturnToCivilian(_school, _current.CityCampaign.SchoolResearch,
            _current.CityCampaign.SchoolCount, count));

    private SchoolActionFailure UpdateResearchers(SchoolStaffingResult result)
    {
        if (result.Succeeded)
            CommitCity(_current.CityCampaign with { SchoolResearch = result.State });
        return result.Failure;
    }

    public TechnologyPurchaseFailure BuyTechnology(string? technologyId)
    {
        if (string.IsNullOrWhiteSpace(technologyId))
            return TechnologyPurchaseFailure.UnknownTechnology;
        var city = _current.CityCampaign;
        var ledger = ResearchLedger.Restore(ResearchPoints.FromMilliPoints(
            city.ResearchBalanceMilliPoints), city.UnlockedTechnologyIds, _technologies);
        var owned = new HashSet<string>(city.OtherBuildings
            .Where(b => b.Count > 0).Select(b => b.BuildingId), StringComparer.Ordinal);
        foreach (var family in city.ProductiveFamilies)
            if ((long)family.Base.Buildings + family.Large.Buildings + family.Advanced.Buildings > 0)
                owned.Add(family.FamilyId);
        foreach (var site in FoodIndustrySites.Ids)
            if (FoodIndustrySites.Get(_current.Industry.Buildings.Counts, site) > 0)
                owned.Add(site);
        if (city.SchoolCount > 0) owned.Add(V14CampaignSnapshots.SchoolId);
        var attempt = TechnologyPurchases.TryUnlock(_technologies, ledger, technologyId, owned);
        if (attempt.Succeeded)
            CommitCity(city with
            {
                ResearchBalanceMilliPoints = attempt.Ledger.Balance.MilliPoints,
                UnlockedTechnologyIds = attempt.Ledger.UnlockedTechnologies
                    .OrderBy(id => id, StringComparer.Ordinal).ToImmutableArray()
            });
        return attempt.Failure;
    }

    public GameActionResult Enlist(int quantity)
    {
        var city = _current.CityCampaign;
        if (_baseline.MilitaryRules is not { } rules) return new(GameError.MissingBarracks);
        if (quantity <= 0) return new(GameError.InvalidQuantity);
        var barracks = city.OtherBuildings.Single(b => b.BuildingId == rules.BarracksId).Count;
        if (barracks == 0) return new(GameError.MissingBarracks);
        if (quantity > AvailableWorkers) return new(GameError.InsufficientPopulation);
        try
        {
            var busy = checked((int)(V15FoodCampaignCheckpoints.BusyOutsideFood(city) -
                city.Military.EnlistedPopulation + _current.Industry.Buildings.Assigned.Total));
            var attempt = rules.IndividualTraining
                ? IndividualRecruitment.Enlist(rules, city.Military, city.Resources,
                    city.Population, busy, barracks, quantity)
                : MilitaryTraining.Enlist(rules, city.Military, city.Resources,
                    city.Population, busy, barracks, quantity);
            return ApplyRecruitment(attempt);
        }
        catch (OverflowException) { return new(GameError.NumericLimit); }
    }

    public GameActionResult CancelRecruits(int quantity)
    {
        var city = _current.CityCampaign;
        if (_baseline.MilitaryRules is not { } rules) return new(GameError.MissingBarracks);
        var barracks = city.OtherBuildings.Single(b => b.BuildingId == rules.BarracksId).Count;
        var capacity = V15CityStorage.Calculate(_baseline, city);
        var attempt = rules.IndividualTraining
            ? IndividualRecruitment.Cancel(rules, city.Military, city.Resources, capacity, barracks, quantity)
            : MilitaryTraining.CancelWaiting(rules, city.Military, city.Resources, capacity, barracks, quantity);
        return ApplyRecruitment(attempt);
    }

    public GameActionResult DischargeSoldiers(int quantity)
    {
        var city = _current.CityCampaign;
        if (_baseline.MilitaryRules is not { } rules) return new(GameError.MissingBarracks);
        var barracks = city.OtherBuildings.Single(b => b.BuildingId == rules.BarracksId).Count;
        return ApplyRecruitment(MilitaryTraining.Discharge(rules, city.Military,
            city.Resources, barracks, quantity));
    }

    private GameActionResult ApplyRecruitment(RecruitmentAttempt attempt)
    {
        if (attempt.Failure != RecruitmentFailure.None)
            return new(attempt.Failure switch
            {
                RecruitmentFailure.InvalidQuantity => GameError.InvalidQuantity,
                RecruitmentFailure.MissingBarracks => GameError.MissingBarracks,
                RecruitmentFailure.InsufficientPopulation => GameError.InsufficientPopulation,
                RecruitmentFailure.InsufficientSwords => GameError.InsufficientSwords,
                RecruitmentFailure.InsufficientSoldiers => GameError.InsufficientSoldiers,
                RecruitmentFailure.InsufficientQueuedRecruits => GameError.InsufficientQueuedRecruits,
                RecruitmentFailure.InsufficientStorage => GameError.InsufficientStorage,
                RecruitmentFailure.DischargeUnavailable => GameError.DischargeUnavailable,
                _ => GameError.NumericLimit
            });
        CommitCity(_current.CityCampaign with { Resources = attempt.Resources, Military = attempt.State });
        return GameActionResult.Success;
    }
}
