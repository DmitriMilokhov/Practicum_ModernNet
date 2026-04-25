using EventManager.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Features.Bookings.Model;

public class BookingDto //: IValidatableObject
{
    [Required(ErrorMessage = ValidationMessages.IdIsRequiredMsg)]
    public Guid Id { get; init; }

    [Required(ErrorMessage = ValidationMessages.EventIdIsRequiredMsg)]
    public required Guid EventId { get; set; }

    [Required(ErrorMessage = ValidationMessages.BookingStatusIsRequiredMsg)]
    public BookingStatus Status { get; set; }

    [Required(ErrorMessage = ValidationMessages.CreatedAtIsRequiredMsg)]
    public DateTime? CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    //{
    //    if (EndAt <= StartAt)
    //    {
    //        yield return new ValidationResult(ValidationMessages.EndDateLaterThanStartMsg, [nameof(EndAt)]);
    //    }
    //}
}
