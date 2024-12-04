using Azure.Core;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Newtonsoft.Json;
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
            var getCalculationDetailsResponse = await this.GetCalclDetailsAsync(runId);

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

        /// <summary>
        /// Deletes the calculation details.
        /// </summary>
        /// <param name="runId">The status update view model.</param>
        /// <returns>The delete confirmation view.</returns>
        public IActionResult DeleteCalcDetails(int runId)
        {
            var dashboardCalculatorRunApi = this.configuration.GetSection(ConfigSection.DashboardCalculatorRun)
                                                 .GetSection(ConfigSection.DashboardCalculatorRunApi)
                                                 .Value;

            if (string.IsNullOrEmpty(dashboardCalculatorRunApi))
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }

            var client = this.clientFactory.CreateClient();
            client.BaseAddress = new Uri(dashboardCalculatorRunApi);
            var statusUpdateViewModel = new CalculatorRunStatusUpdateDto
            {
                RunId = runId,
                ClassificationId = (int)RunClassification.DELETED,
            };

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri($"{dashboardCalculatorRunApi}?runId={statusUpdateViewModel.RunId}&classificationId={statusUpdateViewModel.ClassificationId}"));
            var response = client.SendAsync(request);
            response.Wait();

            if (!response.IsCompleted)
            {
                this.logger.LogError($"Request to {dashboardCalculatorRunApi} failed with status code {response.Result}");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
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