using EventManager.Interfaces.IRepositories;

namespace EventManager.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddAppRepositories(this IServiceCollection services) 
    {
        services.AddSingleton<IEventRepository, InMemoryEventRepository>();

        return services;
    }
}
