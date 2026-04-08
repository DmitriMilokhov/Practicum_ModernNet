namespace EventManager.Infrastructure.Exceptions;

public class EventNotFoundException : NotFoundException
{
    public Guid EventId { get; }

    public EventNotFoundException(Guid eventId) : base($"Event {eventId} is not found")
    {
        EventId = eventId;
    }

    public EventNotFoundException(Guid eventId, Exception inner) : base($"Event {eventId} is not found", inner)
    {
        EventId = eventId;
    }
}
