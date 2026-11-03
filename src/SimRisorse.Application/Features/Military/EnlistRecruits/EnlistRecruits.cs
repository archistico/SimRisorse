namespace SimRisorse.Application.Features.Military.EnlistRecruits;

public sealed record EnlistRecruitsRequest(int Count);

public sealed class EnlistRecruitsHandler(GameRuntime runtime)
{
    public CommandResult Handle(EnlistRecruitsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runtime.Execute(session => session.Enlist(request.Count));
    }
}
