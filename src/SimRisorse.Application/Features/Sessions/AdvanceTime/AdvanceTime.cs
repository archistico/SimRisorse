namespace SimRisorse.Application.Features.Sessions.AdvanceTime;

public sealed record AdvanceTimeRequest(TimeSpan Elapsed);

public sealed class AdvanceTimeHandler(GameRuntime runtime)
{
    public CommandResult Handle(AdvanceTimeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runtime.Execute(session => session.Advance(request.Elapsed));
    }
}
