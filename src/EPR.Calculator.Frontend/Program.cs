using System.Reflection;
using EPR.Calculator.Frontend;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Exceptions;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.HealthCheck;
using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);

// Suppress the default response header so the hosting server is not advertised.
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services.AddPayCalAuthentication(builder.Configuration);

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // Applied globally by UseCookiePolicy() so every outgoing cookie inherits these.
    options.Secure = CookieSecurePolicy.Always; // Forces HTTPS only
    options.HttpOnly = HttpOnlyPolicy.Always; // Prevents JS access
    options.MinimumSameSitePolicy = SameSiteMode.None; // Required for auth flow redirects
});

// Configure the cookie handler used by Microsoft Identity Web ("Cookies" scheme).
// Note: ConfigureApplicationCookie() targets the ASP.NET Identity scheme
// ("Identity.Application") which is not used here, so it would be a no-op.
builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.Cookie.Name = ".PayCal.Auth";
        options.Cookie.SameSite = SameSiteMode.None; // Required for auth flow redirects
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("SessionTimeOut"));
    options.Cookie.Name = ".PayCal.Session";
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None; // Required for auth flow redirects
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".PayCal.Antiforgery";
});

builder.Services
    .AddRazorPages()
    .AddMvcOptions(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser().RequireRole(UserRoles.SASuperUser)
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    })
    .AddMicrosoftIdentityUI();

builder.Services
    .AddControllersWithViews(options =>
    {
        options.Filters.Add<GlobalExceptionFilter>();

        options.Filters.Add(new ResponseCacheAttribute
        {
            NoStore  = true,
            Location = ResponseCacheLocation.None
        });
    })
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Add("/{0}.cshtml");
    })
    .AddMicrosoftIdentityUI();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CalculatorRunNameValidator>();
builder.Services.AddFeatureManagement();
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks();

if (!builder.Environment.IsLocal())
{
    builder.Services.AddDataProtection()
        .PersistKeysToAzureBlobStorage(builder.Configuration.GetSection("BlobStorage:ConnectionString").Value, SessionConstants.Paycal, SessionConstants.PaycalDataProtection)
        .SetApplicationName(SessionConstants.PaycalAppName);
}

builder.Services.AddScoped<IBillingInstructionsMapper, BillingInstructionsMapper>();
builder.Services.AddScoped<IFileDownloadService, FileDownloadService>();
builder.Services.AddScoped<IEprCalculatorApiService, EprCalculatorApiService>();

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
if (!app.Environment.IsDevelopment() && !app.Environment.IsLocal())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy(); // Must run before any middleware that writes cookies
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Dashboard}/{action=Index}/{id?}");
app.MapHealthChecks("/admin/health", HealthCheckOptionsBuilder.Build()).AllowAnonymous();

app.Run();
