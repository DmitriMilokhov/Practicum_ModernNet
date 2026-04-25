using EventManager.Infrastructure.Constants;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Features.Events.Model;

public class EventDto : IValidatableObject
{
    [Required(AllowEmptyStrings = false, ErrorMessage = Messages.TitleIsRequiredMsg)]
    public required string Title { get; set; }
    public string? Description { get; set; }

    [Required(ErrorMessage = Messages.StartAtIsRequiredMsg)]
    public DateTime? StartAt { get; set; }

    [Required(ErrorMessage = Messages.EndAtIsRequiredMsg)]
    public DateTime? EndAt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndAt <= StartAt)
        {
            yield return new ValidationResult(Messages.EndDateLaterThanStartMsg, [nameof(EndAt)]);
        }
    }
}

public class FullEventDto : EventDto
{
    public Guid Id { get; init; }
}
