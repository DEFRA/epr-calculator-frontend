using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Frontend.Exceptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
    /// </summary>
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        /// <inheritdoc/>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "An unhandled exception occurred.");
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new
            {
                Status = httpContext.Response.StatusCode,
                Title = "An error occurred while processing your request.",
                exception.Message,
                Instance = httpContext.Request.Path,
                Detail = env.IsDevelopment() || string.Equals(env.EnvironmentName, Constants.Environment.Local, StringComparison.InvariantCultureIgnoreCase) ? exception.StackTrace : null,
            };

            var errorJson = JsonSerializer.Serialize(errorResponse, Options);
            await httpContext.Response.WriteAsync(errorJson, cancellationToken: cancellationToken);
            return true;
        }
    }
}