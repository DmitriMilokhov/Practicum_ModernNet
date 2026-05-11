using EventManager.Features.Bookings;
using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Interfaces;
using Moq;

namespace EventManagerTests.BookingServiceTests;

public abstract class BookingServiceTestsBase
{
    protected readonly Mock<IBookingRepository> BookingRepositoryMock = new();
    protected readonly Mock<IEventRepository> EventRepositoryMock = new();
    protected readonly Mock<IBookingFactory> BookingFactoryMock = new();

    protected readonly BookingFactory BookingFactory = new();
    protected readonly BookingService BookingService;
    protected readonly IEventBookingLockProvider EventBookingLockProvider = new EventBookingLockProvider();

    protected BookingServiceTestsBase()
    {
        BookingService = new BookingService(
            BookingFactoryMock.Object,
            BookingRepositoryMock.Object,
            EventRepositoryMock.Object,
            EventBookingLockProvider);
    }
}
