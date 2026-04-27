using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Moq;
using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagerTests.BookingServiceTests;

public class UpdateBookingStatusTests : BookingServiceTestsBase
{
    [Theory]
    [InlineData(BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Rejected)]
    public async Task UpdateBookingStatusTests_Positive(BookingStatus updatedBookingStatus)
    {
        //Arrange
        var notUpdatedBooking = BookingFactory.CreateBookingDto(Guid.NewGuid()).ToEntity();
        var updatedBooking = BookingFactory.CreateBookingDto(Guid.NewGuid()).ToEntity();

        BookingRepositoryMock.Setup(r => r.GetAsync(updatedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedBooking);

        //Act
        await BookingService.UpdateBookingStatusAsync(updatedBooking.Id, updatedBookingStatus);

        //Assert
        BookingRepositoryMock.Verify(r => r.GetAsync(updatedBooking.Id, It.IsAny<CancellationToken>()),
            Times.Once());

        notUpdatedBooking.Status.Should().Be(BookingStatus.Pending);
        notUpdatedBooking.ProcessedAt.Should().BeNull();

        updatedBooking.Status.Should().Be(updatedBookingStatus);
        updatedBooking.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateBookingStatusTests_Negative()
    {
        //Arrange
        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Booking)} {randomGuid} is not found";

        BookingRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Throws(new EntityNotFoundException(nameof(Booking), randomGuid));

        //Act
        var action = async () => await BookingService.UpdateBookingStatusAsync(randomGuid, BookingStatus.Confirmed);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }
}
