using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DesignatedRunController"/> class.
    /// </summary>
    /// <param name="configuration">The configuration settings.</param>
    /// <param name="clientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="tokenAcquisition">The token acquisition service.</param>
    /// <param name="telemetryClient">The telemetry client.</param>
    [Route("[controller]")]
    public class DesignatedRunController(
        IConfiguration configuration,
        IApiService apiService,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(
            configuration,
            tokenAcquisition,
            telemetryClient,
            apiService,
            calculatorRunDetailsService)
    {
        [HttpGet("{runId}")]
        public async Task<IActionResult> Index(int runId)
        {
            var viewModel = await this.CreateViewModel(runId);

            if (viewModel.CalculatorRunDetails == null || viewModel.CalculatorRunDetails.RunId == 0 || !IsRunEligibleForDisplay(viewModel.CalculatorRunDetails))
            {
                return this.RedirectToAction(
                    ActionNames.StandardErrorIndex,
                    CommonUtil.GetControllerName(typeof(StandardErrorController)));
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

            return this.RedirectToRoute(RouteNames.BillingInstructionsIndex, new { calculationRunId = runId });
        }

        private static bool IsRunEligibleForDisplay(CalculatorRunDetailsViewModel calculatorRunDetails)
        {
            return calculatorRunDetails.RunClassificationId == RunClassification.INITIAL_RUN
                ||
                calculatorRunDetails.RunClassificationId == RunClassification.INTERIM_RECALCULATION_RUN
                ||
                calculatorRunDetails.RunClassificationId == RunClassification.FINAL_RUN
                ||
                calculatorRunDetails.RunClassificationId == RunClassification.FINAL_RECALCULATION_RUN;
        }

        private async Task<ClassifyRunConfirmationViewModel> CreateViewModel(int runId)
        {
            var viewModel = new ClassifyRunConfirmationViewModel()
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculatorRunDetails = new CalculatorRunDetailsViewModel(),
                BackLink = ControllerNames.CalculationRunDetails,
                HideBackLink = true,
            };

            var runDetails = await this.CalculatorRunDetailsService.GetCalculatorRundetailsAsync(
                this.HttpContext,
                runId);
            if (runDetails != null && runDetails!.RunId != 0)
            {
                viewModel.CalculatorRunDetails = runDetails;
            }

            return viewModel;
        }
    }
}
