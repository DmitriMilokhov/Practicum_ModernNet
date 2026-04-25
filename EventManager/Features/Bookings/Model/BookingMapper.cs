namespace EventManager.Features.Bookings.Model;

public static class BookingMapper
{
    public static Booking ToEntity(this BookingDto dto)
    {
        return new Booking
        {
            Id = dto.Id,
            EventId = dto.EventId,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt!.Value,
            ProcessedAt = dto.ProcessedAt
        };
    }

    public static BookingDto ToDto(this Booking entity)
    {
        return new BookingDto
        {
            Id = entity.Id,
            EventId = entity.EventId,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            ProcessedAt = entity.ProcessedAt 
        };
    }
}
