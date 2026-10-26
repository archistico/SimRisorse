namespace SimRisorse.Application.Features.Construction.ConstructBuildings;

public sealed record ConstructBuildingsRequest(string BuildingId, int Count);

public sealed class ConstructBuildingsHandler(GameRuntime runtime)
{
    public CommandResult Handle(ConstructBuildingsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runtime.Execute(session => session.Construct(request.BuildingId, request.Count));
    }
}
