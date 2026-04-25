using EventManager.Features.Events.Model;

namespace EventManager.Features.Events.Interfaces;

public interface IEventFilterValidator
{
    public void Validate(EventFilter filter);
}
