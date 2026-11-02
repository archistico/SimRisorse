namespace SimRisorse.Application.Features.Military.CancelRecruits;

public sealed record CancelRecruitsRequest(int Count);

/// <summary>Returns waiting recruits first, then those in training with the least progress, with their equipment.</summary>
public sealed class CancelRecruitsHandler(GameRuntime runtime)
{
    public CommandResult Handle(CancelRecruitsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runtime.Execute(session => session.CancelRecruits(request.Count));
    }
}
