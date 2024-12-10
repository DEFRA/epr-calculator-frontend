namespace EPR.Calculator.Frontend.Controllers
{
    using System.Net;
    using System.Reflection;
    using System.Runtime.Serialization;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Enums;
    using EPR.Calculator.Frontend.Helpers;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    [Authorize(Roles = "SASuperUser")]
    public class DashboardController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        public DashboardController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
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
        public IActionResult Index()
        {
            try
            {
                Task<HttpResponseMessage> response = GetHttpRequest(this.configuration, this.clientFactory);

                if (response.Result.IsSuccessStatusCode)
                {
                    var deserializedRuns = JsonConvert.DeserializeObject<List<CalculationRun>>(response.Result.Content.ReadAsStringAsync().Result);

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
                foreach (var calculationRun in calculationRuns)
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
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the API URL is null or empty.</exception>
        private static Task<HttpResponseMessage> GetHttpRequest(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
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

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(dashboardCalculatorRunApi));
            var runParms = new CalculatorRunParamsDto
            {
                FinancialYear = year,
            };
            var content = new StringContent(JsonConvert.SerializeObject(runParms), System.Text.Encoding.UTF8, StaticHelpers.MediaType);
            request.Content = content;
            var response = client.SendAsync(request);
            response.Wait();

            return response;
        }
    }
}