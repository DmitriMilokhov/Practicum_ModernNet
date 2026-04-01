using EventManager.Interfaces;

namespace EventManager.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddAppServices(this IServiceCollection services) 
    {
        services.AddSingleton<IEventService, EventService>();

        return services;
    }
}
