using api.Exceptions;
using System.Text.Json;

namespace api.Middleware
{
    public class GlobalErrorHandling(ILogger<GlobalErrorHandling> logger) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                logger.LogInformation("Request URL: {url}", Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(context.Request));
                await next(context);
            }
            catch (Exception e) when (e is InvalidUserDataException or UserNotFoundException or UserAlreadyExistsException)
            {
                logger.LogError(e, "A validation exception was thrown");

                context.Response.StatusCode = (int)ExceptionCodesDictionary.GetExceptionStatusCode(e);
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    error = e.Message
                });

                await context.Response.WriteAsync(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, "An unhandled exception occurred.");
                context.Response.StatusCode = (int)ExceptionCodesDictionary.GetExceptionStatusCode(e);
                await context.Response.WriteAsync(e.Message);
            }
        }
    }
}
