using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Frontend.Controllers
{
    using EPR.Calculator.Frontend.Common.Constants;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Helpers;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Identity.Web;
    using Newtonsoft.Json;

    /// <summary>
    /// Controller for handling the dashboard.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    [Route("/")]
    public class DashboardController : BaseController
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        /// <param name="tokenAcquisition">The token acquisition service.</param>
        /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
        public DashboardController(IConfiguration configuration, IHttpClientFactory clientFactory, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
        }

        private bool ShowDetailedError { get; set; }

        /// <summary>
        /// Handles the Index action for the controller.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> that renders the Dashboard Index view with the calculation runs data,
        /// or redirects to the Standard Error page if an error occurs.
        /// </returns>
        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> Index()
        {
            try
            {
                this.IsShowDetailedError();

                var financialYear = CommonUtil.GetFinancialYear(DateTime.Now);
                return await this.GoToDashboardView(financialYear);
            }
            catch (Exception)
            {
                if (this.ShowDetailedError)
                {
                    throw;
                }

                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        /// <summary>
        /// Returns the list of calculation runs for the given financial year.
        /// </summary>
        /// <param name="financialYear">The financial year to filter the calculation runs.</param>
        /// <returns>The list of calculation runs.</returns>
        [Authorize(Roles = "SASuperUser")]
        [Route("Dashboard/GetCalculations")]
        public async Task<IActionResult> GetCalculations(string financialYear)
        {
            try
            {
                this.IsShowDetailedError();

                return await this.GoToDashboardView(financialYear, true);
            }
            catch (Exception)
            {
                if (this.ShowDetailedError)
                {
                    throw;
                }

                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        private void IsShowDetailedError()
        {
            var showDetailedError = this.configuration.GetValue(typeof(bool), CommonConstants.ShowDetailedError);
            if (showDetailedError != null)
            {
                this.ShowDetailedError = (bool)showDetailedError;
            }
        }

        private async Task<ActionResult> GoToDashboardView(string financialYear, bool returnPartialView = false)
        {
            var accessToken = await this.AcquireToken();

            this.HttpContext.Session.SetString(SessionConstants.FinancialYear, financialYear);

            var dashboardCalculatorRunApi = this.configuration.GetSection(ConfigSection.DashboardCalculatorRun)
                .GetSection(ConfigSection.DashboardCalculatorRunApi)
                .Value;

            if (dashboardCalculatorRunApi == null)
            {
                throw new ArgumentNullException(dashboardCalculatorRunApi);
            }

            using var response =
                await this.GetHttpRequest(financialYear, dashboardCalculatorRunApi, this.clientFactory, accessToken);

            var dashboardViewModel = new DashboardViewModel
            {
                AccessToken = accessToken,
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                FinancialYear = financialYear,
                FinancialYearListApi = this.configuration.GetSection(ConfigSection.FinancialYearListApi).Value ?? string.Empty,
                Calculations = null,
            };

            if (response.IsSuccessStatusCode)
            {
                var deserializedRuns = JsonConvert.DeserializeObject<List<CalculationRun>>(response.Content.ReadAsStringAsync().Result);

                // Ensure deserializedRuns is not null
                var calculationRuns = deserializedRuns ?? new List<CalculationRun>();
                var dashboardRunData = DashboardHelper.GetCalulationRunsData(calculationRuns);
                dashboardViewModel.Calculations = dashboardRunData;
            }

            return returnPartialView
                ? this.PartialView("_CalculationRunsPartial", dashboardViewModel.Calculations)
                : this.View(dashboardViewModel);
        }

        /// <summary>
        /// Sends an HTTP POST request to the Dashboard Calculator Run API with the specified parameters.
        /// </summary>
        /// <param name="dashboardCalculatorRunApi">The configuration object to retrieve API URL and parameters.</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        /// <param name="accessToken">token generated</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the API URL is null or empty.</exception>
        private async Task<HttpResponseMessage> GetHttpRequest(
            string year,
            string dashboardCalculatorRunApi,
            IHttpClientFactory clientFactory,
            string accessToken)
        {
            var client = clientFactory.CreateClient();
            client.BaseAddress = new Uri(dashboardCalculatorRunApi);
            client.DefaultRequestHeaders.Add("Authorization", accessToken);

            this.TelemetryClient.TrackTrace(
                $"client.DefaultRequestHeaders.Authorization is {client.DefaultRequestHeaders.Authorization}",
                SeverityLevel.Information);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(dashboardCalculatorRunApi));
            var runParms = new CalculatorRunParamsDto
            {
                FinancialYear = year,
            };
            var content = new StringContent(JsonConvert.SerializeObject(runParms), System.Text.Encoding.UTF8, StaticHelpers.MediaType);
            request.Content = content;
            var response = await client.SendAsync(request);
            this.TelemetryClient.TrackTrace($"Response is {response.StatusCode}", SeverityLevel.Warning);
            return response;
        }
    }
}