using EventManager.DataAccess;
using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Constants;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagerTests.BookingServiceTests;

public class CreateBookingTests : BookingServiceTestsBase
{
    [Fact]
    public async Task CreateBooking_Positive()
    {
        //Arrange
        using var scope = CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var someEvent = new Event(
            "testEvent",
            "descr",
            new DateTime(2026, 05, 20),
            new DateTime(2026, 06, 20),
            100);

        await dbContext.Events.AddAsync(someEvent);
        await dbContext.SaveChangesAsync();
        var eventId = someEvent.Id;

        //Act
        var result = await bookingService.CreateBookingAsync(eventId);

        //Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Status.Should().Be(BookingStatus.Pending);
        result.EventId.Should().Be(eventId);

        var savedBooking = await bookingRepository.GetAsync(result.Id);

        savedBooking.Should().NotBeNull();
        savedBooking.EventId.Should().Be(eventId);
        savedBooking.Status.Should().Be(BookingStatus.Pending);
        
        var updatedEvent = await eventRepository.GetAsync(eventId);

        updatedEvent.AvailableSeats.Should().Be(updatedEvent.TotalSeats - 1);
    }

    [Fact]
    public async Task CreateBooking_Positive_SeveralBookingsForOneEvent()
    {
        //Arrange
        using var scope = CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var someEvent = new Event(
            "testEvent",
            "descr",
            new DateTime(2026, 05, 20),
            new DateTime(2026, 06, 20),
            2);

        await dbContext.Events.AddAsync(someEvent);
        await dbContext.SaveChangesAsync();
        var eventId = someEvent.Id;

        //Act
        var firstBookingResult = await bookingService.CreateBookingAsync(eventId);
        var secondBookingResult = await bookingService.CreateBookingAsync(eventId);

        //Assert

        firstBookingResult.Should().NotBeNull();
        secondBookingResult.Should().NotBeNull();

        firstBookingResult.Id.Should().NotBe(secondBookingResult.Id);
        firstBookingResult.EventId.Should().Be(secondBookingResult.EventId);

        var firstSavedBooking = await bookingRepository.GetAsync(firstBookingResult.Id);
        var secondSavedBooking = await bookingRepository.GetAsync(secondBookingResult.Id);

        firstSavedBooking.Should().NotBeNull();
        secondSavedBooking.Should().NotBeNull();

        var updatedEvent = await eventRepository.GetAsync(eventId);

        updatedEvent.AvailableSeats.Should().Be(0);
    }

    [Fact]
    public async Task CreateBooking_Negative_EventNotFound()
    {
        //Arrange
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var eventId = Guid.NewGuid();
        var expectedExceptionMessage = $"Event {eventId} is not found";

        //Act
        var action = async () => await bookingService.CreateBookingAsync(eventId);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }

    [Fact]
    public async Task CreateBooking_Negative_NoAvailableSeats()
    {
        //Arrange
        using var scope = CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var totalSeats = 3;
        var someEvent = new Event(
            "testEvent",
            "descr",
            new DateTime(2026, 05, 20),
            new DateTime(2026, 06, 20),
            2);

        await dbContext.Events.AddAsync(someEvent);
        await dbContext.SaveChangesAsync();

        var eventId = someEvent.Id;

        //Act
        var action = async () => 
        {
            for (var i = 0; i < totalSeats + 1; i++)
            {
                await bookingService.CreateBookingAsync(eventId);
            }
        };

        //Assert
        await action.Should().ThrowAsync<NoAvailableSeatsException>().WithMessage(Constants.NoAvailableSeatsExceptionMsg);
    }
}
