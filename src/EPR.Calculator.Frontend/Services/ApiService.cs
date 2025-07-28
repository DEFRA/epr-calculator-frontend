using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Identity.Web;
using System.Configuration;
using System.Text;
using System.Text.Json;

namespace EPR.Calculator.Frontend.Services
{
    public class ApiService(
        IConfiguration configuration,
        TelemetryClient telemetryClient,
        IHttpClientFactory clientFactory,
        ITokenAcquisition tokenAcquisition) : IApiService
    {
        protected IConfiguration Configuration { get; init; } = configuration;

        protected TelemetryClient TelemetryClient { get; init; } = telemetryClient;

        /// <summary>
        /// Gets an HTTP client factory that will be used for making connections to the calculator API.
        /// </summary>
        private IHttpClientFactory ClientFactory { get; init; } = clientFactory;

        private ITokenAcquisition TokenAcquisition { get; init; } = tokenAcquisition;

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> CallApi(
            HttpContext httpContext,
            HttpMethod httpMethod,
            Uri apiUrl,
            string argument,
            object? body)
        {
            var argsString = !string.IsNullOrEmpty(argument) ? $"/{argument}" : string.Empty;
            argsString = !argument.Contains('&') ? argsString : $"?{argument}";
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

            var client = await this.GetHttpClient(httpContext);
            return await client.SendAsync(request);
        }

        /// <inheritdoc/>
        public Uri GetApiUrl(string configSection, string configKey)
            => new Uri(this.GetConfigSetting(configSection, configKey));

        /// <inheritdoc/>
        public Uri GetApiUrl(string configKey)
            => new Uri(this.GetConfigSetting(configKey));

        protected async Task<string> AcquireToken(HttpContext httpContext)
        {
            this.TelemetryClient.TrackTrace("AcquireToken");
            var token = httpContext.Session.GetString("accessToken");
            if (string.IsNullOrEmpty(token))
            {
                try
                {
                    var scope = this.Configuration.GetSection("DownstreamApi:Scopes").Value!;
                    this.TelemetryClient.TrackTrace($"GetAccessTokenForUserAsync with scope- {scope}");
                    token = await this.TokenAcquisition.GetAccessTokenForUserAsync([scope]);
                }
                catch (Exception ex)
                {
                    this.TelemetryClient.TrackException(ex);
                    throw;
                }

                this.TelemetryClient.TrackTrace("after generating..");
                httpContext.Session.SetString("accessToken", token);
            }

            var accessToken = $"Bearer {token}";
            this.TelemetryClient.TrackTrace($"accessToken is {accessToken}", SeverityLevel.Information);
            this.TelemetryClient.TrackTrace($"accessToken length {accessToken.Length}", SeverityLevel.Information);
            return accessToken;
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

        /// <summary>
        /// Retrieves a configuration setting from the specified key.
        /// </summary>
        private string GetConfigSetting(string configKey)
        {
            var value = this.Configuration.GetValue<string>(configKey);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException(
                    $"{configKey} is null or empty. Please check the configuration settings. " +
                    $"{ConfigSection.CalculationRunSettings}");
            }

            return value;
        }

        private async Task<HttpClient> GetHttpClient(HttpContext httpContext)
        {
            var client = this.ClientFactory.CreateClient();
            var accessToken = await this.AcquireToken(httpContext);
            if (client.DefaultRequestHeaders is not null && !client.DefaultRequestHeaders.Contains("Authorization"))
            {
                client.DefaultRequestHeaders.Add("Authorization", accessToken);
            }

            return client;
        }
    }
}