namespace SimRisorse.Application.Features.Population.AssignWorkers;

public sealed record AssignWorkersRequest(string BuildingId, int Count);

public sealed class AssignWorkersHandler(GameRuntime runtime)
{
    public CommandResult Handle(AssignWorkersRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runtime.Execute(session => session.AssignWorkers(request.BuildingId, request.Count));
    }
}
