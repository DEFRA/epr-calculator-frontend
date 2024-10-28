using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculationRunNameController"/> class.
    /// </summary>
    public class CalculationRunNameController : Controller
    {
        private const string CalculationRunNameIndexView = ViewNames.CalculationRunNameIndex;
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger<CalculationRunNameController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationRunNameController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger instance.</param>
        public CalculationRunNameController(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<CalculationRunNameController> logger)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Displays the index view for calculation run names.
        /// </summary>
        /// <returns>The index view.</returns>
        public IActionResult Index()
        {
            return this.View(CalculationRunNameIndexView);
        }

        /// <summary>
        /// Handles the calculator run initiation.
        /// </summary>
        /// <param name="calculationRunModel">The model containing calculation run details.</param>
        /// <returns>The result of the action.</returns>
        [HttpPost]
        public async Task<IActionResult> RunCalculator(InitiateCalculatorRunModel calculationRunModel)
        {
            if (!this.ModelState.IsValid)
            {
                var errorMessages = this.ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage);
                this.ViewBag.Errors = CreateErrorViewModel(errorMessages.First());
                return this.View(CalculationRunNameIndexView);
            }

            try
            {
                if (!string.IsNullOrEmpty(calculationRunModel.CalculationName))
                {
                    var calculationNameExistsResponse = await this.CheckIfCalculationNameExistsAsync(calculationRunModel.CalculationName);
                    if (calculationNameExistsResponse.IsSuccessStatusCode)
                    {
                        this.ViewBag.Errors = CreateErrorViewModel(ErrorMessages.CalculationRunNameExists);
                        return this.View(CalculationRunNameIndexView);
                    }

                    this.HttpContext.Session.SetString(SessionConstants.CalculationName, calculationRunModel.CalculationName);
                }

                return this.RedirectToAction(ActionNames.RunCalculatorConfirmation);
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException(ex.ToString());
            }
        }

        /// <summary>
        /// Displays the confirmation view after running the calculator.
        /// </summary>
        /// <returns>The confirmation view.</returns>
        public async Task<IActionResult> Confirmation()
        {
            try
            {
                string runName = this.HttpContext.Session.GetString(SessionConstants.CalculationName) ?? string.Empty;

                var response = await this.PostHttpRequestAsync(runName);

                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted)
                {
                    return this.View(ViewNames.CalculationRunConfirmation);
                }

                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during confirmation process.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        /// <summary>
        /// Creates an error view model.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The error view model.</returns>
        private static ErrorViewModel CreateErrorViewModel(string errorMessage)
        {
            return new ErrorViewModel
            {
                DOMElementId = ViewControlNames.CalculationRunName,
                ErrorMessage = errorMessage,
            };
        }

        /// <summary>
        /// Sends an HTTP request to the calculator run API.
        /// </summary>
        /// <param name="calculatorRunName">The name of the calculator run.</param>
        /// <returns>The HTTP response message.</returns>
        private async Task<HttpResponseMessage> PostHttpRequestAsync(string calculatorRunName)
        {
            var calculatorRunApi = this.configuration[$"{ConfigSection.CalculationRunSettings}:{ConfigSection.CalculationRunApi}"];

            if (string.IsNullOrEmpty(calculatorRunApi))
            {
                throw new ArgumentNullException(calculatorRunApi, "The API URL is null or empty. Check the configuration settings for calculatorRun");
            }

            var year = this.configuration[$"{ConfigSection.CalculationRunSettings}:{ConfigSection.RunParameterYear}"];

            if (string.IsNullOrEmpty(year))
            {
                throw new ArgumentNullException(year, "RunParameterYear is null or empty. Check the configuration settings for calculatorRun.");
            }

            var client = this.clientFactory.CreateClient();
            client.BaseAddress = new Uri(calculatorRunApi);

            var runParms = new CreateCalculatorRunDto
            {
                CalculatorRunName = calculatorRunName,
                FinancialYear = year,
                CreatedBy = "Test User",
            };

            var content = new StringContent(JsonConvert.SerializeObject(runParms), System.Text.Encoding.UTF8, StaticHelpers.MediaType);
            var request = new HttpRequestMessage(HttpMethod.Post, calculatorRunApi) { Content = content };

            return await client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> CheckIfCalculationNameExistsAsync(string calculationName)
        {
            var apiUrl = this.configuration
                          .GetSection(ConfigSection.CalculationRunSettings)
                          .GetValue<string>(ConfigSection.CalculationRunNameApi);

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new ArgumentNullException(apiUrl, "CalculationRunNameApi is null or empty. Please check the configuration settings.");
            }

            var client = this.clientFactory.CreateClient();
            client.BaseAddress = new Uri(apiUrl);

            var requestUri = new Uri($"{apiUrl}/{calculationName}", UriKind.Absolute);
            return await client.GetAsync(requestUri);
        }
    }
}