using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SessionExtensions = EPR.Calculator.Frontend.Extensions.SessionExtensions;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling the dashboard.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DashboardController"/> class.
    /// </remarks>
    /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
    /// <param name="eprCalculatorApiService">The api service.</param>
    /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
    [Route("/")]
    public class DashboardController(
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
        /// Handles the Index action for the controller.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> that renders the Dashboard Index view with the calculation runs data,
        /// or redirects to the Standard Error page if an error occurs.
        /// </returns>
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            SessionExtensions.ClearAllSession(this.HttpContext.Session);
            var model = await this.GetDashboardViewModel(CommonUtil.GetRelativeYear(this.HttpContext.Session, this.relativeYearStartingMonth));
            return this.View(ViewNames.DashboardIndex, model);
        }

        /// <summary>
        /// Returns the list of calculation runs for the given relative year.
        /// </summary>
        /// <param name="relativeYear">The relative year to filter the calculation runs.</param>
        /// <returns>The list of calculation runs.</returns>
        [Route("Dashboard/GetCalculations")]
        public async Task<IActionResult> GetCalculations(int relativeYear) // matches query param name
        {
            var model = await this.GetDashboardViewModel(new RelativeYear(relativeYear));
            return this.PartialView("_CalculationRunsPartial", model.Calculations);
        }

        private static List<RelativeYear> GetFilteredRelativeYears(List<RelativeYear> allYears, int relativeYearStartingMonth)
        {
            var today = DateTime.Today;
            int currentStartYear = today.Month >= relativeYearStartingMonth ? today.Year : today.Year - 1;

            var filteredYears = allYears
                .Where(y => y.Value <= currentStartYear)
                .OrderByDescending(y => y.Value)
                .ToList();

            return filteredYears;
        }

        private async Task<DashboardViewModel> GetDashboardViewModel(RelativeYear relativeYear)
        {
            this.HttpContext.Session.SetInt32(SessionConstants.RelativeYear, relativeYear.Value);

            var relativeYears = await this.GetRelativeYearsAsync(relativeYear);
            var dashboardViewModel = new DashboardViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                RelativeYear = relativeYear,
                Calculations = null,
                RelativeYearSelectList = relativeYears,
            };

            using var response = await this.PostCalculatorRunsAsync(relativeYear);
            if (response.IsSuccessStatusCode)
            {
                var deserializedRuns =
                    await response.Content.ReadFromJsonAsync<List<CalculationRun>>();

                // Ensure deserializedRuns is not null
                var calculationRuns = deserializedRuns ?? new List<CalculationRun>();
                var dashboardRunData = DashboardHelper.GetCalulationRunsData(calculationRuns);
                dashboardViewModel.Calculations = dashboardRunData;
            }

            return dashboardViewModel;
        }

        /// <summary>
        /// Calls the "calculatorRuns" POST endpoint.
        /// </summary>
        /// <returns>The response message returned by the endpoint.</returns>
        private async Task<HttpResponseMessage> PostCalculatorRunsAsync(RelativeYear relativeYear)
        {
            return await this.EprCalculatorApiService.CallApi(
                this.HttpContext,
                httpMethod: HttpMethod.Post,
                relativePath: "v1/calculatorRuns",
                body: new { RelativeYear = relativeYear });
        }

        private async Task<List<SelectListItem>> GetRelativeYearsAsync(RelativeYear selectedRelativeYear)
        {
            var response = await this.EprCalculatorApiService.CallApi(
                httpContext: this.HttpContext,
                httpMethod: HttpMethod.Get,
                relativePath: "v1/RelativeYears");

            if (!response.IsSuccessStatusCode)
            {
                this.TelemetryClient.TrackTrace("Unable to fetch relative years from API", SeverityLevel.Error);
                throw new InvalidOperationException("Failed to fetch relative years.");
            }

            var relativeYearsList = await response.Content.ReadFromJsonAsync<List<int>>() ?? new List<int>();
            var relativeYears = GetFilteredRelativeYears(
                relativeYearsList.Select(year => new RelativeYear(year)).ToList(),
                this.relativeYearStartingMonth);

            // Ensure current year is first
            var currentYear = CommonUtil.GetDefaultRelativeYear(DateTime.UtcNow, this.relativeYearStartingMonth);
            relativeYears.Remove(currentYear);
            relativeYears.Insert(0, currentYear);

            // Build SelectListItem list
            var selectList = relativeYears
                .Select(y => new SelectListItem
                {
                    Value = y.Value.ToString(),
                    Text = y.ToFinancialYear(),
                    Selected = y == selectedRelativeYear,
                })
                .ToList();

            return selectList;
        }
    }
}