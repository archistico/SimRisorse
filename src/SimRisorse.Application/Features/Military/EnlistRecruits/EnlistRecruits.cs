namespace SimRisorse.Application.Features.Military.EnlistRecruits;

public sealed record EnlistRecruitsRequest(int Count);

/// <summary>Turns free civilians into recruits, spending their equipment at once.</summary>
public sealed class EnlistRecruitsHandler(GameRuntime runtime)
{
    public CommandResult Handle(EnlistRecruitsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runtime.Execute(session => session.Enlist(request.Count));
    }
}
