using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;

namespace EventManager.Features.Events;

public class InMemoryEventRepository : IEventRepository
{
    private readonly List<Event> _events = [];

    public Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<Event>>([.. _events]);
    }

    public Task<Event> GetAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(TryGetEvent(id));
    }

    public Task AddAsync(Event eventModel, CancellationToken ct = default)
    {
        _events.Add(eventModel);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Guid eventId, Event data, CancellationToken ct = default)
    {
        var result = TryGetEvent(eventId);
        result.Update(data.Title, data.Description, data.StartAt, data.EndAt);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid eventId, CancellationToken ct = default)
    {
        var result = TryGetEvent(eventId);
        _events.Remove(result);
        return Task.CompletedTask;
    }

    private Event TryGetEvent(Guid id)
    {
        var result = _events.FirstOrDefault(e => e.Id == id);

        if (result == null)
        {
            throw new EntityNotFoundException(nameof(Event), id);
        }

        return result;
    }
}
