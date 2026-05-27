using EventManager.DataAccess;
using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventManagerTests.BookingServiceTests;

public class RejectBookingAndReleaseEventTests : BookingServiceTestsBase
{
    [Fact]
    public async Task RejectBookingAndReleaseEventTests_Positive()
    {
        // Arrange
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

        typeof(Event).GetProperty(nameof(Event.AvailableSeats))!.SetValue(someEvent, 99);

        var updatedBooking = BookingFactory.CreateBookingDto(someEvent.Id).ToEntity();

        await dbContext.Events.AddAsync(someEvent);
        await dbContext.Bookings.AddAsync(updatedBooking);
        await dbContext.SaveChangesAsync();

        // Act
        await bookingService.RejectBookingAndReleaseEvent(updatedBooking.Id);

        // Assert
        var rejectedBooking = await bookingRepository.GetAsync(updatedBooking.Id);

        var updatedEvent =  await eventRepository.GetAsync(updatedBooking.EventId);

        rejectedBooking.Status.Should().Be(BookingStatus.Rejected);
        rejectedBooking.ProcessedAt.Should().NotBeNull();

        updatedEvent.AvailableSeats.Should().Be(updatedEvent.TotalSeats);
    }

    [Fact]
    public async Task RejectBookingAndReleaseEventTests_Negative_NotFoundBooking()
    {
        //Arrange
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Booking)} {randomGuid} is not found";

        //Act
        var action = async () => await bookingService.RejectBookingAndReleaseEvent(randomGuid);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }

    [Fact]
    public async Task RejectBookingAndReleaseEventTests_Negative_NotFoundEvent()
    {
        //Arrange
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var updatedBooking = BookingFactory.CreateBookingDto(Guid.NewGuid()).ToEntity();
        var expectedExceptionMessage = $"{nameof(Event)} {updatedBooking.EventId} is not found";

        await dbContext.Bookings.AddAsync(updatedBooking);
        await dbContext.SaveChangesAsync();

        //Act
        var action = async () => await bookingService.RejectBookingAndReleaseEvent(updatedBooking.Id);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }
}
