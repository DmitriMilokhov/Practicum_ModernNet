using EventManager.Models;

namespace EventManager.Interfaces;

public interface IEventService
{
    IReadOnlyList<FullEventDto> GetAllEvents();
    FullEventDto GetEvent(Guid id);
    FullEventDto AddEvent(EventDto eventModel);
    void UpdateEvent(Guid eventId, EventDto data);
    void DeleteEvent(Guid eventId);
}
