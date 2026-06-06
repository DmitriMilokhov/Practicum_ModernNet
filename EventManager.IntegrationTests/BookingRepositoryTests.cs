using EventManager.DataAccess;
using EventManager.Features.Bookings;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventManager.IntegrationTests;

public class BookingRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();
    public async Task DisposeAsync() => await _postgres.DisposeAsync();
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.Migrate();
        return context;
    }

    private async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();
        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE events, bookings RESTART IDENTITY CASCADE");
    }

    #region Create

    [Fact]
    public async Task AddBooking_Positive()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var eventModel = new Event(
            "Test Event",
            "Test description",
            new DateTime(2025, 02, 02, 0,0,0, DateTimeKind.Utc),
            new DateTime(2025, 04, 04, 0, 0, 0, DateTimeKind.Utc),
            20);

        await context.Events.AddAsync(eventModel);
        await context.SaveChangesAsync();

        var bookingModel = new Booking(Guid.NewGuid(), eventModel.Id, BookingStatus.Pending, DateTime.UtcNow);

        await using var repositoryContext = CreateContext();
        var repository = new BookingRepository(repositoryContext);

        // Act
        await repository.AddAsync(bookingModel);
        await repository.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateContext();
        var savedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.EventId == eventModel.Id);

        savedBooking.Should().NotBeNull();
        savedBooking.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task AddBooking_Negative_EventNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();

        var bookingModel = new Booking(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Pending, DateTime.UtcNow);

        await using var repositoryContext = CreateContext();
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
        await ResetDatabaseAsync();
        await using var context = CreateContext();

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

        await using var repositoryContext = CreateContext();
        var repository = new BookingRepository(repositoryContext);

        // Act
        var result = await repository.GetAsync(bookingModel.Id);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(eventModel.Id);
    }

    [Fact]
    public async Task GetBooking_Negative_NotFound()
    {
        // Arrange
        await ResetDatabaseAsync();

        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Booking)} {randomGuid} is not found";

        await using var repositoryContext = CreateContext();
        var repository = new BookingRepository(repositoryContext);

        // Act
        var action = async () => await repository.GetAsync(randomGuid);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }

    #endregion
}
