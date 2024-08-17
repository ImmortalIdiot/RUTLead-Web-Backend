using api.Exceptions;

namespace api.Middleware
{
    public class GlobalErrorHandling : IMiddleware
    {
        private readonly ILogger<GlobalErrorHandling> _logger;

        public GlobalErrorHandling(ILogger<GlobalErrorHandling> logger) => _logger = logger;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                _logger.LogInformation($"Request URL: {Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(context.Request)}");
                await next(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An unhandled exception occurred.");
                context.Response.StatusCode = (int)ExceptionCodesDictionary.GetExceptionStatusCode(e);
                await context.Response.WriteAsync(e.Message);
            }
        }
    }
}
