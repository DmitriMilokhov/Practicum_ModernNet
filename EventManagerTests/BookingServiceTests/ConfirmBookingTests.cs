using EventManager.DataAccess;
using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagerTests.BookingServiceTests;

public class ConfirmBookingTests : BookingServiceTestsBase
{
    [Fact]
    public async Task ConfirmBooking_Positive()
    {
        //Arrange
        var notUpdatedBooking  = BookingFactory.CreateBookingDto(Guid.NewGuid()).ToEntity();
        var booking = BookingFactory.CreateBookingDto(Guid.NewGuid()).ToEntity();
       
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

        //Act
        await dbContext.Bookings.AddAsync(booking);
        await dbContext.SaveChangesAsync();
        await bookingService.ConfirmBooking(booking.Id);

        //Assert
        var updatedBooking = await bookingRepository.GetAsync(booking.Id);

        notUpdatedBooking.Status.Should().Be(BookingStatus.Pending);
        notUpdatedBooking.ProcessedAt.Should().BeNull();

        updatedBooking.Status.Should().Be(BookingStatus.Confirmed);
        updatedBooking.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ConfirmBooking_Negative()
    {
        //Arrange
        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Booking)} {randomGuid} is not found";

        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        //Act
        var action = async () => await bookingService.ConfirmBooking(randomGuid);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }
}
