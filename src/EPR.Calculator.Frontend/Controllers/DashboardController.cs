using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
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
    /// <param name="apiService">The api service.</param>
    /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
    [Route("/")]
    public class DashboardController(
        IConfiguration configuration,
        IApiService apiService,
        TelemetryClient telemetryClient,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(
            configuration,
            telemetryClient,
            apiService,
            calculatorRunDetailsService)
    {
        private readonly int financialMonth = CommonUtil.GetFinancialYearStartingMonth(configuration);

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
            var model = await this.GetDashboardViewModel(CommonUtil.GetFinancialYear(this.HttpContext.Session, this.financialMonth));
            return this.View(ViewNames.DashboardIndex, model);
        }

        /// <summary>
        /// Returns the list of calculation runs for the given financial year.
        /// </summary>
        /// <param name="financialYear">The financial year to filter the calculation runs.</param>
        /// <returns>The list of calculation runs.</returns>
        [Route("Dashboard/GetCalculations")]
        public async Task<IActionResult> GetCalculations(string financialYear)
        {
            var model = await this.GetDashboardViewModel(financialYear);
            return this.PartialView("_CalculationRunsPartial", model.Calculations);
        }

        private static List<string> GetFilteredFinancialYears(List<string> allYears, int financialYearStartingMonth)
        {
            var today = DateTime.Today;
            int currentStartYear = today.Month >= financialYearStartingMonth ? today.Year : today.Year - 1;

            var filteredYears = allYears
                .Where(fy => int.Parse(fy.Split('-')[0]) <= currentStartYear)
                .OrderByDescending(fy => int.Parse(fy.Split('-')[0]))
                .ToList();

            return filteredYears;
        }

        private async Task<DashboardViewModel> GetDashboardViewModel(string financialYear)
        {
            this.HttpContext.Session.SetString(SessionConstants.FinancialYear, financialYear);

            var financialYears = await this.GetFinancialYearsAsync(financialYear);
            var dashboardViewModel = new DashboardViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                FinancialYear = financialYear,
                Calculations = null,
                FinancialYearSelectList = financialYears,
            };

            using var response = await this.PostCalculatorRunsAsync(financialYear);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var deserializedRuns = JsonConvert.DeserializeObject<List<CalculationRun>>(content);

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
        private async Task<HttpResponseMessage> PostCalculatorRunsAsync(string financialYear)
        {
            var apiUrl = this.ApiService.GetApiUrl(
                ConfigSection.DashboardCalculatorRun,
                ConfigSection.DashboardCalculatorRunApi);

            return await this.ApiService.CallApi(
                this.HttpContext,
                HttpMethod.Post,
                apiUrl,
                string.Empty,
                (CalculatorRunParamsDto)financialYear);
        }

        private async Task<List<SelectListItem>> GetFinancialYearsAsync(string selectedFinancialYear)
        {
            var apiUrl = this.ApiService.GetApiUrl(ConfigSection.FinancialYearListApi);
            var response = await this.ApiService.CallApi(this.HttpContext, HttpMethod.Get, apiUrl, string.Empty, null);

            if (!response.IsSuccessStatusCode)
            {
                this.TelemetryClient.TrackTrace("Unable to fetch financial years from API", SeverityLevel.Error);
                throw new InvalidOperationException("Failed to fetch financial years.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var years = JsonConvert.DeserializeObject<List<FinancialYearDto>>(content) ?? new List<FinancialYearDto>();
            var financialYears = years.Select(x => x.Name).ToList();

            // Sort by starting year descending
            financialYears = financialYears
                .OrderByDescending(fy => int.Parse(fy.Substring(0, 4)))
                .ToList();

            financialYears = GetFilteredFinancialYears(financialYears, this.financialMonth);

            // Ensure current year is first
            var currentYear = CommonUtil.GetDefaultFinancialYear(DateTime.UtcNow, this.financialMonth);
            financialYears.Remove(currentYear);
            financialYears.Insert(0, currentYear);

            // Build SelectListItem list
            var selectList = financialYears
                .Select(fy => new SelectListItem
                {
                    Value = fy,
                    Text = fy,
                    Selected = fy == selectedFinancialYear,
                })
                .ToList();

            return selectList;
        }
    }
}