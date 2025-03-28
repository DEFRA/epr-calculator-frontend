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
using System.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculationRunDetailsController"/> class.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    [Route("payment-calculator")]
    public class CalculationRunDetailsNewController : BaseController
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<CalculationRunDetailsNewController> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationRunDetailsNewController"/> class.
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationRunDetailsNewController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="tokenAcquisition">The token acquisition service.</param>
        /// <param name="telemetryClient">The telemetry client.</param>
        public CalculationRunDetailsNewController(
            IConfiguration configuration,
            IHttpClientFactory clientFactory,
            ILogger<CalculationRunDetailsNewController> logger,
            ITokenAcquisition tokenAcquisition,
            TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Displays the calculation run details index view.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <returns>The calculation run details index view.</returns>
        [Authorize(Roles = "SASuperUser")]
        [Route("rundetails/{runId}")]
        public async Task<IActionResult> IndexAsync(int runId)
        {
            try
            {
                var response = await GetCalculationDetailsAsync(runId);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Request failed with status code {StatusCode}", response.StatusCode);
                    return RedirectToErrorPage();
                }

                var calculatorRun = await DeserializeResponseAsync(response);
                if (calculatorRun == null)
                {
                    throw new ArgumentNullException($"Calculator with run id {runId} not found");
                }

                if (!IsRunEligibleForDisplay(calculatorRun))
                {
                    return View(ViewNames.CalculationRunDetailsErrorPage, CreateViewModelCommonData());
                }

                var viewModel = CreateViewModel(runId, calculatorRun);
                SetDownloadParameters(viewModel);

                return View(ViewNames.CalculationRunDetailsNewIndex, viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                return RedirectToErrorPage();
            }
        }

        private static bool IsRunEligibleForDisplay(CalculatorRunDto calculatorRun)
            => calculatorRun.RunClassificationId == (int)RunClassification.UNCLASSIFIED;

        private async Task<HttpResponseMessage> GetCalculationDetailsAsync(int runId)
        {
            var client = CreateHttpClient();
            client.DefaultRequestHeaders.Add("Authorization", await AcquireToken());
            return await client.GetAsync($"{client.BaseAddress}/{runId}");
        }

        private HttpClient CreateHttpClient()
        {
            var apiUrl = _configuration.GetValue<string>($"{ConfigSection.DashboardCalculatorRun}:{ConfigSection.DashboardCalculatorRunApi}");
            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri(apiUrl!);
            return client;
        }

        private async Task<CalculatorRunDto?> DeserializeResponseAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CalculatorRunDto>(content);
        }

        private ViewModelCommonData CreateViewModelCommonData()
        {
            return new ViewModelCommonData { CurrentUser = CommonUtil.GetUserName(HttpContext) };
        }

        private CalculatorRunDetailsNewViewModel CreateViewModel(int runId, CalculatorRunDto calculatorRun)
        {
            return new CalculatorRunDetailsNewViewModel
            {
                CurrentUser = CommonUtil.GetUserName(HttpContext),
                Data = new CalculatorRunDto
                {
                    RunId = runId,
                    RunClassificationId = calculatorRun.RunClassificationId,
                    RunName = calculatorRun.RunName,
                    CreatedAt = calculatorRun.CreatedAt,
                    CreatedBy = "Jo Bloggs", // TODO: Replace with actual data
                    FinancialYear = "2024-25", // TODO: Replace with actual data
                    FileExtension = calculatorRun.FileExtension,
                    RunClassificationStatus = calculatorRun.RunClassificationStatus,
                },
            };
        }

        private void SetDownloadParameters(CalculatorRunDetailsNewViewModel viewModel)
        {
            var baseApiUrl = _configuration.GetValue<string>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultApi}");
            viewModel.DownloadResultURL = new Uri($"{baseApiUrl}/{viewModel.Data.RunId}");
            viewModel.DownloadErrorURL = $"/DownloadFileError/{viewModel.Data.RunId}";
            viewModel.DownloadTimeout = _configuration.GetValue<int>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultTimeoutInMilliSeconds}");
        }

        private IActionResult RedirectToErrorPage()
         => RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
    }
}