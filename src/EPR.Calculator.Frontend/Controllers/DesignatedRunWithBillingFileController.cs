using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for the calculation run overview page.
    /// </summary>
    [Route("[controller]")]
    public class DesignatedRunWithBillingFileController(
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
        [Route("{runId}")]
        public async Task<IActionResult> Index(int runId)
        {
            if (runId <= 0)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }

            var viewModel = await this.CreateViewModel(runId);
            if (viewModel.CalculatorRunDetails.RunId <= 0)
            {
                this.TelemetryClient.TrackTrace($"No run details found for runId: {runId}");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }

            return this.View(ViewNames.CalculationRunOverviewIndex, viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(int runId)
        {
            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction(ActionNames.Index, new { runId });
            }

            return this.RedirectToAction(ActionNames.Index, ControllerNames.SendBillingFile, new { runId = runId });
        }

        /// <summary>
        /// Generate the billing file.
        /// </summary>
        /// <param name="runId">The unique identifier for the calculation run.</param>
        [HttpGet]
        public async Task<IActionResult> GenerateDraftBillingFile(int runId)
        {
            var result = await this.TryGenerateBillingFile(runId);
            if (result)
            {
                return this.RedirectToRoute(new
                {
                    controller = ControllerNames.CalculationRunOverview,
                    action = "Index",
                    runId,
                });
            }
            else
            {
                return this.RedirectToStandardError;
            }
        }

        private IActionResult RedirectToStandardError
        {
            get
            {
                var controllerName = CommonUtil.GetControllerName(typeof(StandardErrorController));
                return this.RedirectToAction(ActionNames.StandardErrorIndex, controllerName);
            }
        }

        private async Task<bool> TryGenerateBillingFile(int runId)
        {
            var acceptApiUrl = this.ApiService.GetApiUrl(
                ConfigSection.CalculationRunSettings,
                ConfigSection.ProducerBillingInstructionsAcceptApi);

            var responseDto = await this.ApiService.CallApi(
                this.HttpContext,
                HttpMethod.Put,
                acceptApiUrl,
                runId.ToString(),
                null);

            if (!responseDto.IsSuccessStatusCode)
            {
                this.TelemetryClient.TrackTrace($"Billing instructions acceptance failed for RunId {runId}. StatusCode: {responseDto.StatusCode}, Reason: {responseDto.ReasonPhrase}");
                return false;
            }

            return true;
        }

        private async Task<CalculatorRunOverviewViewModel> CreateViewModel(int runId)
        {
            var currentUser = CommonUtil.GetUserName(this.HttpContext);
            var viewModel = new CalculatorRunOverviewViewModel()
            {
                CurrentUser = currentUser,
                CalculatorRunDetails = new CalculatorRunDetailsViewModel(),
                BackLinkViewModel = new BackLinkViewModel
                {
                    BackLink = string.Empty,
                    CurrentUser = currentUser,
                    HideBackLink = this.GetBackLink() != ControllerNames.Dashboard,
                },
            };

            var runDetails = await this.CalculatorRunDetailsService.GetCalculatorRundetailsAsync(
                this.HttpContext,
                runId);
            if (runDetails != null && runDetails!.RunId > 0)
            {
                viewModel.CalculatorRunDetails = runDetails;
            }

            return viewModel;
        }
    }
}
