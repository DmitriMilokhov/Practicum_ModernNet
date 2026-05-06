using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Infrastructure;
using EventManager.Infrastructure.Interfaces;

namespace EventManager.Features.Bookings;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingServices(this IServiceCollection services) 
    {
        services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
        services.AddSingleton<ITaskQueue<BookingDto>, InMemoryTaskQueue<BookingDto>>();
        services.AddSingleton<IBookingFactory, BookingFactory>();  
        services.AddHostedService<BookingBackgroundService>();

        services.AddScoped<IBookingService, BookingService>();

        return services;
    }
}
