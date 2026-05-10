using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Interfaces;
using EventManager.Infrastructure.Constants;
using EventManager.Infrastructure.Exceptions;
using System.Collections.Concurrent;

namespace EventManager.Features.Bookings;

public class BookingService(IBookingFactory bookingFactory,
    IBookingRepository bookingRepository, IEventRepository eventRepository) : IBookingService
{
    //COMMENT FOR REVIEWER: использую отдельный семафор для отдельного события, вместо Lock. Так как мой репозиторий уже асинхронные методы
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();
    public async Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken ct = default)
    {
        var isEventExist = await eventRepository.ExistsAsync(eventId, ct);
        if (!isEventExist) 
        {
            throw new EntityNotFoundException("Event", eventId);
        }

        var bookingEventSemaphore = _locks.GetOrAdd(
            eventId,
            _ => new SemaphoreSlim(1, 1));

        await bookingEventSemaphore.WaitAsync(ct);

        try
        {
            var eventForBooking = await eventRepository.GetAsync(eventId, ct);
            var reserved = eventForBooking.TryReserveSeats();
            if (!reserved)
            {
                throw new NoAvailableSeatsException(Constants.NoAvailableSeatsExceptionMsg);
            }

            var bookingDto = bookingFactory.CreateBookingDto(eventId);
            await bookingRepository.AddAsync(bookingDto.ToEntity(), ct);

            return bookingDto;
        }
        finally
        {
            bookingEventSemaphore.Release();
        }
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        var bookingEntity = await bookingRepository.GetAsync(bookingId, ct);
        return bookingEntity.ToDto();
    }

    public async Task UpdateBookingStatusAsync(Guid bookingId, BookingStatus status, CancellationToken ct = default)
    {
        var bookingEntity = await bookingRepository.GetAsync(bookingId, ct);
        bookingEntity.Update(status, DateTime.UtcNow);
    }
}
