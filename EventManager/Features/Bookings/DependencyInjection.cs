using EventManager.Features.Bookings.Interfaces;

namespace EventManager.Features.Bookings;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingServices(this IServiceCollection services) 
    {
        services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();

        //services.AddScoped<IEventService, EventService>();

        return services;
    }
}
