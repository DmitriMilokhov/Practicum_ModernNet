using EventManager.Infrastructure.Constants;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Features.Events.Model;

public class Event
{
    public Guid Id { get; init; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public int TotalSeats { get; private set; }
    public int AvailableSeats { get; private set; }

    public Event(Guid id, 
        string title, 
        string? description, 
        DateTime startAt, 
        DateTime endAt,
        int totalSeats)
    {
        Validate(title, startAt, endAt, totalSeats);

        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
    }

    public void Update(string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
    {
        Validate(title, startAt, endAt, totalSeats);

        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;

        if(AvailableSeats > TotalSeats)
        {
            AvailableSeats = TotalSeats;
        }
    }

    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats < count)
        {
            return false;
        }

        AvailableSeats -= count;
        return true;
    }

    public bool TryReleaseSeats(int count = 1)
    {
        if (TotalSeats < count + AvailableSeats)
        {
            return false;
        }

        AvailableSeats += count;
        return true;
    }

    private static void Validate(string title, DateTime startAt, DateTime endAt, int totalSeats)
    {
        if(string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException(Constants.TitleIsRequiredMsg);
        }

        if (endAt <= startAt)
        {
            throw new ValidationException(Constants.EndDateLaterThanStartMsg);
        }

        if (totalSeats <= 0) 
        {
            throw new ValidationException(Constants.TotalSeatsAboveZeroMsg);
        }
    }
}
