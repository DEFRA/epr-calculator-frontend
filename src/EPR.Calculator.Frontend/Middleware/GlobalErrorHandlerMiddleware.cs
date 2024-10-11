using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Middleware
{
    public class GlobalErrorHandlerMiddleware
    {
        private readonly RequestDelegate requestDelegate;
        private readonly ILogger<GlobalErrorHandlerMiddleware> logger;

        public GlobalErrorHandlerMiddleware(RequestDelegate requestDelegate, ILogger<GlobalErrorHandlerMiddleware> logger)
        {
            this.requestDelegate = requestDelegate;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this.requestDelegate(context);

                if (context.Session != null)
                {
                    await context.Session.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception Occurred: {Message}", ex.Message);
                var problemDetails = new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = ex.Message,
                    Instance = context.Request.Path,
                    Status = StatusCodes.Status500InternalServerError,
                };

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                // Commit the session state before writing the response
                if (context.Session != null)
                {
                    await context.Session.CommitAsync();
                }

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}