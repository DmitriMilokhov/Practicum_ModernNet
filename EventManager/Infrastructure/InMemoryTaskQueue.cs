using EventManager.Infrastructure.Interfaces;
using System.Threading.Channels;

namespace EventManager.Infrastructure;

public class InMemoryTaskQueue<T>(ILogger<InMemoryTaskQueue<T>> logger) : ITaskQueue<T> where T : class
{
    private readonly Channel<T> _queue = Channel.CreateBounded<T>(new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.Wait
    });

    public async ValueTask EnqueueAsync(T bookingDto, CancellationToken ct = default)
    {
        logger.LogInformation("Task for object {objectType} has been enqueued", typeof(T));
        await _queue.Writer.WriteAsync(bookingDto, ct);
    }

    public IAsyncEnumerable<T> ReadAllAsync(CancellationToken ct = default)
    {
        return _queue.Reader.ReadAllAsync(ct);
    }
}
