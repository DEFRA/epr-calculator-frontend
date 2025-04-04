using System.Configuration;
using System.Text;
using System.Text.Json;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
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

        private async Task<HttpClient> GetHttpClient()
        {
            var client = this.ClientFactory.CreateClient();
            var accessToken = await this.AcquireToken();
            client.DefaultRequestHeaders.Add("Authorization", accessToken);
            return client;
        }

        private async Task<HttpResponseMessage> CallApi(
            HttpMethod httpMethod,
            Uri apiUrl,
            object? body)
            => await this.CallApi(httpMethod, apiUrl, string.Empty, body);

        private async Task<HttpResponseMessage> CallApi(
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

        protected async Task<HttpResponseMessage> GetCalculatorRunAsync(int runId)
        {
            var apiUrl = this.GetApiUrl(
                ConfigSection.DashboardCalculatorRun,
                ConfigSection.DashboardCalculatorRunApi);
            return await this.CallApi(
                HttpMethod.Get,
                apiUrl,
                runId.ToString(),
                null);
        }

        protected async Task<HttpResponseMessage> CheckCalcNameExistsAsync(string calculationName)
        {
            var apiUrl = this.GetApiUrl(
                ConfigSection.CalculationRunSettings,
                ConfigSection.CalculationRunNameApi);
            return await this.CallApi(HttpMethod.Get, apiUrl, calculationName);
        }

        protected async Task<HttpResponseMessage> PutCalculatorRun(int runId, RunClassification classification)
        {
            var apiUrl = this.GetApiUrl(
                ConfigSection.DashboardCalculatorRun,
                ConfigSection.DashboardCalculatorRunApi);
            var args = (runId, (int)classification);
            return await this.CallApi(HttpMethod.Put, apiUrl, args);
        }

        protected async Task<HttpResponseMessage> PostCalculatorRuns()
        {
            var apiUrl = this.GetApiUrl(
                ConfigSection.DashboardCalculatorRun,
                ConfigSection.DashboardCalculatorRunApi);
            var financialYear = this.GetConfigSetting(
                ConfigSection.DashboardCalculatorRun,
                ConfigSection.RunParameterYear);
            if (string.IsNullOrEmpty(financialYear))
            {
                throw new ArgumentNullException(
                    financialYear,
                    "RunParameterYear is null or empty. Check the configuration settings for calculatorRun.");
            }

            return await this.CallApi(HttpMethod.Post, apiUrl, (CalculatorRunParamsDto)financialYear);
        }

        protected async Task<HttpResponseMessage> PostLapcapData(CreateDefaultParameterSettingDto dto)
        {
            var apiUrl = this.GetApiUrl(
                ConfigSection.ParameterSettings,
                ConfigSection.DefaultParameterSettingsApi);
            return await this.CallApi(HttpMethod.Post, apiUrl, dto);
        }

        protected async Task<HttpResponseMessage> PostAsync(Uri apiUrl, object dto)
            => await this.CallApi(HttpMethod.Post, apiUrl, dto);

        private string GetConfigSetting(string configSection, string configKey)
        {
            var value = this.Configuration
                            .GetSection(configSection)
                            .GetValue<string>(configKey);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException(
                    $"{configSection}:{configKey} is null or empty. Please check the configuration settings. " +
                    $"${ConfigSection.CalculationRunSettings}");
            }

            return value;
        }

        private Uri GetApiUrl(string configSection, string configKey)
            => new Uri(this.GetConfigSetting(configSection, configKey));
    }
}