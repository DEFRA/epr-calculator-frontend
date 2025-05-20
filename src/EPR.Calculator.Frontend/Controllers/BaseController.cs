using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using EPR.Calculator.Frontend.Common;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
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
        /// <returns>Returns the financial year.</returns>
        /// <exception cref="ArgumentNullException">Returns error if financial year is null or empty.</exception>
        protected string GetFinancialYear()
        {
            var parameterYear = this.HttpContext.Session.GetString(SessionConstants.FinancialYear);

            if (string.IsNullOrWhiteSpace(parameterYear))
            {
                parameterYear = CommonUtil.GetFinancialYear(DateTime.Now);
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
            argsString = !argument.Contains("&") ? argsString : $"?{argument}";
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

        protected async Task<CalculatorRunDetailsViewModel?> GetCalculatorRundetails(int runId)
        {
            var runDetails = new CalculatorRunDetailsViewModel();
            var apiUrl = this.GetApiUrl(
                    ConfigSection.DashboardCalculatorRun,
                    ConfigSection.DashboardCalculatorRunApi);

            var response = await this.CallApi(HttpMethod.Get, apiUrl, runId.ToString(), null);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                 runDetails = response.Content.ReadFromJsonAsync<CalculatorRunDetailsViewModel>().Result;
                 return runDetails;
            }

            return runDetails;
        }

        /// <summary>
        /// Retrieves the calculation run with billing details.
        /// </summary>
        /// <param name="runId">run id.</param>
        /// <returns>calculator run post billing file data transfer objet.</returns>
        protected async Task<CalculatorRunPostBillingFileDto?> GetCalculatorRunWithBillingdetails(int runId)
        {
            var runDetails = new CalculatorRunPostBillingFileDto();
            var apiUrl = this.GetApiUrl(
                    ConfigSection.CalculationRunSettings,
                    ConfigSection.CalculationRunApiV2);

            var response = await this.CallApi(HttpMethod.Get, apiUrl, runId.ToString(), null);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                runDetails = response.Content.ReadFromJsonAsync<CalculatorRunPostBillingFileDto>().Result;
                return runDetails;
            }

            return runDetails;
        }

        private async Task<HttpClient> GetHttpClient()
        {
            var client = this.ClientFactory.CreateClient();
            var accessToken = await this.AcquireToken();
            if (client.DefaultRequestHeaders is not null && !client.DefaultRequestHeaders.Contains("Authorization"))
            {
                client.DefaultRequestHeaders.Add("Authorization", accessToken);
            }

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