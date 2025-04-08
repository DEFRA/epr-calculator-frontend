using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    [Authorize(Roles = "SASuperUser")]
    [Route("[controller]")]
    public class SendBillingFileController : Controller
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
        public IActionResult SubmitSendBillingFile(int runId)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(ActionNames.Index, new { runId });
            }

            return RedirectToAction("BillingFileSuccess", ControllerNames.PaymentCalculator);
        }
    }
}
