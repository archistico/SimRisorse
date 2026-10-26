using SimRisorse.Application.Persistence;

namespace SimRisorse.Application.Features.Persistence.LoadGame;

public sealed class LoadGameHandler(GameRuntime runtime, IGameSaveStore store)
{
    public async Task<PersistenceResult> HandleAsync(CancellationToken cancellationToken = default)
    {
        var revision = runtime.BeginLoad();
        var entered = false;
        try
        {
            await runtime.PersistenceGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            entered = true;
            var snapshot = await Task.Run(store.Load, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return new(runtime.TryRestore(snapshot, revision), runtime.Read());
        }
        catch (GameSaveException exception) { return new(exception.Error, runtime.Read()); }
        catch (OperationCanceledException) { return new(GameSaveError.Canceled, runtime.Read()); }
        finally { if (entered) runtime.PersistenceGate.Release(); }
    }
}
