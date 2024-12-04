using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculationRunDetailsController"/> class.
    /// </summary>
    public class CalculationRunDetailsController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger<CalculationRunNameController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationRunDetailsController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger instance.</param>
        public CalculationRunDetailsController(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<CalculationRunNameController> logger)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Displays the calculation run details index view.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <returns>The calculation run details index view.</returns>
        public async Task<IActionResult> IndexAsync(int runId)
        {
            var calculationNameExistsResponse = await this.GetCalclDetailsAsync(runId);

            if (!calculationNameExistsResponse.IsSuccessStatusCode)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }

            var calculatorRunStatusUpdate = new CalculatorRunStatusUpdateViewModel
            {
                RunId = runId,
                ClassificationId = (int)RunClassification.DELETED,
            };

            return this.View(ViewNames.CalculationRunDetailsIndex, calculatorRunStatusUpdate);
        }

        /// <summary>
        /// Deletes the calculation details.
        /// </summary>
        /// <param name="statusUpdateViewModel">The status update view model.</param>
        /// <returns>The delete confirmation view.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the API URL or status update view model is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        public async Task<IActionResult> DeleteCalcDetailsAsync(CalculatorRunStatusUpdateViewModel statusUpdateViewModel)
        {
            ArgumentNullException.ThrowIfNull(statusUpdateViewModel);

            var apiUrl = this.configuration
                .GetSection(ConfigSection.DashboardCalculatorRun)
                .GetValue<string>(ConfigSection.DashboardCalculatorRunApi);

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new ArgumentNullException(nameof(apiUrl), "CalculationRunNameApi is null or empty. Please check the configuration settings.");
            }

            var client = this.clientFactory.CreateClient();
            client.BaseAddress = new Uri(apiUrl);

            var requestUri = new Uri($"{apiUrl}/{statusUpdateViewModel}", UriKind.Absolute);
            var response = await client.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError($"Request to {requestUri} failed with status code {response.StatusCode}");
                throw new HttpRequestException($"Request to {requestUri} failed with status code {response.StatusCode}");
            }

            return this.View(ViewNames.DeleteConfirmation);
        }

        /// <summary>
        /// Gets the calculation details.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <returns>The HTTP response message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the API URL is null or empty.</exception>
        private async Task<HttpResponseMessage> GetCalclDetailsAsync(int runId)
        {
            var apiUrl = this.configuration
                          .GetSection(ConfigSection.DashboardCalculatorRun)
                          .GetValue<string>(ConfigSection.DashboardCalculatorRunApi);

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new ArgumentNullException(apiUrl, "CalculationRunNameApi is null or empty. Please check the configuration settings.");
            }

            var client = this.clientFactory.CreateClient();
            client.BaseAddress = new Uri(apiUrl);

            var requestUri = new Uri($"{apiUrl}/{runId}", UriKind.Absolute);
            return await client.GetAsync(requestUri);
        }
    }
}