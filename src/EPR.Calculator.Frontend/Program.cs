using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Exceptions;
using EPR.Calculator.Frontend.HealthCheck;
using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var environmentName = builder.Environment.EnvironmentName?.ToLower() ?? string.Empty;

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd")
    .EnableTokenAcquisitionToCallDownstreamApi(builder.Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' '))
    .AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
    .AddInMemoryTokenCaches();

builder.Services.Configure<CookieAuthenticationOptions>(
    "Cookies",
    options =>
    {
        var timeout = builder.Configuration.GetValue<int>("SessionTimeOut");

        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;

        options.ExpireTimeSpan = TimeSpan.FromMinutes(timeout);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.Redirect("/MicrosoftIdentity/Account/SignIn");
            return Task.CompletedTask;
        };
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("SessionTimeOut"));
    options.Cookie.Name = builder.Configuration.GetValue<string>("SessionCookieName");
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // Ensures that SameSite=None is respected
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().RequireRole(UserRoles.SASuperUser)
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining<CalculatorRunNameValidator>();

builder.Services.AddFeatureManagement();

builder.Services.AddHealthChecks();


if (!string.Equals(environmentName, EPR.Calculator.Frontend.Constants.Environment.Local, StringComparison.InvariantCultureIgnoreCase))
{
    builder.Services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(builder.Configuration.GetSection("BlobStorage:ConnectionString").Value, SessionConstants.Paycal, SessionConstants.PaycalDataProtection)
    .SetApplicationName(SessionConstants.PaycalAppName);
}

builder.Services.AddHttpClient();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddScoped<IBillingInstructionsMapper, BillingInstructionsMapper>();

builder.Services.AddScoped<IBillingInstructionsApiService, BillingInstructionsApiService>();

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

var app = builder.Build();

// add csp headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "font-src 'self';frame-src 'self'; img-src 'self';frame-ancestors 'self';");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    await next();
});

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/StandardError/Index");

if (!app.Environment.IsDevelopment() && !string.Equals(environmentName, EPR.Calculator.Frontend.Constants.Environment.Local, StringComparison.InvariantCultureIgnoreCase))
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapHealthChecks("/admin/health", HealthCheckOptionsBuilder.Build()).AllowAnonymous();

app.Run();