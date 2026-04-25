using EventManager.Features.Events.Model;
using EventManager.Models;

namespace EventManager.Features.Events.Interfaces;

public interface IEventService
{
    Task<PagedResponse<FullEventDto>> GetEventsAsync(EventFilter filter, CancellationToken ct = default);
    Task<FullEventDto> GetEventAsync(Guid id, CancellationToken ct = default);
    Task<FullEventDto> AddEventAsync(EventDto eventModel, CancellationToken ct = default);
    Task UpdateEventAsync(Guid eventId, EventDto data, CancellationToken ct = default);
    Task DeleteEventAsync(Guid eventId, CancellationToken ct = default);
}
