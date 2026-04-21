using EventManager.Models;
using EventManager.Models.Filters;

namespace EventManager.Interfaces;

public interface IEventService
{
    Task<PagedResponse<FullEventDto>> GetEventsAsync(EventFilter filter, CancellationToken ct = default);
    Task<FullEventDto> GetEventAsync(Guid id, CancellationToken ct = default);
    Task<FullEventDto> AddEventAsync(EventDto eventModel, CancellationToken ct = default);
    Task UpdateEventAsync(Guid eventId, EventDto data, CancellationToken ct = default);
    Task DeleteEventAsync(Guid eventId, CancellationToken ct = default);
}
