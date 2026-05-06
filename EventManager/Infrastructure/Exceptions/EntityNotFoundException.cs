namespace EventManager.Infrastructure.Exceptions;

public class EntityNotFoundException : NotFoundException
{
    public string EntityName { get; }
    public Guid EventId { get; }

    public EntityNotFoundException(string entityName, Guid eventId) : base($"{entityName} {eventId} is not found")
    {
        EntityName = entityName;
        EventId = eventId;
    }

    public EntityNotFoundException(string entityName, Guid eventId, Exception inner) 
        : base($"{entityName} {eventId} is not found", inner)
    {
        EntityName = entityName;
        EventId = eventId;
    }
}
