using SimRisorse.Application.ReadModels;

namespace SimRisorse.Application.Features.Sessions.GetGameState;

public sealed class GetGameStateHandler(GameRuntime runtime)
{
    public GameView Handle() => runtime.Read();
}
