namespace EventManager.Features.Events.Model;

public static class EventFilterExtensions
{
    public static IEnumerable<Event> ApplyFilter(this IEnumerable<Event> events, EventFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            events = events.Where(e => e.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.From.HasValue)
        {
            events = events.Where(e => e.StartAt >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            events = events.Where(e => e.EndAt <= filter.To.Value);
        }

        return events;
    }
}
