using EventManager.Interfaces;
using EventManager.Interfaces.IFilters;
using EventManager.Interfaces.IRepositories;
using EventManager.Models;
using EventManager.Models.Filters;

namespace EventManager.Services;

public class EventService(IEventRepository repository, IEventFilterValidator eventFilterValidator) : IEventService
{
    public PagedResponse<FullEventDto> GetEvents(EventFilter filter)
    {
        eventFilterValidator.Validate(filter);

        var query = repository
            .GetAll()
            .ApplyFilter(filter);

        var totalItems = query.Count();

        var items= query
            .ApplyPagination(filter.Page, filter.PageSize)
            .Select(e => e.ToDto())
            .ToList();

        var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

        return new PagedResponse<FullEventDto>(items, filter.Page, filter.PageSize, totalItems, totalPages);
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
