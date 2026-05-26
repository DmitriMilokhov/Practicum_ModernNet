using EventManager.Features.Bookings.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.DataAccess.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.Property(b => b.Id).ValueGeneratedNever();
        builder.Property(b => b.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.CreatedAt).IsRequired();

        builder.HasOne(b => b.Event).WithMany(e => e.Bookings).HasForeignKey(b => b.EventId);
    }
}
