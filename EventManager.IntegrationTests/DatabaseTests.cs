using EventManager.Features.Bookings.Model;
using EventManager.Features.Events;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using EventManager.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EventManager.IntegrationTests;

[Collection("Postgres")]
public class DatabaseTests(PostgreSqlFixture fixture) 
{
    [Fact]
    public async Task Migrations_Positive_ShouldBeApplied()
    {
        // Arrange
        await using var context = fixture.CreateContext();

        // Act
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        // Assert
        pendingMigrations.Should().BeEmpty();
    }

    [Fact]
    public async Task Migrations_Positive_ContainsTables()
    {
        await using var context = fixture.CreateContext();

        var tables = await context.Database
            .SqlQueryRaw<string>(
                """
                SELECT table_name
                FROM information_schema.tables
                WHERE table_schema = 'public'
                """)
            .ToListAsync();

        tables.Should().Contain("events");
        tables.Should().Contain("bookings");
    }

    [Fact]
    public void Booking_Positive_HasForeignKey()
    {
        using var context = fixture.CreateContext();

        var entity = context.Model.FindEntityType(typeof(Booking));

        var fk = entity!.GetForeignKeys().Single();

        fk.PrincipalEntityType.ClrType.Should().Be(typeof(Event));
    }

    [Fact]
    public async Task CascadeDeleteEvent_Positive_WithAllBookings()
    {
        //Arrange
        await fixture.ResetDatabaseAsync();
        await using var context = fixture.CreateContext();

        var eventModel = new Event(
            "Test Event",
            "Test description",
            new DateTime(2025, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 04, 04, 0, 0, 0, DateTimeKind.Utc),
            20);

        await context.Events.AddAsync(eventModel);

        var bookingModel1 = new Booking(Guid.NewGuid(), eventModel.Id, BookingStatus.Pending, DateTime.UtcNow);
        var bookingModel2 = new Booking(Guid.NewGuid(), eventModel.Id, BookingStatus.Pending, DateTime.UtcNow);

        await context.Bookings.AddRangeAsync(bookingModel1, bookingModel2);

        await context.SaveChangesAsync();

        await using var repositoryContext = fixture.CreateContext();
        var repository = new EventRepository(repositoryContext);

        //Act
        await repository.DeleteAsync(eventModel.Id);
        await repository.SaveChangesAsync();

        //Assert
        await using var assertionContext = fixture.CreateContext();
        var bookingsForEvent = await assertionContext.Bookings.Where(e => e.EventId == eventModel.Id).ToListAsync();
        bookingsForEvent.Should().BeEmpty();
    }
}
