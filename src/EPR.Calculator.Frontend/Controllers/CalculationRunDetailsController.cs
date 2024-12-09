using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculationRunDetailsController"/> class.
    /// </summary>
    public class CalculationRunDetailsController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger<CalculationRunDetailsController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationRunDetailsController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger instance.</param>
        public CalculationRunDetailsController(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<CalculationRunDetailsController> logger)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Displays the calculation run details index view.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <param name="calcName">The calcName of the calculation run.</param>
        /// <returns>The calculation run details index view.</returns>
        public async Task<IActionResult> IndexAsync(int runId, string calcName)
        {
            try
            {
                var getCalculationDetailsResponse = await this.GetCalculationDetailsAsync(runId);

                if (getCalculationDetailsResponse == null)
                {
                    this.logger.LogError(
                            $"Request failed with status code {getCalculationDetailsResponse.StatusCode}");
                    return this.RedirectToAction(ActionNames.StandardErrorIndex,
                            CommonUtil.GetControllerName(typeof(StandardErrorController)));
                }

                var statusUpdateViewModel = new CalculatorRunStatusUpdateDto
                {
                    RunId = runId,
                    ClassificationId = (int)RunClassification.DELETED,
                    CalcName = calcName,
                };

                if (!getCalculationDetailsResponse.IsSuccessStatusCode)
                {
                    return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
                }

                return this.View(ViewNames.CalculationRunDetailsIndex, statusUpdateViewModel);
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        /// <summary>
        /// Deletes the calculation details.
        /// </summary>
        /// <param name="runId">The status update view model.</param>
        /// <param name="calcName">The calculation name.</param>
        /// <param name="deleteChecked">The delete is checked or not.</param>
        /// <returns>The delete confirmation view.</returns>
        public IActionResult DeleteCalcDetails(int runId, string calcName, bool deleteChecked)
        {
            try
            {
                var dashboardCalculatorRunApi = this.configuration.GetSection(ConfigSection.DashboardCalculatorRun)
                    .GetSection(ConfigSection.DashboardCalculatorRunApi)
                    .Value;

                var client = this.clientFactory.CreateClient();
                client.BaseAddress = new Uri(dashboardCalculatorRunApi);
                var statusUpdateViewModel = new CalculatorRunStatusUpdateDto
                {
                    RunId = runId,
                    ClassificationId = (int)RunClassification.DELETED,
                    CalcName = calcName,
                };

                if (!deleteChecked)
                {
                    this.ViewBag.Errors = CreateErrorViewModel(ErrorMessages.SelectDeleteCalculation);
                    return this.View(ViewNames.CalculationRunDetailsIndex, statusUpdateViewModel);
                }

                var request = new HttpRequestMessage(HttpMethod.Put,
                    new Uri($"{dashboardCalculatorRunApi}?runId={statusUpdateViewModel.RunId}&classificationId={statusUpdateViewModel.ClassificationId}"));
                var response = client.SendAsync(request);
                response.Wait();

                if (response.Result.StatusCode != HttpStatusCode.Created)
                {
                    this.logger.LogError($"Request to {dashboardCalculatorRunApi} failed with status code {response.Result}");
                    this.ViewBag.Errors = CreateErrorViewModel(ErrorMessages.DeleteCalculationError);
                    return this.View(ViewNames.CalculationRunDetailsIndex, statusUpdateViewModel);
                }

                return this.View(ViewNames.DeleteConfirmation, statusUpdateViewModel);
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        /// <summary>
        /// Asynchronously retrieves calculation details for a given run ID.
        /// </summary>
        /// <param name="runId">The ID of the calculation run to retrieve details for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the API URL is null or empty.</exception>
        private async Task<HttpResponseMessage> GetCalculationDetailsAsync(int runId)
        {
            var client = this.CreateHttpClient();
            var apiUrl = client.BaseAddress.ToString();
            var requestUri = new Uri($"{apiUrl}/{runId}", UriKind.Absolute);
            return await client.GetAsync(requestUri);
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
                DOMElementId = ViewControlNames.DeleteCalculationName,
                ErrorMessage = errorMessage,
            };
        }

        /// <summary>
        /// Creates and configures an <see cref="HttpClient"/> instance.
        /// </summary>
        /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the API URL is null or empty.</exception>
        private HttpClient CreateHttpClient()
        {
            var apiUrl = this.configuration
                .GetSection(ConfigSection.DashboardCalculatorRun)
                .GetValue<string>(ConfigSection.DashboardCalculatorRunApi);

            var client = this.clientFactory.CreateClient();
            client.BaseAddress = new Uri(apiUrl);
            return client;
        }
    }
}