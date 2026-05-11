using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventManagerTests.BookingServiceTests;

public class RejectBookingAndReleaseEventTests : BookingServiceTestsBase
{
    [Fact]
    public async Task RejectBookingAndReleaseEventTests_Positive()
    {
        //Arrange
        var updatedBooking = BookingFactory.CreateBookingDto(Guid.NewGuid()).ToEntity();
        var someEvent = new Event(
            "testEvent",
            "descr",
            new DateTime(2026, 05, 20),
            new DateTime(2026, 06, 20),
            100);
        typeof(Event).GetProperty(nameof(Event.AvailableSeats))!.SetValue(someEvent, 99);


        BookingRepositoryMock.Setup(r => r.GetAsync(updatedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedBooking);
        EventRepositoryMock.Setup(r => r.GetAsync(updatedBooking.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(someEvent);

        //Act
        await BookingService.RejectBookingAndReleaseEvent(updatedBooking.Id);

        //Assert
        BookingRepositoryMock.Verify(r => r.GetAsync(updatedBooking.Id, It.IsAny<CancellationToken>()),
            Times.Once());
        EventRepositoryMock.Verify(r => r.GetAsync(updatedBooking.EventId, It.IsAny<CancellationToken>()),
            Times.Once());

        updatedBooking.Status.Should().Be(BookingStatus.Rejected);
        updatedBooking.ProcessedAt.Should().NotBeNull();
        someEvent.AvailableSeats.Should().Be(someEvent.TotalSeats);
    }

    [Fact]
    public async Task RejectBookingAndReleaseEventTests_Negative_NotFoundBooking()
    {
        //Arrange
        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Booking)} {randomGuid} is not found";

        BookingRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Throws(new EntityNotFoundException(nameof(Booking), randomGuid));

        //Act
        var action = async () => await BookingService.RejectBookingAndReleaseEvent(randomGuid);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }

    [Fact]
    public async Task RejectBookingAndReleaseEventTests_Negative_NotFoundEvent()
    {
        //Arrange
        var updatedBooking = BookingFactory.CreateBookingDto(Guid.NewGuid()).ToEntity();
        var expectedExceptionMessage = $"{nameof(Event)} {updatedBooking.EventId} is not found";

        BookingRepositoryMock.Setup(r => r.GetAsync(updatedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedBooking);
        EventRepositoryMock.Setup(r => r.GetAsync(updatedBooking.EventId, It.IsAny<CancellationToken>()))
            .Throws(new EntityNotFoundException(nameof(Event), updatedBooking.EventId));

        //Act
        var action = async () => await BookingService.RejectBookingAndReleaseEvent(updatedBooking.Id);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }
}
