using System.Collections.Immutable;

namespace SimRisorse.Core.Military;

/// <summary>
/// People who left the civilian workforce. <paramref name="QueuedRecruits"/> wait for a free barracks place;
/// <paramref name="TrainingProgressSeconds"/> holds the completed seconds of each recruit in training, most advanced first.
/// Every person here is still a resident: housed, fed and counted in the population.
/// </summary>
public sealed record MilitaryState(int QueuedRecruits, ImmutableArray<int> TrainingProgressSeconds, int Soldiers)
{
    public static MilitaryState Empty { get; } = new(0, ImmutableArray<int>.Empty, 0);

    public int TrainingRecruits => TrainingProgressSeconds.IsDefault ? 0 : TrainingProgressSeconds.Length;

    /// <summary>Recruits not yet trained, queued or in a barracks place.</summary>
    public long Recruits => (long)QueuedRecruits + TrainingRecruits;

    /// <summary>Residents not available for civilian work.</summary>
    public long People => Recruits + Soldiers;

    public bool IsEmpty => People == 0;

    public bool Equals(MilitaryState? other) => other is not null && QueuedRecruits == other.QueuedRecruits &&
        Soldiers == other.Soldiers && TrainingRecruits == other.TrainingRecruits &&
        (TrainingRecruits == 0 || TrainingProgressSeconds.SequenceEqual(other.TrainingProgressSeconds));

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(QueuedRecruits);
        hash.Add(Soldiers);
        if (!TrainingProgressSeconds.IsDefault)
            foreach (var progress in TrainingProgressSeconds) hash.Add(progress);
        return hash.ToHashCode();
    }
}
