namespace EventManager.Infrastructure.Interfaces;

public interface ITaskQueue<T>
{
    ValueTask EnqueueAsync(T bookingDto, CancellationToken ct = default);
    IAsyncEnumerable<T> ReadAllAsync(CancellationToken ct = default);
}
