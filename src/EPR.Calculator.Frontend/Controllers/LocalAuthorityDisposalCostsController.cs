using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalAuthorityDisposalCostsController"/> class.
    /// </summary>
    public class LocalAuthorityDisposalCostsController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAuthorityDisposalCostsController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        public LocalAuthorityDisposalCostsController(IConfiguration configuration, IHttpClientFactory clientFactory, TelemetryClient telemetryClient)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            this._telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Sends an HTTP POST request to the Local Authority Disposal Costs API with the specified parameters.
        /// </summary>
        /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the API URL is null or empty.</exception>
        public static async Task<HttpResponseMessage> GetHttpRequest(IConfiguration configuration, IHttpClientFactory clientFactory, TelemetryClient telemetryClient)
        {
            var lapcapRunApi = configuration.GetSection(ConfigSection.LapcapSettings)
                                                  .GetSection(ConfigSection.LapcapSettingsApi)
                                                  .Value;

            if (string.IsNullOrEmpty(lapcapRunApi))
            {
                telemetryClient.TrackException(new ArgumentNullException("LapcapRunApi", "API URL is missing."));
                throw new ArgumentNullException(lapcapRunApi, ApiUrl.Url);
            }

            telemetryClient.TrackTrace("Creating HTTP client for LapcapRun API.");
            var client = clientFactory.CreateClient();
            client.BaseAddress = new Uri(lapcapRunApi);
            var year = configuration.GetSection(ConfigSection.LapcapSettings).GetSection(ConfigSection.ParameterYear).Value;
            var uri = new Uri(string.Format("{0}/{1}", lapcapRunApi, year));
            telemetryClient.TrackEvent("HttpRequestInitiated", new Dictionary<string, string>
            {
                { "ApiBaseAddress", lapcapRunApi },
                { "YearParameter", year },
            });

            var response = await client.GetAsync(uri);

            telemetryClient.TrackEvent("HttpRequestCompleted", new Dictionary<string, string>
            {
                 { "StatusCode", response.StatusCode.ToString() },
            });
            return response;
        }

        /// <summary>
        /// Handles the Index action asynchronously. Sends an HTTP request and processes the response to display local authority disposal costs.
        /// </summary>
        /// <returns>
        /// An IActionResult that renders the LocalAuthorityDisposalCostsIndex view with grouped local authority data if the request is successful.
        /// Redirects to the StandardErrorIndex action in case of an error.
        /// </returns>
        public IActionResult Index()
        {
            try
            {
                this._telemetryClient.TrackTrace("Index action started. Initiating HTTP request...");

                Task<HttpResponseMessage> response = GetHttpRequest(this.configuration, this.clientFactory, this._telemetryClient);

                if (response.Result.IsSuccessStatusCode)
                {
                    this._telemetryClient.TrackEvent("SuccessfulHttpResponse");

                    var deserializedLapcapData = JsonConvert.DeserializeObject<List<LocalAuthorityDisposalCost>>(
                        response.Result.Content.ReadAsStringAsync().Result);

                    // Log metric: number of records retrieved
                    this._telemetryClient.TrackMetric("LapcapDataCount", deserializedLapcapData?.Count ?? 0);

                    var localAuthorityDisposalCosts = deserializedLapcapData ?? new List<LocalAuthorityDisposalCost>();
                    var localAuthorityData = LocalAuthorityDataUtil.GetLocalAuthorityData(localAuthorityDisposalCosts, MaterialTypes.Other);

                    var localAuthorityDataGroupedByCountry = localAuthorityData?.GroupBy((data) => data.Country).ToList();

                    this._telemetryClient.TrackEvent("DataProcessingCompleted", new Dictionary<string, string>
                    {
                        { "GroupsCount", localAuthorityDataGroupedByCountry?.Count.ToString() ?? "0" },
                    });

                    return this.View(ViewNames.LocalAuthorityDisposalCostsIndex, localAuthorityDataGroupedByCountry);
                }

                if (response.Result.StatusCode == HttpStatusCode.NotFound)
                {
                    this._telemetryClient.TrackEvent("HttpResponseNotFound");
                    return this.View(ViewNames.LocalAuthorityDisposalCostsIndex);
                }

                this._telemetryClient.TrackTrace("Redirecting to standard error page due to unexpected response.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
            catch (Exception ex)
            {
                this._telemetryClient.TrackException(ex, new Dictionary<string, string>
                {
                    { "Action", "Index" },
                });
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }
    }
}