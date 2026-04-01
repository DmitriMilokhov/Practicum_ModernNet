namespace EventManager.Models;

public class Event
{
    public Guid Id { get; init; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }

    public Event() { }
    public Event(Guid id, string title, string? description, DateTime startAt, DateTime endAt)
    {
        SetValues(title, description, startAt, endAt);
        Id = id;
    }

    public void Update(string title, string? description, DateTime startAt, DateTime endAt)
    {
        SetValues(title, description, startAt, endAt);
    }

    private void SetValues(string title, string? description, DateTime startAt, DateTime endAt)
    {
        if(string.IsNullOrEmpty(title))
        {
            throw new ArgumentException("Title is required");
        }

        if (endAt <= startAt)
        {
            throw new ArgumentException("EndAt must be later than StartAt");
        }

        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }
}
