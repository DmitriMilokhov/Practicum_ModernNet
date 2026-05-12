using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Moq;

namespace EventManagerTests.BookingServiceTests;

public class RejectBookingTests : BookingServiceTestsBase
{
    [Fact]
    public async Task RejectBooking_Positive()
    {
        //Arrange
        var notUpdatedBooking = BookingFactory.CreateBookingDto(Guid.NewGuid()).ToEntity();
        var updatedBooking = BookingFactory.CreateBookingDto(Guid.NewGuid()).ToEntity();

        BookingRepositoryMock.Setup(r => r.GetAsync(updatedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedBooking);

        //Act
        await BookingService.RejectBooking(updatedBooking.Id);

        //Assert
        BookingRepositoryMock.Verify(r => r.GetAsync(updatedBooking.Id, It.IsAny<CancellationToken>()),
            Times.Once());

        notUpdatedBooking.Status.Should().Be(BookingStatus.Pending);
        notUpdatedBooking.ProcessedAt.Should().BeNull();

        updatedBooking.Status.Should().Be(BookingStatus.Rejected);
        updatedBooking.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RejectBooking_Negative()
    {
        //Arrange
        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Booking)} {randomGuid} is not found";

        BookingRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Throws(new EntityNotFoundException(nameof(Booking), randomGuid));

        //Act
        var action = async () => await BookingService.RejectBooking(randomGuid);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }
}
