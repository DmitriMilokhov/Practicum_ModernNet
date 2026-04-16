using EventManager.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Models;

public class Event
{
    public Guid Id { get; init; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }

    public Event(Guid id, string title, string? description, DateTime startAt, DateTime endAt)
    {
        Validate(title, startAt, endAt);

        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    public void Update(string title, string? description, DateTime startAt, DateTime endAt)
    {
        Validate(title, startAt, endAt);

        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    private void Validate(string title, DateTime startAt, DateTime endAt)
    {
        if(string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException(ValidationMessages.TitleIsRequiredMsg);
        }

        if (endAt <= startAt)
        {
            throw new ValidationException(ValidationMessages.EndDateLaterThanStartMsg);
        }
    }
}
