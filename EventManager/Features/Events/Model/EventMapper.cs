namespace EventManager.Features.Events.Model;

public static class EventMapper
{
    public static Event ToEntity(this EventDto model)
    {
        return new Event(
            Guid.NewGuid(), 
            model.Title, 
            model.Description, 
            model.StartAt!.Value, 
            model.EndAt!.Value,
            model.TotalSeats);
    }

    public static FullEventDto ToDto(this Event model)
    {
        return new FullEventDto
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            StartAt = model.StartAt,
            EndAt = model.EndAt,
            TotalSeats = model.TotalSeats,
            AvailableSeats = model.AvailableSeats
        };
    }
}
