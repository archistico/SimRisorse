namespace SimRisorse.Application.Features.Military.CancelQueuedRecruits;

public sealed record CancelQueuedRecruitsRequest(int Count);

public sealed class CancelQueuedRecruitsHandler(GameRuntime runtime)
{
    public CommandResult Handle(CancelQueuedRecruitsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runtime.Execute(session => session.CancelQueuedRecruits(request.Count));
    }
}
