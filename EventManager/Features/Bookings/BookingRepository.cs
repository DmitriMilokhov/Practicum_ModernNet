using EventManager.DataAccess;
using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Features.Bookings;

public class BookingRepository(AppDbContext context) : IBookingRepository
{
    public async Task AddAsync(Booking bookingModel, CancellationToken ct = default)
    {
        await context.Bookings.AddAsync(bookingModel, ct);
    }

    public async Task<Booking> GetAsync(Guid id, CancellationToken ct = default)
    {
        var booking = await context.Bookings.FirstOrDefaultAsync(e => e.Id == id, ct);

        if (booking is null)
        {
            throw new EntityNotFoundException(nameof(Booking), id);
        }

        return booking;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }
}
