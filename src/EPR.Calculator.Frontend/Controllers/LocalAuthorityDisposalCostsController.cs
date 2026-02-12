using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalAuthorityDisposalCostsController"/> class.
    /// </summary>
    /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
    /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
    /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
    public class LocalAuthorityDisposalCostsController(
        IConfiguration configuration,
        IEprCalculatorApiService eprCalculatorApiService,
        TelemetryClient telemetryClient,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(
            configuration,
            telemetryClient,
            eprCalculatorApiService,
            calculatorRunDetailsService)
    {
        private readonly int relativeYearStartingMonth = CommonUtil.GetRelativeYearStartingMonth(configuration);

        /// <summary>
        /// Handles the Index action asynchronously. Sends an HTTP request and processes the response to display local authority disposal costs.
        /// </summary>
        /// <returns>
        /// An IActionResult that renders the LocalAuthorityDisposalCostsIndex view with grouped local authority data if the request is successful.
        /// Redirects to the StandardErrorIndex action in case of an error.
        /// </returns>
        [Route("ViewLocalAuthorityDisposalCosts")]
        public async Task<IActionResult> Index()
        {
            var relativeYear = CommonUtil.GetRelativeYear(this.HttpContext.Session, this.relativeYearStartingMonth);

            var response = await this.GetLapcapDataAsync(relativeYear);

            var currentUser = CommonUtil.GetUserName(this.HttpContext);

            if (response.IsSuccessStatusCode)
            {
                var deserializedLapcapData = await response.Content.ReadFromJsonAsync<List<LocalAuthorityDisposalCost>>() ?? new List<LocalAuthorityDisposalCost>();

                // Ensure deserializedRuns is not null
                var localAuthorityData = LocalAuthorityDataUtil.GetLocalAuthorityData(deserializedLapcapData, MaterialTypes.Other);

                var localAuthorityDataGroupedByCountry = localAuthorityData?.GroupBy((data) => data.Country).ToList();

                return this.View(
                    ViewNames.LocalAuthorityDisposalCostsIndex,
                    new LocalAuthorityViewModel
                    {
                        CurrentUser = currentUser,
                        LastUpdatedBy = deserializedLapcapData.FirstOrDefault()?.CreatedBy ?? ErrorMessages.UnknownUser,
                        ByCountry = localAuthorityDataGroupedByCountry,
                        BackLinkViewModel = new BackLinkViewModel()
                        {
                            BackLink = string.Empty,
                            CurrentUser = currentUser,
                        },
                    });
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
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

        private async Task<HttpResponseMessage> GetLapcapDataAsync(RelativeYear relativeYear)
        {
            return await this.EprCalculatorApiService.CallApi(
                httpContext: this.HttpContext,
                httpMethod: HttpMethod.Get,
                relativePath: $"v1/lapcapData/{relativeYear}");
        }
    }
}