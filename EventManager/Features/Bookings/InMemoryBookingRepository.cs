using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Infrastructure.Exceptions;
using System.Collections.Concurrent;

namespace EventManager.Features.Bookings;

public class InMemoryBookingRepository : IBookingRepository
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookings = [];

    public Task AddAsync(Booking bookingModel, CancellationToken ct = default)
    {
        _bookings.TryAdd(bookingModel.Id, bookingModel);
        return Task.CompletedTask;
    }

    public Task<Booking> GetAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(TryGetBooking(id));
    }

    private Booking TryGetBooking(Guid id)
    {
        if (!_bookings.TryGetValue(id, out var result))
        {
            throw new EntityNotFoundException(nameof(Booking), id);
        }

        return result;
    }
}
