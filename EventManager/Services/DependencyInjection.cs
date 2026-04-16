using EventManager.Interfaces;
using EventManager.Interfaces.IFilters;
using EventManager.Models.Filters;

namespace EventManager.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddAppServices(this IServiceCollection services) 
    {
        services.AddTransient<IEventFilterValidator, EventFilterValidator>();
        services.AddScoped<IEventService, EventService>();

        return services;
    }
}
