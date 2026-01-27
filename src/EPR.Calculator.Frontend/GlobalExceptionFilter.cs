using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Exceptions
{
    public class GlobalExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            this.logger = logger;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.Exception is MicrosoftIdentityWebChallengeUserException)
            {
                // Existing behavior: trigger login challenge
                context.ExceptionHandled = true;
                await context.HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
                return;
            }

            // Log the exception
            this.logger.LogError(context.Exception, "Unexpected error");

            // Redirect to the standard error page
            context.ExceptionHandled = true;
            context.Result = new RedirectToActionResult("Index", "StandardError", null);
        }
    }
}
