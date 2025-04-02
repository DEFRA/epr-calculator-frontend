using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for the calculation run overview page.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    [Route("payment-calculator")]
    public class CalculationRunOverviewController : BaseController
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationRunOverviewController"/> class.
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <param name="clientFactory">clientFactory</param>
        /// <param name="logger">logger</param>
        /// <param name="tokenAcquisition">tokenAcquisition</param>
        /// <param name="telemetryClient">telemetryClient</param>
        public CalculationRunOverviewController(
            IConfiguration configuration,
            IHttpClientFactory clientFactory,
            ILogger<CalculationRunOverviewController> logger,
            ITokenAcquisition tokenAcquisition,
            TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the calculation run overview page.
        /// </summary>
        /// <param name="runId"> Run Id.</param>
        /// <returns>View.</returns>
        [Authorize(Roles = "SASuperUser")]
        [Route("runoverview/{runId}")]
        public Task<IActionResult> IndexAsync(int runId)
        {
            // Get the calculation run details from the API
            var calculatorRun = new CalculatorRunDto()
            {
                RunId = runId,
                FinancialYear = "2024-25",
                FileExtension = "xlsx",
                RunClassificationStatus = "Draft",
                RunName = "Calculation run 99",
                RunClassificationId = 240008,
                CreatedAt = DateTime.Now,
                CreatedBy = "Jo Bloggs",
            };

            var viewModel = CreateViewModel(runId, calculatorRun);
            SetDownloadParameters(viewModel);

            return Task.FromResult<IActionResult>(View(ViewNames.CalculationRunOverviewIndex, viewModel));
        }

        private CalculatorRunOverviewViewModel CreateViewModel(int runId, CalculatorRunDto calculatorRun)
        {
            return new CalculatorRunOverviewViewModel
            {
                CurrentUser = CommonUtil.GetUserName(HttpContext),
                Data = new CalculatorRunDto
                {
                    RunId = runId,
                    RunClassificationId = calculatorRun.RunClassificationId,
                    RunName = calculatorRun.RunName,
                    CreatedAt = calculatorRun.CreatedAt,
                    CreatedBy = calculatorRun.CreatedBy,
                    FinancialYear = calculatorRun.FinancialYear,
                    FileExtension = calculatorRun.FileExtension,
                    RunClassificationStatus = calculatorRun.RunClassificationStatus,
                },
            };
        }

        private void SetDownloadParameters(CalculatorRunOverviewViewModel viewModel)
        {
            var baseApiUrl = _configuration.GetValue<string>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultApi}");
            viewModel.DownloadResultURL = new Uri($"{baseApiUrl}/{viewModel.Data.RunId}");

            var draftedBillingApiUrl = _configuration.GetValue<string>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadDraftBillingApi}");
            viewModel.DownloadDraftBillingURL = new Uri($"{draftedBillingApiUrl}/{viewModel.Data.RunId}");

            viewModel.DownloadErrorURL = $"/DownloadFileError/{viewModel.Data.RunId}";
            viewModel.DownloadTimeout = _configuration.GetValue<int>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultTimeoutInMilliSeconds}");
        }
    }
}
