using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using System.Collections.Concurrent;

namespace EventManager.Features.Events;

public class InMemoryEventRepository : IEventRepository
{
    private readonly ConcurrentDictionary<Guid, Event> _events = [];

    public Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<Event>>([.. _events.Values]);
    }

    public Task<Event> GetAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(TryGetEvent(id));
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        var isEventExist = _events.ContainsKey(id);
        return Task.FromResult(isEventExist);
    }

    public Task AddAsync(Event eventModel, CancellationToken ct = default)
    {
        _events.TryAdd(eventModel.Id, eventModel);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Guid eventId, Event data, CancellationToken ct = default)
    {
        var result = TryGetEvent(eventId);
        result.Update(data.Title, data.Description, data.StartAt, data.EndAt, data.TotalSeats);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid eventId, CancellationToken ct = default)
    {
        if (!_events.TryRemove(eventId, out _))
        {
            throw new EntityNotFoundException(nameof(Event), eventId);
        }
        return Task.CompletedTask;
    }

    private Event TryGetEvent(Guid id)
    {
        if (!_events.TryGetValue(id, out var result))
        {
            throw new EntityNotFoundException(nameof(Event), id);
        }

        return result;
    }
}
