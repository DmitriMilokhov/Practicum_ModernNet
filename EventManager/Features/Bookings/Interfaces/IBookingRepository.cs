using EventManager.Features.Bookings.Model;

namespace EventManager.Features.Bookings.Interfaces;

public interface IBookingRepository
{
    Task<Booking> GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Booking bookingModel, CancellationToken ct = default);
}
