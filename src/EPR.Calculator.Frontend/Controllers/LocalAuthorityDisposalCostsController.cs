using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
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
    /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
    /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
    /// <param name="tokenAcquisition">The token acquisition service.</param>
    /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
    public class LocalAuthorityDisposalCostsController(
        IConfiguration configuration,
        IApiService apiService,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(
            configuration,
            tokenAcquisition,
            telemetryClient,
            apiService,
            calculatorRunDetailsService)
    {
        /// <summary>
        /// Handles the Index action asynchronously. Sends an HTTP request and processes the response to display local authority disposal costs.
        /// </summary>
        /// <returns>
        /// An IActionResult that renders the LocalAuthorityDisposalCostsIndex view with grouped local authority data if the request is successful.
        /// Redirects to the StandardErrorIndex action in case of an error.
        /// </returns>
        [Route("ViewLocalAuthorityDisposalCosts")]
        public IActionResult Index()
        {
            try
            {
                var lapcapRunApi = this.Configuration.GetSection(ConfigSection.LapcapSettings).GetSection(ConfigSection.LapcapSettingsApi).Value;

                var year = CommonUtil.GetFinancialYear(this.HttpContext.Session);

                if (string.IsNullOrWhiteSpace(year))
                {
                    throw new ConfigurationErrorsException("RunParameterYear missing");
                }

                var response = this.GetLapcapDataAsync(year);

                var currentUser = CommonUtil.GetUserName(this.HttpContext);

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
                            CurrentUser = currentUser,
                            LastUpdatedBy = deserializedlapcapdata?.First().CreatedBy ?? ErrorMessages.UnknownUser,
                            ByCountry = localAuthorityDataGroupedByCountry,
                            BackLinkViewModel = new BackLinkViewModel()
                            {
                                BackLink = string.Empty,
                                CurrentUser = currentUser,
                            },
                        });
                }

                if (response.Result.StatusCode == HttpStatusCode.NotFound)
                {
                    return this.View(ViewNames.LocalAuthorityDisposalCostsIndex, new LocalAuthorityViewModel
                    {
                        CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                        LastUpdatedBy = ErrorMessages.UnknownUser,
                        ByCountry = new List<IGrouping<string, LocalAuthorityViewModel.LocalAuthorityData>>(),
                        BackLinkViewModel = new BackLinkViewModel()
                        {
                            BackLink = string.Empty,
                            CurrentUser = currentUser,
                        },
                    });
                }

                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        private async Task<HttpResponseMessage> GetLapcapDataAsync(string parameterYear)
        {
            var apiUrl = this.ApiService.GetApiUrl(
                ConfigSection.LapcapSettings,
                ConfigSection.LapcapSettingsApi);
            return await this.ApiService.CallApi(
                this.HttpContext,
                HttpMethod.Get,
                apiUrl,
                parameterYear,
                null);
        }
    }
}