using CsvHelper.Configuration;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Identity.Abstractions;

namespace EPR.Calculator.Frontend.Controllers
{
    using Azure.Core;
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
    using System.Runtime.Serialization;
    using static System.Formats.Asn1.AsnWriter;

    [Authorize(Roles = "SASuperUser")]
    public class DashboardController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        private readonly IAuthorizationHeaderProvider authProvider;
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        /// <param name="authProvider"></param>
        public DashboardController(IConfiguration configuration, IHttpClientFactory clientFactory,
            IAuthorizationHeaderProvider authProvider, TelemetryClient telemetryClient)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            this.authProvider = authProvider;
            this.telemetryClient = telemetryClient;
        }

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
        [AuthorizeForScopes(Scopes = new[] { "api://542488b9-bf70-429f-bad7-1e592efce352/default" })]
        public async Task<IActionResult> Index()
        {
            try
            {
                var scopes = new List<string> { "api://542488b9-bf70-429f-bad7-1e592efce352/default" };
                var accessToken = await this.authProvider.CreateAuthorizationHeaderForUserAsync(scopes);

                using HttpResponseMessage response = await GetHttpRequest(this.configuration, this.clientFactory, accessToken);

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
                    return this.View();
                }

                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
            catch (Exception e)
            {
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
                    var classification_val = runClassifications.Find(c => (int)c == calculationRun.CalculatorRunClassificationId);
                    var member = typeof(RunClassification).GetTypeInfo().DeclaredMembers.SingleOrDefault(x => x.Name == classification_val.ToString());

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
        /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        /// <param name="accessToken">token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the API URL is null or empty.</exception>
        private async Task<HttpResponseMessage> GetHttpRequest(IConfiguration configuration,
            IHttpClientFactory clientFactory, string accessToken)
        {
            // var scopes = new List<string> { "api://542488b9-bf70-429f-bad7-1e592efce352/default" };
            // this.telemetryClient.TrackTrace($"before generating {scopes.First()}");
            // var accessToken = HttpContext?.Session?.GetString("accessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                // accessToken = await this.authProvider.CreateAuthorizationHeaderForUserAsync(scopes);
                this.telemetryClient.TrackTrace("after generating..");
                HttpContext?.Session?.SetString("accessToken", accessToken);
            }

            this.telemetryClient.TrackTrace($"accessToken is {accessToken}", SeverityLevel.Warning);
            this.telemetryClient.TrackTrace($"accessToken is {accessToken}", SeverityLevel.Error);
            this.telemetryClient.TrackTrace($"accessToken is {accessToken}", SeverityLevel.Critical);
            this.telemetryClient.TrackTrace($"accessToken is {accessToken}", SeverityLevel.Information);

            var dashboardCalculatorRunApi = configuration.GetSection(ConfigSection.DashboardCalculatorRun)
                                                  .GetSection(ConfigSection.DashboardCalculatorRunApi)
                                                  .Value;

            if (string.IsNullOrEmpty(dashboardCalculatorRunApi))
            {
                // Handle the null or empty case appropriately
                throw new ArgumentNullException(dashboardCalculatorRunApi, "The API URL cannot be null or empty.");
            }

            var year = configuration.GetSection(ConfigSection.DashboardCalculatorRun)
                                          .GetSection(ConfigSection.RunParameterYear)
                                          .Value;
            var client = clientFactory.CreateClient();
            client.BaseAddress = new Uri(dashboardCalculatorRunApi);
            client.DefaultRequestHeaders.Add("Authorization", accessToken);

            this.telemetryClient.TrackTrace($"client.DefaultRequestHeaders.Authorization is {client.DefaultRequestHeaders.Authorization}", SeverityLevel.Information);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(dashboardCalculatorRunApi));
            var runParms = new CalculatorRunParamsDto
            {
                FinancialYear = year,
            };
            var content = new StringContent(JsonConvert.SerializeObject(runParms), System.Text.Encoding.UTF8, StaticHelpers.MediaType);
            request.Content = content;
            var response = await client.SendAsync(request);
            this.telemetryClient.TrackTrace($"Response is {response.StatusCode}", SeverityLevel.Warning);
            return response;
        }
    }
}