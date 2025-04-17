using System.Configuration;
using System.Text;
using EPR.Calculator.Frontend.Common;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using System.Text.Json;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseController"/> class.
    /// </summary>
    /// <param name="clientFactory">an HTTP client factory that will be used for making connections to the calculator API.</param>
    public class BaseController(
        IConfiguration configuration,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        IHttpClientFactory clientFactory) : Controller
    {
        private readonly ITokenAcquisition tokenAcquisition = tokenAcquisition;

        protected TelemetryClient TelemetryClient { get; init; } = telemetryClient;

        /// <summary>Gets the configuration object to retrieve API URL and parameters.</summary>
        protected IConfiguration Configuration { get; init; } = configuration;

        /// <summary>
        /// Gets an HTTP client factory that will be used for making connections to the calculator API.
        /// </summary>
        private IHttpClientFactory ClientFactory { get; init; } = clientFactory;

        protected async Task<string> AcquireToken()
        {
            this.TelemetryClient.TrackTrace("AcquireToken");
            var token = this.HttpContext?.Session?.GetString("accessToken");
            if (string.IsNullOrEmpty(token))
            {
                try
                {
                    var scope = this.Configuration.GetSection("DownstreamApi:Scopes").Value!;
                    this.TelemetryClient.TrackTrace($"GetAccessTokenForUserAsync with scope- {scope}");
                    token = await this.tokenAcquisition.GetAccessTokenForUserAsync([scope]);
                }
                catch (Exception ex)
                {
                    this.TelemetryClient.TrackException(ex);
                    throw;
                }

                this.TelemetryClient.TrackTrace("after generating..");
                this.HttpContext?.Session?.SetString("accessToken", token);
            }

            var accessToken = $"Bearer {token}";
            this.TelemetryClient.TrackTrace($"accessToken is {accessToken}", SeverityLevel.Information);
            this.TelemetryClient.TrackTrace($"accessToken length {accessToken.Length}", SeverityLevel.Information);
            return accessToken;
        }

        /// <summary>
        /// Returns the financial year from session if feature enabled, else from config.
        /// </summary>
        /// <param name="configSection">The configuration section.</param>
        /// <returns>Returns the financial year.</returns>
        /// <exception cref="ArgumentNullException">Returns error if financial year is null or empty.</exception>
        protected string GetFinancialYear(string configSection)
        {
            var parameterYear = this.Configuration.IsFeatureEnabled(FeatureFlags.ShowFinancialYear)
                ? this.HttpContext.Session.GetString(SessionConstants.FinancialYear)
                : this.Configuration.GetSection(configSection).GetValue<string>("ParameterYear");

            if (string.IsNullOrWhiteSpace(parameterYear))
            {
                throw new ArgumentNullException(parameterYear, "ParameterYear is null. Check the configuration settings.");
            }

            return parameterYear;
        }

        protected async Task<HttpResponseMessage> CallApi(
            HttpMethod httpMethod,
            Uri apiUrl,
            string argument,
            object? body)
        {
            var argsString = !string.IsNullOrEmpty(argument)
                ? $"/{argument}"
                : string.Empty;
            var contentString = JsonSerializer.Serialize(body);
            var request = new HttpRequestMessage(
                httpMethod,
                new Uri($"{apiUrl}{argsString}"));
            if (body is not null)
            {
                request.Content = new StringContent(
                    contentString,
                    Encoding.UTF8,
                    StaticHelpers.MediaType);
            }

            var client = await this.GetHttpClient();
            return await client.SendAsync(request);
        }

        /// <summary>
        /// Retrieves the API URL from the specified configuration section and key.
        /// </summary>
        protected Uri GetApiUrl(string configSection, string configKey)
            => new Uri(this.GetConfigSetting(configSection, configKey));

        private async Task<HttpClient> GetHttpClient()
        {
            var client = this.ClientFactory.CreateClient();
            var accessToken = await this.AcquireToken();
            client.DefaultRequestHeaders.Add("Authorization", accessToken);
            return client;
        }

        /// <summary>
        /// Retrieves a configuration setting from the specified section and key.
        /// </summary>
        private string GetConfigSetting(string configSection, string configKey)
        {
            var value = this.Configuration
                            .GetSection(configSection)
                            .GetValue<string>(configKey);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException(
                    $"{configSection}:{configKey} is null or empty. Please check the configuration settings. " +
                    $"{ConfigSection.CalculationRunSettings}");
            }

            return value;
        }
    }
}