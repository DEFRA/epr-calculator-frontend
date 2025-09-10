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

        private async Task<CalculatorRunOverviewViewModel> CreateViewModel(int runId)
        {
            var viewModel = new CalculatorRunOverviewViewModel()
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculatorRunDetails = new CalculatorRunDetailsViewModel(),
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
