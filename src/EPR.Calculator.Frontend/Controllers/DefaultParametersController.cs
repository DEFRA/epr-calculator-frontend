using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling default parameter settings.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DefaultParametersController"/> class.
    /// </remarks>
    /// <param name="configuration">The configuration settings for the application.</param>
    /// <param name="clientFactory">The factory for creating HTTP clients.</param>
    /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
    public class DefaultParametersController(
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
        /// Handles the Index action for retrieving and displaying default scheme parameters.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> that renders the view with the retrieved data or redirects to an error page.
        /// </returns>
        [Route("ViewDefaultParameters")]
        public async Task<IActionResult> Index()
        {
            var relativeYear = CommonUtil.GetRelativeYear(this.HttpContext.Session, this.relativeYearStartingMonth);
            var currentUser = CommonUtil.GetUserName(this.HttpContext);

            var response = await this.GetDefaultParametersAsync(relativeYear);

            if (response.IsSuccessStatusCode)
            {
                var viewModel = new DefaultParametersViewModel
                {
                    CurrentUser = currentUser,
                    LastUpdatedBy = CommonUtil.GetUserName(this.HttpContext),
                    SchemeParameters = new List<SchemeParametersViewModel>(),
                    BackLinkViewModel = new BackLinkViewModel()
                    {
                        BackLink = string.Empty,
                        CurrentUser = currentUser,
                    },
                };
                var data = await response.Content.ReadAsStringAsync();
                var defaultSchemeParameters = await response.Content.ReadFromJsonAsync<List<DefaultSchemeParameters>>() ?? new List<DefaultSchemeParameters>();

                foreach (ParameterType name in (ParameterType[])Enum.GetValues(typeof(ParameterType)))
                {
                    viewModel.SchemeParameters.Add(GetSchemeParametersBasedonCategory(defaultSchemeParameters, name));
                }

                viewModel.EffectiveFrom = defaultSchemeParameters.First().EffectiveFrom;
                viewModel.IsDataAvailable = true;

                return this.View(viewModel);

            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return this.View(new DefaultParametersViewModel
                {
                    CurrentUser = currentUser,
                    LastUpdatedBy = string.Empty,
                    IsDataAvailable = false,
                    BackLinkViewModel = new BackLinkViewModel()
                    {
                        BackLink = string.Empty,
                        CurrentUser = currentUser,
                    },
                });
            }

            return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
        }

        private static SchemeParametersViewModel GetSchemeParametersBasedonCategory(List<DefaultSchemeParameters> defaultSchemeParameters, ParameterType parameterType)
        {
            var type = parameterType.GetDisplayName();
            bool shouldDisplayPrefix = !IsExcludedFromPrefixDisplay(parameterType);

            return new SchemeParametersViewModel
            {
                DefaultSchemeParameters = defaultSchemeParameters.Where(t => t.ParameterType == type).ToList(),
                IsDisplayPrefix = shouldDisplayPrefix,
                SchemeParameterName = type,
            };
        }

        private static bool IsExcludedFromPrefixDisplay(ParameterType parameterType)
        {
            return parameterType == ParameterType.LateReportingTonnage || parameterType == ParameterType.BadDebtProvision;
        }

        private async Task<HttpResponseMessage> GetDefaultParametersAsync(RelativeYear relativeYear)
        {
            return await this.EprCalculatorApiService.CallApi(
                httpContext: this.HttpContext,
                httpMethod: HttpMethod.Get,
                relativePath: $"v1/defaultParameterSetting/{relativeYear}");
        }
    }
}