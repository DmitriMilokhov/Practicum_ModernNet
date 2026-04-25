namespace EventManager.Features.Bookings.Model;

public static class BookingFactory
{
    public static BookingDto CreateBookingDto(Guid eventId)
    {
        return new BookingDto
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
}
