using EventManager.Models;

namespace EventManager.Interfaces.IRepositories;

public interface IEventsRepository
{
    IReadOnlyList<Event> GetAll();
    Event Get(Guid id);
    void Add(Event eventModel);
    void Update(Guid eventId, Event data);
    void Delete(Guid eventId);
}
