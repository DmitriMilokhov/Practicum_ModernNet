using System.ComponentModel.DataAnnotations;
using EventManager.Infrastructure.Constants;

namespace EventManager.Features.Bookings.Model;

public enum BookingStatus { Pending, Confirmed, Rejected }

public class Booking(Guid id, Guid eventId, BookingStatus status, DateTime createdAt)
{
    public Guid Id { get; init; } = id;
    public Guid EventId { get; init; } = eventId;
    public BookingStatus Status { get; private set; } = status;
    public DateTime CreatedAt { get; init; } = createdAt;
    public DateTime? ProcessedAt { get; private set; }

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

