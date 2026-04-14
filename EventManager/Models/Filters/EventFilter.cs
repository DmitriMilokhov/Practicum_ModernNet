namespace EventManager.Models.Filters;

public class EventFilter
{
    /// <summary>
    /// Filter by title (case-insensitive, substring match)
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Start date (inclusive). Format: yyyy-MM-dd 
    /// </summary>
    public DateTime? From { get; init; }

    /// <summary>
    /// End date (inclusive). Format: yyyy-MM-dd 
    public DateTime? To { get; init; }
}

public static class EventFilterExtension
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
