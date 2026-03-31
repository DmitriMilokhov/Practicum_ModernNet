using EventManager.Interfaces;
using EventManager.Services;

namespace EventManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen();

        return services;
    }
}
