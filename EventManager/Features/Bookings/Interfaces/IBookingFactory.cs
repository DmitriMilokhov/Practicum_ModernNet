using EventManager.Features.Bookings.Model;

namespace EventManager.Features.Bookings.Interfaces;

public interface IBookingFactory
{
    BookingDto CreateBookingDto(Guid eventId);
}
