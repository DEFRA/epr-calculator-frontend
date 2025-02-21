﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Exceptions
{
    public class RejectSessionCookieWhenAccountNotInCacheEvents : CookieAuthenticationEvents
    {
        private readonly string[] downstreamScopes;

        public RejectSessionCookieWhenAccountNotInCacheEvents(string[] downstreamScopes)
        {
            this.downstreamScopes = downstreamScopes;
        }

        public async override Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            try
            {
                var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                string token = await tokenAcquisition.GetAccessTokenForUserAsync(
                    scopes: this.downstreamScopes,
                    user: context.Principal);
                if (token == null) { context.RejectPrincipal(); }
            }
            catch (MicrosoftIdentityWebChallengeUserException ex) when (AccountDoesNotExitInTokenCache(ex))
            {
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
