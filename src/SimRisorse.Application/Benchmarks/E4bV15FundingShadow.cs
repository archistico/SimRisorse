using System.Collections.Immutable;
using SimRisorse.Core.Content;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;
using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Benchmarks;

/// <summary>
/// RC94 experimental composite checkpoint: the proposed v15 city inventory and
/// the Curve A escrow travel together. This is a SHADOW of a v15 checkpoint,
/// not a playable session, a player save, or authorization to create buildings.
/// There is deliberately no synthetic material-delivery or mint-completion API.
/// </summary>
public sealed record E4bV15FundingShadowCheckpoint(
    int Version, long Revision, V15FoodCampaignCheckpoint Campaign,
    E4bFundingCheckpoint Funding);

public enum E4bV15FundingReadiness
{
    NoPendingMint, MissingPayment, MissingPrerequisite, MissingResearch,
    ReadyForFutureAtomicCommit
}

/// <summary>
/// Immutable opt-in experimental mint funding checkpoint. Each escrow transfer
/// shares the validated v15 inventory. RC98 can complete a funded mint in
/// this isolated shadow, but never touches playable state or player saves.
/// Authoritative player-save integration remains a separate milestone.
/// </summary>
public sealed class E4bV15FundingShadow
{
    public const int Version = 1;
    public const string Mode = "curve-A/v15-shadow/not-playable/not-persisted";

    private readonly GameScenario _baseline;
    private readonly SchoolResearchRules _school;
    private readonly TechnologyCatalog _technologies;
    private readonly FoodConversionPolicy _food;

    private E4bV15FundingShadow(GameScenario baseline, SchoolResearchRules school,
        TechnologyCatalog technologies, FoodConversionPolicy food,
        V15FoodCampaignCheckpoint campaign, E4bCurveAFundingSandbox funding, long revision)
    {
        _baseline = baseline;
        _school = school;
        _technologies = technologies;
        _food = food;
        Campaign = campaign;
        Funding = funding;
        Revision = revision;
    }

    public V15FoodCampaignCheckpoint Campaign { get; }
    public E4bCurveAFundingSandbox Funding { get; }
    public long Revision { get; }
    public bool IsFunding => Funding.HasPendingPurchase;
    public E4bV15FundingShadowCheckpoint ExportCheckpoint() =>
        new(Version, Revision, Campaign, Funding.ExportCheckpoint());

    public static E4bV15FundingShadow Open(GameScenario baseline, SchoolResearchRules school,
        TechnologyCatalog technologies, FoodConversionPolicy food,
        V15FoodCampaignCheckpoint campaign)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(school);
        ArgumentNullException.ThrowIfNull(technologies);
        ArgumentNullException.ThrowIfNull(food);
        ArgumentNullException.ThrowIfNull(campaign);
        V15FoodCampaignCheckpoints.Validate(campaign, baseline, school, technologies, food);
        var capacity = V15CityStorage.Calculate(baseline, campaign.CityCampaign);
        var owned = MintCounter(campaign);
        var mint = campaign.CityCampaign.ProductiveFamilies.Single(f => f.FamilyId == "mint");
        var origin = new E4bMintOrigin(owned, mint.Base.Buildings,
            mint.Large.Buildings, mint.Advanced.Buildings);
        var funding = E4bCurveAFundingSandbox.Open(campaign.CityCampaign.Resources, capacity,
            owned, origin);
        return new(baseline, school, technologies, food, campaign, funding, 0);
    }

    /// <summary>
    /// Restore a compound snapshot only if the escrow stock equals the real
    /// projected city stock, real capacity and historical mint counter. Material
    /// synthetic deliveries are prohibited; completed mint receipts must match
    /// real city building counts, counter and total committed payments.
    /// No standalone RC93 financing database can be treated as a player save.
    /// </summary>
    public static E4bV15FundingShadow Restore(E4bV15FundingShadowCheckpoint checkpoint,
        GameScenario baseline, SchoolResearchRules school,
        TechnologyCatalog technologies, FoodConversionPolicy food)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        if (checkpoint.Version != Version || checkpoint.Revision < 0 ||
            checkpoint.Campaign is null || checkpoint.Funding is null)
            throw new ArgumentException("Invalid v15 shadow checkpoint header.", nameof(checkpoint));
        V15FoodCampaignCheckpoints.Validate(checkpoint.Campaign, baseline, school, technologies, food);
        var funding = E4bCurveAFundingSandbox.Restore(checkpoint.Funding);
        var capacity = V15CityStorage.Calculate(baseline, checkpoint.Campaign.CityCampaign);
        var mint = checkpoint.Campaign.CityCampaign.ProductiveFamilies.Single(f => f.FamilyId == "mint");
        var origin = funding.MintOrigin;
        var receipts = funding.CompletedMints;
        var expectedBase = (long)origin.Base + receipts.Count(r => r.Tier == ProductionTier.Base);
        var expectedLarge = (long)origin.Large + receipts.Count(r => r.Tier == ProductionTier.Large);
        var expectedAdvanced = (long)origin.Advanced + receipts.Count(r => r.Tier == ProductionTier.Advanced);
        if (mint.Base.Buildings != expectedBase || mint.Large.Buildings != expectedLarge ||
            mint.Advanced.Buildings != expectedAdvanced ||
            !funding.Capacity.Equals(capacity) ||
            !funding.Stock.Equals(checkpoint.Campaign.CityCampaign.Resources) ||
            funding.AcquiredMints != MintCounter(checkpoint.Campaign) ||
            !funding.HypotheticalDelivered.Equals(ResourceBundle.Empty) ||
            !(funding.InitialStock + funding.RealClockCredits).Equals(
                funding.Stock + funding.Escrowed + funding.Committed + funding.RealClockDebits))
            throw new ArgumentException("The v15 city and escrow diverge or contain sandbox-only deliveries.", nameof(checkpoint));
        return new(baseline, school, technologies, food,
            checkpoint.Campaign, funding, checkpoint.Revision);
    }

    /// <summary>Begin a frozen quotation against the historical mint copy counter,
    /// never against the number of occupied buildings. Prerequisites are checked
    /// again at readiness time, not bypassed by pre-financing.</summary>
    public bool TryBeginMint(ProductionTier tier, long expectedRevision,
        out E4bV15FundingShadow next)
    {
        next = this;
        if (expectedRevision != Revision || Revision == long.MaxValue ||
            !Funding.TryBeginMint(tier, out var started)) return false;
        next = New(Campaign, started);
        return true;
    }

    /// <summary>Transfers ONLY resources already in the projected city inventory.
    /// No made-up deliveries or implicit warehouse expansion.
    /// The city remains a valid v15 checkpoint or nothing changes.</summary>
    public bool TryDeposit(ResourceBundle materials, long expectedRevision,
        out E4bV15FundingShadow next)
    {
        ArgumentNullException.ThrowIfNull(materials);
        next = this;
        if (expectedRevision != Revision || Revision == long.MaxValue ||
            !Funding.TryDeposit(materials, out var funded)) return false;
        var candidate = Campaign with
        {
            CityCampaign = Campaign.CityCampaign with { Resources = funded.Stock }
        };
        return TryProject(candidate, funded, out next);
    }

    /// <summary>RC97: one REAL second in the v15 clock, independent of
    /// whether a mint is currently being financed. This is deliberately not
    /// exposed on the playable session; city and escrow remain in the same
    /// immutable checkpoint, and no escrow material enters production.</summary>
    public bool TryAdvanceClock(MeatChainPolicy meat, HorticulturePolicy crops,
        long expectedRevision, out E4bV15FundingShadow next)
    {
        ArgumentNullException.ThrowIfNull(meat);
        ArgumentNullException.ThrowIfNull(crops);
        next = this;
        if (expectedRevision != Revision || Revision == long.MaxValue) return false;
        try
        {
            var plan = V15UnifiedCampaignClock.Plan(Campaign, _baseline, _school,
                _technologies, FoodIndustryConstructionCatalog.Candidate,
                meat, crops, _food, TimeSpan.FromSeconds(1));
            // Audit all physical owners (city, animals, transit and depot) before
            // recording the city-only inventory change in the funding ledger.
            if (!V15StockAuditor.ReconcileSecond(Campaign, plan).Balanced) return false;
            var capacity = V15CityStorage.Calculate(_baseline, plan.Next.CityCampaign);
            if (!Funding.TryRecordRealClock(Campaign.CityCampaign.Resources,
                plan.Next.CityCampaign.Resources, capacity, out var funded)) return false;
            return TryProject(plan.Next, funded, out next);
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException or OverflowException)
        {
            return false;
        }
    }

    /// <summary>RC98 isolated atomic completion: material has ALREADY left
    /// city stock through deposits. Increment exactly one v15 mint tier and
    /// its historical counter, move escrow to committed receipts and recompute
    /// physical capacity as one validated immutable transition. Never call the
    /// ordinary priced construction command here (it would charge twice).</summary>
    public bool TryCompleteMint(long expectedRevision, out E4bV15FundingShadow next)
    {
        next = this;
        if (expectedRevision != Revision || Revision == long.MaxValue ||
            Readiness() != E4bV15FundingReadiness.ReadyForFutureAtomicCommit ||
            Funding.PendingTier is not { } tier) return false;
        try
        {
            var city = Campaign.CityCampaign;
            var mint = city.ProductiveFamilies.Single(f => f.FamilyId == "mint");
            var portfolio = TieredBuildingPortfolio.Create(mint.FamilyId,
                mint.Base, mint.Large, mint.Advanced,
                V15CrewRules.CrewSize(Campaign.CrewMode, "mint")).Build(tier, 1);
            var nextCity = city with
            {
                ProductiveFamilies = city.ProductiveFamilies.Select(f => f.FamilyId == "mint"
                    ? new V15ProductiveFamilySnapshot("mint", portfolio.Base,
                        portfolio.Large, portfolio.Advanced) : f).ToImmutableArray(),
                PurchaseCounters = city.PurchaseCounters.Select(c => c.FamilyId == "mint"
                    ? c with { Acquired = checked(c.Acquired + 1) } : c).ToImmutableArray()
            };
            var nextCapacity = V15CityStorage.Calculate(_baseline, nextCity);
            if (!Funding.TryCompleteWithCapacity(nextCapacity, out var funded)) return false;
            return TryProject(Campaign with { CityCampaign = nextCity }, funded, out next);
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException or OverflowException)
        {
            return false;
        }
    }

    public bool TryCancel(long expectedRevision, out E4bV15FundingShadow next)
    {
        next = this;
        if (expectedRevision != Revision || Revision == long.MaxValue ||
            !Funding.TryCancel(out var canceled)) return false;
        var candidate = Campaign with
        {
            CityCampaign = Campaign.CityCampaign with { Resources = canceled.Stock }
        };
        return TryProject(candidate, canceled, out next);
    }

    /// <summary>A paid mint requires its source building and tier research.
    /// Readiness alone never constructs; only the isolated RC98 completion
    /// transition creates a building in the experimental city checkpoint.</summary>
    public E4bV15FundingReadiness Readiness()
    {
        if (Funding.PendingTier is not { } tier)
            return E4bV15FundingReadiness.NoPendingMint;
        if (Funding.Escrow is not { IsFullyPaid: true })
            return E4bV15FundingReadiness.MissingPayment;
        var city = Campaign.CityCampaign;
        var mint = _baseline.Catalog.BuildingsById["mint"];
        foreach (var required in mint.Prerequisites)
        {
            if (!city.OtherBuildings.Any(b => b.BuildingId == required && b.Count > 0) &&
                !city.ProductiveFamilies.Any(f => f.FamilyId == required &&
                    (long)f.Base.Buildings + f.Large.Buildings + f.Advanced.Buildings > 0) &&
                !(required == "school" && city.SchoolCount > 0))
                return E4bV15FundingReadiness.MissingPrerequisite;
        }
        var tierRules = V14TierConstructionRules.Candidate;
        var family = TieredProductionCatalog.FromCatalog(_baseline.Catalog,
            tierRules.LargeMultiplier, tierRules.AdvancedMultiplier)
            .Single(f => f.FamilyId == "mint");
        if (!family.IsUnlocked(tier, city.UnlockedTechnologyIds.ToHashSet(StringComparer.Ordinal)))
            return E4bV15FundingReadiness.MissingResearch;
        return E4bV15FundingReadiness.ReadyForFutureAtomicCommit;
    }

    private E4bV15FundingShadow New(V15FoodCampaignCheckpoint campaign,
        E4bCurveAFundingSandbox funded) =>
        new(_baseline, _school, _technologies, _food, campaign, funded, checked(Revision + 1));

    private bool TryProject(V15FoodCampaignCheckpoint campaign,
        E4bCurveAFundingSandbox funded, out E4bV15FundingShadow next)
    {
        next = this;
        try
        {
            V15FoodCampaignCheckpoints.Validate(campaign, _baseline, _school, _technologies, _food);
            var preview = New(campaign, funded);
            _ = Restore(preview.ExportCheckpoint(), _baseline, _school, _technologies, _food);
            next = preview;
            return true;
        }
        catch (Exception e) when (e is ArgumentException or OverflowException)
        {
            return false;
        }
    }

    private static int MintCounter(V15FoodCampaignCheckpoint campaign)
    {
        var matches = campaign.CityCampaign.PurchaseCounters.Where(c => c.FamilyId == "mint").ToArray();
        if (matches.Length != 1 || matches[0].Acquired < 0 ||
            campaign.CityCampaign.ProductiveFamilies.Count(f => f.FamilyId == "mint") != 1)
            throw new ArgumentException("The v15 city has no unique mint acquisition history.", nameof(campaign));
        return matches[0].Acquired;
    }
}
