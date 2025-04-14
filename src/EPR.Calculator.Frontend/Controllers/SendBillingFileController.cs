using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for sending billing files.
    /// </summary>
    [Route("[controller]")]
    public class SendBillingFileController : BaseController
    {
        public SendBillingFileController(IConfiguration configuration, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
        }

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
        public IActionResult Submit(int runId)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(ActionNames.Index, new { runId });
            }

            return RedirectToAction(ActionNames.BillingFileSuccess, ControllerNames.PaymentCalculator);
        }
    }
}
