using System.Text.Json;
using api.Exceptions;

namespace api.Middleware
{
    public class ValidationErrorHandling : IMiddleware
    {
        private readonly ILogger<ValidationErrorHandling> _logger;

        public ValidationErrorHandling(ILogger<ValidationErrorHandling> logger) => _logger = logger;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                _logger.LogInformation($"Request URL: {Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(context.Request)}");
                await next(context);
            }
            catch (Exception e) when (e is InvalidUserDataException || e is UserNotFoundException)
            {
                _logger.LogError(e, "An exception was thrown");

                context.Response.StatusCode = (int)ExceptionCodesDictionary.GetExceptionStatusCode(e);
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    error = e.Message
                });

                await context.Response.WriteAsync(result);
            }
        }
    }
}
