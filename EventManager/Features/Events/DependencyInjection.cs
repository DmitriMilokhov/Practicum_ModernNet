using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;

namespace EventManager.Features.Events;

public static class DependencyInjection
{
    public static IServiceCollection AddEventServices(this IServiceCollection services) 
    {
        services.AddTransient<IEventFilterValidator, EventFilterValidator>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventService, EventService>();

        return services;
    }
}
