using System.ComponentModel.DataAnnotations;

namespace EventManager.Models;

public class EventDto : IValidatableObject
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required")]
    public required string Title { get; set; }
    public string? Description { get; set; }

    [Required(ErrorMessage = "StartAt is required")]
    public required DateTime StartAt { get; set; }

    [Required(ErrorMessage = "EndAt is required")]
    public required DateTime EndAt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndAt <= StartAt)
        {
            yield return new ValidationResult(
                "EndAt must be later than StartAt",
                new[] { nameof(EndAt) }
            );
        }
    }
}

public class FullEventDto : EventDto
{
    public Guid Id { get; set; }
}
