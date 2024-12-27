using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ConfigurationException = CsvHelper.Configuration.ConfigurationException;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculationRunNameController"/> class.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    public class CalculationRunNameController : BaseController
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
        public CalculationRunNameController(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<CalculationRunNameController> logger, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Displays the index view for calculation run names.
        /// </summary>
        /// <returns>The index view.</returns>
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index()
        {
            return this.View(
                CalculationRunNameIndexView,
                new InitiateCalculatorRunModel
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                });
        }

        /// <summary>
        /// Handles the calculator run initiation.
        /// </summary>
        /// <param name="calculationRunModel">The model containing calculation run details.</param>
        /// <returns>The result of the action.</returns>
        [HttpPost]
        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> RunCalculator(InitiateCalculatorRunModel calculationRunModel)
        {
            if (!this.ModelState.IsValid)
            {
                var errorMessages = this.ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage);
                this.ViewBag.Errors = CreateErrorViewModel(errorMessages.First());
                return this.View(CalculationRunNameIndexView, calculationRunModel);
            }

            try
            {
                if (!string.IsNullOrEmpty(calculationRunModel.CalculationName))
                {
                    var calculationName = calculationRunModel.CalculationName.Trim();
                    var calculationNameExistsResponse = await this.CheckIfCalculationNameExistsAsync(calculationName);
                    if (calculationNameExistsResponse.IsSuccessStatusCode)
                    {
                        this.ViewBag.Errors = CreateErrorViewModel(ErrorMessages.CalculationRunNameExists);
                        return this.View(CalculationRunNameIndexView, calculationRunModel);
                    }

                    var response = await this.HttpPostToCalculatorRunApi(calculationName);

                    if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                    {
                        var errorDto = new ErrorDto() { Message = await this.ExtractErrorMessageAsync(response) };
                        return this.RedirectToAction(ActionNames.CalculationRunErrorIndex, "CalculationRunError", errorDto);
                    }

                    if (!response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.Accepted)
                    {
                        return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
                    }
                }

                return this.RedirectToAction(ActionNames.RunCalculatorConfirmation, calculationRunModel);
            }
            catch (Exception e)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        /// <summary>
        /// Displays the confirmation view after running the calculator.
        /// </summary>
        /// <param name="calculationRunModel">The model containing calculation run details.</param>
        /// <returns>The result of the action.</returns>
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Confirmation(InitiateCalculatorRunModel calculationRunModel)
        {
            return this.View(ViewNames.CalculationRunConfirmation, calculationRunModel);
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
        ///  Sends an HTTP request to the calculator run API.
        /// </summary>
        /// <param name="calculatorRunName">The name of the calculator run.</param>
        /// <returns>The HTTP response message.</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException will be thrown</exception>
        private async Task<HttpResponseMessage> HttpPostToCalculatorRunApi(string calculatorRunName)
        {
            var (calculatorRunApi, year) = this.GetCalculatorRunParameters();
            var client = this.clientFactory.CreateClient();
            client.BaseAddress = new Uri(calculatorRunApi);
            var accessToken = await this.AcquireToken();

            // Assuming you use the accessToken, calculatorRunName, and year in the request
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var content = new StringContent($"{{ \"name\": \"{calculatorRunName}\", \"year\": \"{year}\" }}", Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/calculations", content);
            return response;
        }

        /// <summary>
        /// Checks if a calculation name exists asynchronously.
        /// </summary>
        /// <param name="calculationName">The name of the calculation to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message indicating whether the calculation name exists.</returns>
        private async Task<HttpResponseMessage> CheckIfCalculationNameExistsAsync(string calculationName)
        {
            var apiUrl = this.GetApiUrl();
            var client = this.clientFactory.CreateClient();
            client.BaseAddress = new Uri(apiUrl);
            var accessToken = await this.AcquireToken();

            // Assuming you use the accessToken and calculationName in the request
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.GetAsync($"api/calculations/{calculationName}");
            return response;
        }

        /// <summary>
        /// Extracts the error message coming in api response.
        /// </summary>
        /// <param name="response">Http response from api.</param>
        /// <returns>Error message.</returns>
        private async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
        {
            try
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseBody);
                return json["message"]?.ToString() ?? "An error occurred. Please try again.";
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error parsing response");
                return "Unable to process the error response.";
            }
        }

        private string GetApiUrl()
        {
            var apiUrl = this.configuration
                .GetSection(ConfigSection.CalculationRunSettings)
                .GetValue<string>(ConfigSection.CalculationRunNameApi);
            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new ConfigurationException(
                    $"CalculationRunNameApi is null or empty. Please check the configuration settings. ${ConfigSection.CalculationRunSettings}");
            }

            return apiUrl;
        }

        public (string ApiUrl, string Year) GetCalculatorRunParameters()
        {
            var calculatorRunApi = this.configuration
                .GetSection(ConfigSection.CalculationRunSettings)
                .GetValue<string>(ConfigSection.CalculationRunApi);

            var year = this.configuration
                .GetSection(ConfigSection.CalculationRunSettings)
                .GetValue<string>(ConfigSection.RunParameterYear);
            if (string.IsNullOrEmpty(year) || string.IsNullOrEmpty(calculatorRunApi))
            {
                throw new ConfigurationErrorsException(
                    "RunParameterYear or CalculationRunSettings is null or empty. Check the configuration settings for calculatorRun.");
            }

            return (calculatorRunApi, year);
        }
    }
}