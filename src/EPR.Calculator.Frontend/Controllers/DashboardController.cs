using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Frontend.Controllers
{
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Enums;
    using EPR.Calculator.Frontend.Helpers;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Identity.Web;
    using Newtonsoft.Json;
    using System.Net;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Runtime.Serialization;
    using System.Security.Cryptography.Xml;

    [Authorize(Roles = "SASuperUser")]
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
        /// <exception cref="ArgumentNullException">
        /// Thrown when the API URL is null or empty.
        /// </exception>
        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> Index()
        {
            try
            {
                this.ShowDetailedError = (bool)this.configuration.GetValue(typeof(bool), "ShowDetailedError");
                var accessToken = await this.AcquireToken();

                var dashboardCalculatorRunApi = this.configuration.GetSection(ConfigSection.DashboardCalculatorRun)
                    .GetSection(ConfigSection.DashboardCalculatorRunApi)
                    .Value;

                var year = this.configuration.GetSection(ConfigSection.DashboardCalculatorRun)
                    .GetSection(ConfigSection.RunParameterYear)
                    .Value;

                using var response =
                    await this.GetHttpRequest(year, dashboardCalculatorRunApi, this.clientFactory, accessToken);

                if (response.IsSuccessStatusCode)
                {
                    var deserializedRuns = JsonConvert.DeserializeObject<List<CalculationRun>>(response.Content.ReadAsStringAsync().Result);

                    // Ensure deserializedRuns is not null
                    var calculationRuns = deserializedRuns ?? new List<CalculationRun>();
                    var dashboardRunData = GetCalulationRunsData(calculationRuns);
                    return this.View(
                        ViewNames.DashboardIndex,
                        new DashboardViewModel
                        {
                            CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                            Calculations = dashboardRunData,
                        });
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return this.View(new DashboardViewModel
                    {
                        CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                    });
                }

                return this.ShowDetailedError ? throw new Exception(new HttpResponseMessage(HttpStatusCode.BadRequest).ToString()) : this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
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
        /// Processes a list of calculation runs and assigns status values based on their classifications.
        /// </summary>
        /// <param name="calculationRuns">The list of calculation runs to be processed.</param>
        /// <returns>A list of <see cref="DashboardViewModel"/> objects containing the processed data.</returns>
        private static List<DashboardViewModel.CalculationRunViewModel> GetCalulationRunsData(List<CalculationRun> calculationRuns)
        {
            var runClassifications = Enum.GetValues(typeof(RunClassification)).Cast<RunClassification>().ToList();
            var dashboardRunData = new List<DashboardViewModel.CalculationRunViewModel>();

            if (calculationRuns.Count > 0)
            {
                var displayRuns = calculationRuns.Where(x =>
                    x.CalculatorRunClassificationId != (int)RunClassification.DELETED &&
                    x.CalculatorRunClassificationId != (int)RunClassification.PLAY &&
                    x.CalculatorRunClassificationId != (int)RunClassification.QUEUE);
                foreach (var calculationRun in displayRuns)
                {
                    var classificationVal = runClassifications.Find(c => (int)c == calculationRun.CalculatorRunClassificationId);
                    var member = typeof(RunClassification).GetTypeInfo().DeclaredMembers.SingleOrDefault(x => x.Name == classificationVal.ToString());

                    var attribute = member?.GetCustomAttribute<EnumMemberAttribute>(false);

                    calculationRun.Status = attribute?.Value ?? string.Empty; // Use a default value if attribute or value is null

                    dashboardRunData.Add(new DashboardViewModel.CalculationRunViewModel(calculationRun));
                }
            }

            return dashboardRunData;
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