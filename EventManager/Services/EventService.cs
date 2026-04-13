using EventManager.Interfaces;
using EventManager.Interfaces.IRepositories;
using EventManager.Models;

namespace EventManager.Services;

public class EventService(IEventsRepository repository) : IEventService
{
    public IReadOnlyList<FullEventDto> GetAllEvents()
    {
        return repository.GetAll()
                         .Select(e => e.ToDto())
                         .ToList()
                         .AsReadOnly();
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
