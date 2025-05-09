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

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassifyRunConfirmationController"/> class.
    /// </summary>
    /// <param name="configuration">The configuration settings.</param>
    /// <param name="clientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="tokenAcquisition">The token acquisition service.</param>
    /// <param name="telemetryClient">The telemetry client.</param>
    [Route("[controller]")]
    public class ClassifyRunConfirmationController(
        IConfiguration configuration,
        IHttpClientFactory clientFactory,
        ILogger<ClassifyRunConfirmationController> logger,
        ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
        : BaseController(configuration, tokenAcquisition, telemetryClient, clientFactory)
    {
        private readonly ILogger<ClassifyRunConfirmationController> logger = logger;
        private readonly IConfiguration _configuration = configuration;

        [Route("{runId}")]
        public IActionResult Index(int runId)
        {
            try
            {
                var statusUpdateViewModel = new ClassifyRunConfirmationViewModel
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                    CalculatorRunDetails = new CalculatorRunDetailsViewModel
                    {
                        RunId = runId,
                        RunClassificationId = RunClassification.INITIAL_RUN,
                        RunName = "Calculation Run 99",
                        CreatedAt = new DateTime(2024, 5, 1, 12, 09, 0, DateTimeKind.Utc),
                        RunClassificationStatus = "3",
                        FinancialYear = "2024-25",
                    },
                    BackLink = ControllerNames.ClassifyingCalculationRun,
                };
                this.SetDownloadParameters(statusUpdateViewModel);

                return this.View(ViewNames.ClassifyRunConfirmationIndex, statusUpdateViewModel);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(int runId)
        {
            if (!this.ModelState.IsValid)
            {
                return RedirectToAction(ActionNames.Index, new { runId });
            }

            return RedirectToAction(ActionNames.Index, ControllerNames.PaymentCalculator, new { runId = runId });
        }

        private void SetDownloadParameters(ClassifyRunConfirmationViewModel viewModel)
        {
            var baseApiUrl = this._configuration.GetValue<string>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultApi}");
            viewModel.DownloadResultURL = new Uri($"{baseApiUrl}/{viewModel.CalculatorRunDetails.RunId}");

            viewModel.DownloadErrorURL = $"/DownloadFileErrorNew/{viewModel.CalculatorRunDetails.RunId}";
            viewModel.DownloadTimeout = this._configuration.GetValue<int>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultTimeoutInMilliSeconds}");
        }
    }
}
