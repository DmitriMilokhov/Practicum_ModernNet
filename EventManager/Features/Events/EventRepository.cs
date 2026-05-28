using EventManager.DataAccess;
using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Features.Events;

public class EventRepository(AppDbContext context) : IEventRepository
{
    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Events.AsNoTracking().ToListAsync(ct);
    }

    public async Task<Event> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await TryGetEventAsync(id, ct);
    }

    public async Task AddAsync(Event eventModel, CancellationToken ct = default)
    {
        await context.Events.AddAsync(eventModel, ct);
    }

    public async Task UpdateAsync(Guid eventId, Event data, CancellationToken ct = default)
    {
        var foundEvent = await TryGetEventAsync(eventId, ct);
        foundEvent.Update(data.Title, data.Description, data.StartAt, data.EndAt, data.TotalSeats);
    }

    public async Task DeleteAsync(Guid eventId, CancellationToken ct = default)
    {
        var foundEvent = await TryGetEventAsync(eventId, ct);
        context.Events.Remove(foundEvent);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }

    private async Task<Event> TryGetEventAsync(Guid id, CancellationToken ct = default)
    {
        var foundEvent = await context.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

        if (foundEvent is null)
        {
            throw new EntityNotFoundException(nameof(Event), id);
        }

        return foundEvent;
    }

}
