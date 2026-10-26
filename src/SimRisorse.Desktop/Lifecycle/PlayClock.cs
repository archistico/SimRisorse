namespace SimRisorse.Desktop.Lifecycle;

public sealed class PlayClock(Action<TimeSpan> advance, Action interrupted, TimeProvider? provider = null)
{
    private readonly TimeProvider _provider = provider ?? TimeProvider.System;
    private long? _last;
    public void Reset() => _last = _provider.GetTimestamp();
    public void Pulse()
    {
        var now = _provider.GetTimestamp();
        if (_last is { } previous)
        {
            var elapsed = _provider.GetElapsedTime(previous, now);
            _last = now;
            if (elapsed < TimeSpan.Zero || elapsed > TimeSpan.FromSeconds(2)) interrupted();
            else advance(elapsed);
        }
        else _last = now;
    }
    public void Suspend() { Reset(); interrupted(); }
}
