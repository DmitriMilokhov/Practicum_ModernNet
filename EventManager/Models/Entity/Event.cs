namespace EventManager.Models;

public class Event
{
    public required Guid Id { get; init; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public void Update(string title, string? description, DateTime startAt, DateTime endAt)
    {
        if(endAt < startAt)
        {
            throw new ArgumentException("EndAt must be later than StartAt");
        }

        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }
}
