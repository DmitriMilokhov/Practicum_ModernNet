using EventManager.Features.Bookings.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Moq;

namespace EventManagerTests.BookingServiceTests;

public class GetBookingByIdTests : BookingServiceTestsBase
{
    [Fact]
    public async Task GetBookingById_Positive()
    {
        //Arrange
        var bookingToFind = BookingFactory.CreateBookingDto(Guid.NewGuid()).ToEntity();
        BookingRepositoryMock.Setup(r => r.GetAsync(bookingToFind.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookingToFind);

        //Act
        var result = await BookingService.GetBookingByIdAsync(bookingToFind.Id);

        //Assert
        BookingRepositoryMock.Verify(r => r.GetAsync(bookingToFind.Id, It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(bookingToFind, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetBookingById_Negative()
    {
        //Arrange
        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Booking)} {randomGuid} is not found";

        BookingRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Throws(new EntityNotFoundException(nameof(Booking), randomGuid));

        //Act
        var action = async () => await BookingService.GetBookingByIdAsync(randomGuid);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }
}
