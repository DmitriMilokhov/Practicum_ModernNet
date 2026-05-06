namespace EventManager.Features.Bookings.Model;

public static class BookingMapper
{
    public static Booking ToEntity(this BookingDto dto)
    {
        return new Booking(dto.Id, dto.EventId, dto.Status, dto.CreatedAt!.Value);
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
