using EventManager.Infrastructure;
using EventManager.Interfaces;
using EventManager.Models;

namespace EventManager.Services;

public class EventService() : IEventService
{
    private static readonly List<Event> _events = [];

    public IReadOnlyList<FullEventDto> GetAllEvents()
    {
        return _events.Select(e => e.ToDto()).ToList().AsReadOnly();
    }

    public FullEventDto GetEvent(Guid id)
    {
        var result = TryGetEvent(id);
        return result.ToDto();
    }

    public FullEventDto AddEvent(EventDto eventModel)
    {
        var eventEntity = eventModel.ToEntity();
        _events.Add(eventEntity);

        return eventEntity.ToDto();
    }

    public void DeleteEvent(Guid eventId)
    {
        var result = TryGetEvent(eventId);
        _events.Remove(result);
    }

    public void UpdateEvent(Guid eventId, EventDto data)
    {
        //TODO: maybe to return updated data (FullEventDto)?

        var result = TryGetEvent(eventId);
        result.Update(data.Title, data.Description, data.StartAt, data.EndAt);
    }

    private Event TryGetEvent(Guid id)
    {
        var result = _events.FirstOrDefault(e => e.Id == id);

        if (result == null)
        {
            throw new NotFoundException($"Event {id} is not found");
        }

        return result;
    }

}
