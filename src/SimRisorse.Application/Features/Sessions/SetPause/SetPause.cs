using SimRisorse.Core.Sessions;

namespace SimRisorse.Application.Features.Sessions.SetPause;

public sealed record SetPauseRequest(bool IsPaused);

public sealed class SetPauseHandler(GameRuntime runtime)
{
    public CommandResult Handle(SetPauseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runtime.Execute(session =>
        {
            if (request.IsPaused) session.Pause(); else session.Resume();
            return GameActionResult.Success;
        });
    }
}
