namespace EventManager.Models;

public static class EventMapper
{
    public static Event ToEntity(this EventDto model)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = model.Title,
            Description = model.Description,
            StartAt = model.StartAt,
            EndAt = model.EndAt
        };
    }

    public static FullEventDto ToDto(this Event model)
    {
        return new FullEventDto
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            StartAt = model.StartAt,
            EndAt = model.EndAt
        };
    }
}
