using EventManager.Infrastructure.Exceptions;
using EventManager.Interfaces.IRepositories;
using EventManager.Models;

namespace EventManager.Repository;

public class InMemoryEventsRepository : IEventsRepository
{
    private readonly List<Event> _events = [];

    public IReadOnlyList<Event> GetAll()
    {
        return [.. _events];
    }

    public Event Get(Guid id)
    {
        return TryGetEvent(id);
    }

    public void Add(Event eventModel)
    {
        _events.Add(eventModel);
    }

    public void Update(Guid eventId, Event data)
    {
        var result = TryGetEvent(eventId);
        result.Update(data.Title, data.Description, data.StartAt, data.EndAt);
    }

    public void Delete(Guid eventId)
    {
        var result = TryGetEvent(eventId);
        _events.Remove(result);
    }

    private Event TryGetEvent(Guid id)
    {
        var result = _events.FirstOrDefault(e => e.Id == id);

        if (result == null)
        {
            throw new EventNotFoundException(id);
        }

        return result;
    }
}
