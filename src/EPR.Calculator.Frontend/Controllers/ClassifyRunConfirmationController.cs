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
        private readonly ILogger<ClassifyRunConfirmationController> _logger = logger;
        private readonly IConfiguration _configuration = configuration;

        [HttpGet("{runId}")]
        public async Task<IActionResult> Index(int runId)
        {
            var viewModel = await this.CreateViewModel(runId);

            if (viewModel.CalculatorRunDetails == null || viewModel.CalculatorRunDetails.RunId == 0)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
            else if (!IsRunEligibleForDisplay(viewModel.CalculatorRunDetails))
            {
                this.ModelState.AddModelError(viewModel.CalculatorRunDetails.RunName!, ErrorMessages.RunDetailError);
                return this.View(ViewNames.CalculationRunDetailsNewErrorPage, viewModel);
            }

            return this.View(ViewNames.ClassifyRunConfirmationIndex, viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(int runId)
        {
            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction(ActionNames.Index, new { runId });
            }

            return RedirectToAction(ActionNames.Index, ControllerNames.PaymentCalculator, new { runId = runId });
        }

        private static bool IsRunEligibleForDisplay(CalculatorRunDetailsViewModel calculatorRunDetails)
        {
            return calculatorRunDetails.RunClassificationId == RunClassification.UNCLASSIFIED;
        }

        private async Task<ClassifyRunConfirmationViewModel> CreateViewModel(int runId)
        {
            var viewModel = new ClassifyRunConfirmationViewModel()
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculatorRunDetails = new CalculatorRunDetailsViewModel(),
                BackLink = ControllerNames.CalculationRunDetails,
            };

            var runDetails = await this.GetCalculatorRundetails(runId);
            if (runDetails != null && runDetails!.RunId != 0)
            {
                viewModel.CalculatorRunDetails = runDetails;
                this.SetDownloadParameters(viewModel);
            }

            return viewModel;
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
