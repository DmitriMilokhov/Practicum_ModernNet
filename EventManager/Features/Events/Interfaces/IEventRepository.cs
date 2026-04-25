using EventManager.Features.Events.Model;

namespace EventManager.Features.Events.Interfaces;

public interface IEventRepository
{
    Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken ct = default);
    Task<Event> GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Event eventModel, CancellationToken ct = default);
    Task UpdateAsync(Guid eventId, Event data, CancellationToken ct = default);
    Task DeleteAsync(Guid eventId, CancellationToken ct = default);
}
