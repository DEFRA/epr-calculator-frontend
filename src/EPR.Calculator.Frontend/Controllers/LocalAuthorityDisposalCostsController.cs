using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAuthorityDisposalCostsController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        public LocalAuthorityDisposalCostsController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
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
                Task<HttpResponseMessage> response = GetHttpRequest(this.configuration, this.clientFactory);

                if (response.Result.IsSuccessStatusCode)
                {
                    var deserializedlapcapdata = JsonConvert.DeserializeObject<List<LocalAuthorityDisposalCost>>(response.Result.Content.ReadAsStringAsync().Result);

                    // Ensure deserializedRuns is not null
                    var localAuthorityDisposalCosts = deserializedlapcapdata ?? new List<LocalAuthorityDisposalCost>();
                    var localAuthorityData = LocalAuthorityDataUtil.GetLocalAuthorityData(localAuthorityDisposalCosts);

                    var localAuthorityDataGroupedByCountry = localAuthorityData?.GroupBy((data) => data.Country).ToList();

                    return this.View(ViewNames.LocalAuthorityDisposalCostsIndex, localAuthorityDataGroupedByCountry);
                }

                if (response.Result.StatusCode == HttpStatusCode.NotFound)
                {
                    return this.View();
                }

                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        /// <summary>
        /// Sends an HTTP POST request to the Local Authority Disposal Costs API with the specified parameters.
        /// </summary>
        /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the API URL is null or empty.</exception>
        private static async Task<HttpResponseMessage> GetHttpRequest(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            var lapcapRunApi = configuration.GetSection(ConfigSection.LapcapSettings)
                                                  .GetSection(ConfigSection.LapcapSettingsApi)
                                                  .Value;

            if (string.IsNullOrEmpty(lapcapRunApi))
            {
                // Handle the null or empty case appropriately
                throw new ArgumentNullException(lapcapRunApi, ApiUrl.Url);
            }

            var client = clientFactory.CreateClient();
            client.BaseAddress = new Uri(lapcapRunApi);
            var year = configuration.GetSection(ConfigSection.LapcapSettings).GetSection(ConfigSection.ParameterYear).Value;
            var uri = new Uri(string.Format("{0}/{1}", lapcapRunApi, year));
            var response = await client.GetAsync(uri);

            return response;
        }
    }
}