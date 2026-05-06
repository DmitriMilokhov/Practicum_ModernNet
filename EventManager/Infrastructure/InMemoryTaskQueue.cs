using EventManager.Infrastructure.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace EventManager.Infrastructure;

public class InMemoryTaskQueue<T>(ILogger<InMemoryTaskQueue<T>> logger) : ITaskQueue<T> where T : class
{
    private readonly ConcurrentQueue<T> _queue = new();
    public void Enqueue(T task)
    {
        logger.LogInformation("Task for object {objectType} has been enqueued", typeof(T));
        _queue.Enqueue(task);
    }

    public bool TryDequeue([MaybeNullWhen(false)] out T task)
    {
        return _queue.TryDequeue(out task);
    }
}
