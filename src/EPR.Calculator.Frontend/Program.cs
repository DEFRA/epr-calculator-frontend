using System.Reflection;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Exceptions;
using EPR.Calculator.Frontend.HealthCheck;
using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);
var environmentName = builder.Environment.EnvironmentName?.ToLower() ?? string.Empty;

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd")
    .EnableTokenAcquisitionToCallDownstreamApi(builder.Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' '))
    .AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
    .AddInMemoryTokenCaches();

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

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("SessionTimeOut"));
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = builder.Configuration.GetValue<string>("SessionCookieName");
});

if (environmentName != EPR.Calculator.Frontend.Constants.Environment.Local.ToLower())
{
    builder.Services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(builder.Configuration.GetSection("BlobStorage:ConnectionString").Value, SessionConstants.Paycal, SessionConstants.PaycalDataProtection)
    .SetApplicationName(SessionConstants.PaycalAppName);
}

builder.Services.AddHttpClient();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddScoped<IBillingInstructionsMapper, BillingInstructionsMapper>();

builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);

// Register services.
builder.Services.AddTransient<ICalculatorRunDetailsService, CalculatorRunDetailsService>();
builder.Services.AddTransient<IApiService, ApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/StandardError/Index");

if (!app.Environment.IsDevelopment() && environmentName != EPR.Calculator.Frontend.Constants.Environment.Local.ToLower())
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