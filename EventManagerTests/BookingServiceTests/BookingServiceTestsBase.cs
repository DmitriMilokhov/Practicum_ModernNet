using EventManager.DataAccess;
using EventManager.Features.Bookings;
using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events;
using EventManager.Features.Events.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagerTests.BookingServiceTests;

public abstract class BookingServiceTestsBase : IDisposable
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly BookingFactory BookingFactory = new();
    protected readonly IEventBookingLockProvider EventBookingLockProvider = new EventBookingLockProvider();

    protected BookingServiceTestsBase()
    {
        var services = new ServiceCollection();

        var dbName = Guid.NewGuid().ToString();

        services.AddDbContext<AppDbContext>(options =>options.UseInMemoryDatabase(dbName));

        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingFactory, BookingFactory>();
        services.AddScoped<IBookingService, BookingService>();

        services.AddSingleton<IEventBookingLockProvider>(EventBookingLockProvider);

        ServiceProvider = services.BuildServiceProvider();
    }

    protected IServiceScope CreateScope()
    {
        return ServiceProvider.CreateScope();
    }

    public void Dispose()
    {
        ServiceProvider.Dispose();
    }
}
