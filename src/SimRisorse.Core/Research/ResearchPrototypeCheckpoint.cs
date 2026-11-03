using System.Collections.Immutable;

namespace SimRisorse.Core.Research;

/// <summary>
/// E3-only transfer shape for repeatable tests and a future migration design.
/// This is NOT GameSnapshot v3, NOT a SQLite schema and is NEVER read by a v13 session.
/// </summary>
public sealed record ResearchPrototypeCheckpoint(string ModelId, long BalanceMilliPoints,
    ImmutableArray<string> UnlockedTechnologyIds, int WaitingStudents,
    ImmutableArray<int> TrainingProgressSeconds, int QualifiedResearchers, int ActiveResearchers);

public sealed record RestoredResearchPrototype(ResearchLedger Ledger, SchoolResearchState School);

public static class ResearchPrototypeCheckpoints
{
    public const string ModelId = "research-prototype-e3-1";

    public static ResearchPrototypeCheckpoint Capture(ResearchLedger ledger, SchoolResearchState school,
        SchoolResearchRules rules, int schools)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        SchoolResearch.Validate(rules, school, schools);
        return new(ModelId, ledger.Balance.MilliPoints,
            [.. ledger.UnlockedTechnologies.OrderBy(id => id, StringComparer.Ordinal)],
            school.WaitingStudents, school.TrainingProgressSeconds, school.QualifiedResearchers, school.ActiveResearchers);
    }

    public static RestoredResearchPrototype Restore(ResearchPrototypeCheckpoint checkpoint,
        TechnologyCatalog catalog, SchoolResearchRules rules, int schools, int totalPopulation,
        int assignedCivilianWorkers, int militaryPopulation)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        if (checkpoint.ModelId != ModelId)
            throw new ArgumentException("Research prototype checkpoint tag is incompatible.", nameof(checkpoint));
        if (checkpoint.UnlockedTechnologyIds.IsDefault)
            throw new ArgumentException("Missing unlocked technology list.", nameof(checkpoint));
        var ledger = ResearchLedger.Restore(ResearchPoints.FromMilliPoints(checkpoint.BalanceMilliPoints),
            checkpoint.UnlockedTechnologyIds, catalog);
        var state = new SchoolResearchState(checkpoint.WaitingStudents, checkpoint.TrainingProgressSeconds,
            checkpoint.QualifiedResearchers, checkpoint.ActiveResearchers);
        SchoolResearch.Validate(rules, state, schools);
        if (totalPopulation < 0 || assignedCivilianWorkers < 0 || militaryPopulation < 0 ||
            (long)assignedCivilianWorkers + militaryPopulation + state.ReservedPopulation > totalPopulation)
            throw new ArgumentException("Checkpoint assigns one inhabitant to more than one occupation.", nameof(checkpoint));
        return new(ledger, state);
    }
}
