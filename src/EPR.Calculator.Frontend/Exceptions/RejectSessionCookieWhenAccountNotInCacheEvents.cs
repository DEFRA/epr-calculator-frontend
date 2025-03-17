using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using NuGet.Common;

namespace EPR.Calculator.Frontend.Exceptions
{
    public class RejectSessionCookieWhenAccountNotInCacheEvents : CookieAuthenticationEvents
    {
        private readonly string[] downstreamScopes;

        private TelemetryClient telemetryClient;

        public RejectSessionCookieWhenAccountNotInCacheEvents(string[] downstreamScopes, TelemetryClient telemetryClient)
        {
            this.downstreamScopes = downstreamScopes;
            this.telemetryClient = telemetryClient;
        }

        public async override Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            try
            {
                this.telemetryClient.TrackTrace("Inside Validate Principal");
                var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                string token = await tokenAcquisition.GetAccessTokenForUserAsync(
                    scopes: this.downstreamScopes,
                    user: context.Principal);
                this.telemetryClient.TrackTrace($"token :{token}");
                if (token == null)
                {
                    context.RejectPrincipal();
                }
            }
            catch (MicrosoftIdentityWebChallengeUserException ex) when (AccountDoesNotExitInTokenCache(ex))
            {
                this.telemetryClient.TrackTrace($"token not exists :{ex.Message} && {ex.InnerException}");
                context.RejectPrincipal();
            }
        }

        /// <summary>
        /// Is the exception thrown because there is no account in the token cache?
        /// </summary>
        /// <param name="ex">Exception thrown by <see cref="ITokenAcquisition"/>.GetTokenForXX methods.</param>
        /// <returns>A boolean telling if the exception was about not having an account in the cache</returns>
        private static bool AccountDoesNotExitInTokenCache(MicrosoftIdentityWebChallengeUserException ex)
        {
            return ex.InnerException is MsalUiRequiredException && (ex.InnerException as MsalUiRequiredException).ErrorCode == "user_null";
        }
    }
}
