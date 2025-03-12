using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Configuration;
using System.Net;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalAuthorityDisposalCostsController"/> class.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    public class LocalAuthorityDisposalCostsController : BaseController
    {
        private readonly IHttpClientFactory clientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAuthorityDisposalCostsController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        /// <param name="tokenAcquisition">The token acquisition service.</param>
        /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
        public LocalAuthorityDisposalCostsController(IConfiguration configuration, IHttpClientFactory clientFactory, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.clientFactory = clientFactory;
        }

        /// <summary>
        /// Sends an HTTP POST request to the Local Authority Disposal Costs API with the specified parameters.
        /// </summary>
        /// <param name="lapcapRunApi">Lapcap Run Api API URL and parameters.</param>
        /// <param name="year">year from configuration</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the API URL is null or empty.</exception>
        public async Task<HttpResponseMessage> GetHttpRequest(string lapcapRunApi, string year, IHttpClientFactory clientFactory)
        {
            var client = clientFactory.CreateClient();
            client.BaseAddress = new Uri(lapcapRunApi);
            var accessToken = await this.AcquireToken();
            client.DefaultRequestHeaders.Add("Authorization", accessToken);
            var uri = new Uri(string.Format("{0}/{1}", lapcapRunApi, year));
            var response = await client.GetAsync(uri);

            return response;
        }

        /// <summary>
        /// Handles the Index action asynchronously. Sends an HTTP request and processes the response to display local authority disposal costs.
        /// </summary>
        /// <returns>
        /// An IActionResult that renders the LocalAuthorityDisposalCostsIndex view with grouped local authority data if the request is successful.
        /// Redirects to the StandardErrorIndex action in case of an error.
        /// </returns>
        [Authorize(Roles = "SASuperUser")]
        [Route("ViewLocalAuthorityDisposalCosts")]
        public IActionResult Index()
        {
            try
            {
                var lapcapRunApi = this.Configuration.GetSection(ConfigSection.LapcapSettings).GetSection(ConfigSection.LapcapSettingsApi).Value;

                var year = this.Configuration.GetSection(ConfigSection.LapcapSettings)
                    .GetSection(ConfigSection.ParameterYear).Value;

                if (string.IsNullOrWhiteSpace(year) || string.IsNullOrWhiteSpace(lapcapRunApi))
                {
                    throw new ConfigurationErrorsException("LapcapSettings or RunParameterYear missing");
                }

                var response = this.GetHttpRequest(lapcapRunApi, year, this.clientFactory);

                if (response.Result.IsSuccessStatusCode)
                {
                    var deserializedlapcapdata = JsonConvert.DeserializeObject<List<LocalAuthorityDisposalCost>>(response.Result.Content.ReadAsStringAsync().Result);

                    // Ensure deserializedRuns is not null
                    var localAuthorityDisposalCosts = deserializedlapcapdata ?? new List<LocalAuthorityDisposalCost>();
                    var localAuthorityData = LocalAuthorityDataUtil.GetLocalAuthorityData(localAuthorityDisposalCosts, MaterialTypes.Other);

                    var localAuthorityDataGroupedByCountry = localAuthorityData?.GroupBy((data) => data.Country).ToList();

                    return this.View(
                        ViewNames.LocalAuthorityDisposalCostsIndex,
                        new LocalAuthorityViewModel
                        {
                            CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                            LastUpdatedBy = deserializedlapcapdata?.First().CreatedBy ?? ErrorMessages.UnknownUser,
                            ByCountry = localAuthorityDataGroupedByCountry,
                        });
                }

                if (response.Result.StatusCode == HttpStatusCode.NotFound)
                {
                    return this.View(ViewNames.LocalAuthorityDisposalCostsIndex, new LocalAuthorityViewModel
                    {
                        CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                        LastUpdatedBy = ErrorMessages.UnknownUser,
                        ByCountry = new List<IGrouping<string, LocalAuthorityViewModel.LocalAuthorityData>>(),
                    });
                }

                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }
    }
}