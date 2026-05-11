using EventManager.Features.Bookings.Interfaces;
using System.Collections.Concurrent;

namespace EventManager.Features.Bookings;

public class EventBookingLockProvider : IEventBookingLockProvider
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public async Task<IDisposable> AcquireAsync(Guid eventId, CancellationToken ct = default)
    {
        var semaphore = _locks.GetOrAdd(eventId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);

        return new Releaser(semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                semaphore.Release();
            }
        }
    }
}
