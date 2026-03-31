namespace EventManager.Models;

public class EventDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }
}

public class FullEventDto : EventDto
{
    public Guid Id { get; set; }
}
