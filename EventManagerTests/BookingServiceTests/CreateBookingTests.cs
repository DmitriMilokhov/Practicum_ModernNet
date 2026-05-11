using EventManager.Features.Bookings.Model;
using EventManager.Features.Events;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using EventManager.Infrastructure.Constants;

namespace EventManagerTests.BookingServiceTests;

public class CreateBookingTests : BookingServiceTestsBase
{
    [Fact]
    public async Task CreateBooking_Positive()
    {
        //Arrange
        var someEvent = new Event(
            "testEvent",
            "descr",
            new DateTime(2026, 05, 20),
            new DateTime(2026, 06, 20),
            100);
        var eventId = someEvent.Id;
        var bookingDto = BookingFactory.CreateBookingDto(eventId);

        EventRepositoryMock.Setup(r => r.ExistsAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        EventRepositoryMock.Setup(r => r.GetAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(someEvent);
        BookingFactoryMock.Setup(f => f.CreateBookingDto(eventId)).Returns(bookingDto);

        //Act
        var result = await BookingService.CreateBookingAsync(eventId);

        //Assert
        BookingRepositoryMock.Verify(r => r.AddAsync(
                It.Is<Booking>(b =>
                    b.Id == bookingDto.Id &&
                    b.EventId == bookingDto.EventId &&
                    b.Status == bookingDto.Status &&
                    b.CreatedAt == bookingDto.CreatedAt),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Status.Should().Be(BookingStatus.Pending);
        result.Should().BeEquivalentTo(bookingDto);

        someEvent.AvailableSeats.Should().Be(someEvent.TotalSeats - 1);
    }

    [Fact]
    public async Task CreateBooking_Positive_SeveralBookingsForOneEvent()
    {
        //Arrange
        var someEvent = new Event(
            "testEvent",
            "descr",
            new DateTime(2026, 05, 20),
            new DateTime(2026, 06, 20),
            2);
        var eventId = someEvent.Id;

        EventRepositoryMock.Setup(r => r.ExistsAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        EventRepositoryMock.Setup(r => r.GetAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(someEvent);
        BookingFactoryMock.Setup(f => f.CreateBookingDto(eventId)).Returns(() => BookingFactory.CreateBookingDto(eventId));

        //Act
        var firstBookingResult = await BookingService.CreateBookingAsync(eventId);
        var secondBookingResult = await BookingService.CreateBookingAsync(eventId);

        //Assert

        firstBookingResult.Should().NotBeNull();
        secondBookingResult.Should().NotBeNull();

        firstBookingResult.Id.Should().NotBe(secondBookingResult.Id);
        firstBookingResult.EventId.Should().Be(secondBookingResult.EventId);
    }

    [Fact]
    public async Task CreateBooking_Negative_EventNotFound()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var expectedExceptionMessage = $"Event {eventId} is not found";

        EventRepositoryMock.Setup(r => r.ExistsAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        //Act
        var action = async () => await BookingService.CreateBookingAsync(eventId);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }

    [Fact]
    public async Task CreateBooking_Negative_NoAvailableSeats()
    {
        //Arrange
        var totalSeats = 3;
        var someEvent = new Event(
            "testEvent",
            "descr",
            new DateTime(2026, 05, 20),
            new DateTime(2026, 06, 20),
            totalSeats);
        var eventId = someEvent.Id;

        EventRepositoryMock.Setup(r => r.ExistsAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        EventRepositoryMock.Setup(r => r.GetAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(someEvent);
        BookingFactoryMock.Setup(f => f.CreateBookingDto(eventId)).Returns(() => BookingFactory.CreateBookingDto(eventId));

        //Act
        var action = async () => 
        {
            for (var i = 0; i < totalSeats + 1; i++)
            {
                await BookingService.CreateBookingAsync(eventId);
            }
        };

        //Assert
        await action.Should().ThrowAsync<NoAvailableSeatsException>().WithMessage(Constants.NoAvailableSeatsExceptionMsg);
    }
}
