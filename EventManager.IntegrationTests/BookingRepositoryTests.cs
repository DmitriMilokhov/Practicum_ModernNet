using EventManager.Features.Bookings;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using EventManager.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EventManager.IntegrationTests;

[Collection("Postgres")]
public class BookingRepositoryTests(PostgreSqlFixture fixture) 
{
    #region Create

    [Fact]
    public async Task AddBooking_Positive()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();
        await using var context = fixture.CreateContext();

        var eventModel = new Event(
            "Test Event",
            "Test description",
            new DateTime(2025, 02, 02, 0,0,0, DateTimeKind.Utc),
            new DateTime(2025, 04, 04, 0, 0, 0, DateTimeKind.Utc),
            20);

        await context.Events.AddAsync(eventModel);
        await context.SaveChangesAsync();

        var bookingModel = new Booking(Guid.NewGuid(), eventModel.Id, BookingStatus.Pending, DateTime.UtcNow);

        await using var repositoryContext = fixture.CreateContext();
        var repository = new BookingRepository(repositoryContext);

        // Act
        await repository.AddAsync(bookingModel);
        await repository.SaveChangesAsync();

        // Assert
        await using var verifyContext = fixture.CreateContext();
        var savedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.EventId == eventModel.Id);

        savedBooking.Should().NotBeNull();
        savedBooking.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task AddBooking_Negative_EventNotFound()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var bookingModel = new Booking(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Pending, DateTime.UtcNow);

        await using var repositoryContext = fixture.CreateContext();
        var repository = new BookingRepository(repositoryContext);

        // Act
        var action = async () =>
        {
            await repository.AddAsync(bookingModel);
            await repository.SaveChangesAsync();
        };

        // Assert
        await action.Should().ThrowAsync<DbUpdateException>();
    }

    #endregion

    #region ReadSingle

    [Fact]
    public async Task GetBooking_Positive()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();
        await using var context = fixture.CreateContext();

        var eventModel = new Event(
            "Test Event",
            "Test description",
            new DateTime(2025, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 04, 04, 0, 0, 0, DateTimeKind.Utc),
            20);

        await context.Events.AddAsync(eventModel);
        await context.SaveChangesAsync();

        var bookingModel = new Booking(Guid.NewGuid(), eventModel.Id, BookingStatus.Pending, DateTime.UtcNow);
        await context.Bookings.AddAsync(bookingModel);
        await context.SaveChangesAsync();

        await using var repositoryContext = fixture.CreateContext();
        var repository = new BookingRepository(repositoryContext);

        // Act
        var result = await repository.GetAsync(bookingModel.Id);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(eventModel.Id);
    }

    [Fact]
    public async Task GetBooking_Positive_StatusChangedConfirmed()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();
        await using var context = fixture.CreateContext();

        var eventModel = new Event(
            "Test Event",
            "Test description",
            new DateTime(2025, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 04, 04, 0, 0, 0, DateTimeKind.Utc),
            20);

        await context.Events.AddAsync(eventModel);
        await context.SaveChangesAsync();

        var bookingModel = new Booking(Guid.NewGuid(), eventModel.Id, BookingStatus.Pending, DateTime.UtcNow);
        await context.Bookings.AddAsync(bookingModel);
        await context.SaveChangesAsync();

        await using var repositoryContext = fixture.CreateContext();
        var bookingRepository = new BookingRepository(repositoryContext);
        var eventRepository = new EventRepository(repositoryContext);

        var bookingService = new BookingService(new BookingFactory(), bookingRepository, eventRepository, new EventBookingLockProvider());

        // Assert
        await bookingService.ConfirmBooking(bookingModel.Id);
        var result = await bookingRepository.GetAsync(bookingModel.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task GetBooking_Positive_StatusChangedRejected()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();
        await using var context = fixture.CreateContext();

        var eventModel = new Event(
            "Test Event",
            "Test description",
            new DateTime(2025, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 04, 04, 0, 0, 0, DateTimeKind.Utc),
            20);

        await context.Events.AddAsync(eventModel);
        await context.SaveChangesAsync();

        var bookingModel = new Booking(Guid.NewGuid(), eventModel.Id, BookingStatus.Pending, DateTime.UtcNow);
        await context.Bookings.AddAsync(bookingModel);
        await context.SaveChangesAsync();

        await using var repositoryContext = fixture.CreateContext();
        var bookingRepository = new BookingRepository(repositoryContext);
        var eventRepository = new EventRepository(repositoryContext);

        var bookingService = new BookingService(new BookingFactory(), bookingRepository, eventRepository, new EventBookingLockProvider());

        // Assert
        await bookingService.RejectBooking(bookingModel.Id);
        var result = await bookingRepository.GetAsync(bookingModel.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Rejected);
    }

    [Fact]
    public async Task GetBooking_Negative_NotFound()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Booking)} {randomGuid} is not found";

        await using var repositoryContext = fixture.CreateContext();
        var repository = new BookingRepository(repositoryContext);

        // Act
        var action = async () => await repository.GetAsync(randomGuid);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }

    #endregion
}
