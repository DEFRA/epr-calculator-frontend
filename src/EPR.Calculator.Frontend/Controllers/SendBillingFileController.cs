using System.Net;
using Azure;
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
    /// Controller for sending billing files.
    /// </summary>
    [Route("[controller]")]
    public class SendBillingFileController(
        IConfiguration configuration,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        IApiService apiService,
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
            var runDetails = await this.CalculatorRunDetailsService.GetCalculatorRundetailsAsync(
                this.HttpContext,
                runId);
            if (runDetails == null || runDetails.RunName == null)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }

            var billingFileViewModel = new SendBillingFileViewModel()
            {
                RunId = runId,
                CalcRunName = runDetails.RunName,
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                BackLink = ControllerNames.CalculationRunOverview,
            };

            return this.View(billingFileViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SendBillingFileViewModel viewModel)
        {
            if (viewModel.ConfirmSend != true || !this.ModelState.IsValid)
            {
                return this.View(ViewNames.SendBillingFileIndex, viewModel);
            }

            try
            {
                var response = await this.PrepareBillingFileSendToFSSAsync(viewModel.RunId);

                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    return this.RedirectToAction(ActionNames.BillingFileSuccess, CommonUtil.GetControllerName(typeof(BillingInstructionsController)));
                }

                if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    viewModel.IsBillingFileLatest = false;
                    return this.View(ActionNames.Index, viewModel);
                }

                this.TelemetryClient.TrackTrace($"1.Request (send billing file) not accepted response code:{response.StatusCode}");
                var contentString = await response.Content.ReadAsStringAsync();
                this.TelemetryClient.TrackTrace($"2.Request (send billing file) not accepted response message:{contentString}");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
            catch (Exception e)
            {
                this.TelemetryClient.TrackException(e);
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        /// <summary>
        /// Calls the "prepareBillingFileSendToFSS" POST endpoint.
        /// </summary>
        /// <param name="runId">The runId parameter to be used as url parameter.</param>
        /// <returns>The response message returned by the endpoint.</returns>
        protected async Task<HttpResponseMessage> PrepareBillingFileSendToFSSAsync(int runId)
        {
            var apiUrl = this.ApiService.GetApiUrl(
                ConfigSection.CalculationRunSettings,
                ConfigSection.PrepareBillingFileSendToFSS);

            return await this.ApiService.CallApi(this.HttpContext, HttpMethod.Post, apiUrl, runId.ToString(), null);
        }
    }
}
