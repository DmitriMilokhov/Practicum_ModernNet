using System.ComponentModel.DataAnnotations;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Constants;

namespace EventManager.Features.Bookings.Model;

public enum BookingStatus { Pending, Confirmed, Rejected }

public class Booking
{
    public Guid Id { get; init; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; private set; }
    public Guid EventId { get; init; }
    public Event Event { get; init; } = null!;

    public Booking() {}

    public Booking(Guid id, Guid eventId, BookingStatus status, DateTime createdAt)
    {
        Id = id;
        EventId = eventId;
        Status = status;
        CreatedAt = createdAt;
    }

    public void Update(BookingStatus status, DateTime? processedAt)
    {
        Status = status;

        if (!processedAt.HasValue) return;

        if (processedAt.Value < CreatedAt)
        {
            throw new ValidationException(Constants.ProcessedDateLaterThanCreatedMsg);
        }

        ProcessedAt = processedAt;
    }
}

