using SimRisorse.Application.ReadModels;
using SimRisorse.Core.Content;

namespace SimRisorse.Application.Features.Sessions.StartGame;

public sealed record StartGameRequest(GameScenario? Scenario = null);

public sealed class StartGameHandler(GameRuntime runtime)
{
    public GameView Handle(StartGameRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runtime.Start(request.Scenario);
    }
}
