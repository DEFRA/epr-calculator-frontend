using System.Net;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
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
        IHttpClientFactory httpClientFactory)
        : BaseController(configuration, tokenAcquisition, telemetryClient, httpClientFactory)
    {
        [Route("{runId}")]
        public IActionResult Index(int runId)
        {
            var billingFileViewModel = new SendBillingFileViewModel()
            {
                RunId = runId,
                CalcRunName = "Calculation Run 99",
                ConfirmationContent = CommonConstants.ConfirmationContent,
                SendBillFileHeading = CommonConstants.SendBillingFile,
                WarningContent = CommonConstants.WarningContent,
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                BackLink = ControllerNames.CalculationRunOverview,
            };

            return this.View(billingFileViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int runId)
        {
            if (!ModelState.IsValid)
            {
                return this.RedirectToAction(ActionNames.Index, new { runId });
            }

            try
            {
                var response = await this.PrepareBillingFileSendToFSSAsync(runId);

                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted)
                {
                    return this.RedirectToAction(ActionNames.BillingFileSuccess, ControllerNames.PaymentCalculator);
                }

                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
            catch (Exception)
            {
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
            var apiUrl = this.GetApiUrl(
                ConfigSection.CalculationRunSettings,
                ConfigSection.PrepareBillingFileSendToFSS);

            var fullApiUrl = new Uri($"{apiUrl.AbsoluteUri.TrimEnd('/')}/{runId}");

            return await this.CallApi(HttpMethod.Post, fullApiUrl, string.Empty, null);
        }
    }
}
