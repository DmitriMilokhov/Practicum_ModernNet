using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace EventManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(kv => kv.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value!.Errors.Select(e => e.ErrorMessage));

                var errorDetailsMsg = $"{string.Join(", ", errors.Select(kv => $"{kv.Key}: {string.Join(",", kv.Value)}"))}";
                var result = new ApiErrorResult 
                { 
                    Message = "Validation issues. See Error Details", 
                    ErrorDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Detail = errorDetailsMsg
                    }
                };

                return new BadRequestObjectResult(result);
            };
        });


        services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}
