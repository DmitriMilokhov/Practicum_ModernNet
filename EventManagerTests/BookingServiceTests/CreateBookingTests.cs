using EventManager.Features.Bookings.Model;
using EventManager.Features.Events;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Moq;
using System;

namespace EventManagerTests.BookingServiceTests;

public class CreateBookingTests : BookingServiceTestsBase
{
    [Fact]
    public async Task CreateBooking_Positive()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var bookingDto = BookingFactory.CreateBookingDto(eventId);

        EventRepositoryMock.Setup(r => r.ExistsAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
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
    }

    [Fact]
    public async Task CreateBooking_Positive_SeveralBookingsForOneEvent()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var bookingDtoFirstCall = BookingFactory.CreateBookingDto(eventId);
        var bookingDtoSecondCall = BookingFactory.CreateBookingDto(eventId);

        EventRepositoryMock.Setup(r => r.ExistsAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        BookingFactoryMock.SetupSequence(f => f.CreateBookingDto(eventId))
            .Returns(bookingDtoFirstCall)
            .Returns(bookingDtoSecondCall);

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
}
