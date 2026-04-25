using EventManager.Features.Bookings.Model;

namespace EventManager.Features.Bookings.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken ct = default);
        Task<BookingDto> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default);
        Task UpdateBookingStatusAsync(Guid bookingId, BookingStatus status, CancellationToken ct = default);
    }
}
