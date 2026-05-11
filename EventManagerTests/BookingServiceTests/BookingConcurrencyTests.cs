using EventManager.Features.Bookings;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Moq;

namespace EventManagerTests.BookingServiceTests;

public class BookingConcurrencyTests : BookingServiceTestsBase
{
    [Fact]
    public async Task CreateBooking_Positive_SeveralBookingsForOneEvent()
    {
        //Arrange
        var someEvent = new Event(
            "testEvent",
            "descr",
            new DateTime(2026, 05, 20),
            new DateTime(2026, 06, 20),
            5);
        var eventId = someEvent.Id;

        EventRepositoryMock.Setup(r => r.ExistsAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        EventRepositoryMock.Setup(r => r.GetAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(someEvent);
        BookingFactoryMock.Setup(f => f.CreateBookingDto(eventId)).Returns(() => BookingFactory.CreateBookingDto(eventId));

        var tasks = Enumerable.Range(0, 20)
            .Select(async _ =>
            {
                try
                {
                    var result = await BookingService.CreateBookingAsync(eventId);

                    return new
                    {
                        Success = true,
                        Exception = (Exception?)null
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Exception = (Exception?)ex
                    };
                }
            });

        //Act
        var results = await Task.WhenAll(tasks);

        //Assert

        results.Count(r => r.Success).Should().Be(5);

        results.Count(r =>
                r.Exception is NoAvailableSeatsException)
            .Should()
            .Be(15);
    }

    [Fact]
    public async Task CreateBooking_Positive_UniqueIds()
    {
        //Arrange
        var totalSeats = 10;
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

        var tasks = Enumerable.Range(0, totalSeats)
            .Select(async _ =>
            {
                try
                {
                    var result = await BookingService.CreateBookingAsync(eventId);

                    return new
                    {
                        Success = true,
                        Booking = (BookingDto?)result,
                        Exception = (Exception?)null
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Success = false,
                        Booking = (BookingDto?)null,
                        Exception = (Exception?)ex
                    };
                }
            });

        //Act
        var results = await Task.WhenAll(tasks);

        //Assert

        results.Count(r => r.Success).Should().Be(totalSeats);
        results.Count(r => !r.Success).Should().Be(0);
        results.Where(r => r.Success)
            .Select(r => r.Booking!.Id)
            .Distinct()
            .Count()
            .Should()
            .Be(totalSeats);
    }
}
