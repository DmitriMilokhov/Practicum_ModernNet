using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Interfaces;
using EventManager.Infrastructure.Constants;
using EventManager.Infrastructure.Exceptions;

namespace EventManager.Features.Bookings;

public class BookingService(IBookingFactory bookingFactory,
    IBookingRepository bookingRepository,
    IEventRepository eventRepository,
    IEventBookingLockProvider lockProvider) : IBookingService
{
    public async Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken ct = default)
    {
        var isEventExist = await eventRepository.ExistsAsync(eventId, ct);
        if (!isEventExist) 
        {
            throw new EntityNotFoundException("Event", eventId);
        }

        //FOR_REVIEWER: осознанно использую синхронизацию по событию через собственный синглтон
        //(внутри SemaphoreSlim - т.к. вызовы асинхронные
        using (await lockProvider.AcquireAsync(eventId, ct))
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
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        var bookingEntity = await bookingRepository.GetAsync(bookingId, ct);
        return bookingEntity.ToDto();
    }

    public async Task ConfirmBooking(Guid bookingId, CancellationToken ct = default)
    {
        var bookingEntity = await bookingRepository.GetAsync(bookingId, ct);
        bookingEntity.Update(BookingStatus.Confirmed, DateTime.UtcNow);
    }

    public async Task RejectBooking(Guid bookingId, CancellationToken ct = default)
    {
        var bookingEntity = await bookingRepository.GetAsync(bookingId, ct);
        bookingEntity.Update(BookingStatus.Rejected, DateTime.UtcNow);
    }

    public async Task RejectBookingAndReleaseEvent(Guid bookingId, CancellationToken ct = default)
    {
        var bookingEntity = await bookingRepository.GetAsync(bookingId, ct);

        using (await lockProvider.AcquireAsync(bookingEntity.EventId, ct))
        {
            var eventToUpdate = await eventRepository.GetAsync(bookingEntity.EventId, ct);
            eventToUpdate.ReleaseSeats();
            bookingEntity.Update(BookingStatus.Rejected, DateTime.UtcNow);
        }
    }
}
