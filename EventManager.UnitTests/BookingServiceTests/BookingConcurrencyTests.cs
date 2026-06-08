using EventManager.DataAccess;
using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.UnitTests.BookingServiceTests;

public class BookingConcurrencyTests : BookingServiceTestsBase
{
    [Fact]
    public async Task CreateBooking_Positive_SeveralBookingsForOneEvent()
    {
        // Arrange

        using var setupScope = CreateScope();

        var setupDbContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var someEvent = new Event(
            "testEvent",
            "descr",
            new DateTime(2026, 05, 20),
            new DateTime(2026, 06, 20),
            5);

        await setupDbContext.Events.AddAsync(someEvent);
        await setupDbContext.SaveChangesAsync();

        var eventId = someEvent.Id;

        var tasks = Enumerable.Range(0, 20)
            .Select(async _ =>
            {
                using var scope = CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                try
                {
                    await bookingService.CreateBookingAsync(eventId);

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

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert

        results.Count(r => r.Success).Should().Be(5);

        results.Count(r =>  r.Exception is NoAvailableSeatsException).Should().Be(15);

        using var assertScope = CreateScope();
        var eventRepository = assertScope.ServiceProvider.GetRequiredService<IEventRepository>();

        var updatedEvent = await eventRepository.GetAsync(eventId);
        updatedEvent.AvailableSeats.Should().Be(0);
    }

    [Fact]
    public async Task CreateBooking_Positive_UniqueIds()
    {
        // Arrange

        using var setupScope = CreateScope();
        var setupDbContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var totalSeats = 10;

        var someEvent = new Event(
            "testEvent",
            "descr",
            new DateTime(2026, 05, 20),
            new DateTime(2026, 06, 20),
            totalSeats);

        await setupDbContext.Events.AddAsync(someEvent);
        await setupDbContext.SaveChangesAsync();

        var eventId = someEvent.Id;
        var tasks = Enumerable.Range(0, totalSeats)
            .Select(async _ =>
            {
                using var scope = CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                try
                {
                    var result = await bookingService.CreateBookingAsync(eventId);

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

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert

        results.Count(r => r.Success).Should().Be(totalSeats);
        results.Count(r => !r.Success).Should().Be(0);

        results.Where(r => r.Success)
            .Select(r => r.Booking!.Id)
            .Distinct()
            .Count()
            .Should()
            .Be(totalSeats);

        using var assertScope = CreateScope();
        var eventRepository = assertScope.ServiceProvider.GetRequiredService<IEventRepository>();

        var updatedEvent = await eventRepository.GetAsync(eventId);
        updatedEvent.AvailableSeats.Should().Be(0);
    }
}
