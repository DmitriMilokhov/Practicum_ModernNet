using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Infrastructure.Exceptions;

namespace EventManager.Features.Bookings;

public class InMemoryBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = [];
    public Task AddAsync(Booking bookingModel, CancellationToken ct = default)
    {
        _bookings.Add(bookingModel);
        return Task.CompletedTask;
    }

    public Task<Booking> GetAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(TryGetBooking(id));
    }

    private Booking TryGetBooking(Guid id)
    {
        var result = _bookings.FirstOrDefault(e => e.Id == id);

        if (result == null)
        {
            throw new EntityNotFoundException(nameof(Booking), id);
        }

        return result;
    }
}
