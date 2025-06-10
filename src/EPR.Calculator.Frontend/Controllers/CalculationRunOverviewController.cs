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
    [Route("[controller]")]
    public class CalculationRunOverviewController(
        IConfiguration configuration,
        IHttpClientFactory clientFactory,
        ILogger<CalculationRunOverviewController> logger,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient)
        : BaseController(configuration, tokenAcquisition, telemetryClient, clientFactory)
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
                return RedirectToAction(ActionNames.Index, new { runId });
            }

            return RedirectToAction(ActionNames.Index, ControllerNames.SendBillingFile, new { runId = runId });
        }

        private async Task<CalculatorRunOverviewViewModel> CreateViewModel(int runId)
        {
            var viewModel = new CalculatorRunOverviewViewModel()
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculatorRunDetails = new CalculatorRunDetailsViewModel(),
            };

            var runDetails = await this.GetCalculatorRundetails(runId);
            if (runDetails != null && runDetails!.RunId > 0)
            {
                viewModel.CalculatorRunDetails = runDetails;
                this.SetDownloadParameters(viewModel);
            }

            return viewModel;
        }

        private void SetDownloadParameters(CalculatorRunOverviewViewModel viewModel)
        {
            var baseApiUrl = this.Configuration.GetValue<string>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultApi}");
            viewModel.DownloadResultURL = new Uri($"{baseApiUrl}/{viewModel.CalculatorRunDetails.RunId}");

            var draftedBillingApiUrl = this.Configuration.GetValue<string>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadDraftBillingApi}");
            viewModel.DownloadDraftBillingURL = new Uri($"{draftedBillingApiUrl}/{viewModel.CalculatorRunDetails.RunId}");

            viewModel.DownloadErrorURL = $"/DownloadFileErrorNew/{viewModel.CalculatorRunDetails.RunId}";
            viewModel.DownloadTimeout = this.Configuration.GetValue<int>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultTimeoutInMilliSeconds}");
        }
    }
}
