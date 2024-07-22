namespace MrKWatkins.Sesharp;

public sealed class Disposable(Action action) : IDisposable
{
    private int disposed;

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref disposed, 1, 0) == 0)
        {
            action();
        }
    }
}