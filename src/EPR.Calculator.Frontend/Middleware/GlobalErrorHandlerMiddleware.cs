using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EPR.Calculator.Frontend.Middleware
{
    public class GlobalErrorHandlerMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly ILogger<GlobalErrorHandlerMiddleware> _logger;

        public GlobalErrorHandlerMiddleware(RequestDelegate requestDelegate, ILogger<GlobalErrorHandlerMiddleware> logger)
        {
            _requestDelegate = requestDelegate;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _requestDelegate(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var code = HttpStatusCode.InternalServerError;

            var result = JsonSerializer.Serialize(new { error = ex.Message, exception = ex.Source });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            //return Task.FromResult(result);
            return context.Response.WriteAsync(result);
        }
    }
}