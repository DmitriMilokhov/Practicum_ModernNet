namespace EventManager.Features.Bookings.Interfaces;

public interface IEventBookingLockProvider
{
    Task<IDisposable> AcquireAsync(Guid eventId, CancellationToken ct = default);
}
