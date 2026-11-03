namespace SimRisorse.Application.Features.Military.CancelRecruits;

/// <summary>Cancel pending recruits and, in the current v13 rules, active trainees too.</summary>
public sealed record CancelRecruitsRequest(int Count);

public sealed class CancelRecruitsHandler(GameRuntime runtime)
{
    public CommandResult Handle(CancelRecruitsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runtime.Execute(session => session.CancelQueuedRecruits(request.Count));
    }
}
