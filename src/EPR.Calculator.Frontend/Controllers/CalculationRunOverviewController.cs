using EPR.Calculator.Frontend.Common.Constants;
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

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for the calculation run overview page.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    [Route("payment-calculator")]
    public class CalculationRunOverviewController : BaseController
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<CalculationRunOverviewController> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationRunOverviewController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger instance.</param>
        public CalculationRunOverviewController(IConfiguration configuration,
            IHttpClientFactory clientFactory,
            ILogger<CalculationRunOverviewController> logger,
            ITokenAcquisition tokenAcquisition,
            TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
            _logger = logger;
        }


        /// <summary>
        /// Gets the calculation run overview page.
        /// </summary>
        /// <param name="runId"></param>
        /// <returns></returns>
        [Authorize(Roles = "SASuperUser")]
        [Route("runoverview/{runId}")]
        public async Task<IActionResult> IndexAsync(int runId)
        {
            try
            {
                // Get the calculation run details from the API
                var calculatorRun = new CalculatorRunDto()
                {
                    RunId = 1,
                    FinancialYear = "2024-25",
                    FileExtension = "xlsx",
                    RunClassificationStatus = "Draft",
                    RunName = "Calculation run 99",
                    RunClassificationId = 240008,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Jo Bloggs",
                };

                if (!IsRunEligibleForDisplay(calculatorRun))
                {
                    return View(ViewNames.CalculationRunDetailsErrorPage, CreateViewModelCommonData());
                }

                var viewModel = CreateViewModel(runId, calculatorRun);
                SetDownloadParameters(viewModel);

                return View(ViewNames.CalculationRunOverviewIndex, viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                return RedirectToErrorPage();
            }
        }

        private static bool IsRunEligibleForDisplay(CalculatorRunDto calculatorRun)
            => calculatorRun.RunClassificationId == (int)RunClassification.UNCLASSIFIED;

        private ViewModelCommonData CreateViewModelCommonData()
        {
            return new ViewModelCommonData { CurrentUser = CommonUtil.GetUserName(HttpContext) };
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

        private IActionResult RedirectToErrorPage()
         => RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
    }
}
