using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Exceptions;
using EPR.Calculator.Frontend.HealthCheck;
using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var environmentName = builder.Environment.EnvironmentName?.ToLower() ?? string.Empty;

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration, "AzureAd")
    .EnableTokenAcquisitionToCallDownstreamApi(builder.Configuration["DownstreamApi:Scopes"]?.Split(' ', StringSplitOptions.RemoveEmptyEntries))
    .AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
    .AddDistributedTokenCaches();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // Ensures that SameSite=None is respected
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;             // Required for Azure AD redirects
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
    options.Cookie.HttpOnly = true;                          // Prevent JS access
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
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = builder.Configuration.GetValue<string>("SessionCookieName");
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
builder.Services.AddTransient<IApiService, ApiService>();

// Add Hsts
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.Logging.AddConsole();

var app = builder.Build();

// add csp headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "font-src 'self';frame-src 'self'; img-src 'self';frame-ancestors 'self';");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    await next();
});

if (!app.Environment.IsDevelopment() && !string.Equals(environmentName, EPR.Calculator.Frontend.Constants.Environment.Local, StringComparison.InvariantCultureIgnoreCase))
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapHealthChecks("/admin/health", HealthCheckOptionsBuilder.Build()).AllowAnonymous();

app.Run();