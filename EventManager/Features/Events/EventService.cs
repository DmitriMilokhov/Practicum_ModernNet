using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Models;

namespace EventManager.Features.Events;

public class EventService(IEventRepository repository, IEventFilterValidator eventFilterValidator) : IEventService
{
    public async Task<PagedResponse<FullEventDto>> GetEventsAsync(EventFilter filter, CancellationToken ct = default)
    {
        eventFilterValidator.Validate(filter);

        var data = await repository.GetAllAsync(ct);
        var query = data.ApplyFilter(filter);

        var totalItems = query.Count();

        var items= query
            .ApplyPagination(filter.Page, filter.PageSize)
            .Select(e => e.ToDto())
            .ToList();

        var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

        return new PagedResponse<FullEventDto>(items, filter.Page, filter.PageSize, totalItems, totalPages);
    }

    public async Task<FullEventDto> GetEventAsync(Guid id, CancellationToken ct = default)
    {
        var eventData = await repository.GetAsync(id, ct);
        return eventData.ToDto();
    }

    public async Task<FullEventDto> AddEventAsync(EventDto eventModel, CancellationToken ct = default)
    {
        var eventEntity = eventModel.ToEntity();

        await repository.AddAsync(eventEntity, ct);
        await repository.SaveChangesAsync(ct);

        return eventEntity.ToDto();
    }

    public async Task DeleteEventAsync(Guid eventId, CancellationToken ct = default)
    {
        await repository.DeleteAsync(eventId, ct);
        await repository.SaveChangesAsync(ct);
    }

    public async Task UpdateEventAsync(Guid eventId, EventDto data, CancellationToken ct = default)
    {
        await repository.UpdateAsync(eventId, data.ToEntity(), ct);
        await repository.SaveChangesAsync(ct);
    }

}
