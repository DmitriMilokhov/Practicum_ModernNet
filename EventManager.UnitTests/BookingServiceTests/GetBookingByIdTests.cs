using EventManager.DataAccess;
using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.UnitTests.BookingServiceTests;

public class GetBookingByIdTests : BookingServiceTestsBase
{
    [Fact]
    public async Task GetBookingById_Positive()
    {
        //Arrange
        var bookingToFind = BookingFactory.CreateBookingDto(Guid.NewGuid()).ToEntity();
        
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        
        await dbContext.Bookings.AddAsync(bookingToFind);
        await dbContext.SaveChangesAsync();
        
        //Act
        var result =  await bookingService.GetBookingByIdAsync(bookingToFind.Id);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(bookingToFind, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetBookingById_Negative()
    {
        //Arrange
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Booking)} {randomGuid} is not found";

        //Act
        var action = async () => await bookingService.GetBookingByIdAsync(randomGuid);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }
}
