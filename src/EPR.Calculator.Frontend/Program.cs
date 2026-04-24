using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.HealthCheck;
using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var environmentName = builder.Environment.EnvironmentName?.ToLower() ?? string.Empty;

// Suppress the default response header so the hosting server is not advertised.
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration, "AzureAd")
    .EnableTokenAcquisitionToCallDownstreamApi(builder.Configuration["DownstreamApi:Scopes"]?.Split(' ', StringSplitOptions.RemoveEmptyEntries))
    .AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
    .AddDistributedTokenCaches();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // Applied globally by UseCookiePolicy() so every outgoing cookie inherits these.
    options.Secure = CookieSecurePolicy.Always; // HTTPS only
    options.HttpOnly = HttpOnlyPolicy.Always; // Prevent JS access

    // The app and session cookies need SameSite=None for the Azure AD redirect flow,
    // so don't let the policy downgrade them to the framework default of Lax.
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Configure the cookie handler used by Microsoft Identity Web ("Cookies" scheme).
// Note: ConfigureApplicationCookie() targets the ASP.NET Identity scheme
// ("Identity.Application") which is not used here, so it would be a no-op.
builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.Cookie.Name = ".PayCal.Auth";
        options.Cookie.SameSite = SameSiteMode.None; // Required for Azure AD redirects
    });

// Rebrand the OIDC correlation/nonce cookies issued during the Azure AD redirect flow.
builder.Services.Configure<OpenIdConnectOptions>(
    OpenIdConnectDefaults.AuthenticationScheme,
    options =>
    {
        options.NonceCookie.Name = ".PayCal.OIDC.Nonce.";
        options.CorrelationCookie.Name = ".PayCal.OIDC.Correlation.";
    });

// Rebrand the AspNetCore antiforgery cookie issued on pages with form tags.
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".PayCal.Antiforgery";
});

builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().RequireRole(UserRoles.SASuperUser)
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<EPR.Calculator.Frontend.Exceptions.GlobalExceptionFilter>();

    options.Filters.Add(new ResponseCacheAttribute
    {
        NoStore  = true,
        Location = ResponseCacheLocation.None,
    });
}).AddMicrosoftIdentityUI();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining<CalculatorRunNameValidator>();

builder.Services.AddFeatureManagement();

builder.Services.AddHealthChecks();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("SessionTimeOut"));
    options.Cookie.SameSite = SameSiteMode.None; // Required for Azure AD redirects
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".PayCal.Session";
});

if (!string.Equals(environmentName, EPR.Calculator.Frontend.Constants.Environment.Local, StringComparison.InvariantCultureIgnoreCase))
{
    builder.Services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(builder.Configuration.GetSection("BlobStorage:ConnectionString").Value, SessionConstants.Paycal, SessionConstants.PaycalDataProtection)
    .SetApplicationName(SessionConstants.PaycalAppName);
}

builder.Services.AddHttpClient();

builder.Services.AddScoped<IBillingInstructionsMapper, BillingInstructionsMapper>();

builder.Services.AddScoped<IResultBillingFileService, ResultBillingFileService>();

builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);

// Register services.
builder.Services.AddTransient<ICalculatorRunDetailsService, CalculatorRunDetailsService>();
builder.Services.AddTransient<IEprCalculatorApiService, EprCalculatorApiService>();

// Add Hsts
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.Logging.AddConsole();

var app = builder.Build();

// HSTS and HTTPS redirection must run before any response-writing middleware.
if (!app.Environment.IsDevelopment() && !string.Equals(environmentName, EPR.Calculator.Frontend.Constants.Environment.Local, StringComparison.InvariantCultureIgnoreCase))
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src  'self' 'unsafe-inline'; " +
        "img-src    'self' data:; " +
        "font-src   'self'; " +
        "connect-src 'self'; " +
        "frame-src  'self'; " +
        "frame-ancestors 'self'; " +
        "form-action 'self' https://login.microsoftonline.com; " + // allow AAD sign-in POST
        "base-uri   'self'; " +
        "object-src 'none'";

    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    await next();
});

app.UseStaticFiles();

app.UseRouting();

// Must run before any middleware that writes cookies (UseSession, UseAuthentication,
// and the MVC endpoints that issue the antiforgery cookie).
app.UseCookiePolicy();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapHealthChecks("/admin/health", HealthCheckOptionsBuilder.Build()).AllowAnonymous();

app.Run();
