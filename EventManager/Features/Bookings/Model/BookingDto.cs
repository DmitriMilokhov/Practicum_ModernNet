using EventManager.Infrastructure.Constants;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Features.Bookings.Model;

public class BookingDto 
{
    [Required(ErrorMessage = Constants.IdIsRequiredMsg)]
    public Guid Id { get; init; }

    [Required(ErrorMessage = Constants.EventIdIsRequiredMsg)]
    public required Guid EventId { get; set; }

    [Required(ErrorMessage = Constants.BookingStatusIsRequiredMsg)]
    public BookingStatus Status { get; set; }

    [Required(ErrorMessage = Constants.CreatedAtIsRequiredMsg)]
    public DateTime? CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }
}
