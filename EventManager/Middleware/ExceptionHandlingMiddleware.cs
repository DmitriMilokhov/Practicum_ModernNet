using EventManager.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EventManager.Middleware
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occured!");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var response = ex switch
            {
                NotFoundException nf => new
                {
                    status = StatusCodes.Status404NotFound,
                    body = new ApiResult
                    {
                        Message = nf.Message
                    }
                },

                ValidationException ve => new
                {
                    status = StatusCodes.Status400BadRequest,
                    body = new ApiResult
                    {
                        Message = ve.Message
                    }
                },

                _ => new
                {
                    status = StatusCodes.Status500InternalServerError,
                    body = new ApiResult
                    {
                        Message = "Internal server error"
                    }
                }
            };

            context.Response.StatusCode = response.status;
            var json = JsonSerializer.Serialize(response.body);

            await context.Response.WriteAsync(json);
        }
    }
}
