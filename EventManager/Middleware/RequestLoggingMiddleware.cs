namespace FirstCoreApp.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        logger.LogInformation($"Request path: {context.Request.Path} ({context.Request.Method})");

        await next(context);

        logger.LogInformation($"Request finished. Status Code:{context.Response.StatusCode}");
    }
}
