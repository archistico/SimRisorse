namespace SimRisorse.Application.Features.Population.ReleaseWorkers;

public sealed record ReleaseWorkersRequest(string BuildingId, int Count);

public sealed class ReleaseWorkersHandler(GameRuntime runtime)
{
    public CommandResult Handle(ReleaseWorkersRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runtime.Execute(session => session.ReleaseWorkers(request.BuildingId, request.Count));
    }
}
