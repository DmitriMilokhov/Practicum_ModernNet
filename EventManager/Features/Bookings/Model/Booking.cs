using System.ComponentModel.DataAnnotations;
using EventManager.Infrastructure.Constants;

namespace EventManager.Features.Bookings.Model;

public enum BookingStatus { Pending, Confirmed, Rejected }

public class Booking
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; set; }

    public void Update(BookingStatus status, DateTime? processedAt)
    {
        Status = status;

        if (!processedAt.HasValue) return;

        if (processedAt.Value < CreatedAt)
        {
            throw new ValidationException(Messages.ProcessedDateLaterThanCreatedMsg);
        }

        ProcessedAt = processedAt;
    }
}

