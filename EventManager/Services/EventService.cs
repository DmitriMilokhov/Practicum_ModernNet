using EventManager.Interfaces;
using EventManager.Interfaces.IRepositories;
using EventManager.Models;
using EventManager.Models.Filters;

namespace EventManager.Services;

public class EventService(IEventsRepository repository) : IEventService
{
    public List<FullEventDto> GetEvents(EventFilter filter)
    {
        return repository
            .GetAll()
            .ApplyFilter(filter)
            .Select(e => e.ToDto())
            .ToList();
    }

    public FullEventDto GetEvent(Guid id)
    {
        return repository.Get(id).ToDto();
    }

    public FullEventDto AddEvent(EventDto eventModel)
    {
        var eventEntity = eventModel.ToEntity();
        repository.Add(eventEntity);

        return eventEntity.ToDto();
    }

    public void DeleteEvent(Guid eventId)
    {
        repository.Delete(eventId);
    }

    public void UpdateEvent(Guid eventId, EventDto data)
    {
        repository.Update(eventId, data.ToEntity());
    }

}
