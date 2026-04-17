using EventManager.Models;
using EventManager.Models.Filters;

namespace EventManager.Interfaces;

public interface IEventService
{
    PagedResponse<FullEventDto> GetEvents(EventFilter filter);
    FullEventDto GetEvent(Guid id);
    FullEventDto AddEvent(EventDto eventModel);
    void UpdateEvent(Guid eventId, EventDto data);
    void DeleteEvent(Guid eventId);
}
