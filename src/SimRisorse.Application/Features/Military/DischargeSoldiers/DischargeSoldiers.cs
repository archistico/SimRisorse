namespace SimRisorse.Application.Features.Military.DischargeSoldiers;

public sealed record DischargeSoldiersRequest(int Count);

public sealed class DischargeSoldiersHandler(GameRuntime runtime)
{
    public CommandResult Handle(DischargeSoldiersRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runtime.Execute(session => session.DischargeSoldiers(request.Count));
    }
}
