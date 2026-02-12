using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.Configuration;
using System.Text;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Services
{
    public interface IEprCalculatorApiService
    {
        Task<HttpResponseMessage> CallApi(
            HttpContext httpContext,
            HttpMethod httpMethod,
            string relativePath,
            IDictionary<string, string?>? queryParams = null,
            object? body = null);
    }

    public class EprCalculatorApiService(
        IConfiguration configuration,
        TelemetryClient telemetryClient,
        IHttpClientFactory clientFactory,
        ITokenAcquisition tokenAcquisition) : IEprCalculatorApiService
    {
        protected IConfiguration Configuration { get; init; } = configuration;

        protected TelemetryClient TelemetryClient { get; init; } = telemetryClient;

        private IHttpClientFactory ClientFactory { get; init; } = clientFactory;

        private ITokenAcquisition TokenAcquisition { get; init; } = tokenAcquisition;

        private readonly Uri _baseUri =
            new(configuration.GetRequiredSection("EprCalculatorApiService")
                .GetValue<string>("BaseUrl")!);

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> CallApi(
            HttpContext httpContext,
            HttpMethod httpMethod,
            string relativePath,
            IDictionary<string, string?>? queryParams = null,
            object? body = null)
        {
            var uri = queryParams is { Count: > 0 }
                ? new Uri(QueryHelpers.AddQueryString(new Uri(this._baseUri, relativePath).ToString(), queryParams))
                : new Uri(this._baseUri, relativePath);

            var request = new HttpRequestMessage(httpMethod, uri);

            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }

            var client = await this.GetHttpClient(httpContext);
            return await client.SendAsync(request);
        }

        protected async Task<string> AcquireToken(HttpContext httpContext)
        {
            this.TelemetryClient.TrackTrace("AcquireToken");

            var scopesValue = this.Configuration["DownstreamApi:Scopes"];

            if (string.IsNullOrWhiteSpace(scopesValue))
            {
                throw new ConfigurationErrorsException(
                    "DownstreamApi:Scopes is null or empty. Please check the configuration settings.");
            }

            var scopes = scopesValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            try
            {
                this.TelemetryClient.TrackTrace($"GetAccessTokenForUserAsync with scopes: {string.Join(",", scopes)}");

                var token = await this.TokenAcquisition.GetAccessTokenForUserAsync(scopes);

                return $"Bearer {token}";
            }
            catch (MsalUiRequiredException ex)
            {
                throw new MicrosoftIdentityWebChallengeUserException(ex, scopes);
            }
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