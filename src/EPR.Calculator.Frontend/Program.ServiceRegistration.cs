using EPR.Calculator.Frontend.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace EPR.Calculator.Frontend;

public static class ServiceRegistration
{
    public static IServiceCollection AddPayCalAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var entraIdConfig = configuration.GetSection("AzureAd");
        var downstreamApiConfig = configuration.GetSection("DownstreamApi");
        var apiScopes = downstreamApiConfig["Scopes"]?
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

        // Entra ID configuration.
        // EnableTokenAcquisitionToCallDownstreamApi wires up MSAL token caching
        // so EprCalculatorApiService can acquire bearer tokens for the API.
        services
            .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(entraIdConfig)
            .EnableTokenAcquisitionToCallDownstreamApi(apiScopes)
            .AddDownstreamApi("DownstreamApi", downstreamApiConfig)
            .AddDistributedTokenCaches();

        // Rebrands the OIDC correlation/nonce cookies issued during the auth redirect flow.
        services.Configure<OpenIdConnectOptions>(
            OpenIdConnectDefaults.AuthenticationScheme,
            options =>
            {
                options.NonceCookie.Name = ".PayCal.OIDC.Nonce.";
                options.CorrelationCookie.Name = ".PayCal.OIDC.Correlation.";
            });

        return services;
    }
}
