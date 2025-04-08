using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentCalculatorController"/> class.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    [Route("[controller]")]
    public class PaymentCalculatorController : Controller
    {
        [HttpGet]
        [Route("{runId}")]
        public IActionResult Index(int runId)
        {
            var model = new AcceptInvoiceInstructionsViewModel
            {
                RunId = runId,
                AcceptAll = false,
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculationRunTitle = "Calculation Run 99",
                BackLink = ControllerNames.ClassifyRunConfirmation,
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AcceptInvoiceInstructions(int runId)
        {
            if (!this.ModelState.IsValid)
            {
                return RedirectToAction(ActionNames.Index, new { runId });
            }

            return RedirectToAction(ActionNames.Index, ControllerNames.CalculationRunOverview, new { runId });
        }

        /// <summary>
        /// Displays a billing file sent confirmation screen.
        /// </summary>
        /// <returns>Billing file sent page.</returns>
        [Route("BillingFileSuccess")]
        public IActionResult BillingFileSuccess()
        {
            // Create the view model
            var model = new BillingFileSuccessViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                ConfirmationViewModel = new ConfirmationViewModel
                {
                    Title = ConfirmationMessages.BillingFileSuccessTitle,
                    Body = ConfirmationMessages.BillingFileSuccessBody,
                    AdditionalParagraphs = ConfirmationMessages.BillingFileSuccessAdditionalParagraphs,
                    RedirectController = CommonConstants.DashBoard,
                },
            };

            // Return the view
            return this.View(ViewNames.BillingConfirmationSuccess, model);
        }
    }
}