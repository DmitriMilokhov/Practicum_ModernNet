using EventManager.Models.Filters;

namespace EventManager.Interfaces.IFilters;

public interface IEventFilterValidator
{
    public void Validate(EventFilter filter);
}
