using SimRisorse.Application.Persistence;

namespace SimRisorse.Application.Features.Persistence.SaveGame;

public sealed class SaveGameHandler(GameRuntime runtime, IGameSaveStore store)
{
    public async Task<PersistenceResult> HandleAsync(CancellationToken cancellationToken = default)
    {
        var entered = false;
        try
        {
            await runtime.PersistenceGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            entered = true;
            var snapshot = runtime.CaptureSnapshot();
            await Task.Run(() => store.Save(snapshot), cancellationToken).ConfigureAwait(false);
            return new(GameSaveError.None, runtime.Read());
        }
        catch (GameSaveException exception) { return new(exception.Error, runtime.Read()); }
        catch (OperationCanceledException) { return new(GameSaveError.Canceled, runtime.Read()); }
        finally { if (entered) runtime.PersistenceGate.Release(); }
    }
}
