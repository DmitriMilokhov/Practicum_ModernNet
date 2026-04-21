using EventManager.Interfaces;
using EventManager.Interfaces.IFilters;
using EventManager.Interfaces.IRepositories;
using EventManager.Models;
using EventManager.Models.Filters;

namespace EventManager.Services;

public class EventService(IEventRepository repository, IEventFilterValidator eventFilterValidator) : IEventService
{
    public async Task<PagedResponse<FullEventDto>> GetEventsAsync(EventFilter filter, CancellationToken ct = default)
    {
        eventFilterValidator.Validate(filter);

        ct.ThrowIfCancellationRequested();

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
        ct.ThrowIfCancellationRequested();
        var eventData = await repository.GetAsync(id, ct);
        return eventData.ToDto();
    }

    public async Task<FullEventDto> AddEventAsync(EventDto eventModel, CancellationToken ct = default)
    {
        var eventEntity = eventModel.ToEntity();

        ct.ThrowIfCancellationRequested();
        await repository.AddAsync(eventEntity, ct);

        return eventEntity.ToDto();
    }

    public async Task DeleteEventAsync(Guid eventId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await repository.DeleteAsync(eventId, ct);
    }

    public async Task UpdateEventAsync(Guid eventId, EventDto data, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await repository.UpdateAsync(eventId, data.ToEntity(), ct);
    }

}
