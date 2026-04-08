using EventManager.Infrastructure;
using EventManager.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
                logger.LogError(
                    ex,
                    "Exception occured! Method = {Method}, Path = {Path}, RequestId = {RequestId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.Headers["x-request-id"]);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            var apiErrorResponse = new ApiErrorResult
            {
                Message = "Exception occured. See Error Details",
                ErrorDeatails = ex switch
                {
                    NotFoundException nf => new ProblemDetails
                    {
                        Detail = nf.Message,
                        Status = StatusCodes.Status404NotFound,                       
                    },

                    ValidationException ve => new ProblemDetails
                    {
                        Detail = ve.Message,
                        Status = StatusCodes.Status400BadRequest,
                    },
                    
                    _ => new ProblemDetails
                    {
                        Detail = "Internal server error",
                        Status = StatusCodes.Status500InternalServerError,
                    }
                }
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)apiErrorResponse.ErrorDeatails.Status!;

            await context.Response.WriteAsJsonAsync(apiErrorResponse);
        }
    }
}
