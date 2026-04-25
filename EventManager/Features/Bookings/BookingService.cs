using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Interfaces;
using EventManager.Infrastructure.Exceptions;

namespace EventManager.Features.Bookings;

public class BookingService(IBookingFactory bookingFactory,
    IBookingRepository bookingRepository, IEventRepository eventRepository) : IBookingService
{
    public async Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken ct = default)
    {
        var isEventExist = await eventRepository.ExistsAsync(eventId, ct);
        if (!isEventExist) 
        {
            throw new EntityNotFoundException("Event", eventId);
        }

        var bookingDto = bookingFactory.CreateBookingDto(eventId);

        ct.ThrowIfCancellationRequested();
        await bookingRepository.AddAsync(bookingDto.ToEntity(), ct);

        return bookingDto;
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var bookingEntity = await bookingRepository.GetAsync(bookingId, ct);
        return bookingEntity.ToDto();
    }

    public async Task UpdateBookingStatusAsync(Guid bookingId, BookingStatus status, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var bookingEntity = await bookingRepository.GetAsync(bookingId, ct);
        bookingEntity.Update(status, DateTime.UtcNow);
    }
}
